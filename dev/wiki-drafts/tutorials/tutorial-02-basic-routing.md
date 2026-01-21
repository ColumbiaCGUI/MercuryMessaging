# Tutorial 2: Basic Routing

## Overview

MercuryMessaging's power comes from its flexible routing system. Messages can flow in different directions through the hierarchy, target only certain responders, and be filtered by various criteria. This tutorial covers the routing fundamentals.

## What You'll Learn

- **Level Filters**: Control message direction (up, down, both)
- **Active Filters**: Target active or inactive GameObjects
- **Routing with Traditional and DSL APIs**
- **Common routing patterns**

## Prerequisites

- Completed [Tutorial 1: Introduction](Tutorial-1-Introduction)
- Basic hierarchy setup with MmRelayNode components

---

## Level Filters (Message Direction)

The `MmLevelFilter` controls which direction messages travel through the hierarchy:

| Filter | Description | Use Case |
|--------|-------------|----------|
| `Self` | Only the sending node | Local processing |
| `Child` | Direct children only | One level down |
| `Parent` | Direct parents only | One level up |
| `SelfAndChildren` | Self + descendants (DEFAULT) | Broadcast to subtree |
| `SelfAndParent` | Self + ancestors | Notify up the chain |
| `SelfAndBidirectional` | All connected nodes | Full propagation |

### Visual Diagram

```
      GrandParent
           │
         Parent  ◄─── relay.Send("x").ToParents()
           │
      ═══ Self ═══  ◄─── relay.Send("x") [default: SelfAndChildren]
         /    \
     Child1  Child2  ◄─── relay.Send("x").ToChildren()
        │
   Grandchild  ◄─── relay.Send("x").ToDescendants()
```

---

## Routing Examples

### Traditional API

```csharp
using MercuryMessaging;

public class RoutingExamples : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void SendToChildren()
    {
        // Send only to direct children
        relay.MmInvoke(
            MmMethod.MessageString,
            "Hello children",
            new MmMetadataBlock(MmLevelFilter.Child)
        );
    }

    void SendToParent()
    {
        // Send only to direct parent
        relay.MmInvoke(
            MmMethod.MessageString,
            "Hello parent",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }

    void SendToAll()
    {
        // Send in all directions
        relay.MmInvoke(
            MmMethod.MessageString,
            "Hello everyone",
            new MmMetadataBlock(MmLevelFilter.SelfAndBidirectional)
        );
    }
}
```

### DSL API (Recommended)

The Fluent DSL makes routing much cleaner:

```csharp
using MercuryMessaging;

public class RoutingExamplesDSL : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void SendToChildren()
    {
        // Direct children only
        relay.Send("Hello children").ToChildren().Execute();
    }

    void SendToDescendants()
    {
        // All descendants (children, grandchildren, etc.)
        relay.Send("Hello descendants").ToDescendants().Execute();
    }

    void SendToParent()
    {
        // Direct parent only
        relay.Send("Hello parent").ToParents().Execute();
    }

    void SendToAncestors()
    {
        // All ancestors (parent, grandparent, etc.)
        relay.Send("Hello ancestors").ToAncestors().Execute();
    }

    void SendToSiblings()
    {
        // Same-level nodes (share same parent)
        relay.Send("Hello siblings").ToSiblings().Execute();
    }

    void SendToAll()
    {
        // All connected nodes (bidirectional)
        relay.Send("Hello everyone").ToAll().Execute();
    }
}
```

### Property-Based Routing (Shortest)

For the most concise syntax:

```csharp
// Property routing - auto-executes!
relay.To.Children.Send("Hello");
relay.To.Parents.Send("Notify");
relay.To.Descendants.Send(42);
relay.To.Ancestors.Send(true);
```

**Expected Output (varies by target):**
```
Child1: Received 'Hello'
Child2: Received 'Hello'
```

---

## Direction Methods Reference

| DSL Method | Traditional Equivalent | Description |
|------------|----------------------|-------------|
| `.ToChildren()` | `MmLevelFilter.Child` | Direct children only |
| `.ToParents()` | `MmLevelFilter.Parent` | Direct parents only |
| `.ToDescendants()` | `MmLevelFilter.SelfAndChildren` | Self + all descendants |
| `.ToAncestors()` | `MmLevelFilter.SelfAndParent` | Self + all ancestors |
| `.ToSiblings()` | N/A (custom) | Same-level nodes |
| `.ToAll()` | `MmLevelFilter.SelfAndBidirectional` | All directions |
| `.ToSelf()` | `MmLevelFilter.Self` | Only the sender |

---

## Active Filter

Control whether inactive GameObjects receive messages:

### Traditional API

```csharp
// Only active GameObjects (default)
relay.MmInvoke(
    MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.Active)
);

// Include inactive GameObjects
relay.MmInvoke(
    MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All)
);
```

### DSL API

