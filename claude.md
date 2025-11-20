# MercuryMessaging Framework Documentation

## Project Overview

**MercuryMessaging** is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture, eliminating the need for direct component references.

### Key Features
- **Hierarchical Message Routing**: Messages flow through Unity's scene graph structure
- **Multi-Level Filtering**: Target messages by level (parent/child), active state, tags, and network status
- **Built-in Networking**: Automatic message serialization for networked applications
- **FSM Integration**: First-class support for finite state machines
- **Task Management**: System for managing experimental workflows and user studies
- **VR/XR Ready**: Compatible with Unity XR Interaction Toolkit

---

## Directory Structure

### Project Root Structure (Assets/)

```
Assets/
├── _Project/                    # Custom project-specific assets (NEW - organized)
│   ├── Scenes/                  # Production scenes
│   ├── Scripts/                 # All custom scripts (organized by category)
│   │   ├── Core/                # Core application logic
│   │   ├── UI/                  # UI-related scripts
│   │   ├── VR/                  # VR/XR initialization scripts
│   │   ├── Utilities/           # General utility scripts
│   │   ├── Responders/          # Custom MercuryMessaging responders
│   │   ├── TrafficLights/       # Traffic light system scripts
│   │   └── Tutorials/           # Tutorial scripts (from original Script/)
│   ├── Prefabs/                 # Reusable prefab assets
│   │   ├── UI/                  # UI prefabs
│   │   └── Environment/         # Environment prefabs
│   ├── Materials/               # Project-specific materials
│   ├── Resources/               # Runtime-loadable assets
│   └── Settings/                # Project configuration files
│
├── MercuryMessaging/            # Core messaging framework (109 C# scripts)
│   ├── AppState/                # Application state management with FSM
│   ├── Protocol/                # Core messaging protocol (MOST IMPORTANT)
│   │   └── Message/             # Message type definitions
│   ├── Support/                 # Supporting utilities and systems
│   │   ├── Data/                # Data collection and CSV/XML handling
│   │   ├── Editor/              # Custom Unity editor utilities
│   │   ├── Extensions/          # C# extension methods
│   │   ├── FiniteStateMachine/  # Generic FSM implementation
│   │   ├── GUI/                 # GUI utilities and responders
│   │   ├── Input/               # Input handling (keyboard)
│   │   ├── Interpolators/       # Animation/interpolation utilities
│   │   └── ThirdParty/          # Third-party integration utilities
│   ├── Task/                    # Task management system for experiments
│   │   └── Transformation/      # Transform-specific task implementations
│   └── Examples/                # Demo and tutorial content (REORGANIZED)
│       ├── Demo/                # Demo scenes (TrafficLights.unity)
│       └── Tutorials/           # Tutorial scenes and examples
│           ├── SimpleScene/     # Basic light switch example
│           ├── SimpleTutorial_Alternative/
│           └── Tutorial1-5/     # Progressive tutorial series
│
├── UserStudy/                   # User study scenes, scripts, and assets
│
├── ThirdParty/                  # All third-party assets (NEW - consolidated)
│   ├── Plugins/                 # Third-party plugins and libraries
│   │   ├── ALINE/               # Debug drawing library
│   │   ├── EasyPerformantOutline/  # Outline effect system
│   │   ├── QuickOutline/        # Quick outline effect
│   │   ├── MKGlowFree/          # Glow effect system (formerly _MK/)
│   │   ├── Photon/              # Photon Fusion networking
│   │   ├── Android/             # Android-specific plugins
│   │   └── Vuplex/              # WebView plugin
│   ├── AssetStore/              # Unity Asset Store packages
│   │   ├── ModularCityProps/    # City building props (formerly MCP/)
│   │   ├── PBR_TrafficLightsEU/  # PBR traffic lights
│   │   ├── TrafficLightsSystem/  # Traffic light system with tools
│   │   └── Skybox/              # Skybox assets
│   └── GraphSystem/             # Graph visualization systems
│       ├── NewGraph/            # Base graph framework (formerly NewGraph-master/)
│       └── MercuryGraph/        # Mercury-specific graph implementation
│
├── XRConfiguration/             # VR/XR platform configuration (NEW - consolidated)
│   ├── Oculus/                  # Oculus/Meta platform config
│   ├── MetaXR/                  # Meta XR SDK configuration
│   ├── XR/                      # Unity XR Plugin Management
│   ├── XRI/                     # XR Interaction Toolkit settings
│   └── CompositionLayers/       # Composition layers config
│
├── Editor/                      # Unity editor scripts
├── Resources/                   # Unity Resources folder
├── Settings/                    # Project settings
├── Samples/                     # Unity Package Manager samples
└── TextMesh Pro/                # TextMesh Pro package
```

