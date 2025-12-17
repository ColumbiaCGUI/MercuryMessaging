# Networking Implementation Context

**Last Updated:** 2025-12-01 (Session 8 - Bidirectional Messaging Verified)

---

## Session Summary (2025-12-01 - Session 8)

### Bidirectional Messaging: VERIFIED ✅

**Final verification of FishNet integration with console log analysis.**

### Server → Client Test
```
[FishNetTest] Bridge received MmString (NetId: 765583435): "Hello FishNet!" → Routed to 4 responder(s) ✓
[NetworkTestResponder] TestResponder1 received MmString: "Hello FishNet!" (total: 1, global: 1)
[NetworkTestResponder] TestResponder2 received MmString: "Hello FishNet!" (total: 1, global: 2)
[NetworkTestResponder] NestedResponder1 received MmString: "Hello FishNet!" (total: 1, global: 3)
[NetworkTestResponder] TestRoot received MmString: "Hello FishNet!" (total: 1, global: 4)
```

**Call Stack:** `FishNetBackend.OnClientReceivedBroadcast()` → `MmNetworkBridge.HandleReceivedMessage()` → `MmRelayNode.MmInvoke()` (1-3 hops) → `NetworkTestResponder.ReceivedMessage()`

### Client → Server Test
```
[FishNetTest] Bridge received MmInt (NetId: 765583435): 42 → Routed to 4 responder(s) ✓
[NetworkTestResponder] TestResponder1 received MmInt: 42 (total: 1, global: 1)
[NetworkTestResponder] TestResponder2 received MmInt: 42 (total: 1, global: 2)
[NetworkTestResponder] NestedResponder1 received MmInt: 42 (total: 1, global: 3)
[NetworkTestResponder] TestRoot received MmInt: 42 (total: 1, global: 4)
```

**Call Stack:** `FishNetBackend.OnServerReceivedBroadcast()` → `MmNetworkBridge.HandleReceivedMessage()` → `MmRelayNode.MmInvoke()` (1-3 hops) → `NetworkTestResponder.ReceivedMessage()`

### Verified Working
| Feature | Status |
|---------|--------|
| Server → Client (MmString) | ✅ 4 responders |
| Client → Server (MmInt) | ✅ 4 responders |
| Same NetId preserved | ✅ 765583435 |
| Hierarchical routing (1-3 hops) | ✅ Verified |
| Serialization/Deserialization | ✅ Working |

### Phase 1 FishNet: COMPLETE ✅

---

## Session Summary (2025-12-01 - Session 7)

### Hierarchical Routing Fixes: COMPLETE ✅

**Problem Solved:** Messages reaching parent relay node but not propagating to child responders in test hierarchy.

**Root Causes Identified:**
1. **Child responders without relay nodes**: `NetworkTestSceneBuilder` created child GameObjects with only `NetworkTestResponder`, no `MmRelayNode`
2. **MmRefreshResponders() limitation**: Only finds responders on the **same GameObject**, not child GameObjects
3. **RefreshParents() NullReferenceException**: Iterating child routing table items and calling `GetRelayNode()` on plain responders returned null
4. **NetId confusion**: Server/client targeting different relay nodes (old RootNode vs new TestRoot)

### Key Architectural Insight

**Every responder GameObject MUST have an MmRelayNode:**
```
CORRECT:
  TestRoot (MmRelayNode + NetworkTestResponder)
    ├─ TestResponder1 (MmRelayNode + NetworkTestResponder)
    └─ ChildNode (MmRelayNode)
        └─ NestedResponder1 (MmRelayNode + NetworkTestResponder)

WRONG:
  TestRoot (MmRelayNode + NetworkTestResponder)
    ├─ TestResponder1 (NetworkTestResponder only)  ← Missing relay node!
    └─ ChildNode (MmRelayNode)
```

### Fixes Applied This Session

