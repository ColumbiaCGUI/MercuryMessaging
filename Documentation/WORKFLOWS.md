# Common Workflows

This document covers typical usage patterns and the Fluent DSL API.

## Basic Message Sending

```csharp
// Get relay node on current GameObject
MmRelayNode relay = GetComponent<MmRelayNode>();

// Send boolean message to all children
relay.MmInvoke(
    MmMethod.SetActive,
    true,
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.All,
        MmSelectedFilter.All,
        MmNetworkFilter.Local
    )
);
```

## Creating a Custom Responder

**For Standard Methods (0-18) - Use MmBaseResponder:**

```csharp
public class MyCustomResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString message) {
        Debug.Log("Received string: " + message.value);
    }

    protected override void ReceivedMessage(MmMessageInt message) {
        Debug.Log("Received int: " + message.value);
    }

    protected override void ReceivedSetActive(bool active) {
        // Handle SetActive message
        gameObject.SetActive(active);
    }
}
```

**For Custom Methods (>= 1000) - Use MmExtendableResponder (Recommended):**

```csharp
public class MyExtendableResponder : MmExtendableResponder
{
    protected override void Awake()
    {
        base.Awake();

        // Register custom method handlers (clean, no switch statement!)
        RegisterCustomHandler((MmMethod)1000, OnCustomColor);
        RegisterCustomHandler((MmMethod)1001, OnCustomScale);
    }

    private void OnCustomColor(MmMessage message)
    {
        var colorMsg = (ColorMessage)message;
        GetComponent<Renderer>().material.color = colorMsg.color;
    }

    private void OnCustomScale(MmMessage message)
    {
        var scaleMsg = (ScaleMessage)message;
        transform.localScale = scaleMsg.scale;
    }
}
```

**MmExtendableResponder Benefits:**
- No switch statement boilerplate (50% less code)
- Can't forget `base.MmInvoke()` call (prevents silent failures)
- Clearer intent and easier maintenance
- Dynamic handler switching at runtime

**Performance:** Fast path (standard methods) < 200ns, Slow path (custom methods) < 500ns

## Setting Up FSM with MmRelaySwitchNode

```csharp
// On GameObject with MmRelaySwitchNode
public class GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start() {
        switchNode = GetComponent<MmRelaySwitchNode>();

        // Set initial state
        switchNode.RespondersFSM.JumpTo("MainMenu");
    }

    public void GoToGameplay() {
        // Switch to gameplay state
        switchNode.RespondersFSM.JumpTo("Gameplay");
        // All responders in "Gameplay" child object become active
    }
}
```

## Hierarchical Setup

```csharp
// Parent setup (automatic)
// MmRelayNode automatically finds parent nodes in hierarchy
// Call RefreshParents() if hierarchy changes at runtime

MmRelayNode childNode = childObject.GetComponent<MmRelayNode>();
childNode.RefreshParents(); // Updates parent references
```

## Network Message Sending

```csharp
// Send message across network
relay.MmInvoke(
    MmMethod.MessageString,
    "Hello Network",
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        MmActiveFilter.All,
        MmSelectedFilter.All,
        MmNetworkFilter.All  // Will serialize and send over network
    )
);
```

## Tag-Based Filtering

```csharp
// Set responder tags
public class UIResponder : MmBaseResponder {
    void Awake() {
        Tag = MmTag.Tag0; // UI tag
        TagCheckEnabled = true;
    }
}

public class GameplayResponder : MmBaseResponder {
    void Awake() {
        Tag = MmTag.Tag1; // Gameplay tag
        TagCheckEnabled = true;
    }
}

// Send message only to UI components
relay.MmInvoke(
    MmMethod.Refresh,
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: MmTag.Tag0  // Only reaches UIResponder
    )
);
```

---

## Fluent DSL API (Recommended)

The Fluent DSL provides a modern, chainable API that reduces code verbosity by **86%** while maintaining full type safety. It's the recommended approach for new development.

**Full Documentation:** See [API_GUIDE.md](./DSL/API_GUIDE.md)

### Quick Comparison

```csharp
// Traditional API (7 lines)
relay.MmInvoke(
    MmMethod.MessageString,
    "Hello",
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local
    )
);

// Fluent DSL (1 line)
relay.Send("Hello").ToChildren().Active().Execute();
```

### Unified Messaging API (Two-Tier)

The unified API provides **identical methods** for both `MmRelayNode` and `MmBaseResponder`:

