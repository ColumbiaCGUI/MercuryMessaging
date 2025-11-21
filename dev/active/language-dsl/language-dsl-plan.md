# Language DSL Implementation Plan

## Executive Summary

Implement a fluent domain-specific language (DSL) for MercuryMessaging that reduces code verbosity by 70% while maintaining type safety and zero runtime overhead through custom operators, extension methods, and compile-time optimizations.

## Architecture Design

### Component Overview

```
┌─────────────────────────────────────────────────┐
│            Mercury DSL Layer                     │
├─────────────────────────────────────────────────┤
│   Operators          Extensions      Builders   │
│   ┌──────────┐      ┌──────────┐   ┌────────┐ │
│   │    :>    │      │  Fluent  │   │ Route  │ │
│   │    >>    │      │  Methods │   │Builder │ │
│   │    +     │      │          │   │        │ │
│   └──────────┘      └──────────┘   └────────┘ │
├─────────────────────────────────────────────────┤
│          Source Generators (Optional)           │
│   ┌────────────────────────────────────────┐   │
│   │    Compile-time DSL Expansion         │   │
│   └────────────────────────────────────────┘   │
├─────────────────────────────────────────────────┤
│            Mercury Core (Existing)              │
│   ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│   │MmRelayNode│  │MmMessage │  │MmMetadata│   │
│   └──────────┘  └──────────┘  └──────────┘   │
└─────────────────────────────────────────────────┘
```

## Phase 1: Custom Operator Implementation

### The `:>` Message Operator

```csharp
namespace MercuryMessaging.DSL
{
    public static class MmOperators
    {
        // Primary message operator
        public static MmMessageContext operator :>(
            MmRelayNode relay,
            object message)
        {
            return new MmMessageContext(relay, message);
        }

        // String message shorthand
        public static MmMessageContext operator :>(
            MmRelayNode relay,
            string message)
        {
            return new MmMessageContext(relay,
                new MmMessageString(message));
        }

        // Tuple message shorthand
        public static MmMessageContext operator :>(
            MmRelayNode relay,
            (string key, object value) kvp)
        {
            return new MmMessageContext(relay,
                CreateMessage(kvp.key, kvp.value));
        }
    }
}
```

### The `>>` Routing Operator

```csharp
public struct MmMessageContext
{
    private readonly MmRelayNode relay;
    private readonly object message;

    // Route to target
    public MmRoutedMessage operator >>(MmRouteBuilder target)
    {
        return new MmRoutedMessage(relay, message, target.Build());
    }

    // Chain to another message
    public static MmMessageContext operator :>(
        MmRoutedMessage previous,
        object nextMessage)
    {
        // Execute previous message first
        previous.Execute();

        // Return context for next message
        return new MmMessageContext(previous.Relay, nextMessage);
    }
}
```

### The `+` Combination Operator

```csharp
public static class MmRouteOperators
{
    // Combine multiple routes
    public static MmRouteBuilder operator +(
        MmRouteBuilder left,
        MmRouteBuilder right)
    {
        return left.Combine(right);
    }

    // Example: Parents + Children + Siblings
}
```

## Phase 2: Fluent Builder API

