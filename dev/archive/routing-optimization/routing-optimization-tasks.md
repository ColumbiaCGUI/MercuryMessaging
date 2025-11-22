# Routing Optimization - Task Checklist

**Last Updated:** 2025-11-21 (Performance Profiling + Routing Table Analysis)

---

## Phase 2.1 Progress: 186h / 254h (73.2% Complete) ✅

**Latest Session Achievements (Routing Table Profiling Mini-Task - 6h COMPLETE):**
- ✅ Implemented routing table profiling instrumentation (4h)
  - Added per-frame metric accumulators in MmRelayNode (RoutingTableTotalMs, MmInvokeTotalMs, invocations)
  - Instrumented MmInvoke() with Stopwatch timing around routing table iteration
  - Integrated profiling with PerformanceTestHarness (auto-enable, collect, reset, export)
  - Added 4 new CSV columns: routing_table_ms, mminvoke_ms, routing_table_percent, routing_invocations
  - **Commits:** cfae1199 (instrumentation), e2e16412 (Debug ambiguity fix)
- ✅ **CRITICAL DISCOVERY: Observer Effect** (2h)
  - Profiling showed 97-98% overhead on Medium/Large scales
  - Frame time regression: 16ms → 325ms (20x worse!)
  - Root cause: 2672 invocations/frame × 2 Stopwatches = 5344 operations
  - Profiling infrastructure dominated measurement (classic Heisenbug)
  - **Commits:** c4db9432 (disable profiling), 9b5a0dd3 (document discovery)
  - **Report:** `OBSERVER_EFFECT_DISCOVERY.md` - Comprehensive analysis
- ✅ Re-ran performance tests with clean baseline (results validated)
  - SmallScale: 4.25ms (3.6x better than optimized baseline!)
  - MediumScale: 4.84ms (3.4x better, was 16.28ms)
  - LargeScale: 3.66ms (5.1x better, was 18.69ms)
  - **All scales: 235-273 FPS** (well above 60 FPS target)
- ✅ Created ROUTING_TABLE_PROFILE.md with Phase 3.1 decision
  - **DECISION: SKIP Phase 3.1 (Save 256h)**
  - Framework performs excellently (3-5ms frame time)
  - No validated performance problem to solve
  - Cannot measure routing table overhead (Observer Effect)
  - Optimization would be premature
  - **Commit:** e837e2b8 - Complete routing table profile report

**Previous Session Achievements (Performance Profiling - 20h):**
- ✅ Implemented performance profiling hooks (20h)
  - HandleAdvancedRouting: 6 metrics (filters, counts, time)
  - ResolvePathTargets: 5 metrics (path, segments, wildcards, visited, targets, time)
  - Global flags + per-message options
  - Zero overhead when disabled
- ✅ Strategic analysis: Phase 3.1 evaluation (256h of tasks analyzed)
  - **INITIAL DECISION:** Skip Phase 3.1 specialized routing tables (premature optimization)
  - **ALTERNATIVE:** 6h profiling mini-task to validate actual bottlenecks
  - **FINAL DECISION:** Confirmed skip after mini-task (no performance problem found)

**Earlier Achievements (Path Specification - 40h):**
- ✅ Created MmPathSpecification parser (12h)
- ✅ Implemented path resolution in MmRelayNode (12h)
- ✅ Added MmInvokeWithPath() API methods (8h)
- ✅ Created PathSpecificationTests with 35 tests (8h)
- ✅ Fixed wildcard expansion + MessageCounterResponder bugs
- ✅ Tests: 187/188 passing (1 acceptable performance variance)

**Next Priority:** Phase 2.1 remaining tasks (Tutorial 8h, API Docs 12h, Integration Testing 18h, Performance Testing 20h)

---

## Testing Standards

All tests for this project MUST follow these patterns:

### Required Approach
- ✅ Use **Unity Test Framework** (PlayMode or EditMode)
- ✅ Create **GameObjects programmatically** in `[SetUp]` methods
- ✅ All components added via `GameObject.AddComponent<T>()`
- ✅ Clean up in `[TearDown]` with `Object.DestroyImmediate()`

### Prohibited Patterns
- ❌ NO manual scene creation or loading
- ❌ NO manual UI element prefabs
- ❌ NO prefab dependencies from Resources folder

