# Assets Folder Reorganization Plan

> **Status:** ✅ COMPLETE (implemented 2025-11-25)
> **Created:** 2025-11-25
> **Updated:** 2025-11-25
> **Completed:** 2025-11-25

## Related Tasks
- `dev/active/performance-optimization/` - Performance optimization initiative (Phase 3.5 integration)
- `dev/active/networking/` - Network backend implementation
- `dev/active/dsl-overhaul/` - DSL overhaul (StandardLibrary folder)

## Goal
Reorganize Unity Assets folder from 14 scattered folders to 6 clean top-level folders with proper framework isolation, -501MB build size reduction, and scalable architecture.

## Target Structure (6 Folders)

```
Assets/
├── Framework/          # MercuryMessaging framework (portable, zero dependencies)
├── Project/            # Project-specific code (feature-based organization)
├── Research/           # User studies & experiments
├── Plugins/            # Third-party dependencies (categorized)
├── Platform/           # XR/VR configuration (consolidated)
└── Unity/              # Unity-managed folders (Editor, Resources, Settings, TextMesh Pro)
```

## Current → Target Mapping

| Current Location | New Location | Size | Priority |
|------------------|--------------|------|----------|
| `MercuryMessaging/` | `Framework/MercuryMessaging/` | 8.2MB | P2 |
| `_Project/` | `Project/` | 366KB | P2 |
| `UserStudy/` | `Research/UserStudy/` | 39MB | P3 |
| `ThirdParty/` | `Plugins/` | 571MB | P3 |
| `XRConfiguration/` + `Oculus/` + `XR/` + `CompositionLayers/` | `Platform/XR/` | 251KB | P1 |
| `Resources/oculus-controller-art-v1.8/` | `Platform/XR/Oculus/ControllerArt/` | **501MB** | P1 |
| `Resources/test-results/` + `performance-results/` | `dev/` (outside Assets) | 6MB | P1 |
| `Editor/`, `Settings/`, `TextMesh Pro/`, `Samples/` | `Unity/` | 32MB | P4 |

---

## Implementation Phases

### Phase 1: Critical Fixes (30 min, Zero Risk)
**Goal:** Remove 501MB from builds, clean up scattered files

1. **Delete backup file:**
   ```
   DELETE: Assets/MercuryMessaging/Protocol/DSL/MmFluentMessage.cs.backup
   ```

2. **Move test/performance results OUT of Assets:**
   ```
   MOVE: Assets/Resources/test-results/        → dev/test-results/
   MOVE: Assets/Resources/performance-results/ → dev/performance-results/
   ```

   **Update paths in:**
   - `Assets/MercuryMessaging/Tests/Performance/Scripts/PerformanceTestHarness.cs` (lines 46, 415)
   - `Assets/MercuryMessaging/Tests/Performance/Scripts/ComparisonTestHarness.cs`

3. **Delete duplicate Tutorials folder:**
   ```
   DELETE: Assets/MercuryMessaging/Tutorials/ (empty duplicate)
   ```

4. **Delete Recovery folder (already staged):**
   ```
   COMMIT: Assets/_Recovery/ deletion
   ```

**Verification:** Unity opens without errors, no missing references

---

### Phase 2: XR Consolidation (45 min, Low Risk)
**Goal:** Consolidate 4 XR locations + move 501MB out of Resources

1. **Create Platform structure:**
   ```
   CREATE: Assets/Platform/
   CREATE: Assets/Platform/XR/
   CREATE: Assets/Platform/XR/Settings/
   CREATE: Assets/Platform/XR/Oculus/
   ```

2. **Move XR settings (4 locations → 1):**
   ```
   MOVE: Assets/XRConfiguration/MetaXR/    → Assets/Platform/XR/Settings/MetaXR/
   MOVE: Assets/XRConfiguration/Oculus/    → Assets/Platform/XR/Settings/Oculus/
   MOVE: Assets/XRConfiguration/XR/        → Assets/Platform/XR/Settings/XR/
   MOVE: Assets/XRConfiguration/XRI/       → Assets/Platform/XR/Settings/XRI/
   MOVE: Assets/Oculus/*                   → Assets/Platform/XR/Settings/Oculus/
   MOVE: Assets/XR/Settings/*              → Assets/Platform/XR/Settings/XR/
   MOVE: Assets/CompositionLayers/*        → Assets/Platform/XR/Settings/CompositionLayers/

   DELETE EMPTY: Assets/XRConfiguration/
   DELETE EMPTY: Assets/Oculus/
   DELETE EMPTY: Assets/XR/
   DELETE EMPTY: Assets/CompositionLayers/
   ```

3. **Move Oculus controller art (501MB!) out of Resources:**
   ```
   MOVE: Assets/Resources/oculus-controller-art-v1.8/ → Assets/Platform/XR/Oculus/ControllerArt/
   ```

4. **Move XR samples:**
   ```
   MOVE: Assets/Samples/ → Assets/Platform/XR/Samples/
   ```

**Verification:**
- Unity XR settings still accessible (Edit > Project Settings > XR)
- VR scene loads correctly
- Build size reduced by ~501MB

