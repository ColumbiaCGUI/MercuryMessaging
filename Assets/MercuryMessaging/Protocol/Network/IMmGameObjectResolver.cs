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

using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Interface for resolving network IDs to GameObjects and vice versa.
    ///
    /// Each networking backend has its own ID scheme:
    /// - PUN2: PhotonView.ViewID
    /// - FishNet: NetworkObject.ObjectId
    /// - Fusion: NetworkObject.Id.Raw
    ///
    /// Implementations translate between these backend-specific IDs
    /// and Unity GameObjects, enabling MmMessageGameObject to work
    /// across all backends.
    /// </summary>
    public interface IMmGameObjectResolver
    {
        /// <summary>
        /// Get the network ID for a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to get an ID for</param>
        /// <param name="networkId">The resolved network ID (output)</param>
        /// <returns>True if the GameObject has a valid network ID, false otherwise</returns>
        /// <remarks>
        /// Returns false if:
        /// - GameObject is null
        /// - GameObject doesn't have required network component (PhotonView, NetworkObject, etc.)
        /// - Network component isn't properly initialized
        /// </remarks>
        bool TryGetNetworkId(GameObject gameObject, out uint networkId);

        /// <summary>
        /// Get the GameObject for a network ID.
        /// </summary>
        /// <param name="networkId">The network ID to resolve</param>
        /// <param name="gameObject">The resolved GameObject (output)</param>
        /// <returns>True if a valid GameObject was found, false otherwise</returns>
        /// <remarks>
        /// Returns false if:
        /// - Network ID is invalid (0 or not registered)
        /// - Referenced object has been destroyed
        /// - Object hasn't spawned on this client yet
        /// </remarks>
        bool TryGetGameObject(uint networkId, out GameObject gameObject);

        /// <summary>
        /// Get the network ID for a GameObject (non-Try version).
        /// </summary>
        /// <param name="gameObject">The GameObject to get an ID for</param>
        /// <returns>Network ID, or 0 if not found</returns>
        uint GetNetworkId(GameObject gameObject);

        /// <summary>
        /// Get the GameObject for a network ID (non-Try version).
        /// </summary>
        /// <param name="networkId">The network ID to resolve</param>
        /// <returns>GameObject, or null if not found</returns>
        GameObject GetGameObject(uint networkId);

        /// <summary>
        /// Get the MmRelayNode for a network ID.
        /// Convenience method that resolves GameObject and gets its MmRelayNode component.
        /// </summary>
        /// <param name="networkId">The network ID to resolve</param>
        /// <param name="relayNode">The resolved MmRelayNode (output)</param>
        /// <returns>True if a valid MmRelayNode was found, false otherwise</returns>
        bool TryGetRelayNode(uint networkId, out MmRelayNode relayNode);

        /// <summary>
        /// Human-readable name of this resolver (e.g., "PhotonResolver", "FishNetResolver").
        /// </summary>
        string ResolverName { get; }
    }
}
