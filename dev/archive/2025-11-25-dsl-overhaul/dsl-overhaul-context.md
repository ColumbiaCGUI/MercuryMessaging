# MercuryMessaging DSL Overhaul - Context

**Last Updated:** 2025-11-25 (Session 4 - Phases 1-8 COMPLETE, Test Fixes COMPLETE)
**Full Plan:** `.claude/plans/compressed-wandering-fog.md`

---

## PHASES 1-8 COMPLETE!

**All 418 tests passing after test fixes.**

### What Was Delivered
- **Phase 1:** MmMessagingExtensions.cs - Unified Two-Tier API
- **Phase 2:** FSM Configuration DSL - ConfigureStates().OnEnter().Build()
- **Phase 3:** Data Collection DSL - Configure().OutputAsCsv().Collect().Start()
- **Phase 4:** Task Management DSL - MmTaskSequence.Create().Build()
- **Phase 5:** Network Message DSL - .OverNetwork() method
- **Phase 6:** Responder Registration DSL - SetupMercury().AddResponder().Build()
- **Phase 7:** Hierarchy Building DSL - MmHierarchy.Build().AddChild().Build()
- **Phase 8:** App State DSL - MmAppState.Configure().DefineState().OnEnter().Build()

### Test Fixes Applied (Session 4)
- **MmMessagingExtensions.cs:** Removed `.ToDescendants()` from Broadcast methods (use default SelfAndChildren)
- **MmRelaySwitchNode.cs:** Added `RebuildFSM()` method for runtime routing table changes
- **FsmConfigBuilderTests.cs:** Added `RebuildFSM()` call after routing table setup
- **AppStateBuilderTests.cs:** Added routing table registration + `RebuildFSM()` call

---

## SESSION PROGRESS (2025-11-25, Session 4)

### ✅ COMPLETED THIS SESSION
- Analyzed 20 failing tests from TestResults_20251125_125156.xml
- Identified 3 root causes:
  1. AppStateBuilderTests: Missing routing table registration + RebuildFSM
  2. FsmConfigBuilderTests: Missing RebuildFSM after setup
  3. FluentApiTests.BroadcastInitialize: ToDescendants excludes self
- Applied all fixes (4 files modified)
- **All 418 tests now passing**

### 🟡 NEXT TO DO
- **Phase 9:** UI Messages (Standard Library)
- **Phase 10:** Input Messages (Standard Library)
- **Phase 11:** Tutorials & Documentation

### ⚠️ BLOCKERS
- None

---

## Quick Resume

**To continue this task:**
1. Read this context file
2. Check `dsl-overhaul-tasks.md` for detailed task checklist
3. Full approved plan: `.claude/plans/compressed-wandering-fog.md`

**Next actions:**
1. Create `Assets/MercuryMessaging/StandardLibrary/UI/` folder
2. Implement MmUIMessages.cs with UI message types
3. Implement MmUIResponder.cs base responder
4. Create UIMessageTests.cs

---

## Key Files

### Created/Modified This Session (Session 4)
- `Assets/MercuryMessaging/Protocol/DSL/MmMessagingExtensions.cs` - Removed ToDescendants() from Broadcast methods
- `Assets/MercuryMessaging/Protocol/MmRelaySwitchNode.cs` - Added RebuildFSM() method
- `Assets/MercuryMessaging/Tests/FsmConfigBuilderTests.cs` - Added RebuildFSM() call
- `Assets/MercuryMessaging/Tests/AppStateBuilderTests.cs` - Added routing registration + RebuildFSM()

