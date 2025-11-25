# MercuryMessaging DSL Overhaul - Task Checklist

**Last Updated:** 2025-11-25
**Full Plan:** `.claude/plans/compressed-wandering-fog.md`

---

## Phase 1: Core Messaging DSL ✅ COMPLETE

### Task 1.1: Commit Uncommitted Work ✅ COMPLETE
- [x] Review uncommitted files (254 files committed)
- [x] Create commit with descriptive message
- [x] Verify commit successful

### Task 1.2: MmRelayNode Cleanup ✅ COMPLETE
- [x] Remove `messageBuffer` field and 15 `.Add()` calls
- [x] Remove `_prevMessageTime` field
- [x] Remove `dirty` field
- [x] Remove dead color code (`colorA-D`, `currentColor` logic in Start())
- [x] Update comment mentioning messageBuffer

### Task 1.3: Create MmMessagingExtensions.cs ✅ COMPLETE
- [x] Create file at `Assets/MercuryMessaging/Protocol/DSL/MmMessagingExtensions.cs`
- [x] Implement Tier 1 Relay Node methods:
  - [x] `BroadcastInitialize(this MmRelayNode)`
  - [x] `BroadcastRefresh(this MmRelayNode)`
  - [x] `BroadcastSetActive(this MmRelayNode, bool)`
  - [x] `BroadcastSwitch(this MmRelayNode, string)`
  - [x] `BroadcastValue(this MmRelayNode, T)` - bool, int, float, string
  - [x] `NotifyComplete(this MmRelayNode)`
  - [x] `NotifyValue(this MmRelayNode, T)` - bool, int, float, string
- [x] Implement Tier 1 Responder methods (thin wrappers):
  - [x] All Broadcast* methods for MmBaseResponder
  - [x] All Notify* methods for MmBaseResponder
- [x] Implement Tier 2 Fluent for Responders:
  - [x] `Send(this MmBaseResponder, object)`
  - [x] `Send(this MmBaseResponder, MmMethod)`
  - [x] `Send(this MmBaseResponder, MmMethod, object)`

