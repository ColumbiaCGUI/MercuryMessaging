// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// FishNet Integration Test Manager
// Tests MercuryMessaging FishNetBackend with proper MmNetworkBridge routing
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

#if FISHNET_AVAILABLE
using FishNet;
using FishNet.Managing;
using FishNet.Connection;
using FishNet.Transporting;
using MercuryMessaging.Network.Backends;
#endif

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// FishNet integration test manager for MercuryMessaging.
    ///
    /// This component tests end-to-end message routing through MmNetworkBridge.
    /// It uses the shared MmNetworkBridge instance (not its own backend) to ensure
    /// messages are properly routed to MmRelayNode responders via NetId.
    ///
    /// Setup:
    /// 1. Add FishNet's NetworkManager to scene
    /// 2. Add FishNetBridgeSetup component (handles backend wiring)
    /// 3. Add this component for testing GUI
    /// 4. Use ParrelSync to run two Unity editors
    /// 5. Start as Server in one, Client in the other
    /// 6. Use the GUI to send test messages
    ///
    /// Key Difference from direct backend testing:
    /// - Uses MmNetworkBridge.Send() which includes NetId routing
    /// - Messages are delivered to registered MmRelayNodes
    /// - Verifies end-to-end message propagation
    /// </summary>
    public class FishNetTestManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Target MmRelayNode for message routing (optional - uses first registered if null)")]
        public MmRelayNode targetRelayNode;

        [Tooltip("Enable direct backend testing (bypasses MmNetworkBridge routing)")]
        public bool useDirectBackend = false;

        [Header("Status")]
        [SerializeField] private bool isInitialized;
        [SerializeField] private bool isServer;
        [SerializeField] private bool isClient;
        [SerializeField] private int localClientId = -1;
        [SerializeField] private int connectedClients;
        [SerializeField] private uint targetNetId;

        [Header("Test Results")]
        [SerializeField] private int messagesSent;
        [SerializeField] private int messagesReceived;
        [SerializeField] private int messagesRoutedToResponders;
        [SerializeField] private List<string> messageLog = new List<string>();

#if FISHNET_AVAILABLE
        private FishNetBackend _directBackend; // Only used if useDirectBackend is true
        private NetworkManager _networkManager;
#endif

        // Singleton to prevent duplicate initialization
        private static FishNetTestManager _instance;
        private bool _isActiveInstance;

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton check - only one active instance allowed
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[FishNetTest] Duplicate FishNetTestManager detected - disabling this instance");
                enabled = false;
                return;
            }
            _instance = this;
            _isActiveInstance = true;

#if FISHNET_AVAILABLE
            _networkManager = FindFirstObjectByType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogWarning("[FishNetTest] No NetworkManager found in scene. Add one from FishNet prefabs.");
            }
#endif
        }

        private void Start()
        {
            if (!_isActiveInstance) return;
            Initialize();

            // Ensure proper test responder exists on target relay node
            EnsureNetworkTestResponder();
        }

        private void Update()
        {
            if (!_isActiveInstance) return;
            UpdateStatus();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            Shutdown();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the test manager. Uses MmNetworkBridge by default.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized || !_isActiveInstance) return;

#if FISHNET_AVAILABLE
            // Subscribe to MmNetworkBridge events for tracking
            if (MmNetworkBridge.Instance != null)
            {
                MmNetworkBridge.Instance.OnMessageReceived += OnBridgeMessageReceived;
            }

            // Subscribe to FishNet connection events (for both bridge and direct modes)
            if (_networkManager != null)
            {
                if (_networkManager.ServerManager != null)
                    _networkManager.ServerManager.OnServerConnectionState += OnFishNetServerConnectionState;
                if (_networkManager.ClientManager != null)
                    _networkManager.ClientManager.OnClientConnectionState += OnFishNetClientConnectionState;
            }

            // Only create direct backend if explicitly requested (for raw transport testing)
            if (useDirectBackend)
            {
                _directBackend = new FishNetBackend();
                _directBackend.OnMessageReceived += OnDirectMessageReceived;
                _directBackend.OnClientConnected += OnClientConnected;
                _directBackend.OnClientDisconnected += OnClientDisconnected;
                _directBackend.OnConnectedToServer += OnConnectedToServer;
                _directBackend.OnDisconnectedFromServer += OnDisconnectedFromServer;
                _directBackend.Initialize();
                AddLog("Direct FishNetBackend initialized (bypassing MmNetworkBridge)");
            }
            else
            {
                AddLog("Using MmNetworkBridge for message routing");
            }

            // Resolve target NetId (may need to re-resolve when network connects)
            ResolveTargetNetId();

            isInitialized = true;
