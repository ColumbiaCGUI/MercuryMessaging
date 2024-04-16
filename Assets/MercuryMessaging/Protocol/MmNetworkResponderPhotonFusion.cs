using UnityEngine;
using System;


#if PHOTON_AVAILABLE
using ExitGames.Client.Photon;
using Photon.Realtime;
using Fusion;

namespace MercuryMessaging
{
    [RequireComponent(typeof(NetworkObject))]
    public class MmNetworkResponderPhotonFusion : MmNetworkResponderFusion
    {
        private NetworkObject _networkObject;
        private NetworkRunner _networkRunner;

        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();
            _networkObject = GetComponent<NetworkObject>();
            _networkRunner = GetComponent<NetworkRunner>();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public override bool IsActiveAndEnabled{ get { return _networkRunner != null && _networkRunner.IsRunning; } }

        public override bool OnServer{ get { return _networkRunner!=null && _networkRunner.IsServer; } }

        public override bool OnClient{ get { return _networkRunner!=null && _networkRunner.IsClient; } }

        public override void MmInvoke(MmMessage msg, int connectionId = -1) 
        { 
            // msg.NetId = (uint) photonView.ViewID;

            // // If the connection ID is defined, only send it there,
            // // otherwise, it follows the standard execution flow for the chosen 
            // // network solution.
            // if (connectionId != -1)
            // {
            //     MmSendMessageToClient(connectionId, msg);
            //     return;
            // }

            // // Need to call the right method based on whether this object 
            // // is a client or a server.
            // if (IsActiveAndEnabled)
            //     MmSendMessageToClient(msg);
            // else if (AllowClientToSend)
            //     MmSendMessageToServer(msg);
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
            // byte eventCode = (byte)(1); 
            // object[] data = msg.Serialize();
            // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            // PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
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
            // byte eventCode = (byte)(1); 
            // object[] data = msg.Serialize();
            // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            // SendOptions sendOptions = new SendOptions { Reliability = true, Channel = (byte) channelId };
            // PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, sendOptions);
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
            // byte eventCode = (byte)(1); 
            // object[] data = msg.Serialize();
            // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            // PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        #endregion
        
        /// <summary>
        /// Process a message and send it to the associated object.
        /// </summary>
        /// <param name="photonEvent">Photon RaiseEvent message data</param>
        public virtual void ReceivedMessage(EventData photonEvent)
		{
        //     short eventCode = (short)photonEvent.Code;
        //     if (eventCode != 1)
        //     {
        //         return;
        //     }
		// 	MmMessageType mmMessageType = (MmMessageType)(eventCode);
        //     object[] data = (object[])photonEvent.CustomData;
        //     MmMessage msg = new MmMessage();
        //     msg.Deserialize(data);
		//     try
		//     {
		//         switch (msg.MmMessageType)
		//         {
		//             case MmMessageType.MmVoid:
		//                 MmRelayNode.MmInvoke(msg);
		//                 break;
		//             case MmMessageType.MmInt:
		//                 MmMessageInt msgInt = new MmMessageInt();
        //                 msgInt.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgInt);
		//                 break;
		//             case MmMessageType.MmBool:
		//                 MmMessageBool msgBool = new MmMessageBool();
        //                 msgBool.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgBool);
		//                 break;
		//             case MmMessageType.MmFloat:
		//                 MmMessageFloat msgFloat = new MmMessageFloat();
        //                 msgFloat.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgFloat);
		//                 break;
		//             case MmMessageType.MmVector3:
		//                 MmMessageVector3 msgVector3 = new MmMessageVector3();
        //                 msgVector3.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgVector3);
		//                 break;
		//             case MmMessageType.MmVector4:
		//                 MmMessageVector4 msgVector4 = new MmMessageVector4();
        //                 msgVector4.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgVector4);
		//                 break;
		//             case MmMessageType.MmString:
		//                 MmMessageString msgString = new MmMessageString();
        //                 msgString.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgString);
		//                 break;
		//             case MmMessageType.MmByteArray:
		//                 MmMessageByteArray msgByteArray = new MmMessageByteArray();
        //                 msgByteArray.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgByteArray);
        //                 break;
		//             case MmMessageType.MmTransform:
		//                 MmMessageTransform msgTransform = new MmMessageTransform();
        //                 msgTransform.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgTransform);
		//                 break;
		//             case MmMessageType.MmTransformList:
		//                 MmMessageTransformList msgTransformList = new MmMessageTransformList();
        //                 msgTransformList.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgTransformList);
		//                 break;
        //             case MmMessageType.MmSerializable:
		//                 MmMessageSerializable msgSerializable = new MmMessageSerializable();
        //                 msgSerializable.Deserialize(data);
		//                 MmRelayNode.MmInvoke(msgSerializable);
		//                 break;
        //             case MmMessageType.MmGameObject:
        //                 MmMessageGameObject msgGameObject = new MmMessageGameObject();
        //                 msgGameObject.Deserialize(data);
        //                 MmRelayNode.MmInvoke(msgGameObject);
        //                 break;
        //         default:
        //                 Debug.Log(eventCode);
		//                 throw new ArgumentOutOfRangeException();
		//         }
		//     }
		//     catch (Exception e)
        //     {
        //         MmLogger.LogError(e.Message);
        //     }
		} 


    }
}
#else
namespace MercuryMessaging
{
    public class MmNetworkResponderPhotonFusion : MmNetworkResponder
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