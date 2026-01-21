# Frequent Errors & Bug Reference

**Purpose:** Quick reference for avoiding repeated mistakes in MercuryMessaging development.

**Last Updated:** 2025-11-21 (Added ActiveFilter pattern for PathSpecification)

---

## Critical Patterns (NEVER FORGET These!)

### 0. ⚠️ CRITICAL: ActiveFilter for Path-Based Routing

**Bug:** PathSpecificationTests failing - messages reach correct nodes but responders don't receive them

**Why This Happens:**
- `ResponderCheck()` has 5 checks: TagCheck, LevelCheck, **ActiveCheck**, SelectedCheck, NetworkCheck
- Default metadata uses `MmActiveFilter.Active` which requires `GameObject.activeInHierarchy == true`
- Test GameObjects created with `new GameObject()` may not be fully active when forwarded messages arrive
- Even though LevelCheck passes, ActiveCheck fails → `ResponderCheck()` returns FALSE

**Symptoms:**
- Debug log shows: `checkPassed=False` even though `level=Self` matches correctly
- Path resolution works (finds correct nodes) but messages don't reach responders
- AdvancedRoutingTests pass but PathSpecificationTests fail

**Pattern (For Path-Based Routing):**
```csharp
// CORRECT - Use ActiveFilter.All for path-based routing
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize);  // Default now uses All

// CORRECT - Explicitly use All if providing custom metadata
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.All));

// WRONG - Will fail if target GameObject is inactive
relay.MmInvokeWithPath("parent/sibling", MmMethod.Initialize,
    new MmMetadataBlock(MmLevelFilter.Self, MmActiveFilter.Active));
```

**Rationale:**
- Path-based routing targets **specific nodes by hierarchical path**, NOT by active state
- Active state filtering is orthogonal to path-based addressing
- If you explicitly target a node by path, it should receive the message regardless of active state
- Path resolution already found exact targets; active state shouldn't block delivery

**Where Fixed:**
- `MmRelayNode.cs` lines 1051-1062 (and 4 other `MmInvokeWithPath` overloads)
- All `MmInvokeWithPath()` methods now default to `MmActiveFilter.All`

---

### 1. ⚠️ CRITICAL: Level Filter Transformation When Forwarding Messages

