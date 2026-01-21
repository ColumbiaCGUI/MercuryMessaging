// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if UNITY_TEXTMESHPRO
using TMPro;
#endif

namespace MercuryMessaging.Tests.Performance.Editor
{
    /// <summary>
    /// Editor utility to create performance test scenes programmatically.
    /// </summary>
    public class PerformanceSceneBuilder : EditorWindow
    {
        private bool createSmallScale = true;
        private bool createMediumScale = true;
        private bool createLargeScale = true;

        [MenuItem("MercuryMessaging/Performance/Build Test Scenes")]
        public static void ShowWindow()
        {
            GetWindow<PerformanceSceneBuilder>("Build Performance Scenes");
        }

        private void OnGUI()
        {
            GUILayout.Label("Performance Test Scene Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This will create three performance test scenes:\n\n" +
                "• SmallScale: 10 responders, 3 levels, 100 msg/sec\n" +
                "• MediumScale: 50 responders, 5 levels, 500 msg/sec\n" +
                "• LargeScale: 100+ responders, 7-10 levels, 1000 msg/sec",
                MessageType.Info);

            GUILayout.Space(10);

            createSmallScale = EditorGUILayout.Toggle("Create SmallScale.unity", createSmallScale);
            createMediumScale = EditorGUILayout.Toggle("Create MediumScale.unity", createMediumScale);
            createLargeScale = EditorGUILayout.Toggle("Create LargeScale.unity", createLargeScale);

            GUILayout.Space(10);

            if (GUILayout.Button("Build Scenes", GUILayout.Height(40)))
            {
                BuildScenes();
            }
        }

        private void BuildScenes()
        {
            if (createSmallScale)
            {
                BuildSmallScaleScene();
            }

            if (createMediumScale)
            {
                BuildMediumScaleScene();
            }

            if (createLargeScale)
            {
                BuildLargeScaleScene();
            }

            EditorUtility.DisplayDialog("Success", "Performance test scenes created successfully!", "OK");
        }

        #region SmallScale Scene

        private void BuildSmallScaleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create Root with MmRelayNode
            GameObject root = new GameObject("Root");
            var rootRelay = root.AddComponent<MmRelayNode>();

            // Add PerformanceTestHarness
            var harness = root.AddComponent<PerformanceTestHarness>();
            harness.testScenario = PerformanceTestHarness.TestScenario.Small;
            harness.responderCount = 10;
            harness.hierarchyDepth = 3;
            harness.messageVolume = 100;
            harness.testDuration = 60f;
            harness.autoStart = false;
            harness.exportPath = "performance-results/smallscale_results.csv";
            harness.relayNode = rootRelay;

            // Add MessageGenerator
            var generator = root.AddComponent<MessageGenerator>();
            generator.messagesPerSecond = 100;
            generator.messageMethod = MmMethod.MessageString;
            generator.levelFilter = MmLevelFilterHelper.SelfAndChildren;
            generator.autoStart = true; // Auto-start message generation
            generator.relayNode = rootRelay;
            generator.testHarness = harness;

            // Create UI Canvas for display
            CreateUICanvas(root, harness);

            // Build hierarchy: 3 levels, 10 responders total
            // Level 1: 2 branches
            GameObject level1A = CreateRelayNode("Level1_A", root.transform);
            GameObject level1B = CreateRelayNode("Level1_B", root.transform);

            // Level 2: 3 responders under A, 2 under B
            CreateResponder("Responder1", level1A.transform);
            CreateResponder("Responder2", level1A.transform);
            CreateResponder("Responder3", level1A.transform);

            CreateResponder("Responder4", level1B.transform);
            CreateResponder("Responder5", level1B.transform);

            // Level 3: 3 more responders (split between A and B's children)
            GameObject level2A = CreateRelayNode("Level2_A", level1A.transform);
            CreateResponder("Responder6", level2A.transform);
            CreateResponder("Responder7", level2A.transform);

            GameObject level2B = CreateRelayNode("Level2_B", level1B.transform);
            CreateResponder("Responder8", level2B.transform);
            CreateResponder("Responder9", level2B.transform);
            CreateResponder("Responder10", level2B.transform);

            // CRITICAL: Register all responders and establish parent-child relationships
            RefreshHierarchy(root);

