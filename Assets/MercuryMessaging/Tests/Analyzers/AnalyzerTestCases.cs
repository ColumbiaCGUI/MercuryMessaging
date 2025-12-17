// Copyright (c) 2017-2025, Columbia University
// Test cases for MercuryMessaging Roslyn Analyzers
// This file intentionally contains code that should trigger analyzer warnings/errors
//
// To test: Open Unity Editor and check the Console/Error List for:
// - MM001: Info suggesting DSL for verbose MmInvoke
// - MM002: Warning on Self-only level filter
// - MM003: Warning on network context without network filter
// - MM004: Info suggesting Broadcast convenience methods
// - MM005: Warning on missing Execute()
// - MM006: Warning on missing MmRefreshResponders
// - MM007: Warning on potential infinite loop
// - MM008: Warning on SetParent without routing table
// - MM009: Warning on missing base.MmInvoke call
// - MM010: Error on non-partial class with [MmGenerateDispatch]
// - MM011: Info suggesting MmExtendableResponder
// - MM012: Warning on Tag without TagCheckEnabled
// - MM013: Info on responder without relay node reference
// - MM014: Warning on misspelled handler method
// - MM015: Warning on bitwise filter equality check
//
// After verifying the analyzers work, you can delete this file or comment out the test cases.
// #define MM_ANALYZER_TEST
#if UNITY_EDITOR && MM_ANALYZER_TEST
// Uncomment the #define above to enable these test cases

using UnityEngine;

namespace MercuryMessaging.Tests.Analyzers
{
    // =========================================================================
    // MM001: Suggest fluent DSL for verbose MmInvoke
    // =========================================================================
    public class MM001TestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM001: Verbose MmInvoke with MmMetadataBlock
        void TestVerboseMmInvoke()
        {
            // MM001 info expected - suggests using fluent DSL
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
        }

