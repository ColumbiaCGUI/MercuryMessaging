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

using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Simple registry-based GameObject resolver.
    ///
    /// Uses a dictionary to map network IDs to GameObjects.
    /// Suitable for testing and scenarios where manual registration is acceptable.
    ///
    /// For production use with networking backends, prefer backend-specific
    /// resolvers (PhotonResolver, FishNetResolver, etc.) that use the backend's
    /// native ID system.
    /// </summary>
    public class MmRegistryResolver : IMmGameObjectResolver
    {
        private readonly Dictionary<uint, GameObject> _idToGameObject = new Dictionary<uint, GameObject>();
        private readonly Dictionary<GameObject, uint> _gameObjectToId = new Dictionary<GameObject, uint>();
        private uint _nextId = 1;

        /// <inheritdoc/>
        public string ResolverName => "RegistryResolver";

        #region Registration

        /// <summary>
        /// Register a GameObject with an auto-generated network ID.
        /// </summary>
        /// <param name="gameObject">The GameObject to register</param>
        /// <returns>The assigned network ID</returns>
        public uint Register(GameObject gameObject)
        {
            if (gameObject == null) return 0;

            // Check if already registered
            if (_gameObjectToId.TryGetValue(gameObject, out uint existingId))
            {
                return existingId;
            }

            uint id = _nextId++;
            RegisterWithId(id, gameObject);
            return id;
        }

        /// <summary>
        /// Register a GameObject with a specific network ID.
        /// </summary>
        /// <param name="networkId">The network ID to assign</param>
        /// <param name="gameObject">The GameObject to register</param>
        public void RegisterWithId(uint networkId, GameObject gameObject)
        {
            if (gameObject == null || networkId == 0) return;

            // Remove any existing registrations
            if (_idToGameObject.TryGetValue(networkId, out GameObject existing) && existing != gameObject)
            {
                _gameObjectToId.Remove(existing);
            }
            if (_gameObjectToId.TryGetValue(gameObject, out uint existingId) && existingId != networkId)
            {
                _idToGameObject.Remove(existingId);
            }

            _idToGameObject[networkId] = gameObject;
            _gameObjectToId[gameObject] = networkId;

            // Update next ID if necessary
            if (networkId >= _nextId)
            {
                _nextId = networkId + 1;
            }
        }

        /// <summary>
        /// Unregister a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to unregister</param>
        public void Unregister(GameObject gameObject)
        {
            if (gameObject == null) return;

            if (_gameObjectToId.TryGetValue(gameObject, out uint id))
            {
                _idToGameObject.Remove(id);
                _gameObjectToId.Remove(gameObject);
            }
        }

        /// <summary>
        /// Unregister by network ID.
        /// </summary>
        /// <param name="networkId">The network ID to unregister</param>
        public void Unregister(uint networkId)
        {
            if (_idToGameObject.TryGetValue(networkId, out GameObject gameObject))
            {
                _idToGameObject.Remove(networkId);
                _gameObjectToId.Remove(gameObject);
            }
        }

        /// <summary>
        /// Clear all registrations.
        /// </summary>
        public void Clear()
        {
            _idToGameObject.Clear();
            _gameObjectToId.Clear();
            _nextId = 1;
        }

        /// <summary>
        /// Number of registered GameObjects.
        /// </summary>
        public int Count => _idToGameObject.Count;

        #endregion

        #region IMmGameObjectResolver Implementation

        /// <inheritdoc/>
        public bool TryGetNetworkId(GameObject gameObject, out uint networkId)
        {
            if (gameObject == null)
            {
                networkId = 0;
                return false;
            }
            return _gameObjectToId.TryGetValue(gameObject, out networkId);
        }

        /// <inheritdoc/>
        public bool TryGetGameObject(uint networkId, out GameObject gameObject)
        {
            if (networkId == 0)
            {
                gameObject = null;
                return false;
            }

            if (_idToGameObject.TryGetValue(networkId, out gameObject))
            {
                // Check if GameObject still exists
                if (gameObject == null)
                {
                    // Clean up stale reference
                    _idToGameObject.Remove(networkId);
                    return false;
                }
                return true;
            }

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

        #endregion
    }
}
