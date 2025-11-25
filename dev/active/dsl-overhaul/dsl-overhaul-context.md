# MercuryMessaging DSL Overhaul - Context

**Last Updated:** 2025-11-25

---

## SESSION PROGRESS (2025-11-25)

### ✅ COMPLETED
- Full planning phase completed
- Approved plan saved to `.claude/plans/compressed-wandering-fog.md`
- Dev docs structure created

### 🟡 IN PROGRESS
- Task 1.1: Commit uncommitted DSL work (72 files)

### ⏳ NOT STARTED
- Task 1.2: MmRelayNode cleanup
- Task 1.3: Create MmMessagingExtensions.cs
- Task 1.4: Add Tier 2 Fluent for Responders
- Task 1.5: Deprecate old APIs
- Task 1.6: Fix DSL_Comparison test
- Task 1.7: Create tests (~28 new tests)
- Task 1.8: Update documentation

### ⚠️ BLOCKERS
- None

---

## Quick Resume

**To continue this task:**
1. Read this context file first
2. Check `dsl-overhaul-tasks.md` for current progress
3. The approved plan is at `.claude/plans/compressed-wandering-fog.md`
4. Current focus: Phase 1 - Core Messaging DSL

**Next action:** Complete Task 1.1 (commit uncommitted work), then proceed to Task 1.2 (MmRelayNode cleanup)

---

## Key Files

### Core Protocol Files

**`Assets/MercuryMessaging/Protocol/MmRelayNode.cs`** (1400+ lines)
- Central message router
- Contains unused fields to remove: `messageBuffer`, `_prevMessageTime`, `dirty`
- Contains debug fields to extract: `colorA-D`, `nodePosition`, `layer`, `positionOffset`

**`Assets/MercuryMessaging/Protocol/MmBaseResponder.cs`** (318 lines)
- Base class for all responders
- Has `GetRelayNode()` method for relay access
- Will receive new extension methods for unified API

**`Assets/MercuryMessaging/Protocol/MmResponder.cs`** (197 lines)
- Abstract base class
- Defines `GetRelayNode()` abstract method

### DSL Files

**`Assets/MercuryMessaging/Protocol/DSL/MmFluentMessage.cs`** (1069 lines)
- Core fluent builder struct
- Already has optimizations: fast path, cached flags, pre-allocated defaults
- Used by all fluent operations

**`Assets/MercuryMessaging/Protocol/DSL/MmFluentExtensions.cs`** (334 lines)
- Entry points for `Send()` methods on relay nodes
- Has 14 overloads (to be consolidated)
- Contains [Obsolete] methods: `BroadcastInitialize()`, `BroadcastRefresh()`

**`Assets/MercuryMessaging/Protocol/DSL/MmQuickExtensions.cs`** (186 lines)
- Ultra-minimal methods: `Init()`, `Done()`, `Sync()`, `Tell()`, `Report()`
- Will be deprecated in favor of MmMessagingExtensions

**`Assets/MercuryMessaging/Protocol/DSL/MmRelayNodeExtensions.cs`** (554 lines)
- Convenience methods: `Broadcast()`, `Notify()`, `SendTo()`
- Partially deprecated for redundant methods

### Test Files

**`Assets/MercuryMessaging/Tests/FluentApiTests.cs`**
- Existing fluent API tests
- Will add responder fluent tests

**`Assets/MercuryMessaging/Tests/FluentApiPerformanceTests.cs`**
- Performance overhead tests
- Will add unified API performance tests

**`Assets/MercuryMessaging/Tests/Performance/Scripts/MessageGenerator_DSL.cs`**
- DSL version of message generator
- Currently uses `Broadcast()` (tests wrappers, not fluent)
- Needs fix to use `Send().Execute()` for fair comparison

### Documentation Files

**`CLAUDE.md`**
- Main project documentation
- Contains Fluent DSL section (lines 468-587)
- Needs update for unified API

**`Assets/MercuryMessaging/Protocol/DSL/DSL_API_GUIDE.md`**
- Full DSL documentation
- Needs tier hierarchy update

---

## Key Technical Decisions

