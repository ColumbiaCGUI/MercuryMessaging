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
using System;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with byte array payload
    /// </summary>
	public class MmMessageByteArray : MmMessage
    {
        /// <summary>
        /// Byte array payload
        /// </summary>
		public byte[] byteArr;

        /// <summary>
        /// Length of array
        /// </summary>
		public int length;

        /// <summary>
        /// Creates a basic MmMessageByteArray
        /// </summary>
		public MmMessageByteArray ()
		{}

        /// <summary>
        /// Creates a basic MmMessageByteArray, with control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageByteArray(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock, MmMessageType.MmByteArray)
        {}

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and byte array
        /// </summary>
        /// <param name="iVal">Byte array payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageByteArray(byte[] iVal, 
			MmMethod mmMethod = default(MmMethod), 
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, MmMessageType.MmByteArray, metadataBlock)
        {
			byteArr = iVal;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
		public MmMessageByteArray(MmMessageByteArray message) : base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
			MmMessageByteArray newMessage = new MmMessageByteArray (this);
            newMessage.length = length;
            newMessage.byteArr = new byte[byteArr.Length];
            byteArr.CopyTo(newMessage.byteArr, 0);

            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageByteArray
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageByteArray</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
		{
			int index = base.Deserialize(data);
            length = (int) data[index++];
            byteArr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byteArr[i] = (byte) data[index++];
            }
            return index;
		}

        /// <summary>
        /// Serialize the MmMessageByteArray
        /// </summary>
        /// <returns>Object array representation of a MmMessageByteArray</returns>
        /// <remarks>
        /// Optimized from O(n²) to O(n) by pre-allocating exact size instead of
        /// repeatedly calling Concat().ToArray() in a loop.
        /// </remarks>
        public override object[] Serialize()
		{
            object[] baseSerialized = base.Serialize();

            // Pre-allocate combined array: base + 1 (length) + byte array length
            // This fixes O(n²) complexity from the foreach + Concat pattern
            object[] result = new object[baseSerialized.Length + 1 + byteArr.Length];

            // Copy base data using Array.Copy (no LINQ)
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            // Fill length
            result[baseSerialized.Length] = byteArr.Length;

            // Fill byte array directly (no loop concatenation)
            int idx = baseSerialized.Length + 1;
            for (int i = 0; i < byteArr.Length; i++)
            {
                result[idx + i] = byteArr[i];
            }

            return result;
		}
    }
}