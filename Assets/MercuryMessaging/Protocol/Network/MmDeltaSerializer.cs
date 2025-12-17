// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmDeltaSerializer.cs - Serialization for delta compression messages
// Efficiently serializes and deserializes MmStateDelta objects for network transmission

using System;
using System.Collections.Generic;
using System.IO;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Serializes and deserializes MmStateDelta objects for network transmission.
    /// Uses compact binary format to minimize bandwidth.
    /// </summary>
    public static class MmDeltaSerializer
    {
        /// <summary>
        /// Magic header for delta packets to identify packet type.
        /// </summary>
        public const ushort DeltaPacketHeader = 0xDD01;

        /// <summary>
        /// Serialize a single delta to bytes.
        /// </summary>
        public static byte[] Serialize(MmStateDelta delta)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                WriteDelta(writer, delta);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serialize multiple deltas to a single packet (batching).
        /// </summary>
        public static byte[] SerializeBatch(IList<MmStateDelta> deltas)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write header
                writer.Write(DeltaPacketHeader);

                // Write count
                writer.Write((ushort)deltas.Count);

                // Write each delta
                foreach (var delta in deltas)
                {
                    WriteDelta(writer, delta);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize a single delta from bytes.
        /// </summary>
        public static MmStateDelta Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                return ReadDelta(reader);
            }
        }

        /// <summary>
        /// Deserialize a batch of deltas from bytes.
        /// </summary>
        public static List<MmStateDelta> DeserializeBatch(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                // Read and verify header
                ushort header = reader.ReadUInt16();
                if (header != DeltaPacketHeader)
                {
                    throw new InvalidDataException($"Invalid delta packet header: 0x{header:X4}");
                }

                // Read count
                ushort count = reader.ReadUInt16();

                // Read deltas
                var deltas = new List<MmStateDelta>(count);
                for (int i = 0; i < count; i++)
                {
                    deltas.Add(ReadDelta(reader));
                }

                return deltas;
            }
        }

        /// <summary>
        /// Check if data is a delta batch packet.
        /// </summary>
        public static bool IsDeltaBatchPacket(byte[] data)
        {
            if (data == null || data.Length < 2) return false;
            ushort header = (ushort)(data[0] | (data[1] << 8));
            return header == DeltaPacketHeader;
        }

        private static void WriteDelta(BinaryWriter writer, MmStateDelta delta)
        {
            // Write delta header
            writer.Write(delta.DeltaId);
            writer.Write(delta.BaselineSequence);
            writer.Write(delta.NetId);
            writer.Write(delta.Timestamp);
            writer.Write(delta.IsFullSnapshot);

            // Write field count
            writer.Write((ushort)delta.ChangedFields.Count);

            // Write each field
            foreach (var field in delta.ChangedFields)
            {
                WriteField(writer, field);
            }
        }

        private static MmStateDelta ReadDelta(BinaryReader reader)
        {
            var delta = new MmStateDelta
            {
                DeltaId = reader.ReadUInt32(),
                BaselineSequence = reader.ReadUInt32(),
                NetId = reader.ReadUInt32(),
                Timestamp = reader.ReadDouble(),
                IsFullSnapshot = reader.ReadBoolean()
            };

            // Read field count
            ushort fieldCount = reader.ReadUInt16();

            // Read each field
            delta.ChangedFields = new List<MmDeltaField>(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                delta.ChangedFields.Add(ReadField(reader));
            }

            return delta;
        }

        private static void WriteField(BinaryWriter writer, MmDeltaField field)
        {
            writer.Write(field.FieldId);
            writer.Write((byte)field.FieldType);
            writer.Write((ushort)field.Value.Length);
            writer.Write(field.Value);
        }

        private static MmDeltaField ReadField(BinaryReader reader)
        {
            ushort fieldId = reader.ReadUInt16();
            MmDeltaFieldType fieldType = (MmDeltaFieldType)reader.ReadByte();
            ushort valueLength = reader.ReadUInt16();
            byte[] value = reader.ReadBytes(valueLength);

            return new MmDeltaField(fieldId, fieldType, value);
        }
    }

    /// <summary>
    /// Message priority levels for the priority queue system.
    /// Higher priority messages are sent first.
    /// </summary>
    public enum MmMessagePriority : byte
    {
        /// <summary>
        /// Low priority - can be dropped under congestion.
        /// Use for non-critical state updates.
        /// </summary>
        Low = 0,

        /// <summary>
        /// Normal priority - default for most messages.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// High priority - sent before normal messages.
        /// Use for important gameplay events.
        /// </summary>
        High = 2,

        /// <summary>
        /// Critical priority - must be delivered.
        /// Use for game-critical events like damage, death.
        /// </summary>
        Critical = 3
    }

    /// <summary>
    /// Queued message entry for priority-based sending.
    /// </summary>
    public struct MmQueuedMessage : IComparable<MmQueuedMessage>
    {
        /// <summary>
        /// Message data to send.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Priority level.
        /// </summary>
        public MmMessagePriority Priority;

        /// <summary>
        /// Timestamp when message was queued.
        /// </summary>
        public double QueuedTime;

        /// <summary>
        /// Target client ID (-1 for broadcast, 0 for server).
        /// </summary>
        public int TargetClient;

        /// <summary>
        /// Reliability level for this message.
        /// </summary>
        public MmReliability Reliability;

        /// <summary>
        /// Compare for priority queue ordering.
        /// Higher priority comes first, then earlier queued time.
        /// </summary>
        public int CompareTo(MmQueuedMessage other)
        {
            // Higher priority first
            int priorityCompare = other.Priority.CompareTo(Priority);
            if (priorityCompare != 0) return priorityCompare;

            // Earlier time first (FIFO within same priority)
            return QueuedTime.CompareTo(other.QueuedTime);
        }
    }

    /// <summary>
    /// Priority queue for outgoing messages.
    /// Ensures high-priority messages are sent first under bandwidth constraints.
    /// </summary>
    public class MmMessagePriorityQueue
    {
        private readonly List<MmQueuedMessage> _heap = new List<MmQueuedMessage>();
        private readonly object _lock = new object();

        /// <summary>
        /// Number of messages in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _heap.Count;
                }
            }
        }

        /// <summary>
        /// Total bytes of all queued messages.
        /// </summary>
        public int TotalBytes
        {
            get
            {
                lock (_lock)
                {
                    int total = 0;
                    foreach (var msg in _heap)
                    {
                        total += msg.Data.Length;
                    }
                    return total;
                }
            }
        }

        /// <summary>
        /// Enqueue a message with priority.
        /// </summary>
        public void Enqueue(byte[] data, MmMessagePriority priority, int targetClient, MmReliability reliability, double currentTime)
        {
            var msg = new MmQueuedMessage
            {
                Data = data,
                Priority = priority,
                QueuedTime = currentTime,
                TargetClient = targetClient,
                Reliability = reliability
            };

            lock (_lock)
            {
                _heap.Add(msg);
                HeapifyUp(_heap.Count - 1);
            }
        }

        /// <summary>
        /// Dequeue the highest priority message.
        /// </summary>
        public bool TryDequeue(out MmQueuedMessage message)
        {
            lock (_lock)
            {
                if (_heap.Count == 0)
                {
                    message = default;
                    return false;
                }

                message = _heap[0];

                // Move last element to top and heapify down
                int lastIdx = _heap.Count - 1;
                _heap[0] = _heap[lastIdx];
                _heap.RemoveAt(lastIdx);

                if (_heap.Count > 0)
                {
                    HeapifyDown(0);
                }

                return true;
            }
        }

        /// <summary>
        /// Peek at the highest priority message without removing.
        /// </summary>
        public bool TryPeek(out MmQueuedMessage message)
        {
            lock (_lock)
            {
                if (_heap.Count == 0)
                {
                    message = default;
                    return false;
                }

                message = _heap[0];
                return true;
            }
        }

        /// <summary>
        /// Dequeue multiple messages up to a byte limit.
        /// </summary>
        public List<MmQueuedMessage> DequeueBatch(int maxBytes)
        {
            var result = new List<MmQueuedMessage>();
            int totalBytes = 0;

            lock (_lock)
            {
                while (_heap.Count > 0)
                {
                    var peek = _heap[0];
                    if (totalBytes + peek.Data.Length > maxBytes && result.Count > 0)
                    {
                        break; // Would exceed limit, stop here
                    }

                    if (TryDequeue(out var msg))
                    {
                        result.Add(msg);
                        totalBytes += msg.Data.Length;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Drop low priority messages to free up queue space.
        /// </summary>
        public int DropLowPriority(int maxToKeep)
        {
            int dropped = 0;

            lock (_lock)
            {
                if (_heap.Count <= maxToKeep) return 0;

                // Rebuild heap keeping only higher priority messages
                var toKeep = new List<MmQueuedMessage>();
                foreach (var msg in _heap)
                {
                    if (msg.Priority >= MmMessagePriority.Normal || toKeep.Count < maxToKeep)
                    {
                        toKeep.Add(msg);
                    }
                    else
                    {
                        dropped++;
                    }
                }

                // Rebuild heap
                _heap.Clear();
                foreach (var msg in toKeep)
                {
                    _heap.Add(msg);
                    HeapifyUp(_heap.Count - 1);
                }
            }

            return dropped;
        }

        /// <summary>
        /// Clear all messages from the queue.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _heap.Clear();
            }
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIdx = (index - 1) / 2;
                if (_heap[index].CompareTo(_heap[parentIdx]) >= 0)
                {
                    break;
                }

                // Swap
                var temp = _heap[index];
                _heap[index] = _heap[parentIdx];
                _heap[parentIdx] = temp;

                index = parentIdx;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIdx = _heap.Count - 1;

            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild <= lastIdx && _heap[leftChild].CompareTo(_heap[smallest]) < 0)
                {
                    smallest = leftChild;
                }

                if (rightChild <= lastIdx && _heap[rightChild].CompareTo(_heap[smallest]) < 0)
                {
                    smallest = rightChild;
                }

                if (smallest == index)
                {
                    break;
                }

                // Swap
                var temp = _heap[index];
                _heap[index] = _heap[smallest];
                _heap[smallest] = temp;

                index = smallest;
            }
        }
    }
}
