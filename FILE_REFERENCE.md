# MercuryMessaging File Reference

This document provides a quick reference to the most important files in the MercuryMessaging framework, organized by category.

For complete documentation, see [CLAUDE.md](CLAUDE.md).

---

## Core Protocol (Critical Files)

These are the most important files in the framework. Understanding these is essential for working with MercuryMessaging.

| File | Lines | Purpose |
|------|-------|---------|
| `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs` | ~1500 | **Central message router** - manages routing table, filters, hierarchy. The most important class in the framework. |
| `Assets/MercuryMessaging/Protocol/Responders/MmBaseResponder.cs` | ~400 | Base responder with method routing (SetActive, Initialize, Switch, etc.). Extend this class for standard message handling. |
| `Assets/MercuryMessaging/Protocol/Responders/MmResponder.cs` | ~130 | Abstract base implementing IMmResponder with lifecycle management. Foundation for all responders. |
| `Assets/MercuryMessaging/Protocol/IMmResponder.cs` | ~30 | Core interface defining the messaging contract. All responders implement this. |
| `Assets/MercuryMessaging/Protocol/Nodes/MmRelaySwitchNode.cs` | ~200 | Relay node with FSM capabilities for state management. Used for state-based applications. |
| `Assets/MercuryMessaging/Protocol/MmSwitchResponder.cs` | ~150 | Controller for MmRelaySwitchNode with state transitions. Manages FSM state changes. |
| `Assets/MercuryMessaging/Protocol/Responders/MmExtendableResponder.cs` | ~200 | Base responder with registration-based custom method handling. Preferred for custom methods (>= 1000). |

---

## Message System

Message types and message infrastructure.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs` | Base message class with method, metadata, and network support. All messages derive from this. |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageBool.cs` | Boolean message type (true/false values) |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageInt.cs` | Integer message type |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageFloat.cs` | Float message type |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageString.cs` | String message type |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageVector3.cs` | Vector3 message type (Unity coordinates) |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageTransform.cs` | Transform message type (position, rotation, scale) |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageGameObject.cs` | GameObject reference message |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageByteArray.cs` | Byte array for custom serialization |
| `Assets/MercuryMessaging/Protocol/Message/MmMessageSerializable.cs` | Generic serializable message |

---

## Filtering and Routing

Filter types and routing configuration.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs` | Routing control parameters (contains all filters). Controls message targeting. |
| `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs` | Collection of responders with filtering capabilities. Manages registered responders. |
| `Assets/MercuryMessaging/Protocol/MmRoutingTableItem.cs` | Individual responder entry in routing table with delegate support. |
| `Assets/MercuryMessaging/Protocol/MmLevelFilter.cs` | Direction filter (Parent/Child/Self combinations). Controls hierarchy navigation. |
| `Assets/MercuryMessaging/Protocol/MmActiveFilter.cs` | Active GameObject filter (Active only vs All). Filters by active state. |
| `Assets/MercuryMessaging/Protocol/MmSelectedFilter.cs` | FSM selection filter (Selected vs All). Used with MmRelaySwitchNode. |
| `Assets/MercuryMessaging/Protocol/MmNetworkFilter.cs` | Network message filter (Local/Network/All). Controls network routing. |
| `Assets/MercuryMessaging/Protocol/MmTag.cs` | Multi-tag system (8 flags: Tag0-Tag7). Bitwise tag filtering. |
| `Assets/MercuryMessaging/Protocol/MmMethod.cs` | Standard method enum (0-18 + custom 1000+). Message method identifiers. |

---

## Core Performance (Hot-Path Optimized)

Performance-critical code for message pooling and dispatch.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Protocol/Core/MmMessagePool.cs` | ObjectPool implementation for zero-allocation message handling. |
| `Assets/MercuryMessaging/Protocol/Core/MmHashSetPool.cs` | Pool for VisitedNodes HashSets used in cycle detection. |

---

## Fluent DSL API

Chainable API for simplified message sending with 86% verbosity reduction.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Protocol/DSL/MmFluentMessage.cs` | Core fluent message builder with Execute() method. |
| `Assets/MercuryMessaging/Protocol/DSL/MmMessagingExtensions.cs` | Send() extensions for MmRelayNode and MmBaseResponder. |
| `Assets/MercuryMessaging/Protocol/DSL/MmRelayNodeExtensions.cs` | Broadcast/Notify auto-execute methods for relay nodes. |
| `Assets/MercuryMessaging/Protocol/DSL/MmResponderExtensions.cs` | Broadcast/Notify auto-execute methods for responders. |
| `Assets/MercuryMessaging/Protocol/DSL/MmFluentFilters.cs` | Active(), WithTag(), Where() filter extensions. |
| `Assets/MercuryMessaging/Protocol/DSL/MmFluentPredicates.cs` | OfType(), Named() predicate extensions. |
| `Assets/MercuryMessaging/Protocol/DSL/MmRoutingExtensions.cs` | ToChildren(), ToParents(), ToSiblings() routing extensions. |
| `Assets/MercuryMessaging/Protocol/DSL/MmTemporalExtensions.cs` | After(), Every(), When() temporal extensions. |
| `Assets/MercuryMessaging/Protocol/DSL/MmQueryExtensions.cs` | Query/Response pattern extensions. |
| `Assets/MercuryMessaging/Protocol/DSL/MmListener.cs` | Callback-based message listener support. |

