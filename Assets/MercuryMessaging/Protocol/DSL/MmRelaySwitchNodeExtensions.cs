// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmRelaySwitchNodeExtensions.cs - Fluent DSL extensions for FSM configuration
// Part of DSL Overhaul Phase 2

using System;


namespace MercuryMessaging
{
    /// <summary>
    /// Extension methods for MmRelaySwitchNode to enable fluent FSM configuration.
    /// Provides a clean, chainable API for setting up state machine event handlers.
    ///
    /// Example Usage:
    /// <code>
    /// // Configure state handlers fluently
    /// switchNode.ConfigureStates()
    ///     .OnGlobalEnter(() => Debug.Log("State changed"))
    ///     .OnEnter("MainMenu", () => ShowMenu())
    ///     .OnExit("MainMenu", () => HideMenu())
    ///     .OnEnter("Gameplay", () => StartGame())
    ///     .StartWith("MainMenu")
    ///     .Build();
    ///
    /// // Quick state transitions
    /// switchNode.GoTo("Gameplay");
    /// switchNode.GoToNext();
    /// switchNode.GoToPrevious();
    /// </code>
    /// </summary>
    public static class MmRelaySwitchNodeExtensions
    {
        #region Configuration Builder

        /// <summary>
        /// Start configuring the FSM states with a fluent builder.
        /// </summary>
        /// <param name="switchNode">The switch node to configure.</param>
        /// <returns>A fluent builder for FSM configuration.</returns>
        public static FsmConfigBuilder<MmRoutingTableItem> ConfigureStates(this MmRelaySwitchNode switchNode)
        {
            if (switchNode == null)
                throw new ArgumentNullException(nameof(switchNode));

            if (switchNode.RespondersFSM == null)
                throw new InvalidOperationException("RespondersFSM is not initialized. Ensure Awake() has been called.");

            // Create resolver that looks up states by name in the routing table
            Func<string, MmRoutingTableItem> stateResolver = stateName =>
            {
                try
                {
                    return switchNode.RoutingTable[stateName];
                }
                catch
                {
                    UnityEngine.Debug.LogWarning($"[MmRelaySwitchNode] State '{stateName}' not found in routing table.");
                    return null;
                }
            };

            return new FsmConfigBuilder<MmRoutingTableItem>(switchNode.RespondersFSM, stateResolver);
        }

        #endregion

        #region Quick State Transitions

        /// <summary>
        /// Quick transition to a named state.
        /// Equivalent to switchNode.JumpTo(stateName) but with cleaner naming.
        /// </summary>
        public static void GoTo(this MmRelaySwitchNode switchNode, string stateName)
        {
            switchNode?.JumpTo(stateName);
        }

        /// <summary>
        /// Quick transition to a state by relay node reference.
        /// </summary>
        public static void GoTo(this MmRelaySwitchNode switchNode, MmRelayNode state)
        {
            switchNode?.JumpTo(state);
        }

        /// <summary>
        /// Transition to the previous state (if available).
        /// </summary>
        public static bool GoToPrevious(this MmRelaySwitchNode switchNode)
        {
            if (switchNode?.RespondersFSM?.Previous == null)
                return false;

            var previousState = switchNode.RespondersFSM.Previous;
            if (previousState.Responder != null)
            {
                switchNode.RespondersFSM.JumpTo(previousState);
                return true;
            }
            return false;
        }

        #endregion

        #region State Queries

        /// <summary>
        /// Check if the FSM is currently in a specific state.
        /// </summary>
        public static bool IsInState(this MmRelaySwitchNode switchNode, string stateName)
        {
            if (switchNode?.RespondersFSM?.Current == null)
                return false;

            return switchNode.CurrentName == stateName;
        }

        /// <summary>
        /// Check if the FSM is currently in a specific state.
        /// </summary>
        public static bool IsInState(this MmRelaySwitchNode switchNode, MmRelayNode state)
        {
            if (switchNode?.RespondersFSM?.Current == null || state == null)
                return false;

            return switchNode.Current == state;
        }

        /// <summary>
        /// Get the name of the current state.
        /// </summary>
        public static string GetCurrentStateName(this MmRelaySwitchNode switchNode)
        {
            return switchNode?.CurrentName;
        }

        /// <summary>
        /// Get the name of the previous state.
        /// </summary>
        public static string GetPreviousStateName(this MmRelaySwitchNode switchNode)
        {
            return switchNode?.RespondersFSM?.Previous?.Name;
        }

        #endregion

        #region Event Registration Shortcuts

        /// <summary>
        /// Quick registration of enter handler for a state.
        /// For complex configurations, use ConfigureStates() instead.
        /// </summary>
        public static void OnStateEnter(this MmRelaySwitchNode switchNode, string stateName, Action callback)
        {
            if (switchNode == null) return;

            var stateEvents = switchNode[stateName];
            if (stateEvents != null)
                stateEvents.Enter = callback != null ? new Transition(callback) : null;
        }

        /// <summary>
        /// Quick registration of exit handler for a state.
        /// For complex configurations, use ConfigureStates() instead.
        /// </summary>
        public static void OnStateExit(this MmRelaySwitchNode switchNode, string stateName, Action callback)
        {
            if (switchNode == null) return;

            var stateEvents = switchNode[stateName];
            if (stateEvents != null)
                stateEvents.Exit = callback != null ? new Transition(callback) : null;
        }

        #endregion
    }
}
