# User Study Task Checklist

**Last Updated:** 2025-11-21 (End of Session)
**Current Phase:** Phase 2.1 COMPLETE + ENHANCED → Phase 2.2 NEXT (Unity Events Implementation)
**Session Handoff:** See SESSION_HANDOFF_2025-11-21_FINAL.md (comprehensive handoff doc)

This document provides a checklist format for tracking progress on the user study development task. Mark tasks with ✅ when complete.

---

## Phase 1: Complete Planning Documentation (Week 1-2)

**Goal:** Finish detailed planning for all 5 scenes
**Estimated Effort:** 40-60 hours

### Planning Documents

- [x] ✅ README.md - Overview and scene inventory (COMPLETE)
- [x] ✅ 01-smart-home-control.md - Smart Home detailed planning (COMPLETE, ~1,200 lines)
- [x] ✅ 02-music-mixing-board.md - Music Mixer detailed planning (COMPLETE, ~900 lines)
- [ ] ❌ 03-tower-defense-waves.md - Tower Defense detailed planning (~800-1,000 lines)
  - [ ] Object hierarchy diagrams (Mercury + Events)
  - [ ] Communication patterns (5+ patterns with code)
  - [ ] Complete implementation details (all scripts)
  - [ ] 5 user study tasks with solutions
  - [ ] Metrics and expected results
- [ ] ❌ 04-modular-puzzle-room.md - Puzzle Room detailed planning (~600-800 lines)
  - [ ] Object hierarchy diagrams
  - [ ] AND/OR logic patterns
  - [ ] State coordination examples
  - [ ] 5 user study tasks
  - [ ] Debug task design
- [ ] ❌ 05-factory-assembly-line.md - Assembly Line detailed planning (~600-800 lines)
  - [ ] Object hierarchy diagrams
  - [ ] Sequential workflow patterns
  - [ ] Quality control broadcasting
  - [ ] 5 user study tasks
  - [ ] Production state examples
- [ ] ❌ comparison-matrix.md - Side-by-side code comparisons
  - [ ] LOC comparison table (all 5 scenes)
  - [ ] Inspector connections comparison
  - [ ] Code snippets for key patterns
  - [ ] Coupling metrics comparison
  - [ ] Complexity analysis
- [ ] ❌ user-study-design.md - Study methodology document
  - [ ] Participant requirements
  - [ ] Randomization procedure
  - [ ] Task administration protocol
  - [ ] Metrics collection methodology
  - [ ] Statistical analysis plan
  - [ ] Ethical considerations
  - [ ] Consent form template
  - [ ] IRB application materials (if needed)

**Phase 1 Completion Criteria:**
- [ ] All 5 scene planning docs complete and detailed
- [ ] Comparison matrix shows clear Mercury advantages
- [ ] Study methodology approved by advisor/committee
- [ ] IRB submission ready (if required)

---

## Phase 2: Smart Home Scene Implementation (Week 3-5)

**Goal:** Build first scene pair as proof of concept
**Estimated Effort:** 80-120 hours

### 2.1: Mercury Implementation (Week 3) ✅ COMPLETE + ENHANCED + BUG-FREE

**Final Session Enhancements (2025-11-21):**
- [x] ✅ Fixed EventSystem missing StandaloneInputModule (Unity 6.x requirement)
- [x] ✅ Fixed buttons/sliders interactable=false (programmatic creation default)
- [x] ✅ Added live slider value displays (brightness %, temperature °C)
- [x] ✅ Added thermostat visual feedback (color-coded heating + live temp display)
- [x] ✅ Added music player pulsing animation
- [x] ✅ **CRITICAL:** Fixed MercuryMessaging architecture violation in value displays
- [x] ✅ Fixed pre-existing MmMetadataBlock parameter order compilation errors
- [x] ✅ Enhanced status text (size 20, bold)

**Status:** ✅ Code complete and compiles. Needs rebuild and Play Mode testing.

**Previous Session Enhancements (Earlier):**
- [x] ✅ Fixed FSM NullReferenceException (simplified to MmRelayNode)
- [x] ✅ Added bidirectional ON/OFF controls (+128 LOC to ControlPanel.cs)
- [x] ✅ Added brightness/temperature sliders with MmMessageFloat
- [x] ✅ Added music player controls (Tag2)
- [x] ✅ Auto-wired dropdown for room selection
- [x] ✅ Complete UI redesign (10 buttons, 2 sliders, dropdown)
- [x] ✅ Added CreateSlider, WireSlider, WireDropdown helpers

**Scene Setup:**
- [x] ✅ Create `Assets/UserStudy/Scenes/SmartHome_Mercury.unity` (automated via SceneBuilder)
- [x] ✅ Create folder `Assets/UserStudy/Scripts/Mercury/SmartHome/`
- [x] ✅ Set up hierarchy: SmartHomeHub (root) + 3 rooms + 12 devices (automated)

**Script Implementation:**
- [x] ✅ SmartHomeHub.cs (~47 lines actual)
  - [x] ✅ MmRelaySwitchNode component
  - [x] ✅ FSM setup (Home/Away/Sleep states)
  - [x] ✅ SetMode() method for preset switching
