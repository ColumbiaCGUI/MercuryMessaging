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
// Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================
//  
//  
using UnityEngine;

namespace MercuryMessaging
{
	public abstract class MmNetworkResponder : MonoBehaviour, IMmNetworkResponder
	{
        /// <summary>
        /// Get a handle to an MmRelayNode that shares the same GameObject.
        /// </summary>
	    public MmRelayNode MmRelayNode { get; private set; }

        #region Implementation of IMmNetworkResponder

        /// <summary>
        /// Event triggered in the MmNetworkResponder when 
        /// this object is awoken.
        /// </summary>
        event IMmCallback MmAwakeCompleteCallback;

        /// <summary>
        /// Event triggered in the MmNetworkResponder when 
        /// this object is started.
        /// </summary>
        event IMmCallback MmStartCompleteCallback;

        /// <summary>
        /// Event triggered in the MmNetworkResponder when 
        /// this object is awoken.
        /// Todo: Needs to be swapped for MmAwakeCompleteCallback
        /// </summary>
        public event IMmCallback MmAwake;

        /// <summary>
        /// Event triggered in the MmNetworkResponder when 
        /// this object is started.
        /// Todo: Needs to be swapped for MmStartCompleteCallback
        /// </summary>
        public event IMmCallback MmStart;

        /// <summary>
        /// Post Awake callback used between objects implementing this interface.
        /// This allows for initialization steps that must occur before one
        /// instance's Awake and its Start.
        /// </summary>
        public virtual void MmOnAwakeComplete()
        {
            if (MmAwakeCompleteCallback != null)
                MmAwakeCompleteCallback();
        }

        /// <summary>
        /// Post Start callback used between objects implementing this interface
        /// This allows for initialization steps that must occur before one
        /// instance's Start and its first Update.
        /// </summary>
        public virtual void MmOnStartComplete()
        {
            if (MmStartCompleteCallback != null)
                MmStartCompleteCallback();
        }

        /// <summary>
        /// It is possible that certain handles are not going to be in-place
        /// when registration of the OnAwakeComplete callback is invoked.
        /// The is especially true in scenarios where MmRelayNodes are networked.
        /// This allows for a deferred registration, eliminating most 
        /// instances where the Awake callback invocations fail.
        /// </summary>
        /// <param name="callback">Callback to be registered.</param>
        public void MmRegisterAwakeCompleteCallback(IMmCallback callback)
        {
            MmAwakeCompleteCallback += callback;
        }

        /// <summary>
        /// It is possible that certain handles are not going to be in-place
        /// when registration of the OnStartComplete callback is invoked.
        /// The is especially true in scenarios where MmRelayNodes are networked.
        /// This allows for a deferred registration, eliminating most 
        /// instances where the Start callback invocations fail.
        /// </summary>
        /// <param name="callback">Callback to be registered.</param>
        public void MmRegisterStartCompleteCallback(IMmCallback callback)
        {
            MmStartCompleteCallback += callback;
        }

        /// <summary>
        /// Allows us to stop messages on client from sending.
        /// </summary>
	    private bool allowClientToSend = true;

        /// <summary>
        /// Allows us to stop messages on client from sending.
        /// </summary>
        public bool AllowClientToSend { get {return allowClientToSend;} }

        /// <summary>
        /// Set when Network Obj is active & enabled.
        /// This is important since Objects in network scenarios
        /// may not be awake/active at the same times.
        /// </summary>
        public abstract bool IsActiveAndEnabled { get; }

	    /// <summary>
	    /// Indicates whether the network responder is executing on a server 
	    /// </summary>
	    public abstract bool OnServer { get; }

	    /// <summary>
	    /// Indicates whether the network responder is executing on a client 
	    /// </summary>
	    public abstract bool OnClient { get; }

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
        public abstract void MmInvoke(MmMessage msg, int connectionId = -1);

        #endregion

        #region Initialization

        /// <summary>
        /// Awake gets the MmRelayNode, if one is present.
        /// Also calls the post-awake callback.
        /// </summary>
        public virtual void Awake()
		{
		    MmLogger.LogFramework(gameObject.name + ": Network Responder Awake");

            MmRelayNode = GetComponent<MmRelayNode>();

            if (MmAwake != null)
                MmAwake();
        }

        /// <summary>
        /// Attempts to register the this responder with the 
        /// MmNetworkManager.
        /// Also calls the post-start callback.
        /// </summary>
        public virtual void Start()
        {
            MmLogger.LogFramework(gameObject.name + ": Network Responder Started");
			
            if (MmStart != null)
                MmStart();
        }

        #endregion

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
        public abstract void MmSendMessageToServer(MmMessage msg);

        /// <summary>
        /// Send a message to a specific client.
        /// </summary>
        /// <param name="channelId">Client connection ID</param>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        public abstract void MmSendMessageToClient(int channelId, MmMessage msg);

        /// <summary>
        /// Send a message to all clients.
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="msg">The message to send.</param>
        public abstract void MmSendMessageToClient(MmMessage msg);
    }
}
