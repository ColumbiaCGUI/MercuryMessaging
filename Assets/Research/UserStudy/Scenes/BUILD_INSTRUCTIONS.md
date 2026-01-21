# Smart Home Scene Construction Instructions

**Last Updated:** 2025-11-21

---

## Automated Scene Construction

An automated scene builder has been created to construct the Smart Home Mercury scene with one click!

### How to Build the Scene

1. **Open Unity Editor** (with this project loaded)

2. **Run the Scene Builder:**
   - Click menu: **UserStudy → Build Smart Home Mercury Scene**
   - Confirm the dialog

3. **Wait for completion** (~10 seconds)
   - The script will create:
     - 16 GameObjects with proper hierarchy
     - All components and scripts attached
     - 2 materials (LightOn, LightOff)
     - UI Canvas with buttons and status text
     - Scene saved at: `Assets/UserStudy/Scenes/SmartHome_Mercury.unity`

4. **Open the created scene:**
   - Navigate to `Assets/UserStudy/Scenes/SmartHome_Mercury.unity`
   - Double-click to open

---

## Manual Steps After Automated Build

The scene builder creates 99% of the scene automatically, but a few manual steps are required:

### Step 1: Configure FSM States

1. Select **SmartHomeHub** in Hierarchy
2. Find **MmRelaySwitchNode** component in Inspector
3. Expand **Responders FSM** section
4. Set up 3 states:
   - **State 0:** Name = "Home"
   - **State 1:** Name = "Away"
   - **State 2:** Name = "Sleep"
5. Set **Initial State** to "Home"

### Step 2: Wire Room Dropdown (Optional)

The dropdown needs manual wiring for room selection:

1. Select **Canvas → Panel → RoomDropdown** in Hierarchy
2. Find **TMP_Dropdown** component
3. Scroll to **On Value Changed** event
4. Add 3 listeners:
   - **Option 0 (Bedroom):**
     - Target: ControlPanel
     - Function: ControlPanel.SelectRoom(GameObject)
     - Parameter: Room_Bedroom GameObject
   - **Option 1 (Kitchen):**
     - Target: ControlPanel
     - Function: ControlPanel.SelectRoom(GameObject)
     - Parameter: Room_Kitchen GameObject
   - **Option 2 (Living Room):**
     - Target: ControlPanel
     - Function: ControlPanel.SelectRoom(GameObject)
     - Parameter: Room_LivingRoom GameObject

**Note:** Room Off functionality will work without this if you don't test room-specific control.

### Step 3: Add Audio Clip (Optional)

The MusicPlayer needs an audio clip:

1. Select **Room_LivingRoom → MusicPlayer_Living**
2. Find **Audio Source** component
3. Assign any audio clip to **Audio Clip** field
4. Recommended: Use a music loop (not sound effects)

