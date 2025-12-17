# Tutorial 10: Application State

Scene setup instructions for Tutorial 10 MmAppState for global state management.

## Scene Hierarchy

```
Tutorial10_AppState
├── Main Camera
├── Directional Light
├── AppController (MmRelayNode + T10_MyAppController)
│   ├── LoadingState (MmRelayNode + MmAppStateResponder + T10_LoadingBehavior)
│   │   └── LoadingUI (Spinner, Progress Bar)
│   ├── MenuState (MmRelayNode + MmAppStateResponder + T10_MenuBehavior)
│   │   └── MenuUI (Buttons, Title)
│   ├── GameplayState (MmRelayNode + MmAppStateResponder + T10_GameplayBehavior)
│   │   ├── Player
│   │   └── GameplayUI
│   └── PauseState (MmRelayNode + MmAppStateResponder + T10_PauseBehavior)
│       └── PauseUI
└── EventSystem
```

## Setup Steps

1. Create new scene: `Tutorial10_AppState.unity`
2. Create "AppController" with MmRelayNode and T10_MyAppController
3. Create 4 child states:
   - LoadingState: MmAppStateResponder + T10_LoadingBehavior
   - MenuState: MmAppStateResponder + T10_MenuBehavior
   - GameplayState: MmAppStateResponder + T10_GameplayBehavior
   - PauseState: MmAppStateResponder + T10_PauseBehavior
4. Each state needs MmAppStateResponder configured:
   - Set `State Name` to match GameObject name
   - Set `Initial Active State` appropriately
5. Create UI elements under each state

## App State vs FSM

| Feature | MmRelaySwitchNode (FSM) | MmAppState |
|---------|------------------------|------------|
| State history | No | Yes (stack) |
| Push/Pop | No | Yes |
| Parallel states | No | Possible |
| Use case | Game states | App navigation |

## Controls

- **L**: Simulate loading complete
- **ENTER**: Start game (from Menu)
- **ESCAPE**: Toggle pause (from Gameplay)
- **BACKSPACE**: Go back (pop state)
- **Q**: Quit to menu

## State Stack Example

```
Initial: [Loading]
After load: [Menu]
Start game: [Menu, Gameplay]
Pause: [Menu, Gameplay, Pause]
Resume: [Menu, Gameplay]
Quit: [Menu]
```

## Learning Objectives

- MmAppStateResponder for state-aware components
- Push/Pop state navigation
- State stack management
- Difference between FSM and AppState patterns
- Global application state management
