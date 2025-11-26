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
    /// Wrapper objects for MmResponders for use in MmRoutingTables.
    /// </summary>
    [System.Serializable]
    public class MmRoutingTableItem
    {
        /// <summary>
        /// Name of the list item.
        /// These are not intended to be global.
        /// In their use in the editor, they will take the 
        /// name of the GameObject upon which the MmResponder was 
        /// placed.
        /// </summary>
        public string Name;

        /// <summary>
        /// The MmResponder and its derivations.
        /// </summary>
		public MmResponder Responder;

        /// <summary>
        /// Indicates whether the MmResponder should be cloned
        /// on start or if the MmRoutingTableItem should reference
        /// the original.
        /// </summary>
        public bool Clone;

        /// <summary>
        /// MmTags allow you to specify filters for execution in 
        /// MercuryMessaging hierarchies. <see cref="MmTag"/>
        /// </summary>
		public MmTag Tags;

        /// <summary>
        /// The level of the MmResponder relative to the container of the
        /// MmRoutingTable and MmRoutingTableItem.
        /// </summary>
		public MmLevelFilter Level;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MmRoutingTableItem()
        {}

        /// <summary>
        /// Create an MmRoutingTableItem.
        /// </summary>
        /// <param name="name">Name of the MmRoutingTableItem</param>
        /// <param name="responder">Reference to the MmResponder to be stored.</param>
		public MmRoutingTableItem(string name, MmResponder responder)
        {
            Name = name;
			Responder = responder;
        }

        /// <summary>
        /// Create an MmRoutingTableItem.
        /// </summary>
        /// <param name="name">Name of the MmRoutingTableItem</param>
        /// <param name="responder">Reference to the MmResponder to be stored.</param>
        /// <param name="clone">Whether to clone the MmResponder & GameObject
        /// or to use the original.</param>
		public MmRoutingTableItem(string name, MmResponder responder, bool clone)
        {
            Name = name;
			Responder = responder;
            Clone = clone;
        }

        /// <summary>
        /// Create an MmRoutingTableItem.
        /// </summary>
        /// <param name="name">Name of the MmRoutingTableItem</param>
        /// <param name="responder">Reference to the MmResponder to be stored.</param>
        /// <param name="clone">Whether to clone the MmResponder & GameObject
        /// or to use the original.</param>
        /// <param name="tags">Tags to apply to the MmRoutingTableItem.</param>
		public MmRoutingTableItem(string name, MmResponder responder, bool clone, MmTag tags)
        {
            Name = name;
			Responder = responder;
            Clone = clone;

            Tags = tags;
        }

        /// <summary>
        /// Method to clone a GameObject of MmRoutingTableItem.
        /// </summary>
        /// <returns>Handle to the new GameObject.</returns>
        public GameObject CloneGameObject()
        {
			GameObject cloneGameObject = UnityEngine.GameObject.Instantiate(Responder.gameObject);
			Responder = cloneGameObject.GetComponent<MmRelayNode>();

            return cloneGameObject;
        }
    }
}