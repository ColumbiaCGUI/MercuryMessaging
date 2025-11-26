# MercuryMessaging Full DSL Overhaul - Implementation Plan

**Last Updated:** 2025-11-25

## Executive Summary

Complete DSL overhaul for MercuryMessaging framework to improve developer experience through fluent APIs. This plan covers 8 phases of improvements across messaging, FSM, data collection, task management, networking, responder setup, hierarchy building, and app state management.

**Primary Goals:**
- Reduce code verbosity by 50-80% for common operations
- Unify API across relay nodes AND responders
- Fix misleading 360% overhead in DSL comparison test
- Clean up MmRelayNode by removing unused fields and extracting debug functionality

---

## Current State Analysis

### Problems Identified

1. **API Fragmentation**: 6+ ways to send the same message
   - `relay.Init()` (Quick tier)
   - `relay.Broadcast(MmMethod.Initialize)` (Convenience tier)
   - `relay.Send(MmMethod.Initialize).Execute()` (Fluent tier)
   - `relay.MmInvoke(MmMethod.Initialize, metadata)` (Raw tier)

2. **Responders as Second-Class Citizens**: Responders must use verbose `GetComponent<MmRelayNode>().MmInvoke(...)` pattern (39+ instances in codebase)

3. **Misleading Performance Test**: DSL_Comparison shows 360% overhead because it tests wrapper methods, not the fluent builder fast path

4. **MmRelayNode Bloat**:
   - Unused fields: `messageBuffer`, `_prevMessageTime`, `dirty`
   - Debug visualization mixed with core functionality

5. **No Fluent APIs for Other Systems**: FSM, Data Collection, Task Management all require verbose setup

### Key Metrics

| Current API | Lines to Send Message | Flexibility |
|-------------|----------------------|-------------|
| MmInvoke (Raw) | 7+ lines | Full |
| Broadcast/Notify | 1 line | Limited |
| Fluent (relay) | 1 line | Full |
| Fluent (responder) | N/A | N/A |

---

## Proposed Future State

### Two-Tier Unified API

**Tier 1: Auto-Execute (Works on BOTH relay nodes AND responders)**
```csharp
relay.BroadcastInitialize();      // Initialize descendants
relay.NotifyComplete();           // Signal completion to parents
relay.BroadcastValue("hello");    // Send value to descendants

responder.BroadcastInitialize();  // Same API!
responder.NotifyValue("status");  // Same API!
```

**Tier 2: Fluent Chain (Full control, also works on responders)**
```csharp
relay.Send("hello").ToDescendants().WithTag(MmTag.Tag0).Execute();
responder.Send("hello").ToDescendants().Execute();  // NEW!
```

### Naming Convention

Matches original `MmInvoke(MmMethod.Xxx, ...)` pattern:
- `Broadcast` prefix = down direction (to descendants)
- `Notify` prefix = up direction (to parents)
- Method name suffix = MmMethod enum name

---

## Implementation Phases

### Phase 1: Core Messaging DSL (Current Sprint)
**Effort:** Large (L) | **Duration:** 4-6 hours | **Priority:** Critical

#### Task 1.1: Commit Uncommitted Work
- **Description:** Commit 72 files with DSL optimizations from Sessions 3-4
- **Acceptance Criteria:** Clean git history with descriptive commit message
- **Effort:** S

#### Task 1.2: MmRelayNode Cleanup
- **Description:** Remove unused fields, extract debug to separate component
- **Acceptance Criteria:**
  - `messageBuffer`, `_prevMessageTime`, `dirty` removed
  - `MmRelayNodeDebug.cs` created with visual debugging fields
  - MmRelayNode ~50-100 lines lighter
- **Dependencies:** Task 1.1
- **Effort:** M

#### Task 1.3: Create Unified Messaging API
- **Description:** Create `MmMessagingExtensions.cs` with Tier 1 methods
- **Acceptance Criteria:**
  - All Broadcast* methods work on both MmRelayNode and MmBaseResponder
  - All Notify* methods work on both types
  - Null-safe (handles missing relay nodes gracefully)
