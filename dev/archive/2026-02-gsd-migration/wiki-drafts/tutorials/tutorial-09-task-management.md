# Tutorial 9: Task Management

## Overview

`MmTaskManager` provides a framework for managing sequential tasks, experiments, and user studies. It's particularly useful for research applications where you need to present a series of trials, collect data, and track progress.

## What You'll Learn

- Setting up `MmTaskManager` for experiment workflows
- Loading task definitions from files (JSON/CSV)
- Navigating between tasks
- Integrating with data collection (`MmDataCollector`)
- Creating a simple user study example

## Prerequisites

- Completed [Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)
- Understanding of FSM concepts

---

## Core Components

The task system consists of several components:

| Component | Purpose |
|-----------|---------|
| `MmTaskManager<T>` | Manages task sequence and navigation |
| `MmTaskUserConfigurator` | Configures tasks based on user/participant ID |
| `IMmTaskInfo` | Interface for task data |
| `IMmTaskInfoCollectionLoader<T>` | Loads tasks from file (JSON/CSV) |
| `MmDataCollector` | Collects and exports experiment data |

---

## Architecture Overview

```
TaskManager (MmTaskManager + MmTaskUserConfigurator)
  └── TasksNode (MmRelaySwitchNode)  ← FSM for task states
        ├── Trial1 (MmRelayNode + TrialResponder)
        ├── Trial2 (MmRelayNode + TrialResponder)
        └── Trial3 (MmRelayNode + TrialResponder)
```

The task manager uses an `MmRelaySwitchNode` internally to manage which task is currently active.

---

## Step-by-Step Setup

### Step 1: Define Your Task Info Class

Create a class implementing `IMmTaskInfo`:

```csharp
using System;
using MercuryMessaging.Task;

[Serializable]
public class MyTaskInfo : IMmTaskInfo
{
    // Required by IMmTaskInfo
    public string TaskName { get; set; }
    public int TaskIndex { get; set; }
    public bool IsComplete { get; set; }

    // Custom fields for your experiment
    public string StimulusType { get; set; }
    public float TargetValue { get; set; }
    public int TrialNumber { get; set; }
    public string Condition { get; set; }
}
```

### Step 2: Create Task Manager

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Task;

public class ExperimentManager : MmTaskManager<MyTaskInfo>
{
    protected override void Start()
    {
        base.Start();

        // Subscribe to task events
        if (TasksNode != null && TasksNode.RespondersFSM != null)
        {
            TasksNode.RespondersFSM.GlobalEnter += OnTaskStarted;
        }
    }

    void OnTaskStarted()
    {
        Debug.Log($"Starting task: {CurrentTaskInfo.TaskName}");
        Debug.Log($"Trial {CurrentTaskInfo.TrialNumber}, Condition: {CurrentTaskInfo.Condition}");
    }

    public void OnTrialComplete(bool success, float responseTime)
    {
        // Record result
        CurrentTaskInfo.IsComplete = true;

        // Move to next task
        if (NextTaskInfo != null)
        {
            AdvanceToNextTask();
        }
        else
        {
            Debug.Log("Experiment complete!");
            OnExperimentComplete();
        }
    }

    void AdvanceToNextTask()
    {
        // The task manager advances through TaskInfos
        // and syncs with the TasksNode FSM
    }

    void OnExperimentComplete()
    {
        // Save data, show completion screen, etc.
    }
}
```

### Step 3: Create Task Definition File

Create a JSON file with your task definitions:

**tasks.json:**
```json
{
    "tasks": [
        {
            "TaskName": "Practice",
            "TaskIndex": 0,
            "TrialNumber": 1,
            "StimulusType": "visual",
            "TargetValue": 0.5,
            "Condition": "practice"
        },
        {
            "TaskName": "Trial",
            "TaskIndex": 1,
            "TrialNumber": 1,
            "StimulusType": "visual",
            "TargetValue": 0.3,
            "Condition": "A"
        },
        {
            "TaskName": "Trial",
            "TaskIndex": 2,
            "TrialNumber": 2,
            "StimulusType": "auditory",
            "TargetValue": 0.7,
            "Condition": "B"
        }
    ]
}
```

### Step 4: Setup Task Loader

Implement `IMmTaskInfoCollectionLoader<T>`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using MercuryMessaging.Task;

public class JsonTaskLoader : MonoBehaviour, IMmTaskInfoCollectionLoader<MyTaskInfo>
{
    [SerializeField] private TextAsset taskFile;

    public LinkedList<MyTaskInfo> Load()
    {
        var result = new LinkedList<MyTaskInfo>();

        if (taskFile == null)
        {
            Debug.LogError("Task file not assigned!");
            return result;
        }

        // Parse JSON (using Unity's JsonUtility or Newtonsoft)
        var wrapper = JsonUtility.FromJson<TaskWrapper>(taskFile.text);

        foreach (var task in wrapper.tasks)
        {
            result.AddLast(task);
        }

        Debug.Log($"Loaded {result.Count} tasks");
        return result;
    }

    [System.Serializable]
    private class TaskWrapper
    {
        public MyTaskInfo[] tasks;
    }
}
```

