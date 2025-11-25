using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Extension methods for MmRelayNode to enable fluent message sending.
    /// These methods are the entry point for the fluent DSL API.
    /// </summary>
    public static class MmFluentExtensions
    {
        #region Basic Send Methods

        /// <summary>
        /// Send a message using the fluent API.
        /// </summary>
        /// <param name="relay">The relay node to send from.</param>
        /// <param name="payload">The message payload (will auto-detect type).</param>
        /// <returns>A fluent message builder for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, object payload)
        {
            if (relay == null)
                throw new ArgumentNullException(nameof(relay));

            // Auto-detect message type based on payload
            MmMethod method = DetectMethodFromPayload(payload);
            return new MmFluentMessage(relay, method, payload);
        }

        /// <summary>
        /// Send a boolean message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, bool value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageBool, value);
        }

        /// <summary>
        /// Send an integer message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, int value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageInt, value);
        }

        /// <summary>
        /// Send a float message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, float value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageFloat, value);
        }

        /// <summary>
        /// Send a string message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, string value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageString, value);
        }

        /// <summary>
        /// Send a Vector3 message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, Vector3 value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageVector3, value);
        }

        /// <summary>
        /// Send a Vector4 message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, Vector4 value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageVector4, value);
        }

        /// <summary>
        /// Send a Quaternion message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, Quaternion value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageQuaternion, value);
        }

        /// <summary>
        /// Send a Transform reference message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, Transform value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageTransform, value);
        }

        /// <summary>
        /// Send a GameObject reference message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, GameObject value)
        {
            return new MmFluentMessage(relay, MmMethod.MessageGameObject, value);
        }

        /// <summary>
        /// Send a custom MmMessage.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMessage message)
        {
            return new MmFluentMessage(relay, message.MmMethod, message);
        }

        #endregion
        #region Send With Explicit Method

        /// <summary>
        /// Send a message with explicit method (no payload).
        /// Use for methods like Initialize, Refresh, Complete, NoOp that don't require a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method)
        {
            return new MmFluentMessage(relay, method, null);
        }

        /// <summary>
        /// Send a message with explicit method and int value.
        /// Use this when you need to specify the method explicitly (e.g., for custom methods).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, int value)
        {
            return new MmFluentMessage(relay, method, value);
        }

        /// <summary>
        /// Send a message with explicit method and bool value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, bool value)
        {
            return new MmFluentMessage(relay, method, value);
        }

        /// <summary>
        /// Send a message with explicit method and float value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, float value)
        {
            return new MmFluentMessage(relay, method, value);
        }

        /// <summary>
        /// Send a message with explicit method and string value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, string value)
        {
            return new MmFluentMessage(relay, method, value);
        }

        /// <summary>
        /// Send a message with explicit method and Vector3 value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, Vector3 value)
        {
            return new MmFluentMessage(relay, method, value);
        }

        /// <summary>
        /// Send a message with explicit method and object payload.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Send(this MmRelayNode relay, MmMethod method, object payload)
        {
            return new MmFluentMessage(relay, method, payload);
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Send an Initialize command.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Initialize(this MmRelayNode relay)
        {
            return new MmFluentMessage(relay, MmMethod.Initialize, null);
        }

        /// <summary>
        /// Send a Refresh command.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Refresh(this MmRelayNode relay)
        {
            return new MmFluentMessage(relay, MmMethod.Refresh, null);
        }

        /// <summary>
        /// Send a Complete command.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Complete(this MmRelayNode relay)
        {
            return new MmFluentMessage(relay, MmMethod.Complete, null);
        }

        /// <summary>
        /// Send a SetActive command.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage SetActive(this MmRelayNode relay, bool active)
        {
            return new MmFluentMessage(relay, MmMethod.SetActive, active);
        }

        /// <summary>
        /// Send a Switch state command.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Switch(this MmRelayNode relay, string stateName)
        {
            return new MmFluentMessage(relay, MmMethod.Switch, stateName);
        }

        #endregion

        #region Broadcast Methods

        /// <summary>
        /// Broadcast a message to all connected nodes.
        /// Convenience method that automatically sets routing to SelfAndBidirectional.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Broadcast(this MmRelayNode relay, object payload)
        {
            return Send(relay, payload).ToAll();
        }

        /// <summary>
        /// Broadcast a string message to all connected nodes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmFluentMessage Broadcast(this MmRelayNode relay, string message)
        {
            return Send(relay, message).ToAll();
        }

        /// <summary>
        /// Broadcast an Initialize command to self and all connected nodes.
        /// Auto-executes immediately for convenience.
        /// Uses SelfAndChildren routing so responders on the same GameObject also receive the message.
        /// </summary>
        /// <remarks>
        /// DEPRECATED: Use relay.Init() from MmQuickExtensions instead.
        /// Init() broadcasts to Descendants (more common) and is shorter to type.
        /// </remarks>
        [Obsolete("Use relay.Init() instead for shorter syntax and descendant-focused routing.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastInitialize(this MmRelayNode relay)
        {
            // Use direct MmInvoke with SelfAndChildren so responders on the same object receive the message
            relay.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a Refresh command to self and all connected nodes.
        /// Auto-executes immediately for convenience.
        /// Uses SelfAndChildren routing so responders on the same GameObject also receive the message.
        /// </summary>
        /// <remarks>
        /// DEPRECATED: Use relay.Sync() from MmQuickExtensions instead.
        /// Sync() broadcasts to Descendants (more common) and is shorter to type.
        /// </remarks>
        [Obsolete("Use relay.Sync() instead for shorter syntax and descendant-focused routing.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BroadcastRefresh(this MmRelayNode relay)
        {
            // Use direct MmInvoke with SelfAndChildren so responders on the same object receive the message
            relay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Auto-detect the appropriate MmMethod based on payload type.
        /// </summary>
        private static MmMethod DetectMethodFromPayload(object payload)
        {
            return payload switch
            {
                bool => MmMethod.MessageBool,
                int => MmMethod.MessageInt,
                float => MmMethod.MessageFloat,
                string => MmMethod.MessageString,
                Vector3 => MmMethod.MessageVector3,
                Vector4 => MmMethod.MessageVector4,
                Quaternion => MmMethod.MessageQuaternion,
                Transform => MmMethod.MessageTransform,
                GameObject => MmMethod.MessageGameObject,
                MmMessage msg => msg.MmMethod,
                null => MmMethod.NoOp,
                _ => MmMethod.Message // Generic message for unknown types
            };
        }

        #endregion
    }
}