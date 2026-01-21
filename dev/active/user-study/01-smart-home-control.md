# Scene 1: Smart Home Control Panel

## Scene Overview

**Complexity:** Simple
**Estimated Time:** 30-45 minutes
**Priority:** ⭐⭐⭐⭐⭐ (RECOMMENDED START)

**Description:**
A single room with multiple smart home devices (lights, thermostat, blinds, music player) controlled by a central touch panel. Participants implement hierarchical control and state synchronization demonstrating the differences between MercuryMessaging and Unity Events.

**Why This Scene:**
- **Simplest to implement** - Best for first-time participants
- **Clearest Mercury advantages** - Hierarchy, tags, and FSM all shine
- **Familiar domain** - Everyone understands smart home automation
- **Fair comparison** - Implementable with Unity Events, just more tedious
- **Scalable tasks** - Easy to adjust difficulty

---

## Learning Objectives

Participants will experience:

1. **Hierarchical Broadcasting** - Control panel sends one message that reaches all devices
2. **Room-Level Grouping** - Commands target specific rooms via GameObject hierarchy
3. **Tag-Based Filtering** - Target device types (lights, climate, entertainment) without manual wiring
4. **State Management** - Switch between home modes (Home/Away/Sleep) with FSM
5. **Parent Notification** - Devices report status back to control panel
6. **Loose Coupling** - No Inspector references required (Mercury) vs tight coupling (Unity Events)

---

## Object Hierarchy

### MercuryMessaging Implementation

```
SmartHomeHub (MmRelaySwitchNode - FSM for modes)
├── ControlPanel (MmBaseResponder - UI)
├── Room_Bedroom (MmRelayNode - Tag: Room)
│   ├── SmartLight_Bedroom1 (MmBaseResponder - Tag: Light)
│   ├── SmartLight_Bedroom2 (MmBaseResponder - Tag: Light)
│   ├── Thermostat_Bedroom (MmBaseResponder - Tag: Climate)
│   └── SmartBlinds_Bedroom (MmBaseResponder - Tag: Climate)
├── Room_Kitchen (MmRelayNode - Tag: Room)
│   ├── SmartLight_Kitchen1 (MmBaseResponder - Tag: Light)
│   ├── SmartLight_Kitchen2 (MmBaseResponder - Tag: Light)
│   └── SmartLight_Kitchen3 (MmBaseResponder - Tag: Light)
└── Room_LivingRoom (MmRelayNode - Tag: Room)
    ├── SmartLight_Living1 (MmBaseResponder - Tag: Light)
    ├── SmartLight_Living2 (MmBaseResponder - Tag: Light)
    ├── Thermostat_Living (MmBaseResponder - Tag: Climate)
    ├── SmartBlinds_Living (MmBaseResponder - Tag: Climate)
    └── MusicPlayer_Living (MmBaseResponder - Tag: Entertainment)
```

**Component Counts:**
- Total GameObjects: 16
- MmRelayNode: 4 (Hub + 3 Rooms)
- MmBaseResponder: 12 (devices)

**Tag Assignments:**
- `MmTag.Tag0` - Lights (8 devices)
- `MmTag.Tag1` - Climate (4 devices: 2 thermostats, 2 blinds)
- `MmTag.Tag2` - Entertainment (1 device: music player)
- `MmTag.Tag3` - Room (3 rooms)

**FSM States (SmartHomeHub):**
- `Home` - Normal operation, all devices enabled
- `Away` - Lights off, thermostat energy-saving, blinds closed
- `Sleep` - Lights dimmed, thermostat night mode, music off

---

### Unity Events Implementation

```
SmartHomeController (MonoBehaviour - custom state machine)
├── ControlPanelUI (MonoBehaviour)
├── Room_Bedroom (MonoBehaviour - optional grouping)
│   ├── SmartLight_Bedroom1 (SmartLightBehaviour)
│   ├── SmartLight_Bedroom2 (SmartLightBehaviour)
│   ├── Thermostat_Bedroom (ThermostatBehaviour)
│   └── SmartBlinds_Bedroom (SmartBlindsBehaviour)
├── Room_Kitchen (MonoBehaviour - optional grouping)
│   ├── SmartLight_Kitchen1 (SmartLightBehaviour)
│   ├── SmartLight_Kitchen2 (SmartLightBehaviour)
│   └── SmartLight_Kitchen3 (SmartLightBehaviour)
└── Room_LivingRoom (MonoBehaviour - optional grouping)
    ├── SmartLight_Living1 (SmartLightBehaviour)
    ├── SmartLight_Living2 (SmartLightBehaviour)
    ├── Thermostat_Living (ThermostatBehaviour)
    ├── SmartBlinds_Living (SmartBlindsBehaviour)
    └── MusicPlayer_Living (MusicPlayerBehaviour)
```

**Component Counts:**
- Total GameObjects: 16
- Controller: 1 (SmartHomeController with custom state machine)
- Device scripts: 12 (each with UnityEvent)