**Note**: The project structure was reorganized on 2025-11-18 to improve organization and follow Unity best practices. See REORGANIZATION_SUMMARY.md for details.

---

## Core Architecture

### 1. Responder Pattern

Components implement the `IMmResponder` interface to receive messages:

```
IMmResponder (interface)
    ↓
MmResponder (abstract base)
    ↓
MmBaseResponder (implements method routing)
    ↓
Your Custom Responders
```

**Key Files:**
- `Assets/MercuryMessaging/Protocol/IMmResponder.cs` - Core interface
- `Assets/MercuryMessaging/Protocol/MmResponder.cs` - Base implementation
- `Assets/MercuryMessaging/Protocol/MmBaseResponder.cs` - Method routing with switch statements

### 2. Relay Node Pattern

`MmRelayNode` acts as the central message router:

```
Message → MmRelayNode → MmRoutingTable → Filtered Responders
                ↓
        Applies Filters:
        - Level (Parent/Child/Self)
        - Active (Active only vs All)
        - Tag (Multi-tag system)
        - Network (Local/Network/All)
```

**Key File:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (1422 lines - THE MOST IMPORTANT CLASS)

### 3. Hierarchical Message Flow

Messages propagate through Unity's GameObject hierarchy:

```
RootNode (MmRelaySwitchNode)
  ├── State1 (MmRelayNode) ← LevelFilter.Parent reaches here
  │   ├── Component1 (MmBaseResponder) ← LevelFilter.Child reaches here
  │   └── ChildNode (MmRelayNode)
  │       └── DeepComponent (MmBaseResponder)
  └── State2 (MmRelayNode)
```

**Direction Control:**
- `MmLevelFilter.Self` - Only the originating node
- `MmLevelFilter.Child` - Down the hierarchy
- `MmLevelFilter.Parent` - Up the hierarchy
- `MmLevelFilter.SelfAndChildren` - Self + descendants (default)
- `MmLevelFilter.SelfAndBidirectional` - All connected nodes

---

## Important Files Reference

### Core Protocol (Critical Files)

| File | Lines | Purpose |
|------|-------|---------|
| `Protocol/MmRelayNode.cs` | 1422 | **Central message router** - manages routing table, filters, hierarchy |
| `Protocol/MmBaseResponder.cs` | 383 | Base responder with method routing (SetActive, Initialize, Switch, etc.) |
| `Protocol/MmResponder.cs` | 124 | Abstract base implementing IMmResponder with lifecycle management |
| `Protocol/IMmResponder.cs` | 28 | Core interface defining the messaging contract |
| `Protocol/MmRelaySwitchNode.cs` | 188 | Relay node with FSM capabilities for state management |
| `Protocol/MmSwitchResponder.cs` | 148 | Controller for MmRelaySwitchNode with state transitions |

### Message System

| File | Purpose |
|------|---------|
| `Protocol/Message/MmMessage.cs` | Base message class with method, metadata, and network support |
| `Protocol/Message/MmMessageBool.cs` | Boolean message type |
| `Protocol/Message/MmMessageInt.cs` | Integer message type |
| `Protocol/Message/MmMessageFloat.cs` | Float message type |
| `Protocol/Message/MmMessageString.cs` | String message type |
| `Protocol/Message/MmMessageVector3.cs` | Vector3 message type |
| `Protocol/Message/MmMessageTransform.cs` | Transform message type |
| `Protocol/Message/MmMessageGameObject.cs` | GameObject reference message |
| `Protocol/Message/MmMessageByteArray.cs` | Byte array for custom serialization |
| `Protocol/Message/MmMessageSerializable.cs` | Generic serializable message |

### Filtering and Routing

| File | Purpose |
|------|---------|
| `Protocol/MmMetadataBlock.cs` | Routing control parameters (contains all filters) |
| `Protocol/MmRoutingTable.cs` | Collection of responders with filtering capabilities |
| `Protocol/MmLevelFilter.cs` | Direction filter (Parent/Child/Self combinations) |
| `Protocol/MmActiveFilter.cs` | Active GameObject filter |
| `Protocol/MmSelectedFilter.cs` | FSM selection filter |
| `Protocol/MmNetworkFilter.cs` | Network message filter |
| `Protocol/MmTag.cs` | Multi-tag system (8 flags: Tag0-Tag7) |
| `Protocol/MmMethod.cs` | Standard method enum (0-18 + custom 1000+) |

### Task Management

| File | Purpose |
|------|---------|
| `Task/MmTaskManager.cs` | Generic task management with progression tracking |
| `Task/MmTaskInfo.cs` | Basic task information |
| `Task/MmTaskResponder.cs` | Responder handling task-specific logic |
| `Task/IMmTaskInfo.cs` | Task information interface |
| `Task/Transformation/MmTransformationTaskInfo.cs` | Transform-specific tasks |
| `Task/Transformation/MmTransformationTaskResponder.cs` | Transform task responder |

