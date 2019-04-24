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
using System.Linq;
using MercuryMessaging.Support.Extensions;

namespace MercuryMessaging
{
    /// <summary>
    /// A form of Reorderable List <see cref="ReorderableList{T}"/>
    ///     specifically for all derivations of MmResponder.
    /// </summary>
    [System.Serializable]
    public class MmRoutingTable : ReorderableList<MmRoutingTableItem>
    {
        /// <summary>
        /// Useful for extracting certain types of MmResponders from the list.
        /// </summary>
        public enum ListFilter { All = 0, RelayNodeOnly, ResponderOnly };

        /// <summary>
        /// Accessor for MmRoutingTableItems by name.
        /// Will throw KeyNotFoundException if not found.
        /// </summary>
        /// <param name="name">Name of MmRoutingTableItem.</param>
        /// <returns>First item with the name.</returns>
        public MmRoutingTableItem this[string name]
        {
            get { return _list.Find(item => item.Name == name); }
            set
            {
                MmRoutingTableItem refVal = this[name];

                if (refVal == null)
                {
                    throw new KeyNotFoundException();
                }
                int itemIndex = _list.IndexOf(refVal);
                _list[itemIndex] = value;
            }
        }

        /// <summary>
        /// Accessor for MmRoutingTableItems by MmResponder reference.
        /// </summary>
        /// <param name="responder">MmResponder for which to search.</param>
        /// <returns>MmRoutingTableItem with reference or NULL.</returns>
        public MmRoutingTableItem this[MmResponder responder]
        {
            get { return _list.Find(item => item.Responder == responder); }
        }

        /// <summary>
        /// Get a list of the names all MmRoutingTableItems that 
        /// match the provided filters.
        /// </summary>
        /// <param name="filter">ListFilter <see cref="ListFilter"/></param>
        /// <param name="levelFilter">LevelFilter <see cref="MmLevelFilter"/></param>
        /// <returns>List of names of MmRoutingTableItems that pass filter checks.</returns>
        public List<string> GetMmNames(ListFilter filter = default(ListFilter),
            MmLevelFilter levelFilter = MmLevelFilterHelper.Default)
        {
            return GetMmRoutingTableItems(filter, levelFilter).
                    Select(x => x.Name).ToList();
        }

        /// <summary>
        /// Get a list of all MmRoutingTableItems that 
        /// match the provided filters.
        /// </summary>
        /// <param name="filter">ListFilter <see cref="ListFilter"/></param>
        /// <param name="levelFilter">LevelFilter <see cref="MmLevelFilter"/></param>
        /// <returns>List of MmRoutingTableItems that pass filter checks.</returns>
        public List<MmRoutingTableItem> GetMmRoutingTableItems(
            ListFilter filter = default(ListFilter),
            MmLevelFilter levelFilter = MmLevelFilterHelper.Default)
        {
            return this.Where(x => CheckFilter(x, filter, levelFilter)).ToList();
        }

        /// Get a list of all MmRoutingTableItems that reference MmRelayNodes.
        /// <returns>List of all MmRoutingTableItems that reference MmRelayNodes.</returns>
        public List<MmRelayNode> GetOnlyMmRelayNodes()
        {
            return this.Where(x => x.Responder is MmRelayNode).
                Select(x => (MmRelayNode)(x.Responder)).ToList();
        }

        /// <summary>
        /// Checks whether the MmRoutingTable contains an item with the provided name.
        /// </summary>
        /// <param name="key">Name for which to search.</param>
        /// <returns>Whether the MmRoutingTable contains an item with 
        /// the provided name.</returns>
        public bool ContainsKey(string key)
        {
            return (this[key] != null);
        }

        /// <summary>
        /// Checks whether the MmRoutingTable contains an item with the 
        /// provided MmResponder reference.
        /// </summary>
        /// <param name="responder">MmResponder for which to search.</param>
        /// <returns>Whether the MmRoutingTable contains an item with the 
        /// provided MmResponder reference.</returns>
        public bool Contains(MmResponder responder)
        {
            return (this[responder] != null);
        }

        /// <summary>
        /// Checks the provided MmRoutingTableItem to see
        /// whether it passes the list filter requirements.
        /// </summary>
        /// <param name="item">Observed MmRoutingTableItem.</param>
        /// <param name="listFilter">ListFilter <see cref="ListFilter"/></param>
        /// <param name="levelFilter">LevelFilter <see cref="MmLevelFilter"/></param>
        /// <returns>Whether MmRoutingTableItem passes filter check.</returns>
        public bool CheckFilter(MmRoutingTableItem item, 
            ListFilter listFilter, MmLevelFilter levelFilter)
        {
            //Level Check
            if ((levelFilter & item.Level) == 0)
                return false;

            //List Filter check
            if (listFilter == ListFilter.RelayNodeOnly && !(item.Responder is MmRelayNode))
                return false;
            if (listFilter == ListFilter.ResponderOnly && item.Responder is MmRelayNode)
                return false;

            //All conditions passed, return true
            return true;
        }
    }
}
