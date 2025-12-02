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
// Carmine Elvezio, Mengu Sukan, Steven Feiner, [Contributors]
// =============================================================
//

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Efficient binary serializer for MercuryMessaging messages.
    ///
    /// Converts MmMessage objects to compact byte arrays for network transport.
    /// This replaces the previous object[] serialization which was inefficient
    /// and not portable across different networking backends.
    ///
    /// Binary Format:
    /// - Header (15 bytes fixed):
    ///   - Magic: 4 bytes "MMSG"
    ///   - Version: 1 byte
    ///   - MessageType: 2 bytes (short)
    ///   - MmMethod: 2 bytes (short)
    ///   - NetId: 4 bytes (uint)
    ///   - Metadata: 2 bytes (packed filters + tag)
    /// - Payload: Variable length, type-specific
    /// </summary>
    public static class MmBinarySerializer
    {
        /// <summary>
        /// Magic number for message validation: "MMSG"
        /// </summary>
        private static readonly byte[] MagicNumber = { 0x4D, 0x4D, 0x53, 0x47 };

        /// <summary>
        /// Current serialization format version.
        /// Increment when making breaking changes to the format.
        /// </summary>
        public const byte FormatVersion = 1;

        /// <summary>
        /// Fixed header size in bytes.
        /// </summary>
        public const int HeaderSize = 15;

        #region Serialization

        /// <summary>
        /// Serialize an MmMessage to a byte array.
        /// </summary>
        /// <param name="message">The message to serialize</param>
        /// <returns>Byte array representation of the message</returns>
        public static byte[] Serialize(MmMessage message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Write header
                writer.Write(MagicNumber);
                writer.Write(FormatVersion);
                writer.Write((short)message.MmMessageType);
                writer.Write((short)message.MmMethod);
                writer.Write(message.NetId);
                writer.Write(PackMetadata(message.MetadataBlock));

                // Write type-specific payload
                WritePayload(writer, message);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Pack metadata block into 2 bytes.
        /// Byte 0: Level(4 bits) | Active(2 bits) | Selected(2 bits)
        /// Byte 1: Network(2 bits) | Tag high bits(6 bits reserved)
        /// </summary>
        private static ushort PackMetadata(MmMetadataBlock metadata)
        {
            int packed = 0;
            packed |= ((int)metadata.LevelFilter & 0x0F);
            packed |= ((int)metadata.ActiveFilter & 0x03) << 4;
            packed |= ((int)metadata.SelectedFilter & 0x03) << 6;
            packed |= ((int)metadata.NetworkFilter & 0x03) << 8;
            // Tag uses remaining bits (supports Tag0-Tag7)
            packed |= ((int)metadata.Tag & 0x3F) << 10;
            return (ushort)packed;
        }

        /// <summary>
        /// Write type-specific payload data.
        /// </summary>
        private static void WritePayload(BinaryWriter writer, MmMessage message)
        {
            switch (message.MmMessageType)
            {
                case MmMessageType.MmVoid:
                    // No payload for void messages
                    break;

                case MmMessageType.MmInt:
                    writer.Write(((MmMessageInt)message).value);
                    break;

                case MmMessageType.MmBool:
                    writer.Write(((MmMessageBool)message).value);
                    break;

                case MmMessageType.MmFloat:
                    writer.Write(((MmMessageFloat)message).value);
                    break;

                case MmMessageType.MmString:
                    WriteString(writer, ((MmMessageString)message).value);
                    break;

                case MmMessageType.MmVector3:
                    WriteVector3(writer, ((MmMessageVector3)message).value);
                    break;

                case MmMessageType.MmVector4:
                    WriteVector4(writer, ((MmMessageVector4)message).value);
                    break;

                case MmMessageType.MmQuaternion:
                    WriteQuaternion(writer, ((MmMessageQuaternion)message).value);
                    break;

                case MmMessageType.MmByteArray:
                    WriteByteArray(writer, ((MmMessageByteArray)message).byteArr);
                    break;

                case MmMessageType.MmTransform:
                    WriteTransformData(writer, (MmMessageTransform)message);
                    break;

                case MmMessageType.MmTransformList:
                    WriteTransformListData(writer, (MmMessageTransformList)message);
                    break;

                case MmMessageType.MmGameObject:
                    WriteGameObjectData(writer, (MmMessageGameObject)message);
                    break;

                case MmMessageType.MmSerializable:
                    WriteSerializableData(writer, (MmMessageSerializable)message);
                    break;

                default:
                    throw new ArgumentException($"Unknown message type: {message.MmMessageType}");
            }
        }

        #endregion

        #region Deserialization

        /// <summary>
        /// Deserialize a byte array to an MmMessage.
        /// </summary>
        /// <param name="data">The byte array to deserialize</param>
        /// <returns>Deserialized MmMessage</returns>
        public static MmMessage Deserialize(byte[] data)
        {
            if (data == null || data.Length < HeaderSize)
            {
                throw new ArgumentException("Invalid message data: too short");
            }

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // Validate magic number
                byte[] magic = reader.ReadBytes(4);
                if (!ValidateMagic(magic))
                {
                    throw new InvalidDataException("Invalid message: bad magic number");
                }

                // Read version
                byte version = reader.ReadByte();
                if (version > FormatVersion)
                {
                    throw new InvalidDataException($"Unsupported message version: {version}");
                }

                // Read header
                var messageType = (MmMessageType)reader.ReadInt16();
                var method = (MmMethod)reader.ReadInt16();
                uint netId = reader.ReadUInt32();
                ushort packedMetadata = reader.ReadUInt16();
                var metadata = UnpackMetadata(packedMetadata);

                // Create and populate message
                MmMessage message = CreateMessage(messageType, metadata);
                message.MmMethod = method;
                message.NetId = netId;

                // Read type-specific payload
                ReadPayload(reader, message);

                return message;
            }
        }

        /// <summary>
        /// Validate the magic number.
        /// </summary>
        private static bool ValidateMagic(byte[] magic)
        {
            if (magic.Length != 4) return false;
            for (int i = 0; i < 4; i++)
            {
                if (magic[i] != MagicNumber[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Unpack metadata from 2 bytes.
        /// </summary>
        private static MmMetadataBlock UnpackMetadata(ushort packed)
        {
            var level = (MmLevelFilter)(packed & 0x0F);
            var active = (MmActiveFilter)((packed >> 4) & 0x03);
            var selected = (MmSelectedFilter)((packed >> 6) & 0x03);
            var network = (MmNetworkFilter)((packed >> 8) & 0x03);
            var tag = (MmTag)((packed >> 10) & 0x3F);

            return new MmMetadataBlock(tag, level, active, selected, network);
        }

        /// <summary>
        /// Create an empty message of the appropriate type.
        /// </summary>
        private static MmMessage CreateMessage(MmMessageType type, MmMetadataBlock metadata)
        {
            switch (type)
            {
                case MmMessageType.MmVoid:
                    return new MmMessage(metadata, type);
                case MmMessageType.MmInt:
                    return new MmMessageInt(metadata);
                case MmMessageType.MmBool:
                    return new MmMessageBool(metadata);
                case MmMessageType.MmFloat:
                    return new MmMessageFloat(metadata);
                case MmMessageType.MmString:
                    return new MmMessageString(metadata);
                case MmMessageType.MmVector3:
                    return new MmMessageVector3(metadata);
                case MmMessageType.MmVector4:
                    return new MmMessageVector4(metadata);
                case MmMessageType.MmQuaternion:
                    return new MmMessageQuaternion(metadata);
                case MmMessageType.MmByteArray:
                    return new MmMessageByteArray(metadata);
                case MmMessageType.MmTransform:
                    return new MmMessageTransform(metadata);
                case MmMessageType.MmTransformList:
                    return new MmMessageTransformList(metadata);
                case MmMessageType.MmGameObject:
                    return new MmMessageGameObject(metadata);
                case MmMessageType.MmSerializable:
                    return new MmMessageSerializable(metadata);
                default:
                    throw new ArgumentException($"Unknown message type: {type}");
            }
        }

        /// <summary>
        /// Read type-specific payload data.
        /// </summary>
        private static void ReadPayload(BinaryReader reader, MmMessage message)
        {
            switch (message.MmMessageType)
            {
                case MmMessageType.MmVoid:
                    // No payload
                    break;

                case MmMessageType.MmInt:
                    ((MmMessageInt)message).value = reader.ReadInt32();
                    break;

                case MmMessageType.MmBool:
                    ((MmMessageBool)message).value = reader.ReadBoolean();
                    break;

                case MmMessageType.MmFloat:
                    ((MmMessageFloat)message).value = reader.ReadSingle();
                    break;

                case MmMessageType.MmString:
                    ((MmMessageString)message).value = ReadString(reader);
                    break;

                case MmMessageType.MmVector3:
                    ((MmMessageVector3)message).value = ReadVector3(reader);
                    break;

                case MmMessageType.MmVector4:
                    ((MmMessageVector4)message).value = ReadVector4(reader);
                    break;

                case MmMessageType.MmQuaternion:
                    ((MmMessageQuaternion)message).value = ReadQuaternion(reader);
                    break;

                case MmMessageType.MmByteArray:
                    ((MmMessageByteArray)message).byteArr = ReadByteArray(reader);
                    break;

                case MmMessageType.MmTransform:
                    ReadTransformData(reader, (MmMessageTransform)message);
                    break;

                case MmMessageType.MmTransformList:
                    ReadTransformListData(reader, (MmMessageTransformList)message);
                    break;

                case MmMessageType.MmGameObject:
                    ReadGameObjectData(reader, (MmMessageGameObject)message);
                    break;

                case MmMessageType.MmSerializable:
                    ReadSerializableData(reader, (MmMessageSerializable)message);
                    break;
            }
        }

        #endregion

        #region Type Writers

        private static void WriteString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write((ushort)0);
                return;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static void WriteVector3(BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        private static void WriteVector4(BinaryWriter writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        private static void WriteQuaternion(BinaryWriter writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        private static void WriteByteArray(BinaryWriter writer, byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                writer.Write(0);
                return;
            }
            writer.Write(value.Length);
            writer.Write(value);
        }

        private static void WriteTransformData(BinaryWriter writer, MmMessageTransform msg)
        {
            WriteVector3(writer, msg.MmTransform.Translation);
            WriteQuaternion(writer, msg.MmTransform.Rotation);
            WriteVector3(writer, msg.MmTransform.Scale);
            writer.Write(msg.LocalTransform);
        }

        private static void WriteTransformListData(BinaryWriter writer, MmMessageTransformList msg)
        {
            int count = msg.transforms?.Count ?? 0;
            writer.Write(count);
            if (msg.transforms != null)
            {
                foreach (var t in msg.transforms)
                {
                    WriteVector3(writer, t.Translation);
                    WriteQuaternion(writer, t.Rotation);
                    WriteVector3(writer, t.Scale);
                }
            }
        }

        private static void WriteGameObjectData(BinaryWriter writer, MmMessageGameObject msg)
        {
            // Write the network ID (GameObject reference will be resolved by IMmGameObjectResolver)
            writer.Write(msg.GameObjectNetId);
        }

        private static void WriteSerializableData(BinaryWriter writer, MmMessageSerializable msg)
        {
            // Write the type name for deserialization
            string typeName = msg.value?.GetType().AssemblyQualifiedName ?? "";
            WriteString(writer, typeName);

            if (msg.value is MercuryMessaging.Task.IMmSerializable serializable)
            {
                // Use the proper IMmSerializable interface
                object[] data = serializable.Serialize();
                writer.Write(data.Length);
                foreach (var obj in data)
                {
                    WriteObjectValue(writer, obj);
                }
            }
            else if (msg.value != null)
            {
                // Fallback for non-IMmSerializable types - log warning
                Debug.LogWarning($"MmBinarySerializer: Type {typeName} does not implement IMmSerializable. Serialization may fail.");
                writer.Write(0); // Empty array
            }
            else
            {
                writer.Write(0); // Null value - empty array
            }
        }

        /// <summary>
        /// Write an object value to the binary stream with type information.
        /// </summary>
        private static void WriteObjectValue(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write((byte)0); // Null type marker
                return;
            }

            switch (value)
            {
                case int i:
                    writer.Write((byte)1);
                    writer.Write(i);
                    break;
                case bool b:
                    writer.Write((byte)2);
                    writer.Write(b);
                    break;
                case float f:
                    writer.Write((byte)3);
                    writer.Write(f);
                    break;
                case string s:
                    writer.Write((byte)4);
                    WriteString(writer, s);
                    break;
                case double d:
                    writer.Write((byte)5);
                    writer.Write(d);
                    break;
                case long l:
                    writer.Write((byte)6);
                    writer.Write(l);
                    break;
                case byte by:
                    writer.Write((byte)7);
                    writer.Write(by);
                    break;
                case short sh:
                    writer.Write((byte)8);
                    writer.Write(sh);
                    break;
                default:
                    Debug.LogWarning($"MmBinarySerializer: Unsupported object type in IMmSerializable: {value.GetType().Name}");
                    writer.Write((byte)0); // Treat as null
                    break;
            }
        }

        #endregion

        #region Type Readers

        private static string ReadString(BinaryReader reader)
        {
            ushort length = reader.ReadUInt16();
            if (length == 0) return string.Empty;
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );
        }

        private static Vector4 ReadVector4(BinaryReader reader)
        {
            return new Vector4(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );
        }

        private static Quaternion ReadQuaternion(BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );
        }

        private static byte[] ReadByteArray(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length <= 0) return Array.Empty<byte>();
            return reader.ReadBytes(length);
        }

        private static void ReadTransformData(BinaryReader reader, MmMessageTransform msg)
        {
            // Read in same order as WriteTransformData: Translation, Rotation, Scale
            var translation = ReadVector3(reader);
            var rotation = ReadQuaternion(reader);
            var scale = ReadVector3(reader);
            msg.MmTransform = new MmTransform(translation, scale, rotation);
            msg.LocalTransform = reader.ReadBoolean();
        }

        private static void ReadTransformListData(BinaryReader reader, MmMessageTransformList msg)
        {
            int count = reader.ReadInt32();
            msg.transforms = new System.Collections.Generic.List<MmTransform>(count);
            for (int i = 0; i < count; i++)
            {
                var translation = ReadVector3(reader);
                var rotation = ReadQuaternion(reader);
                var scale = ReadVector3(reader);
                msg.transforms.Add(new MmTransform(translation, scale, rotation));
            }
        }

        private static void ReadGameObjectData(BinaryReader reader, MmMessageGameObject msg)
        {
            msg.GameObjectNetId = reader.ReadUInt32();
            // Note: Actual GameObject resolution happens in MmNetworkBridge using IMmGameObjectResolver
        }

        private static void ReadSerializableData(BinaryReader reader, MmMessageSerializable msg)
        {
            string typeName = ReadString(reader);
            int dataLength = reader.ReadInt32();

            if (string.IsNullOrEmpty(typeName))
            {
                return; // No type, nothing to deserialize
            }

            // Read the object array
            object[] data = new object[dataLength];
            for (int i = 0; i < dataLength; i++)
            {
                data[i] = ReadObjectValue(reader);
            }

            try
            {
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    Debug.LogError($"MmBinarySerializer: Cannot find type '{typeName}' for deserialization");
                    return;
                }

                var obj = Activator.CreateInstance(type);
                if (obj is MercuryMessaging.Task.IMmSerializable serializable)
                {
                    // Call Deserialize with the object array
                    serializable.Deserialize(data, 0);
                    msg.value = serializable;
                }
                else
                {
                    Debug.LogError($"MmBinarySerializer: Type '{typeName}' does not implement IMmSerializable");
                }
            }
            catch (Exception e)
            {
                // FAIL-FAST: Log full exception, not just message
                Debug.LogException(e);
                throw new InvalidOperationException($"Failed to deserialize MmMessageSerializable of type '{typeName}'", e);
            }
        }

        /// <summary>
        /// Read an object value from the binary stream with type information.
        /// </summary>
        private static object ReadObjectValue(BinaryReader reader)
        {
            byte typeMarker = reader.ReadByte();
            switch (typeMarker)
            {
                case 0: return null;
                case 1: return reader.ReadInt32();
                case 2: return reader.ReadBoolean();
                case 3: return reader.ReadSingle();
                case 4: return ReadString(reader);
                case 5: return reader.ReadDouble();
                case 6: return reader.ReadInt64();
                case 7: return reader.ReadByte();
                case 8: return reader.ReadInt16();
                default:
                    Debug.LogWarning($"MmBinarySerializer: Unknown type marker {typeMarker} during deserialization");
                    return null;
            }
        }

        #endregion
    }
}
