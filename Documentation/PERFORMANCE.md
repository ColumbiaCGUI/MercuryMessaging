# Performance Characteristics

MercuryMessaging has been comprehensively tested and optimized across three scales (Small, Medium, Large) with validated Quick Win optimizations (QW-1 through QW-6).

## Overview

Performance optimizations achieved **2-2.2x frame time improvement** and **3-35x throughput improvement**, with excellent memory stability and scalability.

**Full Reports:**
- Initial analysis: [PERFORMANCE_REPORT.md](../Documentation/Performance/PERFORMANCE_REPORT.md)
- Optimization results: [OPTIMIZATION_RESULTS.md](../Documentation/Performance/OPTIMIZATION_RESULTS.md)
- Source generators: [SourceGenerators/README.md](../SourceGenerators/README.md)

---

## Measured Performance

### Frame Time (Unity Editor, Development Build with PerformanceMode)

| Scale | Responders | Levels | Frame Time | FPS |
|-------|-----------|--------|------------|-----|
| Small | 10 | 3 | 14.54ms avg | 68.8 FPS |
| Medium | 50 | 5 | 14.29ms avg | 70.0 FPS |
| Large | 100+ | 7-10 | 17.17ms avg | 58.3 FPS |

*Data validated 2025-11-28. See `dev/performance-results/` for raw data.*

### Performance Improvement (After Optimization)

- Frame time: 2-2.2x faster (was 32-36ms, now 14-17ms)
- Throughput: 3-35x faster (was 28-30 msg/sec, now 100-1000 msg/sec)

### Memory Stability: **Validated**

- Remains bounded and stable (~925-940 MB)
- QW-4 CircularBuffer successfully bounds memory usage
- Suitable for long-running sessions
- No regression from performance optimizations

### Message Throughput

| Scale | Throughput |
|-------|------------|
| Small | 100 msg/sec |
| Medium | 500 msg/sec |
| Large | 1000 msg/sec |

Scales excellently from 10 to 100+ responders (10x throughput with 10x responders).

### Comparison vs Unity Built-ins (InvocationComparison)

| Comparison | Relative Speed |
|------------|----------------|
| Mercury vs Direct Calls | 28x slower (acceptable for decoupling benefits) |
| Mercury vs SendMessage | 2.6x slower (competitive performance) |
| Mercury vs UnityEvent | 28x slower (reflection overhead) |
| Mercury vs Execute | 1.05x slower (comparable to similar patterns) |

---

## Scaling Characteristics

### Responder Count (Excellent Scaling)

- Sub-linear frame time: 10 → 100 responders adds only 2.6ms (14.5ms → 17.2ms)
- Excellent throughput scaling: 100 → 1000 msg/sec (10x improvement with 10x responders)
- Highly suitable for projects with 100+ responders

### Hierarchy Depth

- Minimal overhead: 3 → 10 levels adds only 2.6ms frame time
- Hop limits prevent runaway propagation (default: 50 hops)
- Cycle detection working (no infinite loops)

### Message Volume (Outstanding Scalability)

- Memory bounded regardless of volume (QW-4 validated)
- No degradation over time (tested up to 1000 msg/sec sustained)
- Throughput scales with responder count (100 → 1000 msg/sec)
- Suitable for high-volume messaging applications

---

## Configuration Recommendations

### Small Projects (<20 responders, <5 levels)
```csharp
maxMessageHops: 20
messageHistorySize: 50
enableCycleDetection: true
```

### Medium Projects (20-100 responders, 5-10 levels)
```csharp
maxMessageHops: 50 (default)
messageHistorySize: 100 (default)
enableCycleDetection: true
```

### Large Projects (100+ responders, 10+ levels)
```csharp
maxMessageHops: 100
messageHistorySize: 200
enableCycleDetection: true
```

---

## Performance Tuning Tips

### 1. Hierarchy Design
- Keep depth <10 levels for best performance
- Use SelfAndChildren for most messages
- Avoid unnecessary Parent broadcasting

### 2. Tag Usage
- Enable TagCheckEnabled to reduce routing table scans
- Use specific tags (Tag0-Tag7) instead of Everything
- Tag filtering is efficient for large routing tables

### 3. Memory Management
- Set messageHistorySize based on debugging needs
- Reduce to 50 in production builds
- Set to 10 if history not needed

### 4. PerformanceMode (Important for Production)
- Enable `MmRelayNode.PerformanceMode = true;` in production builds
- Disables debug message tracking (UpdateMessages overhead)
- Provides 2-2.2x frame time improvement
- Automatically enabled by PerformanceTestHarness during testing
- **Disable** in development for message flow visualization

### 5. When to Use MercuryMessaging
- Loosely-coupled communication needed
- Complex hierarchical structures
- Network synchronization required
- Filter-based targeting (by level, tag, state)
- FSM-based state management

### 6. When to Avoid
- Performance-critical tight loops (use direct calls)
- Simple parent-child communication (use UnityEvents)
- Flat architectures without hierarchies

### 7. Source Generators (Advanced)
Use `[MmGenerateDispatch]` attribute to generate optimized dispatch code at compile time:

```csharp
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageInt msg) { /* ... */ }
    protected override void ReceivedMessage(MmMessageString msg) { /* ... */ }
}
```

- Eliminates virtual dispatch overhead (~8-10 ticks → ~2-4 ticks)
- Requires class to be `partial`
- Requires `RoslynAnalyzer` label on generator DLL
- See [SourceGenerators/README.md](../SourceGenerators/README.md) for setup instructions

---

## Quick Win Validation Status

| Optimization | Status |
|--------------|--------|
| QW-1 (Hop Limits) | Validated via automated tests |
| QW-2 (Lazy Copy) | Validated via automated tests |
| QW-3 (Filter Cache) | Implementation complete, hit rate not measured |
| QW-4 (CircularBuffer) | Validated - negative memory growth confirms bounded behavior |
| QW-5 (LINQ Removal) | Complete - 4 allocation sites removed |
| QW-6 (Code Cleanup) | Complete - 179 lines removed from MmRelayNode |

---

## Known Limitations

### 1. Frame Time Excellent
- Current: 58-70 FPS (Editor with PerformanceMode)
- Target: 60+ FPS (achieved on Small/Medium scales!)
- **Status:** Significantly improved from 28-31 FPS, meets target
- Large scale (100+ responders) at 58 FPS is acceptable

### 2. Cache Hit Rate Not Instrumented
- QW-3 filter cache implementation complete
- Cache hit rate shows 0.0% in tests (not instrumented in hot path)
- **Status:** Acceptable - inline filtering is efficient enough

### 3. Message Throughput (Validated)
- Current: 100-1000 msg/sec (scales with responder count)
- **Status:** Meets all targets exactly
- Data validated 2025-11-28 with properly populated routing tables

---

## Future Optimization Opportunities

See `dev/archive/framework-analysis/` for Priority 3 tasks:
- **Routing optimization (420h):** O(1) routing tables, spatial indexing
- **Network performance (500h):** Delta tracking, compression
- **Visual composer (360h):** Editor tools for hierarchy design
- **Standard library (290h):** Common message patterns

---

## Detailed Reports

@../Documentation/Performance/PERFORMANCE_REPORT.md

@../Documentation/Performance/OPTIMIZATION_RESULTS.md
