// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//  * Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//  * Neither the name of Columbia University nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
// =============================================================
// Authors:
// Ben Yang, Carmine Elvezio, Mengu Sukan, Samuel Silverman, Steven Feiner
// =============================================================
//
//
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging
{
    /// <summary>
    /// Base responder with extensible custom method handling via registration API.
    /// Provides hybrid fast/slow path: standard methods (0-999) use fast switch,
    /// custom methods (1000+) use dictionary lookup with handler registration.
    ///
    /// This class eliminates boilerplate switch statements for custom methods and
    /// prevents common errors like forgetting to call base.MmInvoke().
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Performance Characteristics:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Standard methods (0-999): ~100-150ns (same as MmBaseResponder)</item>
    /// <item>Custom methods (1000+): ~300-500ns (dictionary lookup overhead)</item>
    /// <item>Memory overhead: ~8 bytes (null reference) until first handler registered</item>
    /// <item>Memory overhead: ~320 bytes with 3 custom handlers</item>
    /// </list>
    ///
    /// <para>
    /// <b>Usage Guidelines:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>Use this class when implementing custom methods (>= 1000)</item>
    /// <item>Register handlers in Awake() for best performance</item>
    /// <item>Use MmBaseResponder if only using standard methods (0-18)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyCustomResponder : MmExtendableResponder
    /// {
    ///     protected override void Awake()
    ///     {
    ///         base.Awake();
    ///
    ///         // Register custom method handlers
    ///         RegisterCustomHandler((MmMethod)1000, OnCustomColor);
    ///         RegisterCustomHandler((MmMethod)1001, OnCustomScale);
    ///     }
    ///
    ///     private void OnCustomColor(MmMessage msg)
    ///     {
    ///         var colorMsg = (ColorMessage)msg;
    ///         GetComponent&lt;Renderer&gt;().material.color = colorMsg.color;
    ///     }
    ///
    ///     private void OnCustomScale(MmMessage msg)
    ///     {
    ///         var scaleMsg = (ScaleMessage)msg;
    ///         transform.localScale = scaleMsg.scale;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class MmExtendableResponder : MmBaseResponder
    {
        /// <summary>
        /// Dictionary storing custom method handlers.
        /// Lazy-initialized on first RegisterCustomHandler() call to minimize memory overhead.
        /// </summary>
        private Dictionary<MmMethod, Action<MmMessage>> customHandlers;

        /// <summary>
        /// Hybrid fast/slow path message dispatch.
        /// Standard methods (0-999) are routed via base.MmInvoke() switch statement for maximum performance.
        /// Custom methods (1000+) are routed via dictionary lookup for flexibility and clean code.
        /// </summary>
        /// <param name="message">The message to process. <see cref="MmMessage"/></param>
        /// <remarks>
        /// <para>
        /// This implementation provides optimal performance for both standard and custom methods:
        /// </para>
        /// <list type="bullet">
        /// <item>Fast path: Standard methods execute with ~100-150ns latency via switch statement</item>
        /// <item>Slow path: Custom methods execute with ~300-500ns latency via dictionary lookup</item>
        /// <item>Handler exceptions are caught and logged without disrupting other handlers</item>
        /// <item>Unhandled custom methods invoke OnUnhandledCustomMethod() for custom error handling</item>
        /// </list>
        /// </remarks>
        public override void MmInvoke(MmMessage message)
        {
            int methodValue = (int)message.MmMethod;

            // FAST PATH: Standard framework methods (0-999)
            // Route through MmBaseResponder's optimized switch statement
            if (methodValue < 1000)
            {
                base.MmInvoke(message);
                return;
            }

            // SLOW PATH: Custom methods (1000+)
            // Route through registered handler dictionary
            if (customHandlers != null &&
                customHandlers.TryGetValue(message.MmMethod, out var handler))
            {
                try
                {
                    handler(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[{GetType().Name}] Error in custom handler for method {message.MmMethod} ({methodValue}): {ex}\n" +
                        $"GameObject: {gameObject.name}\n" +
                        $"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                OnUnhandledCustomMethod(message);
            }
        }

        /// <summary>
        /// Register a handler for a custom method (>= 1000).
        /// Call this in Awake() to set up custom message handling.
        /// </summary>
        /// <param name="method">Custom method enum (must be >= 1000). Values 0-999 are reserved for framework methods.</param>
        /// <param name="handler">Handler function to invoke for this method. Receives the full MmMessage for type casting.</param>
        /// <exception cref="ArgumentException">Thrown if method value is less than 1000</exception>
        /// <exception cref="ArgumentNullException">Thrown if handler is null</exception>
        /// <remarks>
        /// <para>
        /// Registration best practices:
        /// </para>
        /// <list type="bullet">
        /// <item>Register handlers in Awake() for optimal performance</item>
        /// <item>Use method values >= 1000 (values 0-999 reserved for framework)</item>
        /// <item>Duplicate registration overwrites the existing handler</item>
        /// <item>Keep handler methods private for encapsulation</item>
        /// <item>Type cast the message parameter inside the handler as needed</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// protected override void Awake()
        /// {
        ///     base.Awake();
        ///     RegisterCustomHandler((MmMethod)1000, OnCustomMethod);
        /// }
        ///
        /// private void OnCustomMethod(MmMessage msg)
        /// {
        ///     var customMsg = (MyCustomMessage)msg;
        ///     // Handle the custom message
        /// }
        /// </code>
        /// </example>
        protected void RegisterCustomHandler(MmMethod method, Action<MmMessage> handler)
        {
            int methodValue = (int)method;

            // Validate method ID is in custom range
            if (methodValue < 1000)
            {
                throw new ArgumentException(
                    $"Custom methods must be >= 1000. Got: {method} ({methodValue}). " +
                    $"Values 0-999 are reserved for framework methods. " +
                    $"Please use a method ID of 1000 or higher.",
                    nameof(method));
            }

            // Validate handler is not null
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler),
                    $"Handler for method {method} ({methodValue}) cannot be null. " +
                    $"Please provide a valid Action<MmMessage> delegate.");
            }

            // Lazy-initialize dictionary on first registration
            if (customHandlers == null)
            {
                customHandlers = new Dictionary<MmMethod, Action<MmMessage>>();
            }

            // Register handler (overwrites if already registered)
            customHandlers[method] = handler;

            MmLogger.LogFramework(
                $"[{GetType().Name}] Registered custom handler for method {method} ({methodValue}) " +
                $"on GameObject '{gameObject.name}'");
        }

        /// <summary>
        /// Unregister a custom handler. Useful for dynamic behavior changes.
        /// Safe to call even if the handler was never registered.
        /// </summary>
        /// <param name="method">The custom method to unregister</param>
        /// <remarks>
        /// <para>
        /// Use cases for unregistration:
        /// </para>
        /// <list type="bullet">
        /// <item>Dynamic behavior switching based on game state</item>
        /// <item>Disabling handlers during cleanup</item>
        /// <item>Replacing a handler with a different implementation</item>
        /// </list>
        /// <para>
        /// Note: This method is safe to call even if the method was never registered.
        /// No exception will be thrown.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Temporarily disable a handler
        /// UnregisterCustomHandler((MmMethod)1000);
        ///
        /// // Re-register with different implementation
        /// RegisterCustomHandler((MmMethod)1000, OnDifferentBehavior);
        /// </code>
        /// </example>
        protected void UnregisterCustomHandler(MmMethod method)
        {
            if (customHandlers != null && customHandlers.Remove(method))
            {
                MmLogger.LogFramework(
                    $"[{GetType().Name}] Unregistered custom handler for method {method} ({(int)method}) " +
                    $"on GameObject '{gameObject.name}'");
            }
        }

        /// <summary>
        /// Check if a custom handler is registered for a method.
        /// Useful for conditional logic based on handler availability.
        /// </summary>
        /// <param name="method">The method to check</param>
        /// <returns>True if a handler is registered for this method, false otherwise</returns>
        /// <remarks>
        /// This method is useful for:
        /// <list type="bullet">
        /// <item>Runtime validation of handler registration</item>
        /// <item>Conditional message sending based on handler availability</item>
        /// <item>Debugging handler registration issues</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// if (HasCustomHandler((MmMethod)1000))
        /// {
        ///     relay.MmInvoke((MmMethod)1000, customData);
        /// }
        /// else
        /// {
        ///     Debug.LogWarning("Handler not registered yet");
        /// }
        /// </code>
        /// </example>
        protected bool HasCustomHandler(MmMethod method)
        {
            return customHandlers?.ContainsKey(method) == true;
        }

        /// <summary>
        /// Called when a custom method has no registered handler.
        /// Override to customize behavior (e.g., ignore vs. warn vs. error).
        /// Default implementation logs a helpful warning with registration instructions.
        /// </summary>
        /// <param name="message">The unhandled message</param>
        /// <remarks>
        /// <para>
        /// Common override patterns:
        /// </para>
        /// <list type="bullet">
        /// <item>Silent ignore: Override with empty method (no warning)</item>
        /// <item>Strict error: Override to throw exception instead of logging warning</item>
        /// <item>Fallback behavior: Override to provide default handling</item>
        /// <item>Debug mode: Override to provide additional debugging information</item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Example 1: Silent ignore (no warnings)
        /// protected override void OnUnhandledCustomMethod(MmMessage message)
        /// {
        ///     // Do nothing - silently ignore unhandled messages
        /// }
        ///
        /// // Example 2: Strict error handling
        /// protected override void OnUnhandledCustomMethod(MmMessage message)
        /// {
        ///     throw new InvalidOperationException(
        ///         $"No handler registered for method {message.MmMethod}");
        /// }
        ///
        /// // Example 3: Fallback behavior
        /// protected override void OnUnhandledCustomMethod(MmMessage message)
        /// {
        ///     // Provide default handling for all unregistered methods
        ///     Debug.Log($"Using default handler for {message.MmMethod}");
        /// }
        /// </code>
        /// </example>
        protected virtual void OnUnhandledCustomMethod(MmMessage message)
        {
            int methodValue = (int)message.MmMethod;
            Debug.LogWarning(
                $"[{GetType().Name}] Unhandled custom method: {message.MmMethod} ({methodValue})\n" +
                $"GameObject: {gameObject.name}\n" +
                $"Suggestion: Register a handler with RegisterCustomHandler() in Awake():\n" +
                $"    RegisterCustomHandler((MmMethod){methodValue}, OnYourHandlerMethod);");
        }
    }
}
