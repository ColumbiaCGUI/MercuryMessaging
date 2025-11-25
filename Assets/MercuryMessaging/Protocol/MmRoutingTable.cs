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
using MercuryMessaging.Support.Extensions;

namespace MercuryMessaging
{
    /// <summary>
    /// Cache key for filter result caching (QW-3).
    /// Combines ListFilter and MmLevelFilter for efficient lookup.
    /// </summary>
    internal struct MmFilterCacheKey : System.IEquatable<MmFilterCacheKey>
    {
        public readonly MmRoutingTable.ListFilter ListFilter;
        public readonly MmLevelFilter LevelFilter;

        public MmFilterCacheKey(MmRoutingTable.ListFilter listFilter, MmLevelFilter levelFilter)
        {
            ListFilter = listFilter;
            LevelFilter = levelFilter;
        }

        public override int GetHashCode()
        {
            // Combine hash codes using prime multiplication
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (int)ListFilter;
                hash = hash * 31 + (int)LevelFilter;
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is MmFilterCacheKey && Equals((MmFilterCacheKey)obj);
        }

        public bool Equals(MmFilterCacheKey other)
        {
            return ListFilter == other.ListFilter && LevelFilter == other.LevelFilter;
        }
    }

    /// <summary>
    /// A form of Reorderable List <see cref="ReorderableList{T}"/>
    ///     specifically for all derivations of MmResponder.
    /// Includes LRU cache for filtered responder lists (QW-3 optimization).
    /// </summary>
    [System.Serializable]
    public class MmRoutingTable : ReorderableList<MmRoutingTableItem>
    {
        /// <summary>
        /// Useful for extracting certain types of MmResponders from the list.
        /// </summary>
        public enum ListFilter { All = 0, RelayNodeOnly, ResponderOnly };

        #region Filter Result Caching (QW-3)

        /// <summary>
        /// Maximum number of cached filter results.
        /// Prevents unbounded memory growth while maintaining high hit rate.
        /// </summary>
        private const int MAX_CACHE_SIZE = 100;

        /// <summary>
        /// Cache for filtered responder lists.
        /// Key: combination of ListFilter and MmLevelFilter
        /// Value: pre-filtered list of routing table items
        /// </summary>
        [System.NonSerialized]
        private Dictionary<MmFilterCacheKey, List<MmRoutingTableItem>> _filterCache;

        /// <summary>
        /// LRU tracking for cache eviction.
        /// Most recently used keys are at the end of the list.
        /// </summary>
        [System.NonSerialized]
        private LinkedList<MmFilterCacheKey> _lruOrder;

        /// <summary>
        /// Cache statistics for profiling (optional).
        /// </summary>
        [System.NonSerialized]
        private int _cacheHits = 0;
        [System.NonSerialized]
        private int _cacheMisses = 0;

        /// <summary>
        /// Get cache hit rate (0.0 to 1.0).
        /// Returns 0 if no cache operations yet.
        /// </summary>
        public float CacheHitRate
        {
            get
            {
                int total = _cacheHits + _cacheMisses;
                return total > 0 ? (float)_cacheHits / total : 0f;
            }
        }

        /// <summary>
        /// Initialize cache structures if needed.
        /// </summary>
        private void EnsureCacheInitialized()
        {
            if (_filterCache == null)
            {
                _filterCache = new Dictionary<MmFilterCacheKey, List<MmRoutingTableItem>>(MAX_CACHE_SIZE);
                _lruOrder = new LinkedList<MmFilterCacheKey>();
            }
        }

        /// <summary>
        /// Clear all cached filter results.
        /// Called when routing table is modified (add/remove responder).
        /// </summary>
        private void InvalidateCache()
        {
            if (_filterCache != null)
            {
                _filterCache.Clear();
                _lruOrder.Clear();
            }
        }

        /// <summary>
        /// Update LRU tracking for cache key.
        /// Moves key to end of LRU list (most recently used position).
        /// </summary>
        /// <param name="key">Cache key that was accessed</param>
        private void TouchCacheKey(MmFilterCacheKey key)
        {
            // Remove from current position (if present)
            _lruOrder.Remove(key);
            // Add to end (most recently used)
            _lruOrder.AddLast(key);
        }