**Inspector Wiring Required:**
- ControlPanel → SmartHomeController: 1 reference
- SmartHomeController → All 12 devices: 12 references
- Each device → ControlPanel (for status updates): 12 UnityEvent connections
- **Total: 25 manual connections**

---

## Communication Patterns

### Pattern 1: Broadcast to All Devices

**Scenario:** User presses "All Off" button on control panel

**MercuryMessaging:**
```csharp
// In ControlPanel.cs
public void OnAllOffButton() {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.SetActive,
        false,
        new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All)
    );
}
```
- **Single method call** reaches all 12 devices
- **No references** to specific devices needed
- **Automatic hierarchy traversal**

**Unity Events:**
```csharp
// In SmartHomeController.cs
[SerializeField] private List<ISmartDevice> allDevices; // 12 references

public void OnAllOffButton() {
    foreach (var device in allDevices) {
        device.TurnOff();
    }
}
```
- **12 manual Inspector connections** required
- **Must track all devices** in list
- **Adding new device requires Inspector update**

---

### Pattern 2: Tag-Based Targeting

**Scenario:** User presses "Lights Off" button (only lights, not climate/entertainment)

**MercuryMessaging:**
```csharp
// In ControlPanel.cs
public void OnLightsOffButton() {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.SetActive,
        false,
        new MmMetadataBlock(
            MmLevelFilter.Child,
            MmActiveFilter.All,
            MmSelectedFilter.All,
            MmNetworkFilter.Local,
            MmTag.Tag0 // Only lights
        )
    );
}
```
- **Tag filter automatically selects** only lights
- **No device-type checking** required
- **Works with any number of lights**

**Unity Events:**
```csharp
// In SmartHomeController.cs
[SerializeField] private List<SmartLightBehaviour> lights; // 8 references

public void OnLightsOffButton() {
    foreach (var light in lights) {
        light.TurnOff();
    }
}
```
- **Separate list for lights** (8 manual connections)
- **Need separate lists** for climate and entertainment
- **Type-based filtering** requires separate lists

---

### Pattern 3: Room-Level Control

**Scenario:** User selects "Bedroom" and presses "Room Off"

**MercuryMessaging:**
```csharp
// In ControlPanel.cs
private GameObject selectedRoom; // Set by UI button

public void OnRoomOffButton() {
    if (selectedRoom != null) {
        selectedRoom.GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.SetActive,
            false,
            new MmMetadataBlock(MmLevelFilter.Child)
        );
    }
}
```
- **Single message to room** reaches all devices in that room
- **Hierarchy naturally groups** devices by room
- **Adding device to room** is automatic (just parent it)

**Unity Events:**
```csharp
// In RoomController.cs (separate script needed)
[SerializeField] private List<ISmartDevice> devicesInRoom; // 2-4 references per room

public void TurnOffRoom() {
    foreach (var device in devicesInRoom) {
        device.TurnOff();
    }
}

// In SmartHomeController.cs
[SerializeField] private RoomController bedroomController;
[SerializeField] private RoomController kitchenController;
[SerializeField] private RoomController livingRoomController;

public void OnRoomOffButton() {
    selectedRoomController.TurnOffRoom(); // Need to track which room is selected
}
```
- **Additional RoomController script** required
- **Each room needs its own device list**
- **Adding device requires Inspector update**

---

### Pattern 4: State-Based Control (FSM)

**Scenario:** User selects "Sleep Mode" - lights dim, thermostats to night mode, music off

**MercuryMessaging:**
```csharp
// In ControlPanel.cs
public void OnSleepModeButton() {
    GetComponent<MmRelaySwitchNode>().RespondersFSM.JumpTo("Sleep");

    // Broadcast sleep mode message
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.Switch,
        2, // Sleep mode index
        new MmMetadataBlock(MmLevelFilter.Child)
    );
}

// In SmartLight.cs
protected override void ReceivedSwitch(int modeIndex) {
    if (modeIndex == 2) { // Sleep mode
        SetBrightness(0.1f); // Dim to 10%
    } else if (modeIndex == 0) { // Home mode
        SetBrightness(1.0f); // Full brightness
    }
}

// In MusicPlayer.cs
protected override void ReceivedSwitch(int modeIndex) {
    if (modeIndex == 2) { // Sleep mode
        Stop();
    }
}
```
- **FSM built-in** (MmRelaySwitchNode)
- **Single broadcast** reaches all devices
- **Each device handles** mode change appropriately

**Unity Events:**
```csharp
// In SmartHomeController.cs
public enum HomeMode { Home, Away, Sleep }
private HomeMode currentMode = HomeMode.Home;

[SerializeField] private List<SmartLightBehaviour> lights;
[SerializeField] private List<ThermostatBehaviour> thermostats;
[SerializeField] private MusicPlayerBehaviour musicPlayer;

public void OnSleepModeButton() {
    currentMode = HomeMode.Sleep;

    // Manually call each device type
    foreach (var light in lights) {
        light.SetBrightness(0.1f);
    }
    foreach (var thermostat in thermostats) {
        thermostat.SetNightMode(true);
    }
    musicPlayer.Stop();
}
```
- **Custom state enum** required
- **Manual iteration** over each device type
- **Adding new device type** requires controller update
- **25+ lines of code** for mode switching

