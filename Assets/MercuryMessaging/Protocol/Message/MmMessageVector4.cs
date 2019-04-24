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
using UnityEngine;
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with Vector4 payload
    /// </summary>
	public class MmMessageVector4 : MmMessage
    {
        /// <summary>
        /// Vector4 payload
        /// </summary>
		public Vector4 value;

        /// <summary>
        /// Creates a basic MmMessageVector4
        /// </summary>
		public MmMessageVector4()
		{
		}

        /// <summary>
        /// Creates a basic MmMessageVector4, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageVector4(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock)
		{
        }

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and a Vector4
        /// </summary>
        /// <param name="iVal">Vector4 payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageVector4(Vector4 iVal, 
			MmMethod mmMethod = default(MmMethod),
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, metadataBlock)
        {
			value = iVal;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageVector4(MmMessageVector4 message) : 
            base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
			MmMessageVector4 newMessage = new MmMessageVector4 (this);
            newMessage.value = value;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="reader">UNET based deserializer object</param>
        public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize (reader);
			value = reader.ReadVector4();
		}

        /// <summary>
        /// Serialize the MmMessage
        /// </summary>
        /// <param name="writer">UNET based serializer</param>
        public override void Serialize(NetworkWriter writer)
		{
			base.Serialize (writer);
			writer.Write (value);
		}
    }
}