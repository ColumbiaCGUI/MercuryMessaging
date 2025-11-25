# MercuryMessaging DSL Overhaul - Task Checklist

**Last Updated:** 2025-11-25

---

## Phase 1: Core Messaging DSL 🟡 IN PROGRESS

### Task 1.1: Commit Uncommitted Work 🟡 IN PROGRESS
- [ ] Review 72 uncommitted files
- [ ] Create commit with descriptive message
- [ ] Verify commit successful

### Task 1.2: MmRelayNode Cleanup ⏳ NOT STARTED
- [ ] Remove `messageBuffer` field (~line 572-578)
- [ ] Remove `_prevMessageTime` field (~line 167)
- [ ] Remove `dirty` field (~line 184)
- [ ] Create `MmRelayNodeDebug.cs`
- [ ] Move debug fields to MmRelayNodeDebug:
  - [ ] `colorA`, `colorB`, `colorC`, `colorD`
  - [ ] `nodePosition`
  - [ ] `layer`
  - [ ] `positionOffset`
- [ ] Move color assignment from `Start()` to MmRelayNodeDebug
- [ ] Add optional reference in MmRelayNode: `[SerializeField] private MmRelayNodeDebug _debugVisualizer`
- [ ] Verify MmRelayNode compiles

### Task 1.3: Create MmMessagingExtensions.cs ⏳ NOT STARTED

**Tier 1: Relay Node Methods**
- [ ] `BroadcastInitialize(this MmRelayNode relay)`
- [ ] `BroadcastRefresh(this MmRelayNode relay)`
- [ ] `BroadcastSetActive(this MmRelayNode relay, bool active)`
- [ ] `BroadcastSwitch(this MmRelayNode relay, string state)`
- [ ] `BroadcastValue(this MmRelayNode relay, bool v)`
- [ ] `BroadcastValue(this MmRelayNode relay, int v)`
- [ ] `BroadcastValue(this MmRelayNode relay, float v)`
- [ ] `BroadcastValue(this MmRelayNode relay, string v)`
- [ ] `NotifyComplete(this MmRelayNode relay)`
- [ ] `NotifyValue(this MmRelayNode relay, bool v)`
- [ ] `NotifyValue(this MmRelayNode relay, int v)`
- [ ] `NotifyValue(this MmRelayNode relay, float v)`
- [ ] `NotifyValue(this MmRelayNode relay, string v)`

**Tier 1: Responder Methods (Thin Wrappers)**
- [ ] `BroadcastInitialize(this MmBaseResponder r)`
- [ ] `BroadcastRefresh(this MmBaseResponder r)`
- [ ] `BroadcastSetActive(this MmBaseResponder r, bool active)`
- [ ] `BroadcastSwitch(this MmBaseResponder r, string state)`
- [ ] `BroadcastValue(this MmBaseResponder r, bool v)`
- [ ] `BroadcastValue(this MmBaseResponder r, int v)`
- [ ] `BroadcastValue(this MmBaseResponder r, float v)`
- [ ] `BroadcastValue(this MmBaseResponder r, string v)`
- [ ] `NotifyComplete(this MmBaseResponder r)`
- [ ] `NotifyValue(this MmBaseResponder r, bool v)`
- [ ] `NotifyValue(this MmBaseResponder r, int v)`
- [ ] `NotifyValue(this MmBaseResponder r, float v)`
- [ ] `NotifyValue(this MmBaseResponder r, string v)`

### Task 1.4: Add Tier 2 Fluent for Responders ⏳ NOT STARTED
- [ ] `Send(this MmBaseResponder r, object payload)`
- [ ] `Send(this MmBaseResponder r, MmMethod method)`
- [ ] `Send(this MmBaseResponder r, MmMethod method, object payload)`
- [ ] Handle null relay node (return default MmFluentMessage)

### Task 1.5: Deprecate Old APIs ⏳ NOT STARTED

**MmQuickExtensions.cs**
- [ ] Add `[Obsolete("Use BroadcastInitialize() from MmMessagingExtensions")]` to `Init()`
- [ ] Add `[Obsolete("Use NotifyComplete() from MmMessagingExtensions")]` to `Done()`
- [ ] Add `[Obsolete("Use BroadcastRefresh() from MmMessagingExtensions")]` to `Sync()`
- [ ] Add `[Obsolete("Use BroadcastValue() from MmMessagingExtensions")]` to `Tell()` variants
- [ ] Add `[Obsolete("Use NotifyValue() from MmMessagingExtensions")]` to `Report()` variants
- [ ] Add `[Obsolete]` to `Activate()` and `State()`

**MmRelayNodeExtensions.cs**
- [ ] Add `[Obsolete]` to redundant `Broadcast()` overloads
- [ ] Add `[Obsolete]` to redundant `Notify()` overloads

### Task 1.6: Fix DSL_Comparison Test ⏳ NOT STARTED
- [ ] Open `MessageGenerator_DSL.cs`
- [ ] Find default case in SendDSL_String (around line 285)
- [ ] Change from `relayNode.Broadcast(MmMethod.MessageString, value)`
- [ ] To `relayNode.Send(value).Execute()`
- [ ] Apply same fix to other SendDSL_* methods
- [ ] Verify test runs and shows reduced overhead

### Task 1.7: Create Tests ⏳ NOT STARTED