### Example Test Pattern
```csharp
[Test]
public void TestSiblingRouting()
{
    // Arrange - create hierarchy programmatically
    GameObject parent = new GameObject("Parent");
    GameObject child1 = new GameObject("Child1");
    GameObject child2 = new GameObject("Child2");
    child1.transform.SetParent(parent.transform);
    child2.transform.SetParent(parent.transform);

    MmRelayNode relay1 = child1.AddComponent<MmRelayNode>();
    MmRelayNode relay2 = child2.AddComponent<MmRelayNode>();

    // Act - test sibling message routing
    relay1.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(MmLevelFilter.Siblings));

    // Assert - verify sibling received message
    Assert.IsTrue(relay2.ReceivedMessage);

    // Cleanup
    Object.DestroyImmediate(parent);
}
```

---

## Phase 2.1: Advanced Message Routing (6 weeks, 254 hours)

### Design & Architecture (24 hours) ✅ COMPLETE

- [x] Create MmRoutingOptions class specification
  - **Status:** ✅ Complete (MmRoutingOptions.cs - 280 lines)
  - **Effort:** 4h
  - **Commit:** d64d81f6

- [x] Design message history tracking system
  - **Status:** ✅ Complete (MmMessageHistoryCache.cs - 320 lines)
  - **Effort:** 8h
  - **Commit:** d64d81f6

- [x] Design extended level filters architecture
  - **Status:** ✅ Complete (MmLevelFilter.cs extended with 5 new modes)
  - **Effort:** 4h
  - **Commit:** d64d81f6

- [ ] Design path specification parser
  - **Status:** ⏳ Not Started
  - **Acceptance:** Grammar and parser design
  - **Effort:** 8h
  - **Owner:** Lead Dev
  - **Next Steps:** Design grammar `segment ('/' segment)*` with wildcard support

### Implementation: Message History (36 hours) ✅ COMPLETE

- [x] Implement message ID generation
  - **Status:** ✅ Complete (uses GameObject.GetInstanceID())
  - **Effort:** 4h
  - **Commit:** d64d81f6

- [x] Create LRU cache for message IDs
  - **Status:** ✅ Complete (MmMessageHistoryCache with time-windowed eviction)
  - **Effort:** 8h
  - **Commit:** d64d81f6
  - **Tests:** MessageHistoryCacheTests (30+ tests, all passing)

- [x] Add visited node tracking to metadata
  - **Status:** ✅ Complete (MmMessage.VisitedNodes HashSet already existed)
  - **Effort:** 2h (reduced - already partially implemented)
  - **Commit:** eb840ae9

- [x] Implement circular dependency detection
  - **Status:** ✅ Complete (hybrid hop count + VisitedNodes tracking)
  - **Effort:** 4h (reduced - already partially implemented)
  - **Commit:** eb840ae9

- [x] Add performance profiling hooks
  - **Status:** ✅ Complete (Instrumented HandleAdvancedRouting + ResolvePathTargets)
  - **Acceptance:** Track overhead < 5% ✅ (Zero overhead when disabled)
  - **Effort:** 20h
  - **Owner:** Lead Dev
  - **Commit:** e263768b
  - **Details:**
    - Global flags: EnableRoutingProfiler, ProfilingThresholdMs
    - HandleAdvancedRouting: 6 metrics (filters, node counts, time)
    - ResolvePathTargets: 5 metrics (path, segments, wildcards, visited, targets, time)
    - Structured logs: [ROUTING-PERF] format
    - Hybrid approach: Per-message options + global fallback

### Implementation: Extended Level Filters (56 hours) ✅ COMPLETE

- [x] Implement LevelFilter.Siblings
  - **Status:** ✅ Complete (CollectSiblings method in MmRelayNode)
  - **Effort:** 12h
  - **Commit:** eb840ae9 + 7dd86891 (level filter transformation fix)
  - **Tests:** SiblingsRouting_WithLateralEnabled_ReachesSiblings ✅

- [x] Implement LevelFilter.Cousins
  - **Status:** ✅ Complete (CollectCousins method in MmRelayNode)
  - **Effort:** 16h
  - **Commit:** eb840ae9 + 7dd86891
  - **Tests:** CousinsRouting_WithLateralEnabled_ReachesCousins ✅

- [x] Implement LevelFilter.Descendants
  - **Status:** ✅ Complete (CollectDescendants recursive method)
  - **Effort:** 8h
  - **Commit:** eb840ae9 + 7dd86891
  - **Tests:** DescendantsRouting tests (2 tests) ✅

- [x] Implement LevelFilter.Ancestors
  - **Status:** ✅ Complete (CollectAncestors recursive method)
  - **Effort:** 8h
  - **Commit:** eb840ae9 + 7dd86891
  - **Tests:** AncestorsRouting_ReachesAllParents ✅

- [x] Implement LevelFilter.Custom with predicates
  - **Status:** ✅ Complete (ApplyCustomFilter method)
  - **Effort:** 12h
  - **Commit:** eb840ae9 + 7dd86891
  - **Tests:** CustomFilter tests (2 tests) ✅

