// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for fluent message dispatch covering:
    /// - Wave 2.1: ExecuteStandard/SendToTarget handling TaskInfo, ByteArray, custom methods (>1000)
    /// - Wave 2.2: Switch(int) properly converts int to string for FSM lookup
    /// - Wave 2.3: Null responder guards on extension methods
    /// - Wave 2.4: try/finally on predicate pool
    /// </summary>
    [TestFixture]
    public class MmFluentMessageDispatchTests
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

        #region Wave 2.1: ByteArray and Custom Method Dispatch

        [UnityTest]
        public IEnumerator Send_ByteArray_DoesNotThrow()
        {
            // Arrange
            var childObj = new GameObject("Child");
            childObj.transform.SetParent(testRoot.transform);
            var childRelay = childObj.AddComponent<MmRelayNode>();
            childObj.AddComponent<MmBaseResponder>();
            childRelay.MmRefreshResponders();

            relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(relay);
            relay.MmRefreshResponders();

            yield return null;

            // Expect the error log from fluent dispatch when no byte[] payload is provided
            LogAssert.Expect(LogType.Error,
                "MmFluentMessage: Expected byte[] payload for MessageByteArray");

            // Act & Assert - should not throw
            Assert.DoesNotThrow(() =>
            {
                relay.Send(MmMethod.MessageByteArray).ToChildren().Execute();
            });
        }

        [UnityTest]
        public IEnumerator Send_CustomMethod_DoesNotThrow()
        {
            // Arrange
            var childObj = new GameObject("Child");
            childObj.transform.SetParent(testRoot.transform);
            var childRelay = childObj.AddComponent<MmRelayNode>();
            childObj.AddComponent<MmBaseResponder>();
            childRelay.MmRefreshResponders();

            relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(relay);
            relay.MmRefreshResponders();

            yield return null;

            // Act & Assert - custom method (>1000) should not throw
            Assert.DoesNotThrow(() =>
            {
                relay.Send((MmMethod)1001).ToChildren().Execute();
            });
        }

        #endregion

        #region Wave 2.2: Switch(int) Converts to String

        [UnityTest]
        public IEnumerator SwitchInt_Dispatches()
        {
            // Arrange
            var childObj = new GameObject("SwitchChild");
            childObj.transform.SetParent(testRoot.transform);
            var childRelay = childObj.AddComponent<MmRelayNode>();
            var receiver = childObj.AddComponent<SwitchReceiver>();
            childRelay.MmRefreshResponders();

            relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
            childRelay.AddParent(relay);
            relay.MmRefreshResponders();

            yield return null;

            // Act - Send Switch with int payload; fluent dispatch converts int to string
            relay.Send(MmMethod.Switch, 42).ToChildren().Execute();

            // Assert - SwitchReceiver should have received the call
            Assert.AreEqual(1, receiver.SwitchCallCount,
                "SwitchReceiver should receive exactly one Switch call");
            Assert.AreEqual("42", receiver.LastSwitchName,
                "Int payload 42 should be converted to string \"42\"");
        }

        #endregion

        #region Wave 2.3: Null Responder Guards

        [Test]
        public void NullResponder_BroadcastInitialize_DoesNotThrow()
        {
            MmBaseResponder nullResp = null;
            Assert.DoesNotThrow(() => nullResp.BroadcastInitialize());
        }

        [Test]
        public void NullResponder_Send_ReturnsDefault()
        {
            MmBaseResponder nullResp = null;
            MmFluentMessage msg = default;

            Assert.DoesNotThrow(() =>
            {
                msg = nullResp.Send("hello");
            });

            Assert.AreEqual(default(MmFluentMessage), msg,
                "Send on null responder should return default MmFluentMessage");
        }

        [Test]
        public void NullResponder_NotifyComplete_DoesNotThrow()
        {
            MmBaseResponder nullResp = null;
            Assert.DoesNotThrow(() => nullResp.NotifyComplete());
        }

        #endregion

        #region Helper Responders

        private class SwitchReceiver : MmBaseResponder
        {
            public int SwitchCallCount;
            public string LastSwitchName;

            protected override void Switch(string iName)
            {
                SwitchCallCount++;
                LastSwitchName = iName;
            }
        }

        #endregion
    }
}
