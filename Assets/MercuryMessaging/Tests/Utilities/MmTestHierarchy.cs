// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmTestHierarchy.cs - Test utilities for quickly creating MercuryMessaging hierarchies
// Part of DX3: Developer Experience Improvements

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Test utility for quickly creating MercuryMessaging hierarchies.
    /// Automatically handles routing table registration and parent relationships.
    /// Implements IDisposable for clean teardown in tests.
    /// </summary>
    /// <example>
    /// <code>
    /// // Simple linear hierarchy
    /// using var hierarchy = MmTestHierarchy.Create("Root/Child/Grandchild");
    /// var root = hierarchy.Root;
    /// var grandchild = hierarchy.GetNode("Grandchild");
    ///
    /// // With responder types
    /// using var hierarchy = MmTestHierarchy.Create("Root/Child",
    ///     new[] { null, typeof(MyResponder) });
    ///
    /// // Builder pattern for branching
    /// using var hierarchy = MmTestHierarchy.Build("Root")
    ///     .AddChild&lt;MyResponder&gt;("Child1")
    ///     .Parent()
    ///     .AddChild("Child2")
    ///     .Build();
    /// </code>
    /// </example>
    public class MmTestHierarchy : IDisposable
    {
        private readonly Dictionary<string, MmRelayNode> _nodes = new Dictionary<string, MmRelayNode>();
        private readonly MmRelayNode _root;
        private bool _disposed;

        /// <summary>
        /// The root relay node of this hierarchy.
        /// </summary>
        public MmRelayNode Root => _root;

        /// <summary>
        /// All nodes in this hierarchy, keyed by name.
        /// </summary>
        public IReadOnlyDictionary<string, MmRelayNode> Nodes => _nodes;

        internal MmTestHierarchy(MmRelayNode root, Dictionary<string, MmRelayNode> nodes)
        {
            _root = root;
            _nodes = nodes;
        }

        #region Static Factory Methods

        /// <summary>
        /// Create a hierarchy from a path string.
        /// Path format: "Root/Child1/Grandchild" (creates linear hierarchy)
        /// </summary>
        /// <param name="path">Path like "Root/Child1/Child2"</param>
        public static MmTestHierarchy Create(string path)
        {
            return Create(path, null);
        }

        /// <summary>
        /// Create a hierarchy from a path string with responder types.
        /// </summary>
        /// <param name="path">Path like "Root/Child1/Child2"</param>
        /// <param name="responderTypes">Array of responder types, one per node. Null for no responder.</param>
        public static MmTestHierarchy Create(string path, Type[] responderTypes)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var parts = path.Split('/');
            if (parts.Length == 0)
                throw new ArgumentException("Path must contain at least one node name", nameof(path));

            MmRelayNode parent = null;
            MmRelayNode root = null;
            var nodes = new Dictionary<string, MmRelayNode>();

            for (int i = 0; i < parts.Length; i++)
            {
                var name = parts[i].Trim();
                var responderType = responderTypes != null && i < responderTypes.Length
                    ? responderTypes[i]
                    : null;

                var node = CreateNode(name, parent, responderType);
                nodes[name] = node;

                if (root == null)
                {
                    root = node;
                }

                parent = node;
            }

            return new MmTestHierarchy(root, nodes);
        }

        /// <summary>
        /// Start building a hierarchy using the builder pattern.
        /// </summary>
        /// <param name="rootName">Name of the root node</param>
        public static MmTestHierarchyBuilder Build(string rootName)
        {
            return new MmTestHierarchyBuilder(rootName);
        }

        /// <summary>
        /// Create a simple single-node hierarchy.
        /// </summary>
        public static MmTestHierarchy CreateSingle(string name)
        {
            return Create(name);
        }

        /// <summary>
        /// Create a single node with a specific responder type.
        /// </summary>
        public static MmTestHierarchy CreateSingle<T>(string name) where T : MmBaseResponder
        {
            return Create(name, new[] { typeof(T) });
        }

        #endregion

        #region Node Access

        /// <summary>
        /// Get a node by name from this hierarchy.
        /// </summary>
        public MmRelayNode GetNode(string name)
        {
            return _nodes.TryGetValue(name, out var node) ? node : null;
        }

        /// <summary>
        /// Get the responder component of type T from a named node.
        /// </summary>
        public T GetResponder<T>(string nodeName) where T : MmBaseResponder
        {
            var node = GetNode(nodeName);
            return node?.GetComponent<T>();
        }

        /// <summary>
        /// Get the responder component of type T from the root node.
        /// </summary>
        public T GetRootResponder<T>() where T : MmBaseResponder
        {
            return _root?.GetComponent<T>();
        }

        /// <summary>
        /// Check if a node with the given name exists.
        /// </summary>
        public bool HasNode(string name)
        {
            return _nodes.ContainsKey(name);
        }

        /// <summary>
        /// Get the number of nodes in this hierarchy.
        /// </summary>
        public int Count => _nodes.Count;

        #endregion

        #region Hierarchy Operations

        /// <summary>
        /// Add a new child node to an existing node in this hierarchy.
        /// </summary>
        public MmRelayNode AddChild(string parentName, string childName, Type responderType = null)
        {
            var parent = GetNode(parentName);
            if (parent == null)
                throw new ArgumentException($"Parent node '{parentName}' not found", nameof(parentName));

            var child = CreateNode(childName, parent, responderType);
            _nodes[childName] = child;
            return child;
        }

        /// <summary>
        /// Add a new child node with a responder type.
        /// </summary>
        public MmRelayNode AddChild<T>(string parentName, string childName) where T : MmBaseResponder
        {
            return AddChild(parentName, childName, typeof(T));
        }

        #endregion

        #region Internal Helpers

        private static MmRelayNode CreateNode(string name, MmRelayNode parent, Type responderType)
        {
            var go = new GameObject(name);
            var relay = go.AddComponent<MmRelayNode>();

            // Add responder if specified
            if (responderType != null && typeof(MmBaseResponder).IsAssignableFrom(responderType))
            {
                go.AddComponent(responderType);
            }

            // Wire parent relationship
            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
                parent.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(parent);
            }

            // Refresh responders to pick up any added components
            relay.MmRefreshResponders();

            return relay;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Clean up all GameObjects in this hierarchy.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_root != null)
            {
                UnityEngine.Object.DestroyImmediate(_root.gameObject);
            }

            _nodes.Clear();
        }

        #endregion
    }

    /// <summary>
    /// Builder for creating complex test hierarchies with branching.
    /// </summary>
    public class MmTestHierarchyBuilder
    {
        private readonly NodeSpec _root;
        private NodeSpec _current;
        private readonly Stack<NodeSpec> _parentStack = new Stack<NodeSpec>();

        internal MmTestHierarchyBuilder(string rootName)
        {
            _root = new NodeSpec { Name = rootName };
            _current = _root;
        }

        /// <summary>
        /// Add a child node to the current node. The new child becomes the current node.
        /// </summary>
        public MmTestHierarchyBuilder AddChild(string name)
        {
            var child = new NodeSpec { Name = name };
            _current.Children.Add(child);
            _parentStack.Push(_current);
            _current = child;
            return this;
        }

        /// <summary>
        /// Add a child node with a responder component.
        /// </summary>
        public MmTestHierarchyBuilder AddChild<T>(string name) where T : MmBaseResponder
        {
            var child = new NodeSpec { Name = name, ResponderType = typeof(T) };
            _current.Children.Add(child);
            _parentStack.Push(_current);
            _current = child;
            return this;
        }

        /// <summary>
        /// Add a sibling node (child of the current node's parent).
        /// </summary>
        public MmTestHierarchyBuilder AddSibling(string name)
        {
            if (_parentStack.Count > 0)
            {
                var parent = _parentStack.Peek();
                var sibling = new NodeSpec { Name = name };
                parent.Children.Add(sibling);
                _current = sibling;
            }
            return this;
        }

        /// <summary>
        /// Add a sibling node with a responder component.
        /// </summary>
        public MmTestHierarchyBuilder AddSibling<T>(string name) where T : MmBaseResponder
        {
            if (_parentStack.Count > 0)
            {
                var parent = _parentStack.Peek();
                var sibling = new NodeSpec { Name = name, ResponderType = typeof(T) };
                parent.Children.Add(sibling);
                _current = sibling;
            }
            return this;
        }

        /// <summary>
        /// Navigate back to the parent node.
        /// </summary>
        public MmTestHierarchyBuilder Parent()
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
        public MmTestHierarchyBuilder Root()
        {
            while (_parentStack.Count > 0)
            {
                _current = _parentStack.Pop();
            }
            _current = _root;
            return this;
        }

        /// <summary>
        /// Add a responder type to the current node.
        /// </summary>
        public MmTestHierarchyBuilder WithResponder<T>() where T : MmBaseResponder
        {
            _current.ResponderType = typeof(T);
            return this;
        }

        /// <summary>
        /// Make the current node a switch node (MmRelaySwitchNode).
        /// </summary>
        public MmTestHierarchyBuilder AsSwitchNode()
        {
            _current.IsSwitchNode = true;
            return this;
        }

        /// <summary>
        /// Build the hierarchy and return a disposable MmTestHierarchy.
        /// </summary>
        public MmTestHierarchy Build()
        {
            var nodes = new Dictionary<string, MmRelayNode>();
            var root = BuildNode(_root, null, nodes);
            return new MmTestHierarchy(root, nodes);
        }

        private MmRelayNode BuildNode(NodeSpec spec, MmRelayNode parent, Dictionary<string, MmRelayNode> nodes)
        {
            // Create GameObject
            var go = new GameObject(spec.Name);

            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
            }

            // Add relay node (switch or regular)
            MmRelayNode relay;
            if (spec.IsSwitchNode)
            {
                relay = go.AddComponent<MmRelaySwitchNode>();
            }
            else
            {
                relay = go.AddComponent<MmRelayNode>();
            }

            // Add responder if specified
            if (spec.ResponderType != null)
            {
                go.AddComponent(spec.ResponderType);
            }

            // Wire parent relationship
            if (parent != null)
            {
                parent.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(parent);
            }

            // Build children
            foreach (var childSpec in spec.Children)
            {
                BuildNode(childSpec, relay, nodes);
            }

            // Refresh responders after all children are added
            relay.MmRefreshResponders();

            // Store in nodes dictionary
            nodes[spec.Name] = relay;

            return relay;
        }

        private class NodeSpec
        {
            public string Name { get; set; }
            public Type ResponderType { get; set; }
            public bool IsSwitchNode { get; set; }
            public List<NodeSpec> Children { get; } = new List<NodeSpec>();
        }
    }
}
