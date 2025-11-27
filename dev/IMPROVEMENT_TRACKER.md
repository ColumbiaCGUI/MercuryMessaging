# MercuryMessaging Improvement Tracker

## Priority Order (Approved 2025-11-27)

**Track 1: Production Engineering → Track 2: User Study → Track 3: Research Publications**

---

## Track 1: Production Engineering (IMMEDIATE)

### P1: Performance Optimization (300h) - CRITICAL
- **Target:** MessagePipe parity (zero-allocation, 5x faster than SendMessage)
- **Status:** APPROVED - Ready to start
- **Location:** `dev/active/performance-optimization/`
- **Key Phases:**
  - Phase 1: ObjectPool integration (80-90% allocation reduction)
  - Phase 2: O(1) routing tables (Dictionary lookup)
  - Phase 3-4: Source generators + delegate dispatch
  - Phase 5-6: Compiler optimizations + memory tuning
  - Phase 7-9: Burst compilation (requires NativeContainer migration)
- **Current Performance:**
  - vs Direct Calls: 28x slower
  - vs SendMessage: 2.6x slower
  - vs MessagePipe: ~15x slower
- **Target Performance:**
  - vs SendMessage: 5x FASTER
  - vs MessagePipe: 2x slower (acceptable)

### P2: FishNet Networking (200h)
- **Target:** Production-ready multiplayer
- **Status:** APPROVED - Phase 0B network foundation complete
- **Location:** `dev/active/networking/`
- **Dependency:** Network infrastructure in Protocol/Network/
- **Deliverable:** Working multiplayer demo scene with FishNet backend

### P3: DSL/DX Improvements (120h)
- **Target:** Even shorter syntax, tutorials, better IntelliSense
- **Status:** Planning
- **Location:** `dev/active/dsl-dx-improvements/` (to be created)
- **Key Phases:**
  - Phase 1: Shorter syntax (operator overloads) - 40h
  - Phase 2: Source generators for handlers - 40h
  - Phase 3: Roslyn analyzers - 20h
  - Phase 4: IntelliSense enhancements - 20h
- **Current:** 86% verbosity reduction achieved
- **Target:** 95% verbosity reduction

### P4: Fusion 2 Networking (200h)
- **Target:** Alternative backend, hot-swappable with FishNet
- **Status:** Planning
- **Location:** `dev/active/networking/`
- **Dependency:** FishNet implementation complete

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
- **Location:** `dev/active/networking/` (network prediction component)
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
- **Location:** Merged into performance-optimization Phase 10

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
- See `Assets/Framework/MercuryMessaging/Protocol/DSL/`

### Project Reorganization (2025-11-25)
- 14 → 6 top-level folders
- ~500MB build size reduction
- Framework isolation achieved
- XR consolidation complete

---

## Decision Log

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

*Last Updated: 2025-11-27*
*Next Review: End of Q1 2025*
*Plan File: `.claude/plans/immutable-puzzling-pinwheel.md`*
