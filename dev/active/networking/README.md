# Networking Implementation: FishNet + Photon Fusion 2

**Status:** APPROVED - Ready for Implementation
**Created:** 2025-11-25
**Priority:** HIGH (Research + Production)
**Estimated Total:** 596 hours (~15.5 weeks)
**Plan File:** `C:\Users\yangb\.claude\plans\linear-imagining-whisper.md`

---

## Overview

Consolidated networking initiative implementing both FishNet and Photon Fusion 2 backends for MercuryMessaging, with a modular package architecture for easy open-source adoption.

### Key Decisions
- **Implementation Order:** FishNet first → Fusion 2 second
- **PUN 2 Status:** Keep as reference during development, deprecate after Fusion 2 stable
- **Modularization:** Core must be zero-dependency for open-source adoption

---

## Critical Discovery: Dead Dependencies

**All current asmdef dependencies are unused!**

| Dependency | Files Using It | Action |
|------------|----------------|--------|
| `Unity.XR.Interaction.Toolkit` | 2 tutorial files only | Move to Examples |
| `Unity.InputSystem` | 2 tutorial files only | Move to Examples |
| `Unity.Mathematics` | 0 files | DELETE |
| `NewGraph` | 0 files | DELETE |
| `ALINE` | 0 files | DELETE |
| `EPO` | 0 files | DELETE |

**Quick Win:** Simply remove these from `MercuryMessaging.asmdef` to achieve zero-dependency Core immediately!

---

## Package Architecture

```
MercuryMessaging.Core          ← ZERO DEPENDENCIES (Unity built-in only)
MercuryMessaging.Networking    ← Shared networking infrastructure (depends: Core)
MercuryMessaging.FishNet       ← FishNet provider (depends: Networking + FishNet)
MercuryMessaging.Fusion        ← Fusion 2 provider (depends: Networking + Fusion)
MercuryMessaging.Examples      ← Tutorials (depends: Core + XR Toolkit)
MercuryMessaging.Visualization ← Future: visual-composer deps (ALINE, EPO, NewGraph)
```

---

## Phase Summary

| Phase | Hours | Description |
|-------|-------|-------------|
| 0A | 24h | Package modularization (remove dead deps) |
| 0B | 80h | Network foundation (IMmNetworkBackend, serializer) |
| 1 | 80h | FishNet implementation |
| 2 | 100h | Fusion 2 implementation |
| 3 | 112h | Performance optimization (delta sync, batching) |
| 4 | 52h | Memory optimization (pooling, zero GC) |
| 5 | 148h | Prediction & reconciliation |

---

## Consolidated Tasks

This folder consolidates:
- `dev/active/network-performance/` (292h) - Archived to `archive/`
- `dev/active/network-prediction/` (400h) - Archived to `archive/`

**Savings:** ~120h from task deduplication

---

## Next Steps (Start Here!)

1. **Remove dead dependencies** from `MercuryMessaging.asmdef`
2. **Create** `MercuryMessaging.Examples.asmdef` for tutorial files
3. **Archive** existing network tasks to `archive/` subfolder
4. **Begin Phase 0B** - Design `IMmNetworkBackend` interface

---

## Critical Files

### To Modify
- `Assets/MercuryMessaging/MercuryMessaging.asmdef` - Remove 6 dead dependencies
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:587-602` - Network integration point
- `Assets/MercuryMessaging/Protocol/Message/MmMessageGameObject.cs` - Fix serialization

### To Create
- `Protocol/Network/IMmNetworkBackend.cs`
- `Protocol/Network/MmBinarySerializer.cs`
- `Protocol/Network/MmNetworkBridge.cs`
- `Protocol/Network/Backends/FishNetBackend.cs`
- `Protocol/Network/Backends/Fusion2Backend.cs`

---

## References

- **Full Plan:** `C:\Users\yangb\.claude\plans\linear-imagining-whisper.md`
- **Current PUN 2:** `Protocol/MmNetworkResponderPhoton.cs` (reference implementation)
- **Network Interface:** `Protocol/IMmNetworkResponder.cs`

---

*Last Updated: 2025-11-25*
