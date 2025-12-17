# Doxygen Enhancement & Tutorial Scene Implementation - Context

**Last Updated:** 2025-12-17
**Status:** Ready for Implementation

---

## SESSION PROGRESS (2025-12-17)

### ✅ COMPLETED
- Exploration of current documentation state
- Exploration of existing tutorial scenes and scripts
- Exploration of wiki tutorial requirements (3-12)
- Plan created with all phases defined
- User decisions captured:
  - Enable Graphviz diagrams: YES
  - Delete SimpleScene folders: YES (after extraction)
  - Network fallback: MmLoopbackBackend simulation
  - Skip Tutorials 13-14: YES (focus on 1-12)

### 🟡 IN PROGRESS
- None (ready to start implementation)

### ⏳ NOT STARTED
- Phase 1: Doxygen Enhancement
- Phase 2: Scene Audit
- Phase 3: Scene Creation (6-12)
- Phase 4: Testing
- Phase 5: Tutorial Updates

### ⚠️ BLOCKERS
- None identified

---

## Key Files

### Doxyfile
**Path:** `Doxyfile` (project root)
**Purpose:** Doxygen configuration for API documentation
**Status:** Needs enhancement (Graphviz disabled, missing settings)
**Changes Needed:**
- Enable HAVE_DOT = YES
- Add CLASS_DIAGRAMS, COLLABORATION_GRAPH, etc.
- Enable WARN_IF_UNDOCUMENTED

### Documentation Output
**Path:** `docs/html/`
**Purpose:** Generated Doxygen HTML documentation
**Status:** Exists but needs regeneration after Doxyfile changes

### Existing Tutorial Scenes (Need Audit)
| File | Status |
|------|--------|
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial1/Tutorial1_Base.unity` | Needs XR cleanup |
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial2/Tutorial2_Base.unity` | Needs audit |
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial3/Tutorial3_Base.unity` | Needs audit |
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial4/Tutorial4_Base.unity` | Needs audit |
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial5/Tutorial5_Base.unity` | Needs audit |

### Scenes to Create
| File | Scripts Available |
|------|-------------------|
| `Tutorial6/Tutorial6_FishNet.unity` | T6_NetworkGameController.cs, T6_PlayerResponder.cs, T6_NetworkSetup.cs |
| `Tutorial7/Tutorial7_Fusion2.unity` | T7_Fusion2GameController.cs, T7_Fusion2PlayerResponder.cs, T7_Fusion2Setup.cs |
| `Tutorial8/Tutorial8_FSM.unity` | T8_GameStateController.cs, T8_MenuResponder.cs, T8_GameplayResponder.cs, T8_PauseResponder.cs, T8_GameOverResponder.cs |
| `Tutorial9/Tutorial9_Tasks.unity` | T9_MyTaskInfo.cs, T9_JsonTaskLoader.cs, T9_ExperimentManager.cs, T9_TrialResponder.cs |
| `Tutorial10/Tutorial10_AppState.unity` | T10_MyAppController.cs, T10_LoadingBehavior.cs, T10_MenuBehavior.cs, T10_GameplayBehavior.cs, T10_PauseBehavior.cs |
| `Tutorial11/Tutorial11_AdvancedNetwork.unity` | T11_LoopbackDemo.cs, T11_NetworkResponder.cs |
| `Tutorial12/Tutorial12_VRExperiment.unity` | T12_GoNoGoController.cs, T12_DataLogger.cs |

### Folders to Delete
| Path | Content |
|------|---------|
| `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/` | 3 scenes, 11 scripts (preserve InvocationComparison.cs) |
| `Assets/MercuryMessaging/Examples/Tutorials/SimpleTutorial_Alternative/` | 1 scene, 3 scripts |

### Wiki Tutorial Drafts
**Path:** `dev/wiki-drafts/tutorials/`
**Files:** tutorial-01-introduction.md through tutorial-14-performance.md
**Purpose:** Reference for scene requirements and expected behavior

---

## Important Decisions

### 1. Graphviz Diagrams
**Decision:** Enable Graphviz diagrams
**Rationale:** Provides visual class hierarchy and collaboration diagrams
**Impact:** Requires Graphviz installation on build machines

### 2. Network Fallback Mode
**Decision:** Use MmLoopbackBackend for local simulation
**Rationale:** Allows testing without FishNet/Fusion installed
**Impact:** All network features work locally; shows warning in console

### 3. SimpleScene Cleanup
**Decision:** Delete after extracting InvocationComparison.cs
**Rationale:** Redundant content, tutorials 1-12 provide better coverage
**Impact:** Must preserve InvocationComparison.cs in Tests/Performance/

### 4. Tutorials 13-14
**Decision:** Skip for now
**Rationale:** Listed as "Coming Soon" in wiki, focus on 1-12
**Impact:** No scenes needed for spatial/temporal filtering or performance tutorials

---

## Technical Constraints

### Graphviz Requirement
- Must have Graphviz installed: `where dot` should return path
- If not installed: `winget install graphviz` or download from graphviz.org

### Scene Requirements
- All scenes must work without external networking packages
- Use `#if FISHNET_AVAILABLE` / `#if FUSION2_AVAILABLE` for conditional compilation
- MmLoopbackBackend provides fallback for network testing

### XR Cleanup Rules
- Remove XR Origin/XR Rig from all scenes except Tutorial 12
- Tutorial 12 should have keyboard fallback (Space key)
- Remove Graph Controller, EPOOutline, debug visualization objects

---

## Quick Resume Instructions

To continue this task:

1. **Read this file** for current state
2. **Check tasks file** for what's done and what's next
3. **Start with Phase 1** if not completed:
   - Enhance Doxyfile
   - Generate documentation
4. **Then Phase 2**:
   - Open each Tutorial 1-5 scene in Unity
   - Remove XR/debug clutter
   - Delete SimpleScene folders
5. **Then Phase 3**:
   - Create Tutorial 6-12 scenes using existing scripts
6. **Then Phase 4-5**:
   - Test all scenes against wiki tutorials
   - Update tutorials as needed

---

## Related Files

- **Plan:** `dev/active/doxygen-tutorial-scenes/doxygen-tutorial-scenes-plan.md`
- **Tasks:** `dev/active/doxygen-tutorial-scenes/doxygen-tutorial-scenes-tasks.md`
- **Release Plan:** `~/.claude/plans/sharded-pondering-kurzweil.md` (MercuryMessaging 4.0.0 Release Plan)
- **Wiki Drafts:** `dev/wiki-drafts/tutorials/`
- **Contributing Guide:** `CONTRIBUTING.md`
