# Tutorial 5: Fluent DSL API

## Overview

The Fluent DSL (Domain-Specific Language) is the **recommended way** to use MercuryMessaging. It reduces code by **77%** compared to the traditional API while maintaining full type safety, IntelliSense support, and zero heap allocations.

## What You'll Learn

- **Tier 1**: Auto-execute methods (`BroadcastX()`, `NotifyX()`)
- **Tier 2**: Fluent chains with `.Execute()`
- **Property-based routing** (shortest syntax)
- **Filter methods** for precise targeting
- **Using DSL from responders** (not just relay nodes)
- **Migration** from traditional API

## Prerequisites

- Completed [Tutorial 1](Tutorial-1-Introduction) and [Tutorial 2](Tutorial-2-Basic-Routing)
- Basic understanding of MercuryMessaging hierarchy

---

## Why Use the DSL?

### Before: Traditional API (7+ lines)
```csharp
var metadata = new MmMetadataBlock(
    MmLevelFilter.Child,
    MmActiveFilter.Active,
    MmSelectedFilter.All,
    MmNetworkFilter.Local
);
relay.MmInvoke(MmMethod.MessageString, "Hello", metadata);
```

### After: Fluent DSL (1 line)
```csharp
relay.Send("Hello").ToChildren().Active().Execute();
```

**77% code reduction** with the same functionality!

---

## Two-Tier Design

The DSL is organized into two tiers based on usage patterns:

| Tier | Description | Syntax | When to Use |
|------|-------------|--------|-------------|
| **Tier 1** | Auto-execute methods | `relay.BroadcastX()` | Quick, common operations |
| **Tier 2** | Fluent chains | `relay.Send().ToX().Execute()` | Full control over routing |

---

## Tier 1: Auto-Execute Methods

These methods execute **immediately**—no `.Execute()` needed. They're perfect for common patterns.

### Broadcasting DOWN (to descendants)

```csharp
using MercuryMessaging;

// Broadcast standard methods
relay.BroadcastInitialize();       // → MmMethod.Initialize
relay.BroadcastRefresh();          // → MmMethod.Refresh
relay.BroadcastSetActive(true);    // → MmMethod.SetActive
relay.BroadcastSwitch("MenuState"); // → MmMethod.Switch

// Broadcast values (auto-detects type)
relay.BroadcastValue(42);          // → MmMethod.MessageInt
relay.BroadcastValue(3.14f);       // → MmMethod.MessageFloat
relay.BroadcastValue("hello");     // → MmMethod.MessageString
relay.BroadcastValue(true);        // → MmMethod.MessageBool
relay.BroadcastValue(Vector3.up);  // → MmMethod.MessageVector3
```

**Expected Output (when BroadcastInitialize is called):**
```
[Child1] Initialized!
[Child2] Initialized!
[Grandchild1] Initialized!
```

### Notifying UP (to parents/ancestors)

```csharp
// Notify completion
relay.NotifyComplete();            // → MmMethod.Complete to parents

// Notify values
relay.NotifyValue(100);            // → MmMethod.MessageInt to parents
relay.NotifyValue("done");         // → MmMethod.MessageString to parents
relay.NotifyValue(true);           // → MmMethod.MessageBool to parents
```

### Naming Convention

| Direction | Prefix | Target |
|-----------|--------|--------|
| **Down** | `Broadcast*` | Descendants (children, grandchildren, etc.) |
| **Up** | `Notify*` | Ancestors (parents, grandparents, etc.) |

---

## Tier 2: Fluent Chains

For full control over routing, use fluent chains ending with `.Execute()`:

### Basic Pattern

```csharp
relay.Send(value).ToDirection().Filter().Execute();
```

### Direction Methods

```csharp
// Direct targets
relay.Send("Hello").ToChildren().Execute();      // Direct children only
relay.Send("Hello").ToParents().Execute();       // Direct parents only

// Recursive targets
relay.Send("Hello").ToDescendants().Execute();   // Self + all descendants
relay.Send("Hello").ToAncestors().Execute();     // Self + all ancestors

// Other targets
relay.Send("Hello").ToSiblings().Execute();      // Same-level nodes
relay.Send("Hello").ToAll().Execute();           // All directions
relay.Send("Hello").ToSelf().Execute();          // Only self
```

### Filter Methods

```csharp
// Activity filter
relay.Send("Hello").ToChildren().Active().Execute();         // Active only (default)
relay.Send("Hello").ToChildren().IncludeInactive().Execute(); // Include inactive

// Tag filter
relay.Send("Hello").ToChildren().WithTag(MmTag.Tag0).Execute();
relay.Send("Hello").ToChildren().WithTags(MmTag.Tag0, MmTag.Tag1).Execute();

// FSM selection filter
relay.Send("Hello").ToDescendants().Selected().Execute();    // FSM-selected only

// Network filter
relay.Send("Hello").ToDescendants().LocalOnly().Execute();   // No network (default)
relay.Send("Hello").ToDescendants().OverNetwork().Execute(); // Network sync
```

