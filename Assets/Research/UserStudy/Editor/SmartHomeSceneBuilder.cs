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
using UserStudy.SmartHome.Mercury;

namespace UserStudy.Editor
{
    /// <summary>
    /// Automated scene builder for Smart Home Mercury scene.
    /// Creates all GameObjects, components, and UI elements automatically.
    /// </summary>
    public class SmartHomeSceneBuilder : EditorWindow
    {
        [MenuItem("MercuryMessaging/User Study/Build Smart Home Mercury Scene")]
        public static void BuildScene()
        {
            if (EditorUtility.DisplayDialog(
                "Build Smart Home Scene",
                "This will create the Smart Home Mercury scene with all GameObjects, components, and UI.\n\nContinue?",
                "Yes", "Cancel"))
            {
                CreateScene();
            }
        }

        private static void CreateScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Debug.Log("Building Smart Home Mercury Scene...");

            // Step 1: Create Materials
            Material lightOnMat = CreateLightOnMaterial();
            Material lightOffMat = CreateLightOffMaterial();

            // Step 2: Create Root Hub
            GameObject hub = CreateSmartHomeHub();

            // Step 3: Create Control Panel
            GameObject controlPanel = CreateControlPanel(hub);

            // Step 4: Create Rooms
            GameObject bedroomRoom = CreateRoom(hub, "Room_Bedroom", new Vector3(-5, 0, 0));
            GameObject kitchenRoom = CreateRoom(hub, "Room_Kitchen", new Vector3(0, 0, 0));
            GameObject livingRoom = CreateRoom(hub, "Room_LivingRoom", new Vector3(5, 0, 0));

            // Step 5: Create Devices
            // Bedroom devices
            CreateSmartLight(bedroomRoom, "SmartLight_Bedroom1", new Vector3(-5, 2, 0), lightOnMat, lightOffMat);
            CreateSmartLight(bedroomRoom, "SmartLight_Bedroom2", new Vector3(-4, 2, 0), lightOnMat, lightOffMat);
            CreateThermostat(bedroomRoom, "Thermostat_Bedroom", new Vector3(-5, 0, 1));
            CreateSmartBlinds(bedroomRoom, "SmartBlinds_Bedroom", new Vector3(-5, 1.5f, -1));

            // Kitchen devices
            CreateSmartLight(kitchenRoom, "SmartLight_Kitchen1", new Vector3(0, 2, 0), lightOnMat, lightOffMat);
            CreateSmartLight(kitchenRoom, "SmartLight_Kitchen2", new Vector3(-1, 2, 0), lightOnMat, lightOffMat);
            CreateSmartLight(kitchenRoom, "SmartLight_Kitchen3", new Vector3(1, 2, 0), lightOnMat, lightOffMat);

            // Living room devices
            CreateSmartLight(livingRoom, "SmartLight_Living1", new Vector3(5, 2, 0), lightOnMat, lightOffMat);
            CreateSmartLight(livingRoom, "SmartLight_Living2", new Vector3(6, 2, 0), lightOnMat, lightOffMat);
            CreateThermostat(livingRoom, "Thermostat_Living", new Vector3(5, 0, 1));
            CreateSmartBlinds(livingRoom, "SmartBlinds_Living", new Vector3(5, 1.5f, -1));
            CreateMusicPlayer(livingRoom, "MusicPlayer_Living", new Vector3(5, 0, -2));

            // Step 6: Create UI Canvas
            CreateUICanvas(hub, controlPanel, bedroomRoom, kitchenRoom, livingRoom);

            // Step 7: Save scene
            string scenePath = "Assets/UserStudy/Scenes/SmartHome_Mercury.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            Debug.Log($"Smart Home Mercury Scene created successfully at: {scenePath}");
            Debug.Log("Total GameObjects created: 16");
            Debug.Log("Inspector connections for messaging: 0");

