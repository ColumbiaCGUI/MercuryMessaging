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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// A object that interacts with the UNET networking environment 
    /// </summary>
	public class MmNetworkManager : NetworkBehaviour {

        /// <summary>
        /// Accessor for the the instance of the network manager instance.
        /// 
        /// </summary>
        public static MmNetworkManager Instance { get; private set; }

        /// <summary>
        /// Handle to the network client, if one preent
        /// </summary>
        public NetworkClient NetworkClient;
		
        /// <summary>
        /// Associated MmRelayNodes. Each MmNetworkResponder will attempt to register itself
        /// to this list. This allows for the system to optimize listening so that
        /// not every node attempts to handle a message
        /// </summary>
		public Dictionary<uint, MmRelayNode> MmRelayNodes = new Dictionary<uint, MmRelayNode> ();

        /// <summary>
        /// Callback for this object's Awake method.
        /// </summary>
		public event IMmCallback MmAwake;

        /// <summary>
        /// Callback for this object's Start method.
        /// </summary>
		public event IMmCallback MmStart;

        /// <summary>
        /// Invoke MmAwake on listeners.
        /// </summary>
		public virtual void Awake()
		{
			MmLogger.LogResponder (gameObject.name + ": MmNetworkManager Awake");

            Instance = this;
            
            if (MmAwake != null)
				MmAwake();
		}

        /// <summary>
        /// Attempt to register handlers to client/server objects
        /// Invokes MmStart on listeners.
        /// </summary>
		public virtual void Start()
		{
			MmLogger.LogResponder(gameObject.name + ": MmNetworkManager Started");

			if (NetworkClient == null && NetworkManager.singleton != null)
			{
				NetworkClient = NetworkManager.singleton.client;
			}

            if (NetworkClient != null)
			{
				foreach (var value in Enum.GetValues(typeof(MmMessageType)).Cast<short>())
				{
					NetworkClient.RegisterHandler(value, ReceivedMessage);
				}
			}

		    if (isServer)
		    {
		        foreach (var value in Enum.GetValues(typeof(MmMessageType)).Cast<short>())
		        {
		            NetworkServer.RegisterHandler(value, ReceivedMessage);
		        }
		    }

		    if (MmStart != null)
				MmStart();

		    //NetworkManager.singleton.globalConfig.MaxPacketSize = 2500;
        }

        /// <summary>
        /// Register all MmResponders that can receive messages.
        /// </summary>
        /// <param name="responder"></param>
		public void RegisterMmNetworkResponder(MmNetworkResponder responder)
        {
            MmRelayNode nodeToAdd;
            bool presentInDictionary = MmRelayNodes.TryGetValue(responder.netId.Value, out nodeToAdd);
            if(!presentInDictionary)
			    MmRelayNodes.Add (responder.netId.Value, responder.MmRelayNode);
		}

        /// <summary>
        /// Removes a particular relay node from the dictionary of supported nodes.
        /// </summary>
        public void RemoveMmNetworkResponder(MmNetworkResponder responder)
        {
            MmRelayNode nodeToRemove;
            bool presentInDictionary = MmRelayNodes.TryGetValue(responder.netId.Value, out nodeToRemove);
            if (!presentInDictionary)
                MmRelayNodes.Remove(responder.netId.Value);
        }

        /// <summary>
        /// Process a message and send it to the associated object.
        /// </summary>
        /// <param name="netMsg">UNET network message</param>
		public virtual void ReceivedMessage(NetworkMessage netMsg)
		{
			MmMessageType mmMessageType = (MmMessageType)netMsg.msgType;

		    try
		    {
		        switch (mmMessageType)
		        {
		            case MmMessageType.MmVoid:
		                MmMessage msg = netMsg.ReadMessage<MmMessage>();
		                MmRelayNodes[msg.NetId].MmInvoke(mmMessageType, msg);
		                break;
		            case MmMessageType.MmInt:
		                MmMessageInt msgInt = netMsg.ReadMessage<MmMessageInt>();
		                MmRelayNodes[msgInt.NetId].MmInvoke(mmMessageType, msgInt);
		                break;
		            case MmMessageType.MmBool:
		                MmMessageBool msgBool = netMsg.ReadMessage<MmMessageBool>();
		                MmRelayNodes[msgBool.NetId].MmInvoke(mmMessageType, msgBool);
		                break;
		            case MmMessageType.MmFloat:
		                MmMessageFloat msgFloat = netMsg.ReadMessage<MmMessageFloat>();
		                MmRelayNodes[msgFloat.NetId].MmInvoke(mmMessageType, msgFloat);
		                break;
		            case MmMessageType.MmVector3:
		                MmMessageVector3 msgVector3 = netMsg.ReadMessage<MmMessageVector3>();
		                MmRelayNodes[msgVector3.NetId].MmInvoke(mmMessageType, msgVector3);
		                break;
		            case MmMessageType.MmVector4:
		                MmMessageVector4 msgVector4 = netMsg.ReadMessage<MmMessageVector4>();
		                MmRelayNodes[msgVector4.NetId].MmInvoke(mmMessageType, msgVector4);
		                break;
		            case MmMessageType.MmString:
		                MmMessageString msgString = netMsg.ReadMessage<MmMessageString>();
		                MmRelayNodes[msgString.NetId].MmInvoke(mmMessageType, msgString);
		                break;
		            case MmMessageType.MmByteArray:
		                MmMessageByteArray msgByteArray = netMsg.ReadMessage<MmMessageByteArray>();
		                MmRelayNodes[msgByteArray.NetId].MmInvoke(mmMessageType, msgByteArray);
                        break;
		            case MmMessageType.MmTransform:
		                MmMessageTransform msgTransform = netMsg.ReadMessage<MmMessageTransform>();
		                MmRelayNodes[msgTransform.NetId].MmInvoke(mmMessageType, msgTransform);
		                break;
		            case MmMessageType.MmTransformList:
		                MmMessageTransformList msgTransformList = netMsg.ReadMessage<MmMessageTransformList>();
		                MmRelayNodes[msgTransformList.NetId].MmInvoke(mmMessageType, msgTransformList);
		                break;
                    case MmMessageType.MmSerializable:
		                MmMessageSerializable msgSerializable = netMsg.ReadMessage<MmMessageSerializable>();
		                MmRelayNodes[msgSerializable.NetId].MmInvoke(mmMessageType, msgSerializable);
		                break;
                    case MmMessageType.MmGameObject:
                        MmMessageGameObject msgGameObject = netMsg.ReadMessage<MmMessageGameObject>();
                        MmRelayNodes[msgGameObject.NetId].MmInvoke(mmMessageType, msgGameObject);
                        break;
                default:
		                throw new ArgumentOutOfRangeException();
		        }
		    }
		    catch (Exception e)
            {
                MmLogger.LogError(e.Message);
            }
		}
	}
}