        // Should NOT trigger MM001: Simple MmInvoke without metadata
        void TestSimpleMmInvoke()
        {
            relay.MmInvoke(MmMethod.Initialize);  // No suggestion - already simple
        }
    }

    // =========================================================================
    // MM002: Self-only level filter warning
    // =========================================================================
    public class MM002TestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM002: Self-only filter
        void TestSelfOnlyFilter()
        {
            // MM002 warning expected
            relay.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(MmLevelFilter.Self));
        }

        // Should NOT trigger MM002: SelfAndChildren
        void TestSelfAndChildren()
        {
            relay.MmInvoke(MmMethod.Initialize, new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren));
        }
    }

    // =========================================================================
    // MM003: Network message without OverNetwork filter
    // =========================================================================
    public class MM003NetworkTestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM003: Network context but Local filter
        void SyncPlayerPosition()
        {
            // MM003 warning expected - method name suggests network, but filter is Local
            relay.MmInvoke(MmMethod.MessageVector3, Vector3.zero,
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren, MmActiveFilter.Active,
                    MmSelectedFilter.All, MmNetworkFilter.Local));
        }
    }

    // =========================================================================
    // MM004: Suggest Broadcast convenience methods
    // =========================================================================
    public class MM004TestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM004: Pattern can be simplified
        void TestVerboseInitialize()
        {
            // MM004 info expected - can use BroadcastInitialize()
            relay.MmInvoke(MmMethod.Initialize,
                new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren));
        }
    }

    // =========================================================================
    // MM005: Missing Execute() analyzer
    // =========================================================================
    public class MM005TestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM005: MmFluentMessage without Execute()
        void TestMissingExecute_FluentMessage()
        {
            relay.Send("Hello").ToChildren();  // MM005 warning expected here
        }

        // SHOULD trigger MM005: Chain with filters but no Execute()
        void TestMissingExecute_WithFilters()
        {
            relay.Send(42).ToDescendants().Active().WithTag(MmTag.Tag0);  // MM005 warning expected
        }

        // Should NOT trigger MM005: Proper Execute() call
        void TestProperExecute()
        {
            relay.Send("Hello").ToChildren().Execute();  // No warning - correct usage
        }

        // Should NOT trigger MM005: Auto-execute API
        void TestAutoExecute()
        {
            relay.To.Children.Send("Hello");  // No warning - auto-executes
        }
    }

    // =========================================================================
    // MM006: Missing MmRefreshResponders after AddComponent
    // =========================================================================
    public class MM006TestCases : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM006: AddComponent without MmRefreshResponders
        void TestMissingRefresh()
        {
            var go = new GameObject();
            var responder = go.AddComponent<MmBaseResponder>();  // MM006 warning expected
        }

        // Should NOT trigger MM006: Proper refresh call
        void TestWithRefresh()
        {
            var go = new GameObject();
            var responder = go.AddComponent<MmBaseResponder>();
            relay.MmRefreshResponders();  // No warning - refresh is called
        }
    }

    // =========================================================================
    // MM007: Potential infinite loop
    // =========================================================================
    public class MM007TestCases : MmBaseResponder
    {
        // SHOULD trigger MM007: Sending same message type in handler
        protected override void ReceivedMessage(MmMessageString message)
        {
            var relay = GetComponent<MmRelayNode>();
            relay.Send("echo").Execute();  // MM007 warning expected - infinite loop risk
        }
    }

    // =========================================================================
    // MM008: SetParent without routing table registration
    // =========================================================================
    public class MM008TestCases : MmBaseResponder
    {
        private MmRelayNode relay;

        public override void Start()
        {
            base.Start();
            relay = GetComponent<MmRelayNode>();
        }

        // SHOULD trigger MM008: SetParent without routing table update
        void TestSetParentNoRouting()
        {
            var child = new GameObject().transform;
            child.SetParent(transform);  // MM008 warning expected
        }

        // Should NOT trigger MM008: SetParent with routing table update
        void TestSetParentWithRouting()
        {
            var child = new GameObject().transform;
            child.SetParent(transform);
            var childRelay = child.GetComponent<MmRelayNode>();
            relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
        }
    }

    // =========================================================================
    // MM009: Missing base.MmInvoke call
    // =========================================================================
    public class MM009TestCases : MmBaseResponder
    {
        // SHOULD trigger MM009: Override without base call
        public override void MmInvoke(MmMessage message)
        {
            // MM009 warning expected - no base.MmInvoke(message)
            Debug.Log("Custom handling");
        }
    }

    public class MM009CorrectTestCases : MmBaseResponder
    {
        // Should NOT trigger MM009: Override with base call
        public override void MmInvoke(MmMessage message)
        {
            if (message.MmMethod == (MmMethod)1000)
            {
                Debug.Log("Custom handling");
            }
            base.MmInvoke(message);  // Correct - base is called
        }
    }

    // =========================================================================
    // MM010: Non-partial class with [MmGenerateDispatch]
    // NOTE: This test case is commented out because the source generator
    // automatically creates a partial class, causing a CS0260 error.
    // The MM010 analyzer SHOULD report an error before the generator runs,
    // but in Unity the generator runs first. Uncomment to test in IDE.
    // =========================================================================
    // [MmGenerateDispatch]
    // public class MM010TestCase_BadExample : MmBaseResponder  // MM010 error expected here
    // {
    //     protected override void ReceivedMessage(MmMessageString msg)
    //     {
    //         Debug.Log($"Received: {msg.value}");
    //     }
    // }

    /// Correct usage - partial class with [MmGenerateDispatch]
    /// Should NOT trigger MM010
    [MmGenerateDispatch]
    public partial class MM010TestCase_GoodExample : MmBaseResponder  // No error - correct usage
    {
        protected override void ReceivedMessage(MmMessageInt msg)
        {
            Debug.Log($"Received: {msg.value}");
        }
    }

    // =========================================================================
    // MM011: Suggest MmExtendableResponder for many custom handlers
    // =========================================================================
    public class MM011TestCases : MmBaseResponder
    {
        // SHOULD trigger MM011: Many custom handlers in switch
        public override void MmInvoke(MmMessage message)
        {
            switch (message.MmMethod)
            {
                case (MmMethod)1000:
                    Debug.Log("Handler 1");
                    break;
                case (MmMethod)1001:
                    Debug.Log("Handler 2");
                    break;
                case (MmMethod)1002:
                    Debug.Log("Handler 3");
                    break;
                case (MmMethod)1003:
                    Debug.Log("Handler 4");
                    break;
                case (MmMethod)1004:
                    Debug.Log("Handler 5");
                    break;
            }
            base.MmInvoke(message);
        }
    }

    // =========================================================================
    // MM012: Tag check without TagCheckEnabled
    // =========================================================================
    public class MM012TestCases : MmBaseResponder
    {
        public override void Start()
        {
            base.Start();
            Tag = MmTag.Tag0;  // MM012 warning expected - TagCheckEnabled not set
        }
    }

    public class MM012CorrectTestCases : MmBaseResponder
    {
        public override void Start()
        {
            base.Start();
            Tag = MmTag.Tag0;
            TagCheckEnabled = true;  // No warning - TagCheckEnabled is set
        }
    }

    // =========================================================================
    // MM013: Responder without relay node reference
    // =========================================================================
    public class MM013TestCases : MmBaseResponder
    {
        // SHOULD trigger MM013: No relay node reference but sends messages
        void SendSomething()
        {
            // MM013 info expected - no GetComponent<MmRelayNode>()
            // This would actually fail at runtime
        }
    }

    // =========================================================================
    // MM014: Misspelled handler method
    // =========================================================================
    public class MM014TestCases : MmBaseResponder
    {
        // SHOULD trigger MM014: Misspelled method name
        protected void RecievedInitialize()  // MM014 warning expected - typo "ie" instead of "ei"
        {
            Debug.Log("Init");
        }

        // SHOULD trigger MM014: Wrong prefix
        protected void OnReceivedMessage()  // MM014 warning expected - should be ReceivedMessage
        {
            Debug.Log("Message");
        }
    }

    // =========================================================================
    // MM015: Bitwise filter equality check
    // =========================================================================
    public class MM015TestCases : MmBaseResponder
    {
        void CheckTag()
        {
            var tag = MmTag.Tag0 | MmTag.Tag1;

            // SHOULD trigger MM015: Using == instead of bitwise AND
            if (tag == MmTag.Tag0)  // MM015 warning expected
            {
                Debug.Log("This won't work as expected");
            }

            // Correct usage - no warning
            if ((tag & MmTag.Tag0) != 0)
            {
                Debug.Log("Correct bitwise check");
            }
        }
    }

    // =========================================================================
    // MMG001-MMG003: Source Generator Diagnostics for [MmHandler]
    // =========================================================================

    // CORRECT: Valid [MmHandler] usage with [MmGenerateDispatch]
    [MmGenerateDispatch]
    public partial class MMG_ValidHandlerTestCases : MmBaseResponder
    {
        // Valid: methodId >= 1000
        [MmHandler(1000)]
        private void OnCustomColor(MmMessage msg)
        {
            Debug.Log("Color handler");
        }

        // Valid: another custom handler
        [MmHandler(1001, Name = "ScaleHandler")]
        private void OnCustomScale(MmMessage msg)
        {
            Debug.Log("Scale handler");
        }
    }

    // SHOULD trigger MMG001: Invalid method ID (< 1000)
    [MmGenerateDispatch]
    public partial class MMG001_InvalidIdTestCases : MmBaseResponder
    {
        [MmHandler(500)]  // MMG001 error expected - ID must be >= 1000
        private void OnInvalidMethod(MmMessage msg)
        {
            Debug.Log("Invalid ID");
        }
    }

    // SHOULD trigger MMG002: Duplicate method ID
    [MmGenerateDispatch]
    public partial class MMG002_DuplicateIdTestCases : MmBaseResponder
    {
        [MmHandler(1000)]
        private void OnFirstHandler(MmMessage msg)
        {
            Debug.Log("First");
        }

        [MmHandler(1000)]  // MMG002 error expected - duplicate ID
        private void OnSecondHandler(MmMessage msg)
        {
            Debug.Log("Second");
        }
    }

    // SHOULD trigger MMG003: Invalid handler signature
    [MmGenerateDispatch]
    public partial class MMG003_InvalidSignatureTestCases : MmBaseResponder
    {
        [MmHandler(1000)]  // MMG003 error expected - wrong parameter type
        private void OnBadHandler(string msg)
        {
            Debug.Log("Bad");
        }

        [MmHandler(1001)]  // MMG003 error expected - no parameter
        private void OnNoParamHandler()
        {
            Debug.Log("No param");
        }
    }
}

#endif
