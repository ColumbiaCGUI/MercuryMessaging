# MercuryMessaging

## What This Is

MercuryMessaging is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture, eliminating the need for direct component references. The framework targets research applications in VR/XR, human-robot interaction, and user studies, with a fluent DSL API, built-in networking (FishNet), and finite state machine integration.

## Core Value

Provide the most expressive and debuggable message routing system for Unity, enabling researchers to build complex hierarchical applications with minimal boilerplate while maintaining full control over message flow, filtering, and network distribution.

## Requirements

### Validated

<!-- Shipped and confirmed valuable (Track 1: Production Engineering — COMPLETE). -->

- ✓ Hierarchical message routing through Unity's scene graph — v4.0.0
- ✓ Multi-level filtering (Level, Active, Tag, Network, Selected) — v4.0.0
- ✓ Fluent DSL API with 86% code reduction (Send/Broadcast/Notify) — v4.0.0
- ✓ MmBaseResponder method routing for standard methods (0-18) — v4.0.0
- ✓ MmExtendableResponder registration-based custom method handling (>1000) — v4.0.0
- ✓ FishNet networking with automatic serialization (15/15 loopback tests) — v4.0.0
- ✓ Photon Fusion 2 networking scaffolded (conditional compilation) — v4.0.0
- ✓ Finite state machine integration via MmRelaySwitchNode — v4.0.0
- ✓ Standard Library: UI messages (Click, Hover, Drag, Scroll, Focus, Select, Submit, Cancel) — v4.0.0
- ✓ Standard Library: VR Input messages (6DOF, Gesture, Haptic, Button, Axis, Touch, Gaze) — v4.0.0
- ✓ Performance optimization: 2-2.2x frame time, 3-35x throughput (QW-1 through QW-6) — v4.0.0
- ✓ Source generators: [MmGenerateDispatch] for compile-time dispatch optimization — v4.0.0
- ✓ 15 Roslyn analyzers (MM001-MM015) for framework-specific static analysis — v4.0.0
- ✓ Hop limit protection and cycle detection for safe deep hierarchies — v4.0.0
- ✓ Lazy message copying (zero-copy for single-direction routing) — v4.0.0
- ✓ CircularBuffer for bounded message history (no memory leaks) — v4.0.0
- ✓ Project reorganization (14 to 6 top-level folders, ~500MB build reduction) — v4.0.0
- ✓ Task management system for experimental workflows — v4.0.0
- ✓ 14 tutorial wiki drafts covering full framework — v4.0.0

### Active

<!-- Current scope: UIST 2026 paper + research pipeline. -->

**UIST 2026 Paper (Milestone 1 — current):**

- [ ] Tutorial scenes validated against wiki documentation (12 scenes)
- [ ] Visual Composer: bi-directional graph editor for live message routing authoring and introspection
- [ ] Asymmetry Analysis: benchmarks for message propagation under graph asymmetry across networked peers
- [ ] GPU-accelerated spatial indexing with adaptive octree for spatial-hierarchical routing
- [ ] Parallel hierarchical FSMs for concurrent multi-modal XR input handling
- [ ] Time-travel debugging: message replay with timeline scrubber and filter decision visualization
- [ ] Distributed messaging patterns: distribution semantics, structured change notification, atomic messaging, network topology awareness (MacIntyre-inspired)

**Research Pipeline (Milestone 2 — future):**

- [ ] User study: Mercury vs Unity Events comparison (CHI LBW)
- [ ] Accessibility-first game framework using tag-based modes (CHI/ASSETS)
- [ ] Digital twin communication layer bridging MQTT/IoT with Mercury hierarchy (IEEE IoT/CHI)
- [ ] Multi-user XR collaboration framework with role-based permissions (CHI/IEEE VR)
- [ ] LLM-assisted message network design from natural language (CHI/UIST)
- [ ] Hybrid safety verification: compile-time SCC + runtime Bloom filter (ICSE)
- [ ] Parallel message dispatch with thread-safe data structures and Unity Job System (SIGGRAPH)

### Out of Scope

- General-purpose game engine replacement — Mercury is a messaging layer, not a full engine
- Non-Unity platforms — Unity-only for the foreseeable future
- Real-time audio/video streaming — out of scope, use dedicated solutions
- Commercial packaging/Asset Store release — research-first, open source

## Context

- **Origin:** Columbia University CGUI lab, evolved from CHI 2018 paper
- **Current state:** v4.0.0 nearly released (23/26 release tasks complete). Track 1 (Production Engineering) complete. Wiki tutorials drafted. Publishing scripts created.
- **Remaining v4.0.0 manual steps:** Publish wiki via scripts, export Unity package, create GitHub release with tag v4.0.0
- **Research direction:** UIST 2026 paper as primary target, with secondary submissions to CHI, IEEE VR, ICSE, SIGGRAPH, IEEE IoT
- **Naming consideration:** Project may be renamed to "MercuryLive" or "PRISM" to distinguish from original CHI 2018 paper (see dev-archive for analysis)
- **Performance baseline:** 58-70 FPS at 1000 msg/sec (editor), 28x overhead vs direct calls (acceptable for decoupling benefits)
- **Test coverage:** 200+ automated tests (CircularBuffer, hop limits, cycle detection, lazy copy, FSM, networking)
- **Key reference:** MacIntyre (1998) "Exploratory Programming of Distributed Augmented Environments" — informs distributed messaging patterns

## Constraints

- **Tech stack:** Unity 2021.3+ with .NET Standard 2.1 — must maintain backward compatibility
- **Zero dependencies:** Core framework must have ZERO third-party dependencies (only UnityEngine, System.*)
- **Optional deps:** FishNet (#if FISH_NET) and Photon Fusion 2 (#if FUSION_WEAVER) wrapped in conditional compilation
- **Naming convention:** Core files must use "Mm" prefix (MmRelayNode, MmMessage, etc.)
- **Testing:** All tests must be fully automated (no manual scenes, no prefab dependencies)
- **Serialization:** Force Text serialization for all Unity assets
- **Performance:** Must maintain 60+ FPS at scale; PerformanceMode flag for production builds

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Consolidated namespace (single `using MercuryMessaging`) | 95% of functionality accessible with one import | ✓ Good |
| Fluent DSL as recommended API | 86% code reduction, zero heap allocations, full type safety | ✓ Good |
| FishNet as primary networking | Open source, well-maintained, good Unity integration | ✓ Good |
| Separate milestones (UIST vs Pipeline) | UIST 2026 has concrete deadline; other venues are more flexible | — Pending |
| MacIntyre-inspired distributed patterns | Grounded in established dissertation research from same lab | — Pending |
| Tutorial validation before user study | Can't run study if tutorials don't work | — Pending |

---
*Last updated: 2026-02-11 after GSD initialization*