### Support Systems

| File | Purpose |
|------|---------|
| `Support/FiniteStateMachine/FiniteStateMachine.cs` | Generic FSM with Enter/Exit events |
| `Support/FiniteStateMachine/StateEvents.cs` | State transition event handlers |
| `Support/Data/MmDataCollector.cs` | Data collection management |
| `Support/Data/MmDataHandlerCsv.cs` | CSV file handler |
| `Support/Data/MmDataHandlerXml.cs` | XML file handler |
| `Support/Extensions/GameObjectExtensions.cs` | GameObject helper methods |
| `Support/Extensions/TransformExtensions.cs` | Transform helper methods |
| `Support/Interpolators/Interpolator.cs` | Base interpolator for animations |
| `Protocol/MmLogger.cs` | Logging system with category filters |

### Network Support

| File | Purpose |
|------|---------|
| `Protocol/IMmNetworkResponder.cs` | Network responder interface |
| `Protocol/MmNetworkResponder.cs` | Base network responder |
| `Protocol/MmNetworkResponderPhoton.cs` | Photon networking integration |

---

## Key Concepts

### Message Structure

Every message contains:
```csharp
MmMessage {
    MmMethod method;              // What action to perform
    MmMessageType messageType;    // Type identifier for serialization
    MmMetadataBlock MetadataBlock; // Routing control
    int NetId;                    // Network identifier
    bool IsDeserialized;          // Came from network?
}
```

### MmMethod Enum (Standard Methods 0-18)

```csharp
NoOp = 0,
Initialize = 1,
SetActive = 2,
Refresh = 3,
Switch = 4,
Complete = 5,
TaskInfo = 6,
Message = 7,
MessageBool = 8,
MessageInt = 9,
MessageFloat = 10,
MessageString = 11,
MessageVector3 = 12,
MessageVector4 = 13,
MessageQuaternion = 14,
MessageTransform = 15,
MessageTransformList = 16,
MessageByteArray = 17,
MessageGameObject = 18
// Custom methods: Use values > 1000
```

### Filtering System

**MmMetadataBlock** combines four orthogonal filters:

1. **Level Filter** - Direction
   - `Self` - Only originating node
   - `Child` - Descendants only
   - `Parent` - Ancestors only
   - `SelfAndChildren` - Default
   - `SelfAndBidirectional` - Everything

2. **Active Filter** - GameObject state
   - `Active` - Only active GameObjects
   - `All` - Both active and inactive

3. **Selected Filter** - FSM state
   - `All` - All responders
   - `Selected` - Only currently selected in FSM

4. **Network Filter** - Network origin
   - `Local` - Local messages only
   - `Network` - Network messages only
   - `All` - Both

5. **Tag System** - 8-bit flag system
   - `Tag0` through `Tag7`
   - `Everything` (-1) - Matches all
   - `Nothing` (0) - Matches none
   - Bitwise combinations supported

### Tag Checking

Components can filter messages by tag:
```csharp
public MmTag Tag = MmTag.Tag0;
public bool TagCheckEnabled = true;
```

Message must have matching tag bits to be received:
```csharp
if (TagCheckEnabled && (message.MetadataBlock.Tag & Tag) == 0) {
    return; // Tag mismatch, ignore message
}
```

---

## Common Workflows

### 1. Basic Message Sending

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

### 2. Creating a Custom Responder

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

### 3. Setting Up FSM with MmRelaySwitchNode

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

### 4. Hierarchical Setup

```csharp
// Parent setup (automatic)
// MmRelayNode automatically finds parent nodes in hierarchy
// Call RefreshParents() if hierarchy changes at runtime

MmRelayNode childNode = childObject.GetComponent<MmRelayNode>();
childNode.RefreshParents(); // Updates parent references
```

### 5. Network Message Sending

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

### 6. Tag-Based Filtering

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

## Architecture Patterns

### Pattern 1: State Machine Hierarchy

Use `MmRelaySwitchNode` for state-based game structure:

```
GameManager (MmRelaySwitchNode)
  ├── MainMenu (MmRelayNode) - State 1
  │   ├── MenuUI (MmBaseResponder)
  │   └── MenuCamera (MmBaseResponder)
  ├── Gameplay (MmRelayNode) - State 2
  │   ├── Player (MmRelayNode)
  │   ├── Enemies (MmRelayNode)
  │   └── GameplayUI (MmBaseResponder)
  └── Pause (MmRelayNode) - State 3
      └── PauseUI (MmBaseResponder)
```

Only the current state's responders receive "Selected" filtered messages.

### Pattern 2: Effect Hierarchies

Group related effects under a relay node:

