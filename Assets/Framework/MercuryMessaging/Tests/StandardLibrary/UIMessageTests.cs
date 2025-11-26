// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// UIMessageTests.cs - Tests for Standard Library UI Messages
// Part of DSL Overhaul Phase 9

using System.Collections;
using MercuryMessaging.StandardLibrary.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests.StandardLibrary
{
    /// <summary>
    /// Tests for UI message types and MmUIResponder.
    /// </summary>
    [TestFixture]
    public class UIMessageTests
    {
        private GameObject _rootObj;
        private MmRelayNode _relay;
        private TestUIResponder _responder;

        [SetUp]
        public void SetUp()
        {
            _rootObj = new GameObject("TestRoot");
            _relay = _rootObj.AddComponent<MmRelayNode>();
            _responder = _rootObj.AddComponent<TestUIResponder>();
            // Must refresh after adding responders at runtime
            _relay.MmRefreshResponders();
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootObj != null)
                Object.DestroyImmediate(_rootObj);
        }

        #region Click Message Tests

        [UnityTest]
        public IEnumerator ClickMessage_IsReceived()
        {
            yield return null; // Let Awake run

            var clickMsg = new MmUIClickMessage(new Vector2(100, 200), 1, 0);
            _relay.MmInvoke(clickMsg);

            Assert.IsTrue(_responder.ClickReceived, "Click message should be received");
            Assert.AreEqual(new Vector2(100, 200), _responder.LastClickPosition, "Position should match");
        }

        [UnityTest]
        public IEnumerator ClickMessage_DoubleClick_IsDetected()
        {
            yield return null;

            var clickMsg = new MmUIClickMessage(Vector2.zero, 2, 0);
            _relay.MmInvoke(clickMsg);

            Assert.IsTrue(_responder.LastClickIsDouble, "Double click should be detected");
        }

        [UnityTest]
        public IEnumerator ClickMessage_RightClick_IsDetected()
        {
            yield return null;

            var clickMsg = new MmUIClickMessage(Vector2.zero, 1, 1);
            _relay.MmInvoke(clickMsg);

            Assert.IsTrue(_responder.LastClickIsRight, "Right click should be detected");
        }

        [UnityTest]
        public IEnumerator ClickMessage_Serialization_RoundTrips()
        {
            yield return null;

            var original = new MmUIClickMessage(new Vector2(150, 250), 2, 1);
            var serialized = original.Serialize();
            var deserialized = new MmUIClickMessage();
            deserialized.Deserialize(serialized);

            Assert.AreEqual(original.Position, deserialized.Position);
            Assert.AreEqual(original.ClickCount, deserialized.ClickCount);
            Assert.AreEqual(original.Button, deserialized.Button);
        }

        #endregion

        #region Hover Message Tests

        [UnityTest]
        public IEnumerator HoverMessage_Enter_IsReceived()
        {
            yield return null;

            var hoverMsg = new MmUIHoverMessage(new Vector2(50, 50), true);
            _relay.MmInvoke(hoverMsg);

            Assert.IsTrue(_responder.HoverReceived, "Hover message should be received");
            Assert.IsTrue(_responder.LastHoverIsEnter, "Should be hover enter");
        }

        [UnityTest]
        public IEnumerator HoverMessage_Exit_IsReceived()
        {
            yield return null;

            var hoverMsg = new MmUIHoverMessage(new Vector2(50, 50), false);
            _relay.MmInvoke(hoverMsg);

            Assert.IsTrue(_responder.HoverReceived, "Hover message should be received");
            Assert.IsFalse(_responder.LastHoverIsEnter, "Should be hover exit");
        }

        #endregion

        #region Drag Message Tests

        [UnityTest]
        public IEnumerator DragMessage_Begin_IsReceived()
        {
            yield return null;

            var dragMsg = new MmUIDragMessage(Vector2.zero, Vector2.zero, MmDragPhase.Begin);
            _relay.MmInvoke(dragMsg);

            Assert.IsTrue(_responder.DragReceived, "Drag message should be received");
            Assert.AreEqual(MmDragPhase.Begin, _responder.LastDragPhase);
        }

        [UnityTest]
        public IEnumerator DragMessage_Move_UpdatesDelta()
        {
            yield return null;

            var delta = new Vector2(10, 20);
            var dragMsg = new MmUIDragMessage(new Vector2(100, 100), delta, MmDragPhase.Move);
            _relay.MmInvoke(dragMsg);

            Assert.AreEqual(delta, _responder.LastDragDelta);
        }

        #endregion

        #region Scroll Message Tests

        [UnityTest]
        public IEnumerator ScrollMessage_IsReceived()
        {
            yield return null;

            var scrollMsg = new MmUIScrollMessage(Vector2.zero, new Vector2(0, 120));
            _relay.MmInvoke(scrollMsg);

            Assert.IsTrue(_responder.ScrollReceived, "Scroll message should be received");
            Assert.AreEqual(new Vector2(0, 120), _responder.LastScrollDelta);
        }

        #endregion

        #region Focus Message Tests

        [UnityTest]
        public IEnumerator FocusMessage_Gained_IsReceived()
        {
            yield return null;

            var focusMsg = new MmUIFocusMessage(true, "TestElement");
            _relay.MmInvoke(focusMsg);

            Assert.IsTrue(_responder.FocusReceived, "Focus message should be received");
            Assert.IsTrue(_responder.LastFocusState, "Should be focus gained");
            Assert.AreEqual("TestElement", _responder.LastFocusElementId);
        }

        #endregion

        #region Select Message Tests

        [UnityTest]
        public IEnumerator SelectMessage_IsReceived()
        {
            yield return null;

            var selectMsg = new MmUISelectMessage(2, "Option C", 0);
            _relay.MmInvoke(selectMsg);

            Assert.IsTrue(_responder.SelectReceived, "Select message should be received");
            Assert.AreEqual(2, _responder.LastSelectedIndex);
            Assert.AreEqual("Option C", _responder.LastSelectedValue);
            Assert.AreEqual(0, _responder.LastPreviousIndex);
        }

        #endregion

        #region Submit/Cancel Message Tests

        [UnityTest]
        public IEnumerator SubmitMessage_IsReceived()
        {
            yield return null;

            var submitMsg = new MmUISubmitMessage("FormData");
            _relay.MmInvoke(submitMsg);

            Assert.IsTrue(_responder.SubmitReceived, "Submit message should be received");
            Assert.AreEqual("FormData", _responder.LastSubmitData);
        }

        [UnityTest]
        public IEnumerator CancelMessage_IsReceived()
        {
            yield return null;

            var cancelMsg = new MmUICancelMessage("User pressed Escape");
            _relay.MmInvoke(cancelMsg);

            Assert.IsTrue(_responder.CancelReceived, "Cancel message should be received");
            Assert.AreEqual("User pressed Escape", _responder.LastCancelReason);
        }

        #endregion

        #region Standard Message Passthrough Tests

        [UnityTest]
        public IEnumerator StandardMessage_PassesToBaseClass()
        {
            yield return null;

            // Standard MmMethod.Initialize should still work
            var initMsg = new MmMessage(MmMethod.Initialize);
            _relay.MmInvoke(initMsg);

            Assert.IsTrue(_responder.InitializeReceived, "Standard Initialize should pass through to base class");
        }

        #endregion

        #region Test Responder

        /// <summary>
        /// Test responder that tracks all received UI messages.
        /// </summary>
        private class TestUIResponder : MmUIResponder
        {
            // Click tracking
            public bool ClickReceived;
            public Vector2 LastClickPosition;
            public bool LastClickIsDouble;
            public bool LastClickIsRight;

            // Hover tracking
            public bool HoverReceived;
            public bool LastHoverIsEnter;

            // Drag tracking
            public bool DragReceived;
            public MmDragPhase LastDragPhase;
            public Vector2 LastDragDelta;

            // Scroll tracking
            public bool ScrollReceived;
            public Vector2 LastScrollDelta;

            // Focus tracking
            public bool FocusReceived;
            public bool LastFocusState;
            public string LastFocusElementId;

            // Select tracking
            public bool SelectReceived;
            public int LastSelectedIndex;
            public string LastSelectedValue;
            public int LastPreviousIndex;

            // Submit/Cancel tracking
            public bool SubmitReceived;
            public string LastSubmitData;
            public bool CancelReceived;
            public string LastCancelReason;

            // Standard message tracking
            public bool InitializeReceived;

            protected override void ReceivedClick(MmUIClickMessage message)
            {
                ClickReceived = true;
                LastClickPosition = message.Position;
                LastClickIsDouble = message.IsDoubleClick;
                LastClickIsRight = message.IsRightClick;
            }

            protected override void ReceivedHover(MmUIHoverMessage message)
            {
                HoverReceived = true;
                LastHoverIsEnter = message.IsEnter;
            }

            protected override void ReceivedDrag(MmUIDragMessage message)
            {
                DragReceived = true;
                LastDragPhase = message.Phase;
                LastDragDelta = message.Delta;
            }

            protected override void ReceivedScroll(MmUIScrollMessage message)
            {
                ScrollReceived = true;
                LastScrollDelta = message.ScrollDelta;
            }

            protected override void ReceivedFocus(MmUIFocusMessage message)
            {
                FocusReceived = true;
                LastFocusState = message.IsFocused;
                LastFocusElementId = message.ElementId;
            }

            protected override void ReceivedSelect(MmUISelectMessage message)
            {
                SelectReceived = true;
                LastSelectedIndex = message.SelectedIndex;
                LastSelectedValue = message.SelectedValue;
                LastPreviousIndex = message.PreviousIndex;
            }

            protected override void ReceivedSubmit(MmUISubmitMessage message)
            {
                SubmitReceived = true;
                LastSubmitData = message.Data;
            }

            protected override void ReceivedCancel(MmUICancelMessage message)
            {
                CancelReceived = true;
                LastCancelReason = message.Reason;
            }

            public override void Initialize()
            {
                base.Initialize();
                InitializeReceived = true;
            }
        }

        #endregion
    }
}
