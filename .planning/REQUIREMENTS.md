# Requirements: MercuryMessaging UIST 2026

**Defined:** 2026-02-11
**Core Value:** Provide the most expressive and debuggable message routing system for Unity, with spatial-aware routing, distributed messaging semantics, fluent DSL, and asymmetry tolerance as core research contributions for UIST 2026.

## v1 Requirements

Requirements for UIST 2026 milestone. Each maps to roadmap phases.

### Tutorial Validation

- [ ] **TVAL-01**: All 12 tutorial scenes load and run without errors in Unity 6000.3.7f1
- [ ] **TVAL-02**: Wiki instructions match actual scene behavior for each tutorial
- [ ] **TVAL-03**: Keyboard controls and console output match wiki documentation
- [ ] **TVAL-04**: Code examples from wiki compile without modification
- [ ] **TVAL-05**: Validation report produced with pass/fail per tutorial and GitHub issues for failures

### Spatial Indexing

- [ ] **SPAT-01**: Adaptive octree spatial data structure integrated with Unity's scene graph
- [ ] **SPAT-02**: GPU-accelerated spatial queries via compute shaders (AABB, sphere, frustum, ray, k-NN)
- [ ] **SPAT-03**: CPU-first fallback for platforms without compute shader support (WebGL, mobile)
- [ ] **SPAT-04**: DSL integration: `Within()` and `InCone()` predicates backed by spatial index instead of linear scan
- [ ] **SPAT-05**: Level-of-Detail messaging system for distance-based message filtering
- [ ] **SPAT-06**: O(log n) query performance, <1ms for 100k objects target
- [ ] **SPAT-07**: Dynamic object support: efficient updates when objects move

### Distributed Messaging (scoped to MacIntyre Phases 1-2)

- [ ] **DMSG-01**: Distribution semantics (Replicated/Authoritative/LocalOnly) in fluent API
- [ ] **DMSG-02**: Structured change notification with pre/post callbacks and veto capability

### Asymmetry Analysis

- [ ] **ASYM-01**: Benchmark framework measuring message propagation under graph asymmetry across networked peers
- [ ] **ASYM-02**: Comparison against simulated pub/sub and direct reference approaches
- [ ] **ASYM-03**: Metrics: delivery correctness, latency overhead, structural divergence tolerance
- [ ] **ASYM-04**: Automated test generation for asymmetric graph configurations
- [ ] **ASYM-05**: Paper-ready figures and data tables for UIST submission

### User Study

- [ ] **USTY-01**: Within-subjects experimental design comparing Mercury/Cascade DSL vs Unity Events across representative task set
- [ ] **USTY-02**: N=20 participants with counterbalanced condition ordering and Latin-square task assignment
- [ ] **USTY-03**: Quantitative metrics collected: task completion time, error count, lines of code, compilation attempts
- [ ] **USTY-04**: Subjective instruments administered: NASA-TLX (workload) and SUS (usability) per condition
- [ ] **USTY-05**: Study infrastructure: training materials (validated tutorials), task scaffolding, data collection scripts, and IRB-ready protocol document

### Paper Polish

- [ ] **POLSH-03**: Unity Profiler integration via ProfilerCounter API for framework-specific metrics
- [ ] **POLSH-05**: Evaluation using Olsen (2007) framework: importance, generalizability, viscosity reduction
- [ ] **POLSH-06**: Paper draft with 4 contributions: C1 (user study), C2 (spatial routing), C3 (fluent DSL), C4 (asymmetry analysis)

## v2 Requirements

Deferred to Research Pipeline milestone. Tracked but not in current roadmap.

### Parallel FSM (deferred from UIST 2026)

- **PFSM-01**: Multiple concurrent FSMs (orthogonal state regions) on a single MmRelaySwitchNode
- **PFSM-02**: Message-based synchronization between parallel FSMs (no shared memory)
- **PFSM-03**: Priority-based conflict resolution for competing multimodal inputs
- **PFSM-04**: Three-layer architecture: Interaction (Gesture/Voice/Gaze), Application (Navigation/Manipulation/Menu), System (Network/Performance/Error)
- **PFSM-05**: Cooperative single-thread execution model (NOT Parallel.ForEach -- Unity API safety)
- **PFSM-06**: Optional Jobs-based parallel mode for data-only responders
- **PFSM-07**: Custom Inspector editor displaying parallel FSM state for each region on the corresponding node

### Introspection Infrastructure (deferred from UIST 2026)

- **INSP-01**: Per-responder rejection reason tracking during message routing (why a message was filtered at each hop)
- **INSP-02**: IntrospectionMode flag gating the overhead (like PerformanceMode but for debugging data)
- **INSP-03**: MmIntrospectionHook interface for external systems (Visual Composer, Time-Travel) to observe routing decisions
- **INSP-04**: Zero overhead when IntrospectionMode is disabled (no performance regression)

