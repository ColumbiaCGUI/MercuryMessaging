# Roadmap: MercuryMessaging UIST 2026

## Overview

This roadmap delivers the MercuryMessaging UIST 2026 paper through 6 phases, aligned with the paper's 4 contributions: C1 (first empirical evaluation of hierarchical messaging, user study N=20), C2 (spatial-aware message routing primitives), C3 (fluent internal DSL for hierarchical messaging), and C4 (analysis of message propagation under graph asymmetry). The roadmap begins with tutorial validation as a prerequisite for the user study, then delivers spatial indexing (C2) and distributed messaging (C4 extension) in parallel, followed by asymmetry analysis (C4), user study (C1), and paper polish. The fluent DSL (C3) is already shipped in v4.0.0 and strengthened by the new spatial and distributed extensions.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Tutorial Validation** - Verify all 12 tutorial scenes against wiki documentation
- [ ] **Phase 2: Spatial Indexing** - Adaptive octree with GPU-accelerated queries and DSL integration
- [ ] **Phase 3: Distributed Messaging** - Distribution semantics and change notification (MacIntyre Phases 1-2 only)
- [ ] **Phase 4: Asymmetry Analysis** - Benchmark framework for graph asymmetry across networked peers
- [ ] **Phase 5: User Study** - Mercury/Cascade DSL vs Unity Events within-subjects comparison (N=20)
- [ ] **Phase 6: Paper Polish** - Profiler integration, Olsen framework evaluation, submission-ready draft
- [ ] **Phase 7: VR Visual Composer** - Immersive VR debugging and editing of Mercury networks with blockage indicators

## Phase Details

### Phase 1: Tutorial Validation
**Goal**: Every tutorial scene runs cleanly and matches its wiki documentation, establishing a trustworthy foundation for the user study training materials
**Depends on**: Nothing (first phase)
**Requirements**: TVAL-01, TVAL-02, TVAL-03, TVAL-04, TVAL-05
**Success Criteria** (what must be TRUE):
  1. All 12 tutorial scenes load and run in Unity 6000.3.7f1 without errors or warnings in the console
  2. A developer following the wiki step-by-step for any tutorial reproduces the documented behavior (keyboard controls, console output, visual results)
  3. All code examples from the wiki compile as-is when pasted into a fresh script file
  4. A validation report exists listing pass/fail per tutorial with GitHub issues filed for every failure
**Plans**: 4 plans

Plans:
- [ ] 01-01-PLAN.md — Environment setup + static pre-analysis of all 12 tutorials + user mismatch decisions
- [ ] 01-02-PLAN.md — Apply fixes and validate tutorials 1-5 (Introduction, Routing, Responders, Messages, DSL)
- [ ] 01-03-PLAN.md — Apply fixes and validate tutorials 6-7 (FishNet, Fusion 2 networking)
- [ ] 01-04-PLAN.md — Apply fixes and validate tutorials 8-12 + stubs 13-14 + compile VALIDATION_REPORT.md

### Phase 2: Spatial Indexing
**Goal**: Spatial queries (radius, frustum, cone, ray, k-NN) execute in O(log n) time via an adaptive octree, replacing linear scans in the fluent DSL predicates and strengthening paper contribution C2
**Depends on**: Phase 1
**Requirements**: SPAT-01, SPAT-02, SPAT-03, SPAT-04, SPAT-05, SPAT-06, SPAT-07
**Success Criteria** (what must be TRUE):
  1. The fluent DSL predicates `Within()` and `InCone()` use the spatial index instead of linear scan, returning the same results with O(log n) query time
  2. GPU-accelerated spatial queries via compute shaders run AABB, sphere, frustum, ray, and k-NN queries, with a CPU fallback that produces identical results on platforms without compute shader support
  3. A benchmark with 100k objects demonstrates query times under 1ms for all supported query types
  4. Moving objects update their position in the spatial index efficiently without full rebuild, and the index stays correct across frames
  5. Level-of-Detail messaging filters messages by distance, delivering full-detail messages to nearby responders and reduced-detail or no messages to distant ones
**Plans**: TBD

Plans:
- [ ] 02-01: TBD
- [ ] 02-02: TBD
- [ ] 02-03: TBD

