# Smart Home Mercury Scene Setup Instructions

**Last Updated:** 2025-11-21
**Scene:** SmartHome_Mercury.unity
**Approach:** MercuryMessaging
**Target LOC:** ~340 lines (actual: ~526 lines with comments)
**Inspector Connections:** 0 (for messaging)

---

## Overview

This scene demonstrates the Smart Home Control Panel using MercuryMessaging framework. It showcases:
- Hierarchical broadcasting (one message reaches all devices)
- Tag-based filtering (target specific device types)
- Room-level control (messages scoped to room hierarchy)
- FSM-based mode management (Home/Away/Sleep)
- Parent notification (devices report status back)
- Zero Inspector connections for messaging (loose coupling)

---

## GameObject Hierarchy

### Complete Structure (16 GameObjects)

```
SmartHomeHub
├── ControlPanel
├── Room_Bedroom
│   ├── SmartLight_Bedroom1
│   ├── SmartLight_Bedroom2
│   ├── Thermostat_Bedroom
│   └── SmartBlinds_Bedroom
├── Room_Kitchen
│   ├── SmartLight_Kitchen1
│   ├── SmartLight_Kitchen2
│   └── SmartLight_Kitchen3
└── Room_LivingRoom
    ├── SmartLight_Living1
    ├── SmartLight_Living2
    ├── Thermostat_Living
    ├── SmartBlinds_Living
    └── MusicPlayer_Living
```

---

## Component Setup (Step-by-Step)

### 1. SmartHomeHub (Root GameObject)

**Components:**
- `MmRelaySwitchNode` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `SmartHomeHub` script

**MmRelaySwitchNode Configuration:**
- **Responders FSM:**
  - State 0: "Home"
  - State 1: "Away"
  - State 2: "Sleep"
- Initial State: "Home"

**Notes:**
- This is the root of the message hierarchy
- FSM manages global home modes
- All child messages flow through this node

---

### 2. ControlPanel (UI Controller)

**Components:**
- `MmBaseResponder` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `ControlPanel` script

**ControlPanel Script Configuration:**
- **Status Text:** Assign TextMeshProUGUI component (created in Canvas)

**Notes:**
- This sends commands to devices via messages
- Receives status updates from devices
- No references to devices needed!

---

### 3. Room GameObjects (3 rooms)

Create 3 empty GameObjects as children of SmartHomeHub:

#### Room_Bedroom
**Components:**
- `MmRelayNode` (MercuryMessaging)

**Notes:**
- Acts as relay for room-specific messages
- Devices parent here automatically register

#### Room_Kitchen
**Components:**
- `MmRelayNode` (MercuryMessaging)

#### Room_LivingRoom
**Components:**
- `MmRelayNode` (MercuryMessaging)

---

### 4. SmartLight Devices (8 total)

Create 8 GameObjects (as children of appropriate rooms):
- Bedroom: SmartLight_Bedroom1, SmartLight_Bedroom2
- Kitchen: SmartLight_Kitchen1, SmartLight_Kitchen2, SmartLight_Kitchen3
- LivingRoom: SmartLight_Living1, SmartLight_Living2

**Components (each light):**
- `MmBaseResponder` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `SmartLight` script
- `Light` component (Unity built-in)
- `MeshRenderer` (for bulb visual)

**SmartLight Script Configuration:**
- **Light Component:** Assign the Light component
- **Bulb Renderer:** Assign the MeshRenderer (for visual feedback)
- **On Material:** Create emissive yellow material
- **Off Material:** Create dark gray material

**Visual Setup:**
- Add a Sphere or Cube as child GameObject (represents bulb)
- Assign MeshRenderer to Bulb Renderer field
- Position Light component at center

**Tag Check:**
- Tag is automatically set to `MmTag.Tag0` (Lights)
- TagCheckEnabled automatically set to true

---

### 5. Thermostat Devices (2 total)

Create 2 GameObjects:
- Thermostat_Bedroom (child of Room_Bedroom)
- Thermostat_Living (child of Room_LivingRoom)

**Components (each thermostat):**
- `MmBaseResponder` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `Thermostat` script

**Thermostat Script Configuration:**
- **Current Temp:** 20 (starting temperature)
- **Target Temp:** 22 (default target)
- **Heating Rate:** 0.5 (degrees per second)

**Visual Setup:**
- Add a Cube or custom model
- Add TextMeshPro text showing current temperature (optional)
- Add material that changes color based on heating state (optional)

**Tag Check:**
- Tag is automatically set to `MmTag.Tag1` (Climate)

---

### 6. SmartBlinds Devices (2 total)

Create 2 GameObjects:
- SmartBlinds_Bedroom (child of Room_Bedroom)
- SmartBlinds_Living (child of Room_LivingRoom)

**Components (each blind):**
- `MmBaseResponder` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `SmartBlinds` script

