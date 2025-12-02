// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmRoutingBuilder.cs - Property-based routing builder for shorter DSL syntax
// Part of DSL Phase 1: Shorter Syntax

using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Property-based routing builder for shorter DSL syntax.
    /// Enables syntax like: relay.To.Children.Send("Hello")
    ///
    /// This struct provides a chainable API for setting routing targets
    /// before sending a message. Terminal methods (Send, Initialize, etc.)
    /// auto-execute immediately.
    ///
    /// Example usage:
    /// <code>
    /// // Property-based routing (new shorter syntax)
    /// relay.To.Children.Send("Hello");
    /// relay.To.Parents.Send(42);
    /// relay.To.Descendants.Active.Initialize();
    /// relay.To.Children.WithTag(MmTag.Tag0).Refresh();
    ///
    /// // Responders also work (null-safe)
    /// responder.To().Children.Send("Hello");
    /// </code>
    /// </summary>
    public struct MmRoutingBuilder
    {
        private readonly MmRelayNode _relay;
        private MmLevelFilter _levelFilter;
        private MmActiveFilter _activeFilter;
        private MmSelectedFilter _selectedFilter;
        private MmNetworkFilter _networkFilter;
        private MmTag _tag;

        /// <summary>
        /// Creates a new routing builder for the specified relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmRoutingBuilder(MmRelayNode relay)
        {
            _relay = relay;
            // Defaults - will be overridden by direction properties
            _levelFilter = MmLevelFilterHelper.SelfAndChildren;
            _activeFilter = MmActiveFilter.All;
            _selectedFilter = MmSelectedFilter.All;
            _networkFilter = MmNetworkFilter.Local;
            _tag = MmTagHelper.Everything;
        }

        #region Direction Properties (Chainable)

        /// <summary>Target direct children only.</summary>
        public MmRoutingBuilder Children
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilter.Child);
        }

        /// <summary>Target direct parents only.</summary>
        public MmRoutingBuilder Parents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilter.Parent);
        }

        /// <summary>Target self and all descendants (recursive children).</summary>
        public MmRoutingBuilder Descendants
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilterHelper.SelfAndDescendants);
        }

        /// <summary>Target self and all ancestors (recursive parents).</summary>
        public MmRoutingBuilder Ancestors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilterHelper.SelfAndAncestors);
        }

        /// <summary>Target siblings (same-parent nodes).</summary>
        public MmRoutingBuilder Siblings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilterHelper.SelfAndSiblings);
        }

        /// <summary>Target self and children (default routing).</summary>
        public MmRoutingBuilder SelfAndChildren
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilterHelper.SelfAndChildren);
        }

        /// <summary>Target all connected nodes (bidirectional).</summary>
        public MmRoutingBuilder All
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => WithLevel(MmLevelFilterHelper.SelfAndBidirectional);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MmRoutingBuilder WithLevel(MmLevelFilter filter)
        {
            var builder = this;
            builder._levelFilter = filter;
            return builder;
        }

        #endregion

        #region Filter Properties (Chainable)

        /// <summary>Only target active GameObjects.</summary>
        public MmRoutingBuilder Active
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var builder = this;
                builder._activeFilter = MmActiveFilter.Active;
                return builder;
            }
        }

        /// <summary>Include inactive GameObjects.</summary>
        public MmRoutingBuilder IncludeInactive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var builder = this;
                builder._activeFilter = MmActiveFilter.All;
                return builder;
            }
        }

        /// <summary>Only target selected responders (FSM state).</summary>
        public MmRoutingBuilder Selected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var builder = this;
                builder._selectedFilter = MmSelectedFilter.Selected;
                return builder;
            }
        }

        /// <summary>Send over network as well as locally.</summary>
        public MmRoutingBuilder OverNetwork
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var builder = this;
                builder._networkFilter = MmNetworkFilter.All;
                return builder;
            }
        }

        /// <summary>Send locally only (default).</summary>
        public MmRoutingBuilder LocalOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var builder = this;
                builder._networkFilter = MmNetworkFilter.Local;
                return builder;
            }
        }

        /// <summary>Filter by tag.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmRoutingBuilder WithTag(MmTag tag)
        {
            var builder = this;
            builder._tag = tag;
            return builder;
        }

        #endregion

        #region Terminal Methods - Send Values (Auto-Execute)

        /// <summary>Send a string value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(string value) => Execute(MmMethod.MessageString, value);

        /// <summary>Send an int value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(int value) => Execute(MmMethod.MessageInt, value);

        /// <summary>Send a float value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(float value) => Execute(MmMethod.MessageFloat, value);

        /// <summary>Send a bool value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(bool value) => Execute(MmMethod.MessageBool, value);

        /// <summary>Send a Vector3 value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(Vector3 value) => Execute(MmMethod.MessageVector3, value);

        /// <summary>Send a Vector4 value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(Vector4 value) => Execute(MmMethod.MessageVector4, value);

        /// <summary>Send a Quaternion value. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(Quaternion value) => Execute(MmMethod.MessageQuaternion, value);

        /// <summary>Send a Transform reference. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(Transform value) => Execute(MmMethod.MessageTransform, value);

        /// <summary>Send a GameObject reference. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(GameObject value) => Execute(MmMethod.MessageGameObject, value);

        /// <summary>Send an MmMessage directly. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(MmMessage message)
        {
            if (_relay == null) return;
            message.MetadataBlock = CreateMetadata();
            _relay.MmInvoke(message);
        }

        /// <summary>Send using a specific MmMethod with optional payload. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(MmMethod method, object payload = null) => Execute(method, payload);

        #endregion

        #region Terminal Methods - Standard Commands (Auto-Execute)

        /// <summary>Send Initialize command. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize() => Execute(MmMethod.Initialize, null);

        /// <summary>Send Refresh command. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh() => Execute(MmMethod.Refresh, null);

        /// <summary>Send Complete command. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete() => Execute(MmMethod.Complete, true);

        /// <summary>Send SetActive command. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetActive(bool active) => Execute(MmMethod.SetActive, active);

        /// <summary>Send Switch command with state index. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Switch(int stateIndex) => Execute(MmMethod.Switch, stateIndex);

        /// <summary>Send Switch command with state name. Auto-executes immediately.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Switch(string stateName) => Execute(MmMethod.Switch, stateName);

        #endregion

        #region Internal Execution

        /// <summary>
        /// Creates an MmMetadataBlock from the current builder settings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MmMetadataBlock CreateMetadata()
        {
            return new MmMetadataBlock(
                _tag,
                _levelFilter,
                _activeFilter,
                _selectedFilter,
                _networkFilter
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Execute(MmMethod method, object payload)
        {
            // Null-safe: silently do nothing if no relay node
            if (_relay == null) return;

            // Delegate to the existing fluent message for complex routing
            // This reuses all the target collection and predicate logic
            var fluent = new MmFluentMessage(_relay, method, payload);
            fluent = fluent.To(_levelFilter);

            // Apply other filters
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
