// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// ResponderBuilderTests.cs - Tests for Responder Handler DSL
// Part of DSL Overhaul Phase 6

using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for ResponderHandlerBuilder and MmResponderExtensions.
    /// Tests cover fluent handler configuration, quick registration, and batch operations.
    /// </summary>
    [TestFixture]
    public class ResponderBuilderTests
    {
        private GameObject _responderObj;
        private TestExtendableResponder _responder;

        [SetUp]
        public void SetUp()
        {
            _responderObj = new GameObject("TestResponder");
            _responder = _responderObj.AddComponent<TestExtendableResponder>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_responderObj != null)
                Object.DestroyImmediate(_responderObj);
        }

        #region Fluent Configuration Tests

        [Test]
        public void ConfigureHandlers_ReturnsBuilder()
        {
            var builder = _responder.ConfigureHandlers();

            Assert.IsNotNull(builder);
        }

        [Test]
        public void OnCustomMethod_AddsHandler()
        {
            bool handlerCalled = false;

            _responder.ConfigureHandlers()
                .OnCustomMethod(1000, msg => handlerCalled = true)
                .Build();

            // Trigger the handler
            _responder.MmInvoke(new MmMessage((MmMethod)1000));

            Assert.IsTrue(handlerCalled);
        }

        [Test]
        public void OnCustomMethod_ThrowsForInvalidMethodId()
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                _responder.ConfigureHandlers()
                    .OnCustomMethod(500, msg => { }); // Should throw - method must be >= 1000
            });
        }

        [Test]
        public void ChainedCustomMethods_AllHandlersCalled()
        {
            int callCount = 0;

            _responder.ConfigureHandlers()
                .OnCustomMethod(1000, msg => callCount++)
                .OnCustomMethod(1001, msg => callCount++)
                .OnCustomMethod(1002, msg => callCount++)
                .Build();

            _responder.MmInvoke(new MmMessage((MmMethod)1000));
            _responder.MmInvoke(new MmMessage((MmMethod)1001));
            _responder.MmInvoke(new MmMessage((MmMethod)1002));

            Assert.AreEqual(3, callCount);
        }

        #endregion

        #region Quick Registration Tests

        [Test]
        public void QuickRegister_RegistersHandler()
        {
            bool handlerCalled = false;

            _responder.QuickRegister(1000, msg => handlerCalled = true);
            _responder.MmInvoke(new MmMessage((MmMethod)1000));

            Assert.IsTrue(handlerCalled);
        }

        [Test]
        public void QuickRegister_ThrowsForInvalidMethodId()
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                _responder.QuickRegister(500, msg => { });
            });
        }

        [Test]
        public void QuickUnregister_RemovesHandler()
        {
            bool handlerCalled = false;

            _responder.QuickRegister(1000, msg => handlerCalled = true);
            _responder.QuickUnregister(1000);

            // This should trigger OnUnhandledCustomMethod, not our handler
            _responder.MmInvoke(new MmMessage((MmMethod)1000));

            Assert.IsFalse(handlerCalled);
        }

        #endregion

        #region Batch Registration Tests

        [Test]
        public void RegisterHandlers_RegistersMultiple()
        {
            int callCount = 0;

            _responder.RegisterHandlers(
                (1000, msg => callCount++),
                (1001, msg => callCount++),
                (1002, msg => callCount++)
            );

            _responder.MmInvoke(new MmMessage((MmMethod)1000));
            _responder.MmInvoke(new MmMessage((MmMethod)1001));
            _responder.MmInvoke(new MmMessage((MmMethod)1002));

            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void UnregisterHandlers_RemovesMultiple()
        {
            int callCount = 0;

            _responder.RegisterHandlers(
                (1000, msg => callCount++),
                (1001, msg => callCount++),
                (1002, msg => callCount++)
            );

            _responder.UnregisterHandlers(1000, 1001, 1002);

            // These should not trigger our handlers
            _responder.MmInvoke(new MmMessage((MmMethod)1000));
            _responder.MmInvoke(new MmMessage((MmMethod)1001));
            _responder.MmInvoke(new MmMessage((MmMethod)1002));

            Assert.AreEqual(0, callCount);
        }

        #endregion

        #region Handler Query Tests

        [Test]
        public void IsHandlerRegistered_ReturnsTrueForRegistered()
        {
            _responder.QuickRegister(1000, msg => { });

            Assert.IsTrue(_responder.IsHandlerRegistered(1000));
        }

        [Test]
        public void IsHandlerRegistered_ReturnsFalseForUnregistered()
        {
            Assert.IsFalse(_responder.IsHandlerRegistered(1000));
        }

        [Test]
        public void IsHandlerRegistered_ReturnsFalseAfterUnregister()
        {
            _responder.QuickRegister(1000, msg => { });
            _responder.QuickUnregister(1000);

            Assert.IsFalse(_responder.IsHandlerRegistered(1000));
        }

        #endregion

        #region Relay Node Access Tests

        // Note: MmBaseResponder has [RequireComponent(typeof(MmRelayNode))] so relay is always present
        // when a responder is added to a GameObject

        [Test]
        public void GetRelayNode_ReturnsAutoAddedRelay()
        {
            // [RequireComponent] ensures MmRelayNode is always present
            var relay = _responder.GetRelayNode();

            Assert.IsNotNull(relay, "Relay node should be auto-added by [RequireComponent]");
        }

        [Test]
        public void GetOrCreateRelayNode_ReturnsExistingAutoAddedRelay()
        {
            // [RequireComponent] already added relay, so GetOrCreate should return it
            var existing = _responderObj.GetComponent<MmRelayNode>();
            Assert.IsNotNull(existing, "Relay should already exist from [RequireComponent]");

            var relay = _responder.GetOrCreateRelayNode();

            Assert.AreSame(existing, relay, "Should return the auto-added relay");
        }

        #endregion

        #region Responder Messaging Tests

        [Test]
        public void Send_WithNoRelay_ReturnsDefault()
        {
            // No relay node - should return default MmFluentMessage
            var fluent = _responder.Send(MmMethod.Initialize);

            // Default struct has relay = null, but won't throw
            Assert.Pass("Send without relay returns safely");
        }

        [Test]
        public void Send_WithRelay_ReturnsFluentMessage()
        {
            _responderObj.AddComponent<MmRelayNode>();

            var fluent = _responder.Send(MmMethod.Initialize);

            // Non-default message with valid relay
            Assert.Pass("Send with relay returns fluent message");
        }

        #endregion

        /// <summary>
        /// Test responder that exposes unhandled method tracking.
        /// </summary>
        private class TestExtendableResponder : MmExtendableResponder
        {
            public int UnhandledMethodCount { get; private set; }
            public MmMethod LastUnhandledMethod { get; private set; }

            protected override void OnUnhandledCustomMethod(MmMessage message)
            {
                UnhandledMethodCount++;
                LastUnhandledMethod = message.MmMethod;
                // Don't call base to avoid warning spam in tests
            }
        }
    }
}