### Route Builder Structure

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct MmRouteBuilder
{
    private MmLevelFilter level;
    private MmActiveFilter active;
    private MmSelectedFilter selected;
    private MmNetworkFilter network;
    private MmTag tag;
    private List<Predicate<GameObject>> predicates;

    #region Targets

    public static MmRouteBuilder Self =>
        new MmRouteBuilder { level = MmLevelFilter.Self };

    public static MmRouteBuilder Children =>
        new MmRouteBuilder { level = MmLevelFilter.Child };

    public static MmRouteBuilder Parents =>
        new MmRouteBuilder { level = MmLevelFilter.Parent };

    public static MmRouteBuilder Siblings =>
        new MmRouteBuilder { level = MmLevelFilter.Siblings };

    public static MmRouteBuilder All =>
        new MmRouteBuilder { level = MmLevelFilter.All };

    #endregion

    #region Filters

    public MmRouteBuilder Active
    {
        get
        {
            active = MmActiveFilter.Active;
            return this;
        }
    }

    public MmRouteBuilder IncludeInactive
    {
        get
        {
            active = MmActiveFilter.All;
            return this;
        }
    }

    public MmRouteBuilder Selected
    {
        get
        {
            selected = MmSelectedFilter.Selected;
            return this;
        }
    }

    #endregion

    #region Tags

    public MmRouteBuilder Tag0 => WithTag(MmTag.Tag0);
    public MmRouteBuilder Tag1 => WithTag(MmTag.Tag1);
    public MmRouteBuilder Tag2 => WithTag(MmTag.Tag2);
    // ... etc

    public MmRouteBuilder Tagged(string tagName)
    {
        // Map string to tag enum
        return WithTag(MmTagMapper.GetTag(tagName));
    }

    private MmRouteBuilder WithTag(MmTag newTag)
    {
        tag |= newTag;
        return this;
    }

    #endregion

    #region Spatial Extensions

    public MmRouteBuilder Within(float radius)
    {
        predicates ??= new List<Predicate<GameObject>>();
        predicates.Add(obj =>
        {
            var distance = Vector3.Distance(
                MmContext.CurrentRelay.transform.position,
                obj.transform.position
            );
            return distance <= radius;
        });
        return this;
    }

    public MmRouteBuilder InDirection(Vector3 direction, float angle)
    {
        predicates ??= new List<Predicate<GameObject>>();
        predicates.Add(obj =>
        {
            var toTarget = (obj.transform.position -
                           MmContext.CurrentRelay.transform.position).normalized;
            return Vector3.Angle(direction, toTarget) <= angle;
        });
        return this;
    }

    #endregion

    #region Type Filters

    public MmRouteBuilder OfType<T>() where T : Component
    {
        predicates ??= new List<Predicate<GameObject>>();
        predicates.Add(obj => obj.GetComponent<T>() != null);
        return this;
    }

    public MmRouteBuilder WithComponent<T>() where T : Component
    {
        return OfType<T>();
    }

    #endregion

    #region Custom Predicates

    public MmRouteBuilder Where(Func<GameObject, bool> predicate)
    {
        predicates ??= new List<Predicate<GameObject>>();
        predicates.Add(new Predicate<GameObject>(predicate));
        return this;
    }

    public MmRouteBuilder Where(Func<MmRelayNode, bool> predicate)
    {
        predicates ??= new List<Predicate<GameObject>>();
        predicates.Add(obj =>
        {
            var relay = obj.GetComponent<MmRelayNode>();
            return relay != null && predicate(relay);
        });
        return this;
    }

    #endregion

    #region Network

    public MmRouteBuilder Network
    {
        get
        {
            network = MmNetworkFilter.All;
            return this;
        }
    }

    public MmRouteBuilder LocalOnly
    {
        get
        {
            network = MmNetworkFilter.Local;
            return this;
        }
    }

    #endregion

    #region Building

    internal MmMetadataBlock Build()
    {
        var metadata = new MmMetadataBlock(
            level,
            active,
            selected,
            network,
            tag
        );

        // Apply predicates if any
        if (predicates != null)
        {
            metadata.CustomFilter = CreateCompositeFilter(predicates);
        }

        return metadata;
    }

    #endregion
}
```

## Phase 3: Type-Safe Variable Arguments

### Message Factory Methods

```csharp
public static class MmMessageFactory
{
    // Single argument overloads
    public static MmMessage Create(bool value) =>
        new MmMessageBool(value);

    public static MmMessage Create(int value) =>
        new MmMessageInt(value);

    public static MmMessage Create(float value) =>
        new MmMessageFloat(value);

    public static MmMessage Create(string value) =>
        new MmMessageString(value);

    public static MmMessage Create(Vector3 value) =>
        new MmMessageVector3(value);

    // Params overload for multiple messages
    public static MmMessage[] Create(params object[] values)
    {
        var messages = new MmMessage[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            messages[i] = CreateSingle(values[i]);
        }
        return messages;
    }