**Create MmMessagingExtensionsTests.cs**
- [ ] Create file in `Assets/MercuryMessaging/Tests/`
- [ ] Add necessary using statements
- [ ] Create TestResponder helper class

**Tier 1 Relay Node Tests**
- [ ] `BroadcastInitialize_SendsToDescendants`
- [ ] `BroadcastRefresh_SendsToDescendants`
- [ ] `BroadcastSetActive_SendsToDescendants`
- [ ] `BroadcastSwitch_SendsToDescendants`
- [ ] `BroadcastValue_Bool_SendsToDescendants`
- [ ] `BroadcastValue_Int_SendsToDescendants`
- [ ] `BroadcastValue_Float_SendsToDescendants`
- [ ] `BroadcastValue_String_SendsToDescendants`
- [ ] `NotifyComplete_SendsToParents`
- [ ] `NotifyValue_Bool_SendsToParents`
- [ ] `NotifyValue_Int_SendsToParents`
- [ ] `NotifyValue_Float_SendsToParents`
- [ ] `NotifyValue_String_SendsToParents`

**Tier 1 Responder Tests**
- [ ] `Responder_BroadcastInitialize_SendsToDescendants`
- [ ] `Responder_BroadcastValue_SendsToDescendants`
- [ ] `Responder_NotifyComplete_SendsToParents`
- [ ] `Responder_NotifyValue_SendsToParents`
- [ ] `Responder_WithNullRelayNode_DoesNotThrow`

**Tier 2 Responder Fluent Tests**
- [ ] `Responder_Send_ReturnsFluentMessage`
- [ ] `Responder_Send_ToDescendants_Execute_Works`
- [ ] `Responder_Send_ToParents_WithTag_Execute_Works`
- [ ] `Responder_Send_WithNullRelayNode_ReturnsDefault`

**Naming Convention Tests**
- [ ] `BroadcastInitialize_MatchesMmMethodInitialize`
- [ ] `NotifyComplete_MatchesMmMethodComplete`

**Performance Tests (in FluentApiPerformanceTests.cs)**
- [ ] `UnifiedAPI_BroadcastInitialize_Performance`
- [ ] `UnifiedAPI_ResponderSend_Performance`

### Task 1.8: Update Documentation ⏳ NOT STARTED

**CLAUDE.md**
- [ ] Update Fluent DSL section with unified API
- [ ] Add responder examples
- [ ] Update migration guide
- [ ] Remove references to deprecated methods

**DSL_API_GUIDE.md**
- [ ] Update tier hierarchy documentation
- [ ] Add examples for both relay nodes and responders
- [ ] Document deprecations

---

## Phase 2: FSM Configuration DSL ⏳ NOT STARTED
- [ ] Create `FsmConfigBuilder.cs`
- [ ] Create `MmRelaySwitchNodeExtensions.cs`
- [ ] Add fluent FSM setup methods
- [ ] Create tests
- [ ] Update documentation

---

## Phase 3: Data Collection DSL ⏳ NOT STARTED
- [ ] Create `MmDataCollectorExtensions.cs`
- [ ] Create `DataCollectionBuilder.cs`
- [ ] Add fluent data collection setup
- [ ] Create tests
- [ ] Update documentation

---

## Phase 4: Task Management DSL ⏳ NOT STARTED
- [ ] Create `MmTaskSequenceBuilder.cs`
- [ ] Create `MmTaskManagerExtensions.cs`
- [ ] Add fluent task sequence setup
- [ ] Create tests
- [ ] Update documentation

---

## Phase 5: Network Message DSL ⏳ NOT STARTED
- [ ] Add `.OverNetwork()` to MmFluentMessage
- [ ] Create `MmNetworkExtensions.cs`
- [ ] Handle host double-receive
- [ ] Create tests
- [ ] Update documentation

---

## Phase 6: Responder Registration DSL ⏳ NOT STARTED
- [ ] Create `MmResponderSetupBuilder.cs`
- [ ] Add fluent responder setup
- [ ] Create tests
- [ ] Update documentation

---

## Phase 7: Hierarchy Building DSL ⏳ NOT STARTED
- [ ] Create `MmHierarchyBuilder.cs`
- [ ] Add fluent hierarchy building
- [ ] Create tests
- [ ] Update documentation

---

## Phase 8: App State DSL ⏳ NOT STARTED
- [ ] Create `MmAppStateBuilder.cs`
- [ ] Add fluent app state setup
- [ ] Create tests
- [ ] Update documentation

---

## Final Steps

### Run All Tests
- [ ] Run Unity Test Runner (PlayMode)
- [ ] Verify 292 existing tests pass
- [ ] Verify ~28 new tests pass
- [ ] Check for warnings/errors in console

### Final Commit
- [ ] Review all changes
- [ ] Create comprehensive commit message
- [ ] Push to branch

---

## Progress Summary

| Phase | Status | Completion |
|-------|--------|------------|
| Phase 1: Core | 🟡 In Progress | 0% |
| Phase 2: FSM | ⏳ Not Started | 0% |
| Phase 3: Data | ⏳ Not Started | 0% |
| Phase 4: Tasks | ⏳ Not Started | 0% |
| Phase 5: Network | ⏳ Not Started | 0% |
| Phase 6: Responders | ⏳ Not Started | 0% |
| Phase 7: Hierarchy | ⏳ Not Started | 0% |
| Phase 8: App State | ⏳ Not Started | 0% |
