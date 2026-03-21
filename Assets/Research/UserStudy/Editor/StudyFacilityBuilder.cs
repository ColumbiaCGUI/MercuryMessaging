// Suppress MM006: This is an Editor scene builder - MmRefreshResponders is called at runtime during Awake.
// Suppress MM008: SetParent establishes Unity's Transform hierarchy, routing is auto-discovered at runtime.
#pragma warning disable MM006
#pragma warning disable MM008

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MercuryMessaging;
using MercuryMessaging.Research.UserStudy;

namespace MercuryMessaging.Research.UserStudy.Editor
{
    /// <summary>
    /// Automated scene builder for the UIST User Study facility scene.
    /// Creates two workspaces (Robot Teleoperation + IoT Monitoring) with all
    /// GameObjects, components, materials, and UI wired up automatically.
    /// </summary>
    public class StudyFacilityBuilder : EditorWindow
    {
        [MenuItem("MercuryMessaging/User Study/Build Study Facility Scene")]
        public static void BuildScene()
        {
            if (EditorUtility.DisplayDialog(
                "Build Study Facility Scene",
                "This will create the Study Facility scene with both workspaces, all components, and UI.\n\nContinue?",
                "Yes", "Cancel"))
            {
                CreateScene();
            }
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Debug.Log("[StudyFacilityBuilder] Building Study Facility Scene...");

            // ── Materials ────────────────────────────────────────────────
            string matDir = "Assets/Research/UserStudy/Materials";
            System.IO.Directory.CreateDirectory(matDir);

            Material robotArmMat       = CreateMaterial("RobotArmMat",        new Color(0.25f, 0.25f, 0.25f), matDir);
            Material safetyWarningMat  = CreateTransparentMaterial("SafetyWarningMat",  new Color(1f, 0.92f, 0f, 0.15f), matDir);
            Material safetyEmergencyMat = CreateTransparentMaterial("SafetyEmergencyMat", new Color(1f, 0.1f, 0.1f, 0.15f), matDir);
            Material floorMat          = CreateMaterial("FloorMat",            new Color(0.78f, 0.78f, 0.78f), matDir);
            Material hvacMat           = CreateMaterial("HvacMat",             new Color(0.5f, 0.5f, 0.5f), matDir);
            Material sensorMat         = CreateMaterial("SensorMat",           new Color(0.1f, 0.15f, 0.5f), matDir);
            Material dashboardBgMat    = CreateMaterial("DashboardBgMat",      new Color(0.05f, 0.07f, 0.2f), matDir);
            Material indicatorDefaultMat = CreateMaterial("IndicatorDefaultMat", Color.green, matDir);
            Material dividerMat        = CreateMaterial("DividerMat",          new Color(0.88f, 0.88f, 0.88f), matDir);

            // ── Environment ──────────────────────────────────────────────
            CreateEnvironment(floorMat, dividerMat);

            // ── Robot Workspace (T1 + T2) ─────────────────────────────────
            GameObject robotWorkspace = CreateRobotWorkspace(robotArmMat, safetyWarningMat, safetyEmergencyMat, indicatorDefaultMat);

            // ── IoT Area (T3 + T4) ────────────────────────────────────────
            GameObject iotArea = CreateIoTArea(hvacMat, sensorMat, dashboardBgMat);

            // ── Study Infrastructure ──────────────────────────────────────
            CreateStudyInfrastructure(robotWorkspace, iotArea);

            // ── Camera ────────────────────────────────────────────────────
            SetupCamera();

            // ── EventSystem ───────────────────────────────────────────────
            EnsureEventSystem();

            // ── Save ──────────────────────────────────────────────────────
            string scenesDir = "Assets/Research/UserStudy/Scenes";
            System.IO.Directory.CreateDirectory(scenesDir);
            string scenePath = $"{scenesDir}/StudyFacility.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"[StudyFacilityBuilder] Scene saved to: {scenePath}");

            EditorUtility.DisplayDialog(
                "Scene Built Successfully",
                $"Study Facility scene created!\n\nLocation: {scenePath}\n\n" +
                "Workspaces:\n" +
                "  Left (x=-8): Robot Teleoperation (T1 + T2)\n" +
                "  Right (x=+8): IoT Monitoring (T3 + T4)\n\n" +
                "Next steps:\n" +
                "1. Open the scene\n" +
                "2. Verify hierarchy in Inspector\n" +
                "3. Press Play to test",
                "OK");
        }

        // ═══════════════════════════════════════════════════════════════
        // MATERIALS
        // ═══════════════════════════════════════════════════════════════

        private static Material CreateMaterial(string name, Color color, string dir)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(mat, $"{dir}/{name}.mat");
            return mat;
        }

