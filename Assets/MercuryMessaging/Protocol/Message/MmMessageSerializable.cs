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
using MercuryMessaging.Task;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMessage that sends a serialized object as payload
    /// You need to assign your implementation of the IMSerializable interface
    /// to the Serializable types somewhere in your program. Additionally, you
    /// need to invoke the AssignType function manually if you do not create the 
    /// message using the constructor with an IMSerializable as a parameter 
    /// or the copy constructor.
    /// </summary>
    public class MmMessageSerializable : MmMessage
    {
        /// <summary>
        /// Serialized item Payload
        /// Item needs to implement IMmSerializable
        /// </summary>
        public IMmSerializable value;

        /// <summary>
        /// Creates a basic MmMessageSerializable
        /// </summary>
        public MmMessageSerializable()
        {
        }

        /// <summary>
        /// Creates a basic MmMessageSerializable, with a control block
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageSerializable(MmMetadataBlock metadataBlock = null)
            : base (metadataBlock, MmMessageType.MmSerializable)
        {
        }

        /// <summary>
        /// Create an MmMessage, with control block, MmMethod, and an int
        /// </summary>
        /// <param name="iVal">Serializable Payload</param>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="metadataBlock">Object defining the routing of messages</param>
        public MmMessageSerializable(IMmSerializable iVal,
            MmMethod mmMethod = default(MmMethod),
            MmMetadataBlock metadataBlock = null)
            : base(mmMethod, MmMessageType.MmSerializable, metadataBlock)
        {
            value = iVal;
        }

        /// <summary>
        /// Duplicate an MmMessageSerializable
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessageSerializable(MmMessageSerializable message) : base(message)
        {
        }

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Duplicate of MmMessage</returns>
        public override MmMessage Copy()
        {
            MmMessageSerializable newMessage = new MmMessageSerializable (this);

            newMessage.value = value.Copy();
            return newMessage;
        }

        /// <summary>
        /// Deserialize the MmMessageSerializable
        /// </summary>
        /// <param name="data">Object array representation of a MmMessageSerializable</param>
        /// <returns>The index of the next element to be read from data</returns>
        public override int Deserialize(object[] data)
        {
            int index = base.Deserialize(data);
            Type type = Type.GetType((string) data[index++]);
            value = (IMmSerializable) Activator.CreateInstance(type);
            Debug.Log(index);
            Debug.Log(data.Length);
            index = value.Deserialize(data, index);
            return index;
        }

        /// <summary>
        /// Serialize the MmMessageSerializable
        /// </summary>
        /// <returns>Object array representation of a MmMessageSerializable</returns>
        public override object[] Serialize()
        {
            object[] baseSerialized = base.Serialize();
            object[] thisSerialized = new object[] { value.GetType().ToString() };
            thisSerialized = thisSerialized.Concat(value.Serialize()).ToArray();
            object[] combinedSerialized = baseSerialized.Concat(thisSerialized).ToArray();
            return combinedSerialized;
        }
    }
}