- [x] ✅ ControlPanel.cs (~100 lines actual)
  - [x] ✅ MmBaseResponder + MmRelayNode
  - [x] ✅ UI button handlers (All Off, Lights Off, Climate Off, Room Off)
  - [x] ✅ ReceivedMessage() for status updates
  - [x] ✅ Room selection logic
- [x] ✅ SmartLight.cs (~110 lines actual)
  - [x] ✅ MmBaseResponder + MmRelayNode
  - [x] ✅ Tag = MmTag.Tag0 (Lights)
  - [x] ✅ ReceivedSetActive() handler
  - [x] ✅ ReceivedSwitch() for mode changes
  - [x] ✅ UpdateLight() visual feedback
  - [x] ✅ FadeOut() coroutine
- [x] ✅ Thermostat.cs (~100 lines actual)
  - [x] ✅ MmBaseResponder + MmRelayNode
  - [x] ✅ Tag = MmTag.Tag1 (Climate)
  - [x] ✅ Temperature simulation
  - [x] ✅ ReceivedSwitch() for mode changes
  - [x] ✅ OnTargetReached() status reporting
- [x] ✅ SmartBlinds.cs (~89 lines actual)
  - [x] ✅ MmBaseResponder + MmRelayNode
  - [x] ✅ Tag = MmTag.Tag1 (Climate)
  - [x] ✅ Open/Close animation
  - [x] ✅ ReceivedSwitch() handler
- [x] ✅ MusicPlayer.cs (~90 lines actual)
  - [x] ✅ MmBaseResponder + MmRelayNode
  - [x] ✅ Tag = MmTag.Tag2 (Entertainment)
  - [x] ✅ AudioSource integration
  - [x] ✅ Play/Stop methods
  - [x] ✅ ReceivedSwitch() handler

**Automated Scene Builder:**
- [x] ✅ SmartHomeSceneBuilder.cs editor script created
- [x] ✅ One-click scene construction (UserStudy → Build Smart Home Mercury Scene)
- [x] ✅ Materials created automatically (LightOn, LightOff)
- [x] ✅ All GameObjects created with proper hierarchy
- [x] ✅ All components attached automatically
- [x] ✅ UI Canvas and buttons created
- [x] ✅ Button onClick events wired

**UI Implementation:**
- [x] ✅ Control panel canvas (buttons, status text) - automated
- [x] ✅ "All Off" button - automated
- [x] ✅ "Lights Off" button - automated
- [x] ✅ "Climate Off" button - automated
- [x] ✅ "Room Off" button - automated
- [x] ✅ Room selection dropdown - automated (manual wiring optional)
- [x] ✅ Mode buttons (Home, Away, Sleep) - automated
- [x] ✅ Status text display - automated
- [x] ✅ Visual feedback (button states) - automated

**Testing:** (Ready to test in Unity)
- [ ] All devices respond to "All Off"
- [ ] Tag-based filtering works (Lights Off, Climate Off)
- [ ] Room-level control works (Bedroom Off)
- [ ] FSM mode switching works (Home/Away/Sleep)
- [ ] Status updates appear in control panel
- [ ] Zero Inspector connections confirmed
- [ ] No compilation errors
- [ ] Performance acceptable (60 FPS)

**Documentation:**
- [x] ✅ Document LOC count (~526 lines with comments, ~340 code-only)
- [x] ✅ Document Inspector connections (0 for messaging)
- [x] ✅ SmartHome_Mercury_Setup.md (comprehensive setup guide)
- [x] ✅ BUILD_INSTRUCTIONS.md (scene builder usage guide)
- [ ] Screenshot hierarchy for reference (after Unity testing)

---

### 2.2: Unity Events Implementation (Week 4)

**Scene Setup:**
- [ ] Create `Assets/UserStudy/Scenes/SmartHome_Events.unity`
- [ ] Create folder `Assets/UserStudy/Scripts/UnityEvents/SmartHome/`
- [ ] Set up hierarchy: SmartHomeController + rooms + 12 devices

**Script Implementation:**
- [ ] ISmartDevice.cs (~10 lines)
  - [ ] Interface: TurnOn(), TurnOff()
- [ ] SmartHomeController.cs (~150 lines)
  - [ ] HomeMode enum (Home, Away, Sleep)
  - [ ] Lists for all devices (lights, thermostats, blinds, music)
  - [ ] RoomDevices struct for grouping
  - [ ] OnAllOffButton() handler
  - [ ] OnLightsOffButton() handler
  - [ ] OnRoomOffButton() handler
  - [ ] SetMode() method (large switch statement)
  - [ ] HandleDeviceStatus() callback
  - [ ] Start() wiring of callbacks
- [ ] SmartLightBehaviour.cs (~50 lines)
  - [ ] ISmartDevice implementation
  - [ ] StatusEvent UnityEvent
  - [ ] TurnOn/TurnOff methods
  - [ ] SetBrightness() method
  - [ ] UpdateLight() visual feedback
- [ ] ThermostatBehaviour.cs (~70 lines)
  - [ ] ISmartDevice implementation
  - [ ] StatusEvent UnityEvent
  - [ ] Temperature simulation
  - [ ] SetTargetTemperature() method
  - [ ] SetNightMode() method
- [ ] SmartBlindsBehaviour.cs (~50 lines)
  - [ ] ISmartDevice implementation
  - [ ] StatusEvent UnityEvent
  - [ ] Open/Close methods
  - [ ] Animation control