#### NetworkTestSceneBuilder.cs (3 changes):
1. **Added MmRelayNode to each child responder**: Every child now has relay node + responder
2. **Explicit routing table registration**: `MmAddToRoutingTable()` + `AddParent()` for each child
3. **CleanupTestObjects disables old RootNode**: Prevents NetId confusion

#### FishNetTestManager.cs (1 change):
4. **ResolveTargetNetId prioritizes TestRoot**: If TestRoot exists, use it instead of Inspector-assigned RootNode

#### MmRelayNode.cs (1 change):
5. **Null-safe RefreshParents()**: Check if child responder has relay node before calling methods on it

### Files Modified
- `NetworkTestSceneBuilder.cs` - Fixed hierarchy creation pattern
- `FishNetTestManager.cs` - Fixed target resolution
- `MmRelayNode.cs` - Fixed NullReferenceException in RefreshParents()
- `dev/FREQUENT_ERRORS.md` - Added network test scene patterns

### Documentation Updated
- `dev/FREQUENT_ERRORS.md` - Added sections 4, 5, 6 for network test patterns

---

## Session Summary (2025-12-01 - Session 6)

### FishNet Test Scene Fixes: COMPLETE ✅

**Problem Solved:** Test logs showed "Routed to responder ✓" but responders weren't actually receiving messages.

**Root Causes Identified:**
1. **Missing responder**: Scene had `MmBaseResponder` (no logging) instead of `NetworkTestResponder` (logs all messages)
2. **Misleading log**: `OnBridgeMessageReceived()` only checked relay node existence, not actual responder delivery
3. **RootNode inactive on client**: FishNet `NetworkObject` component auto-disables scene objects on clients

### Fixes Applied This Session

#### FishNetTestManager.cs (4 changes):
1. **Added `EnsureNetworkTestResponder()`** - Auto-adds `NetworkTestResponder` at runtime, replaces plain `MmBaseResponder`
2. **Added `RefreshAfterFrame()` coroutine** - Waits one frame then refreshes routing table
3. **Fixed misleading log** - Now checks actual responder count, not just relay node existence:
   - Shows `→ Routed to N responder(s) ✓` if responders exist
   - Shows `→ Node found but NO responders attached!` if none
4. **Added `ValidateTestSetup()`** - Warns on startup if no responders found

#### NetworkTest.unity Scene (manual fix):
5. **Removed `NetworkObject` component from RootNode** - This was causing FishNet to auto-disable RootNode on clients

### Key Learnings This Session

1. **NetworkObject component not needed for MercuryMessaging**
   - MercuryMessaging uses `MmNetworkBridge` with FishNet's **Broadcast system**
   - Does NOT use NetworkObject-based RPCs
   - `FishNetBridgeSetup` handles registration using deterministic path-based IDs
   - Scene objects with NetworkObject are disabled by FishNet until spawned

2. **Test validation must verify actual delivery**
   - Checking if relay node exists is not enough
   - Must verify responder count: `node.GetComponentsInChildren<NetworkTestResponder>().Length`

3. **ActiveFilter matters for inactive objects**
   - Default `MmActiveFilter.Active` filters out inactive objects
   - For network messages, we chose NOT to override this (preserves sender intent)
   - Instead, we fixed the root cause (removed NetworkObject)

### Test Results (2025-12-01 - ParrelSync)
```
CLIENT (receiving from server):
  [FishNetTest] [OK] Found 1 NetworkTestResponder(s) ready to receive messages
  [FishNetTest] Bridge received MmString (NetId: 294861751): "Hello FishNet!" → Routed to 1 responder(s) ✓
  [NetworkTestResponder] RootNode received MmString: "Hello FishNet!" (total: 1, global: 1)

SERVER (receiving from client):
  [FishNetTest] Bridge received MmString (NetId: 294861751): "Hello FishNet!" → Routed to 1 responder(s) ✓
  [NetworkTestResponder] RootNode received MmString: "Hello FishNet!" (total: 1, global: 2)
```

