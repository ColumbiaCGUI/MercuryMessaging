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

## Unified Messaging API (NEW)

The unified API provides **identical methods** for both `MmRelayNode` and `MmBaseResponder`, organized into two tiers:

### Two-Tier Design

| Tier | Description | When to Use |
|------|-------------|-------------|
| **Tier 1** | Auto-execute methods (`BroadcastInitialize()`, `NotifyComplete()`) | Quick, common operations |
| **Tier 2** | Fluent chain with `.Execute()` (`Send().ToDescendants().Execute()`) | Full control over routing |

### Tier 1: Auto-Execute Methods

These methods execute immediately - no `.Execute()` needed:

```csharp
using MercuryMessaging.Protocol.DSL;

// Broadcast DOWN to descendants (matches MmMethod enum names)
relay.BroadcastInitialize();       // → MmMethod.Initialize
relay.BroadcastRefresh();          // → MmMethod.Refresh
relay.BroadcastSetActive(true);    // → MmMethod.SetActive
relay.BroadcastSwitch("MenuState"); // → MmMethod.Switch
relay.BroadcastValue(42);          // → MmMethod.MessageInt
relay.BroadcastValue(3.14f);       // → MmMethod.MessageFloat
relay.BroadcastValue("hello");     // → MmMethod.MessageString
relay.BroadcastValue(true);        // → MmMethod.MessageBool

// Notify UP to parents/ancestors
relay.NotifyComplete();            // → MmMethod.Complete to parents
relay.NotifyValue(100);            // → MmMethod.MessageInt to parents
relay.NotifyValue("done");         // → MmMethod.MessageString to parents
```

### Works on Responders Too!

The same API works on `MmBaseResponder` (null-safe):

```csharp
public class MyResponder : MmBaseResponder
{
    public void StartTask()
    {
        // Tier 1: Same methods, same behavior!
        this.BroadcastInitialize();
        this.BroadcastValue("task started");
    }

    public void FinishTask()
    {
        this.NotifyComplete();
        this.NotifyValue(100);  // Report final score
    }

    // Tier 2: Fluent chain from responder
    public void CustomBroadcast()
    {
        this.Send("alert").ToDescendants().WithTag(MmTag.Tag0).Execute();
    }
}
```

### Naming Convention

| Direction | Prefix | Example | MmMethod |
|-----------|--------|---------|----------|
| **Down** (descendants) | `Broadcast*` | `BroadcastInitialize()` | Initialize |
| **Up** (parents) | `Notify*` | `NotifyComplete()` | Complete |

### File Location

- `Assets/MercuryMessaging/Protocol/DSL/MmMessagingExtensions.cs`

---

## Property-Based Routing (NEW)

The shortest syntax for common messaging patterns. Routes first, then sends:

```csharp
using MercuryMessaging.Protocol.DSL;

// Property-based routing (shortest syntax)
relay.To.Children.Send("Hello");           // Send string to children
relay.To.Parents.Send(42);                 // Send int to parents
relay.To.Descendants.Initialize();         // Initialize all descendants
relay.To.Children.Active.Refresh();        // Refresh only active children
relay.To.Children.WithTag(MmTag.Tag0).SetActive(false);
```

### Syntax Comparison

| Fluent API (Tier 2) | Property Routing | Savings |
|---------------------|------------------|---------|
| `relay.Send("x").ToChildren().Execute()` | `relay.To.Children.Send("x")` | 38% shorter |
| `relay.Send(42).ToParents().Active().Execute()` | `relay.To.Parents.Active.Send(42)` | 33% shorter |
| `relay.Send(MmMethod.Initialize).ToDescendants().Execute()` | `relay.To.Descendants.Initialize()` | 45% shorter |

### Direction Properties

| Property | Description |
|----------|-------------|
| `.Children` | Direct children only |
| `.Parents` | Direct parents only |
| `.Descendants` | Self + all descendants recursively |
| `.Ancestors` | Self + all ancestors recursively |
| `.Siblings` | Same-parent nodes |
| `.SelfAndChildren` | Default routing |
| `.All` | All connected nodes (bidirectional) |

### Filter Properties

| Property | Description |
|----------|-------------|
| `.Active` | Only active GameObjects |
| `.IncludeInactive` | Include inactive GameObjects |
| `.Selected` | Only FSM-selected responders |
| `.OverNetwork` | Send over network too |
| `.LocalOnly` | Local only (default) |
| `.WithTag(tag)` | Filter by tag |

### Terminal Methods (Auto-Execute)

All terminal methods execute immediately - no `.Execute()` needed:

| Method | MmMethod |
|--------|----------|
| `.Send(string)` | MessageString |
| `.Send(int)` | MessageInt |
| `.Send(float)` | MessageFloat |
| `.Send(bool)` | MessageBool |
| `.Send(Vector3)` | MessageVector3 |
| `.Initialize()` | Initialize |
| `.Refresh()` | Refresh |
| `.Complete()` | Complete |
| `.SetActive(bool)` | SetActive |
| `.Switch(int/string)` | Switch |

