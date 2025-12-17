# MercuryMessaging 4.0.0 Release - Task Checklist

**Last Updated:** 2025-12-17 (Session 4)
**Status:** PACKAGE TESTED ✅ - Phase 7 (GitHub Release) Remains

---

## Quick Resume

**Current Phase:** Phase 6 Complete, Phase 7 Remains
**Next Action:** Create git tag, GitHub Release, upload .unitypackage

---

## Phase 1: Wiki Enhancement & Publishing ✅ COMPLETE

### Task 1.1: Clone Wiki Repository [S]
- [x] Wiki repo exists at `/mnt/c/Users/yangb/Research/MercuryMessaging.wiki`
- [x] Credential helper configured for WSL push

### Task 1.2-1.4: Tutorial Enhancements ✅ COMPLETE
- [x] All 12 tutorials published to wiki
- [x] Old tutorials deleted (different naming convention)
- [x] New tutorials with cleaner naming (e.g., `Tutorial-1:-Introduction.md`)

### Task 1.5: Create Wiki Navigation [S]
- [x] `_Sidebar.md` generated and published
- [x] `Home.md` updated with v4.0.0 content

### Task 1.6: Publish Wiki [S]
- [x] Fixed `publish-wiki.sh` line endings (CRLF → LF)
- [x] Fixed file name mappings in script
- [x] Push successful: `28ce62f..039d282`
- [x] Live at: https://github.com/ColumbiaCGUI/MercuryMessaging/wiki

### Task 1.7: Wiki Fixes (Session 3) ✅ COMPLETE
- [x] Added `Getting-Started.md` (fixes red link)
- [x] Added `Troubleshooting.md`
- [x] Added Tutorial 13 & 14 stubs
- [x] Updated `_Sidebar.md` (removed API Reference, Performance, Contributing links)
- [x] Updated `Home.md` with Unity compatibility table
- [x] Push successful: `039d282..19f0489`

---

## Phase 2: Doxygen & API Documentation ✅ COMPLETE

