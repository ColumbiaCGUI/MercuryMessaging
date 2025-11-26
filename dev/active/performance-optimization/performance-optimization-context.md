# Performance Optimization Context

**Last Updated:** 2025-11-25 (Evening Session)
**Status:** Phase 1 COMPLETE, Ready for Phase 2 or 3
**Full Plan:** `C:\Users\yangb\.claude\plans\typed-popping-puffin.md`

---

## Session Summary (2025-11-25)

### Completed This Session

1. **Assets Reorganization (All 7 Phases)** - Reduced 14 folders to 6:
   - Framework/, Platform/, Plugins/, Project/, Research/, Unity/
   - ~500MB controller art moved out of Resources
   - Commits: c5969e0e through d84b7297

2. **Performance Optimization Phase 1 (ObjectPool Integration)** - COMPLETE:
   - Created MmMessagePool.cs with ObjectPool<T> for all 13 message types
   - Created MmHashSetPool.cs for VisitedNodes pooling
   - Integrated pooling with MmRelayNode (all 13 typed MmInvoke overloads)
   - Added `_isPooled` flag to MmMessage for tracking
   - Created unit tests (MmMessagePoolTests.cs)
   - Commits: 902eab91, 2b2bc324

---

## Current Implementation State

### Phase 1: ObjectPool Integration ✅ COMPLETE

**What Was Implemented:**

1. **MmMessagePool** (`Protocol/Core/MmMessagePool.cs`):
   - 13 ObjectPool<T> instances (one per message type)
   - Type-safe getters: `GetInt()`, `GetString()`, `GetBool()`, etc.
   - `Return(MmMessage)` with type switch for proper routing
   - Reset on get: clears HopCount, VisitedNodes, NetId, _isPooled flag
   - Pool config: default 50, max 500 per type
   - Editor-only statistics method

2. **MmHashSetPool** (`Protocol/Core/MmHashSetPool.cs`):
   - Pool for `HashSet<int>` used by VisitedNodes
   - `Get()`, `Return()`, `GetCopy()` methods
   - Auto-clear on get
   - Pool config: default 100, max 1000

3. **MmMessage Changes** (`Protocol/Message/MmMessage.cs`):
   - Added `internal bool _isPooled` field (line ~102)
   - Used to track if message should be returned to pool

4. **MmRelayNode Changes** (`Protocol/MmRelayNode.cs`):
   - Line 568: `MmHashSetPool.Get()` instead of `new HashSet<int>()`
   - Lines 799-1012: All 13 typed MmInvoke overloads use pool:
     ```csharp
     // Example pattern (repeated for all types):
     public virtual void MmInvoke(MmMethod mmMethod, int param, MmMetadataBlock metadataBlock = null)
     {
         MmMessage msg = MmMessagePool.GetInt(param, mmMethod, metadataBlock);
         MmInvoke(msg);
         MmMessagePool.Return(msg);
     }
     ```

5. **Unit Tests** (`Tests/MmMessagePoolTests.cs`):
   - 20+ tests covering pool operations, reuse, reset behavior
   - Tests for both MmMessagePool and MmHashSetPool

---

## Key Decisions Made

1. **Pool Return Strategy**: Return messages immediately after typed MmInvoke completes
   - Simple and safe - no need for depth tracking
   - Works because typed overloads are the entry points

2. **_isPooled Flag**: Added to MmMessage for future use
   - Currently set by pool but not checked on return
   - Can be used for validation or conditional return

3. **No DSL Changes Needed**: MmFluentMessage delegates to MmRelayNode.MmInvoke
   - Pool integration automatic via the typed overloads

4. **Secondary Files Deferred**: MmMessageFactory.cs, MmRelayNodeExtensions.cs still use `new`
   - Lower priority - not in main hot path
   - Can be optimized later

---

## Files Modified This Session

### New Files Created:
- `Assets/Framework/MercuryMessaging/Protocol/Core/MmMessagePool.cs`
- `Assets/Framework/MercuryMessaging/Protocol/Core/MmHashSetPool.cs`
- `Assets/Framework/MercuryMessaging/Tests/MmMessagePoolTests.cs`

### Modified Files:
- `Assets/Framework/MercuryMessaging/Protocol/Message/MmMessage.cs` (added _isPooled)
- `Assets/Framework/MercuryMessaging/Protocol/MmRelayNode.cs` (pool integration)
- `dev/active/performance-optimization/performance-optimization-tasks.md`
- `documentation/OVERVIEW.md` (new folder structure)
- `dev/ASSETS_REORGANIZATION_PLAN.md` (marked complete)

---

## Known Issues / Blockers

1. **Unity Not Connected**: Could not verify compilation via MCP
   - Changes should compile but need Unity verification
   - Run tests: Window > General > Test Runner > PlayMode > Run All

2. **Secondary Files Not Updated**: These still use `new MmMessage*`:
   - `Protocol/DSL/MmMessageFactory.cs` (19 occurrences)
   - `Protocol/DSL/MmRelayNodeExtensions.cs` (6 occurrences)
   - `Protocol/DSL/MmTemporalExtensions.cs` (1 occurrence)

---

## Next Steps (Priority Order)

### Option A: Phase 3 - Serialize LINQ Removal (8-16h) - QUICK WIN
- Remove `.Concat().ToArray()` pattern from all 13 message types
- Use `Array.Copy()` with pre-sized arrays
- Key files: `Protocol/Message/MmMessage*.cs`
- Low risk, immediate impact on network serialization

### Option B: Phase 2 - O(1) Routing Tables (20-30h)
- Add Dictionary indices to MmRoutingTable
- Replace `List.Find()` with dictionary lookup
- Key file: `Protocol/MmRoutingTable.cs`

### Required Before Either:
1. Open Unity and verify compilation
2. Run existing tests to ensure no regressions
3. Run performance benchmark to establish baseline

---

## Competitive Context

Mercury is the ONLY framework with hierarchy-aware routing. Competitors (MessagePipe, R3, VitalRouter) focus on pub/sub but don't support scene-graph routing.

Current benchmarks:
- Mercury vs Direct Calls: 28x slower (target: ~3x after all phases)
- Mercury vs SendMessage: 2.6x slower (competitive)
- Mercury throughput: 98-980 msg/sec

---

## References

### Internal:
- `dev/active/networking/` - Network backend (Phase 0A/0B complete)
- `dev/active/dsl-overhaul/` - DSL improvements
- `dev/ASSETS_REORGANIZATION_PLAN.md` - Completed reorganization

### External:
- [MessagePipe](https://github.com/Cysharp/MessagePipe) - Zero-alloc pub/sub
- [Unity ObjectPool API](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html)

---

*Context document for performance optimization initiative*
*Last Updated: 2025-11-25 (Evening Session)*