### DSL Files (All Complete)
- `Assets/MercuryMessaging/Protocol/DSL/MmMessagingExtensions.cs` - Unified Tier 1/2 API
- `Assets/MercuryMessaging/Protocol/DSL/MmFluentMessage.cs` - Core fluent builder
- `Assets/MercuryMessaging/Protocol/DSL/MmFluentExtensions.cs` - Routing extensions
- `Assets/MercuryMessaging/Protocol/DSL/MmRelaySwitchNodeExtensions.cs` - FSM extensions
- `Assets/MercuryMessaging/Protocol/DSL/AppStateBuilder.cs` - App state builder
- `Assets/MercuryMessaging/Protocol/DSL/MmAppStateExtensions.cs` - App state extensions
- `Assets/MercuryMessaging/Protocol/DSL/HierarchyBuilder.cs` - Hierarchy builder
- `Assets/MercuryMessaging/Protocol/DSL/ResponderSetupBuilder.cs` - Responder setup
- `Assets/MercuryMessaging/Protocol/DSL/MmNetworkExtensions.cs` - Network extensions
- `Assets/MercuryMessaging/Support/Data/DataCollectionBuilder.cs` - Data collection
- `Assets/MercuryMessaging/Task/TaskSequenceBuilder.cs` - Task sequence

### Test Files (All Passing)
- `Assets/MercuryMessaging/Tests/MmMessagingExtensionsTests.cs` - 27 tests
- `Assets/MercuryMessaging/Tests/FluentApiTests.cs` - Core fluent tests
- `Assets/MercuryMessaging/Tests/FsmConfigBuilderTests.cs` - FSM tests
- `Assets/MercuryMessaging/Tests/AppStateBuilderTests.cs` - App state tests

---

## Key Technical Decisions

### 1. BroadcastInitialize Semantic Fix
**Problem:** `BroadcastInitialize()` used `.ToDescendants()` which sets `MmLevelFilter.Descendants` (excludes self)
**Solution:** Remove `.ToDescendants()` to use default `SelfAndChildren` (includes self)
**Why:** "Broadcast to self and children" is the intuitive interpretation, and tests with no children need self-delivery

### 2. RebuildFSM() Method
**Problem:** `MmRelaySwitchNode.Awake()` builds FSM from routing table, but in tests, routing table is empty at Awake time
**Solution:** Added `RebuildFSM()` method to reconstruct FSM after routing table is populated
**Usage:** Call after `MmAddToRoutingTable()` in test setup

### 3. Standard Library Method Ranges
- **Standard MmMethod:** 0-18 (existing)
- **UI Messages:** 100-199 (Phase 9)
- **Input Messages:** 200-299 (Phase 10)
- **Custom Application:** 1000+ (unchanged)

---

## Phase Overview

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Core Messaging DSL | ✅ Complete |
| 2 | FSM Configuration | ✅ Complete |
| 3 | Data Collection | ✅ Complete |
| 4 | Task Management | ✅ Complete |
| 5 | Network Messages | ✅ Complete |
| 6 | Responder Registration | ✅ Complete |
| 7 | Hierarchy Building | ✅ Complete |
| 8 | App State | ✅ Complete |
| 9 | UI Messages | ⏳ Next |
| 10 | Input Messages | ⏳ Pending |
| 11 | Tutorials | ⏳ Pending |

---

## Code Patterns

### Tier 1 Method Pattern (Fixed)
```csharp
// Uses default SelfAndChildren (includes self)
public static void BroadcastInitialize(this MmRelayNode relay)
    => relay.Send(MmMethod.Initialize).Execute();

// NOT ToDescendants() which excludes self
```

### RebuildFSM Pattern (New)
```csharp
// In test setup
_switchNode = obj.AddComponent<MmRelaySwitchNode>();
_switchNode.MmAddToRoutingTable(state1, MmLevelFilter.Child);
_switchNode.MmAddToRoutingTable(state2, MmLevelFilter.Child);
_switchNode.RebuildFSM(); // CRITICAL: Must call after populating routing table
```

### UI Message Pattern (Phase 9 - To Implement)
```csharp
// Message definition
public class MmUIClickMessage : MmMessage
{
    public Vector2 Position;
    public int ClickCount;
    public bool IsDoubleClick => ClickCount >= 2;
}

// Usage
relay.Send(new MmUIClickMessage { Position = pos, ClickCount = 2 }).Execute();
```

---

## Related Files

- **Approved Plan:** `.claude/plans/compressed-wandering-fog.md`
- **Task Checklist:** `dev/active/dsl-overhaul/dsl-overhaul-tasks.md`
- **Test Results:** `Assets/Resources/test-results/TestResults_20251125_125156.xml`
- **Session Handoff:** `dev/active/SESSION_HANDOFF_2025-11-25.md`