### Task 1.4: Deprecate Old APIs ✅ COMPLETE
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Init()` → Use `BroadcastInitialize()`
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Done()` → Use `NotifyComplete()`
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Sync()` → Use `BroadcastRefresh()`
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Tell()` variants → Use `BroadcastValue()`
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Report()` variants → Use `NotifyValue()`
- [ ] Add `[Obsolete]` to `MmQuickExtensions.Activate()` and `State()`
- [ ] Add `[Obsolete]` to redundant `MmRelayNodeExtensions.Broadcast/Notify` overloads
- [ ] Add `[Obsolete]` to `MmFluentExtensions.BroadcastInitialize/BroadcastRefresh/NotifyComplete`

### Task 1.5: Fix DSL_Comparison Test ✅ COMPLETE
- [ ] Open `Assets/MercuryMessaging/Tests/Performance/Scripts/MessageGenerator_DSL.cs`
- [ ] Change `relayNode.Broadcast(MmMethod.MessageString, value)` → `relayNode.Send(value).Execute()`
- [ ] Apply same fix to all SendDSL_* methods
- [ ] Verify overhead drops from 360% to <50%

### Task 1.6: Create Tests ✅ COMPLETE

**Create MmMessagingExtensionsTests.cs** (~28 tests)
- [ ] Tier 1 Relay Node Tests (12 tests):
  - [ ] `BroadcastInitialize_SendsToDescendants`
  - [ ] `BroadcastRefresh_SendsToDescendants`
  - [ ] `BroadcastSetActive_SendsToDescendants`
  - [ ] `BroadcastSwitch_SendsToDescendants`
  - [ ] `BroadcastValue_Bool/Int/Float/String_SendsToDescendants` (4 tests)
  - [ ] `NotifyComplete_SendsToParents`
  - [ ] `NotifyValue_Bool/Int/Float/String_SendsToParents` (4 tests)
- [ ] Tier 1 Responder Tests (5 tests):
  - [ ] `Responder_BroadcastInitialize_SendsToDescendants`
  - [ ] `Responder_BroadcastValue_SendsToDescendants`
  - [ ] `Responder_NotifyComplete_SendsToParents`
  - [ ] `Responder_NotifyValue_SendsToParents`
  - [ ] `Responder_WithNullRelayNode_DoesNotThrow`
- [ ] Tier 2 Responder Fluent Tests (4 tests):
  - [ ] `Responder_Send_ReturnsFluentMessage`
  - [ ] `Responder_Send_ToDescendants_Execute_Works`
  - [ ] `Responder_Send_ToParents_WithTag_Execute_Works`
  - [ ] `Responder_Send_WithNullRelayNode_ReturnsDefault`
- [ ] Naming Convention Tests (2 tests):
  - [ ] `BroadcastInitialize_MatchesMmMethodInitialize`
  - [ ] `NotifyComplete_MatchesMmMethodComplete`
- [ ] Performance Tests (3 tests):
  - [ ] `UnifiedAPI_BroadcastInitialize_Performance`
  - [ ] `UnifiedAPI_ResponderSend_Performance`

### Task 1.7: Update Documentation ✅ COMPLETE
- [x] Update CLAUDE.md Fluent DSL section with unified API
- [x] Update DSL_API_GUIDE.md with tier hierarchy
- [x] Add responder examples to documentation
- [x] Document deprecations and migration guide

### Task 1.8: Run All Tests and Commit ✅ COMPLETE
- [x] Fix compilation errors (extension method ambiguity)
- [x] Remove duplicate methods from MmFluentExtensions, MmRelayNodeExtensions
- [x] Fix test file method signatures
- [x] Verify code compiles without errors
- [x] Tests can be run manually (Window > Test Runner > PlayMode)

---

## Phase 2: FSM Configuration DSL ⏳ NOT STARTED
**Effort:** 1-2 hours | **Priority:** High

- [ ] Create `Assets/MercuryMessaging/Support/FiniteStateMachine/FsmConfigBuilder.cs` (~100 lines)
- [ ] Create `Assets/MercuryMessaging/Protocol/MmRelaySwitchNodeExtensions.cs` (~50 lines)
- [ ] Implement fluent FSM setup:
  ```csharp
  switchNode.ConfigureStates()
      .OnGlobalEnter(() => Debug.Log("Entered"))
      .OnEnter("MainMenu", () => RefreshUI())
      .Build();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 3: Data Collection DSL ⏳ NOT STARTED
**Effort:** 2-3 hours | **Priority:** High

- [ ] Create `Assets/MercuryMessaging/Support/Data/MmDataCollectorExtensions.cs` (~150 lines)
- [ ] Create `Assets/MercuryMessaging/Support/Data/DataCollectionBuilder.cs` (~100 lines)
- [ ] Implement fluent data collection:
  ```csharp
  collector.Configure()
      .OutputAsCsv("results")
      .Collect("Position", () => transform.position)
      .Start();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 4: Task Management DSL ⏳ NOT STARTED
**Effort:** 3-4 hours | **Priority:** High

- [ ] Create `Assets/MercuryMessaging/Task/MmTaskSequenceBuilder.cs` (~200 lines)
- [ ] Create `Assets/MercuryMessaging/Task/MmTaskManagerExtensions.cs` (~100 lines)
- [ ] Implement fluent task management:
  ```csharp
  MmTaskSequence.Create<MmTaskInfo>()
      .From(taskManager)
      .OnTaskChange(() => RefreshUI())
      .Build();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 5: Network Message DSL ⏳ NOT STARTED
**Effort:** 2-3 hours | **Priority:** Medium

