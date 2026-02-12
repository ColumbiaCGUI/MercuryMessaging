# Feature Research: Message Routing Visualization/Debugging Tools

**Domain:** Research-grade debugging and authoring tools for hierarchical messaging frameworks (Unity game engine)
**Researched:** 2026-02-11
**Confidence:** MEDIUM-HIGH
**Target Venue:** UIST 2026

---

## Executive Summary

The landscape of message routing visualization and debugging tools in 2025-2026 is fragmented across several domains: visual scripting editors (Unity Visual Scripting, Unreal Blueprints), time-travel debuggers (Replay.io, Undo/UDB, rr), robotics introspection tools (ROS 2 rqt_graph, Foxglove Studio), state machine editors (Stately.ai/XState), and distributed system tracers (Akka, agent tracing platforms). **No single tool combines hierarchical message visualization, live introspection, spatial routing, parallel FSMs, and time-travel debugging within a game engine.** This gap is the primary opportunity for a UIST 2026 contribution.

UIST reviewers evaluate systems papers using Olsen's criteria (UIST 2007): importance of the problem, generalizability, reduction of solution viscosity, empowerment of new design participants, and ease of combination. The acceptance rate at UIST 2025 was 22.2% (210/946). A strong systems paper must demonstrate: (1) clear problem definition, (2) thorough related work engagement, (3) implemented system with video, (4) novel technical contribution, and (5) evaluation aligned with Olsen's framework.

---

## Feature Landscape

### Table Stakes (UIST Reviewers Expect These)

Features that UIST reviewers will assume a "message routing debugging tool" must have. Missing these will result in rejection for insufficient contribution or incomplete system.

| # | Feature | Why Expected | Complexity | In dev/active/ Plans? | Notes |
|---|---------|--------------|------------|----------------------|-------|
| TS-1 | **Node-graph visualization of message topology** | Unreal Blueprints (2005+), ROS rqt_graph, Unity Visual Scripting all show computation graphs. A message routing tool without graph visualization is incomplete. | MEDIUM | YES (visual-composer Phase 1) | AGREES with plan. GraphViewBase approach is sound. |
| TS-2 | **Live message flow animation** | Unreal Blueprints "Active Wires" show pulsing execution flow during play mode since UE4. This is now baseline for any visual scripting/routing tool. | MEDIUM | YES (visual-composer Phase 2-3) | AGREES with plan. Must match or exceed Blueprints' active wire quality. |
| TS-3 | **Breakpoint/pause on message** | Unity Visual Scripting and Unreal Blueprints both support breakpoints in visual graphs. Dbux and JIVE support execution pausing. Reviewers expect message-level pause. | MEDIUM | PARTIAL (time-travel Phase 2 mentions "pause/step-through") | Plan mentions it but lacks detail. Need explicit breakpoint-on-message-type, breakpoint-on-filter-match. |
| TS-4 | **Message inspection panel** | All debugging tools (Unity Profiler, Foxglove Studio, Dbux) provide detailed inspection of individual events/messages. Must show method, metadata, routing path, targets. | LOW | YES (time-travel Phase 2, visual-composer Phase 3) | AGREES. Runtime debugger plan covers this. |
| TS-5 | **Filter/search over message history** | Foxglove Studio, Dbux, Unity Profiler all let you filter logged events by type, source, time range. Basic table stakes for any debugging tool. | LOW | YES (time-travel Phase 3 query system) | AGREES. Query builder plan is adequate. |
| TS-6 | **Recording/export of message traces** | ROS 2 rosbag, Foxglove MCAP, Unity Profiler .data files all support trace export. Reproducibility is expected for research tools. | LOW | PARTIAL (time-travel Phase 1 recording, visual-composer Phase 3 message trace export) | AGREES. CircularBuffer already provides bounded recording. Add file export. |
| TS-7 | **User study comparing tool vs baseline** | Olsen's framework and CHI/UIST 2025 reviewing guidelines explicitly require evaluation. N>=12 with quantitative metrics (time, errors, cognitive load). | HIGH | YES (visual-composer evaluation, user-study) | AGREES. NASA-TLX + time-to-fix design is appropriate. Study design looks solid. |
| TS-8 | **Scene graph hierarchy mirroring** | The graph must reflect actual Unity scene hierarchy. ROS rqt_graph mirrors the ROS computation graph. Any tool that shows a graph disconnected from scene structure will confuse users. | MEDIUM | YES (visual-composer hierarchy mirroring, 36h) | AGREES. Bi-directional sync is critical. |

