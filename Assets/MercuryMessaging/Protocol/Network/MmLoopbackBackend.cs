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

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Loopback backend for testing network code without an actual network.
    ///
    /// This backend immediately echoes sent messages back to OnMessageReceived,
    /// allowing unit tests and integration tests to verify serialization,
    /// deserialization, and message routing without network latency or complexity.
    ///
    /// Modes:
    /// - Server mode: SendToAllClients echoes back, SendToServer is ignored
    /// - Client mode: SendToServer echoes back, SendToAllClients is ignored
    /// - Loopback mode (default): All sends echo back immediately
    /// </summary>
    public class MmLoopbackBackend : IMmNetworkBackend
    {
        #region Configuration

        /// <summary>
        /// Operating mode for the loopback backend.
        /// </summary>
        public enum LoopbackMode
        {
            /// <summary>
            /// All sends immediately echo back (default)
            /// </summary>
            Echo,

            /// <summary>
            /// Simulate server behavior
            /// </summary>
            Server,

            /// <summary>
            /// Simulate client behavior
            /// </summary>
            Client
        }

        /// <summary>
        /// Current operating mode.
        /// </summary>
        public LoopbackMode Mode { get; set; } = LoopbackMode.Echo;

        /// <summary>
        /// If true, messages are queued and delivered on ProcessPendingMessages().
        /// If false (default), messages are delivered immediately.
        /// </summary>
        public bool UseMessageQueue { get; set; } = false;

        /// <summary>
        /// Simulated latency in milliseconds (only applies when UseMessageQueue is true).
        /// </summary>
        public int SimulatedLatencyMs { get; set; } = 0;

        #endregion

        #region State

        private bool _isInitialized;
        private readonly Queue<(byte[] data, int senderId, DateTime deliveryTime)> _messageQueue
            = new Queue<(byte[], int, DateTime)>();

        #endregion

        #region IMmNetworkBackend Implementation

        /// <inheritdoc/>
        public bool IsConnected => _isInitialized;

        /// <inheritdoc/>
        public bool IsServer => Mode == LoopbackMode.Server || Mode == LoopbackMode.Echo;

        /// <inheritdoc/>
        public bool IsClient => Mode == LoopbackMode.Client || Mode == LoopbackMode.Echo;

        /// <inheritdoc/>
        public int LocalClientId => Mode == LoopbackMode.Server ? 0 : 1;

        /// <inheritdoc/>
        public string BackendName => "Loopback";

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

        /// <inheritdoc/>
        public void Initialize()
        {
            _isInitialized = true;
            _messageQueue.Clear();

            if (Mode == LoopbackMode.Client)
            {
                OnConnectedToServer?.Invoke();
            }
            else if (Mode == LoopbackMode.Server)
            {
                OnClientConnected?.Invoke(1); // Simulate a client connecting
            }
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_isInitialized) return;

            if (Mode == LoopbackMode.Client)
            {
                OnDisconnectedFromServer?.Invoke();
            }
            else if (Mode == LoopbackMode.Server)
            {
                OnClientDisconnected?.Invoke(1);
            }

            _isInitialized = false;
            _messageQueue.Clear();
        }

        /// <inheritdoc/>
        public void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
            if (!_isInitialized) return;

            // In client mode or echo mode, echo back
            if (Mode != LoopbackMode.Server)
            {
                DeliverMessage(data, LocalClientId);
            }
        }

        /// <inheritdoc/>
        public void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
            if (!_isInitialized) return;

            // In server mode or echo mode, echo back
            if (Mode != LoopbackMode.Client)
            {
                DeliverMessage(data, 0); // Server ID = 0
            }
        }

        /// <inheritdoc/>
        public void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
            if (!_isInitialized) return;

            // In server mode or echo mode, echo back
            if (Mode != LoopbackMode.Client)
            {
                DeliverMessage(data, 0);
            }
        }

        /// <inheritdoc/>
        public void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
        {
            if (!_isInitialized) return;

            // In server mode or echo mode, echo back (excluding the specified client)
            if (Mode != LoopbackMode.Client && excludeClientId != LocalClientId)
            {
                DeliverMessage(data, 0);
            }
        }

        #endregion

        #region Message Delivery

        private void DeliverMessage(byte[] data, int senderId)
        {
            if (UseMessageQueue)
            {
                var deliveryTime = DateTime.Now.AddMilliseconds(SimulatedLatencyMs);
                _messageQueue.Enqueue((data, senderId, deliveryTime));
            }
            else
            {
                // Immediate delivery
                OnMessageReceived?.Invoke(data, senderId);
            }
        }

        /// <summary>
        /// Process any pending messages in the queue.
        /// Call this in Update() or a test loop when UseMessageQueue is true.
        /// </summary>
        public void ProcessPendingMessages()
        {
            var now = DateTime.Now;
            while (_messageQueue.Count > 0)
            {
                var (data, senderId, deliveryTime) = _messageQueue.Peek();
                if (deliveryTime <= now)
                {
                    _messageQueue.Dequeue();
                    OnMessageReceived?.Invoke(data, senderId);
                }
                else
                {
                    break; // Next message not ready yet
                }
            }
        }

        /// <summary>
        /// Get the number of pending messages in the queue.
        /// </summary>
        public int PendingMessageCount => _messageQueue.Count;

        /// <summary>
        /// Force delivery of all pending messages immediately.
        /// </summary>
        public void FlushMessages()
        {
            while (_messageQueue.Count > 0)
            {
                var (data, senderId, _) = _messageQueue.Dequeue();
                OnMessageReceived?.Invoke(data, senderId);
            }
        }

        #endregion

        #region Test Helpers

        /// <summary>
        /// List of all messages sent through this backend (for test verification).
        /// </summary>
        public List<byte[]> SentMessages { get; } = new List<byte[]>();

        /// <summary>
        /// If true, stores all sent messages in SentMessages list for verification.
        /// </summary>
        public bool RecordSentMessages { get; set; } = false;

        /// <summary>
        /// Clear the sent messages list.
        /// </summary>
        public void ClearSentMessages() => SentMessages.Clear();

        /// <summary>
        /// Reset the backend to initial state.
        /// </summary>
        public void Reset()
        {
            Shutdown();
            _messageQueue.Clear();
            SentMessages.Clear();
            Mode = LoopbackMode.Echo;
            UseMessageQueue = false;
            SimulatedLatencyMs = 0;
            RecordSentMessages = false;
        }

        #endregion
    }
}
