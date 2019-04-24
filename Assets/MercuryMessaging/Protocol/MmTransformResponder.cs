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
using MercuryMessaging.Support.Extensions;
using UnityEngine;

namespace MercuryMessaging
{
	public class MmTransformResponder : MmBaseResponder {

	    public bool CanSend;

        /// <summary>
        /// Based on modification of UNET NetworkTransform code
        /// See:https://bitbucket.org/Unity-Technologies/networking/src
        /// </summary>
	    private float lastClientSendTime;

        /// <summary>
        /// Based on modification of UNET NetworkTransform code
        /// See:https://bitbucket.org/Unity-Technologies/networking/src
        /// </summary>
	    public float NetworkSendInterval = 60f;
        
		public override void Update ()
		{
		    if (CanSend && Time.time - lastClientSendTime > (1/NetworkSendInterval))
		    {
		        lastClientSendTime = Time.time;

		        GetRelayNode()
		            .MmInvoke(MmMethod.Transform,
		                new MmTransform(gameObject.transform, true),
		                new MmMetadataBlock(MmLevelFilter.Self,
		                    MmActiveFilter.All, MmSelectedFilter.All)
		            );
		    }

            base.Update();
		}

		protected override void ReceivedMessage(MmMessageTransform msgTransform)
		{
			this.gameObject.transform.SetPosition (msgTransform.MmTransform.Translation, true);
			this.gameObject.transform.SetRotation (msgTransform.MmTransform.Rotation, true);
		}
	}
}