// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - Diagnostic Descriptors
// DSL/DX Phase 3: Roslyn Analyzers

using Microsoft.CodeAnalysis;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Contains all diagnostic descriptors for MercuryMessaging analyzers.
    /// </summary>
    public static class DiagnosticDescriptors
    {
        private const string Category = "MercuryMessaging";

        /// <summary>
        /// MM001: Verbose MmInvoke call could use fluent DSL
        /// </summary>
        public static readonly DiagnosticDescriptor MM001_SuggestFluentDsl = new DiagnosticDescriptor(
            id: "MM001",
            title: "Consider using fluent DSL",
            messageFormat: "This MmInvoke call could be simplified using the fluent DSL: {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The fluent DSL provides a more concise and readable syntax for sending messages. " +
                         "For example: relay.Send(\"Hello\").ToChildren().Execute().");

        /// <summary>
        /// MM005: Missing .Execute() on fluent message builder
        /// </summary>
        public static readonly DiagnosticDescriptor MM005_MissingExecute = new DiagnosticDescriptor(
            id: "MM005",
            title: "Missing .Execute() call",
            messageFormat: "MmFluentMessage chain does not call Execute() - message will not be sent",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "MmFluentMessage and MmDeferredRoutingBuilder are struct-based builders that " +
                         "construct message parameters but don't send until Execute() is called. " +
                         "Without Execute(), the message is constructed but never delivered.");

        /// <summary>
        /// MM010: [MmGenerateDispatch] on non-partial class
        /// </summary>
        public static readonly DiagnosticDescriptor MM010_NonPartialClass = new DiagnosticDescriptor(
            id: "MM010",
            title: "[MmGenerateDispatch] requires partial class",
            messageFormat: "Class '{0}' has [MmGenerateDispatch] but is not declared partial",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The [MmGenerateDispatch] attribute generates optimized dispatch code in a " +
                         "separate partial class file. The class must be declared 'partial' to allow " +
                         "the generated code to be merged.");

        // ============================================================================
        // Additional analyzers based on common MercuryMessaging mistakes
        // ============================================================================

        /// <summary>
        /// MM002: Self-only level filter - messages won't propagate to children
        /// </summary>
        public static readonly DiagnosticDescriptor MM002_SelfOnlyFilter = new DiagnosticDescriptor(
            id: "MM002",
            title: "Self-only level filter may be unintentional",
            messageFormat: "MmLevelFilter.Self only targets the current node, consider using SelfAndChildren",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Using MmLevelFilter.Self alone means only the originating node receives the message. " +
                         "Child responders will not receive it. Use SelfAndChildren for typical broadcast patterns.");

        /// <summary>
        /// MM003: Network message without OverNetwork() filter
        /// </summary>
        public static readonly DiagnosticDescriptor MM003_NetworkWithoutFilter = new DiagnosticDescriptor(
            id: "MM003",
            title: "Network message may not propagate correctly",
            messageFormat: "Message sent over network but MmNetworkFilter is set to Local - use .OverNetwork() or NetworkOnly()",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When sending messages intended for network propagation, ensure the MmNetworkFilter " +
                         "is set to All or Network. Local-only messages won't be transmitted to other clients.");

        /// <summary>
        /// MM004: Suggest using Broadcast convenience methods
        /// </summary>
        public static readonly DiagnosticDescriptor MM004_SuggestBroadcast = new DiagnosticDescriptor(
            id: "MM004",
            title: "Consider using Broadcast convenience method",
            messageFormat: "This pattern can be simplified to {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "MercuryMessaging provides convenience methods like BroadcastInitialize(), " +
                         "BroadcastRefresh(), NotifyComplete() that are more concise than equivalent fluent chains.");

        /// <summary>
        /// MM006: Missing MmRefreshResponders after AddComponent
        /// </summary>
        public static readonly DiagnosticDescriptor MM006_MissingRefresh = new DiagnosticDescriptor(
            id: "MM006",
            title: "Missing MmRefreshResponders() after adding responder",
            messageFormat: "AddComponent<{0}>() adds a responder but MmRefreshResponders() is not called - the responder won't receive messages",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When adding MmBaseResponder components at runtime, you must call MmRefreshResponders() " +
                         "on the relay node to register the new responder in the routing table.");

        /// <summary>
        /// MM007: Potential infinite loop - sending to Self in message handler
        /// </summary>
        public static readonly DiagnosticDescriptor MM007_PotentialInfiniteLoop = new DiagnosticDescriptor(
            id: "MM007",
            title: "Potential infinite message loop",
            messageFormat: "Sending {0} to Self from within a {0} handler may cause infinite recursion",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Sending a message of the same type to Self from within that message's handler " +
                         "can cause infinite recursion. Consider using a different message type or filter.");

        /// <summary>
        /// MM008: SetParent without routing table registration
        /// </summary>
        public static readonly DiagnosticDescriptor MM008_SetParentWithoutRouting = new DiagnosticDescriptor(
            id: "MM008",
            title: "SetParent without routing table update",
            messageFormat: "transform.SetParent() changes Unity hierarchy but not MercuryMessaging routing - call MmAddToRoutingTable() and AddParent()",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Unity's SetParent() only updates the Transform hierarchy. MercuryMessaging requires " +
                         "explicit routing table registration via MmAddToRoutingTable() and AddParent() for " +
                         "messages to route correctly through the new hierarchy.");

        /// <summary>
        /// MM009: Missing base.MmInvoke call in override
        /// </summary>
        public static readonly DiagnosticDescriptor MM009_MissingBaseCall = new DiagnosticDescriptor(
            id: "MM009",
            title: "Override may need base.MmInvoke() call",
            messageFormat: "MmInvoke override doesn't call base.MmInvoke() - standard message handlers won't be invoked",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When overriding MmInvoke(), you typically need to call base.MmInvoke(message) to " +
                         "ensure standard message handlers (ReceivedMessage, ReceivedInitialize, etc.) are invoked.");

        /// <summary>
        /// MM011: Suggest MmExtendableResponder for many custom handlers
        /// </summary>
        public static readonly DiagnosticDescriptor MM011_SuggestExtendable = new DiagnosticDescriptor(
            id: "MM011",
            title: "Consider using MmExtendableResponder",
            messageFormat: "Class has {0} custom message handlers in MmInvoke switch - consider MmExtendableResponder for cleaner code",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "When handling many custom message types (>3), MmExtendableResponder provides a cleaner " +
                         "pattern using RegisterCustomHandler() instead of large switch statements.");

        /// <summary>
        /// MM012: Tag check without TagCheckEnabled
        /// </summary>
        public static readonly DiagnosticDescriptor MM012_TagWithoutEnabled = new DiagnosticDescriptor(
            id: "MM012",
            title: "Tag assignment without TagCheckEnabled",
            messageFormat: "MmTag is assigned but TagCheckEnabled may be false - tag filtering won't work",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Setting the Tag property on a responder has no effect unless TagCheckEnabled is true. " +
                         "Ensure TagCheckEnabled = true for tag filtering to work.");

        /// <summary>
        /// MM013: Responder without relay node on same GameObject
        /// </summary>
        public static readonly DiagnosticDescriptor MM013_ResponderWithoutRelay = new DiagnosticDescriptor(
            id: "MM013",
            title: "Responder may need MmRelayNode",
            messageFormat: "MmBaseResponder on GameObject without MmRelayNode - ensure parent has relay node or add one here",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "MmBaseResponder components need an MmRelayNode somewhere in their hierarchy to receive " +
                         "messages. The relay node can be on the same GameObject or a parent.");

        /// <summary>
        /// MM014: Misspelled ReceivedInitialize method name
        /// </summary>
        public static readonly DiagnosticDescriptor MM014_MisspelledHandler = new DiagnosticDescriptor(
            id: "MM014",
            title: "Possible misspelled message handler",
            messageFormat: "Method '{0}' looks like a message handler but doesn't match any known pattern - did you mean '{1}'?",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Message handler methods must match exact signatures. Common typos like 'RecievedInitialize' " +
                         "or 'OnReceivedMessage' won't be called by the framework.");

        /// <summary>
        /// MM015: Bitwise filter equality check instead of AND
        /// </summary>
        public static readonly DiagnosticDescriptor MM015_BitwiseEquality = new DiagnosticDescriptor(
            id: "MM015",
            title: "Incorrect filter comparison",
            messageFormat: "Filter comparison uses == instead of bitwise AND - use (filter & value) != 0 for flag checks",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "MmTag, MmLevelFilter, and other flags use bitwise operations. Checking equality " +
                         "(tag == MmTag.Tag0) only matches exact values. Use (tag & MmTag.Tag0) != 0 to check if a flag is set.");
    }
}
