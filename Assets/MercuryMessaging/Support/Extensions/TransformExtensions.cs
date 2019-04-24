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
using UnityEngine;

namespace MercuryMessaging.Support.Extensions
{
    /// <summary>
    /// Collection of GameObject extensions useful if creating applications
    /// using MercuryMessaging.
    /// </summary>
    public static class TransformExtensions {

        /// <summary>
        /// Set global or local position member of transform.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="position">Input position.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        public static void SetPosition(this Transform transform, Vector3 position, bool useGlobal)
        {
            if (useGlobal)
            {
                transform.position = position;
            }
            else
            {
                transform.localPosition = position;
            }
        }

        /// <summary>
        /// Set global or local rotation member of transform.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="rotation">Input Rotation.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        public static void SetRotation(this Transform transform, Quaternion rotation, bool useGlobal)
        {
            if (useGlobal)
            {
                transform.rotation = rotation;
            }
            else
            {
                transform.localRotation = rotation;
            }
        }

        /// <summary>
        /// Set global or local position and rotation members of transform.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="position">Input Position.</param>
        /// <param name="rotation">Input Rotation.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        public static void SetPosRot(this Transform transform, Vector3 position,
            Quaternion rotation, bool useGlobal)
        {
            SetPosition(transform, position, useGlobal);
            SetRotation(transform, rotation, useGlobal);
        }

