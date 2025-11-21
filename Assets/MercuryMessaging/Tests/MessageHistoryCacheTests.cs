// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Unit tests for MmMessageHistoryCache (LRU cache with time-based eviction)
// Part of Phase 2.1: Advanced Message Routing

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging.Support;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for MmMessageHistoryCache LRU implementation.
    /// Validates time-based eviction, O(1) lookups, and memory bounds.
    /// </summary>
    [TestFixture]
    public class MessageHistoryCacheTests
    {
        #region Basic Operations

        [Test]
        public void Add_SingleNode_ContainsNode()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);

            // Act
            cache.Add(1);

            // Assert
            Assert.IsTrue(cache.Contains(1), "Cache should contain added node");
            Assert.AreEqual(1, cache.Count, "Cache size should be 1");
        }

        [Test]
        public void Add_MultipleNodes_ContainsAllNodes()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);

            // Act
            cache.Add(1);
            cache.Add(2);
            cache.Add(3);

            // Assert
            Assert.IsTrue(cache.Contains(1));
            Assert.IsTrue(cache.Contains(2));
            Assert.IsTrue(cache.Contains(3));
            Assert.AreEqual(3, cache.Count);
        }

        [Test]
        public void Add_DuplicateNode_DoesNotIncrementCount()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);

            // Act
            cache.Add(1);
            cache.Add(1);
            cache.Add(1);

            // Assert
            Assert.AreEqual(1, cache.Count, "Cache should not duplicate entries");
            Assert.AreEqual(3, cache.AddCount, "Add counter should track all calls");
        }

        [Test]
        public void Contains_NonExistentNode_ReturnsFalse()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);
            cache.Add(1);

            // Act & Assert
            Assert.IsFalse(cache.Contains(2), "Cache should not contain node 2");
            Assert.IsFalse(cache.Contains(999), "Cache should not contain node 999");
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);
            cache.Add(1);
            cache.Add(2);
            cache.Add(3);

            // Act
            cache.Clear();

            // Assert
            Assert.AreEqual(0, cache.Count, "Cache should be empty after clear");
            Assert.IsFalse(cache.Contains(1));
            Assert.IsFalse(cache.Contains(2));
            Assert.IsFalse(cache.Contains(3));
        }

        #endregion

        #region Time-Based Eviction

        [UnityTest]
        public IEnumerator Eviction_AfterTimeWindow_RemovesOldEntries()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(50); // 50ms window
            cache.Add(1);
            cache.Add(2);

            Assert.AreEqual(2, cache.Count, "Initial count should be 2");

            // Act - Wait for entries to expire
            yield return new WaitForSeconds(0.06f); // 60ms > 50ms window

            // Trigger eviction by adding new node
            cache.Add(3);

            // Assert
            Assert.IsFalse(cache.Contains(1), "Node 1 should be evicted");
            Assert.IsFalse(cache.Contains(2), "Node 2 should be evicted");
            Assert.IsTrue(cache.Contains(3), "Node 3 should still be present");
            Assert.AreEqual(1, cache.Count, "Only node 3 should remain");
            Assert.Greater(cache.EvictionCount, 0, "Evictions should have occurred");
        }

        [UnityTest]
        public IEnumerator Eviction_PartialExpiration_RemovesOnlyOld()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100); // 100ms window
            cache.Add(1);

            yield return new WaitForSeconds(0.06f); // 60ms delay

            cache.Add(2); // Added 60ms after node 1

            yield return new WaitForSeconds(0.05f); // Another 50ms (110ms total from node 1)

            // Act - Trigger eviction
            cache.Add(3);

            // Assert
            Assert.IsFalse(cache.Contains(1), "Node 1 should be evicted (added 110ms ago)");
            Assert.IsTrue(cache.Contains(2), "Node 2 should remain (added 50ms ago)");
            Assert.IsTrue(cache.Contains(3), "Node 3 should be present (just added)");
        }

        [UnityTest]
        public IEnumerator Eviction_RefreshTimestamp_ExtendsLifetime()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(60); // 60ms window
            cache.Add(1);

            yield return new WaitForSeconds(0.04f); // 40ms

            // Act - Refresh node 1's timestamp
            cache.Add(1);

            yield return new WaitForSeconds(0.04f); // Another 40ms (80ms from initial add, 40ms from refresh)

            cache.Add(2); // Trigger eviction

            // Assert
            Assert.IsTrue(cache.Contains(1), "Node 1 should remain (timestamp was refreshed)");
            Assert.IsTrue(cache.Contains(2), "Node 2 should be present");
        }

        [Test]
        public void EvictNow_ManualEviction_RemovesExpiredEntries()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(0); // 0ms window (immediate expiration)
            cache.Add(1);
            cache.Add(2);

            // Act
            cache.EvictNow();

            // Assert
            Assert.AreEqual(0, cache.Count, "All entries should be evicted with 0ms window");
            Assert.IsFalse(cache.Contains(1));
            Assert.IsFalse(cache.Contains(2));
        }

        #endregion

        #region Statistics

        [Test]
        public void GetStatistics_TracksAddAndEvictionCounts()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);

            // Act
            cache.Add(1);
            cache.Add(2);
            cache.Add(3);
            cache.Add(1); // Duplicate (should increment AddCount but not size)

            var stats = cache.GetStatistics();

            // Assert
            Assert.AreEqual(3, stats.CurrentSize, "Size should be 3 unique nodes");
            Assert.AreEqual(4, stats.TotalAdds, "AddCount should track all 4 calls");
            Assert.AreEqual(100, stats.WindowSizeMs, "Window size should match constructor");
        }

        [UnityTest]
        public IEnumerator GetStatistics_TracksEvictions()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(50);
            cache.Add(1);
            cache.Add(2);

            yield return new WaitForSeconds(0.06f);

            // Act - Trigger eviction
            cache.Add(3);

            var stats = cache.GetStatistics();

            // Assert
            Assert.Greater(stats.TotalEvictions, 0, "Eviction count should be > 0");
            Assert.AreEqual(1, stats.CurrentSize, "Only node 3 should remain");
        }

        [Test]
        public void GetStatistics_OldestEntryAge_ReflectsActualAge()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(1000); // Long window
            cache.Add(1);

            // Wait a bit (can't be exact in tests, but should be > 0)
            System.Threading.Thread.Sleep(10);

            // Act
            var stats = cache.GetStatistics();

            // Assert
            Assert.Greater(stats.OldestEntryAgeMs, 0, "Oldest entry should have age > 0ms");
            Assert.Less(stats.OldestEntryAgeMs, 1000, "Oldest entry age should be < window size");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Constructor_NegativeWindowSize_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentException>(() =>
            {
                var cache = new MmMessageHistoryCache(-1);
            });
        }

        [Test]
        public void Constructor_ZeroWindowSize_CreatesCache()
        {
            // Act
            var cache = new MmMessageHistoryCache(0);

            // Assert
            Assert.NotNull(cache);
            Assert.AreEqual(0, cache.Count);
        }

        [Test]
        public void Add_LargeNumberOfNodes_MaintainsBounds()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);

            // Act - Add many nodes rapidly
            for (int i = 0; i < 1000; i++)
            {
                cache.Add(i);
            }

            // Assert
            Assert.LessOrEqual(cache.Count, 1000, "Cache should contain added nodes");
            Assert.AreEqual(1000, cache.AddCount, "AddCount should track all adds");

            // All nodes should be findable (added within 100ms window)
            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(cache.Contains(i), $"Node {i} should be in cache");
            }
        }

        [Test]
        public void ToString_ReturnsStatistics()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(100);
            cache.Add(1);
            cache.Add(2);

            // Act
            string str = cache.ToString();

            // Assert
            Assert.IsNotEmpty(str);
            StringAssert.Contains("Size=2", str);
            StringAssert.Contains("Window=100ms", str);
        }

        #endregion

        #region Performance Validation

        [Test]
        [Category("Performance")]
        public void Performance_ContainsLookup_IsConstantTime()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(1000);
            const int nodeCount = 10000;

            // Add many nodes
            for (int i = 0; i < nodeCount; i++)
            {
                cache.Add(i);
            }

            // Act - Benchmark Contains() performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 100000;

            for (int i = 0; i < iterations; i++)
            {
                bool _ = cache.Contains(i % nodeCount);
            }

            stopwatch.Stop();

            // Assert
            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / iterations;
            Debug.Log($"[Performance] Avg Contains() time: {avgNanoseconds:F2} ns ({iterations:N0} iterations, {nodeCount:N0} nodes)");

            // O(1) lookup should be < 100ns even with 10K nodes
            Assert.Less(avgNanoseconds, 100, $"Contains() should be O(1), got {avgNanoseconds:F2}ns");
        }

        [Test]
        [Category("Performance")]
        public void Performance_AddOperation_IsConstantTime()
        {
            // Arrange
            var cache = new MmMessageHistoryCache(1000);

            // Act - Benchmark Add() performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 10000;

            for (int i = 0; i < iterations; i++)
            {
                cache.Add(i);
            }

            stopwatch.Stop();

            // Assert
            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / iterations;
            Debug.Log($"[Performance] Avg Add() time: {avgNanoseconds:F2} ns ({iterations:N0} iterations)");

            // Amortized O(1) should be < 500ns (including occasional eviction)
            Assert.Less(avgNanoseconds, 500, $"Add() should be amortized O(1), got {avgNanoseconds:F2}ns");
        }

        #endregion
    }
}