---

### Pattern 5: Parent Notification (Status Updates)

**Scenario:** Thermostat reaches target temperature and reports back to control panel

**MercuryMessaging:**
```csharp
// In Thermostat.cs
private void OnTargetTemperatureReached() {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.MessageString,
        $"{gameObject.name}: Target temperature reached",
        new MmMetadataBlock(MmLevelFilter.Parent)
    );
}

// In ControlPanel.cs
protected override void ReceivedMessage(MmMessageString message) {
    UpdateStatusDisplay(message.value);
}
```
- **Parent notification** via LevelFilter.Parent
- **No reference to control panel** needed
- **Automatic routing** up hierarchy

**Unity Events:**
```csharp
// In Thermostat.cs
[System.Serializable]
public class StatusEvent : UnityEvent<string> { }
public StatusEvent OnStatusUpdate;

private void OnTargetTemperatureReached() {
    OnStatusUpdate?.Invoke($"{gameObject.name}: Target temperature reached");
}

// In SmartHomeController.cs (wired in Inspector)
public void HandleThermostatStatus(string message) {
    controlPanel.UpdateStatusDisplay(message);
}
```
- **UnityEvent on each device** (12 events)
- **Manual Inspector wiring** for each device (12 connections)
- **Controller acts as intermediary** (2-hop communication)

---

## MercuryMessaging Implementation Details

### Required Scripts

#### 1. SmartHomeHub.cs (Root Controller)
```csharp
using UnityEngine;
using MercuryMessaging;

public class SmartHomeHub : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start() {
        switchNode = GetComponent<MmRelaySwitchNode>();

        // Initialize FSM states
        switchNode.RespondersFSM.JumpTo("Home");
    }

    public void SetMode(string modeName) {
        switchNode.RespondersFSM.JumpTo(modeName);

        // Broadcast mode change to all devices
        int modeIndex = modeName == "Home" ? 0 : modeName == "Away" ? 1 : 2;
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.Switch,
            modeIndex,
            new MmMetadataBlock(MmLevelFilter.Child)
        );
    }
}
```

**Components Required:**
- MmRelaySwitchNode (for FSM)
- MmRelayNode (for message routing)

---

#### 2. ControlPanel.cs (UI Controller)
```csharp
using UnityEngine;
using MercuryMessaging;

public class ControlPanel : MmBaseResponder
{
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    private GameObject selectedRoom;

    // Button: All Off
    public void OnAllOffButton() {
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.SetActive,
            false,
            new MmMetadataBlock(MmLevelFilter.Parent) // Up to hub, then down to all
        );
    }

    // Button: Lights Off
    public void OnLightsOffButton() {
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.SetActive,
            false,
            new MmMetadataBlock(
                MmLevelFilter.Parent,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local,
                MmTag.Tag0 // Lights only
            )
        );
    }

    // Button: Room Off
    public void OnRoomOffButton() {
        if (selectedRoom != null) {
            selectedRoom.GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.SetActive,
                false,
                new MmMetadataBlock(MmLevelFilter.Child)
            );
        }
    }

    // Receive status updates from devices
    protected override void ReceivedMessage(MmMessageString message) {
        statusText.text = message.value;
    }

    // Room selection
    public void SelectRoom(GameObject room) {
        selectedRoom = room;
    }
}
```

**Components Required:**
- MmBaseResponder (for receiving status messages)
- MmRelayNode (for sending commands)

---

#### 3. SmartLight.cs (Device Responder)
```csharp
using UnityEngine;
using MercuryMessaging;

public class SmartLight : MmBaseResponder
{
    [SerializeField] private Light lightComponent;
    [SerializeField] private Renderer bulbRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private bool isOn = true;
    private float brightness = 1.0f;

    protected override void Awake() {
        base.Awake();

        // Set tag for filtering
        Tag = MmTag.Tag0; // Lights
        TagCheckEnabled = true;
    }

    protected override void ReceivedSetActive(bool active) {
        isOn = active;
        UpdateLight();

        // Report status to control panel
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageString,
            $"{gameObject.name}: {(active ? "ON" : "OFF")}",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }

    protected override void ReceivedSwitch(int modeIndex) {
        // 0 = Home, 1 = Away, 2 = Sleep
        if (modeIndex == 0) {
            brightness = 1.0f; // Full brightness
            isOn = true;
        } else if (modeIndex == 1) {
            isOn = false; // Off when away
        } else if (modeIndex == 2) {
            brightness = 0.1f; // Dim for sleep
            isOn = true;
        }
        UpdateLight();
    }

    private void UpdateLight() {
        lightComponent.enabled = isOn;
        lightComponent.intensity = isOn ? brightness : 0;
        bulbRenderer.material = isOn ? onMaterial : offMaterial;
    }
}
```