- **Dependencies:** Task 1.2
- **Effort:** M

#### Task 1.4: Add Tier 2 Fluent for Responders
- **Description:** Add `Send()` extension methods to MmBaseResponder
- **Acceptance Criteria:**
  - `responder.Send(payload)` returns MmFluentMessage
  - Full fluent chain works: `responder.Send("x").ToDescendants().Execute()`
- **Dependencies:** Task 1.3
- **Effort:** S

#### Task 1.5: Deprecate Old APIs
- **Description:** Mark redundant methods as [Obsolete]
- **Acceptance Criteria:**
  - `MmQuickExtensions` methods marked obsolete
  - Redundant `MmRelayNodeExtensions.Broadcast/Notify` marked obsolete
  - Deprecation messages point to new methods
- **Dependencies:** Task 1.4
- **Effort:** S

#### Task 1.6: Fix DSL_Comparison Test
- **Description:** Modify test to use fluent fast path for fair comparison
- **Acceptance Criteria:**
  - DSL uses `Send(value).Execute()` instead of `Broadcast()`
  - Overhead drops from 360% to <50%
- **Dependencies:** Task 1.4
- **Effort:** S

#### Task 1.7: Create Tests
- **Description:** Create `MmMessagingExtensionsTests.cs` with ~28 tests
- **Acceptance Criteria:**
  - Tier 1 relay node tests (12 tests)
  - Tier 1 responder tests (6 tests)
  - Tier 2 responder fluent tests (4 tests)
  - Edge case tests (3 tests)
  - Performance tests (3 tests)
- **Dependencies:** Task 1.4
- **Effort:** L

#### Task 1.8: Update Documentation
- **Description:** Update CLAUDE.md and DSL_API_GUIDE.md
- **Acceptance Criteria:**
  - Unified API documented
  - Migration guide for deprecated methods
  - Examples for both tiers
- **Dependencies:** Task 1.7
- **Effort:** M

---

### Phase 2: FSM Configuration DSL (Priority 1)
**Effort:** Small (S) | **Duration:** 1-2 hours | **Priority:** High

- Create `FsmConfigBuilder.cs` (~100 lines)
- Create `MmRelaySwitchNodeExtensions.cs` (~50 lines)
- Enable fluent FSM setup: `switchNode.ConfigureStates().OnEnter("State", handler).Build()`

---

### Phase 3: Data Collection DSL (Priority 2)
**Effort:** Medium (M) | **Duration:** 2-3 hours | **Priority:** High

- Create `MmDataCollectorExtensions.cs` (~150 lines)
- Create `DataCollectionBuilder.cs` (~100 lines)
- Enable: `collector.Configure().OutputAsCsv("name").Collect("Field", getter).Start()`

---

### Phase 4: Task Management DSL (Priority 3)
**Effort:** Medium-Large (M-L) | **Duration:** 3-4 hours | **Priority:** High

- Create `MmTaskSequenceBuilder.cs` (~200 lines)
- Create `MmTaskManagerExtensions.cs` (~100 lines)
- Enable: `MmTaskSequence.Create().OnTaskChange(handler).Build()`

---

### Phase 5: Network Message DSL (Priority 4)
**Effort:** Medium (M) | **Duration:** 2-3 hours | **Priority:** Medium

- Add `.OverNetwork()` to MmFluentMessage
- Create `MmNetworkExtensions.cs` (~100 lines)
- Handle host double-receive automatically

---

### Phase 6: Responder Registration DSL (Priority 5)
**Effort:** Medium (M) | **Duration:** 2-3 hours | **Priority:** Medium

- Create `MmResponderSetupBuilder.cs` (~180 lines)
- Enable: `gameObject.SetupMercury().AddResponder<T>().Build()`

---

### Phase 7: Hierarchy Building DSL (Priority 6)
**Effort:** Large (L) | **Duration:** 4-5 hours | **Priority:** Medium

