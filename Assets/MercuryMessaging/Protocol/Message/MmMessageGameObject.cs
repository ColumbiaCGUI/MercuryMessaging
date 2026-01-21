// Copyright (c) 2017, Columbia University 
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
using UnityEngine;
#if PHOTON_AVAILABLE
using Photon.Pun;
#endif

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage with MmMessageGameObject payload
    /// </summary>
	public class MmMessageGameObject : MmMessage
	{
        /// <summary>
        /// MmMessageGameObject GameObject payload (local reference)
        /// </summary>
		public GameObject Value;

        /// <summary>
        /// Network ID for backend-agnostic serialization.
        /// Set by MmBinarySerializer when serializing, resolved by IMmGameObjectResolver when deserializing.
        /// </summary>
        public uint GameObjectNetId;

        /// <summary>
        /// Creates a basic MmMessageGameObject
        /// </summary>
		public MmMessageGameObject()
		{}

        /// <summary>
        /// Creates a basic MmMessageGameObject, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
		public MmMessageGameObject(MmMetadataBlock metadataBlock = null)
			: base (metadataBlock, MmMessageType.MmGameObject)
		{
		}

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and an GameObject
        /// </summary>
        /// <param name="transform">MmTransform payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageGameObject(GameObject gameObject,
            MmMethod mmMethod = default(MmMethod),
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, MmMessageType.MmGameObject, metadataBlock)
        {
            Value = gameObject;
        }

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageGameObject(MmMessageGameObject message) : base(message)
		{}

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
            MmMessageGameObject newMessage = new MmMessageGameObject(this);
            newMessage.Value = Value;

            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageGameObject
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageGameObject</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            #if PHOTON_AVAILABLE
            bool networkedGameObject = (bool) data[index++];
            if (networkedGameObject)
            {
                int photonViewId = (int) data[index++];
                PhotonView photonView = PhotonView.Find(photonViewId);
                Value = null;
                if (photonView != null)
                {
                    Value = photonView.gameObject;
                }
            }
            else 
            {
                int instanceID = (int) data[index++];
                Value = GameObject.Find(instanceID.ToString());
            }
            #else
            int instanceID = (int) data[index++];
            Value = GameObject.Find(instanceID.ToString());
            #endif
            return index;
        }

        /// <summary>
        /// Serialize the MmMessageGameObject
        /// </summary>
        /// <returns>Object array representation of a MmMessageGameObject</returns>
        public override object[] Serialize()
        {
            object[] baseSerialized = base.Serialize();

            #if PHOTON_AVAILABLE
            // With Photon: base + 2 (bool + id)
            object[] result = new object[baseSerialized.Length + 2];

            // Copy base data using Array.Copy (no LINQ)
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            if (Value.GetComponent<PhotonView>() != null)
            {
                PhotonView photonView = Value.GetComponent<PhotonView>();
                result[baseSerialized.Length] = true;
                result[baseSerialized.Length + 1] = photonView.ViewID;
            }
            else
            {
                result[baseSerialized.Length] = false;
                result[baseSerialized.Length + 1] = Value.GetInstanceID();
            }
            #else
            // Without Photon: base + 1 (id only)
            object[] result = new object[baseSerialized.Length + 1];

            // Copy base data using Array.Copy (no LINQ)
            Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

            // Fill instance ID directly
            result[baseSerialized.Length] = Value.GetInstanceID();
            #endif

            return result;
        }
	}
}