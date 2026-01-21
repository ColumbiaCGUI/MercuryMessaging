# Tutorial 12: VR Behavioral Experiment

## Overview

This tutorial demonstrates how to build a VR behavioral experiment (Go/No-Go task) using MercuryMessaging. You'll learn to combine the task management system, VR input handling, and data collection into a complete research application.

## What You'll Learn

- Setting up VR input with XR Interaction Toolkit
- Creating task-based experiment workflows
- Collecting and exporting behavioral data
- Implementing stimulus presentation and response collection
- Building a complete Go/No-Go cognitive task

## Prerequisites

- Completed [Tutorial 9: Task Management](Tutorial-9-Task-Management)
- Unity XR Interaction Toolkit installed
- VR headset for testing (Quest, Vive, etc.)

---

## Go/No-Go Task Overview

The Go/No-Go task is a classic cognitive psychology paradigm:

1. **Stimuli**: Present visual targets (Go) and non-targets (No-Go)
2. **Response**: Press button for Go stimuli, withhold for No-Go
3. **Measure**: Reaction time, accuracy, false alarms

```
Trial Timeline:
[Fixation] → [Stimulus] → [Response Window] → [Feedback] → [ITI]
   500ms       500ms         1000ms             200ms       500ms
```

---

## Architecture Overview

```
Experiment (MmRelayNode + ExperimentController)
  ├── TaskManager (MmTaskManager + ExperimentConfig)
  │     └── TasksNode (MmRelaySwitchNode)
  │           ├── Instructions (MmRelayNode + InstructionsState)
  │           ├── Practice (MmRelayNode + TrialState)
  │           ├── MainBlock (MmRelayNode + TrialState)
  │           └── Completion (MmRelayNode + CompletionState)
  ├── StimulusManager (MmRelayNode + StimulusController)
  │     ├── Fixation (MmBaseResponder)
  │     ├── GoTarget (MmBaseResponder)
  │     └── NoGoTarget (MmBaseResponder)
  ├── InputManager (MmRelayNode + VRInputHandler)
  │     ├── LeftHand (MmBaseResponder)
  │     └── RightHand (MmBaseResponder)
  └── DataCollector (MmDataCollector + DataExporter)
```

---

## Step-by-Step Implementation

### Step 1: Define Trial Info

```csharp
using System;
using MercuryMessaging.Task;

[Serializable]
public class GoNoGoTrialInfo : IMmTaskInfo
{
    // Required by IMmTaskInfo
    public string TaskName { get; set; }
    public int TaskIndex { get; set; }
    public bool IsComplete { get; set; }

    // Trial parameters
    public int TrialNumber { get; set; }
    public bool IsGoTrial { get; set; }  // true = Go, false = No-Go
    public string StimulusType { get; set; }  // "circle", "square"
    public float StimulusDuration { get; set; }
    public float ResponseWindow { get; set; }

    // Collected data
    public bool Responded { get; set; }
    public float ReactionTime { get; set; }
    public bool WasCorrect { get; set; }
}
```

### Step 2: Create Trial File (JSON)

**trials.json:**
```json
{
    "trials": [
        {
            "TaskName": "Practice",
            "TaskIndex": 0,
            "TrialNumber": 1,
            "IsGoTrial": true,
            "StimulusType": "circle",
            "StimulusDuration": 0.5,
            "ResponseWindow": 1.0
        },
        {
            "TaskName": "Practice",
            "TaskIndex": 1,
            "TrialNumber": 2,
            "IsGoTrial": false,
            "StimulusType": "square",
            "StimulusDuration": 0.5,
            "ResponseWindow": 1.0
        },
        {
            "TaskName": "MainBlock",
            "TaskIndex": 2,
            "TrialNumber": 1,
            "IsGoTrial": true,
            "StimulusType": "circle",
            "StimulusDuration": 0.5,
            "ResponseWindow": 1.0
        }
    ]
}
```

### Step 3: Stimulus Controller

