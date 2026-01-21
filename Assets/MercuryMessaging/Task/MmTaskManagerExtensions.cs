// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmTaskManagerExtensions.cs - Fluent DSL extensions for task management
// Part of DSL Overhaul Phase 4

using System;
using System.Collections.Generic;
using System.Linq;

namespace MercuryMessaging.Task
{
    /// <summary>
    /// Extension methods for MmTaskManager to enable fluent task management.
    /// Provides convenient methods for task navigation and querying.
    ///
    /// Example Usage:
    /// <code>
    /// // Navigation
    /// taskManager.GoToFirst();
    /// taskManager.GoToNext();
    /// taskManager.GoTo("TaskName");
    ///
    /// // Queries
    /// var remaining = taskManager.GetRemainingCount();
    /// var progress = taskManager.GetProgressPercentage();
    /// var current = taskManager.GetCurrentTaskName();
    ///
    /// // Iteration
    /// foreach (var task in taskManager.GetAllTasks())
    /// {
    ///     Debug.Log(task.TaskName);
    /// }
    /// </code>
    /// </summary>
    public static class MmTaskManagerExtensions
    {
        #region Navigation

        /// <summary>
        /// Go to the first task in the sequence.
        /// </summary>
        public static void GoToFirst<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            manager?.ProceedToFirstTask();
        }

        /// <summary>
        /// Go to the next task in the sequence.
        /// </summary>
        public static void GoToNext<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            manager?.ProceedToNextTask();
        }

        /// <summary>
        /// Check if there are more tasks after current.
        /// </summary>
        public static bool HasNextTask<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return manager?.NextTaskInfo != null;
        }

        /// <summary>
        /// Check if there are previous tasks.
        /// </summary>
        public static bool HasPreviousTask<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return manager?.PrevTaskInfo != null;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Get the current task name.
        /// </summary>
        public static string GetCurrentTaskName<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return manager?.CurrentTaskInfo?.TaskName;
        }

        /// <summary>
        /// Get the total number of tasks.
        /// </summary>
        public static int GetTotalCount<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return manager?.TaskInfos?.Count ?? 0;
        }

        /// <summary>
        /// Get the current task index (0-based).
        /// </summary>
        public static int GetCurrentIndex<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            if (manager?.TaskInfos == null || manager.CurrentTaskInfo == null)
                return -1;

            int index = 0;
            foreach (var task in manager.TaskInfos)
            {
                if (ReferenceEquals(task, manager.CurrentTaskInfo))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Get the number of remaining tasks (including current).
        /// </summary>
        public static int GetRemainingCount<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            var total = manager.GetTotalCount();
            var current = manager.GetCurrentIndex();
            return current >= 0 ? total - current : 0;
        }

        /// <summary>
        /// Get progress as a percentage (0-100).
        /// </summary>
        public static float GetProgressPercentage<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            var total = manager.GetTotalCount();
            if (total == 0) return 0;

            var current = manager.GetCurrentIndex();
            return current >= 0 ? (current / (float)total) * 100f : 0f;
        }

        /// <summary>
        /// Get progress as a fraction (0-1).
        /// </summary>
        public static float GetProgressFraction<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return manager.GetProgressPercentage() / 100f;
        }

        /// <summary>
        /// Check if at the last task.
        /// </summary>
        public static bool IsLastTask<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return !manager.HasNextTask();
        }

        /// <summary>
        /// Check if at the first task.
        /// </summary>
        public static bool IsFirstTask<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return !manager.HasPreviousTask();
        }

        #endregion

        #region Iteration

        /// <summary>
        /// Get all tasks as an enumerable.
        /// </summary>
        public static IEnumerable<T> GetAllTasks<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            if (manager?.TaskInfos == null)
                yield break;

            foreach (var task in manager.TaskInfos)
                yield return task;
        }

        /// <summary>
        /// Get remaining tasks (from current onwards).
        /// </summary>
        public static IEnumerable<T> GetRemainingTasks<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            if (manager?.TaskInfos == null || manager.CurrentTaskInfo == null)
                yield break;

            bool found = false;
            foreach (var task in manager.TaskInfos)
            {
                if (ReferenceEquals(task, manager.CurrentTaskInfo))
                    found = true;

                if (found)
                    yield return task;
            }
        }

        /// <summary>
        /// Get completed tasks (before current).
        /// </summary>
        public static IEnumerable<T> GetCompletedTasks<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            if (manager?.TaskInfos == null || manager.CurrentTaskInfo == null)
                yield break;

            foreach (var task in manager.TaskInfos)
            {
                if (ReferenceEquals(task, manager.CurrentTaskInfo))
                    yield break;

                yield return task;
            }
        }

        /// <summary>
        /// Find tasks by name.
        /// </summary>
        public static IEnumerable<T> FindByName<T>(this MmTaskManager<T> manager, string taskName)
            where T : class, IMmTaskInfo, new()
        {
            return manager.GetAllTasks().Where(t => t.TaskName == taskName);
        }

        #endregion

        #region Fluent Builder Access

        /// <summary>
        /// Create a task sequence builder for this manager.
        /// </summary>
        public static MmTaskSequenceBuilder<T> ConfigureSequence<T>(this MmTaskManager<T> manager)
            where T : class, IMmTaskInfo, new()
        {
            return MmTaskSequence.Create<T>().From(manager);
        }

        #endregion
    }
}
