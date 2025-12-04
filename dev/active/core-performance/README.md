# Core Performance Optimizations

## Overview

This folder contains core framework performance optimizations for MercuryMessaging. These are low-level improvements to the message routing, FSM, and serialization systems.

**Plan File:** `.claude/plans/iterative-leaping-shannon.md`
**Total Effort:** ~26h
**Status:** Planning Complete

---

## Goals

1. **Remove dead code** - Clean up experimental SerialExecutionQueue
2. **Early termination** - Add Handled flag for message propagation control
3. **FSM performance** - Cache current state for faster SelectedCheck
4. **Routing flexibility** - MmRoutingChecks flags to skip unnecessary checks
5. **Serialization overhaul** - Zero-allocation binary serialization

---

## Task Summary

| ID | Task | Effort | Status |
|----|------|--------|--------|
| D1 | Remove SerialExecutionQueue dead code | 15 min | ✅ Complete |
| E1-E3 | Handled flag early termination | 1-2h | ✅ Complete |
| P1-P3 | MmRelaySwitchNode caching | 4h | Pending |
| Q1-Q4 | MmRoutingChecks consolidation | 4h | Pending |
| S1-S7 | Serialization overhaul | 16h | Pending |

---

## Implementation Order

1. **D1** - Quick cleanup, no dependencies
2. **E1-E3** - Independent feature, enables future optimizations
3. **P1-P3** - FSM improvements
4. **Q1-Q4** - Routing improvements (consolidates MmQuickNode)
5. **S1-S7** - Largest change, benefits networking task

---

## Critical Files

### To Modify
- `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs`
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelaySwitchNode.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmBinarySerializer.cs`

### To Delete
- `Assets/MercuryMessaging/Protocol/Nodes/MmQuickNode.cs` (after Q4)
- `Assets/MercuryMessaging/Task/IMmSerializable.cs` (after S7)

### To Create
- `Assets/MercuryMessaging/Protocol/Network/IMmBinarySerializable.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmWriter.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmReader.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmTypeRegistry.cs`

---

## Related Tasks

- `dev/active/networking/` - Benefits from S1-S7 serialization overhaul
- `dev/active/parallel-dispatch/` - Future async work (not this task)
- `dev/archive/framework-analysis/` - Original P1 performance work (completed)

---

## Files in This Folder

- `README.md` - This overview
- `core-performance-tasks.md` - Detailed task implementations
