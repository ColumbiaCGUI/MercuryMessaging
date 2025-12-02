// Copyright (c) 2017-2019, Columbia University
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
// Carmine Elvezio, Mengu Sukan, Steven Feiner
// =============================================================
//  
//  
namespace MercuryMessaging.Support.FiniteStateMachine
{
    /// <summary>
    /// Defines required methods for a 
    /// Finite State Machine.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFiniteStateMachine<T>
    {
        /// <summary>
        /// Current state.
        /// </summary>
        T Current { get; }

        /// <summary>
        /// Previous state.
        /// </summary>
        T Previous { get; }

        /// <summary>
        /// Next state.
        /// </summary>
        T Next { get; }

        /// <summary>
        /// Go to state "newState"
        /// </summary>
        /// <param name="newState">Target state.</param>
        void JumpTo(T newState);

        /// <summary>
        /// Called when entering new state.
        /// </summary>
        /// <param name="newState">Target State.</param>
        void StartTransitionTo(T newState);

        /// <summary>
        /// Stop state transition.
        /// </summary>
        /// <returns>Whether cancellation succeeded.</returns>
        bool CancelStateChange();

        /// <summary>
        /// Enter the next state, stored in "Next" member.
        /// </summary>
        void EnterNext();

        StateEvents this[T key] { get; set; }

        int Count { get; }
    }
}
