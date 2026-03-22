// Builds 8 separate study task scenes (4 tasks × 2 conditions: Mercury, Events)
// Each scene is self-contained with camera, light, floor, and task-specific objects.
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
    public static class TaskSceneBuilder
    {
        private const string ScenesDir = "Assets/Research/UserStudy/Scenes";
        private const string MatDir = "Assets/Research/UserStudy/Materials";

        // Active study: T2, T3, T4 only (T1 archived — redundant with T2)
        [MenuItem("MercuryMessaging/User Study/Build All 12 Study Scenes")]
        public static void BuildAll12Scenes()
        {
            System.IO.Directory.CreateDirectory(ScenesDir);
            System.IO.Directory.CreateDirectory(MatDir);

            // 6 solution scenes
            BuildT2Mercury();
            BuildT2Events();
            BuildT3Mercury();
            BuildT3Events();
            BuildT4Mercury();
            BuildT4Events();

            // 6 starter scenes
            BuildT2MercuryStarter();
            BuildT2EventsStarter();
            BuildT3MercuryStarter();
            BuildT3EventsStarter();
            BuildT4MercuryStarter();
            BuildT4EventsStarter();

            Debug.Log("[TaskSceneBuilder] All 12 study scenes (T2/T3/T4 × solution+starter × Mercury+Events) built!");
        }

        [MenuItem("MercuryMessaging/User Study/Build All 6 Starter Scenes")]
        public static void BuildStarterScenes()
        {
            System.IO.Directory.CreateDirectory(ScenesDir);
            System.IO.Directory.CreateDirectory(MatDir);

            BuildT2MercuryStarter();
            BuildT2EventsStarter();
            BuildT3MercuryStarter();
            BuildT3EventsStarter();
            BuildT4MercuryStarter();
            BuildT4EventsStarter();
            Debug.Log("[TaskSceneBuilder] All 6 starter scenes built!");
        }

        // ================================================================
        // SHARED HELPERS
        // ================================================================

        private static UnityEngine.SceneManagement.Scene NewScene(string name)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            // Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 3f, -6f);
                cam.transform.LookAt(Vector3.zero);
            }
            // Floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            floor.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("FloorMat", new Color(0.78f, 0.78f, 0.78f));
            return scene;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string fileName)
        {
            // Route to organized subfolders based on filename pattern
            string subDir;
            if (fileName.Contains("Starter"))
            {
                string task = fileName.Contains("T1") ? "T1_SensorFanOut" :
                              fileName.Contains("T2") ? "T2_SafetyZone" :
                              fileName.Contains("T3") ? "T3_ModeSwitch" : "T4_AlertAggregation";
                subDir = $"Starters/{task}";
            }
            else
            {
                string task = fileName.Contains("T1") ? "T1_SensorFanOut" :
                              fileName.Contains("T2") ? "T2_SafetyZone" :
                              fileName.Contains("T3") ? "T3_ModeSwitch" : "T4_AlertAggregation";
                subDir = $"Solutions/{task}";
            }
            string dir = $"{ScenesDir}/{subDir}";
            System.IO.Directory.CreateDirectory(dir);
            string path = $"{dir}/{fileName}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[TaskSceneBuilder] Saved: {path}");
        }

        private static Material GetOrCreateMat(string name, Color color)
        {
            string path = $"{MatDir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material GetOrCreateTransparentMat(string name, Color color)
        {
            string path = $"{MatDir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = name };
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        /// <summary>
        /// Wire a persistent listener via SerializedObject (safe even when UnityEvent field is null).
        /// eventPropName is the serialized field name on sourceComponent.
        /// targetComponent is the MonoBehaviour with the target method.
        /// </summary>
        private static void WirePersistentListener(Component sourceComponent, string eventPropName,
            Component targetComponent, string methodName,
            UnityEngine.Events.PersistentListenerMode mode = UnityEngine.Events.PersistentListenerMode.EventDefined)
        {
            var so = new SerializedObject(sourceComponent);
            var callsProp = so.FindProperty($"{eventPropName}.m_PersistentCalls.m_Calls");
            callsProp.arraySize++;
            var call = callsProp.GetArrayElementAtIndex(callsProp.arraySize - 1);
            call.FindPropertyRelative("m_Target").objectReferenceValue = targetComponent;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue =
                targetComponent.GetType().AssemblyQualifiedName;
            call.FindPropertyRelative("m_MethodName").stringValue = methodName;
            call.FindPropertyRelative("m_Mode").intValue = (int)mode;
            call.FindPropertyRelative("m_CallState").intValue =
                (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
            so.ApplyModifiedProperties();
        }

        private static TextMeshProUGUI CreateWorldCanvas(GameObject parent, string text,
            Vector3 localPos, Vector2 size, int fontSize = 8, Color? textColor = null)
        {
            var canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(parent.transform);
            canvasObj.transform.localPosition = localPos;
            canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            canvasObj.GetComponent<RectTransform>().sizeDelta = size;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            var bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.05f, 0.05f);
            textRT.anchorMax = new Vector2(0.95f, 0.95f);
            textRT.sizeDelta = Vector2.zero;
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor ?? Color.white;

            return tmp;
        }

        private static void CreateRobotArmVisual(GameObject parent)
        {
            // Try UR5 DAE meshes first; fall back to primitives if not found
            const string meshDir = "Assets/Research/UserStudy/Models/UR5";
            string[] meshNames = { "base", "shoulder", "upperarm", "forearm", "wrist1", "wrist2", "wrist3" };
            // Approximate vertical positions for a neutral upright pose (UR5 URDF origins)
            Vector3[] positions = {
                new Vector3(0f, 0f, 0f),       // base
                new Vector3(0f, 0.089f, 0f),   // shoulder
                new Vector3(0f, 0.514f, 0f),   // upperarm (0.089 + 0.425)
                new Vector3(0f, 0.906f, 0f),   // forearm  (0.514 + 0.392)
                new Vector3(0f, 1.001f, 0f),   // wrist1   (0.906 + 0.095)
                new Vector3(0f, 1.084f, 0f),   // wrist2   (1.001 + 0.083)
                new Vector3(0f, 1.166f, 0f),   // wrist3   (1.084 + 0.082)
            };

            var firstMesh = AssetDatabase.LoadAssetAtPath<GameObject>($"{meshDir}/{meshNames[0]}.dae");
            if (firstMesh != null)
            {
                // Use UR5 meshes
                var armRoot = new GameObject("UR5_Arm");
                armRoot.transform.SetParent(parent.transform);
                armRoot.transform.localPosition = Vector3.zero;

                for (int i = 0; i < meshNames.Length; i++)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{meshDir}/{meshNames[i]}.dae");
                    if (prefab == null) continue;
                    var part = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    part.name = $"UR5_{meshNames[i]}";
                    part.transform.SetParent(armRoot.transform);
                    part.transform.localPosition = positions[i];
                    part.transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                // Fallback: primitive-based arm
                var mat = GetOrCreateMat("RobotArmMat", new Color(0.25f, 0.25f, 0.25f));
                string[] names = { "Base", "Shoulder", "Elbow", "Wrist", "Gripper" };
                PrimitiveType[] types = { PrimitiveType.Cylinder, PrimitiveType.Capsule, PrimitiveType.Capsule, PrimitiveType.Capsule, PrimitiveType.Cube };
                Vector3[] fallbackPos = { new Vector3(0,0,0), new Vector3(0,0.6f,0), new Vector3(0,1.2f,0), new Vector3(0,1.6f,0), new Vector3(0,1.9f,0) };
                Vector3[] scales = { new Vector3(0.5f,0.1f,0.5f), new Vector3(0.15f,0.4f,0.15f), new Vector3(0.12f,0.3f,0.12f), new Vector3(0.1f,0.2f,0.1f), new Vector3(0.2f,0.05f,0.15f) };
                for (int i = 0; i < names.Length; i++)
                {
                    var part = GameObject.CreatePrimitive(types[i]);
                    part.name = $"Arm{names[i]}";
                    part.transform.SetParent(parent.transform);
                    part.transform.localPosition = fallbackPos[i];
                    part.transform.localScale = scales[i];
                    part.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
        }

        // ================================================================
        // T2: SAFETY ZONE ALERTS
        // ================================================================

        private static void BuildT2Mercury()
        {
            var scene = NewScene("T2_Mercury");
            var indicatorMat = GetOrCreateMat("IndicatorDefaultMat", Color.green);

            // Workspace — root relay for the whole area
            var workspace = new GameObject("RobotWorkspace");
            workspace.AddComponent<MmRelayNode>();

            CreateRobotArmVisual(workspace); // visual only, no relay

            // Safety zone spheres (visual only)
            var warnZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warnZone.name = "SafetyZoneWarning";
            warnZone.transform.SetParent(workspace.transform);
            warnZone.transform.localScale = new Vector3(4f, 4f, 4f);
            Object.DestroyImmediate(warnZone.GetComponent<SphereCollider>());
            warnZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyWarningMat", new Color(1f, 0.92f, 0f, 0.15f));

            var emergZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emergZone.name = "SafetyZoneEmergency";
            emergZone.transform.SetParent(workspace.transform);
            emergZone.transform.localScale = new Vector3(2f, 2f, 2f);
            Object.DestroyImmediate(emergZone.GetComponent<SphereCollider>());
            emergZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyEmergencyMat", new Color(1f, 0.1f, 0.1f, 0.15f));

            // Worker — sender with MmRelayNode + ZoneAlertManager
            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "Worker";
            worker.transform.SetParent(workspace.transform);
            worker.transform.position = new Vector3(0f, 0f, 3f);
            worker.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            worker.AddComponent<WorkerMovement>();
            worker.AddComponent<MmRelayNode>();
            var zam = worker.AddComponent<ZoneAlertManager_Solution>();
            var zamSO = new SerializedObject(zam);
            zamSO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            zamSO.ApplyModifiedProperties();

            // 4 Safety indicators — siblings of Worker under Workspace
            Vector3[] indPos = {
                new Vector3(-2f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 2f),
                new Vector3(1f, 0.5f, -1f),
                new Vector3(2f, 0.5f, 1f)
            };
            for (int i = 0; i < 4; i++)
            {
                var ind = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ind.name = $"SafetyIndicator{i+1}";
                ind.transform.SetParent(workspace.transform);
                ind.transform.position = indPos[i];
                ind.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                ind.GetComponent<Renderer>().sharedMaterial = indicatorMat;
                ind.AddComponent<MmRelayNode>();
                var si = ind.AddComponent<SafetyZoneIndicator>();
                var siSO = new SerializedObject(si);
                siSO.FindProperty("indicatorRenderer").objectReferenceValue = ind.GetComponent<Renderer>();
                var alertTmp = CreateWorldCanvas(ind, "CLEAR", new Vector3(0, 0.5f, 0), new Vector2(30, 10), 5);
                siSO.FindProperty("alertText").objectReferenceValue = alertTmp;
                siSO.ApplyModifiedProperties();
            }

            SaveScene(scene, "T2_SafetyZone_Mercury");
        }

        private static void BuildT2Events()
        {
            var scene = NewScene("T2_Events");
            var indicatorMat = GetOrCreateMat("IndicatorDefaultMat", Color.green);

            var workspace = new GameObject("RobotWorkspace");
            CreateRobotArmVisual(workspace);

            // Safety zone spheres
            var warnZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warnZone.name = "SafetyZoneWarning";
            warnZone.transform.SetParent(workspace.transform);
            warnZone.transform.localScale = new Vector3(4f, 4f, 4f);
            Object.DestroyImmediate(warnZone.GetComponent<SphereCollider>());
            warnZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyWarningMat", new Color(1f, 0.92f, 0f, 0.15f));

            var emergZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emergZone.name = "SafetyZoneEmergency";
            emergZone.transform.SetParent(workspace.transform);
            emergZone.transform.localScale = new Vector3(2f, 2f, 2f);
            Object.DestroyImmediate(emergZone.GetComponent<SphereCollider>());
            emergZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyEmergencyMat", new Color(1f, 0.1f, 0.1f, 0.15f));

            // Worker
            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "Worker";
            worker.transform.SetParent(workspace.transform);
            worker.transform.position = new Vector3(0f, 0f, 3f);
            worker.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            worker.AddComponent<WorkerMovement>();
            var zam = worker.AddComponent<ZoneAlertManager_Events_Solution>();

            // 4 indicators
            Vector3[] indPos = {
                new Vector3(-2f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 2f),
                new Vector3(1f, 0.5f, -1f),
                new Vector3(2f, 0.5f, 1f)
            };
            var indicators = new SafetyZoneIndicator_Events[4];
            for (int i = 0; i < 4; i++)
            {
                var ind = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ind.name = $"SafetyIndicator{i+1}";
                ind.transform.SetParent(workspace.transform);
                ind.transform.position = indPos[i];
                ind.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                ind.GetComponent<Renderer>().sharedMaterial = indicatorMat;
                indicators[i] = ind.AddComponent<SafetyZoneIndicator_Events>();
                var siSO = new SerializedObject(indicators[i]);
                siSO.FindProperty("indicatorRenderer").objectReferenceValue = ind.GetComponent<Renderer>();
                var alertTmp = CreateWorldCanvas(ind, "CLEAR", new Vector3(0, 0.5f, 0), new Vector2(30, 10), 5);
                siSO.FindProperty("alertText").objectReferenceValue = alertTmp;
                siSO.ApplyModifiedProperties();
            }

            // Wire references
            var zamSO = new SerializedObject(zam);
            zamSO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            zamSO.FindProperty("indicator1").objectReferenceValue = indicators[0];
            zamSO.FindProperty("indicator2").objectReferenceValue = indicators[1];
            zamSO.FindProperty("indicator3").objectReferenceValue = indicators[2];
            zamSO.FindProperty("indicator4").objectReferenceValue = indicators[3];
            zamSO.ApplyModifiedProperties();

            SaveScene(scene, "T2_SafetyZone_Events");
        }

        // ================================================================
        // T3: MODE-SWITCH DEBUGGING
        // ================================================================

        private static void BuildT3Mercury()
        {
            var scene = NewScene("T3_Mercury");

            // FacilityHub — root relay
            var hub = new GameObject("FacilityHub");
            hub.AddComponent<MmRelayNode>();
            var modeCtrl = hub.AddComponent<FacilityModeController>();
            var modeCtrlSO = new SerializedObject(modeCtrl);
            modeCtrlSO.FindProperty("facilityRelay").objectReferenceValue = hub.GetComponent<MmRelayNode>();
            modeCtrlSO.ApplyModifiedProperties();

            // HVAC System — child of hub
            var hvac = new GameObject("HvacSystem");
            hvac.transform.SetParent(hub.transform);
            hvac.transform.position = new Vector3(0f, 1f, 0f);
            hvac.AddComponent<MmRelayNode>();
            hvac.AddComponent<HvacController_Buggy>(); // The buggy version for participants to fix

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HvacVisual";
            visual.transform.SetParent(hvac.transform);
            visual.transform.localScale = new Vector3(0.8f, 1.2f, 0.3f);
            visual.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("HvacMat", new Color(0.5f, 0.5f, 0.5f));

            var statusTmp = CreateWorldCanvas(hvac, "HVAC: 22.0°C (Day)", new Vector3(0, 1.5f, 0), new Vector2(60, 20), 6);
            var hvacSO = new SerializedObject(hvac.GetComponent<HvacController_Buggy>());
            hvacSO.FindProperty("statusText").objectReferenceValue = statusTmp;
            hvacSO.ApplyModifiedProperties();

            // Temperature simulator — sends periodic float values to HVAC
            var tempSimObj = new GameObject("TemperatureSimulator");
            tempSimObj.transform.SetParent(hub.transform);
            tempSimObj.transform.position = new Vector3(2f, 0f, 0f);
            var tempSim = tempSimObj.AddComponent<TemperatureSimulator>();
            var tempSimSO = new SerializedObject(tempSim);
            tempSimSO.FindProperty("hvacRelay").objectReferenceValue = hvac.GetComponent<MmRelayNode>();
            tempSimSO.ApplyModifiedProperties();

            // Mode toggle — screen-space button for easy clicking
            var btnCanvas = new GameObject("ModeToggleCanvas");
            var btnCanvasComp = btnCanvas.AddComponent<Canvas>();
            btnCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            btnCanvasComp.sortingOrder = 100;
            btnCanvas.AddComponent<CanvasScaler>();
            btnCanvas.AddComponent<GraphicRaycaster>();

            var btnObj = new GameObject("ToggleButton");
            btnObj.transform.SetParent(btnCanvas.transform, false);
            var btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 1f);
            btnRT.anchorMax = new Vector2(0f, 1f);
            btnRT.pivot = new Vector2(0f, 1f);
            btnRT.anchoredPosition = new Vector2(10f, -10f);
            btnRT.sizeDelta = new Vector2(180f, 50f);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var btn = btnObj.AddComponent<Button>();

            var btnText = new GameObject("Text");
            btnText.transform.SetParent(btnObj.transform, false);
            var btnTextRT = btnText.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;
            var btnTmp = btnText.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "Toggle Day/Night";
            btnTmp.fontSize = 18;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;

            // Wire button → FacilityModeController.ToggleMode
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, modeCtrl.ToggleMode);

            SaveScene(scene, "T3_ModeSwitch_Mercury");
        }

        private static void BuildT3Events()
        {
            var scene = NewScene("T3_Events");

            var hub = new GameObject("FacilityHub");
            var modeCtrl = hub.AddComponent<FacilityModeController>();

            // HVAC — Events version
            var hvac = new GameObject("HvacSystem");
            hvac.transform.SetParent(hub.transform);
            hvac.transform.position = new Vector3(0f, 1f, 0f);
            var hvacCtrl = hvac.AddComponent<HvacController_Events_Buggy>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HvacVisual";
            visual.transform.SetParent(hvac.transform);
            visual.transform.localScale = new Vector3(0.8f, 1.2f, 0.3f);
            visual.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("HvacMat", new Color(0.5f, 0.5f, 0.5f));

            var statusTmp = CreateWorldCanvas(hvac, "HVAC: 22.0°C (Day)", new Vector3(0, 1.5f, 0), new Vector2(60, 20), 6);

            // Wire status text
            var hvacSO = new SerializedObject(hvacCtrl);
            hvacSO.FindProperty("statusText").objectReferenceValue = statusTmp;
            hvacSO.ApplyModifiedProperties();

            // Wire mode controller → HVAC
            WirePersistentListener(modeCtrl, "OnModeChanged", hvacCtrl, "OnModeChanged");

            // Temperature simulator — Events version
            var tempSimObj = new GameObject("TemperatureSimulator");
            tempSimObj.transform.SetParent(hub.transform);
            tempSimObj.transform.position = new Vector3(2f, 0f, 0f);
            var tempSim = tempSimObj.AddComponent<TemperatureSimulator>();
            WirePersistentListener(tempSim, "OnTemperatureRequest", hvacCtrl, "OnTemperatureRequested");

            // Mode toggle — screen-space button for easy clicking
            var btnCanvasE = new GameObject("ModeToggleCanvas");
            var btnCanvasCompE = btnCanvasE.AddComponent<Canvas>();
            btnCanvasCompE.renderMode = RenderMode.ScreenSpaceOverlay;
            btnCanvasCompE.sortingOrder = 100;
            btnCanvasE.AddComponent<CanvasScaler>();
            btnCanvasE.AddComponent<GraphicRaycaster>();

            var btnObjE = new GameObject("ToggleButton");
            btnObjE.transform.SetParent(btnCanvasE.transform, false);
            var btnRTE = btnObjE.AddComponent<RectTransform>();
            btnRTE.anchorMin = new Vector2(0f, 1f);
            btnRTE.anchorMax = new Vector2(0f, 1f);
            btnRTE.pivot = new Vector2(0f, 1f);
            btnRTE.anchoredPosition = new Vector2(10f, -10f);
            btnRTE.sizeDelta = new Vector2(180f, 50f);
            var btnImgE = btnObjE.AddComponent<Image>();
            btnImgE.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var btnE = btnObjE.AddComponent<Button>();

            var btnTextE = new GameObject("Text");
            btnTextE.transform.SetParent(btnObjE.transform, false);
            var btnTextRTE = btnTextE.AddComponent<RectTransform>();
            btnTextRTE.anchorMin = Vector2.zero;
            btnTextRTE.anchorMax = Vector2.one;
            btnTextRTE.sizeDelta = Vector2.zero;
            var btnTmpE = btnTextE.AddComponent<TextMeshProUGUI>();
            btnTmpE.text = "Toggle Day/Night";
            btnTmpE.fontSize = 18;
            btnTmpE.alignment = TextAlignmentOptions.Center;
            btnTmpE.color = Color.white;

            // Wire button → FacilityModeController.ToggleMode
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btnE.onClick, modeCtrl.ToggleMode);

            SaveScene(scene, "T3_ModeSwitch_Events");
        }

        // ================================================================
        // T4: ALERT AGGREGATION
        // ================================================================

        private static void BuildT4Mercury()
        {
            var scene = NewScene("T4_Mercury");

            // CentralDashboard — parent relay that receives alerts from children
            var dashboard = new GameObject("CentralDashboard");
            dashboard.AddComponent<MmRelayNode>();
            var dashComp = dashboard.AddComponent<CentralDashboard>();

            // Dashboard UI
            var logTmp = CreateWorldCanvas(dashboard, "(no alerts yet)",
                new Vector3(0, 2.5f, -2f), new Vector2(150, 100), 5);
            var dashSO = new SerializedObject(dashComp);
            dashSO.FindProperty("alertLogText").objectReferenceValue = logTmp;
            dashSO.ApplyModifiedProperties();

            // 4 subsystem children
            string[] names = { "HvacSubsystem", "OccupancySensor", "AirQualitySensor", "EnergyMonitor" };
            string[] subNames = { "HVAC", "Occupancy", "AirQuality", "Energy" };
            string[][] alerts = {
                new[] { "Temperature above threshold", "Compressor fault", "Filter replacement needed", "Refrigerant low" },
                new[] { "Zone occupied", "Movement detected", "Occupancy threshold exceeded", "Unexpected presence" },
                new[] { "CO2 above limit", "Particulate spike", "Ventilation fault", "Humidity warning" },
                new[] { "Peak usage warning", "Circuit overload", "Battery low", "Consumption anomaly" }
            };
            Vector3[] positions = {
                new Vector3(-3f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 0f),
                new Vector3(1f, 0.5f, 0f),
                new Vector3(3f, 0.5f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                var sub = new GameObject(names[i]);
                sub.transform.SetParent(dashboard.transform);
                sub.transform.position = positions[i];
                sub.AddComponent<MmRelayNode>();
                var alerter = sub.AddComponent<SubsystemAlerter_Solution>();
                var alerterSO = new SerializedObject(alerter);
                alerterSO.FindProperty("subsystemName").stringValue = subNames[i];
                alerterSO.ApplyModifiedProperties();

                var alertSim = sub.AddComponent<AlertSimulator>();
                alertSim.subsystemName = subNames[i];
                alertSim.possibleAlerts = alerts[i];

                // Wire AlertSimulator → SubsystemAlerter
                WirePersistentListener(alertSim, "OnAlertGenerated", alerter, "RaiseAlert");

                // Visual
                var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vis.name = "Visual";
                vis.transform.SetParent(sub.transform);
                vis.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                vis.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("SensorMat", new Color(0.1f, 0.15f, 0.5f));

                CreateWorldCanvas(sub, subNames[i], new Vector3(0, 0.8f, 0), new Vector2(60, 15), 6);
            }

            SaveScene(scene, "T4_AlertAggregation_Mercury");
        }

        private static void BuildT4Events()
        {
            var scene = NewScene("T4_Events");

            // Dashboard
            var dashboard = new GameObject("CentralDashboard");
            var dashComp = dashboard.AddComponent<CentralDashboard_Events>();
            var logTmp = CreateWorldCanvas(dashboard, "(no alerts yet)",
                new Vector3(0, 2.5f, -2f), new Vector2(150, 100), 5);
            var dashSO = new SerializedObject(dashComp);
            dashSO.FindProperty("alertLogText").objectReferenceValue = logTmp;
            dashSO.ApplyModifiedProperties();

            string[] names = { "HvacSubsystem", "OccupancySensor", "AirQualitySensor", "EnergyMonitor" };
            string[] subNames = { "HVAC", "Occupancy", "AirQuality", "Energy" };
            string[][] alerts = {
                new[] { "Temperature above threshold", "Compressor fault", "Filter replacement needed", "Refrigerant low" },
                new[] { "Zone occupied", "Movement detected", "Occupancy threshold exceeded", "Unexpected presence" },
                new[] { "CO2 above limit", "Particulate spike", "Ventilation fault", "Humidity warning" },
                new[] { "Peak usage warning", "Circuit overload", "Battery low", "Consumption anomaly" }
            };
            Vector3[] positions = {
                new Vector3(-3f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 0f),
                new Vector3(1f, 0.5f, 0f),
                new Vector3(3f, 0.5f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                var sub = new GameObject(names[i]);
                sub.transform.SetParent(dashboard.transform);
                sub.transform.position = positions[i];
                var alerter = sub.AddComponent<SubsystemAlerter_Events_Solution>();
                var alerterSO = new SerializedObject(alerter);
                alerterSO.FindProperty("subsystemName").stringValue = subNames[i];
                alerterSO.FindProperty("dashboard").objectReferenceValue = dashComp;
                alerterSO.ApplyModifiedProperties();

                var alertSim = sub.AddComponent<AlertSimulator>();
                alertSim.subsystemName = subNames[i];
                alertSim.possibleAlerts = alerts[i];

                // Wire AlertSimulator → SubsystemAlerter
                WirePersistentListener(alertSim, "OnAlertGenerated", alerter, "RaiseAlert");

                var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vis.name = "Visual";
                vis.transform.SetParent(sub.transform);
                vis.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                vis.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("SensorMat", new Color(0.1f, 0.15f, 0.5f));

                CreateWorldCanvas(sub, subNames[i], new Vector3(0, 0.8f, 0), new Vector2(60, 15), 6);
            }

            SaveScene(scene, "T4_AlertAggregation_Events");
        }

        // ================================================================
        // T2 STARTER: SAFETY ZONE ALERTS
        // Bare starter scene — NO Mercury or Events components on indicators.
        // Both conditions share the same visual layout; only the starter script
        // on Worker differs (Mercury vs Events).
        //
        // What participants must add:
        //   Mercury: MmRelayNode to RobotWorkspace + Worker + each Indicator,
        //            SafetyZoneIndicator to each Indicator, then write
        //            relay.Send().ToAll().Within() in ZoneAlertManager_Starter.
        //   Events:  SafetyZoneIndicator_Events to each Indicator (with Renderer
        //            reference), declare [SerializeField] refs in
        //            ZoneAlertManager_Events_Starter, write Vector3.Distance code.
        // ================================================================

        private static void BuildT2MercuryStarter()
        {
            var scene = NewScene("T2_Mercury_Starter");
            var indicatorMat = GetOrCreateMat("IndicatorDefaultMat", Color.green);

            // RobotWorkspace — bare container, NO MmRelayNode (participant adds it)
            var workspace = new GameObject("RobotWorkspace");
            CreateRobotArmVisual(workspace);

            // Safety zone spheres — visual only, no collider, no components
            var warnZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warnZone.name = "SafetyZoneWarning";
            warnZone.transform.SetParent(workspace.transform);
            warnZone.transform.localScale = new Vector3(4f, 4f, 4f);
            Object.DestroyImmediate(warnZone.GetComponent<SphereCollider>());
            warnZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyWarningMat", new Color(1f, 0.92f, 0f, 0.15f));

            var emergZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emergZone.name = "SafetyZoneEmergency";
            emergZone.transform.SetParent(workspace.transform);
            emergZone.transform.localScale = new Vector3(2f, 2f, 2f);
            Object.DestroyImmediate(emergZone.GetComponent<SphereCollider>());
            emergZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyEmergencyMat", new Color(1f, 0.1f, 0.1f, 0.15f));

            // Worker — WorkerMovement (shared) + ZoneAlertManager_Starter (Mercury)
            // NO MmRelayNode — participant adds it
            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "Worker";
            worker.transform.SetParent(workspace.transform);
            worker.transform.position = new Vector3(0f, 0f, 3f);
            worker.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            worker.AddComponent<WorkerMovement>();
            var zam = worker.AddComponent<ZoneAlertManager_Starter>();
            var zamSO = new SerializedObject(zam);
            zamSO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            zamSO.ApplyModifiedProperties();

            // Safety indicators — bare Cubes with green material, Renderer only
            // NO MmRelayNode, NO SafetyZoneIndicator — participant adds these
            Vector3[] indPos = {
                new Vector3(-2f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 2f),
                new Vector3(1f, 0.5f, -1f),
                new Vector3(2f, 0.5f, 1f)
            };
            for (int i = 0; i < 4; i++)
            {
                var ind = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ind.name = $"SafetyIndicator{i+1}";
                ind.transform.SetParent(workspace.transform);
                ind.transform.position = indPos[i];
                ind.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                ind.GetComponent<Renderer>().sharedMaterial = indicatorMat;
                // Renderer is present; participant adds MmRelayNode + SafetyZoneIndicator
                // Canvas label is visual placeholder only — no component to wire alertText to
                CreateWorldCanvas(ind, "---", new Vector3(0, 0.5f, 0), new Vector2(30, 10), 5);
            }

            AddGoalCanvas("GOAL: Indicators should turn yellow within 2m and red within 1m of the worker.\nMove with WASD / arrow keys.");

            SaveScene(scene, "T2_SafetyZone_Mercury_Starter");
        }

        private static void BuildT2EventsStarter()
        {
            var scene = NewScene("T2_Events_Starter");
            var indicatorMat = GetOrCreateMat("IndicatorDefaultMat", Color.green);

            // RobotWorkspace — bare container, NO Mercury components
            var workspace = new GameObject("RobotWorkspace");
            CreateRobotArmVisual(workspace);

            // Safety zone spheres — visual only
            var warnZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            warnZone.name = "SafetyZoneWarning";
            warnZone.transform.SetParent(workspace.transform);
            warnZone.transform.localScale = new Vector3(4f, 4f, 4f);
            Object.DestroyImmediate(warnZone.GetComponent<SphereCollider>());
            warnZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyWarningMat", new Color(1f, 0.92f, 0f, 0.15f));

            var emergZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emergZone.name = "SafetyZoneEmergency";
            emergZone.transform.SetParent(workspace.transform);
            emergZone.transform.localScale = new Vector3(2f, 2f, 2f);
            Object.DestroyImmediate(emergZone.GetComponent<SphereCollider>());
            emergZone.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateTransparentMat("SafetyEmergencyMat", new Color(1f, 0.1f, 0.1f, 0.15f));

            // Worker — WorkerMovement (shared) + ZoneAlertManager_Events_Starter
            // NO MmRelayNode (not needed for Events condition)
            var worker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            worker.name = "Worker";
            worker.transform.SetParent(workspace.transform);
            worker.transform.position = new Vector3(0f, 0f, 3f);
            worker.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            worker.AddComponent<WorkerMovement>();
            var zam = worker.AddComponent<ZoneAlertManager_Events_Starter>();
            var zamSO = new SerializedObject(zam);
            zamSO.FindProperty("workerTransform").objectReferenceValue = worker.transform;
            zamSO.ApplyModifiedProperties();

            // Safety indicators — bare Cubes with green material, Renderer only
            // NO SafetyZoneIndicator_Events — participant adds those and wires indicatorRenderer
            Vector3[] indPos = {
                new Vector3(-2f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 2f),
                new Vector3(1f, 0.5f, -1f),
                new Vector3(2f, 0.5f, 1f)
            };
            for (int i = 0; i < 4; i++)
            {
                var ind = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ind.name = $"SafetyIndicator{i+1}";
                ind.transform.SetParent(workspace.transform);
                ind.transform.position = indPos[i];
                ind.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                ind.GetComponent<Renderer>().sharedMaterial = indicatorMat;
                // Renderer is present; participant adds SafetyZoneIndicator_Events + wires it
                // Canvas label is visual placeholder only — no component to wire alertText to
                CreateWorldCanvas(ind, "---", new Vector3(0, 0.5f, 0), new Vector2(30, 10), 5);
            }

            AddGoalCanvas("GOAL: Indicators should turn yellow within 2m and red within 1m of the worker.\nMove with WASD / arrow keys.");

            SaveScene(scene, "T2_SafetyZone_Events_Starter");
        }

        // ================================================================
        // T3 STARTER: MODE-SWITCH DEBUGGING
        // T3 starters are identical to T3 solution scenes — both use _Buggy
        // scripts because finding and fixing the bug IS the task.
        // ================================================================

        private static void BuildT3MercuryStarter()
        {
            var scene = NewScene("T3_Mercury_Starter");

            var hub = new GameObject("FacilityHub");
            hub.AddComponent<MmRelayNode>();
            var modeCtrl = hub.AddComponent<FacilityModeController>();
            var modeCtrlSO = new SerializedObject(modeCtrl);
            modeCtrlSO.FindProperty("facilityRelay").objectReferenceValue = hub.GetComponent<MmRelayNode>();
            modeCtrlSO.ApplyModifiedProperties();

            var hvac = new GameObject("HvacSystem");
            hvac.transform.SetParent(hub.transform);
            hvac.transform.position = new Vector3(0f, 1f, 0f);
            hvac.AddComponent<MmRelayNode>();
            hvac.AddComponent<HvacController_Buggy>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HvacVisual";
            visual.transform.SetParent(hvac.transform);
            visual.transform.localScale = new Vector3(0.8f, 1.2f, 0.3f);
            visual.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("HvacMat", new Color(0.5f, 0.5f, 0.5f));

            var statusTmpStarter = CreateWorldCanvas(hvac, "HVAC: 22.0°C (Day)", new Vector3(0, 1.5f, 0), new Vector2(60, 20), 6);
            var hvacSOStarter = new SerializedObject(hvac.GetComponent<HvacController_Buggy>());
            hvacSOStarter.FindProperty("statusText").objectReferenceValue = statusTmpStarter;
            hvacSOStarter.ApplyModifiedProperties();

            var tempSimObj = new GameObject("TemperatureSimulator");
            tempSimObj.transform.SetParent(hub.transform);
            tempSimObj.transform.position = new Vector3(2f, 0f, 0f);
            var tempSim = tempSimObj.AddComponent<TemperatureSimulator>();
            var tempSimSO = new SerializedObject(tempSim);
            tempSimSO.FindProperty("hvacRelay").objectReferenceValue = hvac.GetComponent<MmRelayNode>();
            tempSimSO.ApplyModifiedProperties();

            // Mode toggle — screen-space button for easy clicking
            var btnCanvas = new GameObject("ModeToggleCanvas");
            var btnCanvasComp = btnCanvas.AddComponent<Canvas>();
            btnCanvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            btnCanvasComp.sortingOrder = 100;
            btnCanvas.AddComponent<CanvasScaler>();
            btnCanvas.AddComponent<GraphicRaycaster>();

            var btnObj = new GameObject("ToggleButton");
            btnObj.transform.SetParent(btnCanvas.transform, false);
            var btnRT = btnObj.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0f, 1f);
            btnRT.anchorMax = new Vector2(0f, 1f);
            btnRT.pivot = new Vector2(0f, 1f);
            btnRT.anchoredPosition = new Vector2(10f, -10f);
            btnRT.sizeDelta = new Vector2(180f, 50f);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var btn = btnObj.AddComponent<Button>();

            var btnText = new GameObject("Text");
            btnText.transform.SetParent(btnObj.transform, false);
            var btnTextRT = btnText.AddComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;
            var btnTmp = btnText.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "Toggle Day/Night";
            btnTmp.fontSize = 18;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;

            // Wire button → FacilityModeController.ToggleMode
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, modeCtrl.ToggleMode);

            AddGoalCanvas("GOAL: Find and fix the bug — HVAC temperature should NOT change during Night mode.\nUse the toggle button to switch modes.");

            SaveScene(scene, "T3_ModeSwitch_Mercury_Starter");
        }

        private static void BuildT3EventsStarter()
        {
            var scene = NewScene("T3_Events_Starter");

            var hub = new GameObject("FacilityHub");
            var modeCtrl = hub.AddComponent<FacilityModeController>();

            var hvac = new GameObject("HvacSystem");
            hvac.transform.SetParent(hub.transform);
            hvac.transform.position = new Vector3(0f, 1f, 0f);
            var hvacCtrl = hvac.AddComponent<HvacController_Events_Buggy>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "HvacVisual";
            visual.transform.SetParent(hvac.transform);
            visual.transform.localScale = new Vector3(0.8f, 1.2f, 0.3f);
            visual.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("HvacMat", new Color(0.5f, 0.5f, 0.5f));

            var statusTmp = CreateWorldCanvas(hvac, "HVAC: 22.0°C (Day)", new Vector3(0, 1.5f, 0), new Vector2(60, 20), 6);

            var hvacSO = new SerializedObject(hvacCtrl);
            hvacSO.FindProperty("statusText").objectReferenceValue = statusTmp;
            hvacSO.ApplyModifiedProperties();

            WirePersistentListener(modeCtrl, "OnModeChanged", hvacCtrl, "OnModeChanged");

            var tempSimObj = new GameObject("TemperatureSimulator");
            tempSimObj.transform.SetParent(hub.transform);
            tempSimObj.transform.position = new Vector3(2f, 0f, 0f);
            var tempSim = tempSimObj.AddComponent<TemperatureSimulator>();
            WirePersistentListener(tempSim, "OnTemperatureRequest", hvacCtrl, "OnTemperatureRequested");

            // Mode toggle — screen-space button for easy clicking
            var btnCanvasES = new GameObject("ModeToggleCanvas");
            var btnCanvasCompES = btnCanvasES.AddComponent<Canvas>();
            btnCanvasCompES.renderMode = RenderMode.ScreenSpaceOverlay;
            btnCanvasCompES.sortingOrder = 100;
            btnCanvasES.AddComponent<CanvasScaler>();
            btnCanvasES.AddComponent<GraphicRaycaster>();

            var btnObjES = new GameObject("ToggleButton");
            btnObjES.transform.SetParent(btnCanvasES.transform, false);
            var btnRTES = btnObjES.AddComponent<RectTransform>();
            btnRTES.anchorMin = new Vector2(0f, 1f);
            btnRTES.anchorMax = new Vector2(0f, 1f);
            btnRTES.pivot = new Vector2(0f, 1f);
            btnRTES.anchoredPosition = new Vector2(10f, -10f);
            btnRTES.sizeDelta = new Vector2(180f, 50f);
            var btnImgES = btnObjES.AddComponent<Image>();
            btnImgES.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var btnES = btnObjES.AddComponent<Button>();

            var btnTextES = new GameObject("Text");
            btnTextES.transform.SetParent(btnObjES.transform, false);
            var btnTextRTES = btnTextES.AddComponent<RectTransform>();
            btnTextRTES.anchorMin = Vector2.zero;
            btnTextRTES.anchorMax = Vector2.one;
            btnTextRTES.sizeDelta = Vector2.zero;
            var btnTmpES = btnTextES.AddComponent<TextMeshProUGUI>();
            btnTmpES.text = "Toggle Day/Night";
            btnTmpES.fontSize = 18;
            btnTmpES.alignment = TextAlignmentOptions.Center;
            btnTmpES.color = Color.white;

            // Wire button → FacilityModeController.ToggleMode
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btnES.onClick, modeCtrl.ToggleMode);

            AddGoalCanvas("GOAL: Find and fix the bug — HVAC temperature should NOT change during Night mode.\nUse the toggle button to switch modes.");

            SaveScene(scene, "T3_ModeSwitch_Events_Starter");
        }

        // ================================================================
        // T4 STARTER: ALERT AGGREGATION
        // Bare starter scene — NO Mercury or Events components on dashboard or subsystems.
        // Both conditions share the same visual layout; only the starter script
        // on each subsystem differs (Mercury vs Events).
        //
        // What participants must add:
        //   Mercury: MmRelayNode to CentralDashboard + each subsystem,
        //            CentralDashboard to CentralDashboard (wire alertLogText),
        //            then write relay.NotifyValue() in SubsystemAlerter_Starter.
        //   Events:  CentralDashboard_Events to CentralDashboard (wire alertLogText),
        //            declare [SerializeField] CentralDashboard_Events dashboard ref in
        //            SubsystemAlerter_Events_Starter, wire it, then call
        //            dashboard.ReceiveAlert() in RaiseAlert().
        // ================================================================

        private static void BuildT4MercuryStarter()
        {
            var scene = NewScene("T4_Mercury_Starter");

            // CentralDashboard — canvas only, NO MmRelayNode, NO CentralDashboard component
            // Participant adds those
            var dashboard = new GameObject("CentralDashboard");
            CreateWorldCanvas(dashboard, "(no alerts yet)",
                new Vector3(0, 2.5f, -2f), new Vector2(150, 100), 5);

            string[] names = { "HvacSubsystem", "OccupancySensor", "AirQualitySensor", "EnergyMonitor" };
            string[] subNames = { "HVAC", "Occupancy", "AirQuality", "Energy" };
            string[][] alerts = {
                new[] { "Temperature above threshold", "Compressor fault", "Filter replacement needed", "Refrigerant low" },
                new[] { "Zone occupied", "Movement detected", "Occupancy threshold exceeded", "Unexpected presence" },
                new[] { "CO2 above limit", "Particulate spike", "Ventilation fault", "Humidity warning" },
                new[] { "Peak usage warning", "Circuit overload", "Battery low", "Consumption anomaly" }
            };
            Vector3[] positions = {
                new Vector3(-3f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 0f),
                new Vector3(1f, 0.5f, 0f),
                new Vector3(3f, 0.5f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                // Subsystem — child of CentralDashboard (hierarchy matters for Mercury NotifyValue)
                // NO MmRelayNode — participant adds it
                var sub = new GameObject(names[i]);
                sub.transform.SetParent(dashboard.transform);
                sub.transform.position = positions[i];

                // SubsystemAlerter_Starter has empty RaiseAlert body — participant implements
                var alerter = sub.AddComponent<SubsystemAlerter_Starter>();
                var alerterSO = new SerializedObject(alerter);
                alerterSO.FindProperty("subsystemName").stringValue = subNames[i];
                alerterSO.ApplyModifiedProperties();

                // AlertSimulator is always present — it's a shared data source
                var alertSim = sub.AddComponent<AlertSimulator>();
                alertSim.subsystemName = subNames[i];
                alertSim.possibleAlerts = alerts[i];

                // Wire AlertSimulator → SubsystemAlerter_Starter.RaiseAlert
                WirePersistentListener(alertSim, "OnAlertGenerated", alerter, "RaiseAlert");

                var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vis.name = "Visual";
                vis.transform.SetParent(sub.transform);
                vis.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                vis.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("SensorMat", new Color(0.1f, 0.15f, 0.5f));

                CreateWorldCanvas(sub, subNames[i], new Vector3(0, 0.8f, 0), new Vector2(60, 15), 6);
            }

            AddGoalCanvas("GOAL: Wire 4 subsystem alerts to the central dashboard.\nAlerts should appear in the dashboard log.");

            SaveScene(scene, "T4_AlertAggregation_Mercury_Starter");
        }

        private static void BuildT4EventsStarter()
        {
            var scene = NewScene("T4_Events_Starter");

            // CentralDashboard — canvas only, NO CentralDashboard_Events component
            // Participant adds it and wires alertLogText
            var dashboard = new GameObject("CentralDashboard");
            CreateWorldCanvas(dashboard, "(no alerts yet)",
                new Vector3(0, 2.5f, -2f), new Vector2(150, 100), 5);

            string[] names = { "HvacSubsystem", "OccupancySensor", "AirQualitySensor", "EnergyMonitor" };
            string[] subNames = { "HVAC", "Occupancy", "AirQuality", "Energy" };
            string[][] alerts = {
                new[] { "Temperature above threshold", "Compressor fault", "Filter replacement needed", "Refrigerant low" },
                new[] { "Zone occupied", "Movement detected", "Occupancy threshold exceeded", "Unexpected presence" },
                new[] { "CO2 above limit", "Particulate spike", "Ventilation fault", "Humidity warning" },
                new[] { "Peak usage warning", "Circuit overload", "Battery low", "Consumption anomaly" }
            };
            Vector3[] positions = {
                new Vector3(-3f, 0.5f, 0f),
                new Vector3(-1f, 0.5f, 0f),
                new Vector3(1f, 0.5f, 0f),
                new Vector3(3f, 0.5f, 0f)
            };

            for (int i = 0; i < 4; i++)
            {
                // Subsystem — child of CentralDashboard (participant may need this for wiring)
                var sub = new GameObject(names[i]);
                sub.transform.SetParent(dashboard.transform);
                sub.transform.position = positions[i];

                // SubsystemAlerter_Events_Starter has empty RaiseAlert body — participant implements
                // No dashboard reference wired here — participant adds CentralDashboard_Events
                // to CentralDashboard and drags it into their alerter's dashboard field
                var alerter = sub.AddComponent<SubsystemAlerter_Events_Starter>();
                var alerterSO = new SerializedObject(alerter);
                alerterSO.FindProperty("subsystemName").stringValue = subNames[i];
                alerterSO.ApplyModifiedProperties();

                // AlertSimulator is always present — it's a shared data source
                var alertSim = sub.AddComponent<AlertSimulator>();
                alertSim.subsystemName = subNames[i];
                alertSim.possibleAlerts = alerts[i];

                // Wire AlertSimulator → SubsystemAlerter_Events_Starter.RaiseAlert
                WirePersistentListener(alertSim, "OnAlertGenerated", alerter, "RaiseAlert");

                var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vis.name = "Visual";
                vis.transform.SetParent(sub.transform);
                vis.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                vis.GetComponent<Renderer>().sharedMaterial = GetOrCreateMat("SensorMat", new Color(0.1f, 0.15f, 0.5f));

                CreateWorldCanvas(sub, subNames[i], new Vector3(0, 0.8f, 0), new Vector2(60, 15), 6);
            }

            AddGoalCanvas("GOAL: Wire 4 subsystem alerts to the central dashboard.\nAlerts should appear in the dashboard log.");

            SaveScene(scene, "T4_AlertAggregation_Events_Starter");
        }

        // ================================================================
        // GOAL CANVAS HELPER
        // ================================================================

        private static void AddGoalCanvas(string goalText)
        {
            var goalCanvas = new GameObject("GoalCanvas");
            var canvasComp = goalCanvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.sortingOrder = 99;
            goalCanvas.AddComponent<CanvasScaler>();
            goalCanvas.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("GoalPanel");
            panel.transform.SetParent(goalCanvas.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 1f);
            panelRT.anchorMax = new Vector2(0.5f, 1f);
            panelRT.pivot = new Vector2(0.5f, 1f);
            panelRT.anchoredPosition = new Vector2(0f, -10f);
            panelRT.sizeDelta = new Vector2(500f, 60f);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

            var textObj = new GameObject("GoalText");
            textObj.transform.SetParent(panel.transform, false);
            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.02f, 0.05f);
            textRT.anchorMax = new Vector2(0.98f, 0.95f);
            textRT.sizeDelta = Vector2.zero;
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = goalText;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }
}