### Combining Filters

```csharp
// Multiple filters chained together
relay.Send("Alert")
    .ToDescendants()
    .Active()
    .WithTag(MmTag.Tag0)
    .LocalOnly()
    .Execute();
```

---

## Direction Targeting Reference

| DSL Method | Traditional Equivalent | Description |
|------------|----------------------|-------------|
| `.ToChildren()` | `MmLevelFilter.Child` | Direct children only |
| `.ToParents()` | `MmLevelFilter.Parent` | Direct parents only |
| `.ToDescendants()` | `MmLevelFilter.SelfAndChildren` | Self + all descendants |
| `.ToAncestors()` | `MmLevelFilter.SelfAndParent` | Self + all ancestors |
| `.ToSiblings()` | Custom | Same-parent nodes |
| `.ToAll()` | `MmLevelFilter.SelfAndBidirectional` | All connected nodes |
| `.ToSelf()` | `MmLevelFilter.Self` | Only the sender |

---

## Filter Methods Reference

| Method | Description | Default |
|--------|-------------|---------|
| `.Active()` | Only active GameObjects | ✅ Yes |
| `.IncludeInactive()` | Include inactive GameObjects | |
| `.Selected()` | Only FSM-selected responders | |
| `.AllSelected()` | All responders regardless of FSM | ✅ Yes |
| `.LocalOnly()` | No network propagation | ✅ Yes |
| `.OverNetwork()` | Sync over network | |
| `.WithTag(tag)` | Filter by single tag | |
| `.WithTags(t1, t2)` | Filter by multiple tags (OR) | |
| `.AnyTag()` | Match any tag | ✅ Yes |
| `.NoTags()` | Match no tags | |

---

## Property-Based Routing (Shortest Syntax)

For the most concise code, use property-based routing:

```csharp
// Property routing - routes first, then sends (auto-executes!)
relay.To.Children.Send("Hello");           // Send string to children
relay.To.Parents.Send(42);                 // Send int to parents
relay.To.Descendants.Initialize();         // Initialize all descendants
relay.To.Children.Active.Refresh();        // Refresh active children only
relay.To.Ancestors.Send(true);             // Send bool to ancestors
```

### Adding Filters

```csharp
relay.To.Children.Active.Send("Hello");
relay.To.Descendants.WithTag(MmTag.Tag0).SetActive(false);
relay.To.Parents.OverNetwork.Send("sync");
```

### Terminal Methods (Auto-Execute)

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

---

## Using DSL from Responders

The **same DSL API** works on `MmBaseResponder` (and subclasses). This is null-safe—if there's no relay node, it's a no-op.

### Tier 1 from Responder

```csharp
public class MyResponder : MmBaseResponder
{
    public void OnTaskComplete()
    {
        // Tier 1 methods work identically!
        this.BroadcastInitialize();
        this.BroadcastValue("task completed");
        this.NotifyComplete();
        this.NotifyValue(100);
    }
}
```

### Tier 2 from Responder

```csharp
public class MyResponder : MmBaseResponder
{
    public void SendAlert()
    {
        // Fluent chains work too
        this.Send("alert").ToDescendants().WithTag(MmTag.Tag0).Execute();
        this.Send(42).ToParents().Execute();
    }
}
```

### Property Routing from Responder

```csharp
public class MyResponder : MmBaseResponder
{
    public void Notify()
    {
        // Slightly different syntax: this.To() instead of relay.To
        this.To().Parents.Send("done");
        this.To().Children.Initialize();
    }
}
```

---

## Migration Guide

### Pattern 1: Simple Message

```csharp
// Before
relay.MmInvoke(MmMethod.MessageString, "Hello");

// After (Tier 2)
relay.Send("Hello").Execute();

// After (Property)
relay.To.SelfAndChildren.Send("Hello");
```

### Pattern 2: Targeted Message

```csharp
// Before
relay.MmInvoke(MmMethod.MessageBool, true,
    new MmMetadataBlock(MmLevelFilter.Child));

// After (Tier 2)
relay.Send(true).ToChildren().Execute();

// After (Property)
relay.To.Children.Send(true);
```

### Pattern 3: Initialize Descendants

```csharp
// Before
relay.MmInvoke(MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.SelfAndChildren));

// After (Tier 1 - recommended)
relay.BroadcastInitialize();

// After (Tier 2)
relay.Send(MmMethod.Initialize).ToDescendants().Execute();
```

### Pattern 4: Notify Parent of Completion

```csharp
// Before
relay.MmInvoke(MmMethod.Complete,
    new MmMetadataBlock(MmLevelFilter.Parent));

// After (Tier 1 - recommended)
relay.NotifyComplete();

// After (Tier 2)
relay.Send(MmMethod.Complete).ToParents().Execute();
```

