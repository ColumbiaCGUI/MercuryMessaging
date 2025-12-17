// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Fusion 2 Bridge Setup - Wires MmNetworkBridge with Fusion 2 backend
// Add this component alongside MmNetworkBridge for automatic setup

using UnityEngine;
using MercuryMessaging.Network;

#if FUSION2_AVAILABLE
using MercuryMessaging.Network.Backends;
using Fusion;
using Fusion.Sockets;
#endif

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// Automatically configures MmNetworkBridge with Photon Fusion 2 backend.
    ///
    /// Setup:
    /// 1. Add this component to a GameObject with MmNetworkBridge
    /// 2. Add MmFusion2Bridge component to a spawned NetworkObject in scene
    /// 3. Ensure NetworkRunner is in the scene
    /// 4. The bridge will auto-configure on Start and initialize when Fusion connects
    ///
    /// This component handles:
    /// - Creating Fusion2Backend and Fusion2Resolver
    /// - Configuring MmNetworkBridge
    /// - Initializing when Fusion 2 connects
    /// - Auto-registering MmRelayNodes with NetworkObject components
    /// </summary>
    [RequireComponent(typeof(MmNetworkBridge))]
    public class Fusion2BridgeSetup : MonoBehaviour
#if FUSION2_AVAILABLE
        , INetworkRunnerCallbacks
