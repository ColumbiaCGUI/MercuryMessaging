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

#if FISHNET_AVAILABLE
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Transporting;
#endif

namespace MercuryMessaging.Network.Backends
{
#if FISHNET_AVAILABLE
    /// <summary>
    /// Broadcast struct for MercuryMessaging binary messages.
    /// Uses a single byte[] payload that is serialized by MmBinarySerializer.
    /// </summary>
    public struct MmBroadcast : IBroadcast
    {
        public byte[] Data;
    }
#endif

    /// <summary>
    /// IMmNetworkBackend implementation for FishNet.
    ///
    /// This backend uses FishNet's Broadcast system to provide binary message
    /// transport compatible with the MercuryMessaging networking architecture.
    ///
    /// FishNet Broadcasts are preferred over RPCs because:
    /// - They don't require NetworkObject components
    /// - They work globally (like PUN2's RaiseEvent)
    /// - They support byte[] payloads directly
    /// </summary>
    /// <remarks>
    /// To enable this backend:
    /// 1. Install FishNet via Package Manager
    /// 2. Add FISHNET_AVAILABLE to Scripting Define Symbols
    /// 3. Add FishNet.Runtime to MercuryMessaging.asmdef references
    /// </remarks>
    public class FishNetBackend : IMmNetworkBackend
    {
        #region State

        private bool _isInitialized;

#if FISHNET_AVAILABLE
        /// <summary>
        /// ClientId → NetworkConnection lookup for targeted sends (server-side only).
        /// </summary>
        private readonly Dictionary<int, NetworkConnection> _clientConnections
            = new Dictionary<int, NetworkConnection>();
#endif

        #endregion

        #region IMmNetworkBackend Properties

#if FISHNET_AVAILABLE
        /// <inheritdoc/>
        public bool IsConnected => InstanceFinder.IsServerStarted || InstanceFinder.IsClientStarted;

        /// <inheritdoc/>
        public bool IsServer => InstanceFinder.IsServerStarted;

        /// <inheritdoc/>
        public bool IsClient => InstanceFinder.IsClientStarted;

        /// <inheritdoc/>
        public int LocalClientId
        {
            get
            {
                if (InstanceFinder.ClientManager != null &&
                    InstanceFinder.ClientManager.Connection != null)
                {
                    return InstanceFinder.ClientManager.Connection.ClientId;
                }
                return -1;
            }
        }
#else
        /// <inheritdoc/>
        public bool IsConnected => false;

        /// <inheritdoc/>
        public bool IsServer => false;

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public int LocalClientId => -1;
#endif

        /// <inheritdoc/>
        public string BackendName => "FishNet";

        #endregion

        #region Events

        /// <inheritdoc/>
        public event MmNetworkMessageReceived OnMessageReceived;

        /// <inheritdoc/>
        public event MmNetworkConnectionChanged OnClientConnected;

        /// <inheritdoc/>
        public event MmNetworkConnectionChanged OnClientDisconnected;

        /// <inheritdoc/>
        public event Action OnConnectedToServer;

        /// <inheritdoc/>
        public event Action OnDisconnectedFromServer;

        #endregion

        #region Lifecycle

        /// <inheritdoc/>
        public void Initialize()
        {
            if (_isInitialized) return;

#if FISHNET_AVAILABLE
            // Register broadcast handlers (receives messages)
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.RegisterBroadcast<MmBroadcast>(
                    OnServerReceivedBroadcast);
                InstanceFinder.ServerManager.OnRemoteConnectionState +=
                    OnRemoteConnectionState;
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.RegisterBroadcast<MmBroadcast>(
                    OnClientReceivedBroadcast);
                InstanceFinder.ClientManager.OnClientConnectionState +=
                    OnClientConnectionState;
            }
#endif

            _isInitialized = true;
            MmLog.LogNetwork("FishNetBackend initialized");
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_isInitialized) return;

#if FISHNET_AVAILABLE
            // Unregister broadcast handlers
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.UnregisterBroadcast<MmBroadcast>(
                    OnServerReceivedBroadcast);
                InstanceFinder.ServerManager.OnRemoteConnectionState -=
                    OnRemoteConnectionState;
            }

            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.UnregisterBroadcast<MmBroadcast>(
                    OnClientReceivedBroadcast);
                InstanceFinder.ClientManager.OnClientConnectionState -=
                    OnClientConnectionState;
            }

            _clientConnections.Clear();