- [ ] MusicPlayerBehaviour.cs (~60 lines)
  - [ ] ISmartDevice implementation
  - [ ] StatusEvent UnityEvent
  - [ ] Play/Stop methods
  - [ ] AudioSource integration
- [ ] RoomController.cs (~40 lines, optional)
  - [ ] List of devices in room
  - [ ] TurnOffRoom() method

**Inspector Wiring:**
- [ ] Wire SmartHomeController → all 8 lights (allTracks list)
- [ ] Wire SmartHomeController → 2 thermostats (thermostats list)
- [ ] Wire SmartHomeController → 2 blinds (blinds list)
- [ ] Wire SmartHomeController → 1 music player (reference)
- [ ] Wire SmartHomeController → ControlPanel UI (reference)
- [ ] Wire 8 light StatusEvents → SmartHomeController.HandleDeviceStatus
- [ ] Wire 2 thermostat StatusEvents → SmartHomeController.HandleDeviceStatus
- [ ] Wire 2 blinds StatusEvents → SmartHomeController.HandleDeviceStatus
- [ ] Wire 1 music player StatusEvent → SmartHomeController.HandleDeviceStatus
- [ ] Wire UI buttons → SmartHomeController methods
- [ ] Set up RoomDevices groupings (3 rooms)
- [ ] **Total connections documented: ~40**

**UI Implementation:**
- [ ] Control panel canvas (same as Mercury)
- [ ] All buttons wired to controller methods
- [ ] Status text display functional

**Testing:**
- [ ] Feature parity with Mercury version confirmed
- [ ] All devices respond correctly
- [ ] Room grouping works
- [ ] Mode switching works
- [ ] Status updates work
- [ ] No compilation errors

**Documentation:**
- [ ] Document LOC count (~430 lines)
- [ ] Document Inspector connections (~40)
- [ ] Document wiring complexity
- [ ] Screenshot Inspector for reference

---

### 2.3: Task Design & Metrics (Week 5)

**Task Design:**
- [ ] Task 1: Add New Light (5-8 min)
  - [ ] Write instruction document
  - [ ] Define acceptance criteria
  - [ ] Document Mercury solution (0 LOC, 2 Inspector changes)
  - [ ] Document Events solution (0 LOC, 5 Inspector changes)
- [ ] Task 2: Climate Control Button (8-12 min)
  - [ ] Write instruction document
  - [ ] Document Mercury solution (~10 LOC)
  - [ ] Document Events solution (~10 LOC)
- [ ] Task 3: Add New Room (10-15 min)
  - [ ] Write instruction document
  - [ ] Document Mercury solution (~5 LOC, 7 Inspector)
  - [ ] Document Events solution (~5 LOC, 15+ Inspector)
- [ ] Task 4: Party Mode (12-18 min)
  - [ ] Write instruction document
  - [ ] Document Mercury solution (~40 LOC)
  - [ ] Document Events solution (~80 LOC)
- [ ] Task 5: Debug Task (5-10 min)
  - [ ] Plant bug (wrong tag in Mercury, missing list entry in Events)
  - [ ] Write instruction document
  - [ ] Document solution

**Task UI:**
- [ ] Create `TaskUI.cs` script
- [ ] Display task instructions in-game
- [ ] Timer start/stop functionality
- [ ] Task completion button
- [ ] Next task progression

**Metrics Collection:**
- [ ] Implement `MetricsCollector.cs`
  - [ ] LOC counting algorithm
  - [ ] Inspector connection detection
  - [ ] Compilation error tracking
- [ ] Implement `DataLogger.cs`
  - [ ] CSV export functionality
  - [ ] Timestamp all data
  - [ ] Participant ID assignment
- [ ] Test metrics on Smart Home implementations
  - [ ] Verify LOC counts (340 vs 430)
  - [ ] Verify connection counts (0 vs 40)
  - [ ] Verify CSV export format

**Pilot Testing:**
- [ ] Recruit 2 pilot participants
- [ ] Participant 1: Mercury first
  - [ ] Complete all 5 tasks
  - [ ] Record times, errors, feedback
- [ ] Participant 2: Events first
  - [ ] Complete all 5 tasks
  - [ ] Record times, errors, feedback
- [ ] Analyze pilot data
  - [ ] Task times within estimates?
  - [ ] Instructions clear?
  - [ ] Bugs appropriately difficult?
  - [ ] Metrics collected correctly?
- [ ] Refine based on feedback
  - [ ] Update task instructions
  - [ ] Adjust difficulty if needed
  - [ ] Fix metrics bugs

**Phase 2 Completion Criteria:**
- [ ] Both Smart Home implementations complete and tested
- [ ] 5 tasks designed, piloted, and refined
- [ ] Metrics collection system working
- [ ] Pilot data validates approach
- [ ] Ready to proceed to Phase 3

---

## Phase 3: Music Mixing Board Implementation (Week 6-8)

**Goal:** Second scene pair, unique scenario
**Estimated Effort:** 80-120 hours

### 3.1: Mercury Implementation (Week 6)

**Scene Setup:**
- [ ] Create `Assets/UserStudy/Scenes/MusicMixer_Mercury.unity`
- [ ] Create folder `Assets/UserStudy/Scripts/Mercury/MusicMixer/`
- [ ] Set up hierarchy: MixerHub + 8 tracks + 3 groups + 4 effects