### Differentiators (Competitive Advantage / Novel Contribution)

Features that set MercuryMessaging apart. These are what make the paper publishable at UIST.

| # | Feature | Value Proposition | Complexity | In dev/active/ Plans? | Notes |
|---|---------|-------------------|------------|----------------------|-------|
| D-1 | **Blockage indicators ("why didn't this message arrive?")** | NO existing tool explains *why* a message was filtered out. Unreal shows active wires but not WHY inactive ones are inactive. ROS rqt_graph shows connections but not filter decisions. Dbux/JIVE trace code execution, not routing filter logic. **This is the single strongest novelty claim.** | MEDIUM | YES (visual-composer blockage indicators, time-travel rejection reasons) | AGREES. This is the #1 differentiator. Must be prominently featured in paper. |
| D-2 | **Bi-directional graph editing (graph <-> scene sync)** | No existing game engine tool allows editing a routing graph that writes back to scene objects. Blueprints are code-to-graph only. ROS rqt_graph is read-only. Stately.ai edits state machines but not connected to a runtime scene. | HIGH | YES (visual-composer bi-directional sync) | AGREES. Critical for "empowers new design participants" (Olsen criterion). |
| D-3 | **Runtime topology manipulation during gameplay** | No existing game engine tool allows drag-and-drop rewiring of message routing during play mode. Blueprints require stopping play mode to edit. Stately Inspector is read-only at runtime. | HIGH | YES (visual-composer runtime manipulation) | AGREES. Very strong novelty claim if implemented well. |
| D-4 | **Message-centric time-travel (vs code-centric)** | Existing omniscient debuggers (JIVE, Dbux, Replay.io, rr, Undo/UDB) are all code-centric -- they step through code execution. Mercury's time-travel is MESSAGE-centric: step through message propagation history with filter decisions at each hop. This is a genuinely novel framing. | HIGH | YES (time-travel-debugging, 150h) | AGREES. Novel framing vs Replay.io/JIVE. Must clearly differentiate from code-centric approaches in the paper. |
| D-5 | **Hierarchical routing through scene graph** (core framework) | No other messaging framework routes through Unity's scene hierarchy natively. ROS uses flat pub/sub. Akka uses actor hierarchies but not scene graphs. Blueprints use event dispatchers (flat). This is Mercury's core architectural novelty. | Already implemented | N/A (core framework) | Already exists. Must be prominently positioned as the architectural foundation that enables all other tools. |
| D-6 | **Multi-level filter visualization** (Level + Active + Selected + Tag + Network) | Five orthogonal filters operating simultaneously is unique. No other tool visualizes multi-dimensional filter interactions. ROS has topic-based filtering only. | MEDIUM | PARTIAL (visual-composer color-coded edges) | Plan shows color-coding by filter type. RECOMMEND: Add Venn-diagram or matrix visualization showing how filters compose (intersection). |
| D-7 | **Spatial routing with LOD messaging** | No existing system combines spatial indexing with hierarchical message routing. GPU octree for message delivery is novel application of rendering techniques to communication. | HIGH | YES (spatial-indexing, 360h) | AGREES but CAUTION: 360h is very large. For UIST paper, need at minimum: proof-of-concept spatial filter + fluent API integration + one benchmark. Full GPU octree may be scope overreach. |
| D-8 | **Parallel orthogonal FSM regions with message-based sync** | XState/Stately supports parallel states in JavaScript. itemis CREATE supports orthogonal regions for embedded. **No Unity framework** provides parallel FSMs synchronized via hierarchical messages. Closest competitor: Mucho (IMWUT 2025) does recording-based FSM extraction, not direct authoring. | HIGH | YES (parallel-fsm, 280h) | AGREES. Differentiation from Mucho (recording vs direct authoring) is clear. RECOMMEND: Focus on the message-based synchronization angle, not just parallelism. |
| D-9 | **Graph asymmetry tolerance for networked peers** | No pub/sub or direct-reference system handles structural graph divergence gracefully. Cascade's hierarchical ToDescendants routing follows the receiver's local hierarchy, reaching extra nodes automatically. This is a novel property of hierarchical routing. | MEDIUM | YES (asymmetry-analysis, 40h) | AGREES. Python prototype already validates the concept. C# benchmark + paper integration is well-scoped. |
| D-10 | **Fluent DSL for message routing** (existing) | 86% code reduction with type-safe chainable API. No competing framework offers this level of API ergonomics for message routing in C#/Unity. | Already implemented | N/A (core framework) | Already exists. Strong contribution point for the paper's "ease of combination" (Olsen criterion). |

