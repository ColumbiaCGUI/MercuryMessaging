# Standardized Message Library - Technical Context

**Last Updated:** 2025-11-18
**Status:** Planning - Not Started
**Priority:** MEDIUM (Phase 5.1)

---

## Status

**Current State:** Not Started
**Blockers:** None
**Dependencies:** None (but benefits routing-optimization for versioning)
**Estimated Timeline:** 228 hours (6-7 weeks)

---

## Quick Resume

**Where to start if beginning this task:**
1. Read this document completely
2. Review `standard-library-tasks.md` for detailed checklist
3. Study existing MmMessage types in `Assets/MercuryMessaging/Protocol/Message/`
4. Review versioning requirements and backward compatibility needs
5. Begin with UI namespace (most concrete use cases)

**First 3 steps to take:**
1. Design message library architecture and namespace structure (12h)
2. Implement first 10 UI message types (24h)
3. Create versioning system (MmMessageVersion attribute) (20h)

**Key files to read first:**
- `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs` - Base message class
- `Assets/MercuryMessaging/Protocol/Message/MmMessageString.cs` - Example message type
- Existing responder examples in `Assets/MercuryMessaging/Protocol/`

---

## Technical Overview

This initiative creates a comprehensive library of 40+ standardized message types, eliminating the need for developers to create custom messages for common scenarios.

**Core Problem:**
- Every developer creates custom message types
- No interoperability between components from different developers
- Reinventing the wheel for common patterns (clicks, state changes, etc.)
- No standard patterns for versioning or compatibility

**Solution:**
- Standardized message library across 4 namespaces
- Message versioning system for backward compatibility
- Example responders demonstrating usage
- Tutorial scenes for each category

**Expected Impact:**
- 70% reduction in custom message creation
- Instant component interoperability
- Faster onboarding for new developers
- Enable component marketplace ecosystem

---

## Current State Analysis

### Existing Message System

**Current Message Types (Built-in):**
```csharp
// Generic types in MercuryMessaging/Protocol/Message/
MmMessageBool
MmMessageInt
MmMessageFloat
MmMessageString
MmMessageVector3
MmMessageVector4
MmMessageQuaternion
MmMessageTransform
MmMessageTransformList
MmMessageGameObject
MmMessageByteArray
MmMessageSerializable
```

**Limitations:**
1. Too generic - no semantic meaning
2. No domain-specific types (UI clicks, state changes)
3. No versioning support
4. No interoperability guarantees
5. Developers forced to subclass or use generic types awkwardly

**Example of Current Approach (Awkward):**
```csharp
// Sending a "click" currently requires:
var message = new MmMessageVector3();
message.value = clickPosition;
relay.MmInvoke((MmMethod)1001, message); // Custom method ID

// Recipient must know:
// - Custom method ID 1001 means "click"
// - Vector3 contains click position
// - No way to pass clicked object, pointer ID, etc.
```

**Desired Approach (Standardized):**
```csharp
// With standard library:
var message = new MmMessageClick {
    ClickPosition = clickPosition,
    ClickedObject = hit.collider.gameObject,
    PointerID = 0
};
relay.MmInvoke(MmMethod.UIClick, message);

// Recipient automatically knows structure:
protected override void ReceivedMessage(MmMessageClick message) {
    // Access strongly-typed properties
    Debug.Log($"Clicked {message.ClickedObject.name} at {message.ClickPosition}");
}
```

---

## Architecture Design

### Message Namespace Structure

