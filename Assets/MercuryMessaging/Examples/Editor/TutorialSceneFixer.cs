// Copyright (c) 2017-2025, Columbia University
// Editor utility to fix tutorial scene wiring.
// Run via Mercury > Tutorials > Fix Tutorial Scene Wiring

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MercuryMessaging;

/// <summary>
/// Fixes tutorial scene component wiring for all 12 tutorials.
/// Run from: Mercury > Tutorials > Fix Tutorial Scene Wiring
/// </summary>
public class TutorialSceneFixer : EditorWindow
{
    private const string TUTORIALS_PATH = "Assets/MercuryMessaging/Examples/Tutorials";
    private Vector2 scrollPos;
    private List<string> log = new List<string>();

    [MenuItem("MercuryMessaging/Tutorials/Fix Tutorial Scene Wiring (Window)")]
    static void ShowWindow()
    {
        GetWindow<TutorialSceneFixer>("Tutorial Scene Fixer");
    }

    /// <summary>
    /// Run from menu or batch mode:
    /// Unity.exe -batchmode -projectPath . -executeMethod TutorialSceneFixer.FixAllNow -quit
    /// </summary>
    [MenuItem("MercuryMessaging/Tutorials/Fix ALL Tutorial Scenes Now")]
    public static void FixAllNow()
    {
        var fixer = CreateInstance<TutorialSceneFixer>();
        fixer.log = new List<string>();
        fixer.FixAllScenes();
        DestroyImmediate(fixer);
    }

    void OnGUI()
    {
        GUILayout.Label("Tutorial Scene Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Fix ALL Tutorial Scenes (1-9)", GUILayout.Height(40)))
        {
            log.Clear();
            FixAllScenes();
        }

        GUILayout.Space(10);
        GUILayout.Label("Individual Fixes:", EditorStyles.boldLabel);

        if (GUILayout.Button("Fix Tutorial 1 (Introduction)")) { log.Clear(); FixTutorial1(); }
        if (GUILayout.Button("Fix Tutorial 2 (Basic Routing)")) { log.Clear(); FixTutorial2(); }
        if (GUILayout.Button("Fix Tutorial 3 (Custom Responders)")) { log.Clear(); FixTutorial3(); }
        if (GUILayout.Button("Fix Tutorial 4 (Custom Messages)")) { log.Clear(); FixTutorial4(); }
        if (GUILayout.Button("Fix Tutorial 5 (Fluent DSL)")) { log.Clear(); FixTutorial5(); }
        if (GUILayout.Button("Fix Tutorial 6 (FishNet)")) { log.Clear(); FixTutorial6(); }
        if (GUILayout.Button("Fix Tutorial 7 (Fusion 2)")) { log.Clear(); FixTutorial7(); }
        if (GUILayout.Button("Fix Tutorial 9 (Tasks)")) { log.Clear(); FixTutorial9(); }

