# MercuryMessaging Improvement Tracker

## Priority Order (Approved 2025-11-27)

**Track 1: Production Engineering → Track 2: User Study → Track 3: Research Publications**

---

## Track 1: Production Engineering (IMMEDIATE)

### P1: Performance Optimization (300h) - COMPLETE
- **Target:** MessagePipe parity (zero-allocation, 5x faster than SendMessage)
- **Status:** COMPLETE (Phases 1-8) - Phase 9 deferred for research
- **Location:** `dev/archive/performance-optimization-complete/`
- **Completed Phases:**
  - Phase 1: ObjectPool integration (MmMessagePool, MmHashSetPool)
  - Phase 2: O(1) routing tables (Dictionary lookup)
  - Phase 3: LINQ removal (Array.Copy serialization)
  - Phase 4: Source generators (MmGenerateDispatchAttribute)
  - Phase 5: Delegate dispatch (SetFastHandler API)
  - Phase 6: Compiler optimizations (AggressiveInlining)
  - Phase 7: Memory optimizations (struct layout)
  - Phase 8: Algorithm optimizations (skip unnecessary checks)
- **Deferred:** Phase 9 (Burst compilation) - for 300+ responder scenarios
- **Validated Performance (2025-11-28):**
  - Small: 14.54ms / 68.8 FPS / 100 msg/sec
  - Medium: 14.29ms / 70.0 FPS / 500 msg/sec
  - Large: 17.17ms / 58.3 FPS / 1000 msg/sec
- **Result:** 58-70 FPS at 1000 msg/sec - production ready

### P2: FishNet Networking (200h) - COMPLETE ✅
- **Target:** Production-ready multiplayer
- **Status:** COMPLETE (2025-12-12)
- **Location:** `dev/archive/networking/`
- **Dependency:** Network infrastructure in Protocol/Network/
- **Deliverable:** Working multiplayer demo scene with FishNet backend
- **Completed (2025-12-01):**
  - Phase 0A: Package modularization ✅
  - Phase 0B: Networking foundation ✅
  - Phase 1: FishNet implementation ✅
    - Loopback tests: 15/15 PASS
    - ParrelSync bidirectional messaging: VERIFIED (Session 8)
    - Hierarchical routing over network: VERIFIED (Session 8)
    - Server→Client: MmString routed to 4 responders ✅
    - Client→Server: MmInt routed to 4 responders ✅
- **Key Insight:** Every responder GameObject needs MmRelayNode
- **Result:** FishNet backend production-ready, Fusion 2 backend scaffolded

### P2b: Core Performance Phase 2 (26h) - COMPLETE ✅
- **Target:** FSM caching, routing flexibility, serialization overhaul
- **Status:** COMPLETE (2025-12-04) - All tests passing
- **Location:** `dev/archive/core-performance/`
- **Plan File:** `.claude/plans/iterative-leaping-shannon.md`
- **Completed Tasks:**
  - D1: Remove SerialExecutionQueue dead code ✅
  - E1-E3: Handled flag early termination ✅
  - P1-P3: MmRelaySwitchNode caching ✅
  - Q1-Q4: MmRoutingChecks consolidation ✅ (MmQuickNode marked [Obsolete])
  - S1-S7: Serialization overhaul ✅ (IMmBinarySerializable, MmWriter, MmReader)
  - DX1-DX4: Developer experience improvements ✅
- **Test Infrastructure:**
  - MmTestResultExporter.cs - Auto-exports test results to dev/test-results/
  - MmTestHierarchy - Fluent test hierarchy builder with IDisposable
- **Result:** All 100+ tests passing, no compilation errors

### P3: DSL/DX Improvements (92h) - COMPLETE ✅
- **Target:** Even shorter syntax, Roslyn analyzers, source generators
- **Status:** COMPLETE (2025-12-12) - All phases done
- **Location:** `dev/archive/dsl-dx/`
- **Key Phases:**
  - Phase 1: Property-based syntax (`relay.To.Children.Send()`) - 16h ✅
  - Phase 2: Builder API for advanced cases - 8h ✅
  - Phase 3: Roslyn analyzers (15 analyzers MM001-MM015) - 20h ✅
  - Phase 4: Source generator enhancements ([MmHandler] attribute) - 40h ✅
  - Phase 5: Documentation updates - 8h ✅ (via wiki tutorials)
- **Completed:**
  - `relay.To.Children.Send("Hello")` - auto-executes immediately
  - `relay.Build().ToChildren().Send("Hello").Execute()` - deferred execution
  - Both work on MmRelayNode and MmBaseResponder
  - 26 tests covering property routing and builder API
  - **15 Roslyn Analyzers (MM001-MM015)** with code fix providers for MM005/MM010
  - **[MmHandler] attribute** for source-generated dispatch of custom methods
  - Generator diagnostics MMG001-MMG003 for invalid configurations
  - Test coverage in `Tests/Generators/MmHandlerAttributeTests.cs`