```
EffectManager (MmRelayNode)
  ├── ParticleSystem (MmBaseResponder)
  ├── AudioSource (MmBaseResponder)
  ├── AnimationController (MmBaseResponder)
  └── LightController (MmBaseResponder)
```

Send one message to enable/disable entire effect group:
```csharp
effectManager.MmInvoke(MmMethod.SetActive, false, MmMetadataBlock.Default);
```

### Pattern 3: Bidirectional Communication

Child notifies parent of events:

```csharp
// In child component
public class ButtonResponder : MmBaseResponder {
    public void OnClick() {
        // Notify parent that button was clicked
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageString,
            "ButtonClicked",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }
}

// In parent component
public class MenuController : MmBaseResponder {
    protected override void ReceivedMessage(MmMessageString message) {
        if (message.value == "ButtonClicked") {
            // Handle button click
        }
    }
}
```

---

## Development Notes

### Important Implementation Details

1. **MmRelayNode Registration**
   - Responders automatically register with relay nodes in their hierarchy
   - `RegisterAwakenedResponder()` called during Awake
   - `UnRegisterResponder()` called on Destroy

2. **Network Considerations**
   - Host machines run as both Client + Server
   - Use `IsDeserialized` flag to detect network messages
   - `FlipNetworkFlagOnSend` prevents deep network propagation
   - Override `AllowNetworkPropagationLocally` for custom behavior

3. **Performance**
   - `doNotModifyRoutingTable` flag prevents modification during iteration
   - Queue system for adding responders during message processing
   - Message history tracking can be disabled for production

4. **Hierarchy Changes**
   - Call `RefreshParents()` if GameObject hierarchy changes at runtime
   - Parent-child relationships are cached for performance
   - Circular dependencies are prevented

5. **Custom Methods**
   - Use method values > 1000 for custom application-specific methods
   - Handle custom methods by overriding `MmInvoke(MmMessage)` directly

### Common Gotchas

1. **Tag Checking**: Remember to disable `TagCheckEnabled` if not using tags
2. **Level Filters**: Default is `SelfAndChildren`, not `Self` alone
3. **Active Filter**: Inactive GameObjects ignored by default (use `MmActiveFilter.All` to include)
4. **FSM Selection**: `SelectedFilter.Selected` only works with `MmRelaySwitchNode`
5. **Network Doubles**: Host receives messages twice (local + network) - check `IsDeserialized`

### Debugging Features

MmRelayNode includes visual debugging:
- Message history tracking (`messageInList`, `messageOutList`)
- Visual node connections with colors
- Signal animation for message flow
- Integration with EPOOutline for highlights
- ALINE path drawing for message paths

Enable/disable in MmLogger:
```csharp
MmLogger.logFramework = true;  // Framework logging
MmLogger.logResponder = true;  // Responder logging
MmLogger.logApplication = true; // Application logging
MmLogger.logNetwork = true;    // Network logging
```

---

## Tutorial Scenes

### SimpleScene
`Assets/MercuryMessaging/Tutorials/SimpleScene/`

Basic light switch example demonstrating:
- Message passing between components
- GUI interaction with responders
- VR hand controller integration
- Performance comparison with traditional Unity methods

**Key Scripts:**
- `LightSwitchHandler.cs` - Updates material color based on messages
- `LightSwitchResponder.cs` - Handles switch state messages
- `LightGUIHandler.cs` / `LightGUIResponder.cs` - GUI interaction
- `HandController.cs` - VR hand interaction demo

### Tutorial 1-5
`Assets/MercuryMessaging/Tutorials/Tutorial1-5/`

Progressive tutorial series covering:
- Tutorial 1: Basic message sending
- Tutorial 2: Hierarchy setup (with custom materials)
- Tutorial 3: Filtering and tags (with custom materials)
- Tutorial 4: State machines (with scripts)
- Tutorial 5: Advanced patterns (with scripts)

### Demo Scene
`Assets/MercuryMessaging/Demo/TrafficLights.unity`

Traffic light simulation demonstrating real-world usage.

---

## Testing

### Running Tests

The project uses Unity Test Framework for automated testing. Quick Win optimizations (QW-1, QW-2, QW-4) have comprehensive test coverage.

**Location:** `Assets/MercuryMessaging/Tests/`

**Test Files:**
- `CircularBufferTests.cs` - CircularBuffer implementation tests (30+ tests)
- `CircularBufferMemoryTests.cs` - QW-4 memory stability validation (6 tests)
- `HopLimitValidationTests.cs` - QW-1 hop limit enforcement (6 tests)
- `CycleDetectionValidationTests.cs` - QW-1 cycle detection (6 tests)
- `LazyCopyValidationTests.cs` - QW-2 lazy copying optimization (7 tests)

**How to Run:**
1. Open Unity Editor
2. Window > General > Test Runner
3. Select **PlayMode** tab (most tests require runtime context)
4. Click **Run All** to execute all tests
5. Tests should pass with green checkmarks