---

## Navigating Tasks

### Basic Navigation

```csharp
public class ExperimentManager : MmTaskManager<MyTaskInfo>
{
    // Move to next task
    public void GoToNextTask()
    {
        if (NextTaskInfo != null)
        {
            currentTaskInfo = currentTaskInfo.Next;
            SyncTasksNode();
        }
    }

    // Move to previous task
    public void GoToPreviousTask()
    {
        if (PrevTaskInfo != null)
        {
            currentTaskInfo = currentTaskInfo.Previous;
            SyncTasksNode();
        }
    }

    // Jump to specific task by index
    public void GoToTask(int index)
    {
        var node = TaskInfos.First;
        for (int i = 0; i < index && node != null; i++)
        {
            node = node.Next;
        }

        if (node != null)
        {
            currentTaskInfo = node;
            SyncTasksNode();
        }
    }

    private void SyncTasksNode()
    {
        // Sync the FSM with current task
        // Implementation depends on how tasks map to FSM states
    }
}
```

---

## Data Collection Integration

### Setup MmDataCollector

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Data;

public class ExperimentDataCollector : MonoBehaviour
{
    private MmDataCollector dataCollector;

    void Start()
    {
        dataCollector = GetComponent<MmDataCollector>();

        // Setup CSV headers
        dataCollector.SetHeaders(new string[]
        {
            "ParticipantID",
            "TrialNumber",
            "Condition",
            "StimulusType",
            "ResponseTime",
            "Accuracy",
            "Timestamp"
        });
    }

