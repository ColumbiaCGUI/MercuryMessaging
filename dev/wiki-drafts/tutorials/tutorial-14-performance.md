# Tutorial 14: Performance Optimization

> **Coming Soon**
>
> This tutorial is under development. Check back for updates.

## Planned Content

This tutorial will cover performance optimization techniques for MercuryMessaging applications.

### PerformanceMode Flag

Disable debug features for production:

```csharp
// Enable PerformanceMode in production builds
MmRelayNode.PerformanceMode = true;

// Benefits:
// - Disables message history tracking
// - Removes debug visualization overhead
// - 2-2.2x frame time improvement
```

### Source Generators

Generate optimized dispatch code at compile time:

```csharp
// Add [MmGenerateDispatch] to generate optimized handlers
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageInt msg) { /* ... */ }
    protected override void ReceivedMessage(MmMessageString msg) { /* ... */ }
}

// Benefits:
// - Eliminates virtual dispatch overhead
// - ~8-10 ticks → ~2-4 ticks per message
// - Zero runtime reflection
```

### [MmHandler] Attribute

Cleaner handler registration with compile-time safety:

```csharp
public class MyResponder : MmExtendableResponder
{
    [MmHandler((MmMethod)1001)]
    private void OnCustomMethod(MmMessage msg)
    {
        // Handle custom method
    }

    [MmHandler(MmMethod.Initialize)]
    private void OnInit(MmMessage msg)
    {
        // Handle initialization
    }
}
```

### Memory Optimization

- **CircularBuffer**: Bounded message history (QW-4)
- **Lazy Copying**: Messages only copied when needed (QW-2)
- **Zero-Allocation Serialization**: `SerializePooled()` / `DeserializePooled()`

### Performance Characteristics

| Scale | Responders | Frame Time | Throughput |
|-------|-----------|------------|------------|
| Small | 10 | ~15ms | 100 msg/sec |
| Medium | 50 | ~15ms | 500 msg/sec |
| Large | 100+ | ~17ms | 1000 msg/sec |

---

## Current Status

| Optimization | Status |
|--------------|--------|
| PerformanceMode | Available |
| [MmGenerateDispatch] | Available |
| [MmHandler] | Available |
| CircularBuffer | Available |
| Lazy Copying | Available |
| Pooled Serialization | Available |

---

## Try This

Practice performance optimization:

1. **Measure PerformanceMode impact** - Create a scene with 50 responders sending messages at 100/sec. Measure frame time with `PerformanceMode = false` vs `true`. Calculate the improvement percentage.

2. **Add source generators** - Take an existing MmBaseResponder with multiple message handlers and add the `[MmGenerateDispatch]` attribute. Verify the generated code compiles and the responder still works.

3. **Profile memory allocation** - Use Unity Profiler to compare allocations when sending 1000 messages with `Serialize()` vs `SerializePooled()`. Document the difference in GC allocation.

4. **Benchmark your responder** - Use the InvocationComparison pattern to measure your custom responder's message handling time against direct method calls. Calculate the overhead percentage.

---

## Related Resources

- **[Documentation/PERFORMANCE.md](../Documentation/PERFORMANCE.md)** - Detailed benchmarks
- **[Documentation/SourceGenerators/README.md](../Documentation/SourceGenerators/README.md)** - Generator setup
- **[Tutorial 3: Custom Responders](Tutorial-3-Custom-Responders)** - MmExtendableResponder

---

*Tutorial 14 of 14 - MercuryMessaging Wiki*
