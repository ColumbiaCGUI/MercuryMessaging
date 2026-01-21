// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.4: Standard Library Listener Extensions
// Provides convenience methods for listening to UI and Input messages

using System;
using System.Runtime.CompilerServices;
using MercuryMessaging;
using MercuryMessaging.StandardLibrary.UI;
using MercuryMessaging.StandardLibrary.Input;

namespace MercuryMessaging.StandardLibrary
{
    /// <summary>
    /// Extension methods for subscribing to Standard Library messages.
    /// Part of DSL Phase 2.4: Standard Library Handler Shortcuts.
    /// </summary>
    /// <remarks>
    /// Provides convenience methods for listening to UI (100-199) and Input (200-299) messages
    /// without needing the full builder pattern.
    ///
    /// Example usage:
    /// <code>
    /// // UI message shortcuts
    /// relay.OnClick(msg => HandleClick(msg.Position));
    /// relay.OnHover(msg => ShowTooltip(msg.IsEnter));
    /// relay.OnDrag(msg => MoveObject(msg.Delta));
    ///
    /// // Input message shortcuts
    /// relay.On6DOF(msg => UpdateHand(msg.Position, msg.Rotation));
    /// relay.OnGesture(msg => RecognizeGesture(msg.GestureType));
    /// relay.OnButton(msg => HandleButton(msg.ButtonName, msg.State));
    /// </code>
    /// </remarks>
    public static class MmStandardLibraryListenerExtensions
    {
        // ============================================================================
        // UI MESSAGE LISTENERS (100-199)
        // ============================================================================

        #region UI Click

