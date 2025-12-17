// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// NetworkExtensionsTests.cs - Tests for Network DSL Extensions
// Part of DSL Overhaul Phase 5

using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for MmNetworkExtensions and network-related fluent API.
    /// Tests cover network state queries, conditional execution, and message origin detection.
    /// </summary>
    [TestFixture]
    public class NetworkExtensionsTests
    {
        private GameObject _relayObj;
        private MmRelayNode _relay;
        private GameObject _responderObj;
        private TestNetworkResponder _responder;

        [SetUp]
        public void SetUp()
        {
            _relayObj = new GameObject("TestRelay");
            _relay = _relayObj.AddComponent<MmRelayNode>();

            _responderObj = new GameObject("TestResponder");
            _responderObj.transform.SetParent(_relayObj.transform);
            _responder = _responderObj.AddComponent<TestNetworkResponder>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_relayObj != null)
                Object.DestroyImmediate(_relayObj);
        }

        #region Network State Query Tests

        [Test]
        public void HasNetworkSupport_WithoutNetworkResponder_ReturnsFalse()
        {
            Assert.IsFalse(_relay.HasNetworkSupport());
        }

        [Test]
        public void IsHost_WithoutNetworkResponder_ReturnsFalse()
        {
            Assert.IsFalse(_relay.IsHost());
        }

        [Test]
        public void IsClientOnly_WithoutNetworkResponder_ReturnsFalse()
        {
            Assert.IsFalse(_relay.IsClientOnly());
        }

        [Test]
        public void IsServerOnly_WithoutNetworkResponder_ReturnsFalse()
        {
            Assert.IsFalse(_relay.IsServerOnly());
        }

        #endregion

        #region Message Origin Tests

        [Test]
        public void IsNetworkMessage_WithNullMessage_ReturnsFalse()
        {
            Assert.IsFalse(_relay.IsNetworkMessage(null));
        }

        [Test]
        public void IsNetworkMessage_WithLocalMessage_ReturnsFalse()
        {
            var message = new MmMessage(MmMethod.Initialize);
            Assert.IsFalse(_relay.IsNetworkMessage(message));
        }

        [Test]
        public void IsNetworkMessage_Responder_WithNullMessage_ReturnsFalse()
        {
            Assert.IsFalse(_responder.IsNetworkMessage(null));
        }

        [Test]
        public void IsNetworkMessage_Responder_WithLocalMessage_ReturnsFalse()
        {
            var message = new MmMessage(MmMethod.Initialize);
            Assert.IsFalse(_responder.IsNetworkMessage(message));
        }

        #endregion

        #region Conditional Execution Tests

        [Test]
        public void IfFromNetwork_WithLocalMessage_DoesNotExecute()
        {
            var message = new MmMessage(MmMethod.Initialize);
            bool executed = false;

            _relay.IfFromNetwork(message, () => executed = true);

            Assert.IsFalse(executed);
        }

        [Test]
        public void IfLocal_WithLocalMessage_Executes()
        {
            var message = new MmMessage(MmMethod.Initialize);
            bool executed = false;

            _relay.IfLocal(message, () => executed = true);

            Assert.IsTrue(executed);
        }

        [Test]
        public void IfHost_WithoutNetworkResponder_DoesNotExecute()
        {
            bool executed = false;

            _relay.IfHost(() => executed = true);

            Assert.IsFalse(executed);
        }

        [Test]
        public void IfClient_WithoutNetworkResponder_DoesNotExecute()
        {
            bool executed = false;

            _relay.IfClient(() => executed = true);

            Assert.IsFalse(executed);
        }

        [Test]
        public void IfServer_WithoutNetworkResponder_DoesNotExecute()
        {
            bool executed = false;

            _relay.IfServer(() => executed = true);

            Assert.IsFalse(executed);
        }

        [Test]
        public void Responder_IfFromNetwork_WithLocalMessage_DoesNotExecute()
        {
            var message = new MmMessage(MmMethod.Initialize);
            bool executed = false;

            _responder.IfFromNetwork(message, () => executed = true);

            Assert.IsFalse(executed);
        }

        [Test]
        public void Responder_IfLocal_WithLocalMessage_Executes()
        {
            var message = new MmMessage(MmMethod.Initialize);
            bool executed = false;

            _responder.IfLocal(message, () => executed = true);

            Assert.IsTrue(executed);
        }

        #endregion

        #region Fluent Network Methods Tests

        [Test]
        public void SendNetworked_Method_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked(MmMethod.Initialize);

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void SendNetworked_String_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked("test");

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void SendNetworked_Int_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked(42);

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void SendNetworked_Float_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked(3.14f);

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void SendNetworked_Bool_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked(true);

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void SendNetworked_Vector3_ReturnsFluentMessage()
        {
            var fluent = _relay.SendNetworked(Vector3.up);

            Assert.IsNotNull(fluent);
        }

        #endregion

        #region MmFluentMessage Network Methods Tests

        [Test]
        public void OverNetwork_ReturnsFluentMessage()
        {
            var fluent = _relay.Send(MmMethod.Initialize).OverNetwork();

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void RemoteOnly_ReturnsFluentMessage()
        {
            var fluent = _relay.Send(MmMethod.Initialize).RemoteOnly();

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void LocalOnly_ReturnsFluentMessage()
        {
            var fluent = _relay.Send(MmMethod.Initialize).LocalOnly();

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void NetworkOnly_ReturnsFluentMessage()
        {
            var fluent = _relay.Send(MmMethod.Initialize).NetworkOnly();

            Assert.IsNotNull(fluent);
        }

        [Test]
        public void AllDestinations_ReturnsFluentMessage()
        {
            var fluent = _relay.Send(MmMethod.Initialize).AllDestinations();

            Assert.IsNotNull(fluent);
        }

        #endregion

        /// <summary>
        /// Test responder for network extension tests.
        /// </summary>
        private class TestNetworkResponder : MmBaseResponder
        {
            public MmMessage LastMessage { get; private set; }

            public override void MmInvoke(MmMessage message)
            {
                LastMessage = message;
                base.MmInvoke(message);
            }
        }
    }
}
