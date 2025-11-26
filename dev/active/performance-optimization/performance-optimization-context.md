# Performance Optimization Context

**Last Updated:** 2025-11-25
**Full Plan:** `C:\Users\yangb\.claude\plans\typed-popping-puffin.md`

---

## Analysis Summary

This document captures the comprehensive performance analysis of MercuryMessaging compared to 10+ competing Unity frameworks, identifying specific bottlenecks and optimization strategies.

---

## Competitive Landscape

### Framework Comparison

| Framework | Stars | Zero Alloc | Hierarchy Routing | Network | FSM |
|-----------|-------|------------|-------------------|---------|-----|
| **MessagePipe** | 1.7k | ✅ | ❌ | IPC only | ❌ |
| **R3** | 3.4k | ✅ | ❌ | ❌ | ❌ |
| **VitalRouter** | 319 | ✅ | ❌ | ❌ | ❌ |
| **UniTask** | 10.2k | ✅ | ❌ | ❌ | ❌ |
| **VContainer** | 2.6k | ✅ | ❌ | ❌ | ❌ |
| **MercuryMessaging** | N/A | ❌ | ✅ | ✅ | ✅ |

**Key Finding:** Mercury is the ONLY framework with hierarchy-aware routing. No competitor offers scene-graph-based message propagation with multi-level filtering.

---

## Current Performance Bottlenecks

### Bottleneck 1: Serialize().Concat().ToArray() (HIGH)
**Location:** All 13 message type files
**Impact:** LINQ allocation on every network send

```csharp
// Current (allocates):
return baseSerialized.Concat(thisSerialized).ToArray();

// Fixed (pre-sized):
object[] combined = new object[baseSerialized.Length + 1];
Array.Copy(baseSerialized, combined, baseSerialized.Length);
combined[baseSerialized.Length] = value;
return combined;
```

### Bottleneck 2: RoutingTable.Find() (HIGH)
**Location:** `MmRoutingTable.cs` lines 196, 217, 322
**Impact:** O(n) linear search in hot path

```csharp
// Current (O(n)):
return _list.Find(item => item.Name == name);

// Fixed (O(1)):
return _nameIndex.TryGetValue(name, out var item) ? item : null;
```

### Bottleneck 3: Message.Copy() HashSet duplication (MEDIUM)
**Location:** `MmMessage.cs` lines 189-191
**Impact:** Cascading allocations through hierarchy

```csharp
// Current:
VisitedNodes = new HashSet<int>(message.VisitedNodes);

// Fixed (pool):
VisitedNodes = MmHashSetPool.Get();
foreach (var id in message.VisitedNodes) VisitedNodes.Add(id);
```

### Bottleneck 4: VisitedNodes allocation (MEDIUM)
**Location:** `MmRelayNode.cs` line 568
**Impact:** New HashSet per message

### Bottleneck 5: Advanced routing collections (MEDIUM)
**Location:** `MmRelayNode.cs` lines 1641-1692
**Impact:** Temporary List allocations for routing targets

---

## Optimization Strategies

### Strategy 1: Unity ObjectPool<T>
**Requirement:** Unity 2021.1+ (built-in)
**Expected:** 80-90% allocation reduction

Unity's built-in `UnityEngine.Pool.ObjectPool<T>` provides:
- Stack-based storage
- Lifecycle callbacks (create, get, release, destroy)
- Capacity management

### Strategy 2: Dictionary Indices for O(1) Lookup
**Expected:** Eliminates O(n) routing table scans

Add secondary indices alongside existing List:
- `Dictionary<string, MmRoutingTableItem>` for name lookup
- `Dictionary<MmResponder, MmRoutingTableItem>` for responder lookup

### Strategy 3: Delegate-Based Dispatch
**Expected:** 2-4x faster dispatch (eliminates virtual + switch)

Add optional `Action<MmMessage>` handler to `MmRoutingTableItem`:
- Fast path: Direct delegate invocation (~1-4 ticks)
- Slow path: Virtual MmInvoke fallback (~8-10 ticks)

### Strategy 4: Compiler Optimizations
**Expected:** 10-20% improvement on hot methods

- `[MethodImpl(AggressiveInlining)]` on small hot methods
- `readonly struct` for MmMetadataBlock
- Hot/cold path separation

