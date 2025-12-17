# Tutorial 8: Switch Nodes & FSM

## Overview

`MmRelaySwitchNode` is an FSM-enabled relay node that activates different branches of your hierarchy based on state. This is perfect for game states (Menu, Gameplay, Pause), UI screens, or any scenario where only one "mode" should be active at a time.

## What You'll Learn

- Using `MmRelaySwitchNode` for state-based messaging
- The `SelectedFilter.Selected` filter
- FSM navigation methods: `JumpTo()`, `GoToPrevious()`
- State change events and callbacks
- Practical game state machine example

## Prerequisites

- Completed [Tutorial 1](Tutorial-1-Introduction) through [Tutorial 5](Tutorial-5-Fluent-DSL-API)
- Understanding of basic message routing

---

## Core Concept: Selected Filter

When you use `MmRelaySwitchNode`, messages with `SelectedFilter.Selected` only reach responders in the **currently active state**:

```
GameManager (MmRelaySwitchNode) ← FSM controls which branch is "selected"
  ├── MainMenu (MmRelayNode)     ← Selected when state = "MainMenu"
  │     └── MenuUI
  ├── Gameplay (MmRelayNode)     ← Selected when state = "Gameplay"
  │     ├── Player
  │     └── Enemies
  └── Pause (MmRelayNode)        ← Selected when state = "Pause"
        └── PauseUI
```

When `state = "Gameplay"`:
- Messages with `.Selected()` → Only Gameplay branch receives
- Messages without `.Selected()` → All branches receive

---

## Step-by-Step Setup

### Step 1: Create the Hierarchy

```
GameManager (MmRelaySwitchNode + GameStateController)
  ├── MainMenu (MmRelayNode + MenuResponder)
  ├── Gameplay (MmRelayNode + GameplayResponder)
  └── PauseMenu (MmRelayNode + PauseResponder)
```

Each direct child of `MmRelaySwitchNode` becomes a **state** in the FSM.

### Step 2: Access the FSM

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start()
    {
        switchNode = GetComponent<MmRelaySwitchNode>();

        // Access the FSM
        var fsm = switchNode.RespondersFSM;

        // Check current state
        Debug.Log($"Current state: {switchNode.CurrentName}");
    }
}
```

### Step 3: Navigate Between States

```csharp
public class GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start()
    {
        switchNode = GetComponent<MmRelaySwitchNode>();

        // Start at MainMenu
        GoToState("MainMenu");
    }

    public void GoToState(string stateName)
    {
        // Find the state by name and jump to it
        var fsm = switchNode.RespondersFSM;

        foreach (var item in switchNode.RoutingTable)
        {
            if (item.Name == stateName)
            {
                fsm.JumpTo(item);
                Debug.Log($"Switched to: {stateName}");
                return;
            }
        }

        Debug.LogWarning($"State not found: {stateName}");
    }

    void Update()
    {
        // Press Escape to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (switchNode.CurrentName == "PauseMenu")
                GoToState("Gameplay");
            else if (switchNode.CurrentName == "Gameplay")
                GoToState("PauseMenu");
        }

        // Press Enter to start game
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (switchNode.CurrentName == "MainMenu")
                GoToState("Gameplay");
        }
    }
}
```

---

## Using Selected Filter

### Send Only to Active State

```csharp
// Only the currently selected state receives this
switchNode.Send("Refresh")
    .ToDescendants()
    .Selected()  // Key filter!
    .Execute();

// Or using broadcast (default goes to descendants)
switchNode.BroadcastRefresh();  // Reaches all descendants

// To reach only selected:
switchNode.Send(MmMethod.Refresh)
    .ToChildren()
    .Selected()
    .Execute();
