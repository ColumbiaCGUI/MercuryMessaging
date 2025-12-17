# MercuryMessaging 4.0.0 Release Plan

**Last Updated:** 2025-12-17
**Status:** Ready for Implementation
**Estimated Effort:** 46-66 hours

---

## Executive Summary

This plan covers the complete release process for MercuryMessaging 4.0.0, including:

1. **Wiki Enhancement & Publishing** - Enhance tutorials with best practices, publish to GitHub Wiki
2. **Doxygen & API Documentation** - Update version, fix stale refs, regenerate docs
3. **Tutorial Scene Creation** - Build 7 new scenes for tutorials 6-12
4. **SimpleScene Integration** - Merge useful content, delete redundant folders
5. **Scene Audit & Quality** - Clean all scenes of XR/debug clutter
6. **Unity Package Creation** - Export MercuryMessaging-4.0.0.unitypackage
7. **GitHub Release** - Update README, tag v4.0.0, create release

---

## Current State Analysis

### What's Ready
- Wiki draft tutorials exist: `dev/wiki-drafts/tutorials/` (14 files)
- Tutorial scripts exist for tutorials 6-12 (all have README + scripts)
- DSL documentation exists and is excellent
- Doxyfile configured for v4.0.0
- Core XML documentation is excellent

### What Needs Work
- Wiki drafts need enhancement (Expected Output, Try This exercises)
- Tutorial scenes 6-12 don't exist (only scripts + READMEs)
- Existing scenes 1-5 need XR/debug cleanup
- SimpleScene/SimpleTutorial_Alternative folders are redundant
- Doxygen needs Graphviz enabled

---

## Implementation Phases

### Phase 1: Wiki Enhancement & Publishing (4-6 hours)

**Enhancements Needed:**
1. Add "Expected Output" after each code step
2. Add "Try This" exercises at end of tutorials
3. Minimize explanation in tutorials (link to Reference instead)
4. Add visual diagrams where helpful
5. Create Quick Start page (2-minute getting started)

**Publishing Tasks:**
1. Clone wiki: `git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git`
2. Enhance tutorials with best practices
3. Copy and rename to wiki format (Tutorial-1:-Introduction.md, etc.)
4. Update internal links
5. Create `_Sidebar.md` for navigation
6. Create `Quick-Start.md`
7. Push to wiki repository

**Source Files:** `dev/wiki-drafts/tutorials/` (14 files), `dev/wiki-drafts/pages/` (3 files)

---

### Phase 2: Doxygen & API Documentation (6-8 hours)

**Tasks:**
1. Update Doxyfile version (already 4.0.0)
2. Enable Graphviz diagrams
3. Fix stale path references:
   - `Documentation/Testing/PERFORMANCE_TESTING.md` (13 refs)
   - `Documentation/PERFORMANCE.md` (1 ref)
   - `Documentation/Performance/PERFORMANCE_REPORT.md` (1 ref)
4. Create missing files if needed
5. Regenerate Doxygen HTML
6. Verify generated documentation

---

### Phase 3: Scene Creation for Tutorials 6-12 (16-24 hours)

| Tutorial | Scene | Core Demo | Fallback |
|----------|-------|-----------|----------|
| 6 | Tutorial6_FishNet.unity | Network sync | MmLoopbackBackend |
| 7 | Tutorial7_Fusion2.unity | Fusion RPC | MmLoopbackBackend |
| 8 | Tutorial8_FSM.unity | State machine | Keyboard controls |
| 9 | Tutorial9_Tasks.unity | Sequential trials | JSON task loading |
| 10 | Tutorial10_AppState.unity | App state + history | State stack demo |
| 11 | Tutorial11_AdvancedNetwork.unity | Loopback backend | Built-in demo |
| 12 | Tutorial12_VRExperiment.unity | Go/No-Go task | Keyboard fallback |

**Scripts Available:** All tutorials have T*_ prefixed scripts ready to use.

---

### Phase 4: SimpleScene Content Integration (4-6 hours)

**Integration Plan:**
- Light switch demo → Already covered in Tutorial 1
- InvocationComparison → Move to Tests/Performance/
- HandController → Extract for Tutorial 12
- LightGUI scripts → Already covered in Tutorial 1

**Cleanup:**
- DELETE `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/`
- DELETE `Assets/MercuryMessaging/Examples/Tutorials/SimpleTutorial_Alternative/`

---

### Phase 5: Scene Audit & Quality Checklist (4-6 hours)

