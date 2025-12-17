# Standardized Message Library - Task Checklist

**Last Updated:** 2025-11-18
**Status:** Ready to Start
**Total Effort:** 228 hours (6-7 weeks)

---

## Task Organization

Tasks are organized by namespace and feature area. Each task includes effort estimate, acceptance criteria, and dependencies.

**Progress Tracking:**
- [ ] Not started
- [🔨] In progress
- [✅] Complete

---

## Testing Standards

All tests for this project MUST follow these patterns:

### Required Approach
- ✅ Use **Unity Test Framework** (PlayMode or EditMode)
- ✅ Create **GameObjects programmatically** in `[SetUp]` methods
- ✅ All components added via `GameObject.AddComponent<T>()`
- ✅ Clean up in `[TearDown]` with `Object.DestroyImmediate()`

### Prohibited Patterns
- ❌ NO manual scene creation or loading
- ❌ NO manual UI element prefabs
- ❌ NO prefab dependencies from Resources folder

### Example Test Pattern
```csharp
[Test]
public void TestMessageVersioning()
{
    // Arrange - create message with version
    GameObject obj = new GameObject("TestObj");
    MmRelayNode relay = obj.AddComponent<MmRelayNode>();

    MmMessageUIClick clickMsg = new MmMessageUIClick();
    clickMsg.version = new Version(1, 0);
    clickMsg.elementId = "SubmitButton";

    // Act - send versioned message
    relay.MmInvoke(clickMsg);

    // Assert - verify version compatibility
    Assert.IsTrue(clickMsg.IsCompatible(new Version(1, 0)));

    // Cleanup
    Object.DestroyImmediate(obj);
}
```

---

## Phase 1: Architecture & Versioning (32 hours)

### Task 1.1: Design Message Library Architecture (12h)
- [ ] Define namespace structure (UI, AppState, Input, Task)
- [ ] Design base class hierarchy (MmVersionedMessage)
- [ ] Plan message naming conventions
- [ ] Design serialization strategy
- [ ] Document architecture decisions
- [ ] Review with team and get approval

**Acceptance:** Architecture document complete, namespace structure approved

**Dependencies:** None

---

### Task 1.2: Implement Versioning System (20h)
- [ ] Create MmMessageVersionAttribute class (4h)
- [ ] Implement MmVersionedMessage base class (4h)
- [ ] Add version compatibility checking (4h)
- [ ] Create version migration framework (4h)
- [ ] Implement deprecation warnings (2h)
- [ ] Write unit tests for versioning (2h)

**Acceptance:** Versioning system functional with tests passing

**Dependencies:** Task 1.1

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/Core/`

---

## Phase 2: UI Messages (40 hours)

### Task 2.1: Implement Click & Hover (8h)
- [ ] Create MmMessageClick class (3h)
  - ClickPosition, ClickedObject, PointerID
  - Button, Timestamp, IsDoubleClick
- [ ] Create MmMessageHover class (3h)
  - HoveredObject, IsEnter/Exit
  - HoverDuration, PointerID
- [ ] Write unit tests (2h)

**Acceptance:** Click and hover messages functional with full property support

**Dependencies:** Task 1.2

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/UI/`

---

### Task 2.2: Implement Drag & Drop (8h)
- [ ] Create DragState enum (1h)
- [ ] Create MmMessageDrag class (4h)
  - DraggedObject, positions, delta
  - State tracking (Started, Dragging, Ended)
- [ ] Create MmMessageDrop class (2h)
  - Drop target, drop position
- [ ] Write unit tests (1h)

**Acceptance:** Drag and drop messages support full gesture lifecycle

**Dependencies:** Task 2.1

---

### Task 2.3: Implement Focus & Blur (4h)
- [ ] Create MmMessageFocus class (2h)
- [ ] Create MmMessageBlur class (1h)
- [ ] Write unit tests (1h)

**Acceptance:** Focus/blur messages work for UI navigation

**Dependencies:** Task 2.1

---

### Task 2.4: Implement Scroll, Pinch, Zoom (12h)
- [ ] Create MmMessageScroll class (4h)
  - ScrollDelta, TotalScroll
  - ScrollObject reference
- [ ] Create PinchState enum (1h)
- [ ] Create MmMessagePinch class (4h)
  - PinchCenter, distance, delta
  - Scale factor
- [ ] Create MmMessageZoom class (2h)
- [ ] Write unit tests (1h)

**Acceptance:** Multi-touch gestures fully supported

**Dependencies:** Task 2.1

---

### Task 2.5: Implement Voice Command (4h)
- [ ] Create MmMessageVoiceCommand class (3h)
  - Command text, confidence
  - Language, timestamp
- [ ] Write unit tests (1h)

**Acceptance:** Voice command messages support speech recognition

**Dependencies:** Task 2.1

---

### Task 2.6: Create UI Example Responders (4h)
- [ ] ButtonClickResponder (1h)
- [ ] DraggableObjectResponder (1h)
- [ ] HoverHighlightResponder (1h)
- [ ] ScrollViewResponder (1h)