```
MercuryMessaging.StandardLibrary/
├── UI/                      # 10+ UI interaction messages
│   ├── MmMessageClick
│   ├── MmMessageHover
│   ├── MmMessageDrag
│   ├── MmMessageDrop
│   ├── MmMessageFocus
│   ├── MmMessageBlur
│   ├── MmMessageScroll
│   ├── MmMessagePinch
│   ├── MmMessageZoom
│   └── MmMessageVoiceCommand
│
├── AppState/                # 8+ application state messages
│   ├── MmMessageInitialize
│   ├── MmMessageShutdown
│   ├── MmMessagePause
│   ├── MmMessageResume
│   ├── MmMessageStateChange
│   ├── MmMessageSaveLoad
│   ├── MmMessageError
│   └── MmMessageNotification
│
├── Input/                   # 12+ input event messages
│   ├── MmMessage6DOF
│   ├── MmMessageGesture
│   ├── MmMessageHaptic
│   ├── MmMessageControllerButton
│   ├── MmMessageControllerTrigger
│   ├── MmMessageControllerGrip
│   ├── MmMessageControllerThumbstick
│   ├── MmMessageGazeTarget
│   ├── MmMessageHeadTracking
│   ├── MmMessageHandTracking
│   ├── MmMessageEyeTracking
│   └── MmMessageBodyTracking
│
└── Task/                    # 10+ task management messages
    ├── MmMessageTaskAssigned
    ├── MmMessageTaskStarted
    ├── MmMessageTaskProgress
    ├── MmMessageTaskCompleted
    ├── MmMessageTaskFailed
    ├── MmMessageTaskCancelled
    ├── MmMessageTaskMilestone
    ├── MmMessageTaskDependency
    ├── MmMessageTaskBatch
    └── MmMessageTaskQuery
```

---

## Message Definitions

### 1. UI Namespace

#### MmMessageClick
```csharp
namespace MercuryMessaging.StandardLibrary.UI
{
    /// <summary>
    /// Sent when user clicks/taps an object
    /// </summary>
    [MmMessageVersion(1, 0)]
    public class MmMessageClick : MmMessage
    {
        /// <summary>World space position of the click</summary>
        public Vector3 ClickPosition;

        /// <summary>GameObject that was clicked (if any)</summary>
        public GameObject ClickedObject;

        /// <summary>Pointer/finger ID (for multi-touch)</summary>
        public int PointerID;

        /// <summary>Button (0=left, 1=right, 2=middle)</summary>
        public int Button = 0;

        /// <summary>Timestamp of click</summary>
        public float Timestamp;

        /// <summary>Was this a double-click?</summary>
        public bool IsDoubleClick;
    }
}
```

#### MmMessageHover
```csharp
/// <summary>
/// Sent when cursor enters/exits an object
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageHover : MmMessage
{
    /// <summary>GameObject being hovered</summary>
    public GameObject HoveredObject;

    /// <summary>True = enter, False = exit</summary>
    public bool IsEnter;

    /// <summary>Hover duration (if exiting)</summary>
    public float HoverDuration;

    /// <summary>Pointer ID</summary>
    public int PointerID;
}
```

#### MmMessageDrag
```csharp
/// <summary>
/// Sent during drag operations
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageDrag : MmMessage
{
    /// <summary>Object being dragged</summary>
    public GameObject DraggedObject;

    /// <summary>World space start position</summary>
    public Vector3 StartPosition;

    /// <summary>Current world space position</summary>
    public Vector3 CurrentPosition;

    /// <summary>Delta from previous frame</summary>
    public Vector3 DragDelta;

    /// <summary>Drag state</summary>
    public DragState State; // Started, Dragging, Ended

    /// <summary>Pointer ID</summary>
    public int PointerID;
}

public enum DragState
{
    Started,
    Dragging,
    Ended,
    Cancelled
}
```

#### MmMessageScroll
```csharp
/// <summary>
/// Sent on scroll wheel or trackpad scroll
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageScroll : MmMessage
{
    /// <summary>Scroll delta (-1 to 1)</summary>
    public Vector2 ScrollDelta;

    /// <summary>Accumulated scroll</summary>
    public float TotalScroll;

    /// <summary>Scrolling object (if UI)</summary>
    public GameObject ScrollObject;
}
```

#### MmMessagePinch
```csharp
/// <summary>
/// Sent during pinch gestures (zoom)
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessagePinch : MmMessage
{
    /// <summary>Center point of pinch</summary>
    public Vector3 PinchCenter;

    /// <summary>Distance between fingers/points</summary>
    public float PinchDistance;

    /// <summary>Delta distance from previous frame</summary>
    public float PinchDelta;

    /// <summary>Pinch scale (1.0 = no change)</summary>
    public float PinchScale;

    /// <summary>State</summary>
    public PinchState State; // Started, Pinching, Ended
}
```