        /// <summary>
        /// Set global or local position and rotation members of transform.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="position">Input Position.</param>
        /// <param name="rotation">Input Rotation.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        public static void SetPosRotScale(this Transform transform, Vector3 position,
			Quaternion rotation, Vector3 scale, bool useGlobal)
		{
			SetPosition(transform, position, useGlobal);
			SetRotation(transform, rotation, useGlobal);
			transform.localScale = scale;
		}

        /// <summary>
        /// Get global or local position member of transform.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        /// <returns>Position of transform.</returns>
        public static Vector3 GetPosition(this Transform transform, bool useGlobal)
        {
            if (useGlobal)
            {
                return transform.position;
            }
            else
            {
                return transform.localPosition;
            }
        }

        /// <summary>
        /// Get global or local rotation member of transform.
        /// </summary>
        /// <param name="transform">Observed Transform.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        /// <returns>Rotation as Quaternion.</returns>
        public static Quaternion GetRotation(this Transform transform, bool useGlobal)
        {
            if (useGlobal)
            {
                return transform.rotation;
            }
            else
            {
                return transform.localRotation;
            }
        }

        /// <summary>
        /// Get global or local scale member of transform.
        /// </summary>
        /// <param name="transform">Observed Transform.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        /// <returns>Get scale of transform.</returns>
        public static Vector3 GetScale(this Transform transform, bool useGlobal)
		{
			if (useGlobal)
			{
				return transform.lossyScale;
			}
			else
			{
				return transform.localScale;
			}
		}

        /// <summary>
        /// Returns position and rotation of the transformation in a 
        /// single function call
        /// </summary>
        /// <param name="transform">Observed Transform.</param>
        /// <param name="position">Target position.</param>
        /// <param name="rotation">Target rotation.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
        public static void GetPosRot(this Transform transform, out Vector3 position,
            out Quaternion rotation, bool useGlobal)
        {
            position = transform.GetPosition(useGlobal);
            rotation = transform.GetRotation(useGlobal);
        }

        /// <summary>
        /// Returns position, rotation, and scale of the transformation in a 
        /// single function call
        /// </summary>
        /// <param name="transform">Observed Transform.</param>
        /// <param name="position">Target position.</param>
        /// <param name="rotation">Target rotation.</param>
        /// <param name="scale">Target scale.</param>
        /// <param name="useGlobal">Whether to use global or local transformation values.</param>
		public static void GetPosRotScale(this Transform transform, out Vector3 position,
			out Quaternion rotation, out Vector3 scale, bool useGlobal)
		{
			position = transform.GetPosition(useGlobal);
			rotation = transform.GetRotation(useGlobal);
			scale = transform.GetScale (useGlobal);
		}

        /// <summary>
        /// Copy position of another transformation.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="other">Source Transform.</param>
        /// <param name="setGlobal">Whether to use global or local transformation values.</param>
        public static void CopyPosition(this Transform transform, Transform other, bool setGlobal)
        {
            if (setGlobal)
            {
                transform.position = other.position;
            }
            else
            {
                transform.localPosition = other.localPosition;
            }
        }

        /// <summary>
        /// Copy rotation of another transformation.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="other">Source trasnform.</param>
        /// <param name="setGlobal">Whether to use global or local transformation values.</param>
        public static void CopyRotation(this Transform transform, Transform other, bool setGlobal)
        {
            if (setGlobal)
            {
                transform.rotation = other.rotation;
            }
            else
            {
                transform.localRotation = other.localRotation;
            }
        }

        /// <summary>
        /// Copy position and rotation of another transformation.
        /// </summary>
        /// <param name="transform">Target Transform.</param>
        /// <param name="other">Source transform.</param>
        /// <param name="setGlobal">Whether to use global or local transformation values.</param>
        public static void CopyTransform(this Transform transform, Transform other, bool setGlobal)
        {
            CopyPosition(transform, other, setGlobal);
            CopyRotation(transform, other, setGlobal);
			transform.localScale = other.localScale;
        }

        //public static string ToCSV(this Vector2 v, char delim = ',', string format = "0.0000")
        //{
        //    return string.Format("{0}{2}{1}",
        //        v.x.ToString(format),
        //        v.y.ToString(format),
        //        delim);
        //}

        /// <summary>
        /// Create CSV formatted string from Vector3
        /// </summary>
        /// <param name="v">Source Vector3.</param>
        /// <param name="delim">Delimiter.</param>
        /// <param name="format">String format.</param>
        /// <returns>Formatted string.</returns>
        public static string ToCSV(this Vector3 v, char delim = ',', string format = "0.0000")
        {
            return string.Format("{0}{3}{1}{3}{2}",
                v.x.ToString(format),
                v.y.ToString(format),
                v.z.ToString(format),
                delim);
        }

        /// <summary>
        /// Create CSV formatted string from Vector2
        /// </summary>
        /// <param name="v">Source Vector3.</param>
        /// <param name="delim">Delimiter.</param>
        /// <param name="format">String format.</param>
        /// <returns>Formatted string.</returns>
        public static string ToCSV(this Vector2 v, char delim = ',', string format = "0.0000")
        {
            return string.Format("{0}{3}{1}{3}{2}",
                v.x.ToString(format),
                v.y.ToString(format),
                delim);
        }

        /// <summary>
        /// Create CSV formatted string from Quaternion.
        /// </summary>
        /// <param name="q">Source Vector3.</param>
        /// <param name="delim">Delimiter.</param>
        /// <param name="format">String format.</param>
        /// <returns>Formatted string.</returns>
        public static string ToCSV(this Quaternion q, char delim = ',', string format = "0.0000")
        {
            return string.Format("{0}{4}{1}{4}{2}{4}{3}",
                q.x.ToString(format),
                q.y.ToString(format),
                q.z.ToString(format),
                q.w.ToString(format),
                delim);
        }

        /// <summary>
        /// Get Quaternion from CSV formatted string.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="delim">Delimiter.</param>
        /// <returns>New Quaternion.</returns>
        public static Quaternion CSV2Quaternion(string s, char delim = ',')
        {
            var v = s.Split(delim);

            return new Quaternion(
                float.Parse(v[0]),
                float.Parse(v[1]),
                float.Parse(v[2]),
                float.Parse(v[3])
            );
        }

        /// <summary>
        /// Get Vector3 from CSV formatted string.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="delim">Delimiter.</param>
        /// <returns>New Vector3.</returns>
        public static Vector3 CSV2Vector3(string s, char delim = ',')
        {
            var v = s.Split(delim);

            return new Vector3(
                float.Parse(v[0]),
                float.Parse(v[1]),
                float.Parse(v[2])
            );
        }

        /// <summary>
        /// Get Vector2 from CSV formatted string.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="delim">Delimiter.</param>
        /// <returns>New Vector2.</returns>
        public static Vector3 CSV2Vector2(string s, char delim = ',')
        {
            var v = s.Split(delim);

            return new Vector2(
                float.Parse(v[0]),
                float.Parse(v[1])
            );
        }
    }
}
