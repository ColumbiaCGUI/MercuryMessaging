// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmInputMessages.cs - Standard Library Input Message Types
// Part of DSL Overhaul Phase 10
//
// Input messages use MmMethod values 200-299 (Standard Library Input range)
// Designed for VR/XR input handling including 6DOF, gestures, and haptics

using System.Linq;
using UnityEngine;

namespace MercuryMessaging.StandardLibrary.Input
{
    #region Enums

    /// <summary>
    /// Standard Library Input methods (200-299 range).
    /// Use these with MmInputResponder for type-safe input event handling.
    /// </summary>
    public enum MmInputMethod
    {
        /// <summary>6DOF controller input (position + rotation)</summary>
        Input6DOF = 200,

        /// <summary>Hand gesture recognized</summary>
        Gesture = 201,

        /// <summary>Haptic feedback request</summary>
        Haptic = 202,

        /// <summary>Button press/release</summary>
        Button = 203,

        /// <summary>Axis value changed (joystick, trigger)</summary>
        Axis = 204,

        /// <summary>Touch surface input</summary>
        Touch = 205,

        /// <summary>Controller connection/disconnection</summary>
        ControllerState = 206,

        /// <summary>Eye tracking gaze point</summary>
        Gaze = 207
    }

    /// <summary>
    /// Input message types for serialization (2101+ range).
    /// </summary>
    public enum MmInputMessageType : short
    {
        Input6DOF = 2101,
        InputGesture = 2102,
        InputHaptic = 2103,
        InputButton = 2104,
        InputAxis = 2105,
        InputTouch = 2106,
        InputControllerState = 2107,
        InputGaze = 2108
    }

    /// <summary>
    /// Hand/controller identifier.
    /// </summary>
    public enum MmHandedness
    {
        Unknown = 0,
        Left = 1,
        Right = 2,
        Both = 3
    }

    /// <summary>
    /// Common gesture types for VR/XR.
    /// </summary>
    public enum MmGestureType
    {
        None = 0,
        Pinch,
        Point,
        Fist,
        OpenHand,
        ThumbsUp,
        ThumbsDown,
        Wave,
        Swipe,
        Tap,
        Custom
    }

    /// <summary>
    /// Button state.
    /// </summary>
    public enum MmButtonState
    {
        Released = 0,
        Pressed = 1,
        Held = 2
    }

    /// <summary>
    /// Touch phase for touch surface input.
    /// </summary>
    public enum MmTouchPhase
    {
        Began,
        Moved,
        Stationary,
        Ended,
        Canceled
    }

    #endregion

    #region 6DOF Message

    /// <summary>
    /// 6DOF (Six Degrees of Freedom) input message.
    /// Contains position and rotation for VR/XR controller tracking.
    /// </summary>
    /// <example>
    /// <code>
    /// // Send 6DOF update
    /// relay.Send(new MmInput6DOFMessage(
    ///     MmHandedness.Right,
    ///     transform.position,
    ///     transform.rotation,
    ///     velocity
    /// )).Execute();
    /// </code>
    /// </example>
    public class MmInput6DOFMessage : MmMessage
    {
        /// <summary>Which hand/controller this data is for</summary>
        public MmHandedness Hand;

        /// <summary>World-space position</summary>
        public Vector3 Position;

        /// <summary>World-space rotation</summary>
        public Quaternion Rotation;

        /// <summary>Movement velocity (optional)</summary>
        public Vector3 Velocity;

        /// <summary>Angular velocity (optional)</summary>
        public Vector3 AngularVelocity;

        /// <summary>Whether tracking is currently valid</summary>
        public bool IsTracked;

        public MmInput6DOFMessage() : base((MmMethod)MmInputMethod.Input6DOF, (MmMessageType)MmInputMessageType.Input6DOF)
        {
            Rotation = Quaternion.identity;
            IsTracked = true;
        }

        public MmInput6DOFMessage(MmHandedness hand, Vector3 position, Quaternion rotation,
            Vector3 velocity = default, Vector3 angularVelocity = default, bool isTracked = true,
            MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Input6DOF, (MmMessageType)MmInputMessageType.Input6DOF, metadataBlock)
        {
            Hand = hand;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            IsTracked = isTracked;
        }

        public MmInput6DOFMessage(MmInput6DOFMessage message) : base(message)
        {
            Hand = message.Hand;
            Position = message.Position;
            Rotation = message.Rotation;
            Velocity = message.Velocity;
            AngularVelocity = message.AngularVelocity;
            IsTracked = message.IsTracked;
        }

