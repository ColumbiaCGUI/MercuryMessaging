# Tutorial Validation - Task Checklist

Granular validation checklist for all 12 MercuryMessaging tutorials.

---

## Phase 1: Core Tutorials (1-5)

### Task 1.1: Tutorial 1 - Introduction
- [ ] Load `Tutorial1_Base.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-01-introduction.md`
- [ ] Verify MmRelayNode sends messages correctly
- [ ] Verify MmBaseResponder receives messages
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 1.2: Tutorial 2 - Basic Routing
- [ ] Load `Tutorial2_Base.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-02-basic-routing.md`
- [ ] Test MmLevelFilter.Child routing
- [ ] Test MmLevelFilter.Parent routing
- [ ] Test MmLevelFilter.SelfAndChildren routing
- [ ] Verify keyboard controls match wiki
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 1.3: Tutorial 3 - Custom Responders
- [ ] Load `Tutorial3_Base.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-03-custom-responders.md`
- [ ] Verify ReceivedMessage(MmMessageString) handler
- [ ] Verify ReceivedMessage(MmMessageInt) handler
- [ ] Verify ReceivedSetActive handler
- [ ] Verify ReceivedInitialize handler
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 1.4: Tutorial 4 - Custom Messages
- [ ] Load `Tutorial4_Base.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-04-custom-messages.md`
- [ ] Verify custom MmMessage subclass creation
- [ ] Verify custom message serialization
- [ ] Verify custom message handling in responder
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 1.5: Tutorial 5 - Fluent DSL API
- [ ] Load `Tutorial5_Base.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-05-fluent-dsl-api.md`
- [ ] Test BroadcastInitialize()
- [ ] Test BroadcastRefresh()
- [ ] Test BroadcastSetActive()
- [ ] Test Send().ToChildren().Execute()
- [ ] Test Send().ToParents().Execute()
- [ ] Test NotifyComplete()
- [ ] Verify keyboard controls match wiki
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

## Phase 2: Networking Tutorials (6-7)

### Task 2.1: Tutorial 6 - FishNet Networking
- [ ] Verify FishNet package is installed (`#if FISH_NET`)
- [ ] Load `Tutorial6_FishNet.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-06-fishnet-networking.md`
- [ ] Test Host mode
- [ ] Test Client mode (if possible with single instance)
- [ ] Verify network message routing
- [ ] Check MmNetworkFilter.All behavior
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**
- Requires FishNet package installed
- May need two Unity instances for full testing

---

### Task 2.2: Tutorial 7 - Fusion 2 Networking
- [ ] Verify Photon Fusion 2 package is installed (`#if FUSION_WEAVER`)
- [ ] Load `Tutorial7_Fusion2.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-07-fusion2-networking.md`
- [ ] Test Host mode
- [ ] Test Client mode (if possible with single instance)
- [ ] Verify network message routing
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**
- Requires Photon Fusion 2 package installed
- May need Photon account/AppId

---

## Phase 3: Advanced Tutorials (8-11)

### Task 3.1: Tutorial 8 - Switch Nodes & FSM
- [ ] Load `Tutorial8_FSM.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-08-switch-nodes-fsm.md`
- [ ] Test MmRelaySwitchNode state transitions
- [ ] Verify JumpTo() method
- [ ] Verify StartTransitionTo() method
- [ ] Verify only active state receives messages
- [ ] Verify MmSelectedFilter.Selected behavior
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 3.2: Tutorial 9 - Task Management
- [ ] Load `Tutorial9_Tasks.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-09-task-management.md`
- [ ] Test MmTaskManager task sequences
- [ ] Verify task completion callbacks
- [ ] Verify data collection functionality
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 3.3: Tutorial 10 - Application State
- [ ] Load `Tutorial10_AppState.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-10-application-state.md`
- [ ] Test MmAppState state management
- [ ] Verify MmAppStateResponder behavior
- [ ] Verify global state changes propagate
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

### Task 3.4: Tutorial 11 - Advanced Networking
- [ ] Load `Tutorial11_AdvancedNetwork.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-11-advanced-networking.md`
- [ ] Test custom network backend concepts
- [ ] Test binary serialization concepts
- [ ] Verify advanced network scenarios
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**

---

## Phase 4: VR Tutorial (12)

### Task 4.1: Tutorial 12 - VR Experiment
- [ ] Verify XR packages installed (XRI, MetaXR, etc.)
- [ ] Load `Tutorial12_VRExperiment.unity` in Unity Editor
- [ ] Verify scene loads without errors
- [ ] Connect VR headset (Meta Quest, etc.)
- [ ] Enter Play mode
- [ ] Follow wiki steps in `tutorial-12-vr-experiment.md`
- [ ] Test VR controller input handling
- [ ] Test experiment workflow
- [ ] Verify data collection in VR context
- [ ] Check console output matches wiki expectations
- [ ] Document any discrepancies
- [ ] Create GitHub issue if fixes needed
- [ ] Mark validation complete

**Status:** NOT STARTED
**Notes:**
- Requires VR headset for full testing
- May test in XR Simulator if headset unavailable

---

## Summary

| Tutorial | Phase | Status | Issues |
|----------|-------|--------|--------|
| 1. Introduction | 1 | NOT STARTED | |
| 2. Basic Routing | 1 | NOT STARTED | |
| 3. Custom Responders | 1 | NOT STARTED | |
| 4. Custom Messages | 1 | NOT STARTED | |
| 5. Fluent DSL API | 1 | NOT STARTED | |
| 6. FishNet Networking | 2 | NOT STARTED | |
| 7. Fusion 2 Networking | 2 | NOT STARTED | |
| 8. Switch Nodes & FSM | 3 | NOT STARTED | |
| 9. Task Management | 3 | NOT STARTED | |
| 10. Application State | 3 | NOT STARTED | |
| 11. Advanced Networking | 3 | NOT STARTED | |
| 12. VR Experiment | 4 | NOT STARTED | |

**Total:** 0/12 complete

---

*Last Updated: 2025-12-17*
