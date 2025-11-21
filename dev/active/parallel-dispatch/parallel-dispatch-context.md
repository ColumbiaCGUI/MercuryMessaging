# Parallel Message Dispatch - Context Document

*Last Updated: 2025-11-20*

## Overview

This document captures key context, dependencies, and decisions for the parallel message dispatch implementation. It serves as a reference for understanding the technical landscape and architectural constraints.

---

## Key Files and Components

### Core MercuryMessaging Files

#### Primary Integration Points
- **`Assets/MercuryMessaging/Protocol/MmRelayNode.cs`** (1422 lines)
  - Central message router - MOST CRITICAL FILE
  - Must extend for parallel dispatch
  - Contains routing table and message distribution logic
  - Key methods: MmInvoke, MmInvokeBufferedCommand, RoutingTable property

- **`Assets/MercuryMessaging/Protocol/MmMessage.cs`**
  - Base message type definition
  - Must ensure thread-safety for concurrent access
  - Consider immutability requirements

- **`Assets/MercuryMessaging/Protocol/MmRoutingTable.cs`**
  - Manages responder lists
  - Current implementation is single-threaded
  - Needs thread-safe variant for parallel access

- **`Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs`**
  - Message filtering metadata
  - Stateless, already thread-safe
  - May benefit from caching optimizations

#### Supporting Components
- **`Assets/MercuryMessaging/Protocol/MmResponder.cs`**
  - Base responder interface
  - Message handlers must be thread-safe

- **`Assets/MercuryMessaging/Support/Data/MmCircularBuffer.cs`**
  - Thread-safe buffer implementation (QW-4)
  - Can reuse for message queuing

- **`Assets/MercuryMessaging/Protocol/MmLogger.cs`**
  - Logging system
  - Needs thread-safe extensions

### Performance Infrastructure

- **`Assets/MercuryMessaging/Tests/Runtime/Performance/`**
  - Performance test harness
  - Benchmark baselines established
  - Current metrics: 980 msg/sec (Large scale)

- **`Assets/MercuryMessaging/Documentation/Performance/`**
  - Performance reports and analysis
  - Optimization results documented
  - Baseline measurements available

### Unity Integration Points

#### Unity Systems to Integrate
- **Unity Job System**
  - IJob, IJobParallelFor interfaces
  - JobHandle for synchronization
  - NativeArray for data transfer

- **Burst Compiler**
  - BurstCompile attribute
  - Function pointers
  - Restrictions on managed types

- **Unity Profiler**
  - Profiler.BeginSample/EndSample
  - CustomSampler API
  - Timeline visualization

---

## Technical Dependencies

### External Dependencies

#### Required Unity Packages
```json
{
  "dependencies": {
    "com.unity.burst": "1.8.0",
    "com.unity.jobs": "0.70.0",
    "com.unity.collections": "1.2.0",
    "com.unity.mathematics": "1.2.0",
    "com.unity.profiling.core": "1.0.0"
  }
}
```

