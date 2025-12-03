using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for Phase 2 DSL features: spatial filtering, type filtering, and custom predicates.
    /// </summary>
    [TestFixture]
    public class FluentApiPhase2Tests
    {
        private GameObject _rootObject;
        private MmRelayNode _rootRelay;
        private List<GameObject> _testObjects;
        private List<TestResponder> _responders;

        [SetUp]
        public void Setup()
        {
            _testObjects = new List<GameObject>();
            _responders = new List<TestResponder>();

            // Create root object
            _rootObject = CreateTestObject("Root", Vector3.zero);
            _rootRelay = _rootObject.GetComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _testObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _testObjects.Clear();
            _responders.Clear();

            if (_rootObject != null)
                Object.DestroyImmediate(_rootObject);
        }

        private GameObject CreateTestObject(string name, Vector3 position)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            var relay = obj.AddComponent<MmRelayNode>();
            var responder = obj.AddComponent<TestResponder>();
            relay.MmRefreshResponders();

            _testObjects.Add(obj);
            _responders.Add(responder);
            return obj;
        }

        private void SetupHierarchy(GameObject parent, GameObject child)
        {
            child.transform.SetParent(parent.transform);
            var parentRelay = parent.GetComponent<MmRelayNode>();
            var childRelay = child.GetComponent<MmRelayNode>();
            parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.RefreshParents();
        }

        #region Spatial Extension Tests (Task 2.4)

        [Test]
        public void Within_FiltersObjectsByDistance()
        {
            // Arrange - Create objects at different distances
            var near = CreateTestObject("Near", new Vector3(3, 0, 0));
            var far = CreateTestObject("Far", new Vector3(15, 0, 0));
            var medium = CreateTestObject("Medium", new Vector3(8, 0, 0));

            SetupHierarchy(_rootObject, near);
            SetupHierarchy(_rootObject, far);
            SetupHierarchy(_rootObject, medium);

            // Act - Send message to objects within 10 units
            _rootRelay.Send("Test").ToChildren().Within(10f).Execute();

            // Assert
            var nearResponder = near.GetComponent<TestResponder>();
            var farResponder = far.GetComponent<TestResponder>();
            var mediumResponder = medium.GetComponent<TestResponder>();

            Assert.AreEqual(1, nearResponder.StringMessagesReceived.Count, "Near object should receive");
            Assert.AreEqual(0, farResponder.StringMessagesReceived.Count, "Far object should NOT receive");
            Assert.AreEqual(1, mediumResponder.StringMessagesReceived.Count, "Medium object should receive");
        }

        [Test]
        public void InDirection_FiltersObjectsByAngle()
        {
            // Arrange - Create objects in different directions
            var forward = CreateTestObject("Forward", new Vector3(0, 0, 10));
            var behind = CreateTestObject("Behind", new Vector3(0, 0, -10));
            var side = CreateTestObject("Side", new Vector3(10, 0, 0));

            SetupHierarchy(_rootObject, forward);
            SetupHierarchy(_rootObject, behind);
            SetupHierarchy(_rootObject, side);

            // Act - Send message to objects in forward direction (within 45 degrees)
            _rootRelay.Send("Test").ToChildren().InDirection(Vector3.forward, 45f).Execute();

            // Assert
            var forwardResponder = forward.GetComponent<TestResponder>();
            var behindResponder = behind.GetComponent<TestResponder>();
            var sideResponder = side.GetComponent<TestResponder>();

            Assert.AreEqual(1, forwardResponder.StringMessagesReceived.Count, "Forward object should receive");
            Assert.AreEqual(0, behindResponder.StringMessagesReceived.Count, "Behind object should NOT receive");
            Assert.AreEqual(0, sideResponder.StringMessagesReceived.Count, "Side object should NOT receive");
        }

        [Test]
        public void InBounds_FiltersObjectsByBoundingBox()
        {
            // Arrange - Create objects inside and outside bounds
            var inside = CreateTestObject("Inside", new Vector3(2, 2, 2));
            var outside = CreateTestObject("Outside", new Vector3(20, 20, 20));
            var edge = CreateTestObject("Edge", new Vector3(5, 5, 5));

            SetupHierarchy(_rootObject, inside);
            SetupHierarchy(_rootObject, outside);
            SetupHierarchy(_rootObject, edge);

            var bounds = new Bounds(Vector3.zero, new Vector3(12, 12, 12)); // 6 units in each direction

            // Act
            _rootRelay.Send("Test").ToChildren().InBounds(bounds).Execute();

            // Assert
            var insideResponder = inside.GetComponent<TestResponder>();
            var outsideResponder = outside.GetComponent<TestResponder>();
            var edgeResponder = edge.GetComponent<TestResponder>();

            Assert.AreEqual(1, insideResponder.StringMessagesReceived.Count, "Inside object should receive");
            Assert.AreEqual(0, outsideResponder.StringMessagesReceived.Count, "Outside object should NOT receive");
            Assert.AreEqual(1, edgeResponder.StringMessagesReceived.Count, "Edge object (inside) should receive");
        }

        [Test]
        public void InCone_CombinesDirectionAndDistance()
        {
            // Arrange
            var inCone = CreateTestObject("InCone", new Vector3(0, 0, 5));
            var outOfRange = CreateTestObject("OutOfRange", new Vector3(0, 0, 25));
            var wrongDirection = CreateTestObject("WrongDirection", new Vector3(5, 0, 0));

            SetupHierarchy(_rootObject, inCone);
            SetupHierarchy(_rootObject, outOfRange);
            SetupHierarchy(_rootObject, wrongDirection);

            // Act - Send message in cone: forward direction, 45 degrees, 10 units
            _rootRelay.Send("Test").ToChildren().InCone(Vector3.forward, 45f, 10f).Execute();

            // Assert
            var inConeResponder = inCone.GetComponent<TestResponder>();
            var outOfRangeResponder = outOfRange.GetComponent<TestResponder>();
            var wrongDirectionResponder = wrongDirection.GetComponent<TestResponder>();

            Assert.AreEqual(1, inConeResponder.StringMessagesReceived.Count, "InCone object should receive");
            Assert.AreEqual(0, outOfRangeResponder.StringMessagesReceived.Count, "OutOfRange should NOT receive");
            Assert.AreEqual(0, wrongDirectionResponder.StringMessagesReceived.Count, "WrongDirection should NOT receive");
        }

        #endregion

        #region Type Filter Tests (Task 2.5)

        [Test]
        public void OfType_FiltersObjectsByComponent()
        {
            // Arrange
            var withRigidbody = CreateTestObject("WithRigidbody", Vector3.one);
            withRigidbody.AddComponent<Rigidbody>();

            var withoutRigidbody = CreateTestObject("WithoutRigidbody", Vector3.one * 2);

            SetupHierarchy(_rootObject, withRigidbody);
            SetupHierarchy(_rootObject, withoutRigidbody);

            // Act
            _rootRelay.Send("Test").ToChildren().OfType<Rigidbody>().Execute();

            // Assert
            var withResponder = withRigidbody.GetComponent<TestResponder>();
            var withoutResponder = withoutRigidbody.GetComponent<TestResponder>();

            Assert.AreEqual(1, withResponder.StringMessagesReceived.Count, "Object with Rigidbody should receive");
            Assert.AreEqual(0, withoutResponder.StringMessagesReceived.Count, "Object without Rigidbody should NOT receive");
        }

        [Test]
        public void WithComponent_WorksAliasForOfType()
        {
            // Arrange
            var withCollider = CreateTestObject("WithCollider", Vector3.one);
            withCollider.AddComponent<BoxCollider>();

            var withoutCollider = CreateTestObject("WithoutCollider", Vector3.one * 2);

            SetupHierarchy(_rootObject, withCollider);
            SetupHierarchy(_rootObject, withoutCollider);

            // Act
            _rootRelay.Send("Test").ToChildren().WithComponent<BoxCollider>().Execute();

            // Assert
            var withResponder = withCollider.GetComponent<TestResponder>();
            var withoutResponder = withoutCollider.GetComponent<TestResponder>();

            Assert.AreEqual(1, withResponder.StringMessagesReceived.Count, "Object with BoxCollider should receive");
            Assert.AreEqual(0, withoutResponder.StringMessagesReceived.Count, "Object without BoxCollider should NOT receive");
        }

        [Test]
        public void OfType_CanFilterByMmResponder()
        {
            // Arrange - All test objects have MmRelayNode
            var child1 = CreateTestObject("Child1", Vector3.one);
            var child2 = CreateTestObject("Child2", Vector3.one * 2);

            SetupHierarchy(_rootObject, child1);
            SetupHierarchy(_rootObject, child2);

            // Act - Filter by MmRelayNode (all should match)
            _rootRelay.Send("Test").ToChildren().OfType<MmRelayNode>().Execute();

            // Assert
            var responder1 = child1.GetComponent<TestResponder>();
            var responder2 = child2.GetComponent<TestResponder>();

            Assert.AreEqual(1, responder1.StringMessagesReceived.Count, "Child1 should receive");
            Assert.AreEqual(1, responder2.StringMessagesReceived.Count, "Child2 should receive");
        }

        #endregion

        #region Custom Predicate Tests (Task 2.6)

        [Test]
        public void Where_GameObject_FiltersWithCustomLogic()
        {
            // Arrange
            var child1 = CreateTestObject("Enemy_1", Vector3.one);
            var child2 = CreateTestObject("Player", Vector3.one * 2);
            var child3 = CreateTestObject("Enemy_2", Vector3.one * 3);

            SetupHierarchy(_rootObject, child1);
            SetupHierarchy(_rootObject, child2);
            SetupHierarchy(_rootObject, child3);

            // Act - Filter objects whose name contains "Enemy"
            _rootRelay.Send("Test")
                .ToChildren()
                .Where(go => go.name.Contains("Enemy"))
                .Execute();

            // Assert
            var responder1 = child1.GetComponent<TestResponder>();
            var responder2 = child2.GetComponent<TestResponder>();
            var responder3 = child3.GetComponent<TestResponder>();

            Assert.AreEqual(1, responder1.StringMessagesReceived.Count, "Enemy_1 should receive");
            Assert.AreEqual(0, responder2.StringMessagesReceived.Count, "Player should NOT receive");
            Assert.AreEqual(1, responder3.StringMessagesReceived.Count, "Enemy_2 should receive");
        }

        [Test]
        public void OnLayer_FiltersObjectsByLayer()
        {
            // Arrange
            var child1 = CreateTestObject("Child1", Vector3.one);
            child1.layer = 5; // UI layer

            var child2 = CreateTestObject("Child2", Vector3.one * 2);
            child2.layer = 0; // Default layer

            SetupHierarchy(_rootObject, child1);
            SetupHierarchy(_rootObject, child2);

            // Act - Filter by layer 5
            _rootRelay.Send("Test").ToChildren().OnLayer(5).Execute();

            // Assert
            var responder1 = child1.GetComponent<TestResponder>();
            var responder2 = child2.GetComponent<TestResponder>();

            Assert.AreEqual(1, responder1.StringMessagesReceived.Count, "Layer 5 object should receive");
            Assert.AreEqual(0, responder2.StringMessagesReceived.Count, "Layer 0 object should NOT receive");
        }

        [Test]
        public void Named_FiltersObjectsByNamePattern()
        {
            // Arrange
            var enemy1 = CreateTestObject("Enemy_Soldier", Vector3.one);
            var enemy2 = CreateTestObject("Enemy_Tank", Vector3.one * 2);
            var friendly = CreateTestObject("Friendly_Soldier", Vector3.one * 3);

            SetupHierarchy(_rootObject, enemy1);
            SetupHierarchy(_rootObject, enemy2);
            SetupHierarchy(_rootObject, friendly);

            // Act - Filter objects named "Enemy"
            _rootRelay.Send("Test").ToChildren().Named("Enemy").Execute();

            // Assert
            Assert.AreEqual(1, enemy1.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(1, enemy2.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, friendly.GetComponent<TestResponder>().StringMessagesReceived.Count);
        }

        [Test]
        public void NamedExactly_FiltersObjectsByExactName()
        {
            // Arrange
            var target = CreateTestObject("Target", Vector3.one);
            var notTarget = CreateTestObject("TargetExtra", Vector3.one * 2);

            SetupHierarchy(_rootObject, target);
            SetupHierarchy(_rootObject, notTarget);

            // Act
            _rootRelay.Send("Test").ToChildren().NamedExactly("Target").Execute();

            // Assert
            Assert.AreEqual(1, target.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, notTarget.GetComponent<TestResponder>().StringMessagesReceived.Count);
        }

        #endregion

        #region Combined Filter Tests

        [Test]
        public void CombinedFilters_SpatialAndType()
        {
            // Arrange - Create objects with different positions and components
            var nearWithRb = CreateTestObject("NearWithRb", new Vector3(3, 0, 0));
            nearWithRb.AddComponent<Rigidbody>();

            var nearWithoutRb = CreateTestObject("NearWithoutRb", new Vector3(4, 0, 0));

            var farWithRb = CreateTestObject("FarWithRb", new Vector3(20, 0, 0));
            farWithRb.AddComponent<Rigidbody>();

            SetupHierarchy(_rootObject, nearWithRb);
            SetupHierarchy(_rootObject, nearWithoutRb);
            SetupHierarchy(_rootObject, farWithRb);

            // Act - Filter: within 10 units AND has Rigidbody
            _rootRelay.Send("Test")
                .ToChildren()
                .Within(10f)
                .OfType<Rigidbody>()
                .Execute();

            // Assert - Only nearWithRb should receive
            Assert.AreEqual(1, nearWithRb.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, nearWithoutRb.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, farWithRb.GetComponent<TestResponder>().StringMessagesReceived.Count);
        }

        [Test]
        public void CombinedFilters_SpatialAndCustom()
        {
            // Arrange
            var nearEnemy = CreateTestObject("Enemy_Near", new Vector3(3, 0, 0));
            var nearFriendly = CreateTestObject("Friendly_Near", new Vector3(4, 0, 0));
            var farEnemy = CreateTestObject("Enemy_Far", new Vector3(20, 0, 0));

            SetupHierarchy(_rootObject, nearEnemy);
            SetupHierarchy(_rootObject, nearFriendly);
            SetupHierarchy(_rootObject, farEnemy);

            // Act - Filter: within 10 units AND name contains "Enemy"
            _rootRelay.Send("Test")
                .ToChildren()
                .Within(10f)
                .Named("Enemy")
                .Execute();

            // Assert
            Assert.AreEqual(1, nearEnemy.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, nearFriendly.GetComponent<TestResponder>().StringMessagesReceived.Count);
            Assert.AreEqual(0, farEnemy.GetComponent<TestResponder>().StringMessagesReceived.Count);
        }

        [Test]
        public void CombinedFilters_WithStandardFilters()
        {
            // Arrange
            var activeNear = CreateTestObject("ActiveNear", new Vector3(3, 0, 0));
            var inactiveNear = CreateTestObject("InactiveNear", new Vector3(4, 0, 0));
            inactiveNear.SetActive(false);

            SetupHierarchy(_rootObject, activeNear);
            SetupHierarchy(_rootObject, inactiveNear);

            // Act - Filter: Active AND within 10 units
            _rootRelay.Send("Test")
                .ToChildren()
                .Active()
                .Within(10f)
                .Execute();

            // Assert
            Assert.AreEqual(1, activeNear.GetComponent<TestResponder>().StringMessagesReceived.Count);
            // Note: Inactive objects won't have responder invoked anyway due to Active() filter
        }

        #endregion

        #region MmPredicate Unit Tests

        [Test]
        public void MmPredicate_Within_EvaluatesCorrectly()
        {
            // Arrange
            var source = new GameObject("Source");
            source.transform.position = Vector3.zero;
            var sourceRelay = source.AddComponent<MmRelayNode>();

            var nearTarget = new GameObject("Near");
            nearTarget.transform.position = new Vector3(5, 0, 0);

            var farTarget = new GameObject("Far");
            farTarget.transform.position = new Vector3(15, 0, 0);

            var predicate = MmPredicate.CreateWithin(10f);

            // Act & Assert
            Assert.IsTrue(predicate.Evaluate(sourceRelay, nearTarget), "Near target should pass");
            Assert.IsFalse(predicate.Evaluate(sourceRelay, farTarget), "Far target should fail");

            // Cleanup
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(nearTarget);
            Object.DestroyImmediate(farTarget);
        }

        [Test]
        public void MmPredicate_InBounds_EvaluatesCorrectly()
        {
            // Arrange
            var bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));

            var insideTarget = new GameObject("Inside");
            insideTarget.transform.position = new Vector3(2, 2, 2);

            var outsideTarget = new GameObject("Outside");
            outsideTarget.transform.position = new Vector3(20, 20, 20);

            var predicate = MmPredicate.CreateInBounds(bounds);

            // Act & Assert
            Assert.IsTrue(predicate.Evaluate(null, insideTarget), "Inside target should pass");
            Assert.IsFalse(predicate.Evaluate(null, outsideTarget), "Outside target should fail");

            // Cleanup
            Object.DestroyImmediate(insideTarget);
            Object.DestroyImmediate(outsideTarget);
        }

        [Test]
        public void MmPredicateList_EvaluatesAllPredicates()
        {
            // Arrange
            var source = new GameObject("Source");
            source.transform.position = Vector3.zero;
            var sourceRelay = source.AddComponent<MmRelayNode>();

            var target = new GameObject("Target");
            target.transform.position = new Vector3(5, 0, 0);
            target.AddComponent<Rigidbody>();

            var predicateList = new MmPredicateList();
            predicateList.Add(MmPredicate.CreateWithin(10f)); // Should pass
            predicateList.Add(MmPredicate.CreateWithComponent(typeof(Rigidbody))); // Should pass

            // Act & Assert
            Assert.IsTrue(predicateList.EvaluateAll(sourceRelay, target), "All predicates should pass");

            // Add failing predicate
            predicateList.Add(MmPredicate.CreateWithin(2f)); // Should fail (5 > 2)
            Assert.IsFalse(predicateList.EvaluateAll(sourceRelay, target), "Should fail with failing predicate");

            // Cleanup
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(target);
        }

        #endregion

        /// <summary>
        /// Test responder that tracks received messages.
        /// </summary>
        private class TestResponder : MmBaseResponder
        {
            public List<string> StringMessagesReceived = new();
            public List<int> IntMessagesReceived = new();

            protected override void ReceivedMessage(MmMessageString message)
            {
                StringMessagesReceived.Add(message.value);
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
                IntMessagesReceived.Add(message.value);
            }
        }
    }
}
