// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmTypeRegistry.cs - Type registry for polymorphic binary deserialization
// Part of S4: Serialization System Overhaul

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Registry for mapping type IDs to IMmBinarySerializable types.
    /// Enables polymorphic deserialization without reflection overhead.
    ///
    /// Usage:
    /// <code>
    /// // Register types at startup (e.g., in RuntimeInitializeOnLoadMethod)
    /// MmTypeRegistry.Register&lt;MyTaskInfo&gt;(1001);
    /// MmTypeRegistry.Register&lt;MyTrialData&gt;(1002);
    ///
    /// // Serialize with type ID
    /// writer.WriteUShort(MmTypeRegistry.GetTypeId&lt;MyTaskInfo&gt;());
    /// myTaskInfo.WriteTo(writer);
    ///
    /// // Deserialize polymorphically
    /// ushort typeId = reader.ReadUShort();
    /// var obj = MmTypeRegistry.Create(typeId);
    /// obj.ReadFrom(reader);
    /// </code>
    ///
    /// Type ID Ranges:
    /// - 0: Reserved (null/unknown)
    /// - 1-999: Framework types (MmMessage derivatives)
    /// - 1000-9999: Task/experiment types
    /// - 10000+: Application-specific types
    /// </summary>
    public static class MmTypeRegistry
    {
        /// <summary>
        /// Type ID for null/unknown types.
        /// </summary>
        public const ushort NullTypeId = 0;

        // Type ID -> Factory function
        private static readonly Dictionary<ushort, Func<IMmBinarySerializable>> _factories =
            new Dictionary<ushort, Func<IMmBinarySerializable>>();

        // Type -> Type ID (for serialization)
        private static readonly Dictionary<Type, ushort> _typeToId =
            new Dictionary<Type, ushort>();

        // Type ID -> Type (for debugging)
        private static readonly Dictionary<ushort, Type> _idToType =
            new Dictionary<ushort, Type>();

        #region Registration

        /// <summary>
        /// Register a type with a specific ID.
        /// The type must have a parameterless constructor.
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="typeId">Unique type ID (must be > 0)</param>
        public static void Register<T>(ushort typeId) where T : IMmBinarySerializable, new()
        {
            if (typeId == NullTypeId)
            {
                throw new ArgumentException("Type ID 0 is reserved for null types", nameof(typeId));
            }

            Type type = typeof(T);

            // Check for duplicate ID
            if (_factories.ContainsKey(typeId))
            {
                if (_idToType[typeId] == type)
                {
                    // Same type, same ID - ignore duplicate registration
                    return;
                }
                throw new InvalidOperationException(
                    $"Type ID {typeId} is already registered to {_idToType[typeId].Name}. " +
                    $"Cannot register {type.Name} with the same ID.");
            }

            // Check for duplicate type
            if (_typeToId.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    $"Type {type.Name} is already registered with ID {_typeToId[type]}. " +
                    $"Cannot register with ID {typeId}.");
            }

            _factories[typeId] = () => new T();
            _typeToId[type] = typeId;
            _idToType[typeId] = type;
        }

        /// <summary>
        /// Register a type with a custom factory function.
        /// Use when the type requires special construction.
        /// </summary>
        public static void Register<T>(ushort typeId, Func<T> factory) where T : IMmBinarySerializable
        {
            if (typeId == NullTypeId)
            {
                throw new ArgumentException("Type ID 0 is reserved for null types", nameof(typeId));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Type type = typeof(T);

            if (_factories.ContainsKey(typeId))
            {
                if (_idToType[typeId] == type) return; // Ignore duplicate
                throw new InvalidOperationException(
                    $"Type ID {typeId} is already registered to {_idToType[typeId].Name}");
            }

            if (_typeToId.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    $"Type {type.Name} is already registered with ID {_typeToId[type]}");
            }

            _factories[typeId] = () => factory();
            _typeToId[type] = typeId;
            _idToType[typeId] = type;
        }

        /// <summary>
        /// Unregister a type by ID.
        /// </summary>
        public static bool Unregister(ushort typeId)
        {
            if (!_idToType.TryGetValue(typeId, out Type type))
            {
                return false;
            }

            _factories.Remove(typeId);
            _typeToId.Remove(type);
            _idToType.Remove(typeId);
            return true;
        }

        /// <summary>
        /// Clear all registrations.
        /// </summary>
        public static void Clear()
        {
            _factories.Clear();
            _typeToId.Clear();
            _idToType.Clear();
        }

        #endregion

        #region Lookup

        /// <summary>
        /// Get the type ID for a registered type.
        /// Returns NullTypeId (0) if not registered.
        /// </summary>
        public static ushort GetTypeId<T>() where T : IMmBinarySerializable
        {
            return _typeToId.TryGetValue(typeof(T), out ushort id) ? id : NullTypeId;
        }

        /// <summary>
        /// Get the type ID for a registered type.
        /// Returns NullTypeId (0) if not registered.
        /// </summary>
        public static ushort GetTypeId(Type type)
        {
            if (type == null) return NullTypeId;
            return _typeToId.TryGetValue(type, out ushort id) ? id : NullTypeId;
        }

        /// <summary>
        /// Get the type ID for an instance.
        /// Returns NullTypeId (0) if not registered.
        /// </summary>
        public static ushort GetTypeId(IMmBinarySerializable instance)
        {
            if (instance == null) return NullTypeId;
            return GetTypeId(instance.GetType());
        }

        /// <summary>
        /// Check if a type ID is registered.
        /// </summary>
        public static bool IsRegistered(ushort typeId)
        {
            return _factories.ContainsKey(typeId);
        }

        /// <summary>
        /// Check if a type is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : IMmBinarySerializable
        {
            return _typeToId.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get the registered type for a type ID.
        /// Returns null if not registered.
        /// </summary>
        public static Type GetType(ushort typeId)
        {
            return _idToType.TryGetValue(typeId, out Type type) ? type : null;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Create a new instance of a registered type.
        /// Returns null if typeId is 0 or not registered.
        /// </summary>
        public static IMmBinarySerializable Create(ushort typeId)
        {
            if (typeId == NullTypeId) return null;

            if (!_factories.TryGetValue(typeId, out Func<IMmBinarySerializable> factory))
            {
                Debug.LogError($"MmTypeRegistry: Unknown type ID {typeId}. " +
                              "Ensure the type is registered before deserialization.");
                return null;
            }

            return factory();
        }

        /// <summary>
        /// Create and deserialize an instance from a reader.
        /// Reads the type ID from the reader first.
        /// </summary>
        public static IMmBinarySerializable ReadFrom(MmReader reader)
        {
            ushort typeId = reader.ReadUShort();
            if (typeId == NullTypeId) return null;

            IMmBinarySerializable instance = Create(typeId);
            if (instance == null)
            {
                // Type not registered - skip reading (data will be corrupted)
                Debug.LogError($"MmTypeRegistry: Cannot deserialize unknown type ID {typeId}");
                return null;
            }

            instance.ReadFrom(reader);
            return instance;
        }

        /// <summary>
        /// Serialize an instance with its type ID.
        /// Writes the type ID first, then the instance data.
        /// </summary>
        public static void WriteTo(MmWriter writer, IMmBinarySerializable instance)
        {
            if (instance == null)
            {
                writer.WriteUShort(NullTypeId);
                return;
            }

            ushort typeId = GetTypeId(instance);
            if (typeId == NullTypeId)
            {
                Debug.LogWarning($"MmTypeRegistry: Type {instance.GetType().Name} is not registered. " +
                                "Serialization will write null type ID.");
            }

            writer.WriteUShort(typeId);
            if (typeId != NullTypeId)
            {
                instance.WriteTo(writer);
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get the number of registered types.
        /// </summary>
        public static int Count => _factories.Count;

        /// <summary>
        /// Get all registered type IDs and their types for debugging.
        /// </summary>
        public static IEnumerable<KeyValuePair<ushort, Type>> GetAllRegistrations()
        {
            return _idToType;
        }

        /// <summary>
        /// Log all registered types.
        /// </summary>
        public static void LogRegistrations()
        {
            Debug.Log($"MmTypeRegistry: {_factories.Count} types registered:");
            foreach (var kvp in _idToType)
            {
                Debug.Log($"  [{kvp.Key}] {kvp.Value.FullName}");
            }
        }

        #endregion
    }
}
