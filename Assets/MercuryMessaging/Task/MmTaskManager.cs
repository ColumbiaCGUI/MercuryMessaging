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
using System.Linq;
using MercuryMessaging.Support.Data;
using UnityEngine;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Class to manage a collection of tasks.
    /// </summary>
    /// <typeparam name="U">Must implement IMmTaskInfo</typeparam>
    [Serializable]
    [RequireComponent(typeof(MmTaskUserConfigurator))]
    public class MmTaskManager<U> : MmBaseResponder
        where U : class, IMmTaskInfo, new()
    {
        /// <summary>
        /// This switch node allows you to iterate through a set of tasks
        /// using the same mechanisms that allow you to iterate through an
        /// FSM of MmResponders.
        /// </summary>
        public MmRelaySwitchNode TasksNode;

        /// <summary>
        /// The loaded task infos.
        /// </summary>
        public LinkedList<U> TaskInfos;

        /// <summary>
        /// Contains information of where to load task data, 
        /// and which tasks to load based on user ID.
        /// </summary>
        public MmTaskUserConfigurator MmTaskUserData;

        /// <summary>
        /// Node of currently selected task.
        /// </summary>
        protected LinkedListNode<U> currentTaskInfo;

        /// <summary>
        /// Current task info.
        /// </summary>
		public U CurrentTaskInfo { get { return currentTaskInfo.Value;  } set {currentTaskInfo.Value = value;} }
        /// <summary>
        /// Next task info.
        /// </summary>
        public U NextTaskInfo { get { return (currentTaskInfo.Next == null) ? null : currentTaskInfo.Next.Value; } }
        /// <summary>
        /// Previous task info.
        /// </summary>
        public U PrevTaskInfo { get { return (currentTaskInfo.Previous == null) ? null : currentTaskInfo.Previous.Value; } }

        /// <summary>
        /// Allow task manager to save partial system progression info
        /// </summary>
        public bool SavePartialProgression = false;

        /// <summary>
        /// This should be an item placed on the same GameObject as the MmTaskManager itself
        /// </summary>
	    private ITaskInfoCollectionLoader<U> taskInfoCollectionLoader;
        
        /// <summary>
        /// Total number of task infos that have the same name.
        /// </summary>
        public int TotalTasksWithCurrentName
        {
            get
            {
                return (from t in TaskInfos
                        where t.TaskName == CurrentTaskInfo.TaskName
                        select t).Count();
            }
        }

        /// <summary>
        /// Handle to the task info collection loader that is used to load the 
        /// task collection.
        /// </summary>
	    public ITaskInfoCollectionLoader<U> TaskInfoCollectionLoader
	    {
	        get { return taskInfoCollectionLoader; }
	    }

	    #region MonoBehaviour Methods

        /// <summary>
        /// Get attached ItaskInfoCollectionLoader, which must be attached to the 
        /// same game object.
        /// </summary>
        public override void Awake()
	    {
            MmLogger.LogApplication("MmTaskManager Awake");

            taskInfoCollectionLoader = GetComponent<ITaskInfoCollectionLoader<U>>();
            MmTaskUserData = GetComponent<MmTaskUserConfigurator>();
        }

        /// <summary>
        /// Prepare the tasks that were loaded.
        /// </summary>
	    public override void Start()
	    {
            MmLogger.LogApplication("MmTaskManager Start");

	        PrepareTasks();
	    }

        #endregion
        
        /// <summary>
        /// Get current task responder.
        /// </summary>
        /// <returns>Current task responder</returns>
        public MmTaskResponder<U> GetCurrentTaskResponder()
        {
            return TasksNode.Current.GetComponent<MmTaskResponder<U>>();
        }

        /// <summary>
        /// Extract a task responder from a given MmRelayNode.
        /// </summary>
        /// <param name="_mmRelayNode">MmRelayNode - should share GameObject with 
        /// a task responder.</param>
        /// <returns>MmTaskResponder, if present on GameObject.</returns>
        public MmTaskResponder<U> GetTaskResponder(MmRelayNode _mmRelayNode)
        {
            return _mmRelayNode.GetComponent<MmTaskResponder<U>> ();
        }

        /// <summary>
        /// Set the current pointer to the first task in the list.
        /// </summary>
        public virtual void ProceedToFirstTask()
		{
			currentTaskInfo = TaskInfos.First;
		}

        /// <summary>
        /// Prepare the tasks that were loaded by the 
        /// TaskInfoCollectionLoader.
        /// </summary>
	    public virtual void PrepareTasks()
	    {
            int taskLoadStatus = taskInfoCollectionLoader.PrepareTasks(
                ref TaskInfos, MmTaskUserData.UserId);

	        if (taskLoadStatus >= 0)
	        {
	            currentTaskInfo = GetNodeAt(taskLoadStatus);
	        }
	        else
	            MmLogger.LogError("Task Load Failed");

	        ApplySequenceID();
	    }

        /// <summary>
        /// Move current task pointer to the next task info.
        /// If the instance can, it will attempt to trigger a switch message to 
        /// move the FSM to the next task state.
        /// </summary>
	    public virtual void ProceedToNextTask()
	    {
            if(SavePartialProgression)
                TaskInfoCollectionLoader.SaveCurrentTaskSequenceValue(CurrentTaskInfo.UserSequence);

	        currentTaskInfo = (currentTaskInfo == null)
			        ? TaskInfos.First
			        : currentTaskInfo.Next;

	        if (ShouldTriggerSwitch())
	        {
	            TasksNode.MmInvoke(MmMethod.Switch, CurrentTaskInfo.TaskName,
	                new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));
	        }

	        ApplySequenceID();
	    }

        /// <summary>
        /// Base class implementation of ShouldTriggerSwitch always returns true.
        /// </summary>
        /// <returns>Whether to trigger a switch in the FSM</returns>
        public virtual bool ShouldTriggerSwitch()
        {
            return true;
        }

        /// <summary>
        /// Index accessor for LinkedList of task infos.
        /// </summary>
        /// <param name="index">Value index.</param>
        /// <returns>If in bounds, LinkedListNode at [index] from start.</returns>
        public LinkedListNode<U> GetNodeAt(int index)
        {
            if (index >= TaskInfos.Count) return null;

            var count = 0;
            var task = TaskInfos.First;

            while (task != null)
            {
                if (index == count) return task;
                task = task.Next;
                count++;
            }

            return null;
        }

        /// <summary>
        /// Apply the user-based sequence ID to the associated
        /// MmTaskUserConfigurator.
        /// </summary>
        public virtual void ApplySequenceID()
        {
            MmTaskUserData.SequenceId = currentTaskInfo.Value.UserSequence;
        }
    }
}