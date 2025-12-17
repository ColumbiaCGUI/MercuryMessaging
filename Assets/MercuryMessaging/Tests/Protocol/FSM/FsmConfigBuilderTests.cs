// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// FsmConfigBuilderTests.cs - Tests for FSM Configuration DSL
// Part of DSL Overhaul Phase 2

using System.Collections;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for FsmConfigBuilder and MmRelaySwitchNodeExtensions.
    /// Tests cover fluent FSM configuration API.
    /// </summary>
    [TestFixture]
    public class FsmConfigBuilderTests
    {
        private GameObject _switchNodeObj;
        private MmRelaySwitchNode _switchNode;
        private GameObject _state1Obj;
        private GameObject _state2Obj;
        private MmRelayNode _state1;
        private MmRelayNode _state2;

        [SetUp]
        public void SetUp()
        {
            // Create switch node
            _switchNodeObj = new GameObject("SwitchNode");
            _switchNode = _switchNodeObj.AddComponent<MmRelaySwitchNode>();

            // Create state 1
            _state1Obj = new GameObject("State1");
            _state1Obj.transform.SetParent(_switchNodeObj.transform);
            _state1 = _state1Obj.AddComponent<MmRelayNode>();

            // Create state 2
            _state2Obj = new GameObject("State2");
            _state2Obj.transform.SetParent(_switchNodeObj.transform);
            _state2 = _state2Obj.AddComponent<MmRelayNode>();

            // Setup routing table
            _switchNode.MmAddToRoutingTable(_state1, MmLevelFilter.Child);
            _switchNode.MmAddToRoutingTable(_state2, MmLevelFilter.Child);

            // CRITICAL: Rebuild FSM after routing table is populated
            // The FSM is built in Awake() with an empty routing table
            // so we must rebuild it now that we have registered the states
            _switchNode.RebuildFSM();
        }

        [TearDown]
        public void TearDown()
        {
            if (_switchNodeObj != null) Object.DestroyImmediate(_switchNodeObj);
        }

        #region ConfigureStates Tests

        [UnityTest]
        public IEnumerator ConfigureStates_ReturnsBuilder()
        {
            yield return null; // Let Awake run

            var builder = _switchNode.ConfigureStates();

            Assert.IsNotNull(builder, "ConfigureStates should return a builder");
        }

        [UnityTest]
        public IEnumerator OnGlobalEnter_CallbackInvoked()
        {
            yield return null; // Let Awake run

            bool callbackInvoked = false;

            _switchNode.ConfigureStates()
                .OnGlobalEnter(() => callbackInvoked = true)
                .Build();

            // Jump to state to trigger global enter
            _switchNode.GoTo("State1");

            Assert.IsTrue(callbackInvoked, "Global enter callback should be invoked");
        }

        [UnityTest]
        public IEnumerator OnEnter_StateCallback_Invoked()
        {
            yield return null; // Let Awake run

            bool state1EnterCalled = false;

            _switchNode.ConfigureStates()
                .OnEnter("State1", () => state1EnterCalled = true)
                .Build();

            _switchNode.GoTo("State1");

            Assert.IsTrue(state1EnterCalled, "State1 enter callback should be invoked");
        }

        [UnityTest]
        public IEnumerator OnExit_StateCallback_Invoked()
        {
            yield return null; // Let Awake run

            bool state1ExitCalled = false;

            _switchNode.ConfigureStates()
                .OnExit("State1", () => state1ExitCalled = true)
                .StartWith("State1")
                .Build();

            // Now exit State1 by going to State2
            _switchNode.GoTo("State2");

            Assert.IsTrue(state1ExitCalled, "State1 exit callback should be invoked when leaving");
        }

        [UnityTest]
        public IEnumerator StartWith_SetsInitialState()
        {
            yield return null; // Let Awake run

            _switchNode.ConfigureStates()
                .StartWith("State2")
                .Build();

            Assert.AreEqual("State2", _switchNode.GetCurrentStateName());
        }

        [UnityTest]
        public IEnumerator ChainedConfiguration_AllCallbacksWork()
        {
            yield return null; // Let Awake run

            int enterCount = 0;
            int exitCount = 0;
            int globalEnterCount = 0;

            _switchNode.ConfigureStates()
                .OnGlobalEnter(() => globalEnterCount++)
                .OnEnter("State1", () => enterCount++)
                .OnExit("State1", () => exitCount++)
                .OnEnter("State2", () => enterCount++)
                .StartWith("State1")
                .Build();

            Assert.AreEqual(1, enterCount, "State1 enter should be called");
            Assert.AreEqual(1, globalEnterCount, "Global enter should be called");

            _switchNode.GoTo("State2");

            Assert.AreEqual(2, enterCount, "State2 enter should also be called");
            Assert.AreEqual(1, exitCount, "State1 exit should be called");
            Assert.AreEqual(2, globalEnterCount, "Global enter should be called again");
        }

        #endregion

        #region Quick State Transition Tests

        [UnityTest]
        public IEnumerator GoTo_TransitionsToState()
        {
            yield return null; // Let Awake run

            _switchNode.GoTo("State1");

            Assert.AreEqual("State1", _switchNode.GetCurrentStateName());
        }

        [UnityTest]
        public IEnumerator IsInState_ReturnsCorrectly()
        {
            yield return null; // Let Awake run

            _switchNode.GoTo("State1");

            Assert.IsTrue(_switchNode.IsInState("State1"));
            Assert.IsFalse(_switchNode.IsInState("State2"));
        }

        [UnityTest]
        public IEnumerator GoToPrevious_ReturnsToLastState()
        {
            yield return null; // Let Awake run

            _switchNode.GoTo("State1");
            _switchNode.GoTo("State2");

            bool result = _switchNode.GoToPrevious();

            Assert.IsTrue(result, "GoToPrevious should succeed");
            Assert.AreEqual("State1", _switchNode.GetCurrentStateName());
        }

        #endregion

        #region Quick Registration Tests

        [UnityTest]
        public IEnumerator OnStateEnter_QuickRegistration_Works()
        {
            yield return null; // Let Awake run

            bool called = false;
            _switchNode.OnStateEnter("State1", () => called = true);

            _switchNode.GoTo("State1");

            Assert.IsTrue(called, "Quick-registered enter callback should be invoked");
        }

        [UnityTest]
        public IEnumerator OnStateExit_QuickRegistration_Works()
        {
            yield return null; // Let Awake run

            bool called = false;
            _switchNode.GoTo("State1");

            _switchNode.OnStateExit("State1", () => called = true);
            _switchNode.GoTo("State2");

            Assert.IsTrue(called, "Quick-registered exit callback should be invoked");
        }

        #endregion
    }
}