### Works on Responders Too!

```csharp
public class MyResponder : MmBaseResponder
{
    public void NotifyParent()
    {
        // Same syntax, null-safe (no-op if no relay node)
        this.To().Parents.Send("done");
        this.To().Parents.Send(100);
    }

    public void BroadcastToChildren()
    {
        this.To().Children.Initialize();
        this.To().Descendants.Active.Refresh();
    }
}
```

### File Location

- `Assets/MercuryMessaging/Protocol/DSL/MmRoutingBuilder.cs`

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

### 8. Listener Pattern (Phase 2.1)

Subscribe to incoming messages with a fluent API:

#### Basic Subscription

```csharp
using MercuryMessaging.Protocol.DSL;

// Subscribe to typed messages
var subscription = relay.Listen<MmMessageFloat>()
    .OnReceived(msg => brightness = msg.value)
    .Execute();

// Later, unsubscribe
subscription.Dispose();
```

#### Filtering Incoming Messages

```csharp
// Filter by value
relay.Listen<MmMessageInt>()
    .When(msg => msg.value > 50)
    .OnReceived(msg => TriggerAlert(msg.value))
    .Execute();

// Filter by tag
relay.Listen<MmMessageString>()
    .WithTag(MmTag.Tag0)
    .OnReceived(msg => HandleTaggedMessage(msg))
    .Execute();
```

#### One-Time Listeners

```csharp
// Auto-disposes after first message
relay.ListenOnce<MmMessageString>()
    .OnReceived(msg => ProcessResult(msg.value))
    .Execute();
```

#### Convenience Methods

```csharp
// Quick subscriptions (no builder needed)
var sub1 = relay.OnFloat(value => slider.value = value);
var sub2 = relay.OnInt(value => score = value);
var sub3 = relay.OnString(value => label.text = value);
var sub4 = relay.OnBool(value => gameObject.SetActive(value));

// Method-based listeners
relay.OnInitialize(() => Setup());
relay.OnRefresh(() => UpdateUI());
relay.OnComplete(() => FinishTask());
relay.OnSwitch(stateName => HandleState(stateName));
```

#### Works on Responders

```csharp
// Same API from responder (null-safe)
myResponder.Listen<MmMessageFloat>()
    .OnReceived(msg => HandleFloat(msg.value))
    .Execute();
```

---

### 9. Hierarchy Query DSL (Phase 2.2)

Query and traverse responder hierarchies with LINQ-like syntax:

#### Basic Queries

```csharp
using MercuryMessaging.Protocol.DSL;

// Get query builder
var builder = relay.Query();

// Query children
var children = relay.Query<MmResponder>().Children().ToList();

// Query all descendants
var descendants = relay.Query<MmResponder>().Descendants().ToList();
```

#### Direction Methods

| Method | Description |
|--------|-------------|
| `.Children()` | Direct children only |
| `.Parents()` | Direct parents only |
| `.Descendants()` | All descendants (recursive) |
| `.Ancestors()` | All ancestors (recursive) |
| `.Siblings()` | Same-level nodes |
| `.SelfAndChildren()` | Self + direct children |
| `.All()` | All connected nodes (bidirectional) |

#### Filter Methods

```csharp
// Filter by type
var enemies = relay.Query<EnemyResponder>()
    .Descendants()
    .ToList();

// Filter by tag
var tagged = relay.Query<MmResponder>()
    .Children()
    .WithTag(MmTag.Tag0)
    .ToList();

// Active GameObjects only
var active = relay.Query<MmResponder>()
    .Descendants()
    .Active()
    .ToList();

// Custom predicate
var filtered = relay.Query<MmResponder>()
    .Descendants()
    .Where(r => r.gameObject.name.Contains("Enemy"))
    .ToList();

// By name (exact match)
var named = relay.Query<MmResponder>()
    .Descendants()
    .Named("Player")
    .FirstOrDefault();

// By name pattern (wildcard)
var pattern = relay.Query<MmResponder>()
    .Descendants()
    .NamedLike("Enemy*")
    .ToList();
```

#### Execution Methods

| Method | Description |
|--------|-------------|
| `.Execute(action)` | Call action on each match |
| `.ToList()` | Get all matches as List |
| `.FirstOrDefault()` | Get first match or null |
| `.First()` | Get first match (throws if none) |
| `.Count()` | Count matching items |
| `.Any()` | Check if any match |
| `.Any(predicate)` | Check if any match predicate |

#### Convenience Extensions

```csharp
// Quick searches
var enemy = relay.FindDescendant<EnemyResponder>();
var parent = relay.FindAncestor<GameManager>();
var all = relay.FindAllDescendants<MmResponder>();

// By name
var player = relay.FindByName("Player");
var enemies = relay.FindByPattern("Enemy*");

// Counts and checks
int children = relay.ChildCount();
int descendants = relay.DescendantCount();
bool hasChildren = relay.HasChildren();
bool hasEnemy = relay.HasDescendant<EnemyResponder>();
```

