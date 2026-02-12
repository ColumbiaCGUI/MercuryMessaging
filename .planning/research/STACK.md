# Stack Research: MercuryMessaging UIST 2026 Capabilities

**Domain:** Hierarchical message routing framework extensions for Unity (graph editor, GPU spatial indexing, parallel FSMs, time-travel debugging, distributed messaging, asymmetry analysis)
**Researched:** 2026-02-11
**Confidence:** MEDIUM-HIGH (most technologies verified via official docs; some experimental packages carry risk)

---

## Project Context

**Current Stack:**
- Unity 6000.3.7f1 (Unity 6.3 LTS)
- Burst 1.8.27, Collections 2.6.3, Mathematics 1.3.3 (already installed)
- GraphViewBase (Gentlymad Studios, MIT) already imported via manifest
- FishNet (networking), Photon Fusion 2 (alternative networking)
- .NET Standard 2.1
- Zero third-party dependencies in core framework

**Target:** Add visual composer, GPU spatial indexing, parallel FSMs, time-travel debugging, distributed messaging patterns, and asymmetry analysis benchmarks for UIST 2026 submission.

---

## Recommended Stack

### 1. Graph Visualization in Unity Editor

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| GraphViewBase (Gentlymad) | latest (MIT) | 2D graph editor for Visual Composer | Already imported in project. MIT licensed. Built on UIToolkit, avoids deprecated Experimental.GraphView. Zero-dependency approach aligns with project policy. | HIGH |
| Unity UI Toolkit | built-in (6000.3) | Editor window framework, styling, data binding | Unity's recommended replacement for IMGUI in editor tools. Native to Unity 6. USS styling, UXML layouts, live-editing support. | HIGH |
| GL.Begin/GL.End + Handles | built-in (6000.3) | 3D scene view connection drawing | Zero-dependency, works in both Scene view and builds. URP/HDRP compatible via `RenderPipelineManager.endCameraRendering`. Existing ALINE patterns in project for reference. | HIGH |
| IMGUI (OnGUI) | built-in (6000.3) | Runtime debugging overlay | Only viable option for runtime (non-editor) debug UI that works everywhere. UIToolkit runtime is possible but IMGUI is simpler for transient debug overlays. | HIGH |

**Decision: GraphViewBase over Unity Graph Toolkit (GTK)**

The project already has GraphViewBase imported and planned. Unity's Graph Toolkit (com.unity.graphtoolkit) is a strong alternative that launched as experimental (0.1.0-exp.1) in Unity 6.2, with version 0.4.0-exp.2 available for Unity 6.3. However:

- **GTK is experimental** (0.x version, "not meant for production use" per Unity docs)
- **GTK becomes a built-in module in Unity 6.4**, meaning if the project upgrades, the package install would conflict
- **GraphViewBase is already imported**, reducing integration risk
- **GTK is the future** -- if the project upgrades to Unity 6.4+, GTK would be the natural choice since it becomes a core module

**Recommendation:** Stay with GraphViewBase for the UIST 2026 timeline. Evaluate migrating to GTK when the project moves to Unity 6.4+ (where GTK is a built-in module, not an experimental add-on). The migration path should be straightforward since both are UIToolkit-based node graph frameworks.

### 2. GPU Compute for Spatial Indexing

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| Unity Compute Shaders (HLSL) | built-in (6000.3) | GPU-accelerated spatial queries (octree traversal, radius search, kNN) | Zero-dependency, cross-platform (Shader Model 5.0+). 2025 IEEE paper validated 26x speedup over CPU for octree-based collision in Unity. Direct `StructuredBuffer<T>` / `RWStructuredBuffer<T>` mapping. | HIGH |
| ComputeBuffer / GraphicsBuffer | built-in (6000.3) | CPU-GPU data transfer for spatial index | Standard Unity API. Use `NativeArray<T>` with `SetData` for zero-copy transfer. Struct stride must be multiple of 16 for cross-platform safety. | HIGH |
| Unity Jobs + Burst 1.8.27 | 1.8.27 (installed) | CPU-side octree construction, incremental updates, fallback for non-GPU platforms | Already in project. Burst 1.8.27 adds cross-CPU determinism. `IJobParallelFor` for parallel tree updates. Near-native performance for tree construction. | HIGH |
| Unity Collections 2.6.3 | 2.6.3 (installed) | `NativeArray`, `NativeHashMap` for Burst-compatible spatial data | Already in project. Required for Jobs/Burst interop with compute shaders. `NativeArray<T>` is the bridge between managed C# and both Burst and GPU buffers. | HIGH |
| Unity Mathematics 1.3.3 | 1.3.3 (installed) | SIMD-optimized math for spatial calculations | Already in project. `float3`, `float4x4`, `math.distance()` etc. Required by Burst for vectorized operations. | HIGH |

