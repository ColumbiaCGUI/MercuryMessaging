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
namespace MercuryMessaging
{
    /// <summary>
    /// Implement this interface to utilize 
    /// your chosen Networking framework with 
    /// Mercury XM.
    /// </summary>
	public interface IMmNetworkResponder {

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
        /// Allows us to stop messages on client from sending.
        /// </summary>
        bool AllowClientToSend { get; }

        /// <summary>
        /// Set when Network Obj is active & enabled.
        /// This is important since Objects in UNET scenarios
        /// UNET may not be awake/active at the same times.
        /// </summary>
        bool IsActiveAndEnabled { get; }

        /// <summary>
        /// Indicates whether the network responder is executing on a server 
        /// </summary>
        bool OnServer { get; }

        /// <summary>
        /// Indicates whether the network responder is executing on a client 
        /// </summary>
        bool OnClient { get; }

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
        /// <param name="msg">The message to send.
        /// This class builds on UNET's MessageBase so it is
        /// Auto [de]serialized by UNET.</param>
        /// <param name="connectionId">Connection ID - use to identify clients.</param>
        void MmInvoke(MmMessageType msgType, MmMessage message, int connectionId = -1);
    }
}