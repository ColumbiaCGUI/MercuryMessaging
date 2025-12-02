// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// DSL Phase 2.1: Listener Pattern Unit Tests
// Tests for message listener registration, filtering, and disposal

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;
using MercuryMessaging.Protocol.DSL;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Unit tests for DSL Phase 2.1: Receiving Listener Pattern.
    /// Tests listener registration, filtering, and disposal.
    /// </summary>
    [TestFixture]
    public class MmListenerTests
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
                    Object.DestroyImmediate(obj);
            }
            testObjects.Clear();
        }

        #region Basic Registration Tests

        /// <summary>
        /// Test 1: Basic listener receives typed messages.
        /// </summary>
        [Test]
        public void Listen_ReceivesTypedMessages()
        {
            // Arrange
            float receivedValue = 0;
            var subscription = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => receivedValue = msg.value)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 42.5f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(42.5f, receivedValue, "Listener should receive float value");
            Assert.AreEqual(1, rootRelay.ListenerCount, "Should have 1 listener");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 2: Listener only receives correct message type.
        /// </summary>
        [Test]
        public void Listen_OnlyReceivesCorrectType()
        {
            // Arrange
            int callCount = 0;
            var subscription = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => callCount++)
                .Execute();

            // Act - send different types
            rootRelay.MmInvoke(MmMethod.MessageInt, 42, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageString, "hello", MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageBool, true, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(0, callCount, "Listener should not receive non-float messages");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 3: Dispose removes listener.
        /// </summary>
        [Test]
        public void Dispose_RemovesListener()
        {
            // Arrange
            int callCount = 0;
            var subscription = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => callCount++)
                .Execute();

            Assert.AreEqual(1, rootRelay.ListenerCount, "Should have 1 listener initially");

            // Act
            subscription.Dispose();

            // Assert
            Assert.AreEqual(0, rootRelay.ListenerCount, "Should have 0 listeners after dispose");
            Assert.IsTrue(subscription.IsDisposed, "Subscription should be marked as disposed");

            // Send message after dispose
            rootRelay.MmInvoke(MmMethod.MessageFloat, 42.5f, MmMetadataBlockHelper.Default);
            Assert.AreEqual(0, callCount, "Disposed listener should not receive messages");
        }

        /// <summary>
        /// Test 4: Multiple listeners receive same message.
        /// </summary>
        [Test]
        public void MultipleListeners_AllReceiveMessage()
        {
            // Arrange
            float value1 = 0, value2 = 0, value3 = 0;

            var sub1 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => value1 = msg.value)
                .Execute();

            var sub2 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => value2 = msg.value * 2)
                .Execute();

            var sub3 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(msg => value3 = msg.value * 3)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 10f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(10f, value1, "Listener 1 should receive value");
            Assert.AreEqual(20f, value2, "Listener 2 should receive value * 2");
            Assert.AreEqual(30f, value3, "Listener 3 should receive value * 3");
            Assert.AreEqual(3, rootRelay.ListenerCount, "Should have 3 listeners");

            // Cleanup
            sub1.Dispose();
            sub2.Dispose();
            sub3.Dispose();
        }

        #endregion

        #region One-Time Listener Tests

        /// <summary>
        /// Test 5: ListenOnce auto-disposes after first message.
        /// </summary>
        [Test]
        public void ListenOnce_AutoDisposesAfterFirstMessage()
        {
            // Arrange
            int callCount = 0;
            var subscription = rootRelay.ListenOnce<MmMessageFloat>()
                .OnReceived(msg => callCount++)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 1f, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageFloat, 2f, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageFloat, 3f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(1, callCount, "One-time listener should only receive one message");
            Assert.IsTrue(subscription.IsDisposed, "One-time listener should be disposed after receiving message");
            Assert.AreEqual(0, rootRelay.ListenerCount, "Should have 0 listeners after auto-dispose");
        }

        /// <summary>
        /// Test 6: .Once() modifier works on Listen().
        /// </summary>
        [Test]
        public void Once_Modifier_Works()
        {
            // Arrange
            int callCount = 0;
            var subscription = rootRelay.Listen<MmMessageInt>()
                .Once()
                .OnReceived(msg => callCount++)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 1, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageInt, 2, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(1, callCount, "Once modifier should make listener one-time");
            Assert.IsTrue(subscription.IsDisposed, "Should be disposed after first message");
        }

        #endregion

        #region Filter Tests

        /// <summary>
        /// Test 7: .When() filter only allows matching messages.
        /// </summary>
        [Test]
        public void When_Filter_OnlyAllowsMatchingMessages()
        {
            // Arrange
            var receivedValues = new List<int>();
            var subscription = rootRelay.Listen<MmMessageInt>()
                .When(msg => msg.value > 50)
                .OnReceived(msg => receivedValues.Add(msg.value))
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 25, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageInt, 75, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageInt, 50, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageInt, 100, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(2, receivedValues.Count, "Should only receive values > 50");
            Assert.Contains(75, receivedValues);
            Assert.Contains(100, receivedValues);

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 8: Multiple .When() filters combine with AND logic.
        /// </summary>
        [Test]
        public void MultipleWhen_Filters_CombineWithAnd()
        {
            // Arrange
            var receivedValues = new List<int>();
            var subscription = rootRelay.Listen<MmMessageInt>()
                .When(msg => msg.value > 20)
                .When(msg => msg.value < 80)
                .When(msg => msg.value % 2 == 0) // Even numbers only
                .OnReceived(msg => receivedValues.Add(msg.value))
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 10, MmMetadataBlockHelper.Default); // Fails > 20
            rootRelay.MmInvoke(MmMethod.MessageInt, 30, MmMetadataBlockHelper.Default); // Passes all
            rootRelay.MmInvoke(MmMethod.MessageInt, 35, MmMetadataBlockHelper.Default); // Fails even
            rootRelay.MmInvoke(MmMethod.MessageInt, 90, MmMetadataBlockHelper.Default); // Fails < 80
            rootRelay.MmInvoke(MmMethod.MessageInt, 50, MmMetadataBlockHelper.Default); // Passes all

            // Assert
            Assert.AreEqual(2, receivedValues.Count, "Should only receive even values between 20 and 80");
            Assert.Contains(30, receivedValues);
            Assert.Contains(50, receivedValues);

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 9: .WithTag() filter only allows matching tag.
        /// </summary>
        [Test]
        public void WithTag_Filter_OnlyAllowsMatchingTag()
        {
            // Arrange
            int callCount = 0;
            var subscription = rootRelay.Listen<MmMessageFloat>()
                .WithTag(MmTag.Tag0)
                .OnReceived(msg => callCount++)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 1f,
                new MmMetadataBlock(MmTag.Tag1, MmLevelFilterHelper.SelfAndChildren));
            rootRelay.MmInvoke(MmMethod.MessageFloat, 2f,
                new MmMetadataBlock(MmTag.Tag0, MmLevelFilterHelper.SelfAndChildren));
            rootRelay.MmInvoke(MmMethod.MessageFloat, 3f,
                new MmMetadataBlock(MmTag.Tag2, MmLevelFilterHelper.SelfAndChildren));

            // Assert
            Assert.AreEqual(1, callCount, "Should only receive message with Tag0");

            // Cleanup
            subscription.Dispose();
        }

        #endregion

        #region Convenience Method Tests

        /// <summary>
        /// Test 10: OnFloat convenience method works.
        /// </summary>
        [Test]
        public void OnFloat_ConvenienceMethod_Works()
        {
            // Arrange
            float receivedValue = 0;
            var subscription = rootRelay.OnFloat(value => receivedValue = value);

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 3.14f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(3.14f, receivedValue, "OnFloat should receive float value");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 11: OnInt convenience method works.
        /// </summary>
        [Test]
        public void OnInt_ConvenienceMethod_Works()
        {
            // Arrange
            int receivedValue = 0;
            var subscription = rootRelay.OnInt(value => receivedValue = value);

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 42, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(42, receivedValue, "OnInt should receive int value");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 12: OnString convenience method works.
        /// </summary>
        [Test]
        public void OnString_ConvenienceMethod_Works()
        {
            // Arrange
            string receivedValue = "";
            var subscription = rootRelay.OnString(value => receivedValue = value);

            // Act
            rootRelay.MmInvoke(MmMethod.MessageString, "hello world", MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual("hello world", receivedValue, "OnString should receive string value");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 13: OnBool convenience method works.
        /// </summary>
        [Test]
        public void OnBool_ConvenienceMethod_Works()
        {
            // Arrange
            bool receivedValue = false;
            var subscription = rootRelay.OnBool(value => receivedValue = value);

            // Act
            rootRelay.MmInvoke(MmMethod.MessageBool, true, MmMetadataBlockHelper.Default);

            // Assert
            Assert.IsTrue(receivedValue, "OnBool should receive bool value");

            // Cleanup
            subscription.Dispose();
        }

        #endregion

        #region Method-Based Listener Tests

        /// <summary>
        /// Test 14: OnInitialize convenience method works.
        /// </summary>
        [Test]
        public void OnInitialize_ConvenienceMethod_Works()
        {
            // Arrange
            bool initCalled = false;
            var subscription = rootRelay.OnInitialize(() => initCalled = true);

            // Act
            rootRelay.MmInvoke(MmMethod.Initialize, MmMetadataBlockHelper.Default);

            // Assert
            Assert.IsTrue(initCalled, "OnInitialize should be called");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 15: OnSetActive convenience method works.
        /// </summary>
        [Test]
        public void OnSetActive_ConvenienceMethod_Works()
        {
            // Arrange
            bool receivedActive = false;
            var subscription = rootRelay.OnSetActive(value => receivedActive = value);

            // Act
            rootRelay.MmInvoke(MmMethod.SetActive, true, MmMetadataBlockHelper.Default);

            // Assert
            Assert.IsTrue(receivedActive, "OnSetActive should receive true");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 16: OnSwitch convenience method works.
        /// </summary>
        [Test]
        public void OnSwitch_ConvenienceMethod_Works()
        {
            // Arrange
            string receivedState = "";
            var subscription = rootRelay.OnSwitch(state => receivedState = state);

            // Act
            rootRelay.MmInvoke(MmMethod.Switch, "MainMenu", MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual("MainMenu", receivedState, "OnSwitch should receive state name");

            // Cleanup
            subscription.Dispose();
        }

        #endregion

        #region Cleanup Tests

        /// <summary>
        /// Test 17: ClearListeners removes all listeners.
        /// </summary>
        [Test]
        public void ClearListeners_RemovesAllListeners()
        {
            // Arrange
            var sub1 = rootRelay.Listen<MmMessageFloat>().OnReceived(_ => { }).Execute();
            var sub2 = rootRelay.Listen<MmMessageInt>().OnReceived(_ => { }).Execute();
            var sub3 = rootRelay.Listen<MmMessageString>().OnReceived(_ => { }).Execute();

            Assert.AreEqual(3, rootRelay.ListenerCount, "Should have 3 listeners");

            // Act
            rootRelay.ClearAllListeners();

            // Assert
            Assert.AreEqual(0, rootRelay.ListenerCount, "Should have 0 listeners after clear");
            Assert.IsTrue(sub1.IsDisposed, "Sub1 should be disposed");
            Assert.IsTrue(sub2.IsDisposed, "Sub2 should be disposed");
            Assert.IsTrue(sub3.IsDisposed, "Sub3 should be disposed");
        }

        /// <summary>
        /// Test 18: Double dispose is safe.
        /// </summary>
        [Test]
        public void DoubleDispose_IsSafe()
        {
            // Arrange
            var subscription = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(_ => { })
                .Execute();

            // Act & Assert - should not throw
            subscription.Dispose();
            subscription.Dispose();
            subscription.Dispose();

            Assert.IsTrue(subscription.IsDisposed, "Should remain disposed");
        }

        /// <summary>
        /// Test 19: Handler exception does not break other listeners.
        /// </summary>
        [Test]
        public void HandlerException_DoesNotBreakOtherListeners()
        {
            // Arrange
            int callCount = 0;

            var sub1 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(_ => callCount++)
                .Execute();

            var sub2 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(_ => throw new System.Exception("Test exception"))
                .Execute();

            var sub3 = rootRelay.Listen<MmMessageFloat>()
                .OnReceived(_ => callCount++)
                .Execute();

            // Expect the error log from the exception handler
            LogAssert.Expect(LogType.Error, new Regex(@"\[MmListener\] Exception in handler:.*Test exception"));

            // Act - should not throw despite sub2 exception
            Assert.DoesNotThrow(() =>
                rootRelay.MmInvoke(MmMethod.MessageFloat, 1f, MmMetadataBlockHelper.Default));

            // Assert - sub1 and sub3 should still have been called
            Assert.AreEqual(2, callCount, "Non-throwing handlers should still be called");

            // Cleanup
            sub1.Dispose();
            sub2.Dispose();
            sub3.Dispose();
        }

        #endregion

        #region Responder Extension Tests

        /// <summary>
        /// Test 20: Responder.Listen() receives messages same as relay.
        /// </summary>
        [Test]
        public void Responder_Listen_ReceivesMessages()
        {
            // Arrange - Add a test responder to the same GameObject
            var responder = testRoot.AddComponent<MmBaseResponder>();

            float receivedValue = 0;
            var subscription = responder.Listen<MmMessageFloat>()
                .OnReceived(msg => receivedValue = msg.value)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 42.5f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(42.5f, receivedValue, "Responder listener should receive float value");
            Assert.AreEqual(1, responder.GetListenerCount(), "Should have 1 listener via responder");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 21: Responder.ListenOnce() auto-disposes after first message.
        /// </summary>
        [Test]
        public void Responder_ListenOnce_AutoDisposes()
        {
            // Arrange
            var responder = testRoot.AddComponent<MmBaseResponder>();

            int callCount = 0;
            var subscription = responder.ListenOnce<MmMessageInt>()
                .OnReceived(msg => callCount++)
                .Execute();

            // Act
            rootRelay.MmInvoke(MmMethod.MessageInt, 1, MmMetadataBlockHelper.Default);
            rootRelay.MmInvoke(MmMethod.MessageInt, 2, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(1, callCount, "One-time responder listener should only fire once");
            Assert.IsTrue(subscription.IsDisposed, "Should be auto-disposed");
        }

        /// <summary>
        /// Test 22: Responder.OnFloat() convenience method works.
        /// </summary>
        [Test]
        public void Responder_OnFloat_ConvenienceMethod_Works()
        {
            // Arrange
            var responder = testRoot.AddComponent<MmBaseResponder>();

            float receivedValue = 0;
            var subscription = responder.OnFloat(value => receivedValue = value);

            // Act
            rootRelay.MmInvoke(MmMethod.MessageFloat, 3.14f, MmMetadataBlockHelper.Default);

            // Assert
            Assert.AreEqual(3.14f, receivedValue, "Responder.OnFloat should receive value");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 23: Responder.OnInitialize() convenience method works.
        /// </summary>
        [Test]
        public void Responder_OnInitialize_ConvenienceMethod_Works()
        {
            // Arrange
            var responder = testRoot.AddComponent<MmBaseResponder>();

            bool initCalled = false;
            var subscription = responder.OnInitialize(() => initCalled = true);

            // Act
            rootRelay.MmInvoke(MmMethod.Initialize, MmMetadataBlockHelper.Default);

            // Assert
            Assert.IsTrue(initCalled, "Responder.OnInitialize should be called");

            // Cleanup
            subscription.Dispose();
        }

        /// <summary>
        /// Test 24: Responder without relay node returns null/default safely.
        /// </summary>
        [Test]
        public void Responder_NoRelayNode_ReturnsDefaultSafely()
        {
            // Arrange - Create responder on object WITHOUT relay node
            var orphanGO = new GameObject("OrphanResponder");
            testObjects.Add(orphanGO);
            var orphanResponder = orphanGO.AddComponent<MmBaseResponder>();
            // Note: No MmRelayNode added!

            // Act & Assert - Should not throw, returns default/null
            var builder = orphanResponder.Listen<MmMessageFloat>();

            // The default struct has null relay, so Execute() will log an error and return null
            // We expect the error to be logged
            LogAssert.Expect(LogType.Error, "[MmListener] Cannot create listener without a relay node");
            var subscription = builder.OnReceived(_ => { }).Execute();
            Assert.IsNull(subscription, "Listener on responder without relay should return null");

            // Convenience methods return null silently (no error log) via null-conditional
            var floatSub = orphanResponder.OnFloat(_ => { });
            Assert.IsNull(floatSub, "OnFloat on orphan responder should return null");

            // GetListenerCount should return 0
            Assert.AreEqual(0, orphanResponder.GetListenerCount(), "Orphan responder should have 0 listeners");
        }

        /// <summary>
        /// Test 25: Responder.GetListenerCount() returns correct count.
        /// </summary>
        [Test]
        public void Responder_GetListenerCount_ReturnsCorrectCount()
        {
            // Arrange
            var responder = testRoot.AddComponent<MmBaseResponder>();

            // Act - Add multiple listeners via responder
            var sub1 = responder.OnFloat(_ => { });
            var sub2 = responder.OnInt(_ => { });
            var sub3 = responder.OnString(_ => { });

            // Assert
            Assert.AreEqual(3, responder.GetListenerCount(), "Should have 3 listeners");

            // Dispose one
            sub2.Dispose();
            Assert.AreEqual(2, responder.GetListenerCount(), "Should have 2 listeners after dispose");

            // Cleanup
            sub1.Dispose();
            sub3.Dispose();
        }

        /// <summary>
        /// Test 26: Responder.ClearAllListeners() removes all listeners.
        /// </summary>
        [Test]
        public void Responder_ClearAllListeners_RemovesAll()
        {
            // Arrange
            var responder = testRoot.AddComponent<MmBaseResponder>();

            var sub1 = responder.OnFloat(_ => { });
            var sub2 = responder.OnInt(_ => { });

            Assert.AreEqual(2, responder.GetListenerCount(), "Should have 2 listeners initially");

            // Act
            responder.ClearAllListeners();

            // Assert
            Assert.AreEqual(0, responder.GetListenerCount(), "Should have 0 listeners after clear");
            Assert.IsTrue(sub1.IsDisposed, "Sub1 should be disposed");
            Assert.IsTrue(sub2.IsDisposed, "Sub2 should be disposed");
        }

        #endregion
    }
}