            EditorUtility.DisplayDialog(
                "Scene Built Successfully",
                "Smart Home Mercury scene has been created!\n\n" +
                "Location: Assets/UserStudy/Scenes/SmartHome_Mercury.unity\n\n" +
                "Next steps:\n" +
                "1. Open the scene\n" +
                "2. Press Play to test\n" +
                "3. Use control panel buttons to test messaging",
                "OK");
        }

        #region Materials

        private static Material CreateLightOnMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "LightOn";
            mat.color = Color.yellow;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.yellow * 2f);

            string path = "Assets/UserStudy/Materials/LightOn.mat";
            System.IO.Directory.CreateDirectory("Assets/UserStudy/Materials");
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateLightOffMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = "LightOff";
            mat.color = new Color(0.2f, 0.2f, 0.2f);

            string path = "Assets/UserStudy/Materials/LightOff.mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        #endregion

        #region Hub and Control Panel

        private static GameObject CreateSmartHomeHub()
        {
            GameObject hub = new GameObject("SmartHomeHub");
            hub.transform.position = Vector3.zero;

            // Add MercuryMessaging components
            hub.AddComponent<MmRelayNode>();
            hub.AddComponent<SmartHomeHub>();

            return hub;
        }

        private static GameObject CreateControlPanel(GameObject parent)
        {
            GameObject panel = new GameObject("ControlPanel");
            panel.transform.SetParent(parent.transform);
            panel.transform.localPosition = new Vector3(0, 3, 0);

            // Add MercuryMessaging components
            panel.AddComponent<MmBaseResponder>();
            panel.AddComponent<MmRelayNode>();
            panel.AddComponent<ControlPanel>();

            return panel;
        }

        #endregion

        #region Rooms

        private static GameObject CreateRoom(GameObject parent, string roomName, Vector3 position)
        {
            GameObject room = new GameObject(roomName);
            room.transform.SetParent(parent.transform);
            room.transform.position = position;

            // Add MercuryMessaging relay node
            room.AddComponent<MmRelayNode>();

            // Add visual floor (optional)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(3, 0.1f, 3);

            return room;
        }

        #endregion

        #region Devices

        private static void CreateSmartLight(GameObject parent, string name, Vector3 position, Material onMat, Material offMat)
        {
            GameObject light = new GameObject(name);
            light.transform.SetParent(parent.transform);
            light.transform.position = position;

            // Add MercuryMessaging components
            light.AddComponent<MmBaseResponder>();
            light.AddComponent<MmRelayNode>();
            var smartLight = light.AddComponent<SmartLight>();

            // Add Unity Light component
            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Point;
            lightComp.range = 5f;
            lightComp.intensity = 2f;
            lightComp.color = Color.yellow;

            // Add visual bulb
            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Bulb";
            bulb.transform.SetParent(light.transform);
            bulb.transform.localPosition = Vector3.zero;
            bulb.transform.localScale = Vector3.one * 0.3f;

            // Assign materials via SerializedObject (proper way to set private fields)
            SerializedObject so = new SerializedObject(smartLight);
            so.FindProperty("lightComponent").objectReferenceValue = lightComp;
            so.FindProperty("bulbRenderer").objectReferenceValue = bulb.GetComponent<Renderer>();
            so.FindProperty("onMaterial").objectReferenceValue = onMat;
            so.FindProperty("offMaterial").objectReferenceValue = offMat;
            so.ApplyModifiedProperties();
        }

        private static void CreateThermostat(GameObject parent, string name, Vector3 position)
        {
            GameObject thermostat = new GameObject(name);
            thermostat.transform.SetParent(parent.transform);
            thermostat.transform.position = position;

            // Add MercuryMessaging components
            thermostat.AddComponent<MmBaseResponder>();
            thermostat.AddComponent<MmRelayNode>();
            thermostat.AddComponent<Thermostat>();

            // Add visual cube
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(thermostat.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.1f);

            // Add temperature display (optional)
            GameObject textObj = new GameObject("TempDisplay");
            textObj.transform.SetParent(thermostat.transform);
            textObj.transform.localPosition = new Vector3(0, 0, -0.06f);
            var tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "22°C";
            tmp.fontSize = 3;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void CreateSmartBlinds(GameObject parent, string name, Vector3 position)
        {
            GameObject blinds = new GameObject(name);
            blinds.transform.SetParent(parent.transform);
            blinds.transform.position = position;

            // Add MercuryMessaging components
            blinds.AddComponent<MmBaseResponder>();
            blinds.AddComponent<MmRelayNode>();
            var smartBlinds = blinds.AddComponent<SmartBlinds>();

            // Add visual blinds
            GameObject blindsVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blindsVisual.name = "BlindsVisual";
            blindsVisual.transform.SetParent(blinds.transform);
            blindsVisual.transform.localPosition = Vector3.zero;
            blindsVisual.transform.localScale = new Vector3(2f, 0.1f, 1.5f);

            // Assign via SerializedObject
            SerializedObject so = new SerializedObject(smartBlinds);
            so.FindProperty("blindsTransform").objectReferenceValue = blindsVisual.transform;
            so.ApplyModifiedProperties();
        }

        private static void CreateMusicPlayer(GameObject parent, string name, Vector3 position)
        {
            GameObject player = new GameObject(name);
            player.transform.SetParent(parent.transform);
            player.transform.position = position;

            // Add MercuryMessaging components
            player.AddComponent<MmBaseResponder>();
            player.AddComponent<MmRelayNode>();
            player.AddComponent<MusicPlayer>();

            // Add AudioSource
            var audioSource = player.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            Debug.Log($"Note: Assign an audio clip to {name}'s AudioSource component");

            // Add visual speaker
            GameObject speaker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            speaker.name = "Speaker";
            speaker.transform.SetParent(player.transform);
            speaker.transform.localPosition = Vector3.zero;
            speaker.transform.localScale = new Vector3(0.6f, 0.8f, 0.4f);
        }

        #endregion

        #region UI Canvas

        private static void CreateUICanvas(GameObject hub, GameObject controlPanel, GameObject bedroom, GameObject kitchen, GameObject living)
        {
            // Create EventSystem (required for UI input!)
            // Check if one already exists to avoid conflicts
            GameObject eventSystemObj = GameObject.Find("EventSystem");
            if (eventSystemObj == null)
            {
                eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // Ensure an input module exists (required for mouse/keyboard input)
            // Without this, EventSystem cannot process ANY input events
            var inputModule = eventSystemObj.GetComponent<UnityEngine.EventSystems.BaseInputModule>();
            if (inputModule == null)
            {
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("EventSystem created with StandaloneInputModule for desktop input.");
            }
            else
            {
                Debug.Log($"EventSystem already has input module: {inputModule.GetType().Name}");
            }

            // Create Canvas (Desktop UI - mouse/keyboard interaction only)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Desktop overlay mode
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>(); // Handles mouse clicks

            // Create Panel
            GameObject panel = CreateUIPanel(canvasObj);

            // Create Status Text
            GameObject statusText = CreateStatusText(panel);

            // Assign to ControlPanel
            var controlPanelScript = controlPanel.GetComponent<ControlPanel>();
            SerializedObject so = new SerializedObject(controlPanelScript);
            so.FindProperty("statusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedProperties();

            // Create ON/OFF Button Pairs
            float yPos = 200f;

            // All Devices Row
            CreateButton(panel, "AllOnButton", "All ON", new Vector2(-100, yPos), controlPanel, "OnAllOnButton");
            CreateButton(panel, "AllOffButton", "All OFF", new Vector2(100, yPos), controlPanel, "OnAllOffButton");
            yPos -= 50f;

            // Lights Row
            CreateButton(panel, "LightsOnButton", "Lights ON", new Vector2(-100, yPos), controlPanel, "OnLightsOnButton");
            CreateButton(panel, "LightsOffButton", "Lights OFF", new Vector2(100, yPos), controlPanel, "OnLightsOffButton");
            yPos -= 50f;

            // Climate Row
            CreateButton(panel, "ClimateOnButton", "Climate ON", new Vector2(-100, yPos), controlPanel, "OnClimateOnButton");
            CreateButton(panel, "ClimateOffButton", "Climate OFF", new Vector2(100, yPos), controlPanel, "OnClimateOffButton");
            yPos -= 50f;

            // Music Row
            CreateButton(panel, "MusicOnButton", "Music ON", new Vector2(-100, yPos), controlPanel, "OnMusicOnButton");
            CreateButton(panel, "MusicOffButton", "Music OFF", new Vector2(100, yPos), controlPanel, "OnMusicOffButton");
            yPos -= 50f;

            // Room Row
            CreateButton(panel, "RoomOnButton", "Room ON", new Vector2(-100, yPos), controlPanel, "OnRoomOnButton");
            CreateButton(panel, "RoomOffButton", "Room OFF", new Vector2(100, yPos), controlPanel, "OnRoomOffButton");
            yPos -= 70f;

            // Brightness Slider
            GameObject brightnessSlider = CreateSlider(panel, "BrightnessSlider", new Vector2(0, yPos), "Brightness", 0f, 1f, 1f);
            WireSlider(brightnessSlider, controlPanel, "OnBrightnessChanged");
            yPos -= 60f;

            // Temperature Slider
            GameObject tempSlider = CreateSlider(panel, "TemperatureSlider", new Vector2(0, yPos), "Temperature (16-30°C)", 16f, 30f, 22f);
            WireSlider(tempSlider, controlPanel, "OnTemperatureChanged");
            yPos -= 80f;

            // Create Mode Buttons
            CreateButton(panel, "HomeModeButton", "Home Mode", new Vector2(-130, yPos), hub, "SetMode", "Home");
            CreateButton(panel, "AwayModeButton", "Away Mode", new Vector2(0, yPos), hub, "SetMode", "Away");
            CreateButton(panel, "SleepModeButton", "Sleep Mode", new Vector2(130, yPos), hub, "SetMode", "Sleep");
            yPos -= 70f;

            // Create and Wire Dropdown
            GameObject dropdownObj = CreateDropdown(panel, new Vector2(0, yPos));
            WireDropdown(dropdownObj, controlPanel, bedroom, kitchen, living);

            Debug.Log("Enhanced UI Canvas created: ON/OFF buttons, sliders, and wired dropdown");
        }

        private static GameObject CreateUIPanel(GameObject parent)
        {
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(parent.transform, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 600);
            rt.anchoredPosition = Vector2.zero;

            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            img.raycastTarget = false; // Don't block button clicks

            return panel;
        }

        private static GameObject CreateStatusText(GameObject parent)
        {
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(350, 80);
            rt.anchoredPosition = new Vector2(0, -20);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Status: Ready";
            tmp.fontSize = 20; // Larger font for better visibility
            tmp.fontStyle = TMPro.FontStyles.Bold; // Bold for emphasis
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return textObj;
        }

        private static void CreateButton(GameObject parent, string name, string text, Vector2 position, GameObject target, string methodName, string stringParam = null)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 50);
            rt.anchoredPosition = position;

            Image img = buttonObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.7f);

            Button button = buttonObj.AddComponent<Button>();
            button.interactable = true; // CRITICAL: Enable interaction
            button.targetGraphic = img; // Set target for color transitions

            // Configure color transitions for visual feedback
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f);
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.selectedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            // Create button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Wire button click
            UnityEngine.Events.UnityAction action;
            if (stringParam != null)
            {
                // For SetMode methods with string parameter
                action = () => {
                    var component = target.GetComponent<SmartHomeHub>();
                    if (component != null)
                        component.SetMode(stringParam);
                };
            }
            else
            {
                // For parameterless methods
                action = () => {
                    target.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
                };
            }
            button.onClick.AddListener(action);

            Debug.Log($"Button '{text}' wired to {target.name}.{methodName}()");
        }

        private static GameObject CreateDropdown(GameObject parent, Vector2 position)
        {
            GameObject dropdownObj = new GameObject("RoomDropdown");
            dropdownObj.transform.SetParent(parent.transform, false);

            RectTransform rt = dropdownObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 30);
            rt.anchoredPosition = position;

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Bedroom"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Kitchen"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Living Room"));

            // Add label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(dropdownObj.transform, false);
            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "Bedroom";
            tmp.fontSize = 14;

            SerializedObject so = new SerializedObject(dropdown);
            so.FindProperty("m_CaptionText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();

            return dropdownObj;
        }

        private static GameObject CreateSlider(GameObject parent, string name, Vector2 position, string labelText, float minValue, float maxValue, float defaultValue)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent.transform, false);

            RectTransform rt = sliderObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 30);
            rt.anchoredPosition = position;

            UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();
            slider.interactable = true; // CRITICAL: Enable interaction
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;

            // Create background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(0, 20);
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);
            bgImage.raycastTarget = false; // Background doesn't need clicks

            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.sizeDelta = new Vector2(-20, 0);
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 1f);
            fillImage.raycastTarget = false; // Fill doesn't need clicks

            // Create handle slide area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            handleImage.raycastTarget = true; // CRITICAL: Handle needs clicks for dragging

            // Wire slider components
            SerializedObject so = new SerializedObject(slider);
            so.FindProperty("m_FillRect").objectReferenceValue = fillRect;
            so.FindProperty("m_HandleRect").objectReferenceValue = handleRect;
            so.ApplyModifiedProperties();

            // Add label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(sliderObj.transform, false);
            RectTransform labelRect = label.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(0, 20);
            labelRect.sizeDelta = new Vector2(300, 20);
            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;

            // Add value display for real-time feedback
            GameObject valueText = new GameObject("ValueText");
            valueText.transform.SetParent(sliderObj.transform, false);
            RectTransform valueRect = valueText.AddComponent<RectTransform>();
            valueRect.anchoredPosition = new Vector2(160, 0); // Right of slider
            valueRect.sizeDelta = new Vector2(60, 30);
            TextMeshProUGUI valueTMP = valueText.AddComponent<TextMeshProUGUI>();
            valueTMP.fontSize = 14;
            valueTMP.alignment = TextAlignmentOptions.Center;
            valueTMP.color = Color.white;

            return sliderObj;
        }

        private static void WireSlider(GameObject sliderObj, GameObject target, string methodName)
        {
            UnityEngine.UI.Slider slider = sliderObj.GetComponent<UnityEngine.UI.Slider>();
            ControlPanel controlPanelScript = target.GetComponent<ControlPanel>();

            // Get value display text and pass to ControlPanel
            // ControlPanel owns UI elements, so it updates them directly
            TextMeshProUGUI valueTMP = sliderObj.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();

            if (methodName == "OnBrightnessChanged")
            {
                // Give ControlPanel ownership of the value display
                controlPanelScript.SetBrightnessDisplay(valueTMP);

                // Simple listener - ControlPanel handles both UI update and message sending
                slider.onValueChanged.AddListener(controlPanelScript.OnBrightnessChanged);
            }
            else if (methodName == "OnTemperatureChanged")
            {
                // Give ControlPanel ownership of the value display
                controlPanelScript.SetTemperatureDisplay(valueTMP);

                // Simple listener - ControlPanel handles both UI update and message sending
                slider.onValueChanged.AddListener(controlPanelScript.OnTemperatureChanged);
            }
        }

        private static void WireDropdown(GameObject dropdownObj, GameObject controlPanel, GameObject bedroom, GameObject kitchen, GameObject living)
        {
            TMP_Dropdown dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
            ControlPanel controlPanelScript = controlPanel.GetComponent<ControlPanel>();

            dropdown.onValueChanged.AddListener((index) => {
                GameObject selectedRoom = null;
                switch (index)
                {
                    case 0: selectedRoom = bedroom; break;
                    case 1: selectedRoom = kitchen; break;
                    case 2: selectedRoom = living; break;
                }
                if (selectedRoom != null)
                {
                    controlPanelScript.SelectRoom(selectedRoom);
                    Debug.Log($"Room selected: {selectedRoom.name}");
                }
            });

            Debug.Log("Dropdown wired to ControlPanel.SelectRoom()");
        }

        #endregion
    }
}
