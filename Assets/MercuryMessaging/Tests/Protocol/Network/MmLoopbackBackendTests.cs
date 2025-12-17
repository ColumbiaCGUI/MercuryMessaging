// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmLoopbackBackendTests.cs - Unit tests for MmLoopbackBackend
// Tests loopback modes, message delivery, queuing, and connection events

using System;
using System.Collections.Generic;
using NUnit.Framework;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmLoopbackBackend.
    /// Tests all operating modes, message delivery, queuing, and connection events.
    /// </summary>
    [TestFixture]
    public class MmLoopbackBackendTests
    {
        private MmLoopbackBackend _backend;
        private List<(byte[] data, int senderId)> _receivedMessages;

        [SetUp]
        public void SetUp()
        {
            _backend = new MmLoopbackBackend();
            _receivedMessages = new List<(byte[], int)>();
            _backend.OnMessageReceived += (data, senderId) =>
            {
                _receivedMessages.Add((data, senderId));
            };
        }

        [TearDown]
        public void TearDown()
        {
            _backend?.Shutdown();
            _backend = null;
            _receivedMessages = null;
        }

        #region Initialization Tests

        [Test]
        public void Initialize_SetsIsConnectedTrue()
        {
            _backend.Initialize();
            Assert.IsTrue(_backend.IsConnected);
        }

        [Test]
        public void BeforeInitialize_IsConnectedFalse()
        {
            Assert.IsFalse(_backend.IsConnected);
        }

        [Test]
        public void Shutdown_SetsIsConnectedFalse()
        {
            _backend.Initialize();
            _backend.Shutdown();
            Assert.IsFalse(_backend.IsConnected);
        }

        [Test]
        public void BackendName_ReturnsLoopback()
        {
            Assert.AreEqual("Loopback", _backend.BackendName);
        }

        #endregion

        #region Echo Mode Tests

        [Test]
        public void EchoMode_SendToServer_EchoesBack()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Echo;
            _backend.Initialize();

            byte[] data = { 0x01, 0x02, 0x03 };
            _backend.SendToServer(data);

            Assert.AreEqual(1, _receivedMessages.Count);
            Assert.AreEqual(data, _receivedMessages[0].data);
        }

        [Test]
        public void EchoMode_SendToAllClients_EchoesBack()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Echo;
            _backend.Initialize();

            byte[] data = { 0x04, 0x05, 0x06 };
            _backend.SendToAllClients(data);

            Assert.AreEqual(1, _receivedMessages.Count);
            Assert.AreEqual(data, _receivedMessages[0].data);
        }

        [Test]
        public void EchoMode_SendToClient_EchoesBack()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Echo;
            _backend.Initialize();

            byte[] data = { 0x07, 0x08, 0x09 };
            _backend.SendToClient(1, data);

            Assert.AreEqual(1, _receivedMessages.Count);
            Assert.AreEqual(data, _receivedMessages[0].data);
        }

        [Test]
        public void EchoMode_IsServerAndIsClientBothTrue()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Echo;
            _backend.Initialize();

            Assert.IsTrue(_backend.IsServer);
            Assert.IsTrue(_backend.IsClient);
        }

        #endregion

        #region Server Mode Tests

        [Test]
        public void ServerMode_SendToServer_DoesNotEcho()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.Initialize();

            byte[] data = { 0x01, 0x02 };
            _backend.SendToServer(data);

            Assert.AreEqual(0, _receivedMessages.Count);
        }

        [Test]
        public void ServerMode_SendToAllClients_EchoesBack()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.Initialize();

            byte[] data = { 0x03, 0x04 };
            _backend.SendToAllClients(data);

            Assert.AreEqual(1, _receivedMessages.Count);
        }

        [Test]
        public void ServerMode_IsServerTrue_IsClientFalse()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.Initialize();

            Assert.IsTrue(_backend.IsServer);
            Assert.IsFalse(_backend.IsClient);
        }

        [Test]
        public void ServerMode_LocalClientIdIsZero()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            Assert.AreEqual(0, _backend.LocalClientId);
        }

        #endregion

        #region Client Mode Tests

        [Test]
        public void ClientMode_SendToServer_EchoesBack()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            _backend.Initialize();

            byte[] data = { 0x01, 0x02 };
            _backend.SendToServer(data);

            Assert.AreEqual(1, _receivedMessages.Count);
        }

        [Test]
        public void ClientMode_SendToAllClients_DoesNotEcho()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            _backend.Initialize();

            byte[] data = { 0x03, 0x04 };
            _backend.SendToAllClients(data);

            Assert.AreEqual(0, _receivedMessages.Count);
        }

        [Test]
        public void ClientMode_IsServerFalse_IsClientTrue()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            _backend.Initialize();

            Assert.IsFalse(_backend.IsServer);
            Assert.IsTrue(_backend.IsClient);
        }

        [Test]
        public void ClientMode_LocalClientIdIsOne()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            Assert.AreEqual(1, _backend.LocalClientId);
        }

        #endregion

        #region Connection Event Tests

        [Test]
        public void ClientMode_Initialize_FiresOnConnectedToServer()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            bool connected = false;
            _backend.OnConnectedToServer += () => connected = true;

            _backend.Initialize();

            Assert.IsTrue(connected);
        }

        [Test]
        public void ClientMode_Shutdown_FiresOnDisconnectedFromServer()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Client;
            bool disconnected = false;
            _backend.OnDisconnectedFromServer += () => disconnected = true;

            _backend.Initialize();
            _backend.Shutdown();

            Assert.IsTrue(disconnected);
        }

        [Test]
        public void ServerMode_Initialize_FiresOnClientConnected()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            int connectedClientId = -1;
            _backend.OnClientConnected += (id) => connectedClientId = id;

            _backend.Initialize();

            Assert.AreEqual(1, connectedClientId);
        }

        [Test]
        public void ServerMode_Shutdown_FiresOnClientDisconnected()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            int disconnectedClientId = -1;
            _backend.OnClientDisconnected += (id) => disconnectedClientId = id;

            _backend.Initialize();
            _backend.Shutdown();

            Assert.AreEqual(1, disconnectedClientId);
        }

        #endregion

        #region Message Queue Tests

        [Test]
        public void UseMessageQueue_True_DelaysDelivery()
        {
            _backend.UseMessageQueue = true;
            _backend.Initialize();

            byte[] data = { 0x01 };
            _backend.SendToServer(data);

            // Message should be queued, not delivered
            Assert.AreEqual(0, _receivedMessages.Count);
            Assert.AreEqual(1, _backend.PendingMessageCount);
        }

        [Test]
        public void ProcessPendingMessages_DeliversQueuedMessages()
        {
            _backend.UseMessageQueue = true;
            _backend.Initialize();

            byte[] data = { 0x01 };
            _backend.SendToServer(data);

            Assert.AreEqual(0, _receivedMessages.Count);

            _backend.ProcessPendingMessages();

            Assert.AreEqual(1, _receivedMessages.Count);
            Assert.AreEqual(0, _backend.PendingMessageCount);
        }

        [Test]
        public void FlushMessages_DeliversAllImmediately()
        {
            _backend.UseMessageQueue = true;
            _backend.SimulatedLatencyMs = 10000; // High latency
            _backend.Initialize();

            _backend.SendToServer(new byte[] { 0x01 });
            _backend.SendToServer(new byte[] { 0x02 });
            _backend.SendToServer(new byte[] { 0x03 });

            Assert.AreEqual(0, _receivedMessages.Count);

            _backend.FlushMessages();

            Assert.AreEqual(3, _receivedMessages.Count);
        }

        #endregion

        #region Not Initialized Tests

        [Test]
        public void SendToServer_WhenNotInitialized_DoesNothing()
        {
            // Don't call Initialize()
            byte[] data = { 0x01 };
            _backend.SendToServer(data);

            Assert.AreEqual(0, _receivedMessages.Count);
        }

        [Test]
        public void SendToAllClients_WhenNotInitialized_DoesNothing()
        {
            byte[] data = { 0x01 };
            _backend.SendToAllClients(data);

            Assert.AreEqual(0, _receivedMessages.Count);
        }

        #endregion

        #region Reset and Helper Tests

        [Test]
        public void Reset_RestoresDefaultState()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.UseMessageQueue = true;
            _backend.SimulatedLatencyMs = 100;
            _backend.RecordSentMessages = true;
            _backend.Initialize();

            _backend.Reset();

            Assert.IsFalse(_backend.IsConnected);
            Assert.AreEqual(MmLoopbackBackend.LoopbackMode.Echo, _backend.Mode);
            Assert.IsFalse(_backend.UseMessageQueue);
            Assert.AreEqual(0, _backend.SimulatedLatencyMs);
            Assert.IsFalse(_backend.RecordSentMessages);
        }

        [Test]
        public void SendToOtherClients_ExcludesSpecifiedClient()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.Initialize();

            // Exclude client 2 (which is not LocalClientId=0), so message should be delivered
            byte[] data = { 0x01 };
            _backend.SendToOtherClients(2, data);

            Assert.AreEqual(1, _receivedMessages.Count);
        }

        [Test]
        public void SendToOtherClients_DoesNotDeliverToExcludedClient()
        {
            _backend.Mode = MmLoopbackBackend.LoopbackMode.Server;
            _backend.Initialize();

            // Exclude LocalClientId=0 means message should not be delivered
            byte[] data = { 0x01 };
            _backend.SendToOtherClients(0, data);

            // SendToOtherClients checks excludeClientId != LocalClientId
            // LocalClientId in Server mode is 0, so excluding 0 should not deliver
            Assert.AreEqual(0, _receivedMessages.Count);
        }

        #endregion

        #region Reliability Parameter Tests

        [Test]
        public void SendToServer_WithReliable_Delivers()
        {
            _backend.Initialize();

            byte[] data = { 0x01 };
            _backend.SendToServer(data, MmReliability.Reliable);

            Assert.AreEqual(1, _receivedMessages.Count);
        }

        [Test]
        public void SendToServer_WithUnreliable_Delivers()
        {
            _backend.Initialize();

            byte[] data = { 0x01 };
            _backend.SendToServer(data, MmReliability.Unreliable);

            // Loopback always delivers (reliability is a hint for real backends)
            Assert.AreEqual(1, _receivedMessages.Count);
        }

        #endregion
    }
}
