# Networking Implementation Tasks

**Last Updated:** 2025-11-25 (Session 2)
**Full Plan:** `C:\Users\yangb\.claude\plans\linear-imagining-whisper.md`

---

## Phase 0A: Package Modularization (24h) ✅ COMPLETE

### Task 0A.1: Remove Dead Dependencies (4h) ✅
- [x] Edit `MercuryMessaging.asmdef`
- [x] Remove: Unity.XR.Interaction.Toolkit
- [x] Remove: Unity.InputSystem
- [x] Remove: Unity.Mathematics
- [x] Remove: NewGraph, NewGraph.Editor
- [x] Remove: ALINE, EPO
- [x] Verify compilation succeeds

### Task 0A.2: Create Core.asmdef (8h) - DEFERRED
- [ ] Not needed immediately - main asmdef is now zero-dep
- [ ] Can split later when package structure matures

### Task 0A.3: Create Networking.asmdef (4h) - DEFERRED
- [ ] Not needed yet - networking code in Protocol/Network/
- [ ] Will create when shipping as separate package

### Task 0A.4: Create Examples.asmdef (4h) ✅
- [x] Create `MercuryMessaging.Examples.asmdef`
- [x] Reference: MercuryMessaging, Unity.XR.Interaction.Toolkit, Unity.InputSystem
- [x] Tutorial files automatically included
- [x] Compilation verified

### Task 0A.5: Create Visualization.asmdef Placeholder (4h) - DEFERRED
- [ ] Will create when visual-composer task is active

---

## Phase 0B: Networking Foundation (80h) ✅ COMPLETE

### Task 0B.1: Design IMmNetworkBackend (8h) ✅
- [x] Define interface contract
- [x] Methods: SendToServer, SendToClient, SendToAllClients, SendToOtherClients
- [x] Properties: IsServer, IsClient, IsConnected, LocalClientId, BackendName
- [x] Events: OnMessageReceived, OnClientConnected, OnClientDisconnected, OnConnectedToServer, OnDisconnectedFromServer
- [x] Added MmReliability enum (Reliable/Unreliable)
- [x] Added MmNetworkTarget enum

### Task 0B.2: Implement MmBinarySerializer (24h) ✅
- [x] Binary format design (15-byte header + payload)
- [x] Serialize all 13 message types
- [x] Deserialize with type routing
- [x] Unit tests for each type (MmBinarySerializerTests.cs)
- [x] Discovered MmTransform uses Translation/Scale not Position/LocalScale

### Task 0B.3: Implement MmGameObjectResolver (16h) ✅
- [x] Interface: IMmGameObjectResolver
- [x] PUN2 implementation: Pun2Resolver (PhotonView.Find)
- [x] Registry implementation: MmRegistryResolver (dictionary-based)
- [x] Methods: TryGetNetworkId, TryGetGameObject, TryGetRelayNode

### Task 0B.4: Create MmNetworkTestHarness (16h) ✅
- [x] MmLoopbackBackend for testing without network
- [x] Echo mode, Server mode, Client mode
- [x] Message queue with simulated latency
- [x] RecordSentMessages for test verification

### Task 0B.5: Wrap PUN2 in PUN2Backend (8h) ✅
- [x] Create Pun2Backend implementing IMmNetworkBackend
- [x] Uses PhotonNetwork.RaiseEvent with custom event code 42
- [x] Implements IOnEventCallback for receiving
- [x] Conditional compilation with #if PHOTON_AVAILABLE

### Task 0B.6: Documentation (8h) - PARTIAL
- [ ] Architecture documentation (see networking-context.md)
- [ ] Backend implementation guide
- [ ] Migration guide from direct PUN2 usage

---

## Phase 1: FishNet Implementation (80h) ⬜ NOT STARTED

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
- [ ] Create FishNetResolver implementing IMmGameObjectResolver
- [ ] Integrate with FishNet's NetworkObject
- [ ] Handle spawned vs scene objects

### Task 1.5: Testing (8h) [ ]
- [ ] Test all 13 message types
- [ ] Test hierarchical routing
- [ ] Performance comparison vs PUN2

### Task 1.6: Documentation (8h) [ ]
- [ ] FishNet integration guide
- [ ] Setup instructions
- [ ] Example scenes

---

## Phase 2: Fusion 2 Implementation (100h) ⬜ NOT STARTED

### Task 2.1: Tick-Event Bridge Design (16h) [ ]
- [ ] Analyze Fusion's tick-based model
- [ ] Design message queuing for tick alignment
- [ ] MmFusionTickManager concept

### Task 2.2: Fusion2Backend Core (24h) [ ]
- [ ] Implement IMmNetworkBackend
- [ ] RPC methods with binary serialization
- [ ] NetworkRunner integration

### Task 2.3: Fix MmMessageGameObject (24h) [ ]
- [ ] Backend-agnostic serialization ✅ (GameObjectNetId field added)
- [ ] NetworkObject.Id mapping
- [ ] Scene vs spawned object handling

### Task 2.4: State Authority Handling (16h) [ ]
- [ ] Input Authority vs State Authority
- [ ] Authority transfer scenarios
- [ ] Host mode handling

### Task 2.5: Testing (12h) [ ]
- [ ] All 13 message types
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

## Progress Summary

| Phase | Status | Hours Est | Hours Used |
|-------|--------|-----------|------------|
| 0A | ✅ Complete | 24h | ~4h |
| 0B | ✅ Complete | 80h | ~8h |
| 1 | ⬜ Not Started | 80h | 0h |
| 2 | ⬜ Not Started | 100h | 0h |
| 3-5 | ⬜ Not Started | 312h | 0h |

**Total Progress:** 2/6 phases complete (~17%)

---

## Archived Tasks

Original tasks preserved in `archive/` subfolder:
- `network-performance-original/` (292h)
- `network-prediction-original/` (400h)

---

*Task checklist for networking implementation - Updated 2025-11-25 Session 2*
