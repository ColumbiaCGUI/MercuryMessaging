# MercuryMessaging Full Codebase Codex Review Report

**Date:** 2026-02-27
**Reviewer:** Claude Opus 4.6 + OpenAI Codex (GPT-5.3-Codex, high reasoning)
**Scope:** Full codebase review organized by `.planning` structure
**Method:** 10 parallel subagent reviews, each with Claude analysis + Codex second opinion

---

## Fix Status (2026-02-27)

All 16 critical issues and 10 of 28 warnings have been resolved across 4 commits on `fix/codex-review-issues`:

| Critical # | Status | Commit |
|-----------|--------|--------|
| 1 | RESOLVED | `ef1738d9` — Wave 1: PackMetadata widened to 8-bit LevelFilter |
| 2 | RESOLVED | `ef1738d9` — Wave 1: IsDeserialized set after binary deserialization |
| 3 | RESOLVED | `d93a9371` — Wave 4: NullTypeId sentinel added to pooled write path |
| 4 | RESOLVED | `ef1738d9` — Wave 1: try/finally + _invokeDepth counter |
| 5 | RESOLVED | `ef1738d9` — Wave 1: throw replaced with Debug.LogWarning |
| 6 | RESOLVED | `661d1b92` — Wave 2: default case added to SendToTarget() |
| 7 | RESOLVED | `661d1b92` — Wave 2: Switch(int) converts to string via .ToString() |
| 8 | RESOLVED | `661d1b92` — Wave 2: Custom methods (>1000) handled in ExecuteStandard |
| 9 | RESOLVED | `661d1b92` — Wave 2: Null guards on all 16 extension methods |
| 10 | RESOLVED | `d93a9371` — Wave 4: Null guard after currentTaskInfo.Next |
| 11 | RESOLVED | `d93a9371` — Wave 4: Platform warning + Writer=null for non-standalone |
| 12 | RESOLVED | `d93a9371` — Wave 4: StreamReader wrapped in using statement |
| 13 | RESOLVED | `ef1738d9` — Wave 1: Vector2.ToCSV format string fixed |
| 14 | RESOLVED | `d7f7bec6` — Wave 3: Guard base.MmInvoke for custom methods |
| 15 | RESOLVED | `d7f7bec6` — Wave 3: Send(false) → BroadcastSetActive(false) |
| 16 | RESOLVED | `d7f7bec6` — Wave 3: Fluent TaskInfo → traditional MmMessageSerializable API |

| Warning # | Status | Commit |
|-----------|--------|--------|
| 3 | RESOLVED | `d93a9371` — Duplicate CircularBuffer removed |
| 5 | RESOLVED | `661d1b92` — Predicate pool wrapped in try/finally |
| 10 | RESOLVED | `ef1738d9` — Reentrancy handled via _invokeDepth counter |
| 12 | RESOLVED | `d93a9371` — Bare catch → catch(Exception ex) with logging |
| 13 | RESOLVED | `d93a9371` — Dead LevelFilterAdjust method removed |
| 16 | RESOLVED | `d93a9371` — Debug.Log leftovers removed |
| 26 | RESOLVED | `d93a9371` — TryParse with warning logs |
| 27 | PARTIALLY | `102b6c90` — Test coverage added for fixed issues |
| 28 | RESOLVED | `d93a9371` — Dead code removed from MmAppStateSwitchResponder |

Test coverage: `102b6c90` — 5 new test files, 25+ tests covering all fix areas.
Assembly split: `4f9c143a` — Task and Support split into separate assemblies.

---

## Executive Summary

10 reviews were conducted across the entire MercuryMessaging codebase: 6 framework subsystem reviews and 4 tutorial batch reviews (covering all 12 tutorials). **16 critical issues** and **20+ warnings** were identified.

The highest-risk areas are:
1. **Network serialization layer** — 3 critical bugs that compound each other (LevelFilter truncation, missing IsDeserialized flag, format mismatch)
2. **DSL dispatch** — silent message loss, runtime crashes, unsupported custom methods
3. **Tutorials** — 8 of 12 tutorials have issues that will block students

