// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Unit tests for MmCircularBuffer<T> class

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using MercuryMessaging.Support.Data;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Comprehensive unit tests for MmCircularBuffer implementation.
    /// Tests circular wrapping, enumeration, edge cases, and performance.
    /// </summary>
    [TestFixture]
    public class MmCircularBufferTests
    {
        #region Basic Functionality Tests

        [Test]
        public void Constructor_CreatesBufferWithCorrectCapacity()
        {
            // Arrange & Act
            var buffer = new MmCircularBuffer<int>(50);

            // Assert
            Assert.AreEqual(50, buffer.Capacity);
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Add_IncreasesCount()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(10);

            // Act
            buffer.Add("item1");
            buffer.Add("item2");

            // Assert
            Assert.AreEqual(2, buffer.Count);
        }

        [Test]
        public void Add_StoresItemsInOrder()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(10);

            // Act
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Assert
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual(2, items[1]);
            Assert.AreEqual(3, items[2]);
        }

        #endregion

        #region Circular Wrapping Tests

        [Test]
        public void Add_WhenFull_OverwritesOldestItem()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act - Add 4th item, should overwrite 1
            buffer.Add(4);

            // Assert
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count); // Still only 3 items
            Assert.AreEqual(2, items[0]); // 1 was overwritten
            Assert.AreEqual(3, items[1]);
            Assert.AreEqual(4, items[2]);
        }

        [Test]
        public void Add_MultipleWraps_MaintainsCorrectOrder()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(3);

            // Act - Add 10 items to a size-3 buffer (wraps 3+ times)
            for (int i = 1; i <= 10; i++)
            {
                buffer.Add(i);
            }

            // Assert - Should only have last 3 items
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(8, items[0]);
            Assert.AreEqual(9, items[1]);
            Assert.AreEqual(10, items[2]);
        }

        [Test]
        public void Insert_WithIndexZero_AddsToFront()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(5);
            buffer.Add("first");
            buffer.Add("second");

            // Act - Insert at front (List.Insert(0, item) pattern)
            buffer.Insert(0, "newest");

            // Assert
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("newest", items[0]);
            Assert.AreEqual("first", items[1]);
            Assert.AreEqual("second", items[2]);
        }

        [Test]
        public void Insert_WhenFull_OverwritesOldest()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act - Insert at front when full
            buffer.Insert(0, 99);

            // Assert - Oldest (1) should be removed
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(99, items[0]);
            Assert.AreEqual(2, items[1]);
            Assert.AreEqual(3, items[2]);
        }

        #endregion

        #region Enumeration Tests

        [Test]
        public void GetEnumerator_ReturnsItemsInCorrectOrder()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(5);
            buffer.Add(10);
            buffer.Add(20);
            buffer.Add(30);

            // Act
            var items = new List<int>();
            foreach (var item in buffer)
            {
                items.Add(item);
            }

            // Assert
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(10, items[0]);
            Assert.AreEqual(20, items[1]);
            Assert.AreEqual(30, items[2]);
        }

        [Test]
        public void GetEnumerator_AfterWrapping_ReturnsCorrectOrder()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4); // Overwrites 1
            buffer.Add(5); // Overwrites 2

            // Act
            var items = buffer.ToList();

            // Assert
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, items[0]);
            Assert.AreEqual(4, items[1]);
            Assert.AreEqual(5, items[2]);
        }

        [Test]
        public void LINQ_Methods_WorkCorrectly()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(10);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);
            buffer.Add(5);

            // Act & Assert
            Assert.AreEqual(5, buffer.Count());
            Assert.AreEqual(15, buffer.Sum());
            Assert.AreEqual(3, buffer.Average());
            Assert.IsTrue(buffer.Contains(3));
            Assert.IsFalse(buffer.Contains(99));
            Assert.AreEqual(5, buffer.Max());
            Assert.AreEqual(1, buffer.Min());
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EmptyBuffer_HasZeroCount()
        {
            // Arrange & Act
            var buffer = new MmCircularBuffer<int>(10);

            // Assert
            Assert.AreEqual(0, buffer.Count);
            Assert.AreEqual(0, buffer.ToList().Count);
        }

        [Test]
        public void EmptyBuffer_EnumerationDoesNotThrow()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(10);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                foreach (var item in buffer)
                {
                    // Should not execute
                    Assert.Fail("Should not enumerate empty buffer");
                }
            });
        }

        [Test]
        public void SingleItemBuffer_WorksCorrectly()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(1);

            // Act
            buffer.Add("first");
            buffer.Add("second"); // Should overwrite first

            // Assert
            Assert.AreEqual(1, buffer.Count);
            Assert.AreEqual("second", buffer.First());
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act
            buffer.Clear();

            // Assert
            Assert.AreEqual(0, buffer.Count);
            Assert.AreEqual(0, buffer.ToList().Count);
        }

        [Test]
        public void Clear_AllowsReuse()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Clear();

            // Act
            buffer.Add(10);
            buffer.Add(20);

            // Assert
            var items = buffer.ToList();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(10, items[0]);
            Assert.AreEqual(20, items[1]);
        }

        #endregion

        #region Data Type Tests

        [Test]
        public void WorksWithReferenceTypes()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(3);

            // Act
            buffer.Add("apple");
            buffer.Add("banana");
            buffer.Add("cherry");

            // Assert
            Assert.AreEqual(3, buffer.Count);
            Assert.IsTrue(buffer.Contains("banana"));
        }

        [Test]
        public void WorksWithNullValues()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(3);

            // Act
            buffer.Add("value");
            buffer.Add(null);
            buffer.Add("another");

            // Assert
            var items = buffer.ToList();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("value", items[0]);
            Assert.IsNull(items[1]);
            Assert.AreEqual("another", items[2]);
        }

        [Test]
        public void WorksWithComplexTypes()
        {
            // Arrange
            var buffer = new MmCircularBuffer<List<int>>(3);
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 4, 5, 6 };

            // Act
            buffer.Add(list1);
            buffer.Add(list2);

            // Assert
            Assert.AreEqual(2, buffer.Count);
            Assert.AreSame(list1, buffer.First());
            Assert.AreSame(list2, buffer.Last());
        }

        #endregion

        #region Performance Tests

        [Test]
        public void Add_LargeNumberOfItems_PerformsEfficiently()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(1000);

            // Act - Add 10,000 items (10x capacity)
            var startTime = System.DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                buffer.Add(i);
            }
            var elapsed = System.DateTime.Now - startTime;

            // Assert - Should complete quickly (< 100ms for 10k items)
            Assert.Less(elapsed.TotalMilliseconds, 100, "Add operation should be O(1) and fast");
            Assert.AreEqual(1000, buffer.Count); // Only last 1000 items
        }

        [Test]
        public void Enumeration_LargeBuffer_PerformsEfficiently()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(1000);
            for (int i = 0; i < 1000; i++)
            {
                buffer.Add(i);
            }

            // Act
            var startTime = System.DateTime.Now;
            var sum = 0;
            foreach (var item in buffer)
            {
                sum += item;
            }
            var elapsed = System.DateTime.Now - startTime;

            // Assert
            Assert.Less(elapsed.TotalMilliseconds, 10, "Enumeration should be fast");
            Assert.AreEqual(499500, sum); // Sum of 0..999 = n(n-1)/2 = 499500
        }

        #endregion

        #region Compatibility Tests

        [Test]
        public void Insert_BehavesLikeListInsertAtZero()
        {
            // Arrange
            var buffer = new MmCircularBuffer<string>(5);
            var list = new List<string>();

            // Act - Perform same operations on both
            buffer.Add("a");
            list.Add("a");

            buffer.Add("b");
            list.Add("b");

            buffer.Insert(0, "c");
            list.Insert(0, "c");

            buffer.Insert(0, "d");
            list.Insert(0, "d");

            // Assert - Both should have same order
            var bufferItems = buffer.ToList();
            Assert.AreEqual(list.Count, bufferItems.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], bufferItems[i], $"Item at index {i} should match");
            }
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_AlternatingAddAndInsert()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(100);

            // Act - Alternate between Add and Insert
            for (int i = 0; i < 1000; i++)
            {
                if (i % 2 == 0)
                    buffer.Add(i);
                else
                    buffer.Insert(0, i);
            }

            // Assert - Should maintain exactly 100 items
            Assert.AreEqual(100, buffer.Count);
            Assert.AreEqual(100, buffer.ToList().Count);
        }

        [Test]
        public void StressTest_ClearAndRefill()
        {
            // Arrange
            var buffer = new MmCircularBuffer<int>(50);

            // Act - Fill, clear, refill multiple times
            for (int cycle = 0; cycle < 10; cycle++)
            {
                for (int i = 0; i < 50; i++)
                {
                    buffer.Add(i);
                }
                Assert.AreEqual(50, buffer.Count, $"Cycle {cycle} fill failed");

                buffer.Clear();
                Assert.AreEqual(0, buffer.Count, $"Cycle {cycle} clear failed");
            }

            // Assert - Final fill
            for (int i = 0; i < 50; i++)
            {
                buffer.Add(i * 10);
            }
            Assert.AreEqual(50, buffer.Count);
            Assert.AreEqual(0, buffer.First());
            Assert.AreEqual(490, buffer.Last());
        }

        #endregion
    }
}