```csharp
using UnityEngine;
using MercuryMessaging;

public class StimulusController : MmBaseResponder
{
    [Header("Stimulus Objects")]
    [SerializeField] private GameObject fixationCross;
    [SerializeField] private GameObject goStimulus;    // Circle
    [SerializeField] private GameObject noGoStimulus;  // Square

    [Header("Materials")]
    [SerializeField] private Material greenMaterial;   // Correct feedback
    [SerializeField] private Material redMaterial;     // Incorrect feedback
    [SerializeField] private Material defaultMaterial;

    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        HideAll();
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        switch (msg.value)
        {
            case "ShowFixation":
                ShowFixation();
                break;
            case "ShowGo":
                ShowGo();
                break;
            case "ShowNoGo":
                ShowNoGo();
                break;
            case "HideAll":
                HideAll();
                break;
            case "CorrectFeedback":
                ShowFeedback(true);
                break;
            case "IncorrectFeedback":
                ShowFeedback(false);
                break;
        }
    }

    void ShowFixation()
    {
        HideAll();
        fixationCross.SetActive(true);
    }

    void ShowGo()
    {
        HideAll();
        goStimulus.SetActive(true);
    }

    void ShowNoGo()
    {
        HideAll();
        noGoStimulus.SetActive(true);
    }

    void HideAll()
    {
        fixationCross.SetActive(false);
        goStimulus.SetActive(false);
        noGoStimulus.SetActive(false);
    }

    void ShowFeedback(bool correct)
    {
        var activeStimulus = goStimulus.activeSelf ? goStimulus : noGoStimulus;
        if (activeStimulus.activeSelf)
        {
            var renderer = activeStimulus.GetComponent<Renderer>();
            renderer.material = correct ? greenMaterial : redMaterial;
            Invoke(nameof(ResetMaterial), 0.2f);
        }
    }

    void ResetMaterial()
    {
        goStimulus.GetComponent<Renderer>().material = defaultMaterial;
        noGoStimulus.GetComponent<Renderer>().material = defaultMaterial;
    }
}
```

### Step 4: VR Input Handler

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using MercuryMessaging;

public class VRInputHandler : MmBaseResponder
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference triggerAction;
    [SerializeField] private InputActionReference buttonAAction;

    private MmRelayNode relay;
    private bool inputEnabled = false;
    private float responseStartTime;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        if (triggerAction != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.performed += OnButtonPressed;
        }

        if (buttonAAction != null)
        {
            buttonAAction.action.Enable();
            buttonAAction.action.performed += OnButtonPressed;
        }
    }

    void OnDestroy()
    {
        if (triggerAction != null)
            triggerAction.action.performed -= OnButtonPressed;
        if (buttonAAction != null)
            buttonAAction.action.performed -= OnButtonPressed;
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        if (msg.value == "EnableInput")
        {
            inputEnabled = true;
            responseStartTime = Time.time;
        }
        else if (msg.value == "DisableInput")
        {
            inputEnabled = false;
        }
    }

    void OnButtonPressed(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;

        float reactionTime = Time.time - responseStartTime;

        // Notify parent of response
        relay.Send($"Response:{reactionTime:F3}")
            .ToParents()
            .Execute();

        inputEnabled = false;  // Prevent multiple responses
    }
}
```

### Step 5: Experiment Controller

```csharp
using UnityEngine;
using System.Collections;
using MercuryMessaging;
using MercuryMessaging.Task;
using MercuryMessaging.Data;

public class ExperimentController : MmTaskManager<GoNoGoTrialInfo>
{
    [Header("Timing (seconds)")]
    [SerializeField] private float fixationDuration = 0.5f;
    [SerializeField] private float stimulusDuration = 0.5f;
    [SerializeField] private float responseWindow = 1.0f;
    [SerializeField] private float feedbackDuration = 0.2f;
    [SerializeField] private float interTrialInterval = 0.5f;

    [Header("References")]
    [SerializeField] private MmRelayNode stimulusNode;
    [SerializeField] private MmRelayNode inputNode;
    [SerializeField] private MmDataCollector dataCollector;

    private MmRelayNode relay;
    private bool waitingForResponse;
    private bool responded;
    private float reactionTime;

    protected override void Start()
    {
        base.Start();
        relay = GetComponent<MmRelayNode>();

        // Setup data collector headers
        dataCollector.SetHeaders(new string[]
        {
            "ParticipantID", "TrialNumber", "IsGoTrial", "StimulusType",
            "Responded", "ReactionTime", "WasCorrect", "Timestamp"
        });

        // Subscribe to FSM events
        if (TasksNode != null && TasksNode.RespondersFSM != null)
        {
            TasksNode.RespondersFSM.GlobalEnter += OnTrialStateEnter;
        }
    }

    void OnTrialStateEnter()
    {
        if (CurrentTaskInfo != null)
        {
            StartCoroutine(RunTrial(CurrentTaskInfo));
        }
    }