### Anti-Features (Deliberately NOT Build)

Features that seem appealing but would waste effort or weaken the paper.

| # | Anti-Feature | Why Requested | Why Problematic | Alternative |
|---|-------------|---------------|-----------------|-------------|
| AF-1 | **Full code-centric omniscient debugger** | Replay.io, JIVE, Dbux all do this | Massive scope (years of work). Duplicates existing tools. Reviewers will compare unfavorably to Replay.io which has had 5+ years of development. | Focus on MESSAGE-CENTRIC debugging instead. This is the novel framing. |
| AF-2 | **General-purpose visual scripting (Bolt/Blueprint competitor)** | Visual scripting is trendy | Mercury is a messaging framework, not a scripting language. Attempting to compete with Blueprints (20+ years of development) is futile. | Focus on MESSAGE ROUTING visualization specifically. The constraint (routing-only) is the strength. |
| AF-3 | **AI/LLM-generated message networks** | Stately.ai added AI agent for flow generation. InstructPipe (CHI 2025) uses LLM for pipeline generation. | Tangential to core contribution. LLM integration adds complexity without strengthening the research narrative. Would dilute the paper. | Mention as future work only. |
| AF-4 | **Multiverse/non-deterministic debugging** | Active research area (SLE 2025 Frolich et al., OOPSLA 2025 MIO) | Mercury messages are deterministic (FIFO, single-threaded Unity). Multiverse debugging solves a problem Mercury doesn't have. | Acknowledge in related work as complementary for non-deterministic systems. |
| AF-5 | **Full distributed transaction system** | Planned in distributed-messaging Phase 3 (atomic messaging) | Two-phase commit across network peers is extremely complex. High risk of bugs. Not essential for UIST paper. | For UIST: implement LOCAL atomicity (all children receive before any process). Defer DISTRIBUTED transactions to future work. |
| AF-6 | **Production-grade GPU octree spatial index** | Planned at 360h in spatial-indexing | Full GPU compute shader spatial index is a SIGGRAPH-level contribution on its own. Trying to include this as one feature among six will spread too thin. | For UIST: implement CPU-based radius query + frustum query as proof of concept. Show spatial filtering in fluent API. Defer GPU acceleration. |
| AF-7 | **Cross-engine portability** (Unreal, Godot) | Broader impact claim | Mercury is deeply integrated with Unity's scene graph. Porting requires rewriting core architecture. Reviewers won't penalize Unity-only tools. | Mention cross-engine potential in discussion/future work. |

---

## Gap Analysis: Features Found in Research but MISSING from dev/active/ Plans

These are features identified through ecosystem research that the current plans do not address.