#### Third-Party Considerations
- No external threading libraries (use Unity's systems)
- No additional NuGet packages (maintain zero dependencies)
- Consider .NET concurrent collections if needed

### Internal Dependencies

#### Message System Dependencies
- MmMessage type hierarchy
- MmMethod enumeration
- Serialization system (for network messages)
- MmResponder registration system

#### Framework Features That Must Continue Working
1. **Hierarchical Routing**: Parent/child message propagation
2. **Tag Filtering**: 8-bit tag system
3. **Network Messages**: Serialization and ordering
4. **FSM Integration**: MmRelaySwitchNode state management
5. **Debug Visualization**: Message history tracking

---

## Architectural Decisions

### Decision 1: Unity Job System vs Raw Threads
**Decision**: Use Unity Job System
**Rationale**:
- Better Unity integration
- Automatic thread management
- Burst compiler compatibility
- Safety system prevents race conditions
**Trade-offs**:
- Limited to struct-based data
- No direct GameObject access
- Learning curve for Job System

### Decision 2: Message Copying Strategy
**Decision**: Copy-on-write with immutable messages
**Rationale**:
- Thread safety without locks
- Leverages QW-2 lazy copying
- Predictable performance
**Trade-offs**:
- Memory overhead for copies
- GC pressure if not pooled

### Decision 3: Partitioning Strategy
**Decision**: Hierarchical subtree partitioning
**Rationale**:
- Natural scene graph boundaries
- Minimizes cross-partition messages
- Cache-friendly access patterns
**Trade-offs**:
- Unbalanced trees cause inefficiency
- Repartitioning overhead

### Decision 4: API Design
**Decision**: Opt-in with backwards compatibility
**Rationale**:
- No breaking changes
- Gradual adoption possible
- Fallback for issues
**Trade-offs**:
- Dual code paths to maintain
- Configuration complexity

---

## Performance Context

### Current Performance Baseline
From `Assets/MercuryMessaging/Documentation/Performance/OPTIMIZATION_RESULTS.md`:

| Scale | Responders | Levels | Frame Time | Throughput | Memory |
|-------|------------|--------|------------|------------|--------|
| Small | 10 | 3 | 15.14ms | 98 msg/sec | 925 MB |
| Medium | 50 | 5 | 16.28ms | 492 msg/sec | 935 MB |
| Large | 100+ | 7-10 | 18.69ms | 980 msg/sec | 940 MB |

### Bottleneck Analysis
1. **Single-threaded routing** (40% of frame time)
2. **Synchronous message delivery** (30% of frame time)
3. **Routing table iteration** (20% of frame time)
4. **Message allocation** (10% of frame time)

### Optimization Opportunities
- Parallel routing table traversal
- Concurrent message delivery
- Batch processing for small messages
- Thread-local message pools

---

## Code Patterns and Conventions

### Current Patterns

#### Message Invocation Pattern
```csharp
public void MmInvoke(MmMethod method, object param, MmMetadataBlock metadata) {
    var message = CreateMessage(method, param, metadata);
    var responders = RoutingTable.GetResponders(metadata);
    foreach (var responder in responders) {
        responder.MmInvoke(message);
    }
}
```

#### Responder Registration Pattern
```csharp
public class CustomResponder : MmBaseResponder {
    protected override void RegisterAwakenedResponder() {
        base.RegisterAwakenedResponder();
        // Auto-registration with relay node
    }
}
```

### Threading Patterns to Implement

#### Lock-Free Queue Pattern
```csharp
public class LockFreeQueue<T> {
    private readonly ConcurrentQueue<T> queue;
    private readonly SemaphoreSlim semaphore;

    public void Enqueue(T item) {
        queue.Enqueue(item);
        semaphore.Release();
    }

    public async Task<T> DequeueAsync() {
        await semaphore.WaitAsync();
        queue.TryDequeue(out T item);
        return item;
    }
}
```

#### Job System Pattern
```csharp
[BurstCompile]
struct MessageProcessJob : IJobParallelFor {
    [ReadOnly] public NativeArray<MessageData> messages;
    public NativeArray<ResultData> results;

    public void Execute(int index) {
        // Process message[index]
        results[index] = ProcessMessage(messages[index]);
    }
}
```

---

## Unity-Specific Constraints

### Main Thread Requirements
- GameObject property access
- Transform manipulation
- Component addition/removal
- Unity API calls (most)

### Job System Limitations
- No reference types in jobs
- No static field access
- Limited Unity API access
- Deterministic execution required

### Burst Compiler Restrictions
- No managed allocations
- No virtual calls
- No try-catch blocks
- Limited delegate support

---

## Testing Context

### Existing Test Infrastructure

#### Performance Tests
- `CircularBufferMemoryTests.cs` - Memory stability
- `HopLimitValidationTests.cs` - Message hop limits
- `CycleDetectionValidationTests.cs` - Infinite loop prevention
- `LazyCopyValidationTests.cs` - Message copying

#### Test Patterns
```csharp
[Test]
public void ParallelDispatch_Maintains_MessageOrder() {
    // Arrange
    var relay = CreateTestRelay();
    var messages = CreateOrderedMessages(1000);

    // Act
    relay.EnableParallelDispatch();
    foreach (var msg in messages) {
        relay.MmInvoke(msg);
    }

    // Assert
    AssertMessageOrderPreserved();
}
```

### New Test Requirements
1. **Concurrency Tests**: Race conditions, deadlocks
2. **Performance Tests**: Scaling, throughput
3. **Correctness Tests**: Message ordering, delivery guarantees
4. **Stress Tests**: High load, memory stability

---

## Research Context

### Related Publications
- **Actor Model**: Hewitt et al. (1973) - Foundational concurrent computation
- **Unity DOTS**: Unity Technologies (2019) - Data-oriented design
- **Grand Central Dispatch**: Apple (2009) - Task parallelism

### Competitive Analysis
| System | Threading | Hierarchy | Message-Based | Performance |
|--------|-----------|-----------|---------------|-------------|
| Unity SendMessage | Single | Yes | Yes | Slow (reflection) |
| UnityEvents | Single | No | Yes | Medium |
| Photon Fusion | Multi | No | Yes | Fast (tick-based) |
| **Our System** | **Multi** | **Yes** | **Yes** | **Target: Fastest** |

### Conference Requirements (UIST 2025)
- Submission deadline: April 15, 2025
- Page limit: 10 pages + references
- Evaluation requirements: Technical + user study
- Review criteria: Novelty, significance, validity

---

## Risk Factors

### Technical Risks
1. **Unity version compatibility** - Test on 2021.3, 2022.3, 2023.1
2. **Platform differences** - Windows, Mac, Linux, Mobile, WebGL
3. **Integration complexity** - Many moving parts
4. **Performance variability** - Hardware differences

### Mitigation Strategies
- Comprehensive platform testing
- Fallback mechanisms
- Configuration presets
- Performance adaptation

---

## Development Environment

### Required Setup
```bash
# Unity Version
Unity 2021.3.16f1 LTS (minimum)

# IDE Setup
Visual Studio 2022 / Rider 2023.3
.NET SDK 6.0

# Performance Tools
Unity Profiler
Superluminal Performance Profiler
Intel VTune (optional)

# Version Control
Git with LFS for binaries
```

### Build Configuration
```csharp
// Conditional compilation
#if PARALLEL_DISPATCH_ENABLED && UNITY_2021_3_OR_NEWER
    // Parallel implementation
#else
    // Fallback implementation
#endif
```

---

## Communication Channels

### Documentation Locations
- Technical specs: `dev/active/parallel-dispatch/`
- API documentation: `Assets/MercuryMessaging/Documentation/`
- Progress tracking: `parallel-dispatch-tasks.md`
- Research notes: `README.md`

### Code Organization
```
Assets/MercuryMessaging/
├── Parallel/                    # New parallel system
│   ├── Core/                   # Lock-free structures
│   ├── Jobs/                   # Unity Job implementations
│   ├── Routing/                # Parallel routing
│   └── Tests/                  # Parallel-specific tests
├── Protocol/                    # Existing (modified)
│   └── MmRelayNode.cs          # Extended with parallel
└── Documentation/
    └── ParallelGuide.md        # Usage documentation
```

---

## Quick Reference

### Key Classes to Modify
1. `MmRelayNode` - Add parallel dispatch methods
2. `MmRoutingTable` - Thread-safe version
3. `MmMessage` - Ensure immutability

### Key Classes to Create
1. `ParallelDispatcher` - Main parallel system
2. `LockFreeMessageQueue` - Thread-safe queue
3. `MessageProcessJob` - Job System implementation
4. `ThreadPoolManager` - Thread management

### Performance Targets
- Throughput: 10,000 msg/sec (10x improvement)
- Latency: <1ms (from 5-10ms)
- Scaling: Linear to 8 cores
- Frame time: <16ms for 1000+ responders

---

*This context document should be updated as implementation progresses and new decisions are made.*