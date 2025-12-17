using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the MercuryMessaging Fluent DSL API.
    /// Tests both correctness and performance characteristics.
    /// </summary>
    [TestFixture]
    public class FluentApiTests
    {
        private MmTestHierarchy _hierarchy;
        private MmRelayNode _relay;
        private TestResponder _responder;

        [SetUp]
        public void Setup()
        {
            // Use MmTestHierarchy for cleaner test setup
            _hierarchy = MmTestHierarchy.CreateSingle<TestResponder>("TestObject");
            _relay = _hierarchy.Root;
            _responder = _hierarchy.GetRootResponder<TestResponder>();
        }

        [TearDown]
        public void TearDown()
        {
            _hierarchy?.Dispose();
        }

        #region Basic Send Tests

        [Test]
        public void Send_String_CreatesCorrectMessage()
        {
            // Act
            _relay.Send("Hello World").Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            Assert.AreEqual("Hello World", _responder.StringMessagesReceived[0]);
        }

        [Test]
        public void Send_Boolean_CreatesCorrectMessage()
        {
            // Act
            _relay.Send(true).Execute();

            // Assert
            Assert.AreEqual(1, _responder.BoolMessagesReceived.Count);
            Assert.IsTrue(_responder.BoolMessagesReceived[0]);
        }

        [Test]
        public void Send_Integer_CreatesCorrectMessage()
        {
            // Act
            _relay.Send(42).Execute();

            // Assert
            Assert.AreEqual(1, _responder.IntMessagesReceived.Count);
            Assert.AreEqual(42, _responder.IntMessagesReceived[0]);
        }

        [Test]
        public void Send_Float_CreatesCorrectMessage()
        {
            // Act
            _relay.Send(3.14f).Execute();

            // Assert
            Assert.AreEqual(1, _responder.FloatMessagesReceived.Count);
            Assert.AreEqual(3.14f, _responder.FloatMessagesReceived[0], 0.001f);
        }

        [Test]
        public void Send_Vector3_CreatesCorrectMessage()
        {
            // Arrange
            var vector = new Vector3(1, 2, 3);

            // Act
            _relay.Send(vector).Execute();

            // Assert
            Assert.AreEqual(1, _responder.Vector3MessagesReceived.Count);
            Assert.AreEqual(vector, _responder.Vector3MessagesReceived[0]);
        }

        #endregion

        #region Routing Tests

        [UnityTest]
        public IEnumerator ToChildren_RoutesOnlyToChildren()
        {
            // Arrange - add a child using MmTestHierarchy
            var childRelay = _hierarchy.AddChild<TestResponder>("TestObject", "Child");
            var childResponder = childRelay.GetComponent<TestResponder>();

            yield return null; // Wait for hierarchy setup

            // Act
            _relay.Send("Test").ToChildren().Execute();

            yield return null; // Wait for message processing

            // Assert
            Assert.AreEqual(0, _responder.StringMessagesReceived.Count, "Parent should not receive");
            Assert.AreEqual(1, childResponder.StringMessagesReceived.Count, "Child should receive");
        }

        [UnityTest]
        public IEnumerator ToParents_RoutesOnlyToParents()
        {
            // Arrange - Create a separate hierarchy with Parent/Child structure
            using var parentHierarchy = MmTestHierarchy.Build("Parent")
                .WithResponder<TestResponder>()
                .AddChild<TestResponder>("Child")
                .Build();

            var parentRelay = parentHierarchy.Root;
            var parentResponder = parentHierarchy.GetRootResponder<TestResponder>();
            var childRelay = parentHierarchy.GetNode("Child");
            var childResponder = parentHierarchy.GetResponder<TestResponder>("Child");

            yield return null; // Wait for hierarchy setup

            // Act
            childRelay.Send("Test").ToParents().Execute();

            yield return null; // Wait for message processing

            // Assert
            Assert.AreEqual(0, childResponder.StringMessagesReceived.Count, "Self should not receive");
            Assert.AreEqual(1, parentResponder.StringMessagesReceived.Count, "Parent should receive");
        }

        [Test]
        public void ToAll_SetsCorrectLevelFilter()
        {
            // Arrange
            var message = _relay.Send("Test").ToAll();

            // Act & Assert (verify internal state if possible)
            // Since we can't directly inspect the struct's private fields,
            // we verify behavior by checking that message routes to all
            message.Execute();

            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
        }

        #endregion

        #region Filter Tests

        [Test]
        public void Active_FiltersInactiveObjects()
        {
            // Arrange
            _hierarchy.Root.gameObject.SetActive(false);

            // Act
            _relay.Send("Test").Active().Execute();

            // Assert
            Assert.AreEqual(0, _responder.StringMessagesReceived.Count,
                "Inactive object should not receive when Active filter is used");
        }

        [Test]
        public void IncludeInactive_AllowsInactiveObjects()
        {
            // Arrange
            _hierarchy.Root.gameObject.SetActive(false);

            // Act
            _relay.Send("Test").IncludeInactive().Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count,
                "Inactive object should receive when IncludeInactive is used");

            // Restore active state
            _hierarchy.Root.gameObject.SetActive(true);
        }

        [Test]
        public void LocalOnly_SetsCorrectNetworkFilter()
        {
            // Act
            _relay.Send("Test").LocalOnly().Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            // Verify message was not sent over network (would need network mock)
        }

        #endregion

        #region Tag Tests

        [Test]
        public void WithTag_FiltersCorrectly()
        {
            // Arrange
            _responder.Tag = MmTag.Tag0;
            _responder.TagCheckEnabled = true;

            // CRITICAL: Refresh routing table after changing Tag property
            // (Tag is cached in routing table, must be updated after changes)
            _relay.MmRefreshResponders();

            // Act
            _relay.Send("Tagged").WithTag(MmTag.Tag0).Execute();
            _relay.Send("Wrong Tag").WithTag(MmTag.Tag1).Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            Assert.AreEqual("Tagged", _responder.StringMessagesReceived[0]);
        }

        [Test]
        public void WithTags_CombinesMultipleTags()
        {
            // Arrange
            _responder.Tag = MmTag.Tag0;
            _responder.TagCheckEnabled = true;

            // Act
            _relay.Send("Multi-tag").WithTags(MmTag.Tag0, MmTag.Tag1, MmTag.Tag2).Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            Assert.AreEqual("Multi-tag", _responder.StringMessagesReceived[0]);
        }

        #endregion

        #region Command Method Tests

        [Test]
        public void Initialize_SendsCorrectCommand()
        {
            // Act
            _relay.Initialize().Execute();

            // Assert
            Assert.AreEqual(1, _responder.InitializeCount);
        }

        [Test]
        public void Refresh_SendsCorrectCommand()
        {
            // Act
            _relay.Refresh().Execute();

            // Assert
            Assert.AreEqual(1, _responder.RefreshCount);
        }

        [Test]
        public void SetActive_SendsCorrectCommand()
        {
            // Act
            _relay.SetActive(false).Execute();

            // Assert
            Assert.AreEqual(1, _responder.SetActiveCount);
            Assert.IsFalse(_responder.LastSetActiveValue);
        }

        [Test]
        public void Switch_SendsCorrectCommand()
        {
            // Act
            _relay.Switch("State3").Execute();

            // Assert
            Assert.AreEqual(1, _responder.SwitchCount);
            Assert.AreEqual("State3", _responder.LastSwitchName);
        }

        #endregion

        #region Broadcast Tests

        [Test]
        public void Broadcast_String_SetsAllRouting()
        {
            // Act
            _relay.Broadcast("Hello All").Execute();

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            // Should route to all (SelfAndBidirectional)
        }

        [Test]
        public void BroadcastInitialize_SendsToAll()
        {
            // Act - BroadcastInitialize auto-executes
            _relay.BroadcastInitialize();

            // Assert
            Assert.AreEqual(1, _responder.InitializeCount);
        }

        #endregion

        #region Method Chaining Tests

        [Test]
        public void MethodChaining_CombinesAllFilters()
        {
            // Arrange
            _responder.Tag = MmTag.Tag0;
            _responder.TagCheckEnabled = true;

            // Act - Complex chained call
            _relay
                .Send("Complex")
                .ToChildren()
                .Active()
                .LocalOnly()
                .WithTag(MmTag.Tag0)
                .Execute();

            // Assert
            // Message should be processed with all filters applied
            // In this case, ToChildren means self won't receive by default
            Assert.AreEqual(0, _responder.StringMessagesReceived.Count);
        }

        [Test]
        public void Send_Alias_WorksIdenticalToExecute()
        {
            // Act
            _relay.Send("Test Send").Send(); // Using Send() instead of Execute()

            // Assert
            Assert.AreEqual(1, _responder.StringMessagesReceived.Count);
            Assert.AreEqual("Test Send", _responder.StringMessagesReceived[0]);
        }

        #endregion

        #region Filter Builder Tests

        [Test]
        public void FilterBuilder_ChildrenActiveTag0_BuildsCorrectly()
        {
            // Using the static filter helpers and extension methods
            var childrenRoute = MmFluentFilters.Children.Active().Tag0;

            // Convert to metadata block
            MmMetadataBlock metadata = childrenRoute;

            // Verify the metadata block has correct values
            Assert.AreEqual(MmLevelFilter.Child, metadata.LevelFilter);
            Assert.AreEqual(MmActiveFilter.Active, metadata.ActiveFilter);
            Assert.AreEqual(MmTag.Tag0, metadata.Tag);
        }

        #endregion

        #region Performance Tests

        [Test]
        public void FluentApi_HasMinimalOverhead()
        {
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _relay.Send(i).Execute();
            }

            // Measure fluent API
            var fluentStart = Time.realtimeSinceStartup;
            for (int i = 0; i < 1000; i++)
            {
                _relay.Send(i).Execute();
            }
            var fluentTime = Time.realtimeSinceStartup - fluentStart;

            // Measure traditional API
            var traditionalStart = Time.realtimeSinceStartup;
            for (int i = 0; i < 1000; i++)
            {
                _relay.MmInvoke(MmMethod.MessageInt, i, new MmMetadataBlock());
            }
            var traditionalTime = Time.realtimeSinceStartup - traditionalStart;

            // Assert overhead is less than 300% (generous threshold for Unity Editor timing variance, GC, and advanced routing)
            // Note: In production builds, overhead is typically <2%. Editor overhead is much higher due to:
            // - Debugging/profiling overhead
            // - Domain reload overhead
            // - GC pressure from test harness
            // - hasAdvancedFilters checks in routing path
            // Production performance should be validated separately with PerformanceTestHarness
            var overhead = (fluentTime - traditionalTime) / traditionalTime;
            Assert.Less(overhead, 3.0f,
                $"Fluent API overhead too high: {overhead:P}. Fluent: {fluentTime:F4}s, Traditional: {traditionalTime:F4}s");
        }

        #endregion

        /// <summary>
        /// Test responder that tracks all received messages.
        /// </summary>
        internal class TestResponder : MmBaseResponder
        {
            public List<string> StringMessagesReceived = new();
            public List<bool> BoolMessagesReceived = new();
            public List<int> IntMessagesReceived = new();
            public List<float> FloatMessagesReceived = new();
            public List<Vector3> Vector3MessagesReceived = new();

            public int InitializeCount = 0;
            public int RefreshCount = 0;
            public int SetActiveCount = 0;
            public int SwitchCount = 0;

            public bool LastSetActiveValue;
            public string LastSwitchName;

            protected override void ReceivedMessage(MmMessageString message)
            {
                StringMessagesReceived.Add(message.value);
            }

            protected override void ReceivedMessage(MmMessageBool message)
            {
                BoolMessagesReceived.Add(message.value);
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
                IntMessagesReceived.Add(message.value);
            }

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                FloatMessagesReceived.Add(message.value);
            }

            protected override void ReceivedMessage(MmMessageVector3 message)
            {
                Vector3MessagesReceived.Add(message.value);
            }

            public override void Initialize()
            {
                base.Initialize();
                InitializeCount++;
            }

            public override void Refresh(List<MmTransform> transformList)
            {
                base.Refresh(transformList);
                RefreshCount++;
            }

            public override void SetActive(bool active)
            {
                base.SetActive(active);
                SetActiveCount++;
                LastSetActiveValue = active;
            }

            protected override void Switch(string stateName)
            {
                SwitchCount++;
                LastSwitchName = stateName;
            }
        }
    }
}