---

## Standard Library

Pre-defined message types for common use cases.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/StandardLibrary/UI/MmUIMessages.cs` | UI messages (Click, Hover, Drag, Scroll, Focus, Select, Submit, Cancel). |
| `Assets/MercuryMessaging/StandardLibrary/UI/MmUIResponder.cs` | Base responder for UI message handling. |
| `Assets/MercuryMessaging/StandardLibrary/Input/MmInputMessages.cs` | VR input messages (6DOF, Gesture, Haptic, Button, Axis, Touch, Gaze). |
| `Assets/MercuryMessaging/StandardLibrary/Input/MmInputResponder.cs` | Base responder for VR input message handling. |

---

## Task Management

User study and experimental task management system.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Task/MmTaskManager.cs` | Generic task management with progression tracking. Manages task sequences. |
| `Assets/MercuryMessaging/Task/MmTaskInfo.cs` | Basic task information. Contains task metadata. |
| `Assets/MercuryMessaging/Task/MmTaskResponder.cs` | Responder handling task-specific logic. Processes task messages. |
| `Assets/MercuryMessaging/Task/IMmTaskInfo.cs` | Task information interface. Contract for task data. |
| `Assets/MercuryMessaging/Task/Transformation/MmTransformationTaskInfo.cs` | Transform-specific tasks. Task info for object manipulation. |
| `Assets/MercuryMessaging/Task/Transformation/MmTransformationTaskResponder.cs` | Transform task responder. Handles transformation messages. |
| `Assets/MercuryMessaging/Task/IMmTaskInfoCollectionLoader.cs` | Task collection loader interface. Loads task definitions. |
| `Assets/MercuryMessaging/Task/MmTaskInfoCollectionFileLoader.cs` | File-based task collection loader. Loads tasks from XML/JSON. |

---

## Support Systems

Utilities, data collection, and helper systems.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Support/FiniteStateMachine/FiniteStateMachine.cs` | Generic FSM with Enter/Exit events. Core FSM implementation. |
| `Assets/MercuryMessaging/Support/FiniteStateMachine/StateEvents.cs` | State transition event handlers. Event hooks for state changes. |
| `Assets/MercuryMessaging/Support/Data/MmDataCollector.cs` | Data collection management. Records experimental data. |
| `Assets/MercuryMessaging/Support/Data/MmDataHandlerCsv.cs` | CSV file handler. Exports data to CSV format. |
| `Assets/MercuryMessaging/Support/Data/MmDataHandlerXml.cs` | XML file handler. Exports data to XML format. |
| `Assets/MercuryMessaging/Support/Data/MmCircularBuffer.cs` | Bounded circular buffer. Memory-efficient collection for message history. |
| `Assets/MercuryMessaging/Support/Extensions/GameObjectExtensions.cs` | GameObject helper methods. Utility extensions for GameObjects. |
| `Assets/MercuryMessaging/Support/Extensions/TransformExtensions.cs` | Transform helper methods. Utility extensions for Transforms. |
| `Assets/MercuryMessaging/Support/Interpolators/Interpolator.cs` | Base interpolator for animations. Smooth value transitions. |
| `Assets/MercuryMessaging/Protocol/MmLogger.cs` | Logging system with category filters. Framework logging with enable/disable categories. |

---

## Network Support

Network message synchronization with modular backend architecture.

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Protocol/IMmNetworkResponder.cs` | Network responder interface. Contract for networked responders. |
| `Assets/MercuryMessaging/Protocol/MmNetworkResponder.cs` | Base network responder. Foundation for networked message handling. |
| `Assets/MercuryMessaging/Protocol/MmNetworkResponderPhoton.cs` | Photon networking integration (legacy). Photon-specific network implementation. |
| `Assets/MercuryMessaging/Protocol/Network/IMmNetworkBackend.cs` | Network backend interface. Contract for pluggable networking. |
| `Assets/MercuryMessaging/Protocol/Network/MmNetworkBridge.cs` | Bridge between MercuryMessaging and network backends. |
| `Assets/MercuryMessaging/Protocol/Network/MmBinarySerializer.cs` | Binary message serialization for network transport. |
| `Assets/MercuryMessaging/Protocol/Network/MmLoopbackBackend.cs` | Local testing backend (no network). |
| `Assets/MercuryMessaging/Protocol/Network/Backends/Pun2Backend.cs` | PUN2 backend implementation. |

