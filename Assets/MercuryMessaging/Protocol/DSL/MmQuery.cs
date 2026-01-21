// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.2: Hierarchy Query DSL
// Provides LINQ-like querying of MercuryMessaging hierarchies

// Suppress MM015: Filter equality checks are intentional for exact match routing logic
#pragma warning disable MM015

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging
{
    /// <summary>
    /// Fluent query builder for traversing and querying MercuryMessaging hierarchies.
    /// Supports filtering by type, tag, level, and custom predicates.
    /// Uses lazy evaluation for performance.
    /// </summary>
    /// <typeparam name="T">The type of responder to query for (default: MmResponder)</typeparam>
    public struct MmQuery<T> where T : class
    {
        private readonly MmRelayNode _relay;
        private MmLevelFilter _levelFilter;
        private MmTag _tagFilter;
        private bool _activeOnly;
        private bool _includeInactive;
        private Func<T, bool> _predicate;
        private bool _recursive;

        /// <summary>
        /// Creates a new query builder for the specified relay node.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery(MmRelayNode relay)
        {
            _relay = relay;
            _levelFilter = MmLevelFilterHelper.SelfAndChildren;
            _tagFilter = MmTagHelper.Everything;
            _activeOnly = false;
            _includeInactive = true;
            _predicate = null;
            _recursive = false;
        }

        #region Direction Methods

        /// <summary>
        /// Query direct children only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Children()
        {
            _levelFilter = MmLevelFilter.Child;
            _recursive = false;
            return this;
        }

        /// <summary>
        /// Query direct parents only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Parents()
        {
            _levelFilter = MmLevelFilter.Parent;
            _recursive = false;
            return this;
        }

        /// <summary>
        /// Query all descendants recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Descendants()
        {
            _levelFilter = MmLevelFilter.Child;
            _recursive = true;
            return this;
        }

        /// <summary>
        /// Query all ancestors recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Ancestors()
        {
            _levelFilter = MmLevelFilter.Parent;
            _recursive = true;
            return this;
        }

        /// <summary>
        /// Query siblings (same parent).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Siblings()
        {
            _levelFilter = MmLevelFilter.Siblings;
            _recursive = false;
            return this;
        }

        /// <summary>
        /// Query self and all children.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> SelfAndChildren()
        {
            _levelFilter = MmLevelFilterHelper.SelfAndChildren;
            _recursive = false;
            return this;
        }

        /// <summary>
        /// Query all connected nodes (bidirectional).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> All()
        {
            _levelFilter = MmLevelFilterHelper.SelfAndBidirectional;
            _recursive = true;
            return this;
        }

        #endregion

        #region Filter Methods

        /// <summary>
        /// Filter by a specific responder type.
        /// </summary>
        /// <typeparam name="TTarget">The target responder type.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<TTarget> OfType<TTarget>() where TTarget : class
        {
            var newQuery = new MmQuery<TTarget>(_relay)
            {
                _levelFilter = _levelFilter,
                _tagFilter = _tagFilter,
                _activeOnly = _activeOnly,
                _includeInactive = _includeInactive,
                _recursive = _recursive
            };
            return newQuery;
        }

        /// <summary>
        /// Filter by tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> WithTag(MmTag tag)
        {
            _tagFilter = tag;
            return this;
        }

        /// <summary>
        /// Filter to only active GameObjects.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Active()
        {
            _activeOnly = true;
            _includeInactive = false;
            return this;
        }

        /// <summary>
        /// Include inactive GameObjects in the query.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> IncludeInactive()
        {
            _includeInactive = true;
            _activeOnly = false;
            return this;
        }

        /// <summary>
        /// Filter by a custom predicate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Where(Func<T, bool> predicate)
        {
            if (_predicate == null)
            {
                _predicate = predicate;
            }
            else
            {
                var existingPredicate = _predicate;
                _predicate = item => existingPredicate(item) && predicate(item);
            }
            return this;
        }

        /// <summary>
        /// Filter by name (exact match).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> Named(string name)
        {
            return Where(item =>
            {
                if (item is MmResponder responder)
                    return responder.gameObject.name == name;
                if (item is Component component)
                    return component.gameObject.name == name;
                return false;
            });
        }

        /// <summary>
        /// Filter by name pattern (wildcard: * matches any characters).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> NamedLike(string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(
                "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return Where(item =>
            {
                string name = null;
                if (item is MmResponder responder)
                    name = responder.gameObject.name;
                else if (item is Component component)
                    name = component.gameObject.name;
                return name != null && regex.IsMatch(name);
            });
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Execute an action on each matching item.
        /// </summary>
        public void Execute(Action<T> action)
        {
            if (_relay == null || action == null) return;

            foreach (var item in EnumerateMatches())
            {
                action(item);
            }
        }

        /// <summary>
        /// Get all matching items as a list.
        /// </summary>
        public List<T> ToList()
        {
            var results = new List<T>();
            if (_relay == null) return results;

            foreach (var item in EnumerateMatches())
            {
                results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// Get the first matching item, or default if none found.
        /// </summary>
        public T FirstOrDefault()
        {
            if (_relay == null) return default;

            foreach (var item in EnumerateMatches())
            {
                return item;
            }
            return default;
        }

        /// <summary>
        /// Get the first matching item, or throw if none found.
        /// </summary>
        public T First()
        {
            var result = FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException("Query returned no results");
            return result;
        }

        /// <summary>
        /// Get the count of matching items.
        /// </summary>
        public int Count()
        {
            if (_relay == null) return 0;

            int count = 0;
            foreach (var _ in EnumerateMatches())
            {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Check if any items match.
        /// </summary>
        public bool Any()
        {
            if (_relay == null) return false;

            foreach (var _ in EnumerateMatches())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if any items match the predicate.
        /// </summary>
        public bool Any(Func<T, bool> predicate)
        {
            if (_relay == null) return false;

            foreach (var item in EnumerateMatches())
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }

        #endregion

        #region Internal Enumeration

        /// <summary>
        /// Enumerates all matching items based on current query configuration.
        /// </summary>
        private IEnumerable<T> EnumerateMatches()
        {
            if (_relay == null) yield break;

            var visited = new HashSet<MmRelayNode>();

            if (_recursive)
            {
                // Recursive traversal
                foreach (var item in EnumerateRecursive(_relay, visited))
                {
                    yield return item;
                }
            }
            else
            {
                // Direct traversal of routing table
                foreach (var item in EnumerateDirect(_relay))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Enumerate direct children/parents from routing table.
        /// </summary>
        private IEnumerable<T> EnumerateDirect(MmRelayNode relay)
        {
            if (relay?.RoutingTable == null) yield break;

            foreach (var tableItem in relay.RoutingTable)
            {
                if (tableItem?.Responder == null) continue;

                // Check level filter
                if (!MatchesLevelFilter(tableItem.Level)) continue;

                // Check active filter
                if (_activeOnly && !tableItem.Responder.gameObject.activeInHierarchy) continue;
                if (!_includeInactive && !tableItem.Responder.gameObject.activeInHierarchy) continue;

                // Check tag filter
                if (_tagFilter != MmTagHelper.Everything)
                {
                    if (tableItem.Responder is MmBaseResponder baseResponder)
                    {
                        if ((baseResponder.Tag & _tagFilter) == 0) continue;
                    }
                }

                // Try to cast to target type
                T typedItem = tableItem.Responder as T;
                if (typedItem == null) continue;

                // Check custom predicate
                if (_predicate != null && !_predicate(typedItem)) continue;

                yield return typedItem;
            }
        }

        /// <summary>
        /// Recursively enumerate descendants/ancestors.
        /// </summary>
        private IEnumerable<T> EnumerateRecursive(MmRelayNode relay, HashSet<MmRelayNode> visited)
        {
            if (relay == null || visited.Contains(relay)) yield break;
            visited.Add(relay);

            foreach (var item in EnumerateDirect(relay))
            {
                yield return item;
            }

            // Recurse into child relay nodes
            if (relay.RoutingTable != null)
            {
                foreach (var tableItem in relay.RoutingTable)
                {
                    if (tableItem?.Responder == null) continue;
                    if (!MatchesLevelFilter(tableItem.Level)) continue;

                    if (tableItem.Responder is MmRelayNode childRelay)
                    {
                        foreach (var childItem in EnumerateRecursive(childRelay, visited))
                        {
                            yield return childItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a routing table item's level matches our filter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesLevelFilter(MmLevelFilter itemLevel)
        {
            // Handle special combined filters
            if (_levelFilter == MmLevelFilterHelper.SelfAndChildren)
                return itemLevel == MmLevelFilter.Self || itemLevel == MmLevelFilter.Child;

            if (_levelFilter == MmLevelFilterHelper.SelfAndBidirectional)
                return true; // Match all

            // Direct comparison for simple filters
            return (itemLevel & _levelFilter) != 0;
        }

        #endregion
    }

    /// <summary>
    /// Non-generic query builder that returns MmResponder by default.
    /// </summary>
    public struct MmQueryBuilder
    {
        private readonly MmRelayNode _relay;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MmQueryBuilder(MmRelayNode relay)
        {
            _relay = relay;
        }

        /// <summary>
        /// Start a typed query.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<T> OfType<T>() where T : class
        {
            return new MmQuery<T>(_relay);
        }

        /// <summary>
        /// Query direct children.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> Children()
        {
            return new MmQuery<MmResponder>(_relay).Children();
        }

        /// <summary>
        /// Query direct parents.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> Parents()
        {
            return new MmQuery<MmResponder>(_relay).Parents();
        }

        /// <summary>
        /// Query all descendants recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> Descendants()
        {
            return new MmQuery<MmResponder>(_relay).Descendants();
        }

        /// <summary>
        /// Query all ancestors recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> Ancestors()
        {
            return new MmQuery<MmResponder>(_relay).Ancestors();
        }

        /// <summary>
        /// Query siblings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> Siblings()
        {
            return new MmQuery<MmResponder>(_relay).Siblings();
        }

        /// <summary>
        /// Query all connected nodes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmQuery<MmResponder> All()
        {
            return new MmQuery<MmResponder>(_relay).All();
        }
    }
}
