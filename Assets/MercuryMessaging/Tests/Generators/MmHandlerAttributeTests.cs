// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// Integration tests for [MmHandler] attribute source generator
// These tests verify that the generated dispatch code works correctly at runtime

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests.Generators
{
    /// <summary>
    /// Runtime tests for [MmHandler] attribute functionality.
    /// Verifies that:
    /// 1. Custom handlers are invoked for their registered method IDs
    /// 2. Multiple handlers can coexist in one responder
    /// 3. Standard and custom handlers work together
    /// 4. Unhandled custom methods fall through to base
    /// </summary>
    public class MmHandlerAttributeTests
    {
        private GameObject _root;
        private MmRelayNode _relay;

        [SetUp]
        public void Setup()
        {
            _root = new GameObject("TestRoot");
            _relay = _root.AddComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [UnityTest]
        public IEnumerator MmHandler_SingleHandler_ReceivesMessage()
        {
            // Arrange
            var responder = _root.AddComponent<SingleHandlerResponder>();
            _relay.MmRefreshResponders();
            yield return null;

            // Act - Use MmInvoke with MmMethod only (uses default metadata)
            _relay.MmInvoke((MmMethod)1000);

            // Assert
            Assert.IsTrue(responder.Handler1000Called, "Handler for method 1000 should have been called");
        }

        [UnityTest]
        public IEnumerator MmHandler_MultipleHandlers_EachReceivesCorrectMessage()
        {
            // Arrange
            var responder = _root.AddComponent<MultipleHandlerResponder>();
            _relay.MmRefreshResponders();
            yield return null;

            // Act
            _relay.MmInvoke((MmMethod)1000);
            _relay.MmInvoke((MmMethod)1001);
            _relay.MmInvoke((MmMethod)1002);

            // Assert
            Assert.IsTrue(responder.Handler1000Called, "Handler 1000 should have been called");
            Assert.IsTrue(responder.Handler1001Called, "Handler 1001 should have been called");
            Assert.IsTrue(responder.Handler1002Called, "Handler 1002 should have been called");
        }

        [UnityTest]
        public IEnumerator MmHandler_MixedWithStandardHandlers_BothWork()
        {
            // Arrange
            var responder = _root.AddComponent<MixedHandlerResponder>();
            _relay.MmRefreshResponders();
            yield return null;

            // Act - Standard message (using the int overload)
            _relay.MmInvoke(MmMethod.MessageInt, 42);

            // Act - Custom message
            _relay.MmInvoke((MmMethod)1000);

            // Assert
            Assert.AreEqual(42, responder.ReceivedIntValue, "Standard ReceivedMessage should work");
            Assert.IsTrue(responder.CustomHandler1000Called, "Custom handler 1000 should work");
        }

        [UnityTest]
        public IEnumerator MmHandler_UnregisteredMethod_FallsThrough()
        {
            // Arrange
            var responder = _root.AddComponent<SingleHandlerResponder>();
            _relay.MmRefreshResponders();
            yield return null;

            // Act - Send method 1001 which has no handler
            _relay.MmInvoke((MmMethod)1001);

            // Assert - Handler 1000 should NOT be called, no exception thrown
            Assert.IsFalse(responder.Handler1000Called, "Unregistered method should not trigger other handlers");
        }

        [UnityTest]
        public IEnumerator MmHandler_WithTypedMessage_CastsCorrectly()
        {
            // Arrange
            var responder = _root.AddComponent<TypedHandlerResponder>();
            _relay.MmRefreshResponders();
            yield return null;

            // Act - Send string message with custom method
            // Use the string overload which will set up the message properly
            _relay.MmInvoke((MmMethod)1000, "TestValue");

            // Assert
            Assert.AreEqual("TestValue", responder.ReceivedValue, "Typed handler should receive correct value");
        }
    }

    // =========================================================================
    // Test Responders (these require source generator to generate MmInvoke)
    // =========================================================================
    // NOTE: These are partial classes - the source generator creates the MmInvoke override

    /// <summary>
    /// Simple responder with a single custom handler.
    /// The source generator will create an MmInvoke that dispatches method 1000 to OnCustomMethod1000.
    /// </summary>
    [MmGenerateDispatch]
    public partial class SingleHandlerResponder : MmBaseResponder
    {
        public bool Handler1000Called { get; private set; }

        [MmHandler(1000)]
        private void OnCustomMethod1000(MmMessage msg)
        {
            Handler1000Called = true;
        }
    }

    /// <summary>
    /// Responder with multiple custom handlers.
    /// </summary>
    [MmGenerateDispatch]
    public partial class MultipleHandlerResponder : MmBaseResponder
    {
        public bool Handler1000Called { get; private set; }
        public bool Handler1001Called { get; private set; }
        public bool Handler1002Called { get; private set; }

        [MmHandler(1000, Name = "ColorHandler")]
        private void OnColor(MmMessage msg)
        {
            Handler1000Called = true;
        }

        [MmHandler(1001, Name = "ScaleHandler")]
        private void OnScale(MmMessage msg)
        {
            Handler1001Called = true;
        }

        [MmHandler(1002, Name = "RotationHandler")]
        private void OnRotation(MmMessage msg)
        {
            Handler1002Called = true;
        }
    }

    /// <summary>
    /// Responder mixing standard ReceivedMessage handlers with custom [MmHandler] methods.
    /// </summary>
    [MmGenerateDispatch]
    public partial class MixedHandlerResponder : MmBaseResponder
    {
        public int ReceivedIntValue { get; private set; }
        public bool CustomHandler1000Called { get; private set; }

        // Standard handler (via ReceivedMessage override)
        protected override void ReceivedMessage(MmMessageInt msg)
        {
            ReceivedIntValue = msg.value;
        }

        // Custom handler (via [MmHandler] attribute)
        [MmHandler(1000)]
        private void OnCustomAction(MmMessage msg)
        {
            CustomHandler1000Called = true;
        }
    }

    /// <summary>
    /// Responder that receives a typed message via custom handler.
    /// </summary>
    [MmGenerateDispatch]
    public partial class TypedHandlerResponder : MmBaseResponder
    {
        public string ReceivedValue { get; private set; }

        [MmHandler(1000)]
        private void OnTypedMessage(MmMessage msg)
        {
            if (msg is MmMessageString strMsg)
            {
                ReceivedValue = strMsg.value;
            }
        }
    }
}
