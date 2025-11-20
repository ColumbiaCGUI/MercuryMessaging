# MercuryMessaging Technical Debt

**Last Updated:** 2025-11-19
**Status:** Active tracking of known technical debt items

---

## Overview

This document tracks known technical debt, TODO items, and future improvements that have been identified but not yet implemented. Items are organized by priority and category.

---

## PRIORITY 1: Critical Issues

None currently identified.

---

## PRIORITY 2: Important Improvements

### 1. Thread Safety for Concurrent Message Processing

**Location:** `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (removed line 816 TODO)

**Issue:** The current implementation uses a simple flag (`doNotModifyRoutingTable`) to prevent routing table modifications during message processing. This approach is not thread-safe for multi-threaded applications.

**Current Approach:**
```csharp
doNotModifyRoutingTable = true;
// ... process message ...
doNotModifyRoutingTable = false;
```

**Recommended Solution:**
- Implement proper locking mechanism using `lock` or `System.Threading.Mutex`
- Consider using `ReaderWriterLockSlim` for better concurrent read performance
- Add thread-safe message queue if supporting async message processing

**Impact:**
- Current: Single-threaded Unity main thread execution (safe)
- Future: Required if supporting multi-threaded message processing or async/await patterns

**Effort:** 4-8 hours

**Dependencies:**
- Research Unity's main thread requirements
- Design thread-safe message queue pattern
- Comprehensive multi-threading tests

**Status:** Deferred - Unity's main thread model makes this low priority

---

## PRIORITY 3: Testing & Validation

### 1. FSM State Transition Testing

**Location:** `Assets/MercuryMessaging/Protocol/MmRelaySwitchNode.cs:120`

**Issue:** TODO comment indicates JumpTo() method needs additional testing.

**Context:**
```csharp
//TODO: Test this again
/// <summary>
/// FSM control method: Jump to State, using MmRoutingTableItem Responder reference.
/// </summary>
public virtual void JumpTo(string newState)
{
    RespondersFSM.JumpTo(RoutingTable[newState]);
}
```

**Recommended Actions:**
1. Create comprehensive unit tests for state transitions
2. Test edge cases:
   - Jumping to non-existent state
   - Jumping to current state
   - Rapid state transitions
   - State transitions during message processing
3. Test with complex FSM hierarchies (nested states)
4. Verify Enter/Exit event ordering

**Effort:** 2-4 hours

**Acceptance Criteria:**
- All edge cases covered by unit tests
- Documentation updated with tested behavior
- TODO comment removed

**Status:** Pending - Should be addressed before production use of FSM features

---

## PRIORITY 4: Code Quality & Maintenance

### 1. Commented Debug Visualization Code

**Location:** `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:472-479`

**Issue:** Small section of commented code remains (part of older debug visualization system).

**Code:**
```csharp
// _messageDisplay.SetMessageData("Incoming Messages", "Outgoing Messages", messageInList, messageOutList);

//         // if (!messageDisplayIndicator)
//         // {
//         //     _messageDisplay.ToggleDisplay(messageDisplayIndicator);
//         //     messageDisplayIndicator = true;
//         // }

//         // messageDisplayIndicator = false;
//     }
// }
```

**Recommended Action:**
- Remove if no longer needed
- Or extract to separate debug utility class if still useful

**Effort:** 0.5 hours

**Status:** Low priority - isolated code with no impact

---

## Completed Cleanups (Session 6 - QW-6)

### ✅ Removed Experimental Serial Execution Code

**Location:** `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (formerly lines 820-835)

**What Was Removed:**
```csharp
//Experimental: Allow forced serial execution (ordered) of messages.
//if (serialExecution)
//{
//    if (!_executing)
//    {
//        _executing = true;
//    }
//    else
//    {
//        MmLogger.LogFramework("<<<<<>>>>>Queueing<<<<<>>>>>");
//        KeyValuePair<MmMessageType, MmMessage> newMessage =
//            new KeyValuePair<MmMessageType, MmMessage>(msgType, message);
//        SerialExecutionQueue.Enqueue(newMessage);
//        return;
//    }
//}
```

**Rationale:**
- Experimental code never implemented
- If serial execution needed, should be redesigned with thread-safe queue (see Priority 2 item above)
- Cluttered production code

---

### ✅ Removed Commented Debug Visualization (DrawSignals)

**Location:** `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (formerly lines 482-518)

**What Was Removed:**
- Entire DrawSignals() method (37 lines of commented code)
- Used EPOOutline library for visual debugging
- Signal path visualization with color coding

**Rationale:**
- External dependency on EPOOutline removed in Session 6
- Debug visualization not part of core framework
- If needed in future, should be separate editor tool (see visual-composer tasks)

---

## Summary

**Total Active Items:** 3
- Priority 1 (Critical): 0
- Priority 2 (Important): 1
- Priority 3 (Testing): 1
- Priority 4 (Quality): 1

**Completed This Session:** 2 items
- Serial execution experimental code
- DrawSignals debug visualization

**Next Recommended Action:**
Address Priority 3 item (FSM testing) before using state machine features in production.

---

**Document Version:** 1.0
**Maintained By:** Framework Team

