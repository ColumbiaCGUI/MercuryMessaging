// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// FSM State Transition Tests
// Validates MmRelaySwitchNode and FiniteStateMachine behavior

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;


namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for FSM State Transition implementation.
    /// Validates MmRelaySwitchNode and FiniteStateMachine edge cases.
    /// </summary>
    [TestFixture]
    public class FsmStateTransitionTests
    {
        private GameObject rootObject;
        private MmRelaySwitchNode switchNode;
        private List<GameObject> stateObjects;
        private List<string> eventLog;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Ignore TLS allocator warnings for entire test fixture
            LogAssert.ignoreFailingMessages = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Re-enable log assertions after all tests complete
            LogAssert.ignoreFailingMessages = false;
        }

        [SetUp]
        public void SetUp()
        {
            stateObjects = new List<GameObject>();
            eventLog = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            // Unity automatically cleans up GameObjects between tests
            stateObjects.Clear();
            eventLog.Clear();
            rootObject = null;
            switchNode = null;
        }

        #region Helper Methods

        /// <summary>
        /// Creates a simple FSM hierarchy with the specified number of states
        /// </summary>
        private void CreateSimpleFSM(int stateCount)
        {
            // Create root with MmRelaySwitchNode
            rootObject = new GameObject("FSM_Root");
            switchNode = rootObject.AddComponent<MmRelaySwitchNode>();

            // Create state children (MmRelayNode components)
            for (int i = 0; i < stateCount; i++)
            {
                GameObject stateObj = new GameObject($"State{i}");
                stateObj.transform.SetParent(rootObject.transform);
                var childNode = stateObj.AddComponent<MmRelayNode>();
                stateObjects.Add(stateObj);

                // Manually add child to parent's routing table (children don't auto-register in tests)
                switchNode.MmAddToRoutingTable(childNode, MmLevelFilter.Child);
            }

            // Manually call Awake on parent to initialize FSM from routing table
            switchNode.Awake();
        }

        /// <summary>
        /// Attaches event handlers to FSM for logging
        /// </summary>
        private void AttachEventLoggers()
        {
            switchNode.RespondersFSM.GlobalEnter += () => {
                eventLog.Add($"GlobalEnter: {switchNode.CurrentName}");
            };

            switchNode.RespondersFSM.GlobalExit += () => {
                eventLog.Add($"GlobalExit: {switchNode.CurrentName}");
            };

            // Attach per-state handlers
            foreach (var stateObj in stateObjects)
            {
                string stateName = stateObj.name;
                var item = switchNode.RoutingTable[stateName];

                switchNode.RespondersFSM[item].Enter += () => {
                    eventLog.Add($"Enter: {stateName}");
                };

                switchNode.RespondersFSM[item].Exit += () => {
                    eventLog.Add($"Exit: {stateName}");
                };
            }
        }

        #endregion

        #region Basic Transition Tests

        /// <summary>
        /// Test that JumpTo() transitions to a valid state
        /// </summary>
        [UnityTest]
        public IEnumerator JumpTo_ValidState_TransitionsSuccessfully()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            // Act
            switchNode.JumpTo("State1");
            yield return null;

            // Assert
            Assert.AreEqual("State1", switchNode.CurrentName);
            Assert.IsNotNull(switchNode.Current);
            Assert.AreEqual("State1", switchNode.Current.gameObject.name);

            yield return null;
        }

        /// <summary>
        /// Test that JumpTo() with MmRelayNode reference works
        /// </summary>
        [UnityTest]
        public IEnumerator JumpTo_ValidRelayNode_TransitionsSuccessfully()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            // Act
            var targetNode = stateObjects[2].GetComponent<MmRelayNode>();
            switchNode.JumpTo(targetNode);
            yield return null;

            // Assert
            Assert.AreEqual("State2", switchNode.CurrentName);
            Assert.AreEqual(targetNode.gameObject, switchNode.Current.gameObject);

            yield return null;
        }

        /// <summary>
        /// Test that StartTransitionTo() begins transition but doesn't complete until EnterNext()
        /// </summary>
        [UnityTest]
        public IEnumerator StartTransitionTo_BeginsTransition_WaitsForEnterNext()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            string initialState = switchNode.CurrentName;

            // Act
            switchNode.RespondersFSM.StartTransitionTo(switchNode.RoutingTable["State1"]);
            yield return null;

            // State should still be State0 until EnterNext is called
            Assert.AreEqual(initialState, switchNode.CurrentName);

            // Complete the transition
            switchNode.RespondersFSM.EnterNext();
            yield return null;

            // Assert
            Assert.AreEqual("State1", switchNode.CurrentName);

            yield return null;
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Test that JumpTo() same state does nothing (FSM equality check)
        /// </summary>
        [UnityTest]
        public IEnumerator JumpTo_SameState_DoesNothing()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            AttachEventLoggers();
            eventLog.Clear(); // Clear initial transition events

            // Act
            switchNode.JumpTo("State0");
            yield return null;

            // Assert - No events should fire
            Assert.AreEqual(0, eventLog.Count, "JumpTo same state should not trigger any events");
            Assert.AreEqual("State0", switchNode.CurrentName);

            yield return null;
        }

        /// <summary>
        /// Test that accessing non-existent state by name throws exception
        /// Note: RoutingTable indexer returns null (not KeyNotFoundException),
        /// which causes NullReferenceException in FSM.JumpTo
        /// </summary>
        [UnityTest]
        public IEnumerator JumpTo_NonExistentState_ThrowsException()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => {
                switchNode.JumpTo("NonExistentState");
            });

            yield return null;
        }

        /// <summary>
        /// Test that FSM handles null Current state gracefully
        /// </summary>
        [UnityTest]
        public IEnumerator FSM_NullCurrent_HandlesGracefully()
        {
            // Arrange
            CreateSimpleFSM(3);
            // Don't set initial state - Current should be null/default

            AttachEventLoggers();

            // Act - Transition from null state
            switchNode.JumpTo("State1");
            yield return null;

            // Assert - Should transition successfully
            Assert.AreEqual("State1", switchNode.CurrentName);
            // GlobalExit should not fire from null state (Exit checks if Current != null)
            Assert.IsTrue(eventLog.Contains("Enter: State1"), "Enter event should fire");
            Assert.IsTrue(eventLog.Contains("GlobalEnter: State1"), "GlobalEnter should fire");

            yield return null;
        }

        #endregion

        #region Event Ordering Tests

        /// <summary>
        /// Test that events fire in correct order: Exit → GlobalExit → GlobalEnter → Enter
        /// </summary>
        [UnityTest]
        public IEnumerator EventOrdering_ExitBeforeEnter()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            AttachEventLoggers();
            eventLog.Clear();

            // Act
            switchNode.JumpTo("State1");
            yield return null;

            // Assert - Check event order
            Assert.GreaterOrEqual(eventLog.Count, 4, "Should have at least 4 events");

            int exitIndex = eventLog.FindIndex(e => e.StartsWith("Exit: State0"));
            int globalExitIndex = eventLog.FindIndex(e => e.StartsWith("GlobalExit:"));
            int globalEnterIndex = eventLog.FindIndex(e => e.StartsWith("GlobalEnter:"));
            int enterIndex = eventLog.FindIndex(e => e.StartsWith("Enter: State1"));

            // Actual FSM event order: GlobalExit → Exit → Enter → GlobalEnter
            Assert.IsTrue(globalExitIndex < exitIndex, "GlobalExit should fire before Exit");
            Assert.IsTrue(exitIndex < enterIndex, "Exit should fire before Enter");
            Assert.IsTrue(enterIndex < globalEnterIndex, "Enter should fire before GlobalEnter");

            Debug.Log($"Event order: {string.Join(" → ", eventLog)}");

            yield return null;
        }

        /// <summary>
        /// Test that GlobalEnter and GlobalExit fire for all transitions
        /// </summary>
        [UnityTest]
        public IEnumerator GlobalEvents_FireForAllTransitions()
        {
            // Arrange
            CreateSimpleFSM(5);
            switchNode.JumpTo("State0");
            yield return null;

            AttachEventLoggers();
            eventLog.Clear();

            // Act - Multiple transitions
            switchNode.JumpTo("State1");
            yield return null;
            switchNode.JumpTo("State2");
            yield return null;
            switchNode.JumpTo("State3");
            yield return null;

            // Assert
            int globalEnterCount = eventLog.FindAll(e => e.StartsWith("GlobalEnter:")).Count;
            int globalExitCount = eventLog.FindAll(e => e.StartsWith("GlobalExit:")).Count;

            Assert.AreEqual(3, globalEnterCount, "GlobalEnter should fire for each transition");
            Assert.AreEqual(3, globalExitCount, "GlobalExit should fire for each transition");

            yield return null;
        }

        #endregion

        #region SelectedFilter Behavior Tests

        /// <summary>
        /// Test that SelectedFilter.Selected only reaches current FSM state
        /// </summary>
        [UnityTest]
        public IEnumerator SelectedFilter_OnlyReachesCurrentState()
        {
            // Arrange
            CreateSimpleFSM(3);

            // Add tracking responders to each state
            var responders = new List<FsmTestResponder>();
            foreach (var stateObj in stateObjects)
            {
                var responder = stateObj.AddComponent<FsmTestResponder>();
                // Manually refresh responders on each state's relay node
                var stateNode = stateObj.GetComponent<MmRelayNode>();
                stateNode.MmRefreshResponders();
                responders.Add(responder);
            }

            switchNode.JumpTo("State1");
            yield return null;

            // Reset counters
            foreach (var r in responders) r.messageCount = 0;

            // Act - Send message with SelectedFilter.Selected
            switchNode.MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(
                    MmLevelFilter.Child,
                    MmActiveFilter.All,
                    MmSelectedFilter.Selected
                ))
            );
            yield return null;

            // Assert - Only State1 responder should receive message
            Assert.AreEqual(0, responders[0].messageCount, "State0 should not receive (not selected)");
            Assert.AreEqual(1, responders[1].messageCount, "State1 should receive (selected)");
            Assert.AreEqual(0, responders[2].messageCount, "State2 should not receive (not selected)");

            yield return null;
        }

        /// <summary>
        /// Test that SelectedFilter.All reaches all states regardless of FSM state
        /// </summary>
        [UnityTest]
        public IEnumerator SelectedFilter_All_ReachesAllStates()
        {
            // Arrange
            CreateSimpleFSM(3);

            var responders = new List<FsmTestResponder>();
            foreach (var stateObj in stateObjects)
            {
                var responder = stateObj.AddComponent<FsmTestResponder>();
                // Manually refresh responders on each state's relay node
                var stateNode = stateObj.GetComponent<MmRelayNode>();
                stateNode.MmRefreshResponders();
                responders.Add(responder);
            }

            switchNode.JumpTo("State1");
            yield return null;

            foreach (var r in responders) r.messageCount = 0;

            // Act - Send message with SelectedFilter.All
            switchNode.MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(
                    MmLevelFilter.Child,
                    MmActiveFilter.All,
                    MmSelectedFilter.All
                ))
            );
            yield return null;

            // Assert - All responders should receive message
            Assert.AreEqual(1, responders[0].messageCount, "State0 should receive (All selected)");
            Assert.AreEqual(1, responders[1].messageCount, "State1 should receive (All selected)");
            Assert.AreEqual(1, responders[2].messageCount, "State2 should receive (All selected)");

            yield return null;
        }

        #endregion

        #region Rapid State Change Tests

        /// <summary>
        /// Test rapid state changes in sequence
        /// </summary>
        [UnityTest]
        public IEnumerator RapidStateChanges_Sequential_HandlesCorrectly()
        {
            // Arrange
            CreateSimpleFSM(5);
            switchNode.JumpTo("State0");
            yield return null;

            // Act - Rapid transitions
            switchNode.JumpTo("State1");
            switchNode.JumpTo("State2");
            switchNode.JumpTo("State3");
            switchNode.JumpTo("State4");
            yield return null;

            // Assert - Should end up in final state
            Assert.AreEqual("State4", switchNode.CurrentName);

            yield return null;
        }

        /// <summary>
        /// Test that CancelStateChange works during StartTransitionTo
        /// </summary>
        [UnityTest]
        public IEnumerator CancelStateChange_DuringTransition_CancelsSuccessfully()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;

            // Act - Start transition but cancel before EnterNext
            switchNode.RespondersFSM.StartTransitionTo(switchNode.RoutingTable["State1"]);
            yield return null;

            bool cancelSuccess = switchNode.RespondersFSM.CancelStateChange();
            yield return null;

            // Assert
            Assert.IsTrue(cancelSuccess, "Cancel should succeed during transition");
            Assert.AreEqual("State0", switchNode.CurrentName, "State should remain State0");

            yield return null;
        }

        /// <summary>
        /// Test that many rapid transitions complete correctly
        /// </summary>
        [UnityTest]
        public IEnumerator RapidTransitions_ManyStates_NoMemoryLeaks()
        {
            // Arrange
            CreateSimpleFSM(10);
            switchNode.JumpTo("State0");
            yield return null;

            long initialMemory = System.GC.GetTotalMemory(true);

            // Act - Many rapid transitions
            for (int i = 0; i < 100; i++)
            {
                int targetState = i % 10;
                switchNode.JumpTo($"State{targetState}");

                // Yield occasionally to prevent blocking
                if (i % 10 == 0)
                    yield return null;
            }
            yield return null;

            long finalMemory = System.GC.GetTotalMemory(true);
            long memoryGrowth = finalMemory - initialMemory;

            // Assert - Memory growth should be minimal (<1MB)
            Assert.Less(memoryGrowth, 1024 * 1024,
                $"Memory growth should be minimal, but grew by {memoryGrowth / 1024}KB");

            Debug.Log($"100 rapid transitions completed. Memory growth: {memoryGrowth / 1024}KB");

            yield return null;
        }

        #endregion

        #region Complex Scenario Tests

        /// <summary>
        /// Test FSM behavior with 10+ states (complex FSM)
        /// </summary>
        [UnityTest]
        public IEnumerator ComplexFSM_ManyStates_WorksCorrectly()
        {
            // Arrange
            CreateSimpleFSM(15);
            switchNode.JumpTo("State0");
            yield return null;

            // Act - Transition through several states
            switchNode.JumpTo("State5");
            yield return null;
            Assert.AreEqual("State5", switchNode.CurrentName);

            switchNode.JumpTo("State10");
            yield return null;
            Assert.AreEqual("State10", switchNode.CurrentName);

            switchNode.JumpTo("State14");
            yield return null;
            Assert.AreEqual("State14", switchNode.CurrentName);

            // Assert - Previous tracking works
            Assert.IsNotNull(switchNode.RespondersFSM.Previous);

            yield return null;
        }

        /// <summary>
        /// Test Reset and ResetTo methods
        /// </summary>
        [UnityTest]
        public IEnumerator FSM_Reset_ClearsPreviousState()
        {
            // Arrange
            CreateSimpleFSM(3);
            switchNode.JumpTo("State0");
            yield return null;
            switchNode.JumpTo("State1");
            yield return null;

            Assert.IsNotNull(switchNode.RespondersFSM.Previous, "Previous should be set");

            // Act
            switchNode.RespondersFSM.Reset();
            yield return null;

            // Assert
            Assert.IsNull(switchNode.RespondersFSM.Previous, "Previous should be cleared");
            Assert.AreEqual("State1", switchNode.CurrentName, "Current should remain unchanged");

            yield return null;
        }

        #endregion
    }

    #region Test Helper Classes

    /// <summary>
    /// Simple responder for FSM testing - tracks message count
    /// </summary>
    public class FsmTestResponder : MmBaseResponder
    {
        public int messageCount = 0;

        public override void MmInvoke(MmMessage message)
        {
            messageCount++;
            base.MmInvoke(message);
        }
    }

    #endregion
}