# Tutorial Validation - Technical Context

This document provides technical context for validating MercuryMessaging tutorials.

---

## Scene and Wiki Mapping

| # | Scene File | Wiki Draft | Local README |
|---|------------|------------|--------------|
| 1 | `Tutorial1_Base.unity` | `tutorial-01-introduction.md` | None |
| 2 | `Tutorial2_Base.unity` | `tutorial-02-basic-routing.md` | None |
| 3 | `Tutorial3_Base.unity` | `tutorial-03-custom-responders.md` | None |
| 4 | `Tutorial4_Base.unity` | `tutorial-04-custom-messages.md` | None |
| 5 | `Tutorial5_Base.unity` | `tutorial-05-fluent-dsl-api.md` | None |
| 6 | `Tutorial6_FishNet.unity` | `tutorial-06-fishnet-networking.md` | Has README |
| 7 | `Tutorial7_Fusion2.unity` | `tutorial-07-fusion2-networking.md` | Has README |
| 8 | `Tutorial8_FSM.unity` | `tutorial-08-switch-nodes-fsm.md` | Has README |
| 9 | `Tutorial9_Tasks.unity` | `tutorial-09-task-management.md` | Has README |
| 10 | `Tutorial10_AppState.unity` | `tutorial-10-application-state.md` | Has README |
| 11 | `Tutorial11_AdvancedNetwork.unity` | `tutorial-11-advanced-networking.md` | Has README |
| 12 | `Tutorial12_VRExperiment.unity` | `tutorial-12-vr-experiment.md` | Has README |

---

## File Paths

### Scene Location
```
Assets/Framework/MercuryMessaging/Examples/Tutorials/
├── Tutorial1_Base.unity
├── Tutorial2_Base.unity
├── Tutorial3_Base.unity
├── Tutorial4_Base.unity
├── Tutorial5_Base.unity
├── Tutorial6_FishNet.unity
├── Tutorial7_Fusion2.unity
├── Tutorial8_FSM.unity
├── Tutorial9_Tasks.unity
├── Tutorial10_AppState.unity
├── Tutorial11_AdvancedNetwork.unity
└── Tutorial12_VRExperiment.unity
```

### Wiki Draft Location
```
dev/wiki-drafts/tutorials/
├── tutorial-01-introduction.md
├── tutorial-02-basic-routing.md
├── tutorial-03-custom-responders.md
├── tutorial-04-custom-messages.md
├── tutorial-05-fluent-dsl-api.md
├── tutorial-06-fishnet-networking.md
├── tutorial-07-fusion2-networking.md
├── tutorial-08-switch-nodes-fsm.md
├── tutorial-09-task-management.md
├── tutorial-10-application-state.md
├── tutorial-11-advanced-networking.md
└── tutorial-12-vr-experiment.md
```

---

## Tutorial Content Summary

### Tutorial 1: Introduction
- **Concepts:** MmRelayNode, MmBaseResponder, basic message sending
- **Expected Controls:** None (automatic demo)
- **Key Verification:** Messages flow from relay to responder

### Tutorial 2: Basic Routing
- **Concepts:** MmLevelFilter, parent/child routing, MmMetadataBlock
- **Expected Controls:** Keyboard input for triggering messages
- **Key Verification:** Messages reach correct hierarchy levels

### Tutorial 3: Custom Responders
- **Concepts:** Creating MmBaseResponder subclasses, override methods
- **Expected Controls:** Various inputs triggering different message types
- **Key Verification:** ReceivedMessage handlers called correctly

### Tutorial 4: Custom Messages
- **Concepts:** Creating MmMessage subclasses, serialization
- **Expected Controls:** Custom message triggers
- **Key Verification:** Custom data transmitted correctly

### Tutorial 5: Fluent DSL API
- **Concepts:** Send(), BroadcastInitialize(), ToChildren(), Execute()
- **Expected Controls:** Keyboard shortcuts for DSL methods
- **Key Verification:** DSL produces same results as traditional API

### Tutorial 6: FishNet Networking
- **Concepts:** FishNet integration, network message routing
- **Expected Controls:** Host/Join buttons
- **Key Verification:** Messages sync across network
- **Dependencies:** FishNet package installed

### Tutorial 7: Fusion 2 Networking
- **Concepts:** Photon Fusion 2 integration
- **Expected Controls:** Host/Join buttons
- **Key Verification:** Messages sync across network
- **Dependencies:** Photon Fusion 2 package installed

### Tutorial 8: Switch Nodes & FSM
- **Concepts:** MmRelaySwitchNode, FiniteStateMachine, state transitions
- **Expected Controls:** State change buttons/keys
- **Key Verification:** FSM transitions work, only active state receives messages

### Tutorial 9: Task Management
- **Concepts:** MmTaskManager, task sequences, data collection
- **Expected Controls:** Task progression buttons
- **Key Verification:** Tasks complete in order, data recorded

### Tutorial 10: Application State
- **Concepts:** MmAppState, global state management
- **Expected Controls:** State manipulation inputs
- **Key Verification:** AppState changes propagate correctly

### Tutorial 11: Advanced Networking
- **Concepts:** Custom network backends, binary serialization
- **Expected Controls:** Network simulation controls
- **Key Verification:** Complex network scenarios work

### Tutorial 12: VR Experiment
- **Concepts:** Complete VR behavioral experiment
- **Expected Controls:** VR controllers
- **Key Verification:** Full experiment workflow
- **Dependencies:** VR headset, XR packages

---

## Validation Checklist Template

For each tutorial, verify:

```markdown
### Tutorial N: [Name]

**Scene Load:** [ ] Pass / [ ] Fail
**Play Mode:** [ ] Pass / [ ] Fail
**Wiki Steps Match:** [ ] Pass / [ ] Fail
**Keyboard Controls:** [ ] Pass / [ ] Fail
**Console Output:** [ ] Pass / [ ] Fail
**Code Examples:** [ ] Pass / [ ] Fail

**Notes:**
- [Any observations]

**Issues Found:**
- [ ] Issue description (GitHub issue #)
```

---

## Known Dependencies

| Tutorial | Required Package | Conditional Compilation |
|----------|------------------|------------------------|
| 6 | FishNet | `#if FISH_NET` |
| 7 | Photon Fusion 2 | `#if FUSION_WEAVER` |
| 12 | XR Interaction Toolkit | Various XR defines |

---

## Common Validation Issues

1. **Missing Package:** Tutorial scene won't load if optional package not installed
2. **Keyboard Mapping:** Wiki may document different keys than scene uses
3. **Console Spam:** Excessive debug logging obscures expected output
4. **Outdated Screenshots:** Wiki images may not match current UI
5. **Code Drift:** Wiki code examples may not match actual scene scripts

---

*Last Updated: 2025-12-17*
