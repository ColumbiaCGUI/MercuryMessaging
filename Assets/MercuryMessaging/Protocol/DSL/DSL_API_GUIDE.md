# MercuryMessaging Fluent DSL API Guide

## Overview

The Fluent DSL provides a modern, chainable API for MercuryMessaging that reduces code verbosity by **70%** while maintaining full type safety and zero allocation overhead.

## Quick Start

```csharp
using MercuryMessaging.Protocol.DSL;

// Traditional API (7 lines)
var metadata = new MmMetadataBlock(
    MmLevelFilter.Child,
    MmActiveFilter.Active,
    MmSelectedFilter.All,
    MmNetworkFilter.Local
);
relay.MmInvoke(MmMethod.MessageInt, 42, metadata);

// Fluent DSL (1 line)
relay.Send(MmMethod.MessageInt, 42).ToChildren().Active().Execute();
```

---

## Core APIs

### 1. MmFluentMessage - Chainable Message Builder

The primary entry point for fluent messaging:

```csharp
// Basic usage
relay.Send(MmMethod.Initialize).ToChildren().Execute();

// With payload
relay.Send(MmMethod.MessageInt, 100).ToDescendants().Execute();
relay.Send(MmMethod.MessageString, "Hello").ToParents().Execute();

// Chain multiple filters
relay.Send(MmMethod.SetActive, true)
    .ToChildren()
    .Active()
    .WithTag(MmTag.Tag0)
    .LocalOnly()
    .Execute();
```

#### Routing Methods

| Method | Description |
|--------|-------------|
| `.ToChildren()` | Target direct children |
| `.ToParents()` | Target direct parents |
| `.ToDescendants()` | Target all descendants recursively |
| `.ToAncestors()` | Target all ancestors recursively |
| `.ToSiblings()` | Target siblings (same parent) |
| `.ToAll()` | Target all connected nodes |
| `.To(MmLevelFilter)` | Custom level filter |

#### Filter Methods

| Method | Description |
|--------|-------------|
| `.Active()` | Only active GameObjects |
| `.IncludeInactive()` | Include inactive GameObjects |
| `.Selected()` | Only FSM-selected responders |
| `.WithTag(MmTag)` | Filter by Mercury tag |
| `.WithTags(params MmTag[])` | Multiple tags (OR) |
| `.LocalOnly()` | Local messages only |
| `.NetworkOnly()` | Network messages only |
| `.AllDestinations()` | Both local and network |

---

### 2. Spatial Filtering (Phase 2)

Filter targets based on spatial relationships:

```csharp
// Within radius
relay.Send(MmMethod.MessageString, "Explosion")
    .ToDescendants()
    .Within(10f)  // 10 units radius
    .Execute();

// Directional cone
relay.Send(MmMethod.Initialize)
    .ToDescendants()
    .InCone(transform.forward, 45f, 20f)  // 45° cone, 20 units
    .Execute();

// Bounding box
var bounds = new Bounds(roomCenter, roomSize);
relay.Send(MmMethod.SetActive, true)
    .ToDescendants()
    .InBounds(bounds)
    .Execute();
```

---

### 3. Type Filtering (Phase 2)

Filter by component types:

```csharp
// By component type
relay.Send(MmMethod.Initialize)
    .ToDescendants()
    .OfType<Enemy>()
    .Execute();

// By interface
relay.Send(MmMethod.Complete)
    .ToDescendants()
    .Implementing<ISaveable>()
    .Execute();

// By Unity layer
relay.Send(MmMethod.SetActive, false)
    .ToDescendants()
    .OnLayer("Enemies")
    .Execute();
```

---

### 4. Custom Predicates (Phase 2)

Use lambda expressions for custom filtering:

```csharp
// Custom GameObject predicate
relay.Send(MmMethod.MessageInt, 50)
    .ToDescendants()
    .Where(go => go.GetComponent<Health>()?.Value < 100)
    .Execute();

// Custom relay predicate
relay.Send(MmMethod.Initialize)
    .ToDescendants()
    .WhereRelay(node => node.Tag == MmTag.Tag0)
    .Execute();

// Name pattern matching
relay.Send(MmMethod.Refresh)
    .ToDescendants()
    .Named("Enemy")  // Contains "Enemy"
    .Execute();
```

---

### 5. MmMessageFactory (Phase 3)

Centralized message creation with type inference:

```csharp
// Type-inferred creation
var msg = MmMessageFactory.Create(42);        // MmMessageInt
var msg = MmMessageFactory.Create("Hello");   // MmMessageString
var msg = MmMessageFactory.Create(true);      // MmMessageBool

// Typed factories
var intMsg = MmMessageFactory.Int(100);
var floatMsg = MmMessageFactory.Float(3.14f);
var stringMsg = MmMessageFactory.String("Test");
var vec3Msg = MmMessageFactory.Vector3(Vector3.up);

// Command factories
var init = MmMessageFactory.Initialize();
var active = MmMessageFactory.SetActive(true);
var switchMsg = MmMessageFactory.Switch("GameState");

// Custom methods (>= 1000)
var custom = MmMessageFactory.Custom(1001);
var customWithPayload = MmMessageFactory.Custom(1002, 42);

// Metadata configuration
var msg = MmMessageFactory.Int(100)
    .ToChildren()
    .WithTag(MmTag.UI);
```

---

### 6. Convenience Extensions (Phase 3)

High-level helpers for common patterns:

#### Broadcast (Downward)

```csharp
// Broadcast to all descendants
relay.Broadcast(MmMethod.Initialize);
relay.Broadcast(MmMethod.SetActive, true);
relay.Broadcast(MmMethod.MessageInt, 42);

// Convenience shortcuts
relay.BroadcastInitialize();
relay.BroadcastSetActive(false);
```