**Bug:** Messages not reaching target responders (Bug #3 - commit 7dd86891)

**Why This Happens:**
- Target responders register with `MmLevelFilter.Self` (0x01) in routing tables
- Level check uses bitwise AND: `(messageFilter & responderLevel) > 0`
- Without transformation: `(Siblings & Self) = (0x08 & 0x01) = 0` → REJECTED
- With transformation: `(SelfAndChildren & Self) = (0x03 & 0x01) = 1` → ACCEPTED

**Pattern (ALWAYS follow this):**
```csharp
// When forwarding messages to other relay nodes:
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
node.MmInvoke(forwardedMessage);
```

**Exceptions:**
- Recursive routing (Descendants/Ancestors): Use `MmLevelFilter.Self` to prevent double-delivery
- Custom filters with explicit targets: Use `MmLevelFilter.Self`

**Reference:** Standard routing pattern at `MmRelayNode.cs:705-722`

**Affected Locations:**
- `RouteLateral()` - Line ~1499
- `RouteRecursive()` - Line ~1527
- `HandleAdvancedRouting()` custom filter - Line ~1313

---

### 2. ⚠️ Routing Table Registration for Runtime Hierarchies

**Bug:** Test hierarchies not finding siblings/cousins/descendants (Bug #2 - commit 5cacfa45)

**Why This Happens:**
- `transform.SetParent()` ONLY updates Unity's Transform hierarchy
- MercuryMessaging maintains a **separate** routing table
- Automatic registration happens in `Awake()` for scene hierarchies
- Runtime hierarchies need **manual registration**

**Pattern (ALWAYS use both):**
```csharp
// Step 1: Unity hierarchy
child.transform.SetParent(parent.transform);

// Step 2: MercuryMessaging routing table (CRITICAL!)
var parentRelay = parent.GetComponent<MmRelayNode>();
var childRelay = child.GetComponent<MmRelayNode>();
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);
```

**When Needed:**
- ✅ Tests creating GameObjects with `new GameObject()` at runtime
- ✅ Any programmatic hierarchy construction
- ✅ Runtime instantiation without prefab

**When NOT Needed:**
- ❌ Scene hierarchies (automatic during `Awake()`)
- ❌ Prefabs instantiated at runtime (automatic)
- ❌ GameObjects already in scene at edit time

---

### 3. ⚠️ Runtime Component Registration

**Bug:** Responders not receiving messages after runtime component addition (Bug #4 - current session)

**Why This Happens:**
- `MmRelayNode.Awake()` calls `MmRefreshResponders()` to find responders via `GetComponents<MmResponder>()`
- Adding components at runtime doesn't trigger re-registration
- Unity doesn't guarantee `Awake()` execution order between components added in same frame

**Pattern (ALWAYS refresh after adding):**
```csharp
// Add responder component
var responder = gameObject.AddComponent<MessageCounterResponder>();

// CRITICAL: Explicitly refresh routing table
var relay = gameObject.GetComponent<MmRelayNode>();
relay.MmRefreshResponders();

// Optional but recommended: Extra frame for safety
yield return null;
```

**Test Pattern:**
```csharp
[UnityTest]
public IEnumerator MyTest()
{
    // Create hierarchy
    var parent = CreateNodeWithResponder("Parent");
    var child = CreateNodeWithResponder("Child", parent.transform);

    // CRITICAL: Refresh all nodes
    parent.GetComponent<MmRelayNode>().MmRefreshResponders();
    child.GetComponent<MmRelayNode>().MmRefreshResponders();

    yield return null; // Let Unity process
    yield return null; // Extra frame for safety

    // Now safe to send messages
    child.GetComponent<MmRelayNode>().MmInvoke(MmMethod.Initialize);
}
```

---

## All Bugs Fixed (Reference)

### Bug #1: Compilation Errors - Method Signatures

**Commit:** db8dc342
**Session:** 2025-11-21 Phase 2.1
**File:** `Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs`

**Symptoms:**
- 3 compilation errors in `MessageCounterResponder` class
- CS0507: Cannot change access modifiers
- CS0115: No suitable method found to override

**Root Causes:**
1. `protected override void Awake()` - Wrong access modifier (should be `public`)
2. `protected override void OnDestroy()` - No virtual method in base class
3. `protected override void ReceivedInitialize()` - Method doesn't exist (should be `Initialize()`)

**Fix:**
```csharp
// BEFORE (broken):
protected override void Awake() { base.Awake(); }
protected override void OnDestroy() { }
protected override void ReceivedInitialize() { MessageCount++; }

// AFTER (fixed):
public override void Awake() { base.Awake(); }
private void OnDestroy() { }  // No override
public override void Initialize() { MessageCount++; }
```

**Lesson:** Always check base class for virtual method signatures before overriding.

---

### Bug #2: Test Hierarchies Not Registered

**Commit:** 5cacfa45
**Session:** 2025-11-21 Phase 2.1
**File:** `Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs`

**Symptoms:**
- All 7 AdvancedRoutingTests failing with 0 messages received
- `CollectSiblings()`, `CollectCousins()` finding no relatives
- Empty routing tables despite correct Unity hierarchy

**Root Cause:**
- Tests used `transform.SetParent()` only
- Unity Transform hierarchy ≠ MercuryMessaging routing table
- Children not registered in parent's routing table
- Collection methods search routing table (found nothing)

**Fix:**
Modified `CreateNodeWithResponder()` helper to explicitly register:
```csharp
if (parent != null)
{
    obj.transform.SetParent(parent);

    // CRITICAL: Explicit registration
    var parentRelay = parent.GetComponent<MmRelayNode>();
    if (parentRelay != null)
    {
        parentRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
        relay.AddParent(parentRelay);
    }
}
```

**Lesson:** Programmatic hierarchies require explicit `MmAddToRoutingTable()` + `AddParent()` calls.

---

### Bug #3: Level Filter Transformation Missing (CRITICAL)

**Commit:** 7dd86891
**Session:** 2025-11-21 Phase 2.1
**File:** `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`

**Symptoms:**
- All 7 AdvancedRoutingTests failing (even after Bug #2 fixed)
- Messages reaching target nodes but responders not invoked
- MessageCounterResponder showing 0 message count
- No console errors or warnings

**Root Cause (CRITICAL DISCOVERY):**
- Advanced routing forwarded messages WITHOUT transforming level filter
- Target responders registered as `MmLevelFilter.Self` (0x01)
- Messages forwarded with original filter (e.g., `Siblings` = 0x08)
- `LevelCheck()` performs bitwise AND: `(0x08 & 0x01) = 0` → FALSE (rejected)
- Standard routing (lines 705-722) already transforms filters correctly
- Advanced routing didn't follow this pattern

**Fix Applied in 3 Locations:**

**Location 1 - RouteLateral() (line ~1499):**
```csharp
foreach (var node in lateralNodes)
{
    if (node != null)
    {
        // Transform level filter so target can process its Self responders
        var forwardedMessage = message.Copy();
        forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
        node.MmInvoke(forwardedMessage);
    }
}
```

**Location 2 - RouteRecursive() (line ~1527):**
```csharp
foreach (var node in nodes)
{
    if (node != null)
    {
        // Transform level filter so target can process its Self responders
        var forwardedMessage = message.Copy();
        forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
        node.MmInvoke(forwardedMessage);
    }
}
```

**Location 3 - HandleAdvancedRouting() custom filter (line ~1313):**
```csharp
if (hasCustom && options != null && options.CustomFilter != null)
{
    var filteredNodes = ApplyCustomFilter(options.CustomFilter);
    foreach (var node in filteredNodes)
    {
        if (node != null)
        {
            // Transform level filter so target can process its Self responders
            var forwardedMessage = message.Copy();
            forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
            node.MmInvoke(forwardedMessage);
        }
    }
}
```

**Why This Works:**
- `SelfAndChildren = 0x03` (includes Self bit 0x01)
- `LevelCheck`: `(0x03 & 0x01) = 0x01` → TRUE (accepted)
- Responders execute, MessageCount increments

**Lesson:** ALWAYS transform level filter when forwarding between relay nodes. Reference standard routing pattern.

---

### Bug #4: Responder Registration Timing

**Session:** 2025-11-21 (current)
**File:** `Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs`

**Symptoms:**
- Tests showing 0 message counts after adding responders at runtime
- Hierarchy correctly registered but messages not reaching responders

**Root Cause:**
- Adding responder component after `MmRelayNode` setup doesn't auto-register
- Unity doesn't guarantee `Awake()` execution order for runtime components
- `MmRefreshResponders()` not called after component addition

**Fix:**
```csharp
// After creating all nodes:
parent.GetComponent<MmRelayNode>().MmRefreshResponders();
child1.GetComponent<MmRelayNode>().MmRefreshResponders();
child2.GetComponent<MmRelayNode>().MmRefreshResponders();

yield return null;
yield return null; // Extra frame for responder registration
```

**Lesson:** Runtime component additions require manual `MmRefreshResponders()` call.

---

### Bug #5: Double-Delivery in Recursive Routing

**Session:** 2025-11-21 (current)
**File:** `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`

**Symptoms:**
- `AncestorsRouting_ReachesAllParents`: Expected 1, got 3 messages to Parent
- `DescendantsRouting_ReachesAllChildren`: Expected 1, got 2 messages to Grandchild

**Root Cause:**
- `RouteRecursive()` transformed filter to `SelfAndChildren`
- `CollectDescendants()` already found ALL nodes recursively: [Child, Grandchild, GreatGrandchild]
- When Child received message with `SelfAndChildren` filter:
  - Child's responder received message ✓
  - Child ALSO routed to its children (because of Children bit)
  - Grandchild received twice: once from Root directly, once from Child

**Fix:**
Changed line 1528 in `RouteRecursive()` and line 1313 in `HandleAdvancedRouting()`:
```csharp
// BEFORE (caused double-delivery):
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;

// AFTER (prevents re-propagation):
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
```

**Rationale:**
- `CollectDescendants`/`CollectAncestors` already finds ALL targets recursively
- We only need to deliver to each target's responders, not propagate further
- Using `Self` prevents re-routing through intermediate nodes

**Lesson:** Recursive routing should use `Self` to prevent double-delivery. Siblings/cousins use `SelfAndChildren`.

---

## Bug Categories

### Category A: Unity-Specific Issues

#### A1: Component Initialization Timing
- **Issue:** Unity doesn't call `Awake()` on components added at runtime until next frame
- **Impact:** MercuryMessaging registration happens in `Awake()`, so responders not registered immediately
- **Anti-Pattern:** `AddComponent<>()` → immediately send message (responder not registered yet)
- **Pattern:** `AddComponent<>()` → call `MmRefreshResponders()` → send message

#### A2: Transform Hierarchy vs Routing Table
- **Issue:** `transform.SetParent()` updates Unity Transform hierarchy only
- **Impact:** MercuryMessaging maintains separate routing table, not automatically synced
- **Anti-Pattern:** `SetParent()` alone (routing table empty)
- **Pattern:** `SetParent()` + `MmAddToRoutingTable()` + `AddParent()`

#### A3: Virtual Method Signatures
- **Issue:** Unity lifecycle methods vs MercuryMessaging virtual methods
- **Unity:** `Awake()`, `OnDestroy()`, `Update()` (no virtual/override)
- **MercuryMessaging:** `public override void Awake()`, `Initialize()`, `MmInvoke()`
- **Anti-Pattern:** Mix Unity and MM method signatures
- **Pattern:** Check base class for virtual methods before overriding

---

### Category B: MercuryMessaging Framework Patterns

#### B1: Level Filter Transformation (CRITICAL)
- **Why:** Responders register with Self (0x01), messages need matching bit
- **Anti-Pattern:** Forward message without transformation (bitwise AND fails)
- **Pattern:** Copy message + set filter to `SelfAndChildren` (includes Self bit)
- **Exception:** Recursive routing uses `Self` to prevent double-delivery
- **Reference:** Lines 705-722 in MmRelayNode.cs (standard routing)

#### B2: Routing Table Registration
- **Automatic:** Scene hierarchies, prefabs at Awake time
- **Manual Required:** Runtime GameObject creation, programmatic hierarchies
- **Pattern:**
  ```csharp
  parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
  childRelay.AddParent(parentRelay);
  ```

#### B3: Recursive Routing Direction
- **Issue:** `SelfAndChildren` during recursion causes double-delivery
- **Pattern:** Use `Self` for immediate local delivery (no re-propagation)
- **Example:** Descendants routing delivers to Self first, collection finds all targets recursively

---

### Category C: Test Writing Issues

#### C1: Programmatic GameObject Creation
- **Challenge:** Tests need explicit routing table setup
- **Pattern:** Helper method that handles both Unity and MM hierarchy
  ```csharp
  GameObject CreateNodeWithResponder(string name, Transform parent = null)
  {
      var obj = new GameObject(name);
      var relay = obj.AddComponent<MmRelayNode>();
      obj.AddComponent<MessageCounterResponder>();

      if (parent != null)
      {
          obj.transform.SetParent(parent);
          var parentRelay = parent.GetComponent<MmRelayNode>();
          if (parentRelay != null)
          {
              parentRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
              relay.AddParent(parentRelay);
          }
      }
      return obj;
  }
  ```

#### C2: Message Count Tracking
- **Pattern:** Use helper responder class with counter
- **Pitfall:** Forget to reset counter between tests
- **Solution:** Static counter + `[SetUp]` method reset
  ```csharp
  [SetUp]
  public void SetUp()
  {
      MessageCounterResponder.ResetAllCounters();
  }
  ```

#### C3: Responder Registration in Tests
- **Pitfall:** Tests fail with 0 message counts
- **Pattern:** Explicitly call `MmRefreshResponders()` after creating all nodes
  ```csharp
  var child = CreateNodeWithResponder("Child", parent.transform);
  child.GetComponent<MmRelayNode>().MmRefreshResponders();
  yield return null;
  yield return null; // Extra frame for safety
  ```

---

### Category D: Common Coding Mistakes

#### D1: Base Class Method Calls
- **Anti-Pattern:** Override `MmInvoke()` without calling `base.MmInvoke()`
- **Result:** Standard methods silently fail (no handler invoked)
- **Pattern:** Always call `base.MmInvoke()` for unhandled methods
  ```csharp
  public override void MmInvoke(MmMessage message)
  {
      if (message.method == (MmMethod)1000)
      {
          // Handle custom method
      }
      else
      {
          base.MmInvoke(message); // NEVER FORGET THIS
      }
  }
  ```

#### D2: Method Name Confusion
- **Common Errors:**
  - `ReceivedInitialize()` doesn't exist (it's `Initialize()`)
  - `ReceivedMessage(MmMessageInt)` vs `ReceivedMessage(MmMessageString)` (overloaded)
- **Pattern:** Check `MmBaseResponder.cs` for exact method signatures

#### D3: Bitwise Filter Logic
- **Common Error:** Assume filter equality instead of bitwise AND
- **Wrong:** `if (messageFilter == responderLevel)`
- **Correct:** `if ((messageFilter & responderLevel) > 0)` (any matching bit)
- **Example:** `(SelfAndChildren & Self) = (0x03 & 0x01) = 0x01` ✅

---

## Quick Code Patterns (Copy-Paste Ready)

### Pattern 1: Forward Message Between Relay Nodes
```csharp
// Standard forwarding (siblings, cousins, etc.)
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
node.MmInvoke(forwardedMessage);

// Recursive forwarding (descendants, ancestors)
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
node.MmInvoke(forwardedMessage);
```

### Pattern 2: Create Runtime Hierarchy (Both Unity + MM)
```csharp
// Step 1: Unity hierarchy
child.transform.SetParent(parent.transform);

// Step 2: MercuryMessaging routing table
var parentRelay = parent.GetComponent<MmRelayNode>();
var childRelay = child.GetComponent<MmRelayNode>();
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);
```

### Pattern 3: Add Component at Runtime (With Refresh)
```csharp
// Add responder component
var responder = gameObject.AddComponent<MyResponder>();

// CRITICAL: Explicitly refresh routing table
var relay = gameObject.GetComponent<MmRelayNode>();
relay.MmRefreshResponders();

// Optional: Extra frame for safety
yield return null;
```

### Pattern 4: Override MmInvoke Safely (With Base Call)
```csharp
public override void MmInvoke(MmMessage message)
{
    // Handle custom methods (>= 1000)
    if (message.method == (MmMethod)1000)
    {
        var customMsg = (CustomMessage)message;
        // Handle custom method
        return;
    }

    // CRITICAL: Call base for standard methods
    base.MmInvoke(message);
}
```

### Pattern 5: Test Setup (Complete)
```csharp
[UnityTest]
public IEnumerator MyAdvancedRoutingTest()
{
    // Arrange - Create hierarchy
    var parent = CreateNodeWithResponder("Parent");
    var child1 = CreateNodeWithResponder("Child1", parent.transform);
    var child2 = CreateNodeWithResponder("Child2", parent.transform);

    // CRITICAL: Refresh responders
    parent.GetComponent<MmRelayNode>().MmRefreshResponders();
    child1.GetComponent<MmRelayNode>().MmRefreshResponders();
    child2.GetComponent<MmRelayNode>().MmRefreshResponders();

    yield return null; // Let hierarchy settle
    yield return null; // Extra frame for responder registration

    // Act - Send message
    var child1Relay = child1.GetComponent<MmRelayNode>();
    var options = MmRoutingOptions.WithLateralRouting();
    var metadata = new MmMetadataBlock(MmLevelFilter.Siblings);
    metadata.Options = options;
    child1Relay.MmInvoke(MmMethod.Initialize, metadata);

    yield return new WaitForSeconds(0.1f);

    // Assert - Verify message counts
    Assert.AreEqual(0, GetMessageCount(child1), "Child1 (sender) should not receive");
    Assert.AreEqual(1, GetMessageCount(child2), "Child2 (sibling) should receive");
}
```

---

## Debugging Checklists

### Checklist 1: Message Not Reaching Responder

When a message isn't reaching its target responder, check these in order:

- [ ] **Routing table registered?**
  - Verify: `Debug.Log($"Routing table count: {relay.RoutingTable.Count}");`
  - If 0: Call `MmAddToRoutingTable()` for children

- [ ] **Level filter includes matching bit?**
  - Check: `(message.MetadataBlock.LevelFilter & responderLevel) > 0`
  - Common issue: Filter transformed to include Self bit?

- [ ] **Tag matching enabled/correct?**
  - Check: `(message.MetadataBlock.Tag & responder.Tag) != 0`
  - If `TagCheckEnabled = true`, ensure tags match

- [ ] **Responder refresh called (runtime)?**
  - If components added at runtime: Call `MmRefreshResponders()`

- [ ] **MmLogger enabled?**
  - Enable: `MmLogger.logFramework = true;`
  - Check console for routing decisions

---

### Checklist 2: Test Failing with 0 Message Count

When tests fail with responders showing 0 messages:

- [ ] **Hierarchy fully registered?**
  - Verify: `parentRelay.MmAddToRoutingTable(child, MmLevelFilter.Child);`
  - Verify: `childRelay.AddParent(parentRelay);`

- [ ] **MmRefreshResponders called?**
  - After creating all nodes: `relay.MmRefreshResponders();`
  - Wait extra frame: `yield return null;`

- [ ] **Level filter transformed?**
  - Check forwarding code includes: `forwardedMessage.MetadataBlock.LevelFilter = ...`
  - Should be `SelfAndChildren` or `Self`

- [ ] **Counter reset in [SetUp]?**
  - Verify: `[SetUp]` method calls `ResetAllCounters()`

---

### Checklist 3: Double-Delivery Issues

When responders receive messages multiple times:

- [ ] **Recursive routing using wrong filter?**
  - Should use `MmLevelFilter.Self` (not `SelfAndChildren`)
  - Check: `RouteRecursive()`, `CollectDescendants()`, `CollectAncestors()`

- [ ] **Parent and child both receiving intentionally?**
  - Verify filter: `SelfAndChildren` includes both
  - If unintended: Use `Child` only

---

## Common Pitfalls

1. **Tag checking enabled but tags not set** → Messages rejected silently
2. **LevelFilter default is `SelfAndChildren`** → Not `Self` alone
3. **Inactive GameObjects ignored by default** → Need `MmActiveFilter.All` to include
4. **FSM SelectedFilter only works with MmRelaySwitchNode** → Won't work with MmRelayNode
5. **Host machines receive messages twice** → Local + network, check `IsDeserialized`
6. **Transform.SetParent() only updates Unity hierarchy** → Doesn't update routing table
7. **Adding components at runtime doesn't auto-register** → Need `MmRefreshResponders()`
8. **Forwarding without level filter transformation** → Responders won't receive messages

---

## Reference Files

- **Core Routing Logic:** `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`
  - Standard routing pattern: See MmInvoke implementation
  - HandleAdvancedRouting: See advanced routing section
  - RouteLateral: See lateral routing methods
  - RouteRecursive: See recursive routing methods

- **Base Responder:** `Assets/MercuryMessaging/Protocol/Responders/MmBaseResponder.cs`
  - Method routing with switch statements
  - Virtual method signatures

- **Level Filters:** `Assets/MercuryMessaging/Protocol/MmLevelFilter.cs`
  - Enum definitions and helper combinations
  - Bitwise flag values

- **Test Examples:** `Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs`
  - CreateNodeWithResponder helper
  - MessageCounterResponder pattern
  - Test setup patterns

---

## Path Specification Patterns (Phase 2.1 - Path Implementation)

### Pattern: Wildcard Semantic

**Question:** What does `*` mean in paths like `"parent/*/child"`?

**Answer:** **Collection Expansion (fan-out multiplier)**

**Example:**
```
Hierarchy:
  Grandparent
  ├── Parent → You
  ├── Aunt → Cousin1, Cousin2
  └── Uncle → Cousin3

Path: "parent/*/child"
Execution:
1. parent → Parent node
2. * → ALL children of Parent's parent = [Parent, Aunt, Uncle]
3. child → Each node's children = [You, Cousin1, Cousin2, Cousin3]

Result: All siblings' children
```

**NOT:** XPath-style node name matching
**NOT:** Any relationship type

### Pattern: Path Resolution Level Filter

**Use `MmLevelFilter.Self` (NOT `SelfAndChildren`)**

**Why:**
- Path resolution already found exact targets
- No further routing propagation needed
- More explicit and safer

**Code:**
```csharp
foreach (var targetNode in ResolvePathTargets(path))
{
    var forwardedMessage = message.Copy();
    forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilter.Self;
    targetNode.MmInvoke(forwardedMessage);
}
```

**Comparison:**
- **Standard routing:** `SelfAndChildren` (needs to propagate)
- **Advanced routing:** `SelfAndChildren` (collection + routing)
- **Path routing:** `Self` (exact targets, no propagation)

### Pattern: Wildcard Validation

**Rules:**
- ❌ Cannot be first: `"*/child"` → No context to expand
- ❌ Cannot be last: `"parent/*"` → Nothing to navigate to
- ❌ Cannot be consecutive: `"parent/*/*"` → Ambiguous
- ✅ Mid-path valid: `"parent/*/child"` → Clear fan-out

---

---

## Network Test Scene Creation (FishNet)

### 4. ⚠️ CRITICAL: Every Responder Needs a Relay Node

**Bug:** Messages reach parent relay node but don't propagate to child responders

**Why This Happens:**
- Child GameObjects with only responders (no `MmRelayNode`) aren't properly routed
- `MmRefreshResponders()` only finds responders on the **same GameObject**
- `RefreshParents()` can throw `NullReferenceException` when iterating child responders without relay nodes

**Anti-Pattern (DON'T DO THIS):**
```csharp
// WRONG - Child responders without relay nodes
var responderObj = new GameObject("TestResponder1");
responderObj.transform.SetParent(testRootObj.transform);
responderObj.AddComponent<NetworkTestResponder>();  // Only responder, no relay node!

// This causes:
// 1. Routing table doesn't include this responder properly
// 2. Messages don't propagate to it
// 3. NullReferenceException in RefreshParents() when iterating
```

**Correct Pattern (DO THIS):**
```csharp
// CORRECT - Every responder GameObject has a relay node
var responderObj = new GameObject("TestResponder1");
responderObj.transform.SetParent(testRootObj.transform);

// Add relay node FIRST, then responder
var childRelay = responderObj.AddComponent<MmRelayNode>();
responderObj.AddComponent<NetworkTestResponder>();

// Register with parent
testRootRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(testRootRelay);
```

**Hierarchy Structure:**
```
TestRoot (MmRelayNode + NetworkTestResponder)      ← Correct
  ├─ TestResponder1 (MmRelayNode + NetworkTestResponder)  ← Correct
  ├─ TestResponder2 (MmRelayNode + NetworkTestResponder)  ← Correct
  └─ ChildNode (MmRelayNode)
      └─ NestedResponder1 (MmRelayNode + NetworkTestResponder)  ← Correct
```

**Files Fixed:** `NetworkTestSceneBuilder.cs` (2025-12-01)

---

### 5. ⚠️ Test Scene NetId Confusion (RootNode vs TestRoot)

**Bug:** Server/client targeting different relay nodes (different NetIds)

**Why This Happens:**
- Scene has leftover `RootNode` from earlier testing
- `FishNetTestManager` targets Inspector-assigned node (RootNode) instead of dynamically created TestRoot
- Build Test Hierarchy creates TestRoot but doesn't disable/remove RootNode

**Symptoms:**
- Client receives messages (targets TestRoot)
- Server sends to wrong NetId (targets old RootNode)
- Log shows: `Node found but NO responders attached!`

**Pattern:**
```csharp
// In CleanupTestObjects() - disable old scene objects
var oldRootNode = GameObject.Find("RootNode");
if (oldRootNode != null)
{
    oldRootNode.SetActive(false);
    Debug.Log("[SceneBuilder] Disabled old RootNode");
}

// In ResolveTargetNetId() - prioritize TestRoot
var testRoot = GameObject.Find("TestRoot");
if (testRoot != null)
{
    targetRelayNode = testRoot.GetComponent<MmRelayNode>();
    // Use TestRoot instead of Inspector-assigned RootNode
}
```

**Files Fixed:** `NetworkTestSceneBuilder.cs`, `FishNetTestManager.cs` (2025-12-01)

---

### 6. Network Test Scene Checklist

Before testing FishNet hierarchical routing:

- [ ] **Click "Build Test Hierarchy" on BOTH server AND client**
- [ ] **Verify TestRoot is active** (not the old RootNode)
- [ ] **Check responder count in logs:** Should show 4+ responders
- [ ] **Verify NetId matches on both sides:** Compare logs for same path-based ID
- [ ] **Each child responder has its own MmRelayNode**
- [ ] **MmAddToRoutingTable + AddParent called for each child**

**Debug Logging:**
```
[FishNetTest] Target set to TestRoot (NetId: 765583435)
[FishNetTest] [OK] Found 4 NetworkTestResponder(s) ready to receive messages
```

---

**Document Version:** 1.3
**Created:** 2025-11-21
**Updated:** 2025-12-01 (Added network test scene patterns, relay node requirement)

**Note:** This document will be updated as new bugs are discovered and patterns emerge. When you encounter a new bug, add it here following the established format.
