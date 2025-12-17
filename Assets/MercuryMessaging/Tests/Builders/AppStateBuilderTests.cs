// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// AppStateBuilderTests.cs - Tests for App State DSL
// Part of DSL Overhaul Phase 8


using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for AppStateBuilder and MmAppStateExtensions.
    /// Tests cover fluent state configuration, transitions, and callbacks.
    /// </summary>
    [TestFixture]
    public class AppStateBuilderTests
    {
        private GameObject _rootObj;
        private MmRelaySwitchNode _switchNode;
        private GameObject _state1Obj;
        private GameObject _state2Obj;
        private GameObject _state3Obj;

        [SetUp]
        public void SetUp()
        {
            _rootObj = new GameObject("SwitchNode");
            _switchNode = _rootObj.AddComponent<MmRelaySwitchNode>();

            // Create state child objects
            _state1Obj = new GameObject("State1");
            _state1Obj.transform.SetParent(_rootObj.transform);
            var state1Relay = _state1Obj.AddComponent<MmRelayNode>();

            _state2Obj = new GameObject("State2");
            _state2Obj.transform.SetParent(_rootObj.transform);
            var state2Relay = _state2Obj.AddComponent<MmRelayNode>();

            _state3Obj = new GameObject("State3");
            _state3Obj.transform.SetParent(_rootObj.transform);
            var state3Relay = _state3Obj.AddComponent<MmRelayNode>();

            // CRITICAL: Register states in routing table
            // The FSM is built from the routing table, so states must be registered
            _switchNode.MmAddToRoutingTable(state1Relay, MmLevelFilter.Child);
            _switchNode.MmAddToRoutingTable(state2Relay, MmLevelFilter.Child);
            _switchNode.MmAddToRoutingTable(state3Relay, MmLevelFilter.Child);

            // CRITICAL: Rebuild FSM after routing table is populated
            // The FSM was built in Awake() with an empty routing table
            _switchNode.RebuildFSM();
        }

        [TearDown]
        public void TearDown()
        {
            if (_rootObj != null)
                Object.DestroyImmediate(_rootObj);
        }

        #region Configuration Tests

        [Test]
        public void Configure_ReturnsBuilder()
        {
            var builder = MmAppState.Configure(_switchNode);

            Assert.IsNotNull(builder);
        }

        [Test]
        public void ConfigureStates_ReturnsBuilder()
        {
            var builder = _switchNode.ConfigureStates();

            Assert.IsNotNull(builder);
        }

        [Test]
        public void DefineState_ReturnsBuilder()
        {
            var builder = MmAppState.Configure(_switchNode)
                .DefineState("State1");

            Assert.IsNotNull(builder);
        }

        [Test]
        public void ChainedDefinition_Works()
        {
            var result = MmAppState.Configure(_switchNode)
                .DefineState("State1")
                    .OnEnter(() => { })
                    .OnExit(() => { })
                .DefineState("State2")
                    .OnEnter(() => { })
                .Build();

            Assert.IsNotNull(result);
            Assert.AreSame(_switchNode, result);
        }

        #endregion

        #region Callback Tests

        [Test]
        public void OnEnter_CallbackInvoked()
        {
            bool entered = false;

            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                    .OnEnter(() => entered = true)
                .StartWith("State1")
                .Build();

            Assert.IsTrue(entered);
        }

        [Test]
        public void OnExit_CallbackInvoked()
        {
            bool exited = false;

            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                    .OnExit(() => exited = true)
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            _switchNode.GoTo("State2");

            Assert.IsTrue(exited);
        }

        [Test]
        public void OnTransition_SetsBothCallbacks()
        {
            bool entered = false;
            bool exited = false;

            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                    .OnTransition(() => entered = true, () => exited = true)
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            Assert.IsTrue(entered);

            _switchNode.GoTo("State2");
            Assert.IsTrue(exited);
        }

        [Test]
        public void OnAnyStateEnter_CalledForAllStates()
        {
            int enterCount = 0;

            MmAppState.Configure(_switchNode)
                .OnAnyStateEnter(() => enterCount++)
                .DefineState("State1")
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            _switchNode.GoTo("State2");

            Assert.AreEqual(2, enterCount);
        }

        #endregion

        #region Navigation Tests

        [Test]
        public void GoTo_ChangesState()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            _switchNode.GoTo("State2");

            Assert.AreEqual("State2", _switchNode.GetCurrentStateName());
        }

        [Test]
        public void GoBack_ReturnsToPreviousState()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            _switchNode.GoTo("State2");
            _switchNode.GoToPrevious();

            Assert.AreEqual("State1", _switchNode.GetCurrentStateName());
        }

        [Test]
        public void IsInState_ReturnsCorrectValue()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .StartWith("State1")
                .Build();

            Assert.IsTrue(_switchNode.IsInState("State1"));
            Assert.IsFalse(_switchNode.IsInState("State2"));
        }

        #endregion

        #region Query Tests

        [Test]
        public void GetCurrentStateName_ReturnsStateName()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .StartWith("State1")
                .Build();

            Assert.AreEqual("State1", _switchNode.GetCurrentStateName());
        }

        [Test]
        public void GetPreviousStateName_ReturnsPreviousStateName()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            _switchNode.GoTo("State2");

            Assert.AreEqual("State1", _switchNode.GetPreviousStateName());
        }

        [Test]
        public void HasPreviousState_ReturnsTrueAfterTransition()
        {
            MmAppState.Configure(_switchNode)
                .DefineState("State1")
                .DefineState("State2")
                .StartWith("State1")
                .Build();

            // Initially null Previous
            _switchNode.GoTo("State2");

            Assert.IsNotNull(_switchNode.RespondersFSM?.Previous);
        }

        #endregion
    }
}
