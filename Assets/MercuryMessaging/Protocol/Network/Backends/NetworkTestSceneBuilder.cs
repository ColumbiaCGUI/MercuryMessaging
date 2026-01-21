// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Builds a proper test scene hierarchy for network + MercuryMessaging testing.
// Ensures consistent setup across ParrelSync instances.
// Works with both FishNet and Fusion 2 backends.

using UnityEngine;

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// Builds a standardized test scene for MercuryMessaging networking tests.
    /// Supports both FishNet and Fusion 2 backends.
    ///
    /// Creates hierarchy:
    /// - MmNetworkBridge
    /// - TestRoot (MmRelayNode)
    ///   - TestResponder1 (NetworkTestResponder)
    ///   - TestResponder2 (NetworkTestResponder)
    ///   - ChildNode (MmRelayNode)
    ///     - NestedResponder (NetworkTestResponder)
    /// </summary>
    public class NetworkTestSceneBuilder : MonoBehaviour
    {
        [Header("Build Settings")]
        [Tooltip("Number of responders to create under TestRoot")]
        public int rootResponderCount = 2;

        [Tooltip("Number of nested responders under ChildNode")]
        public int nestedResponderCount = 1;

        [Header("Runtime References (Auto-populated)")]
        public GameObject networkManagerObj;
        public GameObject bridgeObj;
        public GameObject testRootObj;
        public MmRelayNode testRootRelay;

        /// <summary>
        /// Build the test hierarchy at runtime.
        /// Call this from a button or on Start.
        /// </summary>
        [ContextMenu("Build Test Hierarchy")]
        public void BuildTestHierarchy()
        {
            Debug.Log("[SceneBuilder] Building test hierarchy...");

            // Clean up existing test objects
            CleanupTestObjects();

            // Create MmNetworkBridge if not exists
            bridgeObj = GameObject.Find("MmNetworkBridge");
            if (bridgeObj == null)
            {
                bridgeObj = new GameObject("MmNetworkBridge");
                bridgeObj.AddComponent<MercuryMessaging.Network.MmNetworkBridge>();
                // Backend-specific components are added by the caller (Fusion2TestManager or FishNetTestManager)
                Debug.Log("[SceneBuilder] Created MmNetworkBridge (add backend-specific setup components)");
            }

            // Create TestRoot with MmRelayNode
            testRootObj = new GameObject("TestRoot");
            testRootRelay = testRootObj.AddComponent<MmRelayNode>();

            // Add a responder directly on TestRoot
            testRootObj.AddComponent<NetworkTestResponder>();
            Debug.Log("[SceneBuilder] Created TestRoot with MmRelayNode and NetworkTestResponder");

            // Create child responders under TestRoot - each with its own relay node
            for (int i = 0; i < rootResponderCount; i++)
            {
                var responderObj = new GameObject($"TestResponder{i + 1}");
                responderObj.transform.SetParent(testRootObj.transform);

                // Add relay node first, then responder (standard pattern)
                var childRelay = responderObj.AddComponent<MmRelayNode>();
                responderObj.AddComponent<NetworkTestResponder>();

                // Register child relay node with parent
                testRootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(testRootRelay);
                Debug.Log($"[SceneBuilder] Created TestResponder{i + 1} with MmRelayNode");
            }

            // Create a nested MmRelayNode for deeper hierarchy testing
            var childNodeObj = new GameObject("ChildNode");
            childNodeObj.transform.SetParent(testRootObj.transform);
            var childNodeRelay = childNodeObj.AddComponent<MmRelayNode>();

            // CRITICAL: Register child relay node in parent's routing table
            testRootRelay.MmAddToRoutingTable(childNodeRelay, MmLevelFilter.Child);
            childNodeRelay.AddParent(testRootRelay);
            Debug.Log("[SceneBuilder] Created and registered ChildNode with MmRelayNode");

            // Create nested responders under ChildNode - each with its own relay node
            for (int i = 0; i < nestedResponderCount; i++)
            {
                var nestedObj = new GameObject($"NestedResponder{i + 1}");
                nestedObj.transform.SetParent(childNodeObj.transform);

                // Add relay node first, then responder (standard pattern)
                var nestedRelay = nestedObj.AddComponent<MmRelayNode>();
                nestedObj.AddComponent<NetworkTestResponder>();

                // Register nested relay node with parent
                childNodeRelay.MmAddToRoutingTable(nestedRelay, MmLevelFilter.Child);
                nestedRelay.AddParent(childNodeRelay);
                Debug.Log($"[SceneBuilder] Created NestedResponder{i + 1} with MmRelayNode");
            }

            // Refresh the relay nodes to pick up self-responders
            testRootRelay.MmRefreshResponders();
            childNodeRelay.MmRefreshResponders();

            int totalResponders = 1 + rootResponderCount + nestedResponderCount; // root + children + nested
            Debug.Log($"[SceneBuilder] Test hierarchy built! Total responders: {totalResponders}");
            Debug.Log("[SceneBuilder] Hierarchy:");
            Debug.Log("  TestRoot (MmRelayNode + NetworkTestResponder)");
            for (int i = 0; i < rootResponderCount; i++)
                Debug.Log($"    TestResponder{i + 1} (NetworkTestResponder)");
            Debug.Log("    ChildNode (MmRelayNode)");
            for (int i = 0; i < nestedResponderCount; i++)
                Debug.Log($"      NestedResponder{i + 1} (NetworkTestResponder)");
        }

        /// <summary>
        /// Clean up test objects (but preserve NetworkManager and MmNetworkBridge).
        /// Also disables the old RootNode from the scene to avoid confusion.
        /// </summary>
        [ContextMenu("Cleanup Test Objects")]
        public void CleanupTestObjects()
        {
            // Clean up any existing TestRoot
            var existingRoot = GameObject.Find("TestRoot");
            if (existingRoot != null)
            {
                DestroyImmediate(existingRoot);
                Debug.Log("[SceneBuilder] Cleaned up existing TestRoot");
            }

            // Disable old RootNode from scene (don't destroy - it may be a scene object)
            // This prevents confusion between old RootNode and new TestRoot
            var oldRootNode = GameObject.Find("RootNode");
            if (oldRootNode != null)
            {
                oldRootNode.SetActive(false);
                Debug.Log("[SceneBuilder] Disabled old RootNode (use TestRoot instead)");
            }
        }

        /// <summary>
        /// Verify the hierarchy is set up correctly.
        /// </summary>
        [ContextMenu("Verify Hierarchy")]
        public void VerifyHierarchy()
        {
            Debug.Log("[SceneBuilder] Verifying hierarchy...");

            // Check MmNetworkBridge
            var bridge = FindFirstObjectByType<MercuryMessaging.Network.MmNetworkBridge>();
            if (bridge == null)
            {
                Debug.LogError("[SceneBuilder] FAIL: MmNetworkBridge not found!");
                return;
            }
            Debug.Log("[SceneBuilder] OK: MmNetworkBridge found");

            // Check for backend-specific setup (Fusion 2 or other backends)
            // Note: FishNetBridgeSetup is in test assembly, so we skip checking for it here
            bool hasBackendSetup = false;
#if FUSION2_AVAILABLE
            hasBackendSetup |= FindFirstObjectByType<Fusion2BridgeSetup>() != null;
#endif
            // Also check by type name for cross-assembly discovery
            if (!hasBackendSetup)
            {
                var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var mb in allMonoBehaviours)
                {
                    var typeName = mb.GetType().Name;
                    if (typeName.Contains("BridgeSetup") || typeName.Contains("Backend"))
                    {
                        hasBackendSetup = true;
                        break;
                    }
                }
            }
            if (!hasBackendSetup)
            {
                Debug.LogWarning("[SceneBuilder] WARN: No network backend setup found");
            }
            else
            {
                Debug.Log("[SceneBuilder] OK: Backend setup found");
            }

            // Check TestRoot
            var testRoot = GameObject.Find("TestRoot");
            if (testRoot == null)
            {
                Debug.LogWarning("[SceneBuilder] WARN: TestRoot not found. Run 'Build Test Hierarchy' first.");
                return;
            }

            var relay = testRoot.GetComponent<MmRelayNode>();
            if (relay == null)
            {
                Debug.LogError("[SceneBuilder] FAIL: TestRoot missing MmRelayNode!");
                return;
            }
            Debug.Log("[SceneBuilder] OK: TestRoot has MmRelayNode");

            // Count responders
            var responders = testRoot.GetComponentsInChildren<NetworkTestResponder>(true);
            Debug.Log($"[SceneBuilder] OK: Found {responders.Length} NetworkTestResponder(s) in hierarchy");

            // Check relay nodes
            var relayNodes = FindObjectsByType<MmRelayNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[SceneBuilder] OK: Found {relayNodes.Length} MmRelayNode(s) in scene");

            Debug.Log("[SceneBuilder] Verification complete!");
        }

        // Editor menus are now consolidated in MmEditorMenus.cs
    }
}