- Create `MmHierarchyBuilder.cs` (~250 lines)
- Enable: `MmHierarchy.Build(root).AddChild(a).AddChild(b).Build()`

---

### Phase 8: App State DSL (Priority 7)
**Effort:** Medium (M) | **Duration:** 2-3 hours | **Priority:** Low

- Create `MmAppStateBuilder.cs` (~120 lines)
- Enable: `MmAppState.Create().DefineStates(...).Build(gameObject)`

---

## Risk Assessment

### High Risk
| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking existing code | High | Use [Obsolete] instead of removing; maintain backward compatibility |
| Performance regression | Medium | Run performance tests; use AggressiveInlining |

### Medium Risk
| Risk | Impact | Mitigation |
|------|--------|------------|
| Test failures | Medium | Run 292 existing tests before/after |
| MmRelayNode debug extraction breaks visualizers | Medium | Test graph visualization after extraction |

### Low Risk
| Risk | Impact | Mitigation |
|------|--------|------------|
| Documentation gaps | Low | Update CLAUDE.md incrementally |

---

## Success Metrics

### Phase 1 Success Criteria
- [ ] DSL overhead < 50% (down from 360%)
- [ ] All 292 existing tests pass
- [ ] 28 new tests pass
- [ ] Same API works on both relay nodes and responders
- [ ] MmRelayNode 50+ lines lighter

### Full Overhaul Success Criteria
- [ ] 50-80% code reduction for common operations
- [ ] All 8 fluent APIs implemented
- [ ] Developer onboarding time reduced
- [ ] No breaking changes (deprecation only)

---

## Required Resources

### Files to Create (Phase 1)
| File | Lines | Purpose |
|------|-------|---------|
| `MmMessagingExtensions.cs` | ~250 | Unified API |
| `MmRelayNodeDebug.cs` | ~80 | Debug visualization |
| `MmMessagingExtensionsTests.cs` | ~300 | Test coverage |

### Files to Modify (Phase 1)
| File | Changes |
|------|---------|
| `MmRelayNode.cs` | Remove unused fields (-50 lines) |
| `MmQuickExtensions.cs` | Add [Obsolete] |
| `MmRelayNodeExtensions.cs` | Add [Obsolete] |
| `MessageGenerator_DSL.cs` | Fix comparison test |
| `FluentApiTests.cs` | Add responder tests |
| `CLAUDE.md` | Update documentation |

### Total New Code
- **Phase 1:** ~700 lines
- **Phases 2-8:** ~1,600 lines
- **Grand Total:** ~2,300 lines across 11 new files

---

## Timeline Estimates

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1 (Core) | 4-6 hours | None |
| Phase 2 (FSM) | 1-2 hours | Phase 1 |
| Phase 3 (Data) | 2-3 hours | Phase 1 |
| Phase 4 (Tasks) | 3-4 hours | Phase 1 |
| Phase 5 (Network) | 2-3 hours | Phase 1 |
| Phase 6 (Responders) | 2-3 hours | Phase 1 |
| Phase 7 (Hierarchy) | 4-5 hours | Phase 1 |
| Phase 8 (App State) | 2-3 hours | Phase 1 |
| **Total** | **20-30 hours** | |

---

## Design Decisions Log

### Why Fluent APIs Over Custom Operators?
- C# only allows overloading fixed operators (+, -, *, /, ==)
- Cannot create custom operators like `>>` or `|>`
- Fluent APIs have full IntelliSense support
- Industry standard: LINQ, DOTween, FluentAssertions, Entity Framework

### Why Broadcast/Notify Naming?
- Matches original `MmInvoke(MmMethod.Xxx, ...)` pattern
- Direction is clear: Broadcast = down, Notify = up
- Autocomplete-friendly grouping
- Consistent with existing codebase conventions

### Why Responders Get Fluent API?
- Responders are primary message senders (39+ instances in codebase)
- Current pattern is verbose: `GetComponent<MmRelayNode>().MmInvoke(...)`
- Same API on both types reduces cognitive load
