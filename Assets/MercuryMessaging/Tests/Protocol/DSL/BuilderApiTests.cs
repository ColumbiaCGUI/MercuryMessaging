// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// BuilderApiTests.cs - Tests for Builder API (Phase 2: Deferred Execution)
// Part of DSL/DX Improvements

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for the Builder API: relay.Build().ToChildren().Send("Hello").Execute()
    ///
    /// Unlike relay.To.Children.Send() which auto-executes,
    /// Build() returns a deferred builder that requires .Execute() to send.
    ///
    /// Use cases:
    /// - Conditional sending: if (ready) builder.Execute();
    /// - Storing message config for later reuse
    /// - Building message in stages before sending
    /// </summary>
    [TestFixture]
    public class BuilderApiTests
    {
        private GameObject _parentObj;
        private GameObject _childObj;
        private MmRelayNode _parentRelay;
        private MmRelayNode _childRelay;
        private TestMessageResponder _parentResponder;
        private TestMessageResponder _childResponder;

        [SetUp]
        public void SetUp()
        {
            // Create parent
            _parentObj = new GameObject("Parent");
            _parentRelay = _parentObj.AddComponent<MmRelayNode>();
            _parentResponder = _parentObj.AddComponent<TestMessageResponder>();

            // Create child
            _childObj = new GameObject("Child");
            _childObj.transform.SetParent(_parentObj.transform);
            _childRelay = _childObj.AddComponent<MmRelayNode>();
            _childResponder = _childObj.AddComponent<TestMessageResponder>();

            // Setup routing
            _parentRelay.MmAddToRoutingTable(_childRelay, MmLevelFilter.Child);
            _childRelay.AddParent(_parentRelay);
            _parentRelay.MmRefreshResponders();
            _childRelay.MmRefreshResponders();
        }

        [TearDown]
        public void TearDown()
        {
            if (_parentObj != null) Object.DestroyImmediate(_parentObj);
            if (_childObj != null) Object.DestroyImmediate(_childObj);
        }

        #region Build() Method Tests

        [Test]
        public void Build_ReturnsNonNullBuilder()
        {
            // Build() should return a valid builder struct
            var builder = _parentRelay.Build();

            // Struct is never null, but we verify it's usable
            Assert.DoesNotThrow(() => builder.ToChildren());
        }

        [UnityTest]
        public IEnumerator Build_DoesNotAutoExecute_UntilExecuteCalled()
        {
            yield return null;
            _childResponder.Reset();

            // Build a message but don't execute
            var builder = _parentRelay.Build().ToChildren().Send("deferred");
            yield return null;

            // Message should NOT have been sent yet
            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod,
                "Build() should not auto-execute - message should not be received yet");

            // Now execute
            builder.Execute();
            yield return null;

            // NOW the message should be received
            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("deferred", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Build_ConditionalExecution_OnlyExecutesWhenConditionMet()
        {
            yield return null;
            _childResponder.Reset();

            bool shouldSend = false;
            var builder = _parentRelay.Build().ToChildren().Send("conditional");

            // Condition is false, don't execute
            if (shouldSend)
            {
                builder.Execute();
            }
            yield return null;

            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod,
                "Message should not be sent when condition is false");

            // Now set condition to true and execute
            shouldSend = true;
            if (shouldSend)
            {
                builder.Execute();
            }
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("conditional", _childResponder.LastStringValue);
        }

        #endregion

        #region Routing Method Tests

        [UnityTest]
        public IEnumerator Build_ToChildren_Send_DeliversToChildren()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.Build().ToChildren().Send("hello").Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("hello", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Build_ToParents_Send_DeliversToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.Build().ToParents().Send(42).Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(42, _parentResponder.LastIntValue);
        }

        [UnityTest]
        public IEnumerator Build_ToDescendants_Initialize_SendsInitialize()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.Build().ToDescendants().Initialize().Execute();
            yield return null;

            Assert.AreEqual(MmMethod.Initialize, _childResponder.LastReceivedMethod);
        }

        #endregion

        #region Filter Tests

        [UnityTest]
        public IEnumerator Build_Active_FiltersInactive()
        {
            yield return null;
            _childResponder.Reset();
            _childObj.SetActive(false);

            _parentRelay.Build().ToChildren().Active().Send("filtered").Execute();
            yield return null;

            // Should NOT receive because child is inactive
            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod);

            _childObj.SetActive(true);
        }

        [UnityTest]
        public IEnumerator Build_WithTag_FiltersByTag()
        {
            yield return null;
            _childResponder.Reset();
            _childResponder.Tag = MmTag.Tag0;
            _childResponder.TagCheckEnabled = true;
            _childRelay.MmRefreshResponders();
            yield return null;

            // Wrong tag - should not receive
            _parentRelay.Build().ToChildren().WithTag(MmTag.Tag1).Send("wrong").Execute();
            yield return null;
            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod);

            // Correct tag - should receive
            _childResponder.Reset();
            _parentRelay.Build().ToChildren().WithTag(MmTag.Tag0).Send("correct").Execute();
            yield return null;
            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("correct", _childResponder.LastStringValue);

            _childResponder.TagCheckEnabled = false;
            _childRelay.MmRefreshResponders();
        }

        #endregion

        #region Comparison with To Property

        [UnityTest]
        public IEnumerator Build_ProducesSameResult_AsToProperty()
        {
            yield return null;

            // Test with Build().Execute()
            _childResponder.Reset();
            _parentRelay.Build().ToChildren().Send("test").Execute();
            yield return null;
            var buildMethod = _childResponder.LastReceivedMethod;
            var buildValue = _childResponder.LastStringValue;

            // Test with To (auto-execute)
            _childResponder.Reset();
            _parentRelay.To.Children.Send("test");
            yield return null;
            var toMethod = _childResponder.LastReceivedMethod;
            var toValue = _childResponder.LastStringValue;

            // Should produce identical results
            Assert.AreEqual(toMethod, buildMethod, "Methods should match");
            Assert.AreEqual(toValue, buildValue, "Values should match");
        }

        #endregion

        #region Responder Extension Tests

        [UnityTest]
        public IEnumerator Responder_Build_Works()
        {
            yield return null;
            _childResponder.Reset();

            _parentResponder.Build().ToChildren().Send("from responder").Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("from responder", _childResponder.LastStringValue);
        }

        [Test]
        public void Responder_Build_WithNullRelayNode_ReturnsDefault()
        {
            var standaloneObj = new GameObject("Standalone");
            var standaloneResponder = standaloneObj.AddComponent<TestMessageResponder>();

            // Should return default builder without throwing
            var builder = standaloneResponder.Build();
            Assert.DoesNotThrow(() => builder.ToChildren().Send("test").Execute());

            Object.DestroyImmediate(standaloneObj);
        }

        #endregion
    }
}