### Current State
- **Phase 1 FishNet Implementation: COMPLETE** ✅
- Loopback tests: 15/15 PASS
- ParrelSync tests: PASS (bidirectional, verified end-to-end)
- Test scene properly validates actual message delivery
- Ready for production use with FishNet

---

## Session Summary (2025-11-28 - Session 5)

### ParrelSync Multi-Client Testing: SUCCESS ✅

**All networking features verified working:**
- Server → Client messaging ✅
- Client → Server messaging ✅
- Bidirectional routing ✅
- Multiple message types (MmString, MmInt) ✅
- Deterministic path-based IDs match across instances ✅

### Fixes Applied This Session
1. **FishNetResolver.cs**: Reject ObjectId 0 (FishNet uses 0 for scene objects, but MercuryMessaging treats 0 as "no target")
2. **FishNetBridgeSetup.cs**: Use `FindObjectsInactive.Include` to find NetworkObjects disabled by FishNet
3. **FishNetTestManager.cs**:
   - Subscribe to FishNet connection events for re-resolution
   - Use `FindObjectsInactive.Include` for relay node discovery
   - Add debug logging for troubleshooting

### Key Learnings
1. **FishNet disables NetworkObjects** - Scene objects with NetworkObject component may start inactive until spawned
2. **ObjectId 0 is valid in FishNet** - But MercuryMessaging treats NetId 0 as "no target", so we reject it and use path-based fallback
3. **FindObjectsInactive.Include is essential** - Without it, inactive NetworkObjects aren't found during registration
4. **Deterministic path-based IDs** - Using `GetHierarchyPath().GetHashCode() | 1` ensures same ID on server and client (ParrelSync compatible)
5. **Re-resolve on connection** - targetNetId must be re-resolved when FishNet connection state changes

---

## Session Summary (2025-11-28 - Session 4)

### Fixes Applied
- **MercuryMessaging.asmdef**: Added FishNet.Runtime reference (GUID) and versionDefines for FISHNET_AVAILABLE
- **MercuryMessaging.Tests.asmdef**: Added FishNet.Runtime reference and versionDefines for FISHNET_AVAILABLE
- **MercuryMessaging.Generators.dll.meta**: Configured as RoslynAnalyzer with disabled reference validation
- **FishNetTestManager.cs**: Fixed message constructor argument order and `MmMetadataBlockHelper.Default`
- **NetworkBackendTestRunner.cs**: Fixed message constructor argument order and `MmMetadataBlockHelper.Default`

### Key Learnings
1. **versionDefines don't propagate** - Each asmdef needs its own versionDefines for conditional compilation
2. **Message constructor order**: `(value, MmMethod, MmMetadataBlock)` - value first!
3. **MmMetadataBlockHelper.Default** - Use helper class, not `MmMetadataBlock.Default`
4. **RoslynAnalyzer DLLs**: Need proper meta file with `validateReferences: 0` and excluded platforms

### Test Results (2025-11-28 - Loopback)
```
[NetworkTest] === MercuryMessaging Network Backend Tests ===
  Backend initialized: Loopback
  IsConnected: True | IsServer: True | IsClient: True

  PASS: MmVoid (15 bytes)
  PASS: MmInt (19 bytes)
  PASS: MmBool (16 bytes)
  PASS: MmFloat (19 bytes)
  PASS: MmString (31 bytes)
  PASS: MmVector3 (27 bytes)
  PASS: MmVector4 (31 bytes)
  PASS: MmQuaternion (31 bytes)
  PASS: MmByteArray
  PASS: MmTransform
  PASS: MmTransformList
  PASS: MmGameObject
  PASS: ConnectionEvents

All tests passed! (15/15)
```

---

## Session Summary (2025-11-27 - Session 3)