**Architecture: Hybrid CPU-GPU Pipeline**

```
CPU (Burst/Jobs):                    GPU (Compute Shader):
  Octree construction        -->       Octree traversal queries
  Incremental node updates   -->       Batch radius/AABB/frustum queries
  NativeArray<OctreeNode>    -->       StructuredBuffer<OctreeNode>
  ComputeBuffer.SetData()    -->       [numthreads(64,1,1)] kernel
  Result readback (async)    <--       AppendStructuredBuffer<int> results
```

**Key constraint:** GPU readback latency is the primary bottleneck. Use `AsyncGPUReadback.Request()` (built-in since Unity 2018) to avoid stalling the main thread. For real-time spatial queries, keep result buffers small and use `InterlockedAdd` for atomic result counting.

### 3. Message Recording and Replay (Time-Travel Debugging)

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| MmCircularBuffer (existing) | in-project | Bounded message history storage | Already implemented and tested (30+ tests). Reuse for recording buffer. O(1) add, configurable capacity. | HIGH |
| BinaryWriter / MemoryStream | System.IO (.NET Std 2.1) | Message serialization to binary | Zero-dependency. Sufficient for recording MmMessage + MmMetadataBlock to byte streams. Existing MmNetworkBridge already has binary serialization patterns. | HIGH |
| UI Toolkit (Editor) | built-in (6000.3) | Timeline scrubber editor window | Native to Unity 6. Data binding for message details panel. ListView for message stream. | HIGH |
| MmIntrospectionHook (new) | custom | Message capture at MmRelayNode.MmInvoke | Tap into existing MmInvoke hot path. Record source, message, routing decisions, rejections. Conditional compilation (`#if MM_DEBUG`) for zero overhead in production. | HIGH |

**Architecture: Message-Centric, Not Code-Centric**

The existing plan correctly identifies this as the key differentiator from tools like JIVE, rr, and Dbux-PDG. The recording system should:

1. Hook into `MmRelayNode.MmInvoke()` to capture `RecordedMessage` structs
2. Store in `MmCircularBuffer<RecordedMessage>` with configurable capacity (e.g., 10,000 messages)
3. Each record includes: frame number, timestamp, source relay, message copy, routing decisions, reached/rejected responders with rejection reasons
4. Timeline UI in UIToolkit editor window with frame-by-frame scrubbing
5. Query API: "Why didn't MmMethod.X reach responder Y in frame range Z?"

**NOT recommended: Full deterministic replay (rr-style)**. This would require capturing all nondeterministic inputs (physics, random, timing) and is massively complex. The message-centric approach is sufficient for debugging routing issues and is achievable in the 150-hour budget.

### 4. Parallel State Machine Implementations

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| Custom ParallelFSM extending MmRelaySwitchNode | custom | Orthogonal parallel state regions | Must integrate with MercuryMessaging's existing FSM (FiniteStateMachine class, MmRelaySwitchNode). No external library can provide this integration. | HIGH |
| System.Threading.Tasks | .NET Std 2.1 | Parallel region execution | Available in .NET Standard 2.1. `Parallel.ForEach` for concurrent region updates. But see threading caveat below. | MEDIUM |
| ConcurrentQueue<T> | .NET Std 2.1 | Lock-free message passing between regions | Standard .NET concurrent collection. Zero-allocation after initial setup. Suitable for inter-region message synchronization. | HIGH |

**Critical Design Decision: Main-Thread vs Multi-Thread**

The existing plan proposes `Parallel.ForEach` for concurrent region execution. This is **risky in Unity** because:
- Unity's API is not thread-safe (Transform, GameObject, Component access must be on main thread)
- Responder callbacks (`ReceivedMessage`, `ReceivedSetActive`) typically modify Unity objects
- Synchronization back to main thread adds latency and complexity