| # | Gap | Where Found | Relevance | Recommendation |
|---|-----|-------------|-----------|----------------|
| G-1 | **Sequence diagram visualization of message flow** | Stately Inspector generates sequence diagrams for actor communication. JIVE uses object/sequence diagrams. Foxglove Studio has timeline views. | HIGH -- sequence diagrams are the standard way to communicate event-driven system behavior in academic papers. | ADD to visual-composer or time-travel plans. A message sequence diagram panel (source -> relay -> responder(s)) would strengthen both the tool and the paper's figures. |
| G-2 | **Profiler integration (Unity Profiler custom module)** | Unity's ProfilerCounter API and custom Profiler modules allow extending the Profiler. Netcode for Entities (2025) uses this for network profiling. | MEDIUM -- integrating with Unity's existing profiler would reduce learning curve and leverage familiar workflows. | CONSIDER adding a lightweight MercuryMessaging Profiler Module that shows message throughput, routing time, and filter hit rates alongside CPU/GPU/Memory in Unity's standard Profiler. Low effort (~20h), high polish. |
| G-3 | **Diff/comparison between two message traces** | didiffff (ICPC 2022) compares execution traces. Foxglove supports side-by-side rosbag comparison. Replay.io enables "retroactive print statements" for comparing behaviors. | MEDIUM -- useful for regression testing message behavior after hierarchy changes. | DEFER to post-paper. Mention in future work. |
| G-4 | **Pre/post message hooks with veto capability** | Planned in distributed-messaging Phase 2 but NOT connected to visual tooling. Akka has supervision strategies. ROS2 has message filters. | MEDIUM -- veto visualization ("message blocked by safety callback") would strengthen the blockage indicator story. | CONNECT distributed-messaging Phase 2 (OnBeforeReceive veto) with visual-composer blockage indicators. Show vetoed messages as red-blocked in the graph. |
| G-5 | **Heatmap overlay showing message frequency per node** | Foxglove shows topic throughput. Unity Network Profiler shows message counts. Agent tracing platforms show interaction frequency maps. | LOW-MEDIUM -- would enhance the visual composer's utility for performance analysis. | CONSIDER as low-hanging fruit for visual-composer. Color node backgrounds by message frequency (cool=low, hot=high). ~8h effort. |
| G-6 | **Collaborative/remote debugging (shared session)** | Foxglove supports remote collaboration and shared sessions. Replay.io shares replays as URLs. | LOW for UIST paper. | DEFER entirely. Not needed for paper. Mention as future work for distributed teams. |
| G-7 | **Accessibility features in visual editor** | CHI 2025 increasingly expects accessibility considerations. No accessibility plan in visual-composer. | MEDIUM for reviewer perception -- CHI/UIST reviewers increasingly flag accessibility gaps. | Add basic keyboard navigation and screen-reader-compatible labels to graph editor. Mention in paper's limitations section if not fully accessible. |

---

## Feature Dependencies

```
[D-5: Hierarchical Routing] (EXISTING - core framework)
    |
    +--requires--> [TS-1: Node-graph visualization]
    |                  |
    |                  +--requires--> [TS-8: Scene graph hierarchy mirroring]
    |                  |
    |                  +--enhances--> [D-2: Bi-directional graph editing]
    |                  |                  |
    |                  |                  +--enhances--> [D-3: Runtime topology manipulation]
    |                  |
    |                  +--enhances--> [D-6: Multi-level filter visualization]
    |                  |                  |
    |                  |                  +--enhances--> [D-1: Blockage indicators] **STRONGEST NOVELTY**
    |                  |
    |                  +--enhances--> [G-1: Sequence diagram panel]
    |                  |
    |                  +--enhances--> [G-5: Heatmap overlay]
    |
    +--requires--> [TS-6: Recording/export]
    |                  |
    |                  +--requires--> [TS-4: Message inspection panel]
    |                  |
    |                  +--enhances--> [D-4: Message-centric time-travel]
    |                                     |
    |                                     +--requires--> [TS-5: Filter/search message history]
    |                                     |
    |                                     +--requires--> [TS-3: Breakpoint/pause on message]
    |
    +--independent--> [D-7: Spatial routing] (can be built/demoed independently)
    |
    +--independent--> [D-8: Parallel FSMs] (builds on existing MmRelaySwitchNode)
    |
    +--independent--> [D-9: Graph asymmetry] (needs only core framework + network bridge)

[D-8: Parallel FSMs] --enhances--> [D-6: Multi-level filter visualization]
                                    (Selected filter becomes more interesting with parallel regions)

[G-4: Pre/post hooks with veto] --enhances--> [D-1: Blockage indicators]
                                               (vetoed messages appear as blocked in graph)

[D-7: Spatial routing] --conflicts-scope--> [AF-6: Full GPU octree]
                        (must choose: proof-of-concept OR production-grade)
```

