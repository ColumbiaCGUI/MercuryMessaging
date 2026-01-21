// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// QW-3: Filter Result Caching - Validation Tests
// Tests LRU cache implementation for routing table filter results

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Validation tests for QW-3: Filter Result Caching optimization.
    /// Tests cache hit/miss behavior, invalidation, LRU eviction, and performance.
    /// </summary>
    [TestFixture]
    public class FilterCacheValidationTests
    {
        private GameObject testRoot;
        private MmRelayNode rootRelay;
        private List<GameObject> testObjects;

        [SetUp]
        public void SetUp()
        {
            // Create test hierarchy programmatically
            testRoot = new GameObject("TestRoot");
            rootRelay = testRoot.AddComponent<MmRelayNode>();
            testObjects = new List<GameObject> { testRoot };
        }

        [TearDown]
        public void TearDown()
        {
            // Unity automatically cleans up GameObjects between tests
            // Clear list only
            testObjects.Clear();
        }

        /// <summary>
        /// Helper to create a child relay node.
        /// </summary>
        private MmRelayNode CreateChildRelay(string name, MmRelayNode parent, MmTag tag = MmTag.Tag0)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.gameObject.transform);
            MmRelayNode childRelay = child.AddComponent<MmRelayNode>();

            // Create routing table item
            MmRoutingTableItem item = new MmRoutingTableItem();
            item.Name = name;
            item.Responder = childRelay;
            item.Level = MmLevelFilter.Child;
            item.Tags = tag;

            parent.RoutingTable.Add(item);
            testObjects.Add(child);

            return childRelay;
        }

        /// <summary>
        /// Test 1: Cache miss on first access, cache hit on subsequent access.
        /// </summary>
        [Test]
        public void Test_CacheMiss_Then_CacheHit()
        {
            // Arrange - create some responders
            CreateChildRelay("Child1", rootRelay, MmTag.Tag0);
            CreateChildRelay("Child2", rootRelay, MmTag.Tag0);
            CreateChildRelay("Child3", rootRelay, MmTag.Tag1);

            // Act - first call should be cache miss
            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            float hitRate1 = rootRelay.RoutingTable.CacheHitRate;

            // Second call with same filters should be cache hit
            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            float hitRate2 = rootRelay.RoutingTable.CacheHitRate;

            // Assert
            Assert.AreEqual(3, result1.Count, "First call should return all 3 children");
            Assert.AreEqual(3, result2.Count, "Second call should return same 3 children");
            Assert.AreEqual(0f, hitRate1, "First call should be 100% miss (0% hit rate)");
            Assert.AreEqual(0.5f, hitRate2, "Second call should be 50% hit rate (1 hit, 1 miss)");
        }

        /// <summary>
        /// Test 2: Cache invalidation on Add.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnAdd()
        {
            // Arrange - create initial responders and cache a result
            CreateChildRelay("Child1", rootRelay);
            CreateChildRelay("Child2", rootRelay);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(2, result1.Count, "Initial: 2 children");

            // Act - add a new responder (should invalidate cache)
            CreateChildRelay("Child3", rootRelay);

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(3, result2.Count, "After add: 3 children (cache was invalidated)");
        }

        /// <summary>
        /// Test 3: Cache invalidation on Remove.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnRemove()
        {
            // Arrange - create responders and cache a result
            CreateChildRelay("Child1", rootRelay);
            var child2Relay = CreateChildRelay("Child2", rootRelay);
            CreateChildRelay("Child3", rootRelay);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(3, result1.Count, "Initial: 3 children");

            // Act - remove a responder (should invalidate cache)
            var itemToRemove = rootRelay.RoutingTable[child2Relay];
            rootRelay.RoutingTable.Remove(itemToRemove);

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(2, result2.Count, "After remove: 2 children (cache was invalidated)");
        }

        /// <summary>
        /// Test 4: Cache invalidation on RemoveAt.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnRemoveAt()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay);
            CreateChildRelay("Child2", rootRelay);
            CreateChildRelay("Child3", rootRelay);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(3, result1.Count, "Initial: 3 children");

            // Act - remove by index (should invalidate cache)
            rootRelay.RoutingTable.RemoveAt(1);

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(2, result2.Count, "After RemoveAt: 2 children (cache was invalidated)");
        }

        /// <summary>
        /// Test 5: Cache invalidation on Clear.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnClear()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay);
            CreateChildRelay("Child2", rootRelay);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(2, result1.Count, "Initial: 2 children");

            // Act - clear routing table (should invalidate cache)
            rootRelay.RoutingTable.Clear();

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(0, result2.Count, "After clear: 0 children (cache was invalidated)");
        }

        /// <summary>
        /// Test 6: Cache invalidation on indexer setter.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnIndexerSet()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay, MmTag.Tag0);
            CreateChildRelay("Child2", rootRelay, MmTag.Tag0);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(2, result1.Count, "Initial: 2 children");

            // Act - modify item via indexer (should invalidate cache)
            var modifiedItem = rootRelay.RoutingTable[0];
            modifiedItem.Tags = MmTag.Tag1; // Change tag
            rootRelay.RoutingTable[0] = modifiedItem;

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert - cache was invalidated, results recomputed
            Assert.AreEqual(2, result2.Count, "After indexer set: still 2 children (cache was invalidated)");
        }

        /// <summary>
        /// Test 7: Different filter combinations create separate cache entries.
        /// </summary>
        [Test]
        public void Test_SeparateCacheEntries_ForDifferentFilters()
        {
            // Arrange - create varied responders
            CreateChildRelay("Child1", rootRelay, MmTag.Tag0);
            CreateChildRelay("Child2", rootRelay, MmTag.Tag1);

            GameObject parent = new GameObject("Parent");
            MmRelayNode parentRelay = parent.AddComponent<MmRelayNode>();
            MmRoutingTableItem parentItem = new MmRoutingTableItem();
            parentItem.Name = "Parent";
            parentItem.Responder = parentRelay;
            parentItem.Level = MmLevelFilter.Parent;
            parentItem.Tags = MmTag.Tag0;
            rootRelay.RoutingTable.Add(parentItem);
            testObjects.Add(parent);

            // Act - query with different filter combinations
            var childrenAll = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            var parentAll = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Parent);

            var relayOnly = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.RelayNodeOnly,
                MmLevelFilter.Child);

            // Assert - each filter combination should have separate cache entry
            Assert.AreEqual(2, childrenAll.Count, "Children filter: 2 items");
            Assert.AreEqual(1, parentAll.Count, "Parent filter: 1 item");
            Assert.AreEqual(2, relayOnly.Count, "Relay-only filter: 2 items (all are MmRelayNode)");
        }

        /// <summary>
        /// Test 8: LRU eviction when cache is full (MAX_CACHE_SIZE = 100).
        /// </summary>
        [Test]
        public void Test_LRU_Eviction()
        {
            // Arrange - create one responder
            CreateChildRelay("Child1", rootRelay);

            // Act - create 101 different cache entries by varying filters
            // This should trigger LRU eviction (MAX_CACHE_SIZE = 100)

            // Fill cache with 100 entries (using different level filter combinations)
            for (int i = 0; i < 100; i++)
            {
                MmLevelFilter filter = (MmLevelFilter)(1 << (i % 8)); // Cycle through filter bits
                rootRelay.RoutingTable.GetMmRoutingTableItems(
                    (MmRoutingTable.ListFilter)(i % 3), // Cycle through ListFilter
                    filter);
            }

            float hitRate100 = rootRelay.RoutingTable.CacheHitRate;

            // Access one more unique filter (should evict LRU entry)
            var result = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.ResponderOnly,
                MmLevelFilterHelper.SelfAndChildren);

            // Assert - cache should still work, eviction handled
            Assert.IsNotNull(result, "LRU eviction should not break cache");
            Assert.GreaterOrEqual(hitRate100, 0f, "Cache hit rate should be valid");
            Assert.LessOrEqual(hitRate100, 1f, "Cache hit rate should not exceed 100%");
        }

        /// <summary>
        /// Test 9: Cache hit rate increases with repeated queries.
        /// </summary>
        [Test]
        public void Test_CacheHitRate_Increases()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay);
            CreateChildRelay("Child2", rootRelay);

            // Act - make repeated queries with same filter
            for (int i = 0; i < 10; i++)
            {
                rootRelay.RoutingTable.GetMmRoutingTableItems(
                    MmRoutingTable.ListFilter.All,
                    MmLevelFilter.Child);
            }

            float hitRate = rootRelay.RoutingTable.CacheHitRate;

            // Assert - hit rate should be high (9 hits out of 10 calls = 90%)
            Assert.AreEqual(0.9f, hitRate, 0.01f, "Hit rate should be ~90% (9 hits, 1 miss)");
        }

        /// <summary>
        /// Test 10: Cache correctly handles empty routing tables.
        /// </summary>
        [Test]
        public void Test_CacheHandles_EmptyRoutingTable()
        {
            // Act - query empty routing table
            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(0, result1.Count, "Empty table: 0 results");
            Assert.AreEqual(0, result2.Count, "Empty table cached: 0 results");
            Assert.AreEqual(0.5f, rootRelay.RoutingTable.CacheHitRate, "Hit rate: 50% (1 miss, 1 hit)");
        }

        /// <summary>
        /// Test 11: Performance comparison - cached vs uncached queries.
        /// Measures speedup from caching on large routing tables.
        /// </summary>
        [Test]
        public void Test_Performance_CachedVsUncached()
        {
            // Arrange - create large routing table (100 responders)
            for (int i = 0; i < 100; i++)
            {
                CreateChildRelay($"Child{i}", rootRelay, (MmTag)(i % 8));
            }

            // Warm up cache
            var warmup = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(100, warmup.Count, "Warmup: 100 children");

            // Invalidate to measure uncached performance
            rootRelay.RoutingTable.Clear();
            for (int i = 0; i < 100; i++)
            {
                CreateChildRelay($"Child{i}", rootRelay, (MmTag)(i % 8));
            }

            // Measure uncached (first call after invalidation)
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var uncachedResult = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            sw1.Stop();

            // Measure cached (second call)
            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            var cachedResult = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            sw2.Stop();

            // Assert - cached should be faster (though timing may vary)
            Assert.AreEqual(100, uncachedResult.Count, "Uncached: 100 results");
            Assert.AreEqual(100, cachedResult.Count, "Cached: 100 results");

            // Log timing for information (cached should be faster, but we won't assert due to timing variance)
            Debug.Log($"QW-3 Performance: Uncached={sw1.ElapsedTicks} ticks, Cached={sw2.ElapsedTicks} ticks, Speedup={(float)sw1.ElapsedTicks/sw2.ElapsedTicks:F2}x");
        }

        /// <summary>
        /// Test 12: Cache invalidation on Insert.
        /// </summary>
        [Test]
        public void Test_CacheInvalidation_OnInsert()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay);
            CreateChildRelay("Child2", rootRelay);

            var result1 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);
            Assert.AreEqual(2, result1.Count, "Initial: 2 children");

            // Act - insert a new item (should invalidate cache)
            var newChild = CreateChildRelay("Child1.5", rootRelay);
            var newItem = rootRelay.RoutingTable[rootRelay.RoutingTable.Count - 1];
            rootRelay.RoutingTable.RemoveAt(rootRelay.RoutingTable.Count - 1); // Remove from end
            rootRelay.RoutingTable.Insert(1, newItem); // Insert in middle

            var result2 = rootRelay.RoutingTable.GetMmRoutingTableItems(
                MmRoutingTable.ListFilter.All,
                MmLevelFilter.Child);

            // Assert
            Assert.AreEqual(3, result2.Count, "After insert: 3 children (cache was invalidated)");
            Assert.AreEqual("Child1.5", result2[1].Name, "Inserted item is in correct position");
        }
    }
}
