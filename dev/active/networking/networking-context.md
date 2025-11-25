# Networking Implementation Context

**Last Updated:** 2025-11-25 (Session 2 - Implementation)

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

### Working Code
- All Phase 0A and 0B code compiles and is committed
- MmBinarySerializer handles all 13 message types
- MmLoopbackBackend ready for testing without network
- Pun2Backend wraps existing PUN2 with new interface

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

### Phase 1: FishNet Implementation (80h)
1. **Task 1.1**: Install FishNet package via Package Manager
2. **Task 1.2**: Create `FishNetBackend` implementing `IMmNetworkBackend`
3. **Task 1.3**: Create `FishNetResolver` implementing `IMmGameObjectResolver`
4. **Task 1.4**: Test with MmLoopbackBackend pattern first

### Quick Start Commands
```bash
# Verify current state
git log --oneline -3
git status

# Check for compilation errors
# Open Unity, check Console for errors

# Run tests (in Unity Test Runner, PlayMode)
```

---

## Blockers & Issues

1. **None for networking** - Phase 0A and 0B complete
2. **Unrelated**: StandardLibrary tests have pre-existing errors (InputMessageTests, UIMessageTests)

---

*Context file for seamless continuation after context reset*
