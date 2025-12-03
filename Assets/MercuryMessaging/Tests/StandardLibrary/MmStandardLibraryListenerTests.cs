// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
// Tests for DSL Phase 2.4: Standard Library Listener Extensions

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MercuryMessaging;
using MercuryMessaging.StandardLibrary;
using MercuryMessaging.StandardLibrary.UI;
using MercuryMessaging.StandardLibrary.Input;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Unit tests for MmStandardLibraryListenerExtensions.
    /// Tests convenience methods for UI and Input message subscriptions.
    /// </summary>
    [TestFixture]
    public class MmStandardLibraryListenerTests
    {
        private GameObject _testGo;
        private MmRelayNode _relay;

        [SetUp]
        public void SetUp()
        {
            _testGo = new GameObject("TestObject");
            _relay = _testGo.AddComponent<MmRelayNode>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testGo != null) Object.DestroyImmediate(_testGo);
        }

        // ============================================================================
        // UI MESSAGE TESTS
        // ============================================================================

        #region OnClick Tests

        [UnityTest]
        public IEnumerator OnClick_ReceivesClickMessage()
        {
            yield return null;

            MmUIClickMessage received = null;
            var sub = _relay.OnClick(msg => received = msg);
            yield return null;

            // Send a click message
            var clickMsg = new MmUIClickMessage(new Vector2(100, 200), clickCount: 1, button: 0);
            _relay.MmInvoke(clickMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.AreEqual(new Vector2(100, 200), received.Position);
            Assert.AreEqual(1, received.ClickCount);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnClick_ReceivesDoubleClick()
        {
            yield return null;

            bool wasDoubleClick = false;
            var sub = _relay.OnClick(msg => wasDoubleClick = msg.IsDoubleClick);
            yield return null;

            var doubleClickMsg = new MmUIClickMessage(Vector2.zero, clickCount: 2);
            _relay.MmInvoke(doubleClickMsg);
            yield return null;

            Assert.IsTrue(wasDoubleClick);
            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnClick_Position_ReceivesPosition()
        {
            yield return null;

            Vector2 receivedPos = Vector2.zero;
            var sub = _relay.OnClick((Vector2 pos) => receivedPos = pos);
            yield return null;

            var clickMsg = new MmUIClickMessage(new Vector2(50, 75));
            _relay.MmInvoke(clickMsg);
            yield return null;

            Assert.AreEqual(new Vector2(50, 75), receivedPos);
            sub.Dispose();
        }

        #endregion

        #region OnHover Tests

        [UnityTest]
        public IEnumerator OnHover_ReceivesHoverEnter()
        {
            yield return null;

            bool receivedEnter = false;
            var sub = _relay.OnHover(msg => receivedEnter = msg.IsEnter);
            yield return null;

            var hoverMsg = new MmUIHoverMessage(Vector2.zero, isEnter: true);
            _relay.MmInvoke(hoverMsg);
            yield return null;

            Assert.IsTrue(receivedEnter);
            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnHover_Bool_ReceivesEnterState()
        {
            yield return null;

            bool? receivedState = null;
            var sub = _relay.OnHover((bool isEnter) => receivedState = isEnter);
            yield return null;

            var hoverMsg = new MmUIHoverMessage(Vector2.zero, isEnter: false);
            _relay.MmInvoke(hoverMsg);
            yield return null;

            Assert.AreEqual(false, receivedState);
            sub.Dispose();
        }

        #endregion

        #region OnDrag Tests

        [UnityTest]
        public IEnumerator OnDrag_ReceivesDragMessage()
        {
            yield return null;

            MmUIDragMessage received = null;
            var sub = _relay.OnDrag(msg => received = msg);
            yield return null;

            var dragMsg = new MmUIDragMessage(new Vector2(10, 20), new Vector2(5, -5), MmDragPhase.Move);
            _relay.MmInvoke(dragMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.AreEqual(MmDragPhase.Move, received.Phase);
            Assert.AreEqual(new Vector2(5, -5), received.Delta);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnDrag_Delta_ReceivesDelta()
        {
            yield return null;

            Vector2 receivedDelta = Vector2.zero;
            var sub = _relay.OnDrag((Vector2 delta) => receivedDelta = delta);
            yield return null;

            var dragMsg = new MmUIDragMessage(Vector2.zero, new Vector2(10, 15), MmDragPhase.Move);
            _relay.MmInvoke(dragMsg);
            yield return null;

            Assert.AreEqual(new Vector2(10, 15), receivedDelta);
            sub.Dispose();
        }

        #endregion

        #region OnSelect Tests

        [UnityTest]
        public IEnumerator OnSelect_ReceivesSelection()
        {
            yield return null;

            int receivedIndex = -1;
            var sub = _relay.OnSelect((int index) => receivedIndex = index);
            yield return null;

            var selectMsg = new MmUISelectMessage(selectedIndex: 3, selectedValue: "Option3");
            _relay.MmInvoke(selectMsg);
            yield return null;

            Assert.AreEqual(3, receivedIndex);
            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnSelect_String_ReceivesValue()
        {
            yield return null;

            string receivedValue = null;
            var sub = _relay.OnSelect((string value) => receivedValue = value);
            yield return null;

            var selectMsg = new MmUISelectMessage(selectedIndex: 1, selectedValue: "TestOption");
            _relay.MmInvoke(selectMsg);
            yield return null;

            Assert.AreEqual("TestOption", receivedValue);
            sub.Dispose();
        }

        #endregion

        // ============================================================================
        // INPUT MESSAGE TESTS
        // ============================================================================

        #region On6DOF Tests

        [UnityTest]
        public IEnumerator On6DOF_ReceivesTrackingData()
        {
            yield return null;

            MmInput6DOFMessage received = null;
            var sub = _relay.On6DOF(msg => received = msg);
            yield return null;

            var pos = new Vector3(1, 2, 3);
            var rot = Quaternion.Euler(10, 20, 30);
            var sixDofMsg = new MmInput6DOFMessage(MmHandedness.Right, pos, rot);
            _relay.MmInvoke(sixDofMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.AreEqual(MmHandedness.Right, received.Hand);
            Assert.AreEqual(pos, received.Position);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator On6DOF_FilterByHand_OnlyReceivesMatchingHand()
        {
            yield return null;

            int receivedCount = 0;
            var sub = _relay.On6DOF(MmHandedness.Left, msg => receivedCount++);
            yield return null;

            // Send right hand message - should not trigger
            var rightMsg = new MmInput6DOFMessage(MmHandedness.Right, Vector3.zero, Quaternion.identity);
            _relay.MmInvoke(rightMsg);
            yield return null;

            Assert.AreEqual(0, receivedCount);

            // Send left hand message - should trigger
            var leftMsg = new MmInput6DOFMessage(MmHandedness.Left, Vector3.zero, Quaternion.identity);
            _relay.MmInvoke(leftMsg);
            yield return null;

            Assert.AreEqual(1, receivedCount);

            sub.Dispose();
        }

        #endregion

        #region OnGesture Tests

        [UnityTest]
        public IEnumerator OnGesture_ReceivesGestureMessage()
        {
            yield return null;

            MmInputGestureMessage received = null;
            var sub = _relay.OnGesture(msg => received = msg);
            yield return null;

            var gestureMsg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Pinch, confidence: 0.95f);
            _relay.MmInvoke(gestureMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.AreEqual(MmGestureType.Pinch, received.GestureType);
            Assert.AreEqual(0.95f, received.Confidence, 0.001f);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnGesture_FilterByType_OnlyReceivesMatchingType()
        {
            yield return null;

            int pinchCount = 0;
            var sub = _relay.OnGesture(MmGestureType.Pinch, msg => pinchCount++);
            yield return null;

            // Send fist gesture - should not trigger
            var fistMsg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Fist);
            _relay.MmInvoke(fistMsg);
            yield return null;

            Assert.AreEqual(0, pinchCount);

            // Send pinch gesture - should trigger
            var pinchMsg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Pinch);
            _relay.MmInvoke(pinchMsg);
            yield return null;

            Assert.AreEqual(1, pinchCount);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnGesture_FilterByConfidence_OnlyReceivesHighConfidence()
        {
            yield return null;

            int receivedCount = 0;
            var sub = _relay.OnGesture(MmGestureType.Pinch, 0.8f, msg => receivedCount++);
            yield return null;

            // Send low confidence - should not trigger
            var lowConfMsg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Pinch, confidence: 0.5f);
            _relay.MmInvoke(lowConfMsg);
            yield return null;

            Assert.AreEqual(0, receivedCount);

            // Send high confidence - should trigger
            var highConfMsg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Pinch, confidence: 0.9f);
            _relay.MmInvoke(highConfMsg);
            yield return null;

            Assert.AreEqual(1, receivedCount);

            sub.Dispose();
        }

        #endregion

        #region OnButton Tests

        [UnityTest]
        public IEnumerator OnButton_ReceivesButtonMessage()
        {
            yield return null;

            MmInputButtonMessage received = null;
            var sub = _relay.OnButton(msg => received = msg);
            yield return null;

            var buttonMsg = new MmInputButtonMessage(MmHandedness.Right, 0, "Trigger", MmButtonState.Pressed);
            _relay.MmInvoke(buttonMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.AreEqual("Trigger", received.ButtonName);
            Assert.AreEqual(MmButtonState.Pressed, received.State);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnButtonPressed_OnlyReceivesPresses()
        {
            yield return null;

            int pressCount = 0;
            var sub = _relay.OnButtonPressed(msg => pressCount++);
            yield return null;

            // Send released - should not trigger
            var releasedMsg = new MmInputButtonMessage(MmHandedness.Right, 0, "Trigger", MmButtonState.Released);
            _relay.MmInvoke(releasedMsg);
            yield return null;

            Assert.AreEqual(0, pressCount);

            // Send pressed - should trigger
            var pressedMsg = new MmInputButtonMessage(MmHandedness.Right, 0, "Trigger", MmButtonState.Pressed);
            _relay.MmInvoke(pressedMsg);
            yield return null;

            Assert.AreEqual(1, pressCount);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnButton_FilterByName_OnlyReceivesMatchingButton()
        {
            yield return null;

            int triggerCount = 0;
            var sub = _relay.OnButton("Trigger", msg => triggerCount++);
            yield return null;

            // Send Grip - should not trigger
            var gripMsg = new MmInputButtonMessage(MmHandedness.Right, 0, "Grip", MmButtonState.Pressed);
            _relay.MmInvoke(gripMsg);
            yield return null;

            Assert.AreEqual(0, triggerCount);

            // Send Trigger - should trigger
            var triggerMsg = new MmInputButtonMessage(MmHandedness.Right, 0, "Trigger", MmButtonState.Pressed);
            _relay.MmInvoke(triggerMsg);
            yield return null;

            Assert.AreEqual(1, triggerCount);

            sub.Dispose();
        }

        #endregion

        #region OnGaze Tests

        [UnityTest]
        public IEnumerator OnGaze_ReceivesGazeMessage()
        {
            yield return null;

            MmInputGazeMessage received = null;
            var sub = _relay.OnGaze(msg => received = msg);
            yield return null;

            var gazeMsg = new MmInputGazeMessage(
                origin: Vector3.zero,
                direction: Vector3.forward,
                hitPoint: new Vector3(0, 0, 5),
                isHitting: true,
                confidence: 0.98f
            );
            _relay.MmInvoke(gazeMsg);
            yield return null;

            Assert.IsNotNull(received);
            Assert.IsTrue(received.IsHitting);
            Assert.AreEqual(0.98f, received.Confidence, 0.001f);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator OnGazeHit_OnlyReceivesHits()
        {
            yield return null;

            int hitCount = 0;
            var sub = _relay.OnGazeHit(msg => hitCount++);
            yield return null;

            // Send non-hit - should not trigger
            var missMsg = new MmInputGazeMessage(Vector3.zero, Vector3.forward, isHitting: false);
            _relay.MmInvoke(missMsg);
            yield return null;

            Assert.AreEqual(0, hitCount);

            // Send hit - should trigger
            var hitMsg = new MmInputGazeMessage(Vector3.zero, Vector3.forward, new Vector3(0, 0, 5), isHitting: true);
            _relay.MmInvoke(hitMsg);
            yield return null;

            Assert.AreEqual(1, hitCount);

            sub.Dispose();
        }

        #endregion

        #region Subscription Management Tests

        [UnityTest]
        public IEnumerator Dispose_StopsReceivingMessages()
        {
            yield return null;

            int clickCount = 0;
            var sub = _relay.OnClick((MmUIClickMessage msg) => clickCount++);
            yield return null;

            // First click - should receive
            _relay.MmInvoke(new MmUIClickMessage(Vector2.zero));
            yield return null;
            Assert.AreEqual(1, clickCount);

            // Dispose subscription
            sub.Dispose();

            // Second click - should not receive
            _relay.MmInvoke(new MmUIClickMessage(Vector2.zero));
            yield return null;
            Assert.AreEqual(1, clickCount); // Still 1
        }

        [UnityTest]
        public IEnumerator MultipleSubscriptions_AllReceiveMessages()
        {
            yield return null;

            int handler1Count = 0;
            int handler2Count = 0;

            var sub1 = _relay.OnClick((MmUIClickMessage msg) => handler1Count++);
            var sub2 = _relay.OnClick((MmUIClickMessage msg) => handler2Count++);
            yield return null;

            _relay.MmInvoke(new MmUIClickMessage(Vector2.zero));
            yield return null;

            Assert.AreEqual(1, handler1Count);
            Assert.AreEqual(1, handler2Count);

            sub1.Dispose();
            sub2.Dispose();
        }

        #endregion
    }
}
