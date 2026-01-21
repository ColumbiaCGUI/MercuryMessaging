// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmWriter.cs - Zero-allocation binary writer using ArrayPool
// Part of S2: Serialization System Overhaul

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// High-performance binary writer that uses ArrayPool for zero-allocation serialization.
    /// Implements IDisposable to return the buffer to the pool.
    ///
    /// Usage:
    /// <code>
    /// using (var writer = MmWriter.Get())
    /// {
    ///     writer.WriteInt(42);
    ///     writer.WriteString("Hello");
    ///     byte[] data = writer.ToArray(); // Only allocation is the final array
    /// }
    /// </code>
    /// </summary>
    public sealed class MmWriter : IDisposable
    {
        private const int DefaultCapacity = 256;
        private const int MaxCapacity = 1024 * 1024; // 1MB max

        private byte[] _buffer;
        private int _position;
        private bool _disposed;

        // Reusable UTF8 encoder to avoid allocations
        private static readonly Encoding Utf8 = Encoding.UTF8;

        #region Object Pool

        private static readonly ObjectPool<MmWriter> Pool = new ObjectPool<MmWriter>(
            () => new MmWriter(),
            writer => writer.Reset()
        );

        /// <summary>
        /// Get a writer from the pool. Must be disposed when done.
        /// </summary>
        public static MmWriter Get()
        {
            return Pool.Get();
        }

        /// <summary>
        /// Get a writer from the pool with initial capacity hint.
        /// </summary>
        public static MmWriter Get(int capacityHint)
        {
            var writer = Pool.Get();
            writer.EnsureCapacity(capacityHint);
            return writer;
        }

        #endregion

        private MmWriter()
        {
            _buffer = ArrayPool<byte>.Shared.Rent(DefaultCapacity);
            _position = 0;
            _disposed = false;
        }

        private void Reset()
        {
            _position = 0;
            _disposed = false;
            // Re-allocate buffer if it was returned to ArrayPool during Dispose
            if (_buffer == null)
            {
                _buffer = ArrayPool<byte>.Shared.Rent(DefaultCapacity);
            }
        }

        /// <summary>
        /// Current write position (number of bytes written).
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// Current buffer capacity.
        /// </summary>
        public int Capacity => _buffer.Length;

        #region Primitive Writers

        public void WriteByte(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
        }

        public void WriteSByte(sbyte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = (byte)value;
        }

        public void WriteBool(bool value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value ? (byte)1 : (byte)0;
        }

        public void WriteShort(short value)
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        public void WriteUShort(ushort value)
        {
            EnsureCapacity(2);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        public void WriteInt(int value)
        {
            EnsureCapacity(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        public void WriteUInt(uint value)
        {
            EnsureCapacity(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        public void WriteLong(long value)
        {
            EnsureCapacity(8);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        public void WriteULong(ulong value)
        {
            EnsureCapacity(8);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        public void WriteFloat(float value)
        {
            EnsureCapacity(4);
            // Use BitConverter for safe float-to-bytes conversion
            byte[] bytes = BitConverter.GetBytes(value);
            _buffer[_position++] = bytes[0];
            _buffer[_position++] = bytes[1];
            _buffer[_position++] = bytes[2];
            _buffer[_position++] = bytes[3];
        }

        public void WriteDouble(double value)
        {
            EnsureCapacity(8);
            // Use BitConverter for safe double-to-bytes conversion
            byte[] bytes = BitConverter.GetBytes(value);
            _buffer[_position++] = bytes[0];
            _buffer[_position++] = bytes[1];
            _buffer[_position++] = bytes[2];
            _buffer[_position++] = bytes[3];
            _buffer[_position++] = bytes[4];
            _buffer[_position++] = bytes[5];
            _buffer[_position++] = bytes[6];
            _buffer[_position++] = bytes[7];
        }

        #endregion

        #region String Writer

        /// <summary>
        /// Write a string with length prefix (ushort).
        /// Null strings are written as empty strings.
        /// </summary>
        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUShort(0);
                return;
            }

            // Get required byte count
            int byteCount = Utf8.GetByteCount(value);
            if (byteCount > ushort.MaxValue)
            {
                throw new ArgumentException($"String too long: {byteCount} bytes (max {ushort.MaxValue})");
            }

            WriteUShort((ushort)byteCount);
            EnsureCapacity(byteCount);

            // Encode directly into buffer
            Utf8.GetBytes(value, 0, value.Length, _buffer, _position);
            _position += byteCount;
        }

        #endregion

        #region Array Writers

        /// <summary>
        /// Write a byte array with length prefix (int).
        /// </summary>
        public void WriteBytes(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                WriteInt(0);
                return;
            }

            WriteInt(value.Length);
            EnsureCapacity(value.Length);
            Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
            _position += value.Length;
        }

        /// <summary>
        /// Write raw bytes without length prefix.
        /// </summary>
        public void WriteRawBytes(byte[] value, int offset, int count)
        {
            if (value == null || count == 0) return;

            EnsureCapacity(count);
            Buffer.BlockCopy(value, offset, _buffer, _position, count);
            _position += count;
        }

        #endregion

        #region Unity Type Writers

        public void WriteVector2(Vector2 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public void WriteVector4(Vector4 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public void WriteColor(Color value)
        {
            WriteFloat(value.r);
            WriteFloat(value.g);
            WriteFloat(value.b);
            WriteFloat(value.a);
        }

        public void WriteColor32(Color32 value)
        {
            WriteByte(value.r);
            WriteByte(value.g);
            WriteByte(value.b);
            WriteByte(value.a);
        }

        #endregion

        #region Output

        /// <summary>
        /// Get the written data as a new byte array.
        /// This is the only allocation in steady state.
        /// </summary>
        public byte[] ToArray()
        {
            byte[] result = new byte[_position];
            Buffer.BlockCopy(_buffer, 0, result, 0, _position);
            return result;
        }

        /// <summary>
        /// Get access to the internal buffer segment.
        /// Valid only until writer is disposed.
        /// </summary>
        public ArraySegment<byte> AsSegment()
        {
            return new ArraySegment<byte>(_buffer, 0, _position);
        }

        /// <summary>
        /// Copy written data to a destination array.
        /// </summary>
        public void CopyTo(byte[] destination, int destinationOffset)
        {
            Buffer.BlockCopy(_buffer, 0, destination, destinationOffset, _position);
        }

        #endregion

        #region Capacity Management

        private void EnsureCapacity(int additionalBytes)
        {
            int required = _position + additionalBytes;
            if (required <= _buffer.Length) return;

            // Calculate new size (double until sufficient)
            int newSize = _buffer.Length;
            while (newSize < required && newSize < MaxCapacity)
            {
                newSize *= 2;
            }

            if (newSize > MaxCapacity)
            {
                newSize = MaxCapacity;
            }

            if (required > newSize)
            {
                throw new InvalidOperationException($"MmWriter buffer overflow: required {required} bytes, max {MaxCapacity}");
            }

            // Rent new buffer and copy data
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _position);

            // Return old buffer to pool
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Return buffer to pool
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;

            // Return writer to object pool
            Pool.Return(this);
        }

        #endregion
    }

    /// <summary>
    /// Simple thread-safe object pool for MmWriter instances.
    /// </summary>
    internal class ObjectPool<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _reset;
        private readonly Stack<T> _pool;
        private readonly object _lock = new object();
        private const int MaxPoolSize = 16;

        public ObjectPool(Func<T> factory, Action<T> reset)
        {
            _factory = factory;
            _reset = reset;
            _pool = new Stack<T>();
        }

        public T Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    T item = _pool.Pop();
                    _reset(item);
                    return item;
                }
            }
            return _factory();
        }

        public void Return(T item)
        {
            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    _pool.Push(item);
                }
            }
        }
    }
}