### Phase 3: Distributed Messaging
**Goal**: Messages carry explicit distribution semantics (Replicated, Authoritative, LocalOnly) through the fluent API, and responders can observe and veto property changes, extending paper contribution C4 with distributed context
**Depends on**: Phase 1 (can run in PARALLEL with Phase 2)
**Requirements**: DMSG-01, DMSG-02
**Success Criteria** (what must be TRUE):
  1. A developer can annotate a fluent message with `.Replicated()`, `.Authoritative()`, or `.LocalOnly()` and the message is routed according to its distribution mode (replicated delivers to all peers, authoritative delivers only from owner, local-only stays on the originating machine)
  2. A responder can register pre-change and post-change callbacks on observable properties, and a pre-change callback returning false vetoes the change before it propagates
  3. Distribution semantics compose correctly with existing Mercury filters (Level, Active, Tag, Network) without breaking any existing routing behavior
**Plans**: TBD

Plans:
- [ ] 03-01: TBD

### Phase 4: Asymmetry Analysis
**Goal**: Paper-ready benchmark data showing how MercuryMessaging handles graph asymmetry across networked peers compared to pub/sub and direct reference approaches, directly implementing paper contribution C4
**Depends on**: Phase 1; benefits from Phase 3 (distributed messaging adds richer evaluation context)
**Requirements**: ASYM-01, ASYM-02, ASYM-03, ASYM-04, ASYM-05
**Success Criteria** (what must be TRUE):
  1. A benchmark framework generates asymmetric graph configurations (10-50% structural divergence between peers) and measures delivery correctness, latency overhead, and structural divergence tolerance
  2. Side-by-side comparison data exists for Mercury cascade routing vs simulated pub/sub vs direct references, showing Mercury's behavior under graph asymmetry
  3. Paper-ready figures (charts, data tables) are exported in a format suitable for UIST submission
**Plans**: TBD

Plans:
- [ ] 04-01: TBD

### Phase 5: User Study
**Goal**: A within-subjects experiment (N=20) comparing Mercury/Cascade DSL vs Unity Events produces quantitative task metrics and subjective workload/usability data, directly implementing paper contribution C1
**Depends on**: Phase 1 (validated tutorials serve as training materials); can run in PARALLEL with Phases 2, 3, 4
**Requirements**: USTY-01, USTY-02, USTY-03, USTY-04, USTY-05
**Success Criteria** (what must be TRUE):
  1. A within-subjects experimental protocol exists with counterbalanced condition ordering, representative tasks, and IRB approval
  2. N=20 participants each complete tasks using both Mercury DSL and Unity Events, with task completion time, error count, lines of code, and compilation attempts recorded automatically
  3. NASA-TLX and SUS questionnaires are administered after each condition and results are exported in analysis-ready format (CSV or equivalent)
  4. Training materials derived from validated tutorials (Phase 1) bring participants to baseline competency in both conditions before timed tasks begin
**Plans**: TBD

Plans:
- [ ] 05-01: TBD
- [ ] 05-02: TBD

### Phase 6: Paper Polish
**Goal**: The UIST paper is submission-ready with all 4 contributions (C1: user study, C2: spatial routing, C3: fluent DSL, C4: asymmetry analysis) integrated, profiler data collected, and evaluation framed using Olsen (2007)
**Depends on**: Phase 2 (spatial indexing data), Phase 3 (distributed messaging data), Phase 4 (asymmetry data), Phase 5 (user study data)
**Requirements**: POLSH-03, POLSH-05, POLSH-06
**Success Criteria** (what must be TRUE):
  1. Unity Profiler shows MercuryMessaging-specific metrics (message throughput, routing time, cache hit rate, spatial query time) via ProfilerCounter API
  2. The paper draft evaluates contributions through the Olsen (2007) framework, articulating importance, generalizability, and viscosity reduction for each contribution
  3. A complete paper draft integrates user study results (C1), spatial routing benchmarks (C2), DSL design rationale (C3), and asymmetry analysis data (C4) into a coherent UIST submission
**Plans**: TBD

Plans:
- [ ] 06-01: TBD
- [ ] 06-02: TBD
- [ ] 06-03: TBD

