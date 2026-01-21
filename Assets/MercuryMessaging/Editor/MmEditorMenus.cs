// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmEditorMenus.cs - Consolidated editor menu system for MercuryMessaging
// All editor menus should be defined here for consistency.
//
// Menu Structure:
// MercuryMessaging/
//   ├── Validation/
//   │   ├── Validate Hierarchy
//   │   └── Validate Selected
//   ├── Network Tests/
//   │   ├── Build FishNet Test Scene
//   │   ├── Build Fusion 2 Test Scene
//   │   ├── Build Test Hierarchy (runtime)
//   │   └── Verify Hierarchy
//   ├── Performance/
//   │   └── Build Test Scenes
//   └── User Study/
//       ├── Build Smart Home Mercury Scene
//       └── Build Smart Home Events Scene

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MercuryMessaging.Editor
{
    /// <summary>
    /// Consolidated editor menu system for MercuryMessaging.
    /// All editor menus are centralized here for easy maintenance.
    /// </summary>
    public static class MmEditorMenus
    {
        // Menu path constants for consistency
        private const string MENU_ROOT = "MercuryMessaging";
        private const string VALIDATION = MENU_ROOT + "/Validation";
        private const string NETWORK_TESTS = MENU_ROOT + "/Network Tests";
        private const string PERFORMANCE = MENU_ROOT + "/Performance";

        #region Network Test Scene Builders

        /// <summary>
        /// Build a complete Fusion 2 network test scene from scratch.
        /// Creates NetworkRunner, MmFusion2Bridge, MmNetworkBridge, and test hierarchy.
        /// </summary>
        [MenuItem(NETWORK_TESTS + "/Build Fusion 2 Test Scene", false, 10)]
        public static void BuildFusion2TestScene()
        {
#if FUSION2_AVAILABLE
            if (!EditorUtility.DisplayDialog(
                "Build Fusion 2 Test Scene",
                "This will create a new scene with:\n" +
                "• NetworkRunner (Fusion 2)\n" +
                "• MmNetworkBridge + Fusion2BridgeSetup\n" +
                "• Fusion2TestManager (GUI)\n" +
                "• Test hierarchy with NetworkTestResponders\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create NetworkRunner
            var runnerObj = new GameObject("NetworkRunner");
            runnerObj.AddComponent<Fusion.NetworkRunner>();
            Debug.Log("[Fusion2Builder] Created NetworkRunner");

            // Create MmNetworkBridge with Fusion 2 setup
            var bridgeObj = new GameObject("MmNetworkBridge");
            bridgeObj.AddComponent<MercuryMessaging.Network.MmNetworkBridge>();
            bridgeObj.AddComponent<MercuryMessaging.Network.Backends.Fusion2BridgeSetup>();
            bridgeObj.AddComponent<MercuryMessaging.Network.Backends.Fusion2TestManager>();
            Debug.Log("[Fusion2Builder] Created MmNetworkBridge with Fusion2BridgeSetup and TestManager");

            // Build test hierarchy
            BuildTestHierarchyInternal();

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[Fusion2Builder] Fusion 2 test scene created successfully!");
            Debug.Log("[Fusion2Builder] Next steps:");
            Debug.Log("  1. Save the scene");
            Debug.Log("  2. Create MmFusion2Bridge prefab (spawned NetworkObject)");
            Debug.Log("  3. Use ParrelSync for multi-client testing");
            Debug.Log("  4. Start as Host/Client and use the GUI to test messages");

            EditorUtility.DisplayDialog(
                "Fusion 2 Test Scene Created",
                "Scene created with all necessary components.\n\n" +
                "Next steps:\n" +
                "1. Save the scene\n" +
                "2. Create MmFusion2Bridge prefab\n" +
                "3. Use ParrelSync for testing\n" +
                "4. Start Host/Client and test",
                "OK");
#else
            EditorUtility.DisplayDialog(
                "Fusion 2 Not Available",
                "Photon Fusion 2 is not installed or FUSION2_AVAILABLE is not defined.\n\n" +
                "To enable:\n" +
                "1. Install Fusion 2 via Package Manager\n" +
                "2. Add FUSION2_AVAILABLE to Scripting Define Symbols",
                "OK");
#endif
        }

        /// <summary>
        /// Build a complete FishNet network test scene from scratch.
        /// </summary>
        [MenuItem(NETWORK_TESTS + "/Build FishNet Test Scene", false, 11)]
        public static void BuildFishNetTestScene()
        {
#if FISHNET_AVAILABLE
            if (!EditorUtility.DisplayDialog(
                "Build FishNet Test Scene",
                "This will create a new scene with:\n" +
                "• NetworkManager (FishNet)\n" +
                "• MmNetworkBridge + FishNetBridgeSetup\n" +
                "• Test hierarchy with NetworkTestResponders\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Note: FishNet NetworkManager needs to be added from FishNet prefab
            // Just create placeholder for now
            var managerObj = new GameObject("NetworkManager (Add FishNet)");
            Debug.Log("[FishNetBuilder] Created NetworkManager placeholder - add FishNet NetworkManager component");

            // Create MmNetworkBridge (FishNetBridgeSetup is in test assembly)
            var bridgeObj = new GameObject("MmNetworkBridge");
            bridgeObj.AddComponent<MercuryMessaging.Network.MmNetworkBridge>();
            Debug.Log("[FishNetBuilder] Created MmNetworkBridge - add FishNetBridgeSetup from Tests assembly");

            // Build test hierarchy
            BuildTestHierarchyInternal();

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[FishNetBuilder] FishNet test scene created!");
            Debug.Log("[FishNetBuilder] You need to manually add:");
            Debug.Log("  1. FishNet NetworkManager to NetworkManager object");
            Debug.Log("  2. FishNetBridgeSetup to MmNetworkBridge object");

            EditorUtility.DisplayDialog(
                "FishNet Test Scene Created",
                "Scene created. You need to manually add:\n\n" +
                "1. FishNet NetworkManager component\n" +
                "2. FishNetBridgeSetup component\n" +
                "3. FishNetTestManager component",
                "OK");
#else
            EditorUtility.DisplayDialog(
                "FishNet Not Available",
                "FishNet is not installed or FISHNET_AVAILABLE is not defined.\n\n" +
                "To enable:\n" +
                "1. Install FishNet via Package Manager\n" +
                "2. Ensure FISHNET_AVAILABLE is defined",
                "OK");
#endif
        }

        /// <summary>
        /// Build test hierarchy in the current scene (for runtime use).
        /// </summary>
        [MenuItem(NETWORK_TESTS + "/Build Test Hierarchy (Current Scene)", false, 30)]
        public static void BuildTestHierarchy()
        {
            BuildTestHierarchyInternal();
        }

        /// <summary>
        /// Verify the network test hierarchy setup.
        /// </summary>
        [MenuItem(NETWORK_TESTS + "/Verify Hierarchy", false, 31)]
        public static void VerifyHierarchy()
        {
            var builder = Object.FindFirstObjectByType<MercuryMessaging.Network.Backends.NetworkTestSceneBuilder>();
            if (builder == null)
            {
                var builderObj = new GameObject("NetworkTestSceneBuilder");
                builder = builderObj.AddComponent<MercuryMessaging.Network.Backends.NetworkTestSceneBuilder>();
            }
            builder.VerifyHierarchy();
        }

        /// <summary>
        /// Internal method to build test hierarchy.
        /// </summary>
        private static void BuildTestHierarchyInternal()
        {
            // Create test hierarchy
            var testRootObj = new GameObject("TestRoot");
            var testRootRelay = testRootObj.AddComponent<MmRelayNode>();
            testRootObj.AddComponent<MercuryMessaging.Network.Backends.NetworkTestResponder>();

            // Create child responders
            for (int i = 0; i < 2; i++)
            {
                var responderObj = new GameObject($"TestResponder{i + 1}");
                responderObj.transform.SetParent(testRootObj.transform);
                var childRelay = responderObj.AddComponent<MmRelayNode>();
                responderObj.AddComponent<MercuryMessaging.Network.Backends.NetworkTestResponder>();
                testRootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(testRootRelay);
            }

            // Create nested node
            var childNodeObj = new GameObject("ChildNode");
            childNodeObj.transform.SetParent(testRootObj.transform);
            var childNodeRelay = childNodeObj.AddComponent<MmRelayNode>();
            testRootRelay.MmAddToRoutingTable(childNodeRelay, MmLevelFilter.Child);
            childNodeRelay.AddParent(testRootRelay);

            // Create nested responder
            var nestedObj = new GameObject("NestedResponder1");
            nestedObj.transform.SetParent(childNodeObj.transform);
            var nestedRelay = nestedObj.AddComponent<MmRelayNode>();
            nestedObj.AddComponent<MercuryMessaging.Network.Backends.NetworkTestResponder>();
            childNodeRelay.MmAddToRoutingTable(nestedRelay, MmLevelFilter.Child);
            nestedRelay.AddParent(childNodeRelay);

            // Refresh responders
            testRootRelay.MmRefreshResponders();
            childNodeRelay.MmRefreshResponders();

            Debug.Log("[TestBuilder] Test hierarchy created with 4 NetworkTestResponders");
        }

        #endregion
    }
}
