// Copyright (c) 2017, Columbia University 
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

using UnityEditor;

namespace MercuryMessaging
{
    /// <summary>
    /// Custom Editor for MmResponder.
    /// </summary>
    [CustomEditor(typeof(MmResponder), true)]
    public class MmBehaviorEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Reference to the item's MmRelayNode.
        /// </summary>
        private MmRelayNode _myRelayNode;
        /// <summary>
        /// Reference to the item's MmRoutingTableItem.
        /// </summary>
        private MmRoutingTableItem myListItem;

        protected void OnEnable()
        {
            var mxmBehavior = (MmResponder)target;
            _myRelayNode = mxmBehavior.GetComponent<MmRelayNode>();
            if (_myRelayNode == null || _myRelayNode == target) return;
            myListItem = _myRelayNode.MmAddToRoutingTable(mxmBehavior, mxmBehavior.name);
        }

        protected void OnDisable()
        {
            // Not used at the moment
        }

        protected void OnDestroy()
        {
            // Should this be in OnDisable()?
            if (_myRelayNode == null) return;

            _myRelayNode.MmRefreshResponders();
        }

        public override void OnInspectorGUI() {
            // Show default inspector property editor
            DrawDefaultInspector ();
        }
    }
}