**Components Required:**
- MmBaseResponder (for receiving commands)
- MmRelayNode (for sending status updates)
- Light (Unity component)
- Renderer (for visual feedback)

**Tag:** `MmTag.Tag0` (Lights)

---

#### 4. Thermostat.cs (Climate Device)
```csharp
using UnityEngine;
using MercuryMessaging;

public class Thermostat : MmBaseResponder
{
    [SerializeField] private float currentTemp = 20f;
    [SerializeField] private float targetTemp = 22f;
    [SerializeField] private float heatingRate = 0.5f;

    private bool isHeating = true;
    private bool isNightMode = false;

    protected override void Awake() {
        base.Awake();

        // Set tag for filtering
        Tag = MmTag.Tag1; // Climate
        TagCheckEnabled = true;
    }

    void Update() {
        if (isHeating && currentTemp < targetTemp) {
            currentTemp += heatingRate * Time.deltaTime;

            if (currentTemp >= targetTemp) {
                currentTemp = targetTemp;
                OnTargetReached();
            }
        }
    }

    protected override void ReceivedSwitch(int modeIndex) {
        // 0 = Home, 1 = Away, 2 = Sleep
        if (modeIndex == 0) {
            targetTemp = 22f;
            isNightMode = false;
        } else if (modeIndex == 1) {
            targetTemp = 18f; // Energy saving
        } else if (modeIndex == 2) {
            targetTemp = 19f; // Night mode
            isNightMode = true;
        }
        isHeating = true;
    }

    private void OnTargetReached() {
        // Report to control panel
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageString,
            $"{gameObject.name}: Target temperature reached ({targetTemp}°C)",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }
}
```

**Components Required:**
- MmBaseResponder
- MmRelayNode

**Tag:** `MmTag.Tag1` (Climate)

---

#### 5. MusicPlayer.cs (Entertainment Device)
```csharp
using UnityEngine;
using MercuryMessaging;

public class MusicPlayer : MmBaseResponder
{
    [SerializeField] private AudioSource audioSource;
    private bool isPlaying = false;

    protected override void Awake() {
        base.Awake();

        // Set tag for filtering
        Tag = MmTag.Tag2; // Entertainment
        TagCheckEnabled = true;
    }

    protected override void ReceivedSetActive(bool active) {
        if (active) {
            Play();
        } else {
            Stop();
        }
    }

    protected override void ReceivedSwitch(int modeIndex) {
        // 0 = Home, 1 = Away, 2 = Sleep
        if (modeIndex == 2) { // Sleep mode
            Stop();
        }
    }

    private void Play() {
        if (!isPlaying) {
            audioSource.Play();
            isPlaying = true;

            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: Music playing",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }
    }

    private void Stop() {
        if (isPlaying) {
            audioSource.Stop();
            isPlaying = false;

            GetComponent<MmRelayNode>().MmInvoke(
                MmMethod.MessageString,
                $"{gameObject.name}: Music stopped",
                new MmMetadataBlock(MmLevelFilter.Parent)
            );
        }
    }
}
```

**Components Required:**
- MmBaseResponder
- MmRelayNode
- AudioSource

**Tag:** `MmTag.Tag2` (Entertainment)

---

### Total Code (MercuryMessaging)

**Estimated Lines of Code:**
- SmartHomeHub.cs: ~30 lines
- ControlPanel.cs: ~60 lines
- SmartLight.cs: ~70 lines
- Thermostat.cs: ~70 lines
- MusicPlayer.cs: ~60 lines
- SmartBlinds.cs: ~50 lines (similar to Thermostat, not shown)

**Total: ~340 lines** (actual implementation)

**Inspector Connections:** 0 (all communication via messages)

---

## Unity Events Implementation Details

### Required Scripts

