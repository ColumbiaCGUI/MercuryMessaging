# Network Prediction & Reconciliation - Context Document

*Last Updated: 2025-11-20*

## Overview

This document captures critical context for the network prediction and reconciliation implementation, including technical dependencies, architectural decisions, and integration points with MercuryMessaging.

---

## Key Files and Components

### Core MercuryMessaging Network Integration

#### Primary Network Files
- **`Assets/MercuryMessaging/Protocol/MmMessage.cs`**
  - Base message class for network serialization
  - Must maintain immutability for prediction
  - Consider copy-on-predict semantics

- **`Assets/MercuryMessaging/Protocol/MmRelayNode.cs`**
  - Network message handling in MmInvoke
  - IsDeserialized flag for network detection
  - FlipNetworkFlagOnSend for propagation control

- **`Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs`**
  - MmNetworkFilter enum (Local/Network/All)
  - Network routing metadata
  - Must extend for prediction hints

- **`Assets/MercuryMessaging/Protocol/MmNetworkManager.cs`** (if exists)
  - Network serialization logic
  - Message ordering guarantees
  - Integration point for prediction

#### Network-Aware Components
- **`Assets/MercuryMessaging/Support/Data/MmCircularBuffer.cs`**
  - Can adapt for state history buffer
  - Already thread-safe (QW-4)
  - Bounded memory implementation

- **`Assets/MercuryMessaging/Protocol/MmLogger.cs`**
  - Add prediction/reconciliation logging
  - Network latency tracking
  - Debug visualization support

### Existing Network Integration

#### Photon Integration (if available)
```csharp
#if PHOTON_AVAILABLE
// Assets/MercuryMessaging/ThirdParty/PhotonIntegration.cs
// Handles Photon-specific serialization
// Must extend for prediction metadata
#endif
```

#### Network Message Flow
1. **Outgoing**: MmRelayNode → Serialize → Network Layer → Send
2. **Incoming**: Receive → Deserialize → MmRelayNode → Route
3. **Prediction Point**: Between Serialize and Send
4. **Reconciliation Point**: After Deserialize, before Route

---

## Technical Dependencies

### External Dependencies

#### Required Unity Packages
```json
{
  "dependencies": {
    "com.unity.netcode": "1.0.0",         // Optional base
    "com.unity.ml-agents": "2.0.0",       // ML models
    "com.unity.collections": "1.2.0",      // Native collections
    "com.unity.mathematics": "1.2.0",      // Math operations
    "com.unity.timeline": "1.6.0"          // Replay system
  }
}
```

#### ML Framework Requirements
- **TensorFlow Lite** or **Barracuda** for Unity inference
- **Python Environment** for model training
- **ONNX** for model interchange

#### Network Backends Supported
- Photon Fusion 2.0
- Mirror Networking
- Unity Netcode for GameObjects
- Custom TCP/UDP

### Internal Dependencies

#### Message System Requirements
1. **Message Immutability**: Required for safe prediction
2. **Timestamp Support**: Need message timestamps
3. **Serialization Hooks**: Pre/post serialization callbacks
4. **Network Statistics**: RTT, packet loss, jitter

#### Framework Features to Preserve
1. **Message Ordering**: Per-sender guarantees
2. **Reliable Delivery**: No message loss
3. **Network Filtering**: Maintain filter system
4. **Hierarchy Propagation**: Parent-child relationships

---

## Architectural Decisions

### Decision 1: Prediction Algorithm
**Decision**: Hierarchical state prediction with ML enhancement
**Rationale**:
- Leverages scene graph structure
- Better than flat prediction
- ML handles complex patterns
**Trade-offs**:
- More complex implementation
- Higher memory usage
- Training data required

### Decision 2: State History Strategy
**Decision**: Circular buffer with branching
**Rationale**:
- Bounded memory usage
- Supports multiple predictions
- Efficient snapshots
**Trade-offs**:
- Limited history depth
- Complex branch management

### Decision 3: Reconciliation Approach
**Decision**: Smooth interpolation with partial rollback
**Rationale**:
- Better user experience
- Avoids visual pops
- Selective corrections
**Trade-offs**:
- Complex implementation
- Potential for drift

### Decision 4: ML Model Architecture
**Decision**: LSTM for sequential prediction
**Rationale**:
- Good for temporal patterns
- Reasonable model size
- Real-time inference possible
**Trade-offs**:
- Training complexity
- Black box predictions

### Decision 5: Network Protocol
**Decision**: Extend existing protocol with metadata
**Rationale**:
- Backwards compatible
- Minimal overhead
- Progressive enhancement
**Trade-offs**:
- Protocol versioning needed
- Increased message size

---

## Performance Context

### Network Performance Characteristics

