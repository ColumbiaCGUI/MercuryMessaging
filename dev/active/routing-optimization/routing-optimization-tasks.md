# Routing Optimization - Task Checklist

**Last Updated:** 2025-11-21

---

## Phase 2.1 Progress: 116h / 254h (45.7% Complete) ✅

**Session 2025-11-21 Achievements:**
- ✅ Completed foundation classes (60h)
- ✅ Completed routing logic implementation (56h)
- ✅ Fixed 3 critical bugs (level filter transformation, test hierarchy, compilation)
- ✅ All 159 tests passing (100%)

**Next Priority:** Path specification parser (40h remaining in Phase 2.1)

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

- [ ] Add performance profiling hooks
  - **Status:** ⏳ Not Started
  - **Acceptance:** Track overhead < 5%
  - **Effort:** 20h
  - **Owner:** Lead Dev
  - **Note:** MmRoutingOptions.EnableProfiling exists but not integrated

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

### Implementation: Path Specification (40 hours)

- [ ] Create path specification string parser
  - **Acceptance:** Parse "parent->sibling->child"
  - **Effort:** 12h
  - **Owner:** Lead Dev

- [ ] Implement path validation logic
  - **Acceptance:** Detect invalid paths, clear errors
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Add dynamic path resolution
  - **Acceptance:** Resolve paths at runtime
  - **Effort:** 12h
  - **Owner:** Lead Dev

- [ ] Create MmInvokeWithPath API method
  - **Acceptance:** New API works alongside existing
  - **Effort:** 8h
  - **Owner:** Unity Dev

### Testing & Documentation (98 hours)

- [ ] Write unit tests for routing options
  - **Acceptance:** 90%+ coverage
  - **Effort:** 12h
  - **Owner:** Unity Dev

- [ ] Write unit tests for extended filters
  - **Acceptance:** 90%+ coverage
  - **Effort:** 16h
  - **Owner:** Unity Dev

- [ ] Write unit tests for path specification
  - **Acceptance:** 90%+ coverage
  - **Effort:** 12h
  - **Owner:** Unity Dev

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
