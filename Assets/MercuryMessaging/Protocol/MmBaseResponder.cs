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
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
// 
// 

using System;
using System.Collections.Generic;
using MercuryMessaging.Message;
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
					var messageMessage = (MmMessageString)message;
					ReceivedMessage(messageMessage.value);
					break;
                default:
                    Debug.Log(message.MmMethod.ToString());
                    throw new ArgumentOutOfRangeException();
            }
        }

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
        /// Handle MmMethod: Message
        /// </summary>
        /// <param name="message">String message extracted from MmMessageString.</param>
		protected virtual void ReceivedMessage(string message)
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
    }
}