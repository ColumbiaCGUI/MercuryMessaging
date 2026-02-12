# Architecture Research: UIST 2026 Feature Integration

**Domain:** Unity Hierarchical Message Routing Framework (MercuryMessaging v4.0.0+)
**Researched:** 2026-02-11
**Confidence:** HIGH (based on direct source code analysis + existing architecture plans)

---

## Executive Summary

Seven features must integrate with MercuryMessaging's established architecture centered on `MmRelayNode.MmInvoke()` as the single routing hot-path. After analyzing the existing codebase, all seven feature plans, and domain research, I identify three architectural layers for integration: (1) **Core Pipeline Hooks** that touch MmRelayNode.MmInvoke directly, (2) **Companion Components** that sit alongside relay nodes without modifying the hot-path, and (3) **Editor-Only Systems** that observe but never alter runtime behavior. The critical architectural risk is "hot-path bloat" -- every feature that adds conditionals to MmInvoke degrades the 14-17ms frame time established by QW-1 through QW-6.

---

## Existing Architecture (Baseline)

### System Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         EDITOR LAYER                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │ Unity        │  │ Custom       │  │ GraphViewBase│              │
│  │ Inspector    │  │ Editors      │  │ (MIT)        │              │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘              │
│         │ SerializedObject │ ScriptableObj   │ Editor Window        │
├─────────┴──────────────────┴────────────────┴──────────────────────┤
│                         RUNTIME LAYER                               │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────┐        │
│  │                  MmRelayNode.MmInvoke()                 │ ◄──── HOT PATH
│  │  ┌──────────┐  ┌────────────┐  ┌───────────────┐       │        │
│  │  │ Hop/Cycle│  │ Routing    │  │ Responder     │       │        │
│  │  │ Detection│  │ Table Scan │  │ Dispatch      │       │        │
│  │  └──────────┘  └────────────┘  └───────────────┘       │        │
│  └──────────────────────┬──────────────────────────────────┘        │
│                         │                                           │
│  ┌──────────┐  ┌────────┴───────┐  ┌───────────────┐               │
│  │ MmMessage│  │ MmRoutingTable │  │ MmMetadataBlock│              │
│  │ (payload)│  │ (O(1) lookup)  │  │ (5 filters)   │              │
│  └──────────┘  └────────────────┘  └───────────────┘               │
│                                                                     │
│  ┌─────────────────────────────┐  ┌──────────────────────┐         │
│  │ MmRelaySwitchNode           │  │ DSL Extension Methods│         │
│  │ (FSM via FiniteStateMachine)│  │ (Send/Broadcast/     │         │
│  └─────────────────────────────┘  │  Notify/Query)       │         │
│                                   └──────────────────────┘         │
├─────────────────────────────────────────────────────────────────────┤
│                         NETWORK LAYER                               │
│  ┌───────────────┐  ┌────────────────┐  ┌──────────────────┐       │
│  │ MmNetworkBridge│  │ MmBinary      │  │ IMmNetworkBackend│       │
│  │ (Singleton)   │  │ Serializer    │  │ (FishNet/Fusion) │       │
│  └───────────────┘  └────────────────┘  └──────────────────┘       │
└─────────────────────────────────────────────────────────────────────┘
```

### Key Integration Points in MmRelayNode.MmInvoke()

The `MmInvoke(MmMessage)` method (lines 805-1080 in `MmRelayNode.cs`) is the single routing hot-path. Its current execution order:

1. **Debug tracking** (`UpdateMessages` -- gated by `PerformanceMode` flag)
2. **Listener notification** (`NotifyListeners` -- DSL Phase 2.1)
3. **Hop limit check** (`maxMessageHops` -- QW-1)
4. **Cycle detection** (`enableCycleDetection` + `VisitedNodes` -- QW-1)
5. **Network send** (to `MmNetworkResponder` if NetworkFilter != Local)
6. **Lazy copy decision** (single vs multi-direction routing -- QW-2)
7. **Routing table iteration** (filter checks + dispatch to responders)
8. **Advanced routing** (`HandleAdvancedRouting` -- Phase 2.1 extensions)
9. **Queued responder processing** (runtime additions)

### Existing Hook Points (Already Available)

| Hook | Location | Purpose | Available To |
|------|----------|---------|--------------|
| `PerformanceMode` check | Line 808 | Gate debug overhead | Time-Travel, Visual Composer |
| `NotifyListeners()` | Line 815 | Subscribe to messages without creating responders | Time-Travel, Visual Composer, Distributed |
| `VisitedNodes` on MmMessage | Line 846 | Track message path | Time-Travel |
| `HandleAdvancedRouting()` | Line 1050 | Virtual, overridable | Spatial Indexing |
| `MmRoutingTable` indices | O(1) lookups | Name + Responder | All features |
| `MmRoutingChecks` flags | Line 136 | Selective filter bypass | Performance-sensitive features |

---

## Feature Integration Architecture

### 1. Visual Composer (Editor Graph Tool)

**Integration Type:** Editor-Only Observer (Layer 3)

**Assessment of Existing Plan:** SOUND. The plan to use GraphViewBase (MIT) for 2D graph, custom GL for 3D scene visualization, and IMGUI for runtime debugger is architecturally correct. The separation into three rendering subsystems matches Unity's rendering contexts (Editor window, Scene view, Game view).

**RISK: Unity Graph Toolkit (GTK) Competition.** Unity released the experimental Graph Toolkit package in Unity 6.2 (2025). GTK provides a more modern graph authoring framework than GraphViewBase. However, GTK is experimental and not yet stable -- sticking with GraphViewBase (MIT, already imported) is the safer choice for a UIST 2026 deadline. GTK migration could be a future enhancement.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmComposerWindow` (EditorWindow) | 2D graph editing via GraphViewBase | MmRelayNode (via SerializedObject) |
| `MmNodeView` (extends BaseNode) | Visual representation of MmRelayNode | MmComposerWindow, Scene |
| `MmEdgeView` (extends BaseEdge) | Visual representation of routing table entries | MmComposerWindow |
| `MmSceneDrawer` (MonoBehaviour) | 3D GL line rendering in Scene view | MmRelayNode (reads RoutingTable) |
| `MmRuntimeDebugger` (MonoBehaviour) | IMGUI message stream overlay | MmRelayNode.NotifyListeners |

