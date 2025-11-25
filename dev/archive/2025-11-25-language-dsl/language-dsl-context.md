# Language DSL Technical Context

## Design Rationale

### Why a DSL for Mercury?

Game developers face unique challenges with message-passing systems:

1. **High-Frequency Usage**: Message sends occur hundreds of times per frame
2. **Complex Routing**: Spatial, temporal, and hierarchical filtering requirements
3. **Rapid Prototyping**: Need to quickly iterate on gameplay logic
4. **Team Collaboration**: Code must be readable by designers and programmers

Traditional verbose APIs create friction that slows development and introduces bugs.

## Language Design Philosophy

### Principle 1: Visual Metaphor

The `:>` operator visually represents message flow:

```csharp
// Reads left-to-right like natural language
player :> "Jump" >> Self;
// "Player sends Jump to Self"

// Arrow metaphor shows direction
source :> data >> destination;
```

### Principle 2: Progressive Complexity

```csharp
// Level 1: Simplest possible
relay :> "Hello" >> Children;

// Level 2: Add one filter
relay :> "Hello" >> Children.Active;

// Level 3: Multiple filters
relay :> "Hello" >> Children.Active.Tag0.Network;

// Level 4: Custom predicates
relay :> "Hello" >> Children.Where(c => c.name.StartsWith("Enemy"));

// Level 5: Complex scenarios
relay :> new DamageMessage(50)
    >> Children
        .OfType<Enemy>()
        .Within(explosionRadius)
        .Where(e => !e.GetComponent<Shield>()?.IsActive ?? true)
        .Tag0
        .Network;
```

### Principle 3: Fail-Safe Defaults

```csharp
// These all have safe, sensible defaults:
Children;        // Active=true, Network=Local
Parents;         // All states, Local only
Siblings;        // Active only, Local only
All;            // Everything, everywhere

// Explicit overrides when needed:
Children.IncludeInactive.Network;
```

## Operator Design Decisions

### Why `:>` Instead of Alternatives?

We evaluated multiple operator options:

| Operator | Pros | Cons | Example |
|----------|------|------|---------|
| `->` | Familiar from C++ | Not available in C# | N/A |
| `=>` | Lambda-like | Already means lambda | `relay => message` |
| `>>` | Stream-like | Ambiguous alone | `relay >> message` |
| `:>` | Unique, arrow-like | Non-standard | `relay :> message` |
| `|>` | F# pipe style | Unfamiliar to C# devs | `relay |> message` |

`:>` was chosen because:
- Visually distinctive and memorable
- Cannot be confused with existing C# operators
- Suggests "casting to" or "sending to"
- Available for overloading

### Operator Precedence and Associativity

```csharp
// `:>` has same precedence as cast operator
// `>>` has same precedence as shift operators
// This creates natural grouping:

relay :> "A" >> Target1 :> "B" >> Target2;
// Parses as: ((relay :> "A") >> Target1) :> "B") >> Target2

// Multiple targets with `+`:
relay :> "Hello" >> (Parents + Children + Siblings);
// Parentheses required due to precedence
```

## Type System Integration

### Generic Type Inference

```csharp
public static class MmGenericExtensions
{
    // Compiler infers T from usage
    public static void Send<T>(this MmRelayNode relay,
                               T message,
                               MmRouteBuilder route)
        where T : struct, IMessage
    {
        var typedMessage = new MmMessage<T>(message);
        (relay :> typedMessage >> route).Execute();
    }
}

// Usage - T inferred as PlayerData
struct PlayerData : IMessage { public int Health; }
relay.Send(new PlayerData { Health = 100 }, Children);
```

### Implicit Conversions

```csharp
public class MmRouteBuilder
{
    // Allow implicit conversion from enums
    public static implicit operator MmRouteBuilder(MmLevelFilter level)
    {
        return new MmRouteBuilder { Level = level };
    }

    // This enables:
    relay :> "Hello" >> MmLevelFilter.Child;
    // Instead of:
    relay :> "Hello" >> new MmRouteBuilder(MmLevelFilter.Child);
}
```

### Variance in Message Handlers

```csharp
// Contravariance allows base handlers to accept derived messages
interface IMmHandler<in TMessage> where TMessage : MmMessage
{
    void Handle(TMessage message);
}

class DamageHandler : IMmHandler<MmMessage>
{
    public void Handle(MmMessage message) { }
}

// DamageHandler can handle any message type
IMmHandler<DamageMessage> handler = new DamageHandler(); // OK
```

