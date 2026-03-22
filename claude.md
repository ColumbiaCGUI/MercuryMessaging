# MercuryMessaging Framework Documentation

**MercuryMessaging** is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture.

*Last Updated: 2026-02-11*
*Framework Version: Based on Unity 2021.3+ with VR/XR support*

---

## Wiki Tutorials (Learning Path)

Step-by-step tutorials for learning MercuryMessaging:

**[GitHub Wiki](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki)**

| # | Tutorial | Description |
|---|----------|-------------|
| 1 | Introduction | Getting started, basic concepts |
| 2 | Basic Routing | Message direction, level filters |
| 3 | Custom Responders | Creating message handlers (`MmBaseResponder`) |
| 4 | Custom Messages | Creating message types (`MmMessage`) |
| 5 | **Fluent DSL API** | Modern API with 77% code reduction |
| 6 | **FishNet Networking** | Primary networking solution |
| 7 | Fusion 2 Networking | Alternative networking with Photon |
| 8 | Switch Nodes & FSM | State machines with `MmRelaySwitchNode` |
| 9 | Task Management | User studies and experimental workflows |
| 10 | Application State | Global state with `MmAppState` |
| 11 | Advanced Networking | Custom backends, binary serialization |
| 12 | VR Experiment | Complete VR behavioral experiment |
| 13-14 | (Coming Soon) | Spatial/Temporal Filtering, Performance |

---

## Technical Reference (Documentation/)

@./Documentation/OVERVIEW.md

@./Documentation/ARCHITECTURE.md

@./Documentation/API_REFERENCE.md

@./Documentation/WORKFLOWS.md

@./Documentation/STANDARD_LIBRARY.md

@./Documentation/TESTING.md

@./Documentation/PERFORMANCE.md

---

## Critical References

@./CONTRIBUTING.md

- [Frequent Errors](Documentation/FREQUENT_ERRORS.md) - Common bugs & debugging patterns (MUST READ)
- [DSL API Guide](Documentation/DSL/API_GUIDE.md) - Comprehensive Fluent API documentation
- [FILE_REFERENCE.md](Documentation/FILE_REFERENCE.md) - Complete list of important files with descriptions
- [Source Generators](Documentation/SourceGenerators/README.md) - `[MmGenerateDispatch]` and `[MmHandler]` attributes

---

## Development Resources

- [Documentation/WORKFLOW.md](Documentation/WORKFLOW.md) - Feature development, bug fixes, testing, release workflows
- [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md) - Guidelines for AI assistants
- [.planning/ROADMAP.md](.planning/ROADMAP.md) - UIST 2026 project roadmap (GSD)

---

## Development Standards

**Key Standards:**
- Core framework files must use "Mm" prefix (MmRelayNode, MmMessage, etc.)
- Minimize external dependencies (Unity, System.* only)
- All tests must be fully automated (no manual scenes or prefabs)
- Use Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.)

---

## User Study Design (UIST 2026)

**Location:** `Assets/Research/UserStudy/`
**Scene builder:** Unity Menu > MercuryMessaging > User Study > Build Study Facility Scene
**Study scene:** `Assets/Research/UserStudy/Scenes/StudyFacility.unity`

### 3-Task Design

T1 (Sensor Fan-Out) has been archived. The three active study tasks are:

| Task | Name | Pattern | Design |
|------|------|---------|--------|
| T2 | Safety Zone Alerts | Spatial filtering | Bare scene |
| T3 | Mode-Switch Debugging | Debugging | Pre-wired buggy scene |
| T4 | Alert Aggregation | Many-to-one | Bare scene |

### Bare Scene Design (T2, T4)

All GameObjects, components, and Inspector wiring are pre-built. Participants only
write code in the starter script. They do NOT add components or drag Inspector
references. This isolates the coding API comparison from procedural Unity knowledge.

**T2 Mercury solution (2 lines):**
```csharp
relay.Send("warning").ToDescendants().Within(2f).Execute();
relay.Send("emergency").ToDescendants().Within(1f).Execute();
```

