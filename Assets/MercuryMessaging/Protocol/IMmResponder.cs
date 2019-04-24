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
    /// The core of MercuryMessaging.
    /// This interface specifies MmInvoke - which is the 
    /// message-passing utility of the framework.
    /// Awake/Start callbacks are also required to allow the framework
    /// to ensure certain components are in place (in both networked and local environments)
    /// when needed.
    /// </summary>
    public interface IMmResponder
    {
        /// <summary>
        /// Post Awake callback used between objects implementing this interface.
        /// This allows for initialization steps that must occur before one
        /// instance's Awake and its Start.
        /// </summary>
        void MmOnAwakeComplete();

        /// <summary>
        /// Post Start callback used between objects implementing this interface
        /// This allows for initialization steps that must occur before one
        /// instance's Start and its first Update.
        /// </summary>
        void MmOnStartComplete();

        /// <summary>
        /// It is possible that certain handles are not going to be in-place
        /// when registration of the OnAwakeComplete callback is invoked.
        /// The is especially true in scenarios where MmRelayNodes are networked.
        /// This allows for a deferred registration, eliminating most 
        /// instances where the Awake callback invocations fail.
        /// </summary>
        /// <param name="callback">Callback to be registered.</param>
        void MmRegisterAwakeCompleteCallback(IMmCallback callback);

        /// <summary>
        /// It is possible that certain handles are not going to be in-place
        /// when registration of the OnStartComplete callback is invoked.
        /// The is especially true in scenarios where MmRelayNodes are networked.
        /// This allows for a deferred registration, eliminating most 
        /// instances where the Start callback invocations fail.
        /// </summary>
        /// <param name="callback">Callback to be registered.</param>
        void MmRegisterStartCompleteCallback(IMmCallback callback);

        /// <summary>
        /// MmTags allow you to specify filters for execution in 
        /// MercuryMessaging Hierarchies. <see cref="MmTag"/>
        /// </summary>
        MmTag Tag { get; set; }

        /// <summary>
        /// Handle to an instance's GameObject.
        /// </summary>
        GameObject MmGameObject { get; }

        /// <summary>
        /// Determines whether tag checking
        /// is enabled for this IMmResponder
        /// </summary>
        bool TagCheckEnabled { get; set; }

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
        void MmInvoke(MmMessageType msgType, MmMessage message);

        /// <summary>
        /// Get a handle to a IMmResponder's MmRelayNode,
        /// if one is present.
        /// </summary>
        /// <returns>an MmRelayNode sharing the GameObject
        /// with the instance.</returns>
		MmRelayNode GetRelayNode ();
    }

    /// <summary>
    /// IMm callback type used by implementations of IMmResponder. 
    /// </summary>
	public delegate void IMmCallback();	
}