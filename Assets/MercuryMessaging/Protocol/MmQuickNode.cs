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
    /// Modification of the standard MmRelayNode that allows skipping of standard checks to allow for 
    /// faster propagation.
    /// </summary>
    public class MmQuickNode : MmRelayNode {

        public bool AllowStandardMmInvoke = false;

        /// <summary>
        /// Override the basic functionality of MmRelayNode to allow for faster processing by skipping checks.
        /// </summary>
        /// <param name="msgType">Type of message. This specifies
        /// the type of the payload. This is important in 
        /// networked scenarios, when proper deseriaization into 
        /// the correct type requires knowing what was 
        /// used to serialize the object originally.</param>
        /// <param name="message">The message to send.
        /// This class builds on UNET's MessageBase so it is
        /// Auto [de]serialized by UNET.</param>
        public override void MmInvoke (MmMessageType msgType, MmMessage message)
        {
            if(AllowStandardMmInvoke)
            {
                base.MmInvoke (msgType, message);
            }
            else
            {
                //If the MmRelayNode has not been initialized, initialize it here,
                //  and refresh the parents - to ensure proper routing can occur.
                InitializeNode();

                MmNetworkFilter networkFilter = message.MetadataBlock.NetworkFilter;

                if (MmNetworkResponder != null &&
                    message.MetadataBlock.NetworkFilter != MmNetworkFilter.Local &&
                    !message.IsDeserialized)
                {
                    MmNetworkFilter originalNetworkFilter = NetworkFilterAdjust(ref message);
                    MmNetworkResponder.MmInvoke (msgType, message);
                    message.MetadataBlock.NetworkFilter = originalNetworkFilter;
                }

                if (!AllowNetworkPropagationLocally && !message.IsDeserialized &&
                    message.MetadataBlock.NetworkFilter == MmNetworkFilter.Network)
                {
                    return;
                }

                foreach (var routingTableItem in RoutingTable) {
                    var responder = routingTableItem.Responder;

                    //bool isLocalResponder = responder.MmGameObject == this.gameObject;
                    MmLevelFilter responderLevel = routingTableItem.Level;

                    responder.MmInvoke (msgType, message);

                }
            }
        }
    }
}
