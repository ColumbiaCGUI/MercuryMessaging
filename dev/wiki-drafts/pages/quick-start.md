# Quick Start (2 Minutes)

Get MercuryMessaging working in your Unity project in under 2 minutes.

---

## Step 1: Install (30 seconds)

Copy `Assets/MercuryMessaging/` to your project's Assets folder.

---

## Step 2: Create Hierarchy (30 seconds)

Create this structure in your scene:

```
MessageHub (Empty GameObject)
├── MmRelayNode (component)
└── Child (Empty GameObject)
    ├── MmRelayNode (component)
    └── QuickResponder (script below)
```

---

## Step 3: Add This Script (30 seconds)

Create `QuickResponder.cs`:

```csharp
using UnityEngine;
using MercuryMessaging;

public class QuickResponder : MmBaseResponder
{
    protected override void ReceivedInitialize()
    {
        Debug.Log($"[{name}] Initialized!");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"[{name}] Received: {msg.value}");
    }
}
```

---

## Step 4: Send Messages (30 seconds)

Add `QuickSender.cs` to MessageHub:

```csharp
using UnityEngine;
using MercuryMessaging;

public class QuickSender : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        relay.BroadcastInitialize();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            relay.Send("Hello from parent!")
                .ToDescendants()
                .Execute();
        }
    }
}
```

---

## Step 5: Run & Test

1. Press Play
2. Press Space

**Expected Output (Console):**
```
[Child] Initialized!
[Child] Received: Hello from parent!
```

---

## You Did It!

Messages are flowing through your hierarchy. Now explore:

- **[Tutorial 1: Introduction](Tutorial-1-Introduction)** - Core concepts explained
- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Modern API features
- **[Home](Home)** - Full documentation

---

*MercuryMessaging - Hierarchical Message Routing for Unity*
