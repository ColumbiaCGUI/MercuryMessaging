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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================

using System.Collections.Generic;
using UnityEngine.Pool;

namespace MercuryMessaging
{
    /// <summary>
    /// Pool for HashSet&lt;int&gt; used by VisitedNodes in message routing.
    /// Eliminates per-message HashSet allocations in cycle detection.
    ///
    /// Usage:
    ///   var visitedNodes = MmHashSetPool.Get();
    ///   // ... use for cycle detection ...
    ///   MmHashSetPool.Return(visitedNodes);
    /// </summary>
    public static class MmHashSetPool
    {
        #region Configuration

        /// <summary>
        /// Default pool capacity.
        /// </summary>
        public const int DEFAULT_CAPACITY = 100;

        /// <summary>
        /// Maximum pool size.
        /// </summary>
        public const int MAX_SIZE = 1000;

        /// <summary>
        /// Initial capacity for each HashSet (typical hierarchy depth).
        /// </summary>
        private const int HASHSET_INITIAL_CAPACITY = 16;

        /// <summary>
        /// Enable collection checks in editor for debugging pool misuse.
        /// </summary>
        #if UNITY_EDITOR
        private const bool COLLECTION_CHECK = true;
        #else
        private const bool COLLECTION_CHECK = false;
        #endif

        #endregion

        #region Pool

        private static readonly ObjectPool<HashSet<int>> _pool = new ObjectPool<HashSet<int>>(
            createFunc: () => new HashSet<int>(HASHSET_INITIAL_CAPACITY),
            actionOnGet: set => set.Clear(), // Always start with empty set
            actionOnRelease: null,
            actionOnDestroy: null,
            collectionCheck: COLLECTION_CHECK,
            defaultCapacity: DEFAULT_CAPACITY,
            maxSize: MAX_SIZE
        );

        #endregion

        #region Public API

        /// <summary>
        /// Get a cleared HashSet from the pool.
        /// The returned set is guaranteed to be empty.
        /// </summary>
        /// <returns>An empty HashSet&lt;int&gt; for tracking visited nodes.</returns>
        public static HashSet<int> Get()
        {
            return _pool.Get();
        }

        /// <summary>
        /// Return a HashSet to the pool for reuse.
        /// </summary>
        /// <param name="set">The HashSet to return.</param>
        public static void Return(HashSet<int> set)
        {
            if (set != null)
            {
                _pool.Release(set);
            }
        }

        /// <summary>
        /// Get a HashSet from the pool and populate it with existing values.
        /// Used when copying VisitedNodes from one message to another.
        /// </summary>
        /// <param name="source">Source HashSet to copy from (can be null).</param>
        /// <returns>A new HashSet containing copied values.</returns>
        public static HashSet<int> GetCopy(HashSet<int> source)
        {
            var set = _pool.Get();
            if (source != null)
            {
                foreach (var id in source)
                {
                    set.Add(id);
                }
            }
            return set;
        }

        #endregion

        #region Statistics (Editor Only)

        #if UNITY_EDITOR
        /// <summary>
        /// Get pool statistics for debugging.
        /// </summary>
        public static string GetStatistics()
        {
            return $"MmHashSetPool: {_pool.CountActive} active, {_pool.CountInactive} inactive";
        }

        /// <summary>
        /// Current number of HashSets in use.
        /// </summary>
        public static int ActiveCount => _pool.CountActive;

        /// <summary>
        /// Current number of HashSets available in pool.
        /// </summary>
        public static int InactiveCount => _pool.CountInactive;
        #endif

        #endregion
    }
}
