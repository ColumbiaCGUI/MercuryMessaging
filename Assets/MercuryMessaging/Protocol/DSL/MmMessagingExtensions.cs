// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmMessagingExtensions.cs - Unified Messaging API for MercuryMessaging
// Part of the DSL Overhaul (Phase 1)
//
// This file provides a unified API that works on BOTH MmRelayNode AND MmBaseResponder.
//
// Two-Tier API:
// - Tier 1: Auto-Execute methods (BroadcastInitialize, NotifyComplete, etc.)
// - Tier 2: Fluent Chain methods (Send().ToDescendants().Execute())

using System.Runtime.CompilerServices;

namespace MercuryMessaging
{
    /// <summary>
    /// Unified messaging extensions for both MmRelayNode and MmBaseResponder.
    /// Provides a consistent API across both types for common messaging operations.
    ///
    /// Naming Convention:
    /// - Broadcast* = Send DOWN to descendants (matches MmMethod enum names)
    /// - Notify* = Send UP to parents (matches MmMethod enum names)
    ///
    /// Example Usage:
    /// <code>
    /// // On relay nodes
    /// relay.BroadcastInitialize();
    /// relay.BroadcastValue("hello");
    /// relay.NotifyComplete();
    ///
    /// // Same API on responders!
    /// responder.BroadcastInitialize();
    /// responder.NotifyValue(42);
    ///
    /// // Fluent chain (Tier 2)
    /// responder.Send("hello").ToDescendants().WithTag(MmTag.Tag0).Execute();
    /// </code>
    /// </summary>
    public static class MmMessagingExtensions
    {
        #region Tier 1: Broadcast (Down) - Relay Node

        /// <summary>
        /// Broadcast Initialize to self and all children.
        /// Uses default SelfAndChildren routing (includes self).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastInitialize(this MmRelayNode relay)
            => relay.Send(MmMethod.Initialize).Execute();

        /// <summary>
        /// Broadcast Refresh to self and all children.
        /// Uses default SelfAndChildren routing (includes self).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastRefresh(this MmRelayNode relay)
            => relay.Send(MmMethod.Refresh).Execute();

        /// <summary>
        /// Broadcast SetActive to self and all children.
        /// </summary>
        /// <param name="relay">The relay node to broadcast from.</param>
        /// <param name="active">The active state to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastSetActive(this MmRelayNode relay, bool active)
            => relay.Send(MmMethod.SetActive, active).Execute();

        /// <summary>
        /// Broadcast Switch to self and all children.
        /// </summary>
        /// <param name="relay">The relay node to broadcast from.</param>
        /// <param name="state">The state name to switch to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastSwitch(this MmRelayNode relay, string state)
            => relay.Send(MmMethod.Switch, state).Execute();

        /// <summary>Broadcast bool value to self and all children.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmRelayNode relay, bool value)
            => relay.Send(value).Execute();

        /// <summary>Broadcast int value to self and all children.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmRelayNode relay, int value)
            => relay.Send(value).Execute();

        /// <summary>Broadcast float value to self and all children.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmRelayNode relay, float value)
            => relay.Send(value).Execute();

        /// <summary>Broadcast string value to self and all children.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmRelayNode relay, string value)
            => relay.Send(value).Execute();

        #endregion

        #region Tier 1: Notify (Up) - Relay Node

        /// <summary>
        /// Notify Complete to all parents/ancestors.
        /// Equivalent to: relay.Send(MmMethod.Complete).ToParents().Execute()
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyComplete(this MmRelayNode relay)
            => relay.Send(MmMethod.Complete, true).ToParents().Execute();

        /// <summary>Notify bool value to all parents.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmRelayNode relay, bool value)
            => relay.Send(value).ToParents().Execute();

        /// <summary>Notify int value to all parents.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmRelayNode relay, int value)
            => relay.Send(value).ToParents().Execute();

        /// <summary>Notify float value to all parents.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmRelayNode relay, float value)
            => relay.Send(value).ToParents().Execute();