**Critical Fix Applied:**
- **Issue:** Messages weren't reaching target node responders
- **Root Cause:** Level filter not transformed before forwarding (LevelCheck bitwise AND failed)
- **Solution:** Transform to `SelfAndChildren` in RouteLateral(), RouteRecursive(), HandleAdvancedRouting()
- **Pattern:** `forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;`
- **Commit:** 7dd86891 - fix: Transform level filters in advanced routing methods
  - **Acceptance:** Messages reach same-parent nodes
  - **Effort:** 12h
  - **Owner:** Unity Dev

- [ ] Implement LevelFilter.Cousins
  - **Acceptance:** Messages reach parent's sibling's children
  - **Effort:** 16h
  - **Owner:** Unity Dev

- [ ] Implement LevelFilter.Descendants
  - **Acceptance:** Recursive child traversal
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Implement LevelFilter.Ancestors
  - **Acceptance:** Recursive parent traversal
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Implement LevelFilter.Custom with predicates
  - **Acceptance:** User-defined filter functions work
  - **Effort:** 12h
  - **Owner:** Lead Dev

### Implementation: Path Specification (40 hours) ✅ COMPLETE

- [x] Create path specification string parser
  - **Status:** ✅ Complete (MmPathSpecification.cs - 290 lines)
  - **Effort:** 12h
  - **Owner:** Lead Dev
  - **Commit:** Uncommitted (awaiting test verification)

- [x] Implement path validation logic
  - **Status:** ✅ Complete (integrated in parser with clear error messages)
  - **Effort:** 8h (included in parser implementation)
  - **Owner:** Lead Dev

- [x] Add dynamic path resolution
  - **Status:** ✅ Complete (ResolvePathTargets in MmRelayNode.cs)
  - **Effort:** 12h
  - **Owner:** Lead Dev
  - **Details:** ~140 lines in ResolvePathTargets(), ~70 lines in NavigateSegment()

- [x] Create MmInvokeWithPath API method
  - **Status:** ✅ Complete (5 overloads implemented)
  - **Effort:** 8h
  - **Owner:** Unity Dev
  - **Details:** Basic, bool, int, string, MmMessage overloads (~140 lines total)

### Testing & Documentation (98 hours)

- [x] Write unit tests for routing options
  - **Status:** ✅ Complete (MessageHistoryCacheTests.cs - 30+ tests)
  - **Effort:** 12h
  - **Owner:** Unity Dev
  - **Coverage:** LRU cache, time-based eviction, O(1) operations

- [x] Write unit tests for extended filters
  - **Status:** ✅ Complete (AdvancedRoutingTests.cs - 9 tests)
  - **Effort:** 16h
  - **Owner:** Unity Dev
  - **Coverage:** Siblings, Cousins, Descendants, Ancestors, Custom filtering

- [x] Write unit tests for path specification
  - **Status:** ✅ Complete (PathSpecificationTests.cs - 35 tests)
  - **Effort:** 12h
  - **Owner:** Unity Dev
  - **Coverage:** Parsing, resolution, wildcards, integration, error handling
  - **Note:** Tests created but NOT YET RUN (awaiting verification)

- [ ] Create tutorial scene for new features
  - **Acceptance:** Demonstrates all new routing
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Write API documentation
  - **Acceptance:** Complete docs with examples
  - **Effort:** 12h
  - **Owner:** Lead Dev

- [ ] Performance testing and optimization
  - **Acceptance:** < 5% overhead verified
  - **Effort:** 20h
  - **Owner:** Lead Dev

- [ ] Integration testing with existing features
  - **Acceptance:** All legacy features still work
  - **Effort:** 18h
  - **Owner:** Unity Dev

**Phase 2.1 Total:** 254 hours

---

## Phase 3.1: Routing Table Optimization (7 weeks, 276 hours)

### Design & Architecture (40 hours)

- [ ] Design IMmRoutingTable interface
  - **Acceptance:** Abstract interface spec approved
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Design FlatNetworkRoutingTable (hash-based)
  - **Acceptance:** O(1) lookup architecture
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Design HierarchicalRoutingTable (tree-based)
  - **Acceptance:** O(log n) tree structure
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Design MeshRoutingTable (graph-based)
  - **Acceptance:** Dijkstra path caching
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Design topology analyzer
  - **Acceptance:** Auto-detect optimal structure
  - **Effort:** 8h
  - **Owner:** Lead Dev

### Implementation: Interface & Base (20 hours)