#else
            AddLog("FishNet not available - install package first");
#endif
        }

        /// <summary>
        /// Shutdown the test manager.
        /// </summary>
        public void Shutdown()
        {
            if (!isInitialized) return;

#if FISHNET_AVAILABLE
            if (MmNetworkBridge.Instance != null)
            {
                MmNetworkBridge.Instance.OnMessageReceived -= OnBridgeMessageReceived;
            }

            // Unsubscribe from FishNet connection events
            if (_networkManager != null)
            {
                if (_networkManager.ServerManager != null)
                    _networkManager.ServerManager.OnServerConnectionState -= OnFishNetServerConnectionState;
                if (_networkManager.ClientManager != null)
                    _networkManager.ClientManager.OnClientConnectionState -= OnFishNetClientConnectionState;
            }

            if (_directBackend != null)
            {
                _directBackend.Shutdown();
                _directBackend = null;
            }
#endif

            isInitialized = false;
            AddLog("FishNetTestManager shutdown");
        }

        private void ResolveTargetNetId()
        {
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;

            // Debug: Log bridge state
            AddLog($"ResolveTargetNetId: Bridge={bridge != null}, Resolver={bridge?.Resolver != null}");

            // PRIORITY: Check if TestRoot exists (from Build Test Hierarchy)
            // TestRoot takes priority over Inspector-assigned RootNode
            var testRoot = GameObject.Find("TestRoot");
            if (testRoot != null)
            {
                var testRootRelay = testRoot.GetComponent<MmRelayNode>();
                if (testRootRelay != null)
                {
                    targetRelayNode = testRootRelay;
                    targetNetId = GetDeterministicId(testRoot);
                    AddLog($"Target set to TestRoot (NetId: {targetNetId}) - use 'Build Test Hierarchy' results");
                    return;
                }
            }

            // Try to get NetId from targetRelayNode (Inspector-assigned)
            if (targetRelayNode != null && targetRelayNode.gameObject.activeInHierarchy)
            {
                if (bridge?.Resolver != null && bridge.Resolver.TryGetNetworkId(targetRelayNode.gameObject, out uint netId))
                {
                    targetNetId = netId;
                    AddLog($"Target NetId resolved: {targetNetId} ({targetRelayNode.gameObject.name})");
                    return;
                }

                // Fallback: Use deterministic path-based hash (matches FishNetBridgeSetup)
                targetNetId = GetDeterministicId(targetRelayNode.gameObject);
                AddLog($"Target NetId (path-based): {targetNetId} ({targetRelayNode.gameObject.name})");
                return;
            }

            // Try to find any MmRelayNode in scene, INCLUDING inactive ones
            // (FishNet may disable NetworkObjects until spawned)
            var relayNodes = FindObjectsByType<MmRelayNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AddLog($"Found {relayNodes.Length} MmRelayNode(s) in scene (including inactive)");

            // Try to get NetworkObject ID first (for spawned objects with valid ObjectId > 0)
            foreach (var node in relayNodes)
            {
                if (bridge?.Resolver != null && bridge.Resolver.TryGetNetworkId(node.gameObject, out uint netId))
                {
                    targetNetId = netId;
                    targetRelayNode = node;
                    AddLog($"Auto-resolved target NetId: {targetNetId} ({node.gameObject.name})");
                    return;
                }
            }

            // Fallback: Use first relay node with deterministic ID (path-based)
            if (relayNodes.Length > 0)
            {
                var firstNode = relayNodes[0];
                targetNetId = GetDeterministicId(firstNode.gameObject);
                targetRelayNode = firstNode;
                AddLog($"Auto-resolved target (path-based): {targetNetId} ({firstNode.gameObject.name})");
                return;
            }

            AddLog("No relay nodes found - messages will not route to responders");
#endif
        }

        /// <summary>
        /// Ensures the target relay node has a NetworkTestResponder component.
        /// Called during initialization to fix scenes that only have MmBaseResponder.
        /// </summary>
        private void EnsureNetworkTestResponder()
        {
            if (targetRelayNode == null) return;

            // Check if NetworkTestResponder already exists
            var responder = targetRelayNode.GetComponent<NetworkTestResponder>();
            if (responder != null)
            {
                AddLog($"[OK] {targetRelayNode.name} has NetworkTestResponder");
                return;
            }

            // Remove plain MmBaseResponder if present (it doesn't log messages)
            var baseResponder = targetRelayNode.GetComponent<MmBaseResponder>();
            if (baseResponder != null && baseResponder.GetType() == typeof(MmBaseResponder))
            {
                Destroy(baseResponder);
                AddLog($"Removed plain MmBaseResponder from {targetRelayNode.name}");
            }

            // Add NetworkTestResponder
            targetRelayNode.gameObject.AddComponent<NetworkTestResponder>();
            AddLog($"Added NetworkTestResponder to {targetRelayNode.name}");

            // Refresh routing table after adding component
            StartCoroutine(RefreshAfterFrame());
        }

        private IEnumerator RefreshAfterFrame()
        {
            yield return null; // Wait one frame for Awake/Start
            if (targetRelayNode != null)
            {
                targetRelayNode.MmRefreshResponders();
                AddLog("Routing table refreshed");
            }
        }

        /// <summary>
        /// Validates the test setup and warns if no responders are found.
        /// </summary>
        private void ValidateTestSetup()
        {
            if (targetRelayNode == null)
            {
                AddLog("[WARNING] No target relay node assigned!");
                return;
            }

            var responders = targetRelayNode.GetComponentsInChildren<NetworkTestResponder>();
            if (responders.Length == 0)
            {
                AddLog($"[WARNING] {targetRelayNode.name} has no NetworkTestResponder components!");
                AddLog("Messages will reach the relay node but won't be logged.");
                AddLog("Use 'Build Test Hierarchy' button or add NetworkTestResponder manually.");
            }
            else
            {
                AddLog($"[OK] Found {responders.Length} NetworkTestResponder(s) ready to receive messages");
            }
        }

        /// <summary>
        /// Generate a deterministic ID based on GameObject hierarchy path.
        /// Must match FishNetBridgeSetup.GetDeterministicId() for cross-instance routing.
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

        private void UpdateStatus()
        {
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;
            if (bridge != null && bridge.Backend != null)
            {
                isServer = bridge.IsServer;
                isClient = bridge.IsClient;
                localClientId = bridge.Backend.LocalClientId;
            }
            else if (_directBackend != null)
            {
                isServer = _directBackend.IsServer;
                isClient = _directBackend.IsClient;
                localClientId = _directBackend.LocalClientId;
            }

            if (_networkManager != null && _networkManager.ServerManager != null)
            {
                connectedClients = _networkManager.ServerManager.Clients.Count;
            }
#endif
        }

        #endregion

        #region Event Handlers