**Acceptance:** Example responders demonstrate each UI message type

**Dependencies:** Tasks 2.1-2.5

---

## Phase 3: AppState Messages (28 hours)

### Task 3.1: Implement Lifecycle Messages (8h)
- [ ] Create MmMessageInitialize class (2h)
  - Initialization parameters
- [ ] Create MmMessageShutdown class (2h)
  - Shutdown reason, cleanup flags
- [ ] Create MmMessagePause class (1h)
- [ ] Create MmMessageResume class (1h)
- [ ] Write unit tests (2h)

**Acceptance:** Application lifecycle messages functional

**Dependencies:** Task 1.2

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/AppState/`

---

### Task 3.2: Implement State Management (8h)
- [ ] Create MmMessageStateChange class (4h)
  - Previous/new state names
  - State data dictionary
  - Change reason, timestamp
- [ ] Create SaveLoadOperation enum (1h)
- [ ] Create MmMessageSaveLoad class (3h)

**Acceptance:** State management messages support FSM integration

**Dependencies:** Task 3.1

---

### Task 3.3: Implement Error & Notification (6h)
- [ ] Create ErrorSeverity enum (1h)
- [ ] Create MmMessageError class (3h)
  - Error message, code, severity
  - Stack trace, exception
- [ ] Create MmMessageNotification class (2h)

**Acceptance:** Error and notification messages provide rich debugging info

**Dependencies:** Task 3.1

---

### Task 3.4: Create AppState Example Responders (6h)
- [ ] StateMachineResponder (2h)
- [ ] SaveLoadResponder (2h)
- [ ] ErrorHandlerResponder (1h)
- [ ] NotificationResponder (1h)

**Acceptance:** Example responders demonstrate AppState patterns

**Dependencies:** Tasks 3.1-3.3

---

## Phase 4: Input Messages (36 hours)

### Task 4.1: Implement 6DOF Tracking (8h)
- [ ] Create InputDeviceType enum (1h)
- [ ] Create MmMessage6DOF class (5h)
  - Position, rotation
  - Velocity, angular velocity
  - Tracking confidence, device type
- [ ] Write unit tests (2h)

**Acceptance:** 6DOF messages support full VR tracking

**Dependencies:** Task 1.2

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/Input/`

---

### Task 4.2: Implement Gesture Recognition (8h)
- [ ] Create GestureType enum (2h)
  - Tap, Swipe, Pinch, etc.
- [ ] Create HandType enum (1h)
- [ ] Create MmMessageGesture class (4h)
  - Type, confidence, direction
  - Touch points, duration, hand
- [ ] Write unit tests (1h)

**Acceptance:** Gesture messages support hand tracking

**Dependencies:** Task 4.1

---

### Task 4.3: Implement Haptic Feedback (4h)
- [ ] Create HapticPattern enum (1h)
- [ ] Create MmMessageHaptic class (2h)
  - Device, intensity, duration
  - Pattern, frequency
- [ ] Write unit tests (1h)

**Acceptance:** Haptic messages support controller vibration

**Dependencies:** Task 4.1

---

### Task 4.4: Implement Controller Input (8h)
- [ ] Create MmMessageControllerButton class (2h)
- [ ] Create MmMessageControllerTrigger class (2h)
- [ ] Create MmMessageControllerGrip class (2h)
- [ ] Create MmMessageControllerThumbstick class (2h)

**Acceptance:** All VR controller inputs supported

**Dependencies:** Task 4.1

---

### Task 4.5: Implement Advanced Tracking (8h)
- [ ] Create MmMessageGazeTarget class (2h)
- [ ] Create MmMessageHeadTracking class (2h)
- [ ] Create MmMessageHandTracking class (2h)
- [ ] Create MmMessageEyeTracking class (1h)
- [ ] Create MmMessageBodyTracking class (1h)

**Acceptance:** Advanced VR/AR tracking messages complete

**Dependencies:** Task 4.1

---

## Phase 5: Task Messages (28 hours)

### Task 5.1: Implement Task Lifecycle (12h)
- [ ] Create TaskPriority enum (1h)
- [ ] Create MmMessageTaskAssigned class (3h)
  - Task ID, name, description
  - Assignee, priority, due date
- [ ] Create MmMessageTaskStarted class (2h)
- [ ] Create MmMessageTaskCompleted class (3h)
  - Success flag, results
  - Completion time, duration
- [ ] Create MmMessageTaskFailed class (2h)
- [ ] Write unit tests (1h)

**Acceptance:** Task lifecycle messages support full task management

