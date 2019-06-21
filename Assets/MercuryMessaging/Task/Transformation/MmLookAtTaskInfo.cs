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

namespace MercuryMessaging.Task
{
    public class MmLookAtTaskInfo : MmTransformationTaskInfo
    {
        //Viewing position
        public Vector3 Position;

        //Billboard position
        public Vector3 LookAt;
        // Optional Target orientation
        public Quaternion TargetObjOrientation; // This can be used if we want to place an object to look at
                                                // and if the orientation of that object matters
                                                // (e.g. a sign on a wall instead of a sphere)

        [SerializeField]
        protected Vector3 up = Vector3.up;

        public Quaternion Orientation;

        protected char delimiter = '|';
        
        public MmTransformTaskThreshold Threshold; // This should be enough, has angle and distance

        #region Constructors & Copiers

        public MmLookAtTaskInfo()
        { }

        public MmLookAtTaskInfo(string str)
        {
            Parse(str);
        }

        public MmLookAtTaskInfo(MmLookAtTaskInfo orig)
        {
            Position = orig.Position;
            LookAt = orig.LookAt;
            TargetObjOrientation = orig.TargetObjOrientation;

            CalculateOrientation();
        }

        protected virtual void CalculateOrientation()
        {
            Orientation = Quaternion.LookRotation(LookAt - Position, up);
        }

        public override IMmSerializable Copy()
        {
            return new MmLookAtTaskInfo(this);
        }

        #endregion

        #region Serialization & Deserialization

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}",
                base.ToString(),
                Position.ToCSV(delimiter),
                LookAt.ToCSV(delimiter),
                TargetObjOrientation.ToCSV(delimiter));
        }

        public override int Parse(string str)
        {
            base.Parse(str);

            var words = str.Split(',');

            Position = TransformExtensions.CSV2Vector3(words[16], '|');
            LookAt = TransformExtensions.CSV2Vector3(words[17], '|');
            TargetObjOrientation = TransformExtensions.CSV2Quaternion(words[18], '|');

            CalculateOrientation();

            return 19;
        }

        public override string Headers()
        {
            return string.Format("{0},{1},{2},{3}",
                base.Headers(), "Position", "LookAt", "TargetObjOrientation");
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Position = reader.ReadVector3();
            LookAt = reader.ReadVector3();
            TargetObjOrientation = reader.ReadQuaternion();

            CalculateOrientation();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Position);
            writer.Write(LookAt);
            writer.Write(TargetObjOrientation);
        }

        #endregion
    }
}