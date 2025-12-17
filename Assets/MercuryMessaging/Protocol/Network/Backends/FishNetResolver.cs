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

#if FISHNET_AVAILABLE
using FishNet;
using FishNet.Object;
#endif

namespace MercuryMessaging.Network.Backends
{
    /// <summary>
    /// IMmGameObjectResolver implementation for FishNet.
    ///
    /// Uses NetworkObject.ObjectId to map between network IDs and GameObjects.
    /// FishNet maintains separate spawned object dictionaries for server and client,
    /// so this resolver checks both paths.
    /// </summary>
    /// <remarks>
    /// FishNet specifics:
    /// - Uses NetworkObject.ObjectId for identification
    /// - Has separate server/client spawned dictionaries
    /// - Compare with Fusion2Resolver which uses NetworkObject.Id.Raw
    /// </remarks>
    public class FishNetResolver : IMmGameObjectResolver
    {
        /// <inheritdoc/>
        public string ResolverName => "FishNetResolver";

        /// <inheritdoc/>
        public bool TryGetNetworkId(GameObject gameObject, out uint networkId)
        {
#if FISHNET_AVAILABLE
            if (gameObject != null)
            {
                var nob = gameObject.GetComponent<NetworkObject>();
                // Reject ObjectId 0 - FishNet uses 0 for scene objects, but MercuryMessaging
                // treats 0 as "no target". Force fallback to deterministic ID for scene objects.
                if (nob != null && nob.IsSpawned && nob.ObjectId > 0)
                {
                    networkId = (uint)nob.ObjectId;
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
#if FISHNET_AVAILABLE
            // Try server-side spawned objects first
            if (InstanceFinder.IsServerStarted &&
                InstanceFinder.ServerManager != null &&
                InstanceFinder.ServerManager.Objects != null)
            {
                if (InstanceFinder.ServerManager.Objects.Spawned
                    .TryGetValue((int)networkId, out NetworkObject serverNob))
                {
                    gameObject = serverNob.gameObject;
                    return true;
                }
            }

            // Try client-side spawned objects
            if (InstanceFinder.IsClientStarted &&
                InstanceFinder.ClientManager != null &&
                InstanceFinder.ClientManager.Objects != null)
            {
                if (InstanceFinder.ClientManager.Objects.Spawned
                    .TryGetValue((int)networkId, out NetworkObject clientNob))
                {
                    gameObject = clientNob.gameObject;
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
