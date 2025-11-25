using System.Runtime.CompilerServices;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Provides convenient static properties for common routing filters.
    /// These can be used with the fluent API for more readable message routing.
    /// Example: relay.Send("Hello").To(Children.Active.Tag0)
    /// </summary>
    public static class MmFluentFilters
    {
        #region Level Filters

        /// <summary>
        /// Target only the current node.
        /// </summary>
        public static MmLevelFilter Self => MmLevelFilter.Self;

        /// <summary>
        /// Target child nodes.
        /// </summary>
        public static MmLevelFilter Children => MmLevelFilter.Child;

        /// <summary>
        /// Target parent nodes.
        /// </summary>
        public static MmLevelFilter Parents => MmLevelFilter.Parent;

        /// <summary>
        /// Target sibling nodes (same parent).
        /// </summary>
        public static MmLevelFilter Siblings => MmLevelFilter.Siblings;

        /// <summary>
        /// Target cousin nodes (parent's siblings' children).
        /// </summary>
        public static MmLevelFilter Cousins => MmLevelFilter.Cousins;

        /// <summary>
        /// Target all descendant nodes recursively.
        /// </summary>
        public static MmLevelFilter Descendants => MmLevelFilter.Descendants;

        /// <summary>
        /// Target all ancestor nodes recursively.
        /// </summary>
        public static MmLevelFilter Ancestors => MmLevelFilter.Ancestors;

        /// <summary>
        /// Target self and all children.
        /// </summary>
        public static MmLevelFilter SelfAndChildren => MmLevelFilterHelper.SelfAndChildren;

        /// <summary>
        /// Target all connected nodes bidirectionally.
        /// </summary>
        public static MmLevelFilter All => MmLevelFilterHelper.SelfAndBidirectional;

        #endregion

        #region Tag Constants

        /// <summary>
        /// UI-related components tag.
        /// </summary>
        public static MmTag UI => MmTag.Tag0;

        /// <summary>
        /// Gameplay-related components tag.
        /// </summary>
        public static MmTag Gameplay => MmTag.Tag1;

        /// <summary>
        /// Network-synced components tag.
        /// </summary>
        public static MmTag Network => MmTag.Tag2;

        /// <summary>
        /// Debug/development components tag.
        /// </summary>
        public static MmTag Debug => MmTag.Tag3;

        /// <summary>
        /// VR/XR-specific components tag.
        /// </summary>
        public static MmTag VR => MmTag.Tag4;

        /// <summary>
        /// Audio-related components tag.
        /// </summary>
        public static MmTag Audio => MmTag.Tag5;

        /// <summary>
        /// Visual effects components tag.
        /// </summary>
        public static MmTag VFX => MmTag.Tag6;

        /// <summary>
        /// Physics-related components tag.
        /// </summary>
        public static MmTag Physics => MmTag.Tag7;

        #endregion
    }

    /// <summary>
    /// Provides a fluent route builder for combining multiple filters.
    /// This enables syntax like: Children.Active.Tag0
    /// </summary>
    public struct MmRouteBuilder
    {
        private MmLevelFilter _level;
        private MmActiveFilter _active;
        private MmSelectedFilter _selected;
        private MmNetworkFilter _network;
        private MmTag _tag;

        /// <summary>
        /// Initialize with a level filter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmRouteBuilder(MmLevelFilter level)
        {
            _level = level;
            _active = MmActiveFilter.All;
            _selected = MmSelectedFilter.All;
            _network = MmNetworkFilter.Local;
            _tag = MmTagHelper.Everything;
        }

        #region Properties for Chaining

        /// <summary>
        /// Filter to only active GameObjects.
        /// </summary>
        public MmRouteBuilder Active
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _active = MmActiveFilter.Active;
                return this;
            }
        }

        /// <summary>
        /// Include all GameObjects (active and inactive).
        /// </summary>
        public MmRouteBuilder AllActive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _active = MmActiveFilter.All;
                return this;
            }
        }

        /// <summary>
        /// Filter to only selected responders (FSM).
        /// </summary>
        public MmRouteBuilder Selected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _selected = MmSelectedFilter.Selected;
                return this;
            }
        }

        /// <summary>
        /// Send locally only.
        /// </summary>
        public MmRouteBuilder Local
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _network = MmNetworkFilter.Local;
                return this;
            }
        }

        /// <summary>
        /// Send over network only.
        /// </summary>
        public MmRouteBuilder NetworkOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _network = MmNetworkFilter.Network;
                return this;
            }
        }

        /// <summary>
        /// Send both locally and over network.
        /// </summary>
        public MmRouteBuilder AllNetworks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _network = MmNetworkFilter.All;
                return this;
            }
        }

        #endregion

        #region Tag Properties

        /// <summary>
        /// Filter by Tag0.
        /// </summary>
        public MmRouteBuilder Tag0
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag0;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag1.
        /// </summary>
        public MmRouteBuilder Tag1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag1;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag2.
        /// </summary>
        public MmRouteBuilder Tag2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag2;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag3.
        /// </summary>
        public MmRouteBuilder Tag3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag3;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag4.
        /// </summary>
        public MmRouteBuilder Tag4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag4;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag5.
        /// </summary>
        public MmRouteBuilder Tag5
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag5;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag6.
        /// </summary>
        public MmRouteBuilder Tag6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag6;
                return this;
            }
        }

        /// <summary>
        /// Filter by Tag7.
        /// </summary>
        public MmRouteBuilder Tag7
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _tag = MmTag.Tag7;
                return this;
            }
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Build the metadata block from this route builder.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmMetadataBlock Build()
        {
            // Use the constructor with tag as first parameter if tag is not Everything
            if (_tag != MmTagHelper.Everything)
            {
                return new MmMetadataBlock(_tag, _level, _active, _selected, _network);
            }
            else
            {
                return new MmMetadataBlock(_level, _active, _selected, _network);
            }
        }

        /// <summary>
        /// Implicit conversion to MmMetadataBlock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MmMetadataBlock(MmRouteBuilder builder)
        {
            return builder.Build();
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for MmLevelFilter to enable property chaining.
    /// </summary>
    public static class MmLevelFilterExtensions
    {
        /// <summary>
        /// Convert a level filter to a route builder for chaining.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRouteBuilder AsRoute(this MmLevelFilter level)
        {
            return new MmRouteBuilder(level);
        }

        // Enable property-like access on MmLevelFilter directly
        public static MmRouteBuilder Active(this MmLevelFilter level) => new MmRouteBuilder(level).Active;
        public static MmRouteBuilder Selected(this MmLevelFilter level) => new MmRouteBuilder(level).Selected;
        public static MmRouteBuilder Local(this MmLevelFilter level) => new MmRouteBuilder(level).Local;
        public static MmRouteBuilder NetworkOnly(this MmLevelFilter level) => new MmRouteBuilder(level).NetworkOnly;
        public static MmRouteBuilder Tag0(this MmLevelFilter level) => new MmRouteBuilder(level).Tag0;
        public static MmRouteBuilder Tag1(this MmLevelFilter level) => new MmRouteBuilder(level).Tag1;
        public static MmRouteBuilder Tag2(this MmLevelFilter level) => new MmRouteBuilder(level).Tag2;
        public static MmRouteBuilder Tag3(this MmLevelFilter level) => new MmRouteBuilder(level).Tag3;
        public static MmRouteBuilder Tag4(this MmLevelFilter level) => new MmRouteBuilder(level).Tag4;
        public static MmRouteBuilder Tag5(this MmLevelFilter level) => new MmRouteBuilder(level).Tag5;
        public static MmRouteBuilder Tag6(this MmLevelFilter level) => new MmRouteBuilder(level).Tag6;
        public static MmRouteBuilder Tag7(this MmLevelFilter level) => new MmRouteBuilder(level).Tag7;
    }
}