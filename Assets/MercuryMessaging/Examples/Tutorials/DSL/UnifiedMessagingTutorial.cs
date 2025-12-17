// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// UnifiedMessagingTutorial.cs - Tutorial for Unified Messaging API
// Part of DSL Overhaul Phase 11
//
// This tutorial demonstrates the Two-Tier Unified Messaging API:
// - Tier 1: Auto-execute methods (BroadcastInitialize, NotifyComplete, etc.)
// - Tier 2: Fluent chains (Send().ToDescendants().Execute())

using UnityEngine;
using MercuryMessaging;


namespace MercuryMessaging.Examples.Tutorials.DSL
{
    /// <summary>
    /// Tutorial: Unified Messaging API
    ///
    /// The Unified API provides IDENTICAL methods for both MmRelayNode and MmBaseResponder,
    /// making it easy to send messages from any context.
    ///
    /// Key Concepts:
    /// - Broadcast* = Send DOWN to descendants (matches MmMethod enum names)
    /// - Notify* = Send UP to parents (matches MmMethod enum names)
    /// - Both tiers work on both MmRelayNode AND MmBaseResponder!
    /// </summary>
    public class UnifiedMessagingTutorial : MmBaseResponder
    {
        private MmRelayNode relay;

        public override void Awake()
        {
            base.Awake();
            relay = GetComponent<MmRelayNode>();
        }

        new void Start()
        {
            // Run tutorial examples
            Tier1Examples();
            Tier2Examples();
            ResponderExamples();
        }

        #region Tier 1: Auto-Execute Methods

        /// <summary>
        /// Tier 1 methods execute immediately - no .Execute() needed!
        /// These are the simplest and most common messaging patterns.
        /// </summary>
        void Tier1Examples()
        {
            Debug.Log("=== TIER 1: AUTO-EXECUTE METHODS ===");

            // =========================================
            // BROADCAST (Down to descendants)
            // =========================================

            // Initialize all children and self
            relay.BroadcastInitialize();
            // Equivalent to: relay.Send(MmMethod.Initialize).Execute();

            // Refresh all children and self
            relay.BroadcastRefresh();
            // Equivalent to: relay.Send(MmMethod.Refresh).Execute();

            // Set active state on all children
            relay.BroadcastSetActive(true);
            // Equivalent to: relay.Send(MmMethod.SetActive, true).Execute();

            // Switch FSM state
            relay.BroadcastSwitch("MainMenu");
            // Equivalent to: relay.Send(MmMethod.Switch, "MainMenu").Execute();

            // Send typed values (bool, int, float, string)
            relay.BroadcastValue(true);      // MmMethod.MessageBool
            relay.BroadcastValue(42);        // MmMethod.MessageInt
            relay.BroadcastValue(3.14f);     // MmMethod.MessageFloat
            relay.BroadcastValue("Hello");   // MmMethod.MessageString

            // =========================================
            // NOTIFY (Up to parents/ancestors)
            // =========================================

            // Notify completion to parents
            relay.NotifyComplete();
            // Equivalent to: relay.Send(MmMethod.Complete, true).ToParents().Execute();

            // Send values to parents
            relay.NotifyValue(true);         // Send bool up
            relay.NotifyValue(100);          // Send int up (e.g., score)
            relay.NotifyValue(0.75f);        // Send float up (e.g., progress)
            relay.NotifyValue("Status OK");  // Send string up

            Debug.Log("Tier 1 examples complete - messages sent to hierarchy");
        }

        #endregion

        #region Tier 2: Fluent Chain Methods

