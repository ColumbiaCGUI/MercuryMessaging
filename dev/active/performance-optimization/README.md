# Performance Optimization Initiative

**Status:** APPROVED - Ready for Implementation
**Created:** 2025-11-25
**Priority:** HIGH
**Estimated Total:** 200-300 hours (~5-8 weeks)
**Plan File:** `C:\Users\yangb\.claude\plans\typed-popping-puffin.md`

---

## Overview

9-phase performance optimization initiative to improve Mercury from 28x slower than direct calls to ~3x slower, while maintaining full hierarchical routing capabilities.

### Key Targets
- Beat SendMessage: 2-5x FASTER (currently 2.6x slower)
- Match UnityEvent: ~equal (currently 28x slower)
- Approach MessagePipe: 2x slower (currently 15x slower)

---

## Phase Summary

| Phase | Hours | Target vs Direct | Description |
|-------|-------|------------------|-------------|
| 1 | 40-60h | 19x slower | ObjectPool integration |
| 2 | 20-30h | 12x slower | O(1) routing tables |
| 3 | 8-16h | 12x slower | Serialize() LINQ removal |
| 4 | 80-120h | 10x slower | Source generators |
| 5 | 20-40h | 5x slower | Delegate dispatch |
| 6 | 8-16h | 4x slower | Compiler optimizations |
| 7 | 16-24h | 3.5x slower | Memory optimizations |
| 8 | 16-24h | 3x slower | Algorithm optimizations |
| 9 | 80-120h | ~2x slower | Burst compilation |
| 10 | 4-8h | ~2x slower | Thread safety (async/await support) |

**Note:** Phase 10 (Thread Safety) merged from `dev/active/thread-safety/` on 2025-11-27.
Only implement when async/await messaging is needed.

---

## Current Performance Baseline

| Metric | Current Value |
|--------|---------------|
| vs Direct Calls | 28x slower |
| vs SendMessage | 2.6x slower |
| vs UnityEvent | 28x slower |
| vs MessagePipe | ~15x slower |
| Frame Time | 15-19ms (53-66 FPS) |
| Throughput | 98-980 msg/sec |
| Memory | Stable 925-940 MB |

---

## Critical Files

### To Modify
- `Protocol/MmRelayNode.cs` - Pool integration, delegate dispatch
- `Protocol/MmRoutingTable.cs` - O(1) lookup indices
- `Protocol/Message/*.cs` - All 13 message types for Serialize() fix
- `Protocol/MmRoutingTableItem.cs` - Add Handler delegate
- `Protocol/MmMetadataBlock.cs` - readonly struct conversion
- `Protocol/DSL/MmFluentMessage.cs` - Execute() pool integration

### To Create
- `Protocol/MmMessagePool.cs` - ObjectPool implementation
- `Protocol/MmHashSetPool.cs` - VisitedNodes pooling
- `Tests/PerformanceOptimizationTests.cs` - Regression tests

---

## Related Tasks

| Task | Location | Relevance |
|------|----------|-----------|
| **Networking** | `dev/active/networking/` | Serialization paths shared |
| **DSL Overhaul** | `dev/active/dsl-overhaul/` | Execute() integration point |
| **Assets Reorganization** | `dev/ASSETS_REORGANIZATION_PLAN.md` | Phase 3.5 adds Core/ folder |

---

## Key Optimizations Summary

### Phase 1: ObjectPool (80-90% allocation reduction)
```csharp
// Uses Unity's built-in ObjectPool<T> (2021.1+)
var msg = MmMessagePool.GetInt(42, metadata);
// ... routing ...
MmMessagePool.Return(msg);
```

### Phase 2: O(1) Routing Tables
```csharp
// Replace List.Find() with Dictionary lookup
private Dictionary<string, MmRoutingTableItem> _nameIndex;
public MmRoutingTableItem this[string name] => _nameIndex.TryGetValue(name, out var item) ? item : null;
```

### Phase 5: Delegate Dispatch
```csharp
// Add direct delegate invocation path
if (item.Handler != null)
    item.Handler(message);     // Fast: 1-4 ticks
else
    item.Responder.MmInvoke(message); // Slow: 8-10 ticks
```

---

## Expected Outcomes

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| vs Direct Calls | 28x slower | ~3x slower | **9x faster** |
| vs SendMessage | 2.6x slower | 2-5x FASTER | **5-13x faster** |
| Allocations/msg | 3-5 objects | 0-1 objects | **80-90% reduction** |
| Throughput | 980 msg/sec | 2000+ msg/sec | **2x throughput** |

---

## Next Steps

1. Read `performance-optimization-context.md` for full technical details
2. Check `performance-optimization-tasks.md` for implementation checklist
3. Coordinate with `dev/ASSETS_REORGANIZATION_PLAN.md` Phase 3.5 for folder structure
4. Begin with Phase 1 (ObjectPool) as highest priority

---

*Last Updated: 2025-11-25*
