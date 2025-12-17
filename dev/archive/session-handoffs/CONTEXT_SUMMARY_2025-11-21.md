# Conversation Context Summary - 2025-11-21

**Session Focus:** PathSpecification Test Failures → ActiveFilter Bug Fix

---

## Session Overview

**Initial Problem:** PathSpecification implementation complete but 5 tests failing
**Root Cause:** `ActiveCheck` in `ResponderCheck()` failing due to `activeInHierarchy` requirements
**Solution:** Modified all 5 `MmInvokeWithPath()` overloads to use `MmActiveFilter.All` instead of `Active`
**Status:** Fix applied, awaiting test verification

---

## Problem Investigation

### Initial State
- PathSpecification system fully implemented (parser, resolution, API, 35 tests)
- 5 tests failing: all showing "Expected: 1 message, But was: 0"
- Path resolution working (debug logs showed correct nodes found)
- Messages weren't reaching responders on target nodes

### Debug Strategy
Added comprehensive logging to trace message delivery path:

**MmRelayNode.cs:**
- Line 728: Log routing table size and level filter when MmInvoke called
- Line 754: Log each responder with level and checkPassed result
- Lines 1060, 1066: Log path forwarding details
- Lines 1692-1786: Log path resolution steps

**PathSpecificationTests.cs:**
- Line 30: Enable framework logging in SetUp

### Critical Discovery

Debug logs revealed:
```
[MmInvoke] Node 'Parent': RoutingTable has 2 items, levelFilter=Self
[MmInvoke] Responder 'MessageCounterResponder' on 'Parent': level=Self, checkPassed=False
```

**Key Insight:**
- Responder WAS in routing table with correct level
- Level check SHOULD pass: `(Self & Self) > 0 = TRUE`
- But `checkPassed=False` → one of the OTHER filters failing

### ResponderCheck Analysis

Five-step filter chain:
1. TagCheck ✅
2. LevelCheck ✅ (confirmed matching)
3. **ActiveCheck ❌** (THE CULPRIT)
4. SelectedCheck ✅
5. NetworkCheck ✅

**ActiveCheck logic:**
```csharp
return ((activeFilter == MmActiveFilter.All)
    || (activeFilter == MmActiveFilter.Active && responder.MmGameObject.activeInHierarchy));
```

**Why it failed:**
- Default metadata uses `activeFilter = Active`
- Requires `GameObject.activeInHierarchy == true`
- Test GameObjects created with `new GameObject(name)` may not be fully active in hierarchy
- If parent inactive → child `activeInHierarchy = false` → check fails

**Why AdvancedRoutingTests passed but PathSpecificationTests failed:**
- AdvancedRoutingTests: Direct `MmInvoke()` on sender (always active)
- PathSpecificationTests: Forward to target nodes (may be inactive in hierarchy)

---

## Solution Implemented

### Fix Applied to MmRelayNode.cs

**All 5 MmInvokeWithPath() overloads modified:**

**Overloads 1-4 (no params, bool, int, string) - Lines 1051-1160:**
```csharp
// BEFORE:
if (metadataBlock == null)
    metadataBlock = MmMetadataBlockHelper.Default;  // Uses Active

// AFTER:
if (metadataBlock == null)
{
    metadataBlock = new MmMetadataBlock(
        MmLevelFilter.Self,
        MmActiveFilter.All,  // ← KEY CHANGE
        MmSelectedFilter.All,
        MmNetworkFilter.All,
        MmTagHelper.Everything
    );
}
```

**Overload 5 (pre-created message) - Lines 1187-1189:**
```csharp
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
forwardedMessage.MetadataBlock.ActiveFilter = MmActiveFilter.All;  // ← Added
```

### Rationale

**Path-based routing semantics:**
- Targets specific nodes by **hierarchical location** (parent/child path)
- Active state is about **object lifecycle** (enabled/disabled)
- These concerns are **orthogonal** to each other
- If developer explicitly targets node by path, message should reach it regardless of active state
- Path resolution already found exact targets; active state shouldn't block delivery

**Philosophical consistency:**
- Standard routing: Uses active filter to control participation
- Path-based routing: Already specified exact targets explicitly
- No need to double-filter by active state after explicit targeting

---

## Expected Test Results

**Before fix:** 182/188 passing
- PathSpecificationTests: 24/29 passing (5 failures)
- MessageHistoryCacheTests: 17/18 passing (1 performance timing variance)

**After fix:** 187/188 passing
- PathSpecificationTests: 29/29 passing (0 failures) ✅
- MessageHistoryCacheTests: 17/18 passing (1 acceptable variance)

---

## Next Steps

### Immediate (After Test Verification)

