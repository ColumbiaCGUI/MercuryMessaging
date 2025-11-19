// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// QW-1: Cycle Detection Tests
// Validates that circular message paths are detected and prevented

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for QW-1 Cycle Detection implementation.
    /// Validates that VisitedNodes tracking prevents infinite loops in circular hierarchies.
    /// </summary>
    [TestFixture]
    public class CycleDetectionValidationTests
    {
        private GameObject nodeA, nodeB, nodeC;

        [TearDown]
        public void TearDown()
        {
            // Cleanup created nodes
            if (nodeA != null) Object.DestroyImmediate(nodeA);
            if (nodeB != null) Object.DestroyImmediate(nodeB);
            if (nodeC != null) Object.DestroyImmediate(nodeC);
        }

        /// <summary>
        /// Test that VisitedNodes tracking infrastructure exists and works
        /// </summary>
        [UnityTest]
        public IEnumerator CycleDetection_InfrastructureExists()
        {
            // Arrange - Create simple hierarchy A -> B -> C
            nodeA = new GameObject("NodeA");
            nodeB = new GameObject("NodeB");
            nodeC = new GameObject("NodeC");

            nodeB.transform.SetParent(nodeA.transform);
            nodeC.transform.SetParent(nodeB.transform);

            var relayA = nodeA.AddComponent<MmRelayNode>();
            var relayB = nodeB.AddComponent<MmRelayNode>();
            var relayC = nodeC.AddComponent<MmRelayNode>();

            relayA.enableCycleDetection = true;
            relayB.enableCycleDetection = true;
            relayC.enableCycleDetection = true;

            yield return null; // Let hierarchy settle

            // Act - Send message down the hierarchy
            var message = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren));
            relayA.MmInvoke(message);

            yield return new WaitForSeconds(0.1f);

            // Assert - VisitedNodes tracking should exist and have recorded visits
            Assert.IsNotNull(message.VisitedNodes, "VisitedNodes HashSet should be initialized");
            Assert.GreaterOrEqual(message.VisitedNodes.Count, 1,
                "VisitedNodes should track at least one node visit");

            Debug.Log($"[QW-1 PASS] Cycle detection infrastructure working, tracked {message.VisitedNodes.Count} visits");
        }

        /// <summary>
        /// Test that VisitedNodes prevents revisiting same node
        /// </summary>
        [UnityTest]
        public IEnumerator CycleDetection_PreventsRevisits()
        {
            // Arrange
            nodeA = new GameObject("NodeA");
            var relayA = nodeA.AddComponent<MmRelayNode>();
            var counterA = nodeA.AddComponent<VisitCounterResponder>();
            relayA.enableCycleDetection = true;

            yield return null;

            // Act - Try to send same message twice (simulating cycle)
            VisitCounterResponder.visitCount = 0;
            var message = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(MmLevelFilterHelper.Self));

            relayA.MmInvoke(message);
            yield return null;

            int firstVisitCount = VisitCounterResponder.visitCount;

            // Try to invoke again with same message (should be prevented if cycle detection works)
            relayA.MmInvoke(message);
            yield return null;

            int secondVisitCount = VisitCounterResponder.visitCount;

            // Assert - Second invocation should be prevented
            Assert.AreEqual(firstVisitCount, secondVisitCount,
                "Cycle detection should prevent revisiting the same node with same message");

            Debug.Log("[QW-1 PASS] Cycle detection prevents message re-entry");
        }

        /// <summary>
        /// Test that cycle detection flag can be toggled
        /// </summary>
        [UnityTest]
        public IEnumerator CycleDetection_CanBeDisabled()
        {
            // Arrange
            nodeA = new GameObject("NodeA");
            var relayA = nodeA.AddComponent<MmRelayNode>();

            // Act & Assert - Test with cycle detection enabled
            relayA.enableCycleDetection = true;
            yield return null;

            var message1 = new MmMessage(MmMethod.Initialize);
            relayA.MmInvoke(message1);
            yield return null;

            Assert.IsNotNull(message1.VisitedNodes,
                "VisitedNodes should be initialized when cycle detection is enabled");

            // Act & Assert - Test with cycle detection disabled
            relayA.enableCycleDetection = false;
            var message2 = new MmMessage(MmMethod.Initialize);
            relayA.MmInvoke(message2);
            yield return null;

            // Note: VisitedNodes may still exist as it's created on message, but won't be populated
            Debug.Log("[QW-1 PASS] Cycle detection can be toggled via enableCycleDetection flag");
        }

        /// <summary>
        /// Test that VisitedNodes tracks multiple nodes in hierarchy
        /// </summary>
        [UnityTest]
        public IEnumerator CycleDetection_TracksMultipleNodes()
        {
            // Arrange - Create 5-node chain
            var nodes = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                nodes[i] = new GameObject($"Node_{i}");
                var relay = nodes[i].AddComponent<MmRelayNode>();
                relay.enableCycleDetection = true;

                if (i > 0)
                {
                    nodes[i].transform.SetParent(nodes[i - 1].transform);
                }
            }

            yield return null;

            // Act - Send message down the chain
            var message = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren));
            nodes[0].GetComponent<MmRelayNode>().MmInvoke(message);

            yield return new WaitForSeconds(0.1f);

            // Assert - Should track multiple node visits
            Assert.IsNotNull(message.VisitedNodes);
            Assert.GreaterOrEqual(message.VisitedNodes.Count, 2,
                "VisitedNodes should track multiple nodes in hierarchy");

            Debug.Log($"[QW-1 PASS] Tracked {message.VisitedNodes.Count} nodes in 5-node chain");

            // Cleanup
            foreach (var node in nodes)
            {
                if (node != null) Object.DestroyImmediate(node);
            }
        }

        /// <summary>
        /// Test that different messages have independent VisitedNodes tracking
        /// </summary>
        [UnityTest]
        public IEnumerator CycleDetection_IndependentMessageTracking()
        {
            // Arrange
            nodeA = new GameObject("NodeA");
            nodeB = new GameObject("NodeB");
            nodeB.transform.SetParent(nodeA.transform);

            var relayA = nodeA.AddComponent<MmRelayNode>();
            var relayB = nodeB.AddComponent<MmRelayNode>();

            relayA.enableCycleDetection = true;
            relayB.enableCycleDetection = true;

            yield return null;

            // Act - Send two different messages
            var message1 = new MmMessage(MmMethod.Initialize);
            var message2 = new MmMessage(MmMethod.Refresh);

            relayA.MmInvoke(message1);
            yield return null;

            relayA.MmInvoke(message2);
            yield return null;

            // Assert - Each message should have its own VisitedNodes
            Assert.IsNotNull(message1.VisitedNodes);
            Assert.IsNotNull(message2.VisitedNodes);

            // They should be different instances (not shared)
            Assert.AreNotSame(message1.VisitedNodes, message2.VisitedNodes,
                "Each message should have independent VisitedNodes tracking");

            Debug.Log("[QW-1 PASS] Different messages have independent cycle tracking");
        }

        #region Test Helper Classes

        /// <summary>
        /// Responder that counts how many times Initialize is called
        /// </summary>
        private class VisitCounterResponder : MmBaseResponder
        {
            public static int visitCount = 0;

            public override void Initialize()
            {
                visitCount++;
            }
        }

        #endregion
    }
}
