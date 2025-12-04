// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// E1-E3: Handled Flag Early Termination Tests
// Validates that setting Handled=true stops message propagation to remaining responders

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for Handled flag implementation.
    /// Validates that messages stop propagating when Handled=true.
    /// </summary>
    [TestFixture]
    public class HandledFlagTests
    {
        private GameObject rootNode;
        private List<GameObject> testObjects;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [SetUp]
        public void SetUp()
        {
            testObjects = new List<GameObject>();
            HandledTestResponder.invokeOrder.Clear();
            HandledTestResponder.invokeCount = 0;
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
        /// Test that default Handled=false doesn't change existing behavior
        /// </summary>
        [UnityTest]
        public IEnumerator HandledFlag_DefaultFalse_AllRespondersReceiveMessage()
        {
            // Arrange - Create node with multiple responders
            rootNode = CreateNodeWithResponders(3, handleMessage: false);
            yield return null;

            // Act
            HandledTestResponder.invokeCount = 0;
            var relay = rootNode.GetComponent<MmRelayNode>();
            relay.MmInvoke(MmMethod.Initialize);

            yield return null;

            // Assert - All responders should receive the message
            Assert.AreEqual(3, HandledTestResponder.invokeCount,
                "With Handled=false (default), all responders should receive the message");
        }

        /// <summary>
        /// Test that Handled=true stops propagation to remaining responders
        /// </summary>
        [UnityTest]
        public IEnumerator HandledFlag_True_StopsPropagation()
        {
            // Arrange - Create node with 3 responders, first one handles the message
            rootNode = CreateNodeWithResponders(3, handleMessage: true, handlerIndex: 0);
            yield return null;

            // Act
            HandledTestResponder.invokeCount = 0;
            var relay = rootNode.GetComponent<MmRelayNode>();
            relay.MmInvoke(MmMethod.Initialize);

            yield return null;

            // Assert - Only first responder should receive the message
            Assert.AreEqual(1, HandledTestResponder.invokeCount,
                "With Handled=true, remaining responders should NOT receive the message");
        }

        /// <summary>
        /// Test that Handled flag doesn't pollute between new messages
        /// </summary>
        [UnityTest]
        public IEnumerator HandledFlag_ResetsForNewMessages()
        {
            // Arrange
            rootNode = CreateNodeWithResponders(3, handleMessage: false);
            yield return null;

            var relay = rootNode.GetComponent<MmRelayNode>();

            // Act - Send first message
            HandledTestResponder.invokeCount = 0;
            var msg1 = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid);
            msg1.Handled = true; // Set handled on this message
            relay.MmInvoke(msg1);

            yield return null;

            int firstCount = HandledTestResponder.invokeCount;

            // Act - Send second message (new message, should not be handled)
            HandledTestResponder.invokeCount = 0;
            var msg2 = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid);
            relay.MmInvoke(msg2);

            yield return null;

            // Assert - Second message should reach all responders
            Assert.AreEqual(3, HandledTestResponder.invokeCount,
                "New messages should not inherit Handled state from previous messages");
        }

        /// <summary>
        /// Test that ReceiveHandledMessages=true allows responder to see handled messages
        /// </summary>
        [UnityTest]
        public IEnumerator ReceiveHandledMessages_True_ReceivesHandledMessages()
        {
            // Arrange - Create node with handler, then a responder that wants handled messages
            rootNode = new GameObject("Root");
            testObjects.Add(rootNode);

            var relay = rootNode.AddComponent<MmRelayNode>();

            // First responder handles the message
            var handler = rootNode.AddComponent<HandledTestResponder>();
            handler.shouldHandle = true;
            handler.responderId = 0;

            // Second responder should not receive (default)
            var normalResponder = rootNode.AddComponent<HandledTestResponder>();
            normalResponder.shouldHandle = false;
            normalResponder.responderId = 1;

            // Third responder has ReceiveHandledMessages=true
            var alwaysResponder = rootNode.AddComponent<HandledTestResponder>();
            alwaysResponder.shouldHandle = false;
            alwaysResponder.ReceiveHandledMessages = true;
            alwaysResponder.responderId = 2;

            yield return null;
            relay.MmRefreshResponders();
            yield return null;

            // Act
            HandledTestResponder.invokeOrder.Clear();
            relay.MmInvoke(MmMethod.Initialize);

            yield return null;

            // Assert - First (handler) and third (ReceiveHandledMessages) should receive
            Assert.IsTrue(HandledTestResponder.invokeOrder.Contains(0),
                "Handler should receive the message");
            Assert.IsFalse(HandledTestResponder.invokeOrder.Contains(1),
                "Normal responder should NOT receive handled message");
            Assert.IsTrue(HandledTestResponder.invokeOrder.Contains(2),
                "Responder with ReceiveHandledMessages=true SHOULD receive handled message");
        }

        /// <summary>
        /// Test Handled flag with ToChildren routing
        /// </summary>
        [UnityTest]
        public IEnumerator HandledFlag_ToChildren_StopsPropagation()
        {
            // Arrange - Create parent with children
            rootNode = new GameObject("Root");
            testObjects.Add(rootNode);
            var relay = rootNode.AddComponent<MmRelayNode>();

            // Create child nodes
            var childRelays = new List<MmRelayNode>();
            for (int i = 0; i < 3; i++)
            {
                var child = new GameObject($"Child_{i}");
                child.transform.SetParent(rootNode.transform);
                var childRelay = child.AddComponent<MmRelayNode>();
                var responder = child.AddComponent<HandledTestResponder>();
                responder.responderId = i;
                responder.shouldHandle = (i == 0); // First child handles
                testObjects.Add(child);
                childRelays.Add(childRelay);

                // CRITICAL: Register child relay with parent's routing table for ToChildren() to work
                relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(relay);
            }

            yield return null;
            // Refresh child relays to register their responders
            foreach (var childRelay in childRelays)
            {
                childRelay.MmRefreshResponders();
            }
            yield return null;

            // Act
            HandledTestResponder.invokeOrder.Clear();
            relay.Send(MmMethod.Initialize).ToChildren().Execute();

            yield return null;

            // Assert - Only first child should receive
            Assert.AreEqual(1, HandledTestResponder.invokeOrder.Count,
                "With Handled=true on first child, remaining children should NOT receive");
        }

        /// <summary>
        /// Test that Handled flag is preserved in message copies
        /// </summary>
        [Test]
        public void HandledFlag_CopyPreservesFlag()
        {
            // Arrange
            var original = new MmMessage(MmMethod.Initialize);
            original.Handled = true;

            // Act
            var copy = original.Copy();

            // Assert
            Assert.IsTrue(copy.Handled, "Message copy should preserve Handled flag");
        }

        /// <summary>
        /// Test that default Handled value is false
        /// </summary>
        [Test]
        public void HandledFlag_DefaultIsFalse()
        {
            // Arrange & Act
            var message = new MmMessage(MmMethod.Initialize);

            // Assert
            Assert.IsFalse(message.Handled, "Default Handled value should be false");
        }

        #region Helper Methods

        private GameObject CreateNodeWithResponders(int count, bool handleMessage, int handlerIndex = 0)
        {
            var node = new GameObject("TestNode");
            testObjects.Add(node);

            var relay = node.AddComponent<MmRelayNode>();

            for (int i = 0; i < count; i++)
            {
                var responder = node.AddComponent<HandledTestResponder>();
                responder.responderId = i;
                responder.shouldHandle = handleMessage && (i == handlerIndex);
            }

            // CRITICAL: Refresh responders after adding them, since MmRelayNode.Start()
            // already ran before the responders were added
            relay.MmRefreshResponders();

            return node;
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Test responder that optionally sets Handled=true
        /// </summary>
        private class HandledTestResponder : MmBaseResponder
        {
            public static int invokeCount = 0;
            public static List<int> invokeOrder = new List<int>();

            public int responderId = 0;
            public bool shouldHandle = false;

            public override void Initialize()
            {
                invokeCount++;
                invokeOrder.Add(responderId);

                // Get the message and set Handled if configured
                // Note: We need to access the current message being processed
                // Since Initialize() doesn't have access to the message directly,
                // we use MmInvoke override instead
            }

            public override void MmInvoke(MmMessage message)
            {
                if (message.MmMethod == MmMethod.Initialize)
                {
                    invokeCount++;
                    invokeOrder.Add(responderId);

                    if (shouldHandle)
                    {
                        message.Handled = true;
                    }
                }
                else
                {
                    base.MmInvoke(message);
                }
            }
        }

        #endregion
    }
}