### Implementation Completed
- **FishNetBackend.cs**: Complete IMmNetworkBackend implementation using FishNet Broadcast system
- **FishNetResolver.cs**: Complete IMmGameObjectResolver implementation using NetworkObject.ObjectId
- Both files use `#if FISHNET_AVAILABLE` conditional compilation (compiles without FishNet)

### Design Decisions
1. **Broadcast over RPCs**: FishNet Broadcast system chosen because:
   - Doesn't require NetworkObject component
   - Works globally (like PUN2's RaiseEvent)
   - Single `MmBroadcast` struct with byte[] payload reuses existing MmBinarySerializer

2. **Connection Management**: Server maintains `Dictionary<int, NetworkConnection>` for targeted sends

3. **Dual Object Lookup**: FishNetResolver checks both ServerManager.Objects.Spawned and ClientManager.Objects.Spawned

### Files Created This Session
| File | Lines | Purpose |
|------|-------|---------|
| `Backends/FishNetBackend.cs` | ~280 | IMmNetworkBackend for FishNet |
| `Backends/FishNetResolver.cs` | ~80 | IMmGameObjectResolver for FishNet |

### Next Steps
1. Install FishNet package via Package Manager
2. Add `FISHNET_AVAILABLE` to Scripting Define Symbols
3. Add `FishNet.Runtime` to MercuryMessaging.asmdef references
4. Create test scene and verify all 13 message types

---

## Session Summary (2025-11-25 - Session 2)

### Implementation Completed
- **Phase 0A Complete**: Removed all dead dependencies, created Examples.asmdef
- **Phase 0B Complete**: Built entire provider-agnostic networking foundation
- Created 8 new files totaling ~2,600 lines of networking infrastructure
- Two commits made to `user_study` branch

### Commits This Session
1. `745179ee` - `refactor(asmdef): Remove dead dependencies for zero-dep Core`
2. `c3f69059` - `feat(network): Add networking foundation infrastructure`

---

## Files Created This Session

### Protocol/Network/ (New Directory)
| File | Lines | Purpose |
|------|-------|---------|
| `IMmNetworkBackend.cs` | 220 | Transport abstraction interface |
| `IMmGameObjectResolver.cs` | 108 | Network ID ↔ GameObject mapping |
| `MmBinarySerializer.cs` | 560 | Binary message format (15-byte header) |
| `MmNetworkBridge.cs` | 436 | Central orchestrator singleton |
| `MmLoopbackBackend.cs` | 308 | Testing backend with echo mode |
| `MmRegistryResolver.cs` | 221 | Simple dictionary-based resolver |
| `Backends/Pun2Backend.cs` | 373 | PUN2 wrapper implementing IMmNetworkBackend |

### Tests/
| File | Lines | Purpose |
|------|-------|---------|
| `MmBinarySerializerTests.cs` | 364 | Unit tests for all message types |

### Files Modified
- `MmLogger.cs` - Added `LogNetwork` and `LogWarning` methods
- `MmMessageGameObject.cs` - Added `GameObjectNetId` field for network serialization
- `MercuryMessaging.asmdef` - Removed all 7 dead dependencies (now zero-dep)
- `MercuryMessaging.Examples.asmdef` - Created for XR tutorial files

---

## Key Architectural Decisions (Implemented)

### 1. Provider-Agnostic Backend Pattern
```
IMmNetworkBackend (interface)
    ├── Pun2Backend (implemented)
    ├── FishNetBackend (TODO - Phase 1)
    └── Fusion2Backend (TODO - Phase 2)
```

### 2. Binary Serialization Format
```
Header (15 bytes fixed):
- Magic: 4 bytes "MMSG"
- Version: 1 byte
- MessageType: 2 bytes (short)
- MmMethod: 2 bytes (short)
- NetId: 4 bytes (uint)
- Metadata: 2 bytes (packed filters + tag)

Payload: Variable length, type-specific
```

### 3. Three-Layer Architecture
- **Transport Layer** (`IMmNetworkBackend`): byte[] only, backend-specific
- **Serialization Layer** (`MmBinarySerializer`): MmMessage ↔ byte[]
- **Resolution Layer** (`IMmGameObjectResolver`): Network IDs ↔ GameObjects

### 4. MmTransform API Discovery
- Uses `Translation` and `Scale`, NOT `Position` and `LocalScale`
- Constructor order: `(Vector3 translation, Vector3 scale, Quaternion rotation)`
- MmMessageTransformList uses `transforms` (lowercase)

---

## Current Implementation State

### Working Code ✅
- All Phase 0A, 0B, and Phase 1 code compiles and is committed
- MmBinarySerializer handles all 13 message types
- MmLoopbackBackend ready for testing without network
- Pun2Backend wraps existing PUN2 with new interface
- **FishNetBackend COMPLETE** - Bidirectional messaging verified
- **FishNetResolver COMPLETE** - Path-based deterministic IDs working
- Hierarchical routing over network verified (1-3 relay hops)

### Known Limitations
- `MmMessageSerializable` binary serialization is placeholder (uses Activator, not full deserialization)
- MmBinarySerializerTests need to be run in PlayMode (not EditMode)
- Some pre-existing StandardLibrary test errors unrelated to networking

### Namespace Pattern
- New networking code uses `MercuryMessaging.Network` namespace
- Must use `using MmLog = MercuryMessaging.MmLogger;` alias in Network namespace files

---

## Dependencies Analysis (Updated)

### MercuryMessaging.asmdef (After Phase 0A)
```json
"references": []  // ZERO DEPENDENCIES!
```

### MercuryMessaging.Examples.asmdef (New)
```json
"references": [
    "MercuryMessaging",
    "Unity.XR.Interaction.Toolkit",
    "Unity.InputSystem"
]
```

---

## Network Message Flow (Updated)

### New Architecture
```
Send Path:
MmRelayNode.MmInvoke()
  → MmNetworkBridge.Send()
  → MmBinarySerializer.Serialize()
  → IMmNetworkBackend.SendToAllClients()

Receive Path:
IMmNetworkBackend.OnMessageReceived
  → MmBinarySerializer.Deserialize()
  → IMmGameObjectResolver.TryGetRelayNode()
  → MmRelayNode.MmInvoke()
```

### Legacy PUN2 Path (Still Supported)
```
MmRelayNode → MmNetworkResponderPhoton → PhotonNetwork.RaiseEvent
```

---

## Next Session Start Point

### Phase 1: FishNet Implementation - COMPLETE ✅

All tasks completed:
- ✅ Install FishNet package via Package Manager
- ✅ Create `FishNetBackend` implementing `IMmNetworkBackend`
- ✅ Create `FishNetResolver` implementing `IMmGameObjectResolver`
- ✅ Loopback tests: 15/15 PASS
- ✅ ParrelSync bidirectional messaging: VERIFIED
- ✅ Hierarchical routing: VERIFIED (Session 8)

### Next Options

**Option A: Phase 2 - Fusion 2 Implementation (100h)**
- Implement Fusion2Backend for Photon Fusion 2
- More complex due to tick-based architecture

**Option B: FishNet Polish (8-16h)**
- Example multiplayer demo scene
- Performance benchmarking vs PUN2

**Option C: Documentation (8h)**
- FishNet integration guide
- Migration guide from PUN2
- Update CLAUDE.md networking section

### Quick Start Commands
```bash
# Verify current state
git log --oneline -3
git status

# Run network test scene
# Open Assets/MercuryMessaging/Tests/Network/Scenes/NetworkTest.unity
```

---

## Blockers & Issues

1. **None for networking** - Phase 1 complete ✅
2. **Unrelated**: StandardLibrary tests have pre-existing errors (InputMessageTests, UIMessageTests)

---

*Context file for seamless continuation after context reset*