**T4 Mercury solution (1 line):**
```csharp
relay.NotifyValue(alertData);  // travels UP to ancestor CentralDashboard
```

### Pre-Wired Buggy Scene (T3)

The scene is fully wired and the bug is active. Participants enter Play Mode and
observe the symptom (HVAC adjustments fire during Night mode), then find and fix the
bug. The bug is structurally identical in both conditions: `isActive` is set but
never checked in the temperature processing method. The fix is one line: `if (!isActive) return;`

### Starter Script Locations

| Task | Mercury | Events |
|------|---------|--------|
| T2 | `Scripts/Mercury/StudyTasks/T2_SafetyZone/ZoneAlertManager_Starter.cs` | `Scripts/Events/StudyTasks/T2_SafetyZone/ZoneAlertManager_Events_Starter.cs` |
| T3 | `Scripts/Mercury/StudyTasks/T3_ModeSwitch/HvacController_Buggy.cs` | `Scripts/Events/StudyTasks/T3_ModeSwitch/HvacController_Events_Buggy.cs` |
| T4 | `Scripts/Mercury/StudyTasks/T4_AlertAggregation/SubsystemAlerter_Starter.cs` | `Scripts/Events/StudyTasks/T4_AlertAggregation/SubsystemAlerter_Events_Starter.cs` |

Solution scripts are in `Scripts/Solutions/Mercury/` and `Scripts/Solutions/Events/`.

### Correctness Checker

`Scripts/Infrastructure/CorrectnessChecker.cs` — automated scoring in Play Mode.
- T2: Moves worker to 3 test positions, checks indicator state (CLEAR/WARNING/EMERGENCY)
- T3: Switches to Night mode, waits 5 seconds, checks HVAC status unchanged at 18°C
- T4: Waits 10 seconds, checks dashboard log for all 4 subsystem names

### Key Pitfalls (Common Participant Errors)

1. **Message types are concrete, not generic:** Use `MmMessageFloat` not `MmMessage<float>`.
   All `.value` fields are lowercase.

2. **`protected override` required for responders:** Receiver methods in `MmBaseResponder`
   must be `protected override void ReceivedMessage(MmMessageFloat msg)`, not public or
   without the override keyword.

3. **`Within()` measures from relay node position:** `relay.Send().ToDescendants().Within(2f)`
   measures 2m from the relay node's world position, not from a custom Transform. For T2,
   the relay node must be co-located with the worker for correct distance measurement.

4. **Direction matters:** `BroadcastValue` goes DOWN (to descendants); `NotifyValue` goes UP
   (to ancestors). T4 requires `NotifyValue` -- using `BroadcastValue` sends the alert
   in the wrong direction and nothing reaches the dashboard.

5. **`ToDescendants()` vs `ToAll()` in T2:** Indicators are siblings of the relay node
   on the Worker, not descendants. `ToAll()` (bidirectional) or positioning the relay
   node carefully is required. The study solution uses `ToAll().Within(2f)`.

6. **Events condition Inspector wiring:** After writing the SerializeField declaration,
   participants must also drag the reference in the Inspector. Code compiling does not
   mean it is wired. This is a separate manual step.

### Session Timing

87 minutes total; ~52 minutes eye tracking active (well within 90-min limit):
- Consent: 3 min
- Calibration: 3 min
- Condition A: 12 min training + 1 min cal check + 3 tasks × 8 min + 2 min NASA-TLX
- Break: 5 min
- Condition B: 12 min training + 1 min cal check + 3 tasks × 8 min + 2 min NASA-TLX
- SUS + Demographics: 3 min

### Power Analysis

Target N=16 (pilot N=4 first). N=16 provides ~80% power at d=0.80 and ~91% at d=1.00.
N=12 is borderline. Run pilot to estimate effect sizes before committing to full recruitment.

---

## Guidelines for AI Assistants

For AI assistants working on this project, see [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md).

**Critical Policy:**
- Use Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.)
- Use `.planning/` (GSD) for project planning and phase execution
- See [Documentation/WORKFLOW.md](Documentation/WORKFLOW.md) for development workflow