**Test Coverage:**
- **QW-1 Hop Limits:** Validates messages stop after configured hop count in deep hierarchies
- **QW-1 Cycle Detection:** Validates VisitedNodes tracking prevents infinite loops
- **QW-2 Lazy Copying:** Validates single-direction reuses messages, multi-direction creates necessary copies
- **QW-4 CircularBuffer:** Validates bounded memory footprint over high message volumes (10K+ messages)

**Running from Command Line (CI/CD):**
```bash
# Run PlayMode tests in batch mode
Unity.exe -runTests -batchmode -projectPath . \
  -testResults ./test-results.xml \
  -testPlatform PlayMode
```

---

## Performance Characteristics

### Overview

MercuryMessaging has been comprehensively tested and optimized across three scales (Small, Medium, Large) with validated Quick Win optimizations (QW-1 through QW-6). Performance optimizations achieved **2-2.2x frame time improvement** and **3-35x throughput improvement**, with excellent memory stability and scalability.

**Full Reports:**
- Initial analysis: `Assets/MercuryMessaging/Documentation/Performance/PERFORMANCE_REPORT.md`
- Optimization results: `Assets/MercuryMessaging/Documentation/Performance/OPTIMIZATION_RESULTS.md`

### Measured Performance

**Frame Time (Unity Editor, Development Build with PerformanceMode):**
- Small (10 responders, 3 levels): 15.14ms avg (66.0 FPS) ✅
- Medium (50 responders, 5 levels): 16.28ms avg (61.4 FPS) ✅
- Large (100+ responders, 7-10 levels): 18.69ms avg (53.5 FPS) ✅

**Performance Improvement (After Optimization):**
- Frame time: 2-2.2x faster (was 32-36ms, now 15-19ms)
- Throughput: 3-35x faster (was 28-30 msg/sec, now 98-980 msg/sec)
- See `Documentation/Performance/OPTIMIZATION_RESULTS.md` for detailed analysis

**Memory Stability:** ✅ **Validated**
- Remains bounded and stable (~925-940 MB)
- QW-4 CircularBuffer successfully bounds memory usage
- Suitable for long-running sessions
- No regression from performance optimizations

**Message Throughput:**
- Small: 98 msg/sec (excellent for low-density scenarios)
- Medium: 492 msg/sec (strong mid-range performance)
- Large: 980 msg/sec (nearly 1000 msg/sec sustained!)
- Scales excellently from 10 to 100+ responders

**Comparison vs Unity Built-ins (InvocationComparison):**
- Mercury vs Direct Calls: 28x slower (acceptable for decoupling benefits)
- Mercury vs SendMessage: 2.6x slower (competitive performance)
- Mercury vs UnityEvent: 28x slower (reflection overhead)
- Mercury vs Execute: 1.05x slower (comparable to similar patterns)

### Scaling Characteristics

**Responder Count (Excellent Scaling):**
- Sub-linear frame time: 10 → 100 responders adds only 4ms (15ms → 19ms)
- Excellent throughput scaling: 98 → 980 msg/sec (10x improvement with 10x responders)
- Cache hit rate unknown (not instrumented - separate investigation needed)
- Highly suitable for projects with 100+ responders

**Hierarchy Depth:**
- Minimal overhead: 3 → 10 levels adds only 4ms frame time
- Hop limits prevent runaway propagation (default: 50 hops)
- Cycle detection working (no infinite loops)

**Message Volume (Outstanding Scalability):**
- Memory bounded regardless of volume (QW-4 validated)
- No degradation over time (tested up to 980 msg/sec sustained)
- Throughput scales with responder count (98 → 980 msg/sec)
- Suitable for high-volume messaging applications

### Configuration Recommendations

**Small Projects (<20 responders, <5 levels):**
```csharp
maxMessageHops: 20
messageHistorySize: 50
enableCycleDetection: true
```

**Medium Projects (20-100 responders, 5-10 levels):**
```csharp
maxMessageHops: 50 (default)
messageHistorySize: 100 (default)
enableCycleDetection: true
```

**Large Projects (100+ responders, 10+ levels):**
```csharp
maxMessageHops: 100
messageHistorySize: 200
enableCycleDetection: true
```

### Performance Tuning Tips

1. **Hierarchy Design:**
   - Keep depth <10 levels for best performance
   - Use SelfAndChildren for most messages
   - Avoid unnecessary Parent broadcasting

2. **Tag Usage:**
   - Enable TagCheckEnabled to reduce routing table scans
   - Use specific tags (Tag0-Tag7) instead of Everything
   - Tag filtering is efficient for large routing tables

3. **Memory Management:**
   - Set messageHistorySize based on debugging needs
   - Reduce to 50 in production builds
   - Set to 10 if history not needed

