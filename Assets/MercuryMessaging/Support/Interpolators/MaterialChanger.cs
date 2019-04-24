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
using MercuryMessaging.Support.FiniteStateMachine;
using MercuryMessaging.Support.Input;
using UnityEngine;

namespace MercuryMessaging.Support.Interpolators
{
    public class MaterialChanger<T, TI> : MonoBehaviour
        where TI : Interpolator<T>, InterpolatorMaterial<T>
    {
        public T Target;
        public float Duration = 1;

        readonly Dictionary<T, TI> interpolators = new Dictionary<T, TI>();

        TI selectedInterpolator;

        enum MaterialState
        {
            Normal,
            Changed
        }

        readonly FiniteStateMachine<MaterialState> materialFSM
            = new FiniteStateMachine<MaterialState>("MaterialState");

        MeshRenderer[] meshRenderers;

        public virtual void Awake()
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            materialFSM[MaterialState.Normal].Exit = delegate
            {
                foreach (var interpolator in interpolators.Values)
                {
                    interpolator.Forward();
                    interpolator.Begin();
                }            
            };

            materialFSM[MaterialState.Changed].Exit = delegate
            {
                foreach (var interpolator in interpolators.Values)
                {
                    interpolator.Backward();
                    interpolator.Begin();
                }
            };

            KeyboardHandler.AddEntry(KeyCode.J, "Start Material Changer", Change);
            KeyboardHandler.AddEntry(KeyCode.K, "Reverse Material Changer", Reverse);
            KeyboardHandler.AddEntry(KeyCode.H, "Cleanup Material Changer", Cleanup);

            Refresh();
        }

        public virtual void Change()
        {
            materialFSM.JumpTo(MaterialState.Changed);
        }

        public virtual void Reverse()
        {
            materialFSM.JumpTo(MaterialState.Normal);
        }

        public virtual void Refresh()
        {
            foreach (var interpolator in interpolators.Values)
            {
                interpolator.Materials.Clear();
            }

            interpolators.Clear();

            foreach (var meshRenderer in meshRenderers)
            {
                foreach (var material in meshRenderer.materials)
                {
                    var first = (selectedInterpolator == null);
                    var addNew = false;

                    if (first)
                    {
                        selectedInterpolator = gameObject.AddComponent<TI>();                    
                    }

                    var original = selectedInterpolator.GetValue(material);

                    //Debug.LogFormat("original: {0}", original);

                    // If it is not the first, check if it exists in dictionary
                    if (!first)
                    {
                        addNew = !interpolators.TryGetValue(original, out selectedInterpolator);

                        // If it is doesn't exist in dictionary, create a new component
                        if (addNew)
                            selectedInterpolator = gameObject.AddComponent<TI>();
                    }

                    // If it is the first one or if we just created a new one and added it
                    // to the dictionary, set it up
                    if (first || addNew)
                    {
                        selectedInterpolator.ValueOriginal = original;
                        selectedInterpolator.ValueTarget = Target;
                        selectedInterpolator.Duration = Duration;
                        selectedInterpolator.Materials = new List<Material>();
                        interpolators[original] = selectedInterpolator;
                    }

                    selectedInterpolator.Materials.Add(material);                
                }
            }
        }

        public virtual void Cleanup()
        {
            foreach (var interpolator in interpolators.Values)
            {
                interpolator.Revert();
                interpolator.Materials.Clear();
                Destroy(interpolator);
            }
        }
    }
}
