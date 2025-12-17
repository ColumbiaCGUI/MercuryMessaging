# Tutorial 8: Switch Nodes & FSM

Scene setup instructions for Tutorial 8 Finite State Machine with MmRelaySwitchNode.

## Scene Hierarchy

```
Tutorial8_FSM
├── Main Camera
├── Directional Light
├── GameManager (MmRelaySwitchNode + T8_GameStateController)
│   ├── MainMenu (MmRelayNode + T8_MenuResponder)
│   │   └── MenuUI Canvas
│   ├── Gameplay (MmRelayNode + T8_GameplayResponder)
│   │   ├── Player (Cube)
│   │   └── GameplayUI Canvas (Score)
│   ├── PauseMenu (MmRelayNode + T8_PauseResponder)
│   │   └── PauseUI Canvas
│   └── GameOver (MmRelayNode + T8_GameOverResponder)
│       └── GameOverUI Canvas
└── EventSystem
```

## Setup Steps

1. Create new scene: `Tutorial8_FSM.unity`
2. Create "GameManager" empty GameObject
   - Add `MmRelaySwitchNode` component
   - Add `T8_GameStateController` script
3. Create 4 child state GameObjects:
   - MainMenu (with MmRelayNode + T8_MenuResponder)
   - Gameplay (with MmRelayNode + T8_GameplayResponder)
   - PauseMenu (with MmRelayNode + T8_PauseResponder)
   - GameOver (with MmRelayNode + T8_GameOverResponder)
4. Create UI Canvas under each state for state-specific UI
5. Configure MmRelaySwitchNode in Inspector

## Controls

- **ENTER**: Start game (from MainMenu)
- **ESCAPE**: Pause/Resume (during Gameplay)
- **R**: Restart (from GameOver)
- **Q**: Quit to Menu (from Pause/GameOver)
- **WASD**: Move player (during Gameplay)

## State Transitions

```
MainMenu --[Start]--> Gameplay
Gameplay --[Pause]--> PauseMenu
PauseMenu --[Resume]--> Gameplay
PauseMenu --[Quit]--> MainMenu
Gameplay --[GameOver]--> GameOver
GameOver --[Restart]--> Gameplay
GameOver --[Quit]--> MainMenu
```

## Learning Objectives

- MmRelaySwitchNode for state management
- FiniteStateMachine API (JumpTo, StartTransitionTo)
- State enter/exit events
- SelectedFilter for state-specific messages