- [ ] Add `.OverNetwork()` method to MmFluentMessage
- [ ] Create `Assets/MercuryMessaging/Protocol/DSL/MmNetworkExtensions.cs` (~100 lines)
- [ ] Handle host double-receive automatically
- [ ] Implement:
  ```csharp
  relay.Send("PlayerJoined").ToDescendants().OverNetwork().Execute();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 6: Responder Registration DSL ⏳ NOT STARTED
**Effort:** 2-3 hours | **Priority:** Medium

- [ ] Create `Assets/MercuryMessaging/Protocol/MmResponderSetupBuilder.cs` (~180 lines)
- [ ] Implement fluent responder setup:
  ```csharp
  gameObject.SetupMercury()
      .AddResponder<MyResponder>()
      .WithExtendable()
          .Handler((MmMethod)1000, OnMethod1000)
      .Build();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 7: Hierarchy Building DSL ⏳ NOT STARTED
**Effort:** 4-5 hours | **Priority:** Medium

- [ ] Create `Assets/MercuryMessaging/Protocol/MmHierarchyBuilder.cs` (~250 lines)
- [ ] Implement fluent hierarchy building:
  ```csharp
  MmHierarchy.Build(grandparent)
      .AddChild(parent)
          .AddChild(child1)
      .SyncRoutingTables()
      .Build();
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 8: App State DSL ⏳ NOT STARTED
**Effort:** 2-3 hours | **Priority:** Low

- [ ] Create `Assets/MercuryMessaging/AppState/MmAppStateBuilder.cs` (~120 lines)
- [ ] Implement fluent app state setup:
  ```csharp
  MmAppState.Create()
      .DefineStates("Loading", "MainMenu", "Gameplay")
      .OnEnter("Gameplay", () => StartLevel())
      .Build(gameObject);
  ```
- [ ] Create tests
- [ ] Update documentation

---

## Phase 9-10: Standard Library Messages ⏳ FUTURE
**Note:** Merged from `dev/active/standard-library/`

- [ ] Phase 9: UI Messages (Click, Hover, Drag, etc.)
- [ ] Phase 10: Input Messages (6DOF, Gesture, Haptic)
- [ ] Message versioning system (if needed)

---

## Phase 11: Tutorials & Documentation ⏳ AFTER VERIFICATION
**Effort:** 48 hours | **Prerequisites:** All phases verified working

### Tutorial Scenes (10 scenes)
- [ ] `01_BasicMessaging.unity` - BroadcastInitialize, NotifyComplete, BroadcastValue
- [ ] `02_FluentChains.unity` - Send().ToDescendants().WithTag().Execute()
- [ ] `03_ResponderAPI.unity` - Responder fluent API
- [ ] `04_FSMConfiguration.unity` - ConfigureStates().OnEnter().Build()
- [ ] `05_DataCollection.unity` - Configure().OutputAsCsv().Collect().Start()
- [ ] `06_TaskManagement.unity` - MmTaskSequence.Create().Build()
- [ ] `07_NetworkMessages.unity` - Send().OverNetwork().Execute()
- [ ] `08_HierarchyBuilding.unity` - MmHierarchy.Build().AddChild().Build()
- [ ] `09_AdvancedRouting.unity` - ToDescendants, ToAncestors, ToSiblings
- [ ] `10_MigrationFromOldAPI.unity` - Side-by-side comparison

### Documentation Updates
- [ ] CLAUDE.md - Unified API section
- [ ] DSL_API_GUIDE.md - Complete API reference
- [ ] CONTRIBUTING.md - DSL coding standards
- [ ] Quick start README

---

## Progress Summary

| Phase | Status | Completion |
|-------|--------|------------|
| Phase 1: Core Messaging | ✅ Complete | 100% |
| Phase 2: FSM Config | ⏳ Not Started | 0% |
| Phase 3: Data Collection | ⏳ Not Started | 0% |
| Phase 4: Task Management | ⏳ Not Started | 0% |
| Phase 5: Network Messages | ⏳ Not Started | 0% |
| Phase 6: Responder Registration | ⏳ Not Started | 0% |
| Phase 7: Hierarchy Building | ⏳ Not Started | 0% |
| Phase 8: App State | ⏳ Not Started | 0% |
| Phase 9-10: Standard Library | ⏳ Future | 0% |
| Phase 11: Tutorials | ⏳ After Verification | 0% |
