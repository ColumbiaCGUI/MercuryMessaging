# Tutorial 10: Application State Management

## Overview

`MmAppStateResponder` and `MmSwitchResponder` provide a pattern for managing application-level state machines. Building on the FSM concepts from Tutorial 8, this tutorial shows how to structure entire applications around states like menus, gameplay, loading screens, and pause states.

## What You'll Learn

- Using `MmSwitchResponder` as a state controller
- Creating `MmAppStateResponder` for state-specific data
- Automatic state activation/deactivation
- Initial state configuration
- Integration with GUI handlers
- Building complete application flows

## Prerequisites

- Completed [Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)
- Understanding of `MmRelaySwitchNode` and `SelectedFilter`

---

## Architecture Overview

The application state system has three layers:

```
MmAppStateSwitchResponder (extends MmSwitchResponder)
  └── Controls: MmRelaySwitchNode
                  ├── State1 (MmRelayNode + MmAppStateResponder)
                  ├── State2 (MmRelayNode + MmAppStateResponder)
                  └── State3 (MmRelayNode + MmAppStateResponder)
```

| Component | Purpose |
|-----------|---------|
| `MmSwitchResponder` | Base controller for FSM-based state switching |
| `MmAppStateSwitchResponder` | Application-specific state controller |
| `MmAppStateResponder` | State-specific data and behavior |
| `MmRelaySwitchNode` | The underlying FSM (from Tutorial 8) |

---

## Step-by-Step Setup

### Step 1: Create the State Hierarchy

```
AppController (MmRelaySwitchNode + MyAppController)
  ├── MainMenu (MmRelayNode + MmAppStateResponder + MenuBehavior)
  │     └── MenuUI (Canvas, Buttons, etc.)
  ├── Loading (MmRelayNode + MmAppStateResponder + LoadingBehavior)
  │     └── LoadingUI (Progress bar, etc.)
  ├── Gameplay (MmRelayNode + MmAppStateResponder + GameplayBehavior)
  │     ├── Player
  │     ├── Enemies
  │     └── GameplayUI
  └── Pause (MmRelayNode + MmAppStateResponder + PauseBehavior)
        └── PauseUI
```

### Step 2: Create Your State Controller

```csharp
using UnityEngine;
using MercuryMessaging;

public class MyAppController : MmSwitchResponder
{
    // Set in Inspector or via code
    // InitialState inherited from MmSwitchResponder

    public override void Start()
    {
        base.Start();

        // The base class already sets up:
        // - GlobalEnter: Activates entering state
        // - GlobalExit: Deactivates exiting state

        Debug.Log("App state system initialized");
    }

    // Easy state switching methods
    public void GoToMainMenu() => SwitchTo("MainMenu");
    public void GoToGameplay() => SwitchTo("Gameplay");
    public void GoToPause() => SwitchTo("Pause");
    public void GoToLoading() => SwitchTo("Loading");

    private void SwitchTo(string stateName)
    {
        // Send Switch message to trigger FSM transition
        MmRelaySwitchNode.MmInvoke(
            MmMethod.Switch,
            stateName,
            new MmMetadataBlock(MmLevelFilter.Self)
        );
    }

    // Keyboard shortcuts for testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MmRelaySwitchNode.CurrentName == "Gameplay")
                GoToPause();
            else if (MmRelaySwitchNode.CurrentName == "Pause")
                GoToGameplay();
        }
    }
}
```

### Step 3: Configure Initial State

In the Inspector, set the `InitialState` field on your controller:

```
MyAppController
├── Initial State: "MainMenu"  ← Set this in Inspector
└── ...
```

Or set it programmatically:

```csharp
public class MyAppController : MmSwitchResponder
{
    void Awake()
    {
        InitialState = "MainMenu";  // Set before base.Awake()
        base.Awake();
    }
}
```

### Step 4: Create State-Specific Responders

```csharp
using UnityEngine;
using MercuryMessaging;

public class MenuBehavior : MmAppStateResponder
{
    [SerializeField] private GameObject menuCanvas;

    public override void SetActive(bool active)
    {
        // Don't call base - we manage visibility ourselves
        Debug.Log($"Menu state active: {active}");

        if (menuCanvas != null)
            menuCanvas.SetActive(active);

        if (active)
        {
            OnStateEnter();
        }
        else
        {
            OnStateExit();
        }
    }

    void OnStateEnter()
    {
        // Menu-specific setup
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
    }

    void OnStateExit()
    {
        // Menu-specific cleanup
    }
}
```

```csharp
public class GameplayBehavior : MmAppStateResponder
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameplayUI;

    public override void SetActive(bool active)
    {
        // Don't call base - we manage game object visibility ourselves
        if (player != null) player.SetActive(active);
        if (gameplayUI != null) gameplayUI.SetActive(active);

        if (active)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }
}
```

```csharp
public class PauseBehavior : MmAppStateResponder
{
    [SerializeField] private GameObject pauseCanvas;

    public override void SetActive(bool active)
    {
        // Don't call base - we manage pause canvas visibility ourselves
        if (pauseCanvas != null)
            pauseCanvas.SetActive(active);

        // Pause/resume time
        Time.timeScale = active ? 0f : 1f;

        if (active)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
```

