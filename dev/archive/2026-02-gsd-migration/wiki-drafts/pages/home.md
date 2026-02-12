# MercuryMessaging Wiki

Welcome to the MercuryMessaging Wiki - your guide to building message-driven Unity applications.

---

## What's New (2025)

### Fluent DSL API
77% less code with the new chainable API:
```csharp
// Before: 7 lines of boilerplate
// After:
relay.Send("Hello").ToChildren().Execute();
```
**[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)**

### FishNet Integration
Full support for FishNet networking with path-based IDs for scene objects.
**[Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)**

### Photon Fusion 2
Updated backend for Photon Fusion 2 with NetworkObject integration.
**[Tutorial 7: Fusion 2 Networking](Tutorial-7-Fusion2-Networking)**

### Source Generators
Compile-time dispatch generation for optimal performance:
```csharp
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder { }
```
**[Tutorial 14: Performance Optimization](Tutorial-14-Performance)**

### [MmHandler] Attribute
Cleaner custom method handling without switch statements:
```csharp
[MmHandler((MmMethod)1001)]
private void OnCustomMethod(MmMessage msg) { }
```
**[Tutorial 3: Custom Responders](Tutorial-3-Custom-Responders)**

---

## Quick Start

### 1. Install MercuryMessaging
Copy the `Assets/MercuryMessaging/` folder to your Unity project.

### 2. Add Components
```
YourGameObject
├── MmRelayNode      ← Message router
└── YourResponder    ← Handles messages
```

### 3. Send Messages
```csharp
using MercuryMessaging;

public class MyController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        relay.BroadcastInitialize();  // Initialize all descendants
    }

    void OnPlayerAction()
    {
        relay.Send("PlayerMoved")
            .ToDescendants()
            .Active()
            .Execute();
    }
}
```

### 4. Receive Messages
```csharp
public class MyResponder : MmBaseResponder
{
    protected override void ReceivedInitialize()
    {
        Debug.Log("Initialized!");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"Received: {msg.value}");
    }
}
```

---

## Tutorials

### Foundation (Start Here)
| # | Title | Description |
|---|-------|-------------|
| 1 | [Introduction](Tutorial-1-Introduction) | Core concepts, first message |
| 2 | [Basic Routing](Tutorial-2-Basic-Routing) | Hierarchy, direction, filters |
| 3 | [Custom Responders](Tutorial-3-Custom-Responders) | Handle custom methods |
| 4 | [Custom Messages](Tutorial-4-Custom-Messages) | Create message types |
| 5 | [Fluent DSL API](Tutorial-5-Fluent-DSL-API) | Modern API (recommended) |

### Networking
| # | Title | Description |
|---|-------|-------------|
| 6 | [FishNet](Tutorial-6-FishNet-Networking) | FishNet integration |
| 7 | [Fusion 2](Tutorial-7-Fusion2-Networking) | Photon Fusion 2 |
| 11 | [Advanced Networking](Tutorial-11-Advanced-Networking) | Custom backends |

### Advanced
| # | Title | Description |
|---|-------|-------------|
| 8 | [Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM) | State machines |
| 9 | [Task Management](Tutorial-9-Task-Management) | Experiments & studies |
| 10 | [Application State](Tutorial-10-Application-State) | Global state |

### Specialized
| # | Title | Description |
|---|-------|-------------|
| 12 | [VR Experiment](Tutorial-12-VR-Experiment) | Go/No-Go task |
| 13 | [Spatial & Temporal](Tutorial-13-Spatial-Temporal) | Coming soon |
| 14 | [Performance](Tutorial-14-Performance) | Optimization |

**[Full Tutorial Index](Tutorials)**

---

## Key Concepts

### Message Flow
Messages flow through Unity's hierarchy:
```
Parent ← MmLevelFilter.Parent
   │
   └── Current Node ← MmLevelFilter.Self
         │
         └── Children ← MmLevelFilter.Child
               │
               └── Descendants ← Recursive
```

### Filtering System
Target exactly the responders you need:
- **Level**: Parent, Child, Descendants, Ancestors, All
- **Active**: Only active GameObjects
- **Tag**: Filter by Tag0-Tag7
- **Selected**: FSM-selected only
- **Network**: Local, Network, or Both

### Core Components
| Component | Purpose |
|-----------|---------|
| `MmRelayNode` | Routes messages through hierarchy |
| `MmBaseResponder` | Handles incoming messages |
| `MmRelaySwitchNode` | FSM-enabled router |
| `MmExtendableResponder` | Dynamic handler registration |
| `MmNetworkBridge` | Network message transport |

---

## API Reference

### Fluent DSL (Recommended)
```csharp
// Auto-execute (broadcasts down, notifies up)
relay.BroadcastInitialize();
relay.NotifyComplete();

// Fluent chains
relay.Send("Hello").ToChildren().Execute();
relay.Send(42).ToDescendants().Active().WithTag(MmTag.Tag0).Execute();
```

### Traditional API
```csharp
relay.MmInvoke(MmMethod.Initialize);
relay.MmInvoke(MmMethod.MessageString, "Hello",
    new MmMetadataBlock(MmLevelFilter.Child));
```

---

## Getting Help

- **[Tutorials](Tutorials)** - Step-by-step learning
- **[API Reference](../Documentation/API_REFERENCE.md)** - Technical details
- **[GitHub Issues](https://github.com/ColumbiaCGUI/MercuryMessaging/issues)** - Bug reports
- **[CGUI Lab](https://graphics.cs.columbia.edu/)** - Research group

---

## License

MercuryMessaging is developed by [Columbia University CGUI Lab](https://graphics.cs.columbia.edu/).

---

*Last Updated: December 2025*