#if FISHNET_AVAILABLE
        /// <summary>
        /// Called when MmNetworkBridge receives and deserializes a message (before routing to relay nodes).
        /// </summary>
        private void OnBridgeMessageReceived(MmMessage message)
        {
            messagesReceived++;
            string info = $"Bridge received {message.MmMessageType} (NetId: {message.NetId})";

            // Extract value based on type
            switch (message.MmMessageType)
            {
                case MmMessageType.MmString:
                    info += $": \"{((MmMessageString)message).value}\"";
                    break;
                case MmMessageType.MmInt:
                    info += $": {((MmMessageInt)message).value}";
                    break;
                case MmMessageType.MmFloat:
                    info += $": {((MmMessageFloat)message).value}";
                    break;
            }

            // Check if it will route to a responder - verify actual responder presence
            if (message.NetId != 0 && MmNetworkBridge.Instance.TryGetRelayNode((uint)message.NetId, out var node))
            {
                var responderCount = node.GetComponentsInChildren<NetworkTestResponder>().Length;
                if (responderCount > 0)
                {
                    messagesRoutedToResponders++;
                    info += $" → Routed to {responderCount} responder(s) ✓";
                }
                else
                {
                    info += " → Node found but NO responders attached!";
                }
            }
            else if (message.NetId != 0)
            {
                info += " → No relay node for NetId";
            }

            AddLog(info);
        }

        /// <summary>
        /// Called when using direct backend mode (bypasses MmNetworkBridge).
        /// </summary>
        private void OnDirectMessageReceived(byte[] data, int senderId)
        {
            messagesReceived++;

            try
            {
                var message = MmBinarySerializer.Deserialize(data);
                string info = $"Direct received {message.MmMessageType} from {senderId}";

                switch (message.MmMessageType)
                {
                    case MmMessageType.MmString:
                        info += $": \"{((MmMessageString)message).value}\"";
                        break;
                    case MmMessageType.MmInt:
                        info += $": {((MmMessageInt)message).value}";
                        break;
                    case MmMessageType.MmFloat:
                        info += $": {((MmMessageFloat)message).value}";
                        break;
                }

                AddLog(info);
            }
            catch (System.Exception e)
            {
                AddLog($"Failed to deserialize: {e.Message}");
            }
        }

        private void OnClientConnected(int clientId)
        {
            AddLog($"Client {clientId} connected");
            // Re-resolve NetId when clients connect (server may have new spawned objects)
            ResolveTargetNetId();
        }

        private void OnClientDisconnected(int clientId)
        {
            AddLog($"Client {clientId} disconnected");
        }

        private void OnConnectedToServer()
        {
            AddLog("Connected to server");
            // Re-resolve NetId when connected (client may now see spawned objects)
            ResolveTargetNetId();
        }

        private void OnDisconnectedFromServer()
        {
            AddLog("Disconnected from server");
        }

        /// <summary>
        /// Called when FishNet server connection state changes.
        /// </summary>
        private void OnFishNetServerConnectionState(ServerConnectionStateArgs args)
        {
            AddLog($"FishNet Server: {args.ConnectionState}");

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                // Re-resolve when server starts (relay nodes should now be registered)
                ResolveTargetNetId();

                // Validate test setup after connection
                ValidateTestSetup();
            }
        }

        /// <summary>
        /// Called when FishNet client connection state changes.
        /// </summary>
        private void OnFishNetClientConnectionState(ClientConnectionStateArgs args)
        {
            AddLog($"FishNet Client: {args.ConnectionState}");

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                // Re-resolve when client connects (relay nodes should now be registered)
                ResolveTargetNetId();

                // Validate test setup after connection
                ValidateTestSetup();
            }
        }