---

## Automatic State Lifecycle

`MmSwitchResponder` automatically handles state activation:

### What Happens on State Transition

1. **Exit current state:**
   - `GlobalExit` delegate fires
   - Sends `SetActive(false)` to current state's children

2. **Enter new state:**
   - FSM transitions to new state
   - `GlobalEnter` delegate fires
   - Sends `SetActive(true)` to new state's children

### Built-in Behavior (from MmSwitchResponder)

```csharp
// Automatically registered in Start()
MmRelaySwitchNode.RespondersFSM.GlobalExit += delegate
{
    // Deactivate exiting state and its children
    MmRelaySwitchNode.Current.MmInvoke(
        MmMethod.SetActive, false,
        new MmMetadataBlock(MmLevelFilterHelper.Default, MmActiveFilter.All)
    );
};

MmRelaySwitchNode.RespondersFSM.GlobalEnter += delegate
{
    // Activate entering state and its children
    MmRelaySwitchNode.Current.MmInvoke(
        MmMethod.SetActive, true,
        new MmMetadataBlock(MmLevelFilterHelper.Default, MmActiveFilter.All)
    );
};
```

---

## Using with DSL API

Combine application state with the Fluent DSL:

```csharp
public class MyAppController : MmSwitchResponder
{
    private MmRelayNode relay;

    void Start()
    {
        base.Start();
        relay = GetComponent<MmRelayNode>();

        // Subscribe to FSM events
        MmRelaySwitchNode.RespondersFSM.GlobalEnter += OnAnyStateEnter;
    }

    void OnAnyStateEnter()
    {
        // Initialize the newly active state using DSL
        relay.Send(MmMethod.Initialize)
            .ToChildren()
            .Selected()  // Only current state
            .Execute();
    }

    public void BroadcastToCurrentState(string message)
    {
        // Send message only to active state
        relay.Send(message)
            .ToDescendants()
            .Selected()
            .Execute();
    }
}
```

---

## Extended Example: MmAppStateSwitchResponder

For more complex applications, extend `MmAppStateSwitchResponder`:

```csharp
using UnityEngine;
using System.Collections.Generic;
using MercuryMessaging;

public class GameAppController : MmAppStateSwitchResponder
{
    [Header("State Configuration")]
    [SerializeField] private string startState = "MainMenu";

    private Dictionary<string, float> stateEnterTimes = new Dictionary<string, float>();

    void Awake()
    {
        InitialState = startState;
        base.Awake();
    }

    public override void SetupAppStates()
    {
        base.SetupAppStates();

        // Register enter/exit callbacks for each state
        var fsm = MmRelaySwitchNode.RespondersFSM;

        foreach (var item in MmRelaySwitchNode.RoutingTable)
        {
            string stateName = item.Name;

            fsm[item].Enter += () =>
            {
                stateEnterTimes[stateName] = Time.time;
                Debug.Log($"Entered {stateName}");
                OnStateEntered(stateName);
            };

            fsm[item].Exit += () =>
            {
                float duration = Time.time - stateEnterTimes.GetValueOrDefault(stateName, Time.time);
                Debug.Log($"Exited {stateName} after {duration:F1}s");
                OnStateExited(stateName);
            };
        }
    }

    protected virtual void OnStateEntered(string stateName)
    {
        switch (stateName)
        {
            case "Loading":
                StartCoroutine(LoadGameAsync());
                break;
            case "Gameplay":
                // Start game systems
                break;
        }
    }

    protected virtual void OnStateExited(string stateName)
    {
        switch (stateName)
        {
            case "Gameplay":
                // Save game progress
                break;
        }
    }

    private System.Collections.IEnumerator LoadGameAsync()
    {
        // Simulate async loading
        yield return new WaitForSeconds(2f);

        // Transition to gameplay when done
        MmRelaySwitchNode.JumpTo("Gameplay");
    }
}
```

---

## Complete Application Flow Example

### Hierarchy

```
App (MmRelaySwitchNode + GameAppController)
  ├── Splash (MmRelayNode + SplashState)
  │     └── SplashCanvas
  ├── MainMenu (MmRelayNode + MenuState)
  │     ├── MenuCanvas
  │     │     ├── PlayButton
  │     │     ├── OptionsButton
  │     │     └── QuitButton
  │     └── BackgroundMusic (AudioSource)
  ├── Options (MmRelayNode + OptionsState)
  │     └── OptionsCanvas
  ├── Loading (MmRelayNode + LoadingState)
  │     └── LoadingCanvas
  │           └── ProgressBar
  ├── Gameplay (MmRelayNode + GameplayState)
  │     ├── Player
  │     ├── World
  │     └── GameplayHUD
  └── Pause (MmRelayNode + PauseState)
        └── PauseCanvas
```

### State Flow Diagram

```
[Splash] ──(auto)──> [MainMenu] ──(Play)──> [Loading] ──(done)──> [Gameplay]
                          │                                            │
                          │<──────────(Back)───────[Options]           │
                          │                                            │
                          └<─────────(Resume)──────[Pause]<──(Esc)─────┘
```

