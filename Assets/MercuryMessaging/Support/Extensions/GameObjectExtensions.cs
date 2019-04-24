// Copyright (c) 2017-2019, Columbia University
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
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MercuryMessaging.Support.Extensions
{

    /// <summary>
    /// Collection of GameObject extensions useful if using MercuryMessaging
    /// to create applications
    /// using MercuryMessaging.
    /// </summary>
    public static class GameObjectExtensions {
    
        /// <summary>
        /// Change layer on GameObject and all of its children.
        /// </summary>
        /// <param name="gameObject">Observed GameObjects.</param>
        /// <param name="layer">Layer to apply.</param>
        public static void SetLayerRecursively(this GameObject gameObject, string layer)
        {
            gameObject.layer = LayerMask.NameToLayer(layer);
            foreach (Transform t in gameObject.transform)
                t.gameObject.SetLayerRecursively(LayerMask.NameToLayer(layer));
        }

        /// <summary>
        /// http://www.third-helix.com/2013/09/30/adding-to-unitys-builtin-classes-using-extension-methods.html
        /// </summary>
        /// <param name="gameObject">Observed GameObject.</param>
        /// <param name="layer">Layer to apply to GameObjects.</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform t in gameObject.transform)
                t.gameObject.SetLayerRecursively(layer);
        }

        /// <summary>
        /// http://www.third-helix.com/2013/09/30/adding-to-unitys-builtin-classes-using-extension-methods.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject">Observed GameObject.</param>
        /// <param name="tag">Tag to use in finding objects.</param>
        /// <returns></returns>
        public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag)
            where T : Component
        {
            List<T> results = new List<T>();

            if (gameObject.CompareTag(tag))
                results.Add(gameObject.GetComponent<T>());

            foreach (Transform t in gameObject.transform)
                results.AddRange(t.gameObject.GetComponentsInChildrenWithTag<T>(tag));

            return results.ToArray();
        }

        /// <summary>
        /// Given a GameObject, replicate it and remove its Network components.
        /// </summary>
        /// <param name="original">Object to duplicate.</param>
        /// <returns>New GameObject without Network components.</returns>
        public static GameObject Replicate(this GameObject original)
        {
            var replica = Object.Instantiate(original);
            replica.name = string.Format("Replica ({0})", original.name);

            replica.RemoveNetworkComponents();

            return replica;
        }

        /// <summary>
        /// Given a GameObject, remove NetworkTransform and NetworkIdentity.
        /// </summary>
        /// <param name="go">Observed GameObject.</param>
        public static void RemoveNetworkComponents(this GameObject go)
        {
            var netTrans = go.GetComponent<NetworkTransform>();

            if (netTrans != null) Object.Destroy(netTrans);

            var netId = go.GetComponent<NetworkIdentity>();

            if (netId != null) Object.Destroy(netId);
        }
    }
}
