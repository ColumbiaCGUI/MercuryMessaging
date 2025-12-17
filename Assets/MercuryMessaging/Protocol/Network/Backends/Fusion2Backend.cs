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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Steven Feiner, [Contributors]
// =============================================================
//

using System;
using System.Collections.Generic;
using UnityEngine;
using MmLog = MercuryMessaging.MmLogger;

// Suppress CS0067: Events may not be invoked when FUSION2_AVAILABLE is not defined
#pragma warning disable 0067

#if FUSION2_AVAILABLE
using Fusion;
using Fusion.Sockets;
#endif

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// IMmNetworkBackend implementation for Photon Fusion 2.
    ///
    /// This backend uses Fusion 2's RPC system with a NetworkBehaviour bridge
    /// to provide binary message transport compatible with MercuryMessaging.
    ///
    /// Fusion 2 Design Considerations:
    /// - Fusion uses tick-based state synchronization with RPCs for events
    /// - RPCs are TickAligned by default (execute at the same tick on all clients)
    /// - Unlike FishNet Broadcasts, Fusion RPCs require a NetworkObject
    /// - We use a singleton NetworkBehaviour (MmFusion2Bridge) for global messaging
    ///
    /// Key Differences from FishNet:
    /// - Requires MmFusion2Bridge MonoBehaviour on a spawned NetworkObject
    /// - Uses NetworkRunner instead of InstanceFinder
    /// - PlayerRef instead of NetworkConnection for client identification
    /// </summary>
    /// <remarks>
    /// To enable this backend:
    /// 1. Install Photon Fusion 2 via Package Manager
    /// 2. Add FUSION2_AVAILABLE to Scripting Define Symbols (or use versionDefines in asmdef)
    /// 3. Add Fusion.Runtime to MercuryMessaging.asmdef references
    /// 4. Add MmFusion2Bridge prefab to your scene (spawned NetworkObject)
    /// </remarks>
    public class Fusion2Backend : IMmNetworkBackend
    {
        #region State

        private bool _isInitialized;

#if FUSION2_AVAILABLE
        /// <summary>
        /// Reference to the active NetworkRunner.
        /// </summary>
        private NetworkRunner _runner;

        /// <summary>
        /// Reference to the bridge NetworkBehaviour that handles RPCs.
        /// </summary>
        private MmFusion2Bridge _bridge;

        /// <summary>
        /// PlayerRef → client ID mapping for consistent identification.
        /// </summary>
        private readonly Dictionary<PlayerRef, int> _playerToClientId
            = new Dictionary<PlayerRef, int>();

        /// <summary>
        /// Client ID → PlayerRef reverse mapping for targeted sends.
        /// </summary>
        private readonly Dictionary<int, PlayerRef> _clientIdToPlayer
            = new Dictionary<int, PlayerRef>();

        /// <summary>
        /// Next client ID to assign (simple incrementing counter).
        /// </summary>
        private int _nextClientId = 1;
#endif

        #endregion

        #region IMmNetworkBackend Properties

#if FUSION2_AVAILABLE
        /// <inheritdoc/>
        public bool IsConnected => _runner != null && _runner.IsRunning;

        /// <inheritdoc/>
        public bool IsServer => _runner != null && _runner.IsServer;

        /// <inheritdoc/>
        public bool IsClient => _runner != null && _runner.IsClient;

        /// <inheritdoc/>
        public int LocalClientId
        {
            get
            {
                if (_runner == null || !_runner.IsRunning)
                    return -1;

                var localPlayer = _runner.LocalPlayer;
                if (_playerToClientId.TryGetValue(localPlayer, out int clientId))
                    return clientId;

                // For host/server, use a fixed ID
                if (_runner.IsServer)
                    return 0;

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
        public string BackendName => "Fusion2";

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

#if FUSION2_AVAILABLE
            // Find any active runner in scene
            _runner = UnityEngine.Object.FindFirstObjectByType<NetworkRunner>();

            if (_runner == null)
            {
                MmLog.LogWarning("Fusion2Backend: No NetworkRunner found. " +
                    "Initialize will complete when a runner becomes available.");
            }
            else
            {
                SetupRunner(_runner);
            }
#endif

            _isInitialized = true;
            MmLog.LogNetwork("Fusion2Backend initialized");
        }

#if FUSION2_AVAILABLE
        /// <summary>
        /// Configure the backend with a specific NetworkRunner.
        /// Call this when the runner becomes available if not found during Initialize().
        /// </summary>
        public void SetRunner(NetworkRunner runner)
        {
            if (runner == null)
            {
                MmLog.LogWarning("Fusion2Backend: SetRunner called with null runner");
                return;
            }

            _runner = runner;
            SetupRunner(runner);
        }

        private void SetupRunner(NetworkRunner runner)
        {
            // Find or create the bridge NetworkBehaviour
            _bridge = UnityEngine.Object.FindFirstObjectByType<MmFusion2Bridge>();
            if (_bridge != null)
            {
                _bridge.SetBackend(this);
                MmLog.LogNetwork("Fusion2Backend: Connected to MmFusion2Bridge");
            }
            else
            {
                MmLog.LogWarning("Fusion2Backend: No MmFusion2Bridge found in scene. " +
                    "Add MmFusion2Bridge prefab to enable network messaging.");
            }
        }

        /// <summary>
        /// Called by MmFusion2Bridge when it spawns and becomes ready.
        /// </summary>
        internal void OnBridgeSpawned(MmFusion2Bridge bridge)
        {
            _bridge = bridge;
            MmLog.LogNetwork("Fusion2Backend: Bridge spawned and connected");
        }
#endif

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_isInitialized) return;

#if FUSION2_AVAILABLE
            _runner = null;
            _bridge = null;
            _playerToClientId.Clear();
            _clientIdToPlayer.Clear();
            _nextClientId = 1;
#endif

            _isInitialized = false;
            MmLog.LogNetwork("Fusion2Backend shutdown");
        }

        #endregion

        #region Sending

        /// <inheritdoc/>
        public void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FUSION2_AVAILABLE
            if (!IsConnected || !IsClient)
            {
                MmLog.LogWarning("Fusion2Backend: Cannot send to server - not connected as client");
                return;
            }

            if (_bridge == null)
            {
                MmLog.LogWarning("Fusion2Backend: No bridge available for sending");
                return;
            }

            _bridge.RpcSendToServer(data);