    private static MmMessage CreateSingle(object value)
    {
        return value switch
        {
            bool b => Create(b),
            int i => Create(i),
            float f => Create(f),
            string s => Create(s),
            Vector3 v => Create(v),
            MmMessage m => m,
            _ => new MmMessageObject(value)
        };
    }
}
```

### Extension Methods for Common Patterns

```csharp
public static class MmRelayNodeExtensions
{
    // Broadcast to all
    public static void Broadcast(this MmRelayNode relay, object message)
    {
        (relay :> message >> MmRouteBuilder.All).Execute();
    }

    // Send to specific target
    public static void SendTo(this MmRelayNode relay,
                             GameObject target,
                             object message)
    {
        var targetRelay = target.GetComponent<MmRelayNode>();
        if (targetRelay != null)
        {
            (relay :> message >> MmRouteBuilder.Target(targetRelay)).Execute();
        }
    }

    // Request-response pattern
    public static async Task<T> Request<T>(this MmRelayNode relay,
                                           object request,
                                           MmRouteBuilder route,
                                           float timeout = 1f)
    {
        var responseTask = new TaskCompletionSource<T>();

        // Set up response handler
        relay.RegisterResponseHandler<T>(responseTask);

        // Send request
        (relay :> request >> route).Execute();

        // Wait for response with timeout
        var timeoutTask = Task.Delay((int)(timeout * 1000));
        var completed = await Task.WhenAny(responseTask.Task, timeoutTask);

        if (completed == timeoutTask)
            throw new TimeoutException($"Request timed out after {timeout}s");

        return await responseTask.Task;
    }
}
```

## Phase 4: Source Generator (Optional Enhancement)

### Compile-Time DSL Expansion

```csharp
[SourceGenerator]
public class MmDslSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Find all DSL usage
        var dslSyntaxReceiver = (DslSyntaxReceiver)context.SyntaxReceiver;

        foreach (var dslExpression in dslSyntaxReceiver.DslExpressions)
        {
            // Parse DSL expression
            var parsed = ParseDslExpression(dslExpression);

            // Generate optimized code
            var optimized = GenerateOptimizedCode(parsed);

            // Add to compilation
            context.AddSource($"{parsed.FileName}_optimized.g.cs", optimized);
        }
    }

    private string GenerateOptimizedCode(ParsedDsl dsl)
    {
        // Pre-compute metadata at compile time
        var metadata = ComputeMetadata(dsl);

        return $@"
            // Pre-computed metadata constant
            private static readonly MmMetadataBlock {dsl.ConstantName} =
                new MmMetadataBlock(
                    {metadata.Level},
                    {metadata.Active},
                    {metadata.Selected},
                    {metadata.Network},
                    {metadata.Tag}
                );

            // Optimized method call
            {dsl.Relay}.MmInvoke(
                {dsl.Method},
                {dsl.Message},
                {dsl.ConstantName}
            );
        ";
    }
}
```

## Migration Strategy

### Automated Refactoring Tool

```csharp
public class MmDslMigrator
{
    public static void MigrateProject()
    {
        var csFiles = Directory.GetFiles("Assets", "*.cs",
                                        SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var migrated = MigrateContent(content);

            if (migrated != content)
            {
                File.WriteAllText(file, migrated);
                Debug.Log($"Migrated: {file}");
            }
        }
    }

    private static string MigrateContent(string content)
    {
        // Pattern: relay.MmInvoke(method, value, metadata)
        var pattern = @"(\w+)\.MmInvoke\s*\(\s*" +
                     @"MmMethod\.(\w+)\s*,\s*" +
                     @"(.+?)\s*,\s*" +
                     @"new\s+MmMetadataBlock\s*\(([^)]+)\)\s*\)";

        return Regex.Replace(content, pattern, match =>
        {
            var relay = match.Groups[1].Value;
            var method = match.Groups[2].Value;
            var value = match.Groups[3].Value;
            var metadata = ParseMetadata(match.Groups[4].Value);

            return ConvertToDsl(relay, method, value, metadata);
        });
    }

