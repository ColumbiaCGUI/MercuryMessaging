// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Phase 2.1: Advanced Message Routing Tests
// Validates Siblings, Cousins, Descendants, Ancestors, and Custom filtering

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for Phase 2.1 Advanced Message Routing features.
    /// Validates new level filters: Siblings, Cousins, Descendants, Ancestors, Custom.
    /// </summary>
    [TestFixture]
    public class AdvancedRoutingTests
    {
        private GameObject rootObject;
        private List<GameObject> testObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            testObjects.Clear();
            MessageCounterResponder.ResetAllCounters();
        }

        [TearDown]
        public void TearDown()
        {
            // Unity automatically cleans up GameObjects between tests
            testObjects.Clear();
            MessageCounterResponder.ResetAllCounters();
        }

        #region Siblings Routing Tests

        [UnityTest]
        public IEnumerator SiblingsRouting_WithLateralEnabled_ReachesSiblings()
        {
            // Arrange - Create parent with 3 children (siblings)
            //   Parent
            //   ├── Child1 (sender)
            //   ├── Child2 (sibling)
            //   └── Child3 (sibling)

            var parent = CreateNodeWithResponder("Parent");
            var child1 = CreateNodeWithResponder("Child1", parent.transform);
            var child2 = CreateNodeWithResponder("Child2", parent.transform);
            var child3 = CreateNodeWithResponder("Child3", parent.transform);

            // Explicitly refresh responders to ensure they're registered
            parent.GetComponent<MmRelayNode>().MmRefreshResponders();
            child1.GetComponent<MmRelayNode>().MmRefreshResponders();
            child2.GetComponent<MmRelayNode>().MmRefreshResponders();
            child3.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null; // Let hierarchy settle
            yield return null; // Extra frame for responder registration

            // Act - Send message from Child1 to siblings
            var child1Relay = child1.GetComponent<MmRelayNode>();
            var options = MmRoutingOptions.WithLateralRouting();
            var metadata = new MmMetadataBlock(MmLevelFilter.Siblings);
            metadata.Options = options;

            child1Relay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Child2 and Child3 should receive, Child1 and Parent should not
            Assert.AreEqual(0, GetMessageCount(child1), "Child1 (sender) should not receive");
            Assert.AreEqual(1, GetMessageCount(child2), "Child2 (sibling) should receive");
            Assert.AreEqual(1, GetMessageCount(child3), "Child3 (sibling) should receive");
            Assert.AreEqual(0, GetMessageCount(parent), "Parent should not receive");

            Debug.Log("[PASS] Siblings routing reaches sibling nodes correctly");
        }

        [UnityTest]
        public IEnumerator SiblingsRouting_WithoutLateralEnabled_Blocked()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child1 = CreateNodeWithResponder("Child1", parent.transform);
            var child2 = CreateNodeWithResponder("Child2", parent.transform);

            yield return null;

            // Act - Try to send without AllowLateralRouting
            var child1Relay = child1.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilter.Siblings);
            // No MmRoutingOptions.AllowLateralRouting

            child1Relay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Should be blocked (no lateral routing allowed)
            Assert.AreEqual(0, GetMessageCount(child2), "Sibling routing should be blocked without AllowLateralRouting");

            Debug.Log("[PASS] Siblings routing blocked without lateral routing enabled");
        }

        #endregion

        #region Cousins Routing Tests

        [UnityTest]
        public IEnumerator CousinsRouting_WithLateralEnabled_ReachesCousins()
        {
            // Arrange - Create family tree with cousins
            //   Grandparent
            //   ├── Parent1
            //   │   ├── Child1 (sender)
            //   │   └── Child2
            //   └── Parent2 (parent's sibling)
            //       ├── Cousin1
            //       └── Cousin2

            var grandparent = CreateNodeWithResponder("Grandparent");
            var parent1 = CreateNodeWithResponder("Parent1", grandparent.transform);
            var parent2 = CreateNodeWithResponder("Parent2", grandparent.transform);
            var child1 = CreateNodeWithResponder("Child1", parent1.transform);
            var child2 = CreateNodeWithResponder("Child2", parent1.transform);
            var cousin1 = CreateNodeWithResponder("Cousin1", parent2.transform);
            var cousin2 = CreateNodeWithResponder("Cousin2", parent2.transform);

            // Explicitly refresh responders to ensure they're registered
            grandparent.GetComponent<MmRelayNode>().MmRefreshResponders();
            parent1.GetComponent<MmRelayNode>().MmRefreshResponders();
            parent2.GetComponent<MmRelayNode>().MmRefreshResponders();
            child1.GetComponent<MmRelayNode>().MmRefreshResponders();
            child2.GetComponent<MmRelayNode>().MmRefreshResponders();
            cousin1.GetComponent<MmRelayNode>().MmRefreshResponders();
            cousin2.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Send message from Child1 to cousins
            var child1Relay = child1.GetComponent<MmRelayNode>();
            var options = MmRoutingOptions.WithLateralRouting();
            var metadata = new MmMetadataBlock(MmLevelFilter.Cousins);
            metadata.Options = options;

            child1Relay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Cousins should receive
            Assert.AreEqual(1, GetMessageCount(cousin1), "Cousin1 should receive");
            Assert.AreEqual(1, GetMessageCount(cousin2), "Cousin2 should receive");
            Assert.AreEqual(0, GetMessageCount(child2), "Sibling Child2 should not receive (only cousins targeted)");
            Assert.AreEqual(0, GetMessageCount(parent1), "Parent1 should not receive");

            Debug.Log("[PASS] Cousins routing reaches cousin nodes correctly");
        }

        #endregion

        #region Descendants Routing Tests

        [UnityTest]
        public IEnumerator DescendantsRouting_ReachesAllChildren()
        {
            // Arrange - Create deep hierarchy
            //   Root
            //   └── Child
            //       └── Grandchild
            //           └── GreatGrandchild

            var root = CreateNodeWithResponder("Root");
            var child = CreateNodeWithResponder("Child", root.transform);
            var grandchild = CreateNodeWithResponder("Grandchild", child.transform);
            var greatGrandchild = CreateNodeWithResponder("GreatGrandchild", grandchild.transform);

            // Explicitly refresh responders to ensure they're registered
            root.GetComponent<MmRelayNode>().MmRefreshResponders();
            child.GetComponent<MmRelayNode>().MmRefreshResponders();
            grandchild.GetComponent<MmRelayNode>().MmRefreshResponders();
            greatGrandchild.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Send message from Root to all descendants
            var rootRelay = root.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilter.Descendants);

            rootRelay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - All descendants should receive
            Assert.AreEqual(1, GetMessageCount(child), "Child should receive");
            Assert.AreEqual(1, GetMessageCount(grandchild), "Grandchild should receive");
            Assert.AreEqual(1, GetMessageCount(greatGrandchild), "GreatGrandchild should receive");
            Assert.AreEqual(0, GetMessageCount(root), "Root (sender) should not receive");

            Debug.Log("[PASS] Descendants routing reaches all descendant nodes");
        }

        [UnityTest]
        public IEnumerator DescendantsRouting_PreventsCircularLoops()
        {
            // Arrange - Create hierarchy (no actual circular reference possible, but test visited tracking)
            var root = CreateNodeWithResponder("Root");
            var child = CreateNodeWithResponder("Child", root.transform);

            // Explicitly refresh responders to ensure they're registered
            root.GetComponent<MmRelayNode>().MmRefreshResponders();
            child.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Multiple sends should not cause issues
            var rootRelay = root.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilter.Descendants);

            for (int i = 0; i < 5; i++)
            {
                rootRelay.MmInvoke(MmMethod.Initialize, metadata);
            }

            yield return new WaitForSeconds(0.1f);

            // Assert - Should receive 5 messages, not infinite
            Assert.AreEqual(5, GetMessageCount(child), "Child should receive exactly 5 messages");

            Debug.Log("[PASS] Descendants routing handles multiple sends correctly");
        }

        #endregion

        #region Ancestors Routing Tests

        [UnityTest]
        public IEnumerator AncestorsRouting_ReachesAllParents()
        {
            // Arrange - Create deep hierarchy
            //   GreatGrandparent
            //   └── Grandparent
            //       └── Parent
            //           └── Child (sender)

            var greatGrandparent = CreateNodeWithResponder("GreatGrandparent");
            var grandparent = CreateNodeWithResponder("Grandparent", greatGrandparent.transform);
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);

            // Explicitly refresh responders to ensure they're registered
            greatGrandparent.GetComponent<MmRelayNode>().MmRefreshResponders();
            grandparent.GetComponent<MmRelayNode>().MmRefreshResponders();
            parent.GetComponent<MmRelayNode>().MmRefreshResponders();
            child.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Send message from Child to all ancestors
            var childRelay = child.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilter.Ancestors);

            childRelay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - All ancestors should receive
            Assert.AreEqual(1, GetMessageCount(parent), "Parent should receive");
            Assert.AreEqual(1, GetMessageCount(grandparent), "Grandparent should receive");
            Assert.AreEqual(1, GetMessageCount(greatGrandparent), "GreatGrandparent should receive");
            Assert.AreEqual(0, GetMessageCount(child), "Child (sender) should not receive");

            Debug.Log("[PASS] Ancestors routing reaches all ancestor nodes");
        }

        #endregion

        #region Custom Filter Tests

        [UnityTest]
        public IEnumerator CustomFilter_WithPredicate_FiltersCorrectly()
        {
            // Arrange - Create nodes with different tags
            var root = CreateNodeWithResponder("Root");
            var taggedChild = CreateNodeWithResponder("TaggedChild", root.transform);
            var untaggedChild = CreateNodeWithResponder("UntaggedChild", root.transform);

            // Tag one child with "Player" tag
            taggedChild.tag = "Player";

            // Explicitly refresh responders to ensure they're registered
            root.GetComponent<MmRelayNode>().MmRefreshResponders();
            taggedChild.GetComponent<MmRelayNode>().MmRefreshResponders();
            untaggedChild.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Send message with custom filter (only Player tagged)
            var rootRelay = root.GetComponent<MmRelayNode>();
            var options = new MmRoutingOptions
            {
                CustomFilter = (node) => node.tag == "Player"
            };
            var metadata = new MmMetadataBlock(MmLevelFilter.Custom);
            metadata.Options = options;

            rootRelay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Only tagged child should receive
            Assert.AreEqual(1, GetMessageCount(taggedChild), "Tagged child should receive");
            Assert.AreEqual(0, GetMessageCount(untaggedChild), "Untagged child should not receive");

            Debug.Log("[PASS] Custom filter applies predicate correctly");
        }

        [UnityTest]
        public IEnumerator CustomFilter_WithoutPredicate_Blocked()
        {
            // Arrange
            var root = CreateNodeWithResponder("Root");
            var child = CreateNodeWithResponder("Child", root.transform);

            yield return null;

            // Act - Try custom filter without predicate
            var rootRelay = root.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilter.Custom);
            // No MmRoutingOptions.CustomFilter

            rootRelay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Should be blocked
            Assert.AreEqual(0, GetMessageCount(child), "Custom filter should be blocked without predicate");

            Debug.Log("[PASS] Custom filter blocked without predicate");
        }

        #endregion

        #region Combined Filters Tests

        [UnityTest]
        public IEnumerator CombinedFilters_SelfAndSiblings_Works()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child1 = CreateNodeWithResponder("Child1", parent.transform);
            var child2 = CreateNodeWithResponder("Child2", parent.transform);

            // Explicitly refresh responders to ensure they're registered
            parent.GetComponent<MmRelayNode>().MmRefreshResponders();
            child1.GetComponent<MmRelayNode>().MmRefreshResponders();
            child2.GetComponent<MmRelayNode>().MmRefreshResponders();

            yield return null;
            yield return null; // Extra frame for responder registration

            // Act - Use combined filter (Self + Siblings)
            var child1Relay = child1.GetComponent<MmRelayNode>();
            var options = MmRoutingOptions.WithLateralRouting();
            var metadata = new MmMetadataBlock(MmLevelFilterHelper.SelfAndSiblings);
            metadata.Options = options;

            child1Relay.MmInvoke(MmMethod.Initialize, metadata);

            yield return new WaitForSeconds(0.1f);

            // Assert - Both self and sibling should receive
            Assert.AreEqual(1, GetMessageCount(child1), "Child1 (self) should receive");
            Assert.AreEqual(1, GetMessageCount(child2), "Child2 (sibling) should receive");

            Debug.Log("[PASS] Combined filters (Self + Siblings) work correctly");
        }

        #endregion

        #region Helper Methods

        private GameObject CreateNodeWithResponder(string name, Transform parent = null)
        {
            var obj = new GameObject(name);
            var relay = obj.AddComponent<MmRelayNode>();
            obj.AddComponent<MessageCounterResponder>();

            // CRITICAL: Refresh responders so MessageCounterResponder is registered with Level=Self
            relay.MmRefreshResponders();

            if (parent != null)
            {
                obj.transform.SetParent(parent);

                // Explicitly register child in parent's routing table
                var parentRelay = parent.GetComponent<MmRelayNode>();
                if (parentRelay != null)
                {
                    parentRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                    relay.AddParent(parentRelay);
                }
            }

            testObjects.Add(obj);
            return obj;
        }

        private int GetMessageCount(GameObject obj)
        {
            var counter = obj.GetComponent<MessageCounterResponder>();
            return counter != null ? counter.MessageCount : 0;
        }

        #endregion

        #region Test Responder

        /// <summary>
        /// Simple responder that counts messages received for testing.
        /// </summary>
        public class MessageCounterResponder : MmBaseResponder
        {
            public int MessageCount { get; private set; } = 0;
            private static List<MessageCounterResponder> allCounters = new List<MessageCounterResponder>();

            public override void Awake()
            {
                base.Awake();
                allCounters.Add(this);
            }

            private void OnDestroy()
            {
                allCounters.Remove(this);
            }

            public override void Initialize()
            {
                MessageCount++;
            }

            protected override void ReceivedMessage(MmMessageString message)
            {
                MessageCount++;
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
                MessageCount++;
            }

            protected override void ReceivedMessage(MmMessageBool message)
            {
                MessageCount++;
            }

            public static void ResetAllCounters()
            {
                foreach (var counter in allCounters)
                {
                    if (counter != null)
                        counter.MessageCount = 0;
                }
            }
        }

        #endregion
    }
}
