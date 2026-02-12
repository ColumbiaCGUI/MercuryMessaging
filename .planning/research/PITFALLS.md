# Pitfalls Research

**Domain:** Hierarchical message routing framework (Unity) for UIST 2026 academic paper
**Researched:** 2026-02-11
**Confidence:** HIGH (based on project codebase analysis, existing dev docs, UIST/CHI author guides, Unity ecosystem evidence)

---

## How to Read This Document

Each pitfall includes:
- **Severity**: Critical (project-killing) / High (major rework) / Medium (delays) / Low (annoyance)
- **Status**: ALREADY KNOWN (team has documented awareness) or NEW (not yet in project docs)
- **Phase**: Which of the 7 planned phases should address it
- **Warning signs**: How to detect early
- **Prevention**: How to avoid

The 7 planned phases referenced throughout:
1. Visual Composer (~316h)
2. Spatial Indexing (~360h)
3. Parallel FSMs (~280h)
4. Time-Travel Debugging (~150h)
5. Static Analysis / Asymmetry Analysis (~280h + 40h)
6. Distributed Messaging (~80h)
7. Tutorial Validation (~12h)

---

## Critical Pitfalls

### Pitfall 1: The 7-Feature Paper That Becomes a 1-Feature Paper

**Severity:** Critical
**Status:** NEW
**Phase:** All phases / paper writing

**What goes wrong:**
The project plans 7 features totaling ~1,234 hours. UIST papers are evaluated on depth, not breadth. The UIST Author Guide explicitly warns: "Beware of trying to write a paper with too large a scope. Often the best papers are very specific in what they provide so as to deliver a concrete solution." A paper that tries to present 7 features will be criticized for shallow treatment of each. Reviewers will ask: "Which of these is the actual contribution?"

**Why it happens:**
Academic teams build features incrementally for a framework, then try to package everything into one paper. This violates the principle that "a conference paper can only present a few concise ideas well" (UIST Author Guide). The ~1,234 hours of work cannot be meaningfully evaluated in 10 pages.

**How to avoid:**
- Decide NOW which 2-3 features are the paper's core contributions. The visual composer + time-travel debugging + static analysis form a natural "developer experience" story. Spatial indexing + parallel FSMs form a separate "performance/scalability" story.
- Frame remaining features as "supporting infrastructure" or defer to a second paper.
- Structure the paper around a research question (e.g., "Can hierarchical message routing support live visual introspection?") not a feature list.
- Apply Olsen's evaluation heuristics (UIST 2007): demonstrate expressive leverage, expressive match, and flexibility -- not feature count.

**Warning signs:**
- Paper outline has more than 3 "contribution" bullets
- Each feature gets less than 1.5 pages of treatment
- Evaluation section tries to evaluate all 7 features with one user study
- Related work section is a laundry list rather than a focused positioning

**Phase to address:** Pre-Phase 1 decision. Must be resolved before implementation begins to avoid building features that will not appear in the paper.

---

### Pitfall 2: GraphView/GraphViewBase Instability Across Unity Versions

**Severity:** Critical
**Status:** PARTIALLY KNOWN (visual-composer README mentions GraphViewBase choice but does not discuss version instability)
**Phase:** Phase 1 (Visual Composer)

**What goes wrong:**
Unity's GraphView API has remained in the `UnityEditor.Experimental` namespace for years. It has documented breaking changes between Unity 2021, 2022, and 2023+, including shader errors (`Hidden/GraphView/GraphViewUIE` invalid subscript), `FrameAll()` not working, forced dark theme on editor windows, and complete feature breakage when porting between versions. The project targets Unity 2021.3+ but the current editor is Unity 6000.3.7f1 (Unity 6.3). GraphViewBase (the MIT wrapper) abstracts some of this, but cannot protect against underlying Unity API changes.

**Why it happens:**
Unity has not committed to stabilizing GraphView. They are developing GraphToolsFoundation as a replacement, but it is also not stable. Building a major paper contribution on an experimental API creates fragile infrastructure.

**How to avoid:**
- Pin the exact Unity version for paper submission and artifact evaluation (currently 6000.3.7f1 -- lock this)
- Create an integration test that opens the Visual Composer window, adds nodes, connects edges, and verifies basic functionality. Run this on every Unity version update.
- Maintain a thin abstraction layer between the Visual Composer and GraphViewBase so the graph rendering backend can be swapped if GraphViewBase breaks.
- Document the exact GraphViewBase version in the paper's artifact description.
- Consider whether a pure IMGUI or UI Toolkit fallback is feasible for the graph view.

**Warning signs:**
- GraphViewBase throws exceptions after a Unity update
- Node/edge rendering breaks silently (elements appear but are not interactive)
- Assembly reload clears all graph state (see Pitfall 5)
- GraphView forces dark theme, breaking the editor window's visual consistency

**Phase to address:** Phase 1, Week 1 -- verify GraphViewBase works on the locked Unity version before investing 80+ hours.

---

### Pitfall 3: Demo-Ware Visual Composer Without Generalizable Evaluation

**Severity:** Critical
**Status:** PARTIALLY KNOWN (visual-composer README includes user study design but the 50% improvement hypothesis is unvalidated)
**Phase:** Phase 1 (Visual Composer evaluation)

