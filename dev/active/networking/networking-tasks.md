# Networking Implementation Tasks

**Last Updated:** 2025-11-25
**Full Plan:** `C:\Users\yangb\.claude\plans\linear-imagining-whisper.md`

---

## Phase 0A: Package Modularization (24h)

### Task 0A.1: Remove Dead Dependencies (4h) [ ]
- [ ] Edit `MercuryMessaging.asmdef`
- [ ] Remove: Unity.XR.Interaction.Toolkit
- [ ] Remove: Unity.InputSystem
- [ ] Remove: Unity.Mathematics
- [ ] Remove: NewGraph, NewGraph.Editor
- [ ] Remove: ALINE, EPO
- [ ] Verify compilation succeeds

### Task 0A.2: Create Core.asmdef (8h) [ ]
- [ ] Create `MercuryMessaging.Core.asmdef`
- [ ] Include: Protocol/, Support/FiniteStateMachine/, Support/Data/, Support/Extensions/
- [ ] Verify zero external dependencies
- [ ] Test compilation

### Task 0A.3: Create Networking.asmdef (4h) [ ]
- [ ] Create `MercuryMessaging.Networking.asmdef`
- [ ] Reference: MercuryMessaging.Core
- [ ] Include: MmNetworkResponder.cs, network infrastructure

### Task 0A.4: Create Examples.asmdef (4h) [ ]
- [ ] Create `MercuryMessaging.Examples.asmdef`
- [ ] Reference: MercuryMessaging.Core, Unity.XR.Interaction.Toolkit, Unity.InputSystem
- [ ] Move: HandController.cs, BoxController.cs
- [ ] Update: Tutorial scenes if needed

### Task 0A.5: Create Visualization.asmdef Placeholder (4h) [ ]
- [ ] Create empty `MercuryMessaging.Visualization.asmdef`
- [ ] Document: Future home for ALINE, EPO, NewGraph integration

---

## Phase 0B: Networking Foundation (80h)

### Task 0B.1: Design IMmNetworkBackend (8h) [ ]
- [ ] Define interface contract
- [ ] Methods: SendToServer, SendToClient, SendToAll
- [ ] Properties: IsServer, IsClient, IsConnected
- [ ] Events: OnMessageReceived, OnConnected, OnDisconnected

### Task 0B.2: Implement MmBinarySerializer (24h) [ ]
- [ ] Binary format design (header + payload)
- [ ] Serialize all 15 message types
- [ ] Deserialize with type routing
- [ ] Unit tests for each type

### Task 0B.3: Implement MmGameObjectResolver (16h) [ ]
- [ ] Interface for network ID ↔ GameObject mapping
- [ ] PUN2 implementation (PhotonView.Find)
- [ ] Placeholder for FishNet/Fusion implementations

### Task 0B.4: Create MmNetworkTestHarness (16h) [ ]
- [ ] Loopback testing without actual network
- [ ] Serialize → Deserialize roundtrip tests
- [ ] Mock backend for unit testing

### Task 0B.5: Wrap PUN2 in PUN2Backend (8h) [ ]
- [ ] Create PUN2Backend implementing IMmNetworkBackend
- [ ] Wrap existing MmNetworkResponderPhoton logic
- [ ] Maintain backward compatibility

### Task 0B.6: Documentation (8h) [ ]
- [ ] Architecture documentation
- [ ] Backend implementation guide
- [ ] Migration guide from direct PUN2 usage

---

## Phase 1: FishNet Implementation (80h)

### Task 1.1: FishNet Setup (8h) [ ]
- [ ] Install FishNet package
- [ ] Configure project settings
- [ ] Create test scene

### Task 1.2: FishNetBackend Core (24h) [ ]
- [ ] Implement IMmNetworkBackend
- [ ] Network connection management
- [ ] Server/Client detection

### Task 1.3: RPC Wrappers (16h) [ ]
- [ ] ServerRpc for client→server
- [ ] ObserversRpc for server→clients
- [ ] TargetRpc for server→specific client

### Task 1.4: Network Object Resolution (16h) [ ]
- [ ] Integrate with FishNet's NetworkObject
- [ ] MmGameObjectResolver implementation
- [ ] Handle spawned vs scene objects

### Task 1.5: Testing (8h) [ ]
- [ ] Test all 15 message types
- [ ] Test hierarchical routing
- [ ] Performance comparison vs PUN2

### Task 1.6: Documentation (8h) [ ]
- [ ] FishNet integration guide
- [ ] Setup instructions
- [ ] Example scenes

---

## Phase 2: Fusion 2 Implementation (100h)

### Task 2.1: Tick-Event Bridge Design (16h) [ ]
- [ ] Analyze Fusion's tick-based model
- [ ] Design message queuing for tick alignment
- [ ] MmFusionTickManager concept

### Task 2.2: Fusion2Backend Core (24h) [ ]
- [ ] Implement IMmNetworkBackend
- [ ] RPC methods with binary serialization
- [ ] NetworkRunner integration

### Task 2.3: Fix MmMessageGameObject (24h) [ ]
- [ ] Backend-agnostic serialization
- [ ] NetworkObject.Id mapping
- [ ] Scene vs spawned object handling

### Task 2.4: State Authority Handling (16h) [ ]
- [ ] Input Authority vs State Authority
- [ ] Authority transfer scenarios
- [ ] Host mode handling

### Task 2.5: Testing (12h) [ ]
- [ ] All 15 message types
- [ ] Hierarchical routing
- [ ] Performance benchmarking

### Task 2.6: Documentation (8h) [ ]
- [ ] Fusion 2 integration guide
- [ ] Migration from PUN2
- [ ] Authority handling guide

---

## Phases 3-5: See Full Plan

Phases 3 (Performance), 4 (Memory), and 5 (Prediction) are detailed in the full plan file.

---

## Archived Tasks

Original tasks preserved in `archive/` subfolder:
- `network-performance-original/` (292h)
- `network-prediction-original/` (400h)

---

*Task checklist for networking implementation*
