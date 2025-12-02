# DSL/DX Context

**Last Updated:** 2025-12-01

---

## Key Decisions

### Syntax Style: Level 1 Property-Based
- Chosen over operator overloads (`>>`) and extension properties (`<<`)
- Most familiar to C#/Unity developers
- Full IntelliSense support

### Auto-Execute Strategy
- Dual API: Auto-execute for simple cases, Builder for advanced
- Analyzer MM005 warns on forgotten .Execute()

### Zero-Config for End Users
- Pre-configure `.meta` files with RoslynAnalyzer label
- Analyzers activate automatically
- Source generators require opt-in: `[MmGenerateDispatch]` + `partial`

---

## Current State

### Existing DSL (86% verbosity reduction achieved)
```csharp
// Traditional (210 chars, 7 lines)
relay.MmInvoke(MmMethod.MessageString, "Hello",
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.Active,
        MmSelectedFilter.All, MmNetworkFilter.Local));

// Current Fluent (48 chars, 1 line)
relay.Send("Hello").ToChildren().Active().Execute();

// Current Quick (32 chars)
relay.BroadcastValue("Hello");
```

### Target DSL (95% verbosity reduction)
```csharp
// Property-based (32 chars) - auto-executes
relay.To.Children.Send("Hello");
```

---

## Implementation Details

### MmRouteBuilder Struct
```csharp
public readonly struct MmRouteBuilder
{
    private readonly MmRelayNode _relay;
    private readonly MmMetadataBlock _metadata;

    public MmRouteBuilder Children =>
        new(_relay, _metadata.WithLevel(MmLevelFilter.Child));

    public MmRouteBuilder Parents =>
        new(_relay, _metadata.WithLevel(MmLevelFilter.Parent));

    public MmRouteBuilder Descendants =>
        new(_relay, _metadata.WithLevel(MmLevelFilter.SelfAndChildren));

    public MmRouteBuilder Ancestors =>
        new(_relay, _metadata.WithLevel(MmLevelFilter.SelfAndBidirectional));

    public MmRouteBuilder Active() =>
        new(_relay, _metadata.WithActive(MmActiveFilter.Active));

    public MmRouteBuilder WithTag(MmTag tag) =>
        new(_relay, _metadata.WithTag(tag));

    public void Send<T>(T value) =>
        _relay.MmInvoke(/* auto-execute with built metadata */);
}
```

### To Property on MmRelayNode
```csharp
public partial class MmRelayNode
{
    public MmRouteBuilder To => new(this, MmMetadataBlockHelper.Default);
}
```

### To Property on MmBaseResponder (null-safe)
```csharp
public class MmBaseResponder
{
    public MmRouteBuilder To =>
        MmRelayNode != null ? MmRelayNode.To : default;
}
```

---

## Analyzer Specifications

### MM005: Missing Execute
- **Trigger:** MmFluentMessage<T> returned but not used
- **Message:** "MmFluentMessage result is not used. Did you forget to call .Execute()?"
- **Quick Fix:** Add `.Execute()` at end of chain

### MM010: Non-Partial Class
- **Trigger:** [MmGenerateDispatch] on class that isn't partial
- **Message:** "Class must be partial to use [MmGenerateDispatch]"
- **Quick Fix:** Add `partial` keyword

### MM001: Suggest DSL (Future)
- **Trigger:** Verbose MmInvoke with MmMetadataBlock
- **Message:** "Consider using fluent API: relay.Send(...).ToChildren().Execute()"
- **Quick Fix:** Convert to fluent API

---

## Files to Reference

### Current DSL Implementation
```
Protocol/DSL/MmFluentMessage.cs
Protocol/DSL/MmMessagingExtensions.cs
Protocol/DSL/MmResponderExtensions.cs
Protocol/DSL/MmRoutingBuilder.cs (exists, needs enhancement)
```

### Existing Source Generator
```
SourceGenerators/MercuryMessaging.Generators/MmDispatchGenerator.cs
```

---

## Session Notes

### 2025-12-01: Planning Discussion
- Evaluated 3 syntax options (Property, Operators, Extensions)
- Selected Level 1 Property-based as most familiar to Unity devs
- Decided on Dual API (auto-execute + builder) + Analyzer warnings
- Discussed analyzer/generator distribution for end users
- Merged dsl-dx-improvements and dsl-dx-syntax into unified task

### 2025-11-27: Initial Task Creation
- Created dsl-dx-improvements task
- Outlined 4 phases: syntax, generators, analyzers, tutorials
- Estimated 120 hours total

---

*Context file for seamless continuation after context reset*