#endif

        #endregion

        #region Send Test Messages

        /// <summary>
        /// Send a test string message through MmNetworkBridge (with NetId routing).
        /// </summary>
        public void SendTestString(string value)
        {
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;
            if (bridge == null || !bridge.IsConnected)
            {
                if (useDirectBackend && _directBackend != null && _directBackend.IsConnected)
                {
                    SendDirectString(value);
                    return;
                }
                AddLog("Not connected");
                return;
            }

            var msg = new MmMessageString(value, MmMethod.MessageString, MmMetadataBlockHelper.Default);
            msg.NetId = targetNetId;

            string direction = bridge.IsServer ? "Server → All clients" : "Client → Server";
            string netIdInfo = targetNetId > 0 ? $" (NetId: {targetNetId})" : " (no NetId - won't route)";

            bridge.Send(msg);
            AddLog($"{direction}: \"{value}\"{netIdInfo}");
            messagesSent++;
#else
            AddLog("FishNet not available");
#endif
        }

        /// <summary>
        /// Send a test int message through MmNetworkBridge (with NetId routing).
        /// </summary>
        public void SendTestInt(int value)
        {
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;
            if (bridge == null || !bridge.IsConnected)
            {
                if (useDirectBackend && _directBackend != null && _directBackend.IsConnected)
                {
                    SendDirectInt(value);
                    return;
                }
                AddLog("Not connected");
                return;
            }

            var msg = new MmMessageInt(value, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
            msg.NetId = targetNetId;

            string direction = bridge.IsServer ? "Server → All clients" : "Client → Server";
            string netIdInfo = targetNetId > 0 ? $" (NetId: {targetNetId})" : " (no NetId - won't route)";

            bridge.Send(msg);
            AddLog($"{direction}: {value}{netIdInfo}");
            messagesSent++;
#else
            AddLog("FishNet not available");
#endif
        }

        /// <summary>
        /// Send a test Vector3 message through MmNetworkBridge (with NetId routing).
        /// </summary>
        public void SendTestVector3(Vector3 value)
        {
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;
            if (bridge == null || !bridge.IsConnected)
            {
                if (useDirectBackend && _directBackend != null && _directBackend.IsConnected)
                {
                    SendDirectVector3(value);
                    return;
                }
                AddLog("Not connected");
                return;
            }

            var msg = new MmMessageVector3(value, MmMethod.MessageVector3, MmMetadataBlockHelper.Default);
            msg.NetId = targetNetId;

            string direction = bridge.IsServer ? "Server → All clients" : "Client → Server";
            string netIdInfo = targetNetId > 0 ? $" (NetId: {targetNetId})" : " (no NetId - won't route)";

            bridge.Send(msg);
            AddLog($"{direction}: {value}{netIdInfo}");
            messagesSent++;
#else
            AddLog("FishNet not available");
#endif
        }

#if FISHNET_AVAILABLE
        // Direct backend methods (bypass MmNetworkBridge - for raw transport testing)
        private void SendDirectString(string value)
        {
            var msg = new MmMessageString(value, MmMethod.MessageString, MmMetadataBlockHelper.Default);
            byte[] data = MmBinarySerializer.Serialize(msg);

            if (_directBackend.IsServer)
            {
                _directBackend.SendToAllClients(data);
                AddLog($"[Direct] Server sent: \"{value}\"");
            }
            else
            {
                _directBackend.SendToServer(data);
                AddLog($"[Direct] Client sent: \"{value}\"");
            }
            messagesSent++;
        }

        private void SendDirectInt(int value)
        {
            var msg = new MmMessageInt(value, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
            byte[] data = MmBinarySerializer.Serialize(msg);

            if (_directBackend.IsServer)
            {
                _directBackend.SendToAllClients(data);
                AddLog($"[Direct] Server sent: {value}");
            }
            else
            {
                _directBackend.SendToServer(data);
                AddLog($"[Direct] Client sent: {value}");
            }
            messagesSent++;
        }

        private void SendDirectVector3(Vector3 value)
        {
            var msg = new MmMessageVector3(value, MmMethod.MessageVector3, MmMetadataBlockHelper.Default);
            byte[] data = MmBinarySerializer.Serialize(msg);

            if (_directBackend.IsServer)
            {
                _directBackend.SendToAllClients(data);
                AddLog($"[Direct] Server sent: {value}");
            }
            else
            {
                _directBackend.SendToServer(data);
                AddLog($"[Direct] Client sent: {value}");
            }
            messagesSent++;
        }
#endif

        #endregion

        #region Hierarchical Routing Tests

        /// <summary>
        /// Build a test hierarchy using NetworkTestSceneBuilder.
        /// Creates: TestRoot (MmRelayNode) with multiple NetworkTestResponder children.
        /// </summary>
        public void BuildTestHierarchy()
        {
            // Reset responder counters
            NetworkTestResponder.ResetTotalCount();

            // Find or create the scene builder
            var builder = FindFirstObjectByType<NetworkTestSceneBuilder>();
            if (builder == null)
            {
                var builderObj = new GameObject("NetworkTestSceneBuilder");
                builder = builderObj.AddComponent<NetworkTestSceneBuilder>();
                AddLog("Created NetworkTestSceneBuilder");
            }

            // Build the hierarchy
            builder.BuildTestHierarchy();

            // Update target to the new TestRoot
            if (builder.testRootRelay != null)
            {
                targetRelayNode = builder.testRootRelay;
                targetNetId = GetDeterministicId(targetRelayNode.gameObject);
                AddLog($"Updated target to TestRoot (NetId: {targetNetId})");

                // Re-register with MmNetworkBridge if available
#if FISHNET_AVAILABLE
                var bridge = MmNetworkBridge.Instance;
                if (bridge != null)
                {
                    // FishNetBridgeSetup should handle registration, but we can force re-registration
                    var bridgeSetup = FindFirstObjectByType<FishNetBridgeSetup>();
                    if (bridgeSetup != null)
                    {
                        bridgeSetup.RefreshRegistrations();
                        AddLog("Refreshed bridge registrations");
                    }
                }
#endif
            }

            // Count responders
            var responders = FindObjectsByType<NetworkTestResponder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            AddLog($"Hierarchy built with {responders.Length} NetworkTestResponder(s)");
        }

        /// <summary>
        /// Test hierarchical message routing by sending a message to TestRoot
        /// and verifying all child responders receive it.
        /// </summary>
        public void TestHierarchicalRouting()
        {
            // Reset counters before test
            NetworkTestResponder.ResetTotalCount();

            var responders = FindObjectsByType<NetworkTestResponder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (responders.Length == 0)
            {
                AddLog("ERROR: No NetworkTestResponders found. Build hierarchy first!");
                return;
            }

            // Reset individual counters
            foreach (var r in responders)
            {
                r.ResetCounters();
            }

            AddLog($"Testing hierarchical routing to {responders.Length} responders...");

            // Test 1: Send via MmRelayNode.MmInvoke (local hierarchical routing)
            if (targetRelayNode != null)
            {
                AddLog("Test 1: Local MmInvoke (MmMethod.MessageString)");
                var metadata = new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.Active,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                );
                targetRelayNode.MmInvoke(MmMethod.MessageString, "HierarchyTest_Local", metadata);

                // Check results after a frame (use coroutine or immediate check)
                int localReceived = NetworkTestResponder.TotalMessagesReceived;
                AddLog($"  → {localReceived}/{responders.Length} responders received message");

                if (localReceived == responders.Length)
                {
                    AddLog("  ✓ PASS: All responders received local message");
                }
                else
                {
                    AddLog($"  ✗ FAIL: Expected {responders.Length}, got {localReceived}");
                }
            }
            else
            {
                AddLog("SKIP: No targetRelayNode set");
            }

            // Test 2: Send via network (if connected)
#if FISHNET_AVAILABLE
            var bridge = MmNetworkBridge.Instance;
            if (bridge != null && bridge.IsConnected)
            {
                AddLog("Test 2: Network send (MmMessageString)");

                // Reset counters for network test
                NetworkTestResponder.ResetTotalCount();
                foreach (var r in responders)
                {
                    r.ResetCounters();
                }

                SendTestString("HierarchyTest_Network");
                AddLog("  → Network message sent (check remote instance for reception)");
            }
            else
            {
                AddLog("SKIP: Network not connected (Test 2 skipped)");
            }
#endif
        }

        /// <summary>
        /// Test sending different message types through the hierarchy.
        /// </summary>
        public void TestAllMessageTypes()
        {
            NetworkTestResponder.ResetTotalCount();

            if (targetRelayNode == null)
            {
                AddLog("ERROR: No targetRelayNode set");
                return;
            }

            var responders = FindObjectsByType<NetworkTestResponder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in responders) r.ResetCounters();

            AddLog("Testing all message types...");

            var metadata = new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );

            // Test each message type
            targetRelayNode.MmInvoke(MmMethod.MessageString, "TestString", metadata);
            AddLog($"  MmString: {NetworkTestResponder.TotalMessagesReceived} received");

            targetRelayNode.MmInvoke(MmMethod.MessageInt, 42, metadata);
            AddLog($"  MmInt: {NetworkTestResponder.TotalMessagesReceived} received");

            targetRelayNode.MmInvoke(MmMethod.MessageFloat, 3.14f, metadata);
            AddLog($"  MmFloat: {NetworkTestResponder.TotalMessagesReceived} received");

            targetRelayNode.MmInvoke(MmMethod.MessageBool, true, metadata);
            AddLog($"  MmBool: {NetworkTestResponder.TotalMessagesReceived} received");

            targetRelayNode.MmInvoke(MmMethod.MessageVector3, Vector3.one, metadata);
            AddLog($"  MmVector3: {NetworkTestResponder.TotalMessagesReceived} received");

            targetRelayNode.MmInvoke(MmMethod.Initialize, metadata);
            AddLog($"  Initialize: {NetworkTestResponder.TotalMessagesReceived} received");

            int expected = responders.Length * 6; // 6 message types
            AddLog($"Total: {NetworkTestResponder.TotalMessagesReceived}/{expected} (expected {responders.Length} responders × 6 types)");

            if (NetworkTestResponder.TotalMessagesReceived == expected)
            {
                AddLog("✓ ALL MESSAGE TYPES PASS");
            }
            else
            {
                AddLog("✗ SOME MESSAGE TYPES FAILED");
            }
        }

        #endregion

        #region Helpers

        private void AddLog(string message)
        {
            string timestamped = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            messageLog.Add(timestamped);
            Debug.Log($"[FishNetTest] {message}");

            // Keep log size reasonable
            while (messageLog.Count > 20)
            {
                messageLog.RemoveAt(0);
            }
        }

        public void ClearLog()
        {
            messageLog.Clear();
            messagesSent = 0;
            messagesReceived = 0;
            messagesRoutedToResponders = 0;
        }

        /// <summary>
        /// Manually trigger NetId resolution (useful after scene changes).
        /// </summary>
        public void RefreshTargetNetId()
        {
            ResolveTargetNetId();
        }

        #endregion

        #region Editor GUI

        private Vector2 _scrollPos;
        private string _testString = "Hello FishNet!";
        private int _testInt = 42;

        private void OnGUI()
        {
            if (!_isActiveInstance) return;

            GUILayout.BeginArea(new Rect(10, 10, 380, 550));

            // Title
            GUILayout.Label("MercuryMessaging FishNet Test", GUI.skin.box);

#if FISHNET_AVAILABLE
            // Status
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Status: {(isInitialized ? "Initialized" : "Not Initialized")}");
            GUILayout.Label($"Server: {isServer} | Client: {isClient}");
            GUILayout.Label($"Local Client ID: {localClientId}");
            GUILayout.Label($"Connected Clients: {connectedClients}");
            GUILayout.Label($"Target NetId: {(targetNetId > 0 ? targetNetId.ToString() : "None")}");
            if (targetRelayNode != null)
            {
                GUILayout.Label($"Target Node: {targetRelayNode.gameObject.name}");
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Controls
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Send Test Messages:");

            // String test
            GUILayout.BeginHorizontal();
            _testString = GUILayout.TextField(_testString, GUILayout.Width(200));
            if (GUILayout.Button("Send String"))
            {
                SendTestString(_testString);
            }
            GUILayout.EndHorizontal();

            // Int test
            GUILayout.BeginHorizontal();
            if (int.TryParse(GUILayout.TextField(_testInt.ToString(), GUILayout.Width(200)), out int parsed))
            {
                _testInt = parsed;
            }
            if (GUILayout.Button("Send Int"))
            {
                SendTestInt(_testInt);
            }
            GUILayout.EndHorizontal();

            // Vector3 test
            if (GUILayout.Button("Send Random Vector3"))
            {
                SendTestVector3(Random.insideUnitSphere * 10f);
            }

            // Refresh button
            if (GUILayout.Button("Refresh Target NetId"))
            {
                RefreshTargetNetId();
            }

            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Hierarchical Routing Test
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Hierarchical Routing Test:");

            // Build hierarchy button
            if (GUILayout.Button("Build Test Hierarchy"))
            {
                BuildTestHierarchy();
            }

            // Test hierarchical routing
            if (GUILayout.Button("Test Hierarchical Routing"))
            {
                TestHierarchicalRouting();
            }

            // Test all message types
            if (GUILayout.Button("Test All Message Types"))
            {
                TestAllMessageTypes();
            }

            // Show responder count
            var responders = FindObjectsByType<NetworkTestResponder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            GUILayout.Label($"Responders in scene: {responders.Length}");
            GUILayout.Label($"Total messages to responders: {NetworkTestResponder.TotalMessagesReceived}");

            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Stats
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Sent: {messagesSent} | Received: {messagesReceived}");
            GUILayout.Label($"Routed to Responders: {messagesRoutedToResponders}");
            GUILayout.EndVertical();

            // Log
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Message Log:");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(150));
            foreach (var log in messageLog)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Clear Log"))
            {
                ClearLog();
            }
            GUILayout.EndVertical();
#else
            // FishNet not available
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("FishNet Not Installed", GUI.skin.box);
            GUILayout.Space(10);
            GUILayout.Label("To test FishNet networking:");
            GUILayout.Label("1. Install FishNet via Package Manager");
            GUILayout.Label("2. Add FishNetBridgeSetup to scene");
            GUILayout.Label("3. Add NetworkManager from FishNet");
            GUILayout.Label("4. Use ParrelSync for two editors");
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.Label("For local testing without FishNet:");
            if (GUILayout.Button("Run Loopback Tests"))
            {
                var runner = FindFirstObjectByType<NetworkBackendTestRunner>();
                if (runner != null)
                {
                    runner.RunAllTests();
                }
                else
                {
                    Debug.Log("Add NetworkBackendTestRunner component to test loopback");
                }
            }
#endif

            GUILayout.EndArea();
        }

        #endregion
    }
}
