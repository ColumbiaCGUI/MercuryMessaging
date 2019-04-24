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
using MercuryMessaging.Support.GUI;

namespace MercuryMessaging.AppState
{
    /// <summary>
    /// MmSwitchResponder for controlling MmAppStates. 
    /// </summary>
    public class MmAppStateSwitchResponder : MmSwitchResponder
    {
        /// <summary>
        /// Collection of MmAppStateResponder.
        /// </summary>
        private Dictionary<string, MmAppStateResponder> mmAppStateResponders;

        /// <summary>
        /// Handle to associated MmGuiHandler.
        /// </summary>
        public MmGuiHandler MmGuiHandler;

        /// <summary>
        /// Current AppState.
        /// </summary>
		public MmRelayNode Current
		{
			get {return MmRelaySwitchNode.Current;}
		}

        /// <summary>
        /// Invokes method to Setup App States.
        /// </summary>
        public override void Start()
        {
            base.Start();

            SetupAppStates();
        }

        /// <summary>
        /// Setup app states in derivations.
        /// Get handle to singleton instance MmGuiHandler, is present.
        /// </summary>
        public virtual void SetupAppStates()
        {
            //Todo: Remove
            MmGuiHandler = MmGuiHandler.Instance;

            MmLogger.LogApplication(gameObject.name + ": Initializing Application States.");
        }
    }
}