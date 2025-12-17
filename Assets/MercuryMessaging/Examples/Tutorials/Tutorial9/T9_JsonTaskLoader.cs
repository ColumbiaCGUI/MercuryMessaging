using UnityEngine;
using MercuryMessaging.Task;
using System.Collections.Generic;

/// <summary>
/// Tutorial 9: Loads task definitions from a JSON file.
/// Implements IMmTaskInfoCollectionLoader interface.
///
/// JSON Format:
/// {
///     "tasks": [
///         {
///             "TaskName": "Trial",
///             "TaskIndex": 0,
///             "TrialNumber": 1,
///             "Condition": "A",
///             "StimulusType": "visual",
///             "TargetValue": 0.5,
///             "TimeLimit": 5.0
///         },
///         ...
///     ]
/// }
/// </summary>
public class T9_JsonTaskLoader : MonoBehaviour, IMmTaskInfoCollectionLoader<T9_MyTaskInfo>
{
    [Header("Task File")]
    [Tooltip("Assign a TextAsset containing task definitions in JSON format")]
    [SerializeField] private TextAsset taskFile;

    [Header("Debug")]
    [SerializeField] private bool logLoadedTasks = true;

    // Store last sequence value for partial progress
    private int lastSequenceValue = 0;

    /// <summary>
    /// Prepare tasks by loading from JSON file or creating sample tasks.
    /// Required by IMmTaskInfoCollectionLoader interface.
    /// </summary>
    /// <param name="taskInfos">Reference to linked list to populate</param>
    /// <param name="userId">User ID for filtering (if applicable)</param>
    /// <returns>Starting index (0) or -1 on failure</returns>
    public int PrepareTasks(ref LinkedList<T9_MyTaskInfo> taskInfos, int userId)
    {
        taskInfos = Load();

        if (taskInfos == null || taskInfos.Count == 0)
        {
            Debug.LogError("[T9_JsonTaskLoader] Failed to load tasks");
            return -1;
        }

        // Apply user ID to all tasks
        foreach (var task in taskInfos)
        {
            task.UserId = userId;
        }

        // Return starting index (0 or resume from lastSequenceValue)
        return lastSequenceValue;
    }

    /// <summary>
    /// Save current sequence value for partial progress resumption.
    /// Required by IMmTaskInfoCollectionLoader interface.
    /// </summary>
    /// <param name="seqVal">Current sequence value to save</param>
    public void SaveCurrentTaskSequenceValue(int seqVal)
    {
        lastSequenceValue = seqVal;

        if (logLoadedTasks)
        {
            Debug.Log($"[T9_JsonTaskLoader] Saved sequence value: {seqVal}");
        }

        // In a real implementation, you might save this to PlayerPrefs or file:
        // PlayerPrefs.SetInt("TaskSequence_" + userId, seqVal);
    }

    /// <summary>
    /// Load tasks from the assigned JSON file.
    /// </summary>
    public LinkedList<T9_MyTaskInfo> Load()
    {
        var result = new LinkedList<T9_MyTaskInfo>();

        if (taskFile == null)
        {
            Debug.LogWarning("[T9_JsonTaskLoader] No task file assigned - using sample tasks");
            return CreateSampleTasks();
        }

        try
        {
            // Parse JSON
            var wrapper = JsonUtility.FromJson<TaskWrapper>(taskFile.text);

            if (wrapper == null || wrapper.tasks == null)
            {
                Debug.LogError("[T9_JsonTaskLoader] Failed to parse task file");
                return CreateSampleTasks();
            }

            // Convert to linked list and assign sequence IDs
            int seq = 0;
            foreach (var task in wrapper.tasks)
            {
                task.UserSequence = seq++;
                result.AddLast(task);

                if (logLoadedTasks)
                {
                    Debug.Log($"[T9_JsonTaskLoader] Loaded: {task}");
                }
            }

            Debug.Log($"[T9_JsonTaskLoader] Successfully loaded {result.Count} tasks");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[T9_JsonTaskLoader] Error parsing JSON: {e.Message}");
            return CreateSampleTasks();
        }

        return result;
    }

    /// <summary>
    /// Create sample tasks when no file is provided.
    /// </summary>
    LinkedList<T9_MyTaskInfo> CreateSampleTasks()
    {
        var result = new LinkedList<T9_MyTaskInfo>();
        int seq = 0;

        result.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Instructions",
            TaskIndex = 0,
            TaskId = 0,
            TrialNumber = 0,
            Condition = "instructions",
            UserSequence = seq++,
            DoNotRecordData = true
        });

        result.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Practice",
            TaskIndex = 1,
            TaskId = 1,
            TrialNumber = 1,
            Condition = "practice",
            StimulusType = "visual",
            TargetValue = 0.5f,
            TimeLimit = 3f,
            UserSequence = seq++,
            DoNotRecordData = true
        });

        result.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Trial",
            TaskIndex = 2,
            TaskId = 2,
            TrialNumber = 1,
            Condition = "A",
            StimulusType = "visual",
            TargetValue = 0.3f,
            TimeLimit = 5f,
            UserSequence = seq++
        });

        result.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Trial",
            TaskIndex = 3,
            TaskId = 3,
            TrialNumber = 2,
            Condition = "B",
            StimulusType = "auditory",
            TargetValue = 0.7f,
            TimeLimit = 5f,
            UserSequence = seq++
        });

        result.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Complete",
            TaskIndex = 4,
            TaskId = 4,
            TrialNumber = 0,
            Condition = "complete",
            UserSequence = seq++,
            DoNotRecordData = true
        });

        Debug.Log($"[T9_JsonTaskLoader] Created {result.Count} sample tasks");
        return result;
    }

    // JSON wrapper class for Unity's JsonUtility
    [System.Serializable]
    private class TaskWrapper
    {
        public T9_MyTaskInfo[] tasks;
    }
}