### Strategy 5: Source Generators (Optional)
**Requirement:** Unity 2021.3+
**Expected:** 3-6x improvement in dispatch

Generate optimized switch statements at compile time, eliminating runtime type checking.

---

## Performance Ceiling Analysis

### Theoretical Minimum Operations (Mercury)
1. Pool lookup (get message)
2. Metadata assignment
3. Routing table lookup
4. Level filter check
5. Tag filter check
6. Virtual dispatch
7. Hop tracking
8. Pool return

**Result:** ~8 operations minimum = ~3x slower than direct calls is the theoretical floor

### Why Mercury Can't Beat Direct Calls
Direct calls: `responder.Method(value)` = 1 virtual method call

Mercury adds: routing table iteration + filtering + dispatch. This is the **feature**, not a bug—you're paying for loose coupling and hierarchical routing.

---

## DSL Integration

The Fluent DSL and performance optimizations are **synergistic**:

1. `MmFluentMessage` is already a **struct** (no heap allocation)
2. DSL already uses **pooling** for predicates
3. DSL provides **single integration point** (`Execute()`) for message pooling

```csharp
// Integration in MmFluentMessage.Execute():
var message = MmMessagePool.Get(_method, _payload);  // Use pool
_relay.MmInvoke(message, BuildMetadata());
// Pool.Return() called automatically at end of routing
```

---

## Networking Coordination

The networking task (`dev/active/networking/`) shares serialization paths:

- **Phase 0B Complete:** MmBinarySerializer already implemented
- **Coordination Point:** Phase 3 (Serialize() fix) affects both local and network paths
- **Key File:** `Protocol/Network/MmBinarySerializer.cs`

Note: MmBinarySerializer uses different serialization (binary) than MmMessage.Serialize() (object[]). Phase 3 affects the legacy object[] serialization.

---

## Assets Reorganization Coordination

The assets reorganization plan (`dev/ASSETS_REORGANIZATION_PLAN.md`) includes Phase 3.5:

**New folders for performance code:**
```
Assets/Framework/MercuryMessaging/Runtime/
├── Protocol/
│   └── Core/              # NEW: MmMessagePool, MmHashSetPool
├── StandardLibrary/       # NEW: For DSL Phase 9-11
│   ├── UI/
│   └── Input/
```

**Recommendation:** Execute reorganization Phases 1-3.5 BEFORE implementing performance optimizations for clean paths.

---

## Benchmark References

### Unity Built-ins (Jackson Dunstan benchmarks)
- C# Event: ~178 ticks (baseline)
- UnityEvent: ~1,482 ticks (8x slower)
- SendMessage: ~42,248 ticks (237x slower)

### MessagePipe (Cysharp benchmarks)
- 78x faster than Prism EventAggregator
- Zero allocation per publish

### Mercury (Current)
- ~4,984 ticks (~28x slower than C# events)
- 3-5 allocations per message

### Mercury (Target after Phase 1-5)
- ~1,000-1,500 ticks (~5-8x slower than C# events)
- 0-1 allocations per message
- FASTER than SendMessage by 2-5x

---

## Research Opportunities

The performance optimization work supports several research initiatives:

| Initiative | Relevance | Publication Target |
|------------|-----------|-------------------|
| **User Study** | Validates usability + performance | CHI 2025 |
| **Parallel Dispatch** | Builds on delegate-based dispatch | UIST 2025 |
| **Visual Composer** | Performance is table stakes | UIST 2025 |

---

## References

### External
- [MessagePipe GitHub](https://github.com/Cysharp/MessagePipe)
- [R3 GitHub](https://github.com/Cysharp/R3)
- [Unity ObjectPool API](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html)
- [Jackson Dunstan Event Benchmarks](https://www.jacksondunstan.com/articles/3335)

### Internal
- `dev/active/networking/` - Network backend integration
- `dev/active/dsl-overhaul/` - DSL Execute() integration
- `dev/ASSETS_REORGANIZATION_PLAN.md` - Folder structure (Phase 3.5)
- `Documentation/Performance/PERFORMANCE_REPORT.md` - Initial analysis
- `Documentation/Performance/OPTIMIZATION_RESULTS.md` - QW-1 through QW-6 results

---

*Context document for performance optimization initiative*
*Created: 2025-11-25*
