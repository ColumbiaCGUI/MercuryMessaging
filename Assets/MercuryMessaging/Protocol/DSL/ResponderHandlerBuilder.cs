// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// ResponderHandlerBuilder.cs - Fluent builder for responder handler configuration
// Part of DSL Overhaul Phase 6

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Fluent builder for configuring message handlers on MmExtendableResponder.
    /// Provides a chainable API for registering both custom and standard method handlers.
    ///
    /// Example Usage:
    /// <code>
    /// responder.ConfigureHandlers()
    ///     .OnCustomMethod(1000, HandleCustomColor)
    ///     .OnCustomMethod(1001, HandleCustomScale)
    ///     .OnInitialize(() => Debug.Log("Initialized"))
    ///     .OnRefresh(() => UpdateDisplay())
    ///     .OnSetActive(active => ToggleEffects(active))
    ///     .Build();
    /// </code>
    /// </summary>
    public class ResponderHandlerBuilder
    {
        private readonly MmExtendableResponder _responder;
        private readonly List<(MmMethod method, Action<MmMessage> handler)> _customHandlers =
            new List<(MmMethod, Action<MmMessage>)>();

        // Standard method callbacks
        private Action _onInitialize;
        private Action<List<MmTransform>> _onRefresh;
        private Action<bool> _onSetActive;
        private Action<string> _onSwitch;
        private Action<bool> _onComplete;
        private Action<MmMessage> _onMessage;
        private Action<MmMessageBool> _onMessageBool;
        private Action<MmMessageInt> _onMessageInt;
        private Action<MmMessageFloat> _onMessageFloat;
        private Action<MmMessageString> _onMessageString;
        private Action<MmMessageVector3> _onMessageVector3;

        /// <summary>
        /// Create a new handler builder for the given responder.
        /// </summary>
        public ResponderHandlerBuilder(MmExtendableResponder responder)
        {
            _responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        #region Custom Method Registration

        /// <summary>
        /// Register a custom method handler (method ID >= 1000).
        /// </summary>
        /// <param name="methodId">The custom method ID (must be >= 1000)</param>
        /// <param name="handler">The handler to invoke for this method</param>
        public ResponderHandlerBuilder OnCustomMethod(int methodId, Action<MmMessage> handler)
        {
            if (methodId < 1000)
                throw new ArgumentException("Custom methods must be >= 1000", nameof(methodId));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _customHandlers.Add(((MmMethod)methodId, handler));
            return this;
        }

        /// <summary>
        /// Register a custom method handler with typed message.
        /// </summary>
        public ResponderHandlerBuilder OnCustomMethod<T>(int methodId, Action<T> handler) where T : MmMessage
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return OnCustomMethod(methodId, msg => handler((T)msg));
        }

        /// <summary>
        /// Register a custom method handler using MmMethod enum.
        /// </summary>
        public ResponderHandlerBuilder OnCustomMethod(MmMethod method, Action<MmMessage> handler)
        {
            return OnCustomMethod((int)method, handler);
        }

        #endregion

        #region Standard Method Registration

        /// <summary>
        /// Set handler for Initialize messages.
        /// </summary>
        public ResponderHandlerBuilder OnInitialize(Action callback)
        {
            _onInitialize = callback;
            return this;
        }

        /// <summary>
        /// Set handler for Refresh messages.
        /// </summary>
        public ResponderHandlerBuilder OnRefresh(Action callback)
        {
            _onRefresh = _ => callback?.Invoke();
            return this;
        }

        /// <summary>
        /// Set handler for Refresh messages with transforms.
        /// </summary>
        public ResponderHandlerBuilder OnRefresh(Action<List<MmTransform>> callback)
        {
            _onRefresh = callback;
            return this;
        }

        /// <summary>
        /// Set handler for SetActive messages.
        /// </summary>
        public ResponderHandlerBuilder OnSetActive(Action<bool> callback)
        {
            _onSetActive = callback;
            return this;
        }

        /// <summary>
        /// Set handler for Switch messages.
        /// </summary>
        public ResponderHandlerBuilder OnSwitch(Action<string> callback)
        {
            _onSwitch = callback;
            return this;
        }

        /// <summary>
        /// Set handler for Complete messages.
        /// </summary>
        public ResponderHandlerBuilder OnComplete(Action<bool> callback)
        {
            _onComplete = callback;
            return this;
        }

        /// <summary>
        /// Set handler for Complete messages (no parameter).
        /// </summary>
        public ResponderHandlerBuilder OnComplete(Action callback)
        {
            _onComplete = _ => callback?.Invoke();
            return this;
        }

        #endregion

        #region Typed Message Registration

        /// <summary>
        /// Set handler for generic messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessage(Action<MmMessage> callback)
        {
            _onMessage = callback;
            return this;
        }

        /// <summary>
        /// Set handler for bool messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessageBool(Action<bool> callback)
        {
            _onMessageBool = msg => callback?.Invoke(msg.value);
            return this;
        }

        /// <summary>
        /// Set handler for int messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessageInt(Action<int> callback)
        {
            _onMessageInt = msg => callback?.Invoke(msg.value);
            return this;
        }

        /// <summary>
        /// Set handler for float messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessageFloat(Action<float> callback)
        {
            _onMessageFloat = msg => callback?.Invoke(msg.value);
            return this;
        }

        /// <summary>
        /// Set handler for string messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessageString(Action<string> callback)
        {
            _onMessageString = msg => callback?.Invoke(msg.value);
            return this;
        }

        /// <summary>
        /// Set handler for Vector3 messages.
        /// </summary>
        public ResponderHandlerBuilder OnMessageVector3(Action<Vector3> callback)
        {
            _onMessageVector3 = msg => callback?.Invoke(msg.value);
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Apply all configured handlers to the responder.
        /// Returns a handler wrapper that can be used for standard method callbacks.
        /// </summary>
        public ResponderHandlerWrapper Build()
        {
            // Register custom handlers using reflection to access protected method
            foreach (var (method, handler) in _customHandlers)
            {
                // Use reflection to call protected RegisterCustomHandler
                var registerMethod = typeof(MmExtendableResponder)
                    .GetMethod("RegisterCustomHandler",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                registerMethod?.Invoke(_responder, new object[] { method, handler });
            }

            // Return wrapper with standard method callbacks
            return new ResponderHandlerWrapper(
                _responder,
                _onInitialize,
                _onRefresh,
                _onSetActive,
                _onSwitch,
                _onComplete,
                _onMessage,
                _onMessageBool,
                _onMessageInt,
                _onMessageFloat,
                _onMessageString,
                _onMessageVector3
            );
        }

        #endregion
    }

    /// <summary>
    /// Wrapper that holds configured handlers for standard methods.
    /// Subclasses can use this to access configured callbacks.
    /// </summary>
    public class ResponderHandlerWrapper
    {
        public MmExtendableResponder Responder { get; }
        public Action OnInitializeCallback { get; }
        public Action<List<MmTransform>> OnRefreshCallback { get; }
        public Action<bool> OnSetActiveCallback { get; }
        public Action<string> OnSwitchCallback { get; }
        public Action<bool> OnCompleteCallback { get; }
        public Action<MmMessage> OnMessageCallback { get; }
        public Action<MmMessageBool> OnMessageBoolCallback { get; }
        public Action<MmMessageInt> OnMessageIntCallback { get; }
        public Action<MmMessageFloat> OnMessageFloatCallback { get; }
        public Action<MmMessageString> OnMessageStringCallback { get; }
        public Action<MmMessageVector3> OnMessageVector3Callback { get; }

        public ResponderHandlerWrapper(
            MmExtendableResponder responder,
            Action onInitialize,
            Action<List<MmTransform>> onRefresh,
            Action<bool> onSetActive,
            Action<string> onSwitch,
            Action<bool> onComplete,
            Action<MmMessage> onMessage,
            Action<MmMessageBool> onMessageBool,
            Action<MmMessageInt> onMessageInt,
            Action<MmMessageFloat> onMessageFloat,
            Action<MmMessageString> onMessageString,
            Action<MmMessageVector3> onMessageVector3)
        {
            Responder = responder;
            OnInitializeCallback = onInitialize;
            OnRefreshCallback = onRefresh;
            OnSetActiveCallback = onSetActive;
            OnSwitchCallback = onSwitch;
            OnCompleteCallback = onComplete;
            OnMessageCallback = onMessage;
            OnMessageBoolCallback = onMessageBool;
            OnMessageIntCallback = onMessageInt;
            OnMessageFloatCallback = onMessageFloat;
            OnMessageStringCallback = onMessageString;
            OnMessageVector3Callback = onMessageVector3;
        }

        /// <summary>
        /// Check if any standard method callbacks are configured.
        /// </summary>
        public bool HasStandardCallbacks =>
            OnInitializeCallback != null ||
            OnRefreshCallback != null ||
            OnSetActiveCallback != null ||
            OnSwitchCallback != null ||
            OnCompleteCallback != null ||
            OnMessageCallback != null ||
            OnMessageBoolCallback != null ||
            OnMessageIntCallback != null ||
            OnMessageFloatCallback != null ||
            OnMessageStringCallback != null ||
            OnMessageVector3Callback != null;
    }
}