        private static Material CreateTransparentMaterial(string name, Color color, string dir)
        {
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(shader) { name = name };

            // Standard shader transparent setup
            mat.SetFloat("_Mode", 3f);  // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;

            AssetDatabase.CreateAsset(mat, $"{dir}/{name}.mat");
            return mat;
        }

        // ═══════════════════════════════════════════════════════════════
        // ENVIRONMENT
        // ═══════════════════════════════════════════════════════════════

        private static void CreateEnvironment(Material floorMat, Material dividerMat)
        {
            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(20f, 1f, 20f);
            if (floorMat != null) floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Divider wall between workspaces
            GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "WorkspaceDivider";
            divider.transform.position = new Vector3(0f, 1.5f, 0f);
            divider.transform.localScale = new Vector3(0.1f, 3f, 10f);
            if (dividerMat != null) divider.GetComponent<Renderer>().sharedMaterial = dividerMat;
        }

        private static void SetupCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;
            mainCam.transform.position = new Vector3(0f, 5f, 10f);
            mainCam.transform.LookAt(Vector3.zero);
        }

        private static void EnsureEventSystem()
        {
            if (GameObject.Find("EventSystem") != null) return;

            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ═══════════════════════════════════════════════════════════════
        // ROBOT WORKSPACE  (T1: Sensor Fan-Out, T2: Safety Zone)
        // ═══════════════════════════════════════════════════════════════

        private static GameObject CreateRobotWorkspace(
            Material robotArmMat,
            Material safetyWarningMat,
            Material safetyEmergencyMat,
            Material indicatorDefaultMat)
        {
            // Root workspace container
            GameObject workspace = new GameObject("RobotWorkspace");
            workspace.transform.position = new Vector3(-8f, 0f, 0f);

            // ── Robot Arm ─────────────────────────────────────────────────
            GameObject robotArm = new GameObject("RobotArm");
            robotArm.transform.SetParent(workspace.transform);
            robotArm.transform.localPosition = Vector3.zero;
            robotArm.AddComponent<MmRelayNode>();
            var jointDataSource = robotArm.AddComponent<JointDataSource>();

            CreateRobotArmVisuals(robotArm, robotArmMat);

            // ── Joint Display Panels (T1 targets) ─────────────────────────
            // Panels are children of RobotArm so Mercury broadcasts reach them
            Vector3[] panelPositions = new Vector3[]
            {
                new Vector3(-9f, 1.5f,  1f),  // Panel1: front-left
                new Vector3(-7f, 1.5f,  1f),  // Panel2: front-right
                new Vector3(-9f, 1.5f, -1f),  // Panel3: back-left
                new Vector3(-7f, 1.5f, -1f),  // Panel4: back-right
            };

            GameObject[] panels = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                panels[i] = CreateJointDisplayPanel(robotArm, $"JointDisplayPanel{i + 1}", panelPositions[i]);
            }

            // Wire JointDataSource.OnJointAngleChanged → JointDataBroadcaster_Starter.SendJointData
            // (Participants implement JointDataBroadcaster_Starter.SendJointData, which is already on RobotArm
            //  via its MmRelayNode. JointDataSource fires the event, starter sends Mercury message.)
            // We add the starter here; participants fill in SendJointData().
            var t1Starter = robotArm.AddComponent<JointDataBroadcaster_Starter>();
            // Wire UnityEvent: JointDataSource.OnJointAngleChanged → t1Starter.SendJointData
            SerializedObject jdsSO = new SerializedObject(jointDataSource);
            // UnityEvent wiring via persistent listener
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                jointDataSource.OnJointAngleChanged, t1Starter.SendJointData);

            // Also add Events-condition broadcaster
            var t1EventsStarter = robotArm.AddComponent<JointDataBroadcaster_Events_Starter>();
            // Wire event for Events condition too
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                jointDataSource.OnJointAngleChanged, t1EventsStarter.SendJointData);
            t1EventsStarter.enabled = false; // disabled by default; enable for Events condition

            // ── Safety Zones (T2 visual props) ────────────────────────────
            CreateSafetyZones(workspace, safetyWarningMat, safetyEmergencyMat);

            // ── Safety Indicators (T2 targets) ────────────────────────────
            Vector3[] indicatorPositions = new Vector3[]
            {
                new Vector3(-10f, 0.5f,  0f),
                new Vector3( -9f, 0.5f,  2f),
                new Vector3( -7f, 0.5f, -1f),
                new Vector3( -6f, 0.5f,  1f),
            };

            GameObject[] indicators = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                indicators[i] = CreateSafetyIndicator(
                    workspace, $"SafetyIndicator{i + 1}", indicatorPositions[i], indicatorDefaultMat);
            }

            // ── Worker (T2 stimulus) ───────────────────────────────────────
            GameObject worker = CreateWorker(workspace);

            // ── ZoneAlertManager (T2 task scripts) ────────────────────────
            // Attach to RobotArm (which has MmRelayNode) so spatial filtering works
            var t2Starter = robotArm.AddComponent<ZoneAlertManager_Starter>();
            SerializedObject t2SO = new SerializedObject(t2Starter);
            t2SO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            t2SO.ApplyModifiedProperties();

            var t2EventsStarter = robotArm.AddComponent<ZoneAlertManager_Events_Starter>();
            SerializedObject t2EvSO = new SerializedObject(t2EventsStarter);
            t2EvSO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            t2EvSO.ApplyModifiedProperties();
            t2EventsStarter.enabled = false;

            return workspace;
        }

        private static void CreateRobotArmVisuals(GameObject robotArm, Material mat)
        {
            // Base — cylinder flat disc
            GameObject baseSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseSegment.name = "ArmBase";
            baseSegment.transform.SetParent(robotArm.transform);
            baseSegment.transform.localPosition = new Vector3(0f, 0f, 0f);
            baseSegment.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            if (mat != null) baseSegment.GetComponent<Renderer>().sharedMaterial = mat;

            // Shoulder — capsule
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shoulder.name = "ArmShoulder";
            shoulder.transform.SetParent(robotArm.transform);
            shoulder.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            shoulder.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            if (mat != null) shoulder.GetComponent<Renderer>().sharedMaterial = mat;

            // Elbow — capsule
            GameObject elbow = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            elbow.name = "ArmElbow";
            elbow.transform.SetParent(robotArm.transform);
            elbow.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            elbow.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            if (mat != null) elbow.GetComponent<Renderer>().sharedMaterial = mat;

            // Wrist — capsule
            GameObject wrist = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            wrist.name = "ArmWrist";
            wrist.transform.SetParent(robotArm.transform);
            wrist.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            wrist.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
            if (mat != null) wrist.GetComponent<Renderer>().sharedMaterial = mat;

            // Gripper — cube
            GameObject gripper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gripper.name = "ArmGripper";
            gripper.transform.SetParent(robotArm.transform);
            gripper.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            gripper.transform.localScale = new Vector3(0.2f, 0.05f, 0.15f);
            if (mat != null) gripper.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static GameObject CreateJointDisplayPanel(GameObject parent, string name, Vector3 worldPosition)
        {
            // Panel root — holds Mercury + Events components
            GameObject panelRoot = new GameObject(name);
            panelRoot.transform.SetParent(parent.transform);
            panelRoot.transform.position = worldPosition;

            // Mercury components
            panelRoot.AddComponent<MmRelayNode>();
            var display = panelRoot.AddComponent<JointAngleDisplay>();

            // Events component (disabled by default)
            var displayEvents = panelRoot.AddComponent<JointAngleDisplay_Events>();
            displayEvents.enabled = false;

            // World-space Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(panelRoot.transform);
            canvasObj.transform.localPosition = Vector3.zero;
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(40f, 30f); // 0.4m × 0.3m at 0.01 scale

            // Background panel image
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRT = bgPanel.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Angle text
            GameObject textObj = new GameObject("AngleText");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.1f, 0.4f);
            textRT.anchorMax = new Vector2(0.9f, 0.9f);
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI angleText = textObj.AddComponent<TextMeshProUGUI>();
            angleText.text = "0.0°";
            angleText.fontSize = 8;
            angleText.alignment = TextAlignmentOptions.Center;
            angleText.color = Color.white;

            // Status indicator image
            GameObject statusObj = new GameObject("StatusIndicator");
            statusObj.transform.SetParent(canvasObj.transform, false);
            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0.1f, 0.05f);
            statusRT.anchorMax = new Vector2(0.9f, 0.35f);
            statusRT.sizeDelta = Vector2.zero;
            Image statusImg = statusObj.AddComponent<Image>();
            statusImg.color = Color.green;

            // Wire UI references to both display components
            SerializedObject mmSO = new SerializedObject(display);
            mmSO.FindProperty("angleText").objectReferenceValue = angleText;
            mmSO.FindProperty("statusIndicator").objectReferenceValue = statusImg;
            mmSO.ApplyModifiedProperties();

            SerializedObject evSO = new SerializedObject(displayEvents);
            evSO.FindProperty("angleText").objectReferenceValue = angleText;
            evSO.FindProperty("statusIndicator").objectReferenceValue = statusImg;
            evSO.ApplyModifiedProperties();

            return panelRoot;
        }

        private static void CreateSafetyZones(GameObject workspace, Material warningMat, Material emergencyMat)
        {
            // Warning zone — 2m radius sphere (scale 4 = diameter 4m → radius 2m)
            GameObject warnZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warnZone.name = "SafetyZoneWarning";
            warnZone.transform.SetParent(workspace.transform);
            warnZone.transform.localPosition = Vector3.zero; // centered on robot base at workspace origin
            warnZone.transform.localScale = new Vector3(4f, 4f, 4f);
            Object.DestroyImmediate(warnZone.GetComponent<SphereCollider>());
            if (warningMat != null) warnZone.GetComponent<Renderer>().sharedMaterial = warningMat;

            // Emergency zone — 1m radius sphere (scale 2 = diameter 2m → radius 1m)
            GameObject emergencyZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emergencyZone.name = "SafetyZoneEmergency";
            emergencyZone.transform.SetParent(workspace.transform);
            emergencyZone.transform.localPosition = Vector3.zero;
            emergencyZone.transform.localScale = new Vector3(2f, 2f, 2f);
            Object.DestroyImmediate(emergencyZone.GetComponent<SphereCollider>());
            if (emergencyMat != null) emergencyZone.GetComponent<Renderer>().sharedMaterial = emergencyMat;
        }

        private static GameObject CreateSafetyIndicator(
            GameObject parent, string name, Vector3 worldPosition, Material defaultMat)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.name = name;
            indicator.transform.SetParent(parent.transform);
            indicator.transform.position = worldPosition;
            indicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            if (defaultMat != null) indicator.GetComponent<Renderer>().sharedMaterial = defaultMat;

            // Mercury components
            indicator.AddComponent<MmRelayNode>();
            var mmIndicator = indicator.AddComponent<SafetyZoneIndicator>();

            // Events component (disabled by default)
            var evIndicator = indicator.AddComponent<SafetyZoneIndicator_Events>();
            evIndicator.enabled = false;

            // Wire Renderer reference into both
            Renderer rend = indicator.GetComponent<Renderer>();
            SerializedObject mmSO = new SerializedObject(mmIndicator);
            mmSO.FindProperty("indicatorRenderer").objectReferenceValue = rend;
            mmSO.ApplyModifiedProperties();

            SerializedObject evSO = new SerializedObject(evIndicator);
            evSO.FindProperty("indicatorRenderer").objectReferenceValue = rend;
            evSO.ApplyModifiedProperties();

            return indicator;
        }

        private static GameObject CreateWorker(GameObject workspace)
        {
            GameObject worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "Worker";
            worker.transform.SetParent(workspace.transform);
            worker.transform.position = new Vector3(-8f, 0f, 3f);
            worker.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            worker.AddComponent<WorkerMovement>();
            return worker;
        }

        // ═══════════════════════════════════════════════════════════════
        // IoT AREA  (T3: Mode Switch / HVAC Bug, T4: Alert Aggregation)
        // ═══════════════════════════════════════════════════════════════

        private static GameObject CreateIoTArea(Material hvacMat, Material sensorMat, Material dashboardBgMat)
        {
            GameObject iotArea = new GameObject("IoTArea");
            iotArea.transform.position = new Vector3(8f, 0f, 0f);

            // ── FacilityHub (root relay for IoT area) ─────────────────────
            GameObject facilityHub = new GameObject("FacilityHub");
            facilityHub.transform.SetParent(iotArea.transform);
            facilityHub.transform.localPosition = Vector3.zero;
            facilityHub.AddComponent<MmRelayNode>();
            var modeController = facilityHub.AddComponent<FacilityModeController>();

            // ── CentralDashboard ─────────────────────────────────────────
            // Must be ANCESTOR of subsystems so NotifyValue() sends up to it.
            GameObject dashboardNode = new GameObject("CentralDashboard");
            dashboardNode.transform.SetParent(facilityHub.transform);
            dashboardNode.transform.localPosition = Vector3.zero;
            dashboardNode.AddComponent<MmRelayNode>();
            var mmDashboard = dashboardNode.AddComponent<CentralDashboard>();
            var evDashboard = dashboardNode.AddComponent<CentralDashboard_Events>();

            // Dashboard world-space canvas
            TextMeshProUGUI dashboardLogText = CreateDashboardCanvas(dashboardNode, dashboardBgMat);

            SerializedObject mmDashSO = new SerializedObject(mmDashboard);
            mmDashSO.FindProperty("alertLogText").objectReferenceValue = dashboardLogText;
            mmDashSO.ApplyModifiedProperties();

            SerializedObject evDashSO = new SerializedObject(evDashboard);
            evDashSO.FindProperty("alertLogText").objectReferenceValue = dashboardLogText;
            evDashSO.ApplyModifiedProperties();

            // ── HVAC System (T3) ──────────────────────────────────────────
            GameObject hvacSystem = CreateHvacSystem(dashboardNode, hvacMat);

            // ── TemperatureSimulator (child of FacilityHub) ───────────────
            GameObject tempSimObj = new GameObject("TemperatureSimulator");
            tempSimObj.transform.SetParent(dashboardNode.transform);
            tempSimObj.transform.localPosition = new Vector3(0f, 0f, 2f);
            var tempSim = tempSimObj.AddComponent<TemperatureSimulator>();
            // Wire to HVAC relay so Mercury condition works
            var hvacRelay = hvacSystem.GetComponent<MmRelayNode>();
            SerializedObject tempSimSO = new SerializedObject(tempSim);
            tempSimSO.FindProperty("hvacRelay").objectReferenceValue = hvacRelay;
            tempSimSO.ApplyModifiedProperties();

            // ── Subsystems (T4): children of CentralDashboard ─────────────
            GameObject occupancy = CreateSubsystem(dashboardNode, "OccupancySensor",
                new Vector3(0f, 2f, 0f), PrimitiveType.Sphere, sensorMat,
                "Occupancy",
                new string[] { "Zone occupied", "Movement detected", "Occupancy threshold exceeded" });

            GameObject airQuality = CreateSubsystem(dashboardNode, "AirQualitySensor",
                new Vector3(2f, 1f, 0f), PrimitiveType.Cube, sensorMat,
                "AirQuality",
                new string[] { "CO2 above limit", "Particulate spike", "Ventilation fault" });

            GameObject energy = CreateSubsystem(dashboardNode, "EnergyMonitor",
                new Vector3(2f, 1.5f, -1f), PrimitiveType.Cube, sensorMat,
                "Energy",
                new string[] { "Peak usage warning", "Circuit overload", "Battery low" });

            // ── FacilityModeController: wire relay reference ──────────────
            SerializedObject modeSO = new SerializedObject(modeController);
            modeSO.FindProperty("facilityRelay").objectReferenceValue = facilityHub.GetComponent<MmRelayNode>();
            modeSO.ApplyModifiedProperties();

            // ── Mode Toggle button ─────────────────────────────────────────
            CreateModeToggle(iotArea, modeController);

            return iotArea;
        }

        private static GameObject CreateHvacSystem(GameObject parent, Material hvacMat)
        {
            GameObject hvac = new GameObject("HvacSystem");
            hvac.transform.SetParent(parent.transform);
            hvac.transform.position = new Vector3(6f, 1f, 0f);

            hvac.AddComponent<MmRelayNode>();
            hvac.AddComponent<MmBaseResponder>();

            // Task scripts — Buggy (Mercury) enabled, Events buggy disabled
            var mmBuggy = hvac.AddComponent<HvacController_Buggy>();
            var evBuggy = hvac.AddComponent<HvacController_Events_Buggy>();
            evBuggy.enabled = false;

            // Visual: cube representing HVAC unit
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HvacVisual";
            visual.transform.SetParent(hvac.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.8f, 1.2f, 0.3f);
            if (hvacMat != null) visual.GetComponent<Renderer>().sharedMaterial = hvacMat;

            // World-space label canvas
            TextMeshProUGUI hvacLabel = CreateSmallLabelCanvas(hvac, "HVAC", new Vector3(0f, 1.2f, 0f));

            return hvac;
        }

        private static GameObject CreateSubsystem(
            GameObject parent,
            string name,
            Vector3 localPosition,
            PrimitiveType primitiveType,
            Material mat,
            string subsystemName,
            string[] alerts)
        {
            GameObject subsystem = new GameObject(name);
            subsystem.transform.SetParent(parent.transform);
            subsystem.transform.localPosition = localPosition;

            subsystem.AddComponent<MmRelayNode>();
            subsystem.AddComponent<MmBaseResponder>();

            // T4 task scripts
            var mmAlerter = subsystem.AddComponent<SubsystemAlerter_Starter>();
            SerializedObject mmAlerterSO = new SerializedObject(mmAlerter);
            mmAlerterSO.FindProperty("subsystemName").stringValue = subsystemName;
            mmAlerterSO.ApplyModifiedProperties();

            var evAlerter = subsystem.AddComponent<SubsystemAlerter_Events_Starter>();
            SerializedObject evAlerterSO = new SerializedObject(evAlerter);
            evAlerterSO.FindProperty("subsystemName").stringValue = subsystemName;
            evAlerterSO.ApplyModifiedProperties();
            evAlerter.enabled = false;

            // Alert simulator
            var alertSim = subsystem.AddComponent<AlertSimulator>();
            alertSim.subsystemName = subsystemName;
            alertSim.possibleAlerts = alerts;

            // Wire AlertSimulator.OnAlertGenerated → SubsystemAlerter_Starter.RaiseAlert
            // UnityEvent<string,int> requires two-arg persistent listener wiring via SerializedObject.
            // We set this up via the serialized event property so it appears in the Inspector.
            SerializedObject alertSimSO = new SerializedObject(alertSim);
            SerializedProperty onAlertProp = alertSimSO.FindProperty("OnAlertGenerated");
            if (onAlertProp != null)
            {
                SerializedProperty callsProp = onAlertProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
                if (callsProp != null)
                {
                    callsProp.arraySize++;
                    SerializedProperty call = callsProp.GetArrayElementAtIndex(callsProp.arraySize - 1);
                    call.FindPropertyRelative("m_Target").objectReferenceValue = mmAlerter;
                    call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
                        mmAlerter.GetType().AssemblyQualifiedName;
                    call.FindPropertyRelative("m_MethodName").stringValue = "RaiseAlert";
                    call.FindPropertyRelative("m_Mode").intValue = (int)UnityEngine.Events.PersistentListenerMode.EventDefined;
                    call.FindPropertyRelative("m_CallState").intValue =
                        (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
                    alertSimSO.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogWarning($"[StudyFacilityBuilder] Could not find m_PersistentCalls on {subsystemName} AlertSimulator. " +
                                     $"Wire AlertSimulator.OnAlertGenerated → SubsystemAlerter_Starter.RaiseAlert manually.");
                }
            }

            // Visual primitive
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "Visual";
            visual.transform.SetParent(subsystem.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            if (mat != null) visual.GetComponent<Renderer>().sharedMaterial = mat;

            // Label
            CreateSmallLabelCanvas(subsystem, subsystemName, new Vector3(0f, 0.5f, 0f));

            return subsystem;
        }

        private static void CreateModeToggle(GameObject iotArea, FacilityModeController controller)
        {
            GameObject toggleBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toggleBtn.name = "ModeToggle";
            toggleBtn.transform.SetParent(iotArea.transform);
            toggleBtn.transform.position = new Vector3(7f, 1f, 2f);
            toggleBtn.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);

            // World-space canvas label
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(toggleBtn.transform);
            canvasObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            canvasObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();

            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100f, 30f);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "DAY/NIGHT";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        // ─────────────────────────────────────────────────────────────────
        // Dashboard world-space canvas (large, 1.5×1m)
        // ─────────────────────────────────────────────────────────────────

        private static TextMeshProUGUI CreateDashboardCanvas(GameObject parent, Material bgMat)
        {
            GameObject canvasObj = new GameObject("DashboardCanvas");
            canvasObj.transform.SetParent(parent.transform);
            canvasObj.transform.position = new Vector3(8f, 1.5f, -2f);
            canvasObj.transform.localRotation = Quaternion.identity;
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(150f, 100f); // 1.5m × 1m at 0.01 scale

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.07f, 0.2f, 0.95f);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasObj.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.88f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "CENTRAL DASHBOARD";
            titleText.fontSize = 8;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.cyan;

            // Alert log text area
            GameObject logObj = new GameObject("AlertLog");
            logObj.transform.SetParent(canvasObj.transform, false);
            RectTransform logRT = logObj.AddComponent<RectTransform>();
            logRT.anchorMin = new Vector2(0.02f, 0.02f);
            logRT.anchorMax = new Vector2(0.98f, 0.86f);
            logRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI logText = logObj.AddComponent<TextMeshProUGUI>();
            logText.text = "(no alerts yet)";
            logText.fontSize = 5;
            logText.alignment = TextAlignmentOptions.TopLeft;
            logText.color = Color.white;
            logText.overflowMode = TextOverflowModes.Truncate;

            return logText;
        }

        // ─────────────────────────────────────────────────────────────────
        // Small label canvas helper (used for HVAC, sensors, etc.)
        // ─────────────────────────────────────────────────────────────────

        private static TextMeshProUGUI CreateSmallLabelCanvas(GameObject parent, string labelText, Vector3 localOffset)
        {
            GameObject canvasObj = new GameObject("LabelCanvas");
            canvasObj.transform.SetParent(parent.transform);
            canvasObj.transform.localPosition = localOffset;
            canvasObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();

            RectTransform rt = canvasObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80f, 20f);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(canvasObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 10;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return tmp;
        }

        // ═══════════════════════════════════════════════════════════════
        // STUDY INFRASTRUCTURE
        // ═══════════════════════════════════════════════════════════════

        private static void CreateStudyInfrastructure(GameObject robotWorkspace, GameObject iotArea)
        {
            GameObject infrastructure = new GameObject("StudyInfrastructure");

            // StudyManager
            GameObject smObj = new GameObject("StudyManager");
            smObj.transform.SetParent(infrastructure.transform);
            var studyManager = smObj.AddComponent<StudyManager>();

            // CorrectnessChecker
            GameObject ccObj = new GameObject("CorrectnessChecker");
            ccObj.transform.SetParent(infrastructure.transform);
            var checker = ccObj.AddComponent<CorrectnessChecker>();

            // Wire T1 panels to CorrectnessChecker
            var t1Panels = new System.Collections.Generic.List<GameObject>();
            for (int i = 1; i <= 4; i++)
            {
                var panelGO = GameObject.Find($"JointDisplayPanel{i}");
                if (panelGO != null) t1Panels.Add(panelGO);
            }

            // Wire T2 indicators and worker
            var t2Indicators = new System.Collections.Generic.List<GameObject>();
            for (int i = 1; i <= 4; i++)
            {
                var indGO = GameObject.Find($"SafetyIndicator{i}");
                if (indGO != null) t2Indicators.Add(indGO);
            }

            GameObject workerGO = GameObject.Find("Worker");
            GameObject facilityHubGO = GameObject.Find("FacilityHub");
            GameObject hvacGO = GameObject.Find("HvacSystem");
            GameObject dashboardGO = GameObject.Find("CentralDashboard");

            SerializedObject ccSO = new SerializedObject(checker);

            // T1 panels array
            var t1PanelsProp = ccSO.FindProperty("t1Panels");
            t1PanelsProp.arraySize = t1Panels.Count;
            for (int i = 0; i < t1Panels.Count; i++)
                t1PanelsProp.GetArrayElementAtIndex(i).objectReferenceValue = t1Panels[i];

            // T2 worker
            if (workerGO != null)
                ccSO.FindProperty("t2Worker").objectReferenceValue = workerGO.transform;

            // T2 test positions (create invisible markers)
            var testPositions = CreateT2TestPositions(robotWorkspace);
            var t2PosProp = ccSO.FindProperty("t2TestPositions");
            t2PosProp.arraySize = testPositions.Length;
            for (int i = 0; i < testPositions.Length; i++)
                t2PosProp.GetArrayElementAtIndex(i).objectReferenceValue = testPositions[i];

            // T2 indicators array
            var t2IndProp = ccSO.FindProperty("t2Indicators");
            t2IndProp.arraySize = t2Indicators.Count;
            for (int i = 0; i < t2Indicators.Count; i++)
                t2IndProp.GetArrayElementAtIndex(i).objectReferenceValue = t2Indicators[i];

            // T3 references
            if (facilityHubGO != null)
                ccSO.FindProperty("t3ModeController").objectReferenceValue =
                    facilityHubGO.GetComponent<FacilityModeController>();

            // T4 dashboard
            if (dashboardGO != null)
                ccSO.FindProperty("t4Dashboard").objectReferenceValue = dashboardGO;

            ccSO.ApplyModifiedProperties();

            // ── Screen-space overlay UI ────────────────────────────────────
            CreateStudyUI(studyManager, checker);
        }

        private static Transform[] CreateT2TestPositions(GameObject robotWorkspace)
        {
            // Three positions relative to robot arm base at (-8, 0, 0)
            // far: >2m from robot center, mid: 1-2m, close: <1m
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-8f, 0f,  4f),  // far: 4m from base
                new Vector3(-8f, 0f,  1.5f), // mid: 1.5m from base
                new Vector3(-8f, 0f,  0.5f), // close: 0.5m from base
            };

            Transform[] markers = new Transform[positions.Length];
            string[] names = new string[] { "T2_TestPos_Far", "T2_TestPos_Mid", "T2_TestPos_Close" };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject marker = new GameObject(names[i]);
                marker.transform.SetParent(robotWorkspace.transform);
                marker.transform.position = positions[i];
                markers[i] = marker.transform;
            }

            return markers;
        }

        private static void CreateStudyUI(StudyManager studyManager, CorrectnessChecker checker)
        {
            // Ensure EventSystem
            EnsureEventSystem();

            // Root canvas
            GameObject canvasObj = new GameObject("StudyUI");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObj.AddComponent<GraphicRaycaster>();

            // ── Status Text (top center) ───────────────────────────────────
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(canvasObj.transform, false);
            RectTransform statusRT = statusObj.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 1f);
            statusRT.anchorMax = new Vector2(0.7f, 1f);
            statusRT.pivot = new Vector2(0f, 1f);
            statusRT.anchoredPosition = new Vector2(10f, -10f);
            statusRT.sizeDelta = new Vector2(0f, 60f);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Status: Ready";
            statusText.fontSize = 20;
            statusText.fontStyle = FontStyles.Bold;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.TopLeft;

            // ── Timer Text (top right) ─────────────────────────────────────
            GameObject timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform timerRT = timerObj.AddComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(1f, 1f);
            timerRT.anchorMax = new Vector2(1f, 1f);
            timerRT.pivot = new Vector2(1f, 1f);
            timerRT.anchoredPosition = new Vector2(-10f, -10f);
            timerRT.sizeDelta = new Vector2(150f, 60f);
            TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "08:00";
            timerText.fontSize = 28;
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = Color.white;
            timerText.alignment = TextAlignmentOptions.TopRight;

            // ── Task Instruction Panel (bottom, collapsible) ───────────────
            GameObject instrPanel = CreateInstructionPanel(canvasObj, out TextMeshProUGUI instrText);

            // ── Buttons ───────────────────────────────────────────────────
            GameObject startBtn  = CreateStudyButton(canvasObj, "StartButton",  "Start Study",
                new Vector2(-80f, 80f), new Color(0.2f, 0.6f, 0.2f));
            GameObject submitBtn = CreateStudyButton(canvasObj, "SubmitButton", "Submit Task",
                new Vector2(80f, 80f), new Color(0.2f, 0.3f, 0.7f));

            // ── Correctness Checker result text ────────────────────────────
            GameObject resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(canvasObj.transform, false);
            RectTransform resultRT = resultObj.AddComponent<RectTransform>();
            resultRT.anchorMin = new Vector2(0f, 0f);
            resultRT.anchorMax = new Vector2(1f, 0f);
            resultRT.pivot = new Vector2(0.5f, 0f);
            resultRT.anchoredPosition = new Vector2(0f, 5f);
            resultRT.sizeDelta = new Vector2(0f, 30f);
            TextMeshProUGUI resultText = resultObj.AddComponent<TextMeshProUGUI>();
            resultText.text = "";
            resultText.fontSize = 14;
            resultText.color = Color.yellow;
            resultText.alignment = TextAlignmentOptions.Center;

            // ── Wire StudyManager fields ───────────────────────────────────
            SerializedObject smSO = new SerializedObject(studyManager);
            smSO.FindProperty("statusText").objectReferenceValue = statusText;
            smSO.FindProperty("timerText").objectReferenceValue = timerText;
            smSO.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
            smSO.FindProperty("submitButton").objectReferenceValue = submitBtn.GetComponent<Button>();
            smSO.FindProperty("taskInstructionText").objectReferenceValue = instrText;
            smSO.ApplyModifiedProperties();

            // ── Wire CorrectnessChecker result text ────────────────────────
            SerializedObject ccSO = new SerializedObject(checker);
            ccSO.FindProperty("resultText").objectReferenceValue = resultText;
            ccSO.ApplyModifiedProperties();
        }

        private static GameObject CreateInstructionPanel(GameObject canvasParent, out TextMeshProUGUI instrText)
        {
            // Semi-transparent panel anchored to bottom of screen
            GameObject panel = new GameObject("TaskInstructionPanel");
            panel.transform.SetParent(canvasParent.transform, false);

            RectTransform panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0f, 0f);
            panelRT.anchorMax = new Vector2(1f, 0f);
            panelRT.pivot = new Vector2(0.5f, 0f);
            panelRT.anchoredPosition = new Vector2(0f, 40f);
            panelRT.sizeDelta = new Vector2(0f, 180f);

            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.15f, 0.88f);

            // Title row
            GameObject titleObj = new GameObject("TaskTitle");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.75f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.sizeDelta = Vector2.zero;
            titleRT.offsetMin = new Vector2(10f, 0f);
            titleRT.offsetMax = new Vector2(-10f, 0f);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Task Instructions";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.cyan;
            titleText.alignment = TextAlignmentOptions.TopLeft;

            // Instruction body
            GameObject bodyObj = new GameObject("InstructionText");
            bodyObj.transform.SetParent(panel.transform, false);
            RectTransform bodyRT = bodyObj.AddComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 0.73f);
            bodyRT.sizeDelta = Vector2.zero;
            bodyRT.offsetMin = new Vector2(10f, 5f);
            bodyRT.offsetMax = new Vector2(-10f, 0f);
            instrText = bodyObj.AddComponent<TextMeshProUGUI>();
            instrText.text = "Waiting for task to begin...";
            instrText.fontSize = 14;
            instrText.color = Color.white;
            instrText.alignment = TextAlignmentOptions.TopLeft;
            instrText.overflowMode = TextOverflowModes.Truncate;

            // TaskInstructionPanel component
            var panelComp = panel.AddComponent<TaskInstructionPanel>();
            SerializedObject panelSO = new SerializedObject(panelComp);
            panelSO.FindProperty("instructionText").objectReferenceValue = instrText;
            panelSO.FindProperty("taskTitleText").objectReferenceValue = titleText;
            panelSO.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateStudyButton(
            GameObject canvasParent, string name, string label,
            Vector2 anchoredPosition, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(canvasParent.transform, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(160f, 36f);

            Image img = btnObj.AddComponent<Image>();
            img.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnObj;
        }
    }
}
