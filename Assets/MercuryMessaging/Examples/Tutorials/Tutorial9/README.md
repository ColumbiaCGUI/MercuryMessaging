# Tutorial 9: Task Management

Scene setup instructions for Tutorial 9 MmTaskManager for experiments.

## Scene Hierarchy

```
Tutorial9_Tasks
├── Main Camera
├── Directional Light
├── ExperimentManager (MmRelayNode + T9_ExperimentManager)
│   ├── TaskLoader (T9_JsonTaskLoader)
│   └── TrialDisplay (MmRelayNode + T9_TrialResponder)
│       ├── StimulusObject
│       └── ResponseTarget
├── UI Canvas
│   ├── Task Info Panel
│   ├── Progress Text
│   └── Instructions
└── StreamingAssets/
    └── tasks.json (task configuration file)
```

## Setup Steps

1. Create new scene: `Tutorial9_Tasks.unity`
2. Create "ExperimentManager" with MmRelayNode and T9_ExperimentManager
3. Add child "TaskLoader" with T9_JsonTaskLoader
4. Create "TrialDisplay" with T9_TrialResponder for showing trials
5. Create UI for displaying task progress
6. Create tasks.json in StreamingAssets folder

## tasks.json Example

```json
{
  "experimentName": "Perception Study",
  "tasks": [
    {
      "id": 1,
      "name": "Practice",
      "stimulusType": "circle",
      "targetColor": "green",
      "duration": 2.0,
      "isPractice": true
    },
    {
      "id": 2,
      "name": "Trial 1",
      "stimulusType": "square",
      "targetColor": "red",
      "duration": 1.5,
      "isPractice": false
    }
  ]
}
```

## Controls

- **ENTER**: Start experiment
- **SPACE**: Advance to next trial
- **R**: Repeat current trial
- **S**: Skip trial (practice only)

## Learning Objectives

- MmTaskManager for sequential task execution
- IMmTaskInfo for custom task definitions
- JSON task loading and parsing
- Task state events (Started, Completed, Skipped)
- Practice vs main trial handling
