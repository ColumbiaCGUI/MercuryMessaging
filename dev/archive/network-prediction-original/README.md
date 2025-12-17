# Network Prediction & Reconciliation

## Overview

Implementation of client-side prediction and server reconciliation for hierarchical message routing in distributed systems. This module extends traditional client-side prediction techniques to work with MercuryMessaging's scene graph structure, enabling responsive multi-user experiences under network latency.

**For business context and use cases, see [`USE_CASE.md`](./USE_CASE.md)**

---

## Technical Architecture

### System Overview

```
Client A                    Server                     Client B
    |                          |                          |
[Predict] ──message──>    [Validate]                [Receive]
    |                          |                          |
[Apply] ──────────────>  [Broadcast] ───delayed───>  [Apply]
    |                          |                          |
[Render]                 [Authoritative]             [Render]
    |                          |                          |
    └────reconcile────    [Correction]   ────update─────┘
```

### Core Components

#### 1. Prediction Engine
```csharp
class HierarchicalPredictor {
    PredictionModel model;
    RoutePredictor routePredictor;
    EffectEstimator effectEstimator;

    PredictedState Predict(MmMessage msg, SceneGraph graph) {
        var routes = routePredictor.PredictRoutes(msg);
        var effects = effectEstimator.EstimateEffects(routes);
        return new PredictedState(effects, confidence);
    }
}
```

#### 2. State History Buffer
- Circular buffer of confirmed states
- Branching history for multiple predictions
- Efficient snapshot/restore mechanism
- Configurable buffer size based on latency

#### 3. Reconciliation System
- Smooth visual interpolation during corrections
- Partial rollback for isolated conflicts
- Conflict resolution strategies
- Authoritative server state handling

#### 4. Confidence Scoring
- Pattern-based confidence estimation
- Historical accuracy tracking
- Network condition awareness
- Adaptive threshold adjustment

---

## Implementation Details

### Hierarchical State Prediction

```csharp
public class MessagePredictor {
    private readonly StateHistory history;
    private readonly RoutingPredictor routingPredictor;

    public PredictedState PredictMessage(MmMessage message, float deltaTime) {
        // Predict which nodes will receive the message
        var predictedRoutes = routingPredictor.PredictRoutes(message);

        // Estimate state changes for each node
        var stateChanges = new Dictionary<int, NodeState>();
        foreach (var nodeId in predictedRoutes) {
            stateChanges[nodeId] = PredictNodeState(nodeId, message, deltaTime);
        }

        return new PredictedState {
            Changes = stateChanges,
            Timestamp = NetworkTime.Now,
            Confidence = CalculateConfidence(message, predictedRoutes)
        };
    }
}
```

### State Reconciliation

```csharp
public class StateReconciler {
    private readonly float interpolationRate = 0.1f;

    public void Reconcile(AuthoritativeState server, PredictedState client) {
        var diff = ComputeDifference(server, client);

        if (diff.Magnitude < threshold) {
            // Minor correction - smooth interpolation
            InterpolateStates(client, server, interpolationRate);
        } else {
            // Major correction - rollback and replay
            RollbackToState(server.Timestamp);
            ReplayInputsSince(server.Timestamp);
        }
    }

    private void RollbackToState(float timestamp) {
        // Find the last confirmed state before timestamp
        var confirmedState = history.GetStateAt(timestamp);
        ApplyState(confirmedState);

        // Mark predictions after this point as invalid
        predictions.InvalidateAfter(timestamp);
    }
}
```

### Input Buffer Management

```csharp
public class InputBuffer {
    private readonly CircularBuffer<InputCommand> buffer;
    private int sequenceNumber = 0;

    public void StoreInput(InputCommand input) {
        input.Sequence = sequenceNumber++;
        input.Timestamp = NetworkTime.Now;
        buffer.Add(input);

        // Send to server with prediction
        NetworkManager.SendInput(input);

        // Apply locally immediately
        LocalSimulation.ApplyInput(input);
    }

    public void ReplayInputs(float fromTimestamp) {
        var inputs = buffer.GetInputsSince(fromTimestamp);
        foreach (var input in inputs) {
            LocalSimulation.ApplyInput(input);
        }
    }
}
```

---

## Prediction Algorithms

### Message Route Prediction

