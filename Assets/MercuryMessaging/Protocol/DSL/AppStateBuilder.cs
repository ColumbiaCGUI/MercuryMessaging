// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// AppStateBuilder.cs - Fluent builder for application state configuration
// Part of DSL Overhaul Phase 8

using System;
using System.Collections.Generic;
using MercuryMessaging.AppState;
using MercuryMessaging.Support.FiniteStateMachine;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Fluent builder for configuring application states.
    /// Works with MmRelaySwitchNode.
    ///
    /// Example Usage:
    /// <code>
    /// // Configure application states
    /// MmAppState.Configure(switchNode)
    ///     .DefineState("MainMenu")
    ///         .OnEnter(() => ShowMenu())
    ///         .OnExit(() => HideMenu())
    ///     .DefineState("Gameplay")
    ///         .OnEnter(() => StartGame())
    ///         .OnExit(() => SaveProgress())
    ///     .DefineState("Pause")
    ///         .OnEnter(() => PauseGame())
    ///         .OnExit(() => ResumeGame())
    ///     .StartWith("MainMenu")
    ///     .Build();
    ///
    /// // Transitions
    /// switchNode.GoTo("Gameplay");
    /// switchNode.GoBack();
    /// </code>
    /// </summary>
    public class AppStateBuilder
    {
        private readonly MmRelaySwitchNode _switchNode;
        private readonly Dictionary<string, StateConfig> _stateConfigs = new Dictionary<string, StateConfig>();
        private string _currentStateName;
        private string _initialState;
        private Action _onGlobalEnter;
        private Action _onGlobalExit;

        /// <summary>
        /// Create an app state builder for a switch node.
        /// </summary>
        public AppStateBuilder(MmRelaySwitchNode switchNode)
        {
            _switchNode = switchNode ?? throw new ArgumentNullException(nameof(switchNode));
        }

        #region State Definition

        /// <summary>
        /// Define or configure a state.
        /// </summary>
        public AppStateBuilder DefineState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                throw new ArgumentException("State name cannot be empty", nameof(stateName));

            _currentStateName = stateName;

            if (!_stateConfigs.ContainsKey(stateName))
            {
                _stateConfigs[stateName] = new StateConfig(stateName);
            }

            return this;
        }

        /// <summary>
        /// Set the entry callback for the current state.
        /// </summary>
        public AppStateBuilder OnEnter(Action callback)
        {
            EnsureCurrentState();
            _stateConfigs[_currentStateName].OnEnter = callback;
            return this;
        }

        /// <summary>
        /// Set the exit callback for the current state.
        /// </summary>
        public AppStateBuilder OnExit(Action callback)
        {
            EnsureCurrentState();
            _stateConfigs[_currentStateName].OnExit = callback;
            return this;
        }

        /// <summary>
        /// Set both enter and exit callbacks for the current state.
        /// </summary>
        public AppStateBuilder OnTransition(Action onEnter, Action onExit)
        {
            EnsureCurrentState();
            _stateConfigs[_currentStateName].OnEnter = onEnter;
            _stateConfigs[_currentStateName].OnExit = onExit;
            return this;
        }

        /// <summary>
        /// Define allowed transitions from current state.
        /// </summary>
        public AppStateBuilder CanTransitionTo(params string[] targetStates)
        {
            EnsureCurrentState();
            foreach (var target in targetStates)
            {
                _stateConfigs[_currentStateName].AllowedTransitions.Add(target);
            }
            return this;
        }

        #endregion

        #region Global Configuration

        /// <summary>
        /// Set the initial state to start with.
        /// </summary>
        public AppStateBuilder StartWith(string stateName)
        {
            _initialState = stateName;
            return this;
        }

        /// <summary>
        /// Set a callback for any state enter (global enter).
        /// </summary>
        public AppStateBuilder OnAnyStateEnter(Action callback)
        {
            _onGlobalEnter = callback;
            return this;
        }

        /// <summary>
        /// Set a callback for any state exit (global exit).
        /// </summary>
        public AppStateBuilder OnAnyStateExit(Action callback)
        {
            _onGlobalExit = callback;
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Build and apply the state configuration.
        /// </summary>
        public MmRelaySwitchNode Build()
        {
            var fsm = _switchNode.RespondersFSM;
            if (fsm == null)
            {
                Debug.LogWarning("[AppStateBuilder] RespondersFSM not initialized. Make sure switch node has been Awake'd.");
                return _switchNode;
            }

            // Register state callbacks using the indexer
            foreach (var kvp in _stateConfigs)
            {
                var stateName = kvp.Key;
                var config = kvp.Value;

                // Get the routing table item for this state
                var routingItem = GetRoutingTableItem(stateName);
                if (routingItem == null)
                {
                    Debug.LogWarning($"[AppStateBuilder] State '{stateName}' not found in routing table");
                    continue;
                }

                // Get or create state events
                var stateEvents = fsm[routingItem];
                if (stateEvents == null)
                {
                    stateEvents = new StateEvents();
                    fsm[routingItem] = stateEvents;
                }

                // Register enter callback
                if (config.OnEnter != null)
                {
                    var existingEnter = stateEvents.Enter;
                    stateEvents.Enter = () =>
                    {
                        existingEnter?.Invoke();
                        config.OnEnter();
                    };
                }

                // Register exit callback
                if (config.OnExit != null)
                {
                    var existingExit = stateEvents.Exit;
                    stateEvents.Exit = () =>
                    {
                        config.OnExit();
                        existingExit?.Invoke();
                    };
                }
            }

            // Register global callbacks
            if (_onGlobalEnter != null)
            {
                var existing = fsm.GlobalEnter;
                fsm.GlobalEnter = () =>
                {
                    existing?.Invoke();
                    _onGlobalEnter();
                };
            }

            if (_onGlobalExit != null)
            {
                var existing = fsm.GlobalExit;
                fsm.GlobalExit = () =>
                {
                    _onGlobalExit();
                    existing?.Invoke();
                };
            }

            // Set initial state
            if (!string.IsNullOrEmpty(_initialState))
            {
                _switchNode.JumpTo(_initialState);
            }

            return _switchNode;
        }

        private MmRoutingTableItem GetRoutingTableItem(string stateName)
        {
            try
            {
                return _switchNode.RoutingTable[stateName];
            }
            catch
            {
                return null;
            }
        }

        #endregion

        private void EnsureCurrentState()
        {
            if (string.IsNullOrEmpty(_currentStateName))
                throw new InvalidOperationException("No current state defined. Call DefineState() first.");
        }

        /// <summary>
        /// Internal state configuration.
        /// </summary>
        private class StateConfig
        {
            public string Name { get; }
            public Action OnEnter { get; set; }
            public Action OnExit { get; set; }
            public HashSet<string> AllowedTransitions { get; } = new HashSet<string>();

            public StateConfig(string name)
            {
                Name = name;
            }
        }
    }

    /// <summary>
    /// Static factory for app state configuration.
    /// </summary>
    public static class MmAppState
    {
        /// <summary>
        /// Start configuring app states for a switch node.
        /// </summary>
        public static AppStateBuilder Configure(MmRelaySwitchNode switchNode)
        {
            return new AppStateBuilder(switchNode);
        }
    }

    // NOTE: Extension methods for MmRelaySwitchNode (GoTo, GoBack, IsInState, ConfigureStates, etc.)
    // are defined in MmRelaySwitchNodeExtensions.cs (Phase 2: FSM Configuration DSL)
    // to avoid duplication and ambiguity.
}