**What goes wrong:**
The visual composer looks impressive in a demo video but the user study fails to show statistically significant improvement. Or worse: the study shows improvement but reviewers argue the comparison is unfair (Inspector-only baseline is too weak) or the tasks are too artificial. Olsen (UIST 2007) calls this the "usability trap" -- systems research cannot be evaluated by simple usability testing because realistic tasks are too complex for controlled studies.

**Why it happens:**
The proposed user study (N=20, fix 5 broken routing paths, Inspector vs Visual Composer) has several risks:
- N=20 may be insufficient for the expected effect size with high variance in developer skill
- "Broken routing paths" are an artificial task that may not reflect real-world debugging
- Inspector-only baseline may not reflect how developers actually debug (they use logging, breakpoints, etc.)
- The 50% improvement hypothesis is aspirational with no pilot data

**How to avoid:**
- Run a pilot study with N=3-5 developers before committing to the full study design. Adjust tasks and measures based on pilot results.
- Use Ledo et al.'s (CHI 2018) evaluation strategies: combine demonstration (diverse examples), technical benchmarks (100+ nodes at 60fps), AND a usage study.
- Compare against a realistic baseline (Inspector + MmLogger + breakpoints), not Inspector-only.
- Include qualitative data (think-aloud, interviews) alongside quantitative metrics.
- Pre-register the study protocol to prevent post-hoc hypothesis fishing.
- Have specific "generalizability" demonstrations: show the visual composer working on 3+ structurally different Mercury hierarchies (FSM, networked, VR).

**Warning signs:**
- No pilot study results by the time the visual composer is feature-complete
- Study tasks can be solved faster with logging than with the visual composer
- Participants struggle to understand the visual composer in the training phase (>15 min)
- Effect sizes are smaller than expected, requiring N>40 for significance

**Phase to address:** Phase 1, final weeks. Design study tasks early (week 2-3), pilot test mid-phase.

---

### Pitfall 4: GPU Compute Shader Platform Exclusion Undermines Spatial Indexing Claims

**Severity:** Critical
**Status:** NEW (spatial-indexing README lists GPU as core feature but does not discuss platform limitations)
**Phase:** Phase 2 (Spatial Indexing)

**What goes wrong:**
The spatial indexing system is designed around GPU-accelerated compute shaders. However:
- WebGL 2.0 does not support compute shaders at all
- WebGPU support is experimental and not available in all browsers
- Many Android devices have partial or no compute shader support
- The project's zero-dependency policy conflicts with requiring Shader Model 5.0 and 4GB+ GPU memory

This means the "GPU-accelerated spatial routing" contribution is limited to desktop platforms with modern GPUs, severely restricting the generalizability claim for a UIST paper.

**Why it happens:**
GPU compute performance numbers look impressive in benchmarks, but the academic systems community values broad applicability. A spatial indexing system that only works on desktop with recent GPUs is a much weaker contribution than one that degrades gracefully across platforms.

**How to avoid:**
- Implement CPU-only octree FIRST as the primary contribution. This works everywhere.
- GPU acceleration is an optional optimization, not the core contribution.
- Frame the paper contribution as "spatial indexing integrated with hierarchical message routing" (the novelty), not "GPU-accelerated queries" (the optimization).
- Include CPU vs GPU benchmarks showing the CPU path is still useful (<5ms for 10K objects).
- Explicitly document platform fallback behavior in the paper.

**Warning signs:**
- All benchmarks are GPU-only with no CPU baseline
- Testing only happens on one development machine with a powerful GPU
- The CPU fallback path is an afterthought with 10x worse performance
- No testing on Quest/mobile/WebGL targets

**Phase to address:** Phase 2, architecture decision in Week 1. Build CPU-first, GPU-optional.

---

### Pitfall 5: Editor State Loss on Assembly Reload Destroys Visual Composer/Time-Travel Data

**Severity:** Critical
**Status:** NEW
**Phase:** Phase 1 (Visual Composer), Phase 4 (Time-Travel Debugging)

**What goes wrong:**
When Unity recompiles scripts (entering/exiting Play Mode, or editing code during Play Mode), it performs an assembly reload. Any non-serialized state is destroyed. This includes:
- Dictionaries, HashSets, and other non-serializable collections
- Private fields without `[SerializeField]`
- Event subscriptions and delegates
- The entire recorded message timeline in time-travel debugging
- Graph layout state in the visual composer if not serialized

For the visual composer, this means the graph view resets every time the developer enters Play Mode. For time-travel debugging, this means the entire recorded message history is lost when exiting Play Mode.

**Why it happens:**
Unity's serialization system only preserves data types it knows about. Complex editor tool state (graph layouts, recorded message histories, event subscriptions) typically uses types that Unity cannot serialize. Developers new to Unity editor tooling are often surprised by this behavior.

**How to avoid:**
- Use `ScriptableObject` for all persistent Visual Composer state (graph layout, node positions, edge connections).
- For time-travel debugging, write recorded messages to a file (binary or JSON) during recording, not just in-memory. The timeline scrubber reads from the file.
- Implement `ISerializationCallbackReceiver` on any custom editor state that must survive assembly reload.
- Test the assembly reload cycle explicitly: enter Play Mode, make graph changes, exit Play Mode, verify state preserved.
- Consider disabling assembly reload during Play Mode for the debugging workflow.