**Dependencies:** Task 1.2

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/Task/`

---

### Task 5.2: Implement Task Progress (8h)
- [ ] Create MmMessageTaskProgress class (4h)
  - Progress percentage
  - Current step / total steps
  - Status message, time remaining
- [ ] Create MmMessageTaskMilestone class (2h)
- [ ] Write unit tests (2h)

**Acceptance:** Progress tracking messages provide detailed status

**Dependencies:** Task 5.1

---

### Task 5.3: Implement Task Coordination (4h)
- [ ] Create MmMessageTaskCancelled class (1h)
- [ ] Create MmMessageTaskDependency class (2h)
- [ ] Create MmMessageTaskBatch class (1h)

**Acceptance:** Task coordination messages support complex workflows

**Dependencies:** Task 5.1

---

### Task 5.4: Create Task Example Responders (4h)
- [ ] TaskQueueResponder (2h)
- [ ] ProgressBarResponder (1h)
- [ ] TaskCoordinatorResponder (1h)

**Acceptance:** Example responders demonstrate task management patterns

**Dependencies:** Tasks 5.1-5.3

---

## Phase 6: Integration & Compatibility (36 hours)

### Task 6.1: Create Conversion Utilities (12h)
- [ ] Build MmMessageConverter class (4h)
  - Convert generic messages to standard
  - Convert standard to generic
- [ ] Create migration helpers (4h)
  - Batch convert existing messages
  - Scan project for conversions
- [ ] Write unit tests (4h)

**Acceptance:** Conversion between old and new message types seamless

**Dependencies:** All Phase 2-5 tasks

**Code Location:** `Assets/MercuryMessaging/StandardLibrary/Utilities/`

---

### Task 6.2: Build Compatibility Layer (16h)
- [ ] Create backward compatibility wrappers (8h)
  - Old responders work with new messages
- [ ] Implement forward compatibility (4h)
  - New responders handle old messages
- [ ] Add version mismatch detection (2h)
- [ ] Write unit tests (2h)

**Acceptance:** Smooth migration path for existing projects

**Dependencies:** Task 6.1

---

### Task 6.3: Integration Testing (8h)
- [ ] Test all message types end-to-end (4h)
- [ ] Test versioning scenarios (2h)
- [ ] Test compatibility layer (2h)

**Acceptance:** All messages work correctly in real scenarios

**Dependencies:** Task 6.2

---

## Phase 7: Documentation & Examples (28 hours)

### Task 7.1: API Documentation (20h)
- [ ] Document UI namespace (5h)
  - Class reference
  - Usage examples
  - Best practices
- [ ] Document AppState namespace (4h)
- [ ] Document Input namespace (6h)
- [ ] Document Task namespace (5h)

**Acceptance:** Complete API reference with examples

**Dependencies:** All implementation tasks

---

### Task 7.2: Create Tutorial Scenes (24h)
- [ ] UI Messages tutorial scene (6h)
  - Interactive button demo
  - Drag-drop demo
  - Scroll demo
- [ ] AppState tutorial scene (4h)
  - State machine demo
  - Save/load demo
- [ ] Input tutorial scene (8h)
  - VR controller demo
  - Gesture recognition demo
  - Haptic feedback demo
- [ ] Task tutorial scene (6h)
  - Task queue demo
  - Progress tracking demo

**Acceptance:** 4 tutorial scenes demonstrating all message categories

**Dependencies:** Task 7.1

**Code Location:** `Assets/MercuryMessaging/Examples/StandardLibrary/`

---

### Task 7.3: Migration Guide (4h)
- [ ] Write conversion guide (2h)
  - From generic messages
  - From custom messages
- [ ] Create migration checklist (1h)
- [ ] Add troubleshooting section (1h)

**Acceptance:** Clear migration guide for existing projects

**Dependencies:** Task 7.1

---

## Summary

**Total Tasks:** 37
**Total Effort:** 228 hours (6-7 weeks)

**Phase Breakdown:**
- Phase 1 (Architecture & Versioning): 32h
- Phase 2 (UI Messages): 40h
- Phase 3 (AppState Messages): 28h
- Phase 4 (Input Messages): 36h
- Phase 5 (Task Messages): 28h
- Phase 6 (Integration & Compatibility): 36h
- Phase 7 (Documentation & Examples): 28h

**Message Type Count:**
- UI: 10+ messages
- AppState: 8+ messages
- Input: 12+ messages
- Task: 10+ messages
- **Total: 40+ standardized message types**

**Critical Path:** Phases must be done sequentially (versioning → messages → integration → docs)

---

## Getting Started

**First 3 tasks to complete:**
1. Task 1.1: Design Message Library Architecture (12h)
2. Task 1.2: Implement Versioning System (20h)
3. Task 2.1: Implement Click & Hover (8h)

These tasks establish the foundation for all other work.

---

## Integration with Existing Systems

### Task Manager Integration
- Coordinate with existing `MmTaskManager.cs` in `Assets/MercuryMessaging/Task/`
- Ensure standard task messages work with current task system
- Add migration from old task format

### FSM Integration
- Ensure state messages work with `MmRelaySwitchNode`
- Add state change listeners
- Test with existing FSM examples

### Network Integration
- Verify all messages serialize correctly
- Test with Photon networking
- Add network-specific examples

---

**Document Version:** 1.0
**Maintained By:** Framework Team
