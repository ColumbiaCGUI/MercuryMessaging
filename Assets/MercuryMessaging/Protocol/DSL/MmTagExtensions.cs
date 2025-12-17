// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.6: Tag Configuration Extensions
// Provides fluent API for configuring tags on responders

// Suppress MM015: Filter equality checks are intentional for exact match routing logic
#pragma warning disable MM015

using System.Runtime.CompilerServices;
using MercuryMessaging;

namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for fluent tag configuration.
    /// Part of DSL Phase 2.6: Tag Configuration.
    /// </summary>
    /// <remarks>
    /// Provides a chainable API for configuring tags on responders and relay nodes.
    ///
    /// Example usage:
    /// <code>
    /// // Single responder
    /// responder.WithTag(MmTag.Tag0).EnableTagChecking();
    ///
    /// // Multiple tags
    /// responder.WithTags(MmTag.Tag0, MmTag.Tag1).EnableTagChecking();
    ///
    /// // Bulk configuration from relay
    /// relay.ConfigureTags()
    ///     .ApplyToSelf(MmTag.Tag0)
    ///     .ApplyToChildren(MmTag.Tag1)
    ///     .Build();
    /// </code>
    /// </remarks>
    public static class MmTagExtensions
    {
        #region Responder Tag Configuration

        /// <summary>
        /// Sets the tag on this responder.
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <param name="tag">The tag to set.</param>
        /// <returns>The responder (for chaining).</returns>
        /// <example>
        /// responder.WithTag(MmTag.Tag0).EnableTagChecking();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder WithTag(this MmBaseResponder responder, MmTag tag)
        {
            if (responder != null)
            {
                responder.Tag = tag;
            }
            return responder;
        }

        /// <summary>
        /// Sets multiple tags on this responder (combined with OR).
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <param name="tags">The tags to combine.</param>
        /// <returns>The responder (for chaining).</returns>
        /// <example>
        /// responder.WithTags(MmTag.Tag0, MmTag.Tag1);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder WithTags(this MmBaseResponder responder, params MmTag[] tags)
        {
            if (responder != null && tags != null)
            {
                MmTag combined = MmTagHelper.Nothing;
                foreach (var tag in tags)
                {
                    combined |= tag;
                }
                responder.Tag = combined;
            }
            return responder;
        }

        /// <summary>
        /// Enables tag checking on this responder.
        /// When enabled, the responder only receives messages with matching tags.
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <returns>The responder (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder EnableTagChecking(this MmBaseResponder responder)
        {
            if (responder != null)
            {
                responder.TagCheckEnabled = true;
            }
            return responder;
        }

        /// <summary>
        /// Disables tag checking on this responder.
        /// When disabled, the responder receives all messages regardless of tag.
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <returns>The responder (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder DisableTagChecking(this MmBaseResponder responder)
        {
            if (responder != null)
            {
                responder.TagCheckEnabled = false;
            }
            return responder;
        }

        /// <summary>
        /// Adds a tag to this responder's existing tags.
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <param name="tag">The tag to add.</param>
        /// <returns>The responder (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder AddTag(this MmBaseResponder responder, MmTag tag)
        {
            if (responder != null)
            {
                responder.Tag |= tag;
            }
            return responder;
        }

        /// <summary>
        /// Removes a tag from this responder's existing tags.
        /// </summary>
        /// <param name="responder">The responder to configure.</param>
        /// <param name="tag">The tag to remove.</param>
        /// <returns>The responder (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder RemoveTag(this MmBaseResponder responder, MmTag tag)
        {
            if (responder != null)
            {
                responder.Tag &= ~tag;
            }
            return responder;
        }

        /// <summary>
        /// Checks if this responder has the specified tag.
        /// </summary>
        /// <param name="responder">The responder to check.</param>
        /// <param name="tag">The tag to check for.</param>
        /// <returns>True if the responder has the tag.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTag(this MmBaseResponder responder, MmTag tag)
        {
            if (responder == null) return false;
            return (responder.Tag & tag) != 0;
        }

        /// <summary>
        /// Checks if this responder has all of the specified tags.
        /// </summary>
        /// <param name="responder">The responder to check.</param>
        /// <param name="tags">The tags to check for.</param>
        /// <returns>True if the responder has all tags.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllTags(this MmBaseResponder responder, params MmTag[] tags)
        {
            if (responder == null || tags == null) return false;

            foreach (var tag in tags)
            {
                if ((responder.Tag & tag) == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if this responder has any of the specified tags.
        /// </summary>
        /// <param name="responder">The responder to check.</param>
        /// <param name="tags">The tags to check for.</param>
        /// <returns>True if the responder has at least one tag.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyTag(this MmBaseResponder responder, params MmTag[] tags)
        {
            if (responder == null || tags == null) return false;

            foreach (var tag in tags)
            {
                if ((responder.Tag & tag) != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clears all tags from this responder.
        /// </summary>
        /// <param name="responder">The responder to clear.</param>
        /// <returns>The responder (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmBaseResponder ClearTags(this MmBaseResponder responder)
        {
            if (responder != null)
            {
                responder.Tag = MmTagHelper.Nothing;
            }
            return responder;
        }

        #endregion

        #region Bulk Tag Configuration

        /// <summary>
        /// Creates a tag configuration builder for bulk tag operations.
        /// </summary>
        /// <param name="relay">The relay node to configure.</param>
        /// <returns>A builder for configuring tags.</returns>
        /// <example>
        /// relay.ConfigureTags()
        ///     .ApplyToSelf(MmTag.Tag0)
        ///     .ApplyToChildren(MmTag.Tag1, enableChecking: true)
        ///     .Build();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmTagConfigBuilder ConfigureTags(this MmRelayNode relay)
        {
            return new MmTagConfigBuilder(relay);
        }

        #endregion
    }

    /// <summary>
    /// Fluent builder for bulk tag configuration.
    /// </summary>
    public struct MmTagConfigBuilder
    {
        private readonly MmRelayNode _relay;

        internal MmTagConfigBuilder(MmRelayNode relay)
        {
            _relay = relay;
        }

        /// <summary>
        /// Applies a tag to the relay node itself (if it has responders).
        /// </summary>
        /// <param name="tag">The tag to apply.</param>
        /// <param name="enableChecking">Whether to enable tag checking.</param>
        /// <returns>The builder (for chaining).</returns>
        public MmTagConfigBuilder ApplyToSelf(MmTag tag, bool enableChecking = false)
        {
            if (_relay == null) return this;

            // Apply to responders on the same GameObject
            var responders = _relay.gameObject.GetComponents<MmBaseResponder>();
            foreach (var responder in responders)
            {
                if (responder != null)
                {
                    responder.Tag = tag;
                    responder.TagCheckEnabled = enableChecking;
                }
            }

            return this;
        }

        /// <summary>
        /// Applies a tag to all direct children.
        /// </summary>
        /// <param name="tag">The tag to apply.</param>
        /// <param name="enableChecking">Whether to enable tag checking.</param>
        /// <returns>The builder (for chaining).</returns>
        public MmTagConfigBuilder ApplyToChildren(MmTag tag, bool enableChecking = false)
        {
            if (_relay?.RoutingTable == null) return this;

            foreach (var item in _relay.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Child && item.Responder is MmBaseResponder responder)
                {
                    responder.Tag = tag;
                    responder.TagCheckEnabled = enableChecking;
                }
            }

            return this;
        }

        /// <summary>
        /// Applies a tag to all descendants recursively.
        /// </summary>
        /// <param name="tag">The tag to apply.</param>
        /// <param name="enableChecking">Whether to enable tag checking.</param>
        /// <returns>The builder (for chaining).</returns>
        public MmTagConfigBuilder ApplyToDescendants(MmTag tag, bool enableChecking = false)
        {
            if (_relay == null) return this;

            ApplyToDescendantsRecursive(_relay, tag, enableChecking);
            return this;
        }

        private void ApplyToDescendantsRecursive(MmRelayNode node, MmTag tag, bool enableChecking)
        {
            if (node?.RoutingTable == null) return;

            foreach (var item in node.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Child)
                {
                    if (item.Responder is MmBaseResponder responder)
                    {
                        responder.Tag = tag;
                        responder.TagCheckEnabled = enableChecking;
                    }

                    if (item.Responder is MmRelayNode childRelay)
                    {
                        ApplyToDescendantsRecursive(childRelay, tag, enableChecking);
                    }
                }
            }
        }

        /// <summary>
        /// Applies tag checking to all responders in the hierarchy.
        /// </summary>
        /// <param name="enable">Whether to enable or disable tag checking.</param>
        /// <returns>The builder (for chaining).</returns>
        public MmTagConfigBuilder SetTagChecking(bool enable)
        {
            if (_relay == null) return this;

            // Apply to self
            var selfResponders = _relay.gameObject.GetComponents<MmBaseResponder>();
            foreach (var responder in selfResponders)
            {
                if (responder != null)
                    responder.TagCheckEnabled = enable;
            }

            // Apply to descendants
            SetTagCheckingRecursive(_relay, enable);
            return this;
        }

        private void SetTagCheckingRecursive(MmRelayNode node, bool enable)
        {
            if (node?.RoutingTable == null) return;

            foreach (var item in node.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Child)
                {
                    if (item.Responder is MmBaseResponder responder)
                    {
                        responder.TagCheckEnabled = enable;
                    }

                    if (item.Responder is MmRelayNode childRelay)
                    {
                        SetTagCheckingRecursive(childRelay, enable);
                    }
                }
            }
        }

        /// <summary>
        /// Finalizes the tag configuration.
        /// </summary>
        /// <returns>The relay node.</returns>
        public MmRelayNode Build()
        {
            return _relay;
        }
    }
}
