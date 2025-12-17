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
    /// Marks a responder class for automatic dispatch code generation.
    /// The source generator will analyze ReceivedMessage overrides and generate
    /// an optimized MmInvoke switch statement that eliminates virtual dispatch overhead.
    ///
    /// Phase 4 Performance Optimization: Source Generators
    ///
    /// Usage:
    ///   [MmGenerateDispatch]
    ///   public class MyResponder : MmBaseResponder
    ///   {
    ///       protected override void ReceivedMessage(MmMessageInt msg) { ... }
    ///       protected override void ReceivedMessage(MmMessageString msg) { ... }
    ///   }
    ///
    /// The generator will create a partial class with an optimized MmInvoke override:
    ///   public partial class MyResponder
    ///   {
    ///       public override void MmInvoke(MmMessage message)
    ///       {
    ///           switch (message.MmMessageType)
    ///           {
    ///               case MmMessageType.MmInt:
    ///                   ReceivedMessage((MmMessageInt)message);
    ///                   return;
    ///               case MmMessageType.MmString:
    ///                   ReceivedMessage((MmMessageString)message);
    ///                   return;
    ///               // ... etc
    ///           }
    ///           base.MmInvoke(message);
    ///       }
    ///   }
    ///
    /// Benefits:
    /// - Eliminates virtual dispatch overhead (~8-10 ticks → ~2-4 ticks per message)
    /// - Compile-time optimization, no runtime reflection
    /// - Type-safe, IDE-friendly generated code
    /// - Automatically handles inheritance (calls base.MmInvoke for unhandled types)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MmGenerateDispatchAttribute : Attribute
    {
        /// <summary>
        /// When true, also generates dispatch for standard methods (SetActive, Initialize, etc.)
        /// in addition to typed message handlers.
        /// Default: true
        /// </summary>
        public bool IncludeStandardMethods { get; set; } = true;

        /// <summary>
        /// When true, generates logging calls for debugging dispatch performance.
        /// Should be disabled in production.
        /// Default: false
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// When true, generates null checks for message parameter.
        /// Slight overhead but prevents NullReferenceException.
        /// Default: true
        /// </summary>
        public bool GenerateNullChecks { get; set; } = true;
    }
}
