# Fluent DSL API Guide

Comprehensive reference for the MercuryMessaging Fluent DSL API.

---

## Table of Contents

1. [Tier 1: Auto-Execute Methods](#tier-1-auto-execute-methods)
2. [Tier 2: Fluent Chain Methods](#tier-2-fluent-chain-methods)
3. [Routing Methods](#routing-methods)
4. [Filter Methods](#filter-methods)
5. [Predicate Methods](#predicate-methods)
6. [Temporal Methods](#temporal-methods)
7. [Query/Response Pattern](#queryresponse-pattern)
8. [Migration Guide](#migration-guide)
9. [Performance Notes](#performance-notes)

---

## Tier 1: Auto-Execute Methods

Simple methods that execute immediately without calling `.Execute()`.

### Broadcast Methods (Down)

Send messages DOWN to descendants.

```csharp
// Standard methods
relay.BroadcastInitialize();              // MmMethod.Initialize
relay.BroadcastRefresh();                 // MmMethod.Refresh
relay.BroadcastSetActive(true);           // MmMethod.SetActive with bool
relay.BroadcastSwitch("StateName");       // MmMethod.Switch with string

// Value methods (auto-detect type)
relay.BroadcastValue(true);               // MmMethod.MessageBool
relay.BroadcastValue(42);                 // MmMethod.MessageInt
relay.BroadcastValue(3.14f);              // MmMethod.MessageFloat
relay.BroadcastValue("hello");            // MmMethod.MessageString
relay.BroadcastValue(new Vector3(1,2,3)); // MmMethod.MessageVector3
```

### Notify Methods (Up)

Send messages UP to parents/ancestors.

```csharp
relay.NotifyComplete();                   // MmMethod.Complete to parents
relay.NotifyValue(42);                    // Value to parents
relay.NotifyValue("status");              // String to parents
relay.NotifyValue(true);                  // Bool to parents
```

### Responder Support

All Tier 1 methods work on `MmBaseResponder` too:

```csharp
public class MyResponder : MmBaseResponder
{
    void DoSomething()
    {
        // These work identically to relay node methods!
        this.BroadcastInitialize();
        this.NotifyComplete();
        this.BroadcastValue("hello");
    }
}
```

---

## Tier 2: Fluent Chain Methods

Full control with chainable builder pattern. **Always call `.Execute()` at the end.**

### Basic Usage

```csharp
// Send with routing
relay.Send("hello").ToChildren().Execute();
relay.Send(42).ToParents().Execute();
relay.Send(MmMethod.Initialize).ToDescendants().Execute();

// Send with filters
relay.Send("hello")
    .ToDescendants()
    .Active()
    .WithTag(MmTag.Tag0)
    .Execute();
```

### Send Overloads

```csharp
// By method
relay.Send(MmMethod.Initialize);
relay.Send(MmMethod.SetActive, true);

// By value (auto-detects MmMethod)
relay.Send(true);                         // MessageBool
relay.Send(42);                           // MessageInt
relay.Send(3.14f);                        // MessageFloat
relay.Send("hello");                      // MessageString
relay.Send(new Vector3(1,2,3));           // MessageVector3

// By message object
relay.Send(customMessage);
```

---

## Routing Methods

Control message direction through the hierarchy.

### Direction Methods

| Method | Description | MmLevelFilter Equivalent |
|--------|-------------|-------------------------|
| `.ToSelf()` | Only self | `Self` |
| `.ToChildren()` | Direct children only | `Child` |
| `.ToParents()` | Direct parents only | `Parent` |
| `.ToDescendants()` | All descendants recursively | `Descendants` |
| `.ToAncestors()` | All ancestors recursively | `Ancestors` |
| `.ToSiblings()` | Same-level nodes | `Siblings` |
| `.ToAll()` | Bidirectional (up and down) | `SelfAndBidirectional` |

### Examples

```csharp
// Direct children only (not grandchildren)
relay.Send("hello").ToChildren().Execute();

// All descendants (children, grandchildren, etc.)
relay.Send("hello").ToDescendants().Execute();

// Direct parents only
relay.Send("complete").ToParents().Execute();

// All ancestors (parents, grandparents, etc.)
relay.Send("complete").ToAncestors().Execute();

// Siblings (same level in hierarchy)
relay.Send("sync").ToSiblings().Execute();

// Everything (up and down)
relay.Send("reset").ToAll().Execute();
```

### Custom Level Filter

```csharp
// Using To() with explicit filter
relay.Send("hello").To(MmLevelFilter.Child | MmLevelFilter.Self).Execute();
```

---

## Filter Methods

Refine which responders receive the message.

### Active Filter

```csharp
// Only active GameObjects (default)
relay.Send("hello").ToDescendants().Active().Execute();

// Include inactive GameObjects
relay.Send("hello").ToDescendants().All().Execute();
```

### Selected Filter

```csharp
// Only responders in current FSM state
relay.Send("hello").ToDescendants().Selected().Execute();

// All responders regardless of FSM state
relay.Send("hello").ToDescendants().Unselected().Execute();
```

### Tag Filter

```csharp
// Single tag
relay.Send("hello").ToDescendants().WithTag(MmTag.Tag0).Execute();

// Multiple tags (OR logic)
relay.Send("hello").ToDescendants().WithTag(MmTag.Tag0 | MmTag.Tag1).Execute();

// Exclude tag
relay.Send("hello").ToDescendants().WithoutTag(MmTag.Tag2).Execute();
```

### Network Filter

```csharp
// Local only (no network propagation)
relay.Send("hello").ToDescendants().Local().Execute();

// Network only
relay.Send("hello").ToDescendants().Network().Execute();

// Both local and network
relay.Send("hello").ToDescendants().Networked().Execute();
```

### Combining Filters

```csharp
relay.Send("hello")
    .ToDescendants()
    .Active()
    .Selected()
    .WithTag(MmTag.Tag0)
    .Local()
    .Execute();
```

---

## Predicate Methods

Advanced filtering using custom conditions.

### Type Filtering

```csharp
// By component type
relay.Send("damage").ToDescendants().OfType<Enemy>().Execute();

// By interface
relay.Send("heal").ToDescendants().Implementing<IDamageable>().Execute();
```

### Custom Predicates

```csharp
// Custom condition
relay.Send("alert")
    .ToDescendants()
    .Where(go => go.layer == LayerMask.NameToLayer("Enemies"))
    .Execute();

// Name matching (supports wildcards)
relay.Send("activate").ToDescendants().Named("Door*").Execute();
relay.Send("deactivate").ToDescendants().Named("*Controller").Execute();
```

### Spatial Filtering

```csharp
// Within radius (meters)
relay.Send("explosion")
    .ToDescendants()
    .Within(10f)
    .Execute();

// In cone (direction, angle, range)
relay.Send("alert")
    .ToDescendants()
    .InCone(transform.forward, 45f, 20f)
    .Execute();

// In bounds
relay.Send("activate")
    .ToDescendants()
    .InBounds(triggerBounds)
    .Execute();
```

---

## Temporal Methods

Time-based message sending patterns.

### Delayed Execution

```csharp
// Send after delay
var handle = relay.After(2f, MmMethod.Initialize);

// With value
relay.After(1.5f, MmMethod.MessageString, "delayed hello");

// Cancel if needed
handle.Cancel();
```

### Repeating Messages

```csharp
// Repeat forever
var handle = relay.Every(1f, MmMethod.Refresh);

// Repeat N times
relay.Every(0.5f, MmMethod.Refresh, repeatCount: 5);

// Cancel
handle.Cancel();
```

### Conditional Execution

```csharp
// Execute when condition becomes true
relay.When(() => isReady, MmMethod.Initialize);

// With timeout
relay.When(() => playerInRange, MmMethod.Activate, timeout: 10f);
```

### Sequence Example

```csharp
// Countdown: 3-2-1-Go!
relay.After(0f, MmMethod.MessageInt, 3);
relay.After(1f, MmMethod.MessageInt, 2);
relay.After(2f, MmMethod.MessageInt, 1);
relay.After(3f, MmMethod.MessageString, "Go!");
```

---

## Query/Response Pattern

Request-response messaging with callbacks.

### Sending Queries

```csharp
// Query with callback
int queryId = relay.Query(MmMethod.MessageInt, response => {
    var value = ((MmMessageInt)response).value;
    Debug.Log($"Received response: {value}");
});
```

### Responding to Queries

```csharp
// In responder
public override void MmInvoke(MmMessage message)
{
    base.MmInvoke(message);

    if (message.QueryId > 0)
    {
        // This is a query, send response
        relay.Respond(message.QueryId, 42);
    }
}
```

---

## Migration Guide

### From Traditional API

| Traditional | Fluent DSL |
|-------------|------------|
| `relay.MmInvoke(MmMethod.Initialize)` | `relay.BroadcastInitialize()` |
| `relay.MmInvoke(MmMethod.Initialize, meta)` | `relay.Send(MmMethod.Initialize).To(filter).Execute()` |
| `relay.MmInvoke(method, value, meta)` | `relay.Send(value).To(filter).Execute()` |
| `new MmMetadataBlock(LevelFilter.Child)` | `.ToChildren()` |
| `new MmMetadataBlock(LevelFilter.Parent)` | `.ToParents()` |
| `MmActiveFilter.Active` | `.Active()` |
| `MmSelectedFilter.Selected` | `.Selected()` |
| `MmNetworkFilter.All` | `.Networked()` |
| `tag: MmTag.Tag0` | `.WithTag(MmTag.Tag0)` |

### Gradual Migration

Traditional and Fluent APIs can coexist:

```csharp
// These are equivalent and can be mixed
relay.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(MmLevelFilter.Child));
relay.Send(MmMethod.Initialize).ToChildren().Execute();
relay.BroadcastInitialize(); // If default routing is acceptable
```

---

## Performance Notes

### Zero Allocations

`MmFluentMessage` is a **struct**, not a class. The builder pattern creates no heap allocations:

```csharp
// This allocates ZERO bytes on the heap
relay.Send("hello").ToDescendants().Active().WithTag(MmTag.Tag0).Execute();
```

### Inlining

All methods use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for optimal performance.

### When to Use Each Tier

| Scenario | Recommended |
|----------|-------------|
| Simple broadcast to children | Tier 1: `BroadcastInitialize()` |
| Notify parent of completion | Tier 1: `NotifyComplete()` |
| Custom routing needed | Tier 2: `Send().To...().Execute()` |
| Multiple filters | Tier 2: Chain `.Active().WithTag()` |
| Spatial/predicate filtering | Tier 2: `.Where()`, `.Within()` |

### Performance Comparison

| API | Overhead |
|-----|----------|
| Traditional `MmInvoke` | Baseline |
| Tier 1 Auto-Execute | ~Same (inlined) |
| Tier 2 Fluent Chain | +2-5ns (struct copy) |

The overhead is negligible for all practical use cases.

---

## Common Patterns

### State Machine Transitions

```csharp
// Switch to new state
relay.BroadcastSwitch("Gameplay");

// Notify completion to parent state machine
relay.NotifyComplete();
```

### Hierarchical Activation

```csharp
// Activate all children
relay.BroadcastSetActive(true);

// Activate only UI-tagged children
relay.Send(MmMethod.SetActive, true)
    .ToDescendants()
    .WithTag(MmTag.UI)
    .Execute();
```

### Networked Position Sync

```csharp
relay.Send(transform.position)
    .ToDescendants()
    .Networked()
    .Execute();
```

### Delayed Respawn

```csharp
relay.After(3f, MmMethod.Initialize);
```

---

*MercuryMessaging 4.0.0 - Fluent DSL API Guide*
