// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmStateDelta.cs - Delta compression for network messages
// Tracks state changes and only sends differences to reduce bandwidth

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Represents a delta (difference) between two states.
    /// Used for bandwidth-efficient state synchronization.
    /// </summary>
    [Serializable]
    public class MmStateDelta
    {
        /// <summary>
        /// Unique identifier for this delta.
        /// </summary>
        public uint DeltaId;

        /// <summary>
        /// The sequence number this delta is based on.
        /// Client uses this to know what baseline state to apply delta to.
        /// </summary>
        public uint BaselineSequence;

        /// <summary>
        /// List of changed fields.
        /// </summary>
        public List<MmDeltaField> ChangedFields = new List<MmDeltaField>();

        /// <summary>
        /// Timestamp when delta was created.
        /// </summary>
        public double Timestamp;

        /// <summary>
        /// Whether this is a full state snapshot (no baseline required).
        /// </summary>
        public bool IsFullSnapshot;

        /// <summary>
        /// Network ID of the object this delta applies to.
        /// </summary>
        public uint NetId;
    }

    /// <summary>
    /// Represents a single changed field in a delta.
    /// </summary>
    [Serializable]
    public struct MmDeltaField
    {
        /// <summary>
        /// Index/ID of the field that changed.
        /// </summary>
        public ushort FieldId;

        /// <summary>
        /// Type of the field value.
        /// </summary>
        public MmDeltaFieldType FieldType;

        /// <summary>
        /// Serialized field value.
        /// </summary>
        public byte[] Value;

        public MmDeltaField(ushort fieldId, MmDeltaFieldType fieldType, byte[] value)
        {
            FieldId = fieldId;
            FieldType = fieldType;
            Value = value;
        }
    }

    /// <summary>
    /// Type of delta field for efficient serialization.
    /// </summary>
    public enum MmDeltaFieldType : byte
    {
        Bool = 0,
        Int = 1,
        Float = 2,
        String = 3,
        Vector3 = 4,
        Vector4 = 5,
        Quaternion = 6,
        ByteArray = 7,
        Transform = 8
    }

    /// <summary>
    /// Tracks state for a single networked object and computes deltas.
    /// </summary>
    public class MmStateTracker
    {
        private readonly Dictionary<ushort, object> _currentState = new Dictionary<ushort, object>();
        private readonly Dictionary<ushort, object> _lastSentState = new Dictionary<ushort, object>();
        private readonly Dictionary<ushort, MmDeltaFieldType> _fieldTypes = new Dictionary<ushort, MmDeltaFieldType>();

        /// <summary>
        /// Network ID of the tracked object.
        /// </summary>
        public uint NetId { get; }

        /// <summary>
        /// Current sequence number for this tracker.
        /// </summary>
        public uint CurrentSequence { get; private set; }

        /// <summary>
        /// Last acknowledged sequence number by remote.
        /// </summary>
        public uint AckedSequence { get; set; }

        public MmStateTracker(uint netId)
        {
            NetId = netId;
            CurrentSequence = 0;
            AckedSequence = 0;
        }

        /// <summary>
        /// Register a field to be tracked.
        /// </summary>
        public void RegisterField(ushort fieldId, MmDeltaFieldType type)
        {
            _fieldTypes[fieldId] = type;
        }

        /// <summary>
        /// Update a tracked field's value.
        /// </summary>
        public void SetFieldValue(ushort fieldId, object value)
        {
            _currentState[fieldId] = value;
        }

        /// <summary>
        /// Compute delta between current and last sent state.
        /// </summary>
        /// <returns>Delta containing only changed fields, or null if no changes</returns>
        public MmStateDelta ComputeDelta()
        {
            var changedFields = new List<MmDeltaField>();

            foreach (var kvp in _currentState)
            {
                ushort fieldId = kvp.Key;
                object currentValue = kvp.Value;

                if (!_lastSentState.TryGetValue(fieldId, out object lastValue) ||
                    !ValuesEqual(currentValue, lastValue))
                {
                    if (_fieldTypes.TryGetValue(fieldId, out var fieldType))
                    {
                        changedFields.Add(new MmDeltaField(
                            fieldId,
                            fieldType,
                            SerializeValue(currentValue, fieldType)
                        ));
                    }
                }
            }

            if (changedFields.Count == 0)
            {
                return null;
            }

            CurrentSequence++;

            return new MmStateDelta
            {
                DeltaId = CurrentSequence,
                BaselineSequence = AckedSequence,
                ChangedFields = changedFields,
                Timestamp = Time.timeAsDouble,
                IsFullSnapshot = false,
                NetId = NetId
            };
        }

        /// <summary>
        /// Create a full state snapshot (for new connections or resync).
        /// </summary>
        public MmStateDelta CreateFullSnapshot()
        {
            var allFields = new List<MmDeltaField>();

            foreach (var kvp in _currentState)
            {
                ushort fieldId = kvp.Key;
                if (_fieldTypes.TryGetValue(fieldId, out var fieldType))
                {
                    allFields.Add(new MmDeltaField(
                        fieldId,
                        fieldType,
                        SerializeValue(kvp.Value, fieldType)
                    ));
                }
            }

            CurrentSequence++;

            return new MmStateDelta
            {
                DeltaId = CurrentSequence,
                BaselineSequence = 0,
                ChangedFields = allFields,
                Timestamp = Time.timeAsDouble,
                IsFullSnapshot = true,
                NetId = NetId
            };
        }

        /// <summary>
        /// Mark current state as sent (update baseline for next delta).
        /// </summary>
        public void MarkAsSent()
        {
            _lastSentState.Clear();
            foreach (var kvp in _currentState)
            {
                _lastSentState[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Apply a delta to the current state.
        /// </summary>
        public void ApplyDelta(MmStateDelta delta)
        {
            foreach (var field in delta.ChangedFields)
            {
                _currentState[field.FieldId] = DeserializeValue(field.Value, field.FieldType);
            }
        }

        /// <summary>
        /// Get current value of a field.
        /// </summary>
        public bool TryGetFieldValue<T>(ushort fieldId, out T value)
        {
            if (_currentState.TryGetValue(fieldId, out object obj) && obj is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        private bool ValuesEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            // Special handling for Vector3 (floating point comparison)
            if (a is Vector3 va && b is Vector3 vb)
            {
                return Vector3.Distance(va, vb) < 0.001f;
            }

            // Special handling for Quaternion
            if (a is Quaternion qa && b is Quaternion qb)
            {
                return Mathf.Abs(Quaternion.Dot(qa, qb)) > 0.9999f;
            }

            return a.Equals(b);
        }

        private byte[] SerializeValue(object value, MmDeltaFieldType type)
        {
            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                switch (type)
                {
                    case MmDeltaFieldType.Bool:
                        writer.Write((bool)value);
                        break;
                    case MmDeltaFieldType.Int:
                        writer.Write((int)value);
                        break;
                    case MmDeltaFieldType.Float:
                        writer.Write((float)value);
                        break;
                    case MmDeltaFieldType.String:
                        writer.Write((string)value ?? "");
                        break;
                    case MmDeltaFieldType.Vector3:
                        var v3 = (Vector3)value;
                        writer.Write(v3.x);
                        writer.Write(v3.y);
                        writer.Write(v3.z);
                        break;
                    case MmDeltaFieldType.Vector4:
                        var v4 = (Vector4)value;
                        writer.Write(v4.x);
                        writer.Write(v4.y);
                        writer.Write(v4.z);
                        writer.Write(v4.w);
                        break;
                    case MmDeltaFieldType.Quaternion:
                        var q = (Quaternion)value;
                        writer.Write(q.x);
                        writer.Write(q.y);
                        writer.Write(q.z);
                        writer.Write(q.w);
                        break;
                    case MmDeltaFieldType.ByteArray:
                        var bytes = (byte[])value;
                        writer.Write(bytes.Length);
                        writer.Write(bytes);
                        break;
                    case MmDeltaFieldType.Transform:
                        var t = (MmTransform)value;
                        writer.Write(t.Translation.x);
                        writer.Write(t.Translation.y);
                        writer.Write(t.Translation.z);
                        writer.Write(t.Scale.x);
                        writer.Write(t.Scale.y);
                        writer.Write(t.Scale.z);
                        writer.Write(t.Rotation.x);
                        writer.Write(t.Rotation.y);
                        writer.Write(t.Rotation.z);
                        writer.Write(t.Rotation.w);
                        break;
                    default:
                        throw new ArgumentException($"Unknown field type: {type}");
                }
                return ms.ToArray();
            }
        }

        private object DeserializeValue(byte[] data, MmDeltaFieldType type)
        {
            using (var ms = new System.IO.MemoryStream(data))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                switch (type)
                {
                    case MmDeltaFieldType.Bool:
                        return reader.ReadBoolean();
                    case MmDeltaFieldType.Int:
                        return reader.ReadInt32();
                    case MmDeltaFieldType.Float:
                        return reader.ReadSingle();
                    case MmDeltaFieldType.String:
                        return reader.ReadString();
                    case MmDeltaFieldType.Vector3:
                        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    case MmDeltaFieldType.Vector4:
                        return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    case MmDeltaFieldType.Quaternion:
                        return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    case MmDeltaFieldType.ByteArray:
                        int len = reader.ReadInt32();
                        return reader.ReadBytes(len);
                    case MmDeltaFieldType.Transform:
                        return new MmTransform(
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                            new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                        );
                    default:
                        throw new ArgumentException($"Unknown field type: {type}");
                }
            }
        }
    }

    /// <summary>
    /// Manages state tracking for multiple networked objects.
    /// </summary>
    public class MmStateDeltaManager
    {
        private readonly Dictionary<uint, MmStateTracker> _trackers = new Dictionary<uint, MmStateTracker>();

        /// <summary>
        /// Get or create a state tracker for a network ID.
        /// </summary>
        public MmStateTracker GetOrCreateTracker(uint netId)
        {
            if (!_trackers.TryGetValue(netId, out var tracker))
            {
                tracker = new MmStateTracker(netId);
                _trackers[netId] = tracker;
            }
            return tracker;
        }

        /// <summary>
        /// Get an existing tracker, or null if not found.
        /// </summary>
        public MmStateTracker GetTracker(uint netId)
        {
            _trackers.TryGetValue(netId, out var tracker);
            return tracker;
        }

        /// <summary>
        /// Remove a tracker for a network ID.
        /// </summary>
        public void RemoveTracker(uint netId)
        {
            _trackers.Remove(netId);
        }

        /// <summary>
        /// Compute deltas for all tracked objects.
        /// </summary>
        /// <returns>List of deltas for objects that have changed</returns>
        public List<MmStateDelta> ComputeAllDeltas()
        {
            var deltas = new List<MmStateDelta>();

            foreach (var tracker in _trackers.Values)
            {
                var delta = tracker.ComputeDelta();
                if (delta != null)
                {
                    deltas.Add(delta);
                }
            }

            return deltas;
        }

        /// <summary>
        /// Mark all trackers as sent.
        /// </summary>
        public void MarkAllAsSent()
        {
            foreach (var tracker in _trackers.Values)
            {
                tracker.MarkAsSent();
            }
        }

        /// <summary>
        /// Process an acknowledgment from remote.
        /// </summary>
        public void ProcessAck(uint netId, uint sequence)
        {
            if (_trackers.TryGetValue(netId, out var tracker))
            {
                if (sequence > tracker.AckedSequence)
                {
                    tracker.AckedSequence = sequence;
                }
            }
        }

        /// <summary>
        /// Get count of tracked objects.
        /// </summary>
        public int TrackerCount => _trackers.Count;
    }
}
