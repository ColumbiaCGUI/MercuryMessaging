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
using MercuryMessaging.Support.Extensions;
using UnityEngine;

namespace MercuryMessaging.Support.Interpolators
{
    public class InterpolatorOrbit : Interpolator<Matrix4x4>
    {
        private float beginLength, endLength;

        public override void Begin()
        {
            beginLength = valueBegin.GetPosition().magnitude;
            endLength = valueEnd.GetPosition().magnitude;

            base.Begin();
        }

        public void SetSpeed(float speed)
        {
            var dist = CalculateDistance();

            Duration = (dist / speed);
        }

        public override void Apply()
        {
            //Notifier.AddMessage(StepCountTotal.ToString());
            var f = Mathf.SmoothStep(0, 1, DurationCurrent / Duration);
            //var f = (float)_step / _steps;
            var l = Mathf.Lerp(beginLength, endLength, f);

            var d = Vector3.Lerp(valueBegin.GetPosition(), valueEnd.GetPosition(), f);
            d.Normalize();
            d *= l;

            ValueCurrent =
                MatrixUtilities.CreateRotationMatrix(Quaternion.Slerp(ValueOriginal.GetRotation(), ValueTarget.GetRotation(), f))
                * MatrixUtilities.CreateTranslationMatrix(d);

            transform.position = d;
            transform.rotation = Quaternion.Slerp(ValueOriginal.GetRotation(), ValueTarget.GetRotation(), f);
        }

        public override void Revert()
        {
            transform.position = ValueOriginal.GetPosition();
            transform.rotation = ValueOriginal.GetRotation();
        }

        private float CalculateDistance()
        {
            // Orbit distance
            var v1 = Utilities.Project2D(ValueOriginal.inverse.GetPosition(), Vector3.forward);
            v1.Normalize();

            var v2 = Utilities.Project2D(ValueTarget.inverse.GetPosition(), Vector3.forward);
            v2.Normalize();

            var angle = Mathf.Abs(Vector3.Angle(v1, v2));

            // Rotate distance
            v1 = Utilities.Project2D(ValueOriginal.GetPosition(), Vector3.forward);
            v1.Normalize();

            v2 = Utilities.Project2D(ValueTarget.GetPosition(), Vector3.forward);
            v2.Normalize();

            var angle2 = Mathf.Abs(Vector3.Angle(v1, v2));

            if (angle2 > angle) angle = angle2;

            var radius = (ValueOriginal.inverse.GetPosition().magnitude + ValueTarget.inverse.GetPosition().magnitude) / 2;
        
            var arcDist = radius * angle;
            var lineDist = (ValueTarget.inverse.GetPosition() - ValueOriginal.inverse.GetPosition()).magnitude;

            return (lineDist > arcDist) ? lineDist : arcDist;
        }
    }
}