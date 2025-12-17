// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// QW-4: MmCircularBuffer Memory Stability Tests
// Validates that message history uses bounded MmCircularBuffer instead of unbounded List

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for QW-4 MmCircularBuffer implementation.
    /// Validates memory stability over high message volumes.
    /// </summary>
    [TestFixture]
    public class MmCircularBufferMemoryTests
    {
        private GameObject testObject;
        private MmRelayNode testRelay;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Ignore TLS allocator warnings for entire test fixture
            LogAssert.ignoreFailingMessages = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Re-enable log assertions after all tests complete
            LogAssert.ignoreFailingMessages = false;
        }

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("MmCircularBufferTest");
            testRelay = testObject.AddComponent<MmRelayNode>();
        }

        /// <summary>
        /// Test that message history is bounded by MmCircularBuffer capacity
        /// </summary>
        [UnityTest]
        public IEnumerator MmCircularBuffer_LimitsHistorySize()
        {
            // Arrange
            int expectedMaxSize = 100;
            testRelay.messageHistorySize = expectedMaxSize;

            yield return null; // Let Awake initialize buffer

            // Act - Send many more messages than buffer capacity
            int messageCount = 1000;
            for (int i = 0; i < messageCount; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
            }

            yield return null;

            // Assert
            Assert.LessOrEqual(testRelay.messageInList.Count, expectedMaxSize,
                $"Message history should not exceed {expectedMaxSize}, but has {testRelay.messageInList.Count} items");
        }

        /// <summary>
        /// Test that memory remains stable over large message volumes (QW-4 primary validation)
        /// </summary>
        [UnityTest]
        public IEnumerator MmCircularBuffer_PreventsMemoryGrowth()
        {
            // Arrange
            int expectedMaxSize = 100;
            int messageCount = 10000;
            testRelay.messageHistorySize = expectedMaxSize;

            yield return null; // Let Awake initialize buffer

            // Force garbage collection to establish baseline
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            yield return null;

            long memoryBefore = System.GC.GetTotalMemory(false);

            // Act - Send many messages
            for (int i = 0; i < messageCount; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));

                // Yield periodically to prevent frame blocking
                if (i % 1000 == 0)
                {
                    yield return null;
                }
            }

            // Force garbage collection to measure actual memory usage
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            yield return null;

            long memoryAfter = System.GC.GetTotalMemory(false);
            long memoryDeltaMB = (memoryAfter - memoryBefore) / 1024 / 1024;

            // Assert - Memory growth should be minimal (< 10MB for 10k messages)
            Assert.Less(memoryDeltaMB, 10,
                $"Memory growth should be less than 10MB, but was {memoryDeltaMB}MB for {messageCount} messages");

            // Verify buffer size stayed bounded
            Assert.LessOrEqual(testRelay.messageInList.Count, expectedMaxSize,
                $"Buffer size {testRelay.messageInList.Count} exceeded capacity {expectedMaxSize}");

            Debug.Log($"[QW-4 PASS] Memory stable: {memoryDeltaMB}MB delta for {messageCount} messages, " +
                     $"history size: {testRelay.messageInList.Count}/{expectedMaxSize}");
        }

        /// <summary>
        /// Test that different buffer sizes work correctly
        /// </summary>
        [Test]
        public void MmCircularBuffer_RespectsConfiguredSize([Values(10, 50, 100, 500)] int bufferSize)
        {
            // Arrange
            testRelay.messageHistorySize = bufferSize;

            // Act - Send double the buffer capacity
            for (int i = 0; i < bufferSize * 2; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
            }

            // Assert
            Assert.LessOrEqual(testRelay.messageInList.Count, bufferSize,
                $"Buffer size should not exceed {bufferSize}");
        }

        /// <summary>
        /// Test that both messageInList and messageOutList are bounded
        /// </summary>
        [UnityTest]
        public IEnumerator MmCircularBuffer_BothHistoryListsAreBounded()
        {
            // Arrange
            int maxSize = 50;
            testRelay.messageHistorySize = maxSize;

            yield return null;

            // Act - Send many messages (which creates both in and out history)
            for (int i = 0; i < 200; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
            }

            yield return null;

            // Assert both history lists
            Assert.LessOrEqual(testRelay.messageInList.Count, maxSize,
                $"messageInList should not exceed {maxSize}");
            Assert.LessOrEqual(testRelay.messageOutList.Count, maxSize,
                $"messageOutList should not exceed {maxSize}");
        }

        /// <summary>
        /// Stress test: Continuous message flow over multiple frames
        /// </summary>
        [UnityTest]
        public IEnumerator MmCircularBuffer_HandlesLongRunningSessions()
        {
            // Arrange
            int bufferSize = 100;
            testRelay.messageHistorySize = bufferSize;

            yield return null;

            // Act - Simulate long-running session with periodic bursts
            for (int burst = 0; burst < 10; burst++)
            {
                // Send burst of messages
                for (int i = 0; i < 500; i++)
                {
                    testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
                }

                // Wait between bursts
                yield return new WaitForSeconds(0.1f);

                // Assert buffer remains bounded
                Assert.LessOrEqual(testRelay.messageInList.Count, bufferSize,
                    $"Buffer size exceeded during burst {burst}");
            }

            Debug.Log("[QW-4 PASS] Long-running session test completed, buffer remained stable");
        }
    }
}
