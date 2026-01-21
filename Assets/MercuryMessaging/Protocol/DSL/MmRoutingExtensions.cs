// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.5: Runtime Registration Extensions
// Simplifies runtime hierarchy setup from 2 lines to 1 line

// Suppress MM015: Filter equality checks are intentional for exact match routing logic
#pragma warning disable MM015

using System.Runtime.CompilerServices;
using MercuryMessaging;

namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for simplified runtime hierarchy registration.
    /// Part of DSL Phase 2.5: Runtime Registration.
    /// </summary>
    /// <remarks>
    /// These extensions simplify the common pattern of:
    /// <code>
    /// // Before (2 lines)
    /// parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
    /// childRelay.AddParent(parentRelay);
    ///
    /// // After (1 line)
    /// childRelay.RegisterWith(parentRelay);
    /// </code>
    /// </remarks>
    public static class MmRoutingExtensions
    {
        #region Child Registration (RegisterWith)

        /// <summary>
        /// Registers this relay node as a child of the specified parent.
        /// Combines MmAddToRoutingTable + AddParent in a single call.
        /// </summary>
        /// <param name="child">The child relay node to register.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <returns>The child relay node (for chaining).</returns>
        /// <example>
        /// // Single-line registration
        /// childRelay.RegisterWith(parentRelay);
        ///
        /// // Chained with creation
        /// var relay = go.AddComponent&lt;MmRelayNode&gt;()
        ///     .RegisterWith(parentRelay);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterWith(this MmRelayNode child, MmRelayNode parent)
        {
            if (child == null || parent == null) return child;

            parent.MmAddToRoutingTable(child, MmLevelFilter.Child);
            child.AddParent(parent);

            // If parent is a switch node, rebuild FSM to include new child
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return child;
        }

        /// <summary>
        /// Registers this relay node as a child with a custom level filter.
        /// </summary>
        /// <param name="child">The child relay node to register.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <param name="level">The level filter for routing (default: Child).</param>
        /// <returns>The child relay node (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterWith(this MmRelayNode child, MmRelayNode parent, MmLevelFilter level)
        {
            if (child == null || parent == null) return child;

            parent.MmAddToRoutingTable(child, level);

            // Only add parent reference if registering as a child
            if (level == MmLevelFilter.Child)
            {
                child.AddParent(parent);
            }

            // If parent is a switch node, rebuild FSM to include new child
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return child;
        }

        /// <summary>
        /// Registers this relay node as a child with a custom name.
        /// </summary>
        /// <param name="child">The child relay node to register.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <param name="name">The name for the routing table entry.</param>
        /// <returns>The child relay node (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterWith(this MmRelayNode child, MmRelayNode parent, string name)
        {
            if (child == null || parent == null) return child;

            parent.MmAddToRoutingTable(child, name);
            child.AddParent(parent);

            // If parent is a switch node, rebuild FSM to include new child
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return child;
        }

        #endregion

        #region Bulk Registration (RegisterChildren)

        /// <summary>
        /// Registers multiple children with this parent relay node.
        /// </summary>
        /// <param name="parent">The parent relay node.</param>
        /// <param name="children">The child relay nodes to register.</param>
        /// <returns>The parent relay node (for chaining).</returns>
        /// <example>
        /// parentRelay.RegisterChildren(child1, child2, child3);
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterChildren(this MmRelayNode parent, params MmRelayNode[] children)
        {
            if (parent == null || children == null) return parent;

            foreach (var child in children)
            {
                if (child == null) continue;
                parent.MmAddToRoutingTable(child, MmLevelFilter.Child);
                child.AddParent(parent);
            }

            // If parent is a switch node, rebuild FSM once after all children added
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return parent;
        }

        /// <summary>
        /// Registers multiple responders with this parent relay node.
        /// </summary>
        /// <param name="parent">The parent relay node.</param>
        /// <param name="responders">The responders to register.</param>
        /// <returns>The parent relay node (for chaining).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterResponders(this MmRelayNode parent, params MmResponder[] responders)
        {
            if (parent == null || responders == null) return parent;

            foreach (var responder in responders)
            {
                if (responder == null) continue;
                parent.MmAddToRoutingTable(responder, MmLevelFilter.Child);

                if (responder is MmRelayNode relayNode)
                {
                    relayNode.AddParent(parent);
                }
            }

            // If parent is a switch node, rebuild FSM once after all responders added
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return parent;
        }

        #endregion

        #region Unregistration

        /// <summary>
        /// Unregisters this relay node from its parent.
        /// Removes from routing table and clears parent reference.
        /// </summary>
        /// <param name="child">The child relay node to unregister.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <returns>The child relay node (for chaining).</returns>
        public static MmRelayNode UnregisterFrom(this MmRelayNode child, MmRelayNode parent)
        {
            if (child == null || parent == null) return child;

            RemoveFromRoutingTable(parent, child);

            // If parent is a switch node, rebuild FSM
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return child;
        }

        /// <summary>
        /// Unregisters multiple children from this parent relay node.
        /// </summary>
        /// <param name="parent">The parent relay node.</param>
        /// <param name="children">The child relay nodes to unregister.</param>
        /// <returns>The parent relay node (for chaining).</returns>
        public static MmRelayNode UnregisterChildren(this MmRelayNode parent, params MmRelayNode[] children)
        {
            if (parent == null || children == null) return parent;

            foreach (var child in children)
            {
                if (child == null) continue;
                RemoveFromRoutingTable(parent, child);
            }

            // If parent is a switch node, rebuild FSM once after all children removed
            if (parent is MmRelaySwitchNode switchNode)
            {
                switchNode.RebuildFSM();
            }

            return parent;
        }

        /// <summary>
        /// Helper method to remove a responder from a relay node's routing table.
        /// </summary>
        private static void RemoveFromRoutingTable(MmRelayNode parent, MmResponder responder)
        {
            if (parent?.RoutingTable == null || responder == null) return;

            // Find and remove the item from routing table
            for (int i = parent.RoutingTable.Count - 1; i >= 0; i--)
            {
                if (parent.RoutingTable[i]?.Responder == responder)
                {
                    parent.RoutingTable.RemoveAt(i);
                    break; // Only remove first match
                }
            }
        }

        #endregion

        #region Responder Extensions

        /// <summary>
        /// Registers this responder's relay node as a child of the specified parent.
        /// Null-safe: no-op if responder has no relay node.
        /// </summary>
        /// <param name="responder">The responder to register.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <returns>The relay node (for chaining), or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode RegisterWith(this MmBaseResponder responder, MmRelayNode parent)
        {
            var relay = responder?.GetRelayNode();
            return relay?.RegisterWith(parent);
        }

        /// <summary>
        /// Unregisters this responder's relay node from the specified parent.
        /// Null-safe: no-op if responder has no relay node.
        /// </summary>
        /// <param name="responder">The responder to unregister.</param>
        /// <param name="parent">The parent relay node.</param>
        /// <returns>The relay node (for chaining), or null if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode UnregisterFrom(this MmBaseResponder responder, MmRelayNode parent)
        {
            var relay = responder?.GetRelayNode();
            return relay?.UnregisterFrom(parent);
        }

        #endregion

        #region Hierarchy Queries

        /// <summary>
        /// Checks if this relay node has any parents registered via AddParent.
        /// Uses the routing table to find parent entries.
        /// </summary>
        /// <param name="relay">The relay node to query.</param>
        /// <returns>True if the node has parent entries in its routing table.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasParents(this MmRelayNode relay)
        {
            if (relay?.RoutingTable == null) return false;

            foreach (var item in relay.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Parent)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the first parent relay node from the routing table.
        /// </summary>
        /// <param name="relay">The relay node to query.</param>
        /// <returns>The first parent relay node, or null if none.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MmRelayNode GetFirstParent(this MmRelayNode relay)
        {
            if (relay?.RoutingTable == null) return null;

            foreach (var item in relay.RoutingTable)
            {
                if (item.Level == MmLevelFilter.Parent && item.Responder is MmRelayNode parentRelay)
                    return parentRelay;
            }
            return null;
        }

        /// <summary>
        /// Checks if this relay node is a root (has no parents).
        /// </summary>
        /// <param name="relay">The relay node to query.</param>
        /// <returns>True if the node has no parent entries.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRoot(this MmRelayNode relay)
        {
            return !relay.HasParents();
        }

        #endregion
    }
}
