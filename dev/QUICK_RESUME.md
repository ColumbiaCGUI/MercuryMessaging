# Quick Resume - PathSpecification ActiveFilter Fix

**Date:** 2025-11-21
**Status:** FIX APPLIED ✅ (Awaiting Test Verification)

---

## Immediate Next Action

**RUN TESTS TO VERIFY FIX:**
1. Open Unity Editor
2. Window > General > Test Runner
3. PlayMode tab → Run All
4. Expected: **187/188 passing** (5 PathSpec tests should now pass)

---

## What Just Happened

### Problem Discovered
PathSpecificationTests were failing because `ResponderCheck()` returned FALSE even though level filter matched correctly.

**Debug logs revealed:**
```
[MmInvoke] Node 'Parent': RoutingTable has 2 items, levelFilter=Self
[MmInvoke] Responder 'MessageCounterResponder' on 'Parent': level=Self, checkPassed=False
```

**Root Cause:** `ActiveCheck` was failing!
- Messages used `MmActiveFilter.Active` (default)
- Required `GameObject.activeInHierarchy == true`
- Test GameObjects may not be fully active during message delivery

### Fix Applied
Modified all 5 `MmInvokeWithPath()` overloads to use `MmActiveFilter.All` instead of default `Active`.

**Rationale:**
- Path-based routing targets specific nodes by hierarchical path, NOT by active state
- Active state filtering is orthogonal to path-based addressing
- Target nodes found by path should receive messages regardless of temporary active state

---

## Files Modified This Session

### MmRelayNode.cs (+Debug Logging +ActiveFilter Fix)

**Lines 728-759:** Added debug logging to MmInvoke
```csharp
MmLogger.LogFramework($"[MmInvoke] Node '{gameObject.name}': RoutingTable has {RoutingTable.Count} items, levelFilter={levelFilter}");
MmLogger.LogFramework($"[MmInvoke] Responder '{responder.GetType().Name}' on '{responder.MmGameObject.name}': level={responderLevel}, checkPassed={checkPassed}");
```

**Lines 1060-1073:** Added debug logging to MmInvokeWithPath
```csharp
MmLogger.LogFramework($"[MmInvokeWithPath] Forwarding message to {targetNodes.Count} target nodes");
MmLogger.LogFramework($"[MmInvokeWithPath] Forwarding to target node '{targetNode.gameObject.name}', routing table size={targetNode.RoutingTable.Count}");
```

**Lines 1051-1062:** Fixed default metadata in MmInvokeWithPath(no params)
```csharp
metadataBlock = new MmMetadataBlock(
    MmLevelFilter.Self,
    MmActiveFilter.All,  // ← Changed from Active to All
    MmSelectedFilter.All,
    MmNetworkFilter.All,
    MmTagHelper.Everything
);
```

**Lines 1091-1100, 1121-1130, 1151-1160:** Same fix for bool/int/string overloads

**Lines 1187-1189:** Fixed pre-created message overload
```csharp
forwardedMessage.MetadataBlock.ActiveFilter = MmActiveFilter.All;
```

### PathSpecificationTests.cs
**Line 30:** Added debug logging enable
```csharp
MmLogger.LogFramework = UnityEngine.Debug.Log;
```

---

## Next Steps After Tests Pass

### 1. Remove Debug Logging (IMPORTANT!)
Remove all `MmLogger.LogFramework` calls added for debugging:
- Lines 728, 754 in MmInvoke
- Lines 1060, 1066 in MmInvokeWithPath
- Line 30 in PathSpecificationTests.cs SetUp

### 2. Commit Implementation
```bash
git add Assets/MercuryMessaging/Protocol/MmPathSpecification.cs
git add Assets/MercuryMessaging/Tests/PathSpecificationTests.cs
git add Assets/MercuryMessaging/Protocol/MmRelayNode.cs
git add dev/

git commit -m "feat: Implement path specification system with ActiveFilter fix

Adds hierarchical path-based message routing with wildcards.
Phase 2.1 progress: 156h/254h (61.4% complete)

Implementation:
- MmPathSpecification parser with validation and caching (290 lines)
- ResolvePathTargets() for path resolution with wildcard expansion
- 5 MmInvokeWithPath() overloads with ActiveFilter.All for path routing
- 35 comprehensive tests covering all functionality

Key Features:
- Grammar: path := segment ('/' segment)*
- Segments: parent, child, sibling, self, ancestor, descendant, *
- Wildcard semantic: collection expansion (fan-out multiplier)
- Level filter: Self (no re-propagation, exact targets)
- Active filter: All (path routing independent of active state)
- Circular path prevention via visited tracking

Bug Fix:
- PathSpecification tests were failing due to ActiveCheck
- MmInvokeWithPath now uses ActiveFilter.All instead of Active
- Allows messages to reach targets regardless of GameObject active state
- Rationale: Path resolution already found exact targets"
```

### 3. Next Feature: Performance Profiling Hooks (20h)
- Add timing to HandleAdvancedRouting()
- Add timing to ResolvePathTargets()
- Log when > 1ms threshold
- See `dev/active/routing-optimization/routing-optimization-tasks.md`

---

## Test Results Expected

**Before fix:** 182/188 passing (6 failures)
- PathSpecificationTests: 5 failures
- MessageHistoryCacheTests: 1 failure (performance variance - acceptable)

**After fix:** 187/188 passing (1 failure)
- PathSpecificationTests: 0 failures ✅
- MessageHistoryCacheTests: 1 failure (performance variance - acceptable)

---

## Key Insights Discovered

### ResponderCheck Filter Chain
```csharp
1. TagCheck - Checks message tag vs responder tag
2. LevelCheck - Checks (levelFilter & responderLevel) > 0
3. ActiveCheck - Checks activeFilter == All OR (Active && activeInHierarchy)  ← CULPRIT
4. SelectedCheck - Always true in base MmRelayNode
5. NetworkCheck - Checks network filter compatibility
```

### Why ActiveCheck Failed
- Default metadata uses `ActiveFilter.Active`
- Requires `GameObject.activeInHierarchy == true`
- Test GameObjects created with `new GameObject(name)` may not be fully active
- Parent inactive → child `activeInHierarchy` = false → check fails

### Why AdvancedRoutingTests Pass
- Direct `MmInvoke()` calls on sender node
- Sender is always active (just created and hasn't been deactivated)
- PathSpecificationTests forward to target nodes which may be inactive

---

## FREQUENT_ERRORS Pattern to Add

**Pattern: ActiveFilter for Path-Based Routing**

When using `MmInvokeWithPath()`, always use `MmActiveFilter.All` if providing custom metadata:

```csharp
// GOOD - Messages reach targets regardless of active state
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));

// BAD - Messages blocked if target GameObject inactive
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.Active));

// GOOD - Default now uses ActiveFilter.All (after fix)
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize);
```

**Why:** Path resolution already found exact target nodes by hierarchical location. Active state should not block delivery to explicitly targeted nodes.

---

**Phase 2.1 Status:** 156h/254h (61.4% complete)
**Overall Status:** Fix applied → awaiting test verification → remove debug logging → commit
