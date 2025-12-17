# Doxygen Enhancement & Tutorial Scene Implementation - Task Checklist

**Last Updated:** 2025-12-17
**Status:** Ready for Implementation

---

## Quick Resume

**Current Phase:** Not Started
**Next Action:** Start Phase 1 - Enhance Doxyfile

---

## Phase 1: Doxygen Enhancement ⏳ NOT STARTED

### Task 1.1: Enhance Doxyfile Configuration [S]
- [ ] Verify Graphviz installed (`where dot`)
- [ ] Update Doxyfile with enhanced settings:
  - [ ] Set EXTRACT_PRIVATE = YES
  - [ ] Set EXTRACT_PACKAGE = YES
  - [ ] Set INLINE_INHERITED_MEMB = YES
  - [ ] Set HTML_COLORSTYLE = AUTO_LIGHT
  - [ ] Set WARN_IF_UNDOCUMENTED = YES
  - [ ] Set WARN_NO_PARAMDOC = YES
- [ ] Enable Graphviz diagrams:
  - [ ] Set HAVE_DOT = YES
  - [ ] Set CLASS_DIAGRAMS = YES
  - [ ] Set COLLABORATION_GRAPH = YES
  - [ ] Set GROUP_GRAPHS = YES
  - [ ] Set INCLUDE_GRAPH = YES
  - [ ] Set INCLUDED_BY_GRAPH = YES
  - [ ] Set GRAPHICAL_HIERARCHY = YES
  - [ ] Set DIRECTORY_GRAPH = YES
  - [ ] Set DOT_IMAGE_FORMAT = svg
  - [ ] Set DOT_TRANSPARENT = YES

### Task 1.2: Fix Stale Documentation References [S]
- [ ] Check `Documentation/WORKFLOWS.md` for dead links
- [ ] Remove reference to non-existent `Documentation/Tutorials/DSL_QUICK_START.md`
- [ ] Verify all other cross-references are valid

### Task 1.3: Generate Doxygen Documentation [S]
- [ ] Run `doxygen Doxyfile`
- [ ] Verify output in `docs/html/`
- [ ] Check version shows 4.0.0 in header
- [ ] Verify class diagrams generated (SVG format)
- [ ] Test search functionality
- [ ] Confirm no generation errors

---

## Phase 2: Audit Existing Tutorial Scenes ⏳ NOT STARTED

### Task 2.1: Audit Tutorial 1 Scene [S]
- [ ] Open `Tutorial1/Tutorial1_Base.unity` in Unity
- [ ] Remove XR Origin / XR Rig GameObjects
- [ ] Remove Graph Controller / DebugGraph objects
- [ ] Remove EPOOutline / visualization helpers
- [ ] Remove unused cameras or lights
- [ ] Remove missing script references
- [ ] Verify scene loads without errors
- [ ] Save scene

### Task 2.2: Audit Tutorial 2 Scene [S]
- [ ] Open `Tutorial2/Tutorial2_Base.unity` in Unity
- [ ] Remove XR/debug clutter (same as 2.1)
- [ ] Verify scene loads without errors
- [ ] Save scene

### Task 2.3: Audit Tutorial 3 Scene [S]
- [ ] Open `Tutorial3/Tutorial3_Base.unity` in Unity
- [ ] Remove XR/debug clutter
- [ ] Verify scene loads without errors
- [ ] Save scene

### Task 2.4: Audit Tutorial 4 Scene [S]
- [ ] Open `Tutorial4/Tutorial4_Base.unity` in Unity
- [ ] Remove XR/debug clutter
- [ ] Verify scene loads without errors
- [ ] Save scene

### Task 2.5: Audit Tutorial 5 Scene [S]
- [ ] Open `Tutorial5/Tutorial5_Base.unity` in Unity
- [ ] Remove XR/debug clutter
- [ ] Verify scene loads without errors
- [ ] Save scene

### Task 2.6: Extract Content from SimpleScene [S]
- [ ] Copy `InvocationComparison.cs` to `Tests/Performance/Scripts/`
- [ ] Verify copy works correctly
- [ ] Note any other useful content

