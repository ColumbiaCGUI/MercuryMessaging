# Project Research Summary

**Project:** MercuryMessaging UIST 2026 Capabilities
**Domain:** Research-grade developer tools for hierarchical message routing framework (Unity)
**Researched:** 2026-02-11
**Confidence:** MEDIUM-HIGH

## Executive Summary

MercuryMessaging is a hierarchical message routing framework for Unity that needs to ship a set of developer tools and framework extensions for a UIST 2026 paper submission. The research across all four dimensions converges on a single critical finding: **the project plans 7 features totaling ~1,234 hours, but a strong UIST paper needs 2-3 deep contributions, not 7 shallow ones.** The recommended approach is to focus the paper on a "developer experience" story centered on three tightly-coupled contributions: (1) a visual composer with bi-directional scene-graph editing, (2) blockage indicators that explain WHY messages were filtered -- a genuinely novel capability no existing tool provides, and (3) message-centric time-travel debugging that differentiates from code-centric tools like Replay.io and JIVE. The remaining features (spatial indexing, parallel FSMs, asymmetry analysis, distributed messaging) should appear as proof-of-concept demonstrations or be deferred to a second paper.

The technology stack requires **zero new dependencies**. All seven features can be built with Unity 6000.3.7f1's built-in APIs (Compute Shaders, Jobs/Burst, UI Toolkit) plus the already-imported GraphViewBase (MIT). The architecture centers on keeping `MmRelayNode.MmInvoke()` -- the single routing hot-path at 14-17ms frame time -- clean by using companion components and a shared Introspection Subsystem rather than inline modifications. Three researchers independently flagged that the parallel FSM plan's use of `Parallel.ForEach` will crash Unity due to its single-threaded component model; the correct approach is sequential per-frame evaluation (logically concurrent, not thread-parallel).

The top risks are: paper scope dilution (Critical -- decide core contributions before coding), GraphViewBase instability across Unity versions (Critical -- pin version and test immediately), editor state loss on assembly reload destroying Visual Composer and time-travel data (Critical -- use ScriptableObject persistence), and distributed messaging breaking MmMetadataBlock's network serialization format (High -- requires version negotiation). A shared Introspection Subsystem (capturing per-responder rejection reasons gated behind `IntrospectionMode`) is needed by both the Visual Composer and Time-Travel Debugging and must be built once as shared infrastructure.

## Key Findings

### Recommended Stack

Zero new dependencies are needed. The entire feature set builds on packages already installed in the project. The only maintenance risk is GraphViewBase (MIT, 4 contributors), which can be forked if abandoned. Unity's Graph Toolkit (GTK) is available as experimental (0.4.0-exp.2) but should be deferred until Unity 6.4+ where it becomes a built-in module.

**Core technologies:**
- **GraphViewBase (MIT, already imported):** 2D graph editor for Visual Composer -- UIToolkit-native, avoids deprecated Experimental.GraphView
- **Unity Compute Shaders + Jobs/Burst 1.8.27 (built-in + installed):** Hybrid CPU-GPU pipeline for spatial indexing -- CPU builds octree, GPU runs parallel queries, AsyncGPUReadback for non-blocking results
- **MmCircularBuffer + BinaryWriter (in-project + System.IO):** Message recording for time-travel debugging -- reuses tested infrastructure (30+ tests), zero-dependency serialization
- **UI Toolkit (built-in):** Editor windows for Visual Composer and Timeline -- native to Unity 6, data binding, ListView
- **GL.Begin/GL.End + Handles (built-in):** 3D scene view line rendering -- zero-dependency, URP/HDRP compatible
- **IMGUI (built-in):** Runtime debugging overlay -- only viable option for transient in-game debug UI
- **FishNet/Fusion 2 (already installed):** Network transport for distributed messaging -- existing conditional compilation wrappers

**Critical version requirement:** Pin Unity 6000.3.7f1 for paper submission and artifact evaluation. Do not upgrade during development.

### Expected Features