        /// <summary>
        /// Evict least recently used cache entry if cache is full.
        /// </summary>
        private void EvictLRUIfNeeded()
        {
            if (_filterCache.Count >= MAX_CACHE_SIZE && _lruOrder.Count > 0)
            {
                // Remove least recently used (first in LRU list)
                var lruKey = _lruOrder.First.Value;
                _lruOrder.RemoveFirst();
                _filterCache.Remove(lruKey);
            }
        }

        #endregion

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
            // Use cached filtered items and extract names (removed LINQ for consistency with QW-5)
            var items = GetMmRoutingTableItems(filter, levelFilter);
            var names = new List<string>(items.Count);
            foreach (var item in items)
            {
                names.Add(item.Name);
            }
            return names;
        }

        /// <summary>
        /// Get a list of all MmRoutingTableItems that
        /// match the provided filters.
        /// Uses LRU cache for performance (QW-3 optimization).
        /// </summary>
        /// <param name="filter">ListFilter <see cref="ListFilter"/></param>
        /// <param name="levelFilter">LevelFilter <see cref="MmLevelFilter"/></param>
        /// <returns>List of MmRoutingTableItems that pass filter checks.</returns>
        public List<MmRoutingTableItem> GetMmRoutingTableItems(
            ListFilter filter = default(ListFilter),
            MmLevelFilter levelFilter = MmLevelFilterHelper.Default)
        {
            // Initialize cache on first use
            EnsureCacheInitialized();

            // Create cache key from filter parameters
            var cacheKey = new MmFilterCacheKey(filter, levelFilter);

            // Check cache for existing result
            if (_filterCache.TryGetValue(cacheKey, out var cachedResult))
            {
                // Cache hit - update LRU and return cached list
                _cacheHits++;
                TouchCacheKey(cacheKey);
                return cachedResult;
            }

            // Cache miss - compute result by filtering routing table
            _cacheMisses++;

            // Manual filtering instead of LINQ (removed Where().ToList() for QW-5 consistency)
            var result = new List<MmRoutingTableItem>();
            foreach (var item in this)
            {
                if (CheckFilter(item, filter, levelFilter))
                {
                    result.Add(item);
                }
            }

            // Store in cache
            EvictLRUIfNeeded();
            _filterCache[cacheKey] = result;
            TouchCacheKey(cacheKey);

            return result;
        }

        /// Get a list of all MmRoutingTableItems that reference MmRelayNodes.
        /// <returns>List of all MmRoutingTableItems that reference MmRelayNodes.</returns>
        public List<MmRelayNode> GetOnlyMmRelayNodes()
        {
            // Manual filtering instead of LINQ (removed Where().Select().ToList() for QW-5 consistency)
            var relayNodes = new List<MmRelayNode>();
            foreach (var item in this)
            {
                if (item.Responder is MmRelayNode)
                {
                    relayNodes.Add((MmRelayNode)item.Responder);
                }
            }
            return relayNodes;
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

        #region Cache Invalidation Overrides (QW-3)

        /// <summary>
        /// Override Add to invalidate cache when routing table changes.
        /// </summary>
        public new void Add(MmRoutingTableItem item)
        {
            base.Add(item);
            InvalidateCache();
        }

        /// <summary>
        /// Override Remove to invalidate cache when routing table changes.
        /// </summary>
        public new bool Remove(MmRoutingTableItem item)
        {
            bool removed = base.Remove(item);
            if (removed)
            {
                InvalidateCache();
            }
            return removed;
        }

        /// <summary>
        /// Override RemoveAt to invalidate cache when routing table changes.
        /// </summary>
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            InvalidateCache();
        }

        /// <summary>
        /// Override Insert to invalidate cache when routing table changes.
        /// </summary>
        public new void Insert(int index, MmRoutingTableItem item)
        {
            base.Insert(index, item);
            InvalidateCache();
        }

        /// <summary>
        /// Override Clear to invalidate cache when routing table changes.
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            InvalidateCache();
        }

        /// <summary>
        /// Override indexer setter to invalidate cache when routing table changes.
        /// </summary>
        public new MmRoutingTableItem this[int index]
        {
            get { return base[index]; }
            set
            {
                base[index] = value;
                InvalidateCache();
            }
        }

        #endregion
    }
}
