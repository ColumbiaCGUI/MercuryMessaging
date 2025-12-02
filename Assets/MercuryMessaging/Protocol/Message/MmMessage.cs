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
// Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================
//  
//
using System;

namespace MercuryMessaging
{
    /// <summary>
    /// Base class for messages passed through MmInvoke
    /// </summary>
    public class MmMessage
    {
        /// <summary>
        /// The MmMethod invoked by the calling object.
        /// </summary>
        public MmMethod MmMethod;

        /// <summary>
        /// The type of the MmMessage - useful for networking 
        /// and serialization/deserialization of messages
        /// </summary>
        public MmMessageType MmMessageType;

        /// <summary>
        /// Control parameters designating how a message should traverse an MercuryMessaging Hierarchy. 
        /// </summary>
        public MmMetadataBlock MetadataBlock;

        /// <summary>
        /// Network identifier of sender/recipient objects.
        /// </summary>
        public uint NetId;

        /// <summary>
        /// Utilized by Mercury serialization/deserialization systems.
        /// </summary>
		public bool IsDeserialized { get; private set; }

        /// <summary>
        /// Deprecated - remove in next version
        /// </summary>
        public bool root = true;

        /// <summary>
        /// Message timestamp, assists in collision avoidance
        /// </summary>
        public string TimeStamp;

        /// <summary>
        /// Hop counter for preventing infinite message loops.
        /// Incremented each time the message passes through a relay node.
        /// </summary>
        public int HopCount = 0;

        /// <summary>
        /// Maximum number of hops allowed before message is dropped.
        /// Prevents infinite loops in circular hierarchies.
        /// Default: 50 (sufficient for most complex hierarchies)
        /// </summary>
        public const int MAX_HOPS_DEFAULT = 50;

        /// <summary>
        /// Set of visited relay node instance IDs for cycle detection.
        /// Helps identify circular message paths in addition to hop counting.
        /// </summary>
        public System.Collections.Generic.HashSet<int> VisitedNodes;

        /// <summary>
        /// Indicates whether this message was obtained from the MmMessagePool.
        /// Used internally to determine if the message should be returned to pool after routing.
        /// </summary>
        internal bool _isPooled;

        /// <summary>
        /// Creates a basic MmMessage with a default control block
        /// </summary>
        public MmMessage()
        {
            MetadataBlock = new MmMetadataBlock();
            MmMessageType = MmMessageType.MmVoid;
        }

        /// <summary>
        /// Creates a basic MmMessage with the passed control block.
        /// </summary>
        /// <param name="metadataBlock">Object defining the routing of messages.</param>
        /// <param name="msgType">Type of Mercury Message.</param>
		public MmMessage(MmMetadataBlock metadataBlock, 
            MmMessageType msgType = default(MmMessageType))
		{
			MetadataBlock = new MmMetadataBlock(metadataBlock);
            MmMessageType = msgType;
        }

        /// <summary>
        /// Creates a basic MmMessage with the passed MmMessageType.
        /// </summary>
        /// <param name="msgType">Type of Mercury Message.</param>
        public MmMessage(MmMessageType msgType) : 
            this(new MmMetadataBlock(), msgType)
        {}

        /// <summary>
        /// Create an MmMessage, with defined control block and MmMethod
        /// </summary>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="msgType">Type of Mercury Message.</param>
        /// <param name="metadataBlock">Object defining the routing of messages through MercuryMessaging Hierarchys.</param>
		public MmMessage(MmMethod mmMethod,
            MmMessageType msgType = default(MmMessageType),
            MmMetadataBlock metadataBlock = null)
		{
			MmMethod = mmMethod;
            MmMessageType = msgType;

            if (metadataBlock != null)
				MetadataBlock = new MmMetadataBlock(metadataBlock);
			else
				MetadataBlock = new MmMetadataBlock();
		}

        /// <summary>
        /// Create an MmMessage, with filters defined directly
        /// </summary>
        /// <param name="mmMethod">Identifier of target MmMethod</param>
        /// <param name="levelFilter">Determines direction of messages</param>
        /// <param name="activeFilter">Determines whether message sent to active and/or inactive objects</param>
        /// <param name="selectedFilter">Determines whether message sent to objects "selected" as defined by MmRelayNode implementation</param>
        /// <param name="networkFilter">Determines whether message will remain local or can be sent over the network</param>
        /// <param name="msgType">Type of Mercury Message.</param>
        public MmMessage(MmMethod mmMethod, 
			MmLevelFilter levelFilter,
			MmActiveFilter activeFilter,
            MmSelectedFilter selectedFilter,
            MmNetworkFilter networkFilter,
            MmMessageType msgType = default(MmMessageType)
            )
        {
            MmMethod = mmMethod;
            MmMessageType = msgType;

            MetadataBlock = new MmMetadataBlock();
            MetadataBlock.LevelFilter = levelFilter;
            MetadataBlock.ActiveFilter = activeFilter;
            MetadataBlock.SelectedFilter = selectedFilter;
            MetadataBlock.NetworkFilter = networkFilter;
        }

        /// <summary>
        /// Duplicate an MmMessage
        /// </summary>
        /// <param name="message">Item to duplicate</param>
        public MmMessage(MmMessage message) :
            this(message.MmMethod,
                message.MmMessageType,
                message.MetadataBlock)
        {
            NetId = message.NetId;
            IsDeserialized = message.IsDeserialized;
            root = message.root;
            TimeStamp = message.TimeStamp;
            HopCount = message.HopCount;

            // Copy visited nodes set for cycle detection (using pool to avoid allocation)
            if (message.VisitedNodes != null)
            {
                VisitedNodes = MmHashSetPool.GetCopy(message.VisitedNodes);
            }
        }

        /// <summary>
        /// Message copy method
        /// </summary>
        /// <returns>Deep copy of message</returns>
        public virtual MmMessage Copy()
        {
            return new MmMessage(this);
        }

        /// <summary>
        /// Deserialize the MmMessage
        /// </summary>
        /// <param name="data">Object array representation of a MmMessage</param>
        /// <returns>The index of the next element to be read from data</returns>
        public virtual int Deserialize(object[] data)
        {
            int index = 0;
            MmMethod = (MercuryMessaging.MmMethod) ((short) data[index++]);
            MmMessageType = (MercuryMessaging.MmMessageType) (short)data[index++];
            NetId = (uint) ((int) data[index++]);
            index = MetadataBlock.Deserialize(data, index);
            IsDeserialized = true;
            
            return index;
        }

        /// <summary>
        /// Number of elements serialized by the base MmMessage class.
        /// Used by derived classes for efficient array pre-allocation.
        /// Base: 3 (MmMethod, MmMessageType, NetId) + Metadata: 5 = 8 total
        /// </summary>
        public const int BASE_SERIALIZED_SIZE = 8;

        /// <summary>
        /// Serialize the MmMessage
        /// </summary>
        /// <returns>Object array representation of a MmMessage</returns>
        public virtual object[] Serialize()
        {
            object[] metadataSerialized = MetadataBlock.Serialize();

            // Pre-allocate combined array: 3 base + metadata length
            object[] result = new object[3 + metadataSerialized.Length];

            // Fill base data directly
            result[0] = (short)MmMethod;
            result[1] = (short)MmMessageType;
            result[2] = (int)NetId;

            // Copy metadata using Array.Copy (no LINQ)
            Array.Copy(metadataSerialized, 0, result, 3, metadataSerialized.Length);

            return result;
        }
    }
}