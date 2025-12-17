// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmReader.cs - Zero-allocation binary reader
// Part of S3: Serialization System Overhaul

using System;
using System.Text;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// High-performance binary reader for deserialization.
    /// Operates directly on the source byte array without additional allocations.
    ///
    /// Usage:
    /// <code>
    /// var reader = new MmReader(data);
    /// int value = reader.ReadInt();
    /// string text = reader.ReadString();
    /// </code>
    /// </summary>
    public sealed class MmReader
    {
        private readonly byte[] _buffer;
        private readonly int _length;
        private int _position;

        // Reusable UTF8 decoder
        private static readonly Encoding Utf8 = Encoding.UTF8;

        /// <summary>
        /// Create a reader wrapping the given byte array.
        /// </summary>
        public MmReader(byte[] buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _length = buffer.Length;
            _position = 0;
        }

        /// <summary>
        /// Create a reader for a portion of a byte array.
        /// </summary>
        public MmReader(byte[] buffer, int offset, int count)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            _position = offset;
            _length = offset + count;
        }

        /// <summary>
        /// Current read position.
        /// </summary>
        public int Position => _position;

        /// <summary>
        /// Total length of readable data.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Number of bytes remaining to read.
        /// </summary>
        public int Remaining => _length - _position;

        /// <summary>
        /// Whether we've reached the end of the buffer.
        /// </summary>
        public bool IsAtEnd => _position >= _length;

        #region Primitive Readers

        public byte ReadByte()
        {
            EnsureAvailable(1);
            return _buffer[_position++];
        }

        public sbyte ReadSByte()
        {
            EnsureAvailable(1);
            return (sbyte)_buffer[_position++];
        }

        public bool ReadBool()
        {
            EnsureAvailable(1);
            return _buffer[_position++] != 0;
        }

        public short ReadShort()
        {
            EnsureAvailable(2);
            short value = (short)(_buffer[_position] | (_buffer[_position + 1] << 8));
            _position += 2;
            return value;
        }

        public ushort ReadUShort()
        {
            EnsureAvailable(2);
            ushort value = (ushort)(_buffer[_position] | (_buffer[_position + 1] << 8));
            _position += 2;
            return value;
        }

        public int ReadInt()
        {
            EnsureAvailable(4);
            int value = _buffer[_position]
                      | (_buffer[_position + 1] << 8)
                      | (_buffer[_position + 2] << 16)
                      | (_buffer[_position + 3] << 24);
            _position += 4;
            return value;
        }

        public uint ReadUInt()
        {
            EnsureAvailable(4);
            uint value = (uint)(_buffer[_position]
                       | (_buffer[_position + 1] << 8)
                       | (_buffer[_position + 2] << 16)
                       | (_buffer[_position + 3] << 24));
            _position += 4;
            return value;
        }

        public long ReadLong()
        {
            EnsureAvailable(8);
            uint lo = (uint)(_buffer[_position]
                    | (_buffer[_position + 1] << 8)
                    | (_buffer[_position + 2] << 16)
                    | (_buffer[_position + 3] << 24));
            uint hi = (uint)(_buffer[_position + 4]
                    | (_buffer[_position + 5] << 8)
                    | (_buffer[_position + 6] << 16)
                    | (_buffer[_position + 7] << 24));
            _position += 8;
            return (long)((ulong)hi << 32 | lo);
        }

        public ulong ReadULong()
        {
            EnsureAvailable(8);
            uint lo = (uint)(_buffer[_position]
                    | (_buffer[_position + 1] << 8)
                    | (_buffer[_position + 2] << 16)
                    | (_buffer[_position + 3] << 24));
            uint hi = (uint)(_buffer[_position + 4]
                    | (_buffer[_position + 5] << 8)
                    | (_buffer[_position + 6] << 16)
                    | (_buffer[_position + 7] << 24));
            _position += 8;
            return (ulong)hi << 32 | lo;
        }

        public float ReadFloat()
        {
            EnsureAvailable(4);
            // Use BitConverter for safe bytes-to-float conversion
            float value = BitConverter.ToSingle(_buffer, _position);
            _position += 4;
            return value;
        }

        public double ReadDouble()
        {
            EnsureAvailable(8);
            // Use BitConverter for safe bytes-to-double conversion
            double value = BitConverter.ToDouble(_buffer, _position);
            _position += 8;
            return value;
        }

        #endregion

        #region String Reader

        /// <summary>
        /// Read a string with length prefix (ushort).
        /// </summary>
        public string ReadString()
        {
            ushort length = ReadUShort();
            if (length == 0) return string.Empty;

            EnsureAvailable(length);
            string value = Utf8.GetString(_buffer, _position, length);
            _position += length;
            return value;
        }

        #endregion

        #region Array Readers

        /// <summary>
        /// Read a byte array with length prefix (int).
        /// </summary>
        public byte[] ReadBytes()
        {
            int length = ReadInt();
            if (length <= 0) return Array.Empty<byte>();

            EnsureAvailable(length);
            byte[] result = new byte[length];
            Buffer.BlockCopy(_buffer, _position, result, 0, length);
            _position += length;
            return result;
        }

        /// <summary>
        /// Read raw bytes without length prefix.
        /// </summary>
        public void ReadRawBytes(byte[] destination, int destinationOffset, int count)
        {
            EnsureAvailable(count);
            Buffer.BlockCopy(_buffer, _position, destination, destinationOffset, count);
            _position += count;
        }

        /// <summary>
        /// Skip a number of bytes without reading them.
        /// </summary>
        public void Skip(int count)
        {
            EnsureAvailable(count);
            _position += count;
        }

        #endregion

        #region Unity Type Readers

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Color ReadColor()
        {
            return new Color(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Color32 ReadColor32()
        {
            return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
        }

        #endregion

        #region Validation

        private void EnsureAvailable(int count)
        {
            if (_position + count > _length)
            {
                throw new InvalidOperationException(
                    $"MmReader buffer underflow: attempted to read {count} bytes at position {_position}, " +
                    $"but only {_length - _position} bytes remaining");
            }
        }

        /// <summary>
        /// Reset the reader position to the start.
        /// </summary>
        public void Reset()
        {
            _position = 0;
        }

        /// <summary>
        /// Seek to a specific position.
        /// </summary>
        public void Seek(int position)
        {
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(position),
                    $"Position {position} is outside valid range [0, {_length}]");
            }
            _position = position;
        }

        #endregion
    }
}
