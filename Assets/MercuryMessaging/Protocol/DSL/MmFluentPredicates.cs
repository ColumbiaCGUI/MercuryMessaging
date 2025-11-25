using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Predicate types for fluent message filtering.
    /// Used to filter target responders based on spatial, type, or custom criteria.
    /// </summary>
    public enum MmPredicateType
    {
        /// <summary>Filter responders within a radius of the source.</summary>
        Within,
        /// <summary>Filter responders in a specific direction from the source.</summary>
        InDirection,
        /// <summary>Filter responders within a bounding box.</summary>
        InBounds,
        /// <summary>Filter responders with a specific component type.</summary>
        WithComponent,
        /// <summary>Custom predicate function.</summary>
        Custom
    }

    /// <summary>
    /// Represents a single predicate for filtering responders.
    /// Stores the predicate type and associated parameters.
    /// </summary>
    public struct MmPredicate
    {
        public MmPredicateType Type;

        // Spatial parameters
        public float Radius;
        public Vector3 Direction;
        public float Angle;
        public Bounds Bounds;

        // Type parameter
        public Type ComponentType;

        // Custom predicate
        public Func<GameObject, bool> CustomFunc;
        public Func<MmRelayNode, bool> CustomRelayFunc;

        /// <summary>
        /// Create a Within radius predicate.
        /// </summary>
        public static MmPredicate CreateWithin(float radius)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.Within,
                Radius = radius
            };
        }

        /// <summary>
        /// Create an InDirection predicate.
        /// </summary>
        public static MmPredicate CreateInDirection(Vector3 direction, float angle)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.InDirection,
                Direction = direction.normalized,
                Angle = angle
            };
        }

        /// <summary>
        /// Create an InBounds predicate.
        /// </summary>
        public static MmPredicate CreateInBounds(Bounds bounds)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.InBounds,
                Bounds = bounds
            };
        }

        /// <summary>
        /// Create a WithComponent predicate.
        /// </summary>
        public static MmPredicate CreateWithComponent(Type componentType)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.WithComponent,
                ComponentType = componentType
            };
        }

        /// <summary>
        /// Create a custom GameObject predicate.
        /// </summary>
        public static MmPredicate CreateCustom(Func<GameObject, bool> predicate)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.Custom,
                CustomFunc = predicate
            };
        }

        /// <summary>
        /// Create a custom MmRelayNode predicate.
        /// </summary>
        public static MmPredicate CreateCustomRelay(Func<MmRelayNode, bool> predicate)
        {
            return new MmPredicate
            {
                Type = MmPredicateType.Custom,
                CustomRelayFunc = predicate
            };
        }

        /// <summary>
        /// Evaluate this predicate against a target GameObject.
        /// </summary>
        /// <param name="source">The source relay node (for spatial calculations)</param>
        /// <param name="target">The target GameObject to evaluate</param>
        /// <returns>True if the target passes the predicate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Evaluate(MmRelayNode source, GameObject target)
        {
            if (target == null) return false;

            switch (Type)
            {
                case MmPredicateType.Within:
                    if (source == null) return true;
                    float distance = Vector3.Distance(
                        source.transform.position,
                        target.transform.position
                    );
                    return distance <= Radius;

                case MmPredicateType.InDirection:
                    if (source == null) return true;
                    Vector3 toTarget = (target.transform.position - source.transform.position).normalized;
                    float angleToTarget = Vector3.Angle(Direction, toTarget);
                    return angleToTarget <= Angle;

                case MmPredicateType.InBounds:
                    return Bounds.Contains(target.transform.position);

                case MmPredicateType.WithComponent:
                    return ComponentType != null && target.GetComponent(ComponentType) != null;

                case MmPredicateType.Custom:
                    if (CustomFunc != null)
                        return CustomFunc(target);
                    if (CustomRelayFunc != null)
                    {
                        var relay = target.GetComponent<MmRelayNode>();
                        return relay != null && CustomRelayFunc(relay);
                    }
                    return true;

                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// Container for multiple predicates. Used by MmFluentMessage to store
    /// predicate chains without allocating on each method call.
    /// </summary>
    public class MmPredicateList
    {
        private readonly List<MmPredicate> _predicates = new List<MmPredicate>(4);

        /// <summary>
        /// Number of predicates in this list.
        /// </summary>
        public int Count => _predicates.Count;

        /// <summary>
        /// Add a predicate to the list.
        /// </summary>
        public void Add(MmPredicate predicate)
        {
            _predicates.Add(predicate);
        }

        /// <summary>
        /// Evaluate all predicates against a target.
        /// Returns true only if ALL predicates pass.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EvaluateAll(MmRelayNode source, GameObject target)
        {
            for (int i = 0; i < _predicates.Count; i++)
            {
                if (!_predicates[i].Evaluate(source, target))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Clear all predicates.
        /// </summary>
        public void Clear()
        {
            _predicates.Clear();
        }
    }

    /// <summary>
    /// Object pool for MmPredicateList to avoid allocations.
    /// </summary>
    internal static class MmPredicateListPool
    {
        private static readonly Stack<MmPredicateList> _pool = new Stack<MmPredicateList>(16);

        /// <summary>
        /// Get a predicate list from the pool or create a new one.
        /// </summary>
        public static MmPredicateList Get()
        {
            if (_pool.Count > 0)
            {
                var list = _pool.Pop();
                list.Clear();
                return list;
            }
            return new MmPredicateList();
        }

        /// <summary>
        /// Return a predicate list to the pool.
        /// </summary>
        public static void Return(MmPredicateList list)
        {
            if (list != null && _pool.Count < 64)
            {
                list.Clear();
                _pool.Push(list);
            }
        }
    }
}