### Dependency Notes

- **D-1 (Blockage indicators) requires TS-1 (node graph):** Blockage indicators need visual nodes to display rejection reasons on.
- **D-4 (Time-travel) requires TS-6 (Recording):** Cannot scrub through history without recording infrastructure.
- **D-7, D-8, D-9 are independent:** Spatial routing, parallel FSMs, and asymmetry analysis can each be built and evaluated independently of the visual tooling.
- **G-4 enhances D-1:** Veto callbacks provide another source of "blockage" to visualize, strengthening the contribution.
- **TS-7 (User study) requires TS-1 through TS-6:** Cannot run user study without a working tool.

---

## MVP Definition (UIST 2026 Paper Scope)

### Launch With (Paper Submission)

Minimum viable system that constitutes a publishable UIST paper.

- [x] [D-5] Hierarchical message routing framework (ALREADY EXISTS)
- [x] [D-10] Fluent DSL API (ALREADY EXISTS)
- [ ] [TS-1] Node-graph visualization with scene hierarchy mirroring
- [ ] [TS-2] Live message flow animation during play mode
- [ ] [D-1] Blockage indicators showing WHY messages were filtered (STRONGEST NOVELTY)
- [ ] [D-2] Bi-directional graph editing (graph changes update scene and vice versa)
- [ ] [TS-4] Message inspection panel with full metadata
- [ ] [TS-6] Message recording with bounded circular buffer
- [ ] [D-4] Timeline scrubber for message history (basic time-travel)
- [ ] [D-1+TS-5] Query: "Why didn't message X reach responder Y?"
- [ ] [TS-7] User study: N>=12, compare visual tool vs Inspector-only, measure time-to-fix + NASA-TLX

### Add After Core Tools Work (Paper Revision / Camera-Ready)

Features to add if time permits, strengthening the paper.

- [ ] [D-3] Runtime topology manipulation (drag-and-drop rewiring during play mode)
- [ ] [G-1] Sequence diagram panel showing message propagation timeline
- [ ] [D-6] Multi-level filter visualization (color-coded edges + filter composition matrix)
- [ ] [G-5] Heatmap overlay showing message frequency per node
- [ ] [G-2] Unity Profiler custom module integration

### Demonstrate as Proof-of-Concept (Paper Section / Subsection)

Features that need working prototypes for specific paper sections, not full production implementations.

- [ ] [D-7] Spatial routing: CPU-based radius query + frustum query + fluent API (`Within(10f)`, `InCone()`)
- [ ] [D-8] Parallel FSMs: Two orthogonal regions with message-based synchronization demo
- [ ] [D-9] Asymmetry analysis: C# benchmark comparing Cascade vs pub/sub vs direct under graph divergence
- [ ] [D-9] Asymmetry analysis: Delivery rate graph across 10-50% divergence levels

### Future Consideration (Post-Paper)

Features to defer until after UIST submission.

- [ ] [AF-6] GPU-accelerated octree spatial indexing -- SIGGRAPH-level contribution, separate paper
- [ ] [AF-5] Distributed atomic transactions -- complex, high-risk, separate paper
- [ ] [AF-3] LLM-assisted message network generation -- future work section
- [ ] [G-3] Trace diff/comparison -- post-paper research tool enhancement
- [ ] [G-6] Collaborative remote debugging -- industry feature, not research contribution

---

## Feature Prioritization Matrix

