# Tutorial 12: VR Experiment

Scene setup instructions for Tutorial 12 Go/No-Go behavioral experiment.

## Scene Hierarchy

```
Tutorial12_VRExperiment
├── Main Camera (or XR Origin for VR)
├── Directional Light
├── ExperimentController (MmRelayNode + T12_GoNoGoController)
│   ├── DataLogger (MmRelayNode + T12_DataLogger)
│   ├── Stimuli
│   │   ├── GoStimulus (Green Circle/Sphere)
│   │   ├── NoGoStimulus (Red Circle/Sphere)
│   │   └── FixationCross
│   └── AudioSource
└── UI Canvas
    ├── Instructions Text
    └── Results Panel
```

## Setup Steps

1. Create new scene: `Tutorial12_VRExperiment.unity`
2. Add Main Camera (or XR Origin for VR testing)
3. Add Directional Light
4. Create "ExperimentController" GameObject
   - Add `MmRelayNode` component
   - Add `T12_GoNoGoController` script
   - Add `AudioSource` component
5. Create child "DataLogger"
   - Add `MmRelayNode` component
   - Add `T12_DataLogger` script
6. Create "Stimuli" container
   - Add green sphere (GoStimulus)
   - Add red sphere (NoGoStimulus)
   - Add fixation cross (3D text or sprite)
7. Assign references in T12_GoNoGoController:
   - goStimulus, noGoStimulus, fixationCross
   - audioSource (add sounds if available)
   - relayNode

## Controls

- **ENTER**: Start experiment
- **SPACE**: Respond to Go stimulus (keyboard fallback)
- VR Trigger: Respond to Go stimulus (when VR enabled)

## Configuration

In T12_GoNoGoController Inspector:
- `Total Trials`: Number of trials (default: 20)
- `Go Trial Ratio`: Percentage of Go trials (default: 0.7 = 70%)
- `Stimulus Duration`: How long stimulus shows (default: 1.5s)
- `Response Window`: Time allowed to respond (default: 2.0s)

## Data Output

T12_DataLogger saves experiment data to:
`<ProjectRoot>/ExperimentData/exp_<participant>_<session>_<timestamp>.csv`

CSV columns:
- timestamp_s: Seconds since experiment start
- event_type: EXPERIMENT_START, STIMULUS_ONSET, RESPONSE, EXPERIMENT_END
- event_data: Additional event details
- participant_id, session_id

## Learning Objectives

- Building complete behavioral experiments
- Message-based experiment event flow
- Data logging with MmBaseResponder
- VR/keyboard input abstraction
