# Requirements: MercuryMessaging UIST 2026

**Defined:** 2026-02-11
**Core Value:** Provide the most expressive and debuggable message routing system for Unity, with visual authoring and introspection tools that have no prior art.

## v1 Requirements

Requirements for UIST 2026 milestone. Each maps to roadmap phases.

### Tutorial Validation

- [ ] **TVAL-01**: All 12 tutorial scenes load and run without errors in Unity 6000.3.7f1
- [ ] **TVAL-02**: Wiki instructions match actual scene behavior for each tutorial
- [ ] **TVAL-03**: Keyboard controls and console output match wiki documentation
- [ ] **TVAL-04**: Code examples from wiki compile without modification
- [ ] **TVAL-05**: Validation report produced with pass/fail per tutorial and GitHub issues for failures

### Introspection Infrastructure

- [ ] **INSP-01**: Per-responder rejection reason tracking during message routing (why a message was filtered at each hop)
- [ ] **INSP-02**: IntrospectionMode flag gating the overhead (like PerformanceMode but for debugging data)
- [ ] **INSP-03**: MmIntrospectionHook interface for external systems (Visual Composer, Time-Travel) to observe routing decisions
- [ ] **INSP-04**: Zero overhead when IntrospectionMode is disabled (no performance regression)

### Visual Composer

- [ ] **VCOMP-01**: Node-graph editor displaying MmRelayNode hierarchy as a directed graph
- [ ] **VCOMP-02**: Live message flow animation with color-coded filter types (Level, Active, Tag, Network)
- [ ] **VCOMP-03**: Bi-directional editing: graph changes update scene hierarchy and vice versa
- [ ] **VCOMP-04**: Blockage indicators showing WHY messages were filtered at each routing hop (novel — no prior art)
- [ ] **VCOMP-05**: Inspector integration showing responder state, tag configuration, and filter settings
- [ ] **VCOMP-06**: Runtime topology manipulation during gameplay (add/remove/reconnect nodes)
- [ ] **VCOMP-07**: 3D overlay visualization of message paths in Scene view (Custom GL rendering)
- [ ] **VCOMP-08**: GraphViewBase-based implementation compatible with Unity 6000.3.7f1

### Time-Travel Debugging

- [ ] **TTD-01**: Record all MmInvoke calls with full metadata, routing decisions, and filter outcomes
- [ ] **TTD-02**: Timeline scrubber UI to step through message history forward and backward
- [ ] **TTD-03**: "Why didn't this message reach responder X?" query engine with filter decision trace at each hop
- [ ] **TTD-04**: Message replay: re-send recorded messages through current hierarchy to test changes
- [ ] **TTD-05**: Tiered recording levels (minimal/standard/full) with configurable overhead budget
- [ ] **TTD-06**: Export/import message traces for sharing and reproduction

### Spatial Indexing

- [ ] **SPAT-01**: Adaptive octree spatial data structure integrated with Unity's scene graph
- [ ] **SPAT-02**: GPU-accelerated spatial queries via compute shaders (AABB, sphere, frustum, ray, k-NN)
- [ ] **SPAT-03**: CPU-first fallback for platforms without compute shader support (WebGL, mobile)
- [ ] **SPAT-04**: DSL integration: `Within()` and `InCone()` predicates backed by spatial index instead of linear scan
- [ ] **SPAT-05**: Level-of-Detail messaging system for distance-based message filtering
- [ ] **SPAT-06**: O(log n) query performance, <1ms for 100k objects target
- [ ] **SPAT-07**: Dynamic object support: efficient updates when objects move

### Parallel FSM

- [ ] **PFSM-01**: Multiple concurrent FSMs (orthogonal state regions) on a single MmRelaySwitchNode
- [ ] **PFSM-02**: Message-based synchronization between parallel FSMs (no shared memory)
- [ ] **PFSM-03**: Priority-based conflict resolution for competing multimodal inputs
- [ ] **PFSM-04**: Three-layer architecture: Interaction (Gesture/Voice/Gaze), Application (Navigation/Manipulation/Menu), System (Network/Performance/Error)
- [ ] **PFSM-05**: Cooperative single-thread execution model (NOT Parallel.ForEach — Unity API safety)
- [ ] **PFSM-06**: Optional Jobs-based parallel mode for data-only responders
- [ ] **PFSM-07**: Visual Composer integration showing parallel FSM state in graph editor

### Asymmetry Analysis

