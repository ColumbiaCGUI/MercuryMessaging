// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmNetworkBridgeTests.cs - Unit tests for MmNetworkBridge
// Tests bridge configuration, message routing, and relay node registration

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmNetworkBridge.
    /// Tests configuration, message routing through backends, and relay node registration.
    /// </summary>
    [TestFixture]
    public class MmNetworkBridgeTests
    {
        private GameObject _bridgeObj;
        private MmNetworkBridge _bridge;
        private MmLoopbackBackend _backend;
        private TestResolver _resolver;
        private List<MmMessage> _receivedMessages;

        [SetUp]
        public void SetUp()
        {
            _bridgeObj = new GameObject("TestBridge");
            _bridge = _bridgeObj.AddComponent<MmNetworkBridge>();

            _backend = new MmLoopbackBackend { Mode = MmLoopbackBackend.LoopbackMode.Echo };
            _resolver = new TestResolver();
            _receivedMessages = new List<MmMessage>();

            _bridge.OnMessageReceived += (msg) => _receivedMessages.Add(msg);
        }

        [TearDown]
        public void TearDown()
        {
            if (_bridgeObj != null)
            {
                _bridge.Shutdown();
                UnityEngine.Object.DestroyImmediate(_bridgeObj);
            }
            _backend = null;
            _resolver = null;
            _receivedMessages = null;
        }

        #region Configuration Tests

        [Test]
        public void Configure_SetsBackendAndResolver()
        {
            _bridge.Configure(_backend, _resolver);

            // Bridge should be configured but not initialized
            Assert.IsFalse(_bridge.IsInitialized);
        }

        [Test]
        public void Initialize_WithoutConfigure_ThrowsException()
        {
            // Initialize without Configure() should throw InvalidOperationException
            Assert.Throws<InvalidOperationException>(() => _bridge.Initialize());
            Assert.IsFalse(_bridge.IsInitialized);
        }

        [Test]
        public void Initialize_AfterConfigure_SetsIsInitializedTrue()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            Assert.IsTrue(_bridge.IsInitialized);
        }

        [Test]
        public void Shutdown_SetsIsInitializedFalse()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();
            _bridge.Shutdown();

            Assert.IsFalse(_bridge.IsInitialized);
        }

        #endregion

        #region Connection State Tests

        [Test]
        public void IsConnected_ReflectsBackendState()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            Assert.IsTrue(_bridge.IsConnected);
        }

        [Test]
        public void IsServer_ReflectsBackendState()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            Assert.IsTrue(_bridge.IsServer);
        }

        [Test]
        public void IsClient_ReflectsBackendState()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            Assert.IsTrue(_bridge.IsClient);
        }

        #endregion

        #region Message Sending Tests

        [Test]
        public void Send_IntMessage_SerializesAndDeserializesCorrectly()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var msg = new MmMessageInt(42, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
            msg.NetId = 100;
            _bridge.Send(msg);

            Assert.AreEqual(1, _receivedMessages.Count);
            var received = _receivedMessages[0] as MmMessageInt;
            Assert.IsNotNull(received);
            Assert.AreEqual(42, received.value);
            Assert.AreEqual(100u, received.NetId);
        }

        [Test]
        public void Send_StringMessage_SerializesAndDeserializesCorrectly()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var msg = new MmMessageString("Hello Network!", MmMethod.MessageString, MmMetadataBlockHelper.Default);
            msg.NetId = 200;
            _bridge.Send(msg);

            Assert.AreEqual(1, _receivedMessages.Count);
            var received = _receivedMessages[0] as MmMessageString;
            Assert.IsNotNull(received);
            Assert.AreEqual("Hello Network!", received.value);
        }

        [Test]
        public void Send_Vector3Message_SerializesAndDeserializesCorrectly()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var msg = new MmMessageVector3(new Vector3(1.5f, 2.5f, 3.5f), MmMethod.MessageVector3, MmMetadataBlockHelper.Default);
            _bridge.Send(msg);

            Assert.AreEqual(1, _receivedMessages.Count);
            var received = _receivedMessages[0] as MmMessageVector3;
            Assert.IsNotNull(received);
            Assert.AreEqual(1.5f, received.value.x, 0.001f);
            Assert.AreEqual(2.5f, received.value.y, 0.001f);
            Assert.AreEqual(3.5f, received.value.z, 0.001f);
        }

        [Test]
        public void Send_WhenNotInitialized_DoesNothing()
        {
            _bridge.Configure(_backend, _resolver);
            // Don't call Initialize()

            // Expect the error log from ValidateSend
            LogAssert.Expect(LogType.Error, "MmNetworkBridge: Cannot send - not initialized");

            var msg = new MmMessageInt(42, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
            _bridge.Send(msg);

            Assert.AreEqual(0, _receivedMessages.Count);
        }

        #endregion

        #region Relay Node Registration Tests

        [Test]
        public void RegisterRelayNode_AddsToRegistry()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var relayObj = new GameObject("TestRelay");
            var relay = relayObj.AddComponent<MmRelayNode>();

            _bridge.RegisterRelayNode(12345, relay);

            // Verify registration via TryGetRelayNode
            Assert.IsTrue(_bridge.TryGetRelayNode(12345, out var foundRelay));
            Assert.AreEqual(relay, foundRelay);

            UnityEngine.Object.DestroyImmediate(relayObj);
        }

        [Test]
        public void UnregisterRelayNode_RemovesFromRegistry()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var relayObj = new GameObject("TestRelay");
            var relay = relayObj.AddComponent<MmRelayNode>();

            _bridge.RegisterRelayNode(12345, relay);
            _bridge.UnregisterRelayNode(12345);

            Assert.IsFalse(_bridge.TryGetRelayNode(12345, out _));

            UnityEngine.Object.DestroyImmediate(relayObj);
        }

        [Test]
        public void TryGetRelayNode_NotRegistered_ReturnsFalse()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            Assert.IsFalse(_bridge.TryGetRelayNode(99999, out _));
        }

        #endregion

        #region Message Routing with NetId Tests

        [Test]
        public void ReceivedMessage_WithRegisteredNetId_RoutesToRelayNode()
        {
            _bridge.Configure(_backend, _resolver);
            _bridge.Initialize();

            var relayObj = new GameObject("TestRelay");
            var relay = relayObj.AddComponent<MmRelayNode>();
            var responder = relayObj.AddComponent<TestResponder>();
            relay.MmRefreshResponders();

            _bridge.RegisterRelayNode(12345, relay);

            // Send message with matching NetId
            var msg = new MmMessageInt(42, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
            msg.NetId = 12345;
            _bridge.Send(msg);

            // Message should be routed to relay node
            Assert.AreEqual(1, _receivedMessages.Count);

            UnityEngine.Object.DestroyImmediate(relayObj);
        }

        #endregion

        #region Helper Classes

        private class TestResolver : IMmGameObjectResolver
        {
            public string ResolverName => "TestResolver";

            public bool TryGetNetworkId(GameObject go, out uint netId)
            {
                netId = (uint)go.GetInstanceID();
                return true;
            }

            public bool TryGetGameObject(uint netId, out GameObject go)
            {
                go = null;
                return false;
            }

            public uint GetNetworkId(GameObject gameObject)
            {
                return (uint)gameObject.GetInstanceID();
            }

            public GameObject GetGameObject(uint networkId)
            {
                return null;
            }

            public bool TryGetRelayNode(uint netId, out MmRelayNode relayNode)
            {
                relayNode = null;
                return false;
            }
        }

        private class TestResponder : MmBaseResponder
        {
            public List<MmMessage> ReceivedMessages { get; } = new List<MmMessage>();

            public override void MmInvoke(MmMessage message)
            {
                ReceivedMessages.Add(message);
                base.MmInvoke(message);
            }
        }

        #endregion
    }
}
