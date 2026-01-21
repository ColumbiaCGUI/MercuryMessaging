# Tutorial 1: Introduction to MercuryMessaging

## Overview

MercuryMessaging is a hierarchical message-passing framework for Unity that enables loosely-coupled communication between GameObjects. Instead of direct component references, you send messages that flow through the scene hierarchy—making your code more modular, testable, and maintainable.

## What You'll Learn

- The core components: **MmRelayNode** and **MmResponder**
- How messages flow through the hierarchy
- Basic message sending with both Traditional and DSL APIs
- Setting up your first messaging hierarchy

## Prerequisites

- Unity 2021.3 or later
- Basic C# knowledge
- MercuryMessaging package installed

## Quick Start (Copy-Paste)

Here's the simplest possible example:

```csharp
using UnityEngine;
using MercuryMessaging;

public class MyFirstResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString message)
    {
        Debug.Log("Received: " + message.value);
    }
}
```

Attach this to a GameObject with an `MmRelayNode` component, and you can receive string messages!

**Expected Output (Console):**
```
Received: Hello World!
```

---

## Core Concepts

### 1. MmRelayNode - The Message Router

`MmRelayNode` is the central component that routes messages through your hierarchy. Think of it as a "mailroom" that knows how to deliver messages to the right recipients.

**Key Responsibilities:**
- Routes messages to child responders
- Forwards messages up/down the hierarchy
- Applies filters (level, active state, tags)

### 2. MmResponder - The Message Handler

`MmResponder` (and its subclass `MmBaseResponder`) components receive and react to messages. They're like "mailboxes" that process incoming mail.

**Key Responsibilities:**
- Receives messages from the relay node
- Processes messages based on type (string, int, float, etc.)
- Can send messages to other parts of the hierarchy

---

## Step-by-Step Guide

### Step 1: Create the Hierarchy

Create the following GameObject structure in your scene:

```
ParentNode (with MmRelayNode)
  └── ChildResponder (with MmRelayNode + MyResponder)
```

1. Create an empty GameObject named "ParentNode"
2. Add an `MmRelayNode` component
3. Create a child GameObject named "ChildResponder"
4. Add an `MmRelayNode` component to it
5. Add your custom responder script (see Step 2)

### Step 2: Create a Simple Responder

Create a new C# script called `MyResponder.cs`:

```csharp
using UnityEngine;
using MercuryMessaging;

public class MyResponder : MmBaseResponder
{
    // Called when a string message is received
    protected override void ReceivedMessage(MmMessageString message)
    {
        Debug.Log($"[{gameObject.name}] Received string: {message.value}");
    }

    // Called when an int message is received
    protected override void ReceivedMessage(MmMessageInt message)
    {
        Debug.Log($"[{gameObject.name}] Received int: {message.value}");
    }

    // Called when Initialize message is received
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized!");
    }
}
```

**Expected Output (when messages are received):**
```
[ChildResponder] Received string: Hello from parent!
[ChildResponder] Received int: 42
[ChildResponder] Initialized!
```

### Step 3: Send Messages (Traditional API)

To send messages from the parent, add this script to ParentNode:

```csharp
using UnityEngine;
using MercuryMessaging;

public class MessageSender : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void Update()
    {
        // Press Space to send a string message to children
        if (Input.GetKeyDown(KeyCode.Space))
        {
            relay.MmInvoke(MmMethod.MessageString, "Hello from parent!");
        }

        // Press I to send Initialize to all children
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.MmInvoke(MmMethod.Initialize);
        }
    }
}
```

**Expected Output (press Space then I):**
```
[ChildResponder] Received string: Hello from parent!
[ChildResponder] Initialized!
```

### Step 4: Send Messages (DSL API - Recommended)

The **Fluent DSL** provides a cleaner, more readable syntax:

```csharp
using UnityEngine;
using MercuryMessaging;

public class MessageSenderDSL : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void Update()
    {
        // Press Space to send string to children (DSL)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            relay.Send("Hello from parent!").ToChildren().Execute();
        }

        // Press I to initialize all descendants (DSL)
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.BroadcastInitialize();  // Even simpler!
        }

        // Press N to send a number
        if (Input.GetKeyDown(KeyCode.N))
        {
            relay.Send(42).ToChildren().Execute();
        }
    }
}
```