#else
            MmLog.LogWarning("Fusion2Backend: Fusion 2 not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FUSION2_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("Fusion2Backend: Cannot send to clients - not running as server");
                return;
            }

            if (_bridge == null)
            {
                MmLog.LogWarning("Fusion2Backend: No bridge available for sending");
                return;
            }

            _bridge.RpcSendToAllClients(data);
#else
            MmLog.LogWarning("Fusion2Backend: Fusion 2 not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FUSION2_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("Fusion2Backend: Cannot send to client - not running as server");
                return;
            }

            if (_bridge == null)
            {
                MmLog.LogWarning("Fusion2Backend: No bridge available for sending");
                return;
            }

            if (!_clientIdToPlayer.TryGetValue(clientId, out PlayerRef player))
            {
                MmLog.LogWarning($"Fusion2Backend: Client {clientId} not found in player list");
                return;
            }

            _bridge.RpcSendToClient(player, data);
#else
            MmLog.LogWarning("Fusion2Backend: Fusion 2 not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if FUSION2_AVAILABLE
            if (!IsConnected || !IsServer)
            {
                MmLog.LogWarning("Fusion2Backend: Cannot send to clients - not running as server");
                return;
            }

            if (_bridge == null)
            {
                MmLog.LogWarning("Fusion2Backend: No bridge available for sending");
                return;
            }

            // Get the PlayerRef to exclude
            PlayerRef excludePlayer = default;
            if (_clientIdToPlayer.TryGetValue(excludeClientId, out var player))
            {
                excludePlayer = player;
            }

            _bridge.RpcSendToOtherClients(excludePlayer, data);
#else
            MmLog.LogWarning("Fusion2Backend: Fusion 2 not available");
#endif
        }

        #endregion

        #region Message Reception (called by bridge)

#if FUSION2_AVAILABLE
        /// <summary>
        /// Called by MmFusion2Bridge when a message is received from a client.
        /// </summary>
        internal void OnMessageFromClient(PlayerRef sender, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                MmLog.LogWarning("Fusion2Backend: Received message with null/empty data");
                return;
            }

            int clientId = GetOrAssignClientId(sender);
            OnMessageReceived?.Invoke(data, clientId);
        }

        /// <summary>
        /// Called by MmFusion2Bridge when a message is received from the server.
        /// </summary>
        internal void OnMessageFromServer(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                MmLog.LogWarning("Fusion2Backend: Received message with null/empty data");
                return;
            }

            // -1 indicates message is from server
            OnMessageReceived?.Invoke(data, -1);
        }
#endif

        #endregion

        #region Connection Events

#if FUSION2_AVAILABLE
        /// <summary>
        /// Called by MmFusion2Bridge when a player joins.
        /// </summary>
        internal void OnPlayerJoined(PlayerRef player)
        {
            int clientId = GetOrAssignClientId(player);
            MmLog.LogNetwork($"Fusion2Backend: Player {player.PlayerId} joined (clientId: {clientId})");
            OnClientConnected?.Invoke(clientId);
        }

        /// <summary>
        /// Called by MmFusion2Bridge when a player leaves.
        /// </summary>
        internal void OnPlayerLeft(PlayerRef player)
        {
            if (_playerToClientId.TryGetValue(player, out int clientId))
            {
                _playerToClientId.Remove(player);
                _clientIdToPlayer.Remove(clientId);
                MmLog.LogNetwork($"Fusion2Backend: Player {player.PlayerId} left (clientId: {clientId})");
                OnClientDisconnected?.Invoke(clientId);
            }
        }

        /// <summary>
        /// Called when local client connects to server.
        /// </summary>
        internal void OnConnected()
        {
            MmLog.LogNetwork("Fusion2Backend: Connected to server");
            OnConnectedToServer?.Invoke();
        }

        /// <summary>
        /// Called when local client disconnects from server.
        /// </summary>
        internal void OnDisconnected()
        {
            MmLog.LogNetwork("Fusion2Backend: Disconnected from server");
            OnDisconnectedFromServer?.Invoke();
        }

        /// <summary>
        /// Get or assign a client ID for a PlayerRef.
        /// </summary>
        private int GetOrAssignClientId(PlayerRef player)
        {
            if (_playerToClientId.TryGetValue(player, out int existingId))
                return existingId;

            int newId = _nextClientId++;
            _playerToClientId[player] = newId;
            _clientIdToPlayer[newId] = player;
            return newId;
        }
#endif

        #endregion
    }
}
