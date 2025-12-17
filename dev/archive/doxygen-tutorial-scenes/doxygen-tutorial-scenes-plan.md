# Doxygen Enhancement & Tutorial Scene Implementation Plan

**Last Updated:** 2025-12-17
**Status:** Ready for Implementation
**Estimated Effort:** 9-15 hours

---

## Executive Summary

Enhance the MercuryMessaging API documentation generation and create complete, testable tutorial scenes for all 12 tutorials. This involves:

1. **Doxygen Enhancement**: Configure professional API documentation with Graphviz class diagrams
2. **Scene Audit**: Clean existing tutorial scenes (1-5) of XR/debug clutter
3. **Scene Creation**: Build 7 new scenes for tutorials 6-12 with proper fallback modes
4. **Scene Testing**: Verify all scenes work correctly against wiki tutorial content
5. **Tutorial Updates**: Fix any issues discovered during testing

---

## Current State Analysis

### Documentation Status
- **Doxyfile**: Already configured for v4.0.0, properly set up for C#
- **DSL Docs**: `Documentation/DSL/README.md` and `API_GUIDE.md` already exist and are excellent
- **Doxygen Output**: Exists in `docs/html/` but may need regeneration
- **XML Comments**: Excellent quality in core files (MmRelayNode, MmFluentMessage)
- **Graphviz**: Currently DISABLED (HAVE_DOT = NO)

### Tutorial Scene Status
| Tutorial | Scene Exists | Scripts Exist | Status |
|----------|-------------|---------------|--------|
| 1-5 | Yes | Yes | Need XR cleanup audit |
| 6 (FishNet) | **NO** | Yes (3) | Create with fallback |
| 7 (Fusion 2) | **NO** | Yes (3) | Create with fallback |
| 8 (FSM) | **NO** | Yes (5) | Create scene |
| 9 (Tasks) | **NO** | Yes (4) | Create scene |
| 10 (AppState) | **NO** | Yes (5) | Create scene |
| 11 (Advanced Network) | **NO** | Yes (2) | Create scene |
| 12 (VR Experiment) | **NO** | Yes (2) | Create with VR fallback |

### Redundant Content to Delete
- `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/` - 3 scenes, 11 scripts
- `Assets/MercuryMessaging/Examples/Tutorials/SimpleTutorial_Alternative/` - 1 scene, 3 scripts

---

## Proposed Future State

### Documentation
- Professional Doxygen output with class hierarchy diagrams
- SVG format for scalable diagrams
- All cross-references enabled
- Comprehensive class documentation

### Tutorial Scenes
- **12 complete, testable scenes** matching wiki tutorials
- No XR/debug clutter in scenes 1-5
- Networking tutorials (6, 7, 11) use MmLoopbackBackend fallback
- VR tutorial (12) has keyboard fallback for non-VR testing
- SimpleScene and SimpleTutorial_Alternative folders deleted

---

## Implementation Phases

### Phase 1: Doxygen Enhancement (1-2 hours)

#### Task 1.1: Enhance Doxyfile Configuration
**Effort:** S (30 min)
**File:** `Doxyfile`

**Changes:**
```
# Better extraction
EXTRACT_PRIVATE = YES
EXTRACT_PACKAGE = YES
INLINE_INHERITED_MEMB = YES

# Better HTML
HTML_COLORSTYLE = AUTO_LIGHT
HTML_DYNAMIC_MENUS = YES

# Better quality
WARN_IF_UNDOCUMENTED = YES
WARN_NO_PARAMDOC = YES

# Enable Graphviz
HAVE_DOT = YES
CLASS_DIAGRAMS = YES
COLLABORATION_GRAPH = YES
GROUP_GRAPHS = YES
INCLUDE_GRAPH = YES
INCLUDED_BY_GRAPH = YES
GRAPHICAL_HIERARCHY = YES
DIRECTORY_GRAPH = YES
DOT_IMAGE_FORMAT = svg
DOT_TRANSPARENT = YES
```

