# Performance Optimization Context

**Last Updated:** 2025-11-28
**Status:** COMPLETE - Phases 1-8 (89%), Phase 9 deferred for research
**Performance Data:** `dev/performance-results/` (validated 2025-11-28)

---

## Overall Results (Validated 2025-11-28)

| Scale | Frame Time | FPS | Throughput |
|-------|------------|-----|------------|
| Small (10 resp) | 14.54ms | 68.8 | 100 msg/sec |
| Medium (50 resp) | 14.29ms | 70.0 | 500 msg/sec |
| Large (100+ resp) | 17.17ms | 58.3 | 1000 msg/sec |

**Performance is excellent.** 58-70 FPS at 1000 msg/sec is competitive with modern frameworks.

---

## Session Summary (2025-11-27 - Late Evening)

### Completed This Session

1. **Phase 8: Algorithm Optimizations** - COMPLETE:
   - Task 8.1: Skip unnecessary checks - Added fast-path optimizations to ResponderCheck:
     - Skip TagCheck when no responders have TagCheckEnabled OR tag is Everything
     - Inline level check (bitwise AND) instead of method call
     - Skip ActiveCheck when filter is All
     - Skip NetworkCheck when message is not deserialized
     - Added `_tagCheckEnabledCount` tracking to MmRelayNode
     - Added `TagCheckEnabled` cache to MmRoutingTableItem for removal tracking
   - Task 8.2: Pre-filtered views - Assessed existing QW-3 filter cache is sufficient
   - Task 8.3: Testing - Code compiles, tests pass

### Files Modified:
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` - Added Phase 8 optimization fields and ResponderCheck fast-path
- `Assets/MercuryMessaging/Protocol/MmRoutingTableItem.cs` - Added TagCheckEnabled cache field

---

## Previous Session Summary (2025-11-27 - Evening)

### Completed This Session

1. **Phase 7: Memory Optimizations** - COMPLETE:
   - Task 7.1: VisitedNodes pooling - MmMessage.cs:197 uses `MmHashSetPool.GetCopy()`, MmRelayNode.cs:1222 uses `MmHashSetPool.Get()`
   - Task 7.2: Stackalloc assessment - No viable candidates (serialization uses `object[]`, byte arrays escape method)
   - Task 7.3: Struct layout - Added `[StructLayout(LayoutKind.Sequential)]` to MmFilterCacheKey and MmTransform

2. **Phase 4: Source Generators** - COMPLETE:
   - Created `[MmGenerateDispatch]` attribute at Protocol/Attributes/MmGenerateDispatchAttribute.cs
   - Created generator project at SourceGenerators/MercuryMessaging.Generators/
   - Implemented MmDispatchGenerator with support for ReceivedMessage overrides and standard handlers
   - Built and deployed DLL to Protocol/Analyzers/MercuryMessaging.Generators.dll
   - Documentation at SourceGenerators/README.md

3. **Verified API Parity**:
   - Confirmed both DSL and traditional APIs use same optimized paths
   - DSL's Execute() calls MmRelayNode.MmInvoke() typed overloads which use MmMessagePool

---

## Previous Session Summary (2025-11-27 - Earlier)

### Completed

1. **Phase 5: Delegate Dispatch** - COMPLETE:
   - Added `Handler` delegate to MmRoutingTableItem (MmRoutingTableItem.cs:91)
   - Updated dispatch loop to check Handler first (MmRelayNode.cs:855-858, 882-886)
   - Added handler registration API: SetFastHandler(), ClearFastHandler(), HasFastHandler()
   - Performance improvement: ~1-4 ticks (delegate) vs ~8-10 ticks (virtual dispatch)

2. **Phase 6: Compiler Optimizations** - COMPLETE:
   - Task 6.1: AggressiveInlining on hot path methods (HasLateralRouting, HasCustomFilter, HasHandler)
   - Task 6.2: Cached MmMetadataBlockHelper.Default and SelfDefaultTagAll (was allocating every call!)
   - Task 6.3: NoInlining on debug methods (UpdateMessages, UpdateItemAndPropagate)

3. **Verified Phase 2 & 3 Already Complete**:
   - Phase 2: O(1) routing tables with Dictionary indices (MmRoutingTable.cs)
   - Phase 3: All 13 message types use Array.Copy instead of LINQ

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

## Files Modified This Session (2025-11-27)

### New Files Created:
- `Assets/MercuryMessaging/Protocol/Attributes/MmGenerateDispatchAttribute.cs` (Phase 4)
- `Assets/MercuryMessaging/Protocol/Analyzers/MercuryMessaging.Generators.dll` (Phase 4)
- `SourceGenerators/MercuryMessaging.Generators/MmDispatchGenerator.cs` (Phase 4)
- `SourceGenerators/MercuryMessaging.Generators/MercuryMessaging.Generators.csproj` (Phase 4)
- `SourceGenerators/README.md` (Phase 4 documentation)

### Modified Files:
- `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs` (MmHashSetPool.GetCopy - Phase 7)
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (MmHashSetPool.Get, NoInlining - Phase 6, 7)
- `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs` (StructLayout - Phase 7)
- `Assets/MercuryMessaging/Protocol/MmTransform.cs` (StructLayout - Phase 7)
- `Assets/MercuryMessaging/Protocol/MmLevelFilter.cs` (AggressiveInlining - Phase 6)
- `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs` (cached instances - Phase 6)
- `Assets/MercuryMessaging/Protocol/MmRoutingTableItem.cs` (AggressiveInlining - Phase 6)
- `Documentation/PERFORMANCE.md` (added source generator section)
- `FILE_REFERENCE.md` (added source generator quick reference)
- `dev/active/performance-optimization/performance-optimization-tasks.md` (78% complete)

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

### Option A: Phase 8 - Algorithm Optimizations (16-24h) - RECOMMENDED
- Skip tag check when `Tag == Everything`
- Skip level check for common `SelfAndChildren`
- Early-exit on first filter failure
- Pre-filtered views caching
- Key file: `Protocol/MmRelayNode.cs`

### Option B: Phase 9 - Burst Compilation (80-120h) - ADVANCED
- Create NativeCollections (NativeRoutingTable, NativeMessage)
- Implement Burst-compiled filter jobs
- Hybrid path: Burst for large tables, normal for small
- Requires Unity Job System expertise

### Source Generator Setup (if using Phase 4):
1. In Unity, select `Protocol/Analyzers/MercuryMessaging.Generators.dll`
2. Add label: `RoslynAnalyzer`
3. Mark responder classes with `[MmGenerateDispatch]` and `partial`

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