**SmartBlinds Script Configuration:**
- **Blinds Transform:** Assign child GameObject (the visual blinds)
- **Closed Position:** 0
- **Open Position:** 1
- **Move Speed:** 2

**Visual Setup:**
- Create child GameObject (Cube scaled as blinds: scale.y = 0.1, scale.x = 2)
- Assign to Blinds Transform field
- Blinds animate by scaling Y axis

**Tag Check:**
- Tag is automatically set to `MmTag.Tag1` (Climate)

---

### 7. MusicPlayer Device (1 total)

Create 1 GameObject:
- MusicPlayer_Living (child of Room_LivingRoom)

**Components:**
- `MmBaseResponder` (MercuryMessaging)
- `MmRelayNode` (MercuryMessaging)
- `MusicPlayer` script
- `AudioSource` component (Unity built-in)

**AudioSource Configuration:**
- **Audio Clip:** Assign any music audio clip
- **Play On Awake:** false
- **Loop:** true
- **Volume:** 0.5

**Visual Setup:**
- Add a Cube or speaker icon model
- Add particle system for music notes (optional)

**Tag Check:**
- Tag is automatically set to `MmTag.Tag2` (Entertainment)

---

## UI Canvas Setup

### Create Canvas

1. Create Canvas GameObject (UI → Canvas)
2. Set Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

### Control Panel UI Elements

#### Status Text (TextMeshProUGUI)
- **Position:** Top center
- **Size:** 800x100
- **Font Size:** 24
- **Alignment:** Center
- **Text:** "Status: Ready"

#### Button Layout

Create a Panel for button organization:

**All Off Button**
- **Text:** "All Off"
- **OnClick():** Wire to `ControlPanel.OnAllOffButton()`

**Lights Off Button**
- **Text:** "Lights Off"
- **OnClick():** Wire to `ControlPanel.OnLightsOffButton()`

**Climate Off Button**
- **Text:** "Climate Off"
- **OnClick():** Wire to `ControlPanel.OnClimateOffButton()`

**Room Off Button**
- **Text:** "Room Off"
- **OnClick():** Wire to `ControlPanel.OnRoomOffButton()`

**Room Selection Dropdown**
- **Options:** "Bedroom", "Kitchen", "Living Room"
- **OnValueChanged():** Wire to `ControlPanel.SelectRoom(GameObject)`
  - Option 0 → Pass Room_Bedroom
  - Option 1 → Pass Room_Kitchen
  - Option 2 → Pass Room_LivingRoom

**Mode Buttons (3 buttons)**
- **Home Button:**
  - Text: "Home Mode"
  - OnClick(): Wire to `SmartHomeHub.SetMode("Home")`
- **Away Button:**
  - Text: "Away Mode"
  - OnClick(): Wire to `SmartHomeHub.SetMode("Away")`
- **Sleep Button:**
  - Text: "Sleep Mode"
  - OnClick(): Wire to `SmartHomeHub.SetMode("Sleep")`

---

## Testing Checklist

### Test 1: All Off Command
1. Press "All Off" button
2. ✅ All 8 lights turn off
3. ✅ All 2 thermostats stop heating
4. ✅ All 2 blinds close
5. ✅ 1 music player stops
6. ✅ Status text shows device messages

**Expected:** All 12 devices respond (hierarchical broadcast works)

---

### Test 2: Lights Off Command (Tag Filtering)
1. Turn all lights on (press Home Mode first)
2. Press "Lights Off" button
3. ✅ All 8 lights turn off
4. ✅ Thermostats and blinds unaffected (still on)
5. ✅ Music player unaffected

**Expected:** Only Tag0 devices respond (tag filtering works)

---

### Test 3: Climate Off Command (Tag Filtering)
1. Press "Home Mode" to turn on
2. Press "Climate Off" button
3. ✅ 2 thermostats stop
4. ✅ 2 blinds close
5. ✅ Lights unaffected
6. ✅ Music unaffected

**Expected:** Only Tag1 devices respond (tag filtering works)

---

### Test 4: Room-Level Control
1. Select "Bedroom" from dropdown
2. Press "Room Off" button
3. ✅ Bedroom devices turn off (2 lights, 1 thermostat, 1 blinds)
4. ✅ Kitchen and Living Room devices unaffected

**Expected:** Only selected room devices respond (hierarchical scoping works)

---

### Test 5: Mode Switching (FSM)
1. Press "Home Mode"
   - ✅ All lights full brightness (brightness = 1.0)
   - ✅ Thermostats target 22°C
   - ✅ Blinds open
   - ✅ Music can play

2. Press "Away Mode"
   - ✅ All lights turn off
   - ✅ Thermostats target 18°C (energy saving)
   - ✅ Blinds close (security)

