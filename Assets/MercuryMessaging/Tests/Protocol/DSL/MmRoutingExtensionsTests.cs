// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// Tests for DSL Phase 2.5: Runtime Registration Extensions

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Unit tests for MmRoutingExtensions.
    /// Tests runtime hierarchy registration and unregistration.
    /// </summary>
    [TestFixture]
    public class MmRoutingExtensionsTests
    {
        private GameObject _parentGo;
        private GameObject _childGo1;
        private GameObject _childGo2;
        private MmRelayNode _parentRelay;
        private MmRelayNode _childRelay1;
        private MmRelayNode _childRelay2;

        [SetUp]
        public void SetUp()
        {
            _parentGo = new GameObject("Parent");
            _childGo1 = new GameObject("Child1");
            _childGo2 = new GameObject("Child2");

            _parentRelay = _parentGo.AddComponent<MmRelayNode>();
            _childRelay1 = _childGo1.AddComponent<MmRelayNode>();
            _childRelay2 = _childGo2.AddComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_parentGo != null) Object.DestroyImmediate(_parentGo);
            if (_childGo1 != null) Object.DestroyImmediate(_childGo1);
            if (_childGo2 != null) Object.DestroyImmediate(_childGo2);
        }

        #region RegisterWith Tests

        [UnityTest]
        public IEnumerator RegisterWith_AddsChildToParentRoutingTable()
        {
            yield return null;

            // Act
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Assert - child should be in parent's routing table
            bool found = false;
            foreach (var item in _parentRelay.RoutingTable)
            {
                if (item.Responder == _childRelay1 && item.Level == MmLevelFilter.Child)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Child should be in parent's routing table");
        }

        [UnityTest]
        public IEnumerator RegisterWith_AddsParentToChildRoutingTable()
        {
            yield return null;

            // Act
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Assert - parent should be in child's routing table
            bool found = false;
            foreach (var item in _childRelay1.RoutingTable)
            {
                if (item.Responder == _parentRelay && item.Level == MmLevelFilter.Parent)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Parent should be in child's routing table");
        }

        [UnityTest]
        public IEnumerator RegisterWith_ReturnsChildForChaining()
        {
            yield return null;

            // Act
            var result = _childRelay1.RegisterWith(_parentRelay);

            // Assert
            Assert.AreSame(_childRelay1, result);
        }

        [Test]
        public void RegisterWith_NullChild_ReturnsNull()
        {
            MmRelayNode nullRelay = null;
            var result = nullRelay.RegisterWith(_parentRelay);
            Assert.IsNull(result);
        }

        [Test]
        public void RegisterWith_NullParent_ReturnsChild()
        {
            var result = _childRelay1.RegisterWith(null);
            Assert.AreSame(_childRelay1, result);
        }

        #endregion

        #region RegisterChildren Tests

        [UnityTest]
        public IEnumerator RegisterChildren_AddsMultipleChildren()
        {
            yield return null;

            // Act
            _parentRelay.RegisterChildren(_childRelay1, _childRelay2);
            yield return null;

            // Assert - both children should be in parent's routing table
            int childCount = 0;
            foreach (var item in _parentRelay.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Child &&
                    (item.Responder == _childRelay1 || item.Responder == _childRelay2))
                {
                    childCount++;
                }
            }
            Assert.AreEqual(2, childCount, "Both children should be registered");
        }

        [UnityTest]
        public IEnumerator RegisterChildren_ReturnsParentForChaining()
        {
            yield return null;

            // Act
            var result = _parentRelay.RegisterChildren(_childRelay1, _childRelay2);

            // Assert
            Assert.AreSame(_parentRelay, result);
        }

        #endregion

        #region UnregisterFrom Tests

        [UnityTest]
        public IEnumerator UnregisterFrom_RemovesChildFromParent()
        {
            yield return null;

            // Arrange
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Act
            _childRelay1.UnregisterFrom(_parentRelay);
            yield return null;

            // Assert - child should not be in parent's routing table
            bool found = false;
            foreach (var item in _parentRelay.RoutingTable)
            {
                if (item.Responder == _childRelay1)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsFalse(found, "Child should be removed from parent's routing table");
        }

        [UnityTest]
        public IEnumerator UnregisterFrom_ReturnsChildForChaining()
        {
            yield return null;

            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Act
            var result = _childRelay1.UnregisterFrom(_parentRelay);

            // Assert
            Assert.AreSame(_childRelay1, result);
        }

        #endregion

        #region UnregisterChildren Tests

        [UnityTest]
        public IEnumerator UnregisterChildren_RemovesMultipleChildren()
        {
            yield return null;

            // Arrange
            _parentRelay.RegisterChildren(_childRelay1, _childRelay2);
            yield return null;

            // Act
            _parentRelay.UnregisterChildren(_childRelay1, _childRelay2);
            yield return null;

            // Assert - neither child should be in parent's routing table
            int childCount = 0;
            foreach (var item in _parentRelay.RoutingTable)
            {
                if (item.Responder == _childRelay1 || item.Responder == _childRelay2)
                {
                    childCount++;
                }
            }
            Assert.AreEqual(0, childCount, "Both children should be unregistered");
        }

        #endregion

        #region Hierarchy Query Tests

        [UnityTest]
        public IEnumerator HasParents_ReturnsFalseWhenNoParents()
        {
            yield return null;

            // Assert
            Assert.IsFalse(_childRelay1.HasParents());
        }

        [UnityTest]
        public IEnumerator HasParents_ReturnsTrueWhenRegistered()
        {
            yield return null;

            // Arrange
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Assert
            Assert.IsTrue(_childRelay1.HasParents());
        }

        [UnityTest]
        public IEnumerator GetFirstParent_ReturnsParentWhenRegistered()
        {
            yield return null;

            // Arrange
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Assert
            Assert.AreSame(_parentRelay, _childRelay1.GetFirstParent());
        }

        [UnityTest]
        public IEnumerator GetFirstParent_ReturnsNullWhenNoParent()
        {
            yield return null;

            // Assert
            Assert.IsNull(_childRelay1.GetFirstParent());
        }

        [UnityTest]
        public IEnumerator IsRoot_ReturnsTrueWhenNoParents()
        {
            yield return null;

            // Assert
            Assert.IsTrue(_parentRelay.IsRoot());
        }

        [UnityTest]
        public IEnumerator IsRoot_ReturnsFalseWhenHasParents()
        {
            yield return null;

            // Arrange
            _childRelay1.RegisterWith(_parentRelay);
            yield return null;

            // Assert
            Assert.IsFalse(_childRelay1.IsRoot());
        }

        #endregion
    }
}
