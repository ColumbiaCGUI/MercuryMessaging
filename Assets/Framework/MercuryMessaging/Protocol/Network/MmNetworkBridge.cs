// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//  * Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
// =============================================================
// Authors:
// Carmine Elvezio, Mengu Sukan, Steven Feiner, [Contributors]
// =============================================================
//

using System;
using System.Collections.Generic;
using UnityEngine;
using MmLog = MercuryMessaging.MmLogger;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Central orchestrator for MercuryMessaging network communication.
    ///
    /// MmNetworkBridge:
    /// - Manages the active network backend (FishNet, Fusion, PUN2, etc.)
    /// - Handles message serialization/deserialization via MmBinarySerializer
    /// - Resolves network IDs to GameObjects via IMmGameObjectResolver
    /// - Routes incoming network messages to the correct MmRelayNode
    ///
    /// Usage:
    /// 1. Set Backend and Resolver before connecting
    /// 2. Call Initialize() to start
    /// 3. Use Send() methods to transmit messages
    /// 4. Received messages are automatically routed to MmRelayNodes
    /// </summary>
    public class MmNetworkBridge : MonoBehaviour
    {
        #region Singleton

        private static MmNetworkBridge _instance;

        /// <summary>
        /// Singleton instance of the network bridge.
        /// </summary>
        public static MmNetworkBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MmNetworkBridge>();
                    if (_instance == null)
                    {
                        var go = new GameObject("MmNetworkBridge");
                        _instance = go.AddComponent<MmNetworkBridge>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// The active network backend.
        /// </summary>
        public IMmNetworkBackend Backend { get; private set; }

        /// <summary>
        /// The active GameObject resolver.
        /// </summary>
        public IMmGameObjectResolver Resolver { get; private set; }

        /// <summary>
        /// Whether the bridge has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Registry of MmRelayNodes by their network IDs.
        /// </summary>
        private readonly Dictionary<uint, MmRelayNode> _relayNodeRegistry = new Dictionary<uint, MmRelayNode>();

        #endregion

        #region Events

        /// <summary>
        /// Fired when a message is received and deserialized (before routing).
        /// Useful for logging and debugging.
        /// </summary>
        public event Action<MmMessage> OnMessageReceived;

        /// <summary>
        /// Fired when a message is sent.
        /// </summary>
        public event Action<MmMessage> OnMessageSent;

        #endregion

        #region Initialization

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Configure the network bridge with a backend and resolver.
        /// </summary>
        /// <param name="backend">Network transport backend</param>
        /// <param name="resolver">GameObject ID resolver</param>
        public void Configure(IMmNetworkBackend backend, IMmGameObjectResolver resolver)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("MmNetworkBridge: Reconfiguring while initialized. Call Shutdown() first.");
                Shutdown();
            }

            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

            MmLog.LogNetwork($"MmNetworkBridge configured with {backend.BackendName} and {resolver.ResolverName}");
        }

        /// <summary>
        /// Initialize the network bridge and start listening for messages.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            if (Backend == null || Resolver == null)
            {
                throw new InvalidOperationException("MmNetworkBridge: Must call Configure() before Initialize()");
            }

            Backend.OnMessageReceived += HandleReceivedMessage;
            Backend.Initialize();

            IsInitialized = true;
            MmLog.LogNetwork("MmNetworkBridge initialized");
        }

        /// <summary>
        /// Shutdown the network bridge and clean up resources.
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized) return;

            if (Backend != null)
            {
                Backend.OnMessageReceived -= HandleReceivedMessage;
                Backend.Shutdown();
            }

            _relayNodeRegistry.Clear();
            IsInitialized = false;
            MmLog.LogNetwork("MmNetworkBridge shutdown");
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        #endregion

        #region Registration

        /// <summary>
        /// Register an MmRelayNode with its network ID for message routing.
        /// </summary>
        /// <param name="networkId">The network ID of the relay node</param>
        /// <param name="relayNode">The relay node to register</param>
        public void RegisterRelayNode(uint networkId, MmRelayNode relayNode)
        {
            if (relayNode == null) return;

            if (_relayNodeRegistry.ContainsKey(networkId))
            {
                MmLog.LogNetwork($"MmNetworkBridge: Updating registration for network ID {networkId}");
            }

            _relayNodeRegistry[networkId] = relayNode;
            MmLog.LogNetwork($"MmNetworkBridge: Registered {relayNode.gameObject.name} with network ID {networkId}");
        }

        /// <summary>
        /// Unregister an MmRelayNode.
        /// </summary>
        /// <param name="networkId">The network ID to unregister</param>
        public void UnregisterRelayNode(uint networkId)
        {
            if (_relayNodeRegistry.Remove(networkId))
            {
                MmLog.LogNetwork($"MmNetworkBridge: Unregistered network ID {networkId}");
            }
        }

        /// <summary>
        /// Try to get a registered relay node by network ID.
        /// </summary>
        public bool TryGetRelayNode(uint networkId, out MmRelayNode relayNode)
        {
            return _relayNodeRegistry.TryGetValue(networkId, out relayNode);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Send a message to all connected clients (server-side).
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="reliability">Delivery guarantee</param>
        public void SendToAllClients(MmMessage message, MmReliability reliability = MmReliability.Reliable)
        {
            if (!ValidateSend(message)) return;

            PrepareMessageForSend(message);
            byte[] data = MmBinarySerializer.Serialize(message);
            Backend.SendToAllClients(data, reliability);

            OnMessageSent?.Invoke(message);
            MmLog.LogNetwork($"MmNetworkBridge: Sent {message.MmMessageType} to all clients");
        }

        /// <summary>
        /// Send a message to the server (client-side).
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="reliability">Delivery guarantee</param>
        public void SendToServer(MmMessage message, MmReliability reliability = MmReliability.Reliable)
        {
            if (!ValidateSend(message)) return;

            PrepareMessageForSend(message);
            byte[] data = MmBinarySerializer.Serialize(message);
            Backend.SendToServer(data, reliability);

            OnMessageSent?.Invoke(message);
            MmLog.LogNetwork($"MmNetworkBridge: Sent {message.MmMessageType} to server");
        }

        /// <summary>
        /// Send a message to a specific client (server-side).
        /// </summary>
        /// <param name="clientId">Target client ID</param>
        /// <param name="message">The message to send</param>
        /// <param name="reliability">Delivery guarantee</param>
        public void SendToClient(int clientId, MmMessage message, MmReliability reliability = MmReliability.Reliable)
        {
            if (!ValidateSend(message)) return;

            PrepareMessageForSend(message);
            byte[] data = MmBinarySerializer.Serialize(message);
            Backend.SendToClient(clientId, data, reliability);

            OnMessageSent?.Invoke(message);
            MmLog.LogNetwork($"MmNetworkBridge: Sent {message.MmMessageType} to client {clientId}");
        }

        /// <summary>
        /// Send a message using the appropriate method based on server/client status.
        /// Clients send to server, server broadcasts to all clients.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="reliability">Delivery guarantee</param>
        public void Send(MmMessage message, MmReliability reliability = MmReliability.Reliable)
        {
            if (Backend.IsServer)
            {
                SendToAllClients(message, reliability);
            }
            else
            {
                SendToServer(message, reliability);
            }
        }

        private bool ValidateSend(MmMessage message)
        {
            if (!IsInitialized)
            {
                MmLog.LogError("MmNetworkBridge: Cannot send - not initialized");
                return false;
            }
            if (!Backend.IsConnected)
            {
                MmLog.LogError("MmNetworkBridge: Cannot send - not connected");
                return false;
            }
            if (message == null)
            {
                MmLog.LogError("MmNetworkBridge: Cannot send null message");
                return false;
            }
            return true;
        }

        private void PrepareMessageForSend(MmMessage message)
        {
            // If this is a GameObject message, resolve the network ID
            if (message is MmMessageGameObject goMsg && goMsg.Value != null)
            {
                if (Resolver.TryGetNetworkId(goMsg.Value, out uint netId))
                {
                    goMsg.GameObjectNetId = netId;
                }
                else
                {
                    MmLog.LogWarning($"MmNetworkBridge: Could not resolve network ID for {goMsg.Value.name}");
                }
            }
        }

        #endregion

        #region Receiving

        private void HandleReceivedMessage(byte[] data, int senderId)
        {
            try
            {
                MmMessage message = MmBinarySerializer.Deserialize(data);

                // Resolve GameObject references
                ResolveGameObjectReferences(message);

                OnMessageReceived?.Invoke(message);

                // Route to the appropriate MmRelayNode
                RouteMessage(message);
            }
            catch (Exception e)
            {
                MmLog.LogError($"MmNetworkBridge: Failed to process received message: {e.Message}");
            }
        }

        private void ResolveGameObjectReferences(MmMessage message)
        {
            // Resolve GameObject message values
            if (message is MmMessageGameObject goMsg && goMsg.GameObjectNetId != 0)
            {
                goMsg.Value = Resolver.GetGameObject(goMsg.GameObjectNetId);
            }
        }

        private void RouteMessage(MmMessage message)
        {
            // Find the target relay node by NetId
            if (message.NetId == 0)
            {
                MmLog.LogWarning("MmNetworkBridge: Received message with no target NetId");
                return;
            }

            if (TryGetRelayNode(message.NetId, out MmRelayNode relayNode))
            {
                // Mark as deserialized so the relay node knows this came from network
                relayNode.MmInvoke(message);
                MmLog.LogNetwork($"MmNetworkBridge: Routed {message.MmMessageType} to {relayNode.gameObject.name}");
            }
            else if (Resolver.TryGetRelayNode(message.NetId, out relayNode))
            {
                // Fallback: try to find via resolver
                RegisterRelayNode(message.NetId, relayNode);
                relayNode.MmInvoke(message);
                MmLog.LogNetwork($"MmNetworkBridge: Routed {message.MmMessageType} to {relayNode.gameObject.name} (via resolver)");
            }
            else
            {
                MmLog.LogWarning($"MmNetworkBridge: No relay node found for network ID {message.NetId}");
            }
        }

        #endregion

        #region Convenience Properties

        /// <summary>
        /// Whether connected to the network.
        /// </summary>
        public bool IsConnected => Backend?.IsConnected ?? false;

        /// <summary>
        /// Whether this instance is the server/host.
        /// </summary>
        public bool IsServer => Backend?.IsServer ?? false;

        /// <summary>
        /// Whether this instance is a client.
        /// </summary>
        public bool IsClient => Backend?.IsClient ?? false;

        /// <summary>
        /// Name of the active backend.
        /// </summary>
        public string BackendName => Backend?.BackendName ?? "None";

        #endregion
    }
}