| Feature | Research Value | Implementation Cost | Priority | Rationale |
|---------|--------------|---------------------|----------|-----------|
| D-1: Blockage indicators | HIGH | MEDIUM (40-60h) | **P1** | Strongest novelty claim. No existing tool does this. |
| D-2: Bi-directional graph editing | HIGH | HIGH (80h) | **P1** | Core of "visual authoring" contribution. Olsen's "empowers new participants." |
| TS-1+TS-8: Node-graph + hierarchy mirror | HIGH | MEDIUM (80h + 36h) | **P1** | Foundation for all visual features. Table stakes. |
| TS-2: Live message animation | HIGH | MEDIUM (40h) | **P1** | Table stakes. Must match Blueprints' active wires. |
| D-4: Message-centric time-travel | HIGH | HIGH (90h) | **P1** | Second strongest novelty claim. Novel framing vs Replay.io/JIVE. |
| TS-7: User study | HIGH | HIGH (80-100h) | **P1** | Required for UIST acceptance. |
| D-9: Asymmetry analysis | MEDIUM-HIGH | LOW (40h) | **P2** | Supports paper's theoretical contribution (Section 5.5). Already prototyped in Python. |
| D-8: Parallel FSMs (proof-of-concept) | MEDIUM | MEDIUM (60-80h for PoC) | **P2** | Demonstrates breadth. Mucho (IMWUT 2025) is direct competitor. |
| D-7: Spatial routing (proof-of-concept) | MEDIUM | MEDIUM (60-80h for PoC) | **P2** | Novel application domain. CPU-only proof sufficient. |
| G-1: Sequence diagram panel | MEDIUM | LOW (20-30h) | **P2** | Strengthens paper figures and user study. |
| D-3: Runtime topology manipulation | MEDIUM-HIGH | HIGH (60h) | **P2** | Strong differentiator but risky (Play Mode persistence). |
| G-2: Unity Profiler integration | LOW-MEDIUM | LOW (20h) | **P3** | Polish feature. Not novel but improves adoption story. |
| G-5: Heatmap overlay | LOW | LOW (8h) | **P3** | Quick win for visual polish. |
| G-7: Accessibility | LOW-MEDIUM | MEDIUM (20-30h) | **P3** | Reviewer goodwill. At minimum, mention in limitations. |

**Priority key:**
- P1: Must have for UIST submission (core contribution)
- P2: Should have, significantly strengthens paper
- P3: Nice to have, polish and reviewer goodwill

---

## Competitor Feature Analysis

| Feature | Unreal Blueprints | ROS2 rqt_graph / Foxglove | Stately.ai (XState) | Replay.io / Dbux | Unity Visual Scripting | **Mercury (Proposed)** |
|---------|-------------------|---------------------------|---------------------|------------------|----------------------|----------------------|
| Node-graph editor | YES (static) | YES (read-only) | YES (interactive) | NO | YES (static) | **YES (bi-directional, live)** |
| Live execution flow | YES (Active Wires) | Partial (topic rates) | YES (Inspector) | YES (time-travel) | YES (blue highlights) | **YES (animated message paths)** |
| Filter decision viz | NO | NO | NO | NO | Partial (predictions) | **YES (blockage indicators)** |
| Runtime graph editing | NO (must stop play) | NO | NO (Inspector is read-only) | NO | NO (must stop play) | **YES (play-mode editing)** |
| Time-travel replay | NO | YES (rosbag playback) | NO | YES (code-centric) | NO | **YES (message-centric)** |
| Spatial routing | NO | NO | NO | NO | NO | **YES (radius/frustum)** |
| Parallel FSMs | NO | NO | YES (parallel states) | NO | NO | **YES (orthogonal regions + message sync)** |
| Hierarchical routing | NO (flat events) | NO (flat pub/sub) | YES (hierarchical states) | NO | NO (flat) | **YES (scene graph hierarchy)** |
| "Why didn't arrive?" query | NO | NO | NO | NO | NO | **YES (filter decision trace)** |
| Multi-level filtering | NO | Topic-only | NO | NO | NO | **YES (5 orthogonal filters)** |
| Network-aware | YES | YES | NO | Web only | NO | **YES (FishNet/Fusion 2)** |
| User study evaluation | Mixed | NO | NO | NO | NO | **YES (planned N>=12)** |

