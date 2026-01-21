// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//  * Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
// =============================================================
// Authors:
// Ben Yang, Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================

// Suppress CS0618: IMmSerializable is obsolete - kept for backward compatibility
#pragma warning disable CS0618

using MercuryMessaging.Task;
using UnityEngine;
using UnityEngine.Pool;

namespace MercuryMessaging
{
    /// <summary>
    /// High-performance message pooling using Unity's ObjectPool.
    /// Eliminates per-message allocations in hot paths.
    ///
    /// Usage:
    ///   var msg = MmMessagePool.GetInt(42, MmMethod.MessageInt, metadata);
    ///   // ... use message ...
    ///   MmMessagePool.Return(msg);  // Called automatically at end of routing
    /// </summary>
    public static class MmMessagePool
    {
        #region Configuration

        /// <summary>
        /// Default pool capacity for each message type.
        /// </summary>
        public const int DEFAULT_CAPACITY = 50;

        /// <summary>
        /// Maximum pool size. Messages beyond this are garbage collected.
        /// </summary>
        public const int MAX_SIZE = 500;

        /// <summary>
        /// Enable collection checks in editor for debugging pool misuse.
        /// </summary>
        #if UNITY_EDITOR
        private const bool COLLECTION_CHECK = true;
        #else
        private const bool COLLECTION_CHECK = false;
        #endif

        #endregion

        #region Pools