            // Save scene
            string scenePath = "Assets/MercuryMessaging/Tests/Performance/Scenes/SmallScale.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[PerformanceSceneBuilder] Created: {scenePath}");
        }

        #endregion

        #region MediumScale Scene

        private void BuildMediumScaleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create Root with MmRelayNode
            GameObject root = new GameObject("Root");
            var rootRelay = root.AddComponent<MmRelayNode>();

            // Add PerformanceTestHarness
            var harness = root.AddComponent<PerformanceTestHarness>();
            harness.testScenario = PerformanceTestHarness.TestScenario.Medium;
            harness.responderCount = 50;
            harness.hierarchyDepth = 5;
            harness.messageVolume = 500;
            harness.testDuration = 60f;
            harness.autoStart = false;
            harness.exportPath = "performance-results/mediumscale_results.csv";
            harness.relayNode = rootRelay;

            // Add MessageGenerator
            var generator = root.AddComponent<MessageGenerator>();
            generator.messagesPerSecond = 500;
            generator.messageMethod = MmMethod.MessageString;
            generator.levelFilter = MmLevelFilterHelper.SelfAndBidirectional; // Multi-direction routing
            generator.autoStart = true; // Auto-start message generation
            generator.relayNode = rootRelay;
            generator.testHarness = harness;

            // Create UI Canvas
            CreateUICanvas(root, harness);

            // Build hierarchy: 5 levels, 50 responders
            // More complex structure with mixed tags
            MmTag[] tags = { MmTag.Tag0, MmTag.Tag1, MmTag.Tag2, MmTag.Tag3 };
            int tagIndex = 0;
            int responderCount = 0;

            // Level 1: 3 branches
            GameObject[] level1 = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                level1[i] = CreateRelayNode($"Level1_{(char)('A' + i)}", root.transform);
            }

            // Level 2-5: Create deeper hierarchy
            foreach (var l1 in level1)
            {
                // Level 2: 2 branches per L1
                for (int i = 0; i < 2; i++)
                {
                    GameObject l2 = CreateRelayNode($"L2_{l1.name}_{i}", l1.transform);

                    // Level 3: 2-3 responders per L2
                    int respondersHere = (i == 0) ? 3 : 2;
                    for (int j = 0; j < respondersHere && responderCount < 50; j++)
                    {
                        GameObject resp = CreateResponder($"Responder_{responderCount + 1}", l2.transform);
                        // Assign tags for filter cache testing
                        var testResp = resp.GetComponent<TestResponder>();
                        testResp.Tag = tags[tagIndex % tags.Length];
                        testResp.TagCheckEnabled = true;
                        tagIndex++;
                        responderCount++;
                    }

                    // Level 4: Additional relay node
                    if (responderCount < 50)
                    {
                        GameObject l3 = CreateRelayNode($"L3_{l2.name}", l2.transform);

                        // Level 5: More responders
                        int respondersDeep = Mathf.Min(2, 50 - responderCount);
                        for (int k = 0; k < respondersDeep; k++)
                        {
                            GameObject resp = CreateResponder($"Responder_{responderCount + 1}", l3.transform);
                            var testResp = resp.GetComponent<TestResponder>();
                            testResp.Tag = tags[tagIndex % tags.Length];
                            testResp.TagCheckEnabled = true;
                            tagIndex++;
                            responderCount++;
                        }
                    }
                }
            }

            // CRITICAL: Register all responders and establish parent-child relationships
            RefreshHierarchy(root);

            // Save scene
            string scenePath = "Assets/MercuryMessaging/Tests/Performance/Scenes/MediumScale.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[PerformanceSceneBuilder] Created: {scenePath} ({responderCount} responders)");
        }

        #endregion

        #region LargeScale Scene

        private void BuildLargeScaleScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create Root with MmRelayNode
            GameObject root = new GameObject("Root");
            var rootRelay = root.AddComponent<MmRelayNode>();
            rootRelay.maxMessageHops = 50; // Ensure hop limit is set for deep hierarchy
            rootRelay.enableCycleDetection = true;

            // Add PerformanceTestHarness
            var harness = root.AddComponent<PerformanceTestHarness>();
            harness.testScenario = PerformanceTestHarness.TestScenario.Large;
            harness.responderCount = 100;
            harness.hierarchyDepth = 10;
            harness.messageVolume = 1000;
            harness.testDuration = 60f;
            harness.autoStart = false;
            harness.exportPath = "performance-results/largescale_results.csv";
            harness.relayNode = rootRelay;

            // Add MessageGenerator
            var generator = root.AddComponent<MessageGenerator>();
            generator.messagesPerSecond = 1000;
            generator.messageMethod = MmMethod.MessageString;
            generator.levelFilter = MmLevelFilterHelper.SelfAndBidirectional;
            generator.autoStart = true; // Auto-start message generation
            generator.relayNode = rootRelay;
            generator.testHarness = harness;

            // Create UI Canvas
            CreateUICanvas(root, harness);

            // Build complex hierarchy: 7-10 levels, 100+ responders
            MmTag[] tags = { MmTag.Tag0, MmTag.Tag1, MmTag.Tag2, MmTag.Tag3 };
            int tagIndex = 0;
            int responderCount = 0;

            // Level 1: 4 main branches
            GameObject[] level1 = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                level1[i] = CreateRelayNode($"Branch{i + 1}", root.transform);
            }

            // Create deep hierarchy with many responders
            foreach (var l1 in level1)
            {
                // Each branch gets ~25 responders across 7-10 levels
                CreateDeepBranch(l1.transform, 1, 10, ref responderCount, tags, ref tagIndex, 25);
            }

            // Add a few MmRelaySwitchNodes for FSM testing
            GameObject switchNode1 = new GameObject("FSM_StateManager");
            switchNode1.transform.SetParent(root.transform);
            var switchRelay = switchNode1.AddComponent<MmRelaySwitchNode>();

            GameObject state1 = CreateRelayNode("State_Active", switchNode1.transform);
            GameObject state2 = CreateRelayNode("State_Idle", switchNode1.transform);

            CreateResponder("FSM_Responder1", state1.transform);
            CreateResponder("FSM_Responder2", state2.transform);
            responderCount += 2;

            // CRITICAL: Register all responders and establish parent-child relationships
            RefreshHierarchy(root);

            // Save scene
            string scenePath = "Assets/MercuryMessaging/Tests/Performance/Scenes/LargeScale.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[PerformanceSceneBuilder] Created: {scenePath} ({responderCount} responders)");
        }

        /// <summary>
        /// Recursively create a deep branch with responders distributed across levels.
        /// </summary>
        private void CreateDeepBranch(Transform parent, int currentLevel, int maxLevel,
                                     ref int responderCount, MmTag[] tags, ref int tagIndex, int targetResponders)
        {
            if (currentLevel > maxLevel || responderCount >= targetResponders)
                return;

            // Create 1-2 relay nodes per level
            int nodesThisLevel = (currentLevel % 2 == 0) ? 2 : 1;

            for (int i = 0; i < nodesThisLevel && responderCount < targetResponders; i++)
            {
                GameObject node = CreateRelayNode($"L{currentLevel}_{parent.name}_{i}", parent);

                // Add 1-3 responders per node
                int respondersHere = Mathf.Min(UnityEngine.Random.Range(1, 4), targetResponders - responderCount);
                for (int j = 0; j < respondersHere; j++)
                {
                    GameObject resp = CreateResponder($"Resp_{responderCount + 1}", node.transform);
                    var testResp = resp.GetComponent<TestResponder>();
                    testResp.Tag = tags[tagIndex % tags.Length];
                    testResp.TagCheckEnabled = true;
                    tagIndex++;
                    responderCount++;
                }

                // Recurse deeper
                if (currentLevel < maxLevel && responderCount < targetResponders)
                {
                    CreateDeepBranch(node.transform, currentLevel + 1, maxLevel,
                                   ref responderCount, tags, ref tagIndex, targetResponders);
                }
            }
        }

        #endregion

        #region Helper Methods

        private GameObject CreateRelayNode(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var relay = obj.AddComponent<MmRelayNode>();

            // CRITICAL: Enable auto-registration so routing table populates on scene load
            relay.AutoGrabAttachedResponders = true;

            return obj;
        }

        private GameObject CreateResponder(string name, Transform parent)
        {
            // OPTION 1: Attach responder to parent relay node if parent has MmRelayNode
            var parentRelay = parent.GetComponent<MmRelayNode>();
            if (parentRelay != null)
            {
                // Attach TestResponder directly to the relay node GameObject
                var responder = parent.gameObject.AddComponent<TestResponder>();
                responder.countMessages = true;
                responder.logMessages = false;
                responder.name = name; // For debugging
                return parent.gameObject;
            }
            else
            {
                // OPTION 2: Create separate GameObject (fallback)
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(parent);
                var responder = obj.AddComponent<TestResponder>();
                responder.countMessages = true;
                responder.logMessages = false;
                return obj;
            }
        }

        private void CreateUICanvas(GameObject root, PerformanceTestHarness harness)
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("UI_Canvas");
            canvasObj.transform.SetParent(root.transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create Text (TextMeshPro)
            GameObject textObj = new GameObject("DisplayText");
            textObj.transform.SetParent(canvasObj.transform);
            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(20, 20);
            rectTransform.offsetMax = new Vector2(-20, -20);

#if UNITY_TEXTMESHPRO
            var tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TMPro.TextAlignmentOptions.TopLeft;
            tmpText.text = "Performance Test Ready\n\nPress Start to begin...";
            harness.displayText = tmpText;
#else
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.text = "Performance Test Ready\n\nPress Start to begin...";
            harness.displayText = text;
#endif
        }

        /// <summary>
        /// Refreshes the entire hierarchy to register responders and establish parent-child relationships.
        /// CRITICAL: Must be called after programmatically creating relay nodes and responders.
        /// </summary>
        private void RefreshHierarchy(GameObject root)
        {
            Debug.Log("[PerformanceSceneBuilder] Refreshing hierarchy to register responders...");

            var allRelayNodes = root.GetComponentsInChildren<MmRelayNode>(true);

            // Step 1: Register TestResponders attached to each relay node
            int totalTestResponders = 0;
            foreach (var relay in allRelayNodes)
            {
                int beforeCount = relay.RoutingTable.Count;
                relay.MmRefreshResponders(); // Registers TestResponders on same GameObject
                int afterCount = relay.RoutingTable.Count;
                int addedCount = afterCount - beforeCount;
                totalTestResponders += addedCount;
                if (addedCount > 0)
                {
                    Debug.Log($"[PerformanceSceneBuilder] {relay.gameObject.name}: Registered {addedCount} TestResponder(s)");
                }
                // CRITICAL: Mark relay node dirty for serialization
                EditorUtility.SetDirty(relay);
            }

            // Step 2: Register child relay nodes in each parent's routing table
            // This must happen BEFORE RefreshParents() because RefreshParents iterates through existing children
            int totalChildNodes = 0;
            foreach (var relay in allRelayNodes)
            {
                // Find direct child relay nodes (only immediate children, not all descendants)
                for (int i = 0; i < relay.transform.childCount; i++)
                {
                    var childTransform = relay.transform.GetChild(i);
                    var childRelay = childTransform.GetComponent<MmRelayNode>();
                    if (childRelay != null)
                    {
                        // Add child relay node to parent's routing table as Child level
                        if (!relay.RoutingTable.Contains(childRelay))
                        {
                            relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                            totalChildNodes++;
                            Debug.Log($"[PerformanceSceneBuilder] {relay.gameObject.name}: Added child relay node {childRelay.gameObject.name}");
                        }
                    }
                }
                // CRITICAL: Mark relay node dirty after adding children
                EditorUtility.SetDirty(relay);
            }

            // Step 3: Establish parent-child relationships starting from root
            var rootRelay = root.GetComponent<MmRelayNode>();
            if (rootRelay != null)
            {
                int beforeParents = rootRelay.RoutingTable.Count;
                rootRelay.RefreshParents();
                int afterParents = rootRelay.RoutingTable.Count;
                Debug.Log($"[PerformanceSceneBuilder] Root: RefreshParents completed (routing table: {afterParents} items)");
                // CRITICAL: Mark root dirty after RefreshParents
                EditorUtility.SetDirty(rootRelay);
            }

            // Step 4: Final verification and dirty marking
            int totalRoutingItems = 0;
            foreach (var relay in allRelayNodes)
            {
                totalRoutingItems += relay.RoutingTable.Count;
                // Ensure all relay nodes are marked dirty for serialization
                EditorUtility.SetDirty(relay);
                EditorUtility.SetDirty(relay.gameObject);
            }

            Debug.Log($"[PerformanceSceneBuilder] ✓ Complete: {allRelayNodes.Length} relay nodes, {totalTestResponders} responders, {totalChildNodes} child nodes, {totalRoutingItems} total routing table items");
            Debug.Log($"[PerformanceSceneBuilder] ✓ All objects marked dirty for serialization");
        }

        #endregion
    }
}
