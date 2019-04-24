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
using System.Linq;
using MercuryMessaging.Support.FiniteStateMachine;

namespace MercuryMessaging
{
    /// <summary>
    /// an MmRelayNode with an FSM to allow for limited MmResponder selection
    /// in the MmRelayNode's MmRoutingTable.
    /// </summary>
	public class MmRelaySwitchNode : MmRelayNode
    {
        /// <summary>
        /// The Finite state machine of the MmRelayNode's RoutingTable.
        /// </summary>
		public FiniteStateMachine<MmRoutingTableItem> RespondersFSM;
        
        /// <summary>
        /// FSM Current State
        /// </summary>
        public MmRelayNode Current
        {
            get { return RespondersFSM.Current.Responder.GetRelayNode(); }
        }

        /// <summary>
        /// MmRoutingTable name of FSM current state.
        /// </summary>
        public string CurrentName
        {
            get { return RespondersFSM.Current.Name; }
        }

        /// <summary>
        /// Converts the standard MmRoutingTable into a FSM.
        /// Calls MmOnAwakeComplete through MmResponder Awake.
        /// </summary>
        public override void Awake()
        {
            MmLogger.LogFramework(gameObject.name + " MmRelaySwitchNode Awake");

            try
            {
                RespondersFSM =
                    new FiniteStateMachine<MmRoutingTableItem>("RespondersFSM",
                        RoutingTable.Where(x => x.Responder is MmRelayNode && x.Level == MmLevelFilter.Child).ToList());
            }
            catch
            {
                MmLogger.LogError(gameObject.name + ": Failed bulding FSM. Missing Node?");
            }

            base.Awake ();
        }

        /// <summary>
        /// Calls MmOnStartComplete through MmResponder Start.
        /// </summary>
		public override void Start()
		{
            MmLogger.LogFramework(gameObject.name + " MmRelaySwitchNode Start");

			base.Start ();
		}

        /// <summary>
        /// Overrides MmRelayNode's Selected check to handle the FSM's current state.
        /// </summary>
        /// <param name="selectedFilter"><see cref="SelectedCheck"/></param>
        /// <param name="responder">Observed MmResponder.</param>
        /// <returns></returns>
	    protected override bool SelectedCheck(MmSelectedFilter selectedFilter, IMmResponder responder)
	    {
			return selectedFilter == MmSelectedFilter.All
			       || (RespondersFSM.Current != null
			           && responder.MmGameObject == RespondersFSM.Current.Responder.MmGameObject);
	    }

        /// <summary>
        /// FSM control method: Jump to State, using MmRoutingTableItem name.
        /// </summary>
        /// <param name="newState">Name of target state.</param>
        public virtual void JumpTo(string newState)
		{
			RespondersFSM.JumpTo(RoutingTable[newState]);
		}

        //TODO: Test this again
        /// <summary>
        /// FSM control method: Jump to State, using MmRoutingTableItem Responder reference.
        /// </summary>
        /// <param name="newState">Name of target state.</param>
		public virtual void JumpTo(MmRelayNode newState)
		{
			RespondersFSM.JumpTo(RoutingTable[newState]);
		}

        /// <summary>
        /// Accessor for particular state by name.
        /// </summary>
        /// <param name="state">Name of target state.</param>
        /// <returns>FSM State if present.</returns>
	    public StateEvents this[string state]
	    {
			get { return RespondersFSM[RoutingTable[state]]; }
			set { RespondersFSM[RoutingTable[state]] = value; }
	    }
    }
}