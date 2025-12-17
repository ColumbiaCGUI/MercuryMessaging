# MercuryMessaging Fluent DSL

The Fluent DSL provides a modern, chainable API for MercuryMessaging that reduces code verbosity by **77%** while maintaining full type safety and zero heap allocations.

## Quick Start

```csharp
using MercuryMessaging;

// Traditional API (7 lines)
relay.MmInvoke(
    MmMethod.MessageString,
    "Hello",
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active
    )
);

// Fluent DSL (1 line)
relay.Send("Hello").ToChildren().Active().Execute();
```

## Two-Tier API

### Tier 1: Auto-Execute Methods

Simple one-liners that execute immediately:

```csharp
// Broadcast DOWN to descendants
relay.BroadcastInitialize();          // MmMethod.Initialize
relay.BroadcastRefresh();             // MmMethod.Refresh
relay.BroadcastSetActive(true);       // MmMethod.SetActive
relay.BroadcastSwitch("StateName");   // MmMethod.Switch
relay.BroadcastValue(42);             // MmMethod.MessageInt
relay.BroadcastValue("hello");        // MmMethod.MessageString

// Notify UP to parents
relay.NotifyComplete();               // MmMethod.Complete to parents
relay.NotifyValue(42);                // Value to parents
```

### Tier 2: Fluent Chain Methods

Full control with chainable methods:

```csharp
relay.Send("hello")
    .ToDescendants()
    .Active()
    .WithTag(MmTag.Tag0)
    .Execute();
```

## Works on Both Relay Nodes AND Responders

The unified API works identically on both types:

```csharp
// On MmRelayNode
relay.BroadcastInitialize();
relay.Send("hello").ToChildren().Execute();

// Same API on MmBaseResponder!
responder.BroadcastInitialize();
responder.Send("hello").ToChildren().Execute();
```

## Documentation

- **[API_GUIDE.md](API_GUIDE.md)** - Complete API reference with all methods
- **[Tutorial 5: Fluent DSL API](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki/Tutorial-5-Fluent-DSL-API)** - Step-by-step tutorial

## Key Features

| Feature | Description |
|---------|-------------|
| **Zero Allocations** | Struct-based builder avoids heap allocations |
| **Type Safety** | Compile-time type checking for all parameters |
| **IntelliSense** | Full autocomplete support in IDEs |
| **Unified API** | Same methods on MmRelayNode and MmBaseResponder |
| **Backward Compatible** | Traditional API still works alongside DSL |

## DSL Files

| File | Purpose |
|------|---------|
| `MmMessagingExtensions.cs` | Unified Tier 1 API (Broadcast/Notify) |
| `MmFluentMessage.cs` | Fluent builder struct |
| `MmFluentExtensions.cs` | Send() extension methods |
| `MmFluentFilters.cs` | Filter chain methods |
| `MmFluentPredicates.cs` | Where/OfType predicates |
| `MmTemporalExtensions.cs` | After/Every/When timing |
| `MmQueryExtensions.cs` | Query/Response pattern |

---

*Part of MercuryMessaging 4.0.0*