| # | Review Area | Verdict |
|---|-------------|---------|
| 1 | Phase 1 Changes | **PASS WITH WARNINGS** |
| 2 | Protocol/Core (Hot Path) | **PASS WITH WARNINGS** |
| 3 | Protocol/DSL (Fluent API) | **NEEDS FIXES** |
| 4 | Nodes + Responders | **PASS WITH WARNINGS** |
| 5 | Messages + Filters + Network | **NEEDS FIXES** |
| 6 | FSM + AppState + StdLib + Task | **NEEDS FIXES** |
| 7 | Tutorials 1-3 | **NEEDS FIXES** |
| 8 | Tutorials 4-6 | T4: PASS, T5: PASS, T6: **NEEDS FIXES** |
| 9 | Tutorials 7-9 | T7: **NEEDS FIXES**, T8: PASS, T9: **NEEDS FIXES** |
| 10 | Tutorials 10-12 | T10-12: All **NEEDS FIXES** |

---

## All Critical Findings (Priority Order)

| # | Area | Issue | Impact | File(s) |
|---|------|-------|--------|---------|
| 1 | **Network** | `PackMetadata` truncates LevelFilter to 4 bits — `ToDescendants()`/`ToAncestors()`/`ToCousins()` silently broken over network | Advanced routing destroyed on wire | `MmBinarySerializer.cs` PackMetadata/UnpackMetadata |
| 2 | **Network** | `IsDeserialized` never set on binary deserialization — infinite re-broadcast | Message storms | `MmMessage.cs`, `MmBinarySerializer.cs`, `MmNetworkBridge.cs` |
| 3 | **Network** | Pooled vs non-pooled serialization format mismatch for MmMessageSerializable | Data corruption | `MmBinarySerializer.cs` |
| 4 | **Nodes** | `doNotModifyRoutingTable` no `try/finally` — exception permanently corrupts node | Permanent node failure | `MmRelayNode.cs:867` |
| 5 | **Nodes** | `MmBaseResponder` throws `ArgumentOutOfRangeException` for unknown methods, halts ALL remaining dispatch | Cascading dispatch halt | `MmBaseResponder.cs:142-144` |
| 6 | **DSL** | `SendToTarget()` silently drops messages — no logging, no default case | Silent message loss | `MmFluentMessage.cs` |
| 7 | **DSL** | `Switch(int)` throws `InvalidCastException` — `MmBaseResponder` casts to `MmMessageString` | Runtime crash | `MmRoutingBuilder.cs:266`, `MmBaseResponder.cs` |
| 8 | **DSL** | Custom methods (>1000) unsupported in fluent `ExecuteStandard()` despite docs | Silent failure | `MmFluentMessage.cs` |
| 9 | **DSL** | `responder.Send()` dereferences `responder.name` when `responder == null` | NullReferenceException | `MmMessagingExtensions.cs:265` |
| 10 | **Task** | `ProceedToNextTask()` NullRef at end of task sequence | Crash during user studies | `MmTaskManager.cs:200-210` |
| 11 | **Task** | `MmDataHandlerFile` null Writer on mobile/WebGL — `#if UNITY_STANDALONE` guard | Silent data loss | `MmDataHandlerFile.cs` |
| 12 | **Task** | `LoadPartiallyFinishedTaskSequence()` StreamReader leak — missing `using` | Resource leak | `MmTaskInfoCollectionFileLoader.cs:110-135` |
| 13 | **Core** | `Vector2.ToCSV()` format string `"{0}{3}{1}{3}{2}"` uses `{3}` with only 3 args | `FormatException` crash | `TransformExtensions.cs:277` |
| 14 | **T3** | `base.MmInvoke()` throws `ArgumentOutOfRangeException` for custom methods — enemy logic unreachable | Tutorial broken | `T3_EnemyResponder.cs:23` |
| 15 | **T2** | `Send(false)` sends `MessageBool`, not `SetActive` — menu toggling is dead code | Tutorial broken | `T2_MenuController.cs:29` |
| 16 | **T9** | `MmMethod.TaskInfo` via fluent API falls through to default — task info never delivered | Silent runtime failure | `T9_ExperimentManager.cs:123-126` |