**Tier 1: Auto-Execute Methods** (execute immediately, no `.Execute()` needed)
```csharp
// Works on BOTH MmRelayNode AND MmBaseResponder!

// Broadcast DOWN to descendants (naming matches MmMethod enum)
relay.BroadcastInitialize();              // Sends MmMethod.Initialize
relay.BroadcastRefresh();                 // Sends MmMethod.Refresh
relay.BroadcastSetActive(true);           // Sends MmMethod.SetActive
relay.BroadcastSwitch("StateName");       // Sends MmMethod.Switch
relay.BroadcastValue(42);                 // Sends MmMethod.MessageInt
relay.BroadcastValue(3.14f);              // Sends MmMethod.MessageFloat
relay.BroadcastValue("hello");            // Sends MmMethod.MessageString
relay.BroadcastValue(true);               // Sends MmMethod.MessageBool

// Notify UP to parents/ancestors
relay.NotifyComplete();                   // Sends MmMethod.Complete to parents
relay.NotifyValue(42);                    // Sends value to parents
relay.NotifyValue("status");              // Sends string to parents

// Same API from a responder!
myResponder.BroadcastInitialize();        // Works identically
myResponder.NotifyComplete();             // Null-safe (no-op if no relay node)
```

**Tier 2: Fluent Chain Methods** (full control with `.Execute()`)
```csharp
// Fluent chain from responder (new!)
myResponder.Send("hello").ToDescendants().Execute();
myResponder.Send(MmMethod.Initialize).ToDescendants().WithTag(MmTag.Tag0).Execute();
myResponder.Send(42).ToParents().Execute();
```

### Core Routing Methods

```csharp
// Direction targeting
relay.Send(value).ToChildren().Execute();      // Direct children only
relay.Send(value).ToParents().Execute();       // Direct parents only
relay.Send(value).ToDescendants().Execute();   // All descendants recursively
relay.Send(value).ToAncestors().Execute();     // All ancestors recursively
relay.Send(value).ToSiblings().Execute();      // Same-level nodes
relay.Send(value).ToAll().Execute();           // Bidirectional (parents + children)

// Filter combinations
relay.Send(value).ToChildren().Active().WithTag(MmTag.Tag0).Execute();
```

### Advanced Filtering

```csharp
// Spatial filtering (requires position)
relay.Send(value).ToDescendants().Within(10f).Execute();          // Within radius
relay.Send(value).ToDescendants().InCone(forward, 45f, 20f).Execute(); // Cone detection

// Type filtering
relay.Send(value).ToDescendants().OfType<Enemy>().Execute();      // By component type
relay.Send(value).ToDescendants().Implementing<IDamageable>().Execute(); // By interface

// Custom predicates
relay.Send(value).ToDescendants().Where(go => go.layer == 8).Execute();
relay.Send(value).ToDescendants().Named("Player*").Execute();     // Wildcard matching
```

### Temporal Extensions

```csharp
// Delayed execution
relay.After(2f, MmMethod.Initialize);           // Execute after 2 seconds

// Repeating messages
relay.Every(1f, MmMethod.Refresh, repeatCount: 5); // Every second, 5 times

// Conditional execution
relay.When(() => isReady, MmMethod.Initialize); // Execute when condition becomes true

// Fluent temporal builder
relay.Schedule(MmMethod.Initialize)
    .ToDescendants()
    .After(2f)
    .Execute();
```

### Query/Response Pattern

```csharp
// Request with callback
int queryId = relay.Query(MmMethod.MessageInt, response => {
    var value = ((MmMessageInt)response).value;
    Debug.Log($"Received: {value}");
});

// Respond to query (in responder)
relay.Respond(queryId, 42);
```

### Migration from Traditional API

| Traditional | Fluent DSL |
|-------------|------------|
| `relay.MmInvoke(method, value, metadata)` | `relay.Send(method, value).To...().Execute()` |
| Custom MmMetadataBlock | Chain `.Active()`, `.WithTag()`, etc. |
| MmLevelFilter.Child | `.ToChildren()` |
| MmLevelFilter.Parent | `.ToParents()` |
| MmLevelFilter.SelfAndChildren | `.ToDescendants()` or default |

**Note:** Traditional and Fluent APIs can be used together - they're fully compatible.

---

## Additional DSL Documentation

For comprehensive DSL coverage including all extension methods:

@./DSL/README.md
