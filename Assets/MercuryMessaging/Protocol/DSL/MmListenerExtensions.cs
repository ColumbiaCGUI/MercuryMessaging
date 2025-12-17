// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.1: Listener Extensions for MmRelayNode
// Provides fluent API entry points for message subscription

using System;
using System.Runtime.CompilerServices;
using MercuryMessaging;

namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for subscribing to messages on MmRelayNode.
    /// Part of DSL Phase 2.1: Receiving Listener Pattern.
    /// </summary>
    public static class MmListenerExtensions
    {
        #region Generic Typed Listeners

        /// <summary>
        /// Creates a listener builder for messages of type T.
        /// Use this to subscribe to specific message types from external components.
        /// </summary>
        /// <typeparam name="T">The message type to listen for.</typeparam>
        /// <param name="relay">The relay node to listen on.</param>
        /// <returns>A builder for configuring the listener.</returns>
        /// <example>
        /// // Basic typed listener
        /// var subscription = relay.Listen&lt;MmMessageFloat&gt;()
        ///     .OnReceived(msg => brightness = msg.value)
        ///     .Execute();
        ///
        /// // Later, unsubscribe
        /// subscription.Dispose();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<T> Listen<T>(this MmRelayNode relay) where T : MmMessage
        {
            return new MmListenerBuilder<T>(relay, oneTime: false);
        }

        /// <summary>
        /// Creates a one-time listener builder that auto-disposes after receiving one message.
        /// Useful for waiting for a specific response or event.
        /// </summary>
        /// <typeparam name="T">The message type to listen for.</typeparam>
        /// <param name="relay">The relay node to listen on.</param>
        /// <returns>A builder for configuring the listener.</returns>
        /// <example>
        /// // Wait for a result message, then auto-unsubscribe
        /// relay.ListenOnce&lt;MmMessageString&gt;()
        ///     .OnReceived(msg => ProcessResult(msg.value))
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<T> ListenOnce<T>(this MmRelayNode relay) where T : MmMessage
        {
            return new MmListenerBuilder<T>(relay, oneTime: true);
        }

        #endregion

        #region Convenience Methods - Direct Subscription

        /// <summary>
        /// Quick subscription for float messages without builder pattern.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the float value.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// var sub = relay.OnFloat(value => slider.value = value);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageFloat> OnFloat(
            this MmRelayNode relay,
            Action<float> handler)
        {
            return relay.Listen<MmMessageFloat>()
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        /// <summary>
        /// Quick subscription for int messages without builder pattern.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the int value.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// var sub = relay.OnInt(value => score = value);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageInt> OnInt(
            this MmRelayNode relay,
            Action<int> handler)
        {
            return relay.Listen<MmMessageInt>()
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        /// <summary>
        /// Quick subscription for string messages without builder pattern.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the string value.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// var sub = relay.OnString(value => label.text = value);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageString> OnString(
            this MmRelayNode relay,
            Action<string> handler)
        {
            return relay.Listen<MmMessageString>()
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        /// <summary>
        /// Quick subscription for bool messages without builder pattern.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the bool value.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// var sub = relay.OnBool(value => gameObject.SetActive(value));
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageBool> OnBool(
            this MmRelayNode relay,
            Action<bool> handler)
        {
            return relay.Listen<MmMessageBool>()
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        /// <summary>
        /// Quick subscription for Vector3 messages without builder pattern.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the Vector3 value.</param>
        /// <returns>A subscription handle.</returns>
        /// <example>
        /// var sub = relay.OnVector3(value => transform.position = value);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageVector3> OnVector3(
            this MmRelayNode relay,
            Action<UnityEngine.Vector3> handler)
        {
            return relay.Listen<MmMessageVector3>()
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        #endregion

        #region Method-Based Listeners

        /// <summary>
        /// Creates a listener for a specific MmMethod.
        /// Listens for any message with the specified method type.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="method">The MmMethod to listen for.</param>
        /// <returns>A builder for configuring the listener.</returns>
        /// <example>
        /// relay.ListenFor(MmMethod.Initialize)
        ///     .OnReceived(msg => HandleInit())
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<MmMessage> ListenFor(this MmRelayNode relay, MmMethod method)
        {
            return relay.Listen<MmMessage>()
                .When(msg => msg.MmMethod == method);
        }

        /// <summary>
        /// Quick subscription for Initialize messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call when Initialize is received.</param>
        /// <returns>A subscription handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnInitialize(
            this MmRelayNode relay,
            Action handler)
        {
            return relay.ListenFor(MmMethod.Initialize)
                .OnReceived(_ => handler())
                .Execute();
        }

        /// <summary>
        /// Quick subscription for SetActive messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the bool value.</param>
        /// <returns>A subscription handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageBool> OnSetActive(
            this MmRelayNode relay,
            Action<bool> handler)
        {
            return relay.Listen<MmMessageBool>()
                .When(msg => msg.MmMethod == MmMethod.SetActive)
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        /// <summary>
        /// Quick subscription for Refresh messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call when Refresh is received.</param>
        /// <returns>A subscription handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnRefresh(
            this MmRelayNode relay,
            Action handler)
        {
            return relay.ListenFor(MmMethod.Refresh)
                .OnReceived(_ => handler())
                .Execute();
        }

        /// <summary>
        /// Quick subscription for Complete messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call when Complete is received.</param>
        /// <returns>A subscription handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnComplete(
            this MmRelayNode relay,
            Action handler)
        {
            return relay.ListenFor(MmMethod.Complete)
                .OnReceived(_ => handler())
                .Execute();
        }

        /// <summary>
        /// Quick subscription for Switch messages.
        /// </summary>
        /// <param name="relay">The relay node to listen on.</param>
        /// <param name="handler">The handler to call with the state name.</param>
        /// <returns>A subscription handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageString> OnSwitch(
            this MmRelayNode relay,
            Action<string> handler)
        {
            return relay.Listen<MmMessageString>()
                .When(msg => msg.MmMethod == MmMethod.Switch)
                .OnReceived(msg => handler(msg.value))
                .Execute();
        }

        #endregion

        #region Cleanup Helpers

        /// <summary>
        /// Removes all listeners from this relay node.
        /// Use with caution - this will stop all external subscribers.
        /// </summary>
        /// <param name="relay">The relay node to clear listeners from.</param>
        public static void ClearAllListeners(this MmRelayNode relay)
        {
            relay?.ClearListeners();
        }

        /// <summary>
        /// Gets the number of active listeners on this relay node.
        /// Useful for debugging and testing.
        /// </summary>
        /// <param name="relay">The relay node to query.</param>
        /// <returns>The number of active listeners.</returns>
        public static int GetListenerCount(this MmRelayNode relay)
        {
            return relay?.ListenerCount ?? 0;
        }

        #endregion

        // ============================================================================
        // RESPONDER EXTENSIONS
        // Following the established pattern from MmMessagingExtensions.cs
        // All responder methods delegate to relay node with null-safety
        // ============================================================================

        #region Generic Typed Listeners - Responder

        /// <summary>
        /// Creates a listener builder for messages of type T (from responder).
        /// Null-safe: returns default builder if responder has no relay node.
        /// </summary>
        /// <typeparam name="T">The message type to listen for.</typeparam>
        /// <param name="responder">The responder to listen from.</param>
        /// <returns>A builder for configuring the listener.</returns>
        /// <example>
        /// // From a responder - same API as relay node!
        /// var subscription = myResponder.Listen&lt;MmMessageFloat&gt;()
        ///     .OnReceived(msg => brightness = msg.value)
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<T> Listen<T>(this MmBaseResponder responder) where T : MmMessage
        {
            var relay = responder.GetRelayNode();
            return relay != null ? new MmListenerBuilder<T>(relay, oneTime: false) : default;
        }

        /// <summary>
        /// Creates a one-time listener builder (from responder).
        /// Null-safe: returns default builder if responder has no relay node.
        /// </summary>
        /// <typeparam name="T">The message type to listen for.</typeparam>
        /// <param name="responder">The responder to listen from.</param>
        /// <returns>A builder for configuring the listener.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<T> ListenOnce<T>(this MmBaseResponder responder) where T : MmMessage
        {
            var relay = responder.GetRelayNode();
            return relay != null ? new MmListenerBuilder<T>(relay, oneTime: true) : default;
        }

        #endregion

        #region Convenience Methods - Responder

        /// <summary>Quick subscription for float messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageFloat> OnFloat(
            this MmBaseResponder responder,
            Action<float> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnFloat(handler);
        }

        /// <summary>Quick subscription for int messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageInt> OnInt(
            this MmBaseResponder responder,
            Action<int> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnInt(handler);
        }

        /// <summary>Quick subscription for string messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageString> OnString(
            this MmBaseResponder responder,
            Action<string> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnString(handler);
        }

        /// <summary>Quick subscription for bool messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageBool> OnBool(
            this MmBaseResponder responder,
            Action<bool> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnBool(handler);
        }

        /// <summary>Quick subscription for Vector3 messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageVector3> OnVector3(
            this MmBaseResponder responder,
            Action<UnityEngine.Vector3> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnVector3(handler);
        }

        #endregion

        #region Method-Based Listeners - Responder

        /// <summary>Creates a listener for a specific MmMethod (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerBuilder<MmMessage> ListenFor(this MmBaseResponder responder, MmMethod method)
        {
            var relay = responder.GetRelayNode();
            return relay != null ? relay.ListenFor(method) : default;
        }

        /// <summary>Quick subscription for Initialize messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnInitialize(
            this MmBaseResponder responder,
            Action handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnInitialize(handler);
        }

        /// <summary>Quick subscription for SetActive messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageBool> OnSetActive(
            this MmBaseResponder responder,
            Action<bool> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnSetActive(handler);
        }

        /// <summary>Quick subscription for Refresh messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnRefresh(
            this MmBaseResponder responder,
            Action handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnRefresh(handler);
        }

        /// <summary>Quick subscription for Complete messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessage> OnComplete(
            this MmBaseResponder responder,
            Action handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnComplete(handler);
        }

        /// <summary>Quick subscription for Switch messages (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmListenerSubscription<MmMessageString> OnSwitch(
            this MmBaseResponder responder,
            Action<string> handler)
        {
            var relay = responder.GetRelayNode();
            return relay?.OnSwitch(handler);
        }

        #endregion

        #region Cleanup Helpers - Responder

        /// <summary>Removes all listeners from this responder's relay node.</summary>
        public static void ClearAllListeners(this MmBaseResponder responder)
        {
            responder.GetRelayNode()?.ClearListeners();
        }

        /// <summary>Gets the number of active listeners on this responder's relay node.</summary>
        public static int GetListenerCount(this MmBaseResponder responder)
        {
            return responder.GetRelayNode()?.ListenerCount ?? 0;
        }

        #endregion
    }
}
