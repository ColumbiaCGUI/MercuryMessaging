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
using System;
using System.Collections.Generic;

namespace MercuryMessaging.Support.FiniteStateMachine
{
    /// <summary>
    /// Finite State Machine, supporting generic types states.
    /// Useful for creating applications with MercuryMessaging.
    /// </summary>
    /// <typeparam name="T">State Type</typeparam>
    public class FiniteStateMachine<T> : KeyedList<T, StateEvents>, IFiniteStateMachine<T>
    {
        #region Fields

        public delegate void Transition();
        
        public readonly string Name;
        public T Current { get; private set; }
        public T Previous { get; private set; }
        public T Next { get; private set; }
        public MmLogger.LogFunc LogMessage { get; set; }

        public Transition GlobalEnter;
        public Transition GlobalExit;

        private bool isTransitioning;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// This constructor assumes your FiniteStateMachine has been created with 
        /// Enums.
        /// </summary>
        /// <param name="name"></param>
        public FiniteStateMachine(string name)
        {
            Name = name;

            System.Type type = typeof(T);

            //When compiling to UNITY_WSA
            // T.isEnum is missing, so we check if the object
            //  is assignable from Enum instead. 
            //  The following line will not work directly.
            //if (typeof(T) != typeof(Enum)) return;
            //if (!(typeof(Enum).IsAssignableFrom(typeof(T)))) return;

            foreach (T val in Enum.GetValues(typeof(T)))
            {
                Add(val, new StateEvents());
            }
        }

        /// <summary>
        /// This constructor allows you to pass in a list of the values you would like 
        /// to initialize
        /// </summary>
        /// <param name="name"></param>
        /// <param name="collection"></param>
        public FiniteStateMachine(string name, List<T> collection)
        {
            Name = name;

            foreach (T val in collection)
            {
                Add(val, new StateEvents());
            }
        }

        #endregion Constructor

        #region Public Methods

        public void JumpTo(T newState)
        {
            if (newState.Equals(Current))
                return;

            Next = newState;

            StartLeaving();
            EnterNext();
        }

        public void StartTransitionTo(T newState)
        {
            Next = newState;

            StartLeaving();
        }

        public bool CancelStateChange()
        {
            if (!isTransitioning) return false;

            Next = default(T);
            isTransitioning = false;

            return true;
        }

        public void EnterNext()
        {
            if (!isTransitioning) return;

            isTransitioning = false;

            if (LogMessage != null)
                LogMessage(String.Format(">>>,{0},{1},{2}", Name, Current, Next));
            Previous = Current;
            Current = Next;
            Next = default(T);

            //Console.WriteLine(String.Format("Entering {0}",Current));
            if (this[Current].Enter != null) this[Current].Enter();
            if (GlobalEnter != null) GlobalEnter();            
        }

        public void Reset()
        {
            Previous = default(T);
        }

        public void ResetTo(T current)
        {
            Previous = default(T);
            Current = current;
        }

        #endregion Public Methods

        #region Private Methods

        private void StartLeaving()
        {
            isTransitioning = true;

            if (Current == null) return;

            //Console.WriteLine(String.Format("Exiting {0}",Current));
            if (GlobalExit != null) GlobalExit();
            if (this[Current].Exit != null) this[Current].Exit();
        }

        #endregion Private Methods
    }
}