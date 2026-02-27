// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Wave 1.1: MmRelayNode.MmInvoke() try/finally safety
//   Validates that _invokeDepth is properly decremented even when a responder throws,
//   so the routing table does not remain permanently locked.
//
// Wave 1.2: MmBaseResponder graceful handling of unhandled custom methods
//   Validates that custom methods (>1000) no longer throw ArgumentOutOfRangeException
//   but instead log a warning.

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Safety tests for MmRelayNode invoke depth tracking and MmBaseResponder
    /// custom method handling. Validates that exceptions in responders do not
    /// corrupt routing table state and that unhandled custom methods are handled
    /// gracefully.
    /// </summary>
    [TestFixture]
    public class MmRelayNodeSafetyTests
    {
        private GameObject testRoot;
        private MmRelayNode relay;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");
            relay = testRoot.AddComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
                Object.DestroyImmediate(testRoot);
        }

        #region Wave 1.1 Tests - try/finally invoke depth safety

        /// <summary>
        /// Wave 1.1: If a responder throws during MmInvoke, the routing table must
        /// not remain locked. Verifies that doNotModifyRoutingTable returns to false
        /// and subsequent messages still route correctly.
        /// </summary>
        [UnityTest]
        public IEnumerator ExceptionInResponder_DoesNotCorruptRoutingTable()
        {
            // Arrange - create a child that throws and another that counts
            var throwChild = new GameObject("ThrowingChild");
            throwChild.transform.SetParent(testRoot.transform);
            throwChild.AddComponent<ThrowingResponder>();

            var countChild = new GameObject("CountingChild");
            countChild.transform.SetParent(testRoot.transform);
            var counter = countChild.AddComponent<CountingResponder>();

            relay.MmRefreshResponders();
            yield return null;

            // Act - send Initialize; ThrowingResponder throws, but routing should continue
            relay.MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                    new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren))
            );

            // Assert - routing table must be unlocked after the call
            Assert.IsFalse(relay.doNotModifyRoutingTable,
                "doNotModifyRoutingTable should be false after MmInvoke completes (even with exception)");

            // Act again - send a second Initialize to verify routing is not permanently corrupted
            relay.MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                    new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren))
            );

            // Assert - CountingResponder should have received at least one message across both calls
            Assert.Greater(counter.InitCount, 0,
                "CountingResponder should still receive messages after a sibling threw an exception");

            Debug.Log($"[Wave 1.1 PASS] Routing table unlocked after exception. " +
                      $"CountingResponder received {counter.InitCount} Initialize messages.");
        }

        /// <summary>
        /// Wave 1.1: Nested (reentrant) MmInvoke calls must correctly unwind the
        /// _invokeDepth counter so doNotModifyRoutingTable returns to false.
        /// </summary>
        [UnityTest]
        public IEnumerator RoutingTableUnlockedAfterNestedInvoke()
        {
            // Arrange - create a child that re-invokes on the same relay during handling
            var reentrantChild = new GameObject("ReentrantChild");
            reentrantChild.transform.SetParent(testRoot.transform);
            var reentrant = reentrantChild.AddComponent<ReentrantResponder>();
            reentrant.RelayToCall = relay;

            relay.MmRefreshResponders();
            yield return null;

            // Act - send Initialize, which triggers a nested MmInvoke(Refresh) inside the handler
            relay.MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                    new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren))
            );

            // Assert - after all invocations unwind, the routing table must be unlocked
            Assert.IsFalse(relay.doNotModifyRoutingTable,
                "doNotModifyRoutingTable should be false after nested MmInvoke fully unwinds");

            Debug.Log("[Wave 1.1 PASS] Routing table unlocked after nested (reentrant) invoke.");
        }

        #endregion

        #region Wave 1.2 Tests - Custom method graceful handling

        /// <summary>
        /// Wave 1.2: Sending a custom method (>1000) to a plain MmBaseResponder must not
        /// throw an ArgumentOutOfRangeException. The responder should log a warning instead.
        /// </summary>
        [UnityTest]
        public IEnumerator CustomMethodAbove1000_DoesNotThrow()
        {
            // Arrange - plain MmBaseResponder with no custom handler registered
            var child = new GameObject("PlainResponder");
            child.transform.SetParent(testRoot.transform);
            child.AddComponent<MmBaseResponder>();

            relay.MmRefreshResponders();
            yield return null;

            // Act & Assert - sending a custom method must not throw
            Assert.DoesNotThrow(() =>
            {
                relay.MmInvoke(
                    new MmMessage((MmMethod)1001, MmMessageType.MmVoid,
                        new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren))
                );
            }, "Sending custom method (MmMethod)1001 should not throw an exception");

            Debug.Log("[Wave 1.2 PASS] Custom method >1000 handled gracefully without exception.");
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Responder that throws an exception during ReceivedInitialize.
        /// Used to verify that routing table state is not corrupted by exceptions.
        /// </summary>
        private class ThrowingResponder : MmBaseResponder
        {
            protected override void ReceivedInitialize()
            {
                throw new System.Exception("Test exception from ThrowingResponder");
            }
        }

        /// <summary>
        /// Responder that counts how many Initialize messages it receives.
        /// Used to verify routing continues to function after exceptions.
        /// </summary>
        private class CountingResponder : MmBaseResponder
        {
            public int InitCount;

            protected override void ReceivedInitialize()
            {
                InitCount++;
            }
        }

        /// <summary>
        /// Responder that performs a reentrant (nested) MmInvoke during message handling.
        /// Used to verify that _invokeDepth correctly supports reentrancy.
        /// </summary>
        private class ReentrantResponder : MmBaseResponder
        {
            public MmRelayNode RelayToCall;

            protected override void ReceivedInitialize()
            {
                if (RelayToCall != null)
                {
                    RelayToCall.MmInvoke(
                        new MmMessage(MmMethod.Refresh, MmMessageType.MmVoid,
                            new MmMetadataBlock(MmLevelFilter.Self))
                    );
                }
            }
        }

        #endregion
    }
}