---

## Testing

Test files for automated testing and validation (200+ tests total).

| File | Purpose |
|------|---------|
| `Assets/MercuryMessaging/Tests/CircularBufferTests.cs` | CircularBuffer implementation tests (30+ tests). Validates bounded memory behavior. |
| `Assets/MercuryMessaging/Tests/CircularBufferMemoryTests.cs` | Memory stability validation for CircularBuffer (QW-4). Long-running memory tests. |
| `Assets/MercuryMessaging/Tests/HopLimitValidationTests.cs` | Hop limit enforcement tests (QW-1). Validates message depth limits. |
| `Assets/MercuryMessaging/Tests/CycleDetectionValidationTests.cs` | Cycle detection tests (QW-1). Validates infinite loop prevention. |
| `Assets/MercuryMessaging/Tests/LazyCopyValidationTests.cs` | Lazy copying optimization tests (QW-2). Validates copy-on-demand behavior. |
| `Assets/MercuryMessaging/Tests/FilterCacheValidationTests.cs` | Filter cache optimization tests (QW-3). Validates filter caching. |
| `Assets/MercuryMessaging/Tests/LinqRemovalValidationTests.cs` | LINQ removal optimization tests (QW-5). Validates allocation reductions. |
| `Assets/MercuryMessaging/Tests/FsmStateTransitionTests.cs` | FSM state transition tests (20 tests). Comprehensive FSM validation. |
| `Assets/MercuryMessaging/Tests/MmExtendableResponderTests.cs` | Extendable responder tests. Validates custom method registration. |
| `Assets/MercuryMessaging/Tests/MmExtendableResponderIntegrationTests.cs` | Integration tests for extendable responder. End-to-end validation. |
| `Assets/MercuryMessaging/Tests/FluentApiTests.cs` | Core fluent API tests. Validates DSL syntax. |
| `Assets/MercuryMessaging/Tests/FluentApiIntegrationTests.cs` | DSL integration tests. End-to-end fluent API validation. |
| `Assets/MercuryMessaging/Tests/FluentApiPhase2Tests.cs` | Phase 2 DSL tests. Advanced routing validation. |
| `Assets/MercuryMessaging/Tests/FluentApiPhase3Tests.cs` | Phase 3 DSL tests. Spatial and temporal extensions. |
| `Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs` | Advanced routing tests. Siblings, cousins, descendants. |
| `Assets/MercuryMessaging/Tests/MmListenerTests.cs` | Listener callback tests. Validates callback-based messaging. |
| `Assets/MercuryMessaging/Tests/MmMessagePoolTests.cs` | Message pool tests. Validates zero-allocation messaging. |
| `Assets/MercuryMessaging/Tests/RoutingTableIndexTests.cs` | O(1) routing table tests. Validates dictionary-based lookup. |
| `Assets/MercuryMessaging/Tests/DelegateDispatchTests.cs` | Delegate dispatch tests. Validates fast handler invocation. |
| `Assets/MercuryMessaging/Tests/MmBinarySerializerTests.cs` | Binary serializer tests. Validates network serialization. |
| `Assets/MercuryMessaging/Tests/StandardLibrary/UIMessageTests.cs` | UI message tests. Validates standard library UI messages. |
| `Assets/MercuryMessaging/Tests/StandardLibrary/InputMessageTests.cs` | Input message tests. Validates standard library VR input messages. |

---

## Examples and Tutorials

Demo scenes and tutorial content.

| Location | Purpose |
|----------|---------|
| `Assets/MercuryMessaging/Examples/Demo/TrafficLights.unity` | Traffic light simulation demo. Real-world usage example. |
| `Assets/MercuryMessaging/Examples/Tutorials/SimpleScene/` | Basic light switch example. Simplest possible MercuryMessaging usage. |
| `Assets/MercuryMessaging/Examples/Tutorials/Tutorial1-5/` | Progressive tutorial series. Step-by-step learning path. |
| `Assets/MercuryMessaging/Examples/Tutorials/DSL/` | Fluent DSL API tutorials. Modern API usage examples. |
| `Assets/Project/Scripts/Tutorials/Tutorial4_ColorChanging/` | Custom method tutorial. Demonstrates MmExtendableResponder usage. |

---

## Documentation

Project documentation files.