#### 1. SmartHomeController.cs (Central Controller)
```csharp
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SmartHomeController : MonoBehaviour
{
    public enum HomeMode { Home, Away, Sleep }
    private HomeMode currentMode = HomeMode.Home;

    // All devices (must be wired in Inspector)
    [SerializeField] private List<SmartLightBehaviour> lights = new List<SmartLightBehaviour>();
    [SerializeField] private List<ThermostatBehaviour> thermostats = new List<ThermostatBehaviour>();
    [SerializeField] private List<SmartBlindsBehaviour> blinds = new List<SmartBlindsBehaviour>();
    [SerializeField] private MusicPlayerBehaviour musicPlayer;
    [SerializeField] private ControlPanelUI controlPanel;

    // Room groupings (optional, but helpful)
    [System.Serializable]
    public class RoomDevices {
        public string roomName;
        public List<ISmartDevice> devices = new List<ISmartDevice>();
    }
    [SerializeField] private List<RoomDevices> rooms = new List<RoomDevices>();
    private int selectedRoomIndex = 0;

    void Start() {
        SetMode(HomeMode.Home);

        // Wire up device status callbacks (must be done in code or Inspector)
        foreach (var light in lights) {
            light.OnStatusUpdate.AddListener(HandleDeviceStatus);
        }
        foreach (var thermostat in thermostats) {
            thermostat.OnStatusUpdate.AddListener(HandleDeviceStatus);
        }
        // ... repeat for all device types
    }

    // Button: All Off
    public void OnAllOffButton() {
        foreach (var light in lights) {
            light.TurnOff();
        }
        foreach (var thermostat in thermostats) {
            thermostat.TurnOff();
        }
        foreach (var blind in blinds) {
            blind.Close();
        }
        if (musicPlayer != null) {
            musicPlayer.Stop();
        }
    }

    // Button: Lights Off
    public void OnLightsOffButton() {
        foreach (var light in lights) {
            light.TurnOff();
        }
    }

    // Button: Room Off
    public void OnRoomOffButton() {
        if (selectedRoomIndex >= 0 && selectedRoomIndex < rooms.Count) {
            foreach (var device in rooms[selectedRoomIndex].devices) {
                device.TurnOff();
            }
        }
    }

    // Mode switching
    public void OnHomeModeButton() {
        SetMode(HomeMode.Home);
    }

    public void OnAwayModeButton() {
        SetMode(HomeMode.Away);
    }

    public void OnSleepModeButton() {
        SetMode(HomeMode.Sleep);
    }

    private void SetMode(HomeMode mode) {
        currentMode = mode;

        switch (mode) {
            case HomeMode.Home:
                foreach (var light in lights) {
                    light.SetBrightness(1.0f);
                    light.TurnOn();
                }
                foreach (var thermostat in thermostats) {
                    thermostat.SetTargetTemperature(22f);
                    thermostat.SetNightMode(false);
                }
                break;

            case HomeMode.Away:
                foreach (var light in lights) {
                    light.TurnOff();
                }
                foreach (var thermostat in thermostats) {
                    thermostat.SetTargetTemperature(18f);
                }
                foreach (var blind in blinds) {
                    blind.Close();
                }
                break;

            case HomeMode.Sleep:
                foreach (var light in lights) {
                    light.SetBrightness(0.1f);
                    light.TurnOn();
                }
                foreach (var thermostat in thermostats) {
                    thermostat.SetTargetTemperature(19f);
                    thermostat.SetNightMode(true);
                }
                if (musicPlayer != null) {
                    musicPlayer.Stop();
                }
                break;
        }
    }

    public void HandleDeviceStatus(string message) {
        if (controlPanel != null) {
            controlPanel.UpdateStatusDisplay(message);
        }
    }

    public void SelectRoom(int roomIndex) {
        selectedRoomIndex = roomIndex;
    }
}
```

**Estimated Lines: ~150 lines** (just the controller!)

---

#### 2. SmartLightBehaviour.cs (Device Script)
```csharp
using UnityEngine;
using UnityEngine.Events;

public interface ISmartDevice {
    void TurnOn();
    void TurnOff();
}

public class SmartLightBehaviour : MonoBehaviour, ISmartDevice
{
    [SerializeField] private Light lightComponent;
    [SerializeField] private Renderer bulbRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    [System.Serializable]
    public class StatusEvent : UnityEvent<string> { }
    public StatusEvent OnStatusUpdate;

    private bool isOn = true;
    private float brightness = 1.0f;

    public void TurnOn() {
        isOn = true;
        UpdateLight();
        OnStatusUpdate?.Invoke($"{gameObject.name}: ON");
    }

    public void TurnOff() {
        isOn = false;
        UpdateLight();
        OnStatusUpdate?.Invoke($"{gameObject.name}: OFF");
    }

    public void SetBrightness(float value) {
        brightness = Mathf.Clamp01(value);
        UpdateLight();
    }

    private void UpdateLight() {
        lightComponent.enabled = isOn;
        lightComponent.intensity = isOn ? brightness : 0;
        bulbRenderer.material = isOn ? onMaterial : offMaterial;
    }
}
```

**Estimated Lines: ~50 lines per device type**

---

### Total Code (Unity Events)

**Estimated Lines of Code:**
- SmartHomeController.cs: ~150 lines
- ISmartDevice.cs: ~10 lines (interface)
- SmartLightBehaviour.cs: ~50 lines
- ThermostatBehaviour.cs: ~70 lines
- SmartBlindsBehaviour.cs: ~50 lines
- MusicPlayerBehaviour.cs: ~60 lines
- ControlPanelUI.cs: ~40 lines

**Total: ~430 lines** (actual implementation)