---

## All Warning Findings

| # | Area | Issue | File(s) |
|---|------|-------|---------|
| 1 | **Core** | `MmMetadataBlock` is a class — heap allocation per message undermines pooling (1000-2000 allocs/sec) | `MmMetadataBlock.cs`, `MmMessagePool.cs` |
| 2 | **Core** | `MmMessage.Copy()` bypasses pool — untracked allocations in multi-direction routing | `MmMessage.cs:215` |
| 3 | **Core** | Duplicate `CircularBuffer` implementations (Protocol/Core vs Support/Data) | `MmCircularBuffer.cs` x2 |
| 4 | **DSL** | Zero-allocation claim is false — `object _payload` boxes value types | `MmFluentMessage.cs` |
| 5 | **DSL** | Predicate pool leak — no `try/finally` protection | `MmFluentMessage.cs` |
| 6 | **DSL** | Thread-safety inconsistency — `_pendingQueries` unlocked vs `_asyncQueries` locked | `MmRelayNodeExtensions.cs`, `MmTemporalExtensions.cs` |
| 7 | **DSL** | Reflection silent failure — `?.Invoke` swallows null on method signature change | `MmResponderExtensions.cs` |
| 8 | **DSL** | Regex allocation in hot path — `new Regex()` per `NamedLike()` call | `MmQuery.cs` |
| 9 | **DSL** | API surface explosion — 4+ overlapping ways to send messages | Multiple DSL files |
| 10 | **Nodes** | Self-reentrancy on same node corrupts `doNotModifyRoutingTable` flag | `MmRelayNode.cs` |
| 11 | **Nodes** | HashSet memory leak in advanced routing (`Copy()` -> `GetCopy()`, never returned) | `MmRelayNode.cs` |
| 12 | **Nodes** | `MmRelaySwitchNode.Awake()` bare `catch` block violates coding standards | `MmRelaySwitchNode.cs:134` |
| 13 | **Nodes** | `LevelFilterAdjust()` is dead code (defined, never called) | `MmRelayNode.cs:1481` |
| 14 | **Network** | `MmWriter.WriteFloat` allocates via `BitConverter.GetBytes` — undermines zero-allocation claim | `MmWriter.cs:174-198` |
| 15 | **Network** | `MmObjectPool` statistics counters not thread-safe | `MmMessagePool.cs` (Network) |
| 16 | **Network** | `Debug.Log` leftover in `MmMessageSerializable.Deserialize` — console spam | `MmMessageSerializable.cs:123-124` |
| 17 | **Network** | `Type.GetType` + `Activator.CreateInstance` with untrusted type names — RCE vector | `MmMessageSerializable.cs` |
| 18 | **Network** | `MmMessageGameObject` legacy Deserialize uses broken `GameObject.Find(instanceID.ToString())` | `MmMessageGameObject.cs` |
| 19 | **Network** | Two files named `MmMessagePool.cs` — naming collision confuses search | `Protocol/Core/` vs `Protocol/Network/` |
| 20 | **FSM** | `StartTransitionTo()` overwrites pending transition without guard (race condition) | `FiniteStateMachine.cs:53-56` |
| 21 | **FSM** | `StartLeaving()` null check on value type is dead code | `FiniteStateMachine.cs:72` |
| 22 | **FSM** | `AppStateBuilder.CanTransitionTo()` populated but never enforced at runtime | `AppStateBuilder.cs` |
| 23 | **StdLib** | All 16 message Serialize() use `.Concat().ToArray()` — GC pressure | `MmUIMessages.cs`, `MmInputMessages.cs` |
| 24 | **StdLib** | Dispatch uses unsafe direct casts without type checking | `MmUIResponder.cs:115-116` |
| 25 | **StdLib** | UI/Input message types not registered in `MmTypeRegistry` — may fail over network | `MmTypeRegistry.cs` |
| 26 | **Task** | `MmTaskInfo.Parse()` uses `int.Parse`/`bool.Parse` without TryParse | `MmTaskInfo.cs:90-98` |
| 27 | **Task** | Task subsystem has ZERO automated test coverage | `Assets/MercuryMessaging/Tests/` |
| 28 | **Task** | `MmAppStateSwitchResponder` has dead code (unused dictionary + TODO comment) | `MmAppStateSwitchResponder.cs:46` |

