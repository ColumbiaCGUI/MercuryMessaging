# Standard Library Messages

The Standard Library provides pre-defined message types for common use cases. These extend the core messaging system with type-safe handlers.

## Method Ranges

| Range | Purpose | API |
|-------|---------|-----|
| 0-18 | Standard MmMethod | Core framework |
| 100-199 | UI Messages | `MmUIMethod`, `MmUIResponder` |
| 200-299 | Input Messages | `MmInputMethod`, `MmInputResponder` |
| 1000+ | Custom Application | `MmExtendableResponder` |

---

## UI Messages (100-199)

For user interface interactions. Extend `MmUIResponder` to receive these messages:

```csharp
using MercuryMessaging.StandardLibrary.UI;

// Send click message
relay.MmInvoke(new MmUIClickMessage(
    position: Input.mousePosition,
    clickCount: 1,
    button: 0  // 0=left, 1=right, 2=middle
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
            transform.position += (Vector3)msg.Delta * 0.01f;
    }

    protected override void ReceivedSelect(MmUISelectMessage msg)
    {
        Debug.Log($"Selected: {msg.SelectedValue}");
    }
}
```

### Available UI Message Types

- `MmUIClickMessage` - Click/tap events (Position, ClickCount, Button, IsDoubleClick, IsRightClick)
- `MmUIHoverMessage` - Hover enter/exit (Position, IsEnter)
- `MmUIDragMessage` - Drag operations (Position, Delta, Phase: Begin/Move/End)
- `MmUIScrollMessage` - Scroll events (Position, ScrollDelta)
- `MmUIFocusMessage` - Focus gained/lost (IsFocused, ElementId)
- `MmUISelectMessage` - Selection changed (SelectedIndex, SelectedValue, PreviousIndex)
- `MmUISubmitMessage` - Submit/confirm (Data)
- `MmUICancelMessage` - Cancel action (Reason)

---

## Input Messages (200-299)

For VR/XR controller input. Extend `MmInputResponder` to receive these messages:

```csharp
using MercuryMessaging.StandardLibrary.Input;

// Send 6DOF tracking data
relay.MmInvoke(new MmInput6DOFMessage(
    hand: MmHandedness.Right,
    position: controller.position,
    rotation: controller.rotation,
    isTracked: true
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

    protected override void ReceivedButton(MmInputButtonMessage msg)
    {
        if (msg.ButtonName == "Trigger" && msg.State == MmButtonState.Pressed)
            Fire();
    }

    protected override void ReceivedHaptic(MmInputHapticMessage msg)
    {
        // Apply haptic feedback
        XRController.SendHaptic(msg.Hand, msg.Intensity, msg.Duration);
    }
}
```

### Available Input Message Types

- `MmInput6DOFMessage` - 6DOF tracking (Hand, Position, Rotation, Velocity, IsTracked)
- `MmInputGestureMessage` - Hand gestures (Hand, GestureType, Confidence, Progress)
- `MmInputHapticMessage` - Vibration (Hand, Intensity, Duration, Frequency)
- `MmInputButtonMessage` - Button input (Hand, ButtonId, ButtonName, State, Value)
- `MmInputAxisMessage` - Joystick/trigger (Hand, AxisName, Value2D, Value1D)
- `MmInputTouchMessage` - Touchpad (Hand, Position, Delta, Phase)
- `MmInputControllerStateMessage` - Connection (Hand, IsConnected, ControllerType, BatteryLevel)
- `MmInputGazeMessage` - Eye tracking (Origin, Direction, HitPoint, IsHitting, Confidence)

### Supporting Enums

- `MmHandedness`: Unknown, Left, Right, Both
- `MmGestureType`: Pinch, Point, Fist, OpenHand, ThumbsUp, Wave, etc.
- `MmButtonState`: Released, Pressed, Held
- `MmTouchPhase`: Began, Moved, Stationary, Ended, Canceled
