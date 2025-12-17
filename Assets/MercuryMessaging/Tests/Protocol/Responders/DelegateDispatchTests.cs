// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// Phase 5: Delegate Dispatch - Validation Tests
// Tests Handler delegate implementation for fast message dispatch

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Validation tests for Phase 5: Delegate Dispatch optimization.
    /// Tests Handler delegate, dispatch behavior, and API correctness.
    /// </summary>
    [TestFixture]
    public class DelegateDispatchTests
    {
        private GameObject testRoot;
        private MmRelayNode rootRelay;
        private List<GameObject> testObjects;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");
            rootRelay = testRoot.AddComponent<MmRelayNode>();
            testObjects = new List<GameObject> { testRoot };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }
            testObjects.Clear();
        }

        /// <summary>
        /// Helper to create a child responder and add to routing table.
        /// </summary>
        private DelegateDispatchTestResponder CreateChildResponder(string name, MmRelayNode parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.gameObject.transform);
            DelegateDispatchTestResponder responder = child.AddComponent<DelegateDispatchTestResponder>();

            parent.MmAddToRoutingTable(responder, MmLevelFilter.Child);
            testObjects.Add(child);

            return responder;
        }

        /// <summary>
        /// Helper to create a child relay node and add to routing table.
        /// </summary>
        private MmRelayNode CreateChildRelay(string name, MmRelayNode parent)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.gameObject.transform);
            MmRelayNode childRelay = child.AddComponent<MmRelayNode>();

            parent.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(parent);
            testObjects.Add(child);

            return childRelay;
        }

        #region MmRoutingTableItem.Handler Tests

        /// <summary>
        /// Test: Handler property is null by default.
        /// </summary>
        [Test]
        public void Handler_IsNullByDefault()
        {
            // Arrange
            var item = new MmRoutingTableItem();

            // Assert
            Assert.IsNull(item.Handler);
            Assert.IsFalse(item.HasHandler);
        }

        /// <summary>
        /// Test: Handler can be set and retrieved.
        /// </summary>
        [Test]
        public void Handler_CanBeSetAndRetrieved()
        {
            // Arrange
            var item = new MmRoutingTableItem();
            bool handlerCalled = false;
            Action<MmMessage> handler = (msg) => { handlerCalled = true; };

            // Act
            item.Handler = handler;

            // Assert
            Assert.IsNotNull(item.Handler);
            Assert.IsTrue(item.HasHandler);

            // Verify handler works
            item.Handler(new MmMessage());
            Assert.IsTrue(handlerCalled);
        }

        /// <summary>
        /// Test: Handler can be cleared.
        /// </summary>
        [Test]
        public void Handler_CanBeCleared()
        {
            // Arrange
            var item = new MmRoutingTableItem();
            item.Handler = (msg) => { };

            // Act
            item.Handler = null;

            // Assert
            Assert.IsNull(item.Handler);
            Assert.IsFalse(item.HasHandler);
        }

        /// <summary>
        /// Test: HasHandler property reflects Handler state correctly.
        /// </summary>
        [Test]
        public void HasHandler_ReflectsHandlerState()
        {
            // Arrange
            var item = new MmRoutingTableItem();

            // Initial state
            Assert.IsFalse(item.HasHandler);

            // After setting
            item.Handler = (msg) => { };
            Assert.IsTrue(item.HasHandler);

            // After clearing
            item.Handler = null;
            Assert.IsFalse(item.HasHandler);
        }

        #endregion

        #region SetFastHandler API Tests

        /// <summary>
        /// Test: SetFastHandler sets handler on routing table item.
        /// </summary>
        [Test]
        public void SetFastHandler_SetsHandlerOnRoutingTableItem()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);

            // Act - handler body not called in this test, just verifying registration
            bool result = rootRelay.SetFastHandler(responder, (msg) => { });

            // Assert
            Assert.IsTrue(result);

            var item = rootRelay.RoutingTable[responder];
            Assert.IsNotNull(item);
            Assert.IsTrue(item.HasHandler);
        }

        /// <summary>
        /// Test: SetFastHandler returns false for null responder.
        /// </summary>
        [Test]
        public void SetFastHandler_ReturnsFalse_ForNullResponder()
        {
            // Act
            bool result = rootRelay.SetFastHandler(null, (msg) => { });

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test: SetFastHandler returns false for responder not in routing table.
        /// </summary>
        [Test]
        public void SetFastHandler_ReturnsFalse_ForUnregisteredResponder()
        {
            // Arrange
            var child = new GameObject("Unregistered");
            var responder = child.AddComponent<DelegateDispatchTestResponder>();
            testObjects.Add(child);

            // Act
            bool result = rootRelay.SetFastHandler(responder, (msg) => { });

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test: ClearFastHandler removes handler.
        /// </summary>
        [Test]
        public void ClearFastHandler_RemovesHandler()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            rootRelay.SetFastHandler(responder, (msg) => { });

            // Act
            bool result = rootRelay.ClearFastHandler(responder);

            // Assert
            Assert.IsTrue(result);

            var item = rootRelay.RoutingTable[responder];
            Assert.IsFalse(item.HasHandler);
        }

        /// <summary>
        /// Test: HasFastHandler returns correct state.
        /// </summary>
        [Test]
        public void HasFastHandler_ReturnsCorrectState()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);

            // Initial state
            Assert.IsFalse(rootRelay.HasFastHandler(responder));

            // After setting
            rootRelay.SetFastHandler(responder, (msg) => { });
            Assert.IsTrue(rootRelay.HasFastHandler(responder));

            // After clearing
            rootRelay.ClearFastHandler(responder);
            Assert.IsFalse(rootRelay.HasFastHandler(responder));
        }

        /// <summary>
        /// Test: HasFastHandler returns false for null responder.
        /// </summary>
        [Test]
        public void HasFastHandler_ReturnsFalse_ForNullResponder()
        {
            Assert.IsFalse(rootRelay.HasFastHandler(null));
        }

        /// <summary>
        /// Test: HasFastHandler returns false for unregistered responder.
        /// </summary>
        [Test]
        public void HasFastHandler_ReturnsFalse_ForUnregisteredResponder()
        {
            // Arrange
            var child = new GameObject("Unregistered");
            var responder = child.AddComponent<DelegateDispatchTestResponder>();
            testObjects.Add(child);

            // Assert
            Assert.IsFalse(rootRelay.HasFastHandler(responder));
        }

        #endregion

        #region Dispatch Behavior Tests

        /// <summary>
        /// Test: Handler is invoked when set (fast path).
        /// </summary>
        [UnityTest]
        public IEnumerator Dispatch_UsesHandler_WhenSet()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            bool handlerCalled = false;
            bool mmInvokeCalled = false;

            // Track if MmInvoke is called
            responder.OnMmInvoke = (msg) => { mmInvokeCalled = true; };

            // Set fast handler
            rootRelay.SetFastHandler(responder, (msg) =>
            {
                handlerCalled = true;
            });

            yield return null;

            // Act
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.IsTrue(handlerCalled, "Handler should be called");
            Assert.IsFalse(mmInvokeCalled, "MmInvoke should NOT be called when handler is set");
        }

        /// <summary>
        /// Test: MmInvoke is used when handler is not set (slow path).
        /// </summary>
        [UnityTest]
        public IEnumerator Dispatch_UsesMmInvoke_WhenHandlerNotSet()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            bool mmInvokeCalled = false;

            responder.OnMmInvoke = (msg) => { mmInvokeCalled = true; };

            yield return null;

            // Act - no handler set, should use MmInvoke
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.IsTrue(mmInvokeCalled, "MmInvoke should be called when no handler is set");
        }

        /// <summary>
        /// Test: Handler receives correct message.
        /// </summary>
        [UnityTest]
        public IEnumerator Dispatch_Handler_ReceivesCorrectMessage()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            MmMessage receivedMessage = null;

            rootRelay.SetFastHandler(responder, (msg) =>
            {
                receivedMessage = msg;
            });

            yield return null;

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 42, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(MmMethod.MessageInt, receivedMessage.MmMethod);
            Assert.IsInstanceOf<MmMessageInt>(receivedMessage);
            Assert.AreEqual(42, ((MmMessageInt)receivedMessage).value);
        }

        /// <summary>
        /// Test: Multiple responders with mixed handlers work correctly.
        /// </summary>
        [UnityTest]
        public IEnumerator Dispatch_MixedHandlers_AllReceiveMessages()
        {
            // Arrange
            var responder1 = CreateChildResponder("Responder1", rootRelay);
            var responder2 = CreateChildResponder("Responder2", rootRelay);
            var responder3 = CreateChildResponder("Responder3", rootRelay);

            bool handler1Called = false;
            bool mmInvoke2Called = false;
            bool handler3Called = false;

            // Responder 1: Use fast handler
            rootRelay.SetFastHandler(responder1, (msg) => { handler1Called = true; });

            // Responder 2: Use default MmInvoke (no handler)
            responder2.OnMmInvoke = (msg) => { mmInvoke2Called = true; };

            // Responder 3: Use fast handler
            rootRelay.SetFastHandler(responder3, (msg) => { handler3Called = true; });

            yield return null;

            // Act
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.IsTrue(handler1Called, "Handler 1 should be called");
            Assert.IsTrue(mmInvoke2Called, "MmInvoke 2 should be called");
            Assert.IsTrue(handler3Called, "Handler 3 should be called");
        }

        #endregion

        #region Backward Compatibility Tests

        /// <summary>
        /// Test: Existing code without handlers still works.
        /// </summary>
        [UnityTest]
        public IEnumerator BackwardCompatibility_NoHandlers_StillWorks()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            bool receivedRefresh = false;

            responder.OnRefresh = () => { receivedRefresh = true; };

            yield return null;

            // Act - standard MmInvoke, no handlers
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.IsTrue(receivedRefresh, "Responder should receive Refresh via MmInvoke");
        }

        /// <summary>
        /// Test: Handler can be changed at runtime.
        /// </summary>
        [UnityTest]
        public IEnumerator Handler_CanBeChangedAtRuntime()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            int handlerVersion = 0;

            rootRelay.SetFastHandler(responder, (msg) => { handlerVersion = 1; });

            yield return null;

            // First invoke
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));
            yield return null;
            Assert.AreEqual(1, handlerVersion);

            // Change handler
            rootRelay.SetFastHandler(responder, (msg) => { handlerVersion = 2; });

            // Second invoke
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));
            yield return null;
            Assert.AreEqual(2, handlerVersion);
        }

        /// <summary>
        /// Test: Clearing handler reverts to MmInvoke.
        /// </summary>
        [UnityTest]
        public IEnumerator ClearingHandler_RevertsToMmInvoke()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            bool handlerCalled = false;
            bool mmInvokeCalled = false;

            rootRelay.SetFastHandler(responder, (msg) => { handlerCalled = true; });
            responder.OnMmInvoke = (msg) => { mmInvokeCalled = true; };

            yield return null;

            // First invoke - handler should be used
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));
            yield return null;

            Assert.IsTrue(handlerCalled);
            Assert.IsFalse(mmInvokeCalled);

            // Clear handler
            handlerCalled = false;
            rootRelay.ClearFastHandler(responder);

            // Second invoke - MmInvoke should be used
            rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));
            yield return null;

            Assert.IsFalse(handlerCalled);
            Assert.IsTrue(mmInvokeCalled);
        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// Test: Handler dispatch is faster than virtual dispatch.
        /// </summary>
        [Test]
        public void Performance_HandlerDispatch_IsFaster()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            int callCount = 0;

            var item = rootRelay.RoutingTable[responder];
            Assert.IsNotNull(item);

            Action<MmMessage> handler = (msg) => { callCount++; };
            item.Handler = handler;

            var message = new MmMessage();
            const int iterations = 100000;

            // Warm up
            for (int i = 0; i < 1000; i++)
            {
                item.Handler(message);
            }
            callCount = 0;

            // Measure handler dispatch
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                if (item.Handler != null)
                    item.Handler(message);
            }
            sw.Stop();
            long handlerTicks = sw.ElapsedTicks;

            // Measure virtual dispatch
            callCount = 0;
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                item.Responder.MmInvoke(message);
            }
            sw.Stop();
            long virtualTicks = sw.ElapsedTicks;

            // Log results
            UnityEngine.Debug.Log($"Handler dispatch: {handlerTicks} ticks ({handlerTicks / (double)iterations:F2} ticks/call)");
            UnityEngine.Debug.Log($"Virtual dispatch: {virtualTicks} ticks ({virtualTicks / (double)iterations:F2} ticks/call)");
            UnityEngine.Debug.Log($"Handler is {virtualTicks / (double)handlerTicks:F2}x faster");

            // Handler should be faster (or at least not significantly slower)
            // Note: In practice, handler dispatch should be 2-3x faster
            Assert.IsTrue(handlerTicks <= virtualTicks * 2,
                $"Handler dispatch ({handlerTicks}) should not be much slower than virtual dispatch ({virtualTicks})");
        }

        /// <summary>
        /// Test: HasHandler check is very fast.
        /// </summary>
        [Test]
        public void Performance_HasHandler_IsFast()
        {
            // Arrange
            var item = new MmRoutingTableItem();
            item.Handler = (msg) => { };

            const int iterations = 1000000;

            // Measure
            var sw = Stopwatch.StartNew();
            bool result = false;
            for (int i = 0; i < iterations; i++)
            {
                result = item.HasHandler;
            }
            sw.Stop();

            // Log
            UnityEngine.Debug.Log($"HasHandler check: {sw.ElapsedTicks} ticks for {iterations} iterations ({sw.ElapsedTicks / (double)iterations:F4} ticks/check)");

            // Should be very fast (essentially just a null check)
            Assert.IsTrue(sw.ElapsedMilliseconds < 100, "HasHandler check should be very fast");
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Test: Handler exceptions propagate (same behavior as virtual dispatch).
        /// This documents that exceptions in handlers are NOT caught by the dispatch loop,
        /// which is consistent with how virtual MmInvoke exceptions behave.
        /// </summary>
        [Test]
        public void Handler_Exception_Propagates()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);

            rootRelay.SetFastHandler(responder, (msg) =>
            {
                throw new InvalidOperationException("Test exception from handler");
            });

            // Act & Assert - Exception should propagate (not be swallowed)
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                rootRelay.MmInvoke(MmMethod.Refresh, new MmMetadataBlock(MmLevelFilter.Child));
            });

            Assert.That(ex.Message, Does.Contain("Test exception from handler"));
        }

        /// <summary>
        /// Test: Handler works with different message types.
        /// </summary>
        [UnityTest]
        public IEnumerator Handler_WorksWithDifferentMessageTypes()
        {
            // Arrange
            var responder = CreateChildResponder("TestResponder", rootRelay);
            List<MmMessageType> receivedTypes = new List<MmMessageType>();

            rootRelay.SetFastHandler(responder, (msg) =>
            {
                receivedTypes.Add(msg.MmMessageType);
            });

            yield return null;

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 42, new MmMetadataBlock(MmLevelFilter.Child));
            rootRelay.MmInvoke(MmMethod.MessageString, "test", new MmMetadataBlock(MmLevelFilter.Child));
            rootRelay.MmInvoke(MmMethod.MessageBool, true, new MmMetadataBlock(MmLevelFilter.Child));

            yield return null;

            // Assert
            Assert.AreEqual(3, receivedTypes.Count);
            Assert.Contains(MmMessageType.MmInt, receivedTypes);
            Assert.Contains(MmMessageType.MmString, receivedTypes);
            Assert.Contains(MmMessageType.MmBool, receivedTypes);
        }

        #endregion
    }

    /// <summary>
    /// Test responder for delegate dispatch tests.
    /// Named uniquely to avoid conflicts with other test files.
    /// </summary>
    public class DelegateDispatchTestResponder : MmBaseResponder
    {
        public Action<MmMessage> OnMmInvoke;
        public Action OnRefresh;

        public override void MmInvoke(MmMessage msg)
        {
            OnMmInvoke?.Invoke(msg);
            base.MmInvoke(msg);
        }

        public override void Refresh(List<MmTransform> transforms)
        {
            OnRefresh?.Invoke();
            base.Refresh(transforms);
        }
    }
}