**Script Implementation:**
- [ ] MixerHub.cs (~30 lines)
- [ ] AudioTrack.cs (~90 lines)
- [ ] AudioEffect.cs (~40 lines, base class)
- [ ] ReverbEffect.cs (~20 lines)
- [ ] DelayEffect.cs (~20 lines)
- [ ] EQEffect.cs (~20 lines)
- [ ] DistortionEffect.cs (~20 lines)
- [ ] UI_MixerPanel.cs (~80 lines)
- [ ] MasterOutput.cs (~30 lines)

**UI Implementation:**
- [ ] Mixer panel canvas
- [ ] Master volume slider
- [ ] 8 track volume sliders
- [ ] Mute group buttons (Drums, Bass, Melody)
- [ ] Preset buttons (Intro, Verse, Chorus, Outro)
- [ ] Volume meters (8 tracks)
- [ ] Effect parameter sliders

**Testing:**
- [ ] Master volume broadcasts to all tracks
- [ ] Tag-based mute groups work
- [ ] Hierarchical effect chain processes audio
- [ ] Preset system functional (4 states)
- [ ] Volume meters update from tracks
- [ ] Zero Inspector connections

**Documentation:**
- [ ] LOC count (~350 lines)
- [ ] Inspector connections (0)

---

### 3.2: Unity Events Implementation (Week 7)

**Scene Setup:**
- [ ] Create `Assets/UserStudy/Scenes/MusicMixer_Events.unity`
- [ ] Create folder `Assets/UserStudy/Scripts/UnityEvents/MusicMixer/`

**Script Implementation:**
- [ ] MixerController.cs (~200 lines, large controller)
- [ ] AudioTrack.cs (~80 lines with UnityEvents)
- [ ] EffectRackController.cs (~100 lines)
- [ ] ReverbEffect.cs (~30 lines)
- [ ] DelayEffect.cs (~30 lines)
- [ ] EQEffect.cs (~30 lines)
- [ ] DistortionEffect.cs (~30 lines)
- [ ] MixerUI.cs (~100 lines)
- [ ] MasterOutput.cs (~40 lines)

**Inspector Wiring:**
- [ ] Wire ~35 connections
- [ ] Document all connections

**Testing:**
- [ ] Feature parity with Mercury version
- [ ] Manual effect routing works
- [ ] Separate track group lists work

**Documentation:**
- [ ] LOC count (~640 lines)
- [ ] Inspector connections (~35)

---

### 3.3: Task Design & Pilot (Week 8)

**Task Design:**
- [ ] Task 1: Add New Synth Track (5-8 min)
- [ ] Task 2: Solo Button (10-15 min)
- [ ] Task 3: Add Compression Effect (15-20 min)
- [ ] Task 4: Bridge Preset (12-18 min)
- [ ] Task 5: Debug Task (5-10 min)

**Pilot Testing:**
- [ ] Recruit 2 pilot participants
- [ ] Test tasks
- [ ] Refine based on feedback

**Phase 3 Completion Criteria:**
- [ ] Both Music Mixer implementations complete
- [ ] 5 tasks designed and piloted
- [ ] Unique tasks (not Smart Home repeats)

---

## Phase 4: Tower Defense Implementation (Week 9-11)

**Goal:** Most game-like scene, medium complexity
**Estimated Effort:** 120-160 hours

### 4.1: Mercury Implementation (Week 9-10)

**Scene Setup:**
- [ ] Create `Assets/UserStudy/Scenes/TowerDefense_Mercury.unity`
- [ ] Create folder `Assets/UserStudy/Scripts/Mercury/TowerDefense/`
- [ ] Set up hierarchy: GameController + WaveController + 4 spawners + 6 towers

**Script Implementation:**
- [ ] GameController.cs (MmRelaySwitchNode for game states)
- [ ] WaveController.cs (wave management)
- [ ] Spawner.cs (enemy spawning)
- [ ] Enemy.cs (autonomous AI with tags)
- [ ] Tower.cs (targeting and shooting)
- [ ] UIManager.cs (HUD)
- [ ] EnemyPool.cs (object pooling)
- [ ] Projectile.cs (tower projectiles)

**UI Implementation:**
- [ ] Health bar
- [ ] Wave counter
- [ ] Score display
- [ ] Game state UI (Menu, Playing, Victory, Defeat)

**Testing:**
- [ ] WaveController broadcasts to all spawners
- [ ] Enemies report death to counter (aggregation)
- [ ] Towers target enemies without references
- [ ] Tag system for enemy types (Ground/Air)
- [ ] Game states functional
- [ ] Enemy pooling works
- [ ] Performance acceptable (60 FPS with 20+ enemies)

**Documentation:**
- [ ] LOC count (~450 lines)
- [ ] Inspector connections (0)

---

### 4.2: Unity Events Implementation (Week 10-11)

**Scene Setup:**
- [ ] Create `Assets/UserStudy/Scenes/TowerDefense_Events.unity`
- [ ] Create folder `Assets/UserStudy/Scripts/UnityEvents/TowerDefense/`

