// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmResponderExtensions.cs - Fluent DSL extensions for responder configuration
// Part of DSL Overhaul Phase 6

using System;

namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for MmExtendableResponder to enable fluent handler configuration.
    /// Provides convenient methods for setting up message handlers.
    ///
    /// Example Usage:
    /// <code>
    /// // Fluent configuration
    /// responder.ConfigureHandlers()
    ///     .OnCustomMethod(1000, msg => HandleCustom(msg))
    ///     .OnInitialize(() => Debug.Log("Initialized"))
    ///     .OnSetActive(active => ToggleEffects(active))
    ///     .Build();
    ///
    /// // Quick handler registration
    /// responder.QuickRegister(1000, HandleCustomMethod);
    /// responder.QuickRegister(1001, HandleAnotherMethod);
    ///
    /// // Batch registration
    /// responder.RegisterHandlers(
    ///     (1000, HandleMethodA),
    ///     (1001, HandleMethodB),
    ///     (1002, HandleMethodC)
    /// );
    /// </code>
    /// </summary>
    public static class MmResponderExtensions
    {
        #region Fluent Configuration

        /// <summary>
        /// Start configuring handlers with a fluent builder.
        /// </summary>
        public static ResponderHandlerBuilder ConfigureHandlers(this MmExtendableResponder responder)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));

            return new ResponderHandlerBuilder(responder);
        }

        #endregion

        #region Quick Registration

        /// <summary>
        /// Quick register a custom method handler.
        /// Uses reflection to access protected RegisterCustomHandler.
        /// </summary>
        public static void QuickRegister(this MmExtendableResponder responder, int methodId, Action<MmMessage> handler)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));
            if (methodId < 1000)
                throw new ArgumentException("Custom methods must be >= 1000", nameof(methodId));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var registerMethod = typeof(MmExtendableResponder)
                .GetMethod("RegisterCustomHandler",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            registerMethod?.Invoke(responder, new object[] { (MmMethod)methodId, handler });
        }

        /// <summary>
        /// Quick register a custom method handler with typed message.
        /// </summary>
        public static void QuickRegister<T>(this MmExtendableResponder responder, int methodId, Action<T> handler)
            where T : MmMessage
        {
            QuickRegister(responder, methodId, msg => handler((T)msg));
        }

        /// <summary>
        /// Quick unregister a custom method handler.
        /// </summary>
        public static void QuickUnregister(this MmExtendableResponder responder, int methodId)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));

            var unregisterMethod = typeof(MmExtendableResponder)
                .GetMethod("UnregisterCustomHandler",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            unregisterMethod?.Invoke(responder, new object[] { (MmMethod)methodId });
        }

        #endregion

        #region Batch Registration

        /// <summary>
        /// Register multiple handlers at once.
        /// </summary>
        public static void RegisterHandlers(this MmExtendableResponder responder,
            params (int methodId, Action<MmMessage> handler)[] handlers)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));

            foreach (var (methodId, handler) in handlers)
            {
                responder.QuickRegister(methodId, handler);
            }
        }

        /// <summary>
        /// Unregister multiple handlers at once.
        /// </summary>
        public static void UnregisterHandlers(this MmExtendableResponder responder, params int[] methodIds)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));

            foreach (var methodId in methodIds)
            {
                responder.QuickUnregister(methodId);
            }
        }

        #endregion

        #region Handler Queries

        /// <summary>
        /// Check if a handler is registered for a method.
        /// </summary>
        public static bool IsHandlerRegistered(this MmExtendableResponder responder, int methodId)
        {
            if (responder == null)
                return false;

            var hasMethod = typeof(MmExtendableResponder)
                .GetMethod("HasCustomHandler",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return hasMethod != null && (bool)hasMethod.Invoke(responder, new object[] { (MmMethod)methodId });
        }

        #endregion

        #region Relay Node Access

        /// <summary>
        /// Get the relay node associated with this responder (if any).
        /// Returns null if no relay node is present on the GameObject.
        /// </summary>
        public static MmRelayNode GetRelayNode(this MmBaseResponder responder)
        {
            return responder?.GetComponent<MmRelayNode>();
        }

        /// <summary>
        /// Property-based routing builder for shorter DSL syntax.
        /// Enables: responder.To().Children.Send("Hello")
        /// Null-safe: returns default struct that no-ops if responder has no relay node.
        /// </summary>
        public static MmRoutingBuilder To(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            return relay != null ? new MmRoutingBuilder(relay) : default;
        }

        /// <summary>
        /// Get the relay node associated with this responder, creating one if needed.
        /// </summary>
        public static MmRelayNode GetOrCreateRelayNode(this MmBaseResponder responder)
        {
            if (responder == null)
                throw new ArgumentNullException(nameof(responder));

            var relay = responder.GetComponent<MmRelayNode>();
            if (relay == null)
            {
                relay = responder.gameObject.AddComponent<MmRelayNode>();
            }
            return relay;
        }

        #endregion

        // NOTE: Responder-level Send methods are defined in MmMessagingExtensions.cs
        // to avoid duplication and ambiguity errors.
        // Use responder.Send() from the MmMessagingExtensions namespace.
    }
}
