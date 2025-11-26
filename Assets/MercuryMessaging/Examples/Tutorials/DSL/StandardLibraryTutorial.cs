// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// StandardLibraryTutorial.cs - Tutorial for Standard Library Messages
// Part of DSL Overhaul Phase 11
//
// This tutorial demonstrates the Standard Library message types:
// - UI Messages (100-199): Click, Hover, Drag, Scroll, Focus, Select, Submit, Cancel
// - Input Messages (200-299): 6DOF, Gesture, Haptic, Button, Axis, Touch, Gaze

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.StandardLibrary.UI;
using MercuryMessaging.StandardLibrary.Input;

namespace MercuryMessaging.Examples.Tutorials.DSL
{
    /// <summary>
    /// Tutorial: Standard Library Messages
    ///
    /// The Standard Library provides pre-defined message types for common use cases:
    /// - UI Messages: For user interface interactions
    /// - Input Messages: For VR/XR controller input
    ///
    /// Method Ranges:
    /// - Standard MmMethod: 0-18 (existing)
    /// - UI Messages: 100-199 (MmUIMethod)
    /// - Input Messages: 200-299 (MmInputMethod)
    /// - Custom Application: 1000+ (unchanged)
    /// </summary>
    public class StandardLibraryTutorial : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
            if (relay == null)
            {
                Debug.LogError("StandardLibraryTutorial requires an MmRelayNode component");
                return;
            }

