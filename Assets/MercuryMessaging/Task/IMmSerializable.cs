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
//  
//
namespace MercuryMessaging.Task
{
    /// <summary>
    /// Interface defining serializable structure
    /// to be used in IMmMessages.
    /// </summary>
    /// <remarks>
    /// OBSOLETE: Use IMmBinarySerializable from MercuryMessaging.Network instead.
    /// IMmBinarySerializable provides:
    /// - Zero-allocation serialization via MmWriter/MmReader
    /// - Direct binary format (no object[] boxing)
    /// - 3-5x performance improvement
    /// - Type registry for polymorphic deserialization
    ///
    /// Migration example:
    /// <code>
    /// // Old (IMmSerializable):
    /// public object[] Serialize() => new object[] { Id, Name };
    /// public int Deserialize(object[] data, int i) { Id = (int)data[i++]; Name = (string)data[i++]; return i; }
    ///
    /// // New (IMmBinarySerializable):
    /// public void WriteTo(MmWriter w) { w.WriteInt(Id); w.WriteString(Name); }
    /// public void ReadFrom(MmReader r) { Id = r.ReadInt(); Name = r.ReadString(); }
    /// </code>
    /// </remarks>
    [System.Obsolete("Use MercuryMessaging.Network.IMmBinarySerializable instead for zero-allocation serialization.")]
    public interface IMmSerializable
    {
        IMmSerializable Copy();

        /// <summary>
        /// Deserialize the IMmSerializable
        /// </summary>
        /// <param name="data">Object array representation of a IMmSerializable</param>
        /// <param name="index">The index of the next element to be read from data</param> 
        /// <returns>The index of the next element to be read from data</returns>
        int Deserialize(object[] data, int index);
        
        /// <summary>
        /// Serialize the IMmSerializable
        /// </summary>
        /// <returns>Object array representation of a IMmSerializable</returns>
        object[] Serialize();
    }
}
