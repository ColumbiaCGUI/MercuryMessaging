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
// Carmine Elvezio, Samuel Silverman, Steven Feiner
// =============================================================
//  
//  
using UnityEngine;
using System;

#if PHOTON_AVAILABLE
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

namespace MercuryMessaging
{
    [RequireComponent(typeof(PhotonView))]
	public class MmNetworkResponderPhoton : MmNetworkResponder
	{
        private PhotonView photonView;

        /// <summary>
        /// Awake gets the MmRelayNode, if one is present.
        /// Also calls the post-awake callback.
        /// </summary>
        public override void Awake()
		{
            base.Awake();
        }

        /// <summary>
        /// Attempts to register the this responder with the 
        /// MmNetworkManager.
        /// Also calls the post-start callback.
        /// </summary>
        public override void Start()
        {
            base.Start();
            photonView = GetComponent<PhotonView>();
        }

        private void OnEnable() 
        {
            PhotonNetwork.NetworkingClient.EventReceived += ReceivedMessage;
        }

        private void OnDisable() 
        {
            PhotonNetwork.NetworkingClient.EventReceived -= ReceivedMessage;
        }

        /// <summary>
        /// Set when Network Obj is active & enabled.
        /// This is important since Objects in Photon scenarios
        /// may not be awake/active at the same times.
        /// </summary>
        public override bool IsActiveAndEnabled { get {return PhotonNetwork.IsConnectedAndReady;} }

	    /// <summary>
	    /// Indicates whether the network responder is executing on a server 
	    /// </summary>
	    public override bool OnServer { get {return PhotonNetwork.IsMasterClient;} }

	    /// <summary>
	    /// Indicates whether the network responder is executing on a client 
	    /// </summary>
	    public override bool OnClient { get {return !OnServer;} }

        /// <summary>
        /// This is the network equivalent of IMmResponder's MmInvoke.
        /// The difference is this class allows specification of connectionIDs
        /// which can be used to ensure messages are routed to the correct
        /// objects on network clients.
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        /// <param name="connectionId">Connection ID - use to identify clients.</param>
        public override void MmInvoke(MmMessage msg, int connectionId = -1) 
        { 
            msg.NetId = (uint) photonView.ViewID;

            // If the connection ID is defined, only send it there,
            // otherwise, it follows the standard execution flow for the chosen 
            // network solution.
            if (connectionId != -1)
            {
                MmSendMessageToClient(connectionId, msg);
                return;
            }

            // Need to call the right method based on whether this object 
            // is a client or a server.
            if (IsActiveAndEnabled)
                MmSendMessageToClient(msg);
            else if (AllowClientToSend)
                MmSendMessageToServer(msg);
        }

        #region Implementation of MmNetworkResponder
        /// <summary>
        /// Method serializes message and sends it to server.
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        public override void MmSendMessageToServer(MmMessage msg)
        {
            byte eventCode = (byte)(1); 
            object[] data = msg.Serialize();
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Send a message to a specific client over chosen Photon.
        /// </summary>
        /// <param name="channelId">Client connection ID</param>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        public override void MmSendMessageToClient(int channelId, MmMessage msg)
        {
            byte eventCode = (byte)(1); 
            object[] data = msg.Serialize();
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true, Channel = (byte) channelId };
            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, sendOptions);
        }