---

### 2. AppState Namespace

#### MmMessageStateChange
```csharp
namespace MercuryMessaging.StandardLibrary.AppState
{
    /// <summary>
    /// Sent when application state changes
    /// </summary>
    [MmMessageVersion(1, 0)]
    public class MmMessageStateChange : MmMessage
    {
        /// <summary>Previous state name</summary>
        public string PreviousState;

        /// <summary>New state name</summary>
        public string NewState;

        /// <summary>Additional state data</summary>
        public Dictionary<string, object> StateData;

        /// <summary>Reason for state change</summary>
        public string ChangeReason;

        /// <summary>Timestamp</summary>
        public float Timestamp;
    }
}
```

#### MmMessageSaveLoad
```csharp
/// <summary>
/// Sent for save/load operations
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageSaveLoad : MmMessage
{
    /// <summary>Operation type</summary>
    public SaveLoadOperation Operation; // Save, Load, Delete, Query

    /// <summary>Save slot name/ID</summary>
    public string SaveSlotName;

    /// <summary>Success flag</summary>
    public bool Success;

    /// <summary>Error message (if failed)</summary>
    public string ErrorMessage;

    /// <summary>Save data (serialized)</summary>
    public byte[] SaveData;

    /// <summary>Timestamp</summary>
    public System.DateTime Timestamp;
}

public enum SaveLoadOperation
{
    Save,
    Load,
    Delete,
    Query,
    AutoSave
}
```

#### MmMessageError
```csharp
/// <summary>
/// Sent when errors occur
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageError : MmMessage
{
    /// <summary>Error message</summary>
    public string ErrorMessage;

    /// <summary>Error code</summary>
    public int ErrorCode;

    /// <summary>Severity</summary>
    public ErrorSeverity Severity; // Info, Warning, Error, Critical

    /// <summary>Source component</summary>
    public string Source;

    /// <summary>Stack trace</summary>
    public string StackTrace;

    /// <summary>Exception (if any)</summary>
    public System.Exception Exception;
}
```

---

### 3. Input Namespace

#### MmMessage6DOF
```csharp
namespace MercuryMessaging.StandardLibrary.Input
{
    /// <summary>
    /// 6 degrees of freedom tracking data
    /// </summary>
    [MmMessageVersion(1, 0)]
    public class MmMessage6DOF : MmMessage
    {
        /// <summary>World position</summary>
        public Vector3 Position;

        /// <summary>World rotation</summary>
        public Quaternion Rotation;

        /// <summary>Linear velocity</summary>
        public Vector3 Velocity;

        /// <summary>Angular velocity</summary>
        public Vector3 AngularVelocity;

        /// <summary>Tracking confidence (0-1)</summary>
        public float Confidence;

        /// <summary>Input device type</summary>
        public InputDeviceType Device; // HMD, LeftHand, RightHand, Tracker

        /// <summary>Timestamp</summary>
        public float Timestamp;
    }

    public enum InputDeviceType
    {
        HMD,
        LeftHand,
        RightHand,
        GenericTracker,
        Custom
    }
}
```

#### MmMessageGesture
```csharp
/// <summary>
/// Recognized gesture data
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageGesture : MmMessage
{
    /// <summary>Gesture type</summary>
    public GestureType Type; // Swipe, Pinch, Rotate, Tap, Hold

    /// <summary>Confidence score (0-1)</summary>
    public float Confidence;

    /// <summary>Gesture direction (if applicable)</summary>
    public Vector3 Direction;

    /// <summary>Touch points</summary>
    public Vector3[] TouchPoints;

    /// <summary>Gesture duration</summary>
    public float Duration;

    /// <summary>Hand (if hand tracking)</summary>
    public HandType Hand; // Left, Right, Both, Unknown
}

public enum GestureType
{
    Tap,
    DoubleTap,
    Hold,
    SwipeLeft,
    SwipeRight,
    SwipeUp,
    SwipeDown,
    Pinch,
    Spread,
    Rotate,
    Custom
}
```