#endif

            _isInitialized = false;
            MmLog.LogNetwork("FishNetBackend shutdown");
        }

        #endregion

        #region Sending

        /// <inheritdoc/>
        public void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FISHNET_AVAILABLE
            if (!IsConnected || !IsClient)
            {
                MmLog.LogWarning("FishNetBackend: Cannot send to server - not connected as client");
                return;
            }

            var msg = new MmBroadcast { Data = data };
            var channel = ToChannel(reliability);
            InstanceFinder.ClientManager.Broadcast(msg, channel);
#else
            MmLog.LogWarning("FishNetBackend: FishNet not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FISHNET_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("FishNetBackend: Cannot send to clients - not running as server");
                return;
            }

            var msg = new MmBroadcast { Data = data };
            var channel = ToChannel(reliability);
            InstanceFinder.ServerManager.Broadcast(msg, true, channel);
#else
            MmLog.LogWarning("FishNetBackend: FishNet not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FISHNET_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("FishNetBackend: Cannot send to client - not running as server");
                return;
            }

            if (!_clientConnections.TryGetValue(clientId, out NetworkConnection conn))
            {
                MmLog.LogWarning($"FishNetBackend: Client {clientId} not found in connection list");
                return;
            }

            var msg = new MmBroadcast { Data = data };
            var channel = ToChannel(reliability);
            InstanceFinder.ServerManager.Broadcast(conn, msg, true, channel);
#else
            MmLog.LogWarning("FishNetBackend: FishNet not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FISHNET_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("FishNetBackend: Cannot send to clients - not running as server");
                return;
            }

            if (!_clientConnections.TryGetValue(excludeClientId, out NetworkConnection excludeConn))
            {
                // If excluded client not found, just broadcast to all
                SendToAllClients(data, reliability);
                return;
            }

            var msg = new MmBroadcast { Data = data };
            var channel = ToChannel(reliability);
            InstanceFinder.ServerManager.BroadcastExcept(excludeConn, msg, true, channel);
#else
            MmLog.LogWarning("FishNetBackend: FishNet not available");
#endif
        }

        #endregion

        #region Receiving

#if FISHNET_AVAILABLE
        /// <summary>
        /// Called when the server receives a broadcast from a client.
        /// </summary>
        private void OnServerReceivedBroadcast(NetworkConnection conn, MmBroadcast msg, Channel channel)
        {
            if (msg.Data == null || msg.Data.Length == 0)
            {
                MmLog.LogWarning("FishNetBackend: Received broadcast with null/empty data");
                return;
            }

            OnMessageReceived?.Invoke(msg.Data, conn.ClientId);
        }

        /// <summary>
        /// Called when the client receives a broadcast from the server.
        /// </summary>
        private void OnClientReceivedBroadcast(MmBroadcast msg, Channel channel)
        {
            if (msg.Data == null || msg.Data.Length == 0)
            {
                MmLog.LogWarning("FishNetBackend: Received broadcast with null/empty data");
                return;
            }

            // -1 indicates message is from server
            OnMessageReceived?.Invoke(msg.Data, -1);
        }
#endif

        #endregion

        #region Connection Events

#if FISHNET_AVAILABLE
        /// <summary>
        /// Called when a remote client connects or disconnects (server-side).
        /// </summary>
        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                _clientConnections[conn.ClientId] = conn;
                MmLog.LogNetwork($"FishNetBackend: Client {conn.ClientId} connected");
                OnClientConnected?.Invoke(conn.ClientId);
            }
            else if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                _clientConnections.Remove(conn.ClientId);
                MmLog.LogNetwork($"FishNetBackend: Client {conn.ClientId} disconnected");
                OnClientDisconnected?.Invoke(conn.ClientId);
            }
        }

        /// <summary>
        /// Called when the local client connection state changes.
        /// </summary>
        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                MmLog.LogNetwork("FishNetBackend: Connected to server");
                OnConnectedToServer?.Invoke();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                MmLog.LogNetwork("FishNetBackend: Disconnected from server");
                OnDisconnectedFromServer?.Invoke();
            }
        }
#endif

        #endregion

        #region Helpers

#if FISHNET_AVAILABLE
        /// <summary>
        /// Convert MmReliability to FishNet Channel.
        /// </summary>
        private static Channel ToChannel(MmReliability reliability)
        {
            return reliability == MmReliability.Reliable
                ? Channel.Reliable
                : Channel.Unreliable;
        }
#endif

        #endregion
    }
}
