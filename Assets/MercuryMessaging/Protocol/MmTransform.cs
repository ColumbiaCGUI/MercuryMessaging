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
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// A struct containing the translation, rotation and scale
    /// of a transformation.
    /// </summary>
	public struct MmTransform
	{
        /// <summary>
        /// Translation.
        /// </summary>
		public Vector3 Translation;

        /// <summary>
        /// Scale.
        /// </summary>
		public Vector3 Scale;

        /// <summary>
        /// Rotation.
        /// </summary>
		public Quaternion Rotation;

        /// <summary>
        /// Create an MmTransform from translation,
        /// rotation, and scale directly.
        /// </summary>
        /// <param name="iTranslation"></param>
        /// <param name="iScale"></param>
        /// <param name="iRotation"></param>
		public MmTransform(Vector3 iTranslation, Vector3 iScale, 
			Quaternion iRotation)
		{
			Translation = iTranslation;
			Scale = iScale;
			Rotation = iRotation;
		}

        /// <summary>
        /// Create an MmTransform from a UnityEngine.Transform.
        /// </summary>
        /// <param name="iTransform">UnityEngine.Transform component.</param>
        /// <param name="useGlobal">Use Global or Local transform values.</param>
        public MmTransform(Transform iTransform, bool useGlobal)
		{
			iTransform.GetPosRotScale (out Translation, out Rotation, out Scale, useGlobal);
		}

        /// <summary>
        /// Copy constructor for MmTransform.
        /// </summary>
        /// <param name="iMmTransform">MmTransform to be copied.</param>
		public MmTransform(MmTransform iMmTransform)
		{
			Translation = iMmTransform.Translation;
			Rotation = iMmTransform.Rotation;
			Scale = iMmTransform.Scale;
		}

        /// <summary>
        /// Deserialize the MmTransform
        /// </summary>
        /// <param name="reader">UNET based deserializer object</param>
        public void Deserialize(NetworkReader reader)
		{
			Translation = reader.ReadVector3();
			Rotation = reader.ReadQuaternion();
			Scale = reader.ReadVector3();
		}

        /// <summary>
        /// Serialize the MmTransform
        /// </summary>
        /// <param name="writer">UNET based serializer</param>
        public void Serialize(NetworkWriter writer)
		{
			writer.Write (Translation);
			writer.Write (Rotation);
			writer.Write (Scale);
		}
	}
}
