// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// FsmConfigBuilder.cs - Fluent builder for FSM configuration
// Part of DSL Overhaul Phase 2

using System;
using System.Collections.Generic;

namespace MercuryMessaging
{
    /// <summary>
    /// Fluent builder for configuring FiniteStateMachine event handlers.
    /// Provides a clean, chainable API for setting up state enter/exit callbacks.
    ///
    /// Example Usage:
    /// <code>
    /// switchNode.ConfigureStates()
    ///     .OnGlobalEnter(() => Debug.Log("Any state entered"))
    ///     .OnGlobalExit(() => Debug.Log("Any state exited"))
    ///     .OnEnter("MainMenu", () => RefreshMenuUI())
    ///     .OnExit("MainMenu", () => CleanupMenu())
    ///     .OnEnter("Gameplay", () => StartGame())
    ///     .Build();
    /// </code>
    /// </summary>
    /// <typeparam name="T">The state type (typically MmRoutingTableItem for MmRelaySwitchNode)</typeparam>
    public class FsmConfigBuilder<T>
    {
        private readonly FiniteStateMachine<T> _fsm;
        private readonly Func<string, T> _stateResolver;
        private readonly List<Action> _deferredActions = new List<Action>();

        private FiniteStateMachine<T>.Transition _globalEnter;
        private FiniteStateMachine<T>.Transition _globalExit;

        /// <summary>
        /// Create a new FSM configuration builder.
        /// </summary>
        /// <param name="fsm">The FSM to configure.</param>
        /// <param name="stateResolver">Function to resolve state name to state object.</param>
        public FsmConfigBuilder(FiniteStateMachine<T> fsm, Func<string, T> stateResolver = null)
        {
            _fsm = fsm ?? throw new ArgumentNullException(nameof(fsm));
            _stateResolver = stateResolver;
        }

        #region Global Events

        /// <summary>
        /// Set the global enter callback (called when ANY state is entered).
        /// </summary>
        public FsmConfigBuilder<T> OnGlobalEnter(Action callback)
        {
            _globalEnter = callback != null ? new FiniteStateMachine<T>.Transition(callback) : null;
            return this;
        }

        /// <summary>
        /// Set the global exit callback (called when ANY state is exited).
        /// </summary>
        public FsmConfigBuilder<T> OnGlobalExit(Action callback)
        {
            _globalExit = callback != null ? new FiniteStateMachine<T>.Transition(callback) : null;
            return this;
        }

        #endregion

        #region State-Specific Events (by state object)

        /// <summary>
        /// Set the enter callback for a specific state.
        /// </summary>
        public FsmConfigBuilder<T> OnEnter(T state, Action callback)
        {
            _deferredActions.Add(() =>
            {
                if (_fsm[state] != null)
                    _fsm[state].Enter = callback != null ? new Transition(callback) : null;
            });
            return this;
        }

        /// <summary>
        /// Set the exit callback for a specific state.
        /// </summary>
        public FsmConfigBuilder<T> OnExit(T state, Action callback)
        {
            _deferredActions.Add(() =>
            {
                if (_fsm[state] != null)
                    _fsm[state].Exit = callback != null ? new Transition(callback) : null;
            });
            return this;
        }

        /// <summary>
        /// Set both enter and exit callbacks for a specific state.
        /// </summary>
        public FsmConfigBuilder<T> On(T state, Action onEnter, Action onExit)
        {
            OnEnter(state, onEnter);
            OnExit(state, onExit);
            return this;
        }

        #endregion

        #region State-Specific Events (by name)

        /// <summary>
        /// Set the enter callback for a state by name.
        /// Requires a state resolver to be set.
        /// </summary>
        public FsmConfigBuilder<T> OnEnter(string stateName, Action callback)
        {
            if (_stateResolver == null)
                throw new InvalidOperationException("State resolver not set. Use OnEnter(T state, Action) instead.");

            _deferredActions.Add(() =>
            {
                var state = _stateResolver(stateName);
                if (state != null && _fsm[state] != null)
                    _fsm[state].Enter = callback != null ? new Transition(callback) : null;
            });
            return this;
        }

        /// <summary>
        /// Set the exit callback for a state by name.
        /// Requires a state resolver to be set.
        /// </summary>
        public FsmConfigBuilder<T> OnExit(string stateName, Action callback)
        {
            if (_stateResolver == null)
                throw new InvalidOperationException("State resolver not set. Use OnExit(T state, Action) instead.");

            _deferredActions.Add(() =>
            {
                var state = _stateResolver(stateName);
                if (state != null && _fsm[state] != null)
                    _fsm[state].Exit = callback != null ? new Transition(callback) : null;
            });
            return this;
        }

        /// <summary>
        /// Set both enter and exit callbacks for a state by name.
        /// </summary>
        public FsmConfigBuilder<T> On(string stateName, Action onEnter, Action onExit)
        {
            OnEnter(stateName, onEnter);
            OnExit(stateName, onExit);
            return this;
        }

        #endregion

        #region Initial State

        /// <summary>
        /// Set the initial state and jump to it.
        /// </summary>
        public FsmConfigBuilder<T> StartWith(T initialState)
        {
            _deferredActions.Add(() => _fsm.JumpTo(initialState));
            return this;
        }

        /// <summary>
        /// Set the initial state by name and jump to it.
        /// </summary>
        public FsmConfigBuilder<T> StartWith(string stateName)
        {
            if (_stateResolver == null)
                throw new InvalidOperationException("State resolver not set. Use StartWith(T state) instead.");

            _deferredActions.Add(() =>
            {
                var state = _stateResolver(stateName);
                if (state != null)
                    _fsm.JumpTo(state);
            });
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Apply all configured settings to the FSM.
        /// </summary>
        public void Build()
        {
            // Apply global handlers
            if (_globalEnter != null)
                _fsm.GlobalEnter = _globalEnter;
            if (_globalExit != null)
                _fsm.GlobalExit = _globalExit;

            // Execute all deferred actions (state-specific handlers)
            foreach (var action in _deferredActions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[FsmConfigBuilder] Error applying configuration: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