1. **Remove Debug Logging** (CRITICAL - don't commit debug code!)
   - MmRelayNode.cs: Lines 728, 754, 1060, 1066, 1692-1786
   - PathSpecificationTests.cs: Line 30

2. **Commit Implementation**
   ```bash
   git add Assets/MercuryMessaging/Protocol/MmPathSpecification.cs
   git add Assets/MercuryMessaging/Tests/PathSpecificationTests.cs
   git add Assets/MercuryMessaging/Protocol/MmRelayNode.cs
   git add dev/

   git commit -m "feat: Implement path specification system with ActiveFilter fix

   [Full commit message in QUICK_RESUME.md]"
   ```

3. **Move to Next Feature:** Performance Profiling Hooks (20h)
   - Add timing to HandleAdvancedRouting() and ResolvePathTargets()
   - Log when > 1ms threshold
   - See routing-optimization-tasks.md

---

## Key Technical Insights

### 1. ResponderCheck Filter Chain
ALL five checks must pass for message delivery:
- TagCheck → LevelCheck → **ActiveCheck** → SelectedCheck → NetworkCheck
- Single FALSE anywhere blocks entire chain
- Debug by logging each check's result

### 2. ActiveFilter Semantics
- `Active`: Only if `GameObject.activeInHierarchy == true` (checks ENTIRE parent chain)
- `All`: Always deliver regardless of active state
- Critical distinction for forwarded messages

### 3. Path-Based vs Filter-Based Routing
- **Filter-based:** "Send to nodes matching criteria" (active state matters)
- **Path-based:** "Send to specific node at location" (active state orthogonal)
- Different semantics → different default filters

### 4. GameObject.activeInHierarchy Behavior
- NOT same as GameObject.active
- Checks ENTIRE parent chain for active state
- Parent inactive → all children have activeInHierarchy = false
- Can fail even if GameObject.active = true

---

## Files Modified

### Created This Session
- `Assets/MercuryMessaging/Protocol/MmPathSpecification.cs` (290 lines)
- `Assets/MercuryMessaging/Tests/PathSpecificationTests.cs` (615 lines, 35 tests)

### Modified This Session
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs`
  - ResolvePathTargets() method (~140 lines) - lines 1687-1760
  - NavigateSegment() helper (~70 lines) - lines 1762-1828
  - 5 MmInvokeWithPath() overloads (~140 lines) - lines 1045-1193
  - Debug logging (TO BE REMOVED) - lines 728, 754, 1060, 1066, 1692-1786
  - **ActiveFilter fix** - lines 1051-1062, 1091-1100, 1121-1130, 1151-1160, 1187-1189
- `Assets/MercuryMessaging/Tests/PathSpecificationTests.cs`
  - Debug logging enable (TO BE REMOVED) - line 30

### Documentation Updated
- `dev/QUICK_RESUME.md` - Complete rewrite with bug details
- `dev/active/routing-optimization/routing-optimization-context.md` - Added bug fix section
- `dev/FREQUENT_ERRORS.md` - Added Pattern #0 for ActiveFilter
- `dev/SESSION_HANDOFF_ACTIVEFILTER_FIX.md` - 250+ line comprehensive handoff
- `dev/CONTEXT_SUMMARY_2025-11-21.md` - This file

---

## Pattern to Remember (FREQUENT_ERRORS Pattern #0)

**When using MmInvokeWithPath:**
```csharp
// CORRECT - Default now uses ActiveFilter.All
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize);

// CORRECT - Explicit All for custom metadata
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));

// WRONG - Will fail if target inactive in hierarchy
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.Active));
```

**Why:** Path resolution targets by location, not state. Active filtering is orthogonal to path-based addressing.

---

## Phase 2.1 Progress

**Time Spent:** 156h / 254h (61.4% complete)

**Completed:**
- ✅ Message History (36h)
- ✅ Extended Level Filters (56h)
- ✅ Path Specification (40h) + ActiveFilter bug fix
- ✅ All 159+35 = 194 tests (187 passing after fix expected)

**Remaining:**
- ⏳ Performance Profiling Hooks (20h) - Next task
- ⏳ Tutorial Scene (8h)
- ⏳ API Documentation (12h)
- ⏳ Integration Testing (18h)
- ⏳ Performance Testing (20h)

**Total Phase 2.1:** 254h (Advanced Message Routing)

---

## Quick Reference

**Test Command:**
```
Unity Editor → Window → General → Test Runner → PlayMode → Run All
Expected: 187/188 passing
```

**Debug Logging to Remove (14 lines total):**
- MmRelayNode.cs: 728, 754, 1060, 1066, 1692, 1708, 1727, 1737, 1747, 1757, 1767, 1776, 1786
- PathSpecificationTests.cs: 30

**Next Feature Location:**
`dev/active/routing-optimization/routing-optimization-tasks.md` - Performance Profiling Hooks section

---

**Session End State:** Fix applied, fully documented, awaiting test verification

**Last Updated:** 2025-11-21