4. **PerformanceMode (Important for Production):**
   - Enable `MmRelayNode.PerformanceMode = true;` in production builds
   - Disables debug message tracking (UpdateMessages overhead)
   - Provides 2-2.2x frame time improvement
   - Automatically enabled by PerformanceTestHarness during testing
   - **Disable** in development for message flow visualization

5. **When to Use MercuryMessaging:**
   - ✅ Loosely-coupled communication needed
   - ✅ Complex hierarchical structures
   - ✅ Network synchronization required
   - ✅ Filter-based targeting (by level, tag, state)
   - ✅ FSM-based state management

6. **When to Avoid:**
   - ❌ Performance-critical tight loops (use direct calls)
   - ❌ Simple parent-child communication (use UnityEvents)
   - ❌ Flat architectures without hierarchies

### Quick Win Validation Status

- **QW-1 (Hop Limits):** ✅ Validated via automated tests
- **QW-2 (Lazy Copy):** ✅ Validated via automated tests
- **QW-3 (Filter Cache):** ⚠️ Implementation complete, hit rate not measured
- **QW-4 (CircularBuffer):** ✅ Validated - negative memory growth confirms bounded behavior
- **QW-5 (LINQ Removal):** ✅ Complete - 4 allocation sites removed
- **QW-6 (Code Cleanup):** ✅ Complete - 179 lines removed from MmRelayNode

### Known Limitations

1. **Frame Time Near Target (Optimized):**
   - Current: 53-66 FPS (Editor with PerformanceMode)
   - Target: 60+ FPS (achieved on Small/Medium scales!)
   - **Status:** ✅ Significantly improved from 28-31 FPS
   - Recommendation: Test standalone builds for further improvement

2. **Cache Hit Rate Unknown:**
   - QW-3 implementation complete but shows 0.0% in tests
   - Possible causes: Not used in hot path, frequent invalidation
   - Recommendation: Separate investigation task for cache usage analysis

3. **Message Throughput (Optimized):**
   - Current: 98-980 msg/sec (scales with responder count)
   - **Status:** ✅ Exceeds expectations (was 28-30 msg/sec)
   - Throughput bottleneck resolved with frame-based generation

### Future Optimization Opportunities

See `dev/active/framework-analysis/framework-analysis-tasks.md` for Priority 3 tasks:
- **Routing optimization (420h):** O(1) routing tables, spatial indexing
- **Network performance (500h):** Delta tracking, compression
- **Visual composer (360h):** Editor tools for hierarchy design
- **Standard library (290h):** Common message patterns

---

## Quick Reference

### Sending Messages

```csharp
// Simple message
relay.MmInvoke(MmMethod.Initialize);

// With parameters
relay.MmInvoke(MmMethod.SetActive, true);
relay.MmInvoke(MmMethod.MessageString, "Hello");
relay.MmInvoke(MmMethod.MessageInt, 42);

// With metadata
relay.MmInvoke(
    MmMethod.Switch,
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All)
);

// Full control
relay.MmInvoke(
    MmMethod.MessageFloat,
    3.14f,
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local,
        MmTag.Tag0
    )
);
```

### Creating Custom Responders

```csharp
public class MyResponder : MmBaseResponder
{
    // Override specific message handlers
    protected override void ReceivedMessage(MmMessageBool msg) { }
    protected override void ReceivedMessage(MmMessageInt msg) { }
    protected override void ReceivedMessage(MmMessageFloat msg) { }
    protected override void ReceivedMessage(MmMessageString msg) { }

    // Override standard methods
    protected override void ReceivedSetActive(bool active) { }
    protected override void ReceivedInitialize() { }
    protected override void ReceivedSwitch(int stateIndex) { }
    protected override void ReceivedRefresh() { }

    // Handle custom methods (>1000)
    public override void MmInvoke(MmMessage message) {
        if (message.method == (MmMethod)1001) {
            // Handle custom method
        } else {
            base.MmInvoke(message); // Call base for standard methods
        }
    }
}
```

### FSM Setup

```csharp
// Component on GameObject with MmRelaySwitchNode
void Start() {
    var fsm = GetComponent<MmRelaySwitchNode>().RespondersFSM;

    // Jump to state
    fsm.JumpTo("StateName");

    // Transition with events
    fsm.StartTransitionTo("StateName");

    // Listen to state changes
    fsm.GlobalEnter += (fromState, toState) => {
        Debug.Log($"Switched from {fromState} to {toState}");
    };
}
```

---

## Best Practices

