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
using System;
using System.Collections.Generic;
using System.Linq;
using MercuryMessaging.Support.Extensions;
using UnityEngine;
using Matrix = UnityEngine.Matrix4x4;

namespace MercuryMessaging.Support
{
	/// <summary>
	/// General utility functions
	/// Some have been found online.
	/// Any taken from online sources should include a reference at the top of the function.
	/// </summary>
    public static class Utilities
    {
		private static float Smooth(float currentValue, float previousValue, float weight)
		{
			return (float)System.Math.Round(currentValue * weight + previousValue * (1 - weight), 3);
		}

        public static Vector2 Project2D(Vector3 v, Vector3 axis)
        {
            if (axis == Vector3.right)
                return new Vector2(v.y, v.z);
            if (axis == Vector3.up)
                return new Vector2(v.x, v.z);
            if (axis == Vector3.forward)
                return new Vector2(v.x, v.y);
            return Vector2.zero;
        }
        
        /// <summary>
        ///  http://www.mathworks.com/matlabcentral/newsreader/view_thread/151925
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double PositiveAngleBetween(Vector2 v1, Vector2 v2)
        {
            var angle = AngleBetween(v1, v2);
            // Atan2 returns a 0 to (-180) angle for clockwise rotations and 0 to 180 angle for counter-clockwise
            // We convert it to 0-360 clockwise
            if (angle < 0) return angle * -1;
            return (2*Mathf.PI) - angle;
        }

        public static double AngleBetween(Vector2 v1, Vector2 v2)
        {
            // angle = mod(atan2(x1*y2-x2*y1,x1*x2+y1*y2),2*pi);
            return System.Math.Atan2(v1.x * v2.y - v2.x * v1.y, v1.x * v2.x + v1.y * v2.y);
        }

        public static Vector3 QuaternionToYawPitchRoll(Quaternion q)
        {

            const float epsilon = 0.0009765625f;
            const float threshold = 0.5f - epsilon;

            float yaw;
            float pitch;
            float roll;

            var xy = q.x * q.y;
            var zw = q.z * q.w;

            var test = xy + zw;

            if (test < -threshold || test > threshold)
            {

                var sign = System.Math.Sign(test);

                yaw = sign * 2 * (float)System.Math.Atan2(q.x, q.w);

                pitch = sign * Mathf.PI/2;

                roll = 0;

            }
            else
            {

                var xx = q.x * q.x;
                var xz = q.x * q.z;
                var xw = q.x * q.w;

                var yy = q.y * q.y;
                var yw = q.y * q.w;
                var yz = q.y * q.z;

                var zz = q.z * q.z;

                yaw = (float)System.Math.Atan2(2 * yw - 2 * xz, 1 - 2 * yy - 2 * zz);

                pitch = (float)System.Math.Atan2(2 * xw - 2 * yz, 1 - 2 * xx - 2 * zz);

                roll = (float)System.Math.Asin(2 * test);

            }

            return new Vector3(yaw, pitch, roll);
        }

        public static EulerAnglesSimple QuaternionToYawPitchRoll2(Quaternion q)
        {
            double ww = q.w * q.w;
            double xx = q.x * q.x;
            double yy = q.y * q.y;
            double zz = q.z * q.z;

            double yz = q.y * q.z;
            double xz = q.x * q.z;
            double xw = q.x * q.w;
            double yw = q.y * q.w;
            double xy = q.x * q.y;
            double zw = q.z * q.w;

            var rotxrad = System.Math.Atan2(2.0 * (yz + xw), (-xx - yy + zz + ww));
            var rotyrad = System.Math.Asin(-2.0 * (xz - yw));
            var rotzrad = System.Math.Atan2(2.0 * (xy + zw), (xx - yy - zz + ww));

            return new EulerAnglesSimple(rotxrad, rotyrad, rotzrad);
        }

        public static float GetMax(Vector3 v)
        {
            if (v.x > v.y && v.x > v.z) return v.x;
            if (v.y > v.z) return v.x;
            return v.z;
        }

        public static float Max(float f1, float f2, float f3)
        {
            return System.Math.Max(System.Math.Max(f1, f2), f3);
        }

        public static string ConvertToOptimizedString(Matrix mat, char separator)
        {
            Quaternion rot;
            Vector3 scale;
            Vector3 trans;
            mat.GetTRS(out scale, out rot, out trans);

            return String.Format("{0}{7}{1}{7}{2}{7}{3}{7}{4}{7}{5}{7}{6}",
                trans.x, trans.y, trans.z, rot.x, rot.y, rot.z, rot.w, separator);
        }

        public static Vector2 CsvToV2(String s)
        {
            var v = Vector2.zero;

            var vals = s.Split(',');
            v.x = float.Parse(vals[0]);
            v.y = float.Parse(vals[1]);

            return v;
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public struct EulerAnglesSimple
        {
            public double X;
            public double Y;
            public double Z;

            public EulerAnglesSimple(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public static T[] Shuffle<T>(T[] iArray)
        {
            T[] array = new T[iArray.Length];
            iArray.CopyTo(array, 0);
            
            System.Random Rnd = new System.Random(System.DateTime.Now.Millisecond);

            var n = array.Length;
            while (n > 1)
            {
                var k = Rnd.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }
    }
}