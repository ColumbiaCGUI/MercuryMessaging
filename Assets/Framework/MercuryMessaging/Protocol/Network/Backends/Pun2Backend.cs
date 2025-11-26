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
using UnityEngine;
using MmLog = MercuryMessaging.MmLogger;

#if PHOTON_AVAILABLE
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
#endif

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// IMmNetworkBackend implementation for Photon Unity Networking 2 (PUN2).
    ///
    /// This backend wraps PUN2's RaiseEvent system to provide binary message
    /// transport compatible with the new MercuryMessaging networking architecture.
    ///
    /// Note: This is a transitional backend that maintains backward compatibility
    /// with existing PUN2-based MercuryMessaging applications while enabling
    /// migration to the new networking system.
    /// </summary>
    public class Pun2Backend : IMmNetworkBackend
#if PHOTON_AVAILABLE
        , IOnEventCallback
#endif
    {
        #region Constants

        /// <summary>
        /// Event code used for MercuryMessaging binary messages.
        /// Using a code in the custom range (1-199).
        /// </summary>
        private const byte MM_EVENT_CODE = 42;

        #endregion

        #region State

        private bool _isInitialized;
        private bool _isListening;

        #endregion

        #region IMmNetworkBackend Properties

#if PHOTON_AVAILABLE
        /// <inheritdoc/>
        public bool IsConnected => PhotonNetwork.IsConnectedAndReady;

        /// <inheritdoc/>
        public bool IsServer => PhotonNetwork.IsMasterClient;

        /// <inheritdoc/>
        public bool IsClient => !PhotonNetwork.IsMasterClient;

        /// <inheritdoc/>
        public int LocalClientId => PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
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
        public string BackendName => "PUN2";

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

#if PHOTON_AVAILABLE
            // Register for Photon events
            PhotonNetwork.AddCallbackTarget(this);
            _isListening = true;
#endif

            _isInitialized = true;
            MmLog.LogNetwork("Pun2Backend initialized");
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_isInitialized) return;

#if PHOTON_AVAILABLE
            if (_isListening)
            {
                PhotonNetwork.RemoveCallbackTarget(this);
                _isListening = false;
            }
#endif

            _isInitialized = false;
            MmLog.LogNetwork("Pun2Backend shutdown");
        }

        #endregion

        #region Sending

        /// <inheritdoc/>
        public void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if PHOTON_AVAILABLE
            if (!IsConnected)
            {
                MmLog.LogWarning("Pun2Backend: Cannot send - not connected");
                return;
            }

            var options = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.MasterClient
            };

            var sendOptions = reliability == MmReliability.Reliable
                ? SendOptions.SendReliable
                : SendOptions.SendUnreliable;

            PhotonNetwork.RaiseEvent(MM_EVENT_CODE, data, options, sendOptions);
#else
            MmLog.LogWarning("Pun2Backend: Photon not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if PHOTON_AVAILABLE
            if (!IsConnected)
            {
                MmLog.LogWarning("Pun2Backend: Cannot send - not connected");
                return;
            }

            var options = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };

            var sendOptions = reliability == MmReliability.Reliable
                ? SendOptions.SendReliable
                : SendOptions.SendUnreliable;

            PhotonNetwork.RaiseEvent(MM_EVENT_CODE, data, options, sendOptions);
#else
            MmLog.LogWarning("Pun2Backend: Photon not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if PHOTON_AVAILABLE
            if (!IsConnected)
            {
                MmLog.LogWarning("Pun2Backend: Cannot send - not connected");
                return;
            }

            var options = new RaiseEventOptions
            {
                TargetActors = new int[] { clientId }
            };

            var sendOptions = reliability == MmReliability.Reliable
                ? SendOptions.SendReliable
                : SendOptions.SendUnreliable;

            PhotonNetwork.RaiseEvent(MM_EVENT_CODE, data, options, sendOptions);
#else
            MmLog.LogWarning("Pun2Backend: Photon not available");
#endif
        }

        /// <inheritdoc/>
        public void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
#if PHOTON_AVAILABLE
            if (!IsConnected)
            {
                MmLog.LogWarning("Pun2Backend: Cannot send - not connected");
                return;
            }

            var options = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others
            };

            var sendOptions = reliability == MmReliability.Reliable
                ? SendOptions.SendReliable
                : SendOptions.SendUnreliable;

            PhotonNetwork.RaiseEvent(MM_EVENT_CODE, data, options, sendOptions);
#else
            MmLog.LogWarning("Pun2Backend: Photon not available");
#endif
        }

        #endregion

        #region Receiving

#if PHOTON_AVAILABLE
        /// <summary>
        /// IOnEventCallback implementation - receives Photon events.
        /// </summary>
        public void OnEvent(EventData photonEvent)
        {
            // Only process our event code
            if (photonEvent.Code != MM_EVENT_CODE) return;

            // Extract binary data
            byte[] data = photonEvent.CustomData as byte[];
            if (data == null)
            {
                MmLog.LogWarning("Pun2Backend: Received event with non-byte[] data");
                return;
            }

            // Get sender ID
            int senderId = photonEvent.Sender;

            // Raise event
            OnMessageReceived?.Invoke(data, senderId);
        }
#endif

        #endregion
    }

    /// <summary>
    /// IMmGameObjectResolver implementation for PUN2.
    ///
    /// Uses PhotonView.ViewID to map between network IDs and GameObjects.
    /// </summary>
    public class Pun2Resolver : IMmGameObjectResolver
    {
        /// <inheritdoc/>
        public string ResolverName => "Pun2Resolver";

        /// <inheritdoc/>
        public bool TryGetNetworkId(GameObject gameObject, out uint networkId)
        {
#if PHOTON_AVAILABLE
            if (gameObject != null)
            {
                var photonView = gameObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.ViewID != 0)
                {
                    networkId = (uint)photonView.ViewID;
                    return true;
                }
            }
#endif
            networkId = 0;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetGameObject(uint networkId, out GameObject gameObject)
        {
#if PHOTON_AVAILABLE
            if (networkId != 0)
            {
                var photonView = PhotonView.Find((int)networkId);
                if (photonView != null)
                {
                    gameObject = photonView.gameObject;
                    return true;
                }
            }
#endif
            gameObject = null;
            return false;
        }

        /// <inheritdoc/>
        public uint GetNetworkId(GameObject gameObject)
        {
            TryGetNetworkId(gameObject, out uint id);
            return id;
        }

        /// <inheritdoc/>
        public GameObject GetGameObject(uint networkId)
        {
            TryGetGameObject(networkId, out GameObject go);
            return go;
        }

        /// <inheritdoc/>
        public bool TryGetRelayNode(uint networkId, out MmRelayNode relayNode)
        {
            if (TryGetGameObject(networkId, out GameObject go) && go != null)
            {
                relayNode = go.GetComponent<MmRelayNode>();
                return relayNode != null;
            }

            relayNode = null;
            return false;
        }
    }
}