**Recommendation:** Use a **single-thread cooperative model** (all regions update sequentially in `Update()`) as the default, with an optional **Jobs-based parallel mode** for responders that only process data (no Unity API calls). This matches how UML statecharts typically handle orthogonal regions -- they execute within the same thread.

UnityHFSM's `ParallelStates` class provides a reference implementation pattern (runs multiple states concurrently within a composite state), but does NOT integrate with MercuryMessaging's routing. The custom implementation must bridge Mercury's message routing with parallel FSM semantics.

### 5. Distributed Messaging Patterns

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| FishNet | latest (installed) | Network transport for Replicated/Authoritative modes | Already integrated via `#if FISH_NET`. Provides RPC, SyncVar, and transport abstraction. | HIGH |
| Fluent DSL extensions | custom | `.Replicated()`, `.AuthoritativeOn()`, `.LocalOnly()` | Extends existing fluent API. Pure C# additions to MmFluentMessage chain. Zero new dependencies. | HIGH |
| MmNetworkBridge (existing) | in-project | Network message serialization/routing | Already handles cross-peer message delivery. Extend with distribution mode metadata. | HIGH |
| Attributes (`[MessageDistribution]`) | custom | Declarative distribution mode per message type | .NET Standard 2.1 custom attributes. Source generator could auto-apply. | MEDIUM |

**No external dependencies needed.** The distributed messaging patterns (distribution semantics, change notification, atomic messaging, network topology awareness) are all API-level extensions to the existing fluent DSL and networking infrastructure. The MacIntyre-inspired design is well-specified in `dev/active/distributed-messaging/`.

### 6. Asymmetry Analysis Benchmarking

| Technology | Version | Purpose | Why Recommended | Confidence |
|------------|---------|---------|-----------------|------------|
| Unity Test Framework | 1.6.0 (installed) | Benchmark harness for asymmetric graph tests | Already used for all existing tests. NUnit-based, PlayMode tests for runtime validation. | HIGH |
| System.Diagnostics.Stopwatch | .NET Std 2.1 | High-resolution timing for benchmark metrics | Zero-dependency, nanosecond precision. Standard approach for micro-benchmarks. | HIGH |

**No new technologies needed.** The asymmetry analysis is a benchmark + test suite exercise using existing infrastructure. The Python prototype in `dev/active/asymmetry-analysis/prototype/` validates the design; the C# implementation uses existing test patterns.

---

## Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MemoryPack (Cysharp) | latest (MIT) | High-performance binary serialization | ONLY if existing BinaryWriter serialization proves too slow for time-travel recording at high message volumes. 10x faster than System.Text.Json. .NET Standard 2.1 + Unity 2021.3+ compatible. |
| Disruptor for Unity3D | latest (MIT) | Lock-free ring buffer for inter-thread messaging | ONLY if parallel FSM requires high-throughput cross-thread message passing. Eliminates ConcurrentQueue allocation overhead. |

**Note:** Both are "break glass in case of emergency" options. Start with zero-dependency approaches first.

---

## Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| Unity Profiler | Performance validation for all new systems | Built-in. Use Deep Profile mode for allocation tracking. Critical for validating <5% overhead targets for recording/spatial indexing. |
| Unity Test Runner (PlayMode) | Automated testing of all new capabilities | Already configured. All tests must be fully programmatic (no scene files). |
| ParrelSync | Multi-editor testing for distributed messaging | Already installed. Essential for testing Replicated/Authoritative distribution modes across peers. |
| Frame Debugger | Compute shader validation for spatial indexing | Built-in. Verify compute dispatch, buffer contents, and shader execution. |

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Graph Editor | GraphViewBase (MIT) | Unity Graph Toolkit (GTK) 0.4.0-exp.2 | GTK is experimental (0.x), GraphViewBase already imported. Revisit when project moves to Unity 6.4+ where GTK becomes built-in. |
| Graph Editor | GraphViewBase | NodeGraphProcessor (alelievr) | NodeGraphProcessor is mature but built on deprecated Experimental.GraphView API, not UIToolkit. GraphViewBase is UIToolkit-native. |
| Spatial Index | Compute Shaders + Jobs/Burst | Unity DOTS Physics (spatial queries) | DOTS Physics is designed for collision, not message routing. Heavy dependency. ECS integration would break MercuryMessaging's MonoBehaviour architecture. |
| Spatial Index | Adaptive Octree | Spatial Hashing (GPU grid) | Octree handles non-uniform distributions better than uniform grids. Most VR/XR scenes have clustered objects, not uniform distributions. |
| Parallel FSM | Custom on MmRelaySwitchNode | UnityHFSM (Inspiaaa) | UnityHFSM has `ParallelStates` but no MercuryMessaging integration. Would require wrapping UnityHFSM inside Mercury's routing, adding coupling and complexity. Custom approach maintains architectural consistency. |
| Parallel FSM | Cooperative single-thread | Full multi-threading (System.Threading) | Unity API is not thread-safe. Multi-threaded responder callbacks would require main-thread synchronization for any Transform/GameObject access. Complexity not justified for UIST 2026 scope. |
| Message Recording | MmCircularBuffer + BinaryWriter | MemoryPack | MemoryPack is faster but adds a dependency. BinaryWriter is sufficient for recording overhead target (<5% frame time). Only escalate if profiling shows serialization is the bottleneck. |
| Message Recording | Message-centric replay | Full deterministic replay (rr-style) | Full deterministic replay requires capturing physics, random seeds, and timing -- massively complex. Message-centric approach answers the key research question ("why didn't this message reach X?") without determinism requirements. |
| 3D Drawing | GL.Begin/GL.End | ALINE (paid plugin) | ALINE is already in the project (study reference) but is a paid dependency. GL API is zero-dependency and sufficient for connection line drawing between relay nodes. |
| Binary Serialization | BinaryWriter/BinaryReader | BinaryFormatter | BinaryFormatter is being removed from .NET (deprecated since .NET 5, errors in .NET 7+). Never use it. |

---

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Unity Experimental.GraphView | Deprecated since Unity 2020. No longer maintained. NodeGraphProcessor and Shader Graph still use it internally but new projects should not. | GraphViewBase (UIToolkit-based) or Unity Graph Toolkit (when stable) |
| BinaryFormatter | Deprecated in .NET 5+, build errors in .NET 7+. Security risk (deserialization attacks). Being removed from Unity. | BinaryWriter/BinaryReader or MemoryPack |
| ECS/Entities for spatial indexing | Forces architectural shift from MonoBehaviour to ECS. MercuryMessaging is MonoBehaviour-based. Mixing architectures creates maintenance burden. | Compute Shaders + Jobs/Burst (works with MonoBehaviours) |
| System.Threading.Thread (raw threads) | No Unity API access from worker threads. Difficult to debug. Synchronization overhead. | Unity Jobs System (for data processing) or main-thread cooperative model (for state machines) |
| DOTS Physics for spatial queries | Over-engineered for message routing. Pulls in full ECS physics stack. | Custom octree with compute shader queries |
| Photon Bolt / Mirror (networking) | Project already has FishNet and Fusion 2. Adding a third networking layer adds confusion. | FishNet (`#if FISH_NET`) or Fusion 2 (`#if FUSION_WEAVER`) |

---

## Stack Patterns by Capability

**Visual Composer:**
- GraphViewBase for 2D graph editor window
- GL.Begin/GL.End for 3D scene visualization
- IMGUI for runtime debugging overlay
- UIToolkit for editor panels (inspector integration, property drawers)
- SerializedObject API for undo/redo and scene persistence

**GPU Spatial Indexing:**
- Compute shaders (HLSL) for GPU-side queries
- Jobs + Burst for CPU-side octree construction/updates
- NativeArray + ComputeBuffer for CPU-GPU data transfer
- AsyncGPUReadback for non-blocking result retrieval
- MmMetadataBlock extension for spatial filter parameters

**Time-Travel Debugging:**
- MmCircularBuffer for bounded recording storage
- BinaryWriter for message serialization
- UIToolkit for timeline editor window
- MmIntrospectionHook for MmInvoke capture
- Conditional compilation for zero-overhead production builds

**Parallel FSMs:**
- Custom ParallelFSM extending MmRelaySwitchNode
- Cooperative single-thread model (default)
- ConcurrentQueue for optional inter-region messaging
- Mercury message bus for cross-region synchronization
- Priority-based conflict resolution

**Distributed Messaging:**
- Fluent DSL extensions (.Replicated(), .AuthoritativeOn(), .LocalOnly())
- FishNet/Fusion 2 transport (existing)
- Custom attributes for declarative distribution mode
- MmNetworkBridge extension for distribution semantics

---

## Version Compatibility Matrix

