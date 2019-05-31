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
using MercuryMessaging.Task;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// MmResponder that implements a switch handling
    /// the framework-provided MmMethods.
    /// </summary>
	public class MmBaseResponder : MmResponder
	{
        /// <summary>
        /// Invoke an MmMethod. 
        /// Implements a switch that handles the different MmMethods
        /// defined by default set in MmMethod <see cref="MmMethod"/>
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally. <see cref="MmMessageType"/>
        /// </param>
        /// <param name="message">The message to send.
        /// This class builds on UNET's MessageBase so it is
        /// Auto [de]serialized by UNET. <see cref="MmMessage"/></param>
        public override void MmInvoke(MmMessageType msgType, MmMessage message)
        {
            var type = message.MmMethod;

            switch (type)
            {
                case MmMethod.NoOp:
                    break;
                case MmMethod.SetActive:
                    var messageBool = (MmMessageBool) message;
                    SetActive(messageBool.value);
                    break;
                case MmMethod.Refresh:
                    var messageTransform = (MmMessageTransformList) message;
                    Refresh(messageTransform.transforms);
                    break;
                case MmMethod.Initialize:
                    Initialize();
                    break;
                case MmMethod.Switch:
                    var messageString = (MmMessageString) message;
                    Switch(messageString.value);
                    break;
                case MmMethod.Complete:
                    var messageCompleteBool = (MmMessageBool)message;
                    Complete(messageCompleteBool.value);
                    break;
                case MmMethod.TaskInfo:
                    var messageSerializable = (MmMessageSerializable)message;
                    ApplyTaskInfo(messageSerializable.value);
                    break;
                case MmMethod.Message:
                    ReceivedMessage(message);
                    break;
                case MmMethod.MessageBool:
                    ReceivedMessage((MmMessageString)message);
                    break;
                case MmMethod.MessageByteArray:
                    ReceivedMessage((MmMessageByteArray)message);
                    break;
                case MmMethod.MessageFloat:
                    ReceivedMessage((MmMessageFloat)message);
                    break;
                case MmMethod.MessageInt:
                    ReceivedMessage((MmMessageInt)message);
                    break;
                case MmMethod.MessageSerializable:
                    ReceivedMessage((MmMessageSerializable)message);
                    break;
                case MmMethod.MessageString:
                    ReceivedMessage((MmMessageString)message);
					break;
                case MmMethod.MessageTransform:
                    ReceivedMessage((MmMessageTransform)message);
                    break;
                case MmMethod.MessageTransformList:
                    ReceivedMessage((MmMessageTransformList)message);
                    break;
                case MmMethod.MessageVector3:
                    ReceivedMessage((MmMessageVector3)message);
                    break;
                case MmMethod.MessageVector4:
                    ReceivedMessage((MmMessageVector4)message);
                    break;
                default:
                    Debug.Log(message.MmMethod.ToString());
                    throw new ArgumentOutOfRangeException();
            }
        }


        #region Base Message Handlers
        /// <summary>
        /// Handle MmMethod: SetActive
        /// </summary>
        /// <param name="active">Value of active state.</param>
        public virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);

            MmLogger.LogResponder("SetActive(" + active + ") called on " + gameObject.name);
        }

        /// <summary>
        /// Handle MmMethod: Initialize
        /// Initialize allows you to provide additional initialization logic
        /// in-between calls to Monobehavior provided Awake() and Start() calls.
        /// </summary>
        public virtual void Initialize()
        {
            MmLogger.LogResponder("Initialize called on " + gameObject.name);
        }

        /// <summary>
        /// Handle MmMethod: Refresh
        /// </summary>
        /// <param name="transformList">List of transforms needed in refreshing an MmResponder.</param>
        public virtual void Refresh(List<MmTransform> transformList)
        {
            MmLogger.LogResponder("Refresh called on " + gameObject.name);
        }

        /// <summary>
        /// Handle MmMethod: Switch
        /// </summary>
        /// <param name="iName">Name of value in which to active.</param>
        protected virtual void Switch(string iName)
        {
        }

        /// <summary>
        /// Handle MmMethod: Switch
        /// </summary>
        /// <param name="active">Can be used to indicate active state 
        /// of object that triggered complete message</param>
        protected virtual void Complete(bool active)
        {
        }

        /// <summary>
        /// Handle MmMethod: TaskInfo
        /// Given a IMmSerializable, extract TaskInfo.
        /// </summary>
        /// <param name="serializableValue">Serializable class containing MmTask Info</param>
	    protected virtual void ApplyTaskInfo(IMmSerializable serializableValue)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: Base MmMessage.
	    /// Override this to handle base Mercury Messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessage"/></param>
	    protected virtual void ReceivedMessage(MmMessage message)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: MessageBool.
	    /// Override this to handle Mercury's bool messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageBool"/></param>
	    protected virtual void ReceivedMessage(MmMessageBool message)
	    {
	    }

        /// <summary>
        /// Handle MmMethod: MessageByteArray.
        /// Override this to handle Mercury's byte array messages.
        /// </summary>
        /// <param name="message"><see cref="MmMessageByteArray"/></param>
        protected virtual void ReceivedMessage(MmMessageByteArray message)
	    {
	    }

        /// <summary>
        /// Handle MmMethod: MessageFloat.
        /// Override this to handle Mercury's float messages.
        /// </summary>
        /// <param name="message"><see cref="MmMessageFloat"/></param>
        protected virtual void ReceivedMessage(MmMessageFloat message)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: MessageInt.
	    /// Override this to handle Mercury's int messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageInt"/></param>
	    protected virtual void ReceivedMessage(MmMessageInt message)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: MessageSerializable.
	    /// Override this to handle Mercury's serializable messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageSerializable"/></param>
	    protected virtual void ReceivedMessage(MmMessageSerializable message)
	    {
	    }

        /// <summary>
        /// Handle MmMethod: MessageString
        /// Override this to handle Mercury's string messages.
        /// </summary>
        /// <param name="message"><see cref="MmMessageString"/></param>
		protected virtual void ReceivedMessage(MmMessageString message)
		{
		}

	    /// <summary>
	    /// Handle MmMethod: MessageTransform
	    /// Override this to handle Mercury's transform messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageTransform"/></param>
	    protected virtual void ReceivedMessage(MmMessageTransform message)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: MessageTransformList
	    /// Override this to handle Mercury's transform list messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageTransformList"/></param>
	    protected virtual void ReceivedMessage(MmMessageTransformList message)
	    {
	    }

        /// <summary>
        /// Handle MmMethod: MessageVector3
        /// Override this to handle Mercury's Vector3 messages.
        /// </summary>
        /// <param name="message"><see cref="MmMessageVector3"/></param>
        protected virtual void ReceivedMessage(MmMessageVector3 message)
	    {
	    }

	    /// <summary>
	    /// Handle MmMethod: MessageVector4
	    /// Override this to handle Mercury's Vector4 messages.
	    /// </summary>
	    /// <param name="message"><see cref="MmMessageVector4"/></param>
	    protected virtual void ReceivedMessage(MmMessageVector4 message)
	    {
	    }

        /// <summary>
        /// Implementation of IMmResponder's GetRelayNode.
        /// </summary>
        /// <returns>Returns MmRelayNode if one attached to GameObject, 
        /// Otherwise returns NULL.
        /// </returns>
	    public override MmRelayNode GetRelayNode()
		{
			return GetComponent<MmRelayNode>();
		}

        #endregion
    }
}