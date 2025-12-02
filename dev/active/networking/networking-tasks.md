# Networking Implementation Tasks

**Last Updated:** 2025-12-01 (Session 8 - Bidirectional Messaging Verified)
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

## Phase 1: FishNet Implementation (80h) ✅ COMPLETE

### Task 1.1: FishNet Setup (8h) ✅
- [x] Install FishNet package (via manifest.json)
- [x] Add FISHNET_AVAILABLE via versionDefines (not Scripting Define Symbols)
- [x] Add FishNet.Runtime to MercuryMessaging.asmdef
- [x] Add FishNet.Runtime to MercuryMessaging.Tests.asmdef
- [x] Test scene exists: Tests/Network/Scenes/NetworkTest.unity

### Task 1.2: FishNetBackend Core (24h) ✅
- [x] Implement IMmNetworkBackend
- [x] Uses FishNet Broadcast system (better than RPCs - no NetworkObject required)
- [x] MmBroadcast struct with byte[] Data payload
- [x] Connection management via Dictionary<int, NetworkConnection>
- [x] Server/Client detection via InstanceFinder
- [x] Conditional compilation with #if FISHNET_AVAILABLE

### Task 1.3: Broadcast Wrappers (16h) ✅ (Renamed from RPC Wrappers)
- [x] ClientManager.Broadcast for client→server
- [x] ServerManager.Broadcast for server→clients
- [x] ServerManager.Broadcast(conn) for server→specific client
- [x] ServerManager.BroadcastExcept for server→other clients
- [x] MmReliability → Channel mapping

### Task 1.4: Network Object Resolution (16h) ✅
- [x] Create FishNetResolver implementing IMmGameObjectResolver
- [x] Uses NetworkObject.ObjectId for network IDs (rejects ObjectId 0)
- [x] Checks both ServerManager.Objects.Spawned and ClientManager.Objects.Spawned
- [x] Handle spawned objects via IsSpawned check
- [x] Fallback to deterministic path-based IDs for scene objects

### Task 1.5: Testing (8h) ✅
- [x] Created NetworkBackendTestRunner.cs (loopback tests for all 13 types)
- [x] Created FishNetTestManager.cs (runtime test GUI)
- [x] Install FishNet and verify compilation with FISHNET_AVAILABLE
- [x] Loopback tests: **15/15 PASS** - All message types serialize correctly
  - MmVoid, MmInt, MmBool, MmFloat, MmString
  - MmVector3, MmVector4, MmQuaternion
  - MmByteArray, MmTransform, MmTransformList
  - MmGameObject, ConnectionEvents
- [x] **ParrelSync tests: PASS** - Bidirectional messaging verified
  - Server → Client: String, Int messages route correctly
  - Client → Server: Messages route correctly
  - Deterministic IDs match across ParrelSync instances
- [x] **Test scene fixes (Session 6)**:
  - Fixed misleading "Routed to responder" log (now checks actual responder count)
  - Added `EnsureNetworkTestResponder()` to auto-add responders at runtime
  - Added `ValidateTestSetup()` for startup warnings
  - Removed `NetworkObject` from RootNode (was causing client deactivation)
  - Verified end-to-end message delivery with `[NetworkTestResponder]` logs
- [x] **Hierarchical routing fixes (Session 7)**:
  - Fixed `NetworkTestSceneBuilder` to add MmRelayNode to each child responder
  - Every responder GameObject now has: MmRelayNode + NetworkTestResponder
  - Explicit routing table registration with `MmAddToRoutingTable()` + `AddParent()`
  - Fixed `RefreshParents()` NullReferenceException for plain responders
  - Fixed NetId confusion (TestRoot vs old RootNode)
  - Documented pattern in `dev/FREQUENT_ERRORS.md`
- [ ] Performance comparison vs PUN2

### Task 1.6: Documentation (8h) ✅
- [x] Created Tests/Network/README.md with setup instructions
- [x] ParrelSync installation guide
- [x] FishNet integration steps
- [ ] Example multiplayer demo scene

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
| 1 | ✅ Complete | 80h | ~8h |
| 2 | ⬜ Not Started | 100h | 0h |
| 3-5 | ⬜ Not Started | 312h | 0h |

**Total Progress:** 3/6 phases complete (Phase 1 FishNet: COMPLETE ✅)

### Phase 1 Summary
- All core tasks complete: FishNet setup, backend, resolver, testing, docs
- Loopback tests: 15/15 PASS
- ParrelSync tests: PASS (bidirectional messaging verified, end-to-end with responder logs)
- Hierarchical routing: VERIFIED (messages traverse 1-3 relay hops)
- Test scene fully functional with proper validation
- **Status: COMPLETE** ✅
- Remaining optional: performance comparison vs PUN2

### Session 8 Accomplishments (2025-12-01)
- **Bidirectional messaging fully verified** with console log analysis
- Server→Client: `MmString "Hello FishNet!"` routed to 4 responders ✓
- Client→Server: `MmInt 42` routed to 4 responders ✓
- Confirmed FishNet Broadcast system working correctly:
  - `OnClientReceivedBroadcast` (FishNetBackend.cs:335) for server→client
  - `OnServerReceivedBroadcast` (FishNetBackend.cs:320) for client→server
- Hierarchical routing verified: 1-3 relay hops depending on responder depth
- Phase 1 FishNet implementation: **COMPLETE**

### Session 7 Accomplishments (2025-12-01)
- Fixed hierarchical routing: child responders now receive messages
- Critical insight: Every responder GameObject needs its own MmRelayNode
- Fixed `NetworkTestSceneBuilder` to create proper hierarchy with relay nodes
- Fixed `RefreshParents()` NullReferenceException for plain responders
- Documented patterns in `dev/FREQUENT_ERRORS.md`

### Session 6 Accomplishments (2025-12-01)
- Fixed test scene to properly validate message delivery (not just routing)
- Discovered NetworkObject component causes FishNet to deactivate scene objects on clients
- Architectural decision: MercuryMessaging uses Broadcasts, not NetworkObject RPCs

---

## Next Tasks (Recommended)

### Option A: Phase 2 - Fusion 2 Implementation
- Implement Fusion2Backend for Photon Fusion 2
- More complex due to tick-based architecture
- Estimated: 100h

### Option B: FishNet Polish & Production Readiness
1. ~~**Hierarchical routing tests**~~ ✅ Verified in Session 8
2. **Clean up test scene** - Remove duplicate FishNetTestManager
3. **Example demo scene** - Simple multiplayer example for documentation
4. **Performance benchmarking** - Compare FishNet vs PUN2 latency/throughput

### Option C: Documentation & Migration
1. **FishNet integration guide** - Step-by-step for new projects
2. **Migration guide** - Moving from PUN2 to FishNet
3. **Architecture documentation** - Update CLAUDE.md networking section

**Recommended:** Option C (Documentation) before moving to Fusion 2

---

## Archived Tasks

Original tasks preserved in `archive/` subfolder:
- `network-performance-original/` (292h)
- `network-prediction-original/` (400h)

---

*Task checklist for networking implementation - Updated 2025-12-01 Session 8 (Bidirectional Messaging Verified)*
