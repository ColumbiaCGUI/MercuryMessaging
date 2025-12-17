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

#if FUSION2_AVAILABLE
using Fusion;
using UnityEngine;
using MercuryMessaging.Network;
using MmLog = MercuryMessaging.MmLogger;

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// NetworkBehaviour bridge for Fusion 2 that handles RPC-based message transport.
    ///
    /// This component must be attached to a NetworkObject that is spawned in the scene.
    /// It provides the RPC methods that Fusion2Backend uses to send messages.
    ///
    /// Fusion 2 RPC Design:
    /// - RPCs are TickAligned by default (ensures consistent timing across clients)
    /// - We use TickAligned=false for immediate delivery (like FishNet broadcasts)
    /// - RPCs can target specific sources/targets using RpcSources and RpcTargets
    /// </summary>
    /// <remarks>
    /// Setup:
    /// 1. Create a prefab with NetworkObject + MmFusion2Bridge
    /// 2. Spawn this prefab when the session starts (or include in scene)
    /// 3. Fusion2Backend will automatically find and connect to this bridge
    /// </remarks>
    [RequireComponent(typeof(NetworkObject))]
    public class MmFusion2Bridge : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        #region State

        /// <summary>
        /// Reference to the backend that owns this bridge.
        /// </summary>
        private Fusion2Backend _backend;

        /// <summary>
        /// Singleton instance for easy access.
        /// </summary>
        public static MmFusion2Bridge Instance { get; private set; }

        #endregion

        #region Lifecycle

        public override void Spawned()
        {
            base.Spawned();

            // Register as singleton
            if (Instance != null && Instance != this)
            {
                MmLog.LogWarning("MmFusion2Bridge: Multiple instances detected. Using newest.");
            }
            Instance = this;

            // Find and connect to the backend
            ConnectToBackend();

            MmLog.LogNetwork($"MmFusion2Bridge spawned (HasStateAuthority: {HasStateAuthority})");
        }

        /// <summary>
        /// Find and connect to the Fusion2Backend via MmNetworkBridge.
        /// </summary>
        private void ConnectToBackend()
        {
            // First check if backend was already set
            if (_backend != null)
            {
                _backend.OnBridgeSpawned(this);
                return;
            }

            // Find MmNetworkBridge and get its backend
            var networkBridge = MmNetworkBridge.Instance;
            if (networkBridge != null && networkBridge.Backend is Fusion2Backend fusion2Backend)
            {
                _backend = fusion2Backend;
                _backend.OnBridgeSpawned(this);
                MmLog.LogNetwork("MmFusion2Bridge: Connected to Fusion2Backend via MmNetworkBridge");
                return;
            }

            // Fallback: Find Fusion2BridgeSetup and get backend from there
            var bridgeSetup = FindFirstObjectByType<Fusion2BridgeSetup>();
            if (bridgeSetup != null)
            {
                // The setup should have configured the backend - try to get it
                networkBridge = bridgeSetup.GetComponent<MmNetworkBridge>();
                if (networkBridge != null && networkBridge.Backend is Fusion2Backend backend)
                {
                    _backend = backend;
                    _backend.OnBridgeSpawned(this);
                    MmLog.LogNetwork("MmFusion2Bridge: Connected to Fusion2Backend via Fusion2BridgeSetup");
                    return;
                }
            }

            MmLog.LogWarning("MmFusion2Bridge: Could not find Fusion2Backend to connect to. " +
                "Ensure MmNetworkBridge with Fusion2BridgeSetup exists in scene.");
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

            if (Instance == this)
                Instance = null;

            MmLog.LogNetwork("MmFusion2Bridge despawned");
        }

        /// <summary>
        /// Set the backend reference. Called by Fusion2Backend.
        /// </summary>
        internal void SetBackend(Fusion2Backend backend)
        {
            _backend = backend;
        }

        #endregion

        #region IPlayerJoined / IPlayerLeft

        public void PlayerJoined(PlayerRef player)
        {
            _backend?.OnPlayerJoined(player);
        }

        public void PlayerLeft(PlayerRef player)
        {
            _backend?.OnPlayerLeft(player);
        }

        #endregion

        #region RPCs - Client to Server

        /// <summary>
        /// RPC from client to server (State Authority).
        /// TickAligned=false for immediate delivery.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, TickAligned = false)]
        public void RpcSendToServer(byte[] data, RpcInfo info = default)
        {
            if (_backend == null)
            {
                MmLog.LogWarning("MmFusion2Bridge: RpcSendToServer - no backend connected");
                return;
            }

            // Get sender's PlayerRef from RpcInfo
            var sender = info.Source;
            _backend.OnMessageFromClient(sender, data);
        }

        #endregion

        #region RPCs - Server to Clients

        /// <summary>
        /// RPC from server to all clients (broadcast).
        /// TickAligned=false for immediate delivery.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
        public void RpcSendToAllClients(byte[] data, RpcInfo info = default)
        {
            if (_backend == null)
            {
                MmLog.LogWarning("MmFusion2Bridge: RpcSendToAllClients - no backend connected");
                return;
            }

            // On clients, this is a message from server
            if (!Runner.IsServer)
            {
                _backend.OnMessageFromServer(data);
            }
            // On server (host mode), we might want to process locally too
            // but typically server-originated messages don't need to loop back
        }

        /// <summary>
        /// RPC from server to a specific client.
        /// This is implemented as a broadcast with client-side filtering.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
        public void RpcSendToClient(PlayerRef targetPlayer, byte[] data, RpcInfo info = default)
        {
            if (_backend == null)
            {
                MmLog.LogWarning("MmFusion2Bridge: RpcSendToClient - no backend connected");
                return;
            }

            // Only process if we're the target player
            if (Runner.LocalPlayer == targetPlayer)
            {
                _backend.OnMessageFromServer(data);
            }
        }

        /// <summary>
        /// RPC from server to all clients except one.
        /// This is implemented as a broadcast with client-side filtering.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
        public void RpcSendToOtherClients(PlayerRef excludePlayer, byte[] data, RpcInfo info = default)
        {
            if (_backend == null)
            {
                MmLog.LogWarning("MmFusion2Bridge: RpcSendToOtherClients - no backend connected");
                return;
            }

            // Process on all clients except the excluded one
            if (Runner.LocalPlayer != excludePlayer && !Runner.IsServer)
            {
                _backend.OnMessageFromServer(data);
            }
        }

        #endregion
    }
}
#endif