---

## Tutorial Health Summary

### Tutorial 1: Introduction — PASS WITH MINOR ISSUES
- **Compilation:** All types resolve correctly. Student's "MessagePool is not a type" error NOT reproducible from tutorial code — likely typed `MessagePool` without `Mm` prefix.
- **Wiki:** Key 3/Alpha3 mapping was corrected in deployed wiki. Console output format differs (uses GameObject name).
- **Traps:** Space key conflict between `T1_SceneController` and `T1_TraditionalApiExample`. Misleading comment in TraditionalApiExample (says "SetActive" but sends MessageString).

### Tutorial 2: Basic Routing — NEEDS FIXES
- **CRITICAL:** `Send(false)` sends `MmMethod.MessageBool` but `T2_ButtonResponder` overrides `SetActive()` which only fires on `MmMethod.SetActive`. Menu toggling is dead code.
- **Wiki is actually more correct** than the code — wiki uses `ReceivedMessage(MmMessageBool)`.
- **Code quality:** `new void Start()` hides base lifecycle in both T2 scripts.

### Tutorial 3: Custom Responders — NEEDS FIXES
- **CRITICAL:** `T3_EnemyResponder.cs:23` calls `base.MmInvoke(message)` unconditionally. `MmBaseResponder` throws `ArgumentOutOfRangeException` for custom methods (1000+). Enemy damage/heal/color logic is unreachable.
- **Fix:** Guard base call: `if ((int)message.MmMethod < 1000) base.MmInvoke(message);`
- **Note:** `T3_EnemyResponderExtendable.cs` does NOT have this bug — `MmExtendableResponder` handles this correctly.
- **Runtime AddComponent without `MmRefreshResponders()`** in `T3_GameController.cs:113`.

### Tutorial 4: Custom Messages — PASS
- Two parallel file sets (root vs Scripts/) with different method IDs (1001 vs 1000) — usability concern but not a bug.
- `MmMessageType` collision: both `T4_MyMessageTypes.ColorIntensity` and `T4_MessageTypes.ColorMessage` use ID 1100.

### Tutorial 5: Fluent DSL API — PASS
- Wiki claims `.ToSelf()` exists but it doesn't — students will get compile error if they try it.
- Orphaned `T5_SceneController.cs` in Scripts/ folder not referenced by wiki.

### Tutorial 6: FishNet Networking — NEEDS FIXES (Wiki)
- **CRITICAL wiki API mismatch:** Wiki uses `bridge.SetBackend()` / `bridge.SetResolver()` — methods don't exist. Actual API: `bridge.Configure(new FishNetBackend(), new FishNetResolver())`.
- Wiki omits `#if FISHNET_AVAILABLE` guards.
- Wiki references `FishNetBridgeSetup` which is test-only.
- Tutorial CODE is correct — only wiki needs fixing.

### Tutorial 7: Fusion 2 Networking — NEEDS FIXES (Wiki)
- **Same wiki API mismatch:** `SetBackend()`/`SetResolver()` don't exist.
- `T7_Fusion2PlayerResponder.cs:33` uses `new void Start()` which hides `MmResponder.Start()`, skipping lifecycle callbacks.
- `CONTRIBUTING.md` mentions `FUSION_WEAVER` but actual define is `FUSION2_AVAILABLE` — documentation inconsistency.

### Tutorial 8: Switch Nodes & FSM — PASS
- Cleanest tutorial of all. Code closely matches wiki.
- Minor: Wiki omits Q (quit), G (game over), 1-4 (direct state jump) keyboard shortcuts.
- Minor: Wiki `MenuResponder.SetActive()` example omits `base.SetActive(active)` call.