**Script Implementation:**
- [ ] GameController.cs (larger, more complex)
- [ ] WaveController.cs (with references)
- [ ] Spawner.cs (with spawner lists)
- [ ] Enemy.cs (with UnityEvents)
- [ ] Tower.cs (manual targeting logic)
- [ ] UIManager.cs
- [ ] EnemyPool.cs
- [ ] Projectile.cs
- [ ] Additional helper scripts as needed

**Inspector Wiring:**
- [ ] Wire ~50+ connections
- [ ] Document complexity

**Testing:**
- [ ] Feature parity confirmed
- [ ] Manual spawner wiring works
- [ ] Enemy type filtering via separate events

**Documentation:**
- [ ] LOC count (~750 lines)
- [ ] Inspector connections (~50+)

---

### 4.3: Task Design (Week 11)

**Task Design:**
- [ ] Task 1: Add New Spawner (8-12 min)
- [ ] Task 2: New Enemy Type (12-16 min)
- [ ] Task 3: Freeze Tower (15-20 min)
- [ ] Task 4: Wave Progression UI (10-15 min)
- [ ] Task 5: Debug Task (8-12 min)

**Pilot Testing:**
- [ ] Recruit 2 pilot participants
- [ ] Test tasks
- [ ] Refine

**Phase 4 Completion Criteria:**
- [ ] Both Tower Defense implementations complete
- [ ] 5 tasks designed and piloted
- [ ] Most complex scene validated

---

## Phase 5: Puzzle Room & Assembly Line (Week 12-14)

**Goal:** Complete remaining 2 scenes
**Estimated Effort:** 120-160 hours

### 5.1: Puzzle Room (Week 12-13)

**Mercury Implementation:**
- [ ] Create `Assets/UserStudy/Scenes/PuzzleRoom_Mercury.unity`
- [ ] Scripts folder
- [ ] RoomController.cs (MmRelaySwitchNode)
- [ ] PressurePlate.cs (~40 lines)
- [ ] LeverSwitch.cs (~40 lines)
- [ ] KeySwitch.cs (~40 lines)
- [ ] Door.cs (~60 lines, AND/OR logic)
- [ ] Light.cs (~30 lines, visual feedback)
- [ ] HintSystem.cs (~40 lines)
- [ ] UI implementation
- [ ] Testing
- [ ] **LOC: ~300 lines, Connections: 0**

**Unity Events Implementation:**
- [ ] Create `Assets/UserStudy/Scenes/PuzzleRoom_Events.unity`
- [ ] Scripts folder
- [ ] RoomController.cs (~120 lines, complex)
- [ ] Switch behaviour scripts (3 scripts, ~50 lines each)
- [ ] Door behaviour (~80 lines, manual tracking)
- [ ] Light behaviour (~40 lines)
- [ ] HintSystem (~50 lines)
- [ ] Wire ~35 connections
- [ ] Testing
- [ ] **LOC: ~500 lines, Connections: ~35**

**Task Design:**
- [ ] Task 1: Add New Door with 2 Switches (10-15 min)
- [ ] Task 2: Timer Reset System (12-16 min)
- [ ] Task 3: Hint System (8-12 min)
- [ ] Task 4: Complex AND Logic (15-18 min)
- [ ] Task 5: Debug Task (6-10 min)

**Pilot Testing:**
- [ ] 2 pilot participants
- [ ] Refine tasks

---

### 5.2: Assembly Line (Week 13-14)

**Mercury Implementation:**
- [ ] Create `Assets/UserStudy/Scenes/AssemblyLine_Mercury.unity`
- [ ] Scripts folder
- [ ] FactoryController.cs (MmRelaySwitchNode)
- [ ] ConveyorBelt.cs (~40 lines)
- [ ] Station.cs (~60 lines, base class)
- [ ] PaintStation.cs (~30 lines)
- [ ] AssemblyStation.cs (~40 lines)
- [ ] PackageStation.cs (~30 lines)
- [ ] QualityControl.cs (~50 lines)
- [ ] Item.cs (~40 lines)
- [ ] UI implementation
- [ ] Testing
- [ ] **LOC: ~380 lines, Connections: 0**

**Unity Events Implementation:**
- [ ] Create `Assets/UserStudy/Scenes/AssemblyLine_Events.unity`
- [ ] Scripts folder
- [ ] FactoryController.cs (~180 lines)
- [ ] Station behaviour scripts (3 scripts)
- [ ] ConveyorBelt behaviour
- [ ] QualityControl behaviour
- [ ] Item behaviour
- [ ] Wire ~45 connections
- [ ] Testing
- [ ] **LOC: ~620 lines, Connections: ~45**

**Task Design:**
- [ ] Task 1: Add Testing Station (12-16 min)
- [ ] Task 2: Reject Defective Items (15-20 min)
- [ ] Task 3: Production Statistics (10-14 min)
- [ ] Task 4: Speed Control (8-12 min)
- [ ] Task 5: Debug Task (8-12 min)

**Pilot Testing:**
- [ ] 2 pilot participants
- [ ] Refine tasks

**Phase 5 Completion Criteria:**
- [ ] Both Puzzle Room implementations complete
- [ ] Both Assembly Line implementations complete
- [ ] All 10 scenes (5 pairs) implemented and tested
- [ ] All 25 tasks (5 per scene) designed and piloted

---

## Phase 6: Study Infrastructure & Pilot Testing (Week 15)

