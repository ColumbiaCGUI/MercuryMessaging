---
phase: 01-tutorial-validation
plan: 01
subsystem: documentation
tags: [wiki, tutorials, static-analysis, api-verification, mercury-messaging]

# Dependency graph
requires: []
provides:
  - "PRE_ANALYSIS.md with complete mismatch inventory for all 12 tutorials + 2 stubs"
  - "All 20 decision items resolved with source code verification"
  - "API existence verification for 7 cross-tutorial concerns (all confirmed)"
  - "Environment readiness documented (render pipeline, Photon, XR, wiki repo, GH label)"
affects: [01-02-PLAN, 01-03-PLAN, 01-04-PLAN]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Wiki uses generic pedagogical names; code uses T{N}_ prefixed names (expected pattern)"
    - "Code is ground truth for wiki-vs-code mismatches per user decision"

key-files:
  created:
    - ".planning/phases/01-tutorial-validation/PRE_ANALYSIS.md"
  modified: []

key-decisions:
  - "All 7 Category 1 APIs verified to exist in framework source code"
  - "Category 2 direction: wiki always updated to match code (code is ground truth)"
  - "MmDataCollector wiki API must be rewritten (uses Add/Write/OpenTag, not SetHeaders/AddRow/SaveToFile)"
  - "Tutorial 12 monolithic architecture accepted for now; flagged for potential Phase 5 refactor"
  - "T13 temporal methods confirmed Available; T14 source generators confirmed Available"
  - "Property-based routing (relay.To.Children) confirmed as real API -- strengthens C3 contribution"

patterns-established:
  - "Static analysis before runtime validation: front-loads investigation for targeted fixing"
  - "DECISION NEEDED pattern: flag mismatches, batch-resolve with user, then annotate in-place"

# Metrics
duration: 5min
completed: 2026-02-13
---

# Phase 1 Plan 1: Pre-Analysis Summary

**Comprehensive static analysis of all 12 wiki tutorials against source code, with all 20 API/mismatch decisions resolved via framework source verification**

## Performance

- **Duration:** 5 min (continuation from checkpoint; Task 1 was ~45 min in prior session)
- **Started:** 2026-02-13T06:59:45Z
- **Completed:** 2026-02-13T07:05:00Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Created PRE_ANALYSIS.md with per-tutorial, per-wiki-step comparison data for all 12 tutorials + 2 stubs
- Verified all 7 cross-tutorial API existence concerns against framework source code (all APIs confirmed present)
- Resolved all 20 DECISION NEEDED items with clear action directives for Plans 02-04
- Documented environment readiness: Built-in render pipeline (Standard shader safe), Photon AppId configured, wiki repo cloned, GH label created, XR Device Simulator not yet imported (needed for T12)

## Task Commits

Each task was committed atomically:

1. **Task 1: Environment setup and comprehensive static analysis of all 12 tutorials** - `8314fad4` (feat)
2. **Task 2: User decides direction for all wiki-vs-code mismatches** - `0f9c62ce` (feat)

## Files Created/Modified
- `.planning/phases/01-tutorial-validation/PRE_ANALYSIS.md` - Complete static analysis report with 12 tutorial sections, environment status, cross-tutorial issues, and all decisions annotated with resolutions

## Decisions Made

### Category 1: API Existence (7 verified)
All APIs confirmed to exist in framework source:
1. **A: Property-based routing** - `relay.To.Children.Send()` exists (MmRelayNode.To property + MmRoutingBuilder struct)
2. **B: Network APIs** - All 9 classes exist (MmNetworkBridge, FishNetBackend, FishNetResolver, Fusion2Backend, MmFusion2Bridge, MmLoopbackBackend, MmBinarySerializer, IMmNetworkBackend, IMmGameObjectResolver)
3. **C: MmDataCollector** - Exists but with different API than wiki (Add/Write/OpenTag pattern, not SetHeaders/AddRow/SaveToFile)
4. **D: Temporal extensions** - After/Every/When all exist with 15+ overloads in MmTemporalExtensions.cs
5. **E: MmWriter/MmReader** - Both exist for binary serialization
6. **F: Task system** - MmTaskManager<U>, IMmTaskInfo, IMmTaskInfoCollectionLoader<U>, MmTaskUserConfigurator all exist
7. **G: App state** - MmSwitchResponder, MmAppStateSwitchResponder, MmAppStateResponder all exist

### Category 2: Wiki-vs-Code Direction (8 resolved)
User direction: "Code is ground truth. Update wiki to match code."
- T1 Key 3: Wiki shows BroadcastValue(42) for Alpha3
- T1 SceneController: Add to wiki
- T3 method constants: Wiki shows TakeDamage=1000 (matching code)
- T4 serialization: object[] primary, MmWriter/MmReader as alternative
- T9 navigation: Wiki shows GoToNextTask()
- T10 base class: Wiki uses MmAppStateSwitchResponder
- T12 architecture: Wiki matches code (monolithic) for now; flag for Phase 5
- T12 VR input: Wiki matches code (legacy) + note XRI alternative

### Category 3: Temporal Features Status (1 resolved)
- Tutorial 13 After/Every/When confirmed "Available" (correct status)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- Wiki repo cloned at `C:/Users/yangb/Research/MercuryMessaging.wiki/` has Windows colon-in-filename issue preventing `git pull` but files are readable. Not blocking.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- PRE_ANALYSIS.md provides complete mismatch inventory for Plans 02-04 to consume
- All decisions resolved -- no further user input needed for fix tasks
- Plans 02-03 (wiki fixes Waves 1 and 2) can proceed immediately
- XR Device Simulator must be imported before Plan 04 (Tutorial 12 validation)

## Self-Check: PASSED

- FOUND: .planning/phases/01-tutorial-validation/PRE_ANALYSIS.md
- FOUND: .planning/phases/01-tutorial-validation/01-01-SUMMARY.md
- FOUND: commit 8314fad4 (Task 1)
- FOUND: commit 0f9c62ce (Task 2)

---
*Phase: 01-tutorial-validation*
*Completed: 2026-02-13*