    public void RecordTrial(
        string participantId,
        int trialNumber,
        string condition,
        string stimulusType,
        float responseTime,
        bool correct)
    {
        dataCollector.AddRow(new string[]
        {
            participantId,
            trialNumber.ToString(),
            condition,
            stimulusType,
            responseTime.ToString("F3"),
            correct ? "1" : "0",
            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    public void SaveData()
    {
        dataCollector.SaveToFile("experiment_results.csv");
    }
}
```

---

## Complete Example: Go/No-Go Task

### Hierarchy

```
Experiment (MmRelayNode + ExperimentController)
  ├── TaskManager (ExperimentManager + MmTaskUserConfigurator + JsonTaskLoader)
  │     └── TasksNode (MmRelaySwitchNode)
  │           ├── Instructions (MmRelayNode + InstructionsResponder)
  │           ├── Practice (MmRelayNode + TrialResponder)
  │           ├── MainTrials (MmRelayNode + TrialResponder)
  │           └── Complete (MmRelayNode + CompletionResponder)
  └── DataCollector (MmDataCollector + ExperimentDataCollector)
```

### ExperimentController.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class ExperimentController : MonoBehaviour
{
    [SerializeField] private ExperimentManager taskManager;
    [SerializeField] private ExperimentDataCollector dataCollector;
    [SerializeField] private string participantId = "P001";

    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Initialize all systems
        relay.BroadcastInitialize();
    }

    public void OnTrialResponse(bool responded, float reactionTime)
    {
        var currentTask = taskManager.CurrentTaskInfo;
        bool isGoTrial = currentTask.Condition == "Go";

        // Calculate accuracy
        bool correct = (isGoTrial && responded) || (!isGoTrial && !responded);

        // Record data
        dataCollector.RecordTrial(
            participantId,
            currentTask.TrialNumber,
            currentTask.Condition,
            currentTask.StimulusType,
            reactionTime,
            correct
        );

        // Advance to next trial
        taskManager.GoToNextTask();
    }

    public void EndExperiment()
    {
        dataCollector.SaveData();
        Debug.Log("Data saved. Experiment complete.");
    }
}
```

### TrialResponder.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class TrialResponder : MmBaseResponder
{
    [SerializeField] private GameObject stimulus;
    [SerializeField] private float stimulusDuration = 0.5f;
    [SerializeField] private float responsePeriod = 1.0f;

    private float trialStartTime;
    private bool awaitingResponse;
    private bool responded;

    public override void Initialize()
    {
        base.Initialize();
        StartTrial();
    }

    void StartTrial()
    {
        responded = false;
        awaitingResponse = true;
        trialStartTime = Time.time;

        // Show stimulus
        if (stimulus != null)
        {
            stimulus.SetActive(true);
            Invoke(nameof(HideStimulus), stimulusDuration);
        }

        // End response period after timeout
        Invoke(nameof(EndResponsePeriod), responsePeriod);
    }

    void HideStimulus()
    {
        if (stimulus != null)
            stimulus.SetActive(false);
    }

    void Update()
    {
        if (awaitingResponse && Input.GetKeyDown(KeyCode.Space))
        {
            responded = true;
            float reactionTime = Time.time - trialStartTime;

            // Notify parent of response
            this.Send($"Response:{reactionTime:F3}")
                .ToParents()
                .Execute();
        }
    }

    void EndResponsePeriod()
    {
        awaitingResponse = false;

        if (!responded)
        {
            // No response - notify parent
            this.Send("NoResponse")
                .ToParents()
                .Execute();
        }
    }
}
```

---

## User Configuration

`MmTaskUserConfigurator` allows per-participant task ordering:

```csharp
using UnityEngine;
using MercuryMessaging.Task;

public class MyUserConfigurator : MmTaskUserConfigurator
{
    [SerializeField] private int participantId = 1;

    public override int GetParticipantId()
    {
        return participantId;
    }

    public override string GetTaskFilePath()
    {
        // Return different task file based on participant
        // Useful for counterbalancing conditions
        int group = participantId % 2;  // 0 or 1
        return $"Tasks/group_{group}_tasks.json";
    }
}
```

---

## Common Patterns

### Pattern 1: Practice Trials

```csharp
public void StartExperiment()
{
    // Start with practice trials
    while (CurrentTaskInfo.Condition == "practice")
    {
        // Run practice trial
        // User can repeat practice if needed
    }

    // Then continue to main trials
}
```

### Pattern 2: Break Between Blocks

```csharp
public void CheckForBreak()
{
    int trialsCompleted = CurrentTaskInfo.TaskIndex;
    int breakEvery = 20;

    if (trialsCompleted > 0 && trialsCompleted % breakEvery == 0)
    {
        ShowBreakScreen();
    }
}
```

### Pattern 3: Save Progress

```csharp
public void SaveProgress()
{
    PlayerPrefs.SetInt("LastCompletedTask", CurrentTaskInfo.TaskIndex);
    PlayerPrefs.Save();
}

public void LoadProgress()
{
    int lastTask = PlayerPrefs.GetInt("LastCompletedTask", 0);
    GoToTask(lastTask + 1);  // Resume from next task
}
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Tasks not loading | Check JSON format and file path |
| TasksNode is null | Assign in Inspector or find in hierarchy |
| Data not saving | Call `SaveToFile()` and check write permissions |
| Wrong task order | Verify task indices in definition file |

---

## Try This

Practice task management for experiments:

1. **Create a Stroop task** - Design tasks with congruent (RED in red) and incongruent (RED in blue) conditions. Track response times and accuracy.

2. **Implement counterbalancing** - Modify the task loader to reorder conditions based on participant ID (odd vs even).

3. **Add practice with feedback** - Show "Correct!" or "Try again" during practice trials, but not during main trials.

4. **Export to multiple formats** - Extend MmDataCollector to export both CSV and JSON files simultaneously.

---

## Next Steps

- **[Tutorial 10: Application State](Tutorial-10-Application-State)** - Global state management
- **[Tutorial 12: VR Behavioral Experiment](Tutorial-12-VR-Experiment)** - VR-specific task setup
- **MmDataCollector Documentation** - Advanced data collection features

---

*Tutorial 9 of 14 - MercuryMessaging Wiki*