| File | Purpose |
|------|---------|
| [CLAUDE.md](CLAUDE.md) | Main framework documentation. Architecture, workflows, performance characteristics. |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Development standards and guidelines. Dependency policy, naming conventions, testing standards. |
| [FILE_REFERENCE.md](FILE_REFERENCE.md) | This file. Quick reference to important files. |
| [dev/WORKFLOW.md](dev/WORKFLOW.md) | Development workflow documentation. Feature development, bug fixes, testing, releases. |
| [dev/IMPROVEMENT_TRACKER.md](dev/IMPROVEMENT_TRACKER.md) | Framework improvements tracker. Completed work, active development, research opportunities. |
| [dev/FREQUENT_ERRORS.md](dev/FREQUENT_ERRORS.md) | **Frequent errors & bug reference.** Common mistakes, debugging checklists, code patterns. CONSULT BEFORE WRITING ROUTING CODE. |
| [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md) | AI assistant guidelines. Git commit policy, task management for AI assistants. |

---

## Quick Navigation by Use Case

### "I want to send messages"
→ Start with `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs` (MmInvoke method)
→ See `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs` for filtering options
→ Use `relay.Send("value").ToChildren().Execute()` (Fluent DSL)

### "I want to receive messages"
→ Extend `Assets/MercuryMessaging/Protocol/Responders/MmBaseResponder.cs` for standard methods
→ Extend `Assets/MercuryMessaging/Protocol/Responders/MmExtendableResponder.cs` for custom methods (>= 1000)

### "I want to use FSM for state management"
→ Use `Assets/MercuryMessaging/Protocol/Nodes/MmRelaySwitchNode.cs` as parent
→ See `Assets/MercuryMessaging/Support/FiniteStateMachine/FiniteStateMachine.cs` for FSM details

### "I want to create custom message types"
→ Extend `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs`
→ See `Assets/MercuryMessaging/Protocol/Message/MmMessageInt.cs` for example

### "I want to filter messages by tags"
→ See `Assets/MercuryMessaging/Protocol/MmTag.cs` for tag system
→ Set `Tag` and `TagCheckEnabled` in responders
→ Use `relay.Send(value).ToChildren().WithTag(MmTag.Tag0).Execute()` (Fluent DSL)

### "I want to use the Fluent DSL API"
→ See `Assets/MercuryMessaging/Protocol/DSL/README.md` for overview
→ See `Assets/MercuryMessaging/Protocol/DSL/DSL_API_GUIDE.md` for full reference

### "I want to understand performance"
→ See `Documentation/Performance/OPTIMIZATION_RESULTS.md`
→ See CLAUDE.md "Performance Characteristics" section

### "I want to use source generators for faster dispatch"
→ See `SourceGenerators/README.md` for setup and usage
→ Add `[MmGenerateDispatch]` attribute to responder classes
→ Mark class as `partial`
→ Add `RoslynAnalyzer` label to `Assets/MercuryMessaging/Protocol/Analyzers/MercuryMessaging.Generators.dll`

### "I want to write tests"
→ See `Assets/MercuryMessaging/Tests/CircularBufferTests.cs` for pattern examples
→ See CONTRIBUTING.md "Testing Standards Policy"

### "I want to contribute code"
→ Read [CONTRIBUTING.md](CONTRIBUTING.md) first
→ Follow [dev/WORKFLOW.md](dev/WORKFLOW.md) for development process

---

## File Naming Conventions

### Core Framework Files (require "Mm" prefix)
- `MmClassName.cs` - Classes in Protocol/, AppState/, Support/Data/, Task/
- `IMmInterfaceName.cs` - Interfaces in core folders
- `MmStructName.cs` - Structs in core folders
- `MmEnumName.cs` - Enums in core folders

### Non-Core Files (no "Mm" prefix required)
- `Support/Extensions/*.cs` - Generic C# extensions
- `Support/FiniteStateMachine/*.cs` - Generic FSM implementation
- `Support/Interpolators/*.cs` - Generic animation utilities
- `Examples/**/*.cs` - Tutorial and demo code
- `Tests/**/*.cs` - Test files

See [CONTRIBUTING.md](CONTRIBUTING.md) for complete naming convention policy.

---

## Getting Help

- **General Documentation**: [CLAUDE.md](CLAUDE.md)
- **Development Standards**: [CONTRIBUTING.md](CONTRIBUTING.md)
- **Development Workflow**: [dev/WORKFLOW.md](dev/WORKFLOW.md)
- **Improvement Tracker**: [dev/IMPROVEMENT_TRACKER.md](dev/IMPROVEMENT_TRACKER.md)
- **AI Assistant Guide**: [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md)

---

**Last Updated:** 2025-11-27
**Maintained By:** Framework Team