---

### Phase 3: Framework Isolation (2-3 hours, Medium Risk)
**Goal:** Rename and reorganize MercuryMessaging as isolated Framework

1. **Rename to Framework:**
   ```
   RENAME: Assets/MercuryMessaging/ → Assets/MercuryMessaging/
   ```

2. **Reorganize internal structure:**
   ```
   Assets/MercuryMessaging/
   ├── Runtime/
   │   ├── Protocol/       (from Protocol/)
   │   ├── AppState/       (from AppState/)
   │   ├── Task/           (from Task/)
   │   └── Support/        (from Support/ minus Editor/)
   ├── Editor/             (from Support/Editor/)
   ├── Tests/              (keep as-is)
   ├── Examples/           (keep as-is)
   └── Documentation/      (consolidate .md files)
   ```

3. **Update assembly definition:**
   ```
   RENAME: MercuryMessaging.asmdef → MercuryMessaging.Runtime.asmdef
   CREATE: MercuryMessaging.Editor.asmdef (new)
   ```

4. **Update CLAUDE.md directory structure section**

**Verification:** All tests pass, no compilation errors

---

### Phase 3.5: Performance-Optimized Internal Structure (1-2 hours, Low Risk)
**Goal:** Organize Framework internals to support performance optimization initiative

> **Related Task:** `dev/active/performance-optimization/`

1. **Create Core folder for hot-path optimized code:**
   ```
   CREATE: Assets/MercuryMessaging/Runtime/Protocol/Core/
   ```

   **Purpose:** Location for performance-critical pooling and dispatch code:
   - `MmMessagePool.cs` - ObjectPool for all message types
   - `MmHashSetPool.cs` - Pool for VisitedNodes HashSets
   - `MmFastDispatch.cs` - Delegate-based dispatch helpers

2. **Create StandardLibrary for typed message handlers:**
   ```
   CREATE: Assets/MercuryMessaging/Runtime/StandardLibrary/
   CREATE: Assets/MercuryMessaging/Runtime/StandardLibrary/UI/
   CREATE: Assets/MercuryMessaging/Runtime/StandardLibrary/Input/
   ```

   **Purpose:** Location for DSL Phase 9-11 message types:
   - `UI/MmUIMessages.cs` - UI event messages (Click, Hover, Drag, etc.)
   - `UI/MmUIResponder.cs` - Base responder for UI messages
   - `Input/MmInputMessages.cs` - VR input messages (6DOF, Gesture, Button)
   - `Input/MmInputResponder.cs` - Base responder for input messages

3. **Reorganize Tests for performance:**
   ```
   CREATE: Assets/MercuryMessaging/Tests/Performance/
   CREATE: Assets/MercuryMessaging/Tests/Integration/
   MOVE: Tests/Performance/Scripts/* → Tests/Performance/
   ```

   **Purpose:** Separate test categories for better organization

4. **Updated Framework internal structure:**
   ```
   Assets/MercuryMessaging/
   ├── Runtime/
   │   ├── Protocol/           (core messaging)
   │   │   ├── Core/           # NEW: Hot-path optimized code
   │   │   ├── DSL/            (fluent API - existing)
   │   │   ├── Network/        (networking backends - existing)
   │   │   └── Message/        (message types - existing)
   │   ├── StandardLibrary/    # NEW: Typed message handlers
   │   │   ├── UI/
   │   │   └── Input/
   │   ├── AppState/
   │   ├── Task/
   │   └── Support/
   ├── Editor/
   ├── Tests/
   │   ├── Unit/               # Existing unit tests
   │   ├── Integration/        # NEW
   │   └── Performance/        # NEW (relocated)
   └── Examples/
   ```

**Verification:**
- All existing tests pass
- New folders created
- Assembly definitions updated if needed

---

### Phase 4: Project Reorganization (1-2 hours, Low Risk)
**Goal:** Feature-based organization for project code

1. **Rename _Project:**
   ```
   RENAME: Assets/_Project/ → Assets/Project/
   ```

2. **Reorganize to feature-based structure:**
   ```
   Assets/Project/
   ├── Runtime/
   │   ├── Features/
   │   │   ├── TrafficLights/   (Scripts + Prefabs + Materials)
   │   │   ├── Tutorials/       (from Scripts/Tutorials/)
   │   │   └── VR/              (from Scripts/VR/)
   │   ├── Core/                (from Scripts/Core/)
   │   ├── Responders/          (from Scripts/Responders/)
   │   └── UI/                  (from Scripts/UI/)
   ├── Editor/
   ├── Scenes/
   ├── Prefabs/
   └── Materials/
   ```

3. **Create assembly definition:**
   ```
   CREATE: Project.Runtime.asmdef (depends on MercuryMessaging.Runtime)
   ```

**Verification:** Scenes load correctly, prefabs intact

---

### Phase 5: Research & Plugins (1-2 hours, Low Risk)
**Goal:** Organize research content and categorize third-party plugins