#### MmMessageHaptic
```csharp
/// <summary>
/// Haptic feedback request
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageHaptic : MmMessage
{
    /// <summary>Device to vibrate</summary>
    public InputDeviceType Device;

    /// <summary>Intensity (0-1)</summary>
    public float Intensity;

    /// <summary>Duration in seconds</summary>
    public float Duration;

    /// <summary>Haptic pattern (if supported)</summary>
    public HapticPattern Pattern; // Constant, Pulse, Ramp, Custom

    /// <summary>Frequency (if supported)</summary>
    public float Frequency;
}
```

---

### 4. Task Namespace

#### MmMessageTaskAssigned
```csharp
namespace MercuryMessaging.StandardLibrary.Task
{
    /// <summary>
    /// Sent when a task is assigned
    /// </summary>
    [MmMessageVersion(1, 0)]
    public class MmMessageTaskAssigned : MmMessage
    {
        /// <summary>Task ID</summary>
        public string TaskID;

        /// <summary>Task name</summary>
        public string TaskName;

        /// <summary>Task description</summary>
        public string Description;

        /// <summary>Assignee (user/agent)</summary>
        public string Assignee;

        /// <summary>Priority</summary>
        public TaskPriority Priority; // Low, Normal, High, Critical

        /// <summary>Due date</summary>
        public System.DateTime DueDate;

        /// <summary>Task parameters</summary>
        public Dictionary<string, object> Parameters;
    }
}
```

#### MmMessageTaskProgress
```csharp
/// <summary>
/// Sent to update task progress
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageTaskProgress : MmMessage
{
    /// <summary>Task ID</summary>
    public string TaskID;

    /// <summary>Progress percentage (0-100)</summary>
    public float ProgressPercent;

    /// <summary>Status message</summary>
    public string StatusMessage;

    /// <summary>Current step</summary>
    public int CurrentStep;

    /// <summary>Total steps</summary>
    public int TotalSteps;

    /// <summary>Estimated time remaining</summary>
    public float EstimatedTimeRemaining;
}
```

#### MmMessageTaskCompleted
```csharp
/// <summary>
/// Sent when task is completed
/// </summary>
[MmMessageVersion(1, 0)]
public class MmMessageTaskCompleted : MmMessage
{
    /// <summary>Task ID</summary>
    public string TaskID;

    /// <summary>Success flag</summary>
    public bool Success;

    /// <summary>Result data</summary>
    public Dictionary<string, object> Results;

    /// <summary>Completion time</summary>
    public System.DateTime CompletionTime;

    /// <summary>Duration (seconds)</summary>
    public float Duration;

    /// <summary>Notes</summary>
    public string Notes;
}
```

---

## Message Versioning System

### Design Goals

1. **Backward Compatibility**: Old responders work with new messages
2. **Forward Compatibility**: New responders gracefully handle old messages
3. **Version Detection**: Know which version you're receiving
4. **Deprecation Warnings**: Notify when using old versions

### Versioning Architecture

```csharp
/// <summary>
/// Marks a message with version information
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MmMessageVersionAttribute : Attribute
{
    public int MajorVersion { get; private set; }
    public int MinorVersion { get; private set; }
    public string DeprecationMessage { get; set; }

    public MmMessageVersionAttribute(int major, int minor)
    {
        MajorVersion = major;
        MinorVersion = minor;
    }

    public Version Version => new Version(MajorVersion, MinorVersion);
}

/// <summary>
/// Base class for versioned messages
/// </summary>
public abstract class MmVersionedMessage : MmMessage
{
    /// <summary>Message version</summary>
    public Version MessageVersion { get; protected set; }

    /// <summary>Get version from attribute</summary>
    protected MmVersionedMessage()
    {
        var attr = GetType().GetCustomAttribute<MmMessageVersionAttribute>();
        if (attr != null)
        {
            MessageVersion = attr.Version;
        }
    }

    /// <summary>Check if compatible with another version</summary>
    public bool IsCompatibleWith(Version otherVersion)
    {
        // Major versions must match
        // Minor version can be >=
        return MessageVersion.Major == otherVersion.Major
            && MessageVersion.Minor >= otherVersion.Minor;
    }
}
```

