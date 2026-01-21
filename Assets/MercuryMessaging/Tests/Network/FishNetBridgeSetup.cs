// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// FishNet Bridge Setup - Wires MmNetworkBridge with FishNet backend
// Add this component alongside MmNetworkBridge for automatic setup

using UnityEngine;
using MercuryMessaging.Network;

#if FISHNET_AVAILABLE
using MercuryMessaging.Network.Backends;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
#endif

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Automatically configures MmNetworkBridge with FishNet backend.
    ///
    /// Setup:
    /// 1. Add this component to a GameObject
    /// 2. Ensure FishNet NetworkManager is in the scene
    /// 3. The bridge will auto-configure on Start
    ///
    /// This component handles:
    /// - Creating FishNetBackend and FishNetResolver
    /// - Configuring MmNetworkBridge
    /// - Initializing when FishNet connects
    /// - Auto-registering MmRelayNodes with NetworkObject components
    /// </summary>
    [RequireComponent(typeof(MmNetworkBridge))]
    public class FishNetBridgeSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Automatically initialize when FishNet connects")]
        public bool autoInitializeOnConnect = true;

        [Tooltip("Log debug information")]
        public bool debugLogging = true;

        [Header("Status (Read-Only)")]
        [SerializeField] private bool isConfigured;
        [SerializeField] private bool isInitialized;
        [SerializeField] private string backendStatus = "Not Started";

#if FISHNET_AVAILABLE
        private MmNetworkBridge _bridge;
        private FishNetBackend _backend;
        private FishNetResolver _resolver;
        private NetworkManager _networkManager;

        private void Awake()
        {
            _bridge = GetComponent<MmNetworkBridge>();
            _networkManager = InstanceFinder.NetworkManager;

            if (_networkManager == null)
            {
                _networkManager = FindFirstObjectByType<NetworkManager>();
            }

            if (_networkManager == null)
            {
                LogError("No NetworkManager found! Add FishNet NetworkManager prefab to scene.");
                backendStatus = "ERROR: No NetworkManager";
                return;
            }
        }

        private void Start()
        {
            Configure();

            if (autoInitializeOnConnect)
            {
                // Subscribe to connection events
                _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            }
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                if (_networkManager.ServerManager != null)
                    _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
                if (_networkManager.ClientManager != null)
                    _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }
        }

        /// <summary>
        /// Configure the bridge with FishNet backend (does not initialize yet).
        /// </summary>
        public void Configure()
        {
            if (isConfigured) return;

            _backend = new FishNetBackend();
            _resolver = new FishNetResolver();

            _bridge.Configure(_backend, _resolver);
            isConfigured = true;
            backendStatus = "Configured (not connected)";

            Log("FishNet backend configured");
        }

        /// <summary>
        /// Initialize the bridge (call after FishNet is connected).
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

            _bridge.Initialize();
            isInitialized = true;

            UpdateStatus();
            Log("FishNet bridge initialized");

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

            Log("FishNet bridge shutdown");
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            Log($"Server connection state: {args.ConnectionState}");

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Initialize();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                Shutdown();
            }

            UpdateStatus();
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            Log($"Client connection state: {args.ConnectionState}");

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Initialize();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                Shutdown();
            }

            UpdateStatus();
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
            // (FishNet may disable NetworkObjects until spawned)
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
                    // This ensures the same ID on both server and client (ParrelSync)
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
        /// This ensures the same relay node gets the same ID on both server and client.
        /// </summary>
        private uint GetDeterministicId(GameObject go)
        {
            string path = GetHierarchyPath(go);
            // Use string hash - consistent across instances
            int hash = path.GetHashCode();
            // Ensure positive and avoid 0 (reserved for "no target")
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

        private void Log(string message)
        {
            if (debugLogging)
                Debug.Log($"[FishNetBridge] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[FishNetBridge] {message}");
        }

#else
        // Stub implementation when FishNet is not available

        private void Start()
        {
            backendStatus = "FishNet not installed";
            Debug.LogWarning("[FishNetBridge] FishNet is not available. Install FishNet package first.");
        }

        public void Configure() { }
        public void Initialize() { }
        public void Shutdown() { }
#endif

        #region Editor GUI

        private void OnGUI()
        {
#if FISHNET_AVAILABLE
            // Only show if FishNetTestManager is NOT present (avoid duplicate GUIs)
            if (GetComponent<FishNetTestManager>() != null) return;
#endif

            // Status panel in top-right corner
            const int panelWidth = 210;
            const int panelHeight = 120;
            const int margin = 10;

            GUILayout.BeginArea(new Rect(Screen.width - panelWidth - margin, margin, panelWidth, panelHeight));
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("MmNetworkBridge", GUI.skin.box);
            GUILayout.Label($"Status: {backendStatus}");
            GUILayout.Label($"Configured: {isConfigured}");
            GUILayout.Label($"Initialized: {isInitialized}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
