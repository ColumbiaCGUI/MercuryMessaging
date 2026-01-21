using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Factory class for creating MmMessage instances from various types.
    /// Provides type-safe message creation with automatic method detection.
    ///
    /// Phase 3 - Task 3.1: Message Factory
    /// </summary>
    public static class MmMessageFactory
    {
        #region Generic Create Methods

        /// <summary>
        /// Create a message from a value, automatically detecting the message type.
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="value">The value to wrap in a message</param>
        /// <returns>An MmMessage containing the value</returns>
        /// <example>
        /// var msg = MmMessageFactory.Create(42);        // Creates MmMessageInt
        /// var msg = MmMessageFactory.Create("Hello");   // Creates MmMessageString
        /// var msg = MmMessageFactory.Create(true);      // Creates MmMessageBool
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Create<T>(T value)
        {
            return value switch
            {
                bool b => Bool(b),
                int i => Int(i),
                float f => Float(f),
                string s => String(s),
                Vector3 v3 => Vector3(v3),
                Vector4 v4 => Vector4(v4),
                Quaternion q => Quaternion(q),
                Transform t => Transform(t),
                GameObject go => GameObject(go),
                byte[] bytes => ByteArray(bytes),
                MmMessage msg => msg, // Pass through existing messages
                _ => throw new ArgumentException($"Unsupported type: {typeof(T).Name}. Use custom message types for complex data.")
            };
        }

        /// <summary>
        /// Create a message from an object, with runtime type detection.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <returns>An MmMessage containing the value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Create(object value)
        {
            if (value == null)
                return new MmMessage { MmMethod = MmMethod.NoOp };

            return value switch
            {
                bool b => Bool(b),
                int i => Int(i),
                float f => Float(f),
                string s => String(s),
                Vector3 v3 => Vector3(v3),
                Vector4 v4 => Vector4(v4),
                Quaternion q => Quaternion(q),
                Transform t => Transform(t),
                GameObject go => GameObject(go),
                byte[] bytes => ByteArray(bytes),
                MmMessage msg => msg,
                _ => throw new ArgumentException($"Unsupported type: {value.GetType().Name}. Use custom message types for complex data.")
            };
        }

        #endregion

        #region Typed Factory Methods

        /// <summary>
        /// Create a boolean message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageBool Bool(bool value)
        {
            return new MmMessageBool
            {
                MmMethod = MmMethod.MessageBool,
                value = value
            };
        }

        /// <summary>
        /// Create an integer message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageInt Int(int value)
        {
            return new MmMessageInt
            {
                MmMethod = MmMethod.MessageInt,
                value = value
            };
        }

        /// <summary>
        /// Create a float message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageFloat Float(float value)
        {
            return new MmMessageFloat
            {
                MmMethod = MmMethod.MessageFloat,
                value = value
            };
        }

        /// <summary>
        /// Create a string message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageString String(string value)
        {
            return new MmMessageString
            {
                MmMethod = MmMethod.MessageString,
                value = value ?? string.Empty
            };
        }

        /// <summary>
        /// Create a Vector3 message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageVector3 Vector3(Vector3 value)
        {
            return new MmMessageVector3
            {
                MmMethod = MmMethod.MessageVector3,
                value = value
            };
        }

        /// <summary>
        /// Create a Vector4 message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageVector4 Vector4(Vector4 value)
        {
            return new MmMessageVector4
            {
                MmMethod = MmMethod.MessageVector4,
                value = value
            };
        }

        /// <summary>
        /// Create a Quaternion message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageQuaternion Quaternion(Quaternion value)
        {
            return new MmMessageQuaternion
            {
                MmMethod = MmMethod.MessageQuaternion,
                value = value
            };
        }

        /// <summary>
        /// Create a Transform message from a Unity Transform.
        /// Note: Converts Transform to MmTransform internally using global coordinates.
        /// </summary>
        /// <param name="value">The Unity Transform to convert.</param>
        /// <param name="useGlobal">Use global (world) coordinates if true, local if false. Default: true.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageTransform Transform(Transform value, bool useGlobal = true)
        {
            return new MmMessageTransform
            {
                MmMethod = MmMethod.MessageTransform,
                MmTransform = value != null ? new MmTransform(value, useGlobal) : default,
                LocalTransform = !useGlobal
            };
        }

        /// <summary>
        /// Create a Transform message from an MmTransform.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageTransform Transform(MmTransform value)
        {
            return new MmMessageTransform
            {
                MmMethod = MmMethod.MessageTransform,
                MmTransform = value
            };
        }

        /// <summary>
        /// Create a GameObject message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageGameObject GameObject(GameObject value)
        {
            return new MmMessageGameObject
            {
                MmMethod = MmMethod.MessageGameObject,
                Value = value
            };
        }

        /// <summary>
        /// Create a byte array message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageByteArray ByteArray(byte[] value)
        {
            var arr = value ?? Array.Empty<byte>();
            return new MmMessageByteArray
            {
                MmMethod = MmMethod.MessageByteArray,
                byteArr = arr,
                length = arr.Length
            };
        }

        /// <summary>
        /// Create a Transform list message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageTransformList TransformList(List<MmTransform> value)
        {
            return new MmMessageTransformList
            {
                MmMethod = MmMethod.MessageTransformList,
                transforms = value ?? new List<MmTransform>()
            };
        }

        #endregion

        #region Command Factory Methods

        /// <summary>
        /// Create an Initialize command message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Initialize()
        {
            return new MmMessage { MmMethod = MmMethod.Initialize };
        }

        /// <summary>
        /// Create a Refresh command message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Refresh()
        {
            return new MmMessage { MmMethod = MmMethod.Refresh };
        }

        /// <summary>
        /// Create a Complete command message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Complete()
        {
            return new MmMessage { MmMethod = MmMethod.Complete };
        }

        /// <summary>
        /// Create a SetActive command message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageBool SetActive(bool active)
        {
            return new MmMessageBool
            {
                MmMethod = MmMethod.SetActive,
                value = active
            };
        }

        /// <summary>
        /// Create a Switch command message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageString Switch(string stateName)
        {
            return new MmMessageString
            {
                MmMethod = MmMethod.Switch,
                value = stateName ?? string.Empty
            };
        }

        /// <summary>
        /// Create a Switch command message by state index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessageInt Switch(int stateIndex)
        {
            return new MmMessageInt
            {
                MmMethod = MmMethod.Switch,
                value = stateIndex
            };
        }

        #endregion

        #region Custom Message Factory

        /// <summary>
        /// Create a custom method message.
        /// </summary>
        /// <param name="methodId">Custom method ID (should be >= 1000)</param>
        /// <returns>A message with the custom method ID</returns>
        /// <example>
        /// var msg = MmMessageFactory.Custom(1001);
        /// relay.MmInvoke(msg);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Custom(int methodId)
        {
            if (methodId < 1000)
            {
                Debug.LogWarning($"Custom method IDs should be >= 1000 to avoid conflicts with standard methods. Got: {methodId}");
            }
            return new MmMessage { MmMethod = (MmMethod)methodId };
        }

        /// <summary>
        /// Create a custom method message with a payload.
        /// </summary>
        /// <typeparam name="T">The payload type</typeparam>
        /// <param name="methodId">Custom method ID</param>
        /// <param name="payload">The payload value</param>
        /// <returns>A message with the custom method and payload</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmMessage Custom<T>(int methodId, T payload)
        {
            var msg = Create(payload);
            msg.MmMethod = (MmMethod)methodId;
            return msg;
        }

        #endregion

        #region Metadata Configuration

        /// <summary>
        /// Configure metadata on a message.
        /// </summary>
        /// <param name="message">The message to configure</param>
        /// <param name="metadata">The metadata block to apply</param>
        /// <returns>The configured message</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithMetadata<T>(this T message, MmMetadataBlock metadata) where T : MmMessage
        {
            message.MetadataBlock = metadata;
            return message;
        }

        /// <summary>
        /// Configure a message to target children.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToChildren<T>(this T message) where T : MmMessage
        {
            message.MetadataBlock.LevelFilter = MmLevelFilter.Child;
            return message;
        }

        /// <summary>
        /// Configure a message to target parents.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToParents<T>(this T message) where T : MmMessage
        {
            message.MetadataBlock.LevelFilter = MmLevelFilter.Parent;
            return message;
        }

        /// <summary>
        /// Configure a message to target descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToDescendants<T>(this T message) where T : MmMessage
        {
            message.MetadataBlock.LevelFilter = MmLevelFilter.Descendants;
            return message;
        }

        /// <summary>
        /// Configure a message to target all directions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToAll<T>(this T message) where T : MmMessage
        {
            message.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndBidirectional;
            return message;
        }

        /// <summary>
        /// Configure a message with a specific tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithTag<T>(this T message, MmTag tag) where T : MmMessage
        {
            message.MetadataBlock.Tag = tag;
            return message;
        }

        #endregion
    }
}
