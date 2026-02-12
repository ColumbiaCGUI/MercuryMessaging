# Distributed Messaging Patterns (MacIntyre-Inspired)

**Status:** Planning
**Priority:** MEDIUM (Research Opportunity + UIST Paper Future Work)
**Estimated Effort:** ~80 hours (2 weeks)
**Target Venues:** UIST 2026 (future work section), CHI, IEEE VR
**Origin:** Analysis of MacIntyre's 1998 dissertation "Exploratory Programming of Distributed Augmented Environments" (Columbia University)

---

## Research Motivation

MacIntyre's Coterie/Repo-3D system (1998) identified fundamental challenges in distributed scene-graph programming that remain unsolved in modern game engines. Cascade's existing fluent DSL and hierarchy-aware routing position it to address several of these challenges through extensions to the messaging API.

**Key insight from MacIntyre:** The distributed 3D graphics library needed three orthogonal capabilities:
1. Multiple distribution semantics (client-server, replicated, local-only)
2. Per-site local modifications to shared state ("local variations")
3. Structured change notification without polling

Cascade already addresses (2) through its graph asymmetry analysis (see `dev/active/asymmetry-analysis/`). This phase addresses (1) and (3), plus additional patterns MacIntyre identified as unsolved.

**Dissertation reference:** MacIntyre, Blair. "Exploratory Programming of Distributed Augmented Environments." PhD Dissertation, Columbia University, 1998. Advisor: Steven K. Feiner.

---

## Phase 1: Distribution Semantics in Fluent API (~25 hours)

### Problem
Cascade's `OverNetwork()` currently has a single mode. MacIntyre identified that distributed applications need three distinct distribution semantics, each optimal for different read/write patterns:
- **Replicated** (high read/write ratio): All peers get updates, reads are local
- **Authoritative** (low read/write ratio): Single owner, remote access via proxy
- **Local-only** (no distribution): Per-peer state, invisible to network

### Implementation

```csharp
// Current: single mode
relay.Send(msg).ToDescendants().OverNetwork().Execute();

// Proposed: explicit distribution semantics
relay.Send(msg).ToDescendants().Replicated().Execute();        // All peers get a copy
relay.Send(msg).ToDescendants().AuthoritativeOn(host).Execute(); // One peer owns truth
relay.Send(msg).ToDescendants().LocalOnly().Execute();          // No network propagation

// Automatic mode selection based on message type attributes
[MessageDistribution(DistributionMode.Authoritative)]
public class RobotSensorData : MmMessage { }

[MessageDistribution(DistributionMode.Replicated)]
public class GameStateUpdate : MmMessage { }
```

### Tasks
1. Define `DistributionMode` enum (Replicated, Authoritative, LocalOnly)
2. Add `Replicated()`, `AuthoritativeOn(peer)`, `LocalOnly()` to fluent API chain
3. Add `[MessageDistribution]` attribute for default per-type distribution
4. Implement authoritative mode in MmNetworkBridge (owner tracking, proxy reads)
5. Implement LocalOnly mode (skip network serialization entirely)
6. Update existing `OverNetwork()` to default to `Replicated()` for backward compatibility
7. Unit tests for all three modes with asymmetric graphs

### HRI Relevance
- Robot sensor data: Authoritative (robot owns its sensor readings)
- Operator commands: Replicated (all peers see the command)
- Local UI state (highlights, selections): LocalOnly

### References
- MacIntyre 1998, Chapter 3: "Three kinds of distribution: Network Objects (client-server), unsynchronized replication, synchronized replication"
- Bal et al. (Orca): Read/write ratio as heuristic for distribution strategy

---

## Phase 2: Structured Change Notification (~20 hours)

### Problem
MacIntyre's Shared Objects provided automatic pre/post callbacks for every update method. Coterie's main usability complaint was that callbacks were **synchronous** (blocking), forcing developers to build their own async queues. Cascade should provide both modes from the start.

### Implementation

