// Copyright (c) 2017-2025, Columbia University
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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//
//
// Suppress MM015: Filter equality checks are intentional for exact match routing logic
#pragma warning disable MM015

using System.Collections.Generic;
using UnityEngine;

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

        #region Cached State (P1, P3)

        // Cached values for performance - updated on state change
        private MmRelayNode _cachedCurrent;
        private string _cachedCurrentName;
        private GameObject _currentStateGameObject;

        /// <summary>
        /// FSM Current State (cached for performance)
        /// </summary>
        public MmRelayNode Current => _cachedCurrent;

        /// <summary>
        /// MmRoutingTable name of FSM current state (cached for performance)
        /// </summary>
        public string CurrentName => _cachedCurrentName;

        /// <summary>
        /// Updates cached state values. Called on FSM state changes.
        /// </summary>
        private void UpdateCurrentCache()
        {
            var currentItem = RespondersFSM?.Current;
            if (currentItem != null)
            {
                _cachedCurrent = currentItem.Responder?.GetRelayNode();
                _cachedCurrentName = currentItem.Name;
                _currentStateGameObject = currentItem.Responder?.MmGameObject;
            }
            else
            {
                _cachedCurrent = null;
                _cachedCurrentName = null;
                _currentStateGameObject = null;
            }
        }

        #endregion

        #region FSM State Buffer (P2)

        // Reusable buffer for FSM state collection - avoids LINQ allocations
        private readonly List<MmRoutingTableItem> _fsmStateBuffer = new List<MmRoutingTableItem>();

        /// <summary>
        /// Gets child relay nodes from routing table without LINQ allocation.
        /// </summary>
        private List<MmRoutingTableItem> GetChildRelayNodes()
        {
            _fsmStateBuffer.Clear();
            foreach (var item in RoutingTable)
            {
                if (item.Responder is MmRelayNode && item.Level == MmLevelFilter.Child)
                {
                    _fsmStateBuffer.Add(item);
                }
            }
            return _fsmStateBuffer;
        }

        #endregion

        /// <summary>
        /// Converts the standard MmRoutingTable into a FSM.
        /// Calls MmOnAwakeComplete through MmResponder Awake.
        /// </summary>
        public override void Awake()
        {
            MmLogger.LogFramework(gameObject.name + " MmRelaySwitchNode Awake");

            try
            {
                // P2: Use non-allocating GetChildRelayNodes() instead of LINQ
                RespondersFSM = new FiniteStateMachine<MmRoutingTableItem>("RespondersFSM", GetChildRelayNodes());

                // Subscribe to state changes to update cache
                RespondersFSM.GlobalEnter += OnFsmStateChanged;

                // Initialize cache
                UpdateCurrentCache();
            }
            catch
            {
                MmLogger.LogError(gameObject.name + ": Failed building FSM. Missing Node?");
            }

            base.Awake();
        }

        /// <summary>
        /// Called when FSM state changes. Updates cached values.
        /// </summary>
        private void OnFsmStateChanged()
        {
            UpdateCurrentCache();
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
        /// P1: Uses cached _currentStateGameObject for performance.
        /// </summary>
        /// <param name="selectedFilter"><see cref="SelectedCheck"/></param>
        /// <param name="responder">Observed MmResponder.</param>
        /// <returns></returns>
	    protected override bool SelectedCheck(MmSelectedFilter selectedFilter, IMmResponder responder)
	    {
            // P1: Use cached GameObject instead of traversing FSM.Current.Responder.MmGameObject
			return selectedFilter == MmSelectedFilter.All
			       || (_currentStateGameObject != null
			           && responder.MmGameObject == _currentStateGameObject);
	    }

        /// <summary>
        /// Rebuild the FSM from the current routing table.
        /// Use this after manually adding items to the routing table at runtime.
        /// </summary>
        public virtual void RebuildFSM()
        {
            try
            {
                // Unsubscribe from old FSM if exists
                if (RespondersFSM != null)
                {
                    RespondersFSM.GlobalEnter -= OnFsmStateChanged;
                }

                // P2: Use non-allocating GetChildRelayNodes() instead of LINQ
                RespondersFSM = new FiniteStateMachine<MmRoutingTableItem>("RespondersFSM", GetChildRelayNodes());

                // Subscribe to state changes
                RespondersFSM.GlobalEnter += OnFsmStateChanged;

                // Update cache after rebuild
                UpdateCurrentCache();
            }
            catch
            {
                MmLogger.LogError(gameObject.name + ": Failed rebuilding FSM. Missing Node?");
            }
        }

        /// <summary>
        /// FSM control method: Jump to State, using MmRoutingTableItem name.
        /// </summary>
        /// <param name="newState">Name of target state.</param>
        public virtual void JumpTo(string newState)
		{
			RespondersFSM.JumpTo(RoutingTable[newState]);
		}

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