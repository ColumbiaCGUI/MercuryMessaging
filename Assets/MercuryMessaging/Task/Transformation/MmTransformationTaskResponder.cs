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

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Responder useful for creating MmTaskResponders handling
    /// transformation tasks in Unity
    /// </summary>
    /// <typeparam name="T">Must be of MmTransformationTaskInfo</typeparam>
    public class MmTransformationTaskResponder<T> : MmTaskResponder<T> where T: 
        MmTransformationTaskInfo, new()
    {
        /// <summary>
        /// Tranformation Mode, supports: rotation, translation, scale, and combinations.
        /// </summary>
        [EnumFlag]
        public MmTransformationType transformationTypeMode = MmTransformTypeHelper.TranslationAndRotation;

        /// <summary>
        /// Tranformation Mode, supports: rotation, translation, scale, and combinations.
        /// </summary>
        public MmTransformationType TransformationTypeMode
        {
            get { return transformationTypeMode; }
            set { transformationTypeMode = value; }
        }

        #region Rotation

        /// <summary>
        /// Angle of rotation
        /// </summary>
        protected float angle;
        /// <summary>
        /// Axis of rotation
        /// </summary>
        protected Vector3 axis;
        /// <summary>
        /// Rotation of task represented in quaternion form.
        /// </summary>
        protected Quaternion rotationFromTo;

        #endregion

        #region Translation

        /// <summary>
        /// Distance for task completion.
        /// </summary>
        protected float distance;

        /// <summary>
        /// Method to return distance requirement of task.
        /// </summary>
        /// <returns>Stored distance requirement of task.</returns>
        public float Distance() { return distance; }

        #endregion

        #region Scale

        /// <summary>
        /// Scale offset for task completion.
        /// </summary>
        protected float scaleOffset;

        /// <summary>
        /// Method to return scale requirement of task.
        /// </summary>
        /// <returns>Stored scale differential requirement of task.</returns>
        public float ScaleOffset() { return scaleOffset; }

        #endregion

        /// <summary>
        /// Task completion check determines completion based on 
        /// TransformationTypeMode.
        /// Supports checking multiple types of transformations simultaneously.
        /// </summary>
        /// <returns>True on task complete.</returns>
        public override bool TaskCompleteCheck()
        {
            bool isComplete = true;

            if ((TransformationTypeMode & MmTransformationType.Rotation) > 0)
            {
                isComplete = isComplete && (angle < mmTaskInfo.Threshold.AngleThreshold);
            }

            if ((TransformationTypeMode & MmTransformationType.Translation) > 0)
            {
                isComplete = isComplete && (distance < mmTaskInfo.Threshold.DistanceThreshold);
            }

            if ((TransformationTypeMode & MmTransformationType.Scale) > 0)
            {
                isComplete = isComplete && (scaleOffset < mmTaskInfo.Threshold.ScaleThreshold);
            }

            return isComplete;
        }

        /// <summary>
        /// Reset calculation values to identity or zero.
        /// </summary>
        public virtual void Reset()
        {
            if ((TransformationTypeMode & MmTransformationType.Rotation) > 0) transform.rotation = Quaternion.identity;
            if ((TransformationTypeMode & MmTransformationType.Translation) > 0) transform.position = Vector3.zero;
            if ((TransformationTypeMode & MmTransformationType.Scale) > 0) transform.localScale = Vector3.one;
        }
    }
}
