// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmMessagingExtensionsTests.cs - Tests for the unified messaging API
// Part of DSL Overhaul Phase 1

using System.Collections;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for MmMessagingExtensions - the unified API for both MmRelayNode and MmBaseResponder.
    /// Tests cover Tier 1 (auto-execute) and Tier 2 (fluent chain) methods.
    /// </summary>
    [TestFixture]
    public class MmMessagingExtensionsTests
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

        #region Tier 1: Relay Node Broadcast Tests

        [UnityTest]
        public IEnumerator BroadcastInitialize_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastInitialize();
            yield return null;

            Assert.AreEqual(MmMethod.Initialize, _childResponder.LastReceivedMethod,
                "Child should receive Initialize message");
        }

        [UnityTest]
        public IEnumerator BroadcastRefresh_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastRefresh();
            yield return null;

            Assert.AreEqual(MmMethod.Refresh, _childResponder.LastReceivedMethod,
                "Child should receive Refresh message");
        }

        [UnityTest]
        public IEnumerator BroadcastSetActive_True_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastSetActive(true);
            yield return null;

            Assert.AreEqual(MmMethod.SetActive, _childResponder.LastReceivedMethod);
            Assert.AreEqual(true, _childResponder.LastBoolValue);
        }

        [UnityTest]
        public IEnumerator BroadcastSetActive_False_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastSetActive(false);
            yield return null;

            Assert.AreEqual(MmMethod.SetActive, _childResponder.LastReceivedMethod);
            Assert.AreEqual(false, _childResponder.LastBoolValue);
        }

        [UnityTest]
        public IEnumerator BroadcastSwitch_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastSwitch("TestState");
            yield return null;

            Assert.AreEqual(MmMethod.Switch, _childResponder.LastReceivedMethod);
            Assert.AreEqual("TestState", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator BroadcastValue_Bool_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastValue(true);
            yield return null;

            Assert.AreEqual(MmMethod.MessageBool, _childResponder.LastReceivedMethod);
            Assert.AreEqual(true, _childResponder.LastBoolValue);
        }

        [UnityTest]
        public IEnumerator BroadcastValue_Int_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastValue(42);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _childResponder.LastReceivedMethod);
            Assert.AreEqual(42, _childResponder.LastIntValue);
        }

        [UnityTest]
        public IEnumerator BroadcastValue_Float_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastValue(3.14f);
            yield return null;

            Assert.AreEqual(MmMethod.MessageFloat, _childResponder.LastReceivedMethod);
            Assert.AreEqual(3.14f, _childResponder.LastFloatValue, 0.001f);
        }

        [UnityTest]
        public IEnumerator BroadcastValue_String_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.BroadcastValue("hello");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("hello", _childResponder.LastStringValue);
        }

        #endregion

        #region Tier 1: Relay Node Notify Tests

        [UnityTest]
        public IEnumerator NotifyComplete_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.NotifyComplete();
            yield return null;

            Assert.AreEqual(MmMethod.Complete, _parentResponder.LastReceivedMethod,
                "Parent should receive Complete message");
        }

        [UnityTest]
        public IEnumerator NotifyValue_Bool_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.NotifyValue(true);
            yield return null;

            Assert.AreEqual(MmMethod.MessageBool, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(true, _parentResponder.LastBoolValue);
        }

        [UnityTest]
        public IEnumerator NotifyValue_Int_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.NotifyValue(100);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(100, _parentResponder.LastIntValue);
        }

        [UnityTest]
        public IEnumerator NotifyValue_Float_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.NotifyValue(2.5f);
            yield return null;

            Assert.AreEqual(MmMethod.MessageFloat, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(2.5f, _parentResponder.LastFloatValue, 0.001f);
        }

        [UnityTest]
        public IEnumerator NotifyValue_String_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.NotifyValue("status");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _parentResponder.LastReceivedMethod);
            Assert.AreEqual("status", _parentResponder.LastStringValue);
        }

        #endregion

        #region Tier 1: Responder Tests

        [UnityTest]
        public IEnumerator Responder_BroadcastInitialize_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentResponder.BroadcastInitialize();
            yield return null;

            Assert.AreEqual(MmMethod.Initialize, _childResponder.LastReceivedMethod);
        }

        [UnityTest]
        public IEnumerator Responder_BroadcastValue_String_SendsToDescendants()
        {
            yield return null;
            _childResponder.Reset();

            _parentResponder.BroadcastValue("test");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("test", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Responder_NotifyComplete_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childResponder.NotifyComplete();
            yield return null;

            Assert.AreEqual(MmMethod.Complete, _parentResponder.LastReceivedMethod);
        }

        [UnityTest]
        public IEnumerator Responder_NotifyValue_Int_SendsToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childResponder.NotifyValue(99);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(99, _parentResponder.LastIntValue);
        }

        [Test]
        public void Responder_WithNullRelayNode_DoesNotThrow()
        {
            // Create responder without relay node
            var standaloneObj = new GameObject("Standalone");
            var standaloneResponder = standaloneObj.AddComponent<TestMessageResponder>();

            // These should not throw
            Assert.DoesNotThrow(() => standaloneResponder.BroadcastInitialize());
            Assert.DoesNotThrow(() => standaloneResponder.NotifyComplete());
            Assert.DoesNotThrow(() => standaloneResponder.BroadcastValue(42));
            Assert.DoesNotThrow(() => standaloneResponder.NotifyValue("test"));

            Object.DestroyImmediate(standaloneObj);
        }

        #endregion

        #region Tier 2: Responder Fluent Chain Tests

        [UnityTest]
        public IEnumerator Responder_Send_ToDescendants_Execute_Works()
        {
            yield return null;
            _childResponder.Reset();

            _parentResponder.Send("fluent").ToDescendants().Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("fluent", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Responder_Send_ToParents_Execute_Works()
        {
            yield return null;
            _parentResponder.Reset();

            _childResponder.Send(77).ToParents().Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(77, _parentResponder.LastIntValue);
        }

        [Test]
        public void Responder_Send_WithAutoAddedRelayNode_ReturnsValidMessage()
        {
            // [RequireComponent] on MmBaseResponder auto-adds MmRelayNode
            var standaloneObj = new GameObject("Standalone");
            var standaloneResponder = standaloneObj.AddComponent<TestMessageResponder>();

            // Relay is auto-added, so Send() should return valid fluent message
            var result = standaloneResponder.Send("test");

            // Verify relay was auto-added and result is valid
            Assert.IsNotNull(standaloneObj.GetComponent<MmRelayNode>(),
                "MmRelayNode should be auto-added by [RequireComponent]");
            Assert.AreNotEqual(default(MmFluentMessage), result,
                "Send should return valid fluent message with auto-added relay");

            Object.DestroyImmediate(standaloneObj);
        }

        #endregion

        #region Performance Tests

        [Test]
        public void UnifiedAPI_HasMinimalOverhead()
        {
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _parentRelay.BroadcastValue(i);
            }

            // Time 1000 calls
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                _parentRelay.BroadcastValue(i);
            }
            sw.Stop();

            double avgNs = (sw.ElapsedTicks * 1_000_000_000.0 / System.Diagnostics.Stopwatch.Frequency) / 1000;

            // Should be under 50 microseconds per call (very generous threshold for Editor)
            Assert.Less(avgNs, 50000, $"Average call time {avgNs:F0}ns exceeds 50us threshold");

            UnityEngine.Debug.Log($"[UnifiedAPI Performance] Average: {avgNs:F0}ns per BroadcastValue call");
        }

        #endregion
    }

    /// <summary>
    /// Test responder that tracks received messages for assertions.
    /// </summary>
    public class TestMessageResponder : MmBaseResponder
    {
        public MmMethod LastReceivedMethod { get; private set; }
        public bool LastBoolValue { get; private set; }
        public int LastIntValue { get; private set; }
        public float LastFloatValue { get; private set; }
        public string LastStringValue { get; private set; }
        public int CallCount { get; private set; }

        public void Reset()
        {
            LastReceivedMethod = MmMethod.NoOp;
            LastBoolValue = false;
            LastIntValue = 0;
            LastFloatValue = 0f;
            LastStringValue = null;
            CallCount = 0;
        }

        protected override void ReceivedMessage(MmMessageBool message)
        {
            LastReceivedMethod = message.MmMethod;
            LastBoolValue = message.value;
            CallCount++;
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            LastReceivedMethod = message.MmMethod;
            LastIntValue = message.value;
            CallCount++;
        }

        protected override void ReceivedMessage(MmMessageFloat message)
        {
            LastReceivedMethod = message.MmMethod;
            LastFloatValue = message.value;
            CallCount++;
        }

        protected override void ReceivedMessage(MmMessageString message)
        {
            LastReceivedMethod = message.MmMethod;
            LastStringValue = message.value;
            CallCount++;
        }

        public override void Initialize()
        {
            LastReceivedMethod = MmMethod.Initialize;
            CallCount++;
        }

        public override void Refresh(System.Collections.Generic.List<MmTransform> transformList)
        {
            LastReceivedMethod = MmMethod.Refresh;
            CallCount++;
        }

        public override void SetActive(bool active)
        {
            LastReceivedMethod = MmMethod.SetActive;
            LastBoolValue = active;
            CallCount++;
        }

        protected override void Switch(string iName)
        {
            LastReceivedMethod = MmMethod.Switch;
            LastStringValue = iName;
            CallCount++;
        }

        protected override void Complete(bool active)
        {
            LastReceivedMethod = MmMethod.Complete;
            LastBoolValue = active;
            CallCount++;
        }
    }
}
