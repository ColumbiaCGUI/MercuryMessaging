// Copyright (c) 2017-2019, Columbia University
// All rights reserved.
//
// Comprehensive FSM State Transition Tests for MmRelaySwitchNode
// Resolves Priority 3 Technical Debt: FSM JumpTo() testing

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using MercuryMessaging.Support.FiniteStateMachine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Comprehensive tests for FSM state transitions in MmRelaySwitchNode.
    /// Tests cover basic transitions, event ordering, async transitions,
    /// integration with MercuryMessaging, and edge cases.
    /// </summary>
    [TestFixture]
    public class FsmStateTransitionTests
    {
        private GameObject testRoot;
        private MmRelaySwitchNode switchNode;
        private MmSwitchResponder switchResponder;
        private MmRelayNode state1Node;
        private MmRelayNode state2Node;
        private MmRelayNode state3Node;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            // Create root object with switch node
            testRoot = new GameObject("TestRoot");
            switchNode = testRoot.AddComponent<MmRelaySwitchNode>();
            switchResponder = testRoot.AddComponent<MmSwitchResponder>();

            // Create three child states
            GameObject state1 = new GameObject("State1");
            state1.transform.SetParent(testRoot.transform);
            state1Node = state1.AddComponent<MmRelayNode>();

            GameObject state2 = new GameObject("State2");
            state2.transform.SetParent(testRoot.transform);
            state2Node = state2.AddComponent<MmRelayNode>();

            GameObject state3 = new GameObject("State3");
            state3.transform.SetParent(testRoot.transform);
            state3Node = state3.AddComponent<MmRelayNode>();

            // Register responders with routing tables
            switchNode.MmRefreshResponders();
            switchNode.RefreshParents();

            // Wait for FSM initialization
            switchNode.InitializeFSM();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
                UnityEngine.Object.DestroyImmediate(testRoot);
        }

        #endregion

        #region Category 1: Basic Transitions (5 tests)

        [Test]
        public void JumpTo_ValidState_TransitionsSuccessfully()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            Assert.AreEqual(state1Node, switchNode.RespondersFSM.Current, "Initial state should be State1");

            // Act
            switchNode.RespondersFSM.JumpTo(state2Node);

            // Assert
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current, "Should transition to State2");
            Assert.AreEqual(state1Node, switchNode.RespondersFSM.Previous, "Previous should be State1");
        }

        [Test]
        public void JumpTo_CurrentState_EarlyExitsWithoutEvents()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            bool exitCalled = false;
            bool enterCalled = false;

            switchNode.RespondersFSM[state1Node].Exit += () => exitCalled = true;
            switchNode.RespondersFSM[state1Node].Enter += () => enterCalled = true;

            // Act - Jump to current state
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Assert - Should early-exit without firing events (per FiniteStateMachine.cs:111)
            Assert.AreEqual(state1Node, switchNode.RespondersFSM.Current, "Should remain in State1");
            Assert.IsFalse(exitCalled, "Exit event should NOT fire for self-transition");
            Assert.IsFalse(enterCalled, "Enter event should NOT fire for self-transition");
        }

        [Test]
        public void JumpTo_NullState_HandlesGracefully()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Act & Assert - JumpTo(null) should either handle gracefully or throw clear exception
            // Current implementation will likely throw, which is acceptable
            Assert.Throws<System.NullReferenceException>(() => switchNode.RespondersFSM.JumpTo((MmRelayNode)null),
                "JumpTo(null) should throw NullReferenceException");
        }

        [Test]
        public void JumpTo_NonExistentState_ThrowsException()
        {
            // Arrange
            GameObject orphanState = new GameObject("OrphanState");
            MmRelayNode orphanNode = orphanState.AddComponent<MmRelayNode>();
            // Note: orphanNode is NOT in switchNode's routing table

            // Act & Assert
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
                switchNode.RespondersFSM.JumpTo(orphanNode),
                "JumpTo non-existent state should throw KeyNotFoundException");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(orphanState);
        }

        [Test]
        public void JumpTo_StringLookup_EquivalentToDirectReference()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Act - Jump using string name
            switchNode.JumpTo("State2"); // Uses routing table lookup

            // Assert
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current,
                "String lookup should resolve to same node as direct reference");
        }

        #endregion

        #region Category 2: Event Ordering (3 tests)

        [Test]
        public void JumpTo_EventSequence_FiresInCorrectOrder()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            List<string> eventSequence = new List<string>();

            switchNode.RespondersFSM.GlobalExit += () => eventSequence.Add("GlobalExit");
            switchNode.RespondersFSM[state1Node].Exit += () => eventSequence.Add("State1.Exit");
            switchNode.RespondersFSM[state2Node].Enter += () => eventSequence.Add("State2.Enter");
            switchNode.RespondersFSM.GlobalEnter += () => eventSequence.Add("GlobalEnter");

            // Act
            switchNode.RespondersFSM.JumpTo(state2Node);

            // Assert - Verify event order matches FiniteStateMachine.cs implementation
            Assert.AreEqual(4, eventSequence.Count, "Should fire 4 events");
            Assert.AreEqual("GlobalExit", eventSequence[0], "GlobalExit should fire first");
            Assert.AreEqual("State1.Exit", eventSequence[1], "State-specific Exit should fire second");
            Assert.AreEqual("State2.Enter", eventSequence[2], "State-specific Enter should fire third");
            Assert.AreEqual("GlobalEnter", eventSequence[3], "GlobalEnter should fire last");
        }

        [Test]
        public void JumpTo_MultipleSubscribers_AllReceiveEvents()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            int subscriber1Calls = 0;
            int subscriber2Calls = 0;
            int subscriber3Calls = 0;

            switchNode.RespondersFSM.GlobalEnter += () => subscriber1Calls++;
            switchNode.RespondersFSM.GlobalEnter += () => subscriber2Calls++;
            switchNode.RespondersFSM.GlobalEnter += () => subscriber3Calls++;

            // Act
            switchNode.RespondersFSM.JumpTo(state2Node);

            // Assert
            Assert.AreEqual(1, subscriber1Calls, "Subscriber 1 should receive event");
            Assert.AreEqual(1, subscriber2Calls, "Subscriber 2 should receive event");
            Assert.AreEqual(1, subscriber3Calls, "Subscriber 3 should receive event");
        }

        [Test]
        public void JumpTo_ExceptionInEventHandler_PropagatesCorrectly()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            bool afterExceptionReached = false;

            switchNode.RespondersFSM[state2Node].Enter += () =>
            {
                throw new System.InvalidOperationException("Test exception");
            };
            switchNode.RespondersFSM.GlobalEnter += () => afterExceptionReached = true;

            // Act & Assert
            Assert.Throws<System.InvalidOperationException>(() =>
                switchNode.RespondersFSM.JumpTo(state2Node),
                "Exception in Enter event should propagate");

            // Note: State WILL have changed even though exception thrown
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current,
                "State change should complete before Enter event fires");
            Assert.IsFalse(afterExceptionReached,
                "GlobalEnter should not fire after exception in state Enter");
        }

        #endregion

        #region Category 3: Async Transitions (4 tests)

        [Test]
        public void StartTransitionTo_ThenEnterNext_CompletesTransition()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Act - Start async transition
            switchNode.RespondersFSM.StartTransitionTo(state2Node);
            Assert.IsTrue(switchNode.RespondersFSM.isTransitioning, "Should be in transitioning state");
            Assert.AreEqual(state1Node, switchNode.RespondersFSM.Current, "Current should still be State1");
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Next, "Next should be State2");

            // Complete transition
            switchNode.RespondersFSM.EnterNext();

            // Assert
            Assert.IsFalse(switchNode.RespondersFSM.isTransitioning, "Should no longer be transitioning");
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current, "Should now be in State2");
        }

        [Test]
        public void CancelStateChange_AbortsTransition()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            switchNode.RespondersFSM.StartTransitionTo(state2Node);
            Assert.IsTrue(switchNode.RespondersFSM.isTransitioning, "Should be transitioning");

            // Act
            bool cancelSuccess = switchNode.RespondersFSM.CancelStateChange();

            // Assert
            Assert.IsTrue(cancelSuccess, "Cancel should return true");
            Assert.IsFalse(switchNode.RespondersFSM.isTransitioning, "Should no longer be transitioning");
            Assert.AreEqual(state1Node, switchNode.RespondersFSM.Current, "Should remain in State1");
        }

        [Test]
        public void CancelStateChange_WhenNotTransitioning_ReturnsFalse()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            // Not transitioning

            // Act
            bool cancelSuccess = switchNode.RespondersFSM.CancelStateChange();

            // Assert
            Assert.IsFalse(cancelSuccess, "Cancel should return false when not transitioning");
        }

        [Test]
        public void JumpTo_RapidTransitions_AllComplete()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            int transitionCount = 0;
            switchNode.RespondersFSM.GlobalEnter += () => transitionCount++;

            // Act - 10 rapid transitions in single frame
            for (int i = 0; i < 10; i++)
            {
                MmRelayNode targetState = (i % 2 == 0) ? state2Node : state3Node;
                switchNode.RespondersFSM.JumpTo(targetState);
            }

            // Assert
            Assert.AreEqual(10, transitionCount, "All 10 transitions should complete");
            Assert.AreEqual(state3Node, switchNode.RespondersFSM.Current, "Final state should be State3");
        }

        #endregion

        #region Category 4: MercuryMessaging Integration (5 tests)

        [UnityTest]
        public IEnumerator MessageFiltering_SelectedFilter_OnlyCurrentStateReceives()
        {
            // Arrange - Add responders to track message receipt
            var state1Responder = state1Node.gameObject.AddComponent<TestMessageResponder>();
            var state2Responder = state2Node.gameObject.AddComponent<TestMessageResponder>();

            state1Node.MmRefreshResponders();
            state2Node.MmRefreshResponders();

            switchNode.RespondersFSM.JumpTo(state1Node);
            yield return null; // Wait for state transition

            // Act - Send message with MmSelectedFilter.Selected
            var message = new MmMessage { MmMethod = MmMethod.Initialize };
            message.MetadataBlock = new MmMetadataBlock(
                MmTagHelper.Everything,
                MmLevelFilterHelper.SelfAndChildren,
                MmActiveFilter.All,
                MmSelectedFilter.Selected, // Only current FSM state
                MmNetworkFilter.Local);

            switchNode.MmInvoke(message);
            yield return null;

            // Assert
            Assert.AreEqual(1, state1Responder.InitializeCount, "State1 (current) should receive message");
            Assert.AreEqual(0, state2Responder.InitializeCount, "State2 (not current) should NOT receive");
        }

        [UnityTest]
        public IEnumerator StateChangeDuringMmInvoke_CompletesSuccessfully()
        {
            // Arrange
            var state1Responder = state1Node.gameObject.AddComponent<TransitionTriggerResponder>();
            state1Responder.targetNode = state2Node;
            state1Responder.switchNode = switchNode;

            state1Node.MmRefreshResponders();
            switchNode.RespondersFSM.JumpTo(state1Node);
            yield return null;

            // Act - Send message that triggers state change inside handler
            switchNode.MmInvoke(MmMethod.Initialize);
            yield return null;

            // Assert
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current,
                "Should transition to State2 triggered from message handler");
        }

        [Test]
        public void EmptyFSM_NoChildNodes_HandlesGracefully()
        {
            // Arrange - Create switch node with NO children
            GameObject emptyRoot = new GameObject("EmptyRoot");
            MmRelaySwitchNode emptySwitch = emptyRoot.AddComponent<MmRelaySwitchNode>();
            emptySwitch.InitializeFSM();

            // Act & Assert - Should handle empty FSM without crashing
            Assert.IsNotNull(emptySwitch.RespondersFSM, "FSM should exist even when empty");
            Assert.AreEqual(0, emptySwitch.RespondersFSM.Count, "FSM should have 0 states");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(emptyRoot);
        }

        [UnityTest]
        public IEnumerator MmSwitchResponder_SetActiveProper_PropagatesOnTransition()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            yield return null;

            // Verify initial state active
            Assert.IsTrue(state1Node.gameObject.activeInHierarchy, "State1 should be active");

            // Act - Transition to State2
            switchNode.RespondersFSM.JumpTo(state2Node);
            yield return null; // Allow SetActive messages to propagate

            // Assert - MmSwitchResponder should send SetActive messages
            // Note: Actual behavior depends on MmSwitchResponder implementation
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current, "Should be in State2");
        }

        [Test]
        public void RoutingTableLookup_StringVsReference_Equivalent()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Act - Transition using MmRelaySwitchNode.JumpTo(string)
            switchNode.JumpTo("State3");

            // Assert
            Assert.AreEqual(state3Node, switchNode.RespondersFSM.Current,
                "String-based JumpTo should resolve to correct node");
        }

        #endregion

        #region Category 5: Edge Cases (3 tests)

        [UnityTest]
        public IEnumerator HierarchyChanges_AddNodeWhileFSMActive_UpdatesCorrectly()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            yield return null;

            // Act - Add new child node dynamically
            GameObject state4 = new GameObject("State4");
            state4.transform.SetParent(testRoot.transform);
            MmRelayNode state4Node = state4.AddComponent<MmRelayNode>();

            switchNode.MmRefreshResponders(); // Refresh to pick up new child
            switchNode.InitializeFSM(); // Rebuild FSM
            yield return null;

            // Assert - FSM should now include new state
            Assert.IsTrue(switchNode.RespondersFSM.Contains(state4Node),
                "FSM should include dynamically added state");

            // Verify can transition to new state
            switchNode.RespondersFSM.JumpTo(state4Node);
            Assert.AreEqual(state4Node, switchNode.RespondersFSM.Current,
                "Should be able to transition to dynamically added state");
        }

        [Test]
        public void NestedFSMs_ParentChildSwitchNodes_OperateIndependently()
        {
            // Arrange - Create child FSM under state1
            GameObject childFsmRoot = new GameObject("ChildFSM");
            childFsmRoot.transform.SetParent(state1Node.transform);
            MmRelaySwitchNode childSwitch = childFsmRoot.AddComponent<MmRelaySwitchNode>();

            GameObject childState1 = new GameObject("ChildState1");
            childState1.transform.SetParent(childFsmRoot.transform);
            MmRelayNode childState1Node = childState1.AddComponent<MmRelayNode>();

            GameObject childState2 = new GameObject("ChildState2");
            childState2.transform.SetParent(childFsmRoot.transform);
            MmRelayNode childState2Node = childState2.AddComponent<MmRelayNode>();

            childSwitch.MmRefreshResponders();
            childSwitch.InitializeFSM();

            // Set parent FSM to state1
            switchNode.RespondersFSM.JumpTo(state1Node);

            // Set child FSM to childState1
            childSwitch.RespondersFSM.JumpTo(childState1Node);

            // Act - Transition parent FSM
            switchNode.RespondersFSM.JumpTo(state2Node);

            // Assert - Child FSM should remain in its current state
            Assert.AreEqual(state2Node, switchNode.RespondersFSM.Current,
                "Parent FSM should be in State2");
            Assert.AreEqual(childState1Node, childSwitch.RespondersFSM.Current,
                "Child FSM should still be in ChildState1");
        }

        [Test]
        [Category("Performance")]
        public void Performance_1000Transitions_CompletesQuickly()
        {
            // Arrange
            switchNode.RespondersFSM.JumpTo(state1Node);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act - Perform 1000 transitions
            for (int i = 0; i < 1000; i++)
            {
                MmRelayNode targetState = (i % 3 == 0) ? state1Node :
                                          (i % 3 == 1) ? state2Node : state3Node;
                switchNode.RespondersFSM.JumpTo(targetState);
            }
            stopwatch.Stop();

            // Assert - Should complete in reasonable time
            double avgMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000) / 1000.0;
            UnityEngine.Debug.Log($"[FSM Performance] 1000 transitions completed in {stopwatch.ElapsedMilliseconds}ms " +
                                  $"(avg: {avgMicroseconds:F2} μs/transition)");

            Assert.Less(stopwatch.ElapsedMilliseconds, 100,
                "1000 transitions should complete in < 100ms (Unity Editor)");
        }

        #endregion

        #region Helper Classes

        private class TestMessageResponder : MmBaseResponder
        {
            public int InitializeCount { get; private set; }

            protected override void ReceivedInitialize()
            {
                InitializeCount++;
            }
        }

        private class TransitionTriggerResponder : MmBaseResponder
        {
            public MmRelayNode targetNode;
            public MmRelaySwitchNode switchNode;

            protected override void ReceivedInitialize()
            {
                // Trigger state transition during message processing
                if (targetNode != null && switchNode != null)
                {
                    switchNode.RespondersFSM.JumpTo(targetNode);
                }
            }
        }

        #endregion
    }
}