**Data Flow:**

```
Scene Objects ──SerializedObject──► MmComposerWindow ──GraphViewBase──► Visual Graph
                                          │
                                          ▼ (bi-directional sync)
MmRelayNode.RoutingTable ◄──────── Editor Manipulation
                                          │
MmRelayNode.NotifyListeners ────► MmRuntimeDebugger ──IMGUI──► Game View Overlay
                                          │
MmRelayNode (positions) ────────► MmSceneDrawer ──GL.Begin──► Scene View Lines
```

**Integration Point:** The Listener API (`AddListener<T>` on MmRelayNode, already implemented at lines 711-787) is the clean hook for runtime introspection. Visual Composer should NOT add any code to the MmInvoke hot-path. Instead, it subscribes via `NotifyListeners` which already runs after the PerformanceMode gate.

**MISSING in Existing Plan:** The plan describes `MmIntrospectionHook` with a static event `OnMessagePropagation` that captures rejected responders and rejection reasons. This requires modifying `ResponderCheck` to record WHY each responder was rejected. Currently, `ResponderCheck` returns a simple boolean -- there is no infrastructure to capture rejection reasons. This is a non-trivial change to the hot-path that must be gated behind a flag (similar to PerformanceMode).

**Suggested Amendment:** Add an `IntrospectionMode` static flag (default: false) to MmRelayNode. When enabled, `ResponderCheck` populates a thread-local `RejectionRecord` struct. This keeps the hot-path clean when introspection is disabled.

**Build Dependencies:** None from other features. Can be built first.

---

### 2. Spatial Indexing (GPU Compute)

**Integration Type:** Companion Component (Layer 2)