    IEnumerator RunTrial(GoNoGoTrialInfo trial)
    {
        responded = false;
        reactionTime = 0f;

        // 1. Show fixation
        SendToStimulus("ShowFixation");
        yield return new WaitForSeconds(fixationDuration);

        // 2. Show stimulus
        if (trial.IsGoTrial)
            SendToStimulus("ShowGo");
        else
            SendToStimulus("ShowNoGo");

        // 3. Enable input and wait for response
        SendToInput("EnableInput");
        waitingForResponse = true;

        float elapsed = 0f;
        while (elapsed < responseWindow && !responded)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        waitingForResponse = false;
        SendToInput("DisableInput");

        // 4. Calculate correctness
        bool correct = (trial.IsGoTrial && responded) || (!trial.IsGoTrial && !responded);

        // 5. Show feedback
        SendToStimulus(correct ? "CorrectFeedback" : "IncorrectFeedback");
        yield return new WaitForSeconds(feedbackDuration);

        SendToStimulus("HideAll");

        // 6. Record data
        trial.Responded = responded;
        trial.ReactionTime = responded ? reactionTime : -1f;
        trial.WasCorrect = correct;
        trial.IsComplete = true;

        RecordTrial(trial);

        // 7. Inter-trial interval
        yield return new WaitForSeconds(interTrialInterval);

        // 8. Advance to next trial
        AdvanceToNextTrial();
    }

    void SendToStimulus(string message)
    {
        stimulusNode.Send(message).ToDescendants().Execute();
    }

    void SendToInput(string message)
    {
        inputNode.Send(message).ToDescendants().Execute();
    }

    // Called by VRInputHandler via message
    protected override void ReceivedMessage(MmMessageString msg)
    {
        if (msg.value.StartsWith("Response:"))
        {
            responded = true;
            float.TryParse(msg.value.Substring(9), out reactionTime);
        }
    }

