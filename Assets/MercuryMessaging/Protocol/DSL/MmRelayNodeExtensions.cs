using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Convenience extension methods for MmRelayNode to simplify common messaging patterns.
    /// Provides high-level abstractions over the fluent API and standard MmInvoke calls.
    ///
    /// Phase 3 - Task 3.2: Convenience Extensions
    ///
    /// NOTE: These methods use the fluent API internally for proper routing.
    /// The fluent API properly transforms level filters so responders receive messages.
    ///
    /// RECOMMENDATION: For shorter code, consider using MmQuickExtensions:
    /// - relay.Init() instead of relay.Broadcast(MmMethod.Initialize)
    /// - relay.Tell(value) instead of relay.Broadcast(MmMethod.MessageX, value)
    /// - relay.Done() instead of relay.NotifyComplete()
    /// - relay.Report(value) instead of relay.Notify(MmMethod.MessageX, value)
    /// </summary>
    public static class MmRelayNodeExtensions
    {
        #region Broadcast Methods

        /// <summary>
        /// Broadcast a message to self and all descendants (children and their children recursively).
        /// Uses SelfAndChildren routing so responders on the same object receive the message.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method)
        {
            relay.MmInvoke(method, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a boolean message to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method, bool value)
        {
            relay.MmInvoke(method, value, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast an integer message to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method, int value)
        {
            relay.MmInvoke(method, value, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a float message to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method, float value)
        {
            relay.MmInvoke(method, value, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a string message to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method, string value)
        {
            relay.MmInvoke(method, value, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a Vector3 message to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMethod method, Vector3 value)
        {
            relay.MmInvoke(method, value, new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            ));
        }

        /// <summary>
        /// Broadcast a message object to self and all descendants.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Broadcast(this MmRelayNode relay, MmMessage message)
        {
            message.MetadataBlock = new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            relay.MmInvoke(message);
        }

        // NOTE: BroadcastSetActive() has been moved to MmMessagingExtensions.cs
        // as part of the DSL Overhaul. Use the unified API from MmMessagingExtensions instead.

        #endregion

        #region Notify Methods (Upward Communication)

        /// <summary>
        /// Notify parent nodes of an event (upward communication).
        /// Uses the fluent API internally for proper routing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMethod method)
        {
            new MmFluentMessage(relay, method, null).ToParents().Execute();
        }

        /// <summary>
        /// Notify parents with a boolean value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMethod method, bool value)
        {
            new MmFluentMessage(relay, method, value).ToParents().Execute();
        }

        /// <summary>
        /// Notify parents with an integer value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMethod method, int value)
        {
            new MmFluentMessage(relay, method, value).ToParents().Execute();
        }

        /// <summary>
        /// Notify parents with a float value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMethod method, float value)
        {
            new MmFluentMessage(relay, method, value).ToParents().Execute();
        }

        /// <summary>
        /// Notify parents with a string value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMethod method, string value)
        {
            new MmFluentMessage(relay, method, value).ToParents().Execute();
        }

        /// <summary>
        /// Notify parents with a message object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify(this MmRelayNode relay, MmMessage message)
        {
            relay.Send(message).ToParents().Execute();
        }

        /// <summary>
        /// Notify all ancestors (recursive upward) with a method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotifyAncestors(this MmRelayNode relay, MmMethod method)
        {
            new MmFluentMessage(relay, method, null).ToAncestors().Execute();
        }

        // NOTE: NotifyComplete() has been moved to MmMessagingExtensions.cs
        // as part of the DSL Overhaul. Use the unified API from MmMessagingExtensions instead.

        #endregion

        #region SendTo Methods (Named Target)

        /// <summary>
        /// Send a message to a specifically named target in the hierarchy.
        /// Searches descendants for a GameObject with the given name.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMethod method)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                // Use Self filter so the target's responders receive the message
                target.MmInvoke(method, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send a boolean message to a named target.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMethod method, bool value)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                target.MmInvoke(method, value, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send an integer message to a named target.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMethod method, int value)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                target.MmInvoke(method, value, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send a float message to a named target.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMethod method, float value)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                target.MmInvoke(method, value, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send a string message to a named target.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMethod method, string value)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                target.MmInvoke(method, value, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send a message object to a named target.
        /// </summary>
        public static void SendTo(this MmRelayNode relay, string targetName, MmMessage message)
        {
            var target = FindTargetRelay(relay, targetName);
            if (target != null)
            {
                message.MetadataBlock = new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                );
                target.MmInvoke(message);
            }
            else
            {
                Debug.LogWarning($"MmRelayNodeExtensions.SendTo: Target '{targetName}' not found in hierarchy");
            }
        }

        /// <summary>
        /// Send a message directly to a specific relay node reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendTo(this MmRelayNode relay, MmRelayNode target, MmMethod method)
        {
            if (target != null)
            {
                target.MmInvoke(method, new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                ));
            }
        }

        /// <summary>
        /// Send a message directly to a specific relay node reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendTo(this MmRelayNode relay, MmRelayNode target, MmMessage message)
        {
            if (target != null)
            {
                message.MetadataBlock = new MmMetadataBlock(
                    MmLevelFilterHelper.SelfAndChildren,
                    MmActiveFilter.All,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                );
                target.MmInvoke(message);
            }
        }

        #endregion

        #region Query/Response Pattern

        private static readonly Dictionary<int, Action<MmMessage>> _pendingQueries =
            new Dictionary<int, Action<MmMessage>>();
        private static int _nextQueryId = 1;

        /// <summary>
        /// Send a query and register a callback for the response.
        /// </summary>
        public static int Query(this MmRelayNode relay, MmMethod method, Action<MmMessage> onResponse)
        {
            int queryId = _nextQueryId++;
            _pendingQueries[queryId] = onResponse;

            var metadata = new MmMetadataBlock(
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var queryMessage = MmMessagePool.GetInt(queryId, method, metadata);

            relay.MmInvoke(queryMessage);
            MmMessagePool.Return(queryMessage);
            return queryId;
        }

        /// <summary>
        /// Send a query to descendants and register a callback.
        /// </summary>
        public static int QueryDescendants(this MmRelayNode relay, MmMethod method, Action<MmMessage> onResponse)
        {
            int queryId = _nextQueryId++;
            _pendingQueries[queryId] = onResponse;

            // Use fluent API for proper descendant routing
            new MmFluentMessage(relay, method, queryId).ToDescendants().Execute();
            return queryId;
        }

        /// <summary>
        /// Send a response to a pending query.
        /// </summary>
        public static void Respond(this MmRelayNode relay, int queryId, MmMessage response)
        {
            if (_pendingQueries.TryGetValue(queryId, out var callback))
            {
                _pendingQueries.Remove(queryId);
                callback?.Invoke(response);
            }
        }

        /// <summary>
        /// Respond to a query with an integer value.
        /// </summary>
        public static void Respond(this MmRelayNode relay, int queryId, int value)
        {
            Respond(relay, queryId, new MmMessageInt { MmMethod = MmMethod.MessageInt, value = value });
        }

        /// <summary>
        /// Respond to a query with a float value.
        /// </summary>
        public static void Respond(this MmRelayNode relay, int queryId, float value)
        {
            Respond(relay, queryId, new MmMessageFloat { MmMethod = MmMethod.MessageFloat, value = value });
        }

        /// <summary>
        /// Respond to a query with a string value.
        /// </summary>
        public static void Respond(this MmRelayNode relay, int queryId, string value)
        {
            Respond(relay, queryId, new MmMessageString { MmMethod = MmMethod.MessageString, value = value });
        }

        /// <summary>
        /// Respond to a query with a boolean value.
        /// </summary>
        public static void Respond(this MmRelayNode relay, int queryId, bool value)
        {
            Respond(relay, queryId, new MmMessageBool { MmMethod = MmMethod.MessageBool, value = value });
        }

        /// <summary>
        /// Cancel a pending query.
        /// </summary>
        public static bool CancelQuery(int queryId)
        {
            return _pendingQueries.Remove(queryId);
        }

        /// <summary>
        /// Clear all pending queries.
        /// </summary>
        public static void ClearPendingQueries()
        {
            _pendingQueries.Clear();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Find a relay node by name in the hierarchy (searches descendants).
        /// </summary>
        private static MmRelayNode FindTargetRelay(MmRelayNode source, string targetName)
        {
            if (source == null || string.IsNullOrEmpty(targetName))
                return null;

            // Check self first
            if (source.gameObject.name == targetName)
                return source;

            // Search in routing table (children)
            if (source.RoutingTable != null)
            {
                foreach (var item in source.RoutingTable)
                {
                    if (item.Level != MmLevelFilter.Child)
                        continue;

                    var childRelay = item.Responder?.GetRelayNode();
                    if (childRelay != null)
                    {
                        if (childRelay.gameObject.name == targetName)
                            return childRelay;

                        // Recursive search
                        var found = FindTargetRelay(childRelay, targetName);
                        if (found != null)
                            return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Try to find a relay node by name, returning whether it was found.
        /// </summary>
        public static bool TryFindTarget(this MmRelayNode relay, string targetName, out MmRelayNode target)
        {
            target = FindTargetRelay(relay, targetName);
            return target != null;
        }

        /// <summary>
        /// Check if a target exists in the hierarchy.
        /// </summary>
        public static bool HasTarget(this MmRelayNode relay, string targetName)
        {
            return FindTargetRelay(relay, targetName) != null;
        }

        #endregion
    }
}
