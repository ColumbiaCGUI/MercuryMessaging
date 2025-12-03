// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Routing Optimization - Message History Cache
// Part of Phase 2.1: Advanced Message Routing

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Data
{
    /// <summary>
    /// Time-windowed LRU cache for tracking visited relay nodes in message routing.
    /// Prevents circular routing more effectively than hop counting alone.
    ///
    /// Design:
    /// - Hybrid approach: HashSet for O(1) lookups + LinkedList for LRU eviction
    /// - Time-based eviction: Entries older than window are automatically removed
    /// - Bounded memory: Evicts old entries to maintain constant memory footprint
    ///
    /// Performance:
    /// - Add: O(1) amortized (eviction only when needed)
    /// - Contains: O(1) always
    /// - Memory: ~24 bytes per entry (node ID + timestamp + pointers)
    ///
    /// Thread Safety: Not thread-safe (designed for Unity main thread usage)
    /// </summary>
    public class MmMessageHistoryCache
    {
        #region Private Types

        /// <summary>
        /// Cache entry combining node ID with timestamp for time-based eviction.
        /// </summary>
        private class CacheEntry
        {
            public int NodeId;
            public float Timestamp;  // Time.realtimeSinceStartup at add time

            public CacheEntry(int nodeId, float timestamp)
            {
                NodeId = nodeId;
                Timestamp = timestamp;
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Fast O(1) lookup for visited node IDs.
        /// </summary>
        private readonly HashSet<int> _visitedNodes = new HashSet<int>();

        /// <summary>
        /// LRU ordering for time-based eviction (oldest entries first).
        /// </summary>
        private readonly LinkedList<CacheEntry> _evictionQueue = new LinkedList<CacheEntry>();

        /// <summary>
        /// Map from node ID to LinkedListNode for O(1) removal during updates.
        /// </summary>
        private readonly Dictionary<int, LinkedListNode<CacheEntry>> _nodeMap =
            new Dictionary<int, LinkedListNode<CacheEntry>>();

        /// <summary>
        /// Time window in seconds (converted from milliseconds).
        /// </summary>
        private readonly float _windowSizeSeconds;

        /// <summary>
        /// Counter for eviction operations (statistics).
        /// </summary>
        private int _evictionCount = 0;

        /// <summary>
        /// Counter for add operations (statistics).
        /// </summary>
        private int _addCount = 0;

        /// <summary>
        /// Timestamp of last eviction check (optimize eviction frequency).
        /// </summary>
        private float _lastEvictionCheck = 0f;

        /// <summary>
        /// Minimum time between eviction sweeps (seconds).
        /// Prevents excessive eviction checks when adding many nodes rapidly.
        /// </summary>
        private const float EVICTION_CHECK_INTERVAL = 0.01f; // 10ms

        #endregion

        #region Properties

        /// <summary>
        /// Number of node IDs currently in cache.
        /// </summary>
        public int Count => _visitedNodes.Count;

        /// <summary>
        /// Total number of evictions performed (statistics).
        /// </summary>
        public int EvictionCount => _evictionCount;

        /// <summary>
        /// Total number of adds performed (statistics).
        /// </summary>
        public int AddCount => _addCount;

        /// <summary>
        /// Cache hit rate = (lookups that found node) / (total lookups).
        /// Updated externally by caller tracking Contains() results.
        /// </summary>
        public float HitRate { get; set; } = 0f;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates message history cache with specified time window.
        /// </summary>
        /// <param name="windowSizeMs">Time window in milliseconds (default: 100ms)</param>
        public MmMessageHistoryCache(int windowSizeMs = 100)
        {
            if (windowSizeMs < 0)
                throw new ArgumentException("Window size must be >= 0", nameof(windowSizeMs));

            _windowSizeSeconds = windowSizeMs / 1000.0f;
            _lastEvictionCheck = Time.realtimeSinceStartup;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds a node ID to the visited set with current timestamp.
        /// If node already exists, updates its timestamp (refreshes TTL).
        /// </summary>
        /// <param name="nodeId">Relay node instance ID</param>
        public void Add(int nodeId)
        {
            float now = Time.realtimeSinceStartup;
            _addCount++;

            // If node already visited, refresh its timestamp (move to end of queue)
            if (_visitedNodes.Contains(nodeId))
            {
                if (_nodeMap.TryGetValue(nodeId, out var existingNode))
                {
                    // Remove from current position
                    _evictionQueue.Remove(existingNode);

                    // Add to end with updated timestamp
                    var newEntry = new CacheEntry(nodeId, now);
                    var newNode = _evictionQueue.AddLast(newEntry);
                    _nodeMap[nodeId] = newNode;
                }
                return;
            }

            // Add new node
            _visitedNodes.Add(nodeId);
            var entry = new CacheEntry(nodeId, now);
            var listNode = _evictionQueue.AddLast(entry);
            _nodeMap[nodeId] = listNode;

            // Evict old entries periodically (amortized cost)
            if (now - _lastEvictionCheck >= EVICTION_CHECK_INTERVAL)
            {
                EvictOldEntries(now);
                _lastEvictionCheck = now;
            }
        }

        /// <summary>
        /// Checks if a node ID has been visited (within time window).
        /// </summary>
        /// <param name="nodeId">Relay node instance ID</param>
        /// <returns>True if node was visited recently, false otherwise</returns>
        public bool Contains(int nodeId)
        {
            return _visitedNodes.Contains(nodeId);
        }

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear()
        {
            _visitedNodes.Clear();
            _evictionQueue.Clear();
            _nodeMap.Clear();
            _evictionCount = 0;
            _addCount = 0;
        }

        /// <summary>
        /// Forces immediate eviction of old entries (normally done lazily during Add).
        /// Useful for testing or manual cache management.
        /// </summary>
        public void EvictNow()
        {
            EvictOldEntries(Time.realtimeSinceStartup);
        }

        /// <summary>
        /// Gets cache statistics for profiling/debugging.
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                CurrentSize = Count,
                TotalAdds = AddCount,
                TotalEvictions = EvictionCount,
                HitRate = HitRate,
                WindowSizeMs = (int)(_windowSizeSeconds * 1000),
                OldestEntryAgeMs = GetOldestEntryAge()
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Evicts entries older than the time window.
        /// Iterates from oldest (front) until hitting entry within window.
        /// </summary>
        private void EvictOldEntries(float currentTime)
        {
            float evictionThreshold = currentTime - _windowSizeSeconds;

            // Evict from front of queue (oldest entries)
            while (_evictionQueue.Count > 0)
            {
                var oldestEntry = _evictionQueue.First.Value;

                // Stop when we hit entry within window
                if (oldestEntry.Timestamp >= evictionThreshold)
                    break;

                // Remove from all data structures
                _visitedNodes.Remove(oldestEntry.NodeId);
                _nodeMap.Remove(oldestEntry.NodeId);
                _evictionQueue.RemoveFirst();
                _evictionCount++;
            }
        }

        /// <summary>
        /// Gets age of oldest entry in milliseconds (for statistics).
        /// Returns 0 if cache is empty.
        /// </summary>
        private int GetOldestEntryAge()
        {
            if (_evictionQueue.Count == 0)
                return 0;

            float now = Time.realtimeSinceStartup;
            float oldestTime = _evictionQueue.First.Value.Timestamp;
            return (int)((now - oldestTime) * 1000);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Cache statistics snapshot for profiling/debugging.
        /// </summary>
        public struct CacheStatistics
        {
            public int CurrentSize;       // Current number of entries
            public int TotalAdds;         // Total Add() calls
            public int TotalEvictions;    // Total evictions performed
            public float HitRate;         // Cache hit rate (0.0 to 1.0)
            public int WindowSizeMs;      // Configured window size
            public int OldestEntryAgeMs;  // Age of oldest entry (ms)

            public override string ToString()
            {
                return $"MmMessageHistoryCache: Size={CurrentSize}, " +
                       $"Adds={TotalAdds}, Evictions={TotalEvictions}, " +
                       $"HitRate={HitRate:P1}, Window={WindowSizeMs}ms, " +
                       $"OldestAge={OldestEntryAgeMs}ms";
            }
        }

        #endregion

        #region Debug Helpers

        public override string ToString()
        {
            return GetStatistics().ToString();
        }

        #endregion
    }
}
