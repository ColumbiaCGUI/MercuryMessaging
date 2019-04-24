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
using UnityEngine.Networking;

namespace MercuryMessaging
{
    /// <summary>
    /// MmMetadataBlock is a collection of settings 
    /// allowing you to specify the precise invocation path
    /// of an MmMessage invoked on an MmRelayNode through its
    /// MercuryMessaging hierarchy.
    /// </summary>
    public class MmMetadataBlock
    {
        /// <summary>
        /// <see cref="MmLevelFilter"/>
        /// </summary>
        public MmLevelFilter LevelFilter;

        /// <summary>
        /// <see cref="MmActiveFilter"/>
        /// </summary>
        public MmActiveFilter ActiveFilter;

        /// <summary>
        /// <see cref="MmSelectedFilter"/>
        /// </summary>
        public MmSelectedFilter SelectedFilter;
        
        /// <summary>
        /// <see cref="MmNetworkFilter"/>
        /// </summary>
        public MmNetworkFilter NetworkFilter;
        
        /// <summary>
        /// <see cref="MmTag"/>
        /// </summary>
        public MmTag Tag;

        /// <summary>
        /// Create an MmMetadataBlock
        /// </summary>
        /// <param name="levelFilter"><see cref="MmLevelFilter"/></param>
        /// <param name="activeFilter"><see cref="MmActiveFilter"/></param>
        /// <param name="selectedFilter"><see cref="MmSelectedFilter"/></param>
        /// <param name="networkFilter"><see cref="MmNetworkFilter"/></param>
        public MmMetadataBlock(
            MmLevelFilter levelFilter = MmLevelFilterHelper.Default,
            MmActiveFilter activeFilter = MmActiveFilter.Active,
            MmSelectedFilter selectedFilter = MmSelectedFilter.All,
            MmNetworkFilter networkFilter = MmNetworkFilter.All)
        {
            LevelFilter = levelFilter;
            ActiveFilter = activeFilter;
            SelectedFilter = selectedFilter;
            NetworkFilter = networkFilter;
            Tag = MmTagHelper.Everything;
        }

        /// <summary>
        /// Create an MmMetadataBlock
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="levelFilter"><see cref="MmLevelFilter"/></param>
        /// <param name="activeFilter"><see cref="MmActiveFilter"/></param>
        /// <param name="selectedFilter"><see cref="MmSelectedFilter"/></param>
        /// <param name="networkFilter"><see cref="MmNetworkFilter"/></param>
        public MmMetadataBlock(MmTag tag,
            MmLevelFilter levelFilter = MmLevelFilterHelper.Default,
            MmActiveFilter activeFilter = default(MmActiveFilter),
            MmSelectedFilter selectedFilter = default(MmSelectedFilter),
            MmNetworkFilter networkFilter = default(MmNetworkFilter))
        {
            LevelFilter = levelFilter;
            ActiveFilter = activeFilter;
            SelectedFilter = selectedFilter;
            NetworkFilter = networkFilter;
            Tag = tag;
        }

        /// <summary>
        /// Copy Constructor for MmMetadataBlock
        /// </summary>
        /// <param name="original">MmMetadataBlock to be copied.</param>
        public MmMetadataBlock (MmMetadataBlock original)
		{
			LevelFilter = original.LevelFilter;
			ActiveFilter = original.ActiveFilter;
			SelectedFilter = original.SelectedFilter;
		    NetworkFilter = original.NetworkFilter;
		    Tag = original.Tag;
		}

        /// <summary>
        /// Deserialize the MmMetadataBlock
        /// </summary>
        /// <param name="reader">UNET based deserializer object</param>
        public virtual void Deserialize(NetworkReader reader)
        {
            LevelFilter = (MmLevelFilter) reader.ReadInt16();
            ActiveFilter = (MmActiveFilter) reader.ReadInt16();
            SelectedFilter = (MmSelectedFilter) reader.ReadInt16();
            NetworkFilter = (MmNetworkFilter) reader.ReadInt16();
            Tag = (MmTag)reader.ReadInt16();
        }

        /// <summary>
        /// Serialize the MmMetadataBlock
        /// </summary>
        /// <param name="writer">UNET based serializer</param>
        public virtual void Serialize(NetworkWriter writer)
        {
            writer.Write((short) LevelFilter);
            writer.Write((short) ActiveFilter);
            writer.Write((short) SelectedFilter);
            writer.Write((short) NetworkFilter);
            writer.Write((short) Tag);
        }
    }

    /// <summary>
    /// Helper class to easily create common MercuryMessaging MetadataBlocks
    /// </summary>
    public static class MmMetadataBlockHelper
    {
        static public MmMetadataBlock Default
        {
            get
            {
                return new MmMetadataBlock(
                    default(MmTag),
                    MmLevelFilterHelper.Default,
                    default(MmActiveFilter),
                    default(MmSelectedFilter),
                    default(MmNetworkFilter)
                );
            }
        }

		static public MmMetadataBlock SelfDefaultTagAll
		{
			get
			{
				return new MmMetadataBlock(
					MmTagHelper.Everything,
					MmLevelFilter.Self,
					default(MmActiveFilter),
					default(MmSelectedFilter),
					default(MmNetworkFilter)
				);
			}
		}
    }
}