### Tutorial 9: Task Management — NEEDS FIXES
- **CRITICAL runtime bug:** `TasksNode.Send(MmMethod.TaskInfo).ToChildren().Selected().Execute()` — `MmMethod.TaskInfo` falls through to `default:` case in `ExecuteStandard()`. Task info is never delivered. `T9_TrialResponder` never receives data.
- **Wiki `IMmTaskInfo` example missing 3 required properties + 3 required methods** — student compile error.
- **Wiki uses `AdvanceToNextTask()`** — method doesn't exist. Framework has `ProceedToNextTask()`.
- **Wiki `MmDataCollector` API wrong** (same as T12).
- **`IMmSerializable` is obsolete** — wiki teaches deprecated path.

### Tutorial 10: Application State — NEEDS FIXES (Wiki)
- Wiki Step 2 uses `MmSwitchResponder` but actual base class is `MmAppStateSwitchResponder`.
- Wiki says "Don't call base" in `SetActive()` — but the code correctly calls `base.SetActive(active)`. Wiki advice would break activation.
- No compilation issues in code.

### Tutorial 11: Advanced Networking — NEEDS FIXES
- Loopback backend instantiated but never connected to `MmNetworkBridge`. `.OverNetwork()` has no effect.
- Massive scope mismatch: wiki covers full 3-layer architecture, code is a minimal loopback demo.
- Keyboard controls (Space, S, I) undocumented in wiki.

### Tutorial 12: VR Experiment — NEEDS FIXES (Critical)
- **CRITICAL: Wiki `MmDataCollector` API (`SetHeaders`/`AddRow`/`SaveToFile`) doesn't exist.** Actual API: `Add`/`Write`/`OpenTag`/`CloseTag`/`CreateDataHandler`. Students get compile errors.
- **`UNITY_XR_AVAILABLE` is non-standard define** — VR input silently excluded. Not documented.
- **Architecture completely diverges from wiki** — Wiki shows MmTaskManager-based design, code is monolithic `MonoBehaviour`.
- Code uses legacy `UnityEngine.XR.InputDevices` API behind custom define, wiki shows modern XR Interaction Toolkit.

---

## Detailed Review Reports

### Review 1: Phase 1 Changes — PASS WITH WARNINGS

**Scope:** Git commits from tutorial validation work (01-01 and 01-02 plans).

**Claude's Assessment:**
- All referenced tutorial scripts exist and compile correctly
- TutorialSceneFixer correctly wires components but is missing Tutorial 8
- Conditional compilation guards (`#if FISHNET_AVAILABLE`, `#if FUSION2_AVAILABLE`) are correct
- Wiki updates prepared but not yet pushed (blocked on human Unity testing)

**Codex's Assessment:**
- Confirmed all scripts referenced by TutorialSceneFixer exist
- Verified scene YAML contains expected component references
- Confirmed conditional compilation guards are proper
- Noted `T4_SceneSetup` uses different namespace

**Synthesis:**
- Phase is ~30-35% complete (Plan 01-01 done, 01-02 partially done)
- No code changes to core framework (correct for this phase)
- Missing Tutorial 8 in fixer (low impact, covered by Plan 01-04)

**Next Steps:**
1. Complete Plan 01-02 Tasks 2-3 (human Unity validation)
2. Add Tutorial 8 to TutorialSceneFixer
3. Proceed to Plans 01-03 and 01-04

---

### Review 2: Protocol/Core (Hot Path) — PASS WITH WARNINGS

**Scope:** 12 files in Protocol/Core/ including MmCircularBuffer, MmHashSetPool, MmLogger, MmMessagePool, Extensions.

**Critical:**
- `Vector2.ToCSV()` format string `"{0}{3}{1}{3}{2}"` uses placeholder `{3}` but only 3 args provided. Will throw `FormatException`. Fix: change to `"{0}{2}{1}"`.

**Warnings:**
- `MmMetadataBlock` is a class — 1000-2000 heap allocations/sec at full throughput
- `MmMessage.Copy()` bypasses pool — untracked allocations in multi-direction routing
- `MmCircularBuffer.GetEnumerator()` allocates via yield (gated by PerformanceMode)
- Duplicate CircularBuffer implementations