### Implementation

```csharp
// SplashState.cs
public class SplashState : MmAppStateResponder
{
    [SerializeField] private float displayTime = 3f;
    private MmSwitchResponder controller;

    void Start()
    {
        controller = GetComponentInParent<MmSwitchResponder>();
    }

    public override void SetActive(bool active)
    {
        // Don't call base - we manage auto-advance ourselves
        if (active)
        {
            StartCoroutine(AutoAdvance());
        }
    }

    System.Collections.IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(displayTime);
        controller.MmRelaySwitchNode.JumpTo("MainMenu");
    }
}

// MenuState.cs
public class MenuState : MmAppStateResponder
{
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private UnityEngine.UI.Button optionsButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;

    private MmSwitchResponder controller;

    void Start()
    {
        controller = GetComponentInParent<MmSwitchResponder>();

        playButton?.onClick.AddListener(() =>
            controller.MmRelaySwitchNode.JumpTo("Loading"));

        optionsButton?.onClick.AddListener(() =>
            controller.MmRelaySwitchNode.JumpTo("Options"));

        quitButton?.onClick.AddListener(() =>
            Application.Quit());
    }
}
```

---

## StateText Property

`MmAppStateResponder` includes a `StateText` property for UI display:

```csharp
public class LoadingState : MmAppStateResponder
{
    [SerializeField] private TMPro.TextMeshProUGUI statusText;

    void Start()
    {
        StateText = "Loading...";  // Inherited from MmAppStateResponder
    }

    void Update()
    {
        if (statusText != null)
            statusText.text = StateText;
    }

    public void UpdateProgress(float progress)
    {
        StateText = $"Loading... {progress * 100:F0}%";
    }
}
```

---

## Integration with MmGuiHandler

`MmAppStateSwitchResponder` automatically connects to `MmGuiHandler` singleton:

```csharp
public class MyAppController : MmAppStateSwitchResponder
{
    public override void SetupAppStates()
    {
        base.SetupAppStates();  // Gets MmGuiHandler.Instance

        if (MmGuiHandler != null)
        {
            Debug.Log("GUI Handler connected");
            // Use MmGuiHandler for global UI operations
        }
    }
}
```

---

## Common Patterns

### Pattern 1: Nested State Machines

```
App (MmRelaySwitchNode)
  └── Gameplay (MmRelayNode + MmRelaySwitchNode)  ← Nested FSM!
        ├── Exploration (MmRelayNode)
        ├── Combat (MmRelayNode)
        └── Dialogue (MmRelayNode)
```

### Pattern 2: State with Sub-states

```csharp
public class GameplayController : MmSwitchResponder
{
    // This is a nested FSM within the main App FSM
    // It has its own states: Exploration, Combat, Dialogue

    public override void SetActive(bool active)
    {
        // Don't call base - we manage sub-FSM ourselves
        if (active)
        {
            // Reset to initial sub-state when gameplay starts
            MmRelaySwitchNode.JumpTo("Exploration");
        }
    }
}
```

### Pattern 3: State History Stack

```csharp
public class MyAppController : MmSwitchResponder
{
    private Stack<string> stateHistory = new Stack<string>();

    public void GoTo(string stateName)
    {
        stateHistory.Push(MmRelaySwitchNode.CurrentName);
        MmRelaySwitchNode.JumpTo(stateName);
    }

    public void GoBack()
    {
        if (stateHistory.Count > 0)
        {
            string previousState = stateHistory.Pop();
            MmRelaySwitchNode.JumpTo(previousState);
        }
    }
}
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| States not activating | Check `InitialState` is set to valid state name |
| State children not responding | Ensure child GameObjects have `MmRelayNode` components |
| Time not pausing/resuming | Set `Time.timeScale` in state's `SetActive` override |
| Multiple states active | Don't call `SetActive` manually, let FSM handle it |
| Missing base.Start() | Always call `base.Start()` in derived classes |

---

## Performance Considerations

1. **State Preloading**: Consider keeping inactive states loaded but hidden
2. **Scene Management**: Use `MmAppStateSwitchResponder` with Unity scene loading for large games
3. **Memory**: Unload unused assets when exiting states like "Gameplay"

---

## Try This

Practice application state management:

1. **Add a Credits screen** - Add a new Credits state accessible from MainMenu. Implement a Back button that uses state history.

2. **Implement state persistence** - Save the current state to PlayerPrefs when exiting. Restore it on next app launch.

3. **Create loading progress** - In the Loading state, simulate loading multiple assets (scenes, textures) and update a progress bar in real time.

4. **Build a settings system** - Create an Options state that modifies and saves audio volume, graphics quality, and control sensitivity.

---

## Next Steps

- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Network state synchronization
- **[Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)** - FSM fundamentals (review)
- **[Tutorial 9: Task Management](Tutorial-9-Task-Management)** - Combine state with experiments

---

*Tutorial 10 of 14 - MercuryMessaging Wiki*