| Package | Version | Compatible With | Notes |
|---------|---------|-----------------|-------|
| Unity Editor | 6000.3.7f1 | All packages listed | Current project version. Unity 6.3 LTS. |
| Burst | 1.8.27 | Unity 6000.3.x, Collections 2.6.3 | Cross-CPU determinism for multiplayer. LLVM 14.0.6. |
| Collections | 2.6.3 | Unity 6000.3.x, Burst 1.8.27 | NativeArray, NativeHashMap for Jobs/Burst interop. |
| Mathematics | 1.3.3 | Unity 6000.3.x, Burst 1.8.27 | SIMD math for spatial calculations. |
| GraphViewBase | latest | Unity 6000.3.x | MIT. UIToolkit-based. Already in manifest. |
| Unity Graph Toolkit | 0.4.0-exp.2 | Unity 6000.2+ (NOT 6.4+) | Experimental. Do NOT install on 6.4+ (becomes built-in module). |
| FishNet | latest | Unity 6000.3.x | Already in manifest. `#if FISH_NET` conditional. |
| Test Framework | 1.6.0 | Unity 6000.3.x | Already in manifest. NUnit-based. |

---

## Alignment with Existing dev/active/ Research

### Visual Composer (`dev/active/visual-composer/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| 2D Graph Editor | GraphViewBase (MIT) | GraphViewBase (MIT) | **AGREE** - Correct choice for UIST 2026 timeline. |
| 3D Scene Viz | Custom GL API | GL.Begin/GL.End + RenderPipelineManager | **AGREE** - Zero-dependency, URP/HDRP compatible. |
| Runtime Debugger | Custom IMGUI | Custom IMGUI | **AGREE** - Only viable option for runtime debug overlays. |
| GTK alternative | Not discussed | GTK 0.4.0-exp.2 available, becomes module in 6.4 | **NEW INFO** - Plan should note migration path to GTK for post-UIST 2026 when Unity upgrades to 6.4+. |
| UIToolkit usage | Implied | Explicitly recommended for editor panels | **ADDS** - Use UIToolkit (not IMGUI) for the editor window chrome, inspector integration, and property drawers. Only use IMGUI for runtime overlay. |

### Spatial Indexing (`dev/active/spatial-indexing/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| GPU Compute Shaders | Yes | Yes | **AGREE** - 2025 IEEE paper validates 26x speedup. |
| Adaptive Octree | Yes | Yes | **AGREE** - Better than grids for non-uniform VR scenes. |
| Shader Model 5.0 | Yes | Yes | **AGREE** - Wide hardware support. |
| CPU fallback via Jobs/Burst | Not explicitly stated | Jobs/Burst for octree construction | **ADDS** - Plan should explicitly include CPU-side tree management with Jobs/Burst rather than managed C#. |
| AsyncGPUReadback | Not mentioned | Critical for non-blocking queries | **ADDS** - Must use AsyncGPUReadback to avoid main-thread stall. This is a significant architectural detail missing from existing plan. |
| Struct stride alignment | Not mentioned | Must be multiple of 16 | **ADDS** - Cross-platform GPU compatibility requirement. Existing plan's `OctreeNode` struct needs padding review. |
| NativeArray bridge | Not mentioned | NativeArray is the CPU-GPU bridge | **ADDS** - Burst-compiled code produces NativeArray, which feeds directly into ComputeBuffer.SetData(). This pipeline should be documented. |

### Parallel FSM (`dev/active/parallel-fsm/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| Orthogonal regions | Yes | Yes | **AGREE** - Core concept is sound. |
| Message-based sync | Yes | Yes | **AGREE** - Aligns with Mercury's architecture. |
| Parallel.ForEach | Yes (for region execution) | **DISAGREE** - Risky in Unity | **DISAGREE** - Unity API is not thread-safe. Responder callbacks typically access Transform/GameObject. Recommend cooperative single-thread as default with optional Jobs-based parallel for data-only responders. |
| Lock-free design | Yes | ConcurrentQueue + cooperative model | **PARTIALLY AGREE** - Lock-free data structures useful for optional parallel mode, but default should be single-thread cooperative. |
| UnityHFSM reference | Not mentioned | UnityHFSM's ParallelStates as pattern reference | **ADDS** - UnityHFSM provides a proven pattern for parallel states within HSM. Study its `ParallelStates` class for API design inspiration, but build custom integration with MmRelaySwitchNode. |
| Performance target <1ms | Yes | Achievable with cooperative model | **AGREE** - Sequential region updates at 10 regions should easily stay under 1ms without threading overhead. |