**Assessment of Existing Plan:** RISKY. The plan proposes `SpatialRouter : MmRelayNode` which would create a new relay node subclass. This is architecturally wrong because it forces users to replace their existing MmRelayNodes with a new type, breaking existing scenes.

**RISK: Subclass Proliferation.** MercuryMessaging already has MmRelayNode and MmRelaySwitchNode. Adding SpatialRouter as a third subclass creates combinatorial problems (what about spatial + FSM? Do we need SpatialSwitchNode?). This path leads to "inheritance diamond" problems.

**Recommended Architecture:** Composition over inheritance. Spatial indexing should be a **companion component** that attaches alongside an MmRelayNode, not a subclass of it.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmSpatialIndex` (MonoBehaviour) | Maintains octree of responder positions | MmRelayNode (reads RoutingTable) |
| `MmSpatialQueryEngine` | CPU/GPU query execution | MmSpatialIndex, Compute Shaders |
| `MmSpatialFilter` (struct) | Query parameters (radius, cone, frustum) | MmFluentMessage (DSL extension) |
| `SpatialQuery.compute` | GPU compute kernel for parallel tree traversal | MmSpatialQueryEngine |

**Data Flow:**

```
MmRelayNode.RoutingTable ──positions──► MmSpatialIndex (Octree)
                                              │
MmFluentMessage.Within(10f) ──SpatialFilter──► MmSpatialQueryEngine
                                              │
                                              ▼ (CPU or GPU path)
                                        Filtered Responder List
                                              │
                                              ▼
                              MmRelayNode.MmInvoke() with filtered targets
```

**Integration Point:** The existing DSL already supports spatial filtering via `Within()` and `InCone()` in `MmFluentPredicates.cs`. The current implementation uses linear scan (`Where(go => ...)` predicate). Spatial indexing replaces this linear scan with O(log n) octree lookup -- the integration is at the predicate evaluation layer, NOT in MmInvoke itself.

**MISSING in Existing Plan:** No discussion of how octree updates track responder movement. Unity GameObjects move constantly. The plan needs an `Update()` strategy: either (a) dirty-flag per-responder that triggers re-insertion, or (b) periodic full rebuild at a configurable frequency. Option (a) is better for sparse movement, option (b) for dense movement. The plan should specify both with an auto-select heuristic.

**MISSING:** GPU compute requires Shader Model 5.0 which excludes some mobile/WebGL targets. The plan needs a CPU fallback path (pure C# octree without compute shaders). This is a hard requirement for a framework that targets broad Unity platforms.

**Build Dependencies:** Depends on DSL infrastructure (already exists). Independent of all other features.

---

### 3. Parallel FSMs

**Integration Type:** Core Extension (Layer 1 -- but contained)

**Assessment of Existing Plan:** RISKY. The plan proposes `Parallel.ForEach` for concurrent region execution. This is dangerous in Unity because:
1. Unity APIs (transform, gameObject, etc.) are NOT thread-safe
2. MmRelayNode.MmInvoke modifies shared state (`doNotModifyRoutingTable`, message queues)
3. The existing `FiniteStateMachine<T>` has no thread-safety mechanisms

Using `Parallel.ForEach` would cause race conditions and crashes.

**Recommended Architecture:** Parallel FSM regions should execute **sequentially within the same frame** using a deterministic ordering, not actual parallel threads. "Parallel" means "logically concurrent" (all regions process the same frame's events) not "thread-parallel." This matches UML Statechart semantics where orthogonal regions execute within the same run-to-completion step.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmParallelSwitchNode` (extends MmRelaySwitchNode) | Manages multiple FSM regions | MmRelaySwitchNode (inherits FSM) |
| `MmParallelRegion` (class) | One orthogonal region with its own FSM | MmParallelSwitchNode |
| `MmConflictResolver` (interface) | Resolves competing transitions | MmParallelSwitchNode |
| `MmRegionSynchronizer` | Cross-region event delivery via Mercury messages | MmRelayNode (standard messaging) |

**Data Flow:**