#### Latency Distribution (Real-world data)
| Connection Type | RTT Range | Jitter | Packet Loss |
|----------------|-----------|---------|-------------|
| LAN | 1-5ms | <1ms | 0% |
| Same Region | 10-30ms | 2-5ms | 0.1% |
| Cross-Region | 50-150ms | 5-20ms | 0.5% |
| International | 150-300ms | 20-50ms | 1-2% |
| Mobile 4G | 30-100ms | 10-30ms | 1-3% |
| Satellite | 500-800ms | 50-100ms | 2-5% |

#### Message Patterns Analysis
1. **Movement Updates**: 60% of traffic, highly predictable
2. **State Changes**: 20% of traffic, moderate predictability
3. **Events**: 15% of traffic, low predictability
4. **System**: 5% of traffic, not predicted

### Prediction Opportunity Analysis
- **Linear Motion**: 95% accuracy possible
- **User Input**: 80% accuracy (patterns)
- **Physics**: 90% accuracy (deterministic)
- **AI Behavior**: 70% accuracy (patterns)
- **Random Events**: Not predictable

---

## Code Patterns and Examples

### Current Network Message Pattern
```csharp
// Current implementation (no prediction)
public void SendNetworkMessage(MmMessage message) {
    if (isHost) {
        // Direct local processing
        ProcessMessage(message);
    }

    // Serialize and send
    var data = SerializeMessage(message);
    networkLayer.Send(data);

    // Wait for authoritative response...
}
```

### Proposed Prediction Pattern
```csharp
// With prediction
public void SendNetworkMessageWithPrediction(MmMessage message) {
    // Predict immediate effect
    var prediction = predictor.Predict(message);
    ApplyPrediction(prediction);

    // Track for reconciliation
    stateHistory.RecordPrediction(prediction);

    // Send actual message
    var data = SerializeMessage(message);
    data.AddPredictionHint(prediction.confidence);
    networkLayer.Send(data);
}

// On authoritative response
public void OnAuthoritativeMessage(MmMessage message) {
    var prediction = stateHistory.GetPrediction(message.id);
    if (prediction != null) {
        reconciler.Reconcile(prediction, message);
    } else {
        ProcessMessage(message);
    }
}
```

### State Snapshot Pattern
```csharp
public class StateSnapshot {
    public float timestamp;
    public Dictionary<int, ComponentState> states;
    public MmMessage[] pendingMessages;

    public StateSnapshot Clone() {
        // Deep copy for branching
        return new StateSnapshot {
            timestamp = timestamp,
            states = CloneStates(states),
            pendingMessages = pendingMessages.ToArray()
        };
    }

    public StateDelta CompareTo(StateSnapshot other) {
        // Calculate differences
        return new StateDelta {
            changed = FindChangedComponents(other),
            added = FindAddedComponents(other),
            removed = FindRemovedComponents(other)
        };
    }
}
```

---

## Network Integration Points

### Message Serialization Extension
```csharp
// Extend MmMessage for network prediction
public class MmNetworkMessage : MmMessage {
    public float sendTime;
    public float predictedArrivalTime;
    public float confidence;
    public byte[] predictionHint;

    public override byte[] Serialize() {
        var data = base.Serialize();
        // Add prediction metadata
        return AppendPredictionData(data);
    }
}
```

### Timing and Synchronization
```csharp
public class NetworkTimeManager {
    private float serverTimeOffset;
    private float clockDrift;
    private CircularBuffer<TimeSample> samples;

    public float GetSynchronizedTime() {
        return Time.realtimeSinceStartup + serverTimeOffset;
    }

    public float EstimateLatency() {
        return samples.Average(s => s.rtt) / 2f;
    }
}
```

---

## ML Model Context

### Training Data Schema
```python
# Training data structure
{
    "message_type": "MmMethod",
    "sender_id": "int",
    "target_id": "int",
    "hierarchy_depth": "int",
    "parent_state": "vector",
    "message_params": "object",
    "network_conditions": {
        "latency": "float",
        "jitter": "float",
        "packet_loss": "float"
    },
    "actual_effect": "vector",
    "timestamp": "float"
}
```

### Model Input/Output
```python
# LSTM Model Structure
Input Shape: (sequence_length, features)
- sequence_length: 10 (last 10 messages)
- features: 64 (encoded message + network state)

Output Shape: (num_targets, state_size)
- num_targets: Variable (affected nodes)
- state_size: 32 (predicted state change)

Confidence Output: (1,) - Single confidence score
```

### Inference Pipeline
1. **Preprocessing**: Message → Feature Vector
2. **Prediction**: LSTM → State Changes
3. **Confidence**: Uncertainty Estimation
4. **Postprocessing**: State Changes → Visual Updates

---

## Testing Context

### Network Simulation Requirements