1. **Create Research folder:**
   ```
   CREATE: Assets/Research/
   MOVE: Assets/UserStudy/ → Assets/Research/UserStudy/
   CREATE: Assets/Research/Archive/    (for completed studies)
   CREATE: Assets/Research/Prototypes/ (for experiments)
   ```

2. **Reorganize Plugins by purpose:**
   ```
   RENAME: Assets/ThirdParty/ → Assets/Plugins/

   Assets/Plugins/
   ├── Rendering/       (EasyPerformantOutline, QuickOutline, MKGlowFree)
   ├── Debugging/       (ALINE)
   ├── Networking/      (Photon)
   ├── Visualization/   (NewGraph, MercuryGraph)
   ├── AssetStore/      (keep as-is)
   └── Native/          (Android)
   ```

**Verification:** All plugin references intact, no pink materials

---

### Phase 6: Unity-Managed Consolidation (30 min, Low Risk)
**Goal:** Group Unity-managed folders together

1. **Create Unity folder:**
   ```
   CREATE: Assets/Unity/
   MOVE: Assets/Editor/        → Assets/Unity/Editor/
   MOVE: Assets/Settings/      → Assets/Unity/Settings/
   MOVE: Assets/TextMesh Pro/  → Assets/Unity/TextMesh Pro/
   MOVE: Assets/StreamingAssets/ → Assets/Unity/StreamingAssets/
   ```

2. **Clean Resources (should be nearly empty now):**
   ```
   MOVE: Assets/Resources/ → Assets/Unity/Resources/
   ```
   Only remaining: OculusRuntimeSettings.asset, InputActions.asset, etc. (~6KB)

**Verification:** Unity opens, TextMesh Pro works

---

### Phase 7: Documentation & Cleanup (1 hour)
**Goal:** Update all documentation to reflect new structure

1. **Update CLAUDE.md:**
   - New directory structure section
   - Update all file paths in examples
   - Update Quick Reference section

2. **Create README files:**
   - `Assets/Framework/README.md`
   - `Assets/Project/README.md`
   - `Assets/Research/README.md`
   - `Assets/Plugins/README.md`
   - `Assets/Platform/README.md`
   - `Assets/Unity/README.md`

3. **Update .gitignore:**
   ```
   dev/test-results/
   dev/performance-results/
   Assets/Research/*/Data/
   ```

4. **Final verification:**
   - Run all tests
   - Build to PC
   - Build to Android (if applicable)
   - Verify build size reduction (~501MB)

---

## Critical Files to Read Before Implementation

1. **`Assets/MercuryMessaging/Tests/Performance/Scripts/PerformanceTestHarness.cs`**
   - Lines 46, 415: Hardcoded paths to `Resources/performance-results/`
   - Must update before moving test results

2. **`Assets/Resources/OculusRuntimeSettings.asset`**
   - May reference controller art paths
   - Verify references after moving Oculus resources

3. **`ProjectSettings/EditorBuildSettings.asset`**
   - Contains scene paths
   - May need updates after folder moves

4. **`Assets/MercuryMessaging/MercuryMessaging.asmdef`**
   - Will be renamed/restructured
   - Check references in other assembly definitions

5. **`CLAUDE.md`**
   - Directory structure section (lines 17-96)
   - Must be updated to reflect new organization

---

## Expected Outcomes

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Top-level folders | 14 | 6 | 57% reduction |
| Resources/ size | 501MB | ~6KB | 99.9% reduction |
| Build size | +501MB | Normal | -501MB |
| Framework isolation | Mixed | Complete | Extractable as package |
| XR config locations | 4 | 1 | Consolidated |

---

## Rollback Plan

Before each phase:
```bash
git add -A && git commit -m "checkpoint: before [phase name]"
```

If issues found:
```bash
git reset --hard HEAD
```

---

## Git Commits (Conventional Format)

1. `chore: delete backup files and empty duplicates`
2. `chore: move test results out of Assets to dev/`
3. `refactor: consolidate XR configuration into Platform/XR/`
4. `refactor: move Oculus controller art out of Resources (-501MB)`
5. `refactor: rename MercuryMessaging to Framework/MercuryMessaging`
6. `refactor: reorganize Framework internal structure`
7. `refactor: rename _Project to Project with feature-based organization`
8. `refactor: move UserStudy to Research/UserStudy`
9. `refactor: reorganize ThirdParty to Plugins by category`
10. `refactor: consolidate Unity-managed folders`
11. `docs: update CLAUDE.md and create README files`

---

## Time Estimate

| Phase | Time | Risk |
|-------|------|------|
| Phase 1: Critical Fixes | 30 min | Zero |
| Phase 2: XR Consolidation | 45 min | Low |
| Phase 3: Framework Isolation | 2-3 hours | Medium |
| Phase 3.5: Performance Structure | 1-2 hours | Low |
| Phase 4: Project Reorganization | 1-2 hours | Low |
| Phase 5: Research & Plugins | 1-2 hours | Low |
| Phase 6: Unity-Managed | 30 min | Low |
| Phase 7: Documentation | 1 hour | None |
| **Total** | **8-12 hours** | |

Recommend executing over 2-3 sessions with thorough testing between phases.