        // Base message pool (for NoOp, Initialize, Refresh, etc.)
        private static readonly ObjectPool<MmMessage> _messagePool = new ObjectPool<MmMessage>(
            createFunc: () => new MmMessage(),
            actionOnGet: ResetMessage,
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        // Typed message pools
        private static readonly ObjectPool<MmMessageBool> _boolPool = new ObjectPool<MmMessageBool>(
            createFunc: () => new MmMessageBool(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageInt> _intPool = new ObjectPool<MmMessageInt>(
            createFunc: () => new MmMessageInt(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageFloat> _floatPool = new ObjectPool<MmMessageFloat>(
            createFunc: () => new MmMessageFloat(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageString> _stringPool = new ObjectPool<MmMessageString>(
            createFunc: () => new MmMessageString(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageVector3> _vector3Pool = new ObjectPool<MmMessageVector3>(
            createFunc: () => new MmMessageVector3(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageVector4> _vector4Pool = new ObjectPool<MmMessageVector4>(
            createFunc: () => new MmMessageVector4(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageQuaternion> _quaternionPool = new ObjectPool<MmMessageQuaternion>(
            createFunc: () => new MmMessageQuaternion(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageTransform> _transformPool = new ObjectPool<MmMessageTransform>(
            createFunc: () => new MmMessageTransform(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        private static readonly ObjectPool<MmMessageTransformList> _transformListPool = new ObjectPool<MmMessageTransformList>(
            createFunc: () => new MmMessageTransformList(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY / 2, // These are larger, use smaller pool
            maxSize: MAX_SIZE / 2
        );

        private static readonly ObjectPool<MmMessageByteArray> _byteArrayPool = new ObjectPool<MmMessageByteArray>(
            createFunc: () => new MmMessageByteArray(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY / 2,
            maxSize: MAX_SIZE / 2
        );

        private static readonly ObjectPool<MmMessageSerializable> _serializablePool = new ObjectPool<MmMessageSerializable>(
            createFunc: () => new MmMessageSerializable(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY / 2,
            maxSize: MAX_SIZE / 2
        );

        private static readonly ObjectPool<MmMessageGameObject> _gameObjectPool = new ObjectPool<MmMessageGameObject>(
            createFunc: () => new MmMessageGameObject(),
            actionOnGet: msg => ResetMessage(msg),
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        #endregion

        #region Reset Helper

        /// <summary>
        /// Resets a message to clean state for reuse.
        /// </summary>
        private static void ResetMessage(MmMessage msg)
        {
            msg.MmMethod = default;
            msg.MetadataBlock = new MmMetadataBlock();
            msg.NetId = 0;
            msg.root = true;
            msg.TimeStamp = null;
            msg.HopCount = 0;
            msg.Handled = false; // Reset Handled flag for E1-E3 early termination feature
            msg._isPooled = true; // Mark as pooled for return tracking

            // Return VisitedNodes HashSet to pool if it exists
            if (msg.VisitedNodes != null)
            {
                MmHashSetPool.Return(msg.VisitedNodes);
                msg.VisitedNodes = null;
            }
        }

        #endregion

        #region Typed Getters

        /// <summary>
        /// Get a base message from the pool (for NoOp, Initialize, Refresh, Complete, Switch).
        /// </summary>
        public static MmMessage Get(MmMethod method = default, MmMetadataBlock metadataBlock = null)
        {
            var msg = _messagePool.Get();
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmVoid;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a bool message from the pool.
        /// </summary>
        public static MmMessageBool GetBool(bool value, MmMethod method = MmMethod.MessageBool, MmMetadataBlock metadataBlock = null)
        {
            var msg = _boolPool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmBool;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get an int message from the pool.
        /// </summary>
        public static MmMessageInt GetInt(int value, MmMethod method = MmMethod.MessageInt, MmMetadataBlock metadataBlock = null)
        {
            var msg = _intPool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmInt;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a float message from the pool.
        /// </summary>
        public static MmMessageFloat GetFloat(float value, MmMethod method = MmMethod.MessageFloat, MmMetadataBlock metadataBlock = null)
        {
            var msg = _floatPool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmFloat;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a string message from the pool.
        /// </summary>
        public static MmMessageString GetString(string value, MmMethod method = MmMethod.MessageString, MmMetadataBlock metadataBlock = null)
        {
            var msg = _stringPool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmString;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a Vector3 message from the pool.
        /// </summary>
        public static MmMessageVector3 GetVector3(Vector3 value, MmMethod method = MmMethod.MessageVector3, MmMetadataBlock metadataBlock = null)
        {
            var msg = _vector3Pool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmVector3;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a Vector4 message from the pool.
        /// </summary>
        public static MmMessageVector4 GetVector4(Vector4 value, MmMethod method = MmMethod.MessageVector4, MmMetadataBlock metadataBlock = null)
        {
            var msg = _vector4Pool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmVector4;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a Quaternion message from the pool.
        /// </summary>
        public static MmMessageQuaternion GetQuaternion(Quaternion value, MmMethod method = MmMethod.MessageQuaternion, MmMetadataBlock metadataBlock = null)
        {
            var msg = _quaternionPool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmQuaternion;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a Transform message from the pool.
        /// </summary>
        public static MmMessageTransform GetTransform(MmTransform value, MmMethod method = MmMethod.MessageTransform, MmMetadataBlock metadataBlock = null)
        {
            var msg = _transformPool.Get();
            msg.MmTransform = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmTransform;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a TransformList message from the pool.
        /// </summary>
        public static MmMessageTransformList GetTransformList(System.Collections.Generic.List<MmTransform> value, MmMethod method = MmMethod.MessageTransformList, MmMetadataBlock metadataBlock = null)
        {
            var msg = _transformListPool.Get();
            msg.transforms = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmTransformList;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a ByteArray message from the pool.
        /// </summary>
        public static MmMessageByteArray GetByteArray(byte[] value, MmMethod method = MmMethod.MessageByteArray, MmMetadataBlock metadataBlock = null)
        {
            var msg = _byteArrayPool.Get();
            msg.byteArr = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmByteArray;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a Serializable message from the pool.
        /// </summary>
        public static MmMessageSerializable GetSerializable(IMmSerializable value, MmMethod method = MmMethod.Message, MmMetadataBlock metadataBlock = null)
        {
            var msg = _serializablePool.Get();
            msg.value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmSerializable;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        /// <summary>
        /// Get a GameObject message from the pool.
        /// </summary>
        public static MmMessageGameObject GetGameObject(GameObject value, MmMethod method = MmMethod.MessageGameObject, MmMetadataBlock metadataBlock = null)
        {
            var msg = _gameObjectPool.Get();
            msg.Value = value;
            msg.MmMethod = method;
            msg.MmMessageType = MmMessageType.MmGameObject;
            if (metadataBlock != null)
                msg.MetadataBlock = new MmMetadataBlock(metadataBlock);
            return msg;
        }

        #endregion

        #region Return Methods

        /// <summary>
        /// Return a message to the appropriate pool based on its type.
        /// Call this at the end of message routing (in root invocation only).
        /// </summary>
        /// <param name="message">The message to return to the pool.</param>
        /// <returns>True if returned to pool, false if message was null or deserialized (network messages are not pooled).</returns>
        public static bool Return(MmMessage message)
        {
            if (message == null)
                return false;

            // Don't pool network-deserialized messages - they have different lifecycles
            if (message.IsDeserialized)
                return false;

            switch (message.MmMessageType)
            {
                case MmMessageType.MmVoid:
                    _messagePool.Release(message);
                    break;
                case MmMessageType.MmBool:
                    _boolPool.Release((MmMessageBool)message);
                    break;
                case MmMessageType.MmInt:
                    _intPool.Release((MmMessageInt)message);
                    break;
                case MmMessageType.MmFloat:
                    _floatPool.Release((MmMessageFloat)message);
                    break;
                case MmMessageType.MmString:
                    _stringPool.Release((MmMessageString)message);
                    break;
                case MmMessageType.MmVector3:
                    _vector3Pool.Release((MmMessageVector3)message);
                    break;
                case MmMessageType.MmVector4:
                    _vector4Pool.Release((MmMessageVector4)message);
                    break;
                case MmMessageType.MmQuaternion:
                    _quaternionPool.Release((MmMessageQuaternion)message);
                    break;
                case MmMessageType.MmTransform:
                    _transformPool.Release((MmMessageTransform)message);
                    break;
                case MmMessageType.MmTransformList:
                    _transformListPool.Release((MmMessageTransformList)message);
                    break;
                case MmMessageType.MmByteArray:
                    _byteArrayPool.Release((MmMessageByteArray)message);
                    break;
                case MmMessageType.MmSerializable:
                    _serializablePool.Release((MmMessageSerializable)message);
                    break;
                case MmMessageType.MmGameObject:
                    _gameObjectPool.Release((MmMessageGameObject)message);
                    break;
                default:
                    // Unknown type, don't pool
                    return false;
            }

            return true;
        }

        #endregion

        #region Statistics (Editor Only)

        #if UNITY_EDITOR
        /// <summary>
        /// Get pool statistics for debugging.
        /// </summary>
        public static string GetStatistics()
        {
            return $"MmMessagePool Statistics:\n" +
                   $"  Base: {_messagePool.CountActive} active, {_messagePool.CountInactive} inactive\n" +
                   $"  Bool: {_boolPool.CountActive} active, {_boolPool.CountInactive} inactive\n" +
                   $"  Int: {_intPool.CountActive} active, {_intPool.CountInactive} inactive\n" +
                   $"  Float: {_floatPool.CountActive} active, {_floatPool.CountInactive} inactive\n" +
                   $"  String: {_stringPool.CountActive} active, {_stringPool.CountInactive} inactive\n" +
                   $"  Vector3: {_vector3Pool.CountActive} active, {_vector3Pool.CountInactive} inactive\n" +
                   $"  Vector4: {_vector4Pool.CountActive} active, {_vector4Pool.CountInactive} inactive\n" +
                   $"  Quaternion: {_quaternionPool.CountActive} active, {_quaternionPool.CountInactive} inactive\n" +
                   $"  Transform: {_transformPool.CountActive} active, {_transformPool.CountInactive} inactive\n" +
                   $"  TransformList: {_transformListPool.CountActive} active, {_transformListPool.CountInactive} inactive\n" +
                   $"  ByteArray: {_byteArrayPool.CountActive} active, {_byteArrayPool.CountInactive} inactive\n" +
                   $"  Serializable: {_serializablePool.CountActive} active, {_serializablePool.CountInactive} inactive\n" +
                   $"  GameObject: {_gameObjectPool.CountActive} active, {_gameObjectPool.CountInactive} inactive";
        }
        #endif

        #endregion
    }
}
