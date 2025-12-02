// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmUIResponder.cs - Base Responder for Standard Library UI Messages
// Part of DSL Overhaul Phase 9
//
// Handles UI messages (MmMethod 100-199) with virtual methods for each type.
// Extend this class to receive UI events in a type-safe manner.

using UnityEngine;

namespace MercuryMessaging.StandardLibrary.UI
{
    /// <summary>
    /// Base responder for handling Standard Library UI messages.
    /// Extends MmBaseResponder with type-safe handling for UI events.
    ///
    /// UI messages use MmMethod values 100-199, which are routed through
    /// MmInvoke and dispatched to virtual handler methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Usage:</b> Extend this class and override the relevant virtual methods
    /// to handle UI events. All handlers are virtual and do nothing by default.
    /// </para>
    /// <para>
    /// <b>Performance:</b> UI method dispatch uses a switch statement for efficiency,
    /// similar to standard MmBaseResponder message handling.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyUIHandler : MmUIResponder
    /// {
    ///     protected override void ReceivedClick(MmUIClickMessage msg)
    ///     {
    ///         Debug.Log($"Click at {msg.Position}");
    ///         if (msg.IsDoubleClick) OpenItem();
    ///     }
    ///
    ///     protected override void ReceivedHover(MmUIHoverMessage msg)
    ///     {
    ///         if (msg.IsEnter) ShowTooltip();
    ///         else HideTooltip();
    ///     }
    ///
    ///     protected override void ReceivedDrag(MmUIDragMessage msg)
    ///     {
    ///         if (msg.Phase == MmDragPhase.Move)
    ///             transform.position += (Vector3)msg.Delta;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class MmUIResponder : MmBaseResponder
    {
        #region MmInvoke Override

        /// <summary>
        /// Routes messages to appropriate handlers.
        /// UI messages (100-199) are handled by this class;
        /// all others are passed to base MmBaseResponder.
        /// </summary>
        /// <param name="message">The message to process</param>
        public override void MmInvoke(MmMessage message)
        {
            int methodValue = (int)message.MmMethod;

            // UI messages are in range 100-199
            if (methodValue >= 100 && methodValue < 200)
            {
                DispatchUIMessage(message);
                return;
            }

            // All other messages go to base class
            base.MmInvoke(message);
        }

        /// <summary>
        /// Dispatches UI messages to the appropriate virtual handler.
        /// </summary>
        private void DispatchUIMessage(MmMessage message)
        {
            switch ((MmUIMethod)(int)message.MmMethod)
            {
                case MmUIMethod.Click:
                    ReceivedClick((MmUIClickMessage)message);
                    break;

                case MmUIMethod.Hover:
                    ReceivedHover((MmUIHoverMessage)message);
                    break;

                case MmUIMethod.Drag:
                    ReceivedDrag((MmUIDragMessage)message);
                    break;

                case MmUIMethod.Scroll:
                    ReceivedScroll((MmUIScrollMessage)message);
                    break;

                case MmUIMethod.Focus:
                    ReceivedFocus((MmUIFocusMessage)message);
                    break;

                case MmUIMethod.Select:
                    ReceivedSelect((MmUISelectMessage)message);
                    break;

                case MmUIMethod.Submit:
                    ReceivedSubmit((MmUISubmitMessage)message);
                    break;

                case MmUIMethod.Cancel:
                    ReceivedCancel((MmUICancelMessage)message);
                    break;

                default:
                    OnUnhandledUIMethod(message);
                    break;
            }
        }

        #endregion

        #region Virtual Handler Methods

        /// <summary>
        /// Called when a click message is received.
        /// Override to handle click/tap events.
        /// </summary>
        /// <param name="message">The click message with position, click count, and button info</param>
        protected virtual void ReceivedClick(MmUIClickMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a hover message is received.
        /// Override to handle pointer enter/exit events.
        /// </summary>
        /// <param name="message">The hover message with position and enter/exit state</param>
        protected virtual void ReceivedHover(MmUIHoverMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a drag message is received.
        /// Override to handle drag begin/move/end events.
        /// </summary>
        /// <param name="message">The drag message with position, delta, and phase</param>
        protected virtual void ReceivedDrag(MmUIDragMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a scroll message is received.
        /// Override to handle scroll wheel or touch scroll events.
        /// </summary>
        /// <param name="message">The scroll message with position and scroll delta</param>
        protected virtual void ReceivedScroll(MmUIScrollMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a focus message is received.
        /// Override to handle focus gained/lost events.
        /// </summary>
        /// <param name="message">The focus message with focus state and element ID</param>
        protected virtual void ReceivedFocus(MmUIFocusMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a select message is received.
        /// Override to handle selection change events.
        /// </summary>
        /// <param name="message">The select message with selected index and value</param>
        protected virtual void ReceivedSelect(MmUISelectMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a submit message is received.
        /// Override to handle form submission or confirmation events.
        /// </summary>
        /// <param name="message">The submit message with optional data</param>
        protected virtual void ReceivedSubmit(MmUISubmitMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when a cancel message is received.
        /// Override to handle cancellation events.
        /// </summary>
        /// <param name="message">The cancel message with optional reason</param>
        protected virtual void ReceivedCancel(MmUICancelMessage message)
        {
            // Override in derived class
        }

        /// <summary>
        /// Called when an unrecognized UI method is received.
        /// Override to customize handling of unknown UI methods.
        /// </summary>
        /// <param name="message">The unhandled message</param>
        protected virtual void OnUnhandledUIMethod(MmMessage message)
        {
            MmLogger.LogFramework(
                $"[{GetType().Name}] Unhandled UI method: {message.MmMethod} ({(int)message.MmMethod}) on {gameObject.name}");
        }

        #endregion
    }
}