**Expected Output (press Space, I, then N):**
```
[ChildResponder] Received string: Hello from parent!
[ChildResponder] Initialized!
[ChildResponder] Received int: 42
```

---

## Fluent DSL Preview

The Fluent DSL reduces code by **77%** while adding type safety. Here's a quick comparison:

### Traditional API (7+ lines)
```csharp
var metadata = new MmMetadataBlock(
    MmLevelFilter.Child,
    MmActiveFilter.Active,
    MmSelectedFilter.All,
    MmNetworkFilter.Local
);
relay.MmInvoke(MmMethod.MessageString, "Hello", metadata);
```

### Fluent DSL (1 line)
```csharp
relay.Send("Hello").ToChildren().Active().Execute();
```

### Auto-Execute Methods (Shortest)
```csharp
relay.BroadcastInitialize();     // Initialize all children
relay.BroadcastValue("Hello");   // Send string to children
relay.NotifyComplete();          // Notify parents of completion
```

**See [Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API) for complete DSL coverage.**

---

## Complete Example

Here's a complete working example you can copy-paste:

**ParentController.cs** (attach to parent with MmRelayNode):
```csharp
using UnityEngine;
using MercuryMessaging;

public class ParentController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Initialize all children on start
        relay.BroadcastInitialize();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            relay.Send("Message 1").ToChildren().Execute();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            relay.Send(100).ToChildren().Execute();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            relay.BroadcastRefresh();
    }
}
```

**ChildResponder.cs** (attach to child with MmRelayNode):
```csharp
using UnityEngine;
using MercuryMessaging;

public class ChildResponder : MmBaseResponder
{
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"{name}: Initialized");
    }

    public override void Refresh(System.Collections.Generic.List<MmTransform> transformList)
    {
        Debug.Log($"{name}: Refreshed");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"{name}: Got string '{msg.value}'");
    }

    protected override void ReceivedMessage(MmMessageInt msg)
    {
        Debug.Log($"{name}: Got int {msg.value}");
    }
}
```

**Expected Output (on Play, then press 1, 2, 3):**
```
ChildResponder: Initialized
ChildResponder: Got string 'Message 1'
ChildResponder: Got int 100
ChildResponder: Refreshed
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Message not received | Ensure both parent and child have `MmRelayNode` components |
| Responder not triggered | Check that your responder inherits from `MmBaseResponder` |
| Wrong message type | Override the correct `ReceivedMessage()` overload for your data type |
| Hierarchy not connected | Responders must be children of the sending relay node's GameObject |

---

## Understanding the Hierarchy

Messages flow through the Unity hierarchy structure:

```
RootNode (MmRelayNode)
  ├── ChildA (MmRelayNode + ResponderA)
  │     └── GrandchildA (MmRelayNode + ResponderGrandA)
  └── ChildB (MmRelayNode + ResponderB)
```

When `RootNode` sends a message:
- **ToChildren()** → ChildA, ChildB receive it
- **ToDescendants()** → ChildA, ChildB, GrandchildA all receive it
- **ToParents()** → Nothing (RootNode has no parent)

---

## Try This

Test your understanding with these exercises:

1. **Add a second child responder** - Create another child GameObject with its own responder. Does it also receive messages when the parent sends?

2. **Try different message types** - Modify `ParentController` to send a `float` value using `relay.Send(3.14f).ToChildren().Execute()`. Add the corresponding handler in `ChildResponder`.

3. **Add a grandchild** - Create a grandchild under ChildResponder. Compare what happens when you use `.ToChildren()` vs `.ToDescendants()`.

4. **Send messages from child to parent** - In `ChildResponder`, add a key handler that sends `relay.NotifyValue("Child says hi!")` to the parent. Add a handler in `ParentController` to receive it.

---

## Next Steps

- **[Tutorial 2: Basic Routing](Tutorial-2-Basic-Routing)** - Learn about level filters and message direction
- **[Tutorial 3: Custom Responders](Tutorial-3-Custom-Responders)** - Create responders for custom methods
- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Master the modern DSL syntax

---

*Tutorial 1 of 14 - MercuryMessaging Wiki*