**Must have (UIST table stakes -- paper will be rejected without these):**
- Node-graph visualization of message topology with scene hierarchy mirroring
- Live message flow animation during play mode (must match Unreal Blueprints' Active Wires quality)
- Blockage indicators showing WHY messages were filtered (**strongest novelty claim** -- zero existing tools do this)
- Message inspection panel with full metadata display
- Message recording/export with bounded circular buffer
- Timeline scrubber for message-centric time-travel (**second strongest novelty claim**)
- Bi-directional graph editing (graph changes update scene and vice versa)
- User study: N>=12, quantitative (time-to-fix) + qualitative (NASA-TLX)

**Should have (significantly strengthens paper):**
- Runtime topology manipulation (drag-and-drop rewiring during play mode)
- Sequence diagram panel for message propagation visualization (missing from all current plans -- STRONGLY recommended)
- Message frequency heatmap overlay (~8h, high visual impact)
- Asymmetry analysis benchmarks (already prototyped in Python, 40h for C# port)
- Parallel FSM proof-of-concept (2-3 orthogonal regions with message-based sync)
- Spatial routing proof-of-concept (CPU-only radius + frustum queries)

**Defer (post-paper or second paper):**
- GPU-accelerated octree spatial indexing (SIGGRAPH-caliber contribution on its own)
- Distributed atomic transactions (two-phase commit over unreliable networks is a decades-old hard problem)
- Full N-region parallel FSM with thread safety (excessive for one paper section)
- LLM-assisted message network generation (tangential to core contribution)
- Cross-engine portability (Unity-specific is fine for UIST)

### Architecture Approach

All seven features integrate with MercuryMessaging's established architecture through three layers: (1) **Core Pipeline Hooks** that touch MmRelayNode.MmInvoke directly (gated behind static flags for zero overhead when disabled), (2) **Companion Components** that sit alongside relay nodes using the existing Listener API, and (3) **Editor-Only Systems** that observe but never alter runtime behavior. The critical shared infrastructure is an **Introspection Subsystem** -- an `IntrospectionMode` static flag on MmRelayNode that, when enabled, causes `ResponderCheck` to record per-responder rejection reasons into a thread-local struct. This single mechanism serves both blockage indicators (Visual Composer) and the "why didn't message reach X?" query (Time-Travel Debugging).

**Major components:**
1. **Introspection Subsystem (shared)** -- `IntrospectionMode` flag + `MmRejectionTracker` capturing filter rejection reasons. Adds ~1ns boolean check to hot-path when disabled. Required by Visual Composer and Time-Travel.
2. **Visual Composer** -- `MmComposerWindow` (GraphViewBase), `MmSceneDrawer` (GL lines), `MmRuntimeDebugger` (IMGUI). Pure editor-observer pattern via `NotifyListeners`.
3. **Time-Travel Debugging** -- `MmMessageRecorder` (via NotifyListeners), `MmRecordedMessage` structs in CircularBuffer, `MmTimelineWindow` (UI Toolkit), `MmQueryEngine` for rejection analysis.
4. **Spatial Indexing** -- `MmSpatialIndex` companion MonoBehaviour (composition, NOT subclass), CPU octree primary with GPU compute optional. Replaces linear scan in `MmFluentPredicates.Where()`.
5. **Parallel FSMs** -- `MmParallelSwitchNode` extending `MmRelaySwitchNode`. Sequential per-frame execution (NOT threaded). Override `SelectedCheck` to check across all active regions.
6. **Distributed Messaging** -- DSL extensions (`.Replicated()`, `.AuthoritativeOn()`, `.LocalOnly()`) + `MmMessageCallbackRegistry` for veto. Integration at network filter decision point in MmInvoke.

### Critical Pitfalls

1. **Paper scope dilution (Critical)** -- 7 features in one paper guarantees shallow treatment. Decide NOW: 2-3 core contributions (visual composer + blockage indicators + time-travel). Frame remaining features as supporting demos. Structure paper around a research question, not a feature list.

2. **Parallel.ForEach crashes Unity (Critical)** -- Flagged independently by Stack, Architecture, AND Pitfalls researchers. Unity's API is single-threaded. Any FSM region that calls `SetActive()`, `GetComponent()`, or accesses `Transform` from a worker thread will crash. Use sequential per-frame evaluation with deterministic ordering.

3. **Editor state loss on assembly reload (Critical)** -- Entering/exiting Play Mode destroys non-serialized state. Visual Composer graph layout and time-travel recording will be wiped. Use `ScriptableObject` for persistent state, write recordings to disk, implement `ISerializationCallbackReceiver`.

4. **GPU platform exclusion undermines spatial claims (Critical)** -- WebGL, many Android devices, and Quest lack compute shader support. Build CPU-first, GPU-optional. Frame the contribution as "spatial indexing integrated with hierarchical routing" (novel), not "GPU acceleration" (optimization).

5. **MmMetadataBlock serialization backward compatibility (High)** -- Distributed messaging needs to add `DistributionMode` to MmMetadataBlock, but the serialization format is a fixed array of shorts. Changing it breaks communication with existing network peers. Use the existing `Options` field for local extensions; implement version negotiation for network-transmitted changes.

## Implications for Roadmap

Based on combined research, the following phase structure maximizes paper quality while managing risk.

### Phase 0: Foundation and Scoping
**Rationale:** All four researchers identified pre-implementation decisions that must happen first. Paper scope, CI/CD, interceptor pattern design, and tutorial validation are prerequisites.
**Delivers:** Paper contribution scope decision, CI/CD pipeline, `IMmMessageInterceptor` interface design, validated tutorials 1-5
**Addresses:** TS-7 prerequisites (user study needs working tutorials), P1 (scope), P9 (CI/CD), P13 (tutorials), P15 (god object prevention)
**Avoids:** P1 (building features that won't appear in paper), P9 (integration regressions), P13 (broken tutorials blocking user study)
**Research needed:** NO -- standard infrastructure work

### Phase 1: Introspection Subsystem
**Rationale:** Both the Visual Composer and Time-Travel Debugging need per-responder rejection reason tracking. Building this shared infrastructure once prevents duplicate hot-path modifications and ensures architectural consistency.
**Delivers:** `IntrospectionMode` static flag on MmRelayNode, `MmRejectionTracker` with enum-based rejection codes, `NotifyListeners`-based subscriber infrastructure
**Addresses:** D-1 (blockage indicators data source), D-4 (time-travel rejection analysis data source)
**Avoids:** P15 (multiple features each adding inline code to MmRelayNode), P7 (string allocation overhead -- uses enum codes instead)
**Research needed:** NO -- direct code changes to existing MmRelayNode with clear insertion points

### Phase 2: Visual Composer
**Rationale:** This is the primary UIST contribution and the most visible deliverable. It provides the visual foundation that blockage indicators, live animation, and the user study all depend on. Must start early to allow pilot study mid-phase.
**Delivers:** GraphViewBase 2D graph editor, scene hierarchy mirroring, bi-directional editing, live message flow animation, blockage indicators on edges/nodes, GL 3D scene visualization, IMGUI runtime debugger
**Uses:** GraphViewBase (MIT), UI Toolkit, GL.Begin/GL.End, Introspection Subsystem (Phase 1)
**Implements:** D-1 (blockage indicators), D-2 (bi-directional editing), TS-1 (node graph), TS-2 (live animation), TS-8 (hierarchy mirror), TS-4 (message inspection)
**Avoids:** P2 (verify GraphViewBase in week 1), P3 (run pilot study N=3-5 mid-phase), P5 (ScriptableObject persistence from week 2)
**Research needed:** YES -- GraphViewBase API patterns need investigation. Bi-directional sync (scene changes reflected in graph and vice versa) is complex editor tooling.

### Phase 3: Time-Travel Debugging
**Rationale:** Second strongest novelty claim. Depends on Introspection Subsystem (Phase 1) for rejection data. Shares data model with Visual Composer. Should be built while Visual Composer is being polished/pilot-tested.
**Delivers:** `MmMessageRecorder` capturing messages via listener API, tiered recording (Level 1-3), `MmTimelineWindow` with frame scrubber, `MmQueryEngine` for "why didn't message X reach Y?"
**Uses:** MmCircularBuffer (existing, tested), BinaryWriter (System.IO), UI Toolkit, Introspection Subsystem
**Implements:** D-4 (message-centric time-travel), TS-5 (filter/search history), TS-6 (recording/export), TS-3 (breakpoint on message)
**Avoids:** P5 (write recordings to disk, not just memory), P7 (tiered recording -- measure Level 1 overhead before building Level 3)
**Research needed:** PARTIAL -- recording overhead profiling needed. Timeline UI is standard.

### Phase 4: Proof-of-Concept Features (Parallel)
**Rationale:** These three features are independent of each other and of the visual tooling. They serve as paper subsections demonstrating framework breadth, not as primary contributions. Building them in parallel saves time.
**Delivers:**
- Asymmetry analysis: C# benchmark comparing Cascade vs pub/sub vs direct under 10-50% graph divergence (40h)
- Parallel FSMs: 2-3 orthogonal regions with message-based sync, VR multimodal demo (60-80h for PoC)
- Spatial routing: CPU-based radius + frustum query integrated with fluent API `Within()` and `InCone()` (60-80h for PoC)
**Implements:** D-9 (asymmetry), D-8 (parallel FSMs -- PoC), D-7 (spatial routing -- PoC)
**Avoids:** P6 (sequential execution, NOT Parallel.ForEach), P4 (CPU-first, GPU-optional), P12 (limit to 2-3 regions, not arbitrary N)
**Research needed:**
- Parallel FSMs: YES -- conflict resolution semantics, SelectedCheck override for multi-region
- Spatial indexing: YES -- octree update strategy for moving objects, CPU fallback benchmarks
- Asymmetry: NO -- Python prototype already validates approach

### Phase 5: Distributed Messaging (Descoped)
**Rationale:** Highest integration risk (network serialization backward compat). Only Phases 1-2 of the original 4-phase plan are tractable. Atomic transactions and topology awareness are deferred to future work.
**Delivers:** Distribution semantics (`.Replicated()`, `.AuthoritativeOn()`, `.LocalOnly()` fluent extensions), change notification with veto (`OnBeforeReceive`/`OnAfterReceive`), veto visualization connected to blockage indicators
**Implements:** Distributed messaging Phases 1-2 only. Connects G-4 (veto visualization) to D-1 (blockage indicators).
**Avoids:** P11 (no atomic transactions -- deferred), serialization backward compat (use Options field, not new MetadataBlock fields)
**Research needed:** YES -- MmMetadataBlock serialization format extension requires careful backward compat analysis

### Phase 6: User Study and Paper Polish
**Rationale:** User study must happen after the visual composer is feature-complete and tutorials validated. Include sequence diagrams and heatmap as visual polish.
**Delivers:** User study (N>=12), NASA-TLX + time-to-fix, qualitative interviews, sequence diagram panel, heatmap overlay, paper draft
**Implements:** TS-7 (user study), G-1 (sequence diagrams), G-5 (heatmap)
**Avoids:** P3 (pilot already run in Phase 2; full study here), P8 (related work includes industrial tools)
**Research needed:** NO -- study design is standard. Pilot data from Phase 2 informs final protocol.

### Phase Ordering Rationale

- **Foundation before features:** CI/CD and paper scope decisions prevent the two most expensive failure modes (integration regressions and building unused features).
- **Introspection Subsystem before consumers:** Both Visual Composer and Time-Travel need rejection tracking. Building it once prevents two separate hot-path modifications.
- **Visual Composer before Time-Travel:** Visual Composer is the primary contribution and user study vehicle. Time-Travel shares its data model but is secondary.
- **Proof-of-concepts in parallel, late:** These are paper subsections, not core contributions. They can be built independently and late without blocking the main story.
- **Distributed messaging last:** Highest integration risk. Network serialization changes can break existing tests. Descoped to Phases 1-2 only.
- **User study at the end:** Cannot run without working tools and validated tutorials. Pilot study embedded in Phase 2 de-risks the full study.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2 (Visual Composer):** GraphViewBase API patterns, bi-directional scene sync, Play Mode persistence -- complex editor tooling with limited community examples
- **Phase 4 (Parallel FSMs):** Conflict resolution semantics, SelectedCheck multi-region override, UML statechart formalization
- **Phase 4 (Spatial Indexing):** Octree update strategy for moving objects, CPU fallback benchmarks, AsyncGPUReadback latency measurement
- **Phase 5 (Distributed Messaging):** MmMetadataBlock serialization backward compatibility, version negotiation protocol

Phases with standard patterns (skip research-phase):
- **Phase 0 (Foundation):** CI/CD setup, tutorial smoke tests, interface design -- well-documented Unity patterns
- **Phase 1 (Introspection):** Direct MmRelayNode modification with clear insertion points
- **Phase 3 (Time-Travel):** CircularBuffer recording, UI Toolkit timeline -- established patterns
- **Phase 4 (Asymmetry Analysis):** Port of validated Python prototype to C# benchmarks
- **Phase 6 (User Study):** Standard HCI evaluation methodology

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Zero new dependencies. All technologies verified against official Unity 6.3 docs. GraphViewBase already imported and tested. Burst/Collections/Mathematics already installed. |
| Features | MEDIUM-HIGH | Table stakes confirmed against 6 competitor tools. Blockage indicators and message-centric time-travel validated as novel. Risk: paper scope needs explicit 2-3 contribution decision. |
| Architecture | HIGH | Based on direct source code analysis of MmRelayNode.cs (1247 lines). Integration points (NotifyListeners, HandleAdvancedRouting, PerformanceMode) verified in code. Companion component pattern validated. |
| Pitfalls | HIGH | 15 pitfalls identified with severity levels. Critical items (threading, assembly reload, scope) confirmed by multiple researchers. Recovery strategies documented. |

**Overall confidence:** MEDIUM-HIGH

The highest uncertainty is in paper scope strategy (which 2-3 features to foreground) and GraphViewBase long-term stability. Both are addressable through early decisions (Phase 0 scope lock, Phase 2 week-1 GraphViewBase verification).

### Gaps to Address

- **Paper contribution framing:** Research identifies features and their novelty but does not resolve which 2-3 form the paper's core argument. This must be decided with the advisor in Phase 0.
- **Pilot study results:** The 50% improvement hypothesis for the Visual Composer user study is aspirational with no pilot data. Pilot study in Phase 2 will validate or force redesign.
- **GraphViewBase stability on Unity 6.3:** No researcher tested GraphViewBase on the exact project version. Phase 2 week-1 must include a "load, add nodes, connect edges, enter Play Mode" smoke test.
- **Recording overhead at scale:** Time-travel recording at Level 3 (full rejection reasons) at 500+ msg/sec is untested. Phase 3 week-1 must prototype Level 1 recording and measure overhead.
- **Bloom filter sizing (Static Analysis):** The 128-bit Bloom filter in the static analysis plan is catastrophically undersized for real hierarchies. If static analysis is included, filter must be resized to ~1440 bits (n=100 nodes, p=0.001 FPR). Better option: keep existing HashSet-based VisitedNodes from QW-1.
- **Three missing features identified:** Sequence diagram visualization (G-1), Unity Profiler integration (G-2), and message frequency heatmaps (G-5) were found in competitor analysis but absent from all current plans. G-1 and G-5 are recommended additions (~28h combined).

## Cross-Cutting Themes

Ten findings emerged independently from multiple researchers:

1. **Parallel.ForEach is WRONG for FSMs** (Stack + Architecture + Pitfalls) -- Unity's API is single-threaded. Use sequential per-frame evaluation.
2. **Paper scope is the #1 risk** (Features + Pitfalls) -- 7 features in one UIST paper guarantees shallow treatment and likely rejection. Pick 2-3 core contributions.
3. **Blockage indicators are the strongest novelty claim** (Features + Architecture) -- Zero existing tools explain WHY a message was filtered. This is the paper's anchor.
4. **Message-centric time-travel is genuinely novel** (Stack + Features) -- All existing omniscient debuggers are code-centric. Novel framing vs Replay.io/JIVE.
5. **Spatial indexing needs CPU-first, GPU-optional** (Stack + Pitfalls) -- WebGL and mobile lack compute shaders. CPU octree is the contribution; GPU is optimization.
6. **Zero new dependencies needed** (Stack + Architecture) -- All features built with existing project dependencies and Unity built-in APIs.
7. **GraphViewBase is correct for UIST 2026 but has maintenance risk** (Stack + Pitfalls) -- 4 contributors, MIT. Pin version. Plan GTK migration for Unity 6.4+.
8. **Distributed messaging has highest integration risk** (Architecture + Pitfalls) -- MmMetadataBlock serialization backward compatibility. Descope to Phases 1-2.
9. **Shared Introspection Subsystem is mandatory** (Architecture) -- Visual Composer and Time-Travel both need rejection tracking. Build once.
10. **Three missing features found** (Features) -- Sequence diagrams, Unity Profiler integration, heatmaps. G-1 and G-5 strongly recommended.

## Sources

### Primary (HIGH confidence)
- Unity 6000.3.7f1 official documentation (Compute Shaders, UI Toolkit, Jobs/Burst, Test Framework)
- MmRelayNode.cs direct source analysis (lines 805-1080, integration points verified)
- UIST 2025 Author Guide (paper scope, evaluation criteria, desk rejection)
- Olsen (UIST 2007) "Evaluating User Interface Systems Research" (evaluation heuristics)
- GraphViewBase GitHub (MIT, already imported in project manifest)
- Unreal Blueprint Debugger, Foxglove Studio, Stately.ai, Replay.io (competitor feature comparison)

### Secondary (MEDIUM confidence)
- Ledo et al. (CHI 2018) "Evaluation Strategies for HCI Toolkit Research" (study design)
- Fast Collision Detection with Octree-Based Parallel Processing (IEEE 2025) -- 26x GPU speedup validation
- UnityHFSM ParallelStates (API design reference for parallel FSMs)
- Unity Graph Toolkit 0.4.0-exp.2 (future migration target, experimental)
- Deterministic Record-and-Replay (ACM 2025) -- time-travel debugging landscape

### Tertiary (LOW confidence)
- Unity GraphView instability across versions (forum reports, needs project-specific verification)
- Bloom filter false positive rate at scale (calculated, not measured in project)
- MemoryPack / Disruptor (emergency fallback options, untested in project context)

---
*Research completed: 2026-02-11*
*Ready for roadmap: yes*
