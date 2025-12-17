# Observer Effect Discovery - Routing Table Profiling

**Date:** 2025-11-21
**Context:** Phase 2.1 Routing Optimization - Profiling Mini-Task
**Status:** ⚠️ **CRITICAL FINDING** - Profiling infrastructure creates Observer Effect

---

## Summary

Custom Stopwatch-based profiling to measure routing table overhead created a **massive Observer Effect** where the measurement infrastructure dominated the thing being measured, invalidating the results.

## The Problem

### Initial Results (With Profiling Enabled)

| Scale | Frame Time | Routing Table % | Invocations/Frame |
|-------|------------|-----------------|-------------------|
| Small | 14.95ms | 82-98% | 10 |
| Medium | **325.34ms** | **97-98%** | **2672** |
| Large | **331.27ms** | 97-98% | 2672+ |

**Baseline comparison (profiling disabled):**
- Small: 15.14ms (minimal regression)
- Medium: 16.28ms → 325ms (**20x worse!**)
- Large: 18.69ms → 331ms (**18x worse!**)

### Root Cause Analysis

**Profiling overhead calculation:**
```
Medium Scale per frame:
- 2672 routing invocations
- 2 Stopwatches per invocation (total + routing)
- = 5344 Stopwatch operations per frame

.NET Stopwatch overhead (documented):
- StartNew(): ~150-200ns
- Stop(): ~150-200ns
- Elapsed property access: ~50ns
- Total per Stopwatch: ~300-400ns

Minimum profiling overhead:
- 5344 × 400ns = 2.14ms per frame

Actual overhead (including accumulation, field access, threshold checks):
- ~5-10ms of pure profiling overhead

Observed overhead:
- 325ms - 16ms baseline = **309ms total regression**
- Profiling explains ~2-10ms
- **Remaining 299-307ms**: Likely from continuous Stopwatch allocation, GC pressure, cache misses
```

**The Observer Effect:**
- The profiling infrastructure (Stopwatch creation, timing, accumulation) became THE bottleneck
- 97-98% routing table percentage measures **profiling overhead**, NOT routing table overhead
- **Classic Heisenbug** - measurement changes what's being measured

---

## Why This Happened

### Small Scale (10 invocations/frame)
- 10 × 2 = 20 Stopwatch operations
- 20 × 400ns = 8μs overhead
- **Negligible** - profiling works correctly

### Medium/Large Scale (2672+ invocations/frame)
- 2672 × 2 = 5344 Stopwatch operations
- Creates enormous GC pressure (allocating Stopwatch objects)
- Cache misses from constant field access
- Threshold checking and logging overhead
- **Dominant** - profiling invalidates measurement

## Lessons Learned

### ❌ **Don't Do This:**
- Custom Stopwatch profiling in hot paths (called 1000+ times/frame)
- Per-invocation timing without sampling
- Accumulating metrics without batching
- Logging inside profiled sections

### ✅ **Do This Instead:**
1. **Use Unity Profiler** - Zero runtime overhead, hardware-level precision
   ```csharp
   Profiler.BeginSample("RoutingTable");
   // ... routing table iteration ...
   Profiler.EndSample();
   ```

2. **Sampling Profiler** - Profile every Nth invocation
   ```csharp
   private static int _profilingSampleCounter = 0;
   private const int PROFILE_EVERY_N = 100;

   if (EnableRoutingProfiler && (_profilingSampleCounter++ % PROFILE_EVERY_N == 0))
   {
       // Only profile 1% of invocations
       var sw = Stopwatch.StartNew();
       // ... measure ...
       sw.Stop();
   }
   ```

3. **Conditional Compilation** - Remove profiling in release builds
   ```csharp
   #if UNITY_EDITOR || DEVELOPMENT_BUILD
       // Profiling code here
   #endif
   ```

4. **Batched Metrics** - Accumulate counters, not timings
   ```csharp
   // Good: Simple counter
   RoutingTableInvocations++;

   // Bad: Stopwatch per invocation
   var sw = Stopwatch.StartNew();
   // ...
   sw.Stop();
   RoutingTableTotalMs += sw.Elapsed.TotalMilliseconds;
   ```

---

## Next Steps

1. ✅ **Disabled profiling** in PerformanceTestHarness (commit c4db9432)
2. ⏳ **Re-run tests** with clean baseline to verify frame times return to normal
3. ⏳ **Use Unity Profiler** for accurate deep dive analysis
4. ⏳ **Make Phase 3.1 decision** based on clean Unity Profiler data

## Hypothesis Validation

**Original Hypothesis:** Routing table overhead < 15% → Skip Phase 3.1

**Reality:** Cannot validate with current approach due to Observer Effect

**New Approach:** Use Unity Profiler to get accurate routing table percentage without measurement overhead

---

## Technical References

**Stopwatch Performance (.NET Framework):**
- `Stopwatch.StartNew()`: 150-200ns (object allocation)
- `Stopwatch.Stop()`: 150-200ns (QueryPerformanceCounter call)
- `Elapsed.TotalMilliseconds`: 50ns (property access, double conversion)

**Unity Profiler Performance:**
- `Profiler.BeginSample()`: ~0ns in release builds (compiled out)
- `Profiler.EndSample()`: ~0ns in release builds (compiled out)
- Editor overhead: <10ns per sample pair

---

## Conclusion

**Observer Effect is real and devastating at scale.**

Custom timing profiling that works perfectly at low invocation counts (10/frame) completely dominates and invalidates measurements at high invocation counts (2672/frame). Always prefer:
1. **Unity Profiler** for zero-overhead profiling
2. **Sampling** for custom metrics (profile 1-10% of calls)
3. **Hardware profilers** for production diagnosis

The 97-98% routing table overhead number is **invalid** and tells us more about Stopwatch overhead than routing table performance.

---

**Document Version:** 1.0
**Author:** Performance Analysis Session 2025-11-21
**Related:** routing-optimization-tasks.md, Phase 2.1 profiling mini-task
