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
// Ben Yang, Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
using System;
using System.Runtime.CompilerServices;

namespace MercuryMessaging
{
    /// <summary>
    /// Filter represents relationship types of MmResponders.
    /// This is a Flags enum - values can be combined using bitwise OR (|).
    ///
    /// Phase 2.1 Extensions (Advanced Routing):
    /// - Siblings: Same-parent nodes (lateral routing)
    /// - Cousins: Parent's-sibling's-children (extended family)
    /// - Descendants: Recursive all children (deep traversal)
    /// - Ancestors: Recursive all parents (upward traversal)
    /// - Custom: User-defined predicate filtering
    /// </summary>
    [Flags]
    public enum MmLevelFilter
    {
        // Original filters (bits 0-2)
        Self = 1 << 0,      // Bit 0: Self node only
        Child = 1 << 1,     // Bit 1: Direct children
        Parent = 1 << 2,    // Bit 2: Direct parents

        // Phase 2.1: Advanced Routing (bits 3-7)
        Siblings = 1 << 3,      // Bit 3: Same-parent nodes (lateral routing)
        Cousins = 1 << 4,       // Bit 4: Parent's-sibling's-children
        Descendants = 1 << 5,   // Bit 5: Recursive all children (Child + grandchildren + ...)
        Ancestors = 1 << 6,     // Bit 6: Recursive all parents (Parent + grandparents + ...)
        Custom = 1 << 7,        // Bit 7: User-defined predicate (requires MmRoutingOptions.CustomFilter)
    }

    /// <summary>
    /// Utility to quickly generate MmLevelFilterHelpers.
    /// Provides common combinations of MmLevelFilter flags.
    /// </summary>
    public static class MmLevelFilterHelper
    {
        // Original helpers
        public const MmLevelFilter SelfAndBidirectional = (MmLevelFilter)(-1);
        public const MmLevelFilter SelfAndChildren = MmLevelFilter.Self | MmLevelFilter.Child;
        public const MmLevelFilter SelfAndParents = MmLevelFilter.Self | MmLevelFilter.Parent;

        /// <summary>
        /// Default is SelfAndChildren (backward compatible)
        /// </summary>
        public const MmLevelFilter Default = SelfAndChildren;

        // Phase 2.1: Advanced Routing Helpers

        /// <summary>
        /// Self + direct siblings (same-parent nodes).
        /// Useful for coordinating peer nodes.
        /// </summary>
        public const MmLevelFilter SelfAndSiblings = MmLevelFilter.Self | MmLevelFilter.Siblings;

        /// <summary>
        /// Self + all descendants (recursive children).
        /// Useful for broadcasting to entire subtree.
        /// </summary>
        public const MmLevelFilter SelfAndDescendants = MmLevelFilter.Self | MmLevelFilter.Descendants;

        /// <summary>
        /// Self + all ancestors (recursive parents).
        /// Useful for bubbling events up the hierarchy.
        /// </summary>
        public const MmLevelFilter SelfAndAncestors = MmLevelFilter.Self | MmLevelFilter.Ancestors;

        /// <summary>
        /// Self + siblings + children (immediate neighborhood).
        /// Useful for local area messaging.
        /// </summary>
        public const MmLevelFilter LocalArea = MmLevelFilter.Self | MmLevelFilter.Siblings | MmLevelFilter.Child;

        /// <summary>
        /// Self + siblings + descendants (lateral + downward).
        /// Useful for fan-out patterns across multiple branches.
        /// </summary>
        public const MmLevelFilter LateralAndDescendants =
            MmLevelFilter.Self | MmLevelFilter.Siblings | MmLevelFilter.Descendants;

        /// <summary>
        /// Extended family: Self + children + siblings + cousins.
        /// Useful for complex multi-branch coordination.
        /// </summary>
        public const MmLevelFilter ExtendedFamily =
            MmLevelFilter.Self | MmLevelFilter.Child | MmLevelFilter.Siblings | MmLevelFilter.Cousins;

        /// <summary>
        /// Checks if lateral routing (siblings/cousins) is enabled in the filter.
        /// Used to validate MmRoutingOptions.AllowLateralRouting requirement.
        /// </summary>
        /// <param name="filter">Filter to check</param>
        /// <returns>True if lateral routing is requested</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasLateralRouting(this MmLevelFilter filter)
        {
            return (filter & (MmLevelFilter.Siblings | MmLevelFilter.Cousins)) != 0;
        }

        /// <summary>
        /// Checks if custom filtering is enabled.
        /// Used to validate MmRoutingOptions.CustomFilter requirement.
        /// </summary>
        /// <param name="filter">Filter to check</param>
        /// <returns>True if custom filtering is requested</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasCustomFilter(this MmLevelFilter filter)
        {
            return (filter & MmLevelFilter.Custom) != 0;
        }
    }
}