```
Incoming MmMessage
       │
       ▼
MmParallelSwitchNode.MmInvoke()
       │
       ├──► Region 1 (Gesture FSM).ProcessEvent(message)
       ├──► Region 2 (Voice FSM).ProcessEvent(message)
       ├──► Region 3 (Gaze FSM).ProcessEvent(message)
       │
       ▼ (after all regions process)
MmConflictResolver.Resolve(pendingTransitions)
       │
       ▼
Cross-region sync messages via MmRelayNode
```

**Integration Point:** `MmParallelSwitchNode` extends `MmRelaySwitchNode`. The override of `SelectedCheck` (line 167 in MmRelaySwitchNode.cs) needs to check across ALL parallel regions, not just one. This is the critical integration point -- `SelectedCheck` must return true if the responder's parent matches ANY active region's current state.

**MISSING in Existing Plan:** No conflict resolution specification. When Region 1 says "transition to StateA" and Region 2 says "transition to StateB" and they affect the same sub-hierarchy, what happens? The plan mentions "Priority-based resolution" but does not define the priority model. Must define: (a) static priority (region ordering), (b) dynamic priority (message timestamp), or (c) veto-based (any region can block).

**MISSING:** No specification of how `RebuildFSM()` works with multiple regions. Currently `RebuildFSM()` (line 179) rebuilds a single FSM from `GetChildRelayNodes()`. Parallel regions need a different initialization path.

**Build Dependencies:** Depends on existing FSM (`FiniteStateMachine<T>`, `MmRelaySwitchNode`). Should be built AFTER Tutorial Validation confirms existing FSM behavior is correct.

---

### 4. Time-Travel Debugging

**Integration Type:** Editor-Only Observer (Layer 3) with Runtime Recording Component (Layer 2)

**Assessment of Existing Plan:** SOUND. The recording-at-MmInvoke approach is correct. The plan correctly identifies that recording must hook into MmInvoke to capture routing decisions.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmMessageRecorder` (MonoBehaviour) | Records all MmInvoke calls via listener API | MmRelayNode.NotifyListeners |
| `MmRecordedMessage` (struct) | Immutable snapshot of a message + routing decisions | MmMessageRecorder (storage) |
| `MmTimelineWindow` (EditorWindow) | Timeline scrubber UI | MmMessageRecorder (reads Timeline) |
| `MmQueryEngine` (class) | "Why didn't message reach X?" analysis | MmMessageRecorder (queries) |
| `MmRejectionTracker` (class) | Captures filter rejection reasons during routing | MmRelayNode (hot-path hook) |

**Data Flow:**

```
MmRelayNode.MmInvoke()
       │
       ├──► [IntrospectionMode only] MmRejectionTracker captures per-responder decisions
       │
       ├──► NotifyListeners() ──► MmMessageRecorder.OnMessage()
       │                                   │
       │                                   ▼
       │                          CircularBuffer<MmRecordedMessage>
       │                                   │
       │                          ◄── MmQueryEngine.Query()
       │                                   │
       │                          ◄── MmTimelineWindow (Editor UI)
       │
       ▼ (normal routing continues)
```

**Integration Point:** Two hooks needed:
1. **NotifyListeners** (already exists) -- captures message metadata, source, method. This provides WHAT was sent.
2. **RejectionTracker** (NEW, requires modification) -- captures WHY responders were rejected. This is shared with Visual Composer's "blockage indicators." Both features need the same data: per-responder rejection reasons during routing.

**SHARED INFRASTRUCTURE:** Visual Composer and Time-Travel Debugging both need rejection reason tracking. This should be built as a single shared component (`MmRejectionTracker`) gated behind `IntrospectionMode`, not duplicated.

**RISK: Memory.** Recording every MmInvoke call can consume significant memory. The plan correctly specifies CircularBuffer (matching QW-4 pattern), but needs to specify maximum buffer size and what happens when full. Recommendation: configurable buffer (default 10,000 messages), oldest dropped, with export-to-file capability for long sessions.

**Build Dependencies:** Depends on shared introspection infrastructure (shared with Visual Composer). Should be built AFTER Visual Composer's introspection hook is established.

---

### 5. Asymmetry Analysis

**Integration Type:** Test/Benchmark Harness (Layer 2 -- runtime tests)

**Assessment of Existing Plan:** SOUND. The plan correctly identifies that this is primarily a benchmark/analysis tool, not a runtime feature. The core finding -- that `ToDescendants` follows the receiver's hierarchy, providing automatic tolerance for extra nodes -- is already a property of the existing `MmRelayNode.MmInvoke()` routing.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmAsymmetryBenchmark` (test class) | Programmatic test harness | MmRelayNode, MmNetworkBridge |
| `MmGraphGenerator` (utility) | Creates asymmetric scene graphs | Unity GameObjects |
| `MmDeliveryTracker` (test responder) | Counts received messages per node | MmBaseResponder |
| Benchmark results (CSV) | Output data | File system |

