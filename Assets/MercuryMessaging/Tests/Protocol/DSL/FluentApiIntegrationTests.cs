// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Integration tests for Language DSL - Complex real-world scenarios
// Phase 4 - Task 4.2: Integration Tests

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Integration tests validating the DSL works correctly in complex,
    /// real-world scenarios with deep hierarchies and multiple filters.
    /// </summary>
    [TestFixture]
    public class FluentApiIntegrationTests
    {
        private GameObject rootObject;
        private MmRelayNode rootRelay;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            rootObject = new GameObject("IntegrationRoot");
            rootRelay = rootObject.AddComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            if (rootObject != null)
                Object.DestroyImmediate(rootObject);

            MmRelayNodeExtensions.ClearPendingQueries();
        }

        #endregion

        #region Deep Hierarchy Tests

        [Test]
        public void Integration_DeepHierarchy_MessageReachesAllLevels()
        {
            // Create 5-level deep hierarchy
            var responders = new List<IntegrationResponder>();
            var currentParent = rootObject;
            MmRelayNode currentParentRelay = rootRelay;

            for (int level = 0; level < 5; level++)
            {
                var child = new GameObject($"Level{level}");
                child.transform.SetParent(currentParent.transform);

                var childRelay = child.AddComponent<MmRelayNode>();
                var responder = child.AddComponent<IntegrationResponder>();
                responder.Level = level;

                // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
                childRelay.MmRefreshResponders();

                currentParentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(currentParentRelay);

                responders.Add(responder);
                currentParent = child;
                currentParentRelay = childRelay;
            }

            // Use DSL to broadcast to all descendants
            rootRelay.Send(MmMethod.Initialize).ToDescendants().Execute();

            // Verify all levels received the message
            for (int i = 0; i < responders.Count; i++)
            {
                Assert.AreEqual(1, responders[i].InitCount,
                    $"Level {i} should receive Initialize message");
            }
        }

        [Test]
        public void Integration_BranchingHierarchy_AllBranchesReceive()
        {
            // Create hierarchy with multiple branches
            //        Root
            //      /   |   \
            //    A     B     C
            //   / \   / \   / \
            //  A1 A2 B1 B2 C1 C2

            var allResponders = new List<IntegrationResponder>();

            // Create 3 branches
            for (int branch = 0; branch < 3; branch++)
            {
                string branchName = ((char)('A' + branch)).ToString();
                var branchObj = new GameObject($"Branch{branchName}");
                branchObj.transform.SetParent(rootObject.transform);

                var branchRelay = branchObj.AddComponent<MmRelayNode>();
                var branchResponder = branchObj.AddComponent<IntegrationResponder>();
                branchResponder.BranchName = branchName;

                // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
                branchRelay.MmRefreshResponders();

                rootRelay.MmAddToRoutingTable(branchRelay, MmLevelFilter.Child);
                branchRelay.AddParent(rootRelay);
                allResponders.Add(branchResponder);

                // Create 2 children per branch
                for (int child = 0; child < 2; child++)
                {
                    var leafObj = new GameObject($"{branchName}{child + 1}");
                    leafObj.transform.SetParent(branchObj.transform);

                    var leafRelay = leafObj.AddComponent<MmRelayNode>();
                    var leafResponder = leafObj.AddComponent<IntegrationResponder>();
                    leafResponder.BranchName = $"{branchName}{child + 1}";

                    // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
                    leafRelay.MmRefreshResponders();

                    branchRelay.MmAddToRoutingTable(leafRelay, MmLevelFilter.Child);
                    leafRelay.AddParent(branchRelay);
                    allResponders.Add(leafResponder);
                }
            }

            // Use DSL broadcast
            rootRelay.Broadcast(MmMethod.Initialize);

            // All 9 responders should receive (3 branches + 6 leaves)
            Assert.AreEqual(9, allResponders.Count);
            foreach (var responder in allResponders)
            {
                Assert.AreEqual(1, responder.InitCount,
                    $"Responder {responder.BranchName} should receive Initialize");
            }
        }

        #endregion

        #region Complex Filter Combination Tests

        [Test]
        public void Integration_CombinedFilters_CorrectTargeting()
        {
            // Create hierarchy with different tags and active states
            var responders = new Dictionary<string, IntegrationResponder>();

            // Child with Tag0, Active
            var child1 = CreateTaggedChild("ActiveTag0", MmTag.Tag0, true);
            responders["ActiveTag0"] = child1.GetComponent<IntegrationResponder>();

            // Child with Tag0, Inactive
            var child2 = CreateTaggedChild("InactiveTag0", MmTag.Tag0, false);
            responders["InactiveTag0"] = child2.GetComponent<IntegrationResponder>();

            // Child with Tag1, Active
            var child3 = CreateTaggedChild("ActiveTag1", MmTag.Tag1, true);
            responders["ActiveTag1"] = child3.GetComponent<IntegrationResponder>();

            // Send only to active children with Tag0
            rootRelay.Send(MmMethod.Initialize)
                .ToChildren()
                .Active()
                .WithTag(MmTag.Tag0)
                .Execute();

            // Only ActiveTag0 should receive
            Assert.AreEqual(1, responders["ActiveTag0"].InitCount, "ActiveTag0 should receive");
            Assert.AreEqual(0, responders["InactiveTag0"].InitCount, "InactiveTag0 should NOT receive (inactive)");
            Assert.AreEqual(0, responders["ActiveTag1"].InitCount, "ActiveTag1 should NOT receive (wrong tag)");
        }

        private GameObject CreateTaggedChild(string name, MmTag tag, bool active)
        {
            var child = new GameObject(name);
            child.transform.SetParent(rootObject.transform);
            child.SetActive(active);

            var relay = child.AddComponent<MmRelayNode>();
            relay.Tag = tag;
            relay.TagCheckEnabled = true;

            var responder = child.AddComponent<IntegrationResponder>();

            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            relay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(rootRelay);

            return child;
        }

        #endregion

        #region Upward Communication Tests

        [Test]
        public void Integration_NotifyParent_ReachesCorrectTarget()
        {
            // Create child that notifies parent
            var childObj = new GameObject("NotifyingChild");
            childObj.transform.SetParent(rootObject.transform);

            var childRelay = childObj.AddComponent<MmRelayNode>();
            childObj.AddComponent<IntegrationResponder>();
            childRelay.MmRefreshResponders();

            var parentResponder = rootObject.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            rootRelay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(rootRelay);

            // Child notifies parent using DSL
            childRelay.Notify(MmMethod.Complete);

            Assert.AreEqual(1, parentResponder.CompleteCount, "Parent should receive Complete notification");
        }

        [Test]
        public void Integration_NotifyWithValue_ParentReceivesValue()
        {
            var childObj = new GameObject("ValueChild");
            childObj.transform.SetParent(rootObject.transform);

            var childRelay = childObj.AddComponent<MmRelayNode>();
            var parentResponder = rootObject.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            rootRelay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(rootRelay);

            // Notify with specific value
            childRelay.Notify(MmMethod.MessageInt, 42);

            Assert.AreEqual(42, parentResponder.LastIntValue, "Parent should receive int value");
        }

        #endregion

        #region SendTo Named Target Tests

        [Test]
        public void Integration_SendToNamed_FindsDeepTarget()
        {
            // Create deep hierarchy: Root > A > B > Target
            var objA = new GameObject("NodeA");
            objA.transform.SetParent(rootObject.transform);
            var relayA = objA.AddComponent<MmRelayNode>();
            rootRelay.MmAddToRoutingTable(relayA, MmLevelFilter.Child);
            relayA.AddParent(rootRelay);

            var objB = new GameObject("NodeB");
            objB.transform.SetParent(objA.transform);
            var relayB = objB.AddComponent<MmRelayNode>();
            relayA.MmAddToRoutingTable(relayB, MmLevelFilter.Child);
            relayB.AddParent(relayA);

            var target = new GameObject("TargetNode");
            target.transform.SetParent(objB.transform);
            var targetRelay = target.AddComponent<MmRelayNode>();
            var targetResponder = target.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            targetRelay.MmRefreshResponders();
            relayB.MmAddToRoutingTable(targetRelay, MmLevelFilter.Child);
            targetRelay.AddParent(relayB);

            // Use SendTo to find deep target
            rootRelay.SendTo("TargetNode", MmMethod.Initialize);

            Assert.AreEqual(1, targetResponder.InitCount, "Deep target should receive message via SendTo");
        }

        [Test]
        public void Integration_SendToWithValue_DeliverValue()
        {
            var target = new GameObject("ValueTarget");
            target.transform.SetParent(rootObject.transform);
            var targetRelay = target.AddComponent<MmRelayNode>();
            var targetResponder = target.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            targetRelay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(targetRelay, MmLevelFilter.Child);
            targetRelay.AddParent(rootRelay);

            // SendTo with value
            rootRelay.SendTo("ValueTarget", MmMethod.MessageFloat, 3.14f);

            Assert.AreEqual(3.14f, targetResponder.LastFloatValue, 0.001f);
        }

        #endregion

        #region MessageFactory Integration Tests

        [Test]
        public void Integration_MessageFactory_AllTypesWork()
        {
            var child = new GameObject("FactoryTest");
            child.transform.SetParent(rootObject.transform);
            var relay = child.AddComponent<MmRelayNode>();
            var responder = child.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            relay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(rootRelay);

            // Test all factory message types
            rootRelay.Broadcast(MmMessageFactory.Bool(true));
            Assert.IsTrue(responder.LastBoolValue, "Bool message should work");

            rootRelay.Broadcast(MmMessageFactory.Int(123));
            Assert.AreEqual(123, responder.LastIntValue, "Int message should work");

            rootRelay.Broadcast(MmMessageFactory.Float(2.5f));
            Assert.AreEqual(2.5f, responder.LastFloatValue, 0.001f, "Float message should work");

            rootRelay.Broadcast(MmMessageFactory.String("Hello DSL"));
            Assert.AreEqual("Hello DSL", responder.LastStringValue, "String message should work");
        }

        [Test]
        public void Integration_MessageFactory_CustomMethods()
        {
            var child = new GameObject("CustomMethodTest");
            child.transform.SetParent(rootObject.transform);
            var relay = child.AddComponent<MmRelayNode>();
            var responder = child.AddComponent<CustomMethodResponder>();
            // CRITICAL: Refresh responders so CustomMethodResponder is registered with Level=Self
            relay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(rootRelay);

            // Use factory for custom method
            var customMsg = MmMessageFactory.Custom(1001);
            rootRelay.Broadcast(customMsg);

            Assert.AreEqual(1, responder.CustomMethodCount, "Custom method via factory should work");
        }

        #endregion

        #region Query/Response Integration Tests

        [Test]
        public void Integration_QueryResponse_WorksAcrossHierarchy()
        {
            var child = new GameObject("QueryResponder");
            child.transform.SetParent(rootObject.transform);
            var childRelay = child.AddComponent<MmRelayNode>();
            var responder = child.AddComponent<QueryableResponder>();
            responder.HealthValue = 75;
            // CRITICAL: Refresh responders so QueryableResponder is registered with Level=Self
            childRelay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(rootRelay);

            int receivedValue = -1;

            // Query for health value
            int queryId = rootRelay.Query(MmMethod.MessageInt, response =>
            {
                receivedValue = ((MmMessageInt)response).value;
            });

            // Simulate responder answering
            childRelay.Respond(queryId, responder.HealthValue);

            Assert.AreEqual(75, receivedValue, "Query response should deliver correct value");
        }

        #endregion

        #region Backward Compatibility Tests

        [Test]
        public void Integration_MixedAPI_TraditionalAndDSLCoexist()
        {
            var child = new GameObject("MixedAPITest");
            child.transform.SetParent(rootObject.transform);
            var relay = child.AddComponent<MmRelayNode>();
            var responder = child.AddComponent<IntegrationResponder>();
            // CRITICAL: Refresh responders so IntegrationResponder is registered with Level=Self
            relay.MmRefreshResponders();

            rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(rootRelay);

            // Traditional API
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Child,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            rootRelay.MmInvoke(MmMethod.MessageInt, 100, metadata);

            // DSL API
            rootRelay.Send(MmMethod.MessageInt, 200).ToChildren().Execute();

            // Both should work
            Assert.AreEqual(200, responder.LastIntValue, "DSL should override traditional value");
            Assert.AreEqual(2, responder.IntMessageCount, "Both messages should be received");
        }

        #endregion

        #region Test Responders

        private class IntegrationResponder : MmBaseResponder
        {
            public int Level;
            public string BranchName;
            public int InitCount;
            public int CompleteCount;
            public int IntMessageCount;
            public int LastIntValue;
            public float LastFloatValue;
            public string LastStringValue;
            public bool LastBoolValue;

            public override void Initialize()
            {
                InitCount++;
            }

            protected override void Complete(bool active)
            {
                CompleteCount++;
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
                IntMessageCount++;
                LastIntValue = message.value;
            }

            protected override void ReceivedMessage(MmMessageFloat message)
            {
                LastFloatValue = message.value;
            }

            protected override void ReceivedMessage(MmMessageString message)
            {
                LastStringValue = message.value;
            }

            protected override void ReceivedMessage(MmMessageBool message)
            {
                LastBoolValue = message.value;
            }
        }

        private class CustomMethodResponder : MmExtendableResponder
        {
            public int CustomMethodCount;

            public override void Awake()
            {
                base.Awake();
                RegisterCustomHandler((MmMethod)1001, msg => CustomMethodCount++);
            }
        }

        private class QueryableResponder : MmBaseResponder
        {
            public int HealthValue;
        }

        #endregion
    }
}