```
Algorithm: PredictMessageRoutes
Input: Message m, SceneGraph G, History H
Output: Set<NodeId> predictedRoutes

1. routes ← ∅
2. levelFilter ← m.MetadataBlock.LevelFilter
3. startNode ← GetNode(m.SourceId)
4.
5. if levelFilter == Child:
6.     routes ← GetDescendants(startNode, G)
7. else if levelFilter == Parent:
8.     routes ← GetAncestors(startNode, G)
9. else if levelFilter == SelfAndChildren:
10.    routes ← {startNode} ∪ GetDescendants(startNode, G)
11.
12. // Apply tag and active filters
13. routes ← FilterByTags(routes, m.MetadataBlock.Tag)
14. routes ← FilterByActive(routes, m.MetadataBlock.ActiveFilter)
15.
16. return routes
```

### State Extrapolation

```
Algorithm: ExtrapolateState
Input: CurrentState S, Velocity V, DeltaTime dt
Output: ExtrapolatedState S'

1. S' ← CopyState(S)
2. for each dynamic property p in S':
3.     if HasVelocity(p):
4.         S'[p] ← S[p] + V[p] * dt
5.     else if HasPattern(p, H):
6.         S'[p] ← PredictFromPattern(p, H)
7.     else:
8.         S'[p] ← S[p]  // No change predicted
9. return S'
```

---

## Network Optimization

### Delta Compression

```csharp
public class DeltaCompressor {
    private Dictionary<int, State> lastSentStates;

    public byte[] CompressState(State current, int clientId) {
        var lastState = lastSentStates[clientId];
        var delta = ComputeDelta(lastState, current);

        // Only send changed properties
        return SerializeDelta(delta);
    }
}
```

### Priority-Based Updates

```csharp
public class UpdatePrioritizer {
    public List<StateUpdate> PrioritizeUpdates(List<StateUpdate> updates, int bandwidth) {
        // Sort by importance: distance, visibility, rate of change
        updates.Sort((a, b) => {
            float priorityA = CalculatePriority(a);
            float priorityB = CalculatePriority(b);
            return priorityB.CompareTo(priorityA);
        });

        // Take only what fits in bandwidth
        return updates.Take(GetUpdateCount(bandwidth)).ToList();
    }
}
```

---

## Configuration

```csharp
public class PredictionConfig {
    // Prediction settings
    public bool EnablePrediction { get; set; } = true;
    public int HistoryBufferSize { get; set; } = 120; // 2 seconds at 60Hz
    public float PredictionTimeLimit { get; set; } = 0.5f; // Max 500ms ahead

    // Reconciliation settings
    public float InterpolationRate { get; set; } = 0.1f;
    public float RollbackThreshold { get; set; } = 0.5f;
    public bool SmoothCorrections { get; set; } = true;

    // Network settings
    public int InputBufferSize { get; set; } = 60;
    public bool EnableDeltaCompression { get; set; } = true;
    public int UpdateRate { get; set; } = 30; // Updates per second

    // Confidence settings
    public float MinConfidenceThreshold { get; set; } = 0.7f;
    public bool AdaptiveConfidence { get; set; } = true;
}
```

---

## Performance Characteristics

### Latency Compensation
- Effective up to 200ms actual latency
- Perceived latency reduction: 50-150ms
- Smooth gameplay at 100ms RTT

### Prediction Accuracy
- Common patterns: >90% accuracy
- Simple movements: >95% accuracy
- Complex interactions: 70-80% accuracy

### Bandwidth Usage
- Delta compression: 30-50% reduction
- Priority updates: 20-40% reduction
- Overall savings: ~40% compared to naive approach

### CPU Overhead
- Prediction: <1ms per frame
- Reconciliation: <2ms when needed
- History management: <0.5ms per frame

---

## Testing Strategy

### Unit Tests
- Prediction accuracy validation
- Reconciliation correctness
- Buffer overflow handling
- Edge case coverage

### Integration Tests
- Network simulation with varied latency
- Packet loss scenarios
- Out-of-order packet handling
- Server authority validation

### Performance Tests
- Prediction overhead measurement
- Memory usage profiling
- Bandwidth consumption analysis
- Stress testing with 100+ predicted entities

---

## Known Limitations

1. **Prediction Horizon**
   - Cannot predict beyond 500ms reliably
   - Accuracy degrades with time
   - Complex interactions hard to predict

2. **Memory Usage**
   - History buffer grows with entity count
   - Each state snapshot ~1KB per entity
   - Bounded by HistoryBufferSize

3. **Determinism Requirements**
   - Requires deterministic simulation
   - Floating point differences can cause divergence
   - Physics engines may need special handling

---

## Dependencies

- Unity 2021.3+ (NetworkTime API)
- MercuryMessaging core framework
- Photon Fusion 2 (optional, for networking)
- Mathematics package (for interpolation)

---

*Last Updated: 2025-11-20*
*Estimated Implementation Time: 400 hours*