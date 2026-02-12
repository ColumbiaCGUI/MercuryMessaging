# Tutorials Index

A complete guide to MercuryMessaging, from basics to advanced topics.

---

## Learning Path

**Recommended order for new users:**
1. Foundation (Tutorials 1-5) - Core concepts
2. Networking (Tutorial 6 or 7) - Multiplayer basics
3. Advanced (Tutorials 8-10) - State management
4. Specialized (Tutorial 12+) - Domain-specific

---

## Foundation (Tutorials 1-5)

Start here. These tutorials cover the core concepts every MercuryMessaging developer needs.

### Tutorial 1: Introduction to MercuryMessaging
**[View Tutorial](Tutorial-1-Introduction)**

Learn the core concepts of hierarchical message routing:
- What is MercuryMessaging?
- MmRelayNode and MmBaseResponder
- Sending your first message
- DSL preview

### Tutorial 2: Basic Routing
**[View Tutorial](Tutorial-2-Basic-Routing)**

Master message routing through Unity hierarchies:
- Direction targeting (Parents, Children, Descendants)
- Filtering (Active, Tag, Selected)
- MmMetadataBlock configuration
- Traditional vs DSL API comparison

### Tutorial 3: Creating Custom Responders
**[View Tutorial](Tutorial-3-Custom-Responders)**

Handle custom application-specific messages:
- Creating custom MmMethod values (1000+)
- Override patterns in MmBaseResponder
- MmExtendableResponder with RegisterCustomHandler()
- [MmHandler] attribute for cleaner code

### Tutorial 4: Creating Custom Messages
**[View Tutorial](Tutorial-4-Custom-Messages)**

Create your own message types with payloads:
- Extending MmMessage
- MmMessageType values (1100+)
- Implementing Copy() and Serialize()
- Network serialization best practices

### Tutorial 5: Fluent DSL API
**[View Tutorial](Tutorial-5-Fluent-DSL-API)**

The modern, recommended API for MercuryMessaging:
- 77% code reduction vs traditional API
- Tier 1: Auto-execute methods (BroadcastX, NotifyX)
- Tier 2: Fluent chains (.Send().ToX().Execute())
- Property-based routing (relay.To.Children.Send())
- Complete migration guide

---

## Networking (Tutorials 6-7, 11)

Multiplayer support with industry-standard networking solutions.

### Tutorial 6: Networking with FishNet
**[View Tutorial](Tutorial-6-FishNet-Networking)**

Free, open-source networking integration:
- Setup and configuration
- MmNetworkBridge with FishNetBackend
- Path-based IDs for scene objects
- Bidirectional messaging examples
- ParrelSync testing

### Tutorial 7: Networking with Photon Fusion 2
**[View Tutorial](Tutorial-7-Fusion2-Networking)**

Premium networking with Photon infrastructure:
- Fusion 2 setup and App ID
- MmFusion2Bridge NetworkBehaviour
- NetworkObject ID resolution
- Shared vs Client-Server modes
- Comparison with FishNet

### Tutorial 11: Advanced Networking
**[View Tutorial](Tutorial-11-Advanced-Networking)**

Deep dive into the network architecture:
- 3-layer architecture overview
- Binary serialization format (17-byte header)
- IMmNetworkBackend interface
- IMmGameObjectResolver implementation
- Creating custom backends
- Reliability options and debugging

---

## Advanced (Tutorials 8-10)

State management and application architecture.

### Tutorial 8: Switch Nodes & FSM
**[View Tutorial](Tutorial-8-Switch-Nodes-FSM)**

Finite state machines with MmRelaySwitchNode:
- FSM-enabled message routing
- SelectedFilter.Selected targeting
- State change events (GlobalEnter/GlobalExit)
- Navigation methods (JumpTo, GoToPrevious)
- Game state machine example

### Tutorial 9: Task Management
**[View Tutorial](Tutorial-9-Task-Management)**

Experiment workflows and user studies:
- MmTaskManager<T> overview
- IMmTaskInfo and IMmTaskInfoCollectionLoader
- JSON/CSV task definitions
- MmDataCollector integration
- Go/No-Go task architecture

### Tutorial 10: Application State Management
**[View Tutorial](Tutorial-10-Application-State)**

Global application state handling:
- MmSwitchResponder controller pattern
- MmAppStateResponder for state data
- Automatic state activation/deactivation
- InitialState configuration
- Menu → Gameplay → Pause flow

---

## Specialized (Tutorials 12-14)

Domain-specific tutorials for research and production.

### Tutorial 12: VR Behavioral Experiment
**[View Tutorial](Tutorial-12-VR-Experiment)**

Build a complete VR research application:
- XR Interaction Toolkit integration
- Stimulus presentation and timing
- VR input handling (controllers, buttons)
- Trial sequencing with MmTaskManager
- Data collection and export
- Go/No-Go cognitive task implementation

### Tutorial 13: Spatial & Temporal Filtering
**[View Tutorial](Tutorial-13-Spatial-Temporal)** *(Coming Soon)*

Advanced filtering by position and time:
- Within() - Radius-based targeting
- InCone() - Direction-based targeting
- After(), Every() - Temporal patterns
- Throttle(), Debounce() - Rate limiting

### Tutorial 14: Performance Optimization
**[View Tutorial](Tutorial-14-Performance)** *(Coming Soon)*

Maximize MercuryMessaging performance:
- PerformanceMode flag
- [MmGenerateDispatch] source generator
- [MmHandler] attribute
- Memory optimization (CircularBuffer, Lazy Copy)
- Benchmark results

---

## Quick Reference

### DSL Cheat Sheet

```csharp
// Broadcasts (down to descendants)
relay.BroadcastInitialize();
relay.BroadcastRefresh();
relay.BroadcastSetActive(true);
relay.BroadcastValue("Hello");

// Notifications (up to parents)
relay.NotifyComplete();
relay.NotifyValue(42);

// Fluent chains
relay.Send("msg").ToChildren().Execute();
relay.Send("msg").ToParents().Execute();
relay.Send("msg").ToDescendants().Active().Execute();
relay.Send("msg").ToAll().WithTag(MmTag.Tag0).Execute();
```

### Message Types

| Type | Example Value | MmMessageType |
|------|---------------|---------------|
| Void | (none) | MmVoid |
| Bool | true/false | MmBool |
| Int | 42 | MmInt |
| Float | 3.14f | MmFloat |
| String | "Hello" | MmString |
| Vector3 | new Vector3(1,2,3) | MmVector3 |

### Method Ranges

| Range | Purpose |
|-------|---------|
| 0-18 | Standard MmMethod |
| 100-199 | UI Messages |
| 200-299 | Input Messages |
| 1000+ | Custom Application |

---

## Additional Resources

- **[Home](Home)** - Wiki home page
- **[API Reference](../Documentation/API_REFERENCE.md)** - Technical reference
- **[Performance Guide](../Documentation/PERFORMANCE.md)** - Benchmarks
- **[Contributing](../CONTRIBUTING.md)** - Development guidelines

---

*MercuryMessaging Wiki - December 2025*
