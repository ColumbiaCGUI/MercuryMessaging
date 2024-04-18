using UnityEngine;
using System;
// using System.Text;
// using System.Text.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


#if PHOTON_AVAILABLE
using ExitGames.Client.Photon;
using Photon.Realtime;
using Fusion;


[Serializable]
public class DataCollection
{   
    public object[] data;
}

namespace MercuryMessaging
{
    

    [RequireComponent(typeof(NetworkObject))]
    public class MmNetworkResponderPhotonFusion : MmNetworkResponderFusion
    {
        private NetworkObject _networkObject;
        private NetworkRunner _networkRunner;

        // [Networked] public string dataTrans {get; set;}

        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();
            _networkObject = GetComponent<NetworkObject>();
            // _networkRunner = GetComponent<NetworkRunner>();
            _networkRunner = _networkObject.Runner;
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        public override bool IsActiveAndEnabled{ get { return _networkRunner !=null && _networkRunner.IsRunning; } }

        public override bool OnServer{ get { return _networkRunner!=null && _networkRunner.IsServer; } }

        public override bool OnClient{ get { return _networkRunner!=null && _networkRunner.IsClient; } }

        public override void MmInvoke(MmMessage msg, int connectionId = -1) 
        {
            _networkRunner = _networkObject.Runner;

            msg.NetId = (uint) _networkObject.Id.Raw;
        
            MmMessageBool msgb = (MmMessageBool) msg;

            object[] data = msg.Serialize();

            DataCollection dc = new();
            dc.data = data;

            // for some reason ToJson doesn't return the right values
            // looks like the object array data is not being serialized correctly
            // you probably need to add serializable / serializablefield tag to everything
            
            string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(dc);

            // // serialize the message before sending it
            byte[] dataSent = System.Text.Encoding.UTF8.GetBytes(jsonText);

            // // If the connection ID is defined, only send it there,
            // // otherwise, it follows the standard execution flow for the chosen 
            // // network solution.
            if (connectionId != -1)
            {
                MmSendMessageToClient(connectionId, msg);
                return;
            }

            // // Need to call the right method based on whether this object 
            // // is a client or a server.
            if (IsActiveAndEnabled)
            {
                // Debug.Log("Sending message to client");
                
                RPC_MmSendMessageToClient(dataSent);
            }
            else if (AllowClientToSend)
            {
                // Debug.Log("Sending message to server");
                RPC_MmSendMessageToServer(dataSent);
            }

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
        /// 
        [Rpc(sources:RpcSources.All, targets:RpcTargets.StateAuthority)]
        public override void RPC_MmSendMessageToServer(byte[] dataSent)
        {
            // deserialize the message after receiving it
            string json = System.Text.Encoding.UTF8.GetString(dataSent);
            
            DataCollection dc = Newtonsoft.Json.JsonConvert.DeserializeObject<DataCollection>(json);
            object[] data = dc.data;

            MmMessage msg = new MmMessage();
            ReceivedMessage(msg, data);
        
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
            Debug.Log("Sending message to client");

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
        
        [Rpc(sources:RpcSources.All, targets:RpcTargets.All)]
        public override void RPC_MmSendMessageToClient(byte[] dataSent)
        {

            string json = System.Text.Encoding.UTF8.GetString(dataSent);
            
            DataCollection dc = Newtonsoft.Json.JsonConvert.DeserializeObject<DataCollection>(json);
            object[] data = dc.data;

            MmMessage msg = new MmMessage();
            
            ReceivedMessage(msg, data);

        }
        #endregion

        
        
        /// <summary>
        /// Process a message and send it to the associated object.
        /// </summary>
        /// <param name="photonEvent">Photon RaiseEvent message data</param>
        
        public virtual void ReceivedMessage(MmMessage msg, object[] data)
		{
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
                        // Debug.Log(eventCode);
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