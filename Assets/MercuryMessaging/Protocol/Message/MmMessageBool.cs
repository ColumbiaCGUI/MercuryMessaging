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
    /// MmMessage with bool payload
    /// </summary>
    public class MmMessageBool : MmMessage
    {
        /// <summary>
        /// Boolean payload
        /// </summary>
        public bool value;

        /// <summary>
        /// Creates a basic MmMessageBool
        /// </summary>
		public MmMessageBool()
		{}

        /// <summary>
        /// Creates a basic MmMessage, with control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageBool(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock, MmMessageType.MmBool)
		{}

        /// <summary>
        /// Create an MmMessage, with defined control block, MmMethod, and boolean value
        /// </summary>
        /// <param name="iVal">Boolean value</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageBool(bool iVal, 
			MmMethod mmMethod = default(MmMethod), 
            MmMetadataBlock metadataBlock = null)
			: base (mmMethod, MmMessageType.MmBool, metadataBlock)
		{
			value = iVal;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
		public MmMessageBool(MmMessage message) : base (message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Deep copy of message</returns>
        public override MmMessage Copy()
        {
			//Todo: Change here.
			MmMessageBool newMessage = new MmMessageBool(this);
            newMessage.value = value;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageBool
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageBool</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
		{
			int index = base.Deserialize(data);
            value = (bool) data[index++];
            return index;
		}

        /// <summary>
        /// Serialize the MmMessageBool
        /// </summary>
        /// <returns>Object array representation of a MmMessageBool</returns>
		public override object[] Serialize()
		{
			object[] baseSerialized = base.Serialize();

            // Pre-allocate combined array: base + 1 payload
            object[] result = new object[baseSerialized.Length + 1];

            // Copy base data using Array.Copy (no LINQ)
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            // Fill payload directly
            result[baseSerialized.Length] = value;

            return result;
		}
    }
}