### Pattern 5: Tagged + Active Filter

```csharp
// Before
relay.MmInvoke(MmMethod.Refresh,
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local,
        MmTag.Tag0
    ));

// After
relay.Send(MmMethod.Refresh)
    .ToChildren()
    .Active()
    .WithTag(MmTag.Tag0)
    .Execute();
```

---

## Complete Example: Game Controller

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Initialize all game systems
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // Press Space to pause
        if (Input.GetKeyDown(KeyCode.Space))
            relay.BroadcastSetActive(false);

        // Press R to refresh UI
        if (Input.GetKeyDown(KeyCode.R))
            relay.To.Children.WithTag(MmTag.Tag0).Refresh();

        // Press 1-5 for different messages
        if (Input.GetKeyDown(KeyCode.Alpha1))
            relay.Send("Player moved").ToDescendants().Execute();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            relay.Send(100).ToChildren().Active().Execute();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            relay.BroadcastValue(Time.time);
    }

    public void OnPlayerDied()
    {
        // Notify parents (game manager hierarchy)
        relay.NotifyValue("PlayerDied");
    }
}
```

---

## Interactive Demo

The MercuryMessaging package includes an interactive demo scene:

**Location:** `Assets/MercuryMessaging/Examples/Tutorials/DSL/`

### Setup

1. Create an empty scene
2. Add a GameObject with `DSLSceneSetup` component
3. Press Play

### Keyboard Controls

| Key | Demo |
|-----|------|
| **1** | Traditional vs Fluent comparison |
| **2** | Routing targets (Children/Descendants/Parents/All) |
| **3** | Typed values (int/float/string/bool/Vector3) |
| **4** | Combined filters (Active + Tag) |
| **5** | Auto-execute convenience methods |
| **T** | Delayed execution (2 second delay) |
| **Y** | Repeating messages (color cycling) |
| **U** | Conditional trigger (press SPACE to activate) |

---

## Performance Notes

The Fluent DSL is designed for **zero overhead**:

- **Struct-based builders** - No heap allocations
- **Aggressive inlining** - Compiler optimizations
- **Same internal calls** - Uses existing `MmInvoke` underneath
- **<2% overhead** compared to traditional API

### Benchmark

```
1000 messages:
- Traditional API: 0.0234s
- Fluent DSL:      0.0238s
- Overhead:        1.7%
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Forgot `.Execute()` | Tier 2 chains require `.Execute()` at the end |
| Using `relay.To` from responder | Use `this.To()` with parentheses from responders |
| Message not sent | Tier 1 methods auto-execute; Tier 2 needs `.Execute()` |
| Wrong direction | Check `.ToChildren()` vs `.ToDescendants()` |

---

## Best Practices

1. **Prefer Tier 1** for common patterns (BroadcastInitialize, NotifyComplete)
2. **Use Tier 2** when you need filters or specific routing
3. **Use property routing** for the most concise code
4. **Chain filters logically**: Direction → Activity → Tags → Network
5. **Don't forget `.Execute()`** on Tier 2 chains

---

## Quick Reference Card

```csharp
// TIER 1: Auto-execute (no .Execute() needed)
relay.BroadcastInitialize();          // Initialize descendants
relay.BroadcastRefresh();             // Refresh descendants
relay.BroadcastSetActive(true);       // Activate descendants
relay.BroadcastValue(42);             // Send value down
relay.NotifyComplete();               // Notify parents
relay.NotifyValue("done");            // Send value up

// TIER 2: Fluent chains (need .Execute())
relay.Send("x").ToChildren().Execute();
relay.Send(42).ToParents().Active().Execute();
relay.Send(true).ToDescendants().WithTag(MmTag.Tag0).Execute();

// PROPERTY ROUTING: Shortest (auto-executes)
relay.To.Children.Send("x");
relay.To.Parents.Send(42);
relay.To.Descendants.Active.Refresh();

// FROM RESPONDER
this.BroadcastInitialize();
this.Send("x").ToChildren().Execute();
this.To().Parents.Send(42);
```

---

## Try This

Practice the Fluent DSL:

1. **Convert Traditional to DSL** - Take any MmInvoke call with MmMetadataBlock and convert it to the equivalent DSL chain. Verify both produce the same behavior.

2. **Compare all three syntaxes** - For the same message, write it using: (a) Traditional API, (b) Tier 2 Fluent chain, (c) Tier 1 auto-execute. Which is clearest for your use case?

3. **Filter combination** - Send a message that targets only active children with Tag0, excluding network propagation. Write it using DSL.

4. **From responder practice** - Create a responder that uses `this.NotifyComplete()` when a task finishes and `this.BroadcastRefresh()` when it needs children to update.

---

## Next Steps

- **[Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)** - Send messages over the network
- **[Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)** - State machines with DSL
- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Network architecture deep dive

---

*Tutorial 5 of 14 - MercuryMessaging Wiki*
