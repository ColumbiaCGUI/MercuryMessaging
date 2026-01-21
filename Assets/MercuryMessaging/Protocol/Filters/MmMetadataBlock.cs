// Copyright (c) 2017-2025, Columbia University
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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================
//  
//
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
        /// Advanced routing options for this message (null = use defaults).
        /// Part of Phase 2.1: Advanced Message Routing.
        /// Optional - defaults to null for backward compatibility.
        /// </summary>
        public MmRoutingOptions Options;

        /// <summary>
        /// Explicit routing path specification (e.g., "parent/sibling/child").
        /// Part of Phase 2.1: Advanced Message Routing.
        /// Optional - null = use LevelFilter instead.
        /// </summary>
        public string ExplicitRoutePath;

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
		    Options = original.Options?.Clone(); // Deep copy if present
		    ExplicitRoutePath = original.ExplicitRoutePath;
		}

        /// <summary>
        /// Deserialize the MmMetadataBlock
        /// </summary>
        /// <param name="data">Object array representation of a MmMetadataBlock</param>
        /// <param name="index">The index of the next element to be read from data</param>
        /// <returns>The index of the next element to be read from data</returns>
        /// <remarks>
        /// Note: Options and ExplicitRoutePath are NOT serialized/deserialized.
        /// They are local routing hints and should not be transmitted over network.
        /// MmRoutingOptions contains function delegates that cannot be serialized.
        /// </remarks>
        public virtual int Deserialize(object[] data, int index)
        {
            LevelFilter = (MercuryMessaging.MmLevelFilter) ((short) data[index++]);
            ActiveFilter = (MercuryMessaging.MmActiveFilter) ((short) data[index++]);
            SelectedFilter = (MercuryMessaging.MmSelectedFilter) ((short) data[index++]);
            NetworkFilter = (MercuryMessaging.MmNetworkFilter) ((short) data[index++]);
            Tag = (MercuryMessaging.MmTag) ((short) data[index++]);
            // Options and ExplicitRoutePath intentionally not deserialized
            return index;
        }

        /// <summary>
        /// Serialize the MmMetadataBlock
        /// </summary>
        /// <returns>Object array representation of a MmMetadataBlock</returns>
        /// <remarks>
        /// Note: Options and ExplicitRoutePath are NOT serialized/deserialized.
        /// They are local routing hints and should not be transmitted over network.
        /// MmRoutingOptions contains function delegates that cannot be serialized.
        /// </remarks>
        public virtual object[] Serialize()
        {
            object[] thisSerialized = new object[] {
                (short) LevelFilter,
                (short) ActiveFilter,
                (short) SelectedFilter,
                (short) NetworkFilter,
                (short) Tag
            };
            // Options and ExplicitRoutePath intentionally not serialized
            return thisSerialized;
        }
    }

    /// <summary>
    /// Helper class to easily create common MercuryMessaging MetadataBlocks.
    /// Phase 6 Optimization: Cached instances to avoid allocation on every access.
    /// WARNING: These cached instances are READ-ONLY. Do not modify their fields.
    /// If you need to modify a MetadataBlock, use the copy constructor: new MmMetadataBlock(MmMetadataBlockHelper.Default)
    /// </summary>
    public static class MmMetadataBlockHelper
    {
        // Phase 6: Cached instances to avoid allocation per access
        private static readonly MmMetadataBlock _default = new MmMetadataBlock(
            MmTagHelper.Everything,  // Use Everything to bypass tag filtering (default(MmTag) = Nothing = 0 would fail all tag checks)
            MmLevelFilterHelper.Default,
            default(MmActiveFilter),
            default(MmSelectedFilter),
            default(MmNetworkFilter)
        );

        private static readonly MmMetadataBlock _selfDefaultTagAll = new MmMetadataBlock(
            MmTagHelper.Everything,
            MmLevelFilter.Self,
            default(MmActiveFilter),
            default(MmSelectedFilter),
            default(MmNetworkFilter)
        );

        /// <summary>
        /// Returns a cached default MmMetadataBlock (SelfAndChildren, Everything tag).
        /// WARNING: This is a shared instance - do NOT modify its fields.
        /// Use new MmMetadataBlock(MmMetadataBlockHelper.Default) if you need a mutable copy.
        /// </summary>
        static public MmMetadataBlock Default => _default;

        /// <summary>
        /// Returns a cached MmMetadataBlock for Self-only routing with Everything tag.
        /// WARNING: This is a shared instance - do NOT modify its fields.
        /// </summary>
		static public MmMetadataBlock SelfDefaultTagAll => _selfDefaultTagAll;
    }
}