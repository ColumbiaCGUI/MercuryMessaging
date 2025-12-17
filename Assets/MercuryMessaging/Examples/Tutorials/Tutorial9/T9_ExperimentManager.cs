using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Task;
using System.Collections.Generic;

/// <summary>
/// Tutorial 9: Experiment manager using MmTaskManager.
/// Manages sequential task execution for user studies.
///
/// Hierarchy Setup:
/// T9_Experiment (MmRelayNode + T9_ExperimentController)
///   └── TaskManager (T9_ExperimentManager)
///         └── TasksNode (MmRelaySwitchNode)
///               ├── Instructions (MmRelayNode + T9_InstructionsResponder)
///               ├── Practice (MmRelayNode + T9_TrialResponder)
///               ├── MainTrials (MmRelayNode + T9_TrialResponder)
///               └── Complete (MmRelayNode + T9_CompletionResponder)
///
/// Keyboard Controls:
/// N - Next task
/// P - Previous task
/// R - Restart current task
/// Space - Record trial response
/// </summary>
public class T9_ExperimentManager : MmTaskManager<T9_MyTaskInfo>
{
    [Header("Experiment Settings")]
    [SerializeField] private string participantId = "P001";

    [Header("Debug")]
    [SerializeField] private bool logTaskChanges = true;

    // Events for external listeners
    public event System.Action<T9_MyTaskInfo> OnTaskStarted;
    public event System.Action<T9_MyTaskInfo, bool> OnTaskCompleted;
    public event System.Action OnExperimentCompleted;

    public override void Start()
    {
        base.Start();

        // Subscribe to FSM events
        if (TasksNode != null && TasksNode.RespondersFSM != null)
        {
            TasksNode.RespondersFSM.GlobalEnter += HandleTaskStarted;
        }

        // If no tasks loaded from file, create sample tasks
        if (TaskInfos == null || TaskInfos.Count == 0)
        {
            CreateSampleTasks();
        }

        if (logTaskChanges)
        {
            Debug.Log($"[T9_ExperimentManager] Loaded {TaskInfos.Count} tasks for participant {participantId}");
            Debug.Log("[T9_ExperimentManager] Press N for next, P for previous, R to restart");
        }
    }

    void CreateSampleTasks()
    {
        // Create sample tasks if no file loader is configured
        TaskInfos = new LinkedList<T9_MyTaskInfo>();

        TaskInfos.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Instructions",
            TaskIndex = 0,
            TrialNumber = 0,
            Condition = "instructions",
            Instructions = "Welcome! Press N to continue."
        });

        TaskInfos.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Practice",
            TaskIndex = 1,
            TrialNumber = 1,
            Condition = "practice",
            StimulusType = "visual",
            TargetValue = 0.5f,
            TimeLimit = 3f
        });

        for (int i = 2; i <= 5; i++)
        {
            TaskInfos.AddLast(new T9_MyTaskInfo
            {
                TaskName = "Trial",
                TaskIndex = i,
                TrialNumber = i - 1,
                Condition = i % 2 == 0 ? "A" : "B",
                StimulusType = i % 2 == 0 ? "visual" : "auditory",
                TargetValue = 0.3f + (i * 0.1f),
                TimeLimit = 5f
            });
        }

        TaskInfos.AddLast(new T9_MyTaskInfo
        {
            TaskName = "Complete",
            TaskIndex = 6,
            TrialNumber = 0,
            Condition = "complete",
            Instructions = "Experiment complete! Thank you."
        });

        // Initialize to first task
        currentTaskInfo = TaskInfos.First;
    }

    void HandleTaskStarted()
    {
        if (CurrentTaskInfo == null) return;

        if (logTaskChanges)
        {
            Debug.Log($"[T9_ExperimentManager] Started: {CurrentTaskInfo}");
        }

        // Send task info to current state
        TasksNode.Send(MmMethod.TaskInfo)
            .ToChildren()
            .Selected()
            .Execute();

        OnTaskStarted?.Invoke(CurrentTaskInfo);
    }

    new void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // N - Next task
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextTask();
        }

        // P - Previous task
        if (Input.GetKeyDown(KeyCode.P))
        {
            GoToPreviousTask();
        }

        // R - Restart current task
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentTask();
        }
    }

    #region Navigation Methods

    public void GoToNextTask()
    {
        if (NextTaskInfo != null)
        {
            // Mark current as complete
            if (CurrentTaskInfo != null)
            {
                CurrentTaskInfo.IsComplete = true;
                OnTaskCompleted?.Invoke(CurrentTaskInfo, true);
            }

            // Move to next
            currentTaskInfo = currentTaskInfo.Next;
            SyncTasksNode();
        }
        else
        {
            if (logTaskChanges)
                Debug.Log("[T9_ExperimentManager] No more tasks - experiment complete!");

            OnExperimentCompleted?.Invoke();
        }
    }

    public void GoToPreviousTask()
    {
        if (PrevTaskInfo != null)
        {
            currentTaskInfo = currentTaskInfo.Previous;
            SyncTasksNode();
        }
        else
        {
            if (logTaskChanges)
                Debug.Log("[T9_ExperimentManager] Already at first task");
        }
    }

    public void GoToTask(int taskIndex)
    {
        var node = TaskInfos.First;
        while (node != null)
        {
            if (node.Value.TaskIndex == taskIndex)
            {
                currentTaskInfo = node;
                SyncTasksNode();
                return;
            }
            node = node.Next;
        }

        Debug.LogWarning($"[T9_ExperimentManager] Task index {taskIndex} not found");
    }

    public void RestartCurrentTask()
    {
        if (CurrentTaskInfo != null)
        {
            CurrentTaskInfo.IsComplete = false;
            SyncTasksNode();

            if (logTaskChanges)
                Debug.Log($"[T9_ExperimentManager] Restarted: {CurrentTaskInfo.TaskName}");
        }
    }

    void SyncTasksNode()
    {
        if (TasksNode == null || CurrentTaskInfo == null) return;

        // Find matching FSM state and jump to it
        foreach (var item in TasksNode.RoutingTable)
        {
            // Match by task name or use index-based matching
            if (item.Name == CurrentTaskInfo.TaskName)
            {
                TasksNode.RespondersFSM.JumpTo(item);
                return;
            }
        }

        // Fallback: jump by index
        int idx = 0;
        foreach (var item in TasksNode.RoutingTable)
        {
            if (idx == CurrentTaskInfo.TaskIndex)
            {
                TasksNode.RespondersFSM.JumpTo(item);
                return;
            }
            idx++;
        }
    }

    #endregion

    #region Task Query Methods

    public bool IsAtFirstTask => PrevTaskInfo == null;
    public bool IsAtLastTask => NextTaskInfo == null;
    public int TotalTasks => TaskInfos?.Count ?? 0;
    public int CompletedTasks => CountCompletedTasks();

    int CountCompletedTasks()
    {
        int count = 0;
        var node = TaskInfos?.First;
        while (node != null)
        {
            if (node.Value.IsComplete) count++;
            node = node.Next;
        }
        return count;
    }

    public float Progress => TotalTasks > 0 ? (float)CurrentTaskInfo.TaskIndex / TotalTasks : 0f;

    #endregion
}
