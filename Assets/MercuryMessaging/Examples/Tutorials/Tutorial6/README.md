# Tutorial 6: FishNet Networking

Scene setup instructions for Tutorial 6 FishNet integration.

## Prerequisites

- FishNet package installed (optional - fallback mode available)
- Define `FISH_NET` symbol if FishNet is installed

## Scene Hierarchy

```
Tutorial6_FishNet
├── Main Camera
├── Directional Light
├── NetworkManager (FishNet NetworkManager if available)
├── GameController (MmRelayNode + T6_NetworkGameController)
│   └── Players (parent for spawned players)
├── PlayerPrefab (MmRelayNode + T6_PlayerResponder) [in Resources/]
└── UI Canvas
    ├── Connection Status
    └── Player List
```

## Setup Steps

1. Create new scene: `Tutorial6_FishNet.unity`
2. If FishNet installed: Add FishNet NetworkManager
3. If no FishNet: Scene runs in local-only simulation mode
4. Create "GameController" with MmRelayNode and T6_NetworkGameController
5. Create player prefab with T6_PlayerResponder
6. Save prefab to Resources folder for network spawning

## Fallback Mode

Without FishNet installed, the scene runs in local-only mode:
- All "network" messages route locally
- Position sync demonstrates MercuryMessaging patterns
- No actual network transport

## Controls

- **H**: Host game
- **C**: Connect as client
- **D**: Disconnect
- **WASD**: Move player

## Learning Objectives

- FishNet integration with MercuryMessaging
- MmNetworkBridge for network message routing
- Network spawning with MmRegistryResolver
- Fallback patterns for development