**Note:** Music player will work without audio (just won't play sound).

---

## Testing the Scene

### Quick Test (3 minutes)

1. **Enter Play Mode** (press Play button)

2. **Test All Off:**
   - Click **"All Off"** button
   - ✅ All lights should turn off (8 lights dim)
   - ✅ Status text updates
   - ✅ Scene should darken

3. **Test Lights Off (Tag Filtering):**
   - Click **"Home Mode"** to turn lights back on
   - Click **"Lights Off"** button
   - ✅ Only lights turn off
   - ✅ Thermostats and blinds unaffected

4. **Test Mode Switching (FSM):**
   - Click **"Home Mode"**
     - ✅ Lights bright
   - Click **"Sleep Mode"**
     - ✅ Lights dim (10% brightness)
     - ✅ Blinds close
     - ✅ Music stops
   - Click **"Away Mode"**
     - ✅ All lights off
     - ✅ Blinds closed

5. **Watch Status Text:**
   - ✅ Should update with device messages
   - Example: "SmartLight_Bedroom1: ON"

### Full Test (10 minutes)

Follow the complete test checklist in `SmartHome_Mercury_Setup.md`:
- Test 1: All Off Command
- Test 2: Lights Off Command (Tag Filtering)
- Test 3: Climate Off Command
- Test 4: Room-Level Control
- Test 5: Mode Switching (FSM)
- Test 6: Status Updates
- Test 7: Zero Inspector Connections

---

## Troubleshooting

### Issue: "MmRelayNode not found" errors in console

**Solution:**
- Make sure MercuryMessaging framework is in the project
- Check that `Assets/MercuryMessaging/` folder exists
- Reimport MercuryMessaging if needed

### Issue: Buttons don't work

**Solution:**
- Check EventSystem exists in scene (should be created automatically)
- Verify Canvas has GraphicRaycaster component
- Check button OnClick events are wired correctly

### Issue: Lights don't turn off

**Solution:**
- Select a SmartLight in Hierarchy
- Verify SmartLight script is attached
- Check MmBaseResponder and MmRelayNode components present
- Verify Tag is set to Tag0 (check in script, not Inspector)

### Issue: FSM modes don't work

**Solution:**
- Select SmartHomeHub
- Verify MmRelaySwitchNode has 3 states defined
- Check state names are exact: "Home", "Away", "Sleep"
- Initial state should be "Home"

### Issue: Status text doesn't update

**Solution:**
- Select ControlPanel
- Check statusText field is assigned to TextMeshProUGUI
- Verify devices have MmRelayNode component (needed to send messages)

### Issue: Scene builder script not found

**Solution:**
- Check file exists: `Assets/UserStudy/Editor/SmartHomeSceneBuilder.cs`
- Wait for Unity to compile scripts
- Check Console for compilation errors
- Restart Unity Editor if needed

---

## Scene Statistics

After successful build:

**GameObjects:** 16 total
- 1 Hub (SmartHomeHub)
- 1 Control Panel
- 3 Rooms
- 8 Smart Lights
- 2 Thermostats
- 2 Smart Blinds
- 1 Music Player

**Components:**
- MmRelaySwitchNode: 1
- MmRelayNode: 16
- MmBaseResponder: 13
- Custom Scripts: 6 types

**Scripts (LOC):**
- Total: ~526 lines (with comments)
- Code only: ~340 lines

**Inspector Connections for Messaging:** 0 ✅
**UI Connections:** 14 (buttons, text field, dropdown)

**Materials:** 2
- LightOn.mat (emissive yellow)
- LightOff.mat (dark gray)

**Performance:** 60 FPS target (should achieve easily with 12 devices)

---

## Next Steps After Testing

Once the Mercury scene is working:

1. **Document Results:**
   - Screenshot hierarchy
   - Verify 0 Inspector connections for messaging
   - Count actual LOC
   - Note any issues encountered

2. **Phase 2.2: Build Unity Events Version:**
   - Create `SmartHome_Events.unity` scene
   - Implement with Unity Events approach
   - Wire ~40 Inspector connections
   - Compare complexity and LOC

3. **Phase 2.3: Design User Study Tasks:**
   - Create 5 tasks for participants
   - Implement task UI and instructions
   - Set up metrics collection
   - Pilot test with 2 developers

---

## Developer Notes

**What the Scene Builder Does:**
- ✅ Creates all 16 GameObjects with correct hierarchy
- ✅ Adds all required components automatically
- ✅ Attaches custom scripts to appropriate GameObjects
- ✅ Creates materials and assigns to lights
- ✅ Builds UI Canvas with buttons and text
- ✅ Wires button onClick events to methods
- ✅ Positions objects in 3D space
- ✅ Saves scene to disk

**What Requires Manual Setup:**
- ⚠️ FSM state names (must be set in Inspector)
- ⚠️ Room dropdown wiring (optional, for room-specific control)
- ⚠️ Audio clip assignment (optional, for music player)

**Estimated Total Time:**
- Automated build: ~10 seconds
- Manual steps: ~5 minutes
- Testing: ~3-10 minutes
- **Total: ~15-20 minutes** (vs 4-6 hours manual setup!)

---

## Success Criteria

✅ **Scene is ready when:**
- All 16 GameObjects created in hierarchy
- All components attached correctly
- FSM states configured (Home, Away, Sleep)
- Buttons work in Play Mode
- All communication patterns validated (7 tests pass)
- Zero Inspector connections for messaging confirmed
- Performance at 60 FPS
- Status text updates from devices

---

**Scene Builder Script:** `Assets/UserStudy/Editor/SmartHomeSceneBuilder.cs`
**Generated Scene:** `Assets/UserStudy/Scenes/SmartHome_Mercury.unity`
**Setup Guide:** `Assets/UserStudy/Scenes/SmartHome_Mercury_Setup.md`

**Last Updated:** 2025-11-21
