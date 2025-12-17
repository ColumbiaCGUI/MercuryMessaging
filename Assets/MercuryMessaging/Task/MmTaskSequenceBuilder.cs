// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmTaskSequenceBuilder.cs - Fluent builder for task sequence configuration
// Part of DSL Overhaul Phase 4

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Fluent builder for configuring task sequences.
    /// Provides a clean, chainable API for setting up task progression callbacks.
    ///
    /// Example Usage:
    /// <code>
    /// MmTaskSequence.Create&lt;MmTaskInfo&gt;()
    ///     .From(taskManager)
    ///     .OnTaskStart(task => Debug.Log($"Started: {task.TaskName}"))
    ///     .OnTaskComplete(task => SaveProgress())
    ///     .OnSequenceComplete(() => ShowResults())
    ///     .Build();
    /// </code>
    /// </summary>
    /// <typeparam name="T">The task info type implementing IMmTaskInfo</typeparam>
    public class MmTaskSequenceBuilder<T> where T : class, IMmTaskInfo, new()
    {
        private MmTaskManager<T> _taskManager;
        private Action<T> _onTaskStart;
        private Action<T> _onTaskComplete;
        private Action _onSequenceStart;
        private Action _onSequenceComplete;
        private Action<T, T> _onTaskChange;
        private Func<T, bool> _taskFilter;
        private bool _autoAdvance = false;

        /// <summary>
        /// Set the task manager to configure.
        /// </summary>
        public MmTaskSequenceBuilder<T> From(MmTaskManager<T> taskManager)
        {
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            return this;
        }

        #region Event Handlers

        /// <summary>
        /// Set callback for when a task starts.
        /// </summary>
        public MmTaskSequenceBuilder<T> OnTaskStart(Action<T> callback)
        {
            _onTaskStart = callback;
            return this;
        }

        /// <summary>
        /// Set callback for when a task completes.
        /// </summary>
        public MmTaskSequenceBuilder<T> OnTaskComplete(Action<T> callback)
        {
            _onTaskComplete = callback;
            return this;
        }

        /// <summary>
        /// Set callback for when task changes (provides previous and new task).
        /// </summary>
        public MmTaskSequenceBuilder<T> OnTaskChange(Action<T, T> callback)
        {
            _onTaskChange = callback;
            return this;
        }

        /// <summary>
        /// Set callback for when task changes (simplified, no parameters).
        /// </summary>
        public MmTaskSequenceBuilder<T> OnTaskChange(Action callback)
        {
            _onTaskChange = (prev, curr) => callback?.Invoke();
            return this;
        }

        /// <summary>
        /// Set callback for when the sequence starts.
        /// </summary>
        public MmTaskSequenceBuilder<T> OnSequenceStart(Action callback)
        {
            _onSequenceStart = callback;
            return this;
        }

        /// <summary>
        /// Set callback for when all tasks are complete.
        /// </summary>
        public MmTaskSequenceBuilder<T> OnSequenceComplete(Action callback)
        {
            _onSequenceComplete = callback;
            return this;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Filter which tasks are included in the sequence.
        /// </summary>
        public MmTaskSequenceBuilder<T> Where(Func<T, bool> predicate)
        {
            _taskFilter = predicate;
            return this;
        }

        /// <summary>
        /// Enable auto-advance to next task when current completes.
        /// </summary>
        public MmTaskSequenceBuilder<T> AutoAdvance()
        {
            _autoAdvance = true;
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Build and return a configured task sequence handler.
        /// </summary>
        public MmTaskSequenceHandler<T> Build()
        {
            if (_taskManager == null)
                throw new InvalidOperationException("TaskManager not set. Call From() first.");

            var handler = new MmTaskSequenceHandler<T>(_taskManager)
            {
                OnTaskStart = _onTaskStart,
                OnTaskComplete = _onTaskComplete,
                OnTaskChange = _onTaskChange,
                OnSequenceStart = _onSequenceStart,
                OnSequenceComplete = _onSequenceComplete,
                TaskFilter = _taskFilter,
                AutoAdvance = _autoAdvance
            };

            return handler;
        }

        #endregion
    }

    /// <summary>
    /// Handler for task sequence events.
    /// Created by MmTaskSequenceBuilder.
    /// </summary>
    public class MmTaskSequenceHandler<T> where T : class, IMmTaskInfo, new()
    {
        private readonly MmTaskManager<T> _taskManager;
        private T _previousTask;

        public Action<T> OnTaskStart { get; set; }
        public Action<T> OnTaskComplete { get; set; }
        public Action<T, T> OnTaskChange { get; set; }
        public Action OnSequenceStart { get; set; }
        public Action OnSequenceComplete { get; set; }
        public Func<T, bool> TaskFilter { get; set; }
        public bool AutoAdvance { get; set; }

        public MmTaskSequenceHandler(MmTaskManager<T> taskManager)
        {
            _taskManager = taskManager;
        }

        /// <summary>
        /// Start the task sequence.
        /// </summary>
        public void Start()
        {
            OnSequenceStart?.Invoke();
            _taskManager.ProceedToFirstTask();
            NotifyTaskStart();
        }

        /// <summary>
        /// Advance to the next task.
        /// </summary>
        public void Next()
        {
            NotifyTaskComplete();

            _previousTask = _taskManager.CurrentTaskInfo;
            _taskManager.ProceedToNextTask();

            if (_taskManager.CurrentTaskInfo == null)
            {
                OnSequenceComplete?.Invoke();
            }
            else
            {
                OnTaskChange?.Invoke(_previousTask, _taskManager.CurrentTaskInfo);
                NotifyTaskStart();
            }
        }

        /// <summary>
        /// Complete the current task (optionally auto-advance).
        /// </summary>
        public void CompleteCurrentTask()
        {
            NotifyTaskComplete();

            if (AutoAdvance)
            {
                Next();
            }
        }

        private void NotifyTaskStart()
        {
            var current = _taskManager.CurrentTaskInfo;
            if (current != null && (TaskFilter == null || TaskFilter(current)))
            {
                OnTaskStart?.Invoke(current);
            }
        }

        private void NotifyTaskComplete()
        {
            var current = _taskManager.CurrentTaskInfo;
            if (current != null && (TaskFilter == null || TaskFilter(current)))
            {
                OnTaskComplete?.Invoke(current);
            }
        }

        /// <summary>
        /// Check if there are more tasks.
        /// </summary>
        public bool HasNextTask => _taskManager.NextTaskInfo != null;

        /// <summary>
        /// Get current task info.
        /// </summary>
        public T CurrentTask => _taskManager.CurrentTaskInfo;
    }

    /// <summary>
    /// Static factory for creating task sequence builders.
    /// </summary>
    public static class MmTaskSequence
    {
        /// <summary>
        /// Create a new task sequence builder.
        /// </summary>
        public static MmTaskSequenceBuilder<T> Create<T>() where T : class, IMmTaskInfo, new()
        {
            return new MmTaskSequenceBuilder<T>();
        }
    }
}
