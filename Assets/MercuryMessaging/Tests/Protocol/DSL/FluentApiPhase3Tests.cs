// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Unit tests for Language DSL Phase 3: Type Inference, Convenience Extensions, and Temporal Patterns
// Phase 3 - Tasks 3.1-3.4

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for Language DSL Phase 3 features.
    /// Task 3.1: Message Factory
    /// Task 3.2: Convenience Extensions
    /// Task 3.3: Async/Await Support
    /// Task 3.4: Temporal Extensions
    /// </summary>
    [TestFixture]
    public class FluentApiPhase3Tests
    {
        private GameObject rootObject;
        private MmRelayNode rootRelay;
        private List<GameObject> childObjects;
        private List<TestResponder> responders;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            // Create root
            rootObject = new GameObject("Root");
            rootRelay = rootObject.AddComponent<MmRelayNode>();

            childObjects = new List<GameObject>();
            responders = new List<TestResponder>();

            // Create hierarchy
            for (int i = 0; i < 3; i++)
            {
                var child = new GameObject($"Child{i}");
                child.transform.SetParent(rootObject.transform);

                var childRelay = child.AddComponent<MmRelayNode>();
                var responder = child.AddComponent<TestResponder>();

                // CRITICAL: Refresh responders so TestResponder is registered with Level=Self
                childRelay.MmRefreshResponders();

                rootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(rootRelay);

                childObjects.Add(child);
                responders.Add(responder);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (rootObject != null)
                Object.DestroyImmediate(rootObject);

            MmRelayNodeExtensions.ClearPendingQueries();
        }

        private class TestResponder : MmBaseResponder
        {
            public int InitializeCount;
            public int RefreshCount;
            public int CompleteCount;
            public bool LastSetActiveValue;
            public int LastIntValue;
            public float LastFloatValue;
            public string LastStringValue;
            public bool LastBoolValue;

            public override void Initialize()
            {
                InitializeCount++;
            }

            public override void Refresh(System.Collections.Generic.List<MmTransform> transformList)
            {
                RefreshCount++;
            }

            protected override void Complete(bool active)
            {
                CompleteCount++;
            }

            public override void SetActive(bool active)
            {
                LastSetActiveValue = active;
            }

            protected override void ReceivedMessage(MmMessageInt message)
            {
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

        #endregion

        #region Task 3.1: Message Factory Tests

        [Test]
        public void MessageFactory_CreateBool_ReturnsCorrectType()
        {
            var msg = MmMessageFactory.Bool(true);

            Assert.IsInstanceOf<MmMessageBool>(msg);
            Assert.AreEqual(MmMethod.MessageBool, msg.MmMethod);
            Assert.IsTrue(msg.value);
        }

        [Test]
        public void MessageFactory_CreateInt_ReturnsCorrectType()
        {
            var msg = MmMessageFactory.Int(42);

            Assert.IsInstanceOf<MmMessageInt>(msg);
            Assert.AreEqual(MmMethod.MessageInt, msg.MmMethod);
            Assert.AreEqual(42, msg.value);
        }

        [Test]
        public void MessageFactory_CreateFloat_ReturnsCorrectType()
        {
            var msg = MmMessageFactory.Float(3.14f);

            Assert.IsInstanceOf<MmMessageFloat>(msg);
            Assert.AreEqual(MmMethod.MessageFloat, msg.MmMethod);
            Assert.AreEqual(3.14f, msg.value, 0.001f);
        }

        [Test]
        public void MessageFactory_CreateString_ReturnsCorrectType()
        {
            var msg = MmMessageFactory.String("Hello");

            Assert.IsInstanceOf<MmMessageString>(msg);
            Assert.AreEqual(MmMethod.MessageString, msg.MmMethod);
            Assert.AreEqual("Hello", msg.value);
        }

        [Test]
        public void MessageFactory_CreateVector3_ReturnsCorrectType()
        {
            var vec = new Vector3(1, 2, 3);
            var msg = MmMessageFactory.Vector3(vec);

            Assert.IsInstanceOf<MmMessageVector3>(msg);
            Assert.AreEqual(MmMethod.MessageVector3, msg.MmMethod);
            Assert.AreEqual(vec, msg.value);
        }

        [Test]
        public void MessageFactory_GenericCreate_InfersType()
        {
            var boolMsg = MmMessageFactory.Create(true);
            var intMsg = MmMessageFactory.Create(42);
            var floatMsg = MmMessageFactory.Create(3.14f);
            var stringMsg = MmMessageFactory.Create("Test");

            Assert.IsInstanceOf<MmMessageBool>(boolMsg);
            Assert.IsInstanceOf<MmMessageInt>(intMsg);
            Assert.IsInstanceOf<MmMessageFloat>(floatMsg);
            Assert.IsInstanceOf<MmMessageString>(stringMsg);
        }

        [Test]
        public void MessageFactory_Initialize_CreatesCorrectCommand()
        {
            var msg = MmMessageFactory.Initialize();

            Assert.AreEqual(MmMethod.Initialize, msg.MmMethod);
        }

        [Test]
        public void MessageFactory_SetActive_CreatesCorrectCommand()
        {
            var msg = MmMessageFactory.SetActive(true);

            Assert.IsInstanceOf<MmMessageBool>(msg);
            Assert.AreEqual(MmMethod.SetActive, msg.MmMethod);
            Assert.IsTrue(msg.value);
        }

        [Test]
        public void MessageFactory_Switch_ByName_CreatesCorrectCommand()
        {
            var msg = MmMessageFactory.Switch("StateA");

            Assert.IsInstanceOf<MmMessageString>(msg);
            Assert.AreEqual(MmMethod.Switch, msg.MmMethod);
            Assert.AreEqual("StateA", msg.value);
        }

        [Test]
        public void MessageFactory_Custom_CreatesWithMethodId()
        {
            var msg = MmMessageFactory.Custom(1001);

            Assert.AreEqual((MmMethod)1001, msg.MmMethod);
        }

        [Test]
        public void MessageFactory_CustomWithPayload_CreatesWithMethodAndValue()
        {
            var msg = MmMessageFactory.Custom(1002, 42);

            Assert.AreEqual((MmMethod)1002, msg.MmMethod);
            Assert.IsInstanceOf<MmMessageInt>(msg);
        }

        [Test]
        public void MessageFactory_WithMetadata_AppliesMetadata()
        {
            var metadata = new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.Active);
            var msg = MmMessageFactory.Int(10).WithMetadata(metadata);

            Assert.AreEqual(MmLevelFilter.Child, msg.MetadataBlock.LevelFilter);
            Assert.AreEqual(MmActiveFilter.Active, msg.MetadataBlock.ActiveFilter);
        }

        [Test]
        public void MessageFactory_ToChildren_SetsLevelFilter()
        {
            var msg = MmMessageFactory.Int(10).ToChildren();

            Assert.AreEqual(MmLevelFilter.Child, msg.MetadataBlock.LevelFilter);
        }

        [Test]
        public void MessageFactory_ToDescendants_SetsLevelFilter()
        {
            var msg = MmMessageFactory.Int(10).ToDescendants();

            Assert.AreEqual(MmLevelFilter.Descendants, msg.MetadataBlock.LevelFilter);
        }

        #endregion

        #region Task 3.2: Convenience Extension Tests

        [Test]
        public void Broadcast_SendsToAllDescendants()
        {
            rootRelay.Broadcast(MmMethod.Initialize);

            foreach (var responder in responders)
            {
                Assert.AreEqual(1, responder.InitializeCount, $"Responder should receive Initialize");
            }
        }

        [Test]
        public void Broadcast_BoolValue_ReachesDescendants()
        {
            rootRelay.Broadcast(MmMethod.SetActive, true);

            foreach (var responder in responders)
            {
                Assert.IsTrue(responder.LastSetActiveValue, "Responder should receive SetActive true");
            }
        }

        [Test]
        public void BroadcastSetActive_ConvenienceMethod_Works()
        {
            rootRelay.BroadcastSetActive(false);

            foreach (var responder in responders)
            {
                Assert.IsFalse(responder.LastSetActiveValue, "Responder should receive SetActive false");
            }
        }

        [Test]
        public void BroadcastInitialize_ConvenienceMethod_Works()
        {
            rootRelay.BroadcastInitialize();

            foreach (var responder in responders)
            {
                Assert.AreEqual(1, responder.InitializeCount, "Responder should receive Initialize");
            }
        }

        [Test]
        public void Notify_SendsToParent()
        {
            // Add parent responder
            var parentResponder = rootObject.AddComponent<TestResponder>();
            // CRITICAL: Refresh responders so TestResponder is registered with Level=Self
            rootRelay.MmRefreshResponders();

            // Child notifies parent
            var childRelay = childObjects[0].GetComponent<MmRelayNode>();
            childRelay.Notify(MmMethod.Complete);

            Assert.AreEqual(1, parentResponder.CompleteCount, "Parent should receive Complete notification");
        }

        [Test]
        public void Notify_WithValue_SendsToParent()
        {
            var parentResponder = rootObject.AddComponent<TestResponder>();
            // CRITICAL: Refresh responders so TestResponder is registered with Level=Self
            rootRelay.MmRefreshResponders();
            var childRelay = childObjects[0].GetComponent<MmRelayNode>();

            childRelay.Notify(MmMethod.MessageInt, 99);

            Assert.AreEqual(99, parentResponder.LastIntValue, "Parent should receive int value");
        }

        [Test]
        public void NotifyComplete_ConvenienceMethod_Works()
        {
            var parentResponder = rootObject.AddComponent<TestResponder>();
            // CRITICAL: Refresh responders so TestResponder is registered with Level=Self
            rootRelay.MmRefreshResponders();
            var childRelay = childObjects[0].GetComponent<MmRelayNode>();

            childRelay.NotifyComplete();

            Assert.AreEqual(1, parentResponder.CompleteCount, "Parent should receive Complete");
        }

        [Test]
        public void SendTo_ByName_FindsTarget()
        {
            // Rename a child for targeting
            childObjects[1].name = "TargetNode";

            rootRelay.SendTo("TargetNode", MmMethod.Initialize);

            Assert.AreEqual(1, responders[1].InitializeCount, "Target should receive message");
            Assert.AreEqual(0, responders[0].InitializeCount, "Non-target should not receive");
            Assert.AreEqual(0, responders[2].InitializeCount, "Non-target should not receive");
        }

        [Test]
        public void SendTo_ByName_WithValue_Works()
        {
            childObjects[0].name = "ValueTarget";

            rootRelay.SendTo("ValueTarget", MmMethod.MessageFloat, 1.5f);

            Assert.AreEqual(1.5f, responders[0].LastFloatValue, 0.01f);
        }

        [Test]
        public void SendTo_NonExistent_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "MmRelayNodeExtensions.SendTo: Target 'NonExistent' not found in hierarchy");
            rootRelay.SendTo("NonExistent", MmMethod.Initialize);
        }

        [Test]
        public void SendTo_DirectReference_Works()
        {
            var targetRelay = childObjects[1].GetComponent<MmRelayNode>();

            rootRelay.SendTo(targetRelay, MmMethod.Initialize);

            Assert.AreEqual(1, responders[1].InitializeCount);
        }

        [Test]
        public void TryFindTarget_ReturnsTrue_WhenExists()
        {
            childObjects[2].name = "FindMe";

            bool found = rootRelay.TryFindTarget("FindMe", out var target);

            Assert.IsTrue(found);
            Assert.IsNotNull(target);
            Assert.AreEqual("FindMe", target.gameObject.name);
        }

        [Test]
        public void TryFindTarget_ReturnsFalse_WhenNotExists()
        {
            bool found = rootRelay.TryFindTarget("NotHere", out var target);

            Assert.IsFalse(found);
            Assert.IsNull(target);
        }

        [Test]
        public void HasTarget_ReturnsCorrectly()
        {
            childObjects[0].name = "ExistingTarget";

            Assert.IsTrue(rootRelay.HasTarget("ExistingTarget"));
            Assert.IsFalse(rootRelay.HasTarget("MissingTarget"));
        }

        [Test]
        public void Query_RegistersCallback()
        {
            MmMessage receivedResponse = null;

            int queryId = rootRelay.Query(MmMethod.MessageInt, response =>
            {
                receivedResponse = response;
            });

            // Simulate response
            rootRelay.Respond(queryId, 42);

            Assert.IsNotNull(receivedResponse);
            Assert.IsInstanceOf<MmMessageInt>(receivedResponse);
            Assert.AreEqual(42, ((MmMessageInt)receivedResponse).value);
        }

        [Test]
        public void CancelQuery_RemovesPendingQuery()
        {
            int queryId = rootRelay.Query(MmMethod.MessageInt, response => { });

            bool cancelled = MmRelayNodeExtensions.CancelQuery(queryId);

            Assert.IsTrue(cancelled);
        }

        #endregion

        #region Task 3.3 & 3.4: Temporal Extension Tests

        [UnityTest]
        public IEnumerator After_DelaysMessage()
        {
            var handle = rootRelay.After(0.1f, MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilter.Descendants));

            // Not sent yet
            Assert.AreEqual(0, responders[0].InitializeCount);

            yield return new WaitForSeconds(0.15f);

            // Now sent
            Assert.AreEqual(1, responders[0].InitializeCount);
        }

        [UnityTest]
        public IEnumerator After_CanBeCancelled()
        {
            var handle = rootRelay.After(0.1f, MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilter.Descendants));

            handle.Cancel();

            yield return new WaitForSeconds(0.15f);

            // Should not be sent
            Assert.AreEqual(0, responders[0].InitializeCount);
        }

        [UnityTest]
        public IEnumerator Every_RepeatsMessage()
        {
            var handle = rootRelay.Every(0.05f, MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilter.Descendants), 3);

            // Wait longer to account for frame timing variance
            // 3 messages at 0.05s interval = 0.15s minimum, add generous margin
            yield return new WaitForSeconds(0.3f);

            // Should have repeated 3 times
            Assert.AreEqual(3, responders[0].InitializeCount);
        }

        [UnityTest]
        public IEnumerator Every_StopsOnCancel()
        {
            var handle = rootRelay.Every(0.05f, MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilter.Descendants));

            yield return new WaitForSeconds(0.12f);
            int countBeforeCancel = responders[0].InitializeCount;

            handle.Cancel();

            yield return new WaitForSeconds(0.1f);

            // Should not have increased much after cancel
            Assert.AreEqual(countBeforeCancel, responders[0].InitializeCount);
        }

        [UnityTest]
        public IEnumerator When_WaitsForCondition()
        {
            bool conditionMet = false;

            var handle = rootRelay.When(() => conditionMet, MmMethod.Initialize);

            yield return new WaitForSeconds(0.05f);
            Assert.AreEqual(0, responders[0].InitializeCount, "Should not send before condition");

            conditionMet = true;
            yield return new WaitForSeconds(0.05f);

            Assert.AreEqual(1, responders[0].InitializeCount, "Should send after condition met");
        }

        [UnityTest]
        public IEnumerator When_TimesOut()
        {
            var handle = rootRelay.When(() => false, MmMethod.Initialize, timeout: 0.1f);

            yield return new WaitForSeconds(0.15f);

            // Should have timed out, message not sent
            Assert.AreEqual(0, responders[0].InitializeCount);
        }

        [Test]
        public void Schedule_FluentBuilder_Works()
        {
            var handle = rootRelay.Schedule(MmMethod.Initialize)
                .ToDescendants()
                .Execute();

            Assert.IsNotNull(handle);
            Assert.AreEqual(1, responders[0].InitializeCount);
        }

        [UnityTest]
        public IEnumerator Schedule_WithDelay_DelaysExecution()
        {
            var handle = rootRelay.Schedule(MmMethod.Initialize)
                .ToDescendants()
                .After(0.1f)
                .Execute();

            Assert.AreEqual(0, responders[0].InitializeCount);

            yield return new WaitForSeconds(0.15f);

            Assert.AreEqual(1, responders[0].InitializeCount);
        }

        [Test]
        public void TemporalHandle_IsCancelled_ReflectsState()
        {
            var handle = new MmTemporalHandle();

            Assert.IsFalse(handle.IsCancelled);

            handle.Cancel();

            Assert.IsTrue(handle.IsCancelled);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Factory_And_Broadcast_Integration()
        {
            var msg = MmMessageFactory.Int(100);
            rootRelay.Broadcast(msg);

            foreach (var responder in responders)
            {
                Assert.AreEqual(100, responder.LastIntValue);
            }
        }

        [Test]
        public void Factory_And_SendTo_Integration()
        {
            childObjects[0].name = "IntTarget";

            var msg = MmMessageFactory.String("Hello");
            rootRelay.SendTo("IntTarget", msg);

            Assert.AreEqual("Hello", responders[0].LastStringValue);
        }

        [UnityTest]
        public IEnumerator Factory_And_After_Integration()
        {
            var msg = MmMessageFactory.Bool(true);
            msg.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Descendants);

            rootRelay.After(0.1f, msg);

            yield return new WaitForSeconds(0.15f);

            Assert.IsTrue(responders[0].LastBoolValue);
        }

        #endregion
    }
}