### Task 2.7: Delete Redundant Folders [S]
- [ ] Delete `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/`
- [ ] Delete `Assets/MercuryMessaging/Examples/Tutorials/SimpleTutorial_Alternative/`
- [ ] Verify project compiles successfully
- [ ] Verify no broken references

---

## Phase 3: Create Missing Tutorial Scenes ⏳ NOT STARTED

### Task 3.1: Create Tutorial 6 Scene (FishNet) [M]
- [ ] Create new scene `Tutorial6/Tutorial6_FishNet.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Create Canvas with Instructions text
- [ ] Create NetworkRoot hierarchy:
  - [ ] NetworkRoot (MmRelayNode + T6_NetworkGameController)
  - [ ] Player child (MmRelayNode + T6_PlayerResponder + Cube)
  - [ ] Enemy child (MmRelayNode + Cube)
  - [ ] ScoreUI child (Text)
- [ ] Configure fallback mode (MmLoopbackBackend)
- [ ] Test S key (server sync)
- [ ] Test Space key (client request)
- [ ] Save scene

### Task 3.2: Create Tutorial 7 Scene (Fusion 2) [M]
- [ ] Create new scene `Tutorial7/Tutorial7_Fusion2.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Create Canvas with Instructions text
- [ ] Create GameRoot hierarchy:
  - [ ] GameRoot (MmRelayNode + T7_Fusion2GameController)
  - [ ] Player1 child (MmRelayNode + T7_Fusion2PlayerResponder + Sphere)
  - [ ] Player2 child (MmRelayNode + Sphere)
- [ ] Configure fallback mode (MmLoopbackBackend)
- [ ] Test controls
- [ ] Save scene

### Task 3.3: Create Tutorial 8 Scene (FSM) [M]
- [ ] Create new scene `Tutorial8/Tutorial8_FSM.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Add EventSystem
- [ ] Create GameManager hierarchy:
  - [ ] GameManager (MmRelaySwitchNode + T8_GameStateController)
  - [ ] MainMenu child (MmRelayNode + T8_MenuResponder)
    - [ ] MenuCanvas with StartButton
  - [ ] Gameplay child (MmRelayNode + T8_GameplayResponder)
    - [ ] Player (Cube)
    - [ ] ScoreText
  - [ ] PauseMenu child (MmRelayNode + T8_PauseResponder)
    - [ ] PauseCanvas with ResumeButton
  - [ ] GameOver child (MmRelayNode + T8_GameOverResponder)
    - [ ] GameOverCanvas with RestartButton
- [ ] Test Escape key (pause toggle)
- [ ] Test Enter key (start/restart)
- [ ] Verify FSM transitions work
- [ ] Save scene

### Task 3.4: Create Tutorial 9 Scene (Task Management) [M]
- [ ] Create new scene `Tutorial9/Tutorial9_Tasks.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Add EventSystem
- [ ] Create Experiment hierarchy:
  - [ ] Experiment (MmRelayNode)
  - [ ] TaskManager child (T9_ExperimentManager)
    - [ ] TasksNode (MmRelaySwitchNode)
      - [ ] Instructions (MmRelayNode)
      - [ ] Practice (MmRelayNode + T9_TrialResponder)
      - [ ] MainTrials (MmRelayNode + T9_TrialResponder)
      - [ ] Complete (MmRelayNode)
  - [ ] StimulusDisplay child (MmRelayNode)
    - [ ] FixationCross (Text "+")
    - [ ] Stimulus (Colored square)
  - [ ] DataCollector child (MmDataCollector)
- [ ] Test Space key response
- [ ] Verify trial progression
- [ ] Save scene

