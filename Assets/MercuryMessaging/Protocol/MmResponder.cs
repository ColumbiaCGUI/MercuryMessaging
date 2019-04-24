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
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Base implmentation of IMmResponder.
    /// </summary>
	public abstract class MmResponder : MonoBehaviour, IMmResponder {

        /// <summary>
        /// Event triggered in the MmResponder when 
        /// this object is awoken.
        /// </summary>
        event IMmCallback MmAwakeCompleteCallback;

        /// <summary>
        /// Event triggered in the MmResponder when 
        /// this object is started.
        /// </summary>
        event IMmCallback MmStartCompleteCallback;

        /// <summary>
        /// Determines whether tag checking
        /// is enabled for this IMmResponder
        /// </summary>
        [SerializeField] private MmTag _mmTag = MmTagHelper.Everything;

        /// <summary>
        /// By default IMmResponders have this set to true so that they receive
        /// all messages. 
        /// </summary>
        [SerializeField] private bool tagCheckEnabled = true;

        /// <summary>
        /// Handle to an instance's GameObject.
        /// </summary>
        public GameObject MmGameObject
		{
			get { return this.gameObject; }
		}

        /// <summary>
        /// MmTags allow you to specify filters for execution in 
        /// Mercury XM graphs.
        /// </summary>
        public MmTag Tag
		{
			get { return _mmTag; }
			set { _mmTag = value; }
		}

        /// <summary>
        /// Determines whether tag checking
        /// is enabled for this IMmResponder
        /// </summary>
        public bool TagCheckEnabled
		{
			get { return tagCheckEnabled; }
			set { tagCheckEnabled = value; }
		}
        
        /// <summary>
        /// Awake: Invokes MmOnAwakeComplete
        /// </summary>
        public virtual void Awake()
		{
			MmLogger.LogResponder(gameObject.name + " responder awoken.");

			MmOnAwakeComplete();
		}

        /// <summary>
        /// Start: Invokes MmOnStartComplete
        /// </summary>
		public virtual void Start()
		{
			MmLogger.LogResponder(gameObject.name + " responder started.");

			MmOnStartComplete();
		}

        /// <summary>
        /// Base Update of MmResponder
        /// Derived classes should always override.
        /// </summary>
		public virtual void Update()
		{}

		#region IMmResponder Implementation

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
        public virtual void MmRegisterAwakeCompleteCallback(IMmCallback callback)
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
        public virtual void MmRegisterStartCompleteCallback(IMmCallback callback)
		{
			MmStartCompleteCallback += callback;
		}

        /// <summary>
        /// Get a handle to a IMmResponder's MmRelayNode,
        /// if one is present.
        /// </summary>
        /// <returns>an MmRelayNode sharing the GameObject
        /// with the instance.</returns>
        public abstract MmRelayNode GetRelayNode ();
        
        /// <summary>
        /// Invoke an MmMethod. 
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.
        /// </param>
        /// <param name="message">The message to send.
        /// This class builds on UNET's MessageBase so it is
        /// Auto [de]serialized by UNET.</param>
        public virtual void MmInvoke(MmMessageType msgType, MmMessage message)
		{
		}

        #endregion
    }
}