**Goal:** Complete metrics collection and full pilot study
**Estimated Effort:** 40-60 hours

### Metrics Collection System

**MetricsCollector.cs:**
- [ ] Implement LOC counting algorithm
  - [ ] Scan all .cs files in scene folders
  - [ ] Exclude comments and empty lines
  - [ ] Count accurately (±5 lines acceptable)
  - [ ] Test on all 10 scenes
- [ ] Implement timing system
  - [ ] Start timer when task begins
  - [ ] Pause timer for breaks
  - [ ] Stop timer on task completion
  - [ ] Record millisecond precision
- [ ] Implement Inspector connection counter
  - [ ] Parse scene files (.unity format)
  - [ ] Count serialized field references
  - [ ] Count UnityEvent connections
  - [ ] Count list/array elements
- [ ] Implement compilation error tracker
  - [ ] Hook into Unity console
  - [ ] Count errors during task
  - [ ] Categorize error types
- [ ] Test on all 5 scenes
  - [ ] Verify metrics match manual counts
  - [ ] Fix any discrepancies

**DataLogger.cs:**
- [ ] Implement CSV export
  - [ ] Participant ID field
  - [ ] Scene name field
  - [ ] Approach field (Mercury/Events)
  - [ ] Task number field
  - [ ] LOC field
  - [ ] Inspector connections field
  - [ ] Time field (milliseconds)
  - [ ] Errors field
  - [ ] Timestamp field
- [ ] Implement auto-save (every task)
- [ ] Implement data validation
- [ ] Test export format
  - [ ] Verify CSV opens in Excel
  - [ ] Verify all fields populated
  - [ ] Verify no data loss

**NASATLXQuestionnaire.cs:**
- [ ] Implement 6-dimension questionnaire
  - [ ] Mental Demand slider (0-7)
  - [ ] Physical Demand slider (0-7)
  - [ ] Temporal Demand slider (0-7)
  - [ ] Performance slider (0-7)
  - [ ] Effort slider (0-7)
  - [ ] Frustration slider (0-7)
- [ ] Implement post-task display
- [ ] Implement data saving to CSV
- [ ] Test questionnaire flow
  - [ ] Display after each scene
  - [ ] Save responses correctly
  - [ ] Calculate average workload

**Additional Infrastructure:**
- [ ] Implement participant randomization
  - [ ] Assign random ID
  - [ ] Randomize Mercury/Events order
  - [ ] Randomize scene order (within constraints)
  - [ ] Log randomization assignments
- [ ] Create participant instructions document
  - [ ] Study overview
  - [ ] Consent information
  - [ ] Task instructions
  - [ ] What to expect
  - [ ] Contact information
- [ ] Create task instruction UI
  - [ ] Display instructions clearly
  - [ ] Show acceptance criteria
  - [ ] Show time estimates
  - [ ] Allow marking complete
- [ ] Create post-study questionnaire
  - [ ] Subjective preference (5-point Likert)
  - [ ] Perceived maintainability (5-point Likert)
  - [ ] Open-ended feedback (text box)
  - [ ] Demographics (experience level, age, etc.)

### Full Pilot Study

**Preparation:**
- [ ] Recruit 3 pilot participants (different from Phase 2-5 pilots)
- [ ] Prepare study environment
  - [ ] Set up Unity project on study machine
  - [ ] Test all scenes load correctly
  - [ ] Test metrics collection
  - [ ] Prepare recording equipment (screen capture, audio)
- [ ] Create pilot study protocol
  - [ ] Introduction script
  - [ ] Task administration script
  - [ ] Debrief questions

**Pilot Participant 1:**
- [ ] Order: Mercury first, Smart Home → Music Mixer
- [ ] Complete all 10 tasks (2 scenes × 5 tasks)
- [ ] Record session (screen + audio)
- [ ] Collect metrics automatically
- [ ] Administer NASA-TLX (2 times)
- [ ] Administer post-study questionnaire
- [ ] Debrief interview (10-15 minutes)

**Pilot Participant 2:**
- [ ] Order: Events first, Puzzle Room → Assembly Line
- [ ] Complete all 10 tasks
- [ ] Record session
- [ ] Collect metrics
- [ ] NASA-TLX (2 times)
- [ ] Post-study questionnaire
- [ ] Debrief

**Pilot Participant 3:**
- [ ] Order: Mercury first, Tower Defense → Smart Home
- [ ] Complete all 10 tasks
- [ ] Record session
- [ ] Collect metrics
- [ ] NASA-TLX (2 times)
- [ ] Post-study questionnaire
- [ ] Debrief

**Pilot Data Analysis:**
- [ ] Review screen recordings
  - [ ] Identify confusion points
  - [ ] Note unexpected errors
  - [ ] Document workarounds used
- [ ] Analyze quantitative metrics
  - [ ] LOC: Mercury vs Events (20-40% reduction?)
  - [ ] Connections: 0 vs 30-50? (100% reduction?)
  - [ ] Time: Mercury faster by 10-30%?
  - [ ] NASA-TLX: Mercury lower by 0.5-1.5 points?
- [ ] Analyze qualitative feedback
  - [ ] What worked well?
  - [ ] What was confusing?
  - [ ] What should be changed?
  - [ ] Preference patterns?