        GUILayout.Space(10);
        GUILayout.Label("Log:", EditorStyles.boldLabel);
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        foreach (var entry in log)
        {
            GUILayout.Label(entry);
        }
        GUILayout.EndScrollView();
    }

    void Log(string msg)
    {
        log.Add(msg);
        Debug.Log("[TutorialFixer] " + msg);
    }

    void FixAllScenes()
    {
        FixTutorial1();
        FixTutorial2();
        FixTutorial3();
        FixTutorial4();
        FixTutorial5();
        FixTutorial6();
        FixTutorial7();
        FixTutorial9();
        Log("=== ALL DONE ===");
    }

    // ========== TUTORIAL 1 ==========
    // Expected hierarchy:
    // T1_Root (MmRelayNode + T1_ParentController)
    //   +-- T1_Child (MmRelayNode + T1_ChildResponder)
    // T1_TraditionalExample (MmRelayNode + T1_TraditionalApiExample)
    // SceneController (MmRelayNode + T1_SceneController) [legacy]
    void FixTutorial1()
    {
        Log("--- Fixing Tutorial 1 ---");
        var scene = OpenScene("Tutorial1/Tutorial1_Base.unity");
        if (!scene.IsValid()) return;

        // Remove any T4 components from existing objects
        RemoveComponentsOfType<T4_CylinderResponder>();
        RemoveComponentsOfType<T4_SphereHandler>();
        RemoveComponentsOfType<T4_ModernCylinderResponder>();
        RemoveComponentsOfType<T4_ModernSphereHandler>();
        RemoveComponentsOfType<T4_SceneSetup>();

        // Clean up any "Missing Script" references
        CleanMissingScripts();

        // Find or create T1_Root
        var root = FindOrCreateGO("T1_Root", null);
        EnsureComponent<MmRelayNode>(root);
        EnsureComponent<T1_ParentController>(root);

        // Find or create T1_Child under T1_Root
        var child = FindOrCreateGO("T1_Child", root.transform);
        EnsureComponent<MmRelayNode>(child);
        EnsureComponent<T1_ChildResponder>(child);

        // Find or create T1_TraditionalExample
        var trad = FindOrCreateGO("T1_TraditionalExample", null);
        EnsureComponent<MmRelayNode>(trad);
        EnsureComponent<T1_TraditionalApiExample>(trad);

        // Ensure SceneController exists (the legacy T1_SceneController)
        var sc = FindGO("SceneController");
        if (sc != null)
        {
            EnsureComponent<MmRelayNode>(sc);
            EnsureComponent<T1_SceneController>(sc);
        }
        else
        {
            sc = FindOrCreateGO("SceneController", null);
            EnsureComponent<MmRelayNode>(sc);
            EnsureComponent<T1_SceneController>(sc);
        }

        SaveScene(scene);
        Log("Tutorial 1: Fixed");
    }

    // ========== TUTORIAL 2 ==========
    // Expected hierarchy:
    // T2_RoutingDemo (MmRelayNode + T2_RoutingExamples)
    // T2_MenuManager (MmRelayNode + T2_MenuController)
    //   +-- MainMenu (MmRelayNode + T2_ButtonResponder, Tag0)
    //   |     +-- PlayButton (MmRelayNode + T2_ButtonResponder, clickMessage="PlayClicked")
    //   |     +-- SettingsButton (MmRelayNode + T2_ButtonResponder, clickMessage="SettingsClicked")
    //   +-- SettingsMenu (MmRelayNode + T2_ButtonResponder, Tag1)
    //         +-- BackButton (MmRelayNode + T2_ButtonResponder, clickMessage="BackClicked")
    void FixTutorial2()
    {
        Log("--- Fixing Tutorial 2 ---");
        var scene = OpenScene("Tutorial2/Tutorial2_Base.unity");
        if (!scene.IsValid()) return;

        // Create routing demo
        var routingDemo = FindOrCreateGO("T2_RoutingDemo", null);
        EnsureComponent<MmRelayNode>(routingDemo);
        EnsureComponent<T2_RoutingExamples>(routingDemo);

        // Create menu hierarchy
        var menuManager = FindOrCreateGO("T2_MenuManager", null);
        EnsureComponent<MmRelayNode>(menuManager);
        EnsureComponent<T2_MenuController>(menuManager);

        var mainMenu = FindOrCreateGO("MainMenu", menuManager.transform);
        EnsureComponent<MmRelayNode>(mainMenu);
        var mmBR = EnsureComponent<T2_ButtonResponder>(mainMenu);
        mmBR.Tag = MmTag.Tag0;
        mmBR.TagCheckEnabled = true;

        var playBtn = FindOrCreateGO("PlayButton", mainMenu.transform);
        EnsureComponent<MmRelayNode>(playBtn);
        var playResp = EnsureComponent<T2_ButtonResponder>(playBtn);
        playResp.clickMessage = "PlayClicked";

        var settingsBtn = FindOrCreateGO("SettingsButton", mainMenu.transform);
        EnsureComponent<MmRelayNode>(settingsBtn);
        var settingsResp = EnsureComponent<T2_ButtonResponder>(settingsBtn);
        settingsResp.clickMessage = "SettingsClicked";

        var settingsMenu = FindOrCreateGO("SettingsMenu", menuManager.transform);
        EnsureComponent<MmRelayNode>(settingsMenu);
        var smBR = EnsureComponent<T2_ButtonResponder>(settingsMenu);
        smBR.Tag = MmTag.Tag1;
        smBR.TagCheckEnabled = true;

        var backBtn = FindOrCreateGO("BackButton", settingsMenu.transform);
        EnsureComponent<MmRelayNode>(backBtn);
        var backResp = EnsureComponent<T2_ButtonResponder>(backBtn);
        backResp.clickMessage = "BackClicked";

        SaveScene(scene);
        Log("Tutorial 2: Fixed");
    }

    // ========== TUTORIAL 3 ==========
    // Expected hierarchy:
    // T3_GameWorld (MmRelayNode + T3_GameController)
    //   +-- Enemies (MmRelayNode)
    //         +-- Enemy1 (MmRelayNode + T3_EnemyResponderExtendable)
    //         +-- Enemy2 (MmRelayNode + T3_EnemyResponderExtendable)
    //         +-- Enemy3 (MmRelayNode + T3_EnemyResponderExtendable)
    void FixTutorial3()
    {
        Log("--- Fixing Tutorial 3 ---");
        var scene = OpenScene("Tutorial3/Tutorial3_Base.unity");
        if (!scene.IsValid()) return;

        var gameWorld = FindOrCreateGO("T3_GameWorld", null);
        EnsureComponent<MmRelayNode>(gameWorld);
        EnsureComponent<T3_GameController>(gameWorld);

        var enemies = FindOrCreateGO("Enemies", gameWorld.transform);
        EnsureComponent<MmRelayNode>(enemies);

        for (int i = 1; i <= 3; i++)
        {
            var enemy = FindOrCreateGO($"Enemy{i}", enemies.transform);
            EnsureComponent<MmRelayNode>(enemy);
            EnsureComponent<T3_EnemyResponderExtendable>(enemy);
        }

        SaveScene(scene);
        Log("Tutorial 3: Fixed");
    }

    // ========== TUTORIAL 4 ==========
    // Expected hierarchy:
    // T4_LightManager (MmRelayNode + T4_LightController)
    //   +-- Lights (MmRelayNode)
    //         +-- Light1 (MmRelayNode + T4_LightResponder + Light)
    //         +-- Light2 (MmRelayNode + T4_LightResponder + Light)
    //         +-- Light3 (MmRelayNode + T4_LightResponder + Light)
    // T4_SceneSetupGO (T4_SceneSetup) [runtime builder - optional]
    void FixTutorial4()
    {
        Log("--- Fixing Tutorial 4 ---");
        var scene = OpenScene("Tutorial4/Tutorial4_Base.unity");
        if (!scene.IsValid()) return;

        // The primary hierarchy for T4 is the LightController pattern
        var lightManager = FindOrCreateGO("T4_LightManager", null);
        EnsureComponent<MmRelayNode>(lightManager);
        EnsureComponent<T4_LightController>(lightManager);

        var lights = FindOrCreateGO("Lights", lightManager.transform);
        EnsureComponent<MmRelayNode>(lights);

        for (int i = 1; i <= 3; i++)
        {
            var lightGO = FindOrCreateGO($"Light{i}", lights.transform);
            EnsureComponent<MmRelayNode>(lightGO);
            EnsureComponent<T4_LightResponder>(lightGO);
            EnsureComponent<Light>(lightGO);
            // Position the lights
            lightGO.transform.localPosition = new Vector3((i - 2) * 3f, 2f, 0);
        }

        // Also ensure T4_SceneSetup is available as runtime builder
        var sceneSetup = FindOrCreateGO("T4_SceneSetup", null);
        EnsureComponent<T4_SceneSetup>(sceneSetup);
        // Disable auto-create since we already built the hierarchy
        var setup = sceneSetup.GetComponent<T4_SceneSetup>();
        if (setup != null) setup.createOnStart = false;

        SaveScene(scene);
        Log("Tutorial 4: Fixed");
    }

    // ========== TUTORIAL 5 ==========
    // Expected hierarchy:
    // T5_DSLDemo (MmRelayNode + T5_DSLSceneSetup) -- creates children at runtime
    // SceneController (MmRelayNode + T5_SceneController) [legacy, may already exist]
    void FixTutorial5()
    {
        Log("--- Fixing Tutorial 5 ---");
        var scene = OpenScene("Tutorial5/Tutorial5_Base.unity");
        if (!scene.IsValid()) return;

        // T5_DSLSceneSetup creates its own hierarchy at runtime
        var dslDemo = FindOrCreateGO("T5_DSLDemo", null);
        EnsureComponent<MmRelayNode>(dslDemo);
        EnsureComponent<T5_DSLSceneSetup>(dslDemo);

        // Add a T5_DemoResponder on a child for testing
        var demoChild = FindOrCreateGO("T5_DemoChild", dslDemo.transform);
        EnsureComponent<MmRelayNode>(demoChild);
        EnsureComponent<T5_DemoResponder>(demoChild);

        // Ensure legacy SceneController has its component if it exists
        var sc = FindGO("SceneController");
        if (sc != null)
        {
            EnsureComponent<MmRelayNode>(sc);
            // T5_SceneController is already wired per the problem statement
        }

        SaveScene(scene);
        Log("Tutorial 5: Fixed (T5_DSLSceneSetup + T5_DemoResponder added)");
    }

    // ========== TUTORIAL 6 ==========
    // Expected hierarchy:
    // NetworkSetup (T6_NetworkSetup)
    // NetworkRoot (MmRelayNode + T6_NetworkGameController)
    //   +-- Player (MmRelayNode + T6_PlayerResponder)
    void FixTutorial6()
    {
        Log("--- Fixing Tutorial 6 ---");
        var scene = OpenScene("Tutorial6/Tutorial6_FishNet.unity");
        if (!scene.IsValid()) return;

        // Create or find NetworkSetup
        var netSetup = FindOrCreateGO("NetworkSetup", null);
        EnsureComponent<T6_NetworkSetup>(netSetup);

        // Ensure NetworkRoot has the controller
        var netRoot = FindGO("NetworkRoot");
        if (netRoot == null) netRoot = FindOrCreateGO("NetworkRoot", null);
        EnsureComponent<MmRelayNode>(netRoot);
        EnsureComponent<T6_NetworkGameController>(netRoot);

        // Ensure Player has responder
        var player = FindGOInChildren(netRoot, "Player");
        if (player == null) player = FindOrCreateGO("Player", netRoot.transform);
        EnsureComponent<MmRelayNode>(player);
        EnsureComponent<T6_PlayerResponder>(player);

        SaveScene(scene);
        Log("Tutorial 6: Fixed (T6_NetworkSetup added)");
    }

    // ========== TUTORIAL 7 ==========
    // Expected hierarchy:
    // Fusion2Setup (T7_Fusion2Setup)
    // GameRoot (MmRelayNode + T7_Fusion2GameController)
    //   +-- Player1 (MmRelayNode + T7_Fusion2PlayerResponder)
    //   +-- Player2 (MmRelayNode + T7_Fusion2PlayerResponder)
    void FixTutorial7()
    {
        Log("--- Fixing Tutorial 7 ---");
        var scene = OpenScene("Tutorial7/Tutorial7_Fusion2.unity");
        if (!scene.IsValid()) return;

        // Create or find Fusion2Setup
        var fusionSetup = FindOrCreateGO("Fusion2Setup", null);
        EnsureComponent<T7_Fusion2Setup>(fusionSetup);

        // Ensure GameRoot has the controller
        var gameRoot = FindGO("GameRoot");
        if (gameRoot == null) gameRoot = FindOrCreateGO("GameRoot", null);
        EnsureComponent<MmRelayNode>(gameRoot);
        EnsureComponent<T7_Fusion2GameController>(gameRoot);

        // Ensure players have responders
        var player1 = FindGOInChildren(gameRoot, "Player1");
        if (player1 == null) player1 = FindOrCreateGO("Player1", gameRoot.transform);
        EnsureComponent<MmRelayNode>(player1);
        EnsureComponent<T7_Fusion2PlayerResponder>(player1);

        var player2 = FindGOInChildren(gameRoot, "Player2");
        if (player2 == null) player2 = FindOrCreateGO("Player2", gameRoot.transform);
        EnsureComponent<MmRelayNode>(player2);
        EnsureComponent<T7_Fusion2PlayerResponder>(player2);

        SaveScene(scene);
        Log("Tutorial 7: Fixed (T7_Fusion2Setup added)");
    }

    // ========== TUTORIAL 9 ==========
    // Expected hierarchy:
    // T9_Experiment (MmRelayNode)
    //   +-- TaskManager (T9_ExperimentManager + T9_JsonTaskLoader)
    //         +-- TasksNode (MmRelaySwitchNode)
    //               +-- Instructions (MmRelayNode + T9_TrialResponder)
    //               +-- Practice (MmRelayNode + T9_TrialResponder)
    //               +-- MainTrials (MmRelayNode + T9_TrialResponder)
    //               +-- Complete (MmRelayNode + T9_TrialResponder)
    // T9_MyTaskInfo is a plain C# class, not a MonoBehaviour - no scene placement needed.
    void FixTutorial9()
    {
        Log("--- Fixing Tutorial 9 ---");
        var scene = OpenScene("Tutorial9/Tutorial9_Tasks.unity");
        if (!scene.IsValid()) return;

        // T9_JsonTaskLoader needs to be on the TaskManager
        var taskMgr = FindGO("TaskManager");
        if (taskMgr != null)
        {
            EnsureComponent<T9_JsonTaskLoader>(taskMgr);
            Log("  Added T9_JsonTaskLoader to TaskManager");
        }
        else
        {
            // Build full hierarchy if TaskManager doesn't exist
            var experiment = FindOrCreateGO("T9_Experiment", null);
            EnsureComponent<MmRelayNode>(experiment);

            taskMgr = FindOrCreateGO("TaskManager", experiment.transform);
            EnsureComponent<T9_ExperimentManager>(taskMgr);
            EnsureComponent<T9_JsonTaskLoader>(taskMgr);

            var tasksNode = FindOrCreateGO("TasksNode", taskMgr.transform);
            EnsureComponent<MmRelaySwitchNode>(tasksNode);

            string[] taskNames = { "Instructions", "Practice", "MainTrials", "Complete" };
            foreach (var tn in taskNames)
            {
                var taskGO = FindOrCreateGO(tn, tasksNode.transform);
                EnsureComponent<MmRelayNode>(taskGO);
                EnsureComponent<T9_TrialResponder>(taskGO);
            }
        }

        SaveScene(scene);
        Log("Tutorial 9: Fixed (T9_JsonTaskLoader added)");
    }

    // ========== HELPERS ==========

    Scene OpenScene(string relativePath)
    {
        string fullPath = $"{TUTORIALS_PATH}/{relativePath}";
        Log($"Opening scene: {fullPath}");

        if (!System.IO.File.Exists(fullPath))
        {
            Log($"  ERROR: Scene not found at {fullPath}");
            return default;
        }

        return EditorSceneManager.OpenScene(fullPath, OpenSceneMode.Single);
    }

    void SaveScene(Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Log($"  Saved scene: {scene.path}");
    }

    GameObject FindGO(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name == name) return go;
        }
        return null;
    }

    GameObject FindGOInChildren(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }

    GameObject FindOrCreateGO(string name, Transform parent)
    {
        // Search in parent's children first
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child.gameObject;
            }
        }
        else
        {
            // Search root objects
            var existing = FindGO(name);
            if (existing != null && existing.transform.parent == null)
                return existing;
        }

        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        Log($"  Created GameObject: {name}" + (parent != null ? $" under {parent.name}" : ""));
        return go;
    }

    T EnsureComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
            Log($"  Added {typeof(T).Name} to {go.name}");
        }
        return comp;
    }

    void RemoveComponentsOfType<T>() where T : Component
    {
        var components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var comp in components)
        {
            Log($"  Removing {typeof(T).Name} from {comp.gameObject.name}");
            DestroyImmediate(comp);
        }
    }

    /// <summary>
    /// Removes any "Missing Script" components from all GameObjects in the scene.
    /// Uses GameObjectUtility.RemoveMonoBehavioursWithMissingScript().
    /// </summary>
    void CleanMissingScripts()
    {
        var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int totalRemoved = 0;
        foreach (var go in allGOs)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                Log($"  Removed {removed} missing script(s) from {go.name}");
                totalRemoved += removed;
            }
        }
        if (totalRemoved > 0)
        {
            Log($"  Total missing scripts removed: {totalRemoved}");
        }
    }
}