        /// <summary>
        /// Tier 2 provides full control with chainable methods.
        /// Always ends with .Execute() to send the message.
        /// </summary>
        void Tier2Examples()
        {
            Debug.Log("=== TIER 2: FLUENT CHAIN METHODS ===");

            // =========================================
            // DIRECTION TARGETING
            // =========================================

            // Direct children only (not grandchildren)
            relay.Send("Hello").ToChildren().Execute();

            // Direct parents only (not grandparents)
            relay.Send("Hello").ToParents().Execute();

            // All descendants recursively
            relay.Send("Hello").ToDescendants().Execute();

            // All ancestors recursively
            relay.Send("Hello").ToAncestors().Execute();

            // Same-level nodes (siblings)
            relay.Send("Hello").ToSiblings().Execute();

            // Bidirectional (parents + children)
            relay.Send("Hello").ToAll().Execute();

            // =========================================
            // FILTER COMBINATIONS
            // =========================================

            // Only active GameObjects
            relay.Send("Hello").ToDescendants().Active().Execute();

            // Include inactive GameObjects
            relay.Send("Hello").ToDescendants().IncludeInactive().Execute();

            // With tag filtering
            relay.Send("Hello").ToDescendants().WithTag(MmTag.Tag0).Execute();

            // Multiple tags (OR logic)
            relay.Send("Hello").ToDescendants().WithTags(MmTag.Tag0, MmTag.Tag1).Execute();

            // Network messages
            relay.Send("Hello").ToDescendants().AllDestinations().Execute();  // Local + Network
            relay.Send("Hello").ToDescendants().NetworkOnly().Execute();      // Network only
            relay.Send("Hello").ToDescendants().LocalOnly().Execute();        // Local only

            // =========================================
            // COMPLEX COMBINATIONS
            // =========================================

            // Full example: UI update to active tagged descendants
            relay.Send("Score: 100")
                .ToDescendants()
                .Active()
                .WithTag(MmTag.Tag0)  // UI tag
                .LocalOnly()
                .Execute();

            Debug.Log("Tier 2 examples complete - messages sent with full control");
        }

        #endregion

        #region Responder Examples

        /// <summary>
        /// The Unified API works identically on responders!
        /// This is a major improvement - no need to get relay node first.
        /// </summary>
        void ResponderExamples()
        {
            Debug.Log("=== RESPONDER API (Same as Relay!) ===");

            // Tier 1 from responder (this)
            this.BroadcastInitialize();  // Works!
            this.BroadcastRefresh();     // Works!
            this.BroadcastValue(42);     // Works!
            this.NotifyComplete();       // Works!
            this.NotifyValue("Done");    // Works!

            // Tier 2 from responder
            this.Send("Hello").ToDescendants().Execute();
            this.Send(MmMethod.Initialize).ToDescendants().WithTag(MmTag.Tag0).Execute();
            this.Send(42).ToParents().Execute();

            // Null-safe: If responder has no relay node, methods do nothing
            // No exceptions thrown - safe to use anywhere

            Debug.Log("Responder examples complete - same API works everywhere!");
        }

        #endregion

        #region Message Handlers

        // These handlers receive the messages sent above

        protected override void ReceivedMessage(MmMessageString message)
        {
            Debug.Log($"[{gameObject.name}] Received string: {message.value}");
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            Debug.Log($"[{gameObject.name}] Received int: {message.value}");
        }

        protected override void ReceivedMessage(MmMessageFloat message)
        {
            Debug.Log($"[{gameObject.name}] Received float: {message.value}");
        }

        protected override void ReceivedMessage(MmMessageBool message)
        {
            Debug.Log($"[{gameObject.name}] Received bool: {message.value}");
        }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"[{gameObject.name}] Initialized!");
        }

        public override void Refresh(System.Collections.Generic.List<MmTransform> transformList)
        {
            base.Refresh(transformList);
            Debug.Log($"[{gameObject.name}] Refreshed!");
        }

        protected override void Complete(bool active)
        {
            Debug.Log($"[{gameObject.name}] Completed!");
        }

        #endregion
    }

    /// <summary>
    /// Example showing how to migrate from traditional API to Unified API.
    /// </summary>
    public class MigrationExample : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();

            // ========================================
            // BEFORE (Traditional API - 7 lines)
            // ========================================
            /*
            relay.MmInvoke(
                MmMethod.MessageString,
                "Hello",
                new MmMetadataBlock(
                    MmLevelFilter.Child,
                    MmActiveFilter.Active,
                    MmSelectedFilter.All,
                    MmNetworkFilter.Local
                )
            );
            */

            // ========================================
            // AFTER (Unified API - 1 line!)
            // ========================================
            relay.Send("Hello").ToChildren().Active().Execute();

            // ========================================
            // SIMPLEST CASES (Tier 1)
            // ========================================

            // Before: relay.MmInvoke(MmMethod.Initialize);
            // After:
            relay.BroadcastInitialize();

            // Before: relay.MmInvoke(MmMethod.Complete, true,
            //         new MmMetadataBlock(MmLevelFilter.Parent));
            // After:
            relay.NotifyComplete();
        }
    }
}