### 1. Naming Convention
**Decision:** Use `Broadcast` + MmMethod name for down, `Notify` + MmMethod for up
**Rationale:** Matches original `MmInvoke(MmMethod.Xxx, ...)` pattern
**Examples:**
- `BroadcastInitialize()` ↔ `MmMethod.Initialize`
- `NotifyComplete()` ↔ `MmMethod.Complete`
- `BroadcastValue(42)` ↔ `MmMethod.MessageInt`

### 2. Responder Fluent API
**Decision:** Add `Send()` extension methods directly to MmBaseResponder
**Rationale:** Responders send 39+ messages in codebase; same API reduces cognitive load
**Implementation:**
```csharp
public static MmFluentMessage Send(this MmBaseResponder r, object payload)
    => r.GetRelayNode()?.Send(payload) ?? default;
```

### 3. Debug Field Extraction
**Decision:** Create separate `MmRelayNodeDebug` component
**Rationale:** Keeps MmRelayNode clean; debug fields only needed for visualization
**Fields to move:** `colorA-D`, `nodePosition`, `layer`, `positionOffset`

### 4. API Consolidation Strategy
**Decision:** Deprecate with [Obsolete], don't remove
**Rationale:** Backward compatibility; existing code continues to work
**Deprecation targets:** `MmQuickExtensions`, redundant `Broadcast/Notify` variants

### 5. Fluent Over Operators
**Decision:** Use fluent APIs instead of custom operators
**Rationale:**
- C# can't create custom operators like `>>` or `|>`
- Fluent has full IntelliSense support
- Industry standard (LINQ, DOTween, etc.)

---

## Code Patterns

### Tier 1 Method Pattern
```csharp
// Relay node version (primary)
public static void BroadcastInitialize(this MmRelayNode relay)
    => relay.Send(MmMethod.Initialize).ToDescendants().Execute();

// Responder version (thin wrapper)
public static void BroadcastInitialize(this MmBaseResponder r)
    => r.GetRelayNode()?.BroadcastInitialize();
```

### Tier 2 Responder Pattern
```csharp
public static MmFluentMessage Send(this MmBaseResponder r, object payload)
    => r.GetRelayNode()?.Send(payload) ?? default;
```

### Test Pattern
```csharp
[UnityTest]
public IEnumerator BroadcastInitialize_SendsToDescendants()
{
    // Setup hierarchy
    var parent = CreateTestHierarchy();
    var childResponder = parent.GetComponentInChildren<TestResponder>();

    // Act
    parent.GetComponent<MmRelayNode>().BroadcastInitialize();
    yield return null;

    // Assert
    Assert.AreEqual(MmMethod.Initialize, childResponder.LastReceivedMethod);
}
```

---

## Dependencies

### External Dependencies
- Unity Test Framework (for new tests)
- No new external packages required

### Internal Dependencies
- `MmFluentMessage` (already exists, fully functional)
- `MmFluentExtensions.Send()` (already exists, extend for responders)
- `MmBaseResponder.GetRelayNode()` (already exists)

### File Dependencies (Execution Order)
1. `MmRelayNode.cs` must be cleaned FIRST (removes unused fields)
2. `MmRelayNodeDebug.cs` created (holds extracted debug fields)
3. `MmMessagingExtensions.cs` created (unified API)
4. Old APIs deprecated with [Obsolete]
5. Tests created/modified
6. Documentation updated

---

## Performance Considerations

### Expected Overhead
- Tier 1 methods: ~0% overhead (direct delegation to fluent)
- Tier 2 responder: ~1-5% overhead (null check + delegation)
- Fast path optimizations already in MmFluentMessage

### Optimizations Applied
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on all methods
- Struct-based MmFluentMessage (no heap allocation)
- Cached `_needsTargetCollection` flag
- Pre-allocated `DefaultMetadata`

---

## Related Files

### Session Handoff
- `dev/active/SESSION_HANDOFF_2025-11-25.md` - Previous session progress

### Approved Plan
- `.claude/plans/compressed-wandering-fog.md` - Full approved plan

### Archive Reference
- `dev/archive/2025-11-25-language-dsl/` - Previous DSL implementation archive