#endif
    {
        [Header("Configuration")]
        [Tooltip("Automatically initialize when Fusion 2 connects")]
        public bool autoInitializeOnConnect = true;

        [Tooltip("Log debug information")]
        public bool debugLogging = true;

        [Header("Status (Read-Only)")]
        [SerializeField] private bool isConfigured;
        [SerializeField] private bool isInitialized;
        [SerializeField] private string backendStatus = "Not Started";

#if FUSION2_AVAILABLE
        private MmNetworkBridge _bridge;
        private Fusion2Backend _backend;
        private Fusion2Resolver _resolver;
        private NetworkRunner _runner;

        private void Awake()
        {
            _bridge = GetComponent<MmNetworkBridge>();
            _runner = FindFirstObjectByType<NetworkRunner>();

            if (_runner == null)
            {
                LogWarning("No NetworkRunner found yet. Will configure when runner becomes available.");
                backendStatus = "Waiting for NetworkRunner";
            }
        }

        private void Start()
        {
            Configure();

            if (autoInitializeOnConnect && _runner != null)
            {
                // Register for callbacks
                _runner.AddCallbacks(this);
            }
        }

        private void Update()
        {
            // Check for NetworkRunner if we don't have one yet
            if (_runner == null)
            {
                _runner = FindFirstObjectByType<NetworkRunner>();
                if (_runner != null && autoInitializeOnConnect)
                {
                    _runner.AddCallbacks(this);
                    Log("NetworkRunner found, registered callbacks");
                }
            }

            // Auto-initialize when runner is running (handles Host/Server case)
            if (!isInitialized && isConfigured && _runner != null && _runner.IsRunning)
            {
                Log("Runner is running, auto-initializing...");
                Initialize();
                UpdateStatus();
            }
        }

        private void OnDestroy()
        {
            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
            }
        }

        /// <summary>
        /// Configure the bridge with Fusion 2 backend (does not initialize yet).
        /// </summary>
        public void Configure()
        {
            if (isConfigured) return;

            _backend = new Fusion2Backend();
            _resolver = new Fusion2Resolver();

            _bridge.Configure(_backend, _resolver);
            isConfigured = true;
            backendStatus = "Configured (not connected)";

            Log("Fusion 2 backend configured");
        }

        /// <summary>
        /// Initialize the bridge (call after Fusion 2 is connected).
        /// </summary>
        public void Initialize()
        {
            if (!isConfigured)
            {
                LogError("Cannot initialize - not configured. Call Configure() first.");
                return;
            }

            if (isInitialized)
            {
                Log("Already initialized");
                return;
            }

            // Set the runner on the backend
            if (_runner != null)
            {
                _backend.SetRunner(_runner);
            }

            _bridge.Initialize();
            isInitialized = true;

            UpdateStatus();
            Log("Fusion 2 bridge initialized");

            // Auto-register any MmRelayNodes with NetworkObject components
            AutoRegisterRelayNodes();
        }

        /// <summary>
        /// Shutdown and cleanup.
        /// </summary>
        public void Shutdown()
        {
            if (!isInitialized) return;

            _bridge.Shutdown();
            isInitialized = false;
            backendStatus = "Shutdown";

            Log("Fusion 2 bridge shutdown");
        }

        private void UpdateStatus()
        {
            if (_backend == null)
            {
                backendStatus = "Not configured";
                return;
            }

            if (_backend.IsServer && _backend.IsClient)
                backendStatus = "Host (Server + Client)";
            else if (_backend.IsServer)
                backendStatus = "Server";
            else if (_backend.IsClient)
                backendStatus = "Client";
            else
                backendStatus = "Disconnected";
        }

        private void AutoRegisterRelayNodes()
        {
            // Find all MmRelayNodes in scene, INCLUDING inactive ones
            var relayNodes = FindObjectsByType<MmRelayNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int registeredWithNetId = 0;
            int registeredWithFallback = 0;

            foreach (var node in relayNodes)
            {
                if (_resolver.TryGetNetworkId(node.gameObject, out uint netId))
                {
                    // Has valid NetworkObject with ID
                    _bridge.RegisterRelayNode(netId, node);
                    registeredWithNetId++;
                    Log($"Registered relay node '{node.gameObject.name}' with NetworkObject ID: {netId}");
                }
                else
                {
                    // Fallback: Use deterministic hash based on hierarchy path
                    uint fallbackId = GetDeterministicId(node.gameObject);
                    _bridge.RegisterRelayNode(fallbackId, node);
                    registeredWithFallback++;
                    Log($"Registered relay node '{node.gameObject.name}' with fallback ID: {fallbackId} (path-based)");
                }
            }

            Log($"Auto-registered {registeredWithNetId} relay nodes with NetworkObject IDs, {registeredWithFallback} with fallback IDs");
        }

        /// <summary>
        /// Generate a deterministic ID based on GameObject hierarchy path.
        /// </summary>
        private uint GetDeterministicId(GameObject go)
        {
            string path = GetHierarchyPath(go);
            int hash = path.GetHashCode();
            return (uint)((hash & 0x7FFFFFFF) | 1);
        }

        private string GetHierarchyPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        /// <summary>
        /// Manually refresh relay node registrations.
        /// Call this after spawning new networked objects.
        /// </summary>
        public void RefreshRegistrations()
        {
            if (!isInitialized)
            {
                LogError("Cannot refresh registrations - not initialized");
                return;
            }

            AutoRegisterRelayNodes();
        }

        #region INetworkRunnerCallbacks

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Log("Connected to server");
            Initialize();
            UpdateStatus();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Log($"Disconnected from server: {reason}");
            Shutdown();
            UpdateStatus();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Log($"Player joined: {player.PlayerId}");
            // Backend handles this via MmFusion2Bridge.IPlayerJoined
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Log($"Player left: {player.PlayerId}");
            // Backend handles this via MmFusion2Bridge.IPlayerLeft
        }

        // Required callback stubs (not used for our purposes)
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Log($"Runner shutdown: {shutdownReason}");
            Shutdown();
        }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            LogError($"Connect failed: {reason}");
            backendStatus = $"Connect failed: {reason}";
        }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        #endregion

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[Fusion2Bridge] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[Fusion2Bridge] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Fusion2Bridge] {message}");
        }

#else
        // Stub implementation when Fusion 2 is not available

        private void Start()
        {
            backendStatus = "Fusion 2 not installed";
            Debug.LogWarning("[Fusion2Bridge] Fusion 2 is not available. Install Photon Fusion 2 package first.");
        }

        public void Configure() { }
        public void Initialize() { }
        public void Shutdown() { }
#endif

        #region Editor GUI

        private void OnGUI()
        {
#if FUSION2_AVAILABLE
            // Only show if Fusion2TestManager is NOT present (avoid duplicate GUIs)
            if (GetComponent<Fusion2TestManager>() != null) return;
#endif

            // Status panel in top-right corner
            const int panelWidth = 210;
            const int panelHeight = 120;
            const int margin = 10;

            GUILayout.BeginArea(new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight));
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("MmNetworkBridge (Fusion 2)", GUI.skin.box);
            GUILayout.Label($"Status: {backendStatus}");
            GUILayout.Label($"Configured: {isConfigured}");
            GUILayout.Label($"Initialized: {isInitialized}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