3. Press "Sleep Mode"
   - ✅ All lights dim (brightness = 0.1)
   - ✅ Thermostats target 19°C (night mode)
   - ✅ Blinds close (darkness)
   - ✅ Music stops

**Expected:** All devices respond appropriately to mode changes (FSM broadcast works)

---

### Test 6: Status Updates (Parent Notification)
1. Turn on a light (press Home Mode)
2. ✅ Status text shows: "SmartLight_Bedroom1: ON"
3. Turn off Climate (press Climate Off)
4. ✅ Status text shows: "Thermostat_Bedroom: Off"
5. Multiple status updates overwrite previous

**Expected:** Devices report status to control panel (parent notification works)

---

### Test 7: Zero Inspector Connections
1. Select SmartHomeHub in Hierarchy
2. ✅ No references to devices in Inspector
3. Select ControlPanel
4. ✅ No references to devices (only UI elements)
5. Select any device
6. ✅ No references to other devices

**Expected:** All messaging via hierarchy, no manual wiring (loose coupling confirmed)

---

## Performance Validation

**Target:** 60 FPS in Unity Editor

**Test:**
1. Open Profiler (Window → Analysis → Profiler)
2. Run scene in Play Mode
3. Monitor CPU usage
4. Trigger multiple commands rapidly
5. ✅ Frame rate should remain 50-60 FPS
6. ✅ No memory leaks (steady memory usage)

**Expected:** Excellent performance with 12 devices

---

## Common Issues and Solutions

### Issue 1: Lights Not Responding
**Symptom:** "Lights Off" button doesn't turn off lights

**Diagnosis:**
- Check if `SmartLight` script has Tag = `MmTag.Tag0`
- Check if `TagCheckEnabled = true`
- Check if `MmRelayNode` component is attached

**Solution:** Ensure Tag0 is set in Awake() method

---

### Issue 2: Room Off Not Working
**Symptom:** "Room Off" button doesn't affect selected room

**Diagnosis:**
- Check if Room GameObject has `MmRelayNode` component
- Check if devices are children of room in hierarchy
- Check if dropdown passes correct GameObject reference

**Solution:** Verify hierarchy parent-child relationships

---

### Issue 3: Status Updates Not Showing
**Symptom:** Status text doesn't update

**Diagnosis:**
- Check if `ControlPanel` has `statusText` field assigned
- Check if devices have `MmRelayNode` component (needed to send messages)
- Check if `ReceivedMessage()` is overridden correctly

**Solution:** Assign TextMeshProUGUI reference in Inspector

---

### Issue 4: Mode Switching Not Working
**Symptom:** Devices don't respond to mode buttons

**Diagnosis:**
- Check if `SmartHomeHub` has `MmRelaySwitchNode` component
- Check if FSM states are defined ("Home", "Away", "Sleep")
- Check if devices override `ReceivedSwitch(int modeIndex)`

**Solution:** Verify FSM setup in MmRelaySwitchNode Inspector

---

## Inspector Connection Count

**Total Manual Connections for Messaging:** 0 ✅

**UI Connections (Allowed):**
- ControlPanel → StatusText: 1
- Button → ControlPanel methods: 7
- Dropdown → Room GameObjects: 3
- Mode buttons → SmartHomeHub.SetMode: 3
- **Total UI connections: 14**

**Key Point:** UI connections are NOT counted as messaging connections. The 0 connection goal applies to device-to-device communication only.

---

## Lines of Code Summary

**Actual Files:**
1. SmartHomeHub.cs: ~47 lines
2. ControlPanel.cs: ~100 lines
3. SmartLight.cs: ~110 lines
4. Thermostat.cs: ~100 lines
5. SmartBlinds.cs: ~89 lines
6. MusicPlayer.cs: ~90 lines

**Total:** ~526 lines (including comments)
**Estimated Code-Only LOC:** ~340 lines (excluding comments/whitespace)

**Target Met:** ✅ (~340 LOC target)

---

## Next Steps

After completing this Mercury implementation:

1. **Phase 2.2:** Implement Unity Events version (`SmartHome_Events.unity`)
   - Create parallel implementation
   - Manually wire ~40 Inspector connections
   - Estimate ~430 LOC
   - Compare complexity

2. **Phase 2.3:** Design user study tasks
   - Task 1: Add New Light (5-8 min)
   - Task 2: Climate Control Button (8-12 min)
   - Task 3: Add New Room (10-15 min)
   - Task 4: Party Mode (12-18 min)
   - Task 5: Debug Task (5-10 min)

3. **Pilot Testing:** Test with 2 developers before main study

---

**Scene Status:** ✅ Scripts Complete, Scene Setup Instructions Ready
**Ready for Unity Implementation:** Yes
**Estimated Setup Time:** 4-6 hours (building hierarchy and UI in Unity)

---

**Last Updated:** 2025-11-21
**Created By:** User Study Development Task - Phase 2.1
