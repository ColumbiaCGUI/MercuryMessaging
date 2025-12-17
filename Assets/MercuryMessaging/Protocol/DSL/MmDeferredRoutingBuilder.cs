// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmDeferredRoutingBuilder.cs - Deferred routing builder for conditional execution
// Part of DSL Phase 2: Builder API for Advanced Cases

// Suppress MM015: Filter equality checks are intentional for exact match routing logic
#pragma warning disable MM015

using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Deferred routing builder that requires explicit Execute() to send messages.
    /// Unlike MmRoutingBuilder which auto-executes, this builder stores the message
    /// configuration and only sends when Execute() is called.
    ///
    /// Use cases:
    /// - Conditional execution: if (ready) builder.Execute();
    /// - Storing message config for later reuse
    /// - Building message in stages before sending
    ///
    /// Example usage:
    /// <code>
    /// // Deferred execution (requires .Execute())
    /// var builder = relay.Build().ToChildren().Send("Hello");
    /// if (condition)
    ///     builder.Execute();
    ///
    /// // Compare with auto-execute (relay.To)
    /// relay.To.Children.Send("Hello"); // Sends immediately
    /// </code>
    /// </summary>
    public struct MmDeferredRoutingBuilder
    {
        private readonly MmRelayNode _relay;
        private MmLevelFilter _levelFilter;
        private MmActiveFilter _activeFilter;
        private MmSelectedFilter _selectedFilter;
        private MmNetworkFilter _networkFilter;
        private MmTag _tag;

        // Deferred payload storage
        private MmMethod _method;
        private object _payload;
        private bool _hasPayload;

        /// <summary>
        /// Creates a new deferred routing builder for the specified relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder(MmRelayNode relay)
        {
            _relay = relay;
            _levelFilter = MmLevelFilterHelper.SelfAndChildren;
            _activeFilter = MmActiveFilter.All;
            _selectedFilter = MmSelectedFilter.All;
            _networkFilter = MmNetworkFilter.Local;
            _tag = MmTagHelper.Everything;
            _method = MmMethod.NoOp;
            _payload = null;
            _hasPayload = false;
        }

        #region Direction Methods (Chainable)

        /// <summary>Target direct children only.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToChildren()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilter.Child;
            return builder;
        }

        /// <summary>Target direct parents only.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToParents()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilter.Parent;
            return builder;
        }

        /// <summary>Target self and all descendants (recursive children).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToDescendants()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilterHelper.SelfAndDescendants;
            return builder;
        }

        /// <summary>Target self and all ancestors (recursive parents).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToAncestors()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilterHelper.SelfAndAncestors;
            return builder;
        }

        /// <summary>Target siblings (same-parent nodes).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToSiblings()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilterHelper.SelfAndSiblings;
            return builder;
        }

        /// <summary>Target self and children (default routing).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToSelfAndChildren()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilterHelper.SelfAndChildren;
            return builder;
        }

        /// <summary>Target all connected nodes (bidirectional).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder ToAll()
        {
            var builder = this;
            builder._levelFilter = MmLevelFilterHelper.SelfAndBidirectional;
            return builder;
        }

        #endregion

        #region Filter Methods (Chainable)

        /// <summary>Only target active GameObjects.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Active()
        {
            var builder = this;
            builder._activeFilter = MmActiveFilter.Active;
            return builder;
        }

        /// <summary>Include inactive GameObjects.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder IncludeInactive()
        {
            var builder = this;
            builder._activeFilter = MmActiveFilter.All;
            return builder;
        }

        /// <summary>Only target selected responders (FSM state).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Selected()
        {
            var builder = this;
            builder._selectedFilter = MmSelectedFilter.Selected;
            return builder;
        }

        /// <summary>Send over network as well as locally.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder OverNetwork()
        {
            var builder = this;
            builder._networkFilter = MmNetworkFilter.All;
            return builder;
        }

        /// <summary>Send locally only (default).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder LocalOnly()
        {
            var builder = this;
            builder._networkFilter = MmNetworkFilter.Local;
            return builder;
        }

        /// <summary>Filter by tag.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder WithTag(MmTag tag)
        {
            var builder = this;
            builder._tag = tag;
            return builder;
        }

        #endregion

        #region Send Methods (Chainable - Does NOT Execute)

        /// <summary>Set string payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(string value)
        {
            var builder = this;
            builder._method = MmMethod.MessageString;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set int payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(int value)
        {
            var builder = this;
            builder._method = MmMethod.MessageInt;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set float payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(float value)
        {
            var builder = this;
            builder._method = MmMethod.MessageFloat;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set bool payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(bool value)
        {
            var builder = this;
            builder._method = MmMethod.MessageBool;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Vector3 payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(Vector3 value)
        {
            var builder = this;
            builder._method = MmMethod.MessageVector3;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Vector4 payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(Vector4 value)
        {
            var builder = this;
            builder._method = MmMethod.MessageVector4;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Quaternion payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(Quaternion value)
        {
            var builder = this;
            builder._method = MmMethod.MessageQuaternion;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Transform payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(Transform value)
        {
            var builder = this;
            builder._method = MmMethod.MessageTransform;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set GameObject payload. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Send(GameObject value)
        {
            var builder = this;
            builder._method = MmMethod.MessageGameObject;
            builder._payload = value;
            builder._hasPayload = true;
            return builder;
        }

        #endregion

        #region Command Methods (Chainable - Does NOT Execute)

        /// <summary>Set Initialize command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Initialize()
        {
            var builder = this;
            builder._method = MmMethod.Initialize;
            builder._payload = null;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Refresh command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Refresh()
        {
            var builder = this;
            builder._method = MmMethod.Refresh;
            builder._payload = null;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Complete command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Complete()
        {
            var builder = this;
            builder._method = MmMethod.Complete;
            builder._payload = true;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set SetActive command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder SetActive(bool active)
        {
            var builder = this;
            builder._method = MmMethod.SetActive;
            builder._payload = active;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Switch command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Switch(int stateIndex)
        {
            var builder = this;
            builder._method = MmMethod.Switch;
            builder._payload = stateIndex;
            builder._hasPayload = true;
            return builder;
        }

        /// <summary>Set Switch command. Does NOT execute - call Execute() to send.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmDeferredRoutingBuilder Switch(string stateName)
        {
            var builder = this;
            builder._method = MmMethod.Switch;
            builder._payload = stateName;
            builder._hasPayload = true;
            return builder;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Execute the deferred message. This is the terminal operation that sends the message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            // FAIL-FAST: Log error if no relay node instead of silently failing
            if (_relay == null)
            {
                UnityEngine.Debug.LogError("[MmDeferredRoutingBuilder] Execute called with no relay node. Message will not be sent.");
                return;
            }

            // Nothing to execute if no payload was set
            if (!_hasPayload)
            {
                UnityEngine.Debug.LogWarning("[MmDeferredRoutingBuilder] Execute called but no payload was set. Call Send() before Execute().");
                return;
            }

            // Delegate to MmFluentMessage for the actual routing logic
            var fluent = new MmFluentMessage(_relay, _method, _payload);
            fluent = fluent.To(_levelFilter);

            // Apply filters
            if (_activeFilter == MmActiveFilter.Active)
                fluent = fluent.Active();
            if (_selectedFilter == MmSelectedFilter.Selected)
                fluent = fluent.Selected();
            if (_networkFilter == MmNetworkFilter.All)
                fluent = fluent.OverNetwork();
            else if (_networkFilter == MmNetworkFilter.Network)
                fluent = fluent.NetworkOnly();
            if (_tag != MmTagHelper.Everything)
                fluent = fluent.WithTag(_tag);

            fluent.Execute();
        }

        #endregion
    }
}
