# Message Replay / Time Travel Debugging

**Status:** Planning
**Priority:** MEDIUM-HIGH (Research Opportunity)
**Estimated Effort:** ~150 hours (4 weeks)
**Target Venues:** UIST, ICSE
**Novelty Assessment:** MEDIUM-HIGH

---

## Research Contribution

### Problem Statement

Debugging message routing issues in hierarchical systems is challenging because:
- Messages propagate asynchronously through multiple hierarchy levels
- Filter decisions happen at each hop (Level, Active, Selected, Tag, Network)
- Developers cannot see "why" a message didn't reach a specific responder
- Traditional breakpoint debugging interrupts message flow timing
- No way to replay specific message sequences for reproducibility

### Novel Technical Approach

We propose **Message-Centric Time Travel Debugging** — a replay system that:
1. Records all MmInvoke calls with full metadata and routing decisions
2. Provides a timeline scrubber to step through message history
3. Visualizes message propagation with filter decisions at each hop
4. Answers "Why didn't this message reach that responder?"

**Key Differentiation from Existing Work:**
- Existing omniscient debuggers (JIVE, Dbux) are **code-centric** (step through code)
- Mercury Time Travel is **message-centric** (step through message flow)
- Focus on filter decisions, not code execution

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| JIVE | 2020 | SPE | Java FSM extraction from traces | Java-only, post-hoc FSM extraction | Real-time Unity message replay |
| didiffff | 2022 | ICPC | Execution trace comparison | Code execution focus, not messages | Message flow focus |
| Dbux-PDG | 2022 | VISSOFT | JavaScript event debugging | Web/JavaScript only, not game engines | Unity/C# game engine |
| RR Debugger | 2021 | Various | Record/replay for Linux | System-level, not application-aware | Message-level granularity |
| Unity Profiler | 2023 | Unity | Performance profiling | No message routing visibility | Message filter decisions |

### Literature Gap Analysis

**What exists:**
- Omniscient debugging for Java/JavaScript (JIVE, Dbux-PDG) - code-centric
- Execution trace comparison tools (didiffff) - line-level, not message-level
- System-level record/replay (RR) - too low-level for application debugging
- Unity Profiler - performance focus, no message routing

**What doesn't exist:**
- **Message-centric** time travel (replay message flows, not code execution)
- **Filter decision visualization** at each routing hop
- **"Why didn't this message arrive?"** debugging queries
- **Unity-integrated** message replay for hierarchical systems

### Novelty Claims

1. **FIRST** message-centric time travel debugging for game engine hierarchies
2. **FIRST** filter decision visualization showing "why message stopped"
3. **FIRST** query-based debugging ("why didn't message reach X?")
4. **Novel** integration with Unity Editor for message replay
5. **Differentiated** from code-centric omniscient debuggers

### Key Citations

```bibtex
@article{jive2020,
  title={JIVE: Java Interactive Visualization Environment for Program Comprehension},
  journal={Software: Practice and Experience (SPE)},
  year={2020}
}

@inproceedings{didiffff2022,
  title={didiffff: Execution Trace Differencing for Debugging},
  booktitle={IEEE International Conference on Program Comprehension (ICPC)},
  year={2022}
}

@inproceedings{dbuxpdg2022,
  title={Dbux: Practical Program Dependence Graph Visualization for JavaScript},
  booktitle={IEEE Working Conference on Software Visualization (VISSOFT)},
  year={2022}
}
```

---

## Technical Architecture

### Message Recording System

```csharp
public class MmMessageRecorder : MonoBehaviour {
    public static List<RecordedMessage> Timeline { get; }

    public struct RecordedMessage {
        public int FrameNumber;
        public float Timestamp;
        public MmRelayNode Source;
        public MmMessage Message;
        public List<RoutingDecision> Decisions;
        public List<MmResponder> Reached;
        public List<RejectedResponder> Rejected;
    }

    public struct RejectedResponder {
        public MmResponder Responder;
        public string RejectionReason; // "Tag mismatch", "Inactive", etc.
    }
}
```

### Timeline Scrubber UI

```
[Frame 0]----[Frame 100]----[Frame 200]----[Frame 300]
    ↑
    └── Current: Frame 142
        Message: MmMethod.Initialize
        Source: GameManager
        Reached: 5 responders
        Rejected: 2 responders
            - Player/HUD (Tag mismatch: expected Tag0, found Tag1)
            - Enemy/Spawner (Inactive)
```

### Query Interface

```csharp
// Query: "Why didn't MmMethod.Initialize reach EnemySpawner?"
var query = Recorder.Query()
    .Method(MmMethod.Initialize)
    .Target("EnemySpawner")
    .InFrameRange(100, 200);

var result = query.Execute();
// Result: "Rejected at frame 142 by Active filter (GameObject inactive)"
```

---

## Implementation Plan

### Phase 1: Recording Infrastructure (40 hours)
- Hook into MmRelayNode.MmInvoke for recording
- Capture routing decisions at each hop
- Store rejection reasons for each filter
- Implement circular buffer for bounded memory

### Phase 2: Timeline UI (50 hours)
- Unity Editor window for timeline visualization
- Frame-by-frame message scrubber
- Message detail panel with routing info
- Visual indicators for rejected responders

### Phase 3: Query System (40 hours)
- Query builder for debugging questions
- "Why didn't message reach X?" analysis
- Filter by method, target, time range
- Export query results for reporting

### Phase 4: Testing & Documentation (20 hours)
- Automated tests for recording accuracy
- Performance benchmarks (<5% overhead)
- User documentation and tutorials
- Example debugging scenarios

---

## Evaluation Methodology

### Performance Benchmarks
- Recording overhead: <5% frame time
- Memory usage: <50MB for 10,000 messages
- Query response: <100ms for typical queries
- Playback: 60fps timeline scrubbing

### User Study Design
- **Participants:** N=12 Unity developers
- **Task:** Debug 5 message routing failures
- **Conditions:**
  - Control: Standard Unity debugging (breakpoints, logs)
  - Treatment: Mercury Time Travel Debugging
- **Metrics:**
  - Time to identify root cause
  - Number of debugging attempts
  - Confidence in diagnosis

### Expected Results
- **40% reduction** in debugging time
- **Higher confidence** in root cause identification
- **Preference** for message-centric approach

---

## Success Metrics

- [ ] Recording captures 100% of MmInvoke calls
- [ ] Rejection reasons accurate for all filter types
- [ ] Timeline UI responsive at 60fps
- [ ] Query system returns results in <100ms
- [ ] User study shows 40% debugging time reduction
- [ ] <5% performance overhead when enabled
- [ ] Integration with Visual Composer (future)

---

## Dependencies

- MercuryMessaging framework
- Unity Editor UI Toolkit
- Unity 2021.3+ LTS

---

## Related Files

- [time-travel-context.md](time-travel-context.md) - Technical design details
- [time-travel-tasks.md](time-travel-tasks.md) - Implementation checklist

---

*Created: 2025-12-17*
*Last Updated: 2025-12-17*