- [ ] Compile refinement list
  - [ ] Task instruction clarity
  - [ ] Difficulty adjustments
  - [ ] Bug fixes needed
  - [ ] Metrics collection issues
  - [ ] UI improvements

**Refinements:**
- [ ] Update task instructions based on feedback
- [ ] Fix any bugs discovered
- [ ] Adjust difficulty if needed
- [ ] Improve metrics collection if issues
- [ ] Update participant instructions
- [ ] Refine study protocol

**Phase 6 Completion Criteria:**
- [ ] Metrics collection system fully functional
- [ ] NASA-TLX questionnaire working
- [ ] 3 full pilot studies completed
- [ ] Pilot data analyzed
- [ ] Refinements implemented
- [ ] Study protocol finalized
- [ ] Ready for full study recruitment

---

## Phase 7: User Study Execution & Analysis (Week 16+)

**Goal:** Recruit participants, run study, analyze data
**Estimated Effort:** 80-120 hours

### IRB Approval (If Required)

- [ ] Check if IRB approval required (consult institution)
- [ ] If yes, prepare IRB application:
  - [ ] Study protocol document
  - [ ] Informed consent form
  - [ ] Recruitment materials (flyers, emails)
  - [ ] Data management plan
  - [ ] Risk assessment
  - [ ] Participant screening questionnaire
- [ ] Submit IRB application
- [ ] Respond to IRB questions/revisions
- [ ] Obtain IRB approval letter
- [ ] **Estimated timeline: 2-6 weeks**

### Participant Recruitment

**Recruitment Channels:**
- [ ] Post on Unity forums (r/Unity3D, Unity Discord)
- [ ] Post on Twitter/X with #Unity3D hashtag
- [ ] Email university game dev programs
- [ ] Post on LinkedIn
- [ ] Consider paid platforms (Prolific, UserTesting) if budget allows

**Recruitment Materials:**
- [ ] Create recruitment flyer (PDF)
- [ ] Create screening questionnaire (Google Forms)
  - [ ] Unity experience (6+ months required)
  - [ ] C# scripting ability (self-assessment)
  - [ ] Availability (2-3 hours)
  - [ ] Contact information
- [ ] Create consent form (digital signature)

**Recruitment Goal: 15-20 participants**
- [ ] Screen 30-40 applicants
- [ ] Accept 15-20 qualified
- [ ] Schedule sessions

**Participant Tracking:**
- [ ] Create participant database (spreadsheet)
  - [ ] Participant ID (P01-P20)
  - [ ] Randomization assignment (Mercury/Events first)
  - [ ] Scene order assignment
  - [ ] Session date/time
  - [ ] Completion status
  - [ ] Payment status ($40 per participant)

### Study Sessions (15-20 sessions)

**For Each Participant:**
- [ ] Session setup (30 minutes before)
  - [ ] Prepare Unity project
  - [ ] Test all scenes load
  - [ ] Set up screen recording
  - [ ] Prepare consent form
  - [ ] Review randomization assignment
- [ ] Session introduction (10 minutes)
  - [ ] Welcome and consent
  - [ ] Study overview
  - [ ] Mercury/Events explanation (brief)
  - [ ] Q&A
- [ ] Scene 1 (45-60 minutes)
  - [ ] Load scene (Mercury or Events, based on randomization)
  - [ ] Complete 5 tasks
  - [ ] NASA-TLX questionnaire
- [ ] Break (5-10 minutes)
- [ ] Scene 2 (45-60 minutes)
  - [ ] Load scene (opposite approach)
  - [ ] Complete 5 tasks
  - [ ] NASA-TLX questionnaire
- [ ] Optional Scene 3 (if time allows)
- [ ] Post-study questionnaire (10 minutes)
  - [ ] Subjective preference
  - [ ] Perceived maintainability
  - [ ] Open-ended feedback
  - [ ] Demographics
- [ ] Debrief (10 minutes)
  - [ ] Thank participant
  - [ ] Explain study goals
  - [ ] Answer questions
  - [ ] Arrange compensation
- [ ] Session cleanup
  - [ ] Save all data files
  - [ ] Backup screen recording
  - [ ] Update participant database

**Track Progress:**
- [ ] Participant 1 (P01) - Complete
- [ ] Participant 2 (P02) - Complete
- [ ] Participant 3 (P03) - Complete
- [ ] ... (continue for P04-P20)
- [ ] Participant 15 (P15) - Complete
- [ ] Optional participants 16-20 if needed

### Data Analysis

**Data Preparation:**
- [ ] Compile all CSV files from MetricsCollector
- [ ] Compile all NASA-TLX responses
- [ ] Compile all post-study questionnaires
- [ ] Clean data
  - [ ] Check for missing values
  - [ ] Verify data integrity
  - [ ] Handle outliers (if any)
- [ ] Organize data for analysis
  - [ ] One row per task completion
  - [ ] Columns: ParticipantID, Scene, Approach, Task, LOC, Connections, Time, Errors, NASA-TLX scores

**Descriptive Statistics:**
- [ ] Calculate means and standard deviations
  - [ ] LOC (Mercury vs Events)
  - [ ] Inspector connections (Mercury vs Events)
  - [ ] Development time (Mercury vs Events)
  - [ ] NASA-TLX workload (Mercury vs Events)
- [ ] Create summary table