**Items to Remove from ALL Scenes:**
- XR Origin / XR Rig GameObjects (except Tutorial 12)
- Graph Controller / DebugGraph objects
- EPOOutline / visualization helpers
- Unused cameras or lights
- Test/Debug GameObjects
- Missing script references

**Quality Checklist (every scene must have):**
- Descriptive name matching tutorial number
- Clean hierarchy (no orphaned objects)
- No XR clutter (except Tutorial 12)
- Simple visuals (cubes, spheres, basic UI)
- All MmRelayNode components properly configured
- Instructions canvas or README object
- Keyboard controls documented

---

### Phase 6: Unity Package Creation (4-6 hours)

**Package Contents (Include):**
```
Assets/MercuryMessaging/
  ├── Protocol/           # Core framework
  ├── AppState/           # State management
  ├── Support/            # Utilities
  ├── Task/               # Task system
  ├── StandardLibrary/    # UI/Input messages
  ├── Examples/Tutorials/ # All 12 tutorial scenes
  ├── Tests/              # Test suite
  ├── Editor/             # Editor tools
  └── MercuryMessaging.asmdef
```

**Exclude from Package:**
- `/dev/` folder
- Large asset store packages
- Platform/XR/ controller art
- Project-specific content

**Export Steps:**
1. Clean project of non-essential files
2. Verify all tutorial scenes load
3. Export via Assets > Export Package
4. Name: `MercuryMessaging-4.0.0.unitypackage`
5. Test import in fresh Unity 2021.3+ project

---

### Phase 7: GitHub Repository Update (2-3 hours)

**README.md Updates:**
- Update version badge to 4.0.0
- Add "What's New in 4.0.0" section:
  - Fluent DSL API (77% code reduction)
  - MmExtendableResponder
  - FishNet integration
  - Standard Library (UI/Input messages)
  - Performance optimizations (2-35x)
- Update tutorial list with wiki links

**Create GitHub Release:**
1. Tag: `git tag v4.0.0`
2. Push: `git push origin v4.0.0`
3. Create release on GitHub
4. Upload package file
5. Write release notes

**Release Notes Template:**
```markdown
## MercuryMessaging 4.0.0

### Highlights
- **Fluent DSL API**: 77% code reduction
- **12 Tutorial Scenes**: Complete runnable examples
- **FishNet Integration**: Production-ready networking
- **Performance**: 2-2.2x frame time improvement

### Breaking Changes
- Removed Photon PUN2 backend (use FishNet or Fusion 2)
- Namespace consolidation (most code needs only `using MercuryMessaging;`)
```

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Wiki publishing access | Low | High | Verify GitHub wiki permissions first |
| FishNet/Fusion compile errors | Medium | Medium | Use #if preprocessor guards |
| Package import failures | Medium | High | Test on fresh Unity project |
| Scene cleanup breaks functionality | Low | Medium | Test thoroughly after cleanup |

---

## Success Metrics

1. **Wiki Published**
   - All 14 tutorials accessible on GitHub Wiki
   - Navigation sidebar working
   - Quick Start page available

2. **Documentation Complete**
   - Doxygen shows v4.0.0
   - Class diagrams generated
   - No broken links

3. **Tutorial Scenes**
   - All 12 scenes load without errors
   - All fallback modes work
   - No XR/debug clutter

4. **Package Release**
   - Clean import on fresh project
   - All tutorials runnable
   - v4.0.0 tag created

---

## Timeline Estimates

| Phase | Tasks | Estimate |
|-------|-------|----------|
| Phase 1 | Wiki Enhancement | 4-6 hours |
| Phase 2 | Doxygen & Docs | 6-8 hours |
| Phase 3 | Scene Creation | 16-24 hours |
| Phase 4 | SimpleScene Integration | 4-6 hours |
| Phase 5 | Scene Audit | 4-6 hours |
| Phase 6 | Package Creation | 4-6 hours |
| Phase 7 | GitHub Release | 2-3 hours |
| **Total** | | **46-66 hours** |

---

## Dependencies

- **GitHub Wiki Access**: Required for Phase 1
- **Graphviz**: Required for class diagrams in Phase 2
- **Unity 2021.3+**: Required for scene editing
- **No FishNet/Fusion**: Scenes must work without these packages

---

## Related Documentation

- **Wiki Best Practices Research:** Applied from Diataxis, Mirror, FishNet wikis
- **Existing Tutorials:** 1-5 have scenes, 6-12 have scripts only
- **DSL Documentation:** Already complete and excellent