1. **Use Hierarchy Wisely**: Organize GameObjects to match message flow patterns
2. **Tag Strategically**: Use tags to separate concerns (UI, Gameplay, Network, etc.)
3. **Prefer SelfAndChildren**: Most messages should target self and descendants
4. **Network-Aware**: Always consider network implications when designing messages
5. **Custom Methods**: Keep custom method numbers > 1000 to avoid conflicts
6. **Debug Logging**: Enable MmLogger categories during development
7. **FSM for States**: Use MmRelaySwitchNode for game states, menus, and phases
8. **Task System**: Use MmTaskManager for user studies and experimental workflows
9. **Loose Coupling**: Avoid storing responder references - always use messages
10. **Test Hierarchy**: Use `RefreshParents()` after runtime hierarchy changes

---

## External Dependencies

**IMPORTANT:** The core MercuryMessaging framework (AppState, Protocol, Support/Data, Task) has **ZERO** third-party dependencies as of 2025-11-19.

### Removed Dependencies
- ❌ **ALINE (Drawing)** - Removed from MmRelayNode.cs (visual debugging not essential)
- ❌ **EPOOutline** - Removed from MmRelayNode.cs (visual debugging not essential)
- ❌ **Unity XR Interaction Toolkit** - Removed from MmRelayNode.cs (moved to user code if needed)
- ❌ **Unity Input System** - Removed from MmRelayNode.cs (not core framework dependency)
- ❌ **NewGraph** - Removed from MmRelayNode.cs (editor visualization not essential)

### Optional Dependencies (Properly Isolated)
- ✅ **Photon Unity Networking** (optional) - Wrapped in `#if PHOTON_AVAILABLE` for network features

**Policy:** The core framework must remain dependency-free to ensure maximum portability and minimal breaking changes.

---

## Development Standards & Best Practices

### External Dependency Policy

**CRITICAL:** The MercuryMessaging core framework MUST minimize external dependencies.

**Allowed Dependencies (ONLY):**
- ✅ **UnityEngine** - Core Unity runtime
- ✅ **UnityEditor** - Unity Editor APIs (editor scripts only)
- ✅ **System.*** - .NET Standard libraries (Collections, Linq, IO, etc.)

**Optional Dependencies (outside core, with conditional compilation):**
- ✅ **Photon Unity Networking** - Wrapped in `#if PHOTON_AVAILABLE`

**Adding New Dependencies:**
1. Must be justified as essential to core functionality
2. Must be wrapped in conditional compilation directives
3. Must include auto-detection script in `MmThirdPartyUtils.cs`
4. Framework must gracefully degrade when dependency is missing

---

### Naming Convention Policy

**CRITICAL:** Core framework files MUST follow the "Mm" prefix convention.

**Folders requiring "Mm" prefix:**
- ✅ `Assets/MercuryMessaging/AppState/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Protocol/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Support/Data/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Task/` - All files must start with "Mm" or "IMm"

**Folders EXEMPT from "Mm" prefix:**
- `Support/Extensions/` - Generic C# extension methods
- `Support/FiniteStateMachine/` - Generic FSM implementation
- `Support/Interpolators/` - Generic animation utilities
- `Support/Input/` - Generic input handling
- `Support/GUI/` - GUI utilities
- `Support/Editor/` - Editor utilities
- `Support/ThirdParty/` - Third-party integrations
- `Examples/` - Tutorial and demo code
- `Tests/` - Test files

**Rules:**
- ✅ All classes in core folders: `MmClassName.cs`
- ✅ All interfaces in core folders: `IMmInterfaceName.cs`
- ✅ All structs in core folders: `MmStructName.cs`
- ✅ All enums in core folders: `MmEnumName.cs`

**Examples:**
- ✅ `Protocol/MmRelayNode.cs`, `Protocol/Message/MmMessage.cs`
- ✅ `Support/Data/MmCircularBuffer.cs`, `Support/Data/MmDataCollector.cs`
- ✅ `Task/MmTaskManager.cs`, `Task/IMmTaskInfoCollectionLoader.cs`
- ❌ `Support/Data/CircularBuffer.cs` (must be MmCircularBuffer.cs)
- ✅ `Support/Extensions/GameObjectExtensions.cs` (exempt folder - no prefix needed)

---

### Testing Standards Policy

**CRITICAL:** All tests MUST be fully automated without manual scene setup or UI creation.

**Required Patterns:**

1. **Use Unity Test Framework**
   - PlayMode tests for runtime behavior
   - EditMode tests for editor-only code

2. **Programmatic GameObject Creation**
   ```csharp
   [SetUp]
   public void Setup() {
       testRoot = new GameObject("TestRoot");
       relay = testRoot.AddComponent<MmRelayNode>();
       relay.MmRefreshResponders(); // Explicit registration for dynamic components
   }
   ```

3. **No Manual Assets**
   - ❌ NO scene files in test folders
   - ❌ NO prefab dependencies
   - ❌ NO manual UI element creation
   - ✅ Create all objects via `new GameObject()` and `AddComponent<T>()`