### Task 2.1: Enable Graphviz [S]
- [x] Graphviz available (Maya's bundled version)
- [x] Doxyfile updated: `HAVE_DOT = YES`
- [x] DOT_PATH set: `"C:/Program Files/Autodesk/Maya2026/bin/graphviz"`

### Task 2.2: Fix Stale References [S]
- [x] Doxyfile configuration verified
- [x] Documentation structure clean

### Task 2.3: Generate Documentation [S]
- [x] Run `doxygen Doxyfile` - COMPLETE
- [x] HTML generated in `docs/html/`
- [x] Version shows 4.0.0
- [x] Class diagrams generated

---

## Phase 3: Tutorial Scene Creation ✅ COMPLETE

### Task 3.1: Create Tutorial 6 Scene [M]
- [x] Create Tutorial6_FishNet.unity ✅
- [x] Add T6_* scripts ✅
- [x] Tested - runs without errors ✅

### Task 3.2: Create Tutorial 7 Scene [M]
- [x] Create Tutorial7_Fusion2.unity ✅
- [x] Add T7_* scripts ✅
- [x] Tested - runs without errors ✅

### Task 3.3: Create Tutorial 8 Scene [M]
- [x] Create Tutorial8_FSM.unity ✅
- [x] Add T8_* scripts ✅
- [x] Tested - runs without errors ✅

### Task 3.4: Create Tutorial 9 Scene [M]
- [x] Create Tutorial9_Tasks.unity ✅
- [x] Add T9_* scripts ✅
- [x] Tested - needs task collection config (expected) ⚠️

### Task 3.5: Create Tutorial 10 Scene [M]
- [x] Create Tutorial10_AppState.unity ✅
- [x] Add T10_* scripts ✅
- [x] Tested - needs FSM setup (expected) ⚠️

### Task 3.6: Create Tutorial 11 Scene [S]
- [x] Create Tutorial11_AdvancedNetwork.unity ✅
- [x] Add T11_* scripts ✅
- [x] Tested - runs without errors ✅

### Task 3.7: Create Tutorial 12 Scene [M]
- [x] Create Tutorial12_VRExperiment.unity ✅
- [x] Add T12_* scripts ✅
- [x] Hierarchy: ExperimentRoot > StimulusManager, InputManager, DataManager
- [x] Stimuli: GoStimulus (Sphere), NoGoStimulus (Cube), FixationCross
- [x] Tested - runs without errors ✅

---

## Phase 4: SimpleScene Integration ✅ COMPLETE

### Task 4.1: Extract Useful Content [S]
- [x] InvocationComparison.cs preserved in Tests/Performance/Scripts/
- [x] Other content covered by tutorials

### Task 4.2: Delete Redundant Folders [S]
- [x] Deleted Examples/Tutorials/SimpleScene/
- [x] Deleted Examples/Tutorials/SimpleTutorial_Alternative/
- [x] Project compiles
- [x] No broken references

---

## Phase 5: Scene Audit & Quality ✅ COMPLETE

### Task 5.1: Audit Tutorial 1-5 Scenes [M]
- [x] Tutorial 1-5: Already clean, no XR clutter found
- [x] All scenes verified functional

### Task 5.2: Verify All Scenes Load [S]
- [x] All 12 scenes tested in Play mode
- [x] Results: 10 pass, 2 need configuration (expected)
- [x] No missing scripts

---

## Phase 6: Unity Package Creation ⏳ NOT STARTED

### Task 6.1: Clean Project [S]
- [x] Remove non-essential files (SimpleScene deleted in Phase 4)
- [x] Verify all tutorial scenes load (Phase 5)
- [x] Check no external dependencies - FIXED:
  - `FishNetTestManager.cs` wrapped in `#if FISHNET_AVAILABLE`
  - `FishNetBackend.cs` and `Fusion2Backend.cs` have `#pragma warning disable 0067`
  - Package imports cleanly without FishNet/Fusion2

### Task 6.2: Export Package [S]
- [x] Select Assets/MercuryMessaging/ folder
- [x] Assets > Export Package
- [x] Name: MercuryMessaging-4.0.0.unitypackage
- [x] Deleted Tests/Network/Scenes/ (contained FishNet/Fusion2 hard references)

### Task 6.3: Test Import [S]
- [x] Tested on Unity 6.3.1f1 LTS - ✅ All tests pass
- [x] Tested on Unity 6.0.62f1 LTS - ✅ All tests pass
- [x] No compilation errors
- [x] No missing script references

---

## Phase 7: GitHub Repository Update ⏳ NOT STARTED

### Task 7.1: Update README.md [S]
- [ ] Update version badge to 4.0.0
- [ ] Add "What's New in 4.0.0" section
- [ ] Update tutorial list with wiki links
- [ ] Review and proofread

### Task 7.2: Create Git Tag [S]
- [ ] `git tag v4.0.0`
- [ ] `git push origin v4.0.0`

### Task 7.3: Create GitHub Release [S]
- [ ] Go to GitHub Releases
- [ ] Create new release from v4.0.0 tag
- [ ] Write release notes (draft in context.md)
- [ ] Upload MercuryMessaging-4.0.0.unitypackage
- [ ] Publish release

---

## Summary

| Phase | Status | Progress |
|-------|--------|----------|
| Phase 1: Wiki Enhancement | ✅ COMPLETE | 6/6 tasks |
| Phase 2: Doxygen & Docs | ✅ COMPLETE | 3/3 tasks |
| Phase 3: Scene Creation | ✅ COMPLETE | 7/7 tasks |
| Phase 4: SimpleScene Integration | ✅ COMPLETE | 2/2 tasks |
| Phase 5: Scene Audit | ✅ COMPLETE | 2/2 tasks |
| Phase 6: Package Creation | ✅ COMPLETE | 3/3 tasks |
| Phase 7: GitHub Release | ⏳ NOT STARTED | 0/3 tasks |
| **TOTAL** | | **23/26 tasks (Phase 7 remains)** |

---

## Notes

- Wiki live at: https://github.com/ColumbiaCGUI/MercuryMessaging/wiki
- Tutorial 9 & 10 need Inspector configuration (intentional - demonstrates system)
- Package testing requires fresh Unity project
- WSL credential helper: `git config credential.helper "/mnt/c/Program\ Files/Git/mingw64/bin/git-credential-manager.exe"`

### Installing Optional Network Packages
After importing MercuryMessaging, developers can optionally install:
- **FishNet**: Install via Unity Package Manager from Asset Store or Git URL
- **Photon Fusion 2**: Install via Photon SDK website

The `FISHNET_AVAILABLE` and `FUSION2_AVAILABLE` defines are automatically set by the asmdef `versionDefines` when these packages are detected.