```csharp
// Pre/post callbacks with veto capability
relay.OnBeforeReceive<DamageMessage>(msg => {
    if (msg.Amount > maxAllowed) return MessageVerdict.Reject; // Veto!
    return MessageVerdict.Accept;
});

relay.OnAfterReceive<DamageMessage>(msg => {
    UpdateHealthBar(msg);
    PlayDamageEffect(msg);
});

// Catch-all for debugging/visualization
relay.OnAnyMessage((msg, metadata) => {
    Debug.Log($"[{metadata.Sender}] {msg.GetType().Name} via {metadata.RoutingPath}");
});

// Execution mode: sync (for safety validation) vs async (default)
relay.OnBeforeReceive<RobotCommand>(
    msg => ValidateSafety(msg),
    ExecutionMode.Synchronous  // Blocks until validation completes
);

relay.OnAfterReceive<SensorUpdate>(
    msg => UpdateDisplay(msg),
    ExecutionMode.Asynchronous  // Queued, non-blocking (default)
);
```

### Tasks
1. Define `MessageVerdict` enum (Accept, Reject, Defer)
2. Implement `OnBeforeReceive<T>` with veto capability on MmRelayNode
3. Implement `OnAfterReceive<T>` on MmRelayNode
4. Implement `OnAnyMessage` catch-all handler
5. Implement `ExecutionMode.Synchronous` vs `Asynchronous` dispatch
6. Ensure async handlers use Cascade's ObjectPool (zero-allocation)
7. Roslyn analyzer: warn about heavy computation in Synchronous handlers
8. Unit tests for veto, async ordering, mixed sync/async

### HRI Relevance
- Safety-critical: Pre-callbacks can veto unsafe robot commands before execution
- Sensor streams: Async handlers prevent sensor data from blocking the main thread
- Debugging: OnAnyMessage enables live message flow visualization

### References
- MacIntyre 1998, Section 3.2: "Callback Objects execute synchronously... programmers often end up building their own asynchronous event notification queues"
- MacIntyre 1998, Section 7.1: "It would be useful if the system were to support asynchronous notification directly"

---

## Phase 3: Multi-Object Atomic Messaging (~20 hours)

### Problem
MacIntyre identified the inability to apply operations atomically across multiple objects as a fundamental limitation that remained unsolved. In Cascade, a message routed to multiple responders has no atomicity guarantee: some may process it while others fail. For coordinated multi-robot commands or synchronized state transitions, this is a real problem.

### Implementation

```csharp
// Transaction-style message batches
using (var tx = relay.BeginTransaction()) {
    tx.Send(new ArmCommand(targetPos)).To(robotArm).Execute();
    tx.Send(new GripperCommand(open: true)).To(gripper).Execute();
    tx.Commit(); // All-or-nothing: either both execute or neither does
}

// Atomic sub-tree delivery
relay.Send(msg).ToChildren().Atomic().Execute();
// All children receive the message before any begin processing

// Two-phase messaging for coordinated state changes
relay.Send(new PrepareShutdown())
     .ToDescendants()
     .TwoPhase()  // Phase 1: all responders validate, Phase 2: all commit
     .Execute();
```

### Tasks
1. Design transaction API: `BeginTransaction()`, `Commit()`, `Rollback()`
2. Implement message buffering for transaction scope
3. Implement `Atomic()` modifier for guaranteed delivery ordering
4. Implement `TwoPhase()` with prepare/commit protocol
5. Handle failure modes: partial delivery, timeout, responder rejection
6. Network support: distributed transactions across peers (stretch goal)
7. Unit tests for rollback, timeout, mixed success/failure

### HRI Relevance
- Multi-robot coordination: Command multiple robots simultaneously
- Safety interlocks: "Stop all robots" must be atomic
- State transitions: Switching modes (autonomous to teleoperation) must be coordinated

### References
- MacIntyre 1998, Section 7.1.4: "The inability to apply operations atomically across multiple objects... none [of the approaches] is particularly clean, efficient or easy to implement"
- Also relevant: Two-phase commit protocols in distributed databases

---

## Phase 4: Network Topology Awareness (~15 hours)

### Problem
MacIntyre's system treated all network connections uniformly, but noted (Section 7.1.2) that latency varies enormously between local processes and remote machines. Cascade's unified local/network syntax is a strength, but it should optionally expose latency and topology information for applications that need it.