#### Notify (Upward)

```csharp
// Notify parent
relay.Notify(MmMethod.Complete);
relay.Notify(MmMethod.MessageInt, 100);

// Notify all ancestors
relay.NotifyAncestors(MmMethod.Refresh);

// Convenience shortcut
relay.NotifyComplete();
```

#### SendTo (Named Target)

```csharp
// Find and send to named target
relay.SendTo("Player", MmMethod.Initialize);
relay.SendTo("ScoreUI", MmMethod.MessageInt, 1000);

// Direct reference
relay.SendTo(playerRelay, MmMethod.SetActive, true);

// Check if target exists
if (relay.HasTarget("Boss"))
{
    relay.SendTo("Boss", MmMethod.Initialize);
}

// Try pattern
if (relay.TryFindTarget("Player", out var target))
{
    // Use target directly
}
```

#### Query/Response

```csharp
// Send query with callback
int queryId = relay.Query(MmMethod.MessageInt, response =>
{
    var health = ((MmMessageInt)response).value;
    Debug.Log($"Player health: {health}");
});

// In responder - send response
relay.Respond(queryId, 75);

// Cancel pending query
MmRelayNodeExtensions.CancelQuery(queryId);

// Clear all queries (on scene change)
MmRelayNodeExtensions.ClearPendingQueries();
```

---

### 7. Temporal Extensions (Phase 3)

Time-based messaging patterns:

#### Delayed Execution

```csharp
// Send after delay
var handle = relay.After(2f, MmMethod.Initialize);

// With metadata
var handle = relay.After(1.5f, MmMethod.SetActive, true);

// Cancel before execution
handle.Cancel();
```

#### Repeating Messages

```csharp
// Repeat indefinitely
var handle = relay.Every(1f, MmMethod.Refresh);

// Repeat N times
var handle = relay.Every(0.5f, MmMethod.MessageInt, 1, repeatCount: 10);

// Stop repeating
handle.Cancel();
```

#### Conditional Execution

```csharp
// Execute when condition is true
relay.When(() => player.IsReady, MmMethod.Initialize);

// With timeout
relay.When(() => levelLoaded, MmMethod.Complete, timeout: 5f);
```

#### Fluent Temporal Builder

```csharp
// Schedule with fluent syntax
relay.Schedule(MmMethod.Initialize)
    .ToDescendants()
    .After(2f)
    .Execute();

// Repeating with routing
relay.Schedule(MmMethod.Refresh)
    .ToChildren()
    .Every(1f, maxRepeats: 5)
    .Execute();
```

#### Async/Await (Task-based)

```csharp
// Async request
var response = await relay.RequestAsync<MmMessageInt>(
    MmMethod.MessageInt,
    timeout: 2f
);
Debug.Log($"Got: {response.value}");

// With cancellation
var cts = new CancellationTokenSource();
try
{
    var result = await relay.RequestAsync<MmMessageFloat>(
        MmMethod.MessageFloat,
        timeout: 5f,
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Debug.Log("Request cancelled");
}
```

---

## Best Practices

### 1. Choose the Right Method

| Scenario | Recommended API |
|----------|-----------------|
| Simple downward message | `Broadcast()` |
| Upward notification | `Notify()` |
| Specific target by name | `SendTo()` |
| Complex filtering | `Send().Where().Execute()` |
| Delayed execution | `After()` |
| Repeating updates | `Every()` |

### 2. Performance Tips

- Use `Broadcast()` for simple cases (minimal overhead)
- Avoid spatial predicates in tight loops
- Prefer `ToChildren()` over `ToDescendants()` when possible
- Cancel temporal handles when destroying GameObjects

### 3. Common Patterns

```csharp
// Initialize entire subtree
gameManager.BroadcastInitialize();

// Child notifies parent of completion
childRelay.NotifyComplete();

// Enable all UI elements
uiRoot.SendTo("MainMenu", MmMethod.SetActive, true);

// Damage enemies in area
playerRelay.Send(MmMethod.MessageInt, damage)
    .ToDescendants()
    .OfType<Enemy>()
    .Within(attackRadius)
    .Execute();

// Periodic health check
healthCheckHandle = relay.Every(1f, MmMethod.Refresh);
// On destroy: healthCheckHandle.Cancel();
```

---

## Migration Guide

### From Traditional API

```csharp
// Before
relay.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(
    MmLevelFilter.Child, MmActiveFilter.All,
    MmSelectedFilter.All, MmNetworkFilter.Local));

// After
relay.Send(MmMethod.Initialize).ToChildren().Execute();
// Or even simpler:
relay.Broadcast(MmMethod.Initialize);
```

### Gradual Migration

Both APIs work together - migrate incrementally:

```csharp
// Mix traditional and DSL
relay.MmInvoke(MmMethod.OldFeature, metadata);  // Keep working
relay.Broadcast(MmMethod.NewFeature);           // Use DSL for new code
```

---

## API Reference Summary

| Class | Purpose |
|-------|---------|
| `MmFluentMessage` | Chainable message builder (struct) |
| `MmFluentExtensions` | `Send()` extension methods on MmRelayNode |
| `MmFluentFilters` | Static routing helpers (Children, Parents, etc.) |
| `MmFluentPredicates` | Predicate infrastructure for filtering |
| `MmMessageFactory` | Centralized message creation |
| `MmRelayNodeExtensions` | Convenience methods (Broadcast, Notify, etc.) |
| `MmTemporalExtensions` | Time-based messaging (After, Every, When) |

---

*Last Updated: 2025-11-24*
*DSL Version: Phase 3 Complete*
