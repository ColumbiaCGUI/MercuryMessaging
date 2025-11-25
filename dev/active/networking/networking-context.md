# Networking Implementation Context

**Last Updated:** 2025-11-25

---

## Session Summary (2025-11-25)

### Planning Completed
- Explored current networking architecture (PUN 2 implementation)
- Discovered all asmdef dependencies are dead code
- Designed modular package architecture for zero-dep Core
- Consolidated network-performance (292h) + network-prediction (400h) tasks
- Created comprehensive plan: 596h across 6 phases

### Key Architectural Decisions

1. **Provider-Agnostic Backend**
   - `IMmNetworkBackend` interface abstracts transport layer
   - Each backend (FishNet, Fusion, PUN2) implements same interface
   - `MmNetworkBridge` orchestrates backend selection

2. **Binary Serialization**
   - Previous Fusion attempt failed with JSON serialization
   - New `MmBinarySerializer` will use byte arrays
   - Each backend resolves network IDs differently

3. **MmMessageGameObject Fix**
   - Current code uses `PhotonView.ViewID` directly
   - Solution: Backend-agnostic `MmGameObjectResolver`
   - Each backend maps its own network IDs to GameObjects

---

## Current Implementation State

### Existing Code (Working)
- `MmNetworkResponderPhoton.cs` - PUN 2 implementation (299 lines)
- `MmNetworkResponder.cs` - Abstract base (234 lines)
- `IMmNetworkResponder.cs` - Interface (114 lines)
- `MmNetworkFilter.cs` - Local/Network/All enum

### Previous Fusion Attempt (Abandoned ~125 commits ago)
- Used JSON serialization (failed)
- `MmMessageGameObject` type conversion issues
- RPC serialization problems
- Files were removed from codebase

### Files Using XR (Tutorial Only)
- `Examples/Tutorials/SimpleScene/Scripts/HandController.cs`
- `Examples/Tutorials/SimpleScene/Scripts/BoxController.cs`

---

## Dependencies Analysis

### Current MercuryMessaging.asmdef References
```json
"references": [
    "Unity.XR.Interaction.Toolkit",  // Only 2 tutorial files
    "Unity.InputSystem",              // Only 2 tutorial files
    "Unity.Mathematics",              // NEVER USED
    "NewGraph",                       // NEVER USED
    "NewGraph.Editor",                // NEVER USED
    "ALINE",                          // NEVER USED
    "EPO"                             // NEVER USED
]
```

**Action:** Remove all, move tutorials to Examples package

---

## Network Message Flow

```
Local:  MmRelayNode.MmInvoke() → MmNetworkResponder.MmInvoke() → Serialize → Network
Remote: Network → Deserialize → MmRelayNode.MmInvoke() (IsDeserialized=true)
```

Key integration point: `MmRelayNode.cs:587-602`

---

## Blockers & Issues

1. **None currently** - Planning phase complete, ready for implementation

---

## Next Session Start Point

1. Run Phase 0A tasks (remove dead dependencies)
2. Create `MercuryMessaging.Examples.asmdef`
3. Begin `IMmNetworkBackend` interface design

---

*Context file for seamless continuation after context reset*
