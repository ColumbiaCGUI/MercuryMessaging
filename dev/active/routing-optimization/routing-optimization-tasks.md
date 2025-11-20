# Routing Optimization - Task Checklist

**Last Updated:** 2025-11-18

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

### Design & Architecture (24 hours)

- [ ] Create MmRoutingOptions class specification
  - **Acceptance:** API design document approved
  - **Effort:** 4h
  - **Owner:** Lead Dev

- [ ] Design message history tracking system
  - **Acceptance:** Architecture diagram approved
  - **Effort:** 8h
  - **Owner:** Lead Dev

- [ ] Design extended level filters architecture
  - **Acceptance:** Enum and logic flow designed
  - **Effort:** 4h
  - **Owner:** Lead Dev

- [ ] Design path specification parser
  - **Acceptance:** Grammar and parser design
  - **Effort:** 8h
  - **Owner:** Lead Dev

### Implementation: Message History (36 hours)

- [ ] Implement message ID generation
  - **Acceptance:** Unique IDs per message
  - **Effort:** 4h
  - **Owner:** Unity Dev

- [ ] Create LRU cache for message IDs
  - **Acceptance:** Cache with configurable size
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Add visited node tracking to metadata
  - **Acceptance:** List of visited node IDs in message
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Implement circular dependency detection
  - **Acceptance:** Prevents infinite loops
  - **Effort:** 8h
  - **Owner:** Unity Dev

- [ ] Add performance profiling hooks
  - **Acceptance:** Track overhead < 5%
  - **Effort:** 8h
  - **Owner:** Lead Dev

### Implementation: Extended Level Filters (56 hours)

- [ ] Implement LevelFilter.Siblings
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
