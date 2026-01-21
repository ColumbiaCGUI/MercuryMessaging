// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmInputResponder.cs - Base Responder for Standard Library Input Messages
// Part of DSL Overhaul Phase 10
//
// Handles Input messages (MmMethod 200-299) with virtual methods for each type.
// Extend this class to receive VR/XR input events in a type-safe manner.

using UnityEngine;

namespace MercuryMessaging.StandardLibrary.Input
{
    /// <summary>
    /// Base responder for handling Standard Library Input messages.
    /// Extends MmBaseResponder with type-safe handling for VR/XR input events.
    ///
    /// Input messages use MmMethod values 200-299, which are routed through
    /// MmInvoke and dispatched to virtual handler methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Usage:</b> Extend this class and override the relevant virtual methods
    /// to handle input events. All handlers are virtual and do nothing by default.
    /// </para>
    /// <para>
    /// <b>Integration:</b> Works with Unity XR Toolkit, Oculus SDK, and other
    /// VR/XR platforms. Send input messages from your input system adapters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyInputHandler : MmInputResponder
    /// {
    ///     protected override void Received6DOF(MmInput6DOFMessage msg)
    ///     {
    ///         if (msg.Hand == MmHandedness.Right)
    ///             rightHandTransform.SetPositionAndRotation(msg.Position, msg.Rotation);
    ///     }
    ///
    ///     protected override void ReceivedGesture(MmInputGestureMessage msg)
    ///     {
    ///         if (msg.GestureType == MmGestureType.Pinch && msg.Confidence > 0.9f)
    ///             SelectObject();
    ///     }
    ///
    ///     protected override void ReceivedHaptic(MmInputHapticMessage msg)
    ///     {
    ///         // Apply haptic feedback to controller
    ///         HapticController.Vibrate(msg.Hand, msg.Intensity, msg.Duration);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class MmInputResponder : MmBaseResponder
    {
        #region MmInvoke Override

        /// <summary>
        /// Routes messages to appropriate handlers.
        /// Input messages (200-299) are handled by this class;
        /// all others are passed to base MmBaseResponder.
        /// </summary>
        /// <param name="message">The message to process</param>
        public override void MmInvoke(MmMessage message)
        {
            int methodValue = (int)message.MmMethod;

            // Input messages are in range 200-299
            if (methodValue >= 200 && methodValue < 300)
            {
                DispatchInputMessage(message);
                return;
            }

            // All other messages go to base class
            base.MmInvoke(message);
        }

        /// <summary>
        /// Dispatches Input messages to the appropriate virtual handler.
        /// </summary>
        private void DispatchInputMessage(MmMessage message)
        {
            switch ((MmInputMethod)(int)message.MmMethod)
            {
                case MmInputMethod.Input6DOF:
                    Received6DOF((MmInput6DOFMessage)message);
                    break;

                case MmInputMethod.Gesture:
                    ReceivedGesture((MmInputGestureMessage)message);
                    break;

                case MmInputMethod.Haptic:
                    ReceivedHaptic((MmInputHapticMessage)message);
                    break;

                case MmInputMethod.Button:
                    ReceivedButton((MmInputButtonMessage)message);
                    break;

                case MmInputMethod.Axis:
                    ReceivedAxis((MmInputAxisMessage)message);
                    break;

                case MmInputMethod.Touch:
                    ReceivedTouch((MmInputTouchMessage)message);
                    break;

                case MmInputMethod.ControllerState:
                    ReceivedControllerState((MmInputControllerStateMessage)message);
                    break;

                case MmInputMethod.Gaze:
                    ReceivedGaze((MmInputGazeMessage)message);
                    break;

                default:
                    OnUnhandledInputMethod(message);
                    break;
            }
        }

        #endregion

        #region Virtual Handler Methods

        /// <summary>
        /// Called when a 6DOF tracking message is received.
        /// Override to handle controller position/rotation updates.
        /// </summary>
        /// <param name="message">The 6DOF message with position, rotation, and velocity</param>
        protected virtual void Received6DOF(MmInput6DOFMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a gesture recognition message is received.
        /// Override to handle hand gesture events.
        /// </summary>
        /// <param name="message">The gesture message with type, confidence, and progress</param>
        protected virtual void ReceivedGesture(MmInputGestureMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a haptic feedback request is received.
        /// Override to trigger controller vibration.
        /// </summary>
        /// <param name="message">The haptic message with intensity and duration</param>
        protected virtual void ReceivedHaptic(MmInputHapticMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a button input message is received.
        /// Override to handle controller button presses.
        /// </summary>
        /// <param name="message">The button message with state and value</param>
        protected virtual void ReceivedButton(MmInputButtonMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when an axis input message is received.
        /// Override to handle joystick/trigger input.
        /// </summary>
        /// <param name="message">The axis message with 1D and 2D values</param>
        protected virtual void ReceivedAxis(MmInputAxisMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a touch surface message is received.
        /// Override to handle touchpad input.
        /// </summary>
        /// <param name="message">The touch message with position and phase</param>
        protected virtual void ReceivedTouch(MmInputTouchMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a controller state change message is received.
        /// Override to handle controller connect/disconnect events.
        /// </summary>
        /// <param name="message">The controller state message</param>
        protected virtual void ReceivedControllerState(MmInputControllerStateMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a gaze tracking message is received.
        /// Override to handle eye tracking input.
        /// </summary>
        /// <param name="message">The gaze message with origin, direction, and hit point</param>
        protected virtual void ReceivedGaze(MmInputGazeMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when an unrecognized Input method is received.
        /// Override to customize handling of unknown Input methods.
        /// </summary>
        /// <param name="message">The unhandled message</param>
        protected virtual void OnUnhandledInputMethod(MmMessage message)
        {
            MmLogger.LogFramework(
                $"[{GetType().Name}] Unhandled Input method: {message.MmMethod} ({(int)message.MmMethod}) on {gameObject.name}");
        }

        #endregion
    }
}