        /// <summary>Notify string value to all parents.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmRelayNode relay, string value)
            => relay.Send(value).ToParents().Execute();

        #endregion

        #region Tier 1: Broadcast (Down) - Responder

        /// <summary>
        /// Broadcast Initialize to self and all children (from responder).
        /// Null-safe: does nothing if responder has no relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastInitialize(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(MmMethod.Initialize).Execute();
        }

        /// <summary>
        /// Broadcast Refresh to self and all children (from responder).
        /// Null-safe: does nothing if responder has no relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastRefresh(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(MmMethod.Refresh).Execute();
        }

        /// <summary>Broadcast SetActive to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastSetActive(this MmBaseResponder responder, bool active)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(MmMethod.SetActive, active).Execute();
        }

        /// <summary>Broadcast Switch to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastSwitch(this MmBaseResponder responder, string state)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(MmMethod.Switch, state).Execute();
        }

        /// <summary>Broadcast bool value to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmBaseResponder responder, bool value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).Execute();
        }

        /// <summary>Broadcast int value to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmBaseResponder responder, int value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).Execute();
        }

        /// <summary>Broadcast float value to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmBaseResponder responder, float value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).Execute();
        }

        /// <summary>Broadcast string value to self and all children (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastValue(this MmBaseResponder responder, string value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).Execute();
        }

        #endregion

        #region Tier 1: Notify (Up) - Responder

        /// <summary>
        /// Notify Complete to all parents/ancestors (from responder).
        /// Null-safe: does nothing if responder has no relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyComplete(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(MmMethod.Complete, true).ToParents().Execute();
        }

        /// <summary>Notify bool value to all parents (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmBaseResponder responder, bool value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).ToParents().Execute();
        }

        /// <summary>Notify int value to all parents (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmBaseResponder responder, int value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).ToParents().Execute();
        }

        /// <summary>Notify float value to all parents (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmBaseResponder responder, float value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).ToParents().Execute();
        }

        /// <summary>Notify string value to all parents (from responder).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyValue(this MmBaseResponder responder, string value)
        {
            var relay = responder.GetRelayNode();
            if (relay != null) relay.Send(value).ToParents().Execute();
        }

        #endregion

        #region Tier 2: Fluent Chain for Responders

        /// <summary>
        /// Start a fluent message chain from a responder with a payload.
        /// Returns a default MmFluentMessage if responder has no relay node.
        ///
        /// Example: responder.Send("hello").ToDescendants().Execute();
        /// </summary>
        /// <param name="responder">The responder to send from.</param>
        /// <param name="payload">The value to send (bool, int, float, string, etc.).</param>
        /// <returns>A fluent message builder for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmBaseResponder responder, object payload)
            => responder.GetRelayNode()?.Send(payload) ?? default;

        /// <summary>
        /// Start a fluent message chain from a responder with a method.
        /// Returns a default MmFluentMessage if responder has no relay node.
        ///
        /// Example: responder.Send(MmMethod.Initialize).ToDescendants().Execute();
        /// </summary>
        /// <param name="responder">The responder to send from.</param>
        /// <param name="method">The MmMethod to invoke.</param>
        /// <returns>A fluent message builder for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmBaseResponder responder, MmMethod method)
            => responder.GetRelayNode()?.Send(method) ?? default;

        /// <summary>
        /// Start a fluent message chain from a responder with method and payload.
        /// Returns a default MmFluentMessage if responder has no relay node.
        ///
        /// Example: responder.Send(MmMethod.MessageString, "hello").ToDescendants().Execute();
        /// </summary>
        /// <param name="responder">The responder to send from.</param>
        /// <param name="method">The MmMethod to invoke.</param>
        /// <param name="payload">The value to send.</param>
        /// <returns>A fluent message builder for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmBaseResponder responder, MmMethod method, object payload)
            => responder.GetRelayNode()?.Send(method, payload) ?? default;

        #endregion
    }
}
