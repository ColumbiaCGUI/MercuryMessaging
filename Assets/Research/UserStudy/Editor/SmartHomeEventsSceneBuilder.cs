using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UserStudy.SmartHome.Events;

namespace UserStudy.Editor
{
    /// <summary>
    /// Automated scene builder for Smart Home scene using Unity Events approach.
    /// Creates the scene with all Inspector connections wired (~40 connections).
    /// Contrast with Mercury: Requires extensive manual wiring in Inspector.
    /// </summary>
    public class SmartHomeEventsSceneBuilder
    {
        [MenuItem("MercuryMessaging/User Study/Build Smart Home Events Scene")]
        public static void BuildScene()
        {
            if (EditorUtility.DisplayDialog(
                "Build Smart Home Events Scene",
                "This will create a new scene: SmartHome_Events.unity\n\n" +
                "This version uses Unity Events with ~40 Inspector connections.\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                CreateScene();
            }
        }

        private static void CreateScene()
        {
            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create materials
            Material lightOnMat = CreateLightOnMaterial();
            Material lightOffMat = CreateLightOffMaterial();

            // Create controller
            GameObject controller = CreateSmartHomeController();
            SmartHomeController controllerScript = controller.GetComponent<SmartHomeController>();

            // Create rooms and devices
            GameObject bedroomRoom = CreateRoom("Room_Bedroom", new Vector3(-5, 0, 0));
            GameObject kitchenRoom = CreateRoom("Room_Kitchen", new Vector3(0, 0, 0));
            GameObject livingRoom = CreateRoom("Room_LivingRoom", new Vector3(5, 0, 0));

            // Create bedroom devices
            var bedroomLight1 = CreateSmartLight_Events(bedroomRoom, "SmartLight_Bedroom1", new Vector3(-5, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var bedroomLight2 = CreateSmartLight_Events(bedroomRoom, "SmartLight_Bedroom2", new Vector3(-4, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var bedroomThermostat = CreateThermostat_Events(bedroomRoom, "Thermostat_Bedroom", new Vector3(-5, 1, 0), controllerScript);
            var bedroomBlinds = CreateSmartBlinds_Events(bedroomRoom, "SmartBlinds_Bedroom", new Vector3(-5, 3, 0), controllerScript);

            // Create kitchen devices
            var kitchenLight1 = CreateSmartLight_Events(kitchenRoom, "SmartLight_Kitchen1", new Vector3(0, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var kitchenLight2 = CreateSmartLight_Events(kitchenRoom, "SmartLight_Kitchen2", new Vector3(1, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var kitchenLight3 = CreateSmartLight_Events(kitchenRoom, "SmartLight_Kitchen3", new Vector3(-1, 2, 0), lightOnMat, lightOffMat, controllerScript);

            // Create living room devices
            var livingLight1 = CreateSmartLight_Events(livingRoom, "SmartLight_Living1", new Vector3(5, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var livingLight2 = CreateSmartLight_Events(livingRoom, "SmartLight_Living2", new Vector3(6, 2, 0), lightOnMat, lightOffMat, controllerScript);
            var livingThermostat = CreateThermostat_Events(livingRoom, "Thermostat_Living", new Vector3(5, 1, 0), controllerScript);
            var livingBlinds = CreateSmartBlinds_Events(livingRoom, "SmartBlinds_Living", new Vector3(5, 3, 0), controllerScript);
            var livingMusic = CreateMusicPlayer_Events(livingRoom, "MusicPlayer_Living", new Vector3(5, 0, 0), controllerScript);

            // Populate controller device lists - THIS IS THE KEY MANUAL WORK!
            PopulateControllerDeviceLists(controllerScript,
                new[] { bedroomLight1, bedroomLight2, kitchenLight1, kitchenLight2, kitchenLight3, livingLight1, livingLight2 },
                new ISmartDevice[] { bedroomThermostat, bedroomBlinds, livingThermostat, livingBlinds },
                new[] { livingMusic },
                new ISmartDevice[] { bedroomLight1, bedroomLight2, bedroomThermostat, bedroomBlinds },
                new ISmartDevice[] { kitchenLight1, kitchenLight2, kitchenLight3 },
                new ISmartDevice[] { livingLight1, livingLight2, livingThermostat, livingBlinds, livingMusic });

            // Create UI
            CreateUICanvas(controller, controllerScript);

            // Save scene
            EditorSceneManager.SaveScene(scene, "Assets/UserStudy/Scenes/SmartHome_Events.unity");

            Debug.Log("Smart Home Events scene created successfully!");
            Debug.Log("Inspector Connections: ~40 (device lists + UI events)");
            Debug.Log("Next step: Test scene and compare with Mercury version");
        }

        #region GameObject Creation
        private static GameObject CreateSmartHomeController()
        {
            GameObject controller = new GameObject("SmartHomeController");
            controller.AddComponent<SmartHomeController>();
            return controller;
        }

        private static GameObject CreateRoom(string roomName, Vector3 position)
        {
            GameObject room = new GameObject(roomName);
            room.transform.position = position;
            return room;
        }

        private static ISmartDevice CreateSmartLight_Events(GameObject parent, string name, Vector3 position, Material onMat, Material offMat, SmartHomeController controller)
        {
            GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            light.name = name;
            light.transform.SetParent(parent.transform);
            light.transform.localPosition = position;

            // Add light component
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Point;
            lightComp.range = 10f;
            lightComp.intensity = 1f;

            // Add script and wire controller reference
            SmartLight_Events script = light.AddComponent<SmartLight_Events>();

            // Wire controller reference via SerializedObject
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("controller").objectReferenceValue = controller;
            so.FindProperty("lightComponent").objectReferenceValue = lightComp;
            so.FindProperty("bulbRenderer").objectReferenceValue = light.GetComponent<Renderer>();
            so.FindProperty("onMaterial").objectReferenceValue = onMat;
            so.FindProperty("offMaterial").objectReferenceValue = offMat;
            so.ApplyModifiedProperties();

            return script;
        }

        private static ISmartDevice CreateThermostat_Events(GameObject parent, string name, Vector3 position, SmartHomeController controller)
        {
            GameObject thermo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            thermo.name = name;
            thermo.transform.SetParent(parent.transform);
            thermo.transform.localPosition = position;
            thermo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Thermostat_Events script = thermo.AddComponent<Thermostat_Events>();

            // Wire controller reference
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("controller").objectReferenceValue = controller;
            so.ApplyModifiedProperties();

            return script;
        }

        private static ISmartDevice CreateSmartBlinds_Events(GameObject parent, string name, Vector3 position, SmartHomeController controller)
        {
            GameObject blinds = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blinds.name = name;
            blinds.transform.SetParent(parent.transform);
            blinds.transform.localPosition = position;
            blinds.transform.localScale = new Vector3(2f, 1f, 0.1f);

            SmartBlinds_Events script = blinds.AddComponent<SmartBlinds_Events>();

            // Wire controller reference and blinds transform
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("controller").objectReferenceValue = controller;
            so.FindProperty("blindsTransform").objectReferenceValue = blinds.transform;
            so.ApplyModifiedProperties();

            return script;
        }

        private static ISmartDevice CreateMusicPlayer_Events(GameObject parent, string name, Vector3 position, SmartHomeController controller)
        {
            GameObject music = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            music.name = name;
            music.transform.SetParent(parent.transform);
            music.transform.localPosition = position;
            music.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            AudioSource audioSource = music.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            MusicPlayer_Events script = music.AddComponent<MusicPlayer_Events>();

            // Wire controller reference
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("controller").objectReferenceValue = controller;
            so.FindProperty("audioSource").objectReferenceValue = audioSource;
            so.ApplyModifiedProperties();

            return script;
        }
        #endregion

        #region Manual Device List Population - KEY DIFFERENCE FROM MERCURY!
        private static void PopulateControllerDeviceLists(
            SmartHomeController controller,
            ISmartDevice[] lights,
            ISmartDevice[] climate,
            ISmartDevice[] entertainment,
            ISmartDevice[] bedroom,
            ISmartDevice[] kitchen,
            ISmartDevice[] living)
        {
            SerializedObject so = new SerializedObject(controller);

            // Populate all devices list
            var allDevicesList = lights.ToList();
            allDevicesList.AddRange(climate);
            allDevicesList.AddRange(entertainment);
            SetDeviceList(so, "allDevices", allDevicesList.ToArray());

            // Populate type-specific lists
            SetDeviceList(so, "lightDevices", lights);
            SetDeviceList(so, "climateDevices", climate);
            SetDeviceList(so, "entertainmentDevices", entertainment);

            // Populate room-specific lists
            SetDeviceList(so, "bedroomDevices", bedroom);
            SetDeviceList(so, "kitchenDevices", kitchen);
            SetDeviceList(so, "livingRoomDevices", living);

            so.ApplyModifiedProperties();
        }

        private static void SetDeviceList(SerializedObject so, string propertyName, ISmartDevice[] devices)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            prop.arraySize = devices.Length;

            for (int i = 0; i < devices.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = devices[i] as MonoBehaviour;
            }
        }
        #endregion

        #region UI Creation with Manual Button Wiring
        private static void CreateUICanvas(GameObject controller, SmartHomeController controllerScript)
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create Panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObj.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(-10, -10);

            // Create buttons with manual wiring
            CreateButton(panel, "AllOffButton", "All Off", new Vector2(10, -10), controllerScript, "OnAllOffButton");
            CreateButton(panel, "LightsOffButton", "Lights Off", new Vector2(10, -60), controllerScript, "OnLightsOffButton");
            CreateButton(panel, "ClimateOffButton", "Climate Off", new Vector2(10, -110), controllerScript, "OnClimateOffButton");
            CreateButton(panel, "RoomOffButton", "Room Off", new Vector2(10, -160), controllerScript, "OnRoomOffButton");

            CreateButton(panel, "HomeModeButton", "Home Mode", new Vector2(200, -10), controllerScript, "OnHomeModeButton");
            CreateButton(panel, "AwayModeButton", "Away Mode", new Vector2(200, -60), controllerScript, "OnAwayModeButton");
            CreateButton(panel, "SleepModeButton", "Sleep Mode", new Vector2(200, -110), controllerScript, "OnSleepModeButton");

            // Create status text
            GameObject statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(panel.transform);
            RectTransform statusRect = statusTextObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0.3f);
            statusRect.offsetMin = new Vector2(10, 10);
            statusRect.offsetMax = new Vector2(-10, -10);

            TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Status: Ready";
            statusText.fontSize = 18;
            statusText.alignment = TextAlignmentOptions.TopLeft;

            // Wire status text to controller
            SerializedObject so = new SerializedObject(controllerScript);
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.ApplyModifiedProperties();
        }

        private static void CreateButton(GameObject parent, string name, string text, Vector2 position, SmartHomeController controller, string methodName)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(180, 40);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f);

            Button button = buttonObj.AddComponent<Button>();

            // Wire button click to controller method - MANUAL WIRING!
            UnityEngine.Events.UnityAction action = null;
            switch (methodName)
            {
                case "OnAllOffButton": action = controller.OnAllOffButton; break;
                case "OnLightsOffButton": action = controller.OnLightsOffButton; break;
                case "OnClimateOffButton": action = controller.OnClimateOffButton; break;
                case "OnRoomOffButton": action = controller.OnRoomOffButton; break;
                case "OnHomeModeButton": action = controller.OnHomeModeButton; break;
                case "OnAwayModeButton": action = controller.OnAwayModeButton; break;
                case "OnSleepModeButton": action = controller.OnSleepModeButton; break;
            }

            if (action != null)
            {
                button.onClick.AddListener(action);
            }

            // Create button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
        }
        #endregion

        #region Material Creation
        private static Material CreateLightOnMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "LightOn_Events";
            mat.color = Color.yellow;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.yellow * 2f);

            AssetDatabase.CreateAsset(mat, "Assets/UserStudy/Materials/LightOn_Events.mat");
            return mat;
        }

        private static Material CreateLightOffMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "LightOff_Events";
            mat.color = new Color(0.2f, 0.2f, 0.2f);

            AssetDatabase.CreateAsset(mat, "Assets/UserStudy/Materials/LightOff_Events.mat");
            return mat;
        }
        #endregion
    }
}