**Acceptance Criteria:**
- [ ] Doxyfile updated with all settings
- [ ] Graphviz path verified (`where dot`)

#### Task 1.2: Fix Stale Documentation References
**Effort:** S (15 min)
**Files:**
- `Documentation/WORKFLOWS.md` - Remove reference to non-existent `Documentation/Tutorials/DSL_QUICK_START.md`

**Acceptance Criteria:**
- [ ] All cross-references valid
- [ ] No dead links in documentation

#### Task 1.3: Generate Doxygen Documentation
**Effort:** S (15 min)
**Command:** `doxygen Doxyfile`

**Acceptance Criteria:**
- [ ] HTML generated in `docs/html/`
- [ ] Version shows 4.0.0
- [ ] Class diagrams generated (SVG)
- [ ] Search functionality works
- [ ] No generation errors

---

### Phase 2: Audit Existing Tutorial Scenes (1-2 hours)

#### Task 2.1: Audit and Clean Tutorial 1-5 Scenes
**Effort:** M (1 hour)
**Files:**
- `Assets/MercuryMessaging/Examples/Tutorials/Tutorial1/Tutorial1_Base.unity`
- `Assets/MercuryMessaging/Examples/Tutorials/Tutorial2/Tutorial2_Base.unity`
- `Assets/MercuryMessaging/Examples/Tutorials/Tutorial3/Tutorial3_Base.unity`
- `Assets/MercuryMessaging/Examples/Tutorials/Tutorial4/Tutorial4_Base.unity`
- `Assets/MercuryMessaging/Examples/Tutorials/Tutorial5/Tutorial5_Base.unity`

**Remove:**
- XR Origin / XR Rig GameObjects
- Graph Controller / DebugGraph objects
- EPOOutline / visualization helpers
- Unused cameras or lights
- Test/Debug GameObjects
- Missing script references

**Acceptance Criteria:**
- [ ] Each scene loads without errors
- [ ] No XR components (except Tutorial 12)
- [ ] No debug visualization objects
- [ ] Clean hierarchy matching wiki tutorial

#### Task 2.2: Extract Useful Content from SimpleScene
**Effort:** S (30 min)
**Source:** `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/`

**Preserve:**
- `InvocationComparison.cs` → Move to `Tests/Performance/`

**Acceptance Criteria:**
- [ ] InvocationComparison.cs preserved in Tests/Performance/
- [ ] All other content documented for reference

#### Task 2.3: Delete Redundant Folders
**Effort:** S (15 min)

**Delete:**
- `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/` (entire folder)
- `Assets/MercuryMessaging/Examples/Tutorials/SimpleTutorial_Alternative/` (entire folder)

**Acceptance Criteria:**
- [ ] Both folders deleted
- [ ] No broken references in project
- [ ] Project compiles successfully

---

### Phase 3: Create Missing Tutorial Scenes (4-6 hours)

#### Task 3.1: Create Tutorial 6 Scene (FishNet)
**Effort:** M (45 min)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial6/Tutorial6_FishNet.unity`

**Hierarchy:**
```
Tutorial6_FishNet
├── Main Camera
├── Directional Light
├── Canvas (Instructions)
└── NetworkRoot (MmRelayNode + T6_NetworkGameController)
      ├── Player (MmRelayNode + T6_PlayerResponder + Cube)
      ├── Enemy (MmRelayNode + Cube)
      └── ScoreUI (Text)
```

**Fallback:** MmLoopbackBackend when FishNet not installed

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Fallback mode works without FishNet
- [ ] S key syncs (server), Space requests (client)
- [ ] Console output matches wiki

#### Task 3.2: Create Tutorial 7 Scene (Fusion 2)
**Effort:** M (45 min)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial7/Tutorial7_Fusion2.unity`

**Hierarchy:**
```
Tutorial7_Fusion2
├── Main Camera
├── Directional Light
├── Canvas (Instructions)
└── GameRoot (MmRelayNode + T7_Fusion2GameController)
      ├── Player1 (MmRelayNode + T7_Fusion2PlayerResponder + Sphere)
      └── Player2 (MmRelayNode + Sphere)
```