- **Analyzer Deployment:** `Assets/MercuryMessaging/Protocol/Analyzers/`
- **Result:** 95% verbosity reduction achieved (32 chars)

### P4: Fusion 2 Networking (200h) - DEFERRED
- **Target:** Alternative backend, hot-swappable with FishNet
- **Status:** Scaffolded, testing deferred until Fusion 2 needed
- **Location:** `dev/archive/networking/` (scaffolding in Protocol/Network/Backends/)
- **Dependency:** FishNet implementation complete ✅
- **Scaffolded (2025-12-11):**
  - Fusion2Backend.cs - IMmNetworkBackend implementation
  - MmFusion2Bridge.cs - NetworkBehaviour for RPC handling
  - Fusion2Resolver.cs - IMmGameObjectResolver implementation
  - RPC design: TickAligned=false for immediate delivery
- **To Complete When Needed:**
  - Install Fusion 2 package and add FUSION2_AVAILABLE define
  - Loopback and multi-client testing
  - State authority handling refinement

---

## Track 2: User Study (Q2 2025)

### P5: Mercury vs UnityEvents Comparison (40h)
- **Target:** CHI LBW 2025 submission
- **Status:** ACTIVE - Smart Home scenes created, ready to run study
- **Location:** `dev/active/user-study/`
- **Deliverable:** Empirical comparison paper (N=15-20 participants)
- **Metrics:** LOC, development time, coupling, debugging efficiency

---

## Track 3: Research Publications (Q2-Q4 2025)

### P6: Spatial Indexing (360h) - UIST 2026
- **Novel:** Hybrid spatial-hierarchical O(log n) routing
- **Status:** Planning
- **Location:** `dev/active/spatial-indexing/`
- **Impact:** 10,000+ user metaverse-scale applications
- **Research Question:** Can hybrid indices achieve O(log n) for spatial-hierarchical queries?

### P7: Parallel FSMs (280h) - UIST 2026
- **Novel:** Multi-modal XR via message-based FSM coordination
- **Status:** Planning
- **Location:** `dev/active/parallel-fsm/`
- **Impact:** Gesture + voice + gaze simultaneous input handling
- **Research Question:** Can message-passing prevent race conditions in multi-modal input?

### P8: Network Prediction (400h) - CHI 2026
- **Novel:** Hierarchical client-side prediction
- **Status:** Planning
- **Location:** `dev/active/network-prediction/`
- **Impact:** 100-200ms perceived latency reduction in distributed XR
- **Research Question:** Can route prediction reduce perceived latency by 100-200ms?

### P9: Visual Composer (360h) - UIST 2026
- **Novel:** Live visual debugging for message systems
- **Status:** Planning
- **Location:** `dev/active/visual-composer/`
- **Impact:** 50% reduction in debugging time (user study hypothesis)
- **Research Question:** Does live introspection reduce debugging time by 50%?

### P10: Static Analysis (280h) - ICSE 2026
- **Novel:** Hybrid static-runtime verification for message systems
- **Status:** Planning
- **Location:** `dev/active/static-analysis/`
- **Impact:** Formal safety guarantees with <200ns overhead
- **Research Question:** Can we guarantee no infinite loops with <200ns overhead?

---

## Deferred Tasks

### Thread Safety (4-8h)
- **Trigger:** When async/await messaging is needed
- **Approach:** Lock-based (Option A) is simplest
- **Location:** `dev/archive/thread-safety/` (design docs)

### Parallel Dispatch (360h)
- **Trigger:** After Performance Optimization complete
- **Target:** SIGGRAPH 2026 (Job System + Burst parallel routing)
- **Location:** `dev/active/parallel-dispatch/`

---

## Competitive Landscape (2025-11-27 Analysis)

| Framework | Performance | Hierarchical | Spatial | Network |
|-----------|-------------|--------------|---------|---------|
| **MessagePipe** | 78x faster | No | No | No |
| **UniRx/R3** | Good | No | No | No |
| **Zenject Signals** | Moderate | No | No | No |
| **Unity DOTS** | Best | No | Via queries | No |
| **SendMessage** | 10x slower | Via hierarchy | No | No |
| **Mercury (current)** | 28x slower | Yes | No | Yes |
| **Mercury (target)** | ~3x slower | Yes | Yes | Yes |

**Key Differentiation:** Mercury is the ONLY framework combining hierarchical + spatial + networked routing.

---

## Completed Improvements

### Quick Wins (QW-1 through QW-6)
1. **QW-1**: Hop limits and cycle detection
2. **QW-2**: Lazy message copying
3. **QW-3**: Filter result caching (infrastructure)
4. **QW-4**: CircularBuffer for bounded memory
5. **QW-5**: LINQ removal for performance
6. **QW-6**: Code cleanup (179 lines removed)

