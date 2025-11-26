// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// HierarchyBuilder.cs - Fluent builder for MercuryMessaging hierarchies
// Part of DSL Overhaul Phase 7

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Fluent builder for constructing MercuryMessaging hierarchies programmatically.
    /// Handles GameObject creation, MmRelayNode setup, and routing table registration.
    ///
    /// Example Usage:
    /// <code>
    /// // Build a message hierarchy
    /// var root = MmHierarchy.Create("GameManager")
    ///     .AddChild("UIController")
    ///         .AddChild("MainMenu")
    ///         .AddChild("GameplayUI")
    ///         .Parent()
    ///     .AddChild("GameLogic")
    ///         .AddChild("PlayerManager")
    ///         .AddChild("EnemyManager")
    ///         .Parent()
    ///     .Build();
    ///
    /// // Add to existing hierarchy
    /// MmHierarchy.AttachTo(existingRelay)
    ///     .AddChild("NewChild")
    ///     .Build();
    /// </code>
    /// </summary>
    public class HierarchyBuilder
    {
        private readonly NodeBuilder _root;
        private NodeBuilder _current;
        private readonly Stack<NodeBuilder> _parentStack = new Stack<NodeBuilder>();

        /// <summary>
        /// Create a new hierarchy builder with a root node.
        /// </summary>
        public HierarchyBuilder(string rootName)
        {
            _root = new NodeBuilder(rootName, null);
            _current = _root;
        }

        /// <summary>
        /// Create a builder attached to an existing relay node.
        /// </summary>
        public HierarchyBuilder(MmRelayNode existingRoot)
        {
            _root = new NodeBuilder(existingRoot);
            _current = _root;
        }

        #region Navigation

        /// <summary>
        /// Add a child node to the current node.
        /// The new child becomes the current node.
        /// </summary>
        public HierarchyBuilder AddChild(string name)
        {
            _parentStack.Push(_current);
            var child = new NodeBuilder(name, _current);
            _current.Children.Add(child);
            _current = child;
            return this;
        }

        /// <summary>
        /// Add a child node with a custom component type.
        /// </summary>
        public HierarchyBuilder AddChild<T>(string name) where T : MmBaseResponder
        {
            AddChild(name);
            _current.ResponderType = typeof(T);
            return this;
        }

        /// <summary>
        /// Navigate back to the parent node.
        /// </summary>
        public HierarchyBuilder Parent()
        {
            if (_parentStack.Count > 0)
            {
                _current = _parentStack.Pop();
            }
            return this;
        }

        /// <summary>
        /// Navigate back to the root node.
        /// </summary>
        public HierarchyBuilder Root()
        {
            while (_parentStack.Count > 0)
            {
                _current = _parentStack.Pop();
            }
            return this;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Set the tag for the current node.
        /// </summary>
        public HierarchyBuilder WithTag(MmTag tag)
        {
            _current.Tag = tag;
            return this;
        }

        /// <summary>
        /// Set the layer for the current node.
        /// </summary>
        public HierarchyBuilder OnLayer(int layer)
        {
            _current.Layer = layer;
            return this;
        }

        /// <summary>
        /// Set the position for the current node.
        /// </summary>
        public HierarchyBuilder AtPosition(Vector3 position)
        {
            _current.Position = position;
            return this;
        }

        /// <summary>
        /// Add a responder component to the current node.
        /// </summary>
        public HierarchyBuilder WithResponder<T>() where T : MmBaseResponder
        {
            _current.ResponderType = typeof(T);
            return this;
        }

        /// <summary>
        /// Add an action to run after the node is created.
        /// </summary>
        public HierarchyBuilder Configure(Action<MmRelayNode> configure)
        {
            _current.ConfigureAction = configure;
            return this;
        }

        /// <summary>
        /// Set this node as inactive by default.
        /// </summary>
        public HierarchyBuilder Inactive()
        {
            _current.StartActive = false;
            return this;
        }

        /// <summary>
        /// Make this a switch node (MmRelaySwitchNode).
        /// </summary>
        public HierarchyBuilder AsSwitchNode()
        {
            _current.IsSwitchNode = true;
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Build the hierarchy and return the root relay node.
        /// </summary>
        public MmRelayNode Build()
        {
            return BuildNode(_root, null);
        }

        /// <summary>
        /// Build the hierarchy under an existing parent.
        /// </summary>
        public MmRelayNode BuildUnder(Transform parent)
        {
            var rootRelay = BuildNode(_root, parent);
            return rootRelay;
        }

        private MmRelayNode BuildNode(NodeBuilder builder, Transform parent)
        {
            GameObject gameObject;
            MmRelayNode relay;

            // If using existing relay node
            if (builder.ExistingRelay != null)
            {
                relay = builder.ExistingRelay;
                gameObject = relay.gameObject;
            }
            else
            {
                // Create new GameObject
                gameObject = new GameObject(builder.Name);

                if (parent != null)
                {
                    gameObject.transform.SetParent(parent);
                }

                // Add relay node (switch or regular)
                if (builder.IsSwitchNode)
                {
                    relay = gameObject.AddComponent<MmRelaySwitchNode>();
                }
                else
                {
                    relay = gameObject.AddComponent<MmRelayNode>();
                }
            }

            // Apply configuration
            if (builder.Tag.HasValue)
            {
                relay.Tag = builder.Tag.Value;
            }

            if (builder.Layer.HasValue)
            {
                relay.layer = builder.Layer.Value;
            }

            if (builder.Position.HasValue)
            {
                gameObject.transform.localPosition = builder.Position.Value;
            }

            // Add responder component
            if (builder.ResponderType != null)
            {
                gameObject.AddComponent(builder.ResponderType);
            }

            // Run configure action
            builder.ConfigureAction?.Invoke(relay);

            // Set active state
            if (!builder.StartActive)
            {
                gameObject.SetActive(false);
            }

            // Build children
            foreach (var childBuilder in builder.Children)
            {
                var childRelay = BuildNode(childBuilder, gameObject.transform);

                // Register child in routing table
                relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(relay);
            }

            // Refresh responders
            relay.MmRefreshResponders();

            return relay;
        }

        #endregion

        /// <summary>
        /// Internal node builder.
        /// </summary>
        private class NodeBuilder
        {
            public string Name { get; }
            public NodeBuilder ParentBuilder { get; }
            public MmRelayNode ExistingRelay { get; }
            public List<NodeBuilder> Children { get; } = new List<NodeBuilder>();

            public MmTag? Tag { get; set; }
            public int? Layer { get; set; }
            public Vector3? Position { get; set; }
            public Type ResponderType { get; set; }
            public Action<MmRelayNode> ConfigureAction { get; set; }
            public bool StartActive { get; set; } = true;
            public bool IsSwitchNode { get; set; }

            public NodeBuilder(string name, NodeBuilder parent)
            {
                Name = name;
                ParentBuilder = parent;
            }

            public NodeBuilder(MmRelayNode existing)
            {
                Name = existing.gameObject.name;
                ExistingRelay = existing;
            }
        }
    }

    /// <summary>
    /// Static factory for creating hierarchy builders.
    /// </summary>
    public static class MmHierarchy
    {
        /// <summary>
        /// Create a new hierarchy starting with a root node.
        /// </summary>
        /// <example>
        /// var root = MmHierarchy.Create("GameRoot")
        ///     .AddChild("StateManager")
        ///     .AsSwitchNode()
        ///     .Build();
        /// </example>
        public static HierarchyBuilder Create(string rootName)
        {
            return new HierarchyBuilder(rootName);
        }

        /// <summary>
        /// Start building under an existing relay node.
        /// </summary>
        public static HierarchyBuilder AttachTo(MmRelayNode existingRoot)
        {
            return new HierarchyBuilder(existingRoot);
        }

        /// <summary>
        /// Create a simple relay node with a name.
        /// </summary>
        public static MmRelayNode CreateNode(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent);

            var relay = go.AddComponent<MmRelayNode>();
            return relay;
        }

        /// <summary>
        /// Create a switch node with a name.
        /// </summary>
        public static MmRelaySwitchNode CreateSwitchNode(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
                go.transform.SetParent(parent);

            var relay = go.AddComponent<MmRelaySwitchNode>();
            return relay;
        }
    }

    /// <summary>
    /// Extension methods for relay node hierarchy management.
    /// </summary>
    public static class MmHierarchyExtensions
    {
        /// <summary>
        /// Add a child relay node to this node.
        /// Creates the GameObject and registers in routing table.
        /// </summary>
        public static MmRelayNode AddChildNode(this MmRelayNode parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);

            var relay = go.AddComponent<MmRelayNode>();
            parent.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(parent);

            return relay;
        }

        /// <summary>
        /// Add a child relay node with a responder component.
        /// </summary>
        public static MmRelayNode AddChildNode<T>(this MmRelayNode parent, string name) where T : MmBaseResponder
        {
            var relay = parent.AddChildNode(name);
            relay.gameObject.AddComponent<T>();
            relay.MmRefreshResponders();
            return relay;
        }

        /// <summary>
        /// Add a switch node child.
        /// </summary>
        public static MmRelaySwitchNode AddSwitchChild(this MmRelayNode parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);

            var relay = go.AddComponent<MmRelaySwitchNode>();
            parent.MmAddToRoutingTable(relay, MmLevelFilter.Child);
            relay.AddParent(parent);

            return relay;
        }

        /// <summary>
        /// Detach this node from its parent hierarchy.
        /// </summary>
        public static void DetachFromParent(this MmRelayNode node)
        {
            if (node == null) return;

            // Remove from parent routing table
            node.RefreshParents(); // Ensure parents are current
            // Note: MmRelayNode doesn't expose direct parent removal
            // but we can detach the transform
            node.transform.SetParent(null);
        }

        /// <summary>
        /// Move this node to a new parent.
        /// </summary>
        public static void ReparentTo(this MmRelayNode node, MmRelayNode newParent)
        {
            if (node == null || newParent == null) return;

            node.transform.SetParent(newParent.transform);
            newParent.MmAddToRoutingTable(node, MmLevelFilter.Child);
            node.AddParent(newParent);
        }

        /// <summary>
        /// Clone this node's hierarchy structure under a new parent.
        /// Does NOT clone responder state, only structure.
        /// </summary>
        public static MmRelayNode CloneStructure(this MmRelayNode source, Transform newParent = null)
        {
            if (source == null) return null;

            var clone = UnityEngine.Object.Instantiate(source.gameObject, newParent);
            return clone.GetComponent<MmRelayNode>();
        }
    }
}
