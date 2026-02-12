# Message Propagation Under Graph Asymmetry

**Status:** Planning
**Priority:** HIGH (UIST 2026 Paper Contribution C4)
**Estimated Effort:** ~40 hours (1 week)
**Target Venues:** UIST 2026 (supports paper contribution C4)

---

## Research Contribution

### Problem Statement

In networked XR/HRI applications, scene graphs across peers frequently diverge:
- **Missing nodes**: One peer lacks nodes the other expects (e.g., disconnected sensor)
- **Extra nodes**: One peer has nodes the other doesn't know about (e.g., new instrument added)
- **Structural divergence**: Same logical entity, different subtree structure (e.g., operator vs robot view)

Current messaging approaches (flat pub/sub, direct references) require explicit per-topic subscriptions or exact object knowledge. When graphs diverge, these approaches silently fail or require complex synchronization.

### Key Insight: Structural Routing Advantage

Cascade's hierarchical `ToDescendants` routing follows the **receiver's** local hierarchy, not the sender's. This means:
- **Extra nodes** under a relay automatically receive broadcast messages without sender knowledge
- **Missing nodes** degrade gracefully (warning log, no crash)
- **Structural divergence** is partially tolerated via relay-level routing

This is fundamentally different from pub/sub (sender must know all topics) and direct references (sender must know all targets).

---

## Implementation Plan

### Phase 1: Benchmark Framework (15 hours)

Build a C# benchmark within the Mercury test suite that models asymmetric scene graphs.

**Tasks:**
1. Create `AsymmetryBenchmark.cs` in test project
2. Implement three asymmetry scenarios:
   - **Scenario A (Missing Nodes)**: Receiver lacks 10-50% of sender's leaf nodes
   - **Scenario B (Extra Nodes)**: Receiver has 10-50% additional nodes under relays
   - **Scenario C (Mixed Divergence)**: Combination of removals and additions
3. Compare three routing strategies:
   - **Cascade `ToDescendants`**: Routes through receiver's hierarchy (reaches extra nodes)
   - **Simulated Pub/Sub**: Only sender-known topics published
   - **Simulated Direct**: Only sender-known targets addressed
4. Metrics to capture:
   - Delivery rate (intended messages reaching targets)
   - Extra node coverage (messages reaching nodes sender doesn't know about)
   - Message loss rate per scenario
   - Routing overhead (time per message)

### Phase 2: HRI Scene Graph Model (10 hours)

Create realistic HRI scene graphs for benchmark scenarios.

**Tasks:**
1. Model operator-side hierarchy (UI panels, camera feeds, alerts)
2. Model robot-side hierarchy (joints, sensors, effectors)
3. Model safety system hierarchy (zones, proximity alerts)
4. Model environment hierarchy (waypoints, obstacles)
5. Implement asymmetry generators:
   - `RemoveRandomLeaves(percentage)` for Scenario A
   - `AddExtraNodes(percentage)` for Scenario B
   - `MixedDivergence(percentage)` for Scenario C

### Phase 3: Network Integration Test (10 hours)

Test actual MmNetworkBridge behavior with asymmetric graphs.

**Tasks:**
1. Create two-peer test setup with divergent hierarchies
2. Verify `RouteMessage` handles missing relay nodes gracefully
3. Measure actual message propagation across MmNetworkBridge
4. Test MmRelayNode.MmInvoke with asymmetric subtrees
5. Profile memory/GC impact of silent node drops

### Phase 4: Results and Paper Integration (5 hours)

**Tasks:**
1. Generate benchmark results tables
2. Create figure showing delivery rates across asymmetry levels
3. Write up findings for UIST paper Section 5.5
4. Verify claims match actual codebase behavior

---

## Existing Codebase References

- **MmNetworkBridge.cs** (`RouteMessage`, lines 386-412): Handles missing nodes with warning log + silent drop
- **MmRelayNode.cs** (`MmInvoke`, lines 805-905): Core router for hierarchical dispatch
- **MmNetworkResponder.cs**: Legacy networking (CHI 2018 era)
- **Two network architectures coexist**: Legacy MmNetworkResponder and new MmNetworkBridge (2024-2025)
- Current missing node behavior: "No relay node found for network ID {X}" warning, no retry

---

## Prototype Simulation (Python)

A Python prototype was developed during paper writing to validate the benchmark design. Key findings from the prototype:

- **Scenario A (missing nodes)**: All strategies degrade equally (expected)
- **Scenario B (extra nodes)**: Cascade's structural routing reaches extra nodes that pub/sub/direct cannot
- **Scenario C (mixed)**: Cascade maintains higher effective coverage via extra node reach

The prototype is preserved in `dev/active/asymmetry-analysis/prototype/asymmetry_benchmark.py`.

---

## Success Metrics

- [ ] Benchmark runs in < 30 seconds for full parameter sweep
- [ ] Clear quantitative difference between Cascade and alternatives in Scenario B
- [ ] Results reproducible with fixed random seed
- [ ] Findings match paper claims in Section 5.5
- [ ] Network integration tests pass with asymmetric graphs

---

## Related Work: MacIntyre's "Local Variations" (1998)

MacIntyre's Repo-3D library (1998 Columbia dissertation) introduced the concept of **local variations** to shared scene graph state, allowing each site to locally modify replicated graphical objects. His system addressed "the frequent need for local variations to the global scene" (Section 5.3.2, p.133).

Cascade's graph asymmetry analysis is a direct descendant of this concept. Where MacIntyre allowed per-site modifications to properties of shared objects, Cascade handles structural divergence (missing/extra/rearranged nodes) across networked peers.

**See also:** `dev/active/distributed-messaging/` for distribution semantics (Replicated/Authoritative/LocalOnly modes) that determine how asymmetric graphs are handled at the message routing level.

---

## Dependencies

- MercuryMessaging core framework
- MercuryMessaging Network (for Phase 3)
- Unity Test Framework (for benchmark harness)
- Related: `dev/active/distributed-messaging/` Phase 1 (distribution semantics affect asymmetry behavior)

---

*Created: 2026-02-11*
*Last Updated: 2026-02-11*
