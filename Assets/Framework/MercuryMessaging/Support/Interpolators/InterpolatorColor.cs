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
using MercuryMessaging.Support.Extensions;
using UnityEngine;

namespace MercuryMessaging.Support.Interpolators
{
    public class InterpolatorColor : Interpolator<Color>, InterpolatorMaterial<Color>
    {
        public List<Material> Materials { get; set; }

        public Color GetValue(Material material)
        {
            return material.color;
        }

        private HSBColor hsbValueBegin;
        private HSBColor hsbValueEnd;
        private HSBColor hsbValueCurrent;

        public override void Begin()
        {
            hsbValueBegin = new HSBColor(valueBegin);
            hsbValueEnd = new HSBColor(valueEnd);

            base.Begin();
        }

        public override void Apply()
        {
            //var f = MathHelper.SmoothStep(0, 1, (float)durationCurrent / StepCountTotal);
            var f = DurationCurrent / Duration;

            hsbValueCurrent = HSBColor.Lerp(hsbValueBegin, hsbValueEnd, f);

            ValueCurrent = hsbValueCurrent.ToColor();

            foreach (var material in Materials)
            {
                material.color = ValueCurrent;
            }
        }

        public override void Revert()
        {
            foreach (var material in Materials)
            {
                material.color = ValueOriginal;
            }
        }    
    }
}