**Good Practices Noted:**
- `MmHashSetPool.GetCopy()` correctly uses pool
- `MmMessagePool.Return()` correctly skips deserialized messages
- Thread-unsafe static pools acceptable (Unity main thread only)

---

### Review 3: Protocol/DSL (Fluent API) — NEEDS FIXES

**Scope:** 23 C# files in Protocol/DSL/ (~8,200 lines). Two-tier fluent API.

**Critical (4):**
1. Silent message loss in `SendToTarget()` — duplicate switch without error logging
2. `Switch(int)` throws `InvalidCastException` — `MmBaseResponder` casts to `MmMessageString`
3. Custom methods (>1000) not supported in `ExecuteStandard()` despite docs
4. Null responder `.name` dereference in warning log

**Warnings (5):**
1. Zero-allocation claim false — `object _payload` boxes value types
2. Predicate pool leak — no `try/finally`
3. Thread-safety inconsistency — `_pendingQueries` unlocked
4. Reflection silent failure — `?.Invoke` swallows null
5. Regex allocation per `NamedLike()` call

**Both Claude and Codex independently agreed: NEEDS FIXES.**

**Priority Fixes:**
1. Unify `ExecuteStandard()`/`SendToTarget()` dispatch
2. Fix `Switch(int)` contract or remove int overloads
3. Support custom methods in fluent execution
4. Fix null responder dereference

---

### Review 4: Nodes + Responders (Core Routing) — PASS WITH WARNINGS

**Scope:** 10 files including MmRelayNode.cs (2282 lines), MmBaseResponder.cs, MmExtendableResponder.cs, MmRelaySwitchNode.cs.

**Critical (2):**
1. `doNotModifyRoutingTable` flag lacks `try/finally` — exception permanently corrupts routing. All subsequent `MmAddToRoutingTable` calls queue indefinitely.
2. `MmBaseResponder` throws `ArgumentOutOfRangeException` for unknown methods — halts ALL remaining dispatch on that node.

**Warnings (4):**
1. HashSet memory leak in advanced routing via `message.Copy()` -> `MmHashSetPool.GetCopy()` (never returned)
2. Lazy copy mutation of shared message state (fragile pattern)
3. Self-reentrancy on same node corrupts flag (inner MmInvoke clears prematurely)
4. `MmRelaySwitchNode.Awake()` bare `catch` block

**Good Practices Noted:**
- Routing correctness is SOUND for all 5 filters (Tag, Level, Active, Selected, Network)
- `MmExtendableResponder` is well-designed — correctly addresses "forgot base.MmInvoke()" anti-pattern
- Hop limit and cycle detection work as designed

**Priority Fixes:**
1. Wrap routing loop in `try/finally` for `doNotModifyRoutingTable`
2. Replace `ArgumentOutOfRangeException` with `Debug.LogWarning` in base responder default case
3. Use integer counter (`_invokeDepth`) instead of boolean for reentrancy safety

---

### Review 5: Messages + Filters + Network — NEEDS FIXES

**Scope:** 45 files across Protocol/Message (14), Protocol/Filters (6), Protocol/Network (25).

**Critical (4):**
1. `PackMetadata` masks `LevelFilter` with `0x0F` — only 4 bits preserved. Descendants(32), Ancestors(64), Cousins(16), Custom(128) silently zeroed on network. Fix: widen to 8 bits (14 unused bits available).
2. `IsDeserialized` has `private set`, only assigned in legacy `Deserialize(object[])`. Binary path never sets it. Network messages re-broadcast infinitely.
3. `WriteSerializableDataPooled` writes ushort type ID, `WriteSerializableData` writes type name string. Incompatible formats if paths mixed.
4. `MmMessageGameObject` legacy Deserialize uses `GameObject.Find(instanceID.ToString())` — broken.

**Warnings (4):**
1. `MmWriter.WriteFloat` allocates via `BitConverter.GetBytes` per call
2. `MmObjectPool` statistics counters not thread-safe
3. `Debug.Log` leftover in `MmMessageSerializable.Deserialize`
4. `Type.GetType` + `Activator.CreateInstance` with untrusted type names — RCE vector in legacy path