```

### Traditional API

```csharp
switchNode.MmInvoke(
    MmMethod.Refresh,
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active,
        MmSelectedFilter.Selected,  // Only selected state
        MmNetworkFilter.Local
    )
);
```

---

## State Change Events

Subscribe to FSM events for state transitions:

```csharp
public class GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;

    void Start()
    {
        switchNode = GetComponent<MmRelaySwitchNode>();
        var fsm = switchNode.RespondersFSM;

        // Global events (called on every state change)
        fsm.GlobalEnter += OnAnyStateEnter;
        fsm.GlobalExit += OnAnyStateExit;

        // Per-state events
        foreach (var item in switchNode.RoutingTable)
        {
            if (item.Name == "Gameplay")
            {
                fsm[item].Enter += OnGameplayEnter;
                fsm[item].Exit += OnGameplayExit;
            }
            else if (item.Name == "PauseMenu")
            {
                fsm[item].Enter += OnPauseEnter;
            }
        }
    }

    void OnAnyStateEnter()
    {
        Debug.Log($"Entered state: {switchNode.CurrentName}");
    }

    void OnAnyStateExit()
    {
        Debug.Log($"Exited state: {switchNode.RespondersFSM.Previous}");
    }

    void OnGameplayEnter()
    {
        Debug.Log("Starting gameplay!");
        Time.timeScale = 1f;  // Resume time
    }

    void OnGameplayExit()
    {
        Debug.Log("Leaving gameplay");
    }

    void OnPauseEnter()
    {
        Debug.Log("Game paused");
        Time.timeScale = 0f;  // Pause time
    }
}
```

---

## Complete Example: Game State Machine

### GameStateController.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameStateController : MonoBehaviour
{
    private MmRelaySwitchNode switchNode;
    private FiniteStateMachine<MmRoutingTableItem> fsm;

    // State names (match child GameObject names)
    public const string STATE_MAIN_MENU = "MainMenu";
    public const string STATE_GAMEPLAY = "Gameplay";
    public const string STATE_PAUSE = "PauseMenu";
    public const string STATE_GAME_OVER = "GameOver";

    void Start()
    {
        switchNode = GetComponent<MmRelaySwitchNode>();
        fsm = switchNode.RespondersFSM;

        // Setup state callbacks
        SetupStateCallbacks();

        // Start at main menu
        GoToMainMenu();
    }

    void SetupStateCallbacks()
    {
        fsm.GlobalEnter += () =>
        {
            // Initialize the new state
            switchNode.Send(MmMethod.Initialize)
                .ToChildren()
                .Selected()
                .Execute();
        };
    }

    // Public methods for state transitions
    public void GoToMainMenu() => JumpToState(STATE_MAIN_MENU);
    public void StartGame() => JumpToState(STATE_GAMEPLAY);
    public void PauseGame() => JumpToState(STATE_PAUSE);
    public void ResumeGame() => JumpToState(STATE_GAMEPLAY);
    public void GameOver() => JumpToState(STATE_GAME_OVER);

    private void JumpToState(string stateName)
    {
        foreach (var item in switchNode.RoutingTable)
        {
            if (item.Name == stateName)
            {
                fsm.JumpTo(item);
                return;
            }
        }
    }

    // Check current state
    public bool IsInState(string stateName) => switchNode.CurrentName == stateName;
    public bool IsPlaying => IsInState(STATE_GAMEPLAY);
    public bool IsPaused => IsInState(STATE_PAUSE);

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPlaying) PauseGame();
            else if (IsPaused) ResumeGame();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (IsInState(STATE_MAIN_MENU)) StartGame();
            else if (IsInState(STATE_GAME_OVER)) GoToMainMenu();
        }
    }
}
```

### MenuResponder.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class MenuResponder : MmBaseResponder
{
    [SerializeField] private GameObject menuCanvas;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Menu initialized");
        if (menuCanvas != null)
            menuCanvas.SetActive(true);
    }

    public override void SetActive(bool active)
    {
        // Don't call base - we manage canvas visibility ourselves
        if (menuCanvas != null)
            menuCanvas.SetActive(active);
    }
}
```

### GameplayResponder.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameplayResponder : MmBaseResponder
{
    private int score = 0;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Gameplay starting!");
        score = 0;
        Time.timeScale = 1f;
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        if (msg.value == "AddScore")
        {
            score += 10;
            Debug.Log($"Score: {score}");
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;  // Ensure time is restored
    }
}
```