### Task 3.5: Create Tutorial 10 Scene (Application State) [M]
- [ ] Create new scene `Tutorial10/Tutorial10_AppState.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Add EventSystem
- [ ] Create App hierarchy:
  - [ ] App (MmRelaySwitchNode + T10_MyAppController)
  - [ ] Splash child (MmRelayNode + T10_LoadingBehavior)
    - [ ] SplashCanvas
  - [ ] MainMenu child (MmRelayNode + T10_MenuBehavior)
    - [ ] MenuCanvas with Play/Options/Quit buttons
  - [ ] Options child (MmRelayNode)
    - [ ] OptionsCanvas with BackButton
  - [ ] Loading child (MmRelayNode + T10_LoadingBehavior)
    - [ ] LoadingCanvas with ProgressBar
  - [ ] Gameplay child (MmRelayNode + T10_GameplayBehavior)
    - [ ] Player (Cube)
    - [ ] GameplayHUD
  - [ ] Pause child (MmRelayNode + T10_PauseBehavior)
    - [ ] PauseCanvas with ResumeButton
- [ ] Test Splash auto-advance
- [ ] Test UI buttons
- [ ] Test Escape key pause
- [ ] Save scene

### Task 3.6: Create Tutorial 11 Scene (Advanced Networking) [S]
- [ ] Create new scene `Tutorial11/Tutorial11_AdvancedNetwork.unity`
- [ ] Add Main Camera
- [ ] Add Directional Light
- [ ] Create Canvas with Instructions and DiagnosticsText
- [ ] Create NetworkDemo hierarchy:
  - [ ] NetworkDemo (MmRelayNode + T11_LoopbackDemo)
  - [ ] Sender child (MmRelayNode)
  - [ ] Receiver child (MmRelayNode + T11_NetworkResponder)
- [ ] Test D key (diagnostics)
- [ ] Verify loopback messages work
- [ ] Save scene

### Task 3.7: Create Tutorial 12 Scene (VR Experiment) [M]
- [ ] Create new scene `Tutorial12/Tutorial12_VRExperiment.unity`
- [ ] Add Main Camera (or XR Origin if VR testing)
- [ ] Add Directional Light
- [ ] Add EventSystem
- [ ] Create ExperimentRoot hierarchy:
  - [ ] ExperimentRoot (MmRelayNode + T12_GoNoGoController)
  - [ ] StimulusManager child (MmRelayNode)
    - [ ] FixationCross (3D Text "+")
    - [ ] GoStimulus (Green Sphere)
    - [ ] NoGoStimulus (Red Square)
  - [ ] InputManager child (MmRelayNode)
  - [ ] DataManager child (T12_DataLogger)
- [ ] Test Space key response (keyboard fallback)
- [ ] Verify Go/No-Go trials work
- [ ] Verify data logging works
- [ ] Save scene

---

## Phase 4: Test All Scenes ⏳ NOT STARTED

### Task 4.1: Test Tutorial 1 Scene [S]
- [ ] Load Tutorial1_Base.unity
- [ ] Enter Play mode
- [ ] Follow wiki tutorial steps
- [ ] Test Space key (send message)
- [ ] Test I key (initialize)
- [ ] Verify console output matches wiki
- [ ] Check for errors/warnings

### Task 4.2: Test Tutorial 2 Scene [S]
- [ ] Load Tutorial2_Base.unity
- [ ] Enter Play mode
- [ ] Follow wiki tutorial steps
- [ ] Test routing directions (ToChildren vs ToDescendants)
- [ ] Verify console output matches wiki

### Task 4.3: Test Tutorial 3 Scene [S]
- [ ] Load Tutorial3_Base.unity
- [ ] Enter Play mode
- [ ] Test D key (damage)
- [ ] Test C key (color change)
- [ ] Verify health system works
- [ ] Verify console output matches wiki

### Task 4.4: Test Tutorial 4 Scene [S]
- [ ] Load Tutorial4_Base.unity
- [ ] Enter Play mode
- [ ] Test R/G/B keys (light colors)
- [ ] Verify intensity changes
- [ ] Verify console output matches wiki

### Task 4.5: Test Tutorial 5 Scene [S]
- [ ] Load Tutorial5_Base.unity
- [ ] Enter Play mode
- [ ] Test 1-5 keys (DSL demos)
- [ ] Test T key (delayed execution)
- [ ] Test Y key (repeating messages)
- [ ] Verify console output matches wiki

### Task 4.6: Test Tutorial 6 Scene [S]
- [ ] Load Tutorial6_FishNet.unity
- [ ] Enter Play mode
- [ ] Verify fallback warning appears
- [ ] Test S key (server sync)
- [ ] Test Space key (client request)
- [ ] Verify console output matches wiki

### Task 4.7: Test Tutorial 7 Scene [S]
- [ ] Load Tutorial7_Fusion2.unity
- [ ] Enter Play mode
- [ ] Verify fallback warning appears
- [ ] Test controls
- [ ] Verify console output matches wiki

### Task 4.8: Test Tutorial 8 Scene [S]
- [ ] Load Tutorial8_FSM.unity
- [ ] Enter Play mode
- [ ] Test Escape key (pause toggle)
- [ ] Test Enter key (start/restart)
- [ ] Verify FSM state transitions
- [ ] Verify console output matches wiki

### Task 4.9: Test Tutorial 9 Scene [S]
- [ ] Load Tutorial9_Tasks.unity
- [ ] Enter Play mode
- [ ] Verify trial progression
- [ ] Test Space key response
- [ ] Verify data collection
- [ ] Verify console output matches wiki

### Task 4.10: Test Tutorial 10 Scene [S]
- [ ] Load Tutorial10_AppState.unity
- [ ] Enter Play mode
- [ ] Verify splash auto-advances
- [ ] Test UI buttons
- [ ] Test Escape key pause
- [ ] Verify console output matches wiki

### Task 4.11: Test Tutorial 11 Scene [S]
- [ ] Load Tutorial11_AdvancedNetwork.unity
- [ ] Enter Play mode
- [ ] Test D key (diagnostics)
- [ ] Verify loopback messages
- [ ] Verify console output matches wiki

### Task 4.12: Test Tutorial 12 Scene [S]
- [ ] Load Tutorial12_VRExperiment.unity
- [ ] Enter Play mode
- [ ] Test Space key response
- [ ] Verify Go/No-Go trials
- [ ] Verify data logging
- [ ] Verify console output matches wiki

---

## Phase 5: Tutorial Updates ⏳ NOT STARTED

### Task 5.1: Document Testing Issues [S]
- [ ] Create list of discrepancies found
- [ ] Note incorrect keyboard mappings
- [ ] Note incorrect expected output
- [ ] Note missing setup steps

### Task 5.2: Update Wiki Tutorials [M]
- [ ] Fix Tutorial 1 issues (if any)
- [ ] Fix Tutorial 2 issues (if any)
- [ ] Fix Tutorial 3 issues (if any)
- [ ] Fix Tutorial 4 issues (if any)
- [ ] Fix Tutorial 5 issues (if any)
- [ ] Fix Tutorial 6 issues (if any)
- [ ] Fix Tutorial 7 issues (if any)
- [ ] Fix Tutorial 8 issues (if any)
- [ ] Fix Tutorial 9 issues (if any)
- [ ] Fix Tutorial 10 issues (if any)
- [ ] Fix Tutorial 11 issues (if any)
- [ ] Fix Tutorial 12 issues (if any)

### Task 5.3: Final Verification [S]
- [ ] Re-test any tutorials that were updated
- [ ] Verify all scenes still work
- [ ] Confirm all expected output sections accurate

---

## Summary

| Phase | Status | Progress |
|-------|--------|----------|
| Phase 1: Doxygen Enhancement | ⏳ NOT STARTED | 0/3 tasks |
| Phase 2: Scene Audit | ⏳ NOT STARTED | 0/7 tasks |
| Phase 3: Scene Creation | ⏳ NOT STARTED | 0/7 tasks |
| Phase 4: Testing | ⏳ NOT STARTED | 0/12 tasks |
| Phase 5: Tutorial Updates | ⏳ NOT STARTED | 0/3 tasks |
| **TOTAL** | | **0/32 tasks** |