### Time-Travel Debugging (`dev/active/time-travel-debugging/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| Message-centric approach | Yes | Yes | **AGREE** - Correct differentiation from code-centric debuggers (JIVE, rr). |
| MmInvoke hook | Yes | Yes | **AGREE** - Hook into existing MmRelayNode.MmInvoke is the right insertion point. |
| Circular buffer storage | Yes | Reuse MmCircularBuffer | **AGREE** - Existing tested implementation (30+ tests). |
| Timeline UI | Yes (Unity Editor UI Toolkit) | UIToolkit | **AGREE** - Consistent with Visual Composer tooling. |
| Query API | Yes | Yes | **AGREE** - "Why didn't message reach X?" is the key UX. |
| Recording overhead <5% | Yes | Achievable with conditional compilation | **AGREE** - Use `#if MM_DEBUG` or `[Conditional("MM_DEBUG")]` for zero overhead in production. |
| Binary serialization | Not specified | BinaryWriter (zero-dependency) | **ADDS** - Plan should specify serialization approach. BinaryWriter sufficient. Escalate to MemoryPack only if profiling shows bottleneck. |
| NOT full deterministic replay | Correctly scoped | Confirmed correct scoping | **AGREE** - Full deterministic replay (rr-style) would be 10x the effort for minimal research value. Message-centric is the right scope. |

### Distributed Messaging (`dev/active/distributed-messaging/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| Three distribution modes | Replicated, Authoritative, LocalOnly | Same | **AGREE** - Well-specified, implementable with existing fluent API infrastructure. |
| Fluent API extensions | Yes | Yes | **AGREE** - Pure C# additions, zero dependencies. |
| Change notification | Pre/post callbacks with veto | Same | **AGREE** - Pattern matches MacIntyre's analysis. |
| Atomic messaging | Transaction API | Same | **AGREE** - Two-phase commit for coordinated delivery. |
| Network topology awareness | Latency-aware routing | Same | **AGREE** - FishNet provides RTT measurement capability. |
| No new dependencies | Yes | Confirmed | **AGREE** - All patterns implementable with existing infrastructure. |

### Asymmetry Analysis (`dev/active/asymmetry-analysis/`)

| Aspect | Existing Plan | This Research | Assessment |
|--------|---------------|---------------|------------|
| Unity Test Framework benchmark | Yes | Yes | **AGREE** - Standard approach for C# benchmarks in Unity. |
| Three asymmetry scenarios | Yes | Yes | **AGREE** - Missing nodes, extra nodes, mixed divergence. |
| Python prototype validation | Yes | N/A (implementation detail) | **AGREE** - Prototype exists, C# port is straightforward. |
| No new technologies needed | Yes | Confirmed | **AGREE** - Pure test/benchmark code using existing framework. |

---

## Risk Assessment

| Technology | Risk Level | Mitigation |
|------------|-----------|------------|
| GraphViewBase (MIT) | LOW | Already imported, MIT licensed, UIToolkit-based. Risk: small community, 4 contributors. Mitigation: code is readable, project can fork if abandoned. |
| Unity Compute Shaders | LOW | Stable API since Unity 5.x. Well-documented. Only risk is cross-platform shader compilation differences. Mitigation: test on target platforms early. |
| Jobs/Burst 1.8.27 | LOW | Verified package, already in project. Cross-CPU determinism validated. |
| Unity Graph Toolkit (if adopted later) | MEDIUM | Experimental 0.x package. API may change. Not for production use. Mitigation: defer to post-Unity-6.4 when it becomes a built-in module. |
| Parallel FSM with threading | HIGH | Unity API not thread-safe. Race conditions possible. Mitigation: default to cooperative single-thread model; only enable parallel for data-only responders with explicit opt-in. |
| MemoryPack (if needed) | LOW | MIT, well-maintained (630 commits), .NET Std 2.1 compatible. Risk: adds dependency. Mitigation: only adopt if BinaryWriter proves insufficient. |
| AsyncGPUReadback | LOW | Built-in since Unity 2018. Stable API. Only risk: 1-2 frame latency for results. Mitigation: design spatial queries to tolerate frame-delayed results. |

---

