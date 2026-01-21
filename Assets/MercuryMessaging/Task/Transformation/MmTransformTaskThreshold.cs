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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================
//
//
// Suppress CS0618: IMmSerializable is obsolete - kept for backward compatibility
#pragma warning disable CS0618

using System;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// This class contains transformation threshold information for 
    /// MmTransformationTaskInfos
    /// </summary>
    [Serializable]
    public class MmTransformTaskThreshold : IMmSerializable {

        /// <summary>
        /// MmRelayNode angular (in degrees) completion threshold.
        /// </summary>
        public float AngleThreshold = 5;

        /// <summary>
        /// MmRelayNode positional completion threshold.
        /// </summary>
        public float DistanceThreshold = float.MaxValue;

        /// <summary>
        /// MmRelayNode scalar completion threshold.
        /// </summary>
        public float ScaleThreshold = float.MaxValue;

        /// <summary>
        /// Create a default MmTransformTaskThreshold.
        /// </summary>
        public MmTransformTaskThreshold()
        {}

        /// <summary>
        /// Construct an MmTransformTaskThreshold by setting angle, distance and scale directly.
        /// </summary>
        /// <param name="angleThreshold">Angular threshold.</param>
        /// <param name="distanceThreshold">Distance threshold.</param>
        /// <param name="scaleThreshold">Scale threshold.</param>
        public MmTransformTaskThreshold(float angleThreshold, float distanceThreshold, float scaleThreshold)
        {
            AngleThreshold = angleThreshold;
            DistanceThreshold = distanceThreshold;
            ScaleThreshold = scaleThreshold;
        }

        /// <summary>
        /// Construct MmTransformTaskThreshold by duplicating another threshold.
        /// </summary>
        /// <param name="orig">Original threshold</param>
        public MmTransformTaskThreshold(MmTransformTaskThreshold orig)
        {
            AngleThreshold = orig.AngleThreshold;
            DistanceThreshold = orig.DistanceThreshold;
            ScaleThreshold = orig.ScaleThreshold;
        }

        /// <summary>
        /// Duplicate the task threshold.
        /// </summary>
        /// <returns>Copy of MmTransformTaskThreshold</returns>
        public IMmSerializable Copy()
        {
            return new MmTransformTaskThreshold(this);
        }

        /// <summary>
        /// Deserialize the MmTransformTaskThreshold
        /// </summary>
        /// <param name="data">Object array representation of a MmTransformTaskThreshold</param>
        /// <param name="index">The index of the next element to be read from data</param>
        /// <returns>The index of the next element to be read from data</returns>
        public int Deserialize(object[] data, int index)
        {
            AngleThreshold = (float) data[index++];
            DistanceThreshold = (float) data[index++];
            ScaleThreshold = (float) data[index++];
            return index;
        }

        /// <summary>
        /// Serialize the MmTransformTaskThreshold
        /// </summary>
        /// <returns>Object array representation of a MmTransformTaskThreshold</returns>
        public object[] Serialize()
        {
            object[] thisSerialized = new object[] { AngleThreshold, DistanceThreshold, ScaleThreshold };
            return thisSerialized;
        }
    }
}