---

## FSM Properties and Methods

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Current` | `T` | Current state |
| `Previous` | `T` | Previous state |
| `Next` | `T` | State being transitioned to |
| `CurrentName` | `string` | Name of current state (MmRelaySwitchNode) |

### Methods

| Method | Description |
|--------|-------------|
| `JumpTo(state)` | Immediately transition to state |
| `StartTransitionTo(state)` | Begin transition (for animated transitions) |
| `EnterNext()` | Complete pending transition |
| `CancelStateChange()` | Cancel pending transition |

### Events

| Event | When Fired |
|-------|------------|
| `GlobalEnter` | After entering any state |
| `GlobalExit` | After exiting any state |
| `[state].Enter` | After entering specific state |
| `[state].Exit` | After exiting specific state |

---

## Common Patterns

### Pattern 1: Animated Transitions

```csharp
public IEnumerator AnimatedTransition(string newState)
{
    // Start transition (fires Exit events)
    fsm.StartTransitionTo(FindState(newState));

    // Play fade out animation
    yield return StartCoroutine(FadeOut());

    // Complete transition (fires Enter events)
    fsm.EnterNext();

    // Play fade in animation
    yield return StartCoroutine(FadeIn());
}
```

### Pattern 2: State-Specific Messages

```csharp
// Only active state handles this
switchNode.Send("UpdateUI")
    .ToDescendants()
    .Selected()
    .Execute();

// All states handle this (for global events)
switchNode.Send("SaveProgress")
    .ToDescendants()
    .AllSelected()
    .Execute();
```

### Pattern 3: Query Current State

```csharp
public void HandlePlayerDeath()
{
    if (switchNode.CurrentName == "Gameplay")
    {
        // Player died during gameplay - show game over
        JumpToState("GameOver");
    }
    // Ignore if already in menu/pause
}
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| State not found | Ensure child GameObjects have `MmRelayNode` component |
| Messages reach wrong state | Use `.Selected()` filter for state-specific messages |
| FSM is null | Check that `MmRelaySwitchNode` (not `MmRelayNode`) is used |
| Callbacks not firing | Register callbacks after `Start()`, FSM initializes in `Awake()` |

---

## Hierarchy Requirements

For `MmRelaySwitchNode` to work correctly:

```
✅ CORRECT:
SwitchNode (MmRelaySwitchNode)
  ├── StateA (MmRelayNode + Responders)
  ├── StateB (MmRelayNode + Responders)
  └── StateC (MmRelayNode + Responders)

❌ WRONG:
SwitchNode (MmRelaySwitchNode)
  ├── StateA (MmBaseResponder only)  ← Missing MmRelayNode!
  └── StateB (No MmRelayNode)        ← Won't be recognized as state
```

Each state must be a **direct child** with an `MmRelayNode` component.

---

## Try This

Practice FSM-based state management:

1. **Add a GameOver state** - Create a fourth state for game over. Transition to it when a "PlayerDied" event occurs, and allow pressing Enter to restart from MainMenu.

2. **Animated transitions** - Implement fade-out/fade-in transitions between states using `StartTransitionTo()` and `EnterNext()` with coroutines.

3. **Track time in each state** - Log how long the player spends in Gameplay and Pause states. Display total play time on game over.

4. **Nested states** - Create a sub-FSM within Gameplay that manages Combat, Exploration, and Dialogue states.

---

## Next Steps

- **[Tutorial 9: Task Management](Tutorial-9-Task-Management)** - FSM for experiment workflows
- **[Tutorial 10: Application State](Tutorial-10-Application-State)** - Global state persistence
- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Network state sync

---

*Tutorial 8 of 14 - MercuryMessaging Wiki*
