# Tutorial 11: Advanced Networking

Scene setup instructions for Tutorial 11.

## Scene Hierarchy

```
Tutorial11_AdvancedNetworking
├── Main Camera
├── Directional Light
├── NetworkDemo (MmRelayNode + T11_LoopbackDemo)
│   └── NetworkResponder (MmRelayNode + T11_NetworkResponder)
└── UI Canvas
    └── Instructions Text
```

## Setup Steps

1. Create new scene: `Tutorial11_AdvancedNetworking.unity`
2. Add Main Camera and Directional Light
3. Create empty GameObject "NetworkDemo"
   - Add `MmRelayNode` component
   - Add `T11_LoopbackDemo` script
4. Create child GameObject "NetworkResponder" under NetworkDemo
   - Add `MmRelayNode` component
   - Add `T11_NetworkResponder` script
5. Create UI Canvas with instructions text

## Controls

- **SPACE**: Send test message through loopback
- **S**: Test serialization round-trip

## Learning Objectives

- Understanding MmLoopbackBackend for local testing
- Message serialization/deserialization
- Checking `IsDeserialized` flag for network origin
- Debug logging with `MmLogger.logNetwork`