**Data Flow:**

```
MmGraphGenerator ──creates──► Peer A Hierarchy (with missing nodes)
                 ──creates──► Peer B Hierarchy (with extra nodes)
                                    │
                                    ▼
MmRelayNode.MmInvoke(ToDescendants) ──routes──► MmDeliveryTracker (counts)
                                    │
                                    ▼
MmNetworkBridge ──serializes──► Peer B receives ──routes──► Tracker (counts)
                                    │
                                    ▼
                            Benchmark results CSV
```

**Integration Point:** No modifications to core framework needed. Uses existing APIs:
- `MmRelayNode.MmInvoke()` for routing
- `MmNetworkBridge.RouteMessage()` for network delivery (line 386-412 in MmNetworkBridge.cs)
- The "No relay node found for network ID" warning (line 410) is the existing graceful degradation for missing nodes

**MISSING in Existing Plan:** The plan references a Python prototype but the C# benchmark needs to model network serialization/deserialization overhead, not just local routing. Asymmetry over the network means the serialized NetId might not resolve on the receiving peer. The existing `RouteMessage` handles this with a warning log + silent drop (line 410), but the benchmark should quantify this behavior.

**Build Dependencies:** Depends on MmNetworkBridge (Phase 3 of plan). Can start Phase 1-2 immediately (local benchmarks).

---

### 6. Distributed Messaging

**Integration Type:** DSL Extension (Layer 2) + Network Layer Enhancement

**Assessment of Existing Plan:** SOUND with one RISK. The four-phase plan (Distribution Semantics, Change Notification, Atomic Messaging, Network Topology) is well-structured. The fluent API extensions (`.Replicated()`, `.AuthoritativeOn()`, `.LocalOnly()`) compose cleanly with existing DSL chain.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `DistributionMode` (enum) | Replicated/Authoritative/LocalOnly | MmMetadataBlock, MmFluentMessage |
| `MmDistributionExtensions` (static) | DSL chain methods | MmFluentMessage |
| `MmMessageCallbackRegistry` (class) | OnBeforeReceive/OnAfterReceive handlers | MmRelayNode |
| `MmTransaction` (class) | Atomic message batching | MmRelayNode |
| `MmLatencyTracker` (class) | Per-peer RTT measurement | MmNetworkBridge, IMmNetworkBackend |

**Data Flow:**

```
relay.Send(msg).Replicated().Execute()
       │
       ▼
MmFluentMessage sets DistributionMode on MmMetadataBlock
       │
       ▼
MmRelayNode.MmInvoke() ──► MmNetworkResponder/MmNetworkBridge
       │                           │
       │                    DistributionMode.Replicated:
       │                    ──► SendToAllClients()
       │
       │                    DistributionMode.Authoritative:
       │                    ──► Check ownership, proxy if remote
       │
       │                    DistributionMode.LocalOnly:
       │                    ──► Skip network entirely
       │
       ▼
MmMessageCallbackRegistry.OnBeforeReceive() ──► Accept/Reject/Defer
       │
       ▼ (if accepted)
Normal responder dispatch
       │
       ▼
MmMessageCallbackRegistry.OnAfterReceive()
```

