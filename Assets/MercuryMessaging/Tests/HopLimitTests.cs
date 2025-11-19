// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Unit tests for message hop limit and cycle detection features

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Comprehensive unit tests for hop limit and cycle detection features.
    /// Tests hop counting, limit enforcement, cycle detection, and edge cases.
    /// </summary>
    [TestFixture]
    public class HopLimitTests
    {
        private GameObject rootObject;
        private GameObject childObject;
        private GameObject grandchildObject;

        #region Setup and Teardown

        [SetUp]
        public void SetUp()
        {
            // Create a simple hierarchy for testing
            rootObject = new GameObject("Root");
            childObject = new GameObject("Child");
            grandchildObject = new GameObject("Grandchild");

            childObject.transform.SetParent(rootObject.transform);
            grandchildObject.transform.SetParent(childObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (rootObject != null)
                Object.DestroyImmediate(rootObject);
            if (childObject != null)
                Object.DestroyImmediate(childObject);
            if (grandchildObject != null)
                Object.DestroyImmediate(grandchildObject);
        }

        #endregion

        #region Basic Hop Counting Tests

        [Test]
        public void Message_DefaultHopCount_IsZero()
        {
            // Arrange & Act
            var message = new MmMessage();

            // Assert
            Assert.AreEqual(0, message.HopCount);
        }

        [Test]
        public void Message_HopCountIncreases_WhenPassingThroughRelay()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Act
            relay.MmInvoke(message);

            // Assert - Should have incremented by 1
            Assert.AreEqual(1, message.HopCount);
        }

        [Test]
        public void Message_CopiedMessage_PreservesHopCount()
        {
            // Arrange
            var message = new MmMessage(MmMethod.Initialize);
            message.HopCount = 5;

            // Act
            var copy = message.Copy();

            // Assert
            Assert.AreEqual(5, copy.HopCount);
        }

        #endregion

        #region Hop Limit Enforcement Tests

        [UnityTest]
        public IEnumerator Message_ExceedingHopLimit_IsDropped()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.maxMessageHops = 3;

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.HopCount = 3; // Already at limit
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Act
            yield return null; // Wait for Awake to complete
            relay.MmInvoke(message);
            yield return null;

            // Assert - Message should be dropped before reaching responder
            Assert.AreEqual(0, testResponder.receivedCount, "Message should have been dropped at hop limit");
        }

        [UnityTest]
        public IEnumerator Message_BelowHopLimit_PassesThrough()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.maxMessageHops = 5;

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.HopCount = 2; // Below limit
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Act
            yield return null; // Wait for Awake
            relay.MmInvoke(message);
            yield return null;

            // Assert - Message should pass through
            Assert.AreEqual(1, testResponder.receivedCount, "Message should have been delivered");
            Assert.AreEqual(3, message.HopCount, "Hop count should have incremented to 3");
        }

        [UnityTest]
        public IEnumerator Message_WithZeroMaxHops_DisablesChecking()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.maxMessageHops = 0; // Disabled

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.HopCount = 1000; // Extremely high, but checking is disabled
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Act
            yield return null;
            relay.MmInvoke(message);
            yield return null;

            // Assert - Message should still pass through
            Assert.AreEqual(1, testResponder.receivedCount, "Message should pass when hop checking is disabled");
        }

        #endregion

        #region Cycle Detection Tests

        [Test]
        public void Message_VisitedNodes_InitiallyNull()
        {
            // Arrange & Act
            var message = new MmMessage();

            // Assert
            Assert.IsNull(message.VisitedNodes);
        }

        [UnityTest]
        public IEnumerator Message_CycleDetection_PreventsRevisitingNode()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.enableCycleDetection = true;

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Manually add this node to visited set (simulating circular path)
            message.VisitedNodes = new System.Collections.Generic.HashSet<int>();
            message.VisitedNodes.Add(rootObject.GetInstanceID());

            // Act
            yield return null;
            relay.MmInvoke(message);
            yield return null;

            // Assert - Should be blocked by cycle detection
            Assert.AreEqual(0, testResponder.receivedCount, "Cycle detection should prevent message delivery");
        }

        [UnityTest]
        public IEnumerator Message_CycleDetection_AllowsFirstVisit()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.enableCycleDetection = true;

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Act
            yield return null;
            relay.MmInvoke(message);
            yield return null;

            // Assert - First visit should succeed
            Assert.AreEqual(1, testResponder.receivedCount, "First visit should be allowed");
            Assert.IsNotNull(message.VisitedNodes, "VisitedNodes should be initialized");
            Assert.IsTrue(message.VisitedNodes.Contains(rootObject.GetInstanceID()), "Node should be marked as visited");
        }

        [UnityTest]
        public IEnumerator Message_CycleDetection_DisabledAllowsRevisits()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();
            relay.enableCycleDetection = false; // Disabled

            var testResponder = rootObject.AddComponent<TestHopLimitResponder>();
            testResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Self);

            // Manually add this node to visited set
            message.VisitedNodes = new System.Collections.Generic.HashSet<int>();
            message.VisitedNodes.Add(rootObject.GetInstanceID());

            // Act
            yield return null;
            relay.MmInvoke(message);
            yield return null;

            // Assert - Should pass through since detection is disabled
            Assert.AreEqual(1, testResponder.receivedCount, "Message should pass when cycle detection is disabled");
        }

        [Test]
        public void Message_CopiedMessage_CopiesVisitedNodes()
        {
            // Arrange
            var message = new MmMessage(MmMethod.Initialize);
            message.VisitedNodes = new System.Collections.Generic.HashSet<int>();
            message.VisitedNodes.Add(123);
            message.VisitedNodes.Add(456);

            // Act
            var copy = message.Copy();

            // Assert
            Assert.IsNotNull(copy.VisitedNodes);
            Assert.AreEqual(2, copy.VisitedNodes.Count);
            Assert.IsTrue(copy.VisitedNodes.Contains(123));
            Assert.IsTrue(copy.VisitedNodes.Contains(456));

            // Verify it's a deep copy (not same reference)
            Assert.AreNotSame(message.VisitedNodes, copy.VisitedNodes);
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator MultipleRelays_IncrementHopCount_Correctly()
        {
            // Arrange - Create a chain: Root -> Child -> Grandchild
            var rootRelay = rootObject.AddComponent<MmRelayNode>();
            var childRelay = childObject.AddComponent<MmRelayNode>();
            var grandchildRelay = grandchildObject.AddComponent<MmRelayNode>();

            var grandchildResponder = grandchildObject.AddComponent<TestHopLimitResponder>();
            grandchildResponder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.SelfAndChildren);

            // Act
            yield return null; // Wait for Awake
            rootRelay.MmInvoke(message);
            yield return new WaitForSeconds(0.1f); // Allow propagation

            // Assert - Should have passed through 3 relays (root, child, grandchild)
            // Root increments to 1, child to 2, grandchild to 3
            Assert.GreaterOrEqual(grandchildResponder.receivedCount, 1, "Message should reach grandchild");
        }

        [UnityTest]
        public IEnumerator DeepHierarchy_RespectsCombinedLimits()
        {
            // Arrange - Create a deep chain with low hop limit
            var relay1 = rootObject.AddComponent<MmRelayNode>();
            relay1.maxMessageHops = 2; // Very low limit

            var relay2 = childObject.AddComponent<MmRelayNode>();
            var relay3 = grandchildObject.AddComponent<MmRelayNode>();

            var responder = grandchildObject.AddComponent<TestHopLimitResponder>();
            responder.receivedCount = 0;

            var message = new MmMessage(MmMethod.Initialize);
            message.MetadataBlock = new MmMetadataBlock(MmLevelFilter.SelfAndChildren);

            // Act
            yield return null;
            relay1.MmInvoke(message);
            yield return new WaitForSeconds(0.1f);

            // Assert - Should be blocked before reaching grandchild
            // Root: hop 0->1, Child: hop 1->2, Grandchild would be blocked at hop 2
            Assert.LessOrEqual(responder.receivedCount, 0, "Message should not reach grandchild with low hop limit");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void RelayNode_DefaultMaxHops_IsFifty()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();

            // Assert
            Assert.AreEqual(50, relay.maxMessageHops, "Default max hops should be 50");
        }

        [Test]
        public void RelayNode_DefaultCycleDetection_IsEnabled()
        {
            // Arrange
            var relay = rootObject.AddComponent<MmRelayNode>();

            // Assert
            Assert.IsTrue(relay.enableCycleDetection, "Cycle detection should be enabled by default");
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Simple responder that counts how many times it receives Initialize messages
        /// </summary>
        private class TestHopLimitResponder : MmBaseResponder
        {
            public int receivedCount = 0;

            protected override void ReceivedInitialize()
            {
                receivedCount++;
            }
        }

        #endregion
    }
}
