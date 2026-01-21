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

using UnityEngine;

#if FUSION2_AVAILABLE
using Fusion;
#endif

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// IMmGameObjectResolver implementation for Photon Fusion 2.
    ///
    /// Uses NetworkObject.Id.Raw to map between network IDs and GameObjects.
    /// Fusion 2 uses NetworkId struct which has a Raw uint property for serialization.
    /// </summary>
    /// <remarks>
    /// Fusion 2 specifics:
    /// - Uses NetworkObject.Id.Raw (compare with FishNet's ObjectId)
    /// - NetworkRunner maintains the spawned object registry
    /// - NetworkId.Raw of 0 indicates invalid/unspawned object
    /// - Both scene objects and spawned objects use the same ID system
    /// </remarks>
    public class Fusion2Resolver : IMmGameObjectResolver
    {
        /// <inheritdoc/>
        public string ResolverName => "Fusion2Resolver";

#if FUSION2_AVAILABLE
        /// <summary>
        /// Cached reference to the NetworkRunner.
        /// </summary>
        private NetworkRunner _runner;

        /// <summary>
        /// Get or find the active NetworkRunner.
        /// </summary>
        private NetworkRunner Runner
        {
            get
            {
                if (_runner == null || !_runner.IsRunning)
                {
                    _runner = NetworkRunner.GetRunnerForGameObject(null);
                    if (_runner == null)
                    {
                        _runner = Object.FindFirstObjectByType<NetworkRunner>();
                    }
                }
                return _runner;
            }
        }
#endif

        /// <inheritdoc/>
        public bool TryGetNetworkId(GameObject gameObject, out uint networkId)
        {
#if FUSION2_AVAILABLE
            if (gameObject != null)
            {
                var nob = gameObject.GetComponent<NetworkObject>();
                // Check IsValid and that Raw is not 0 (invalid/unspawned)
                if (nob != null && nob.IsValid && nob.Id.Raw > 0)
                {
                    networkId = nob.Id.Raw;
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
#if FUSION2_AVAILABLE
            var runner = Runner;
            if (runner != null && runner.IsRunning && networkId > 0)
            {
                // Create NetworkId from raw value
                var netId = new NetworkId { Raw = networkId };

                // Try to find the object in the runner's registry
                if (runner.TryFindObject(netId, out NetworkObject nob))
                {
                    gameObject = nob.gameObject;
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