**Integration Point:** Distribution semantics integrate at the network filter decision point in MmInvoke (line 872-888). Currently, the check is:
```csharp
if (MmNetworkResponder != null &&
    message.MetadataBlock.NetworkFilter != MmNetworkFilter.Local &&
    !message.IsDeserialized)
```
This needs extension to check `DistributionMode` and branch accordingly. This IS a hot-path modification, but it can be structured as a single enum switch that adds minimal overhead.

**RISK: MmMetadataBlock Serialization.** Adding `DistributionMode` to `MmMetadataBlock` changes the network serialization format (lines 150-181 in MmMetadataBlock.cs). The current format serializes 5 shorts (LevelFilter, ActiveFilter, SelectedFilter, NetworkFilter, Tag). Adding a 6th field breaks backward compatibility with existing network peers. Must implement version negotiation or reserve the field in the serialization format now.

**RISK: OnBeforeReceive Veto.** The plan proposes `MessageVerdict.Reject` which stops message delivery. If implemented inside the MmInvoke hot-path, a misbehaving callback could silently drop critical messages. Must ensure rejection is logged and that safety-critical messages (e.g., emergency stop) cannot be vetoed.

**Build Dependencies:** Phase 1 (Distribution Semantics) depends on MmNetworkBridge and MmMetadataBlock serialization. Phase 2 (Change Notification) is independent. Phase 3 (Atomic Messaging) depends on Phase 2. Phase 4 (Network Topology) depends on IMmNetworkBackend.

---

### 7. Tutorial Validation

**Integration Type:** Test Infrastructure (Layer 3 -- Editor/CI only)

**Assessment of Existing Plan:** INCOMPLETE. The current plan is manual validation (load scene, press play, follow steps). For a UIST 2026 paper, this needs to be automated. The plan should produce a test suite that runs in CI, not a human-executed checklist.

**Recommended Architecture:** Use Unity Test Framework's PlayMode tests with programmatic scene loading, following the `SceneValidationTests` pattern from Unity's official documentation.

**Component Boundaries:**

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `MmTutorialValidator` (test class) | PlayMode tests that load tutorial scenes | Unity Test Framework |
| `MmSceneValidator` (utility) | Validates scene structure (expected GameObjects, components) | Scene hierarchy |
| `MmBehaviorValidator` (utility) | Validates runtime behavior (messages sent, state changes) | MmRelayNode.NotifyListeners |
| Tutorial spec files (JSON/ScriptableObject) | Expected scene structure + behavior | Validators |

**Data Flow:**

```
Unity Test Runner ──LoadScene──► Tutorial Scene
                                      │
                   MmSceneValidator ──► Verify GameObjects exist
                   MmSceneValidator ──► Verify Components attached
                   MmSceneValidator ──► Verify Routing Table populated
                                      │
                   [Press virtual button / simulate input]
                                      │
                   MmBehaviorValidator ──► Listen for expected messages
                   MmBehaviorValidator ──► Verify state transitions
                   MmBehaviorValidator ──► Assert outcomes
                                      │
                                 Test Pass/Fail
```

**Integration Point:** Uses existing `NotifyListeners` API to observe message flow during automated test execution. No modifications to core framework needed.

**MISSING in Existing Plan:** No automation strategy. The plan lists manual validation steps. For UIST 2026, recommend:
1. Convert each tutorial's "expected behavior" into a JSON spec
2. Write a generic `MmTutorialValidator` that reads specs and validates programmatically
3. Include in CI pipeline via `Unity.exe -runTests -testPlatform PlayMode`

**Build Dependencies:** Depends on tutorial scenes being functional. Should be done FIRST to validate existing framework behavior before adding new features.

---

## Shared Infrastructure

Three features share infrastructure that should be built once:

### Introspection Subsystem (Shared by Visual Composer + Time-Travel)

