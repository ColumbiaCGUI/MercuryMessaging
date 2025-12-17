# Parallel Hierarchical State Machines

## Overview

Implementation of parallel hierarchical state machines for the MercuryMessaging framework, enabling multiple concurrent FSMs to operate within the same message routing system. This module supports complex multi-modal interactions through orthogonal state regions.

**For business context and use cases, see [`USE_CASE.md`](./USE_CASE.md)**

---

## Technical Architecture

### Parallel FSM Structure

```
Root State Machine
├── Interaction Layer (Parallel Region 1)
│   ├── Gesture FSM
│   ├── Voice FSM
│   └── Gaze FSM
├── Application Layer (Parallel Region 2)
│   ├── Navigation FSM
│   ├── Manipulation FSM
│   └── Menu FSM
└── System Layer (Parallel Region 3)
    ├── Network FSM
    ├── Performance FSM
    └── Error FSM

Message Bus (MercuryMessaging)
  ↓ Synchronization Messages ↓
Cross-Region Communication
```

---

## Core Features

### Orthogonal State Regions
- Multiple independent state machines operating in parallel
- Each region maintains its own state and transitions
- Regions execute concurrently without blocking

### Cross-Machine Synchronization
- Message-based coordination between parallel FSMs
- No shared memory approach for thread safety
- Event ordering guarantees maintained

### Hierarchical Composition
- Parent FSMs orchestrate child FSMs
- State inheritance and propagation
- Nested parallel regions support

### Conflict Resolution
- Priority-based resolution for competing transitions
- Voting mechanisms for consensus
- Hierarchical override capabilities

---

## Implementation

### Parallel Region Implementation
```csharp
class ParallelFSM : MmRelaySwitchNode {
    List<StateMachine> parallelRegions;

    void Update() {
        // All regions process simultaneously
        Parallel.ForEach(parallelRegions, region => {
            region.ProcessMessages();
            region.UpdateState();
        });
        ResolveCrossRegionConflicts();
    }
}
```

### Message-Based Synchronization
- Regions communicate exclusively via Mercury messages
- Prevents race conditions through message isolation
- Maintains event ordering guarantees

### State Machine Components
1. **State Definition**: Encapsulated state behavior
2. **Transition System**: Rule-based state changes
3. **Guard Conditions**: Transition prerequisites
4. **Actions**: State entry/exit/during behaviors

---

## Technical Implementation Phases

### Phase 1: Core System
- Parallel region infrastructure
- Basic synchronization mechanisms
- Simple conflict resolution

### Phase 2: Advanced Features
- Dynamic region creation/destruction
- Complex synchronization patterns
- State persistence and restoration

### Phase 3: Integration
- Multi-modal input handling
- Performance optimization
- Developer tooling

---

## Use Cases

### Multi-Modal Interfaces
- Simultaneous gesture, voice, and gaze input
- Context-aware transition logic
- Smooth modality switching

### Complex Application States
- Game states (menu, gameplay, pause) running in parallel
- UI states independent of application logic
- Network states concurrent with user interaction

### System Management
- Performance monitoring in parallel with application
- Error handling without blocking main flow
- Background task management

---

## Performance Considerations

### Threading Model
- Lock-free message passing
- Thread-safe state transitions
- Concurrent region execution

### Memory Management
- State pooling for frequent transitions
- Message recycling
- Bounded region count

### Optimization Strategies
- Lazy evaluation of unused regions
- Priority-based region scheduling
- Batch message processing

---

## Testing Strategy

### Correctness Testing
- State transition validation
- Deadlock detection
- Race condition prevention

### Performance Testing
- Parallel vs sequential benchmarking
- Scalability with region count
- Message throughput analysis

### Integration Testing
- Mercury message compatibility
- Cross-region communication
- Hierarchical state propagation

---

## Configuration

```csharp
public class ParallelFSMConfig {
    public int MaxParallelRegions { get; set; } = 10;
    public bool EnableConflictResolution { get; set; } = true;
    public ConflictStrategy DefaultStrategy { get; set; } = ConflictStrategy.Priority;
    public int MessageQueueSize { get; set; } = 1000;
    public bool EnableStateLogging { get; set; } = false;
}
```

---

## Dependencies

- MercuryMessaging framework (MmRelaySwitchNode)
- System.Threading.Tasks for parallel execution
- Unity 2021.3+ LTS

---

## Performance Targets

- State transition overhead: <1ms
- Parallel region scaling: Linear to 10+ regions
- Message synchronization: Zero deadlocks
- Memory overhead: <10MB for 10 regions

---

*Last Updated: 2025-11-20*
*Estimated Implementation Time: 280 hours*