    private static string ConvertToDsl(string relay, string method,
                                       string value, MetadataInfo metadata)
    {
        var route = BuildRoute(metadata);
        return $"{relay} :> {value} >> {route}";
    }
}
```

## Performance Validation

### Benchmark Suite

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class DslPerformanceBenchmarks
{
    private MmRelayNode relay;

    [GlobalSetup]
    public void Setup()
    {
        relay = new GameObject().AddComponent<MmRelayNode>();
    }

    [Benchmark(Baseline = true)]
    public void Traditional()
    {
        relay.MmInvoke(
            MmMethod.MessageString,
            "Hello",
            new MmMetadataBlock(
                MmLevelFilter.Child,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local,
                MmTag.Tag0
            )
        );
    }

    [Benchmark]
    public void DslOperator()
    {
        (relay :> "Hello" >> Children.Active.Tag0.LocalOnly).Execute();
    }

    [Benchmark]
    public void DslExtension()
    {
        relay.Broadcast("Hello");
    }
}
```

Expected Results:
```
| Method         | Mean     | Error   | StdDev  | Ratio | Allocated |
|--------------- |---------:|--------:|--------:|------:|----------:|
| Traditional    | 142.3 ns | 2.8 ns  | 2.6 ns  | 1.00  | 88 B      |
| DslOperator    | 145.1 ns | 2.9 ns  | 2.7 ns  | 1.02  | 88 B      |
| DslExtension   | 143.7 ns | 2.7 ns  | 2.5 ns  | 1.01  | 88 B      |
```

## Documentation Generation

### XML Documentation

```csharp
/// <summary>
/// Sends a message through the Mercury routing system.
/// </summary>
/// <param name="relay">The relay node to send from.</param>
/// <param name="message">The message to send.</param>
/// <returns>A message context for routing.</returns>
/// <example>
/// <code>
/// // Send to all children
/// relay :> "Hello" >> Children;
///
/// // Send to active siblings with tag
/// relay :> 42 >> Siblings.Active.Tag0;
/// </code>
/// </example>
public static MmMessageContext operator :>(MmRelayNode relay, object message)
```

### IntelliSense Enhancements

```xml
<CodeSnippets>
    <CodeSnippet Format="1.0.0">
        <Header>
            <Title>Mercury Message</Title>
            <Shortcut>mm</Shortcut>
        </Header>
        <Snippet>
            <Code Language="csharp">
                <![CDATA[$relay$ :> $message$ >> $target$;$end$]]>
            </Code>
            <Declarations>
                <Literal>
                    <ID>relay</ID>
                    <Default>relay</Default>
                </Literal>
                <Literal>
                    <ID>message</ID>
                    <Default>"Message"</Default>
                </Literal>
                <Literal>
                    <ID>target</ID>
                    <Default>Children</Default>
                </Literal>
            </Declarations>
        </Snippet>
    </CodeSnippet>
</CodeSnippets>
```

## Testing Strategy

### Unit Tests

```csharp
[TestFixture]
public class DslTests
{
    [Test]
    public void OperatorCreatesCorrectMessage()
    {
        var relay = CreateTestRelay();
        var context = relay :> "Hello";

        Assert.AreEqual("Hello", context.Message.Value);
        Assert.AreEqual(MmMethod.MessageString, context.Message.Method);
    }

    [Test]
    public void RouteBuilderCreatesCorrectMetadata()
    {
        var route = Children.Active.Tag0.LocalOnly;
        var metadata = route.Build();

        Assert.AreEqual(MmLevelFilter.Child, metadata.Level);
        Assert.AreEqual(MmActiveFilter.Active, metadata.Active);
        Assert.AreEqual(MmTag.Tag0, metadata.Tag);
        Assert.AreEqual(MmNetworkFilter.Local, metadata.Network);
    }

    [Test]
    public void SpatialFiltersWork()
    {
        var route = Children.Within(10f);
        var metadata = route.Build();

        Assert.IsNotNull(metadata.CustomFilter);
        // Test filter with objects at various distances
    }
}
```

## Deliverables

1. **Core DSL Implementation**
   - MmOperators.cs - Custom operators
   - MmRouteBuilder.cs - Fluent builder
   - MmMessageFactory.cs - Type inference
   - MmExtensions.cs - Helper methods

2. **Optional Enhancements**
   - Source generator project
   - Migration tool
   - Performance benchmarks

3. **Documentation**
   - API reference with examples
   - Migration guide
   - IntelliSense snippets
   - Video tutorials

4. **Testing**
   - Comprehensive unit tests
   - Integration tests
   - Performance validation
   - User acceptance tests