            UIMessageExamples();
            InputMessageExamples();
        }

        #region UI Message Examples

        /// <summary>
        /// UI Messages (100-199) for user interface interactions.
        /// Use MmUIResponder to receive these messages with type-safe handlers.
        /// </summary>
        void UIMessageExamples()
        {
            Debug.Log("=== UI MESSAGE EXAMPLES ===");

            // =========================================
            // CLICK MESSAGE
            // =========================================
            var clickMsg = new MmUIClickMessage(
                position: new Vector2(100, 200),
                clickCount: 1,
                button: 0  // 0=left, 1=right, 2=middle
            );
            relay.MmInvoke(clickMsg);
            Debug.Log($"Sent click at {clickMsg.Position}, IsDoubleClick={clickMsg.IsDoubleClick}");

            // Double-click example
            var doubleClickMsg = new MmUIClickMessage(new Vector2(100, 200), clickCount: 2);
            Debug.Log($"IsDoubleClick: {doubleClickMsg.IsDoubleClick}");  // true

            // Right-click example
            var rightClickMsg = new MmUIClickMessage(new Vector2(100, 200), clickCount: 1, button: 1);
            Debug.Log($"IsRightClick: {rightClickMsg.IsRightClick}");  // true

            // =========================================
            // HOVER MESSAGE
            // =========================================
            var hoverEnter = new MmUIHoverMessage(
                position: new Vector2(150, 250),
                isEnter: true
            );
            relay.MmInvoke(hoverEnter);
            Debug.Log($"Hover {(hoverEnter.IsEnter ? "entered" : "exited")} at {hoverEnter.Position}");

            // =========================================
            // DRAG MESSAGE
            // =========================================
            // Drag begin
            relay.MmInvoke(new MmUIDragMessage(
                position: new Vector2(100, 100),
                delta: Vector2.zero,
                phase: MmDragPhase.Begin
            ));

            // Drag move
            relay.MmInvoke(new MmUIDragMessage(
                position: new Vector2(150, 120),
                delta: new Vector2(50, 20),
                phase: MmDragPhase.Move
            ));

            // Drag end
            relay.MmInvoke(new MmUIDragMessage(
                position: new Vector2(200, 150),
                delta: new Vector2(50, 30),
                phase: MmDragPhase.End
            ));

            // =========================================
            // SCROLL MESSAGE
            // =========================================
            var scrollMsg = new MmUIScrollMessage(
                position: new Vector2(200, 300),
                scrollDelta: new Vector2(0, 120)  // Scroll up
            );
            relay.MmInvoke(scrollMsg);

            // =========================================
            // FOCUS MESSAGE
            // =========================================
            var focusGained = new MmUIFocusMessage(
                isFocused: true,
                elementId: "InputField_Username"
            );
            relay.MmInvoke(focusGained);

            // =========================================
            // SELECT MESSAGE
            // =========================================
            var selectMsg = new MmUISelectMessage(
                selectedIndex: 2,
                selectedValue: "Option C",
                previousIndex: 0
            );
            relay.MmInvoke(selectMsg);
            Debug.Log($"Selected '{selectMsg.SelectedValue}' at index {selectMsg.SelectedIndex}");

            // =========================================
            // SUBMIT / CANCEL MESSAGES
            // =========================================
            relay.MmInvoke(new MmUISubmitMessage(data: "FormData"));
            relay.MmInvoke(new MmUICancelMessage(reason: "User pressed Escape"));

            Debug.Log("UI message examples complete");
        }

        #endregion

        #region Input Message Examples

        /// <summary>
        /// Input Messages (200-299) for VR/XR controller input.
        /// Use MmInputResponder to receive these messages with type-safe handlers.
        /// </summary>
        void InputMessageExamples()
        {
            Debug.Log("=== INPUT MESSAGE EXAMPLES ===");

            // =========================================
            // 6DOF MESSAGE (Position + Rotation)
            // =========================================
            var dof6Msg = new MmInput6DOFMessage(
                hand: MmHandedness.Right,
                position: new Vector3(0.3f, 1.2f, 0.5f),
                rotation: Quaternion.Euler(0, 45, 0),
                velocity: new Vector3(0.1f, 0, 0),
                angularVelocity: Vector3.zero,
                isTracked: true
            );
            relay.MmInvoke(dof6Msg);
            Debug.Log($"6DOF: {dof6Msg.Hand} hand at {dof6Msg.Position}, tracked={dof6Msg.IsTracked}");

            // =========================================
            // GESTURE MESSAGE
            // =========================================
            var gestureMsg = new MmInputGestureMessage(
                hand: MmHandedness.Left,
                gestureType: MmGestureType.Pinch,
                confidence: 0.95f,
                progress: 0.8f
            );
            relay.MmInvoke(gestureMsg);
            Debug.Log($"Gesture: {gestureMsg.GestureType} ({gestureMsg.Confidence:P0} confidence)");

            // Custom gesture
            var customGesture = new MmInputGestureMessage(
                hand: MmHandedness.Right,
                gestureType: MmGestureType.Custom,
                confidence: 0.9f,
                progress: 1f,
                customName: "ThumbTap"
            );
            relay.MmInvoke(customGesture);

            // =========================================
            // HAPTIC MESSAGE (Vibration)
            // =========================================
            var hapticMsg = new MmInputHapticMessage(
                hand: MmHandedness.Both,
                intensity: 0.75f,  // 75% strength
                duration: 0.2f,   // 200ms
                frequency: 150f   // 150 Hz (if supported)
            );
            relay.MmInvoke(hapticMsg);
            Debug.Log($"Haptic: {hapticMsg.Intensity:P0} for {hapticMsg.Duration}s");

            // =========================================
            // BUTTON MESSAGE
            // =========================================
            // Trigger pressed
            var triggerPressed = new MmInputButtonMessage(
                hand: MmHandedness.Right,
                buttonId: 0,
                buttonName: "Trigger",
                state: MmButtonState.Pressed,
                value: 1f
            );
            relay.MmInvoke(triggerPressed);

            // Trigger released
            var triggerReleased = new MmInputButtonMessage(
                hand: MmHandedness.Right,
                buttonId: 0,
                buttonName: "Trigger",
                state: MmButtonState.Released,
                value: 0f
            );
            relay.MmInvoke(triggerReleased);

            // =========================================
            // AXIS MESSAGE (Joystick/Trigger)
            // =========================================
            var joystickMsg = new MmInputAxisMessage(
                hand: MmHandedness.Left,
                axisId: 0,
                axisName: "Joystick",
                value2D: new Vector2(0.5f, -0.3f),  // Thumbstick position
                value1D: 0f
            );
            relay.MmInvoke(joystickMsg);
            Debug.Log($"Joystick: {joystickMsg.Value2D}");

            // =========================================
            // TOUCH MESSAGE (Touchpad)
            // =========================================
            var touchMsg = new MmInputTouchMessage(
                hand: MmHandedness.Right,
                touchId: 0,
                position: new Vector2(0.5f, 0.5f),  // Center
                delta: new Vector2(0.1f, 0),        // Moved right
                phase: MmTouchPhase.Moved
            );
            relay.MmInvoke(touchMsg);

            // =========================================
            // CONTROLLER STATE MESSAGE
            // =========================================
            var controllerConnected = new MmInputControllerStateMessage(
                hand: MmHandedness.Right,
                isConnected: true,
                controllerType: "Quest Pro Controller",
                batteryLevel: 0.85f
            );
            relay.MmInvoke(controllerConnected);
            Debug.Log($"Controller: {controllerConnected.ControllerType}, battery {controllerConnected.BatteryLevel:P0}");

            // =========================================
            // GAZE MESSAGE (Eye Tracking)
            // =========================================
            var gazeMsg = new MmInputGazeMessage(
                origin: new Vector3(0, 1.6f, 0),      // Eye position
                direction: Vector3.forward,           // Looking forward
                hitPoint: new Vector3(0, 1.6f, 5f),  // Hit point 5m ahead
                isHitting: true,
                confidence: 0.92f
            );
            relay.MmInvoke(gazeMsg);
            Debug.Log($"Gaze: hitting={gazeMsg.IsHitting} at {gazeMsg.HitPoint}");

            Debug.Log("Input message examples complete");
        }

        #endregion
    }

    /// <summary>
    /// Example UI responder showing how to handle UI messages.
    /// Extend MmUIResponder and override the handlers you need.
    /// </summary>
    public class ExampleUIHandler : MmUIResponder
    {
        [SerializeField] private UnityEngine.UI.Text statusText;

        protected override void ReceivedClick(MmUIClickMessage message)
        {
            Debug.Log($"Click at {message.Position}");

            if (message.IsDoubleClick)
            {
                Debug.Log("Double-click detected - opening item");
            }

            if (message.IsRightClick)
            {
                Debug.Log("Right-click detected - showing context menu");
            }
        }

        protected override void ReceivedHover(MmUIHoverMessage message)
        {
            if (message.IsEnter)
            {
                // Show tooltip
                Debug.Log("Mouse entered - show tooltip");
            }
            else
            {
                // Hide tooltip
                Debug.Log("Mouse exited - hide tooltip");
            }
        }

        protected override void ReceivedDrag(MmUIDragMessage message)
        {
            switch (message.Phase)
            {
                case MmDragPhase.Begin:
                    Debug.Log($"Drag started at {message.Position}");
                    break;

                case MmDragPhase.Move:
                    // Move object by delta
                    transform.position += (Vector3)message.Delta * 0.01f;
                    break;

                case MmDragPhase.End:
                    Debug.Log($"Drag ended at {message.Position}");
                    break;
            }
        }

        protected override void ReceivedSelect(MmUISelectMessage message)
        {
            Debug.Log($"Selected '{message.SelectedValue}' (was index {message.PreviousIndex})");
            if (statusText != null)
                statusText.text = $"Selected: {message.SelectedValue}";
        }

        protected override void ReceivedSubmit(MmUISubmitMessage message)
        {
            Debug.Log($"Form submitted with data: {message.Data}");
        }

        protected override void ReceivedCancel(MmUICancelMessage message)
        {
            Debug.Log($"Cancelled: {message.Reason}");
        }
    }

    /// <summary>
    /// Example VR input handler showing how to handle Input messages.
    /// Extend MmInputResponder and override the handlers you need.
    /// </summary>
    public class ExampleVRInputHandler : MmInputResponder
    {
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private GameObject laserPointer;

        protected override void Received6DOF(MmInput6DOFMessage message)
        {
            // Update hand transform based on tracking data
            Transform handTransform = message.Hand == MmHandedness.Right
                ? rightHandTransform
                : leftHandTransform;

            if (handTransform != null && message.IsTracked)
            {
                handTransform.SetPositionAndRotation(message.Position, message.Rotation);
            }
        }

        protected override void ReceivedGesture(MmInputGestureMessage message)
        {
            // Only react to high-confidence gestures
            if (message.Confidence < 0.8f) return;

            switch (message.GestureType)
            {
                case MmGestureType.Pinch:
                    Debug.Log($"Pinch detected on {message.Hand} hand");
                    // Select/grab object
                    break;

                case MmGestureType.Point:
                    Debug.Log($"Pointing with {message.Hand} hand");
                    // Enable laser pointer
                    if (laserPointer != null)
                        laserPointer.SetActive(true);
                    break;

                case MmGestureType.OpenHand:
                    Debug.Log($"Open hand on {message.Hand}");
                    // Release object
                    break;
            }
        }

        protected override void ReceivedButton(MmInputButtonMessage message)
        {
            Debug.Log($"{message.ButtonName} {message.State} on {message.Hand}");

            if (message.ButtonName == "Trigger" && message.State == MmButtonState.Pressed)
            {
                // Fire/select action
            }

            if (message.ButtonName == "Grip" && message.State == MmButtonState.Pressed)
            {
                // Grab action
            }
        }

        protected override void ReceivedAxis(MmInputAxisMessage message)
        {
            if (message.AxisName == "Joystick")
            {
                // Use for locomotion
                Vector2 input = message.Value2D;
                // Move player based on input
            }
        }

        protected override void ReceivedGaze(MmInputGazeMessage message)
        {
            if (message.IsHitting)
            {
                // Highlight object at gaze point
                Debug.DrawLine(message.Origin, message.HitPoint, Color.green);
            }
        }

        protected override void ReceivedControllerState(MmInputControllerStateMessage message)
        {
            if (!message.IsConnected)
            {
                Debug.LogWarning($"{message.Hand} controller disconnected!");
            }

            if (message.BatteryLevel < 0.2f && message.BatteryLevel >= 0)
            {
                Debug.LogWarning($"{message.Hand} controller battery low: {message.BatteryLevel:P0}");
            }
        }
    }
}
