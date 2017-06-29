// Copyright (c) 2017, Columbia University 
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
using UnityEngine.Networking;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// A task info that supports tasks requiring 
    /// 3D transformations meet particular requirements.
    /// </summary>
    public class MmTransformationTaskInfo : MmTaskInfo {
        #region Rotation
        /// <summary>
        /// Angle requirement.
        /// </summary>
        public float Angle;
        /// <summary>
        /// Rotation Axis requirement.
        /// </summary>
        public Vector3 Axis;
        /// <summary>
        /// Rotational requirement in Quaternion form.
        /// </summary>
        public Quaternion RotationFromTo;

        #endregion

        #region Translation
        /// <summary>
        /// Positional requirement. Can also be used for distance.
        /// </summary>
		public Vector3 Position;

        #endregion

        #region Scale
        /// <summary>
        /// Scale requirement.
        /// </summary>
		public Vector3 Scale;

        #endregion

        /// <summary>
        /// The thresholds for requirements for transformation tasks.
        /// </summary>
        public MmTransformTaskThreshold Threshold;

        /// <summary>
        /// Default constructor for MmTransformationTaskInfo.
        /// </summary>
        public MmTransformationTaskInfo()
        {}

        /// <summary>
        /// Duplicate an MmTransformationTaskInfo
        /// </summary>
        /// <param name="orig">The original MmTransformationTaskInfo.</param>
        public MmTransformationTaskInfo(MmTransformationTaskInfo orig)
        {
            Angle = orig.Angle;
            Axis = orig.Axis;
            RotationFromTo = orig.RotationFromTo;
            Position = orig.Position;
            Scale = orig.Scale;
        }

        /// <summary>
        /// Construct the MmTransformationTaskInfo from String.
        /// </summary>
        /// <param name="str"></param>
        public MmTransformationTaskInfo(string str)
        {
            Parse(str);
        }

        /// <summary>
        /// Convert the MmTransformationTaskInfo to a string.
        /// </summary>
        /// <returns>Outputs MmTransformationTaskInfo in the following form:
        /// Axis.X, Axis.Y, Axis.Z, Angle, Position.X, Position.Y, Position.Z
        /// Scale.X, Scale.Y, Scale.Z</returns>
		public override string ToString()
		{
			return string.Format("{0}," +
			                     "{1:0.0000000}," +
			                     "{2:0.0000000}," +
			                     "{3:0.0000000}," +
			                     "{4:0.0000}," +
								 "{5:0.0000000}," +
								 "{6:0.0000000}," +
								 "{7:0.0000000}," +
								 "{8:0.0000000}," +
								 "{9:0.0000000}," +
								 "{10:0.0000000}",

				base.ToString(),
				Axis.x,Axis.y, Axis.z, Angle,
				Position.x, Position.y, Position.z,
				Scale.x, Scale.y, Scale.z);
		}

        /// <summary>
        /// Parts a string to set members of MmTransformationTaskInfo.
        /// </summary>
        /// <param name="str">String to be parsed.</param>
        public override void Parse(string str)
		{
			base.Parse (str);
			var words = str.Split(',');

			Axis.x = float.Parse(words[6]);
			Axis.y = float.Parse(words[7]);
			Axis.z = float.Parse(words[8]);
			Angle  = float.Parse(words[9]);

			Position.x = float.Parse(words[10]);
			Position.y = float.Parse(words[11]);
			Position.z = float.Parse(words[12]);

			Scale.x = float.Parse(words[13]);
			Scale.y = float.Parse(words[14]);
			Scale.z = float.Parse(words[15]);
		}

        /// <summary>
        /// Generates headers for file/stream storing MmTransformationTaskInfos
        /// </summary>
        /// <returns>Outputs header in the following form:
        /// Axis.X, Axis.Y, Axis.Z, Angle, Position.X, Position.Y, Position.Z
        /// Scale.X, Scale.Y, Scale.Z</returns>
		public override string Headers()
		{
			return base.Headers() + ",Rotation.Axis.X,Rotation.Axis.Y,Rotation.Axis.Z,Rotation.Angle" +
				",Position.X,Position.Y,Position.Z" + 
				",Scale.X,Scale.Y,Scale.Z";
		}

        /// <summary>
        /// Create a duplicate MmTransformationTaskInfo.
        /// </summary>
        /// <returns>Duplicated MmTransformationTaskInfo</returns>
        public override IMmSerializable Copy()
        {
            return new MmTransformationTaskInfo(this);
        }

        /// <summary>
        /// Deserialize the task info from serialized form.
        /// </summary>
        /// <param name="reader">UNET deserializer.</param>
        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Angle = reader.ReadSingle();
            Axis = reader.ReadVector3();
            RotationFromTo = reader.ReadQuaternion();
			Position = reader.ReadVector3();
			Scale = reader.ReadVector3();

        }

        /// <summary>
        /// Serialize the task info into serialized form.
        /// </summary>
        /// <param name="writer">UNET serializer.</param>
        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Angle);
            writer.Write(Axis);
            writer.Write(RotationFromTo);
            writer.Write(Position);
            writer.Write(Scale);
        }
    }
}