- [ ] **ASYM-01**: Benchmark framework measuring message propagation under graph asymmetry across networked peers
- [ ] **ASYM-02**: Comparison against simulated pub/sub and direct reference approaches
- [ ] **ASYM-03**: Metrics: delivery correctness, latency overhead, structural divergence tolerance
- [ ] **ASYM-04**: Automated test generation for asymmetric graph configurations
- [ ] **ASYM-05**: Paper-ready figures and data tables for UIST submission

### Paper Polish & Visualization

- [ ] **POLSH-01**: Sequence diagram visualization of message propagation paths
- [ ] **POLSH-02**: Message frequency heatmap overlay in Visual Composer
- [ ] **POLSH-03**: Unity Profiler integration via ProfilerCounter API for framework-specific metrics
- [ ] **POLSH-04**: User study with N>=12 Unity developers comparing visual tool vs Inspector-only workflow
- [ ] **POLSH-05**: Evaluation using Olsen (2007) framework: importance, generalizability, viscosity reduction
- [ ] **POLSH-06**: Paper draft with 2-3 foregrounded contributions (blockage indicators + time-travel as recommended core)

## v2 Requirements

Deferred to Research Pipeline milestone. Tracked but not in current roadmap.

### User Study (CHI LBW)

- **USDY-01**: Mercury vs Unity Events comparison study with 5 scene types
- **USDY-02**: N=15-20 participants, counterbalanced design, NASA-TLX + quantitative metrics

### Distributed Messaging

- **DMSG-01**: Distribution semantics (Replicated/Authoritative/LocalOnly) in fluent API
- **DMSG-02**: Structured change notification with pre/post callbacks and veto capability
- **DMSG-03**: Multi-object atomic messaging with transaction API
- **DMSG-04**: Network topology awareness with latency-aware routing

### Accessibility Framework

- **ACCS-01**: Tag-based accessibility modes (Visual, Auditory, Motor, Cognitive)
- **ACCS-02**: Alternative input routing (voice, switch, eye tracking, gesture to Mercury messages)

### Digital Twin Layer

- **DTWIN-01**: MQTT/Mercury bridge for IoT sensor integration
- **DTWIN-02**: Hierarchical filtering for targeted digital twin updates

### XR Collaboration

- **XRCOL-01**: Multi-user XR session management via Mercury hierarchy
- **XRCOL-02**: Tag-based role permissions (Instructor, Student, Observer, Admin)

### LLM Message Design

- **LLMD-01**: Natural language to Mercury routing configuration generation
- **LLMD-02**: Semantic validation against Mercury routing rules

### Static Analysis

- **STAT-01**: Compile-time cycle detection via Tarjan's SCC
- **STAT-02**: Runtime safety with correctly-sized Bloom filter (NOT 128-bit — research flagged sizing error)

### Parallel Dispatch

- **PDSP-01**: Thread-safe concurrent message processing with work-stealing queues
- **PDSP-02**: Unity Job System/Burst compilation for parallel dispatch

## Out of Scope

| Feature | Reason |
|---------|--------|
| Full Parallel.ForEach threading for FSMs | Unity API is single-threaded; crashes on Transform/GameObject access. Use cooperative model instead. |
| Atomic messaging (MacIntyre Phase 3) | High complexity, deferred to Research Pipeline |
| Network topology awareness (MacIntyre Phase 4) | Deferred to Research Pipeline |
| WebGL GPU compute shaders | Platform limitation; CPU fallback covers this case |
| Deterministic replay (rr-style) | 10x effort for minimal research value; message-centric approach sufficient |
| Commercial packaging/Asset Store release | Research-first, open source |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| TVAL-01 through TVAL-05 | Phase 1 | Pending |
| INSP-01 through INSP-04 | Phase 2 | Pending |
| VCOMP-01 through VCOMP-08 | Phase 3 | Pending |
| TTD-01 through TTD-06 | Phase 4 | Pending |
| SPAT-01 through SPAT-07 | Phase 5 | Pending |
| PFSM-01 through PFSM-07 | Phase 6 | Pending |
| ASYM-01 through ASYM-05 | Phase 7 | Pending |
| POLSH-01 through POLSH-06 | Phase 8 | Pending |

**Coverage:**
- v1 requirements: 42 total
- Mapped to phases: 42
- Unmapped: 0

---
*Requirements defined: 2026-02-11*
*Last updated: 2026-02-11 after initial definition*