        /// <summary>
        /// Subscribes to UI click messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">Handler called with click message.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// relay.OnClick(msg => {
        ///     if (msg.IsDoubleClick) OpenItem();
        ///     if (msg.IsRightClick) ShowContextMenu();
        /// });
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIClickMessage> OnClick(
            this MmRelayNode relay,
            Action<MmUIClickMessage> handler)
        {
            return relay.Listen<MmUIClickMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI click messages with position-only handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIClickMessage> OnClick(
            this MmRelayNode relay,
            Action<UnityEngine.Vector2> handler)
        {
            return relay.Listen<MmUIClickMessage>()
                .OnReceived(msg => handler(msg.Position))
                .Execute();
        }

        #endregion

        #region UI Hover

        /// <summary>
        /// Subscribes to UI hover messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">Handler called with hover message.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// relay.OnHover(msg => {
        ///     if (msg.IsEnter) ShowTooltip();
        ///     else HideTooltip();
        /// });
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIHoverMessage> OnHover(
            this MmRelayNode relay,
            Action<MmUIHoverMessage> handler)
        {
            return relay.Listen<MmUIHoverMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI hover enter/exit with bool handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIHoverMessage> OnHover(
            this MmRelayNode relay,
            Action<bool> handler)
        {
            return relay.Listen<MmUIHoverMessage>()
                .OnReceived(msg => handler(msg.IsEnter))
                .Execute();
        }

        #endregion

        #region UI Drag

        /// <summary>
        /// Subscribes to UI drag messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">Handler called with drag message.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// relay.OnDrag(msg => {
        ///     if (msg.Phase == MmDragPhase.Move)
        ///         transform.position += (Vector3)msg.Delta;
        /// });
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIDragMessage> OnDrag(
            this MmRelayNode relay,
            Action<MmUIDragMessage> handler)
        {
            return relay.Listen<MmUIDragMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI drag with delta-only handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIDragMessage> OnDrag(
            this MmRelayNode relay,
            Action<UnityEngine.Vector2> handler)
        {
            return relay.Listen<MmUIDragMessage>()
                .OnReceived(msg => handler(msg.Delta))
                .Execute();
        }

        #endregion

        #region UI Scroll

        /// <summary>
        /// Subscribes to UI scroll messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIScrollMessage> OnScroll(
            this MmRelayNode relay,
            Action<MmUIScrollMessage> handler)
        {
            return relay.Listen<MmUIScrollMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI scroll with delta-only handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIScrollMessage> OnScroll(
            this MmRelayNode relay,
            Action<UnityEngine.Vector2> handler)
        {
            return relay.Listen<MmUIScrollMessage>()
                .OnReceived(msg => handler(msg.ScrollDelta))
                .Execute();
        }

        #endregion

        #region UI Focus

        /// <summary>
        /// Subscribes to UI focus messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIFocusMessage> OnFocus(
            this MmRelayNode relay,
            Action<MmUIFocusMessage> handler)
        {
            return relay.Listen<MmUIFocusMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI focus with bool handler (focused/unfocused).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIFocusMessage> OnFocus(
            this MmRelayNode relay,
            Action<bool> handler)
        {
            return relay.Listen<MmUIFocusMessage>()
                .OnReceived(msg => handler(msg.IsFocused))
                .Execute();
        }

        #endregion

        #region UI Select

        /// <summary>
        /// Subscribes to UI selection messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISelectMessage> OnSelect(
            this MmRelayNode relay,
            Action<MmUISelectMessage> handler)
        {
            return relay.Listen<MmUISelectMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI selection with index handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISelectMessage> OnSelect(
            this MmRelayNode relay,
            Action<int> handler)
        {
            return relay.Listen<MmUISelectMessage>()
                .OnReceived(msg => handler(msg.SelectedIndex))
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI selection with value handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISelectMessage> OnSelect(
            this MmRelayNode relay,
            Action<string> handler)
        {
            return relay.Listen<MmUISelectMessage>()
                .OnReceived(msg => handler(msg.SelectedValue))
                .Execute();
        }

        #endregion

        #region UI Submit/Cancel

        /// <summary>
        /// Subscribes to UI submit messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISubmitMessage> OnUISubmit(
            this MmRelayNode relay,
            Action<MmUISubmitMessage> handler)
        {
            return relay.Listen<MmUISubmitMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI submit with data handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISubmitMessage> OnUISubmit(
            this MmRelayNode relay,
            Action<string> handler)
        {
            return relay.Listen<MmUISubmitMessage>()
                .OnReceived(msg => handler(msg.Data))
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI cancel messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUICancelMessage> OnUICancel(
            this MmRelayNode relay,
            Action<MmUICancelMessage> handler)
        {
            return relay.Listen<MmUICancelMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to UI cancel with simple handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUICancelMessage> OnUICancel(
            this MmRelayNode relay,
            Action handler)
        {
            return relay.Listen<MmUICancelMessage>()
                .OnReceived(_ => handler())
                .Execute();
        }

        #endregion

        // ============================================================================
        // INPUT MESSAGE LISTENERS (200-299)
        // ============================================================================

        #region 6DOF Input

        /// <summary>
        /// Subscribes to 6DOF controller input messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">Handler called with 6DOF message.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// relay.On6DOF(msg => {
        ///     if (msg.Hand == MmHandedness.Right && msg.IsTracked)
        ///         rightHand.SetPositionAndRotation(msg.Position, msg.Rotation);
        /// });
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInput6DOFMessage> On6DOF(
            this MmRelayNode relay,
            Action<MmInput6DOFMessage> handler)
        {
            return relay.Listen<MmInput6DOFMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to 6DOF input for a specific hand.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInput6DOFMessage> On6DOF(
            this MmRelayNode relay,
            MmHandedness hand,
            Action<MmInput6DOFMessage> handler)
        {
            return relay.Listen<MmInput6DOFMessage>()
                .When(msg => msg.Hand == hand)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Gesture Input

        /// <summary>
        /// Subscribes to gesture recognition messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGestureMessage> OnGesture(
            this MmRelayNode relay,
            Action<MmInputGestureMessage> handler)
        {
            return relay.Listen<MmInputGestureMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to a specific gesture type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGestureMessage> OnGesture(
            this MmRelayNode relay,
            MmGestureType gestureType,
            Action<MmInputGestureMessage> handler)
        {
            return relay.Listen<MmInputGestureMessage>()
                .When(msg => msg.GestureType == gestureType)
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to gestures with minimum confidence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGestureMessage> OnGesture(
            this MmRelayNode relay,
            MmGestureType gestureType,
            float minConfidence,
            Action<MmInputGestureMessage> handler)
        {
            return relay.Listen<MmInputGestureMessage>()
                .When(msg => msg.GestureType == gestureType && msg.Confidence >= minConfidence)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Haptic Input

        /// <summary>
        /// Subscribes to haptic feedback messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputHapticMessage> OnHaptic(
            this MmRelayNode relay,
            Action<MmInputHapticMessage> handler)
        {
            return relay.Listen<MmInputHapticMessage>()
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Button Input

        /// <summary>
        /// Subscribes to button input messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputButtonMessage> OnButton(
            this MmRelayNode relay,
            Action<MmInputButtonMessage> handler)
        {
            return relay.Listen<MmInputButtonMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to a specific button by name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputButtonMessage> OnButton(
            this MmRelayNode relay,
            string buttonName,
            Action<MmInputButtonMessage> handler)
        {
            return relay.Listen<MmInputButtonMessage>()
                .When(msg => msg.ButtonName == buttonName)
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to button press events only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputButtonMessage> OnButtonPressed(
            this MmRelayNode relay,
            Action<MmInputButtonMessage> handler)
        {
            return relay.Listen<MmInputButtonMessage>()
                .When(msg => msg.State == MmButtonState.Pressed)
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to button release events only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputButtonMessage> OnButtonReleased(
            this MmRelayNode relay,
            Action<MmInputButtonMessage> handler)
        {
            return relay.Listen<MmInputButtonMessage>()
                .When(msg => msg.State == MmButtonState.Released)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Axis Input

        /// <summary>
        /// Subscribes to axis input messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputAxisMessage> OnAxis(
            this MmRelayNode relay,
            Action<MmInputAxisMessage> handler)
        {
            return relay.Listen<MmInputAxisMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to a specific axis by name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputAxisMessage> OnAxis(
            this MmRelayNode relay,
            string axisName,
            Action<MmInputAxisMessage> handler)
        {
            return relay.Listen<MmInputAxisMessage>()
                .When(msg => msg.AxisName == axisName)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Touch Input

        /// <summary>
        /// Subscribes to touch input messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputTouchMessage> OnTouch(
            this MmRelayNode relay,
            Action<MmInputTouchMessage> handler)
        {
            return relay.Listen<MmInputTouchMessage>()
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Controller State

        /// <summary>
        /// Subscribes to controller state messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputControllerStateMessage> OnControllerState(
            this MmRelayNode relay,
            Action<MmInputControllerStateMessage> handler)
        {
            return relay.Listen<MmInputControllerStateMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to controller connect events.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputControllerStateMessage> OnControllerConnected(
            this MmRelayNode relay,
            Action<MmInputControllerStateMessage> handler)
        {
            return relay.Listen<MmInputControllerStateMessage>()
                .When(msg => msg.IsConnected)
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to controller disconnect events.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputControllerStateMessage> OnControllerDisconnected(
            this MmRelayNode relay,
            Action<MmInputControllerStateMessage> handler)
        {
            return relay.Listen<MmInputControllerStateMessage>()
                .When(msg => !msg.IsConnected)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        #region Gaze Input

        /// <summary>
        /// Subscribes to gaze tracking messages.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGazeMessage> OnGaze(
            this MmRelayNode relay,
            Action<MmInputGazeMessage> handler)
        {
            return relay.Listen<MmInputGazeMessage>()
                .OnReceived(handler)
                .Execute();
        }

        /// <summary>
        /// Subscribes to gaze hits only (when looking at something).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGazeMessage> OnGazeHit(
            this MmRelayNode relay,
            Action<MmInputGazeMessage> handler)
        {
            return relay.Listen<MmInputGazeMessage>()
                .When(msg => msg.IsHitting)
                .OnReceived(handler)
                .Execute();
        }

        #endregion

        // ============================================================================
        // RESPONDER EXTENSIONS
        // All responder methods delegate to relay node with null-safety
        // ============================================================================

        #region UI Responder Extensions

        /// <summary>Subscribes to UI click messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIClickMessage> OnClick(
            this MmBaseResponder responder,
            Action<MmUIClickMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnClick(handler);
        }

        /// <summary>Subscribes to UI hover messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIHoverMessage> OnHover(
            this MmBaseResponder responder,
            Action<MmUIHoverMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnHover(handler);
        }

        /// <summary>Subscribes to UI drag messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIDragMessage> OnDrag(
            this MmBaseResponder responder,
            Action<MmUIDragMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnDrag(handler);
        }

        /// <summary>Subscribes to UI scroll messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIScrollMessage> OnScroll(
            this MmBaseResponder responder,
            Action<MmUIScrollMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnScroll(handler);
        }

        /// <summary>Subscribes to UI focus messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUIFocusMessage> OnFocus(
            this MmBaseResponder responder,
            Action<MmUIFocusMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnFocus(handler);
        }

        /// <summary>Subscribes to UI select messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmUISelectMessage> OnSelect(
            this MmBaseResponder responder,
            Action<MmUISelectMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnSelect(handler);
        }

        #endregion

        #region Input Responder Extensions

        /// <summary>Subscribes to 6DOF messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInput6DOFMessage> On6DOF(
            this MmBaseResponder responder,
            Action<MmInput6DOFMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.On6DOF(handler);
        }

        /// <summary>Subscribes to gesture messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGestureMessage> OnGesture(
            this MmBaseResponder responder,
            Action<MmInputGestureMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnGesture(handler);
        }

        /// <summary>Subscribes to button messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputButtonMessage> OnButton(
            this MmBaseResponder responder,
            Action<MmInputButtonMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnButton(handler);
        }

        /// <summary>Subscribes to axis messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputAxisMessage> OnAxis(
            this MmBaseResponder responder,
            Action<MmInputAxisMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnAxis(handler);
        }

        /// <summary>Subscribes to gaze messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmInputGazeMessage> OnGaze(
            this MmBaseResponder responder,
            Action<MmInputGazeMessage> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnGaze(handler);
        }

        #endregion
    }
}