## Performance Considerations

### Zero-Cost Abstraction Goal

The DSL must compile to equivalent code as manual API calls:

```csharp
// DSL version
relay :> "Hello" >> Children.Active.Tag0;

// Should compile to exactly:
relay.MmInvoke(
    MmMethod.MessageString,
    new MmMessageString("Hello"),
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local,
        MmTag.Tag0
    )
);
```

### Struct-Based Builders

```csharp
// Using struct avoids heap allocation
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct MmRouteBuilder
{
    // Bit-packed fields fit in 128 bits
    private ulong flags;    // 64 bits for all boolean flags
    private ulong tags;     // 64 bits for tag mask

    // Methods return modified copy (value semantics)
    public MmRouteBuilder Active
    {
        get
        {
            var copy = this;
            copy.flags |= ACTIVE_FLAG;
            return copy;
        }
    }
}
```

### Aggressive Inlining

```csharp
public struct MmRouteBuilder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MmRouteBuilder Active
    {
        get
        {
            flags |= ACTIVE_FLAG;
            return this;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MmMetadataBlock Build()
    {
        // Directly construct from bit fields
        return new MmMetadataBlock(flags, tags);
    }
}
```

### Compile-Time Constants

```csharp
public static class MmDslConstants
{
    // Pre-computed metadata for common patterns
    public static readonly MmMetadataBlock ChildrenActive =
        new MmMetadataBlock(
            MmLevelFilter.Child,
            MmActiveFilter.Active,
            MmSelectedFilter.All,
            MmNetworkFilter.Local
        );

    // Compiler can optimize known patterns:
    // relay :> "Hello" >> Children.Active;
    // Becomes:
    // relay.MmInvoke(MmMethod.MessageString, "Hello", ChildrenActive);
}
```

## Spatial and Temporal Extensions

### Spatial Filtering Design

```csharp
public static class MmSpatialExtensions
{
    // Sphere cast
    public static MmRouteBuilder Within(
        this MmRouteBuilder builder,
        float radius)
    {
        return builder.Where(obj =>
            Vector3.Distance(
                MmContext.CurrentPosition,
                obj.transform.position
            ) <= radius
        );
    }

    // Box cast
    public static MmRouteBuilder InBounds(
        this MmRouteBuilder builder,
        Bounds bounds)
    {
        return builder.Where(obj =>
            bounds.Contains(obj.transform.position)
        );
    }

    // Cone cast
    public static MmRouteBuilder InCone(
        this MmRouteBuilder builder,
        Vector3 direction,
        float angle)
    {
        return builder.Where(obj =>
        {
            var toTarget = (obj.transform.position -
                          MmContext.CurrentPosition).normalized;
            return Vector3.Angle(direction, toTarget) <= angle;
        });
    }

    // Line of sight
    public static MmRouteBuilder InLineOfSight(
        this MmRouteBuilder builder,
        LayerMask obstacles = default)
    {
        return builder.Where(obj =>
        {
            var origin = MmContext.CurrentPosition;
            var direction = obj.transform.position - origin;
            return !Physics.Raycast(origin, direction,
                                   direction.magnitude, obstacles);
        });
    }
}
```

### Temporal Extensions

```csharp
public static class MmTemporalExtensions
{
    // Delayed execution
    public static MmDelayedMessage After(
        this MmRoutedMessage message,
        float seconds)
    {
        return new MmDelayedMessage(message, seconds);
    }

    // Repeated execution
    public static MmRepeatingMessage Every(
        this MmRoutedMessage message,
        float interval,
        int count = -1)
    {
        return new MmRepeatingMessage(message, interval, count);
    }

    // Conditional timing
    public static MmConditionalMessage When(
        this MmRoutedMessage message,
        Func<bool> condition)
    {
        return new MmConditionalMessage(message, condition);
    }
}

// Usage:
relay :> "Tick" >> Children.Every(1.0f);  // Every second
relay :> "Explode" >> Self.After(3.0f);   // After 3 seconds
relay :> "Start" >> Children.When(() => GameManager.IsReady);
```

## Error Handling and Debugging

### Compile-Time Validation

```csharp
// Roslyn analyzer detects invalid combinations
relay :> "Hello" >> Children.Parents;  // ERROR: Can't combine
relay :> 42 >> Children.Within();      // ERROR: Missing radius

// Type mismatches caught at compile time
IMmHandler<string> handler = ...;
relay :> 42 >> handler;  // ERROR: int != string
```

