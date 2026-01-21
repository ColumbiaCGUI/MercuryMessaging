// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// IMmBinarySerializable.cs - Zero-allocation binary serialization interface
// Part of S1: Serialization System Overhaul

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Interface for types that can be serialized directly to binary format.
    /// Replaces the legacy IMmSerializable which used object[] (causing boxing allocations).
    ///
    /// Implementation Guidelines:
    /// 1. Write/Read fields in the SAME order
    /// 2. Use fixed-size types where possible (int, float, etc.)
    /// 3. For variable-length data, write length prefix first
    /// 4. Register type with MmTypeRegistry for polymorphic deserialization
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyData : IMmBinarySerializable
    /// {
    ///     public int Id;
    ///     public string Name;
    ///
    ///     public void WriteTo(MmWriter writer)
    ///     {
    ///         writer.WriteInt(Id);
    ///         writer.WriteString(Name);
    ///     }
    ///
    ///     public void ReadFrom(MmReader reader)
    ///     {
    ///         Id = reader.ReadInt();
    ///         Name = reader.ReadString();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IMmBinarySerializable
    {
        /// <summary>
        /// Serialize this object's data to the writer.
        /// Called during network transmission.
        /// </summary>
        /// <param name="writer">The pooled writer to write data to</param>
        void WriteTo(MmWriter writer);

        /// <summary>
        /// Deserialize this object's data from the reader.
        /// Called when receiving network data.
        /// </summary>
        /// <param name="reader">The reader containing the binary data</param>
        void ReadFrom(MmReader reader);
    }
}