## Summary of Key Decisions

1. **Graph Editor:** Stay with GraphViewBase (already imported, MIT, UIToolkit-native). Plan migration to GTK when project moves to Unity 6.4+.

2. **GPU Spatial Indexing:** Compute shaders for queries, Jobs/Burst for CPU-side construction. All packages already installed. AsyncGPUReadback for non-blocking results.

3. **Time-Travel Debugging:** Message-centric (not code-centric). Reuse MmCircularBuffer. BinaryWriter serialization. UIToolkit timeline UI. Conditional compilation for zero production overhead.

4. **Parallel FSMs:** Custom extension of MmRelaySwitchNode. Single-thread cooperative model by default. Study UnityHFSM's ParallelStates for API patterns. Avoid raw System.Threading.

5. **Distributed Messaging:** Pure API-level extensions to existing fluent DSL and networking. No new dependencies.

6. **Asymmetry Analysis:** Pure benchmark code using Unity Test Framework. No new technologies.

**Total new dependencies recommended: ZERO.** All capabilities can be built with existing project dependencies and Unity built-in APIs.

---

## Sources

- [Unity Graph Toolkit (GTK) Release Thread](https://discussions.unity.com/t/unity-s-graph-toolkit-experimental-available-today-in-unity-6-2/1664909) -- GTK experimental release, features, limitations (MEDIUM confidence)
- [Unity Graph Toolkit Docs](https://docs.unity3d.com/Packages/com.unity.graphtoolkit@0.1/manual/index.html) -- Official GTK documentation (HIGH confidence)
- [Unity GTK Q2 2025 Update](https://discussions.unity.com/t/unity-graph-toolkit-update-q2-2025/1656349) -- GTK development status (MEDIUM confidence)
- [GraphViewBase GitHub](https://github.com/Gentlymad-Studios/GraphViewBase) -- MIT graph framework already in project (HIGH confidence)
- [Fast Collision Detection with Octree-Based Parallel Processing (IEEE 2025)](https://www.mdpi.com/2673-4591/89/1/37) -- 26x GPU speedup for octree in Unity (MEDIUM confidence)
- [Unity Compute Shader Manual](https://docs.unity3d.com/Manual/class-ComputeShader.html) -- Official compute shader docs (HIGH confidence)
- [Unity ComputeBuffer API](https://docs.unity3d.com/ScriptReference/ComputeBuffer.html) -- Structured buffer API (HIGH confidence)
- [Burst 1.8 Changelog](https://docs.unity3d.com/Packages/com.unity.burst@1.8/changelog/CHANGELOG.html) -- Cross-CPU determinism, LLVM 14 (HIGH confidence)
- [Unity 6 Job System Overview](https://docs.unity3d.com/6000.3/Documentation/Manual/job-system-overview.html) -- Official Jobs docs (HIGH confidence)
- [UnityHFSM GitHub](https://github.com/Inspiaaa/UnityHFSM) -- ParallelStates reference implementation (HIGH confidence)
- [UnityHFSM Feature Overview](https://github.com/Inspiaaa/UnityHFSM/wiki/Feature-Overview) -- ParallelStates class docs (HIGH confidence)
- [Deterministic Record-and-Replay (ACM, May 2025)](https://queue.acm.org/detail.cfm?id=3688088) -- Academic time-travel debugging survey (HIGH confidence)
- [MemoryPack GitHub](https://github.com/Cysharp/MemoryPack) -- High-performance binary serializer, MIT (HIGH confidence)
- [Unity UI Toolkit Manual](https://docs.unity3d.com/Manual/UIElements.html) -- Official UIToolkit docs (HIGH confidence)
- [Unity Gizmos and Handles (6000.2)](https://docs.unity3d.com/6000.2/Documentation/Manual/gizmos-and-handles.html) -- GL/Gizmo drawing API (HIGH confidence)
- [Unity 2026 Roadmap (Unite 2025)](https://discussions.unity.com/t/the-unity-engine-roadmap-unite-2025/1696495) -- ECS core package in 6.4, CoreCLR migration (MEDIUM confidence)
- [ECS December 2025 Status](https://discussions.unity.com/t/ecs-development-status-december-2025/1699284) -- Latest DOTS/ECS status (MEDIUM confidence)

---

*Stack research for: MercuryMessaging UIST 2026 Capabilities*
*Researched: 2026-02-11*