### Runtime Diagnostics

```csharp
public struct MmRouteBuilder
{
    [Conditional("DEBUG")]
    private void ValidateRoute()
    {
        if (HasConflictingFilters())
        {
            Debug.LogError($"Conflicting filters: {this}");
        }

        if (IsImpossibleRoute())
        {
            Debug.LogWarning($"Route will never match: {this}");
        }
    }

    public override string ToString()
    {
        // Human-readable representation for debugging
        var parts = new List<string>();

        parts.Add(Level.ToString());
        if (Active != MmActiveFilter.All)
            parts.Add(Active.ToString());
        if (Tag != MmTag.Everything)
            parts.Add($"Tag{Tag}");

        return string.Join(".", parts);
    }
}
```

### DSL Debugging Visualizer

```csharp
[DebuggerTypeProxy(typeof(MmRouteBuilderDebugView))]
public struct MmRouteBuilder
{
    // ... implementation
}

internal class MmRouteBuilderDebugView
{
    private readonly MmRouteBuilder builder;

    public MmRouteBuilderDebugView(MmRouteBuilder builder)
    {
        this.builder = builder;
    }

    public string Route => builder.ToString();
    public MmLevelFilter Level => builder.Level;
    public MmActiveFilter Active => builder.Active;
    public List<string> Predicates => GetPredicateDescriptions();

    private List<string> GetPredicateDescriptions()
    {
        // Convert predicates to readable strings
        return builder.Predicates?.Select(p => p.Method.Name).ToList()
               ?? new List<string>();
    }
}
```

## Integration with Existing Code

### Backward Compatibility

```csharp
public static class MmCompatibility
{
    // Extension method provides DSL on existing types
    public static MmMessageContext To(
        this MmRelayNode relay,
        object message)
    {
        return relay :> message;
    }

    // Allow both styles:
    relay.To("Hello") >> Children;      // New DSL
    relay.MmInvoke(MmMethod.MessageString, "Hello");  // Original
}
```

### Gradual Migration

```csharp
// Phase 1: Add DSL alongside existing code
if (UseNewDsl)
{
    relay :> "Hello" >> Children.Active;
}
else
{
    relay.MmInvoke(MmMethod.MessageString, "Hello",
                  new MmMetadataBlock(MmLevelFilter.Child));
}

// Phase 2: Parallel usage
relay.MmInvoke(MmMethod.MessageInt, 42);  // Complex message
relay :> "Simple" >> Children;            // Simple message

// Phase 3: Full migration
relay :> new ComplexMessage { Value = 42 } >> Children.Active;
```

## Comparison with Industry DSLs

### Unity's Addressables

```csharp
// Unity Addressables DSL
Addressables.LoadAssetAsync<GameObject>("player")
    .Completed += handle => Instantiate(handle.Result);

// Similar builder pattern, different domain
```

### LINQ

```csharp
// LINQ's fluent interface
var results = collection
    .Where(x => x.IsActive)
    .OrderBy(x => x.Priority)
    .Select(x => x.Name);

// Mercury DSL follows similar chaining
var route = Children
    .Where(x => x.IsActive)
    .WithTag("Enemy")
    .Within(10f);
```

### Rx.NET

```csharp
// Reactive Extensions
Observable
    .Interval(TimeSpan.FromSeconds(1))
    .Where(x => x % 2 == 0)
    .Subscribe(Console.WriteLine);

// Mercury temporal extensions
relay :> "Tick"
    >> Children
    .Every(1.0f)
    .Where(c => c.IsReady);
```

## Future Extensions

### Async/Await Integration

```csharp
// Future: await message responses
var response = await (relay :> "GetStatus" >> target);

// Future: async message handlers
relay.OnMessage<string>(async msg =>
{
    await ProcessAsync(msg);
    relay :> "Done" >> Parents;
});
```

### Pattern Matching

```csharp
// Future: pattern matching on messages
relay.On<MmMessage>(msg => msg switch
{
    MmMessageInt { Value: > 100 } => HandleHighValue(),
    MmMessageString { Value: "Reset" } => Reset(),
    _ => DefaultHandler(msg)
});
```

### SIMD Optimization

```csharp
// Future: SIMD-accelerated spatial queries
relay :> "Damage"
    >> Children
    .WithinSIMD(radius)      // Uses Vector256
    .BatchProcess();          // Processes 8 at once
```