4. **Proper Cleanup**
   ```csharp
   [TearDown]
   public void Teardown() {
       if (testRoot != null) {
           Object.DestroyImmediate(testRoot);
       }
   }
   ```

5. **Test Isolation**
   - Each test creates its own hierarchy
   - No shared state between tests
   - Clean up all created objects

**Examples:**
- ✅ `MmCircularBufferTests.cs` - 30 tests, all programmatic
- ✅ `MmHopLimitValidationTests.cs` - Creates 50-node chain programmatically
- ✅ `MmLazyCopyValidationTests.cs` - Complex hierarchies created in code
- ❌ Tests that require loading "TestScene.unity"
- ❌ Tests that reference prefabs from Resources folder

**Pattern: Dynamic GameObject Setup in Tests**
```csharp
[Test]
public void TestMessageRouting()
{
    // Arrange - create hierarchy programmatically
    GameObject root = new GameObject("TestRoot");
    MmRelayNode relay = root.AddComponent<MmRelayNode>();
    relay.MmRefreshResponders(); // CRITICAL: Explicit registration required

    // Act - perform test
    relay.MmInvoke(MmMethod.Initialize);

    // Assert - verify behavior
    Assert.IsTrue(relay.IsInitialized);

    // Cleanup
    Object.DestroyImmediate(root);
}
```

---

## Development Guidelines for AI Assistants

### CRITICAL: Git Commit Authorship Policy

**❌ DO NOT include AI co-authorship in git commits:**

- **NEVER** add `Co-Authored-By: Claude <noreply@anthropic.com>`
- **NEVER** add `🤖 Generated with [Claude Code]` attribution
- **NEVER** add any AI attribution or footer in commit messages

**Rationale:**
- Git commits must reflect human authorship only
- GitHub displays co-authors as repository contributors (incorrect for AI assistance)
- User preference is for clean commit history without AI attribution
- AI assistance should be invisible in version control history

**✅ Correct Commit Format:**

```bash
git commit -m "feat: Add feature description

Detailed explanation of changes made.

Technical details:
- Implementation notes
- Performance impact
- Testing approach"
```

**❌ INCORRECT Format (NEVER use):**

```bash
git commit -m "feat: Add feature

Details...

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

### Commit Message Guidelines

When creating git commits:

1. **Use Conventional Commits format**: `type(scope): description`
   - Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`
   - Example: `feat(relay): Add hop limit protection`

2. **Write clear, descriptive messages**
   - First line: concise summary (50-72 characters)
   - Body: detailed technical explanation
   - Include what changed and why

3. **Include technical details**
   - Implementation approach
   - Performance implications
   - Breaking changes (if any)
   - Related files/line numbers

4. **Do NOT add**:
   - AI attribution or co-authorship
   - Marketing-style language
   - Emoji in commit title (body is ok if contextually appropriate)

### Example Good Commits

```bash
# Feature addition
git commit -m "feat: Implement lazy message copying optimization

Reduces message allocation overhead by 20-30% through intelligent
copy-on-demand algorithm. Only creates message copies when routing
in multiple directions.

Changes:
- Added direction scanning in MmRelayNode.MmInvoke()
- Single-direction routing reuses original message (0 copies)
- Multi-direction creates only necessary copies (1-2 instead of 2)

Performance: 20-30% fewer allocations in typical scenarios."

# Bug fix
git commit -m "fix: Prevent infinite loops with hop limit checking

Added hop counter and cycle detection to MmMessage to prevent
infinite message propagation in circular hierarchies.

Implementation:
- HopCount field tracks relay depth
- VisitedNodes HashSet detects cycles
- Both configurable via MmRelayNode inspector

Fixes potential Unity crashes in complex message graphs."

# Documentation
git commit -m "docs: Update framework analysis context

Captured implementation details for QW-4, QW-1, QW-2.
Added code patterns, design decisions, and continuation notes
for seamless context handoff."
```

---

## Support and Resources

**Developer**: Columbia University CGUI Lab

**Repository**: This codebase is part of a research project focused on VR interaction and messaging frameworks.

**Key Contacts**: Check project documentation for research lab contact information.

---

# Starting Large Tasks

When exiting plan mode with an accepted plan: 

1.**Create Task Directory**:
mkdir -p ~/git/project/dev/active/[task-name]/

2.**Create Documents**:

- `[task-name]-plan.md` - The accepted plan
- `[task-name]-context.md` - Key files, decisions
- `[task-name]-tasks.md` - Checklist of work

3.**Update Regularly**: Mark tasks complete immediately

### Continuing Tasks

- Check `/dev/active/` for existing tasks
- Read all three files before proceeding
- Update "Last Updated" timestamps

---

*Last Updated: 2025-11-18*
*Framework Version: Based on Unity 2021.3+ with VR/XR support*
