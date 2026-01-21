// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// PropertyRoutingTests.cs - Tests for property-based routing DSL
// Part of DSL Phase 1: Shorter Syntax

using System.Collections;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for property-based routing syntax: relay.To.Children.Send("Hello")
    /// Validates both MmRelayNode.To property and MmBaseResponder.To() extension.
    /// </summary>
    [TestFixture]
    public class PropertyRoutingTests
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

        #region Relay Node Property Routing Tests

        [UnityTest]
        public IEnumerator RelayNode_To_Children_Send_String_DeliversToChildren()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.To.Children.Send("Hello");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("Hello", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_Send_Int_DeliversToChildren()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.To.Children.Send(42);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _childResponder.LastReceivedMethod);
            Assert.AreEqual(42, _childResponder.LastIntValue);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Parents_Send_Int_DeliversToParents()
        {
            yield return null;
            _parentResponder.Reset();

            _childRelay.To.Parents.Send(99);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(99, _parentResponder.LastIntValue);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Descendants_Initialize_SendsInitialize()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.To.Descendants.Initialize();
            yield return null;

            Assert.AreEqual(MmMethod.Initialize, _childResponder.LastReceivedMethod);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_Refresh_SendsRefresh()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.To.Children.Refresh();
            yield return null;

            Assert.AreEqual(MmMethod.Refresh, _childResponder.LastReceivedMethod);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_SetActive_SendsSetActive()
        {
            yield return null;
            _childResponder.Reset();

            _parentRelay.To.Children.SetActive(false);
            yield return null;

            Assert.AreEqual(MmMethod.SetActive, _childResponder.LastReceivedMethod);
            Assert.AreEqual(false, _childResponder.LastBoolValue);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_Active_FiltersActive()
        {
            yield return null;
            _childResponder.Reset();

            // Deactivate child
            _childObj.SetActive(false);

            _parentRelay.To.Children.Active.Send("filtered");
            yield return null;

            // Should NOT receive message because child is inactive and Active filter is applied
            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod);

            // Reactivate for cleanup
            _childObj.SetActive(true);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_IncludeInactive_IncludesInactive()
        {
            yield return null;
            _childResponder.Reset();

            // Deactivate child
            _childObj.SetActive(false);

            _parentRelay.To.Children.IncludeInactive.Send("included");
            yield return null;

            // Should receive message because IncludeInactive is applied
            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("included", _childResponder.LastStringValue);

            // Reactivate for cleanup
            _childObj.SetActive(true);
        }

        [UnityTest]
        public IEnumerator RelayNode_To_Children_WithTag_FiltersbyTag()
        {
            yield return null;
            _childResponder.Reset();
            _childResponder.Tag = MmTag.Tag0;
            _childResponder.TagCheckEnabled = true;

            // Must refresh routing table after changing TagCheckEnabled
            // because the framework caches this value for performance
            _childRelay.MmRefreshResponders();
            yield return null;

            _parentRelay.To.Children.WithTag(MmTag.Tag1).Send("wrong tag");
            yield return null;

            // Should NOT receive because tag doesn't match
            Assert.AreEqual(MmMethod.NoOp, _childResponder.LastReceivedMethod);

            // Now send with correct tag
            _childResponder.Reset();
            _parentRelay.To.Children.WithTag(MmTag.Tag0).Send("correct tag");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("correct tag", _childResponder.LastStringValue);

            // Cleanup
            _childResponder.TagCheckEnabled = false;
            _childRelay.MmRefreshResponders();
        }

        #endregion

        #region Responder Property Routing Tests

        [UnityTest]
        public IEnumerator Responder_To_Children_Send_Works()
        {
            yield return null;
            _childResponder.Reset();

            _parentResponder.To().Children.Send("from responder");
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("from responder", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Responder_To_Parents_Send_Works()
        {
            yield return null;
            _parentResponder.Reset();

            _childResponder.To().Parents.Send(123);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _parentResponder.LastReceivedMethod);
            Assert.AreEqual(123, _parentResponder.LastIntValue);
        }

        [Test]
        public void Responder_To_WithAutoAddedRelayNode_ReturnsValidBuilder()
        {
            // [RequireComponent] on MmBaseResponder auto-adds MmRelayNode
            var standaloneObj = new GameObject("Standalone");
            var standaloneResponder = standaloneObj.AddComponent<TestMessageResponder>();

            // Relay is auto-added, so To() should return valid builder
            var builder = standaloneResponder.To();

            // Verify relay was auto-added and result is valid
            Assert.IsNotNull(standaloneObj.GetComponent<MmRelayNode>(),
                "MmRelayNode should be auto-added by [RequireComponent]");
            Assert.AreNotEqual(default(MmRoutingBuilder), builder,
                "To() should return valid builder with auto-added relay");

            Object.DestroyImmediate(standaloneObj);
        }

        [Test]
        public void Responder_To_WithNullRelayNode_Send_DoesNotThrow()
        {
            var standaloneObj = new GameObject("Standalone");
            var standaloneResponder = standaloneObj.AddComponent<TestMessageResponder>();

            // Should not throw when sending - just silently do nothing
            Assert.DoesNotThrow(() => standaloneResponder.To().Children.Send("test"));
            Assert.DoesNotThrow(() => standaloneResponder.To().Parents.Send(42));
            Assert.DoesNotThrow(() => standaloneResponder.To().Descendants.Initialize());

            Object.DestroyImmediate(standaloneObj);
        }

        #endregion

        #region Comparison Tests - Verify Equivalence with Fluent API

        [UnityTest]
        public IEnumerator PropertyRouting_EquivalentTo_FluentAPI()
        {
            yield return null;

            // Test property routing
            _childResponder.Reset();
            _parentRelay.To.Children.Send("test_message");
            yield return null;
            var propertyMethod = _childResponder.LastReceivedMethod;
            var propertyValue = _childResponder.LastStringValue;

            // Test fluent API with same message
            _childResponder.Reset();
            _parentRelay.Send("test_message").ToChildren().Execute();
            yield return null;
            var fluentMethod = _childResponder.LastReceivedMethod;
            var fluentValue = _childResponder.LastStringValue;

            // Should be equivalent - both APIs deliver the same message
            Assert.AreEqual(fluentMethod, propertyMethod, "Methods should match");
            Assert.AreEqual(fluentValue, propertyValue, "Values should match");
        }

        #endregion

        #region Backward Compatibility Tests

        [UnityTest]
        public IEnumerator FluentAPI_Execute_StillWorks()
        {
            yield return null;
            _childResponder.Reset();

            // Original fluent API should still work
            _parentRelay.Send("original").ToChildren().Execute();
            yield return null;

            Assert.AreEqual(MmMethod.MessageString, _childResponder.LastReceivedMethod);
            Assert.AreEqual("original", _childResponder.LastStringValue);
        }

        [UnityTest]
        public IEnumerator Tier1_BroadcastValue_StillWorks()
        {
            yield return null;
            _childResponder.Reset();

            // Tier 1 auto-execute should still work
            _parentRelay.BroadcastValue(777);
            yield return null;

            Assert.AreEqual(MmMethod.MessageInt, _childResponder.LastReceivedMethod);
            Assert.AreEqual(777, _childResponder.LastIntValue);
        }

        #endregion

        #region Performance Tests

        [Test]
        public void PropertyRouting_HasMinimalOverhead()
        {
            // Warm up
            for (int i = 0; i < 100; i++)
            {
                _parentRelay.To.Children.Send(i);
            }

            // Time 1000 calls
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                _parentRelay.To.Children.Send(i);
            }
            sw.Stop();

            double avgNs = (sw.ElapsedTicks * 1_000_000_000.0 / System.Diagnostics.Stopwatch.Frequency) / 1000;

            // Should be under 100 microseconds per call (generous threshold for Editor)
            Assert.Less(avgNs, 100000, $"Average call time {avgNs:F0}ns exceeds 100us threshold");

            Debug.Log($"[PropertyRouting Performance] Average: {avgNs:F0}ns per To.Children.Send call");
        }

        #endregion
    }
}
