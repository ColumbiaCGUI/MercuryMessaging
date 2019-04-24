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
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with Transform List payload
    /// </summary>
	public class MmMessageTransformList : MmMessage
	{
        /// <summary>
        /// Payload: List of MmTransforms
        /// </summary>
		public List<MmTransform> transforms;

        /// <summary>
        /// Creates a basic MmMessageTransformList
        /// </summary>
		public MmMessageTransformList()
		{
			transforms = new List<MmTransform> ();
		}

        /// <summary>
        /// Creates a basic MmMessageTransformList, 
        /// with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageTransformList(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock)
		{
			transforms = new List<MmTransform>();
		}

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and a
        /// list of MmTransforms
        /// </summary>
        /// <param name="iTransforms">List of transforms to send in payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageTransformList(List<MmTransform> iTransforms, 
			MmMethod mmMethod = default(MmMethod),
			MmMetadataBlock metadataBlock = null)
			: base(mmMethod, metadataBlock)
		{
			transforms = iTransforms;
		}

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageTransformList(MmMessageTransformList message) : base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
			MmMessageTransformList newMessage = new MmMessageTransformList (this);
            newMessage.transforms = new List<MmTransform>(transforms);

            return newMessage;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="reader">UNET based deserializer object</param>
        public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize (reader);
			transforms.Clear ();

			int transformsCount = reader.ReadInt32();
			for(int i = 0; i < transformsCount; i++)
			{
				MmTransform tempTrans = new MmTransform ();
				tempTrans.Deserialize (reader);
				transforms.Add(tempTrans);
			}
		}

        /// <summary>
        /// Serialize the MmMessage
        /// </summary>
        /// <param name="writer">UNET based serializer</param>
        public override void Serialize(NetworkWriter writer)
		{
			base.Serialize (writer);
			writer.Write (transforms.Count);

			for(int i = 0; i < transforms.Count; i++)
			{
				transforms [i].Serialize (writer);
			}
		}
	}
}