#### Execute Actions

```csharp
// Execute action on all matches
relay.Query<EnemyResponder>()
    .Descendants()
    .Active()
    .Execute(enemy => enemy.TakeDamage(10));
```

#### Works on Responders

```csharp
// Same API from responder (null-safe)
var siblings = myResponder.Query<UIResponder>().Siblings().ToList();
int siblingCount = myResponder.SiblingCount();
bool hasSiblings = myResponder.HasSiblings();
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
| `MmListenerBuilder<T>` | Fluent listener subscription builder (Phase 2.1) |
| `MmListenerSubscription<T>` | Disposable subscription handle (Phase 2.1) |
| `MmListenerExtensions` | `Listen()` extension methods (Phase 2.1) |
| `MmQuery<T>` | LINQ-like hierarchy query builder (Phase 2.2) |
| `MmQueryBuilder` | Non-generic query entry point (Phase 2.2) |
| `MmQueryExtensions` | `Query()` extension methods (Phase 2.2) |
| `MmStandardLibraryListenerExtensions` | UI/Input message shortcuts (Phase 2.4) |
| `MmRoutingExtensions` | Runtime hierarchy registration (Phase 2.5) |
| `MmTagExtensions` | Fluent tag configuration (Phase 2.6) |
| `MmTagConfigBuilder` | Bulk tag configuration builder (Phase 2.6) |

---

### 10. Standard Library Listeners (Phase 2.4)

Convenience methods for listening to Standard Library messages:

#### UI Message Shortcuts

```csharp
using MercuryMessaging.Protocol.DSL;

// Click handling
relay.OnClick(msg => {
    if (msg.IsDoubleClick) OpenItem();
    if (msg.IsRightClick) ShowContextMenu();
});

// Hover with bool handler
relay.OnHover(isEnter => {
    if (isEnter) ShowTooltip();
    else HideTooltip();
});

// Drag handling
relay.OnDrag(msg => {
    if (msg.Phase == MmDragPhase.Move)
        transform.position += (Vector3)msg.Delta;
});

// Selection handling
relay.OnSelect((int index) => selectedIndex = index);
relay.OnSelect((string value) => selectedItem = value);
```

#### Input Message Shortcuts (VR/XR)

```csharp
// 6DOF tracking
relay.On6DOF(msg => {
    if (msg.Hand == MmHandedness.Right && msg.IsTracked)
        rightHand.SetPositionAndRotation(msg.Position, msg.Rotation);
});

// Filter by hand
relay.On6DOF(MmHandedness.Left, msg => UpdateLeftHand(msg));

// Gesture with confidence threshold
relay.OnGesture(MmGestureType.Pinch, 0.9f, msg => SelectObject());

// Button press only
relay.OnButtonPressed(msg => Fire());

// Specific button
relay.OnButton("Trigger", msg => HandleTrigger(msg.State));

// Gaze hit only (looking at something)
relay.OnGazeHit(msg => HighlightObject(msg.HitPoint));
```

---

### 11. Runtime Registration (Phase 2.5)

Simplified runtime hierarchy setup:

```csharp
using MercuryMessaging.Protocol.DSL;

// Traditional (2 lines)
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);

// Fluent DSL (1 line)
childRelay.RegisterWith(parentRelay);

// Bulk registration
parentRelay.RegisterChildren(child1, child2, child3);

// Unregistration
childRelay.UnregisterFrom(parentRelay);
parentRelay.UnregisterChildren(child1, child2);

// Hierarchy queries
bool hasParents = childRelay.HasParents();
var parent = childRelay.GetFirstParent();
bool isRoot = parentRelay.IsRoot();
```

---

### 12. Tag Configuration (Phase 2.6)

Fluent tag manipulation:

```csharp
using MercuryMessaging.Protocol.DSL;

// Single responder
responder.WithTag(MmTag.Tag0).EnableTagChecking();

// Multiple tags
responder.WithTags(MmTag.Tag0, MmTag.Tag1);

// Tag manipulation
responder.AddTag(MmTag.Tag2);
responder.RemoveTag(MmTag.Tag0);
responder.ClearTags();

// Tag queries
bool hasTag = responder.HasTag(MmTag.Tag0);
bool hasAll = responder.HasAllTags(MmTag.Tag0, MmTag.Tag1);
bool hasAny = responder.HasAnyTag(MmTag.Tag0, MmTag.Tag1);

// Bulk configuration from relay
relay.ConfigureTags()
    .ApplyToSelf(MmTag.Tag0)
    .ApplyToChildren(MmTag.Tag1, enableChecking: true)
    .ApplyToDescendants(MmTag.Tag2)
    .Build();
```

---

*Last Updated: 2025-11-26*
*DSL Version: Phase 2.6 (Tag Configuration) Complete*