**Inspector Connections Required:**
- Controller → 8 lights: 8 connections
- Controller → 2 thermostats: 2 connections
- Controller → 2 blinds: 2 connections
- Controller → 1 music player: 1 connection
- Controller → ControlPanel: 1 connection
- Each device → Controller (status updates): 13 UnityEvent connections
- Room groupings (optional): 13 additional connections

**Total: ~40 manual Inspector connections**

---

## User Study Tasks

### Task 1: Add a New Light (SIMPLE)
**Estimated Time:** 5-8 minutes

**Instructions:**
"Add a new smart light to the Kitchen. It should respond to:
- 'All Off' button
- 'Lights Off' button
- 'Room Off' button (when Kitchen is selected)
- Mode switching (Home/Away/Sleep)"

**MercuryMessaging Solution:**
1. Duplicate existing SmartLight GameObject
2. Parent it to Room_Kitchen
3. Ensure SmartLight script has `Tag = MmTag.Tag0`
4. Done! (Automatic registration)

**Expected Code Changes:** 0 lines
**Inspector Changes:** 2 (duplicate, parent)

**Unity Events Solution:**
1. Duplicate existing SmartLight GameObject
2. Parent it to Room_Kitchen
3. Add it to SmartHomeController.lights list (Inspector)
4. Add it to Room_Kitchen.devices list (Inspector)
5. Wire OnStatusUpdate event to controller (Inspector)

**Expected Code Changes:** 0 lines
**Inspector Changes:** 5 (duplicate, parent, 3 list additions)

**Success Criteria:**
- ✅ New light appears in scene
- ✅ Responds to "All Off" command
- ✅ Responds to "Lights Off" command
- ✅ Responds to "Room Off" when Kitchen selected
- ✅ Responds to mode changes
- ✅ Status updates appear in control panel

---

### Task 2: Implement "Climate Control" Button (MEDIUM)
**Estimated Time:** 8-12 minutes

**Instructions:**
"Add a 'Climate Control' button to the control panel that:
- Turns off all thermostats and blinds
- Does NOT affect lights or music
- Displays status message when complete"

**MercuryMessaging Solution:**
```csharp
// In ControlPanel.cs
public void OnClimateControlOffButton() {
    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.SetActive,
        false,
        new MmMetadataBlock(
            MmLevelFilter.Parent,
            MmActiveFilter.All,
            MmSelectedFilter.All,
            MmNetworkFilter.Local,
            MmTag.Tag1 // Climate devices only
        )
    );
}
```

**Expected Code Changes:** ~10 lines
**Inspector Changes:** 1 (wire button to method)

**Unity Events Solution:**
```csharp
// In SmartHomeController.cs
public void OnClimateControlOffButton() {
    foreach (var thermostat in thermostats) {
        thermostat.TurnOff();
    }
    foreach (var blind in blinds) {
        blind.Close();
    }
    controlPanel.UpdateStatusDisplay("Climate control off");
}
```

**Expected Code Changes:** ~10 lines
**Inspector Changes:** 1 (wire button to method)

**Success Criteria:**
- ✅ Button appears in UI
- ✅ All thermostats turn off
- ✅ All blinds close
- ✅ Lights and music unaffected
- ✅ Status message displays

---

### Task 3: Add a New Room (COMPLEX)
**Estimated Time:** 10-15 minutes

**Instructions:**
"Add a new 'Bathroom' room with:
- 2 smart lights
- 1 thermostat
- Must respond to all control panel commands
- Must support room-specific control"

**MercuryMessaging Solution:**
1. Create Room_Bathroom GameObject with MmRelayNode
2. Parent to SmartHomeHub
3. Create 2 SmartLight GameObjects (Tag0), parent to Room_Bathroom
4. Create 1 Thermostat GameObject (Tag1), parent to Room_Bathroom
5. Add Room_Bathroom to UI room selection dropdown

**Expected Code Changes:** ~5 lines (UI dropdown)
**Inspector Changes:** 7 (create objects, parent them)

**Unity Events Solution:**
1. Create Room_Bathroom GameObject
2. Create 2 SmartLight GameObjects, parent to Room_Bathroom
3. Create 1 Thermostat GameObject, parent to Room_Bathroom
4. Add 2 lights to SmartHomeController.lights list
5. Add 1 thermostat to SmartHomeController.thermostats list
6. Create new RoomDevices entry for Bathroom
7. Add 3 devices to Bathroom room list
8. Wire 3 OnStatusUpdate events to controller
9. Add Room_Bathroom to UI room selection dropdown

**Expected Code Changes:** ~5 lines (UI dropdown)
**Inspector Changes:** 15+ (create objects, parent, 6 list additions, 3 event wirings)

**Success Criteria:**
- ✅ Bathroom room appears in hierarchy
- ✅ All devices respond to global commands (All Off, Lights Off, etc.)
- ✅ Bathroom room selectable in UI
- ✅ "Room Off" button works for Bathroom
- ✅ Mode switching affects Bathroom devices
- ✅ Status updates from Bathroom devices

---