### Version Evolution Example

```csharp
// Version 1.0
[MmMessageVersion(1, 0)]
public class MmMessageClick : MmVersionedMessage
{
    public Vector3 ClickPosition;
    public GameObject ClickedObject;
}

// Version 1.1 - Add optional fields (backward compatible)
[MmMessageVersion(1, 1)]
public class MmMessageClick : MmVersionedMessage
{
    public Vector3 ClickPosition;
    public GameObject ClickedObject;

    // New in 1.1
    public int PointerID = 0;
    public int Button = 0;
}

// Version 2.0 - Breaking change (new major version)
[MmMessageVersion(2, 0)]
public class MmMessageClick : MmVersionedMessage
{
    public ClickData ClickData; // Encapsulated structure
    public InputContext Context; // New required field
}

// Compatibility layer
public static class MmMessageClickCompat
{
    public static MmMessageClick UpgradeFrom1_0(MmMessageClick_v1_0 old)
    {
        return new MmMessageClick
        {
            ClickData = new ClickData
            {
                Position = old.ClickPosition,
                HitObject = old.ClickedObject
            }
        };
    }
}
```

---

## Design Decisions

### Decision 1: Namespace Organization

**Options:**
A. Single namespace (MercuryMessaging.StandardLibrary)
B. Multiple namespaces by domain (UI, AppState, Input, Task)
C. Hierarchical namespaces (StandardLibrary.UI.Input)

**Decision:** B (Multiple namespaces by domain)

**Rationale:**
- Clear organization by use case
- Easy to find relevant messages
- Can import only what you need
- Reduces naming conflicts
- Matches developer mental model

---

### Decision 2: Versioning Strategy

**Options:**
A. No versioning (break on changes)
B. Attribute-based versioning
C. Separate classes per version (MmMessageClick_v1_0, MmMessageClick_v1_1)

**Decision:** B (Attribute-based versioning)

**Rationale:**
- Clean API (no version suffixes)
- Metadata available via reflection
- Can detect version mismatches
- Supports gradual migration
- Similar to Unity's SerializeField versioning

---

### Decision 3: Message Scope

**Options:**
A. Cover every possible message type (100+)
B. Focus on most common 40-50 types
C. Minimal set (20 types) with extension points

**Decision:** B (40-50 common types)

**Rationale:**
- Covers 80% of use cases
- Manageable implementation scope
- Room for community additions
- Keeps library focused
- Extensible for custom messages

---

## Implementation Strategy

### Week 1: Architecture (12h)
1. Design namespace structure
2. Create versioning system
3. Set up unit test framework

### Week 2-3: UI Messages (24h)
4. Implement 10 UI message types
5. Create example responders
6. Write unit tests

### Week 3-4: AppState Messages (16h)
7. Implement 8 application state types
8. Create example responders
9. Write unit tests

### Week 4-5: Input Messages (20h)
10. Implement 12 input event types
11. Create example responders (VR focus)
12. Write unit tests

### Week 5-6: Task Messages (16h)
13. Implement 10 task management types
14. Integrate with existing MmTaskManager
15. Write unit tests

### Week 6: Integration (36h)
16. Create conversion utilities (24h)
17. Build compatibility layer (16h)
18. Integration testing (20h)

### Week 7: Documentation (44h)
19. API documentation (20h)
20. Tutorial scenes (24h)
21. Migration guide

---

## Open Questions

1. **Should messages support serialization beyond networking?**
   - JSON export for logging/debugging?
   - Binary format for save games?

2. **How to handle platform-specific messages?**
   - VR-only messages in separate namespace?
   - Feature flags?

3. **Community contributions?**
   - Process for accepting new standard messages?
   - Approval criteria?

---

## References

### Related Documents
- Master Plan: Phase 5.1
- Tasks: `standard-library-tasks.md`

### Related Code
- `Assets/MercuryMessaging/Protocol/Message/` - Existing message types
- `Assets/MercuryMessaging/Protocol/MmMethod.cs` - Method enum

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Framework Team