**These 3 critical network bugs are interconnected and would compound in any real networked deployment with advanced routing. No tests cover these paths.**

**Priority Fixes:**
1. Widen LevelFilter in PackMetadata to 8 bits
2. Set `IsDeserialized = true` after binary deserialization
3. Unify serialization format for MmMessageSerializable

---

### Review 6: FSM + AppState + StdLib + Task — NEEDS FIXES

**Scope:** ~40 files across FSM (5), AppState (3), StandardLibrary (6), Task (9), Support/Data (8).

**Critical (3):**
1. `MmTaskManager.ProceedToNextTask()` — when `currentTaskInfo.Next` is null, `CurrentTaskInfo.TaskName` throws NullRef
2. `MmDataHandlerFile.Open()` wrapped in `#if UNITY_STANDALONE || UNITY_EDITOR` — Writer null on mobile/WebGL with no warning
3. `LoadPartiallyFinishedTaskSequence()` StreamReader without `using` — resource leak

**Warnings (7):**
1. FSM `StartLeaving()` null check on value type is dead code
2. FSM `StartTransitionTo()` overwrites pending transition without guard
3. StandardLibrary Serialize() uses LINQ `.Concat().ToArray()` — GC pressure (16 message types)
4. StandardLibrary dispatch uses unsafe direct casts
5. `MmTaskInfo.Parse()` uses `int.Parse` without TryParse
6. Task subsystem has ZERO automated test coverage
7. UI/Input message types not registered in `MmTypeRegistry` — may fail over network

**Priority Fixes:**
1. Add null guard in `ProceedToNextTask()`
2. Fix `MmDataHandlerFile` platform support or add explicit warning
3. Wrap StreamReader in `using`
4. Add Task subsystem tests

---

## Recommended Fix Priority

### Tier 1: Safety-Critical (Fix ASAP)
1. `MmRelayNode.MmInvoke()` — add `try/finally` around `doNotModifyRoutingTable` (~5 lines)
2. `MmBaseResponder` default case — replace throw with `Debug.LogWarning` (~2 lines)
3. `PackMetadata` — widen LevelFilter to 8 bits (~10 lines)
4. `IsDeserialized` — set true after binary deserialization (~3 lines)
5. `Vector2.ToCSV()` — fix format string (~1 line)

### Tier 2: DSL Correctness (Fix before stable release)
6. Unify `ExecuteStandard()`/`SendToTarget()` dispatch
7. Fix `Switch(int)` contract
8. Support custom methods in fluent execution
9. Fix null responder dereference
10. Add `try/finally` for predicate pool

### Tier 3: Tutorial Fixes (Fix before user study)
11. T2: Change `Send(false)` to `BroadcastSetActive(false)` or fix responder override
12. T3: Guard `base.MmInvoke()` for custom methods
13. T6/T7 wiki: Replace `SetBackend()`/`SetResolver()` with `Configure()`
14. T9: Fix `MmMethod.TaskInfo` fluent path or use traditional API
15. T9/T12 wiki: Update `MmDataCollector` API documentation
16. T10 wiki: Fix base class name
17. T12: Document `UNITY_XR_AVAILABLE` define

### Tier 4: Performance + Maintenance (Fix when convenient)
18. Consider making `MmMetadataBlock` a struct
19. Make `MmMessage.Copy()` pool-aware
20. Remove duplicate CircularBuffer
21. Replace LINQ in StandardLibrary Serialize()
22. Add Task subsystem test coverage
23. Remove dead code (`LevelFilterAdjust`, `MmAppStateSwitchResponder` dictionary)
24. Rename Network `MmMessagePool.cs` to avoid naming collision

---

*Report generated by 10 parallel Claude+Codex review agents on 2026-02-27*
*Full agent transcripts available in `C:\Users\yangb\AppData\Local\Temp\claude\C--Users-yangb-Research-MercuryMessaging\tasks\`*
