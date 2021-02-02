// Copyright (c) 2017-2019, Columbia University
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
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System.Linq;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with string payload
    /// </summary>
	public class MmMessageString : MmMessage
    {
        /// <summary>
        /// String payload
        /// </summary>
		public string value;

        /// <summary>
        /// Creates a basic MmMessageString
        /// </summary>
		public MmMessageString()
		{}

        /// <summary>
        /// Creates a basic MmMessageString, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageString(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock, MmMessageType.MmString)
		{
		}

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and a string
        /// </summary>
        /// <param name="initVal">String payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageString(string initVal, 
			MmMethod mmMethod = default(MmMethod), 
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, MmMessageType.MmString, metadataBlock)
        {
			value = initVal;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageString(MmMessageString message) : base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
			MmMessageString newMessage = new MmMessageString (this);
            newMessage.value = value;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageString
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageString</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            value = (string) data[index++];
            return index;
        }

        /// <summary>
        /// Serialize the MmMessageString
        /// </summary>
        /// <returns>Object array representation of a MmMessageString</returns>
        public override object[] Serialize()
        {
            object[] baseSerialized = base.Serialize();
            object[] thisSerialized = new object[] { value };
            object[] combinedSerialized = baseSerialized.Concat(thisSerialized).ToArray();
            return combinedSerialized;
        }
    }
}