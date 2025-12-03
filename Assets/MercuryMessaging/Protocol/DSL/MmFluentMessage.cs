using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Fluent message builder for MercuryMessaging. This struct provides a chainable API
    /// for constructing and sending messages through the Mercury framework.
    /// Uses struct to avoid heap allocations and maintain high performance.
    ///
    /// Phase 2 additions: Supports spatial filtering (Within, InDirection, InBounds),
    /// type filtering (OfType, WithComponent), and custom predicates (Where).
    /// </summary>
    public struct MmFluentMessage
    {
        private readonly MmRelayNode _relay;
        private readonly MmMethod _method;
        private readonly object _payload;
        private MmLevelFilter _levelFilter;
        private MmActiveFilter _activeFilter;
        private MmSelectedFilter _selectedFilter;
        private MmNetworkFilter _networkFilter;
        private MmTag _tag;
        private bool _hasCustomFilters;

        // Optimization 2.1: Cache whether target collection is needed
        // This avoids 4 bitwise ANDs + 3 ORs on every Execute() call
        private bool _needsTargetCollection;

        // Phase 2: Predicate support for advanced filtering
        private MmPredicateList _predicates;

        /// <summary>
        /// Initializes a new fluent message builder.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage(MmRelayNode relay, MmMethod method, object payload = null)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
            _method = method;
            _payload = payload;

            // Default values matching MmMetadataBlock defaults
            _levelFilter = MmLevelFilterHelper.SelfAndChildren;
            _activeFilter = MmActiveFilter.All;
            _selectedFilter = MmSelectedFilter.All;
            _networkFilter = MmNetworkFilter.Local;
            _tag = MmTagHelper.Everything;
            _hasCustomFilters = false;
            _needsTargetCollection = false; // SelfAndChildren doesn't need target collection
            _predicates = null;
        }

        /// <summary>
        /// Ensure predicates list is initialized (lazy allocation).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsurePredicates()
        {
            if (_predicates == null)
            {
                _predicates = MmPredicateListPool.Get();
            }
        }

        #region Routing Methods

        /// <summary>
        /// Target child nodes in the hierarchy.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage To(MmLevelFilter filter)
        {
            _levelFilter = filter;
            _hasCustomFilters = true;
            // Compute target collection requirement for custom filters
            _needsTargetCollection = ComputeNeedsTargetCollection(filter);
            return this;
        }

        /// <summary>
        /// Helper to compute whether a level filter requires target collection.
        /// Extracted to avoid duplicating this logic.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ComputeNeedsTargetCollection(MmLevelFilter filter)
        {
            return (filter & MmLevelFilter.Descendants) != 0 ||
                   (filter & MmLevelFilter.Ancestors) != 0 ||
                   (filter & MmLevelFilter.Siblings) != 0 ||
                   ((filter & MmLevelFilter.Parent) != 0 && (filter & MmLevelFilter.Self) == 0);
        }

        /// <summary>
        /// Target only child nodes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToChildren()
        {
            _levelFilter = MmLevelFilter.Child;
            _hasCustomFilters = true;
            _needsTargetCollection = false; // Direct routing
            return this;
        }

        /// <summary>
        /// Target only parent nodes.
        /// Uses direct MmInvoke routing (Parent filter) which the framework handles natively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToParents()
        {
            _levelFilter = MmLevelFilter.Parent;
            _hasCustomFilters = true;
            _needsTargetCollection = false; // Parent routing is handled natively by MmInvoke
            return this;
        }

        /// <summary>
        /// Target sibling nodes (same parent).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToSiblings()
        {
            _levelFilter = MmLevelFilter.Siblings;
            _hasCustomFilters = true;
            _needsTargetCollection = true; // Siblings require collection
            return this;
        }

        /// <summary>
        /// Target all descendants recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToDescendants()
        {
            _levelFilter = MmLevelFilter.Descendants;
            _hasCustomFilters = true;
            _needsTargetCollection = true; // Descendants require collection
            return this;
        }

        /// <summary>
        /// Target all ancestors recursively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToAncestors()
        {
            _levelFilter = MmLevelFilter.Ancestors;
            _hasCustomFilters = true;
            _needsTargetCollection = true; // Ancestors require collection
            return this;
        }

        /// <summary>
        /// Target all connected nodes (bidirectional).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage ToAll()
        {
            _levelFilter = MmLevelFilterHelper.SelfAndBidirectional;
            _hasCustomFilters = true;
            _needsTargetCollection = false; // Direct routing (includes Self)
            return this;
        }

        #endregion

        #region Active Filter Methods

        /// <summary>
        /// Target only active GameObjects.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Active()
        {
            _activeFilter = MmActiveFilter.Active;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Include inactive GameObjects.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage IncludeInactive()
        {
            _activeFilter = MmActiveFilter.All;
            _hasCustomFilters = true;
            return this;
        }

        #endregion

        #region Selected Filter Methods

        /// <summary>
        /// Target only FSM-selected responders.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Selected()
        {
            _selectedFilter = MmSelectedFilter.Selected;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Target all responders regardless of selection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage AllSelected()
        {
            _selectedFilter = MmSelectedFilter.All;
            _hasCustomFilters = true;
            return this;
        }

        #endregion

        #region Network Filter Methods

        /// <summary>
        /// Send message locally only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage LocalOnly()
        {
            _networkFilter = MmNetworkFilter.Local;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Send message over network only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage NetworkOnly()
        {
            _networkFilter = MmNetworkFilter.Network;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Send message both locally and over network.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage AllDestinations()
        {
            _networkFilter = MmNetworkFilter.All;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Send message over network (alias for AllDestinations).
        /// More intuitive naming for network-enabled messaging.
        /// Host double-receive is automatically handled by the framework.
        /// </summary>
        /// <example>
        /// relay.Send("PlayerMoved").ToDescendants().OverNetwork().Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage OverNetwork()
        {
            return AllDestinations();
        }

        /// <summary>
        /// Shorthand for NetworkOnly() - send only over network, not locally.
        /// Use when local handling is done separately.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage RemoteOnly()
        {
            return NetworkOnly();
        }

        #endregion

        #region Tag Methods

        /// <summary>
        /// Filter by a specific tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WithTag(MmTag tag)
        {
            _tag = tag;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter by multiple tags (bitwise OR).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WithTags(params MmTag[] tags)
        {
            MmTag combined = MmTagHelper.Nothing;
            foreach (var tag in tags)
            {
                combined |= tag;
            }
            _tag = combined;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Match any tag (no filtering).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage AnyTag()
        {
            _tag = MmTagHelper.Everything;
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Match no tags.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage NoTags()
        {
            _tag = MmTagHelper.Nothing;
            _hasCustomFilters = true;
            return this;
        }

        #endregion

        #region Spatial Extension Methods (Phase 2 - Task 2.4)

        /// <summary>
        /// Filter to responders within a specified radius from the source.
        /// </summary>
        /// <param name="radius">Maximum distance from source to include responders.</param>
        /// <example>
        /// relay.Send("Explosion").ToDescendants().Within(10f).Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Within(float radius)
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateWithin(radius));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter to responders in a specific direction from the source.
        /// </summary>
        /// <param name="direction">The direction vector from source (will be normalized).</param>
        /// <param name="angle">Maximum angle (in degrees) from the direction to include responders.</param>
        /// <example>
        /// relay.Send("Alert").ToChildren().InDirection(Vector3.forward, 45f).Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage InDirection(Vector3 direction, float angle)
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateInDirection(direction, angle));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter to responders within a bounding box.
        /// </summary>
        /// <param name="bounds">The world-space bounds to check against.</param>
        /// <example>
        /// var room = new Bounds(roomCenter, roomSize);
        /// relay.Send("Activate").ToDescendants().InBounds(room).Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage InBounds(Bounds bounds)
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateInBounds(bounds));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter to responders within a bounding box defined by center and size.
        /// </summary>
        /// <param name="center">The center of the bounding box in world space.</param>
        /// <param name="size">The size of the bounding box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage InBounds(Vector3 center, Vector3 size)
        {
            return InBounds(new Bounds(center, size));
        }

        /// <summary>
        /// Filter to responders in a cone from the source.
        /// Combines direction and distance filtering.
        /// </summary>
        /// <param name="direction">The direction of the cone (will be normalized).</param>
        /// <param name="angle">The half-angle of the cone in degrees.</param>
        /// <param name="range">The maximum distance of the cone.</param>
        /// <example>
        /// relay.Send("Detect").ToDescendants().InCone(transform.forward, 30f, 20f).Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage InCone(Vector3 direction, float angle, float range)
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateInDirection(direction, angle));
            _predicates.Add(MmPredicate.CreateWithin(range));
            _hasCustomFilters = true;
            return this;
        }

        #endregion

        #region Type Filter Methods (Phase 2 - Task 2.5)

        /// <summary>
        /// Filter to responders whose GameObject has a component of type T.
        /// </summary>
        /// <typeparam name="T">The component type to filter by.</typeparam>
        /// <example>
        /// relay.Send("Enable").ToDescendants().OfType&lt;Enemy&gt;().Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage OfType<T>() where T : Component
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateWithComponent(typeof(T)));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter to responders whose GameObject has a component of type T.
        /// Alias for OfType&lt;T&gt;().
        /// </summary>
        /// <typeparam name="T">The component type to require.</typeparam>
        /// <example>
        /// relay.Send("Damage").ToChildren().WithComponent&lt;Health&gt;().Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WithComponent<T>() where T : Component
        {
            return OfType<T>();
        }

        /// <summary>
        /// Filter to responders whose GameObject has a component of the specified type.
        /// </summary>
        /// <param name="componentType">The component type to filter by.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WithComponent(Type componentType)
        {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateWithComponent(componentType));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter to responders whose GameObject implements a specific interface.
        /// </summary>
        /// <typeparam name="T">The interface type to filter by.</typeparam>
        /// <example>
        /// relay.Send("Save").ToDescendants().Implementing&lt;ISaveable&gt;().Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Implementing<T>() where T : class
        {
            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateCustom(go =>
            {
                // Check all components for the interface
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] is T)
                        return true;
                }
                return false;
            }));
            _hasCustomFilters = true;
            return this;
        }

        #endregion

        #region Custom Predicate Methods (Phase 2 - Task 2.6)

        /// <summary>
        /// Filter responders using a custom predicate on the GameObject.
        /// </summary>
        /// <param name="predicate">Function that returns true for GameObjects that should receive the message.</param>
        /// <example>
        /// relay.Send("Alert")
        ///     .ToDescendants()
        ///     .Where(go => go.layer == LayerMask.NameToLayer("Enemies"))
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Where(Func<GameObject, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateCustom(predicate));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter responders using a custom predicate on the MmRelayNode.
        /// </summary>
        /// <param name="predicate">Function that returns true for relay nodes that should receive the message.</param>
        /// <example>
        /// relay.Send("NetworkSync")
        ///     .ToDescendants()
        ///     .WhereRelay(node => node.layer == 5)
        ///     .Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WhereRelay(Func<MmRelayNode, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            EnsurePredicates();
            _predicates.Add(MmPredicate.CreateCustomRelay(predicate));
            _hasCustomFilters = true;
            return this;
        }

        /// <summary>
        /// Filter responders by Unity layer.
        /// </summary>
        /// <param name="layer">The layer index to filter by.</param>
        /// <example>
        /// relay.Send("Detect").ToDescendants().OnLayer(enemyLayer).Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage OnLayer(int layer)
        {
            return Where((GameObject go) => go.layer == layer);
        }

        /// <summary>
        /// Filter responders by Unity layer name.
        /// </summary>
        /// <param name="layerName">The layer name to filter by.</param>
        /// <example>
        /// relay.Send("Detect").ToDescendants().OnLayer("Enemies").Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage OnLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogWarning($"MmFluentMessage: Layer '{layerName}' not found");
                return this;
            }
            return OnLayer(layer);
        }

        /// <summary>
        /// Filter responders by Unity tag.
        /// Note: This is different from MmTag - this uses Unity's GameObject.tag system.
        /// </summary>
        /// <param name="unityTag">The Unity tag to filter by.</param>
        /// <example>
        /// relay.Send("Player").ToDescendants().WithUnityTag("Player").Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage WithUnityTag(string unityTag)
        {
            return Where((GameObject go) => go.CompareTag(unityTag));
        }

        /// <summary>
        /// Filter responders by name pattern (contains).
        /// </summary>
        /// <param name="pattern">The pattern that the GameObject name must contain.</param>
        /// <example>
        /// relay.Send("Update").ToDescendants().Named("Enemy").Execute();
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage Named(string pattern)
        {
            return Where((GameObject go) => go.name.Contains(pattern));
        }

        /// <summary>
        /// Filter responders by exact name match.
        /// </summary>
        /// <param name="exactName">The exact name the GameObject must have.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MmFluentMessage NamedExactly(string exactName)
        {
            return Where((GameObject go) => go.name == exactName);
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Execute the message send operation.
        /// This is the terminal operation that actually sends the message.
        /// When predicates are present, uses predicate-based filtering.
        /// For Descendants/Ancestors/Parent/Siblings filters, collects targets and sends individually.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            if (_relay == null)
            {
                Debug.LogError("MmFluentMessage: Cannot execute without a relay node");
                return;
            }

            // Optimization 2.2: Fast path for simple messages (no predicates, no target collection)
            // This is the most common case - avoids branching for predicates/collection checks
            if (_predicates == null && !_needsTargetCollection)
            {
                // Optimization 2.3: Use pre-allocated metadata for common cases
                MmMetadataBlock metadata = _hasCustomFilters
                    ? CreateMetadata()
                    : DefaultMetadata;
                ExecuteStandard(metadata);
                return;
            }

            // Build metadata block for non-fast-path cases
            MmMetadataBlock fullMetadata = CreateMetadata();

            // If predicates exist, use predicate-based routing
            if (_predicates != null && _predicates.Count > 0)
            {
                ExecuteWithPredicates(fullMetadata);
                return;
            }

            // Optimization 2.1: Use cached flag instead of computing 4 bitwise ANDs + 3 ORs
            // The flag is set in ToXxx() methods when the level filter changes
            if (_needsTargetCollection)
            {
                ExecuteWithTargetCollection();
                return;
            }

            // Standard routing path (SelfAndChildren, Child, Self)
            ExecuteStandard(fullMetadata);
        }

        /// <summary>
        /// Optimization 2.3: Pre-allocated default metadata block for simple messages.
        /// Avoids allocating a new struct every time.
        /// </summary>
        private static readonly MmMetadataBlock DefaultMetadata = new MmMetadataBlock(
            MmLevelFilterHelper.SelfAndChildren,
            MmActiveFilter.All,
            MmSelectedFilter.All,
            MmNetworkFilter.Local
        );

        /// <summary>
        /// Creates a metadata block from current filter settings.
        /// Extracted to avoid code duplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MmMetadataBlock CreateMetadata()
        {
            if (_tag != MmTagHelper.Everything)
            {
                return new MmMetadataBlock(_tag, _levelFilter, _activeFilter, _selectedFilter, _networkFilter);
            }
            return new MmMetadataBlock(_levelFilter, _activeFilter, _selectedFilter, _networkFilter);
        }

        /// <summary>
        /// Execute by collecting targets and sending to each with MmLevelFilter.Self.
        /// Used for Descendants, Ancestors, and Siblings level filters.
        /// Uses Self filter to prevent double-delivery when iterating through collected targets.
        /// </summary>
        private void ExecuteWithTargetCollection()
        {
            // Collect all potential targets based on level filter
            var targets = CollectTargets();

            // Send to each target with Self metadata
            // Using Self (not SelfAndChildren) prevents double-delivery:
            // - We explicitly send to each collected target
            // - Self reaches responders registered with Level=Self on the target
            // - SelfAndChildren would cause messages to propagate down, hitting children twice
            foreach (var targetRelay in targets)
            {
                if (targetRelay == null || targetRelay.gameObject == null)
                    continue;

                // Create Self metadata - responders are registered with Level=Self so this works
                MmMetadataBlock targetMetadata;
                if (_tag != MmTagHelper.Everything)
                {
                    targetMetadata = new MmMetadataBlock(_tag, MmLevelFilter.Self, _activeFilter, _selectedFilter, _networkFilter);
                }
                else
                {
                    targetMetadata = new MmMetadataBlock(MmLevelFilter.Self, _activeFilter, _selectedFilter, _networkFilter);
                }

                // Send message to this specific target
                SendToTarget(targetRelay, targetMetadata);
            }
        }

        /// <summary>
        /// Standard execution without predicates.
        /// </summary>
        private void ExecuteStandard(MmMetadataBlock metadata)
        {
            // Handle MmMessage payloads directly (from Send(MmMessage) or Broadcast(MmMessage))
            if (_payload is MmMessage message)
            {
                message.MetadataBlock = metadata;
                _relay.MmInvoke(message);
                return;
            }

            // Handle primitive payloads
            switch (_method)
            {
                case MmMethod.Initialize:
                case MmMethod.Refresh:
                case MmMethod.NoOp:
                    _relay.MmInvoke(_method, metadata);
                    break;

                // Complete requires a bool parameter (MmBaseResponder casts to MmMessageBool)
                case MmMethod.Complete:
                    _relay.MmInvoke(_method, _payload is bool completeBool ? completeBool : true, metadata);
                    break;

                case MmMethod.SetActive:
                case MmMethod.MessageBool:
                    if (_payload is bool boolValue)
                        _relay.MmInvoke(_method, boolValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected bool payload for {_method}");
                    break;

                case MmMethod.Switch:
                    if (_payload is string switchName)
                        _relay.MmInvoke(_method, switchName, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected string payload for {_method}");
                    break;

                case MmMethod.MessageInt:
                    if (_payload is int intValue)
                        _relay.MmInvoke(_method, intValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected int payload for {_method}");
                    break;

                case MmMethod.MessageFloat:
                    if (_payload is float floatValue)
                        _relay.MmInvoke(_method, floatValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected float payload for {_method}");
                    break;

                case MmMethod.MessageString:
                    if (_payload is string stringValue)
                        _relay.MmInvoke(_method, stringValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected string payload for {_method}");
                    break;

                case MmMethod.MessageVector3:
                    if (_payload is Vector3 vector3Value)
                        _relay.MmInvoke(_method, vector3Value, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected Vector3 payload for {_method}");
                    break;

                case MmMethod.MessageVector4:
                    if (_payload is Vector4 vector4Value)
                        _relay.MmInvoke(_method, vector4Value, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected Vector4 payload for {_method}");
                    break;

                case MmMethod.MessageQuaternion:
                    if (_payload is Quaternion quaternionValue)
                        _relay.MmInvoke(_method, quaternionValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected Quaternion payload for {_method}");
                    break;

                case MmMethod.MessageTransform:
                    if (_payload is Transform transformValue)
                        _relay.MmInvoke(_method, transformValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected Transform payload for {_method}");
                    break;

                case MmMethod.MessageGameObject:
                    if (_payload is GameObject gameObjectValue)
                        _relay.MmInvoke(_method, gameObjectValue, metadata);
                    else
                        Debug.LogError($"MmFluentMessage: Expected GameObject payload for {_method}");
                    break;

                default:
                    Debug.LogError($"MmFluentMessage: Unsupported method {_method} or invalid payload");
                    break;
            }
        }

        /// <summary>
        /// Execute with predicate filtering.
        /// Collects matching relay nodes and sends messages individually.
        /// Uses Self filter to prevent double-delivery.
        /// </summary>
        private void ExecuteWithPredicates(MmMetadataBlock metadata)
        {
            // Collect all potential targets based on level filter
            var targets = CollectTargets();

            // Apply predicates and send to matching targets
            foreach (var targetRelay in targets)
            {
                if (targetRelay == null || targetRelay.gameObject == null)
                    continue;

                // Check if target passes all predicates
                if (!_predicates.EvaluateAll(_relay, targetRelay.gameObject))
                    continue;

                // Create Self metadata - responders are registered with Level=Self so this works
                // Using Self (not SelfAndChildren) prevents double-delivery
                MmMetadataBlock targetMetadata;
                if (_tag != MmTagHelper.Everything)
                {
                    targetMetadata = new MmMetadataBlock(_tag, MmLevelFilter.Self, _activeFilter, _selectedFilter, _networkFilter);
                }
                else
                {
                    targetMetadata = new MmMetadataBlock(MmLevelFilter.Self, _activeFilter, _selectedFilter, _networkFilter);
                }

                // Send message to this specific target
                SendToTarget(targetRelay, targetMetadata);
            }

            // Return predicate list to pool
            MmPredicateListPool.Return(_predicates);
            _predicates = null;
        }

        /// <summary>
        /// Collect potential targets based on level filter.
        /// </summary>
        private List<MmRelayNode> CollectTargets()
        {
            var targets = new List<MmRelayNode>();

            // Check if Self is included
            bool includeSelf = (_levelFilter & MmLevelFilter.Self) != 0;
            if (includeSelf)
            {
                targets.Add(_relay);
            }

            // Check for children/descendants
            bool includeChildren = (_levelFilter & MmLevelFilter.Child) != 0;
            bool includeDescendants = (_levelFilter & MmLevelFilter.Descendants) != 0;

            if (includeChildren || includeDescendants)
            {
                CollectChildRelays(_relay, targets, includeDescendants);
            }

            // Check for parents/ancestors
            bool includeParents = (_levelFilter & MmLevelFilter.Parent) != 0;
            bool includeAncestors = (_levelFilter & MmLevelFilter.Ancestors) != 0;

            if (includeParents || includeAncestors)
            {
                CollectParentRelays(_relay, targets, includeAncestors);
            }

            // Check for siblings
            bool includeSiblings = (_levelFilter & MmLevelFilter.Siblings) != 0;
            if (includeSiblings)
            {
                CollectSiblingRelays(_relay, targets);
            }

            return targets;
        }

        /// <summary>
        /// Collect child relay nodes recursively if needed.
        /// </summary>
        private void CollectChildRelays(MmRelayNode parent, List<MmRelayNode> targets, bool recursive)
        {
            if (parent == null || parent.RoutingTable == null)
                return;

            foreach (var item in parent.RoutingTable)
            {
                if (item.Level != MmLevelFilter.Child)
                    continue;

                var childRelay = item.Responder?.GetRelayNode();
                if (childRelay != null && !targets.Contains(childRelay))
                {
                    targets.Add(childRelay);

                    if (recursive)
                    {
                        CollectChildRelays(childRelay, targets, true);
                    }
                }
            }
        }

        /// <summary>
        /// Collect parent relay nodes recursively if needed.
        /// Uses RoutingTable entries with Level == Parent.
        /// </summary>
        private void CollectParentRelays(MmRelayNode child, List<MmRelayNode> targets, bool recursive)
        {
            if (child == null || child.RoutingTable == null)
                return;

            foreach (var item in child.RoutingTable)
            {
                if (item.Level != MmLevelFilter.Parent)
                    continue;

                var parentRelay = item.Responder?.GetRelayNode();
                if (parentRelay != null && !targets.Contains(parentRelay))
                {
                    targets.Add(parentRelay);

                    if (recursive)
                    {
                        CollectParentRelays(parentRelay, targets, true);
                    }
                }
            }
        }

        /// <summary>
        /// Collect sibling relay nodes (children of same parent).
        /// First finds parents via RoutingTable, then collects their children.
        /// </summary>
        private void CollectSiblingRelays(MmRelayNode node, List<MmRelayNode> targets)
        {
            if (node == null || node.RoutingTable == null)
                return;

            // Find parents first
            foreach (var item in node.RoutingTable)
            {
                if (item.Level != MmLevelFilter.Parent)
                    continue;

                var parent = item.Responder?.GetRelayNode();
                if (parent == null || parent.RoutingTable == null)
                    continue;

                // Collect parent's children (our siblings)
                foreach (var childItem in parent.RoutingTable)
                {
                    if (childItem.Level != MmLevelFilter.Child)
                        continue;

                    var siblingRelay = childItem.Responder?.GetRelayNode();
                    if (siblingRelay != null && siblingRelay != node && !targets.Contains(siblingRelay))
                    {
                        targets.Add(siblingRelay);
                    }
                }
            }
        }

        /// <summary>
        /// Send the message to a specific target relay node.
        /// </summary>
        private void SendToTarget(MmRelayNode target, MmMetadataBlock metadata)
        {
            // Handle MmMessage payloads directly (from Send(MmMessage) or Broadcast(MmMessage))
            if (_payload is MmMessage message)
            {
                var msgCopy = message.Copy();
                msgCopy.MetadataBlock = metadata;
                target.MmInvoke(msgCopy);
                return;
            }

            // Handle primitive payloads
            switch (_method)
            {
                case MmMethod.Initialize:
                case MmMethod.Refresh:
                case MmMethod.NoOp:
                    target.MmInvoke(_method, metadata);
                    break;

                // Complete requires a bool parameter (MmBaseResponder casts to MmMessageBool)
                case MmMethod.Complete:
                    target.MmInvoke(_method, _payload is bool completeBool ? completeBool : true, metadata);
                    break;

                case MmMethod.SetActive:
                case MmMethod.MessageBool:
                    if (_payload is bool boolValue)
                        target.MmInvoke(_method, boolValue, metadata);
                    break;

                case MmMethod.Switch:
                    if (_payload is string switchName)
                        target.MmInvoke(_method, switchName, metadata);
                    break;

                case MmMethod.MessageInt:
                    if (_payload is int intValue)
                        target.MmInvoke(_method, intValue, metadata);
                    break;

                case MmMethod.MessageFloat:
                    if (_payload is float floatValue)
                        target.MmInvoke(_method, floatValue, metadata);
                    break;

                case MmMethod.MessageString:
                    if (_payload is string stringValue)
                        target.MmInvoke(_method, stringValue, metadata);
                    break;

                case MmMethod.MessageVector3:
                    if (_payload is Vector3 vector3Value)
                        target.MmInvoke(_method, vector3Value, metadata);
                    break;

                case MmMethod.MessageVector4:
                    if (_payload is Vector4 vector4Value)
                        target.MmInvoke(_method, vector4Value, metadata);
                    break;

                case MmMethod.MessageQuaternion:
                    if (_payload is Quaternion quaternionValue)
                        target.MmInvoke(_method, quaternionValue, metadata);
                    break;

                case MmMethod.MessageTransform:
                    if (_payload is Transform transformValue)
                        target.MmInvoke(_method, transformValue, metadata);
                    break;

                case MmMethod.MessageGameObject:
                    if (_payload is GameObject gameObjectValue)
                        target.MmInvoke(_method, gameObjectValue, metadata);
                    break;
            }
        }

        /// <summary>
        /// Alias for Execute() to provide alternative syntax.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send()
        {
            Execute();
        }

        #endregion
    }
}