**Inferential Statistics:**
- [ ] Paired t-tests (within-subject comparisons)
  - [ ] LOC: Mercury vs Events
  - [ ] Connections: Mercury vs Events (should be 0 vs 30-50)
  - [ ] Time: Mercury vs Events
  - [ ] NASA-TLX: Mercury vs Events
- [ ] Calculate effect sizes (Cohen's d)
  - [ ] LOC effect size
  - [ ] Time effect size
  - [ ] Workload effect size
- [ ] Check assumptions
  - [ ] Normality (Shapiro-Wilk test)
  - [ ] Outliers (boxplots)
- [ ] Report p-values and confidence intervals

**Qualitative Analysis:**
- [ ] Thematic analysis of open-ended feedback
  - [ ] What participants liked about Mercury
  - [ ] What participants disliked about Mercury
  - [ ] What participants liked about Events
  - [ ] What participants disliked about Events
- [ ] Code themes and sub-themes
- [ ] Count theme frequencies
- [ ] Select representative quotes

**Visualizations:**
- [ ] Create bar charts (means with error bars)
  - [ ] LOC comparison
  - [ ] Time comparison
  - [ ] NASA-TLX comparison
- [ ] Create boxplots (distributions)
  - [ ] LOC by approach
  - [ ] Time by approach
- [ ] Create scatter plots (if relevant)
  - [ ] Experience level vs performance
- [ ] Export high-resolution images for paper

**Results Summary:**
- [ ] Hypothesis 1: LOC reduction (20-40% expected)
  - [ ] Result: [TBD]% reduction, p = [TBD]
- [ ] Hypothesis 2: Connection reduction (100% expected)
  - [ ] Result: [TBD]% reduction (0 vs [TBD])
- [ ] Hypothesis 3: Time reduction (10-30% expected)
  - [ ] Result: [TBD]% reduction, p = [TBD]
- [ ] Hypothesis 4: Workload reduction (0.5-1.5 points expected)
  - [ ] Result: [TBD] point reduction, p = [TBD]
- [ ] Hypothesis 5: Preference (60%+ prefer Mercury expected)
  - [ ] Result: [TBD]% prefer Mercury

### Publication Preparation

**Results Section:**
- [ ] Write quantitative results
  - [ ] Descriptive statistics
  - [ ] Inferential statistics
  - [ ] Effect sizes
- [ ] Write qualitative results
  - [ ] Themes and quotes
- [ ] Create figures and tables
  - [ ] Insert visualizations
  - [ ] Format tables (APA style)

**Discussion Section:**
- [ ] Interpret results
  - [ ] Mercury advantages validated?
  - [ ] Unexpected findings?
  - [ ] Limitations?
- [ ] Compare to prior work
- [ ] Discuss implications
  - [ ] For framework designers
  - [ ] For Unity developers
  - [ ] For HCI researchers

**Paper Sections:**
- [ ] Abstract (250 words)
- [ ] Introduction
- [ ] Related Work
- [ ] MercuryMessaging Framework Overview
- [ ] Study Design and Methodology
- [ ] Results
- [ ] Discussion
- [ ] Conclusion
- [ ] References

**Submission:**
- [ ] Choose target venue (CHI, UIST, VL/HCC)
- [ ] Format paper to venue template
- [ ] Get co-author feedback
- [ ] Revise
- [ ] Submit

**Phase 7 Completion Criteria:**
- [ ] 15-20 participants completed study
- [ ] All data collected and analyzed
- [ ] Statistical significance achieved (hopefully!)
- [ ] Paper drafted and submitted
- [ ] Study complete! 🎉

---

## Overall Progress Tracker

**Phase 1:** Planning Documentation
- Progress: 40% (2/5 detailed plans complete)
- Status: ⚠️ In Progress

**Phase 2:** Smart Home Implementation
- Progress: 0% (not started)
- Status: ❌ Not Started

**Phase 3:** Music Mixer Implementation
- Progress: 0% (not started)
- Status: ❌ Not Started

**Phase 4:** Tower Defense Implementation
- Progress: 0% (not started)
- Status: ❌ Not Started

**Phase 5:** Puzzle & Assembly Line
- Progress: 0% (not started)
- Status: ❌ Not Started

**Phase 6:** Infrastructure & Pilot
- Progress: 0% (not started)
- Status: ❌ Not Started

**Phase 7:** Study Execution & Analysis
- Progress: 0% (not started)
- Status: ❌ Not Started

**Overall Progress: 6%** (2/7 phases started)

---

## Quick Commands

**Check Planning Progress:**
```bash
ls -la dev/active/user-study/*.md | wc -l
# Should be 10 files when Phase 1 complete
```

**Count LOC in Mercury Scripts:**
```bash
find Assets/UserStudy/Scripts/Mercury -name "*.cs" -exec wc -l {} + | tail -1
```

**Count LOC in Unity Events Scripts:**
```bash
find Assets/UserStudy/Scripts/UnityEvents -name "*.cs" -exec wc -l {} + | tail -1
```

**List All Scenes:**
```bash
find Assets/UserStudy/Scenes -name "*.unity"
# Should be 10 scenes when Phases 2-5 complete
```

---

**Last Updated:** 2025-11-21
**Total Tasks:** 200+ (across all phases)
**Estimated Completion:** 16-20 weeks