### Visual Composer (deferred from UIST 2026)

- **VCOMP-01**: Node-graph editor displaying MmRelayNode hierarchy as a directed graph
- **VCOMP-02**: Live message flow animation with color-coded filter types (Level, Active, Tag, Network)
- **VCOMP-03**: Bi-directional editing: graph changes update scene hierarchy and vice versa
- **VCOMP-04**: Blockage indicators showing WHY messages were filtered at each routing hop (novel -- no prior art)
- **VCOMP-05**: Inspector integration showing responder state, tag configuration, and filter settings
- **VCOMP-06**: Runtime topology manipulation during gameplay (add/remove/reconnect nodes)
- **VCOMP-07**: 3D overlay visualization of message paths in Scene view (Custom GL rendering)
- **VCOMP-08**: GraphViewBase-based implementation compatible with Unity 6000.3.7f1

### Time-Travel Debugging (deferred from UIST 2026)

- **TTD-01**: Record all MmInvoke calls with full metadata, routing decisions, and filter outcomes
- **TTD-02**: Timeline scrubber UI to step through message history forward and backward
- **TTD-03**: "Why didn't this message reach responder X?" query engine with filter decision trace at each hop
- **TTD-04**: Message replay: re-send recorded messages through current hierarchy to test changes
- **TTD-05**: Tiered recording levels (minimal/standard/full) with configurable overhead budget
- **TTD-06**: Export/import message traces for sharing and reproduction

### Visual Composer Visualization (deferred from UIST 2026)

- **POLSH-01**: Sequence diagram visualization of message propagation paths
- **POLSH-02**: Message frequency heatmap overlay in Visual Composer

### Distributed Messaging (remaining scope -- Phases 3-4 of MacIntyre plan)

- **DMSG-03**: Multi-object atomic messaging with transaction API
- **DMSG-04**: Network topology awareness with latency-aware routing

### User Study (CHI LBW -- separate from UIST)

- **USDY-01**: Mercury vs Unity Events comparison study with 5 scene types
- **USDY-02**: N=15-20 participants, counterbalanced design, NASA-TLX + quantitative metrics

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
- **STAT-02**: Runtime safety with correctly-sized Bloom filter (NOT 128-bit -- research flagged sizing error)

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
| POLSH-04 (old visual tool comparison study) | Replaced by USTY-01..05 (Mercury DSL vs Unity Events) to match paper C1 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| TVAL-01 | Phase 1: Tutorial Validation | Pending |
| TVAL-02 | Phase 1: Tutorial Validation | Pending |
| TVAL-03 | Phase 1: Tutorial Validation | Pending |
| TVAL-04 | Phase 1: Tutorial Validation | Pending |
| TVAL-05 | Phase 1: Tutorial Validation | Pending |
| SPAT-01 | Phase 2: Spatial Indexing | Pending |
| SPAT-02 | Phase 2: Spatial Indexing | Pending |
| SPAT-03 | Phase 2: Spatial Indexing | Pending |
| SPAT-04 | Phase 2: Spatial Indexing | Pending |
| SPAT-05 | Phase 2: Spatial Indexing | Pending |
| SPAT-06 | Phase 2: Spatial Indexing | Pending |
| SPAT-07 | Phase 2: Spatial Indexing | Pending |
| DMSG-01 | Phase 3: Distributed Messaging | Pending |
| DMSG-02 | Phase 3: Distributed Messaging | Pending |
| ASYM-01 | Phase 4: Asymmetry Analysis | Pending |
| ASYM-02 | Phase 4: Asymmetry Analysis | Pending |
| ASYM-03 | Phase 4: Asymmetry Analysis | Pending |
| ASYM-04 | Phase 4: Asymmetry Analysis | Pending |
| ASYM-05 | Phase 4: Asymmetry Analysis | Pending |
| USTY-01 | Phase 5: User Study | Pending |
| USTY-02 | Phase 5: User Study | Pending |
| USTY-03 | Phase 5: User Study | Pending |
| USTY-04 | Phase 5: User Study | Pending |
| USTY-05 | Phase 5: User Study | Pending |
| POLSH-03 | Phase 6: Paper Polish | Pending |
| POLSH-05 | Phase 6: Paper Polish | Pending |
| POLSH-06 | Phase 6: Paper Polish | Pending |

**Coverage:**
- v1 requirements: 27 total
- Mapped to phases: 27
- Unmapped: 0

---
*Requirements defined: 2026-02-11*
*Last updated: 2026-02-12 -- Revised: Parallel FSM (PFSM-01..07) moved to v2. DMSG-01, DMSG-02 promoted to v1. Old POLSH-04 replaced by new User Study requirements (USTY-01..05). User Study and Paper Polish split into separate phases. Paper contributions reframed as C1-C4 matching main.tex. Count changed from 28 to 27.*
