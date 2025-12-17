# Core Performance Optimizations

## Overview

This folder contains core framework performance optimizations for MercuryMessaging. These are low-level improvements to the message routing, FSM, and serialization systems.

**Plan File:** `.claude/plans/declarative-swinging-alpaca.md`
**Total Effort:** ~40-50h (including DX improvements)
**Status:** ✅ ALL TASKS COMPLETE

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
| P1-P3 | MmRelaySwitchNode caching | 4h | ✅ Complete |
| Q1-Q4 | MmRoutingChecks consolidation | 4h | ✅ Complete |
| DX1 | [RequireComponent] on MmBaseResponder | 0.5h | ✅ Complete |
| DX2 | MmSetParent() extension | 2h | ✅ Complete |
| DX3 | MmTestHierarchy test utilities | 4h | ✅ Complete |
| DX4 | Editor validation warnings | 2h | ✅ Complete |
| S1-S7 | Serialization overhaul | 16h | ✅ Complete |

---

## Implementation Order

1. **D1** - Quick cleanup, no dependencies
2. **E1-E3** - Independent feature, enables future optimizations
3. **P1-P3** - FSM improvements
4. **Q1-Q4** - Routing improvements (consolidates MmQuickNode)
5. **S1-S7** - Largest change, benefits networking task

---

## Critical Files

### Modified ✅
- `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs` - Handled flag
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs` - MmRoutingChecks flags
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelaySwitchNode.cs` - FSM caching
- `Assets/MercuryMessaging/Protocol/Network/MmBinarySerializer.cs` - Pooled serialization
- `Assets/MercuryMessaging/Protocol/Responders/MmBaseResponder.cs` - RequireComponent
- `Assets/MercuryMessaging/Protocol/DSL/HierarchyBuilder.cs` - MmSetParent()
- `Assets/MercuryMessaging/Protocol/Nodes/MmQuickNode.cs` - Marked [Obsolete]
- `Assets/MercuryMessaging/Task/IMmSerializable.cs` - Marked [Obsolete]

### Created ✅
- `Assets/MercuryMessaging/Protocol/Network/IMmBinarySerializable.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmWriter.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmReader.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmTypeRegistry.cs`
- `Assets/MercuryMessaging/Tests/Utilities/MmTestHierarchy.cs`
- `Assets/MercuryMessaging/Editor/MmHierarchyValidator.cs`
- `Assets/MercuryMessaging/Editor/MmTestResultExporter.cs` - Auto-exports test results to dev/test-results/

---

## Test Status (2025-12-04)

**All tests passing.** No compilation errors.

### Test Fixes Applied
Tests were updated to account for `[RequireComponent(typeof(MmRelayNode))]` on MmBaseResponder:
- `ResponderBuilderTests.cs` - Updated null relay tests (relay is now auto-added)
- `MmListenerTests.cs` - Updated orphan responder test
- `MmMessagingExtensionsTests.cs` - Updated null relay test
- `PropertyRoutingTests.cs` - Updated null relay test
- `BuilderApiTests.cs` - Updated null relay test
- `LazyCopyValidationTests.cs` - Added static counter reset in SetUp/TearDown
- `MmExtendableResponderPerformanceTests.cs` - Increased memory threshold (2KB)

---

## Related Tasks

- `dev/active/networking/` - Benefits from S1-S7 serialization overhaul
- `dev/active/parallel-dispatch/` - Future async work (not this task)
- `dev/archive/framework-analysis/` - Original P1 performance work (completed)

---

## Files in This Folder

- `README.md` - This overview
- `core-performance-tasks.md` - Detailed task implementations
