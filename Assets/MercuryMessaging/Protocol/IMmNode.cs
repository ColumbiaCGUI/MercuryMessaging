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
using System.Collections.Generic;
using MercuryMessaging.Task;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Defines methods required by an MmResponder & utility methods 
    /// making invocation MmMethods with built-in MmMessages easier.
    /// </summary>
    public interface IMmNode
    {
        /// <summary>
        /// Invoke an MmMethod. 
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
        void MmInvoke(MmMessageType msgType, MmMessage message);

        /// <summary>
        /// Invoke an MmMethod with no parameter. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// MmMessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: int. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: int.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// MmMessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, int param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: Vector3.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: Vector3.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// MmMessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, Vector3 param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: Vector4.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: Vector4.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// MmMessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, Vector4 param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: string. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: string.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, string param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: float. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: float.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, float param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: byte array.  
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: byte array.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, byte[] param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: bool. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: bool.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, bool param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: IMmSerializable. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: IMmSerializable. <see cref="IMmSerializable"/> </param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, IMmSerializable param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: MmTransform. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: MmTransform. <see cref="MmTransform"/></param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, MmTransform param, MmMetadataBlock metadataBlock);

        /// <summary>
        /// Invoke an MmMethod with parameter: List<MmTransform>. 
        /// </summary>
        /// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
        /// <param name="param">MmMethod parameter: List<MmTransform>.</param>
        /// <param name="metadataBlock">Object defining the routing of 
        /// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
        void MmInvoke(MmMethod mmMethod, List<MmTransform> param, MmMetadataBlock metadataBlock);

		/// <summary>
		/// Invoke an MmMethod with any message type.
		/// </summary>
		/// <param name="mmMethod">MmMethod Identifier - <see cref="MmMethod"/></param>
		/// <param name="param">MmMethod parameter: Any Message type.</param>
	 	/// <param name="msgType">Type of MmMessage parameter.</param>
		/// <param name="metadataBlock">Object defining the routing of 
		/// Mmessages through MercuryMessaging Hierarchies. <see cref="MmMetadataBlock"/></param>
		void MmInvoke(MmMethod mmMethod, MmMessage param, MmMessageType msgType, MmMetadataBlock metadataBlock);
    }
}