### Performance Optimizations (2025-11-20)
- 2-2.2x frame time improvement (32ms → 15-19ms)
- 3-35x throughput improvement (28 → 98-980 msg/sec)
- Memory stability validated
- See `Documentation/Performance/` for details

### Language DSL (2025-11-25)
- 86% code reduction (210 chars → 48 chars)
- Two-tier API (Auto-Execute + Fluent Chain)
- Works on both MmRelayNode and MmBaseResponder
- 93+ tests passing
- See `Assets/MercuryMessaging/Protocol/DSL/`

### Project Reorganization (2025-11-25)
- 14 → 6 top-level folders
- ~500MB build size reduction
- Framework isolation achieved
- XR consolidation complete

---

## Decision Log

### 2025-12-03: Core Performance Phase 2 Planning
- **New Task:** Created `dev/active/core-performance/` for P2b
- **Dead Code:** D1 removes SerialExecutionQueue (experimental, never completed)
- **Handled Flag:** E1-E3 adds WPF-style early termination for message propagation
- **FSM Caching:** P1-P3 caches current state for faster SelectedCheck
- **Routing Flexibility:** Q1-Q4 adds MmRoutingChecks flags, eliminates MmQuickNode
- **Serialization:** S1-S7 replaces IMmSerializable with zero-allocation IMmBinarySerializable
- **Total Effort:** 26h
- **Plan File:** `.claude/plans/iterative-leaping-shannon.md`

### 2025-12-12: Track 1 Production Engineering Complete
- **Archived:** dsl-dx, networking, wiki-tutorials moved to `dev/archive/`
- **P2 FishNet:** COMPLETE - Production-ready multiplayer with FishNet backend
- **P3 DSL/DX:** COMPLETE - 95% verbosity reduction, 15 analyzers, [MmHandler] attribute
- **P4 Fusion 2:** DEFERRED - Scaffolded, ready when needed
- **Wiki Tutorials:** 14 tutorials drafted in `dev/wiki-drafts/`, ready for wiki push
- **Next:** P5 User Study (Track 2)

### 2025-12-01: DSL/DX Planning and Wiki Tutorials
- **Syntax Decision:** Level 1 Property-based (`relay.To.Children.Send()`) over operators
- **Auto-Execute:** Dual API (auto-execute + builder) + Analyzer warnings (MM005)
- **Wiki Discovery:** Existing wiki at ColumbiaCGUI org (not CGUI-Lab)
- **Wiki Strategy:** Keep existing tutorials 1-10, add new 5c, 11-14 for new features
- **Analyzers Planned:** MM005 (missing Execute), MM010 (non-partial class), MM001 (suggest DSL)
- **Source Generators:** Extend existing `[MmGenerateDispatch]` generator

### 2025-11-27: Task Consolidation and Research Roadmap
- Consolidated 11 active dev tasks into prioritized roadmap
- Added competitive analysis (MessagePipe, UniRx, Zenject, DOTS)
- Identified Mercury's unique differentiation: hierarchical + spatial + networked routing
- Moved session handoffs to archive
- Created DSL/DX improvements task
- Updated publication targets to 2026 venues (UIST, CHI, ICSE)

### 2025-11-25: DSL Optimizations and API Consolidation
- Implemented 3 performance optimizations for MmFluentMessage.Execute()
- Added unified API for both relay nodes and responders
- Archived language-dsl task as complete

### 2025-11-24: Test Fixing and DSL Phase 1 Complete
- Reverted Option C message creation gating
- Fixed 17+ test failures (advanced routing)
- All 211 tests passing

---

### 2025-12-11: Roslyn Analyzers Phase 3 Complete
- **All 15 Analyzers Implemented:** MM001-MM015 deployed and working
- **Key Implementation Details:**
  - Analyzer DLL built with netstandard2.0 target
  - Uses Microsoft.CodeAnalysis.CSharp 4.3.0 (Unity compatible)
  - Deployed to `Assets/MercuryMessaging/Editor/Analyzers/`
  - Meta file requires `RoslynAnalyzer` label for Unity to recognize
- **Test Cases:** `AnalyzerTestCases.cs` with `#define MM_ANALYZER_TEST`
- **Warnings in Test Files:** Expected - analyzers correctly flag patterns that would be problematic in production code
- **Code Fix Providers:** MM005 (add .Execute()) and MM010 (add partial keyword)
- **Next:** Phase 4 - Source generator enhancements

---

*Last Updated: 2025-12-12*
*Next Review: End of Q1 2026*
*Active Task Folders:*
- *P5 User Study: `dev/active/user-study/`*

*Recently Archived:*
- *P2 FishNet: `dev/archive/networking/` - COMPLETE*
- *P3 DSL/DX: `dev/archive/dsl-dx/` - COMPLETE*
- *Wiki Tutorials: `dev/archive/wiki-tutorials/` - COMPLETE, ready for wiki push*
