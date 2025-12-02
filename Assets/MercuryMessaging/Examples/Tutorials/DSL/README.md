# MercuryMessaging DSL Quick Start Guide

## Overview

The MercuryMessaging DSL (Domain-Specific Language) provides a modern, fluent API that reduces code by **70-86%** while maintaining full type safety. This guide covers the essential patterns you need to get started.

## Installation

The DSL is built into MercuryMessaging. Just add the namespace:

```csharp
using MercuryMessaging.Protocol.DSL;
```

For Standard Library messages:
```csharp
using MercuryMessaging.StandardLibrary.UI;    // UI messages
using MercuryMessaging.StandardLibrary.Input; // VR/XR input messages
```

---

## Quick Reference

### Tier 1: Auto-Execute Methods (Simplest)

```csharp
// From MmRelayNode
relay.BroadcastInitialize();     // Initialize all descendants
relay.BroadcastRefresh();        // Refresh all descendants
relay.BroadcastSetActive(true);  // Activate all descendants
relay.BroadcastSwitch("State");  // Switch FSM state
relay.BroadcastValue(42);        // Send int to descendants
relay.BroadcastValue("Hello");   // Send string to descendants

relay.NotifyComplete();          // Notify completion to parents
relay.NotifyValue(100);          // Send int to parents

// SAME API from MmBaseResponder!
myResponder.BroadcastInitialize();
myResponder.NotifyComplete();
```

### Tier 2: Fluent Chains (Full Control)

```csharp
// Basic pattern
relay.Send("Hello").Execute();

// Direction targeting
relay.Send("Hello").ToChildren().Execute();     // Direct children
relay.Send("Hello").ToParents().Execute();      // Direct parents
relay.Send("Hello").ToDescendants().Execute();  // All descendants
relay.Send("Hello").ToAncestors().Execute();    // All ancestors
relay.Send("Hello").ToSiblings().Execute();     // Same level
relay.Send("Hello").ToAll().Execute();          // Bidirectional

// Filters
relay.Send("Hello").ToDescendants().Active().Execute();           // Active only
relay.Send("Hello").ToDescendants().WithTag(MmTag.Tag0).Execute(); // Tagged
relay.Send("Hello").ToDescendants().NetworkOnly().Execute();      // Network

// Combined
relay.Send("Score: 100")
    .ToDescendants()
    .Active()
    .WithTag(MmTag.Tag0)
    .LocalOnly()
    .Execute();
```

---

## Standard Library Messages

### UI Messages (100-199)

```csharp
// Send click message
relay.MmInvoke(new MmUIClickMessage(
    position: Input.mousePosition,
    clickCount: 1,
    button: 0  // 0=left, 1=right
));

// Handle in MmUIResponder
public class MyUIHandler : MmUIResponder
{
    protected override void ReceivedClick(MmUIClickMessage msg)
    {
        if (msg.IsDoubleClick) OpenItem();
        if (msg.IsRightClick) ShowContextMenu();
    }

    protected override void ReceivedHover(MmUIHoverMessage msg)
    {
        if (msg.IsEnter) ShowTooltip();
        else HideTooltip();
    }

    protected override void ReceivedDrag(MmUIDragMessage msg)
    {
        if (msg.Phase == MmDragPhase.Move)
            transform.position += (Vector3)msg.Delta;
    }
}
```

**Available UI Messages:**
- `MmUIClickMessage` - Click/tap events
- `MmUIHoverMessage` - Hover enter/exit
- `MmUIDragMessage` - Drag start/move/end
- `MmUIScrollMessage` - Scroll events
- `MmUIFocusMessage` - Focus gained/lost
- `MmUISelectMessage` - Selection changed
- `MmUISubmitMessage` - Submit/confirm action
- `MmUICancelMessage` - Cancel action

### Input Messages (200-299)

```csharp
// Send 6DOF tracking data
relay.MmInvoke(new MmInput6DOFMessage(
    hand: MmHandedness.Right,
    position: controller.position,
    rotation: controller.rotation
));

// Handle in MmInputResponder
public class MyVRHandler : MmInputResponder
{
    protected override void Received6DOF(MmInput6DOFMessage msg)
    {
        if (msg.IsTracked)
            handTransform.SetPositionAndRotation(msg.Position, msg.Rotation);
    }

    protected override void ReceivedGesture(MmInputGestureMessage msg)
    {
        if (msg.GestureType == MmGestureType.Pinch && msg.Confidence > 0.9f)
            GrabObject();
    }

    protected override void ReceivedHaptic(MmInputHapticMessage msg)
    {
        HapticController.Vibrate(msg.Hand, msg.Intensity, msg.Duration);
    }
}
```

