# Tutorial 7: Fusion 2 Networking

Scene setup instructions for Tutorial 7 Photon Fusion 2 integration.

## Prerequisites

- Photon Fusion 2 package installed (optional - fallback mode available)
- Define `FUSION_WEAVER` symbol if Fusion is installed
- Photon App ID configured in PhotonAppSettings

## Scene Hierarchy

```
Tutorial7_Fusion2
├── Main Camera
├── Directional Light
├── FusionManager (NetworkRunner if Fusion available)
├── GameController (MmRelayNode + T7_Fusion2GameController)
│   └── Players (parent for networked players)
├── PlayerPrefab (MmRelayNode + T7_Fusion2PlayerResponder) [NetworkObject]
└── UI Canvas
    ├── Connection Status
    └── Room Browser
```

## Setup Steps

1. Create new scene: `Tutorial7_Fusion2.unity`
2. If Fusion installed: Add NetworkRunner component
3. If no Fusion: Scene runs in local-only simulation mode
4. Create "GameController" with T7_Fusion2GameController
5. Create NetworkObject player prefab with T7_Fusion2PlayerResponder
6. Configure room settings in inspector

## Fallback Mode

Without Fusion installed, the scene runs in local-only mode:
- Messages route through MmLoopbackBackend
- Demonstrates Fusion integration patterns
- No actual Photon connectivity

## Controls

- **J**: Join or create room
- **L**: Leave room
- **WASD**: Move player
- **F**: Fire projectile (network RPC demo)

## Learning Objectives

- Photon Fusion 2 integration patterns
- State authority and prediction
- Network variables vs Mercury messages
- Room-based multiplayer setup