### Phase 7: VR Visual Composer
**Goal**: A progressive visualization and editing tool (2D → 3D Scene View → VR) for viewing, debugging, and bi-directionally editing MercuryMessaging networks, with blockage indicators explaining WHY messages were filtered and responder capability introspection showing what each component can handle
**Depends on**: Phase 1 (validated tutorials for user study training); can run in PARALLEL with Phases 2-6
**Requirements**: VCOMP-01 (introspection), VCOMP-02 (2D editor), VCOMP-03 (3D scene view), VCOMP-04 (VR rendering), VCOMP-05 (VR editing), VCOMP-06 (time-travel), VCOMP-07 (user study)
**Design Doc**: [docs/plans/2026-02-19-vr-visual-composer-design.md](../../docs/plans/2026-02-19-vr-visual-composer-design.md)
**Success Criteria** (what must be TRUE):
  1. Introspection subsystem captures 6 event streams (routing decisions with full filter audit, topology changes, FSM transitions, network events, per-node metrics, responder capabilities) with zero overhead when MmIntrospectionMode is off
  2. 2D graph editor window shows the Mercury network topology with live message animation, blockage indicators, responder capability badges, and bi-directional editing (graph changes update scene and vice versa)
  3. 3D Scene View renders connection lines between MmRelayNodes overlaid on actual GameObjects with message flow animation and blockage markers
  4. In VR, a developer can see the network as scene-anchored overlays with a detachable overview graph, live message particles, and spatial blockage explanation cards
  5. A developer can create/delete nodes, wire connections, change filters/tags, and fire test messages entirely in VR, with changes reflected in the Unity scene
  6. Timeline scrubber replays recorded message history with historical blockage state, FSM transitions, and topology changes
  7. User study (N>=12) shows measurable improvement in debugging task completion time vs Inspector-only workflow
**Plans**: 7 sub-phases (rendering ladder)

Plans:
- [ ] 07-01-PLAN.md — Introspection Subsystem: 6 event streams, MmIntrospectionBus, MmMessageRecorder, MmTopologySnapshot, responder capability reflection
- [ ] 07-02-PLAN.md — 2D Graph Editor: GraphViewBase editor window, node-link graph, live animation, blockage indicators, capability badges, bi-directional editing, search/filter
- [ ] 07-03-PLAN.md — 3D Scene View: GL/ALINE connection lines in Scene View, message particles, blockage markers, node labels
- [ ] 07-04-PLAN.md — VR Rendering: scene-anchored overlays, overview graph, VR blockage cards, capability cards, message particles
- [ ] 07-05-PLAN.md — VR Interaction & Editing: tool belt, wire/create/delete tools, test message tool, undo system, search
- [ ] 07-06-PLAN.md — Time-Travel & Timeline: scrubber UI, playback modes, historical state replay, session export
- [ ] 07-07-PLAN.md — Polish & User Study: performance optimization, user study (N=20), paper figures

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 (parallel with 3, 5, 7) -> 3 (parallel with 2, 5, 7) -> 4 (benefits from 3) -> 5 (parallel with 2, 3, 4, 7) -> 6 -> 7 user study last

Note: Phase 1 is the sole prerequisite. After Phase 1, Phases 2, 3, 5, and 7 are independent and can execute in parallel. Phase 4 can start after Phase 1 but benefits from Phase 3 completing first. Phase 6 waits for all data-producing phases (2, 3, 4, 5). Phase 7 (VR Visual Composer) can begin after Phase 1 and runs in parallel with Phases 2-6; its user study sub-phase (07-05) should run after the core tool is complete.

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Tutorial Validation | 0/4 | Planning complete | - |
| 2. Spatial Indexing | 0/3 | Not started | - |
| 3. Distributed Messaging | 0/1 | Not started | - |
| 4. Asymmetry Analysis | 0/1 | Not started | - |
| 5. User Study | 0/2 | Not started | - |
| 6. Paper Polish | 0/3 | Not started | - |
| 7. VR Visual Composer | 0/7 | Design complete (Rev 2) | - |

---
*Roadmap created: 2026-02-11*
*Last updated: 2026-02-19*
