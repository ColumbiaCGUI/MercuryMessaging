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
// MercuryMessaging Team
// =============================================================

using System;

namespace MercuryMessaging
{
    /// <summary>
    /// Marks a method as a handler for a specific custom MmMethod value.
    /// When combined with [MmGenerateDispatch] on the class, the source generator
    /// creates compile-time dispatch code that eliminates dictionary lookup overhead.
    ///
    /// Phase 4 DSL/DX Enhancement: Custom Handler Generation
    ///
    /// Usage:
    ///   [MmGenerateDispatch]
    ///   public partial class MyResponder : MmBaseResponder
    ///   {
    ///       [MmHandler(1000)]
    ///       private void OnCustomColor(MmMessage msg)
    ///       {
    ///           var colorMsg = (ColorMessage)msg;
    ///           // Handle color change
    ///       }
    ///
    ///       [MmHandler(1001)]
    ///       private void OnCustomScale(MmMessage msg)
    ///       {
    ///           var scaleMsg = (ScaleMessage)msg;
    ///           // Handle scale change
    ///       }
    ///   }
    ///
    /// The generator creates a switch statement for custom methods (1000+):
    ///   public partial class MyResponder
    ///   {
    ///       public override void MmInvoke(MmMessage message)
    ///       {
    ///           // ... standard message type dispatch ...
    ///
    ///           // Custom method dispatch
    ///           switch ((int)message.MmMethod)
    ///           {
    ///               case 1000:
    ///                   OnCustomColor(message);
    ///                   return;
    ///               case 1001:
    ///                   OnCustomScale(message);
    ///                   return;
    ///           }
    ///           base.MmInvoke(message);
    ///       }
    ///   }
    ///
    /// Benefits:
    /// - Eliminates dictionary lookup overhead (~300-500ns → ~100-150ns per message)
    /// - Compile-time validation of handler method signatures
    /// - No runtime registration required (no Awake() boilerplate)
    /// - Type-safe, IDE-friendly generated code
    /// - Compatible with MmExtendableResponder for hybrid approaches
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MmHandlerAttribute : Attribute
    {
        /// <summary>
        /// The MmMethod value this handler responds to.
        /// Must be >= 1000 (values 0-999 are reserved for framework methods).
        /// </summary>
        public int MethodId { get; }

        /// <summary>
        /// Optional name for the handler (used in debug logging and error messages).
        /// If not specified, the method name is used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a handler attribute for the specified custom method ID.
        /// </summary>
        /// <param name="methodId">
        /// The MmMethod value (must be >= 1000).
        /// Example: [MmHandler(1000)] for custom method 1000.
        /// </param>
        public MmHandlerAttribute(int methodId)
        {
            MethodId = methodId;
        }

        /// <summary>
        /// Creates a handler attribute using an MmMethod enum value.
        /// </summary>
        /// <param name="method">The MmMethod enum value to handle.</param>
        public MmHandlerAttribute(MmMethod method)
        {
            MethodId = (int)method;
        }
    }
}
