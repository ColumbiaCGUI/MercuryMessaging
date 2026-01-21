// Copyright (c) 2017-2024, Columbia University
// All rights reserved.
//
// Ultra-minimal API for MercuryMessaging - auto-executing methods
// that don't require .Execute() for common operations.
//
// Part of Language DSL Phase 4: Quick Extensions

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Ultra-minimal extension methods for MmRelayNode.
    /// These methods auto-execute without requiring .Execute() for simple cases.
    ///
    /// API Tiers:
    /// - Quick (this class): Init(), Tell(), Done() - shortest, auto-execute
    /// - Convenience: Broadcast(), Notify() - one-liners, auto-execute
    /// - Fluent: Send().ToX().Execute() - chainable, requires Execute()
    /// - Raw: MmInvoke() - full control, verbose
    /// </summary>
    public static class MmQuickExtensions
    {
        #region Level 1: Zero-Config Commands (Auto-Execute)

        /// <summary>
        /// Initialize all descendants. Shortest way to broadcast Initialize.
        /// Equivalent to: relay.Broadcast(MmMethod.Initialize)
        /// </summary>
        /// <example>relay.Init(); // 14 chars vs 38 for Broadcast</example>
        [Obsolete("Use BroadcastInitialize() from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init(this MmRelayNode relay)
        {
            new MmFluentMessage(relay, MmMethod.Initialize, null).ToDescendants().Execute();
        }

        /// <summary>
        /// Notify parent(s) of completion. Shortest way to signal done.
        /// Equivalent to: relay.Notify(MmMethod.Complete)
        /// </summary>
        /// <example>relay.Done(); // 14 chars vs 22 for NotifyComplete</example>
        [Obsolete("Use NotifyComplete() from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Done(this MmRelayNode relay)
        {
            new MmFluentMessage(relay, MmMethod.Complete, null).ToParents().Execute();
        }

        /// <summary>
        /// Refresh all descendants. Shortest way to broadcast Refresh.
        /// Equivalent to: relay.Broadcast(MmMethod.Refresh)
        /// </summary>
        /// <example>relay.Sync(); // 14 chars</example>
        [Obsolete("Use BroadcastRefresh() from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sync(this MmRelayNode relay)
        {
            new MmFluentMessage(relay, MmMethod.Refresh, null).ToDescendants().Execute();
        }

        #endregion

        #region Level 1.5: Value Broadcasts to Descendants (Auto-Execute)

        /// <summary>
        /// Send a boolean value to all descendants.
        /// Equivalent to: relay.Broadcast(MmMethod.MessageBool, value)
        /// </summary>
        /// <example>relay.Tell(true); // 19 chars vs 40 for Broadcast</example>
        [Obsolete("Use BroadcastValue(bool) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tell(this MmRelayNode relay, bool value)
        {
            new MmFluentMessage(relay, MmMethod.MessageBool, value).ToDescendants().Execute();
        }

        /// <summary>
        /// Send an integer value to all descendants.
        /// </summary>
        /// <example>relay.Tell(42); // 17 chars</example>
        [Obsolete("Use BroadcastValue(int) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tell(this MmRelayNode relay, int value)
        {
            new MmFluentMessage(relay, MmMethod.MessageInt, value).ToDescendants().Execute();
        }

        /// <summary>
        /// Send a float value to all descendants.
        /// </summary>
        /// <example>relay.Tell(3.14f); // 21 chars</example>
        [Obsolete("Use BroadcastValue(float) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tell(this MmRelayNode relay, float value)
        {
            new MmFluentMessage(relay, MmMethod.MessageFloat, value).ToDescendants().Execute();
        }

        /// <summary>
        /// Send a string value to all descendants.
        /// </summary>
        /// <example>relay.Tell("hello"); // 22 chars</example>
        [Obsolete("Use BroadcastValue(string) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tell(this MmRelayNode relay, string value)
        {
            new MmFluentMessage(relay, MmMethod.MessageString, value).ToDescendants().Execute();
        }

        /// <summary>
        /// Send a Vector3 value to all descendants.
        /// </summary>
        [Obsolete("Use relay.Send(value).ToDescendants().Execute() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Tell(this MmRelayNode relay, Vector3 value)
        {
            new MmFluentMessage(relay, MmMethod.MessageVector3, value).ToDescendants().Execute();
        }

        #endregion

        #region Level 1.5: Value Notifications to Parents (Auto-Execute)

        /// <summary>
        /// Report a boolean value to parent(s).
        /// Equivalent to: relay.Notify(MmMethod.MessageBool, value)
        /// </summary>
        /// <example>relay.Report(true); // 21 chars vs 35 for Notify</example>
        [Obsolete("Use NotifyValue(bool) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Report(this MmRelayNode relay, bool value)
        {
            new MmFluentMessage(relay, MmMethod.MessageBool, value).ToParents().Execute();
        }

        /// <summary>
        /// Report an integer value to parent(s).
        /// </summary>
        /// <example>relay.Report(100); // 19 chars</example>
        [Obsolete("Use NotifyValue(int) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Report(this MmRelayNode relay, int value)
        {
            new MmFluentMessage(relay, MmMethod.MessageInt, value).ToParents().Execute();
        }

        /// <summary>
        /// Report a float value to parent(s).
        /// </summary>
        [Obsolete("Use NotifyValue(float) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Report(this MmRelayNode relay, float value)
        {
            new MmFluentMessage(relay, MmMethod.MessageFloat, value).ToParents().Execute();
        }

        /// <summary>
        /// Report a string value to parent(s).
        /// </summary>
        /// <example>relay.Report("done"); // 22 chars</example>
        [Obsolete("Use NotifyValue(string) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Report(this MmRelayNode relay, string value)
        {
            new MmFluentMessage(relay, MmMethod.MessageString, value).ToParents().Execute();
        }

        #endregion

        #region Level 2: Directional Shortcuts with Optional Chaining

        /// <summary>
        /// Send SetActive to all descendants. Common pattern for enabling/disabling subtrees.
        /// </summary>
        /// <example>relay.Activate(true); // 23 chars vs 31 for BroadcastSetActive</example>
        [Obsolete("Use BroadcastSetActive(bool) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Activate(this MmRelayNode relay, bool active)
        {
            new MmFluentMessage(relay, MmMethod.SetActive, active).ToDescendants().Execute();
        }

        /// <summary>
        /// Send a state switch command to descendants.
        /// </summary>
        /// <example>relay.State("Playing"); // 25 chars</example>
        [Obsolete("Use BroadcastSwitch(string) from MmMessagingExtensions instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void State(this MmRelayNode relay, string stateName)
        {
            new MmFluentMessage(relay, MmMethod.Switch, stateName).ToDescendants().Execute();
        }

        #endregion
    }
}
