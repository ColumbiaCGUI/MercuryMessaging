// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// InputMessageTests.cs - Tests for Standard Library Input Messages
// Part of DSL Overhaul Phase 10

using System.Collections;
using MercuryMessaging.StandardLibrary.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests.StandardLibrary
{
    /// <summary>
    /// Tests for Input message types and MmInputResponder.
    /// </summary>
    [TestFixture]
    public class InputMessageTests
    {
        private GameObject _rootObj;
        private MmRelayNode _relay;
        private TestInputResponder _responder;

        [SetUp]
        public void SetUp()
        {
            _rootObj = new GameObject("TestRoot");
            _relay = _rootObj.AddComponent<MmRelayNode>();
            _responder = _rootObj.AddComponent<TestInputResponder>();
            // Must refresh after adding responders at runtime
            _relay.MmRefreshResponders();
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootObj != null)
                Object.DestroyImmediate(_rootObj);
        }

        #region 6DOF Message Tests

        [UnityTest]
        public IEnumerator Input6DOF_IsReceived()
        {
            yield return null;

            var msg = new MmInput6DOFMessage(
                MmHandedness.Right,
                new Vector3(1, 2, 3),
                Quaternion.Euler(0, 90, 0),
                new Vector3(0.1f, 0, 0)
            );
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.DOF6Received, "6DOF message should be received");
            Assert.AreEqual(MmHandedness.Right, _responder.LastHand);
            Assert.AreEqual(new Vector3(1, 2, 3), _responder.LastPosition);
        }

        [UnityTest]
        public IEnumerator Input6DOF_Serialization_RoundTrips()
        {
            yield return null;

            var original = new MmInput6DOFMessage(
                MmHandedness.Left,
                new Vector3(1, 2, 3),
                Quaternion.Euler(45, 90, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0, 0.1f, 0),
                true
            );

            var serialized = original.Serialize();
            var deserialized = new MmInput6DOFMessage();
            deserialized.Deserialize(serialized);

            Assert.AreEqual(original.Hand, deserialized.Hand);
            Assert.AreEqual(original.Position, deserialized.Position);
            Assert.AreEqual(original.Velocity, deserialized.Velocity);
            Assert.AreEqual(original.IsTracked, deserialized.IsTracked);
        }

        #endregion

        #region Gesture Message Tests

        [UnityTest]
        public IEnumerator Gesture_IsReceived()
        {
            yield return null;

            var msg = new MmInputGestureMessage(MmHandedness.Left, MmGestureType.Pinch, 0.95f, 0.8f);
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.GestureReceived, "Gesture message should be received");
            Assert.AreEqual(MmGestureType.Pinch, _responder.LastGestureType);
            Assert.AreEqual(0.95f, _responder.LastConfidence, 0.001f);
        }

        [UnityTest]
        public IEnumerator Gesture_CustomType_Works()
        {
            yield return null;

            var msg = new MmInputGestureMessage(MmHandedness.Right, MmGestureType.Custom, 1f, 1f, "MyGesture");
            _relay.MmInvoke(msg);

            Assert.AreEqual(MmGestureType.Custom, _responder.LastGestureType);
            Assert.AreEqual("MyGesture", _responder.LastCustomGestureName);
        }

        #endregion

        #region Haptic Message Tests

        [UnityTest]
        public IEnumerator Haptic_IsReceived()
        {
            yield return null;

            var msg = new MmInputHapticMessage(MmHandedness.Both, 0.75f, 0.5f, 100f);
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.HapticReceived, "Haptic message should be received");
            Assert.AreEqual(0.75f, _responder.LastIntensity, 0.001f);
            Assert.AreEqual(0.5f, _responder.LastDuration, 0.001f);
        }

        [UnityTest]
        public IEnumerator Haptic_IntensityClamped()
        {
            yield return null;

            var msg = new MmInputHapticMessage(MmHandedness.Left, 2.0f, 1f); // Intensity > 1
            Assert.AreEqual(1f, msg.Intensity, "Intensity should be clamped to 1");
        }

        #endregion

        #region Button Message Tests

        [UnityTest]
        public IEnumerator Button_Press_IsReceived()
        {
            yield return null;

            var msg = new MmInputButtonMessage(MmHandedness.Right, 0, "Trigger", MmButtonState.Pressed, 1f);
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.ButtonReceived, "Button message should be received");
            Assert.AreEqual("Trigger", _responder.LastButtonName);
            Assert.AreEqual(MmButtonState.Pressed, _responder.LastButtonState);
        }

        [UnityTest]
        public IEnumerator Button_Release_IsReceived()
        {
            yield return null;

            var msg = new MmInputButtonMessage(MmHandedness.Left, 1, "Grip", MmButtonState.Released, 0f);
            _relay.MmInvoke(msg);

            Assert.AreEqual(MmButtonState.Released, _responder.LastButtonState);
            Assert.AreEqual(0f, _responder.LastButtonValue, 0.001f);
        }

        #endregion

        #region Axis Message Tests

        [UnityTest]
        public IEnumerator Axis_IsReceived()
        {
            yield return null;

            var msg = new MmInputAxisMessage(MmHandedness.Right, 0, "Joystick", new Vector2(0.5f, -0.5f));
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.AxisReceived, "Axis message should be received");
            Assert.AreEqual(new Vector2(0.5f, -0.5f), _responder.LastAxis2D);
        }

        #endregion

        #region Touch Message Tests

        [UnityTest]
        public IEnumerator Touch_IsReceived()
        {
            yield return null;

            var msg = new MmInputTouchMessage(MmHandedness.Right, 0, new Vector2(0.3f, 0.7f), Vector2.zero, MmTouchPhase.Began);
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.TouchReceived, "Touch message should be received");
            Assert.AreEqual(MmTouchPhase.Began, _responder.LastTouchPhase);
        }

        #endregion

        #region Controller State Message Tests

        [UnityTest]
        public IEnumerator ControllerState_Connected_IsReceived()
        {
            yield return null;

            var msg = new MmInputControllerStateMessage(MmHandedness.Right, true, "Quest Controller", 0.85f);
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.ControllerStateReceived, "Controller state should be received");
            Assert.IsTrue(_responder.LastControllerConnected);
            Assert.AreEqual("Quest Controller", _responder.LastControllerType);
            Assert.AreEqual(0.85f, _responder.LastBatteryLevel, 0.001f);
        }

        #endregion

        #region Gaze Message Tests

        [UnityTest]
        public IEnumerator Gaze_IsReceived()
        {
            yield return null;

            var msg = new MmInputGazeMessage(
                new Vector3(0, 1.5f, 0),
                Vector3.forward,
                new Vector3(0, 1.5f, 5),
                true,
                0.9f
            );
            _relay.MmInvoke(msg);

            Assert.IsTrue(_responder.GazeReceived, "Gaze message should be received");
            Assert.IsTrue(_responder.LastGazeIsHitting);
            Assert.AreEqual(0.9f, _responder.LastGazeConfidence, 0.001f);
        }

        #endregion

        #region Standard Message Passthrough Tests

        [UnityTest]
        public IEnumerator StandardMessage_PassesToBaseClass()
        {
            yield return null;

            var initMsg = new MmMessage(MmMethod.Initialize);
            _relay.MmInvoke(initMsg);

            Assert.IsTrue(_responder.InitializeReceived, "Standard Initialize should pass through");
        }

        #endregion

        #region Test Responder

        private class TestInputResponder : MmInputResponder
        {
            // 6DOF tracking
            public bool DOF6Received;
            public MmHandedness LastHand;
            public Vector3 LastPosition;

            // Gesture tracking
            public bool GestureReceived;
            public MmGestureType LastGestureType;
            public float LastConfidence;
            public string LastCustomGestureName;

            // Haptic tracking
            public bool HapticReceived;
            public float LastIntensity;
            public float LastDuration;

            // Button tracking
            public bool ButtonReceived;
            public string LastButtonName;
            public MmButtonState LastButtonState;
            public float LastButtonValue;

            // Axis tracking
            public bool AxisReceived;
            public Vector2 LastAxis2D;

            // Touch tracking
            public bool TouchReceived;
            public MmTouchPhase LastTouchPhase;

            // Controller state tracking
            public bool ControllerStateReceived;
            public bool LastControllerConnected;
            public string LastControllerType;
            public float LastBatteryLevel;

            // Gaze tracking
            public bool GazeReceived;
            public bool LastGazeIsHitting;
            public float LastGazeConfidence;

            // Standard message tracking
            public bool InitializeReceived;

            protected override void Received6DOF(MmInput6DOFMessage message)
            {
                DOF6Received = true;
                LastHand = message.Hand;
                LastPosition = message.Position;
            }

            protected override void ReceivedGesture(MmInputGestureMessage message)
            {
                GestureReceived = true;
                LastGestureType = message.GestureType;
                LastConfidence = message.Confidence;
                LastCustomGestureName = message.CustomName;
            }

            protected override void ReceivedHaptic(MmInputHapticMessage message)
            {
                HapticReceived = true;
                LastIntensity = message.Intensity;
                LastDuration = message.Duration;
            }

            protected override void ReceivedButton(MmInputButtonMessage message)
            {
                ButtonReceived = true;
                LastButtonName = message.ButtonName;
                LastButtonState = message.State;
                LastButtonValue = message.Value;
            }

            protected override void ReceivedAxis(MmInputAxisMessage message)
            {
                AxisReceived = true;
                LastAxis2D = message.Value2D;
            }

            protected override void ReceivedTouch(MmInputTouchMessage message)
            {
                TouchReceived = true;
                LastTouchPhase = message.Phase;
            }

            protected override void ReceivedControllerState(MmInputControllerStateMessage message)
            {
                ControllerStateReceived = true;
                LastControllerConnected = message.IsConnected;
                LastControllerType = message.ControllerType;
                LastBatteryLevel = message.BatteryLevel;
            }

            protected override void ReceivedGaze(MmInputGazeMessage message)
            {
                GazeReceived = true;
                LastGazeIsHitting = message.IsHitting;
                LastGazeConfidence = message.Confidence;
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
