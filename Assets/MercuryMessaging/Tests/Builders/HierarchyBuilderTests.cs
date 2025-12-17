// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// HierarchyBuilderTests.cs - Tests for Hierarchy Building DSL
// Part of DSL Overhaul Phase 7


using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for HierarchyBuilder and MmHierarchyExtensions.
    /// Tests cover fluent hierarchy creation, navigation, and configuration.
    /// </summary>
    [TestFixture]
    public class HierarchyBuilderTests
    {
        private MmRelayNode _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root.gameObject);
        }

        #region Creation Tests

        [Test]
        public void Create_CreatesRootNode()
        {
            _root = MmHierarchy.Create("TestRoot").Build();

            Assert.IsNotNull(_root);
            Assert.AreEqual("TestRoot", _root.gameObject.name);
        }

        [Test]
        public void CreateNode_CreatesSimpleNode()
        {
            _root = MmHierarchy.CreateNode("SimpleNode");

            Assert.IsNotNull(_root);
            Assert.AreEqual("SimpleNode", _root.gameObject.name);
        }

        [Test]
        public void CreateSwitchNode_CreatesSwitchNode()
        {
            var switchNode = MmHierarchy.CreateSwitchNode("SwitchNode");
            _root = switchNode;

            Assert.IsNotNull(switchNode);
            Assert.IsInstanceOf<MmRelaySwitchNode>(switchNode);
        }

        #endregion

        #region Child Addition Tests

        [Test]
        public void AddChild_CreatesChildNode()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild("Child")
                .Build();

            Assert.AreEqual(1, _root.transform.childCount);
            Assert.AreEqual("Child", _root.transform.GetChild(0).name);
        }

        [Test]
        public void AddChild_Nested_CreatesNestedChildren()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild("Child1")
                    .AddChild("Grandchild1")
                    .Parent()
                .Parent()
                .AddChild("Child2")
                .Build();

            Assert.AreEqual(2, _root.transform.childCount);

            var child1 = _root.transform.GetChild(0);
            Assert.AreEqual("Child1", child1.name);
            Assert.AreEqual(1, child1.childCount);
            Assert.AreEqual("Grandchild1", child1.GetChild(0).name);
        }

        [Test]
        public void AddChildWithResponder_AddsResponderComponent()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild<TestResponder>("Child")
                .Build();

            var child = _root.transform.GetChild(0).gameObject;
            Assert.IsNotNull(child.GetComponent<TestResponder>());
        }

        #endregion

        #region Navigation Tests

        [Test]
        public void Parent_NavigatesBack()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild("Child1")
                    .AddChild("Grandchild")
                    .Parent()
                .AddChild("Child1_Sibling") // Should be sibling of Grandchild
                .Build();

            var child1 = _root.transform.GetChild(0);
            Assert.AreEqual(2, child1.childCount);
        }

        [Test]
        public void Root_NavigatesToRoot()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild("Child1")
                    .AddChild("Grandchild")
                    .Root()
                .AddChild("Child2") // Should be child of Root
                .Build();

            Assert.AreEqual(2, _root.transform.childCount);
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void WithTag_SetsTag()
        {
            _root = MmHierarchy.Create("Root")
                .WithTag(MmTag.Tag0)
                .Build();

            Assert.AreEqual(MmTag.Tag0, _root.Tag);
        }

        [Test]
        public void OnLayer_SetsLayer()
        {
            _root = MmHierarchy.Create("Root")
                .OnLayer(5)
                .Build();

            Assert.AreEqual(5, _root.layer);
        }

        [Test]
        public void AtPosition_SetsPosition()
        {
            _root = MmHierarchy.Create("Root")
                .AtPosition(new Vector3(1, 2, 3))
                .Build();

            Assert.AreEqual(new Vector3(1, 2, 3), _root.transform.localPosition);
        }

        [Test]
        public void AsSwitchNode_CreatesSwitchNode()
        {
            _root = MmHierarchy.Create("Root")
                .AsSwitchNode()
                .Build();

            Assert.IsInstanceOf<MmRelaySwitchNode>(_root);
        }

        [Test]
        public void Inactive_StartsInactive()
        {
            _root = MmHierarchy.Create("Root")
                .AddChild("InactiveChild")
                .Inactive()
                .Build();

            var child = _root.transform.GetChild(0).gameObject;
            Assert.IsFalse(child.activeSelf);
        }

        [Test]
        public void Configure_RunsConfigureAction()
        {
            bool configured = false;

            _root = MmHierarchy.Create("Root")
                .Configure(relay => configured = true)
                .Build();

            Assert.IsTrue(configured);
        }

        #endregion

        #region Extension Method Tests

        [Test]
        public void AddChildNode_AddsChild()
        {
            _root = MmHierarchy.CreateNode("Root");

            var child = _root.AddChildNode("Child");

            Assert.IsNotNull(child);
            Assert.AreEqual("Child", child.gameObject.name);
            Assert.AreEqual(_root.transform, child.transform.parent);
        }

        [Test]
        public void AddChildNode_WithResponder_AddsResponder()
        {
            _root = MmHierarchy.CreateNode("Root");

            var child = _root.AddChildNode<TestResponder>("Child");

            Assert.IsNotNull(child.GetComponent<TestResponder>());
        }

        [Test]
        public void AddSwitchChild_AddsSwitchNode()
        {
            _root = MmHierarchy.CreateNode("Root");

            var switchChild = _root.AddSwitchChild("SwitchChild");

            Assert.IsInstanceOf<MmRelaySwitchNode>(switchChild);
            Assert.AreEqual(_root.transform, switchChild.transform.parent);
        }

        [Test]
        public void ReparentTo_MovesNode()
        {
            _root = MmHierarchy.CreateNode("Root");
            var child1 = _root.AddChildNode("Child1");
            var child2 = _root.AddChildNode("Child2");

            child1.MmSetParent(child2);

            Assert.AreEqual(child2.transform, child1.transform.parent);
        }

        #endregion

        #region AttachTo Tests

        [Test]
        public void AttachTo_AddsToExisting()
        {
            _root = MmHierarchy.CreateNode("ExistingRoot");

            MmHierarchy.AttachTo(_root)
                .AddChild("NewChild")
                .Build();

            Assert.AreEqual(1, _root.transform.childCount);
            Assert.AreEqual("NewChild", _root.transform.GetChild(0).name);
        }

        #endregion

        /// <summary>
        /// Test responder for hierarchy tests.
        /// </summary>
        private class TestResponder : MmBaseResponder { }
    }
}
