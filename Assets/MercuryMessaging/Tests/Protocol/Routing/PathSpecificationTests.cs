// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Phase 2.1: Path Specification Tests
// Validates path parsing, resolution, wildcard support, and message delivery

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for Phase 2.1 Path Specification feature.
    /// Validates path parsing, node resolution, wildcards, and message delivery.
    /// </summary>
    [TestFixture]
    public class PathSpecificationTests
    {
        private List<GameObject> testObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            testObjects.Clear();
            AdvancedRoutingTests.MessageCounterResponder.ResetAllCounters();
            MmPathSpecification.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            testObjects.Clear();
            AdvancedRoutingTests.MessageCounterResponder.ResetAllCounters();
            MmPathSpecification.ClearCache();
        }

        #region Parsing Tests

        [Test]
        public void Parse_ValidSingleSegment_Success()
        {
            var parsed = MmPathSpecification.Parse("parent");
            Assert.AreEqual(1, parsed.Segments.Length);
            Assert.AreEqual(PathSegment.Parent, parsed.Segments[0]);
        }

        [Test]
        public void Parse_ValidMultipleSegments_Success()
        {
            var parsed = MmPathSpecification.Parse("parent/sibling/child");
            Assert.AreEqual(3, parsed.Segments.Length);
            Assert.AreEqual(PathSegment.Parent, parsed.Segments[0]);
            Assert.AreEqual(PathSegment.Sibling, parsed.Segments[1]);
            Assert.AreEqual(PathSegment.Child, parsed.Segments[2]);
        }

        [Test]
        public void Parse_AllSegmentTypes_Success()
        {
            var paths = new string[] { "parent", "child", "sibling", "self", "ancestor", "descendant" };
            var expected = new PathSegment[] {
                PathSegment.Parent, PathSegment.Child, PathSegment.Sibling,
                PathSegment.Self, PathSegment.Ancestor, PathSegment.Descendant
            };

            for (int i = 0; i < paths.Length; i++)
            {
                var parsed = MmPathSpecification.Parse(paths[i]);
                Assert.AreEqual(expected[i], parsed.Segments[0], $"Failed for path: {paths[i]}");
            }
        }

        [Test]
        public void Parse_CaseInsensitive_Success()
        {
            var parsed1 = MmPathSpecification.Parse("PARENT");
            var parsed2 = MmPathSpecification.Parse("Parent");
            var parsed3 = MmPathSpecification.Parse("parent");

            Assert.AreEqual(PathSegment.Parent, parsed1.Segments[0]);
            Assert.AreEqual(PathSegment.Parent, parsed2.Segments[0]);
            Assert.AreEqual(PathSegment.Parent, parsed3.Segments[0]);
        }

        [Test]
        public void Parse_WithWhitespace_Success()
        {
            var parsed = MmPathSpecification.Parse("  parent / sibling / child  ");
            Assert.AreEqual(3, parsed.Segments.Length);
        }

        [Test]
        public void Parse_EmptyPath_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse(""));
        }

        [Test]
        public void Parse_NullPath_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse(null));
        }

        [Test]
        public void Parse_TrailingSlash_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse("parent/child/"));
        }

        [Test]
        public void Parse_InvalidSegment_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse("parent/invalid/child"));
        }

        [Test]
        public void Parse_Caching_WorksCorrectly()
        {
            // Clear cache first
            MmPathSpecification.ClearCache();
            Assert.AreEqual(0, MmPathSpecification.GetCacheSize());

            // Parse a path
            var parsed1 = MmPathSpecification.Parse("parent/child");
            Assert.AreEqual(1, MmPathSpecification.GetCacheSize());

            // Parse same path - should come from cache
            var parsed2 = MmPathSpecification.Parse("parent/child");
            Assert.AreEqual(1, MmPathSpecification.GetCacheSize());

            // Parse different path
            var parsed3 = MmPathSpecification.Parse("sibling/child");
            Assert.AreEqual(2, MmPathSpecification.GetCacheSize());
        }

        #endregion

        #region Wildcard Parsing Tests

        [Test]
        public void Parse_WildcardMiddle_Success()
        {
            var parsed = MmPathSpecification.Parse("parent/*/child");
            Assert.AreEqual(3, parsed.Segments.Length);
            Assert.AreEqual(PathSegment.Parent, parsed.Segments[0]);
            Assert.AreEqual(PathSegment.Wildcard, parsed.Segments[1]);
            Assert.AreEqual(PathSegment.Child, parsed.Segments[2]);
        }

        [Test]
        public void Parse_WildcardFirst_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse("*/child"));
        }

        [Test]
        public void Parse_WildcardLast_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse("parent/*"));
        }

        [Test]
        public void Parse_ConsecutiveWildcards_ThrowsException()
        {
            Assert.Throws<MmInvalidPathException>(() => MmPathSpecification.Parse("parent/*/*"));
        }

        #endregion

        #region Path Resolution Tests

        [UnityTest]
        public IEnumerator Resolve_Parent_FindsParent()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child = CreateNodeWithResponder("Child", parent.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            var targets = childRelay.ResolvePathTargets("parent");

            // Assert
            Assert.AreEqual(1, targets.Count, "Should find exactly 1 parent");
            Assert.AreEqual(parent.GetComponent<MmRelayNode>(), targets[0]);
        }

        [UnityTest]
        public IEnumerator Resolve_Child_FindsChildren()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child1 = CreateNodeWithResponder("Child1", parent.transform);
            var child2 = CreateNodeWithResponder("Child2", parent.transform);

            yield return null;
            yield return null;

            // Act
            var parentRelay = parent.GetComponent<MmRelayNode>();
            var targets = parentRelay.ResolvePathTargets("child");

            // Assert
            Assert.AreEqual(2, targets.Count, "Should find 2 children");
        }

        [UnityTest]
        public IEnumerator Resolve_Sibling_FindsSiblings()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child1 = CreateNodeWithResponder("Child1", parent.transform);
            var child2 = CreateNodeWithResponder("Child2", parent.transform);
            var child3 = CreateNodeWithResponder("Child3", parent.transform);

            yield return null;
            yield return null;

            // Act
            var child1Relay = child1.GetComponent<MmRelayNode>();
            var targets = child1Relay.ResolvePathTargets("sibling");

            // Assert
            Assert.AreEqual(2, targets.Count, "Should find 2 siblings (excluding self)");
        }

        [UnityTest]
        public IEnumerator Resolve_ParentSibling_FindsParentsSiblings()
        {
            // Arrange
            //   Grandparent
            //   ├── Parent
            //   │   └── Child (sender)
            //   ├── Aunt
            //   └── Uncle

            var grandparent = CreateNodeWithResponder("Grandparent");
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var aunt = CreateNodeWithResponder("Aunt", grandparent.transform);
            var uncle = CreateNodeWithResponder("Uncle", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            var targets = childRelay.ResolvePathTargets("parent/sibling");

            // Assert
            Assert.AreEqual(2, targets.Count, "Should find 2 parent's siblings (Aunt, Uncle)");
        }

        [UnityTest]
        public IEnumerator Resolve_ParentSiblingChild_FindsCousins()
        {
            // Arrange
            //   Grandparent
            //   ├── Parent
            //   │   └── Child (sender)
            //   ├── Aunt
            //   │   ├── Cousin1
            //   │   └── Cousin2
            //   └── Uncle
            //       └── Cousin3

            var grandparent = CreateNodeWithResponder("Grandparent");
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var aunt = CreateNodeWithResponder("Aunt", grandparent.transform);
            var uncle = CreateNodeWithResponder("Uncle", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);
            var cousin1 = CreateNodeWithResponder("Cousin1", aunt.transform);
            var cousin2 = CreateNodeWithResponder("Cousin2", aunt.transform);
            var cousin3 = CreateNodeWithResponder("Cousin3", uncle.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            var targets = childRelay.ResolvePathTargets("parent/sibling/child");

            // Assert
            Assert.AreEqual(3, targets.Count, "Should find 3 cousins");
        }

        [UnityTest]
        public IEnumerator Resolve_Descendant_FindsAllDescendants()
        {
            // Arrange
            //   Root
            //   └── Child
            //       └── Grandchild
            //           └── GreatGrandchild

            var root = CreateNodeWithResponder("Root");
            var child = CreateNodeWithResponder("Child", root.transform);
            var grandchild = CreateNodeWithResponder("Grandchild", child.transform);
            var greatGrandchild = CreateNodeWithResponder("GreatGrandchild", grandchild.transform);

            yield return null;
            yield return null;

            // Act
            var rootRelay = root.GetComponent<MmRelayNode>();
            var targets = rootRelay.ResolvePathTargets("descendant");

            // Assert
            Assert.AreEqual(3, targets.Count, "Should find all 3 descendants");
        }

        [UnityTest]
        public IEnumerator Resolve_Ancestor_FindsAllAncestors()
        {
            // Arrange
            //   GreatGrandparent
            //   └── Grandparent
            //       └── Parent
            //           └── Child (sender)

            var greatGrandparent = CreateNodeWithResponder("GreatGrandparent");
            var grandparent = CreateNodeWithResponder("Grandparent", greatGrandparent.transform);
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            var targets = childRelay.ResolvePathTargets("ancestor");

            // Assert
            Assert.AreEqual(3, targets.Count, "Should find all 3 ancestors");
        }

        [UnityTest]
        public IEnumerator Resolve_Self_FindsSelf()
        {
            // Arrange
            var node = CreateNodeWithResponder("Node");

            yield return null;
            yield return null;

            // Act
            var relay = node.GetComponent<MmRelayNode>();
            var targets = relay.ResolvePathTargets("self");

            // Assert
            Assert.AreEqual(1, targets.Count);
            Assert.AreEqual(relay, targets[0]);
        }

        #endregion

        #region Wildcard Resolution Tests

        [UnityTest]
        public IEnumerator Resolve_ParentWildcardChild_FindsAllSiblingsChildren()
        {
            // Arrange
            //   Grandparent
            //   ├── Parent
            //   │   └── Child (sender)
            //   ├── Aunt
            //   │   ├── Cousin1
            //   │   └── Cousin2
            //   └── Uncle
            //       └── Cousin3

            var grandparent = CreateNodeWithResponder("Grandparent");
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var aunt = CreateNodeWithResponder("Aunt", grandparent.transform);
            var uncle = CreateNodeWithResponder("Uncle", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);
            var cousin1 = CreateNodeWithResponder("Cousin1", aunt.transform);
            var cousin2 = CreateNodeWithResponder("Cousin2", aunt.transform);
            var cousin3 = CreateNodeWithResponder("Cousin3", uncle.transform);

            yield return null;
            yield return null;

            // Act - "parent/*/child" means parent → all parent's children → their children
            var childRelay = child.GetComponent<MmRelayNode>();
            var targets = childRelay.ResolvePathTargets("parent/*/child");

            // Assert - Should find all children of all grandparent's children
            // This includes cousins from all parent's siblings
            Assert.GreaterOrEqual(targets.Count, 3, "Should find at least 3 cousins");
        }

        #endregion

        #region Integration Tests (Message Delivery)

        [UnityTest]
        public IEnumerator InvokeWithPath_Parent_DeliversToParent()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child = CreateNodeWithResponder("Child", parent.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            childRelay.MmInvokeWithPath("parent", MmMethod.Initialize);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(1, GetMessageCount(parent), "Parent should receive message");
            Assert.AreEqual(0, GetMessageCount(child), "Child should not receive message");
        }

        [UnityTest]
        public IEnumerator InvokeWithPath_ParentSiblingChild_DeliversToCousins()
        {
            // Arrange
            var grandparent = CreateNodeWithResponder("Grandparent");
            var parent = CreateNodeWithResponder("Parent", grandparent.transform);
            var aunt = CreateNodeWithResponder("Aunt", grandparent.transform);
            var child = CreateNodeWithResponder("Child", parent.transform);
            var cousin1 = CreateNodeWithResponder("Cousin1", aunt.transform);
            var cousin2 = CreateNodeWithResponder("Cousin2", aunt.transform);

            yield return null;
            yield return null;

            // Act
            var childRelay = child.GetComponent<MmRelayNode>();
            childRelay.MmInvokeWithPath("parent/sibling/child", MmMethod.Initialize);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(1, GetMessageCount(cousin1), "Cousin1 should receive message");
            Assert.AreEqual(1, GetMessageCount(cousin2), "Cousin2 should receive message");
            Assert.AreEqual(0, GetMessageCount(child), "Child (sender) should not receive");
        }

        [UnityTest]
        public IEnumerator InvokeWithPath_WithParameter_DeliversCorrectly()
        {
            // Arrange
            var parent = CreateNodeWithResponder("Parent");
            var child = CreateNodeWithResponder("Child", parent.transform);

            yield return null;
            yield return null;

            // Act - Send with string parameter
            var childRelay = child.GetComponent<MmRelayNode>();
            childRelay.MmInvokeWithPath("parent", MmMethod.MessageString, "TestMessage");

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(1, GetMessageCount(parent), "Parent should receive message");
        }

        [UnityTest]
        public IEnumerator InvokeWithPath_Descendant_ReachesAllDescendants()
        {
            // Arrange
            var root = CreateNodeWithResponder("Root");
            var child = CreateNodeWithResponder("Child", root.transform);
            var grandchild = CreateNodeWithResponder("Grandchild", child.transform);

            yield return null;
            yield return null;

            // Act
            var rootRelay = root.GetComponent<MmRelayNode>();
            rootRelay.MmInvokeWithPath("descendant", MmMethod.Initialize);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(1, GetMessageCount(child), "Child should receive");
            Assert.AreEqual(1, GetMessageCount(grandchild), "Grandchild should receive");
            Assert.AreEqual(0, GetMessageCount(root), "Root should not receive");
        }

        #endregion

        #region Error Handling Tests

        [UnityTest]
        public IEnumerator InvokeWithPath_InvalidPath_ThrowsException()
        {
            // Arrange
            var node = CreateNodeWithResponder("Node");

            yield return null;
            yield return null;

            // Act & Assert
            var relay = node.GetComponent<MmRelayNode>();
            Assert.Throws<MmInvalidPathException>(() => relay.MmInvokeWithPath("invalid/path", MmMethod.Initialize));
        }

        [UnityTest]
        public IEnumerator InvokeWithPath_EmptyPath_ThrowsException()
        {
            // Arrange
            var node = CreateNodeWithResponder("Node");

            yield return null;
            yield return null;

            // Act & Assert
            var relay = node.GetComponent<MmRelayNode>();
            Assert.Throws<MmInvalidPathException>(() => relay.MmInvokeWithPath("", MmMethod.Initialize));
        }

        #endregion

        #region Helper Methods

        private GameObject CreateNodeWithResponder(string name, Transform parent = null)
        {
            var obj = new GameObject(name);
            var relay = obj.AddComponent<MmRelayNode>();
            obj.AddComponent<AdvancedRoutingTests.MessageCounterResponder>();

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

            // Explicitly refresh responders
            relay.MmRefreshResponders();

            testObjects.Add(obj);
            return obj;
        }

        private int GetMessageCount(GameObject obj)
        {
            var counter = obj.GetComponent<AdvancedRoutingTests.MessageCounterResponder>();
            return counter != null ? counter.MessageCount : 0;
        }

        #endregion
    }
}
