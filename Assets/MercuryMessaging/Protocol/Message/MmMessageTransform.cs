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
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with MmTransform payload
    /// </summary>
	public class MmMessageTransform : MmMessage
	{
        /// <summary>
        /// MmTransform payload
        /// </summary>
		public MmTransform MmTransform;

        /// <summary>
        /// Represents whether to use local or global transformation.
        /// </summary>
        public bool LocalTransform;

        /// <summary>
        /// Creates a basic MmMessageTransform
        /// </summary>
		public MmMessageTransform()
		{}

        /// <summary>
        /// Creates a basic MmMessageTransform, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageTransform(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock)
		{
			MmTransform = new MmTransform();
		}

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and an MmTransform
        /// </summary>
        /// <param name="transform">MmTransform payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageTransform(MmTransform transform,
            MmMethod mmMethod = default(MmMethod),
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, metadataBlock)
        {
            MmTransform = transform;
        }

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and an MmTransform
        /// </summary>
        /// <param name="transform">MmTransform payload</param>
        /// <param name="globalTransform">Should message recipient apply to global or local transform?</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageTransform(MmTransform transform,
            bool localTransform,
            MmMethod mmMethod = default(MmMethod),
			MmMetadataBlock metadataBlock = null)
			: base(mmMethod, metadataBlock)
		{
			MmTransform = transform;
            LocalTransform = localTransform;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageTransform(MmMessageTransform message) : base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
			MmMessageTransform newMessage = new MmMessageTransform (this);
            newMessage.MmTransform = MmTransform;
            newMessage.LocalTransform = LocalTransform;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="reader">UNET based deserializer object</param>
        public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize (reader);
			MmTransform.Deserialize (reader);
            LocalTransform = reader.ReadBoolean();
        }

        /// <summary>
        /// Serialize the MmMessage
        /// </summary>
        /// <param name="writer">UNET based serializer</param>
        public override void Serialize(NetworkWriter writer)
		{
			base.Serialize (writer);
			MmTransform.Serialize (writer);
            writer.Write(LocalTransform);
        }
	}
}