        public override MmMessage Copy() => new MmInput6DOFMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            Position = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            Rotation = new Quaternion((float)data[index++], (float)data[index++], (float)data[index++], (float)data[index++]);
            Velocity = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            AngularVelocity = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            IsTracked = (bool)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand,
                Position.x, Position.y, Position.z,
                Rotation.x, Rotation.y, Rotation.z, Rotation.w,
                Velocity.x, Velocity.y, Velocity.z,
                AngularVelocity.x, AngularVelocity.y, AngularVelocity.z,
                IsTracked
            }).ToArray();
        }
    }

    #endregion

    #region Gesture Message

    /// <summary>
    /// Hand gesture recognition message.
    /// Sent when a gesture is recognized from hand tracking.
    /// </summary>
    public class MmInputGestureMessage : MmMessage
    {
        /// <summary>Which hand performed the gesture</summary>
        public MmHandedness Hand;

        /// <summary>Type of gesture recognized</summary>
        public MmGestureType GestureType;

        /// <summary>Confidence score (0-1)</summary>
        public float Confidence;

        /// <summary>Gesture progress for continuous gestures (0-1)</summary>
        public float Progress;

        /// <summary>Custom gesture name (for GestureType.Custom)</summary>
        public string CustomName;

        public MmInputGestureMessage() : base((MmMethod)MmInputMethod.Gesture, (MmMessageType)MmInputMessageType.InputGesture)
        {
            CustomName = string.Empty;
        }

        public MmInputGestureMessage(MmHandedness hand, MmGestureType gestureType,
            float confidence = 1f, float progress = 1f, string customName = null,
            MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Gesture, (MmMessageType)MmInputMessageType.InputGesture, metadataBlock)
        {
            Hand = hand;
            GestureType = gestureType;
            Confidence = confidence;
            Progress = progress;
            CustomName = customName ?? string.Empty;
        }

        public MmInputGestureMessage(MmInputGestureMessage message) : base(message)
        {
            Hand = message.Hand;
            GestureType = message.GestureType;
            Confidence = message.Confidence;
            Progress = message.Progress;
            CustomName = message.CustomName;
        }

        public override MmMessage Copy() => new MmInputGestureMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            GestureType = (MmGestureType)(int)data[index++];
            Confidence = (float)data[index++];
            Progress = (float)data[index++];
            CustomName = (string)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand, (int)GestureType, Confidence, Progress, CustomName ?? string.Empty
            }).ToArray();
        }
    }

    #endregion

    #region Haptic Message

    /// <summary>
    /// Haptic feedback request message.
    /// Sent to trigger vibration/feedback on controllers.
    /// </summary>
    public class MmInputHapticMessage : MmMessage
    {
        /// <summary>Target controller</summary>
        public MmHandedness Hand;

        /// <summary>Vibration intensity (0-1)</summary>
        public float Intensity;

        /// <summary>Duration in seconds</summary>
        public float Duration;

        /// <summary>Vibration frequency in Hz (if supported)</summary>
        public float Frequency;

        public MmInputHapticMessage() : base((MmMethod)MmInputMethod.Haptic, (MmMessageType)MmInputMessageType.InputHaptic)
        {
        }

        public MmInputHapticMessage(MmHandedness hand, float intensity, float duration,
            float frequency = 0f, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Haptic, (MmMessageType)MmInputMessageType.InputHaptic, metadataBlock)
        {
            Hand = hand;
            Intensity = Mathf.Clamp01(intensity);
            Duration = duration;
            Frequency = frequency;
        }

        public MmInputHapticMessage(MmInputHapticMessage message) : base(message)
        {
            Hand = message.Hand;
            Intensity = message.Intensity;
            Duration = message.Duration;
            Frequency = message.Frequency;
        }

        public override MmMessage Copy() => new MmInputHapticMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            Intensity = (float)data[index++];
            Duration = (float)data[index++];
            Frequency = (float)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] { (int)Hand, Intensity, Duration, Frequency }).ToArray();
        }
    }

    #endregion

    #region Button Message

    /// <summary>
    /// Button input message.
    /// Sent when a controller button is pressed/released.
    /// </summary>
    public class MmInputButtonMessage : MmMessage
    {
        /// <summary>Which controller</summary>
        public MmHandedness Hand;

        /// <summary>Button identifier (platform-specific)</summary>
        public int ButtonId;

        /// <summary>Button name (e.g., "Trigger", "Grip", "A", "B")</summary>
        public string ButtonName;

        /// <summary>Current button state</summary>
        public MmButtonState State;

        /// <summary>Analog value for analog buttons (0-1)</summary>
        public float Value;

        public MmInputButtonMessage() : base((MmMethod)MmInputMethod.Button, (MmMessageType)MmInputMessageType.InputButton)
        {
            ButtonName = string.Empty;
        }

        public MmInputButtonMessage(MmHandedness hand, int buttonId, string buttonName,
            MmButtonState state, float value = 1f, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Button, (MmMessageType)MmInputMessageType.InputButton, metadataBlock)
        {
            Hand = hand;
            ButtonId = buttonId;
            ButtonName = buttonName ?? string.Empty;
            State = state;
            Value = value;
        }

        public MmInputButtonMessage(MmInputButtonMessage message) : base(message)
        {
            Hand = message.Hand;
            ButtonId = message.ButtonId;
            ButtonName = message.ButtonName;
            State = message.State;
            Value = message.Value;
        }

        public override MmMessage Copy() => new MmInputButtonMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            ButtonId = (int)data[index++];
            ButtonName = (string)data[index++];
            State = (MmButtonState)(int)data[index++];
            Value = (float)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand, ButtonId, ButtonName ?? string.Empty, (int)State, Value
            }).ToArray();
        }
    }

    #endregion

    #region Axis Message

    /// <summary>
    /// Axis input message.
    /// Sent when an analog axis value changes (joystick, trigger, etc).
    /// </summary>
    public class MmInputAxisMessage : MmMessage
    {
        /// <summary>Which controller</summary>
        public MmHandedness Hand;

        /// <summary>Axis identifier</summary>
        public int AxisId;

        /// <summary>Axis name (e.g., "Joystick", "Trigger")</summary>
        public string AxisName;

        /// <summary>2D value for thumbsticks/touchpads</summary>
        public Vector2 Value2D;

        /// <summary>1D value for triggers/sliders</summary>
        public float Value1D;

        public MmInputAxisMessage() : base((MmMethod)MmInputMethod.Axis, (MmMessageType)MmInputMessageType.InputAxis)
        {
            AxisName = string.Empty;
        }

        public MmInputAxisMessage(MmHandedness hand, int axisId, string axisName,
            Vector2 value2D, float value1D = 0f, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Axis, (MmMessageType)MmInputMessageType.InputAxis, metadataBlock)
        {
            Hand = hand;
            AxisId = axisId;
            AxisName = axisName ?? string.Empty;
            Value2D = value2D;
            Value1D = value1D;
        }

        public MmInputAxisMessage(MmInputAxisMessage message) : base(message)
        {
            Hand = message.Hand;
            AxisId = message.AxisId;
            AxisName = message.AxisName;
            Value2D = message.Value2D;
            Value1D = message.Value1D;
        }

        public override MmMessage Copy() => new MmInputAxisMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            AxisId = (int)data[index++];
            AxisName = (string)data[index++];
            Value2D = new Vector2((float)data[index++], (float)data[index++]);
            Value1D = (float)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand, AxisId, AxisName ?? string.Empty, Value2D.x, Value2D.y, Value1D
            }).ToArray();
        }
    }

    #endregion

    #region Touch Message

    /// <summary>
    /// Touch surface input message.
    /// For controller touchpads or touch screens.
    /// </summary>
    public class MmInputTouchMessage : MmMessage
    {
        /// <summary>Which controller (or Unknown for screen touch)</summary>
        public MmHandedness Hand;

        /// <summary>Touch point ID for multi-touch</summary>
        public int TouchId;

        /// <summary>Position on touch surface (normalized 0-1 or pixels)</summary>
        public Vector2 Position;

        /// <summary>Touch delta since last frame</summary>
        public Vector2 Delta;

        /// <summary>Touch phase</summary>
        public MmTouchPhase Phase;

        public MmInputTouchMessage() : base((MmMethod)MmInputMethod.Touch, (MmMessageType)MmInputMessageType.InputTouch)
        {
        }

        public MmInputTouchMessage(MmHandedness hand, int touchId, Vector2 position,
            Vector2 delta, MmTouchPhase phase, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Touch, (MmMessageType)MmInputMessageType.InputTouch, metadataBlock)
        {
            Hand = hand;
            TouchId = touchId;
            Position = position;
            Delta = delta;
            Phase = phase;
        }

        public MmInputTouchMessage(MmInputTouchMessage message) : base(message)
        {
            Hand = message.Hand;
            TouchId = message.TouchId;
            Position = message.Position;
            Delta = message.Delta;
            Phase = message.Phase;
        }

        public override MmMessage Copy() => new MmInputTouchMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            TouchId = (int)data[index++];
            Position = new Vector2((float)data[index++], (float)data[index++]);
            Delta = new Vector2((float)data[index++], (float)data[index++]);
            Phase = (MmTouchPhase)(int)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand, TouchId, Position.x, Position.y, Delta.x, Delta.y, (int)Phase
            }).ToArray();
        }
    }

    #endregion

    #region Controller State Message

    /// <summary>
    /// Controller connection state message.
    /// Sent when a controller connects or disconnects.
    /// </summary>
    public class MmInputControllerStateMessage : MmMessage
    {
        /// <summary>Which controller</summary>
        public MmHandedness Hand;

        /// <summary>Whether the controller is connected</summary>
        public bool IsConnected;

        /// <summary>Controller type/name (e.g., "Quest Pro Controller")</summary>
        public string ControllerType;

        /// <summary>Battery level (0-1, or -1 if unknown)</summary>
        public float BatteryLevel;

        public MmInputControllerStateMessage()
            : base((MmMethod)MmInputMethod.ControllerState, (MmMessageType)MmInputMessageType.InputControllerState)
        {
            ControllerType = string.Empty;
            BatteryLevel = -1f;
        }

        public MmInputControllerStateMessage(MmHandedness hand, bool isConnected,
            string controllerType = null, float batteryLevel = -1f, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.ControllerState, (MmMessageType)MmInputMessageType.InputControllerState, metadataBlock)
        {
            Hand = hand;
            IsConnected = isConnected;
            ControllerType = controllerType ?? string.Empty;
            BatteryLevel = batteryLevel;
        }

        public MmInputControllerStateMessage(MmInputControllerStateMessage message) : base(message)
        {
            Hand = message.Hand;
            IsConnected = message.IsConnected;
            ControllerType = message.ControllerType;
            BatteryLevel = message.BatteryLevel;
        }

        public override MmMessage Copy() => new MmInputControllerStateMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Hand = (MmHandedness)(int)data[index++];
            IsConnected = (bool)data[index++];
            ControllerType = (string)data[index++];
            BatteryLevel = (float)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                (int)Hand, IsConnected, ControllerType ?? string.Empty, BatteryLevel
            }).ToArray();
        }
    }

    #endregion

    #region Gaze Message

    /// <summary>
    /// Eye tracking gaze message.
    /// Sent when gaze point is updated from eye tracking.
    /// </summary>
    public class MmInputGazeMessage : MmMessage
    {
        /// <summary>World-space gaze origin (eye position)</summary>
        public Vector3 Origin;

        /// <summary>World-space gaze direction</summary>
        public Vector3 Direction;

        /// <summary>World-space hit point (if something is being looked at)</summary>
        public Vector3 HitPoint;

        /// <summary>Whether the gaze is hitting something</summary>
        public bool IsHitting;

        /// <summary>Confidence of eye tracking (0-1)</summary>
        public float Confidence;

        public MmInputGazeMessage() : base((MmMethod)MmInputMethod.Gaze, (MmMessageType)MmInputMessageType.InputGaze)
        {
            Direction = Vector3.forward;
        }

        public MmInputGazeMessage(Vector3 origin, Vector3 direction, Vector3 hitPoint = default,
            bool isHitting = false, float confidence = 1f, MmMetadataBlock metadataBlock = null)
            : base((MmMethod)MmInputMethod.Gaze, (MmMessageType)MmInputMessageType.InputGaze, metadataBlock)
        {
            Origin = origin;
            Direction = direction;
            HitPoint = hitPoint;
            IsHitting = isHitting;
            Confidence = confidence;
        }

        public MmInputGazeMessage(MmInputGazeMessage message) : base(message)
        {
            Origin = message.Origin;
            Direction = message.Direction;
            HitPoint = message.HitPoint;
            IsHitting = message.IsHitting;
            Confidence = message.Confidence;
        }

        public override MmMessage Copy() => new MmInputGazeMessage(this);

        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Origin = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            Direction = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            HitPoint = new Vector3((float)data[index++], (float)data[index++], (float)data[index++]);
            IsHitting = (bool)data[index++];
            Confidence = (float)data[index++];
            return index;
        }

        public override object[] Serialize()
        {
            return base.Serialize().Concat(new object[] {
                Origin.x, Origin.y, Origin.z,
                Direction.x, Direction.y, Direction.z,
                HitPoint.x, HitPoint.y, HitPoint.z,
                IsHitting, Confidence
            }).ToArray();
        }
    }

    #endregion
}