```csharp
// Only active GameObjects (default)
relay.Send("Hello").ToChildren().Active().Execute();

// Include inactive GameObjects
relay.Send("Hello").ToChildren().IncludeInactive().Execute();
```

---

## Practical Example: UI Menu System

Here's a real-world example of a menu system using routing:

### Hierarchy Setup

```
MenuManager (MmRelayNode + MenuController)
  ├── MainMenu (MmRelayNode + MenuPanel)
  │     ├── PlayButton (MmRelayNode + ButtonResponder)
  │     └── SettingsButton (MmRelayNode + ButtonResponder)
  ├── SettingsMenu (MmRelayNode + MenuPanel)
  │     ├── VolumeSlider (MmRelayNode + SliderResponder)
  │     └── BackButton (MmRelayNode + ButtonResponder)
  └── GameplayUI (MmRelayNode + GameplayPanel)
```

### MenuController.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class MenuController : MmBaseResponder
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        // Deactivate all menus first
        relay.Send(false).ToChildren().Execute();
        // Then activate MainMenu specifically (by tag)
        relay.Send(true).ToChildren().WithTag(MmTag.Tag0).Execute();
    }

    public void ShowSettings()
    {
        relay.Send(false).ToChildren().Execute();
        relay.Send(true).ToChildren().WithTag(MmTag.Tag1).Execute();
    }

    // Child buttons notify parent when clicked
    protected override void ReceivedMessage(MmMessageString msg)
    {
        switch (msg.value)
        {
            case "PlayClicked":
                StartGame();
                break;
            case "SettingsClicked":
                ShowSettings();
                break;
            case "BackClicked":
                ShowMainMenu();
                break;
        }
    }

    void StartGame()
    {
        Debug.Log("Starting game...");
    }
}
```

### ButtonResponder.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class ButtonResponder : MmBaseResponder
{
    [SerializeField] private string buttonId = "Button";

    public void OnButtonClick()
    {
        // Notify parent that this button was clicked
        this.Send($"{buttonId}Clicked").ToParents().Execute();
    }

    protected override void ReceivedMessage(MmMessageBool msg)
    {
        // Show/hide based on message
        gameObject.SetActive(msg.value);
    }
}
```

---

## Combining Filters

You can combine multiple filters in a single message:

### DSL Syntax

```csharp
// Send to active children with Tag0
relay.Send("Hello")
    .ToChildren()
    .Active()
    .WithTag(MmTag.Tag0)
    .Execute();

// Send to all descendants, include inactive, over network
relay.Send("Sync")
    .ToDescendants()
    .IncludeInactive()
    .NetworkOnly()
    .Execute();
```

### Traditional Syntax

```csharp
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
```

---

## Common Routing Patterns

### Pattern 1: Broadcast to Subtree

Send initialization to all descendants:

```csharp
// DSL (recommended)
relay.BroadcastInitialize();

// Or explicitly
relay.Send(MmMethod.Initialize).ToDescendants().Execute();
```

### Pattern 2: Notify Parent

Child reports completion to parent:

```csharp
// From responder
this.NotifyComplete();

// Or explicitly
this.Send("TaskDone").ToParents().Execute();
```

### Pattern 3: Sibling Communication

Notify siblings (share same parent):

```csharp
relay.Send("SiblingUpdate").ToSiblings().Execute();
```

### Pattern 4: Full Propagation

Send to entire hierarchy:

```csharp
relay.Send("GlobalRefresh").ToAll().Execute();
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Message goes wrong direction | Check your level filter (ToChildren vs ToParents) |
| Inactive objects not receiving | Use `.IncludeInactive()` or `MmActiveFilter.All` |
| Message not reaching grandchildren | Use `.ToDescendants()` instead of `.ToChildren()` |
| Forgot `.Execute()` | DSL chains require `.Execute()` at the end (except Tier 1 methods) |

---

## Try This

Test your routing knowledge:

1. **Compare ToChildren vs ToDescendants** - Create a 3-level hierarchy (Parent → Child → Grandchild). Send a message using `.ToChildren()` and note which objects receive it. Then try `.ToDescendants()`. What's the difference?

2. **Send to inactive objects** - Deactivate one child GameObject, then send a message with `.IncludeInactive()`. Does the inactive object receive it?

3. **Create a sibling communication pattern** - Set up 3 children under the same parent. Have one child send a message using `.ToSiblings()`. Which objects receive it?

4. **Build the menu system** - Implement the UI Menu System example above. Add keyboard shortcuts to switch between menus.

---

## Next Steps

- **[Tutorial 3: Custom Responders](Tutorial-3-Custom-Responders)** - Handle custom methods
- **[Tutorial 4: Custom Messages](Tutorial-4-Custom-Messages)** - Create custom message types
- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Complete DSL reference

---

*Tutorial 2 of 14 - MercuryMessaging Wiki*
