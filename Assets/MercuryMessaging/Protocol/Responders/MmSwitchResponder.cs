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
    /// Controller for MmRelaySwitchNode.
    /// </summary>
	public class MmSwitchResponder : MmBaseResponder {

        /// <summary>
        /// Handle to MmRelaySwitchNode.
        /// </summary>
		public MmRelaySwitchNode MmRelaySwitchNode { get; private set; }

        /// <summary>
        /// Initial state for MmRelaySwitchNode FSM.
        /// </summary>
		public string InitialState;

        /// <summary>
        /// Auto-grabs the MmRelaySwitchNode. Also assigns/invokes
        /// Awake callbacks.
        /// </summary>
		public override void Awake()
		{
            MmLogger.LogResponder("MmSwitchResponder Awake");

			MmRelaySwitchNode = GetComponent <MmRelaySwitchNode>();

            //This is to avoid the situation where the MmRelaySwitchNode is started
            //  before this script is.
			if(MmRelaySwitchNode.Initialized)
			{
				OnMmSwitchNodeAwakeCompleteCallback ();
			}
			else
			{
				MmRelaySwitchNode.MmRegisterAwakeCompleteCallback(OnMmSwitchNodeAwakeCompleteCallback);
			}

            MmRelaySwitchNode.MmRegisterStartCompleteCallback(OnMmNodeSwitchStartCompleteCallback);

            if (MmRelaySwitchNode.MmNetworkResponder == null ||
                MmRelaySwitchNode.MmNetworkResponder.IsActiveAndEnabled)
            {
                MmRegisterStartCompleteCallback(MmSwitchSetup);
            }
            else
            {
                MmRelaySwitchNode.MmNetworkResponder.MmRegisterStartCompleteCallback(MmSwitchSetup);
            }

            base.Awake ();
		}

        /// <summary>
        /// Assign Global enter/exit Delegates for the MmRelaySwitchNode's FSM.
        /// </summary>
	    public override void Start()
	    {
            MmLogger.LogResponder("MmSwitchResponder Start");

            // Assign default responder for transitioning between MmRelayNodes
            MmRelaySwitchNode.RespondersFSM.GlobalExit += delegate
            {
                MmLogger.LogApplication("MmRelaySwitchNode GlobalExit");

                MmRelaySwitchNode.Current.MmInvoke(MmMethod.SetActive, false,
                    new MmMetadataBlock(MmLevelFilterHelper.Default, MmActiveFilter.All,
                    default(MmSelectedFilter), MmNetworkFilter.Local));
            };

            MmRelaySwitchNode.RespondersFSM.GlobalEnter += delegate
            {
                MmLogger.LogApplication("MmRelaySwitchNode GlobalEnter");
                
                MmRelaySwitchNode.Current.MmInvoke(MmMethod.SetActive, true,
                    new MmMetadataBlock(MmLevelFilterHelper.Default, MmActiveFilter.All,
                    default(MmSelectedFilter), MmNetworkFilter.Local));
            };

            base.Start();
	    }

        /// <summary>
        /// Callback called after associated MmRelaySwitchNode awoken.
        /// </summary>
		public virtual void OnMmSwitchNodeAwakeCompleteCallback()
		{
            MmLogger.LogResponder("OnMmSwitchNodeAwakeComplete invoked");
		}


        /// <summary>
        /// Callback called after associated MmRelaySwitchNode started.
        /// </summary>
		public virtual void OnMmNodeSwitchStartCompleteCallback()
		{
            MmLogger.LogResponder("OnMmNodeSwitchStartComplete invoked");

			if (MmRelaySwitchNode.RoutingTable.Count == 0) {
                MmLogger.LogResponder("No MmRelayNodes in RespondersFSM on: " + gameObject.name);
			}
		}

        /// <summary>
        /// This is called when the MmNetworkResponder is started
        /// Important Note: this will only trigger locally. The reason for this is that
        /// the following code needs to execute on every instance of the MmRelayNode across the network
        /// Thus, in order to avoid triggering the message each time a client connects, we just trigger the message locally.
        /// </summary>
	    public virtual void MmSwitchSetup()
	    {
			MmLogger.LogResponder(gameObject.name + " MmSwitchSetup Invoked.");

            MmRelaySwitchNode.MmInvoke(MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All,
                    default(MmSelectedFilter), MmNetworkFilter.Local));

            MmRelaySwitchNode.MmInvoke(MmMethod.SetActive, false,
                new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All,
                    default(MmSelectedFilter), MmNetworkFilter.Local));

            if (InitialState != "")
            {
                MmLogger.LogResponder(gameObject.name + " attempting to jump to: " + InitialState);
                MmRelaySwitchNode.MmInvoke(MmMethod.Switch,
                    MmRelaySwitchNode.RoutingTable[InitialState].Name,
                    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All, 
                    default(MmSelectedFilter), MmNetworkFilter.Local));
            }
        }

        /// <summary>
        /// This responder will receive the switch message triggered by an MmInvoke.
        /// It will then trigger the jump state in the FSM of the associated 
        /// MmRelaySwitchNode.
        /// </summary>
        /// <param name="iName">Name of state to jump to.</param>
		protected override void Switch(string iName)
		{
		    if (MmRelaySwitchNode) // This can be null if gameObject has not awoken
		        MmRelaySwitchNode.JumpTo(iName);
			//Debug.Log ("Jumping to: " + iName);

			base.Switch (iName);
		}
	}
}