```
MmRelayNode.MmInvoke()
       │
       ├──► [IntrospectionMode] MmRejectionTracker
       │         │
       │         ├──► Per-responder rejection reasons
       │         ├──► Filter that rejected (Level/Active/Selected/Tag/Network)
       │         └──► Formatted reason string
       │
       └──► NotifyListeners() ──► Any subscriber
                │
                ├──► MmMessageRecorder (Time-Travel)
                ├──► MmRuntimeDebugger (Visual Composer)
                └──► MmBehaviorValidator (Tutorial Validation)
```

**Implementation:** Add `IntrospectionMode` static bool to MmRelayNode. When true, `ResponderCheck` writes rejection data to a thread-local struct before returning false. This adds a single boolean check to the hot-path (~1ns) with no allocation when disabled.

---

## Anti-Patterns to Avoid

### Anti-Pattern 1: Subclassing MmRelayNode for Every Feature

**What people do:** Create `SpatialRelayNode`, `ParallelRelayNode`, `RecordingRelayNode` etc.
**Why it is wrong:** Creates N! combinations. Users need Spatial + Parallel + Recording = new subclass. Inheritance does not compose.
**Do this instead:** Use companion MonoBehaviours that attach alongside MmRelayNode. Use the Listener API for observation. Use `HandleAdvancedRouting` override only in MmRelaySwitchNode-derived types where FSM semantics genuinely change.

### Anti-Pattern 2: Unconditional Hot-Path Additions

**What people do:** Add new code directly into MmInvoke without gating.
**Why it is wrong:** Every nanosecond in MmInvoke multiplies by (messages/sec * responders). At 1000 msg/sec with 100 responders, 1ns becomes 100us/frame.
**Do this instead:** Gate all new hot-path code behind static boolean flags (like `PerformanceMode`). Default to OFF. Only enable when the feature is actually in use.

### Anti-Pattern 3: Breaking MmMetadataBlock Serialization

**What people do:** Add fields to MmMetadataBlock without versioning.
**Why it is wrong:** Network peers running different versions cannot communicate. The serialization format is a fixed array of shorts.
**Do this instead:** Use the existing `Options` field (MmRoutingOptions, already present at line 74) for local-only extensions. For network-transmitted extensions, implement version negotiation in MmBinarySerializer.

---

## Suggested Build Order

Based on dependency analysis and risk assessment:

```
Phase 0 (Foundation):  Tutorial Validation ──────────────────► Validates baseline
    │
    ▼
Phase 1 (Shared):      Introspection Subsystem (IntrospectionMode + RejectionTracker)
    │                   ──────────────────────► Shared by Visual Composer + Time-Travel
    │
    ├──► Phase 2a:      Visual Composer (uses Introspection) ──► UIST Major Contribution I
    ├──► Phase 2b:      Asymmetry Analysis (independent) ──────► UIST Contribution C4
    └──► Phase 2c:      Tutorial Validation automation ─────────► CI regression tests
    │
    ▼
Phase 3 (Core):        Parallel FSMs ──────────────────────────► Requires validated FSM
    │                   Spatial Indexing ───────────────────────► Independent, GPU work
    │
    ▼
Phase 4 (Integration): Time-Travel Debugging (uses Introspection + Visual Composer data)
    │                   Distributed Messaging (network layer, highest risk)
    │
    ▼
Phase 5 (Validation):  Full integration testing across all features
```

### Build Order Rationale

1. **Tutorial Validation first** because it validates the existing framework behavior that all other features depend on. If tutorials are broken, we are building on a broken foundation.

2. **Introspection Subsystem second** because Visual Composer and Time-Travel both need it. Building it once avoids duplicate hot-path modifications.

3. **Visual Composer and Asymmetry Analysis in parallel** because they have no dependencies on each other. Visual Composer is the primary UIST contribution and should start early. Asymmetry Analysis is a focused benchmark that can run concurrently.

4. **Parallel FSMs and Spatial Indexing third** because they modify core runtime behavior. Parallel FSMs depend on validated FSM behavior (from Tutorial Validation). Spatial Indexing is computationally isolated but needs careful CPU fallback design.