### Task 4: Implement "Party Mode" (COMPLEX)
**Estimated Time:** 12-18 minutes

**Instructions:**
"Add a new 'Party Mode' that:
- All lights flash colors (R→G→B→R cycle)
- Music player turns on automatically
- Thermostats set to 20°C
- Blinds open
Add a 'Party Mode' button to the control panel"

**MercuryMessaging Solution:**
```csharp
// In SmartHomeHub.cs - Add Party state to FSM
public void SetPartyMode() {
    switchNode.RespondersFSM.JumpTo("Party");

    GetComponent<MmRelayNode>().MmInvoke(
        MmMethod.Switch,
        3, // Party mode index
        new MmMetadataBlock(MmLevelFilter.Child)
    );
}

// In SmartLight.cs - Add party mode handling
protected override void ReceivedSwitch(int modeIndex) {
    if (modeIndex == 3) { // Party mode
        StartCoroutine(ColorFlashRoutine());
    }
    // ... existing modes
}

private IEnumerator ColorFlashRoutine() {
    while (true) {
        lightComponent.color = Color.red;
        yield return new WaitForSeconds(1f);
        lightComponent.color = Color.green;
        yield return new WaitForSeconds(1f);
        lightComponent.color = Color.blue;
        yield return new WaitForSeconds(1f);
    }
}

// Similar updates for Thermostat, MusicPlayer, SmartBlinds
```

**Expected Code Changes:** ~40 lines (add mode handling to 4 device types)
**Inspector Changes:** 1 (wire button)

**Unity Events Solution:**
```csharp
// In SmartHomeController.cs
public enum HomeMode { Home, Away, Sleep, Party } // Update enum

public void OnPartyModeButton() {
    SetMode(HomeMode.Party);
}

private void SetMode(HomeMode mode) {
    currentMode = mode;

    switch (mode) {
        // ... existing modes

        case HomeMode.Party:
            foreach (var light in lights) {
                light.StartColorFlash(); // New method needed
            }
            foreach (var thermostat in thermostats) {
                thermostat.SetTargetTemperature(20f);
            }
            foreach (var blind in blinds) {
                blind.Open(); // New method needed
            }
            if (musicPlayer != null) {
                musicPlayer.Play();
            }
            break;
    }
}

// In SmartLightBehaviour.cs
private Coroutine flashCoroutine;

public void StartColorFlash() {
    if (flashCoroutine != null) {
        StopCoroutine(flashCoroutine);
    }
    flashCoroutine = StartCoroutine(ColorFlashRoutine());
}

public void StopColorFlash() {
    if (flashCoroutine != null) {
        StopCoroutine(flashCoroutine);
        flashCoroutine = null;
    }
}

private IEnumerator ColorFlashRoutine() {
    while (true) {
        lightComponent.color = Color.red;
        yield return new WaitForSeconds(1f);
        lightComponent.color = Color.green;
        yield return new WaitForSeconds(1f);
        lightComponent.color = Color.blue;
        yield return new WaitForSeconds(1f);
    }
}

// Similar updates for SmartBlindsBehaviour
```

**Expected Code Changes:** ~80 lines (controller update + new methods in 2 device types)
**Inspector Changes:** 1 (wire button)

**Success Criteria:**
- ✅ Party mode button appears
- ✅ All lights flash colors when activated
- ✅ Music starts playing
- ✅ Thermostats set to 20°C
- ✅ Blinds open
- ✅ Can switch back to Home mode

---

### Task 5: Debug Task (DIAGNOSTIC)
**Estimated Time:** 5-10 minutes

**Instructions:**
"The bedroom thermostat is not responding to the 'Climate Control Off' button, but it DOES respond to 'All Off'. Debug and fix the issue."

**Planted Bug (MercuryMessaging):**
- Bedroom thermostat has wrong tag: `Tag = MmTag.Tag0` (Light) instead of `Tag = MmTag.Tag1` (Climate)

**Solution:**
1. Check ControlPanel.OnClimateControlOffButton() - uses MmTag.Tag1
2. Check Thermostat_Bedroom inspector - Tag is wrong (Tag0)
3. Change to Tag1
4. Test

**Time: ~5 minutes** (clear tag system makes debugging obvious)

**Planted Bug (Unity Events):**
- Bedroom thermostat not in SmartHomeController.thermostats list

**Solution:**
1. Check SmartHomeController.OnClimateControlOffButton() - iterates thermostats list
2. Check SmartHomeController inspector - Bedroom thermostat missing from list
3. Add Bedroom thermostat to list
4. Test

**Time: ~8 minutes** (need to check Inspector list, less obvious)

**Success Criteria:**
- ✅ Identify the bug
- ✅ Fix the bug
- ✅ Verify thermostat responds to "Climate Control Off"
- ✅ Verify thermostat still responds to "All Off"

---

## Metrics to Collect

### Per Implementation (Mercury vs Events)