    void RecordTrial(GoNoGoTrialInfo trial)
    {
        dataCollector.AddRow(new string[]
        {
            "P001",  // Participant ID (get from config)
            trial.TrialNumber.ToString(),
            trial.IsGoTrial.ToString(),
            trial.StimulusType,
            trial.Responded.ToString(),
            trial.ReactionTime.ToString("F3"),
            trial.WasCorrect.ToString(),
            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    void AdvanceToNextTrial()
    {
        if (NextTaskInfo != null)
        {
            currentTaskInfo = currentTaskInfo.Next;
            SyncTasksNode();
        }
        else
        {
            OnExperimentComplete();
        }
    }

    void OnExperimentComplete()
    {
        dataCollector.SaveToFile($"results_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        Debug.Log("Experiment complete! Data saved.");

        // Transition to completion state
        TasksNode.JumpTo("Completion");
    }

    private void SyncTasksNode()
    {
        if (CurrentTaskInfo != null)
        {
            TasksNode.JumpTo(CurrentTaskInfo.TaskName);
        }
    }
}
```

---

## VR-Specific Considerations

### Controller Mapping

```csharp
// Using XR Interaction Toolkit Input Actions
[Header("XR Input")]
[SerializeField] private InputActionReference primaryButton;
[SerializeField] private InputActionReference triggerButton;

// Common mappings:
// - Quest: A/B buttons, triggers, grip
// - Vive: Trackpad press, trigger, grip
// - Index: A/B buttons, triggers, grip, touchpad
```

### Eye Tracking Integration

```csharp
using MercuryMessaging.StandardLibrary.Input;

public class EyeTrackingHandler : MmInputResponder
{
    protected override void ReceivedGaze(MmInputGazeMessage msg)
    {
        if (msg.Confidence > 0.8f)
        {
            // Use gaze point for attention tracking
            Debug.Log($"Gaze at: {msg.HitPoint}");
        }
    }
}
```

### Haptic Feedback

```csharp
// Send haptic feedback for responses
relay.MmInvoke(new MmInputHapticMessage(
    hand: MmHandedness.Right,
    intensity: 0.5f,
    duration: 0.1f,
    frequency: 100f
));
```

---

## Data Collection Best Practices

### 1. Comprehensive Logging

```csharp
// Log every trial event for analysis
dataCollector.AddRow(new string[]
{
    sessionId,
    blockNumber.ToString(),
    trialNumber.ToString(),
    condition,
    stimulusOnsetTime.ToString("F6"),  // High precision
    responseTime.ToString("F6"),
    accuracy.ToString(),
    headPosition.ToString(),
    gazeDirection.ToString()
});
```

### 2. Save Incrementally

```csharp
// Save after each block to prevent data loss
void OnBlockComplete()
{
    string filename = $"data/session_{sessionId}_block_{blockNumber}.csv";
    dataCollector.SaveToFile(filename);
    dataCollector.Clear();  // Start fresh for next block
}
```

### 3. Include Metadata

```csharp
// Create metadata file alongside data
void SaveMetadata()
{
    var metadata = new Dictionary<string, string>
    {
        {"ParticipantID", participantId},
        {"Date", DateTime.Now.ToString("yyyy-MM-dd")},
        {"HeadsetType", XRSettings.loadedDeviceName},
        {"UnityVersion", Application.unityVersion},
        {"TrialCount", totalTrials.ToString()}
    };

    string json = JsonUtility.ToJson(metadata);
    File.WriteAllText($"data/session_{sessionId}_metadata.json", json);
}
```

---

## Common Experiment Patterns

### Pattern 1: Block Design

```csharp
public class BlockedExperiment : MmTaskManager<TrialInfo>
{
    [SerializeField] private int trialsPerBlock = 20;

    void OnTrialComplete()
    {
        if (CurrentTaskInfo.TrialNumber % trialsPerBlock == 0)
        {
            // Show break screen
            TasksNode.JumpTo("Break");
        }
        else
        {
            AdvanceToNextTrial();
        }
    }
}
```

### Pattern 2: Adaptive Difficulty

```csharp
void AdjustDifficulty()
{
    float recentAccuracy = CalculateRecentAccuracy(10);  // Last 10 trials

    if (recentAccuracy > 0.9f)
    {
        stimulusDuration *= 0.9f;  // Make harder
    }
    else if (recentAccuracy < 0.7f)
    {
        stimulusDuration *= 1.1f;  // Make easier
    }
}
```

### Pattern 3: Counterbalanced Conditions

```csharp
// Load different trial orders based on participant number
void LoadTrialOrder(int participantNumber)
{
    int order = participantNumber % 4;  // 4 counterbalance conditions
    string filename = $"trials_order{order}.json";
    LoadTrials(filename);
}
```

---

## Complete Hierarchy

```
ExperimentRoot
├── XR Origin
│     ├── Camera Offset
│     │     └── Main Camera
│     ├── LeftHand Controller (XR Controller + MmRelayNode + HandResponder)
│     └── RightHand Controller (XR Controller + MmRelayNode + HandResponder)
│
├── ExperimentManager (MmRelayNode + ExperimentController + TrialLoader)
│     └── TasksNode (MmRelaySwitchNode)
│           ├── Instructions (MmRelayNode + InstructionsUI)
│           ├── Practice (MmRelayNode + PracticeState)
│           ├── MainBlock (MmRelayNode + MainBlockState)
│           ├── Break (MmRelayNode + BreakUI)
│           └── Completion (MmRelayNode + CompletionUI)
│
├── StimulusManager (MmRelayNode + StimulusController)
│     ├── FixationCross (MmBaseResponder)
│     ├── GoStimulus (MmBaseResponder)
│     └── NoGoStimulus (MmBaseResponder)
│
├── InputManager (MmRelayNode + VRInputHandler)
│
└── DataManager (MmDataCollector + DataExporter)
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Input not registering | Check InputActionReference is assigned |
| Timing inaccurate | Use `Time.realtimeSinceStartup` for precision |
| Stimuli not appearing | Verify Z-position in VR space |
| Data not saving | Check file permissions, use Application.persistentDataPath |
| FSM stuck | Verify state names match exactly |

---

## Try This

Practice building VR experiments:

1. **Add a Stroop condition** - Extend the Go/No-Go task with a Stroop condition where the stimulus color conflicts with the shape. Track whether congruent vs incongruent trials affect reaction time.

2. **Implement practice with feedback** - Show "Correct!" or "Too slow!" text feedback only during practice trials. Main block trials should show no feedback.

3. **Add eye tracking data** - If you have an eye-tracking headset, log gaze position at stimulus onset and response. Calculate time-to-first-fixation as an additional dependent variable.

4. **Build a break system** - Add automatic breaks every 20 trials. During breaks, display accuracy and average reaction time for the previous block. Let participants resume when ready.

---

## Next Steps

- **[Tutorial 9: Task Management](Tutorial-9-Task-Management)** - Review task system (if needed)
- **[Tutorial 13: Spatial & Temporal Filtering](Tutorial-13-Spatial-Temporal)** - Advanced filtering
- **Standard Library Documentation** - MmInputResponder details

---

*Tutorial 12 of 14 - MercuryMessaging Wiki*