5. **Time-Travel and Distributed Messaging last** because they are the highest integration risk. Time-Travel depends on Visual Composer's introspection data model. Distributed Messaging modifies network serialization, which is the riskiest change for backward compatibility.

---

## Integration Risk Assessment

| Feature | Risk Level | Primary Risk | Mitigation |
|---------|-----------|-------------|------------|
| Visual Composer | MEDIUM | GraphViewBase stability across Unity versions | Pin to specific Unity 6.3 version |
| Spatial Indexing | MEDIUM | GPU compute not available on all platforms | Mandatory CPU fallback path |
| Parallel FSMs | HIGH | Thread-safety if using Parallel.ForEach; SelectedCheck logic complexity | Use sequential per-frame execution, NOT threads |
| Time-Travel Debugging | LOW | Memory consumption from recording | CircularBuffer with configurable cap |
| Asymmetry Analysis | LOW | Benchmark accuracy requires real network | Start with local simulation, add network Phase 3 |
| Distributed Messaging | HIGH | MmMetadataBlock serialization backward compatibility; veto semantics | Version negotiation; non-vetoable message flag |
| Tutorial Validation | LOW | Unity version differences in scene loading | Pin to Unity 6.3 LTS |

---

## Scaling Considerations

| Concern | 10 Responders | 100 Responders | 1000+ Responders |
|---------|---------------|----------------|------------------|
| IntrospectionMode overhead | Negligible (<1%) | ~2-3% frame time | ~5% frame time (consider sampling) |
| Spatial Index updates | CPU-only sufficient | CPU sufficient, GPU optional | GPU recommended |
| Parallel FSM regions | 1-3 regions typical | 5-10 regions | Consider region grouping |
| Time-Travel buffer | 10K messages ~5MB | 10K messages ~10MB | Reduce buffer or use file streaming |
| Network serialization | Negligible | ~1ms per frame | Batch + compress |

---

## Sources

- MmRelayNode.cs source analysis: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Nodes\MmRelayNode.cs`
- MmRelaySwitchNode.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Nodes\MmRelaySwitchNode.cs`
- MmMetadataBlock.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Filters\MmMetadataBlock.cs`
- MmMessage.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Message\MmMessage.cs`
- MmRoutingTable.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Routing\MmRoutingTable.cs`
- MmNetworkBridge.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Network\MmNetworkBridge.cs`
- IMmNetworkBackend.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\Network\IMmNetworkBackend.cs`
- FiniteStateMachine.cs: `C:\Users\yangb\Research\MercuryMessaging\Assets\MercuryMessaging\Protocol\FSM\FiniteStateMachine.cs`
- [Unity Graph Toolkit (GTK)](https://docs.unity3d.com/Packages/com.unity.graphtoolkit@0.1/manual/introduction.html) -- experimental graph authoring framework
- [Unity's 2026 Roadmap](https://digitalproduction.com/2025/11/26/unitys-2026-roadmap-coreclr-verified-packages-fewer-surprises/) -- stability focus, GTK in 6.2
- [Unity Test Framework Scene Validation](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/course/LostCrypt/scene-validation-test.html) -- official pattern for automated scene validation
- [UML State Machines (Wikipedia)](https://en.wikipedia.org/wiki/UML_state_machine) -- orthogonal regions execute within same run-to-completion step
- [CHSM: Concurrent Hierarchical FSM](https://chsm.sourceforge.net/) -- production concurrent FSM implementation
- [Octree Collision Detection in Unity3D (MDPI 2025)](https://www.mdpi.com/2673-4591/89/1/37) -- GPU compute shader 26x speedup
- [Nition/UnityOctree](https://github.com/Nition/UnityOctree) -- reference C# octree implementation
- [i-Octree: Fast Proximity Search](https://arxiv.org/pdf/2309.08315) -- dynamic octree, 19% faster than alternatives
- [Arium: XR Testing Framework](https://github.com/thoughtworks/Arium) -- ThoughtWorks XR automation pattern

---

*Architecture research for: MercuryMessaging UIST 2026 Feature Integration*
*Researched: 2026-02-11*