### Implementation

```csharp
// Latency-aware routing
relay.Send(msg)
     .ToDescendants()
     .MaxLatency(50) // Skip peers that can't meet 50ms RTT
     .Execute();

// Priority-based delivery
relay.Send(msg)
     .ToDescendants()
     .Priority(MessagePriority.Critical) // Safety messages skip queue
     .Execute();

// Topology query API
var localPeers = relay.Network.GetPeersWithLatency(maxMs: 20);
var allPeers = relay.Network.GetConnectedPeers();

// Automatic degradation
[NetworkDegradation(Strategy.ReduceFrequency, ThresholdMs = 100)]
public class SensorUpdate : MmMessage { }
// When latency > 100ms, automatically reduce send rate
```

### Tasks
1. Add latency tracking to MmNetworkBridge (RTT measurement per peer)
2. Implement `MaxLatency()` filter in fluent API
3. Implement `Priority()` with message queue prioritization
4. Add `[NetworkDegradation]` attribute for automatic adaptation
5. Expose `relay.Network` topology query API
6. Visual Composer integration: show latency indicators on network connections
7. Unit tests with simulated latency

### HRI Relevance
- Robot control loops have strict latency requirements (< 50ms for teleoperation)
- Safety messages must have priority over sensor updates
- Graceful degradation when network quality drops

### References
- MacIntyre 1998, Section 7.1.2: "Network Awareness" (identified as future work)
- Also relevant: Hierarchical sequencers for locality-aware message ordering (Section 3.1.1)

---

## Cross-References to Existing Phases

| This Phase | Related Phase | Relationship |
|-----------|--------------|-------------|
| Phase 1 (Distribution Semantics) | `asymmetry-analysis/` | Distribution modes determine how asymmetric graphs are handled |
| Phase 2 (Change Notification) | `visual-composer/` | OnAnyMessage enables live message visualization |
| Phase 3 (Atomic Messaging) | `parallel-dispatch/` | Atomicity must work with parallel message dispatch |
| Phase 4 (Network Awareness) | `xr-collaboration/` | Latency-aware routing benefits multi-user XR |
| Phase 2 (Safety Veto) | `user-study/` | Safety callbacks are relevant to HRI study tasks |

---

## Additional Ideas (Not Yet Phased)

These ideas from the dissertation are valuable but lower priority or better suited as enhancements to other phases:

### Read/Write Ratio Diagnostics
Track per-message-type send/receive ratios. Show in Visual Composer as heatmaps. Suggest distribution mode changes. → Enhance `visual-composer/` phase.

### Custom Per-Message Serialization
`[CascadeSerializer(typeof(CompactSerializer))]` on message types. Delta compression for frequently updated state. Skip `[LocalOnly]` fields during network serialization. → Enhance existing binary serialization in MmNetworkBridge.

### Hierarchical Sequencers
Per-subtree message ordering for network scaling. Cluster-based spatial partitioning. → Future research, potentially its own phase if scaling becomes a priority.

### Exploratory Programming Framing
MacIntyre's entire design philosophy. Validates Cascade's approach (fluent API = simple things simple, full filter system = complex things possible). → UIST 2026 paper framing, not a code phase.

---

## Success Metrics

- [ ] Three distribution modes work with existing OverNetwork() tests
- [ ] Pre/post callbacks with veto prevent unsafe message delivery
- [ ] Async handlers show zero GC allocation (ObjectPool integration)
- [ ] Transaction rollback correctly undoes partial delivery
- [ ] Latency-aware routing skips high-latency peers
- [ ] All new API extensions compose with existing fluent chains
- [ ] Backward compatible: existing code works without changes

---

## Dependencies

- MercuryMessaging core framework (MmRelayNode, MmBaseResponder)
- MercuryMessaging Network (MmNetworkBridge, FishNet/Fusion 2)
- Fluent DSL infrastructure (method chaining, filter composition)
- ObjectPool (for zero-allocation async handler queues)

---

*Created: 2026-02-11*
*Last Updated: 2026-02-11*