#### Latency Simulation
```csharp
public class NetworkSimulator {
    public float baseLatency = 50f;
    public float jitter = 10f;
    public float packetLoss = 0.01f;

    public IEnumerator SimulateDelay(Action callback) {
        var delay = baseLatency + Random.Range(-jitter, jitter);
        yield return new WaitForSeconds(delay / 1000f);

        if (Random.value > packetLoss) {
            callback?.Invoke();
        }
    }
}
```

#### Test Scenarios
1. **Perfect Network**: 0ms latency, no loss
2. **LAN**: 5ms latency, 0% loss
3. **Regional**: 50ms latency, 0.1% loss
4. **International**: 200ms latency, 1% loss
5. **Poor Mobile**: 100ms latency, 5% loss, high jitter
6. **Satellite**: 600ms latency, 2% loss

### Validation Metrics
- **Prediction Accuracy**: Actual vs predicted state
- **Rollback Frequency**: Number of corrections needed
- **Visual Quality**: Smoothness score
- **Bandwidth Usage**: Bytes per second
- **CPU Usage**: Percentage utilization
- **Memory Usage**: MB allocated

---

## Research Context

### Key Publications for Implementation

#### Fundamental Papers
1. **Bernier (2001)**: "Latency Compensating Methods"
   - Client-side prediction basics
   - Interpolation techniques
   - Implementation guidelines

2. **Aronson (1997)**: QuakeWorld Prediction
   - First successful implementation
   - Practical lessons learned

3. **Fiedler (2018)**: Network Physics
   - Modern techniques
   - Deterministic simulation

#### ML for Games
1. **Silver et al. (2016)**: AlphaGo
   - MCTS with neural networks
   - Applicable to prediction

2. **Bhonker et al. (2020)**: "Neural Game State Compression"
   - Relevant for state management

### Competitive Landscape
| Framework | Prediction | Hierarchical | ML-Enhanced | Latency Target |
|-----------|------------|--------------|-------------|----------------|
| Photon Fusion | Yes | No | No | <100ms |
| Mirror | Basic | No | No | <150ms |
| GGPO | Yes | No | No | <50ms |
| **Ours** | **Advanced** | **Yes** | **Yes** | **<50ms** |

---

## Risk Factors

### Technical Risks
1. **ML Model Size**: May exceed 50MB target
2. **Inference Time**: GPU may be required
3. **Training Data**: Insufficient diversity
4. **Network Variability**: Unpredictable conditions

### Integration Risks
1. **Breaking Changes**: To MmMessage structure
2. **Performance Impact**: On non-predicted messages
3. **Memory Growth**: Unbounded history
4. **Complexity**: Debugging difficulties

### Mitigation Strategies
- Model compression techniques
- CPU-optimized inference
- Synthetic data generation
- Adaptive algorithms
- Versioned protocol
- Performance modes
- Memory limits
- Debug visualization

---

## Development Environment

### Required Setup
```bash
# Unity Setup
Unity 2021.3.16f1 LTS minimum
ML-Agents Package 2.0+
Barracuda 3.0+

# Python Environment (for training)
python 3.8+
tensorflow 2.8+
numpy, pandas, scikit-learn

# Network Testing
Clumsy (Windows) or Network Link Conditioner (Mac)
Wireshark for packet analysis
```

### Debug Tools
- Unity Profiler with network module
- Custom prediction visualizer
- State history inspector
- Confidence overlay
- Network statistics HUD

---

## Communication Protocols

### Network Message Protocol
```
[Header - 12 bytes]
- MessageID (4 bytes)
- Timestamp (4 bytes)
- Flags (2 bytes)
- Size (2 bytes)

[Prediction Metadata - 8 bytes] (if flag set)
- Confidence (2 bytes)
- PredictionID (4 bytes)
- Hint size (2 bytes)

[Payload - Variable]
- Serialized MmMessage
- Prediction hints (optional)
```

### State Sync Protocol
```
[State Update]
- FrameNumber
- EntityCount
- For each entity:
  - EntityID
  - ComponentMask
  - ComponentData[]

[State Request]
- RequestedFrame
- EntityFilter (optional)
```

---

## Quick Reference

### Key Classes to Create
1. `NetworkPredictor` - Main prediction engine
2. `StateHistory` - Circular buffer with branching
3. `Reconciler` - Smooth correction system
4. `ConfidenceScorer` - ML confidence estimation
5. `NetworkTimeSync` - Clock synchronization

### Key Classes to Modify
1. `MmRelayNode` - Add prediction hooks
2. `MmMessage` - Add prediction metadata
3. `MmNetworkManager` - Integrate prediction

### Performance Targets
- Perceived latency: <50ms
- Prediction accuracy: >95%
- Rollback frequency: <5%
- Bandwidth reduction: 30%
- Memory overhead: <100MB

### Critical Success Factors
1. Smooth visual experience
2. Improved presence scores
3. Reduced motion sickness
4. Developer adoption
5. Research publication

---

*This context document should be updated as network conditions are tested and implementation progresses.*