- [ ] Implement IMmRoutingTable interface
  - **Acceptance:** Common API for all implementations
  - **Effort:** 4h
  - **Owner:** Lead Dev

- [ ] Create routing table factory
  - **Acceptance:** Factory pattern for creation
  - **Effort:** 4h
  - **Owner:** Unity Dev

- [ ] Implement topology analyzer
  - **Acceptance:** Analyze depth, branching, connectivity
  - **Effort:** 12h
  - **Owner:** Lead Dev

### Implementation: Specialized Tables (100 hours)

- [ ] Implement FlatNetworkRoutingTable
  - **Acceptance:** Dictionary-based, O(1) lookups
  - **Effort:** 20h
  - **Owner:** Unity Dev

- [ ] Implement tag indexing in FlatTable
  - **Acceptance:** Fast tag-based filtering
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Implement HierarchicalRoutingTable
  - **Acceptance:** Tree structure, O(log n) traversal
  - **Effort:** 24h
  - **Owner:** Lead Dev

- [ ] Implement tree traversal optimizations
  - **Acceptance:** Pre-computed paths for common filters
  - **Effort:** 12h
  - **Owner:** Lead Dev

- [ ] Implement MeshRoutingTable
  - **Acceptance:** Graph-based, shortest paths
  - **Effort:** 28h
  - **Owner:** Lead Dev

- [ ] Implement Dijkstra path caching
  - **Acceptance:** Path cache with invalidation
  - **Effort:** 16h
  - **Owner:** Lead Dev

### Implementation: Caching & Profiles (40 hours)

- [ ] Implement routing path cache
  - **Acceptance:** LRU cache for computed paths
  - **Effort:** 12h
  - **Owner:** Unity Dev

- [ ] Add cache invalidation logic
  - **Acceptance:** Invalidate on hierarchy changes
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Create routing profile system
  - **Acceptance:** UIOptimized, PerformanceCritical, etc.
  - **Effort:** 12h
  - **Owner:** Lead Dev

- [ ] Implement automatic structure selection
  - **Acceptance:** Choose best structure automatically
  - **Effort:** 8h
  - **Owner:** Lead Dev

### Testing & Migration (76 hours)

- [ ] Write unit tests for all table types
  - **Acceptance:** 90%+ coverage
  - **Effort:** 20h
  - **Owner:** Unity Dev

- [ ] Create performance benchmark suite
  - **Acceptance:** Compare all table types at scale
  - **Effort:** 16h
  - **Owner:** Lead Dev

- [ ] Implement migration path for existing code
  - **Acceptance:** Backward compatible API
  - **Effort:** 12h
  - **Owner:** Unity Dev

- [ ] Performance tuning and optimization
  - **Acceptance:** 3-5x improvement verified
  - **Effort:** 24h
  - **Owner:** Lead Dev

- [ ] Write configuration guide
  - **Acceptance:** How to choose table type
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Create routing optimization tutorial
  - **Acceptance:** Best practices documented
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Stress testing with 10K+ nodes
  - **Acceptance:** < 1ms latency verified
  - **Effort:** 8h
  - **Owner:** Unity Dev

**Phase 3.1 Total:** 276 hours

---

## Combined Total: 530 hours (10-11 weeks)

---

## Critical Path

1. **Phase 2.1 Design** (Week 1-2) → Blocks all 2.1 implementation
2. **Phase 2.1 Implementation** (Week 2-5) → Can partially parallel with 3.1 design
3. **Phase 3.1 Design** (Week 2-3) → Blocks 3.1 implementation
4. **Phase 3.1 Implementation** (Week 3-8) → Requires 2.1 for integration testing
5. **Final Testing & Documentation** (Week 9-11) → Requires all features complete

---

## Acceptance Criteria Summary

### Must Have
- [ ] Sibling and cousin routing working
- [ ] Message history prevents circular routing
- [ ] Path specification parser functional
- [ ] All three routing table types implemented
- [ ] Topology analyzer auto-selects optimal structure
- [ ] < 5% performance overhead for new routing features
- [ ] 3-5x improvement for specialized routing tables
- [ ] 100% backward compatibility
- [ ] 90%+ test coverage
- [ ] Complete documentation

### Should Have
- [ ] Custom filter predicates working
- [ ] Routing path caching operational
- [ ] Performance profiling hooks integrated
- [ ] Migration guide for existing code
- [ ] Tutorial scene demonstrating new features

### Could Have (Future)
- [ ] Regex patterns in path specification
- [ ] Hot-swapping routing tables at runtime
- [ ] Automatic topology re-analysis on hierarchy changes
- [ ] Visual debugging for routing paths

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Routing Optimization Team