**Warning signs:**
- Visual composer graph resets when entering/exiting Play Mode
- Time-travel timeline is empty after stopping playback
- `NullReferenceException` in editor window after script recompilation
- Users report losing work when switching between Edit and Play modes

**Phase to address:** Phase 1 (Week 2 -- serialization architecture) and Phase 4 (Week 1 -- recording storage design).

---

### Pitfall 6: Parallel FSM Race Conditions in Unity's Single-Threaded Component Model

**Severity:** Critical
**Status:** PARTIALLY KNOWN (parallel-fsm README mentions "lock-free" and "no shared memory" but parallel-dispatch README shows `Parallel.ForEach` and thread pool usage that directly conflict with Unity's threading model)
**Phase:** Phase 3 (Parallel FSMs)

**What goes wrong:**
Unity's `MonoBehaviour` lifecycle methods (Awake, Start, Update, OnDestroy) and the entire component system run on the main thread. Transform access, `GetComponent<T>()`, `SetActive()`, and many other Unity APIs are NOT thread-safe and will throw exceptions or corrupt data when called from worker threads. The parallel FSM design shows `Parallel.ForEach` over FSM regions, but if any region's state transition calls Unity APIs (which it will -- e.g., `gameObject.SetActive(false)` in `ReceivedSetActive`), the system will crash or produce undefined behavior.

**Why it happens:**
The parallel dispatch and parallel FSM designs borrow patterns from general concurrent programming (work-stealing queues, lock-free data structures, `Parallel.ForEach`) without accounting for Unity's fundamental constraint that most of its API is main-thread-only. This is a common mistake when applying concurrent programming patterns to game engines.

**How to avoid:**
- Separate "parallel state evaluation" (can be threaded) from "state transition execution" (must be main thread).
- Use Unity's Job System + Burst Compiler for the evaluation phase, which provides safe parallelism within Unity's constraints.
- All Unity API calls (SetActive, Transform modifications, component access) must be queued back to the main thread via a synchronization point.
- Define a clear "thread boundary contract" in the architecture: what can run off main thread (pure logic, message routing decisions) vs. what cannot (component access, scene graph mutations).
- Consider whether true thread-level parallelism is needed, or if coroutine-based "logical parallelism" (multiple FSMs evaluated sequentially but appearing concurrent) is sufficient for the paper's claims.

**Warning signs:**
- `UnityException: ... can only be called from the main thread` errors in console
- Intermittent test failures that cannot be reproduced consistently
- Race conditions that manifest only under high load (>100 messages/sec)
- Deadlocks during FSM cross-region communication

**Phase to address:** Phase 3, architecture design in Week 1. Choose between true thread parallelism (complex, risky) and logical parallelism (simpler, still publishable).

---

## High Pitfalls

### Pitfall 7: Time-Travel Debugging Recording Overhead Exceeds Budget

**Severity:** High
**Status:** PARTIALLY KNOWN (time-travel README targets <5% overhead, but the recording design captures full routing decisions, rejection reasons, and responder lists per message -- this is far more data than simple message logging)
**Phase:** Phase 4 (Time-Travel Debugging)

**What goes wrong:**
The `RecordedMessage` struct captures: frame number, timestamp, source relay node, full message, List<RoutingDecision>, List<MmResponder> reached, List<RejectedResponder> with rejection strings. For a system sending 500-1000 messages/second, this creates:
- String allocations for rejection reasons (GC pressure)
- List allocations for reached/rejected responders
- Reference retention preventing garbage collection of old messages
- Potential 50-100MB of recording data per minute

The existing CircularBuffer (QW-4) bounds message history at 100 items. The time-travel system needs much larger buffers (10,000+ messages) with much richer data per message. The <5% overhead target may be unrealistic with the proposed recording granularity.

**Why it happens:**
Time-travel debugging requires rich context (why was a message rejected?) but rich context is expensive. The prototype design captures everything without considering the cost of per-message string allocations and reference retention.

**How to avoid:**
- Implement tiered recording: Level 1 (method + source + timestamp only, <1% overhead), Level 2 (+ reached responders, <3% overhead), Level 3 (+ rejection reasons with strings, <10% overhead). Default to Level 1 in production, Level 3 only when actively debugging.
- Use enum-based rejection codes instead of strings (e.g., `RejectionReason.TagMismatch`) to avoid string allocations.
- Pool RecordedMessage structs and Lists to avoid per-message heap allocations.
- Measure actual overhead with the performance test infrastructure (MessageGenerator at 500 msg/sec) before committing to the recording design.
- Use the existing CircularBuffer pattern for bounded memory, but with configurable size (10K messages = ~40MB at Level 3).

**Warning signs:**
- Frame time increases >2ms when recording is enabled
- GC.Collect() spikes appear in Unity Profiler during recording
- Memory grows unbounded during long recording sessions
- Recording must be disabled during performance testing

**Phase to address:** Phase 4, Week 1 -- prototype Level 1 recording and measure overhead before designing the full system.

---

### Pitfall 8: Insufficient Comparison with Prior Art Leads to Desk Rejection

**Severity:** High
**Status:** PARTIALLY KNOWN (each feature's README has a literature analysis, but the comparisons are mostly "X is for a different domain" rather than deep technical differentiation)
**Phase:** Paper writing (cross-cutting)

**What goes wrong:**
UIST 2024/2025 desk rejection criteria include: "Papers that lack adequate methodological details or that fail to engage with relevant prior research may be desk rejected." The current related work in the project READMEs follows a pattern of listing 5-6 papers with brief "limitation" and "Mercury differentiation" columns. This is a literature survey, not a related work argument.

Specific risks:
- The visual composer comparison claims "FIRST bi-directional visual authoring" but does not engage with Unity's Visual Scripting (Bolt), Shader Graph, or VFX Graph which are all bi-directional graph editors in Unity.
- The parallel FSM comparison does not engage with UE5's Gameplay Ability System or HFSM2 (a popular Unity state machine with parallel states).
- The spatial indexing comparison does not engage with Unity's own spatial query systems (Physics.OverlapSphere, etc.) or ECS spatial queries.

**Why it happens:**
The literature analysis focuses on academic papers and misses industry tools that reviewers will know. UIST reviewers include industry practitioners from Unity, Meta, Google, etc. who will immediately ask "how does this compare to [tool I use daily]?"

**How to avoid:**
- For each contribution, identify the 2-3 closest tools (academic AND industrial) and provide a detailed technical comparison, not just a claim of "first."
- Test the comparison claims: actually build the baseline comparison. For the visual composer, create the same routing network using Unity's Inspector and time it. For spatial indexing, compare against Physics.OverlapSphere.
- Frame novelty as "first to combine X with Y" rather than "first X" -- the combination is defensible, individual claims are risky.
- Have someone outside the team read the related work section and challenge every "first" claim.

**Warning signs:**
- Related work section is shorter than 1.5 pages
- No industrial tools appear in the comparison
- Every comparison ends with "our approach is better because it is for game engines"
- Reviewers' first question in rebuttal is "have you looked at [well-known tool]?"

**Phase to address:** Pre-implementation research. Update literature analysis before building each feature.

---

### Pitfall 9: Integration Regression -- 7 New Features Break 60+ Existing Tests

**Severity:** High
**Status:** ALREADY KNOWN (CONTRIBUTING.md mandates running tests, but no CI/CD pipeline exists to enforce this)
**Phase:** All phases

**What goes wrong:**
The project has 60+ test files covering core routing, DSL, FSM, network, serialization, and more. Each new feature modifies core files (especially `MmRelayNode.cs`, the "MOST IMPORTANT CLASS"). Without continuous integration, a change in Phase 2 (spatial indexing adding a new routing path to MmRelayNode) could break Phase 1 (visual composer introspection hooks that assumed a specific MmInvoke flow).

The risk is compounded because:
- MmRelayNode.cs is already 1247 lines and is the central routing hub
- 7 features all need to hook into MmRelayNode's message dispatch
- No CI/CD pipeline runs tests automatically
- Tests are PlayMode tests requiring Unity Editor

**Why it happens:**
Research projects prioritize feature development over infrastructure. Each feature developer tests their own changes but may not run the full test suite. Cross-feature interactions are discovered late.

**How to avoid:**
- Set up GitHub Actions with `game-ci/unity-test-runner` to run PlayMode tests on every push. This is a one-time setup cost (~4 hours) that prevents weeks of debugging.
- Define a "core contract" for MmRelayNode: the specific extension points where new features plug in (introspection hook, spatial filter, parallel dispatch). Each extension point has an interface and integration test.
- Maintain a "compatibility matrix" test that sends messages through a hierarchy using all routing modes (standard, advanced, path-based, spatial) and verifies correct delivery.
- Run the full test suite before AND after each phase merge.

**Warning signs:**
- "I'll run the tests later" becomes the norm
- Test failures are discovered days after the breaking change
- MmRelayNode.cs exceeds 2000 lines as features add inline logic
- Two features conflict on how they modify MmMetadataBlock

**Phase to address:** Pre-Phase 1. Set up CI/CD before starting feature work.

---

### Pitfall 10: Bloom Filter False Positives in Static Analysis Create Silent Safety Failures

**Severity:** High
**Status:** PARTIALLY KNOWN (static-analysis README mentions <0.001% false positive rate but does not address what happens on false positives)
**Phase:** Phase 5 (Static Analysis)

**What goes wrong:**
The Bloom filter-based runtime cycle detection uses a 128-bit filter. With standard Bloom filter math, a 128-bit filter can track ~10 distinct node IDs before the false positive rate exceeds 1%. A hierarchy with 50+ nodes (the Medium test scale) will have a false positive rate of ~90%+, meaning the Bloom filter will incorrectly claim cycles exist in almost every deep message propagation.

The 128-bit design in the README is far too small for real hierarchies:
```csharp
ulong hash1, hash2; // 128-bit filter -- only 2 hash functions
```

With 2 hash functions and 128 bits, this is essentially a pair of 64-bit bitmasks. Any hierarchy with >20 nodes will saturate the filter.

**Why it happens:**
The Bloom filter sizing was done without calculating the actual false positive rate for realistic hierarchy sizes. The <0.001% claim requires a much larger filter (~2048 bits for 100 nodes with 10 hash functions).

**How to avoid:**
- Size the Bloom filter correctly: use `m = -n * ln(p) / (ln(2))^2` where n=max nodes and p=target false positive rate. For n=100, p=0.001: m=1437 bits, k=10 hash functions.
- Use a `struct` with a `fixed byte[180]` array (1440 bits) instead of two ulongs.
- Implement a fallback to HashSet<int> for hierarchies larger than the Bloom filter can handle.
- Profile the actual false positive rate in the performance test scenes (Small=10, Medium=50, Large=100+).
- Consider whether the Bloom filter optimization is necessary at all -- the existing HashSet-based VisitedNodes in QW-1 already works and was validated in tests.

**Warning signs:**
- Messages stop propagating in deep hierarchies (false positive "cycle detected")
- Bloom filter overhead exceeds HashSet overhead for small hierarchies (<50 nodes)
- False positive rate is not measured in benchmarks
- Users report "phantom cycle detection" warnings

**Phase to address:** Phase 5, design review before implementation. Run the math before writing code.

---

## Medium Pitfalls

### Pitfall 11: Scope Creep in Distributed Messaging Phase

**Severity:** Medium
**Status:** NEW
**Phase:** Phase 6 (Distributed Messaging)

**What goes wrong:**
The distributed messaging phase includes 4 sub-phases: distribution semantics, change notification with veto, multi-object atomic messaging (two-phase commit), and network topology awareness. Multi-object atomic messaging in a distributed system is a research problem that has been studied for decades (Lamport, Paxos, Raft). Attempting a correct implementation in 20 hours is unrealistic. If the implementation has subtle bugs (partial delivery, split-brain), it could undermine the entire networking story.

**How to avoid:**
- Phase 1 (distribution semantics) and Phase 2 (change notification) are achievable and useful. Implement these.
- Phase 3 (atomic messaging) should be a "future work" section in the paper, not a shipped feature. Two-phase commit over unreliable networks is a hard problem.
- Phase 4 (topology awareness) is useful but secondary. Implement only if time permits.
- Total scope for Phase 6: ~45 hours (Phases 1+2), not 80 hours.

**Warning signs:**
- Two-phase commit implementation takes longer than 20 hours
- Edge cases in distributed transactions keep appearing (timeout, partial failure, network partition)
- Atomic messaging tests pass locally but fail with FishNet/Fusion

**Phase to address:** Phase 6, scope decision at start.

---

### Pitfall 12: Parallel FSM State Explosion Makes Testing Intractable

**Severity:** Medium
**Status:** NEW (parallel-fsm README lists "state explosion" as a known FSM problem but does not address how parallel regions exacerbate it)
**Phase:** Phase 3 (Parallel FSMs)

**What goes wrong:**
With N parallel regions each having M states, the total state space is M^N. Three regions with 5 states each = 125 combined states. Adding guard conditions, cross-region events, and priority-based conflict resolution makes exhaustive testing impossible. Subtle bugs hide in rare state combinations that are never tested.

**How to avoid:**
- Limit the paper contribution to 2-3 parallel regions with clearly defined interaction contracts (not arbitrary N regions).
- Implement a state space visualization tool (could integrate with the Visual Composer) that shows reachable state combinations.
- Use property-based testing (random state transitions with invariant checking) instead of enumerating all states.
- Define and enforce invariants: "no two regions can be in conflicting states" with runtime assertions.
- Focus the evaluation on a concrete use case (e.g., gaze + gesture + voice for VR) rather than the general N-region case.

**Warning signs:**
- Test coverage cannot reach >60% of state combinations
- Conflict resolution produces different results depending on timing
- Bug reports describe "impossible states" that aren't covered by tests
- The parallel FSM works perfectly with 2 regions but fails with 3+

**Phase to address:** Phase 3, architecture design.

---

### Pitfall 13: Tutorial Validation Reveals Wiki-Code Mismatch Too Late

**Severity:** Medium
**Status:** ALREADY KNOWN (tutorial-validation README documents that 0/12 tutorials have been validated)
**Phase:** Phase 7 (Tutorial Validation)

**What goes wrong:**
Tutorial validation is scheduled last (Phase 7) but the tutorials are the primary onboarding path for user study participants. If tutorials are broken, the user study in Phase 1 (Visual Composer) cannot recruit competent participants. Additionally, broken tutorials undermine the paper's claim that the framework is usable.

**How to avoid:**
- Move tutorial validation to BEFORE the user study, not after.
- At minimum, validate Tutorials 1-5 (basic concepts through Fluent DSL) before recruiting user study participants.
- Create automated "smoke tests" for each tutorial: load scene, enter Play Mode, verify no errors in Console, verify expected GameObjects exist.
- For the paper, only claim tutorials work if they have been validated. Do not cite unvalidated tutorials.

**Warning signs:**
- User study participants cannot complete training tasks
- Tutorials reference API methods that have been renamed or removed
- Wiki screenshots do not match current Unity version's UI
- Tutorial scenes fail to load in Unity 6000.3.7f1

**Phase to address:** Move to pre-Phase 1 or early Phase 1.

---

### Pitfall 14: CPU-GPU Synchronization Stalls in Spatial Indexing

**Severity:** Medium
**Status:** NEW
**Phase:** Phase 2 (Spatial Indexing)

**What goes wrong:**
The spatial indexing compute shader writes query results to a buffer on the GPU. Reading those results back to the CPU requires `ComputeBuffer.GetData()` (synchronous, stalls the GPU pipeline) or `AsyncGPUReadback` (asynchronous, 1-2 frame latency). If spatial query results are needed for the current frame's message routing, the synchronous path creates a GPU pipeline stall that can add 5-15ms per frame. The async path means messages are routed based on 1-2-frame-old spatial data.

**How to avoid:**
- Use `AsyncGPUReadback` and accept 1-2 frame latency for spatial queries. For most use cases (proximity events, area-of-effect), 1-frame latency is acceptable.
- Cache spatial query results and reuse them across multiple messages in the same frame.
- For the paper, explicitly acknowledge the latency tradeoff and measure its impact on user-perceived responsiveness.
- Provide a CPU fallback path (see Pitfall 4) for latency-sensitive applications.

**Warning signs:**
- Spatial queries add >5ms per frame on the CPU timeline
- Message routing is delayed by 1-2 frames when using GPU spatial queries
- GPU memory pressure from multiple simultaneous queries
- `AsyncGPUReadback` callbacks arrive out of order

**Phase to address:** Phase 2, implementation.

---

### Pitfall 15: MmRelayNode.cs Becomes an Unmanageable God Object

**Severity:** Medium
**Status:** PARTIALLY KNOWN (QW-6 reduced MmRelayNode from 1426 to 1247 lines, but 7 new features all need to add code to this class)
**Phase:** All phases

**What goes wrong:**
MmRelayNode.cs is already the "MOST IMPORTANT CLASS" at 1247 lines. Each new feature adds hooks:
- Visual Composer: introspection events for message flow visualization
- Spatial Indexing: spatial query integration in MmInvoke
- Parallel Dispatch: thread-safe message queue integration
- Time-Travel Debugging: recording hooks in MmInvoke
- Static Analysis: Bloom filter check in MmInvoke

After 7 features, MmRelayNode could exceed 2000 lines with 5+ conditional code paths in the hot loop.

**How to avoid:**
- Define an `IMmMessageInterceptor` interface with `OnBeforeRoute(MmMessage)` and `OnAfterRoute(MmMessage, List<MmResponder>)` methods.
- Each feature registers an interceptor rather than modifying MmRelayNode directly.
- MmRelayNode iterates interceptors in the hot path. When no interceptors are registered, the overhead is a single null check.
- This also enables the zero-dependency policy: interceptors are in their respective feature assemblies, not in the core.

**Warning signs:**
- MmRelayNode.cs exceeds 1500 lines
- Multiple `if (featureEnabled)` checks in the MmInvoke hot path
- Two features need conflicting changes to the same MmRelayNode method
- Performance regression from added branching in the hot path

**Phase to address:** Pre-Phase 1. Design the interceptor pattern before any feature modifies MmRelayNode.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Inline feature checks in MmRelayNode.MmInvoke | Fast to implement, no abstraction needed | God object, performance regression, merge conflicts | Never -- use interceptor pattern |
| Storing time-travel data only in memory | Simpler implementation, no file I/O | Lost on Play Mode exit, unbounded memory growth | Only for prototype/pilot testing |
| GPU-only spatial implementation | Best benchmark numbers for paper | Excludes WebGL, mobile, Quest; limits generalizability claim | Only if CPU path also exists as fallback |
| Hard-coding 128-bit Bloom filter | Simple struct, fast allocation | Unusable for >20 node hierarchies, false positives | Never -- size correctly from the start |
| Skipping CI/CD setup | Saves ~4 hours of setup time | Days of debugging integration regressions across 7 features | Never for a 7-feature project |
| Parallel.ForEach on FSM regions | Clean concurrent code | Crashes when any region touches Unity API | Never -- use Job System or main-thread sequential |

## Integration Gotchas

Common mistakes when connecting new features to the existing framework.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Visual Composer + MmRelayNode | Polling routing table state every frame for graph sync | Use event-based sync: hook `MmAddToRoutingTable` and `UnRegisterResponder` to push changes to graph |
| Spatial Indexing + MmMetadataBlock | Adding spatial fields directly to MmMetadataBlock (breaks serialization) | Use separate `SpatialMetadataBlock` subclass or `MmRoutingOptions` extension |
| Time-Travel + CircularBuffer | Replacing existing CircularBuffer message history with time-travel recording | Keep CircularBuffer for debug display; time-travel uses its own storage |
| Parallel Dispatch + PerformanceMode | Assuming PerformanceMode disables all debugging hooks | PerformanceMode only disables `UpdateMessages()`; parallel dispatch needs its own enable/disable |
| Static Analysis + Existing HopLimit (QW-1) | Bloom filter replaces HopCount/VisitedNodes tracking | Keep both: HopCount is deterministic safety; Bloom filter is probabilistic optimization |
| Distributed Messaging + MmNetworkBridge | Modifying MmNetworkBridge internals for distribution semantics | Extend MmNetworkBridge via composition; wrap existing `RouteMessage` with distribution mode logic |
| All Features + Existing Tests | Running only new feature tests, not full suite | Run ALL 60+ test files after each feature merge |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Per-message string allocation in time-travel rejection reasons | GC spikes every 2-3 seconds during recording | Use enum-based rejection codes, pre-allocated string table | >100 msg/sec with Level 3 recording |
| Spatial octree rebuild every frame for moving objects | Frame time spikes of 5-10ms | Incremental update with dirty flag per object | >500 moving objects |
| Visual Composer graph layout computation on main thread | Editor freezes for 1-2 seconds on large hierarchies | Use async layout computation with EditorCoroutine | >100 nodes in graph |
| Bloom filter check on every MmInvoke call | 50-100ns overhead per message even when cycles impossible | Only enable for hierarchies with known cycles (static analysis flags) | >1000 msg/sec in simple acyclic hierarchies |
| Parallel FSM Parallel.ForEach thread creation overhead | Thread pool contention with Unity Job System | Use Unity Job System instead of .NET thread pool | >3 parallel regions with high-frequency transitions |

## UX Pitfalls

Common user experience mistakes for framework developers and editor tools.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Visual Composer requires reading documentation to start | Developers abandon tool before learning it | Include an auto-layout button that arranges existing scene hierarchy into a graph on first open |
| Time-travel debugger captures everything by default | Performance impact discourages use | Opt-in recording with clear "Start Recording" button; indicator shows recording overhead |
| Spatial indexing requires manual configuration (octree depth, split threshold) | Wrong defaults lead to poor performance; users blame the framework | Auto-configure based on scene bounds and object count; expose "Simple/Balanced/Detailed" presets |
| Parallel FSM requires understanding of concurrency concepts | Researchers/designers cannot use the feature | Provide a "sequential mode" that runs regions in order, with a switch to "parallel mode" for advanced users |
| Static analysis reports warnings on valid hierarchies (Bloom filter false positives) | Users learn to ignore warnings, missing real issues | Zero false positives in the default configuration; probabilistic checks are opt-in for performance |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Visual Composer:** Graph renders and syncs with scene -- but does it survive assembly reload? Verify Play Mode enter/exit cycle preserves graph state.
- [ ] **Visual Composer:** Nodes and edges display -- but does bi-directional editing work? Verify that deleting a node in the graph removes the MmRelayNode from the scene.
- [ ] **Spatial Indexing:** Octree queries return correct results -- but do they work with moving objects? Verify spatial index updates when Transform.position changes.
- [ ] **Spatial Indexing:** GPU compute shader works -- but is there a CPU fallback? Verify behavior on a machine without compute shader support.
- [ ] **Parallel FSMs:** Regions process independently -- but do cross-region events arrive in order? Verify message ordering under load.
- [ ] **Parallel FSMs:** Conflict resolution works in tests -- but does it work with real Unity components that modify GameObjects? Verify no `UnityException: main thread` errors.
- [ ] **Time-Travel Debugging:** Messages are recorded -- but do they contain enough context to answer "why didn't message X reach responder Y?" Verify rejection reasons are captured.
- [ ] **Time-Travel Debugging:** Timeline scrubber works -- but does it handle gaps (frames with no messages)? Verify scrubber behavior when no messages were sent for 100+ frames.
- [ ] **Static Analysis:** Tarjan's algorithm finds cycles -- but does it handle runtime hierarchy changes? Verify re-analysis after `MmAddToRoutingTable()` call.
- [ ] **Static Analysis:** Bloom filter detects cycles -- but what is the actual false positive rate at 50+ nodes? Measure and document.
- [ ] **Distributed Messaging:** Three distribution modes work -- but are they backward compatible? Verify existing `OverNetwork()` tests still pass.
- [ ] **Tutorial Validation:** Tutorials load and run -- but do they teach the current API? Verify code examples use the Fluent DSL, not deprecated patterns.
- [ ] **Paper Evaluation:** User study shows significant results -- but are the tasks ecologically valid? Verify tasks resemble real developer workflows, not toy problems.

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| GraphView breaks on Unity update | HIGH | Revert Unity version; if impossible, rewrite Visual Composer on UI Toolkit (2-3 weeks) |
| Paper rejected for scope | MEDIUM | Split into 2 papers (developer tools + performance); resubmit to UIST + CHI |
| Parallel FSM race conditions in production | HIGH | Revert to sequential FSM execution; redesign with Job System (2 weeks) |
| Time-travel recording overhead >10% | LOW | Reduce recording granularity to Level 1; add explicit opt-in for detailed recording |
| Bloom filter false positives | LOW | Replace with correctly-sized filter or fall back to HashSet (1-2 days) |
| Integration test failures after feature merge | MEDIUM | Git bisect to find breaking commit; add missing integration test (1-3 days) |
| User study shows no significant result | HIGH | Add qualitative analysis; reframe as "insights and lessons learned"; consider different evaluation strategy per Ledo et al. (2018) |
| GPU spatial queries don't work on reviewer's machine | MEDIUM | Ensure CPU fallback is default; GPU is opt-in acceleration |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Severity | Prevention Phase | Verification |
|---------|----------|------------------|--------------|
| P1: 7-feature paper scope | Critical | Pre-Phase 1 | Paper outline reviewed by advisor with 2-3 clear contributions identified |
| P2: GraphView instability | Critical | Phase 1, Week 1 | Integration test passes on locked Unity version |
| P3: Demo-ware evaluation | Critical | Phase 1, mid-phase | Pilot study with N=3-5 completed |
| P4: GPU platform exclusion | Critical | Phase 2, Week 1 | CPU fallback benchmark shows <5ms for 10K objects |
| P5: Assembly reload state loss | Critical | Phase 1, Week 2 | Visual composer survives Play Mode enter/exit cycle |
| P6: Parallel FSM threading | Critical | Phase 3, Week 1 | Architecture decision documented; no Parallel.ForEach on Unity API calls |
| P7: Recording overhead | High | Phase 4, Week 1 | Level 1 recording measured at <1% overhead with 500 msg/sec |
| P8: Insufficient prior art comparison | High | Pre-Phase 1 | Related work section includes industrial tools; no undefended "first" claims |
| P9: Integration regression | High | Pre-Phase 1 | CI/CD pipeline runs all tests on every push |
| P10: Bloom filter sizing | High | Phase 5, design review | False positive rate calculated and verified for 100-node hierarchy |
| P11: Distributed messaging scope | Medium | Phase 6, start | Atomic messaging deferred to future work |
| P12: Parallel FSM state explosion | Medium | Phase 3, design | Property-based tests cover state combinations; invariants enforced |
| P13: Tutorial validation timing | Medium | Move to pre-Phase 1 | Tutorials 1-5 validated before user study recruitment |
| P14: CPU-GPU sync stalls | Medium | Phase 2, implementation | AsyncGPUReadback used; latency measured and documented |
| P15: MmRelayNode god object | Medium | Pre-Phase 1 | IMmMessageInterceptor interface designed; features use interceptors |

## Sources

- [UIST 2025 Author Guide](https://uist.acm.org/2025/author-guide/) -- Paper scope, evaluation, desk rejection criteria
- [UIST 2024 Author Guide](https://uist.acm.org/2024/author-guide/) -- Evaluation strategies, related work requirements
- [CHI 2025 Guide to Reviewing Papers](https://chi2025.acm.org/guide-to-reviewing-papers/) -- Reviewer expectations for systems papers
- [Olsen (UIST 2007) "Evaluating User Interface Systems Research"](https://dl.acm.org/doi/10.1145/1294211.1294256) -- Foundational framework for systems evaluation heuristics
- [Ledo et al. (CHI 2018) "Evaluation Strategies for HCI Toolkit Research"](https://dl.acm.org/doi/10.1145/3173574.3173610) -- Four evaluation strategies for toolkit papers
- [Unity GraphView features broken on 2022.3](https://discussions.unity.com/t/graphview-features-broken-on-unity-2022-3-13f1/945630) -- GraphView version instability
- [Unity GraphView API (Experimental)](https://docs.unity3d.com/ScriptReference/Experimental.GraphView.GraphView.html) -- Still in Experimental namespace
- [Unity EditorWindow assembly reload guide](https://forum.unity.com/threads/a-guide-to-editor-script-serialization-making-sure-fields-dont-go-null-on-assembly-reloads.276069/) -- Serialization survival across reloads
- [Unity WebGL compute shader limitation](https://discussions.unity.com/t/platform-does-not-support-compute-shaders-in-dev-console-when-compiled-to-webgl/876954) -- Compute shaders unsupported in WebGL 2.0
- [Unity WebGPU limitations](https://docs.unity3d.com/6000.2/Documentation/Manual/WebGPU-limitations.html) -- Experimental WebGPU restrictions
- [Statecharts: State Machine State Explosion](https://statecharts.dev/state-machine-state-explosion.html) -- Parallel regions and state space growth
- [Game Programming Patterns: State](https://gameprogrammingpatterns.com/state.html) -- FSM patterns and pitfalls in game engines
- [Unreal Engine Replay System](https://dev.epicgames.com/documentation/en-us/unreal-engine/using-the-replay-system-in-unreal-engine) -- Replay performance overhead reference
- [Re-Animator: High-Fidelity Tracing/Replaying](https://www.fsl.cs.sunysb.edu/~umit/files/reanimator.pdf) -- Recording overhead benchmarks (1.8-2.3x)
- Project files: `dev/FREQUENT_ERRORS.md`, `dev/active/visual-composer/README.md`, `dev/active/parallel-dispatch/README.md`, `dev/active/static-analysis/README.md`, `dev/active/parallel-fsm/README.md`, `dev/active/spatial-indexing/README.md`, `dev/active/time-travel-debugging/README.md`, `dev/active/distributed-messaging/README.md`, `dev/active/tutorial-validation/README.md`

---
*Pitfalls research for: MercuryMessaging UIST 2026 paper -- hierarchical message routing framework (Unity)*
*Researched: 2026-02-11*
