// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.1: Listener Pattern for Message Reception
// Provides fluent API for subscribing to messages from external components

using System;
using System.Runtime.CompilerServices;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Represents an active message listener subscription.
    /// Implements IDisposable to allow clean unsubscription.
    /// </summary>
    /// <typeparam name="T">The message type to listen for (e.g., MmMessageFloat, MmMessageString)</typeparam>
    public class MmListenerSubscription<T> : IDisposable where T : MmMessage
    {
        private readonly MmRelayNode _relay;
        private readonly Action<T> _handler;
        private readonly Func<T, bool> _filter;
        private readonly MmTag _tagFilter;
        private readonly bool _oneTime;
        private bool _disposed;

        internal MmListenerSubscription(
            MmRelayNode relay,
            Action<T> handler,
            Func<T, bool> filter,
            MmTag tagFilter,
            bool oneTime)
        {
            _relay = relay;
            _handler = handler;
            _filter = filter;
            _tagFilter = tagFilter;
            _oneTime = oneTime;
            _disposed = false;
        }

        /// <summary>
        /// The relay node this listener is attached to.
        /// </summary>
        public MmRelayNode Relay => _relay;

        /// <summary>
        /// Whether this listener has been disposed.
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// Whether this is a one-time listener (auto-disposes after first message).
        /// </summary>
        public bool IsOneTime => _oneTime;

        /// <summary>
        /// Called by MmRelayNode when a message is received.
        /// Returns true if the listener should remain active, false if it should be removed.
        /// </summary>
        internal bool TryInvoke(MmMessage message)
        {
            if (_disposed)
                return false;

            // Type check
            if (!(message is T typedMessage))
                return true; // Keep listening, just not our type

            // Tag filter check
            if (_tagFilter != MmTagHelper.Everything)
            {
                if ((message.MetadataBlock.Tag & _tagFilter) == 0)
                    return true; // Keep listening, tag doesn't match
            }

            // Custom filter check
            if (_filter != null && !_filter(typedMessage))
                return true; // Keep listening, filter didn't match

            // Invoke the handler
            try
            {
                _handler?.Invoke(typedMessage);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[MmListener] Exception in handler: {ex}");
            }

            // For one-time listeners, mark as disposed after handling
            if (_oneTime)
            {
                _disposed = true;
                return false; // Remove from registry
            }

            return true; // Keep listening
        }

        /// <summary>
        /// Unsubscribes this listener from the relay node.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _relay?.RemoveListener(this);
        }
    }

    /// <summary>
    /// Non-generic base interface for listener subscriptions.
    /// Used by MmRelayNode to store heterogeneous listeners.
    /// </summary>
    public interface IMmListenerSubscription : IDisposable
    {
        /// <summary>
        /// The relay node this listener is attached to.
        /// </summary>
        MmRelayNode Relay { get; }

        /// <summary>
        /// Whether this listener has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Try to invoke this listener with the given message.
        /// Returns true if the listener should remain active.
        /// </summary>
        bool TryInvoke(MmMessage message);
    }

    /// <summary>
    /// Generic listener subscription that implements the interface.
    /// </summary>
    internal class MmListenerSubscriptionImpl<T> : IMmListenerSubscription where T : MmMessage
    {
        private readonly MmListenerSubscription<T> _inner;

        internal MmListenerSubscriptionImpl(MmListenerSubscription<T> inner)
        {
            _inner = inner;
        }

        public MmRelayNode Relay => _inner.Relay;
        public bool IsDisposed => _inner.IsDisposed;

        public bool TryInvoke(MmMessage message)
        {
            return _inner.TryInvoke(message);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        internal MmListenerSubscription<T> GetInner() => _inner;
    }

    /// <summary>
    /// Fluent builder for creating message listeners.
    /// Supports filtering by message type, tags, and custom predicates.
    /// </summary>
    /// <typeparam name="T">The message type to listen for</typeparam>
    public struct MmListenerBuilder<T> where T : MmMessage
    {
        private readonly MmRelayNode _relay;
        private Action<T> _handler;
        private Func<T, bool> _filter;
        private MmTag _tagFilter;
        private bool _oneTime;

        /// <summary>
        /// Creates a new listener builder for the specified relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MmListenerBuilder(MmRelayNode relay, bool oneTime = false)
        {
            _relay = relay;
            _handler = null;
            _filter = null;
            _tagFilter = MmTagHelper.Everything;
            _oneTime = oneTime;
        }

        /// <summary>
        /// Sets the handler to be called when a matching message is received.
        /// </summary>
        /// <param name="handler">The action to invoke with the received message.</param>
        /// <example>
        /// relay.Listen&lt;MmMessageFloat&gt;()
        ///     .OnReceived(msg => brightness = msg.value)
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmListenerBuilder<T> OnReceived(Action<T> handler)
        {
            _handler = handler;
            return this;
        }

        /// <summary>
        /// Adds a filter predicate. Only messages passing the filter will trigger the handler.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <example>
        /// relay.Listen&lt;MmMessageInt&gt;()
        ///     .When(msg => msg.value > 50)
        ///     .OnReceived(msg => StartAlert(msg.value))
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmListenerBuilder<T> When(Func<T, bool> predicate)
        {
            if (_filter == null)
            {
                _filter = predicate;
            }
            else
            {
                // Combine filters with AND logic
                var existingFilter = _filter;
                _filter = msg => existingFilter(msg) && predicate(msg);
            }
            return this;
        }

        /// <summary>
        /// Filters messages by MercuryMessaging tag.
        /// Only messages with matching tag bits will trigger the handler.
        /// </summary>
        /// <param name="tag">The tag to filter by.</param>
        /// <example>
        /// relay.Listen&lt;MmMessageString&gt;()
        ///     .WithTag(MmTag.Tag0)
        ///     .OnReceived(msg => HandleTaggedMessage(msg))
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmListenerBuilder<T> WithTag(MmTag tag)
        {
            _tagFilter = tag;
            return this;
        }

        /// <summary>
        /// Marks this as a one-time listener that auto-disposes after receiving one message.
        /// </summary>
        /// <example>
        /// relay.Listen&lt;MmMessageString&gt;()
        ///     .Once()
        ///     .OnReceived(msg => ProcessResult(msg))
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmListenerBuilder<T> Once()
        {
            _oneTime = true;
            return this;
        }

        /// <summary>
        /// Finalizes the builder and registers the listener with the relay node.
        /// Returns a subscription that can be disposed to unsubscribe.
        /// </summary>
        /// <returns>A subscription handle that can be disposed to unsubscribe.</returns>
        public MmListenerSubscription<T> Execute()
        {
            if (_relay == null)
            {
                UnityEngine.Debug.LogError("[MmListener] Cannot create listener without a relay node");
                return null;
            }

            if (_handler == null)
            {
                UnityEngine.Debug.LogError("[MmListener] Cannot create listener without a handler. Call OnReceived() first.");
                return null;
            }

            var subscription = new MmListenerSubscription<T>(_relay, _handler, _filter, _tagFilter, _oneTime);
            _relay.AddListener(subscription);
            return subscription;
        }
    }
}