**Available Input Messages:**
- `MmInput6DOFMessage` - Position + rotation + velocity
- `MmInputGestureMessage` - Hand gesture recognition
- `MmInputHapticMessage` - Vibration feedback
- `MmInputButtonMessage` - Button press/release
- `MmInputAxisMessage` - Joystick/trigger values
- `MmInputTouchMessage` - Touchpad input
- `MmInputControllerStateMessage` - Connection status
- `MmInputGazeMessage` - Eye tracking

---

## FSM Configuration

### Quick Setup

```csharp
switchNode.ConfigureStates()
    .OnEnter("MainMenu", () => ShowMenu())
    .OnEnter("Gameplay", () => StartGame())
    .OnExit("Gameplay", () => StopGame())
    .StartWith("MainMenu")
    .Build();
```

### Navigation

```csharp
switchNode.GoTo("Gameplay");           // Go to state
switchNode.GoToPrevious();             // Go back
bool inGame = switchNode.IsInState("Gameplay");
string current = switchNode.GetCurrentStateName();
```

### Full State Definition

```csharp
MmAppState.Configure(switchNode)
    .DefineState("MainMenu")
        .OnEnter(() => ShowMenuUI())
        .OnExit(() => HideMenuUI())
    .DefineState("Gameplay")
        .OnTransition(
            onEnter: () => EnablePlayer(),
            onExit: () => DisablePlayer()
        )
    .OnAnyStateEnter(() => LogStateChange())
    .StartWith("MainMenu")
    .Build();
```

---

## Migration from Traditional API

| Traditional (7 lines) | DSL (1 line) |
|----------------------|--------------|
| `relay.MmInvoke(MmMethod.MessageString, "Hello", new MmMetadataBlock(MmLevelFilter.Child, ...));` | `relay.Send("Hello").ToChildren().Execute();` |
| `relay.MmInvoke(MmMethod.Initialize);` | `relay.BroadcastInitialize();` |
| `relay.MmInvoke(MmMethod.Complete, true, new MmMetadataBlock(MmLevelFilter.Parent));` | `relay.NotifyComplete();` |

---

## Method Ranges

| Range | Purpose | API |
|-------|---------|-----|
| 0-18 | Standard MmMethod | Core framework |
| 100-199 | UI Messages | `MmUIMethod`, `MmUIResponder` |
| 200-299 | Input Messages | `MmInputMethod`, `MmInputResponder` |
| 1000+ | Custom Application | `MmExtendableResponder` |

---

## Tutorial Files

### Reference Tutorials (Read/Study)
1. **UnifiedMessagingTutorial.cs** - Tier 1/2 messaging API reference
2. **StandardLibraryTutorial.cs** - UI + Input messages reference
3. **FSMConfigurationTutorial.cs** - State machine configuration reference
4. **FluentDslExample.cs** - Side-by-side comparison (parent folder)

### Interactive Tutorials (Run/Play)
Located in `Scripts/` folder. Add to a GameObject and press Play:

5. **DSLSceneSetup.cs** - Auto-creates tutorial scene hierarchy
6. **DSLBasicDemo.cs** - Press 1-5 for messaging pattern demos
7. **DSLTemporalDemo.cs** - Press T/Y/U for time-based pattern demos
8. **ColorResponder.cs** - Visual feedback responder for tutorials

**Quick Start for Interactive Tutorials:**
```
1. Create empty scene
2. Create empty GameObject named "DSLTutorial"
3. Add DSLSceneSetup component
4. Press Play
5. Use keyboard to test patterns (1-5, T, Y, U)
```

**Keyboard Controls:**
| Key | Demo |
|-----|------|
| 1 | Traditional vs Fluent API comparison |
| 2 | Routing targets (Children/Descendants/Parents/All) |
| 3 | Typed values (int/float/string/bool/Vector3) |
| 4 | Combined filters (Active + Tag) |
| 5 | Auto-execute convenience methods |
| T | Delayed execution (2 second delay) |
| Y | Repeating messages (color cycling) |
| U | Conditional trigger (press SPACE to activate) |

---

## Best Practices

1. **Use Tier 1 for common patterns** - BroadcastInitialize(), NotifyComplete()
2. **Use Tier 2 for complex routing** - When you need filters, tags, or custom destinations
3. **Extend MmUIResponder for UI** - Type-safe handlers for UI events
4. **Extend MmInputResponder for VR** - Type-safe handlers for XR input
5. **Configure FSM early** - Call ConfigureStates() in Start() or Awake()

---

*Last Updated: 2025-11-25*
*Part of DSL Overhaul Phase 11*
