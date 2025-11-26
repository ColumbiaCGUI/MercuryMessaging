// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmNetworkExtensions.cs - Fluent DSL extensions for network messaging
// Part of DSL Overhaul Phase 5

using System;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Extension methods for network-aware messaging.
    /// Provides convenient patterns for handling networked messages.
    ///
    /// Example Usage:
    /// <code>
    /// // Send message to all clients (from host/server)
    /// relay.BroadcastToClients(MmMethod.MessageString, "Hello clients");
    ///
    /// // Send to server (from client)
    /// relay.SendToServer(MmMethod.MessageInt, 42);
    ///
    /// // Check if message came from network
    /// if (relay.IsNetworkMessage(message))
    /// {
    ///     // Handle network-origin message differently
    /// }
    ///
    /// // Send with automatic host handling
    /// relay.SendNetworked("StateUpdate").ToDescendants().Execute();
    /// </code>
    /// </summary>
    public static class MmNetworkExtensions
    {
        #region Network State Queries

        /// <summary>
        /// Check if a message originated from the network (was deserialized).
        /// Useful for distinguishing local vs. remote message sources.
        /// </summary>
        public static bool IsNetworkMessage(this MmRelayNode relay, MmMessage message)
        {
            return message != null && message.IsDeserialized;
        }

        /// <summary>
        /// Check if this node has network capability.
        /// </summary>
        public static bool HasNetworkSupport(this MmRelayNode relay)
        {
            return relay?.MmNetworkResponder != null;
        }

        /// <summary>
        /// Check if this node is running as a host (both client and server).
        /// </summary>
        public static bool IsHost(this MmRelayNode relay)
        {
            var netResponder = relay?.MmNetworkResponder;
            return netResponder != null && netResponder.OnClient && netResponder.OnServer;
        }

        /// <summary>
        /// Check if this node is a client only (not server).
        /// </summary>
        public static bool IsClientOnly(this MmRelayNode relay)
        {
            var netResponder = relay?.MmNetworkResponder;
            return netResponder != null && netResponder.OnClient && !netResponder.OnServer;
        }

        /// <summary>
        /// Check if this node is a server only (not client).
        /// </summary>
        public static bool IsServerOnly(this MmRelayNode relay)
        {
            var netResponder = relay?.MmNetworkResponder;
            return netResponder != null && !netResponder.OnClient && netResponder.OnServer;
        }

        #endregion

        #region Network Send Methods

        /// <summary>
        /// Send a message over the network (locally + remote).
        /// Convenient shorthand for Send().OverNetwork().
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, MmMethod method)
        {
            return MmFluentExtensions.Send(relay, method).OverNetwork();
        }

        /// <summary>
        /// Send a string message over the network.
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, string value)
        {
            return MmFluentExtensions.Send(relay, value).OverNetwork();
        }

        /// <summary>
        /// Send an int message over the network.
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, int value)
        {
            return MmFluentExtensions.Send(relay, value).OverNetwork();
        }

        /// <summary>
        /// Send a float message over the network.
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, float value)
        {
            return MmFluentExtensions.Send(relay, value).OverNetwork();
        }

        /// <summary>
        /// Send a bool message over the network.
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, bool value)
        {
            return MmFluentExtensions.Send(relay, value).OverNetwork();
        }

        /// <summary>
        /// Send a Vector3 message over the network.
        /// </summary>
        public static MmFluentMessage SendNetworked(this MmRelayNode relay, Vector3 value)
        {
            return MmFluentExtensions.Send(relay, value).OverNetwork();
        }

        #endregion

        #region Conditional Network Methods

        /// <summary>
        /// Execute action only if message came from network.
        /// </summary>
        /// <example>
        /// relay.IfFromNetwork(message, () => {
        ///     // Handle network message
        /// });
        /// </example>
        public static void IfFromNetwork(this MmRelayNode relay, MmMessage message, Action action)
        {
            if (message != null && message.IsDeserialized)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Execute action only if message is local (not from network).
        /// </summary>
        public static void IfLocal(this MmRelayNode relay, MmMessage message, Action action)
        {
            if (message != null && !message.IsDeserialized)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Execute action only if this node is the host.
        /// </summary>
        public static void IfHost(this MmRelayNode relay, Action action)
        {
            if (relay.IsHost())
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Execute action only if this node is a client (not server).
        /// </summary>
        public static void IfClient(this MmRelayNode relay, Action action)
        {
            if (relay.IsClientOnly())
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Execute action only if this node is a server (not client).
        /// </summary>
        public static void IfServer(this MmRelayNode relay, Action action)
        {
            if (relay.IsServerOnly())
            {
                action?.Invoke();
            }
        }

        #endregion

        #region Responder Extensions

        /// <summary>
        /// Check if a message originated from the network (for responders).
        /// </summary>
        public static bool IsNetworkMessage(this MmBaseResponder responder, MmMessage message)
        {
            return message != null && message.IsDeserialized;
        }

        /// <summary>
        /// Execute action only if message came from network (for responders).
        /// </summary>
        public static void IfFromNetwork(this MmBaseResponder responder, MmMessage message, Action action)
        {
            if (message != null && message.IsDeserialized)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// Execute action only if message is local (for responders).
        /// </summary>
        public static void IfLocal(this MmBaseResponder responder, MmMessage message, Action action)
        {
            if (message != null && !message.IsDeserialized)
            {
                action?.Invoke();
            }
        }

        #endregion
    }
}
