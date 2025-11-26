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
using Matrix = UnityEngine.Matrix4x4;

namespace MercuryMessaging.Support.Extensions
{
    /// <summary>
    /// Some helpful matrix functions if creating applications using 
    /// MercuryMessaging.
    /// </summary>
    public static class MatrixUtilities {
    
        /// <summary>
        /// Create translation matrix from vector 3
        /// </summary>
        /// <param name="translation">Input translation.</param>
        /// <returns></returns>
        public static Matrix CreateTranslationMatrix(Vector3 translation)
        {
            return Matrix.TRS(translation, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Create translation matrix from 3 floats
        /// </summary>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="z">Z-value</param>
        /// <returns>Translation Matrix.</returns>
        public static Matrix CreateTranslationMatrix(float x, float y, float z)
        {
            return Matrix.TRS(new Vector3(x, y, z), Quaternion.identity, Vector3.one);
        }
	
        /// <summary>
        /// Create rotation matrix from quaternion.
        /// </summary>
        /// <param name="rotation">Input rotation.</param>
        /// <returns>Rotation Matrix.</returns>
        public static Matrix CreateRotationMatrix(Quaternion rotation)
        {
            return Matrix.TRS(Vector3.zero, rotation, Vector3.one);
        }

        /// <summary>
        /// Create rotation from euler angles provided in 3 floats.
        /// </summary>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="z">Z-value</param>
        /// <returns>Rotation matrix.</returns>
        public static Matrix CreateRotationMatrix(float x, float y, float z)
        {
            return Matrix.TRS(Vector3.zero, Quaternion.Euler(new Vector3(x, y, z)), Vector3.one); 
        }
	
        /// <summary>
        /// Create scale matrix from vector3
        /// </summary>
        /// <param name="scale">Input scale.</param>
        /// <returns>Scale Matrix.</returns>
        public static Matrix CreateScaleMatrix(Vector3 scale)
        {
            return Matrix.TRS(Vector3.zero, Quaternion.identity, scale); 
        }
	
        /// <summary>
        /// Get quaternion from matrix.
        /// </summary>
        /// <param name="matrix">Input matrix.</param>
        /// <returns>Rotation in quaternion form.</returns>
        public static Quaternion GetRotation(this Matrix matrix)
        {
		
            return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
		
        }
	
        /// <summary>
        /// Subtract one matrix from another.
        /// </summary>
        /// <param name="lhs">Left-hand side.</param>
        /// <param name="rhs">Right-hand side</param>
        /// <returns>Difference.</returns>
        public static Matrix SubtractMatrix(this Matrix lhs, Matrix rhs)
        {
            Matrix result = Matrix.identity;
		
            result.SetColumn(0, lhs.GetColumn(0) - rhs.GetColumn(0));
            result.SetColumn(1, lhs.GetColumn(1) - rhs.GetColumn(1));
            result.SetColumn(2, lhs.GetColumn(2) - rhs.GetColumn(2));
            result.SetColumn(3, lhs.GetColumn(3) - rhs.GetColumn(3));
		
            return result;
        }

        /// <summary>
        /// Add two matricies together.
        /// </summary>
        /// <param name="lhs">Left-hand side.</param>
        /// <param name="rhs">Right-hand side</param>
        /// <returns>Sum.</returns>
        public static Matrix AddMatrix(this Matrix lhs, Matrix rhs)
        {
            Matrix result = Matrix.identity;
		
            result.SetColumn(0, lhs.GetColumn(0) + rhs.GetColumn(0));
            result.SetColumn(1, lhs.GetColumn(1) + rhs.GetColumn(1));
            result.SetColumn(2, lhs.GetColumn(2) + rhs.GetColumn(2));
            result.SetColumn(3, lhs.GetColumn(3) + rhs.GetColumn(3));
		
            return result;
        }

        /// <summary>
        /// Position from matrix.
        /// http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
        /// </summary>
        /// <param name="m">Input matrix.</param>
        /// <returns>The from matrix.</returns>
        public static Vector3 GetPosition(this Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        /// <summary>
        /// Scale from matrix.
        /// http://answers.unity3d.com/questions/402280/how-to-decompose-a-trs-matrix.html
        /// </summary>
        /// <param name="m">Input matrix.</param> 
        /// <returns>The from matrix.</returns>
        public static Vector3 ScaleFromMatrix(this Matrix4x4 m)
        {
            // Extract new local scale
            return new Vector3(
                m.GetColumn(0).magnitude,
                m.GetColumn(1).magnitude,
                m.GetColumn(2).magnitude
                );
        }

        /// <summary>
        /// Assign matrix into Transform's global transformation.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="mat">Values from this matrix will be applied to the GameObject.</param>
        public static void AssignGlobalTransform(GameObject gameObject, Matrix mat)
        {
            gameObject.transform.position = mat.GetPosition();
            gameObject.transform.rotation = mat.GetRotation();
            gameObject.transform.localScale = mat.ScaleFromMatrix ();
        }

        /// <summary>
        /// Assign matrix into Transform's local transformation.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="mat">Values from this matrix will be applied to the GameObject.</param>
        public static void AssignLocalTransform(GameObject gameObject, Matrix mat)
        {
            gameObject.transform.localPosition = mat.GetPosition();
            gameObject.transform.localRotation = mat.GetRotation();
            gameObject.transform.localScale = mat.ScaleFromMatrix ();
        }

        /// <summary>
        /// Assign matrix into Transform's local or global transformation.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="mat">Values from this matrix will be applied to the GameObject.</param>
        /// <param name="useGlobal">Whether to assign to global or local transform components.</param>
        public static void AssignTransform(GameObject gameObject, Matrix mat, bool useGlobal)
        {
            if (useGlobal)
                AssignGlobalTransform (gameObject, mat);
            else
                AssignLocalTransform (gameObject, mat);
        }

        /// <summary>
        /// Extract Translation, Rotation, and Scale given a Matrix.
        /// </summary>
        /// <param name="m">Input Matrix.</param>
        /// <param name="trans">Output translation.</param>
        /// <param name="rot">Output rotation.</param>
        /// <param name="scale">Output scale.</param>
        public static void GetTRS(this Matrix m, out Vector3 trans, out Quaternion rot, out Vector3 scale)
        {
            trans = m.GetPosition();
            rot = m.GetRotation();
            scale = m.ScaleFromMatrix();
        }

        /// <summary>
        /// Print Matrix Debug Log based on conversion to Row-major systems
        /// Note that Unity is Column Major - but accessors below are still Row,Column 
        /// </summary>
        /// <param name="mat">Matrix to print.</param>
        /// <param name="name">Name of Matrix.</param>
        public static void MatrixPrint(this Matrix4x4 mat, string name)
        {
            Debug.Log 
                ( name + " = [" +
                  mat.m00 + ", " +
                  mat.m10 + ", " +
                  mat.m20 + ", " +
                  mat.m30 + "; " +
                  mat.m01 + ", " +
                  mat.m11 + ", " +
                  mat.m21 + ", " +
                  mat.m31 + "; " +
                  mat.m02 + ", " +
                  mat.m12 + ", " +
                  mat.m22 + ", " +
                  mat.m32 + "; " +
                  mat.m03 + ", " +
                  mat.m13 + ", " +
                  mat.m23 + ", " +
                  mat.m33 + "]; "
                );
        }

        /// <summary>
        /// Print Matrix String based on conversion to Row-major systems
        /// Note that Unity is Column Major - but accessors below are still Row,Column 
        /// </summary>
        /// <param name="mat">Matrix to print.</param>
        /// <param name="name">Name of matrix.</param>
        public static string MatrixPrintS(this Matrix4x4 mat, string name)
        {
            return
                name + " = [" +
                mat.m00 + ", " +
                mat.m10 + ", " +
                mat.m20 + ", " +
                mat.m30 + "; " +
                mat.m01 + ", " +
                mat.m11 + ", " +
                mat.m21 + ", " +
                mat.m31 + "; " +
                mat.m02 + ", " +
                mat.m12 + ", " +
                mat.m22 + ", " +
                mat.m32 + "; " +
                mat.m03 + ", " +
                mat.m13 + ", " +
                mat.m23 + ", " +
                mat.m33 + "]; "
                ;	
        }

        /// <summary>
        /// Prints the full matrix transformation decomposition for translation and rotation.
        /// </summary>
        /// <param name="trans">Print Matrix Transformation.</param>
        /// <param name="name">Print Matrix Name.</param>
        public static void PrintFullMatrixTransRotDecomp(this Matrix4x4 trans, string name)
        {
            Vector3 pos = trans.GetPosition();
            Quaternion rot = trans.GetRotation();
            Debug.Log(name + ": " + " Pos:" + pos.ToString("0.00000")
                      + System.Environment.NewLine 
                      + " Rot:" + rot.ToString("0.00000"));
        }

        /// <summary>
        /// Prints the full matrix transformation decomposition for translation and rotation.
        /// </summary>
        /// <param name="name">Print Matrix Name.</param>
        /// <param name="trans">Print Matrix Transformation.</param>
        public static void PrintTransRot(string name, Vector3 pos, Quaternion rot)
        {
            Debug.Log(name + ": " + " Pos:" + pos.ToString("0.00000")
                      + System.Environment.NewLine 
                      + " Rot:" + rot.ToString("0.00000"));
        }
    }
}