**Development Time:**
- Setup time (creating hierarchy, adding components)
- Coding time (writing scripts)
- Inspector wiring time (connecting references/events)
- Debug time (fixing issues)
- **Total time**

**Lines of Code:**
- Controller script (SmartHomeHub vs SmartHomeController)
- Device scripts (SmartLight, Thermostat, etc.)
- UI script (ControlPanel vs ControlPanelUI)
- Total LOC (excluding Unity-generated)

**Inspector Connections:**
- Serialized field references
- UnityEvent connections
- List additions
- **Total connections**

**Code Coupling:**
- Direct references (GetComponent, serialized fields)
- UnityEvent.AddListener calls
- Afferent coupling (Ca)
- Efferent coupling (Ce)

**Task Performance:**
- Time to complete each task
- Number of errors/bugs encountered
- Number of Inspector changes required
- Number of code changes required

### Qualitative Metrics

**NASA-TLX Workload (1-7 scale):**
- Mental demand: "How mentally demanding was the task?"
- Physical demand: "How physically demanding was the task?"
- Temporal demand: "How hurried or rushed was the pace?"
- Performance: "How successful were you in accomplishing the task?"
- Effort: "How hard did you have to work to accomplish your level of performance?"
- Frustration: "How insecure, discouraged, irritated, stressed, and annoyed were you?"

**Subjective Preference (5-point Likert):**
- "Which approach did you prefer overall?" (1=Strongly prefer Events, 5=Strongly prefer Mercury)
- "Which approach felt more maintainable?" (1=Much harder to maintain, 5=Much easier to maintain)
- "Which approach would you use for a real project?" (1=Definitely Events, 5=Definitely Mercury)

**Open-Ended Questions:**
- "What did you like about MercuryMessaging?"
- "What did you dislike about MercuryMessaging?"
- "What did you like about Unity Events?"
- "What did you dislike about Unity Events?"
- "Which specific features were most helpful/frustrating?"

---

## Expected Results

### Hypothesis: MercuryMessaging Advantages

**Lines of Code:**
- Mercury: ~340 lines
- Events: ~430 lines
- **Reduction: 21% fewer lines**

**Inspector Connections:**
- Mercury: 0 manual connections
- Events: ~40 manual connections
- **Reduction: 100% fewer connections**

**Task 1 (Add Light):**
- Mercury: 2 Inspector changes, 0 code changes (~5 min)
- Events: 5 Inspector changes, 0 code changes (~8 min)
- **37% faster**

**Task 3 (Add Room):**
- Mercury: 7 Inspector changes, ~5 LOC (~10 min)
- Events: 15+ Inspector changes, ~5 LOC (~15 min)
- **33% faster**

**Task 5 (Debug):**
- Mercury: Clear tag system makes bug obvious (~5 min)
- Events: Must check Inspector lists (~8 min)
- **37% faster**

### Hypothesis: Unity Events Advantages

**Discoverability:**
- Events: Inspector shows all connections visually
- Mercury: Message flow not visible in Inspector
- **Events easier to understand initially**

**Learning Curve:**
- Events: Familiar to all Unity developers
- Mercury: Requires learning new framework
- **Events faster for beginners**

---

## Implementation Notes

### Setup Time Estimate

**MercuryMessaging:**
- Create hierarchy: 5 minutes
- Add MmRelayNode components: 3 minutes
- Write scripts: 30 minutes
- Test and debug: 10 minutes
- **Total: ~48 minutes**

**Unity Events:**
- Create hierarchy: 5 minutes
- Write scripts: 45 minutes
- Wire Inspector connections: 15 minutes
- Test and debug: 10 minutes
- **Total: ~75 minutes**

### Participant Requirements

**Minimum Experience:**
- 6+ months Unity experience
- Familiar with Unity Events
- Comfortable with C# scripting
- Understand GameObject hierarchy

**Not Required:**
- Prior MercuryMessaging experience (will be taught)
- Advanced programming knowledge
- Game development background

### Pilot Testing Recommendations

1. **Run 2 pilot sessions** before full study
2. **Time each task** to validate estimates
3. **Check for ambiguous instructions**
4. **Verify planted bugs are appropriately difficult**
5. **Test counterbalancing** (Mercury first vs Events first)

---

## Conclusion

This scene provides the **simplest and clearest comparison** between MercuryMessaging and Unity Events. The smart home domain is familiar, the hierarchy is intuitive, and the Mercury advantages (tags, FSM, loose coupling) are immediately apparent.

**Recommended for:**
- ✅ First-time MercuryMessaging users
- ✅ Quick demonstrations (30-45 min)
- ✅ User study pilot testing
- ✅ Teaching hierarchical message routing concepts

**Scales well for:**
- Adding more devices
- Adding more rooms
- Adding more modes
- Adding more complex logic

---

*Planning document created: 2025-11-21*
*Scene complexity: Simple*
*Priority: ⭐⭐⭐⭐⭐ (HIGHEST)*