**Fallback:** MmLoopbackBackend when Fusion 2 not installed

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Fallback mode works without Fusion 2
- [ ] Controls work as documented

#### Task 3.3: Create Tutorial 8 Scene (FSM)
**Effort:** M (1 hour)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial8/Tutorial8_FSM.unity`

**Hierarchy:**
```
Tutorial8_FSM
├── Main Camera
├── Directional Light
├── EventSystem
└── GameManager (MmRelaySwitchNode + T8_GameStateController)
      ├── MainMenu (MmRelayNode + T8_MenuResponder)
      │   └── MenuCanvas (StartButton)
      ├── Gameplay (MmRelayNode + T8_GameplayResponder)
      │   ├── Player (Cube)
      │   └── ScoreText
      ├── PauseMenu (MmRelayNode + T8_PauseResponder)
      │   └── PauseCanvas (ResumeButton)
      └── GameOver (MmRelayNode + T8_GameOverResponder)
          └── GameOverCanvas (RestartButton)
```

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Escape toggles pause
- [ ] Enter starts/restarts game
- [ ] FSM state transitions work correctly

#### Task 3.4: Create Tutorial 9 Scene (Task Management)
**Effort:** M (1 hour)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial9/Tutorial9_Tasks.unity`

**Hierarchy:**
```
Tutorial9_Tasks
├── Main Camera
├── Directional Light
├── EventSystem
└── Experiment (MmRelayNode)
      ├── TaskManager (T9_ExperimentManager)
      │   └── TasksNode (MmRelaySwitchNode)
      │       ├── Instructions (MmRelayNode)
      │       ├── Practice (MmRelayNode + T9_TrialResponder)
      │       ├── MainTrials (MmRelayNode + T9_TrialResponder)
      │       └── Complete (MmRelayNode)
      ├── StimulusDisplay (MmRelayNode)
      │   ├── FixationCross (Text "+")
      │   └── Stimulus (Colored square)
      └── DataCollector (MmDataCollector)
```

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Trials progress correctly
- [ ] Space responds to stimulus
- [ ] Data collection works

#### Task 3.5: Create Tutorial 10 Scene (Application State)
**Effort:** M (1 hour)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial10/Tutorial10_AppState.unity`

**Hierarchy:**
```
Tutorial10_AppState
├── Main Camera
├── Directional Light
├── EventSystem
└── App (MmRelaySwitchNode + T10_MyAppController)
      ├── Splash (MmRelayNode + T10_LoadingBehavior)
      │   └── SplashCanvas
      ├── MainMenu (MmRelayNode + T10_MenuBehavior)
      │   └── MenuCanvas (Play/Options/Quit buttons)
      ├── Options (MmRelayNode)
      │   └── OptionsCanvas (BackButton)
      ├── Loading (MmRelayNode + T10_LoadingBehavior)
      │   └── LoadingCanvas (ProgressBar)
      ├── Gameplay (MmRelayNode + T10_GameplayBehavior)
      │   ├── Player (Cube)
      │   └── GameplayHUD
      └── Pause (MmRelayNode + T10_PauseBehavior)
          └── PauseCanvas (ResumeButton)
```

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Splash auto-advances
- [ ] UI buttons work
- [ ] Escape toggles pause

#### Task 3.6: Create Tutorial 11 Scene (Advanced Networking)
**Effort:** S (30 min)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial11/Tutorial11_AdvancedNetwork.unity`

**Hierarchy:**
```
Tutorial11_AdvancedNetwork
├── Main Camera
├── Directional Light
├── Canvas (Instructions + DiagnosticsText)
└── NetworkDemo (MmRelayNode + T11_LoopbackDemo)
      ├── Sender (MmRelayNode)
      └── Receiver (MmRelayNode + T11_NetworkResponder)
```

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] D key shows diagnostics
- [ ] Loopback messages work