        /// <summary>
        /// Send a message to all clients using Photon
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        public override void MmSendMessageToClient(MmMessage msg)
        {
            byte eventCode = (byte)(1); 
            object[] data = msg.Serialize();
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        #endregion
        
        /// <summary>
        /// Process a message and send it to the associated object.
        /// </summary>
        /// <param name="photonEvent">Photon RaiseEvent message data</param>
        public virtual void ReceivedMessage(EventData photonEvent)
		{
            short eventCode = (short)photonEvent.Code;
            if (eventCode != 1)
            {
                return;
            }
			MmMessageType mmMessageType = (MmMessageType)(eventCode);
            object[] data = (object[])photonEvent.CustomData;
            MmMessage msg = new MmMessage();
            msg.Deserialize(data);
		    try
		    {
		        switch (msg.MmMessageType)
		        {
		            case MmMessageType.MmVoid:
		                MmRelayNode.MmInvoke(msg);
		                break;
		            case MmMessageType.MmInt:
		                MmMessageInt msgInt = new MmMessageInt();
                        msgInt.Deserialize(data);
		                MmRelayNode.MmInvoke(msgInt);
		                break;
		            case MmMessageType.MmBool:
		                MmMessageBool msgBool = new MmMessageBool();
                        msgBool.Deserialize(data);
		                MmRelayNode.MmInvoke(msgBool);
		                break;
		            case MmMessageType.MmFloat:
		                MmMessageFloat msgFloat = new MmMessageFloat();
                        msgFloat.Deserialize(data);
		                MmRelayNode.MmInvoke(msgFloat);
		                break;
		            case MmMessageType.MmVector3:
		                MmMessageVector3 msgVector3 = new MmMessageVector3();
                        msgVector3.Deserialize(data);
		                MmRelayNode.MmInvoke(msgVector3);
		                break;
		            case MmMessageType.MmVector4:
		                MmMessageVector4 msgVector4 = new MmMessageVector4();
                        msgVector4.Deserialize(data);
		                MmRelayNode.MmInvoke(msgVector4);
		                break;
		            case MmMessageType.MmString:
		                MmMessageString msgString = new MmMessageString();
                        msgString.Deserialize(data);
		                MmRelayNode.MmInvoke(msgString);
		                break;
		            case MmMessageType.MmByteArray:
		                MmMessageByteArray msgByteArray = new MmMessageByteArray();
                        msgByteArray.Deserialize(data);
		                MmRelayNode.MmInvoke(msgByteArray);
                        break;
		            case MmMessageType.MmTransform:
		                MmMessageTransform msgTransform = new MmMessageTransform();
                        msgTransform.Deserialize(data);
		                MmRelayNode.MmInvoke(msgTransform);
		                break;
		            case MmMessageType.MmTransformList:
		                MmMessageTransformList msgTransformList = new MmMessageTransformList();
                        msgTransformList.Deserialize(data);
		                MmRelayNode.MmInvoke(msgTransformList);
		                break;
                    case MmMessageType.MmSerializable:
		                MmMessageSerializable msgSerializable = new MmMessageSerializable();
                        msgSerializable.Deserialize(data);
		                MmRelayNode.MmInvoke(msgSerializable);
		                break;
                    case MmMessageType.MmGameObject:
                        MmMessageGameObject msgGameObject = new MmMessageGameObject();
                        msgGameObject.Deserialize(data);
                        MmRelayNode.MmInvoke(msgGameObject);
                        break;
                default:
                        Debug.Log(eventCode);
		                throw new ArgumentOutOfRangeException();
		        }
		    }
		    catch (Exception e)
            {
                // FAIL-FAST: Log full exception with stack trace, not just message
                MmLogger.LogError($"MmNetworkResponderPhoton: Failed to process network event");
                Debug.LogException(e);
                // Note: We don't rethrow here as network event processing should be resilient,
                // but the full error is logged for debugging
            }
		} 
    }
}
#else
namespace MercuryMessaging
{
    public class MmNetworkResponderPhoton : MmNetworkResponder
    {
        public override bool IsActiveAndEnabled { get { return false; } }

        public override bool OnServer { get { return false; } }

        public override bool OnClient { get { return false; } }

        public override void MmInvoke(MmMessage message, int connectionId = -1) {}

        public override void MmSendMessageToServer(MmMessage msg) {}

        public override void MmSendMessageToClient(int channelId, MmMessage msg) {}

        public override void MmSendMessageToClient(MmMessage msg) {}
    }
}
#endif
