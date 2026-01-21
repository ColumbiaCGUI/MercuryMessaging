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

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Delegate for handling received network messages.
    /// </summary>
    /// <param name="data">Raw binary message data</param>
    /// <param name="senderId">Network ID of the sender (-1 if server/unknown)</param>
    public delegate void MmNetworkMessageReceived(byte[] data, int senderId);

    /// <summary>
    /// Delegate for connection state changes.
    /// </summary>
    /// <param name="clientId">ID of the connected/disconnected client</param>
    public delegate void MmNetworkConnectionChanged(int clientId);

    /// <summary>
    /// Reliability mode for network messages.
    /// </summary>
    public enum MmReliability
    {
        /// <summary>
        /// Messages may be dropped but arrive in order (UDP-like)
        /// </summary>
        Unreliable,

        /// <summary>
        /// Messages are guaranteed to arrive in order (TCP-like)
        /// </summary>
        Reliable
    }

    /// <summary>
    /// Target recipients for network messages.
    /// </summary>
    public enum MmNetworkTarget
    {
        /// <summary>
        /// Send to server/host only
        /// </summary>
        Server,

        /// <summary>
        /// Send to all connected clients (server broadcast)
        /// </summary>
        AllClients,

        /// <summary>
        /// Send to all clients except the sender
        /// </summary>
        OtherClients,

        /// <summary>
        /// Send to a specific client by ID
        /// </summary>
        SpecificClient
    }

    /// <summary>
    /// Provider-agnostic network transport interface.
    ///
    /// Implement this interface to integrate MercuryMessaging with any
    /// networking solution (FishNet, Photon Fusion, Netcode, Mirror, etc.).
    ///
    /// The backend handles only raw byte transport - message serialization
    /// is handled separately by MmBinarySerializer.
    /// </summary>
    /// <remarks>
    /// Design principles:
    /// - No MonoBehaviour dependency (backends can be pure C# or wrappers)
    /// - Binary-only transport (no object[] or JSON)
    /// - Event-driven message reception
    /// - Backend-specific ID resolution via IMmGameObjectResolver
    /// </remarks>
    public interface IMmNetworkBackend
    {
        #region Connection State

        /// <summary>
        /// Whether currently connected to the network.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Whether this instance is the server/host.
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// Whether this instance is a client (non-host).
        /// Note: On host machines, both IsServer and IsClient may be true.
        /// </summary>
        bool IsClient { get; }

        /// <summary>
        /// This client's network ID (-1 if not connected or if server-only).
        /// </summary>
        int LocalClientId { get; }

        /// <summary>
        /// Human-readable name of this backend (e.g., "FishNet", "Fusion2").
        /// </summary>
        string BackendName { get; }

        #endregion

        #region Events

        /// <summary>
        /// Fired when a message is received from the network.
        /// The handler receives raw binary data that must be deserialized.
        /// </summary>
        event MmNetworkMessageReceived OnMessageReceived;

        /// <summary>
        /// Fired when a client connects (server-side only).
        /// </summary>
        event MmNetworkConnectionChanged OnClientConnected;

        /// <summary>
        /// Fired when a client disconnects (server-side only).
        /// </summary>
        event MmNetworkConnectionChanged OnClientDisconnected;

        /// <summary>
        /// Fired when this client connects to a server.
        /// </summary>
        event Action OnConnectedToServer;

        /// <summary>
        /// Fired when this client disconnects from the server.
        /// </summary>
        event Action OnDisconnectedFromServer;

        #endregion

        #region Message Sending

        /// <summary>
        /// Send a message to the server (client-side).
        /// </summary>
        /// <param name="data">Binary message data</param>
        /// <param name="reliability">Delivery guarantee</param>
        void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable);

        /// <summary>
        /// Send a message to all clients (server-side broadcast).
        /// </summary>
        /// <param name="data">Binary message data</param>
        /// <param name="reliability">Delivery guarantee</param>
        void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable);

        /// <summary>
        /// Send a message to a specific client (server-side).
        /// </summary>
        /// <param name="clientId">Target client's network ID</param>
        /// <param name="data">Binary message data</param>
        /// <param name="reliability">Delivery guarantee</param>
        void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable);

        /// <summary>
        /// Send a message to all clients except one (server-side).
        /// Useful for forwarding messages from one client to others.
        /// </summary>
        /// <param name="excludeClientId">Client ID to exclude</param>
        /// <param name="data">Binary message data</param>
        /// <param name="reliability">Delivery guarantee</param>
        void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable);

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initialize the backend. Called once before use.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Clean up resources. Called when backend is no longer needed.
        /// </summary>
        void Shutdown();

        #endregion
    }
}