#### Task 3.7: Create Tutorial 12 Scene (VR Experiment)
**Effort:** M (1 hour)
**Scene:** `Assets/MercuryMessaging/Examples/Tutorials/Tutorial12/Tutorial12_VRExperiment.unity`

**Hierarchy:**
```
Tutorial12_VRExperiment
├── Main Camera (or XR Origin for VR)
├── Directional Light
├── EventSystem
└── ExperimentRoot (MmRelayNode + T12_GoNoGoController)
      ├── StimulusManager (MmRelayNode)
      │   ├── FixationCross (3D Text "+")
      │   ├── GoStimulus (Green Sphere)
      │   └── NoGoStimulus (Red Square)
      ├── InputManager (MmRelayNode)
      └── DataManager (T12_DataLogger)
```

**Fallback:** Space key for non-VR testing

**Acceptance Criteria:**
- [ ] Scene loads without errors
- [ ] Space key works as response input
- [ ] Go/No-Go trials run correctly
- [ ] Data logging works

---

### Phase 4: Test All Scenes (2-3 hours)

#### Task 4.1: Test Tutorial 1-5 Scenes
**Effort:** M (1 hour)

For each scene (1-5):
1. Load scene in Unity Editor
2. Enter Play mode
3. Follow wiki tutorial steps exactly
4. Verify console output matches
5. Test all keyboard interactions
6. Check for errors/warnings

**Acceptance Criteria:**
- [ ] All 5 scenes pass testing
- [ ] No console errors
- [ ] Output matches wiki tutorials

#### Task 4.2: Test Tutorial 6-12 Scenes
**Effort:** M (1.5 hours)

For each scene (6-12):
1. Load scene in Unity Editor
2. Enter Play mode
3. Follow wiki tutorial steps exactly
4. Verify fallback modes work (networking)
5. Test all keyboard interactions
6. Check for errors/warnings

**Acceptance Criteria:**
- [ ] All 7 scenes pass testing
- [ ] Fallback modes work correctly
- [ ] No console errors
- [ ] Output matches wiki tutorials

---

### Phase 5: Tutorial Updates (1-2 hours)

#### Task 5.1: Update Wiki Tutorials Based on Testing
**Effort:** M (1 hour)
**Files:** `dev/wiki-drafts/tutorials/tutorial-*.md`

**Potential Updates:**
- Fix incorrect keyboard mappings
- Update expected console output
- Add missing setup steps
- Correct hierarchy descriptions
- Add troubleshooting tips

**Acceptance Criteria:**
- [ ] All tutorials verified against working scenes
- [ ] Expected output sections accurate
- [ ] Keyboard controls documented correctly

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Graphviz not installed | Medium | Low | Instructions to install or disable diagrams |
| FishNet/Fusion scripts have compile errors | Medium | Medium | Use #if preprocessor guards |
| Scene hierarchy differs from tutorial | Medium | Medium | Update tutorials to match scenes |
| XR cleanup breaks functionality | Low | Medium | Test thoroughly after cleanup |

---

## Success Metrics

1. **Doxygen Documentation**
   - Version shows 4.0.0
   - Class diagrams generated
   - Search works
   - No broken links

2. **Tutorial Scenes**
   - All 12 scenes load without errors
   - All scenes match wiki tutorials
   - All fallback modes work
   - No XR/debug clutter

3. **Testing**
   - 100% of tutorials can be completed using the scenes
   - Console output matches expected output
   - All keyboard controls work

---

## Dependencies

- **Graphviz**: Required for class diagrams (install separately)
- **Unity 2021.3+**: Required for scene editing
- **No FishNet/Fusion**: Scenes must work without these packages

---

## Timeline Estimates

| Phase | Tasks | Estimate |
|-------|-------|----------|
| Phase 1 | Doxygen Enhancement | 1-2 hours |
| Phase 2 | Scene Audit | 1-2 hours |
| Phase 3 | Scene Creation | 4-6 hours |
| Phase 4 | Testing | 2-3 hours |
| Phase 5 | Tutorial Updates | 1-2 hours |
| **Total** | | **9-15 hours** |
