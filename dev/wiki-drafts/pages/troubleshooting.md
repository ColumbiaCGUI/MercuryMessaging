# Troubleshooting Guide

This guide covers common issues and their solutions when working with MercuryMessaging.

---

## Quick Debugging Checklist

**Message not reaching responder?**
- [ ] Routing table registered? (`MmAddToRoutingTable` called for runtime hierarchies?)
- [ ] Responder refreshed? (`MmRefreshResponders` called after adding components at runtime?)
- [ ] Level filter correct? (Check if `Self` bit is included)
- [ ] Tag matching? (Check `TagCheckEnabled` and tag bits match)
- [ ] Active filter? (Use `MmActiveFilter.All` if targeting inactive GameObjects)

---

## Critical Patterns

### 1. Runtime Hierarchy Registration

**Problem:** Messages don't reach child responders created at runtime.

**Why:** `transform.SetParent()` only updates Unity's hierarchy, not MercuryMessaging's routing table.

**Solution:**
```csharp
// Step 1: Unity hierarchy
child.transform.SetParent(parent.transform);

// Step 2: MercuryMessaging routing table (REQUIRED!)
var parentRelay = parent.GetComponent<MmRelayNode>();
var childRelay = child.GetComponent<MmRelayNode>();
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);
```

**When Required:**
- ✅ `new GameObject()` at runtime
- ✅ Any programmatic hierarchy construction
- ✅ Runtime instantiation without prefabs

**When NOT Required:**
- ❌ Scene hierarchies (automatic during `Awake()`)
- ❌ Prefabs instantiated at runtime (automatic)

---

### 2. Runtime Component Registration

**Problem:** Responders added at runtime don't receive messages.

**Why:** `MmRelayNode.Awake()` finds responders via `GetComponents<MmResponder>()`. Adding components later doesn't trigger re-registration.

**Solution:**
```csharp
// Add responder component
var responder = gameObject.AddComponent<MyResponder>();

// REQUIRED: Explicitly refresh routing table
var relay = gameObject.GetComponent<MmRelayNode>();
relay.MmRefreshResponders();

// Recommended: Wait a frame for safety
yield return null;
```

---

### 3. Level Filter Transformation

**Problem:** Messages reach target relay nodes but responders don't receive them.

**Why:** Responders register with `MmLevelFilter.Self` (0x01). The level check uses bitwise AND. If the forwarded message doesn't include the Self bit, the check fails.

**Solution (when forwarding messages):**
```csharp
// Standard forwarding (siblings, cousins)
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
node.MmInvoke(forwardedMessage);

// Recursive forwarding (descendants, ancestors) - use Self to prevent double-delivery
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
node.MmInvoke(forwardedMessage);
```

---

### 4. Every Responder Needs a Relay Node (Networking)

**Problem:** Network messages reach parent but don't propagate to child responders.

**Why:** Child GameObjects with only responders (no `MmRelayNode`) aren't properly routed.

**Correct Structure:**
```
TestRoot (MmRelayNode + Responder)     ← Correct
  ├─ Child1 (MmRelayNode + Responder)  ← Correct - has relay node
  └─ Child2 (Responder only)           ← WRONG - missing relay node!
```

**Solution:** Always add `MmRelayNode` to any GameObject that has a responder:
```csharp
var childObj = new GameObject("Child");
childObj.transform.SetParent(parent.transform);

// Add relay node FIRST, then responder
var childRelay = childObj.AddComponent<MmRelayNode>();
childObj.AddComponent<MyResponder>();

// Register with parent
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);
```

---

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Tag checking enabled but tags not set | Set matching tags or disable `TagCheckEnabled` |
| Inactive GameObjects ignored | Use `MmActiveFilter.All` to include inactive objects |
| FSM `SelectedFilter` not working | Only works with `MmRelaySwitchNode`, not `MmRelayNode` |
| Host receives messages twice | Check `IsDeserialized` flag to distinguish local vs network |
| `transform.SetParent()` only updates Unity | Also call `MmAddToRoutingTable()` + `AddParent()` |
| Components added at runtime not registered | Call `MmRefreshResponders()` after adding |
| Custom method not handled | Remember to call `base.MmInvoke(message)` for standard methods |

---

## Debugging Tips

### Enable Framework Logging
```csharp
MmLogger.logFramework = true;  // Framework routing decisions
MmLogger.logResponder = true;  // Responder activity
MmLogger.logNetwork = true;    // Network messages
```

### Check Routing Table Contents
```csharp
var relay = GetComponent<MmRelayNode>();
Debug.Log($"Routing table count: {relay.RoutingTable.Count}");
foreach (var item in relay.GetMmRoutingTableItems())
{
    Debug.Log($"  - {item.Responder.name} (Level: {item.Level})");
}
```

### Verify Level Filter Matching
```csharp
// Level check uses bitwise AND
bool willMatch = (message.MetadataBlock.LevelFilter & responderLevel) > 0;
Debug.Log($"Filter: {message.MetadataBlock.LevelFilter}, Responder: {responderLevel}, Match: {willMatch}");
```

---

## Test Pattern Template

```csharp
[UnityTest]
public IEnumerator MyTest()
{
    // Arrange - Create hierarchy
    var parent = new GameObject("Parent");
    var parentRelay = parent.AddComponent<MmRelayNode>();
    parent.AddComponent<TestResponder>();

    var child = new GameObject("Child");
    child.transform.SetParent(parent.transform);
    var childRelay = child.AddComponent<MmRelayNode>();
    child.AddComponent<TestResponder>();

    // CRITICAL: Register routing table
    parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
    childRelay.AddParent(parentRelay);

    // CRITICAL: Refresh responders
    parentRelay.MmRefreshResponders();
    childRelay.MmRefreshResponders();

    yield return null; // Let hierarchy settle
    yield return null; // Extra frame for safety

    // Act
    parentRelay.MmInvoke(MmMethod.Initialize);

    yield return new WaitForSeconds(0.1f);

    // Assert
    Assert.AreEqual(1, TestResponder.MessageCount);

    // Cleanup
    Object.DestroyImmediate(parent);
}
```

---

## Getting Help

If you're still stuck:

1. **Check the tutorials** - [Tutorial Index](Tutorials)
2. **Review the architecture** - Understand relay nodes and responders
3. **Enable logging** - Use `MmLogger` to trace message flow
4. **Create minimal repro** - Isolate the problem in a simple test

---

*Last Updated: 2025-12-12*
