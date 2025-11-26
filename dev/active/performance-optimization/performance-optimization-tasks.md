# Performance Optimization Tasks

**Last Updated:** 2025-11-25
**Full Plan:** `C:\Users\yangb\.claude\plans\typed-popping-puffin.md`

---

## Phase 1: ObjectPool Integration (40-60h) ⬜ NOT STARTED

### Task 1.1: Create MmMessagePool (16h) [ ]
- [ ] Create `Protocol/MmMessagePool.cs`
- [ ] Implement `ObjectPool<T>` for each of 13 message types
- [ ] Add typed getters: `GetInt()`, `GetString()`, `GetBool()`, etc.
- [ ] Add `Return(MmMessage)` with type switch
- [ ] Configure pool sizes (default: 50, max: 500)

### Task 1.2: Create MmHashSetPool (4h) [ ]
- [ ] Create `Protocol/MmHashSetPool.cs`
- [ ] Pool `HashSet<int>` for VisitedNodes
- [ ] Add `Get()` and `Return()` methods
- [ ] Add `Clear()` on get

### Task 1.3: Integrate with MmRelayNode (16h) [ ]
- [ ] Modify `MmInvoke()` to use pooled messages
- [ ] Add `isRootInvocation` tracking
- [ ] Call `MmMessagePool.Return()` at end of routing
- [ ] Handle network messages (don't return deserialized)

### Task 1.4: Integrate with DSL Execute() (8h) [ ]
- [ ] Modify `MmFluentMessage.Execute()` to use pool
- [ ] Update `CreateMessage()` helper methods
- [ ] Ensure pool return handled correctly

### Task 1.5: Testing (8h) [ ]
- [ ] Unit tests for MmMessagePool
- [ ] Unit tests for MmHashSetPool
- [ ] Integration tests with routing
- [ ] Performance benchmark comparison

---

## Phase 2: O(1) Routing Tables (20-30h) ⬜ NOT STARTED

### Task 2.1: Add Dictionary Indices (8h) [ ]
- [ ] Add `_nameIndex: Dictionary<string, MmRoutingTableItem>`
- [ ] Add `_responderIndex: Dictionary<MmResponder, MmRoutingTableItem>`
- [ ] Modify `Add()` to update all indices
- [ ] Modify `Remove()` to update all indices

### Task 2.2: Update Lookup Methods (4h) [ ]
- [ ] Modify `this[string name]` to use `_nameIndex`
- [ ] Modify `this[MmResponder]` to use `_responderIndex`
- [ ] Modify `ContainsKey()` to use dictionary
- [ ] Modify `Contains(responder)` to use dictionary

### Task 2.3: Handle Index Invalidation (8h) [ ]
- [ ] Clear indices on `Clear()`
- [ ] Update indices on name change
- [ ] Handle edge cases (null names, duplicate names)

### Task 2.4: Testing (8h) [ ]
- [ ] Unit tests for all lookup methods
- [ ] Performance benchmark for routing tables of 10/50/100 items
- [ ] Regression tests for existing functionality

---

## Phase 3: Serialize() LINQ Removal (8-16h) ⬜ NOT STARTED

### Task 3.1: Update Base MmMessage (2h) [ ]
- [ ] Modify `MmMessage.Serialize()` to use `Array.Copy`
- [ ] Remove `.Concat().ToArray()` pattern

### Task 3.2: Update All Message Types (8h) [ ]
Files to modify (13 total):
- [ ] `MmMessageBool.cs`
- [ ] `MmMessageInt.cs`
- [ ] `MmMessageFloat.cs`
- [ ] `MmMessageString.cs`
- [ ] `MmMessageVector3.cs`
- [ ] `MmMessageVector4.cs`
- [ ] `MmMessageQuaternion.cs`
- [ ] `MmMessageTransform.cs`
- [ ] `MmMessageTransformList.cs`
- [ ] `MmMessageByteArray.cs`
- [ ] `MmMessageSerializable.cs`
- [ ] `MmMessageGameObject.cs`
- [ ] `MmMetadataBlock.Serialize()`

### Task 3.3: Testing (4h) [ ]
- [ ] Unit tests for each Serialize() method
- [ ] Round-trip serialization/deserialization tests
- [ ] Verify network compatibility

---

## Phase 4: Source Generators (80-120h) ⬜ NOT STARTED

### Task 4.1: Generator Infrastructure (24h) [ ]
- [ ] Create generator project (Microsoft.CodeAnalysis.CSharp 4.3)
- [ ] Set up Unity integration (RoslynAnalyzer label)
- [ ] Create `[MmGenerateDispatch]` attribute

### Task 4.2: Dispatch Generator (40h) [ ]
- [ ] Analyze responder classes for ReceivedMessage overrides
- [ ] Generate optimized MmInvoke switch
- [ ] Handle inheritance correctly

### Task 4.3: Testing & Documentation (32h) [ ]
- [ ] Unit tests for generator output
- [ ] Integration tests with actual responders
- [ ] Documentation and examples

---

## Phase 5: Delegate Dispatch (20-40h) ⬜ NOT STARTED

### Task 5.1: Add Handler to MmRoutingTableItem (4h) [ ]
- [ ] Add `public Action<MmMessage> Handler;`
- [ ] Add typed handlers dictionary (optional)

### Task 5.2: Update Dispatch Loop (8h) [ ]
- [ ] Check `item.Handler != null` before `MmInvoke`
- [ ] Call handler directly if available
- [ ] Fall back to virtual dispatch

### Task 5.3: Handler Registration API (8h) [ ]
- [ ] Add `RegisterHandler<T>(Action<T>)` to responder base
- [ ] Auto-register in Awake() pattern
- [ ] Document usage

### Task 5.4: Testing (8h) [ ]
- [ ] Unit tests for delegate dispatch
- [ ] Performance benchmarks vs virtual dispatch
- [ ] Backward compatibility tests

---

## Phase 6: Compiler Optimizations (8-16h) ⬜ NOT STARTED

### Task 6.1: Aggressive Inlining (4h) [ ]
- [ ] Add `[MethodImpl(AggressiveInlining)]` to:
  - [ ] Filter check methods
  - [ ] Tag matching
  - [ ] Level filter helpers
  - [ ] Small property getters

### Task 6.2: Readonly Struct (4h) [ ]
- [ ] Convert `MmMetadataBlock` to `readonly struct`
- [ ] Update all usages to use `in` parameter
- [ ] Remove defensive copies

### Task 6.3: Hot/Cold Path Separation (4h) [ ]
- [ ] Extract debug code to `[NoInlining]` methods
- [ ] Ensure PerformanceMode check is branch-predicted

---

## Phase 7: Memory Optimizations (16-24h) ⬜ NOT STARTED

### Task 7.1: VisitedNodes Pool (4h) [ ]
- [ ] Use MmHashSetPool in MmMessage.Copy()
- [ ] Ensure proper return on message disposal

### Task 7.2: Stackalloc for Small Arrays (8h) [ ]
- [ ] Identify small temporary array allocations
- [ ] Replace with `Span<T>` + `stackalloc` where safe
- [ ] Document memory limits

### Task 7.3: Struct Layout (4h) [ ]
- [ ] Add `[StructLayout(LayoutKind.Sequential)]` to hot structs
- [ ] Order fields for cache line efficiency
- [ ] Measure cache hit improvement

---

## Phase 8: Algorithm Optimizations (16-24h) ⬜ NOT STARTED

### Task 8.1: Skip Unnecessary Checks (8h) [ ]
- [ ] Skip tag check when `Tag == Everything`
- [ ] Skip level check for common `SelfAndChildren`
- [ ] Early-exit on first filter failure

### Task 8.2: Pre-filtered Views (8h) [ ]
- [ ] Cache filtered lists for common filter combinations
- [ ] Invalidate on routing table change
- [ ] Measure hit rate

### Task 8.3: Testing (8h) [ ]
- [ ] Verify correct filtering behavior
- [ ] Performance benchmarks
- [ ] Edge case testing

---

## Phase 9: Burst Compilation (80-120h) ⬜ NOT STARTED

### Task 9.1: NativeCollections (24h) [ ]
- [ ] Create `NativeRoutingTable` with NativeHashMap
- [ ] Create `NativeMessage` struct for Burst

### Task 9.2: Filter Jobs (32h) [ ]
- [ ] Create `FilterJob : IJobParallelFor` with `[BurstCompile]`
- [ ] Implement parallel filter evaluation
- [ ] Handle result collection

### Task 9.3: Integration (32h) [ ]
- [ ] Hybrid path: Burst for large tables, normal for small
- [ ] Main thread synchronization
- [ ] Unity Job System scheduling

### Task 9.4: Testing (32h) [ ]
- [ ] Correctness tests
- [ ] Performance benchmarks at scale (100+ responders)
- [ ] Memory safety validation

---

## Quick Wins (Can Do Anytime)

| Task | Hours | Impact | Status |
|------|-------|--------|--------|
| Pool VisitedNodes HashSet | 2h | Medium | [ ] |
| Reuse MetadataBlock.Default | 1h | Low | [ ] |
| Cache stateless messages (Initialize, Refresh) | 4h | Medium | [ ] |
| Pre-size Serialize arrays | 8h | High | [ ] |

---

## Progress Summary

| Phase | Status | Hours Est | Hours Used |
|-------|--------|-----------|------------|
| 1 | ⬜ Not Started | 40-60h | 0h |
| 2 | ⬜ Not Started | 20-30h | 0h |
| 3 | ⬜ Not Started | 8-16h | 0h |
| 4 | ⬜ Not Started | 80-120h | 0h |
| 5 | ⬜ Not Started | 20-40h | 0h |
| 6 | ⬜ Not Started | 8-16h | 0h |
| 7 | ⬜ Not Started | 16-24h | 0h |
| 8 | ⬜ Not Started | 16-24h | 0h |
| 9 | ⬜ Not Started | 80-120h | 0h |

**Total Progress:** 0/9 phases complete (0%)

---

## Dependencies

```
Phase 1 (ObjectPool) ─────┐
                          ├──► Phase 4 (Source Gen) - optional
Phase 2 (O(1) Routing) ───┤
                          ├──► Phase 5 (Delegates)
Phase 3 (Serialize) ──────┘
                              │
                              ▼
                          Phase 6-8 (Polish)
                              │
                              ▼
                          Phase 9 (Burst) - requires Phase 5
```

---

*Task checklist for performance optimization - Updated 2025-11-25*
