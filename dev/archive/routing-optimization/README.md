# Routing Optimization

**Status:** Ready to Start
**Priority:** CRITICAL
**Estimated Effort:** ~420 hours (10-11 weeks)
**Phase:** 2.1 (Advanced Message Routing) + 3.1 (Routing Table Optimization)

---

## Overview

This initiative addresses fundamental architectural limitations in Mercury's message routing system and optimizes routing table performance for large-scale applications.

**Core Problem:** Mercury currently cannot send messages to siblings, cousins, or lateral relationships. The routing table uses a generic structure that doesn't optimize for specific network topologies.

**Solution:** Extend the routing system to support flexible path specifications and implement specialized routing table structures for different network patterns.

---

## Goals

### Phase 2.1: Advanced Message Routing
1. Enable sibling and cousin message routing
2. Implement message history tracking to prevent circular dependencies
3. Add flexible path specification (e.g., "parent->sibling->child")
4. Create custom filter predicates for complex routing logic
5. Maintain < 5% performance overhead

### Phase 3.1: Routing Table Optimization
1. Create specialized routing tables for different topologies
2. Implement automatic topology detection and structure selection
3. Achieve 3-5x performance improvement for specialized patterns
4. Support 10,000+ nodes with < 1ms routing latency
5. Reduce memory overhead to < 100 bytes per node

---

## Scope

### In Scope
- Extended `MmLevelFilter` enum (Siblings, Cousins, Descendants, Ancestors, Custom)
- Message history tracking with LRU cache
- Path specification parser and validator
- Three specialized routing tables: Flat (O(1)), Hierarchical (O(log n)), Mesh (graph-based)
- Topology analyzer for automatic structure selection
- Routing path caching with invalidation
- Performance profiling hooks
- Complete backward compatibility

### Out of Scope
- Network synchronization features (covered in network-performance/)
- Visualization tools (covered in developer-tools/)
- Message batching and pooling (covered in network-performance/)
- Cross-platform ports (covered in cross-platform/)

---

## Dependencies

**Prerequisites:**
- None - this is a foundational enhancement

**Blockers:**
- None identified

**Parallel Work:**
- Can be developed in parallel with Phase 1 (UIST Paper)
- Should inform Phase 4.1 (XR Visualization) design

---

## Quick Start

### For Developers

1. **Read the context document first**
   - `routing-optimization-context.md` - Technical architecture and design decisions

2. **Review the task checklist**
   - `routing-optimization-tasks.md` - Specific implementation tasks with acceptance criteria

3. **Set up development environment**
   ```bash
   git checkout -b feature/routing-optimization
   # Create baseline performance benchmarks
   # Set up test scenes for different topologies
   ```

4. **Start with Phase 2.1 Task 1: MmRoutingOptions class**
   - See detailed specification in context document
   - Reference existing `MmMetadataBlock` for similar pattern

### Key Files to Modify

**Core Files:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (1422 lines) - Central router
- `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs` - Add routing options
- `Assets/MercuryMessaging/Protocol/MmLevelFilter.cs` - Extend enum
- `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs` - Create interface

**New Files to Create:**
- `Assets/MercuryMessaging/Protocol/MmRoutingOptions.cs`
- `Assets/MercuryMessaging/Protocol/IMmRoutingTable.cs`
- `Assets/MercuryMessaging/Protocol/FlatNetworkRoutingTable.cs`
- `Assets/MercuryMessaging/Protocol/HierarchicalRoutingTable.cs`
- `Assets/MercuryMessaging/Protocol/MeshRoutingTable.cs`
- `Assets/MercuryMessaging/Support/MmTopologyAnalyzer.cs`

---

## Success Metrics

### Performance Targets
- [ ] Sibling/cousin routing overhead < 5%
- [ ] 3-5x performance improvement for specialized tables
- [ ] 10,000 node network routed in < 1ms
- [ ] Memory overhead < 100 bytes per node
- [ ] 100% backward compatibility verified

### Quality Targets
- [ ] 90%+ unit test coverage
- [ ] Zero critical bugs in production
- [ ] Clear error messages for invalid routes
- [ ] No infinite loops or circular routing
- [ ] Complete API documentation

### Deliverables
- [ ] Enhanced MmRelayNode with extended routing
- [ ] Three specialized routing table implementations
- [ ] Topology analyzer with auto-detection
- [ ] Comprehensive test suite (50+ tests)
- [ ] Performance benchmark suite
- [ ] API documentation with examples
- [ ] Tutorial scene demonstrating new features

---

## Timeline

### Phase 2.1: Advanced Message Routing (6 weeks)
- **Week 1-2:** Design and core architecture
- **Week 2-3:** Message history tracking implementation
- **Week 3-4:** Extended level filters (Siblings, Cousins, etc.)
- **Week 4-5:** Path specification parser
- **Week 5-6:** Testing, optimization, documentation

### Phase 3.1: Routing Table Optimization (7 weeks)
- **Week 1-2:** Interface design and topology analyzer
- **Week 2-4:** Flat and Hierarchical table implementations
- **Week 4-6:** Mesh table and path caching
- **Week 6-7:** Performance testing, tuning, documentation

**Total Duration:** 10-11 weeks (can be parallelized with other work)

---

## Risk Assessment

### Technical Risks
- **Performance Degradation (Medium):** New features may add overhead
  - *Mitigation:* Continuous benchmarking, feature flags, < 5% overhead budget

- **Circular Routing Bugs (Medium):** Complex routing may create loops
  - *Mitigation:* Message history tracking, hop count limits, comprehensive testing

- **Backward Compatibility (Low):** Breaking existing implementations
  - *Mitigation:* Deprecation strategy, compatibility layer, extensive testing

### Project Risks
- **Scope Creep (Medium):** Feature requests expand implementation
  - *Mitigation:* Strict acceptance criteria, feature freeze dates

- **Testing Complexity (Medium):** Many edge cases to cover
  - *Mitigation:* Automated test suite, property-based testing

---

## Next Steps

1. **Week 1 Actions:**
   - [ ] Review and approve this plan
   - [ ] Assign team members
   - [ ] Create feature branch
   - [ ] Set up performance benchmarking
   - [ ] Begin Phase 2.1 Task 1: MmRoutingOptions design

2. **Ongoing:**
   - Weekly progress updates in context document
   - Daily standup during active development
   - Bi-weekly design reviews for architecture decisions

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Routing Optimization Team
**Related Docs:**
- `routing-optimization-context.md` - Technical details
- `routing-optimization-tasks.md` - Task checklist
