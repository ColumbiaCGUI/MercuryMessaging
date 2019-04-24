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
using MercuryMessaging.Support.FiniteStateMachine;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// MmTaskResponder are MmBaseResponder that deal with IMmTaskInfos.
    /// Useful if you're trying to prepare MmBaseResponder derivations,
    /// but do not need to re-implement basic interaction with IMmTaskInfos
    /// </summary>
    /// <typeparam name="T">Must implement IMmTaskInfo</typeparam>
    public class MmTaskResponder<T> : MmBaseResponder where T : IMmTaskInfo, new()
    {
        /// <summary>
        /// The completion FSM. Starts in a incomplete state and goes to complete when the task is complete.
        /// </summary>
        public FiniteStateMachine<TaskState> TaskStateFSM;

        protected T mmTaskInfo;

        private MmRelayNode _mmRelayNode;

        /// <summary>
        /// The task info contains all details of a particular task.
        /// </summary>
        public T MmTaskInfo
        {
            get { return mmTaskInfo; }
        }

        /// <summary>
        /// TaskState represents the task's completion state
        /// </summary>
        public enum TaskState
        {
            NotComplete,
            Complete
        };

        /// <summary>
        /// Determine if the current task is complete.
        /// This class, which is intended to be used as a base class normally,
        /// will return false here.
        /// </summary>
        /// <returns>Is the task complete?</returns>
        public virtual bool TaskCompleteCheck()
        {
            return false;
        }

        /// <summary>
        /// MmTaskInfo constructed early to allow assignment through other GameObject awake calls if this
        /// responder's awake has not been called yet.
        /// </summary>
        public MmTaskResponder()
        {
            mmTaskInfo = new T();
        }

        /// <summary>
        /// Awake initializes completion state FSM
        /// </summary>
        public override void Awake()
        {
            //Debug.Log ("Awake called on: " + gameObject.name);

            InitializeCompletionFSM();

            _mmRelayNode = GetComponent<MmRelayNode>();

            base.Awake();
        }

        /// <summary>
        /// Prepare the completion state FSM entry/exit delegates
        /// </summary>
        public virtual void InitializeCompletionFSM()
        {
            TaskStateFSM = new FiniteStateMachine<TaskState>
                (gameObject.name + "_TaskStateFSM")
            {
                LogMessage =  MmLogger.LogApplication
            };

            TaskStateFSM[TaskState.Complete].Enter = delegate
            {
                if (_mmRelayNode != null)
                {
                    _mmRelayNode.MmInvoke(MmMethod.Complete, true,
                        new MmMetadataBlock(MmLevelFilter.Parent));
                }
            };

            TaskStateFSM.JumpTo(TaskState.NotComplete);

            TaskStateFSM[TaskState.Complete].Exit = delegate
            {
                if (_mmRelayNode != null)
                {
                    _mmRelayNode.MmInvoke(MmMethod.Complete, false,
                        new MmMetadataBlock(MmLevelFilter.Parent));
                }
            };
        }

        /// <summary>
        /// Update method determines whether the task is complete
        /// by invoking TaskCompleteCheck()
        /// </summary>
        public override void Update()
        {
            //Debug.Log("MmTaskResponder Update " + gameObject.name);

            if (TaskCompleteCheck())
            {
                if(TaskStateFSM.Current == TaskState.NotComplete)
                   TaskStateFSM.JumpTo(TaskState.Complete);
            }
            else
            {
                if(TaskStateFSM.Current == TaskState.Complete)
                    TaskStateFSM.JumpTo(TaskState.NotComplete);
            }
        }
    }
}