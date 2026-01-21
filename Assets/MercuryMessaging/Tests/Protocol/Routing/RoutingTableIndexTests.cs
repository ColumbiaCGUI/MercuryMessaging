// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// Phase 2: O(1) Routing Table Lookup - Validation Tests
// Tests Dictionary index implementation for routing table lookups

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;
using System.Collections.Generic;
using System.Diagnostics;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Validation tests for Phase 2: O(1) Routing Table Lookup optimization.
    /// Tests Dictionary index correctness, maintenance, and performance.
    /// </summary>
    [TestFixture]
    public class RoutingTableIndexTests
    {
        private GameObject testRoot;
        private MmRelayNode rootRelay;
        private List<GameObject> testObjects;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");
            rootRelay = testRoot.AddComponent<MmRelayNode>();
            testObjects = new List<GameObject> { testRoot };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            testObjects.Clear();
        }

        /// <summary>
        /// Helper to create a child relay node and add to routing table.
        /// </summary>
        private MmRelayNode CreateChildRelay(string name, MmRelayNode parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.gameObject.transform);
            MmRelayNode childRelay = child.AddComponent<MmRelayNode>();

            MmRoutingTableItem item = new MmRoutingTableItem();
            item.Name = name;
            item.Responder = childRelay;
            item.Level = MmLevelFilter.Child;

            parent.RoutingTable.Add(item);
            testObjects.Add(child);

            return childRelay;
        }

        #region Correctness Tests

        /// <summary>
        /// Test 1: Lookup by name returns correct item.
        /// </summary>
        [Test]
        public void LookupByName_ReturnsCorrectItem()
        {
            // Arrange
            var child1 = CreateChildRelay("Child1", rootRelay);
            var child2 = CreateChildRelay("Child2", rootRelay);
            var child3 = CreateChildRelay("Child3", rootRelay);

            // Act
            var item1 = rootRelay.RoutingTable["Child1"];
            var item2 = rootRelay.RoutingTable["Child2"];
            var item3 = rootRelay.RoutingTable["Child3"];

            // Assert
            Assert.IsNotNull(item1, "Child1 should be found");
            Assert.IsNotNull(item2, "Child2 should be found");
            Assert.IsNotNull(item3, "Child3 should be found");
            Assert.AreEqual(child1, item1.Responder, "Child1 responder should match");
            Assert.AreEqual(child2, item2.Responder, "Child2 responder should match");
            Assert.AreEqual(child3, item3.Responder, "Child3 responder should match");
        }

        /// <summary>
        /// Test 2: Lookup by responder returns correct item.
        /// </summary>
        [Test]
        public void LookupByResponder_ReturnsCorrectItem()
        {
            // Arrange
            var child1 = CreateChildRelay("Child1", rootRelay);
            var child2 = CreateChildRelay("Child2", rootRelay);

            // Act
            var item1 = rootRelay.RoutingTable[child1];
            var item2 = rootRelay.RoutingTable[child2];

            // Assert
            Assert.IsNotNull(item1, "Child1 item should be found by responder");
            Assert.IsNotNull(item2, "Child2 item should be found by responder");
            Assert.AreEqual("Child1", item1.Name, "Item1 name should be Child1");
            Assert.AreEqual("Child2", item2.Name, "Item2 name should be Child2");
        }

        /// <summary>
        /// Test 3: Lookup nonexistent name returns null.
        /// </summary>
        [Test]
        public void LookupByName_NonExistent_ReturnsNull()
        {
            // Arrange
            CreateChildRelay("Child1", rootRelay);

            // Act
            var item = rootRelay.RoutingTable["NonExistent"];

            // Assert
            Assert.IsNull(item, "Nonexistent name should return null");
        }

        /// <summary>
        /// Test 4: Lookup null responder returns null.
        /// </summary>
        [Test]
        public void LookupByResponder_Null_ReturnsNull()
        {
            // Act
            var item = rootRelay.RoutingTable[(MmResponder)null];

            // Assert
            Assert.IsNull(item, "Null responder lookup should return null");
        }

        /// <summary>
        /// Test 5: Contains uses O(1) index lookup.
        /// </summary>
        [Test]
        public void Contains_UsesIndexLookup()
        {
            // Arrange
            var child1 = CreateChildRelay("Child1", rootRelay);
            var child2 = CreateChildRelay("Child2", rootRelay);

            // Create a responder NOT in the routing table
            var orphan = new GameObject("Orphan");
            var orphanRelay = orphan.AddComponent<MmRelayNode>();
            testObjects.Add(orphan);

            // Act & Assert
            Assert.IsTrue(rootRelay.RoutingTable.Contains(child1), "Should contain child1");
            Assert.IsTrue(rootRelay.RoutingTable.Contains(child2), "Should contain child2");
            Assert.IsFalse(rootRelay.RoutingTable.Contains(orphanRelay), "Should not contain orphan");
        }

        #endregion

        #region Index Maintenance Tests

        /// <summary>
        /// Test 6: Index updated on Add.
        /// </summary>
        [Test]
        public void Index_UpdatedOnAdd()
        {
            // Arrange - lookup should return null before add
            Assert.IsNull(rootRelay.RoutingTable["NewChild"], "Should not exist before add");

            // Act - add child
            var child = CreateChildRelay("NewChild", rootRelay);

            // Assert - index should be updated
            var item = rootRelay.RoutingTable["NewChild"];
            Assert.IsNotNull(item, "Should exist after add");
            Assert.AreEqual(child, item.Responder, "Responder should match");
        }

        /// <summary>
        /// Test 7: Index updated on Remove.
        /// </summary>
        [Test]
        public void Index_UpdatedOnRemove()
        {
            // Arrange
            var child = CreateChildRelay("ToRemove", rootRelay);
            var item = rootRelay.RoutingTable["ToRemove"];
            Assert.IsNotNull(item, "Should exist before remove");

            // Act
            rootRelay.RoutingTable.Remove(item);

            // Assert
            Assert.IsNull(rootRelay.RoutingTable["ToRemove"], "Should not exist after remove");
            Assert.IsFalse(rootRelay.RoutingTable.Contains(child), "Contains should return false");
        }

        /// <summary>
        /// Test 8: Index updated on Clear.
        /// </summary>
        [Test]
        public void Index_UpdatedOnClear()
        {
            // Arrange
            var child1 = CreateChildRelay("Child1", rootRelay);
            var child2 = CreateChildRelay("Child2", rootRelay);
            Assert.IsNotNull(rootRelay.RoutingTable["Child1"], "Child1 should exist");
            Assert.IsNotNull(rootRelay.RoutingTable["Child2"], "Child2 should exist");

            // Act
            rootRelay.RoutingTable.Clear();

            // Assert
            Assert.IsNull(rootRelay.RoutingTable["Child1"], "Child1 should not exist after clear");
            Assert.IsNull(rootRelay.RoutingTable["Child2"], "Child2 should not exist after clear");
            Assert.IsFalse(rootRelay.RoutingTable.Contains(child1), "Contains child1 should be false");
            Assert.IsFalse(rootRelay.RoutingTable.Contains(child2), "Contains child2 should be false");
        }

        /// <summary>
        /// Test 9: Duplicate names - first occurrence is indexed.
        /// </summary>
        [Test]
        public void DuplicateNames_FirstOccurrenceIndexed()
        {
            // Arrange - create two items with same name
            var child1 = CreateChildRelay("DupeName", rootRelay);
            var child2 = CreateChildRelay("DupeName", rootRelay);

            // Act
            var item = rootRelay.RoutingTable["DupeName"];

            // Assert - first occurrence should be returned
            Assert.IsNotNull(item, "Should find item with duplicate name");
            Assert.AreEqual(child1, item.Responder, "First occurrence should be indexed");
        }

        /// <summary>
        /// Test 10: Removing first duplicate updates index to next.
        /// </summary>
        [Test]
        public void RemoveFirstDuplicate_IndexUpdatesToNext()
        {
            // Arrange
            var child1 = CreateChildRelay("DupeName", rootRelay);
            var child2 = CreateChildRelay("DupeName", rootRelay);
            var firstItem = rootRelay.RoutingTable["DupeName"];
            Assert.AreEqual(child1, firstItem.Responder, "First should be indexed initially");

            // Act - remove first item
            rootRelay.RoutingTable.Remove(firstItem);

            // Assert - second should now be indexed
            var newItem = rootRelay.RoutingTable["DupeName"];
            Assert.IsNotNull(newItem, "Should still find item with name");
            Assert.AreEqual(child2, newItem.Responder, "Second occurrence should now be indexed");
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// Test 11: O(1) lookup performance vs O(n) baseline.
        /// </summary>
        [Test]
        public void Performance_LookupIsO1()
        {
            // Arrange - create many children
            const int childCount = 100;
            var children = new List<MmRelayNode>();
            for (int i = 0; i < childCount; i++)
            {
                children.Add(CreateChildRelay($"Child{i}", rootRelay));
            }

            // Warm up indices
            var _ = rootRelay.RoutingTable["Child0"];

            // Act - measure lookup time for first and last items
            var sw = Stopwatch.StartNew();
            const int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                var item = rootRelay.RoutingTable["Child0"];
            }
            long firstLookupTicks = sw.ElapsedTicks;

            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var item = rootRelay.RoutingTable[$"Child{childCount - 1}"];
            }
            long lastLookupTicks = sw.ElapsedTicks;

            // Assert - O(1) means first and last should be similar time
            // Allow 10x variance for system noise/cache effects
            // Note: O(n) would show ~100x difference for 100 items
            float ratio = (float)lastLookupTicks / firstLookupTicks;
            UnityEngine.Debug.Log($"[Phase 2] O(1) Lookup Performance: First={firstLookupTicks} ticks, Last={lastLookupTicks} ticks, Ratio={ratio:F2}x");

            Assert.Less(ratio, 10.0f, "Last lookup should not be significantly slower than first (O(1) property)");
        }

        /// <summary>
        /// Test 12: O(1) Contains performance.
        /// </summary>
        [Test]
        public void Performance_ContainsIsO1()
        {
            // Arrange
            const int childCount = 100;
            var children = new List<MmRelayNode>();
            for (int i = 0; i < childCount; i++)
            {
                children.Add(CreateChildRelay($"Child{i}", rootRelay));
            }

            // Warm up
            var _ = rootRelay.RoutingTable.Contains(children[0]);

            // Act
            var sw = Stopwatch.StartNew();
            const int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                var contains = rootRelay.RoutingTable.Contains(children[0]);
            }
            long firstContainsTicks = sw.ElapsedTicks;

            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var contains = rootRelay.RoutingTable.Contains(children[childCount - 1]);
            }
            long lastContainsTicks = sw.ElapsedTicks;

            // Assert - O(1) means first and last should be similar time
            // Allow 10x variance for system noise/cache effects
            float ratio = (float)lastContainsTicks / firstContainsTicks;
            UnityEngine.Debug.Log($"[Phase 2] O(1) Contains Performance: First={firstContainsTicks} ticks, Last={lastContainsTicks} ticks, Ratio={ratio:F2}x");

            Assert.Less(ratio, 10.0f, "Last Contains should not be significantly slower than first (O(1) property)");
        }

        #endregion
    }
}