**Key Takeaway:** Mercury's proposed tool suite occupies a unique position: it is the only tool that combines hierarchical routing visualization, filter decision debugging (blockage indicators), runtime graph editing, message-centric time-travel, spatial routing, and parallel FSMs. The closest competitors are Unreal Blueprints (live flow visualization only), Foxglove/ROS2 (robotics-specific, read-only), and Stately.ai (parallel states but no game engine integration).

---

## Comparison with Existing dev/active/ Plans

### Areas of AGREEMENT (research confirms plans are well-targeted)

1. **Visual composer with bi-directional editing** -- The planned approach (GraphViewBase + custom GL + IMGUI runtime debugger) targets a genuine gap. No existing Unity or game engine tool provides this combination.
2. **Blockage indicators** -- Confirmed as the strongest novelty claim. Zero existing tools provide "why message was filtered" visualization.
3. **Message-centric time-travel** -- Confirmed as a novel framing. All existing time-travel debuggers are code-centric. Differentiating from Replay.io/JIVE is achievable with this framing.
4. **Asymmetry analysis benchmarks** -- The Python prototype validates the concept. The planned C# benchmark (40h) is well-scoped.
5. **User study design** -- NASA-TLX + time-to-fix is standard and appropriate. The 5-scene comparison study is comprehensive.

### Areas of DISAGREEMENT (research suggests adjustments)

1. **Spatial indexing scope is too large (360h)** -- Full GPU octree is a separate SIGGRAPH-caliber contribution. For UIST, a CPU-based proof-of-concept (~60-80h) demonstrating spatial filtering in the fluent API would be sufficient. RECOMMEND: Descope to CPU-only radius + frustum queries.
2. **Parallel FSM scope is too large (280h)** -- Full implementation with thread safety, conflict resolution, and dynamic regions is excessive for one section of a paper. RECOMMEND: Descope to proof-of-concept with 2-3 orthogonal regions, message-based sync demo, and one VR multimodal use case.
3. **Distributed messaging (80h) includes risky features** -- Atomic transactions (Phase 3) and distributed topology awareness (Phase 4) are high-risk. RECOMMEND: For UIST, implement only Phase 1 (distribution semantics) and Phase 2 (change notification with veto). Connect veto to blockage indicators.

### Areas where research ADDS NEW features not currently planned

1. **G-1: Sequence diagram visualization** -- NOT in any current plan. Standard visualization for event-driven systems in academic papers. High value for both the tool and paper figures. STRONGLY RECOMMEND adding (~20-30h).
2. **G-2: Unity Profiler integration** -- NOT in any current plan. Low effort (~20h) to add a custom Profiler module showing message throughput and filter stats. Demonstrates integration with existing developer workflows.
3. **G-5: Message frequency heatmap** -- NOT in any current plan. Very low effort (~8h) to color-code graph nodes by message frequency. Provides immediate "wow factor" in screenshots.
4. **G-4: Veto visualization connected to blockage indicators** -- Distributed-messaging Phase 2 plans veto callbacks but does NOT connect them to visual tooling. RECOMMEND explicit connection.
5. **G-7: Basic accessibility** -- NOT mentioned in any plan. CHI/UIST reviewers increasingly flag this. At minimum, add keyboard navigation and mention accessibility in the paper.

---

## Sources

