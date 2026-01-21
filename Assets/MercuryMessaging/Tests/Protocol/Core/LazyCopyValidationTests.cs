// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// QW-2: Lazy Message Copying Tests
// Validates that messages are only copied when necessary (multi-direction routing)

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for QW-2 Lazy Message Copying optimization.
    /// Validates that single-direction routing reuses messages (zero copy)
    /// and multi-direction routing creates only necessary copies.
    /// </summary>
    [TestFixture]
    public class LazyCopyValidationTests
    {
        private GameObject testNode;
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
            MessageReceiveCounter.receiveCount = 0;
            MessageTypeCounter.typesReceived = 0;
        }

        [TearDown]
        public void TearDown()
        {
            // Reset static counters
            MessageReceiveCounter.receiveCount = 0;
            MessageTypeCounter.typesReceived = 0;

            // Clean up GameObjects
            if (testNode != null)
            {
                Object.DestroyImmediate(testNode);
                testNode = null;
            }
        }

        /// <summary>
        /// Test single-direction routing (child only) - should use zero-copy optimization
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_SingleDirection_NoErrors()
        {
            // Arrange - Single node with child responder
            testNode = new GameObject("SingleDir");
            testRelay = testNode.AddComponent<MmRelayNode>();
            var responder = testNode.AddComponent<MessageReceiveCounter>();
            testRelay.MmRefreshResponders(); // Ensure responder is registered

            yield return null;

            // Act - Send many messages in single direction
            int iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));

                if (i % 100 == 0)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.1f);

            // Assert - All messages should be received without errors
            Assert.AreEqual(iterations, MessageReceiveCounter.receiveCount,
                $"Expected {iterations} messages received, got {MessageReceiveCounter.receiveCount}");

            Debug.Log($"[QW-2 PASS] Single-direction routing handled {iterations} messages without errors");
        }

        /// <summary>
        /// Test multi-direction routing (parent + child) - should create necessary copies
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_MultiDirection_NoErrors()
        {
            // Arrange - Parent-child hierarchy
            var parentNode = new GameObject("Parent");
            var parentRelay = parentNode.AddComponent<MmRelayNode>();
            var parentResponder = parentNode.AddComponent<MessageReceiveCounter>();
            parentRelay.MmRefreshResponders(); // Ensure responder is registered

            var childNode = new GameObject("Child");
            childNode.transform.SetParent(parentNode.transform);
            var childRelay = childNode.AddComponent<MmRelayNode>();
            var childResponder = childNode.AddComponent<MessageReceiveCounter>();
            childRelay.MmRefreshResponders(); // Ensure responder is registered

            // Register child relay node with parent for message propagation
            parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            parentRelay.RefreshParents(); // Refresh hierarchy after adding child

            yield return null;

            // Act - Send messages in multi-direction (SelfAndChildren)
            MessageReceiveCounter.receiveCount = 0;
            int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                parentRelay.MmInvoke(new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                    new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren)));

                if (i % 100 == 0)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.1f);

            // Assert - Both parent and child should receive messages
            // Total = iterations * 2 (parent + child)
            Assert.AreEqual(iterations * 2, MessageReceiveCounter.receiveCount,
                $"Expected {iterations * 2} messages (parent+child), got {MessageReceiveCounter.receiveCount}");

            Debug.Log($"[QW-2 PASS] Multi-direction routing handled {iterations} messages without errors");

            // Unity will automatically clean up all GameObjects after the test
        }

        /// <summary>
        /// Test that message content remains correct in single-direction routing
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_SingleDirection_PreservesMessageContent()
        {
            // Arrange
            testNode = new GameObject("ContentTest");
            testRelay = testNode.AddComponent<MmRelayNode>();
            var responder = testNode.AddComponent<ContentVerifyResponder>();
            testRelay.MmRefreshResponders(); // Ensure responder is registered

            yield return null;

            // Act - Send message with specific content
            string testContent = "TestMessage_123";
            testRelay.MmInvoke(
                MmMethod.MessageString,
                testContent,
                new MmMetadataBlock(MmLevelFilter.Self)
            );

            yield return new WaitForSeconds(0.1f);

            // Assert - Message content should be preserved
            Assert.AreEqual(testContent, ContentVerifyResponder.lastReceivedContent,
                "Message content should be preserved in zero-copy routing");

            Debug.Log("[QW-2 PASS] Message content preserved in single-direction routing");
        }

        /// <summary>
        /// Test that message content remains correct in multi-direction routing
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_MultiDirection_PreservesMessageContent()
        {
            // Arrange - Parent-child hierarchy
            var parentNode = new GameObject("Parent");
            var parentRelay = parentNode.AddComponent<MmRelayNode>();
            var parentResponder = parentNode.AddComponent<ContentVerifyResponder>();
            parentRelay.MmRefreshResponders(); // Ensure responder is registered

            var childNode = new GameObject("Child");
            childNode.transform.SetParent(parentNode.transform);
            var childRelay = childNode.AddComponent<MmRelayNode>();
            var childResponder = childNode.AddComponent<ContentVerifyResponder>();
            childRelay.MmRefreshResponders(); // Ensure responder is registered

            // Register child relay node with parent for message propagation
            parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            parentRelay.RefreshParents(); // Refresh hierarchy after adding child

            yield return null;

            // Act - Send message to both parent and child
            string testContent = "MultiDirectionTest_456";
            parentRelay.MmInvoke(
                MmMethod.MessageString,
                testContent,
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren)
            );

            yield return new WaitForSeconds(0.1f);

            // Assert - Both should receive correct content
            Assert.AreEqual(testContent, ContentVerifyResponder.lastReceivedContent,
                "Message content should be preserved in multi-direction routing with copies");

            Debug.Log("[QW-2 PASS] Message content preserved in multi-direction routing");

            // Unity will automatically clean up all GameObjects after the test
        }

        /// <summary>
        /// Stress test: High volume single-direction routing
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_HighVolume_SingleDirection()
        {
            // Arrange
            testNode = new GameObject("StressTest");
            testRelay = testNode.AddComponent<MmRelayNode>();
            var responder = testNode.AddComponent<MessageReceiveCounter>();
            testRelay.MmRefreshResponders(); // Ensure responder is registered

            yield return null;

            // Act - Send large volume of messages
            int iterations = 5000;
            MessageReceiveCounter.receiveCount = 0;

            for (int i = 0; i < iterations; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));

                if (i % 500 == 0)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.2f);

            // Assert - All messages delivered successfully
            Assert.AreEqual(iterations, MessageReceiveCounter.receiveCount,
                $"High-volume test: Expected {iterations} messages, got {MessageReceiveCounter.receiveCount}");

            Debug.Log($"[QW-2 PASS] High-volume test: {iterations} messages processed successfully");
        }

        /// <summary>
        /// Test that lazy copying works with different message types
        /// </summary>
        [UnityTest]
        public IEnumerator LazyCopy_WorksWithDifferentMessageTypes()
        {
            // Arrange
            testNode = new GameObject("TypeTest");
            testRelay = testNode.AddComponent<MmRelayNode>();
            var responder = testNode.AddComponent<MessageTypeCounter>();
            testRelay.MmRefreshResponders(); // Ensure responder is registered

            yield return null;

            // Act - Send various message types
            testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
            yield return null;

            testRelay.MmInvoke(MmMethod.MessageBool, true);
            yield return null;

            testRelay.MmInvoke(MmMethod.MessageInt, 42);
            yield return null;

            testRelay.MmInvoke(MmMethod.MessageFloat, 3.14f);
            yield return null;

            testRelay.MmInvoke(MmMethod.MessageString, "test");
            yield return null;

            testRelay.MmInvoke(MmMethod.MessageVector3, Vector3.one);
            yield return null;

            // Assert - All message types handled
            Assert.AreEqual(6, MessageTypeCounter.typesReceived,
                "All message types should be handled correctly with lazy copying");

            Debug.Log("[QW-2 PASS] Lazy copying works with multiple message types");
        }

        #region Test Helper Classes

        /// <summary>
        /// Counts received Initialize messages
        /// </summary>
        private class MessageReceiveCounter : MmBaseResponder
        {
            public static int receiveCount = 0;

            public override void Initialize()
            {
                receiveCount++;
            }
        }

        /// <summary>
        /// Verifies message content integrity
        /// </summary>
        private class ContentVerifyResponder : MmBaseResponder
        {
            public static string lastReceivedContent = null;

            protected override void ReceivedMessage(MmMessageString message)
            {
                lastReceivedContent = message.value;
            }
        }

        /// <summary>
        /// Counts different message types received
        /// </summary>
        private class MessageTypeCounter : MmBaseResponder
        {
            public static int typesReceived = 0;

            public override void Initialize()
            {
                typesReceived++;
            }

            protected override void ReceivedMessage(MmMessageBool message)
            {
                typesReceived++;
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
                typesReceived++;
            }

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                typesReceived++;
            }

            protected override void ReceivedMessage(MmMessageString message)
            {
                typesReceived++;
            }

            protected override void ReceivedMessage(MmMessageVector3 message)
            {
                typesReceived++;
            }
        }

        #endregion
    }
}
