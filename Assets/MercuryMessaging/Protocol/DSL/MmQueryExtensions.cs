// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.2: Query Extensions for MmRelayNode and MmBaseResponder
// Provides fluent API entry points for hierarchy traversal queries

using System;
using System.Runtime.CompilerServices;
using MercuryMessaging;

namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for querying MercuryMessaging hierarchies.
    /// Part of DSL Phase 2.2: Hierarchy Query DSL.
    /// </summary>
    /// <remarks>
    /// Provides LINQ-like querying capabilities for traversing and filtering
    /// responder hierarchies. Supports both MmRelayNode and MmBaseResponder.
    ///
    /// Example usage:
    /// <code>
    /// // Query from relay node
    /// var enemies = relay.Query()
    ///     .Descendants()
    ///     .OfType&lt;EnemyResponder&gt;()
    ///     .Active()
    ///     .ToList();
    ///
    /// // Execute action on all matching responders
    /// relay.Query()
    ///     .Children()
    ///     .WithTag(MmTag.Tag0)
    ///     .Execute(r => r.DoSomething());
    ///
    /// // Query from responder (same API!)
    /// myResponder.Query()
    ///     .Siblings()
    ///     .OfType&lt;UIResponder&gt;()
    ///     .FirstOrDefault();
    /// </code>
    /// </remarks>
    public static class MmQueryExtensions
    {
        #region MmRelayNode Entry Points

        /// <summary>
        /// Creates a query builder for traversing this relay node's hierarchy.
        /// Use fluent methods to filter and execute queries.
        /// </summary>
        /// <param name="relay">The relay node to query from.</param>
        /// <returns>A query builder for configuring the query.</returns>
        /// <example>
        /// relay.Query()
        ///     .Descendants()
        ///     .OfType&lt;MyResponder&gt;()
        ///     .ToList();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmQueryBuilder Query(this MmRelayNode relay)
        {
            return new MmQueryBuilder(relay);
        }

        /// <summary>
        /// Starts a typed query directly for the specified responder type.
        /// Shorthand for Query().OfType&lt;T&gt;().
        /// </summary>
        /// <typeparam name="T">The responder type to query for.</typeparam>
        /// <param name="relay">The relay node to query from.</param>
        /// <returns>A typed query for the specified type.</returns>
        /// <example>
        /// var enemies = relay.Query&lt;EnemyResponder&gt;()
        ///     .Descendants()
        ///     .Active()
        ///     .ToList();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmQuery<T> Query<T>(this MmRelayNode relay) where T : class
        {
            return new MmQuery<T>(relay);
        }

        #endregion

        #region MmRelayNode Convenience Methods

        /// <summary>
        /// Finds the first responder of type T in descendants.
        /// Shorthand for Query&lt;T&gt;().Descendants().FirstOrDefault().
        /// </summary>
        /// <typeparam name="T">The responder type to find.</typeparam>
        /// <param name="relay">The relay node to search from.</param>
        /// <returns>The first matching responder, or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FindDescendant<T>(this MmRelayNode relay) where T : class
        {
            return relay.Query<T>().Descendants().FirstOrDefault();
        }

        /// <summary>
        /// Finds the first responder of type T in ancestors.
        /// Shorthand for Query&lt;T&gt;().Ancestors().FirstOrDefault().
        /// </summary>
        /// <typeparam name="T">The responder type to find.</typeparam>
        /// <param name="relay">The relay node to search from.</param>
        /// <returns>The first matching responder, or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FindAncestor<T>(this MmRelayNode relay) where T : class
        {
            return relay.Query<T>().Ancestors().FirstOrDefault();
        }

        /// <summary>
        /// Finds all responders of type T in descendants.
        /// Shorthand for Query&lt;T&gt;().Descendants().ToList().
        /// </summary>
        /// <typeparam name="T">The responder type to find.</typeparam>
        /// <param name="relay">The relay node to search from.</param>
        /// <returns>A list of all matching responders.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Collections.Generic.List<T> FindAllDescendants<T>(this MmRelayNode relay) where T : class
        {
            return relay.Query<T>().Descendants().ToList();
        }

        /// <summary>
        /// Finds a responder by name in the hierarchy.
        /// Searches descendants by default.
        /// </summary>
        /// <param name="relay">The relay node to search from.</param>
        /// <param name="name">The exact name to match.</param>
        /// <returns>The first matching responder, or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmResponder FindByName(this MmRelayNode relay, string name)
        {
            return relay.Query<MmResponder>().Descendants().Named(name).FirstOrDefault();
        }

        /// <summary>
        /// Finds responders matching a name pattern in the hierarchy.
        /// Supports wildcards (* matches any characters).
        /// </summary>
        /// <param name="relay">The relay node to search from.</param>
        /// <param name="pattern">The name pattern (e.g., "Enemy*").</param>
        /// <returns>A list of all matching responders.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Collections.Generic.List<MmResponder> FindByPattern(this MmRelayNode relay, string pattern)
        {
            return relay.Query<MmResponder>().Descendants().NamedLike(pattern).ToList();
        }

        /// <summary>
        /// Gets the count of direct children.
        /// </summary>
        /// <param name="relay">The relay node to count from.</param>
        /// <returns>The number of direct children.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ChildCount(this MmRelayNode relay)
        {
            return relay.Query<MmResponder>().Children().Count();
        }

        /// <summary>
        /// Gets the count of all descendants.
        /// </summary>
        /// <param name="relay">The relay node to count from.</param>
        /// <returns>The number of descendants.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DescendantCount(this MmRelayNode relay)
        {
            return relay.Query<MmResponder>().Descendants().Count();
        }

        /// <summary>
        /// Checks if any children exist.
        /// </summary>
        /// <param name="relay">The relay node to check.</param>
        /// <returns>True if at least one child exists.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasChildren(this MmRelayNode relay)
        {
            return relay.Query<MmResponder>().Children().Any();
        }

        /// <summary>
        /// Checks if any responders of type T exist in descendants.
        /// </summary>
        /// <typeparam name="T">The responder type to check for.</typeparam>
        /// <param name="relay">The relay node to check.</param>
        /// <returns>True if at least one matching responder exists.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasDescendant<T>(this MmRelayNode relay) where T : class
        {
            return relay.Query<T>().Descendants().Any();
        }

        #endregion

        #region MmBaseResponder Entry Points

        /// <summary>
        /// Creates a query builder for traversing this responder's hierarchy.
        /// Null-safe: returns default builder if responder has no relay node.
        /// </summary>
        /// <param name="responder">The responder to query from.</param>
        /// <returns>A query builder for configuring the query.</returns>
        /// <example>
        /// myResponder.Query()
        ///     .Siblings()
        ///     .OfType&lt;UIResponder&gt;()
        ///     .Execute(r => r.Hide());
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmQueryBuilder Query(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            return relay != null ? new MmQueryBuilder(relay) : default;
        }

        /// <summary>
        /// Starts a typed query directly for the specified responder type (from responder).
        /// Null-safe: returns default query if responder has no relay node.
        /// </summary>
        /// <typeparam name="T">The responder type to query for.</typeparam>
        /// <param name="responder">The responder to query from.</param>
        /// <returns>A typed query for the specified type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmQuery<T> Query<T>(this MmBaseResponder responder) where T : class
        {
            var relay = responder.GetRelayNode();
            return relay != null ? new MmQuery<T>(relay) : default;
        }

        #endregion

        #region MmBaseResponder Convenience Methods

        /// <summary>
        /// Finds the first responder of type T in descendants (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FindDescendant<T>(this MmBaseResponder responder) where T : class
        {
            var relay = responder.GetRelayNode();
            return relay?.FindDescendant<T>();
        }

        /// <summary>
        /// Finds the first responder of type T in ancestors (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FindAncestor<T>(this MmBaseResponder responder) where T : class
        {
            var relay = responder.GetRelayNode();
            return relay?.FindAncestor<T>();
        }

        /// <summary>
        /// Finds all responders of type T in descendants (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Collections.Generic.List<T> FindAllDescendants<T>(this MmBaseResponder responder) where T : class
        {
            var relay = responder.GetRelayNode();
            return relay?.FindAllDescendants<T>() ?? new System.Collections.Generic.List<T>();
        }

        /// <summary>
        /// Finds a responder by name in the hierarchy (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmResponder FindByName(this MmBaseResponder responder, string name)
        {
            var relay = responder.GetRelayNode();
            return relay?.FindByName(name);
        }

        /// <summary>
        /// Finds responders matching a name pattern (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Collections.Generic.List<MmResponder> FindByPattern(this MmBaseResponder responder, string pattern)
        {
            var relay = responder.GetRelayNode();
            return relay?.FindByPattern(pattern) ?? new System.Collections.Generic.List<MmResponder>();
        }

        /// <summary>
        /// Gets the count of siblings (from responder).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SiblingCount(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            return relay?.Query<MmResponder>().Siblings().Count() ?? 0;
        }

        /// <summary>
        /// Checks if this responder has siblings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSiblings(this MmBaseResponder responder)
        {
            var relay = responder.GetRelayNode();
            return relay?.Query<MmResponder>().Siblings().Any() ?? false;
        }

        #endregion
    }
}