### Academic Papers & Conferences
- [Olsen, "Evaluating User Interface Systems Research," UIST 2007](https://dl.acm.org/doi/10.1145/1294211.1294256) -- MEDIUM confidence (foundational, still cited in CHI 2025 reviewer guidelines)
- [UIST 2025 Author Guide](https://uist.acm.org/2025/author-guide/) -- HIGH confidence (official source)
- [CHI 2025 Guide to Reviewing Papers](https://chi2025.acm.org/guide-to-reviewing-papers/) -- HIGH confidence (official source)
- [Frolich et al., "Exploratory, Omniscient, and Multiverse Diagnostics," SLE 2025](https://dl.acm.org/doi/10.1145/3732771.3742719) -- MEDIUM confidence (published proceedings)
- [Dbux: Program Dependence Graph Visualization, VISSOFT 2022](https://domiii.github.io/dbux/) -- MEDIUM confidence (published + live tool)
- [JIVE: Java Interactive Visualization Environment](https://www.researchgate.net/figure/Jive-s-architecture_fig2_228716472) -- MEDIUM confidence (established academic tool)
- [Bedard et al., "Message flow analysis with complex causal links for distributed ROS 2 systems"](https://www.sciencedirect.com/science/article/abs/pii/S0921889022002500) -- MEDIUM confidence (ScienceDirect published)
- [i-Octree, ICRA 2024](https://github.com/zhujun3753/i-octree) -- MEDIUM confidence (ICRA proceedings)
- [DeepFlow, IUI 2025](https://dl.acm.org/doi/10.1145/3708359.3712109) -- MEDIUM confidence (published proceedings)
- [DEBT 2025 Workshop on Future Debugging Techniques](https://2025.ecoop.org/home/debt-2025) -- LOW confidence (workshop, not proceedings)

### Commercial/Open-Source Tools
- [Replay.io Documentation](https://docs.replay.io/) -- HIGH confidence (official docs)
- [Foxglove Studio](https://foxglove.dev/) -- HIGH confidence (official product)
- [Stately.ai / XState Documentation](https://stately.ai/docs) -- HIGH confidence (official docs)
- [Stately Inspector](https://stately.ai/blog/2024-01-15-stately-inspector) -- HIGH confidence (official blog)
- [Unity Visual Scripting Debugging](https://docs.unity3d.com/Packages/com.unity.visualscripting@1.7/manual/vs-debugging.html) -- HIGH confidence (official Unity docs)
- [Unreal Blueprint Debugger](https://dev.epicgames.com/documentation/en-us/unreal-engine/blueprint-debugger-in-unreal-engine) -- HIGH confidence (official Epic docs)
- [Unity Network Profiler](https://docs.unity3d.com/Packages/com.unity.multiplayer.tools@2.2/manual/profiler.html) -- HIGH confidence (official Unity docs)
- [ROS 2 rqt_graph](https://roboticsbackend.com/rqt-graph-visualize-and-debug-your-ros-graph/) -- MEDIUM confidence (tutorial, verified against official docs)
- [Nition/UnityOctree](https://github.com/Nition/UnityOctree) -- MEDIUM confidence (well-known open source)
- [XState Parallel States](https://stately.ai/docs/parallel-states) -- HIGH confidence (official docs)

### Industry/Community
- [Unity Discussions: Node graph debugging request (2025)](https://discussions.unity.com/t/request-for-node-graph-based-visualization-debugging-tooling/1617858) -- LOW confidence (community discussion, confirms demand)
- [JetBrains Game Dev Report 2025](https://blog.jetbrains.com/dotnet/2026/01/29/game-dev-in-2025-excerpts-from-the-state-of-game-development-report/) -- MEDIUM confidence (industry survey)
- [Agent Tracing for Debugging Multi-Agent AI Systems](https://www.getmaxim.ai/articles/agent-tracing-for-debugging-multi-agent-ai-systems/) -- LOW confidence (single commercial source)
- [Undo/UDB Time Travel Debugging](https://undo.io/) -- HIGH confidence (official product)
- [Microsoft TTD](https://learn.microsoft.com/en-us/windows-hardware/drivers/debuggercmds/time-travel-debugging-overview) -- HIGH confidence (official Microsoft docs)

---

*Feature research for: Message Routing Visualization/Debugging Tools for UIST 2026*
*Researched: 2026-02-11*
