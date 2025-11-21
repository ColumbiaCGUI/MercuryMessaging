# Parallel Message Dispatch

## Overview

Implementation of concurrent message processing for the MercuryMessaging framework using thread-safe data structures and work-stealing queues. This module enables parallel dispatch of messages across multiple threads while maintaining message ordering guarantees within sender-receiver pairs.

---

## Technical Architecture

### System Overview

```
Main Thread                    Worker Threads (1..N)
    |                               |
    v                               v
[Message Queue] --distribute--> [Thread Pool]
    |                               |
    v                               v
[Routing Table] --partition-->  [Local Tables]
    |                               |
    v                               v
[Responders] <--synchronize--  [Responders]
```

### Key Components

#### 1. Lock-Free Message Queue
- MPSC (multi-producer, single-consumer) for main thread
- SPMC (single-producer, multi-consumer) for distribution
- Based on standard concurrent queue patterns

#### 2. Hierarchical Partitioning
- Scene graph analysis for independent subtrees
- Work-stealing queue for load balancing
- Cache-aware routing table design

#### 3. Consistency Model
- Happens-before relationships preserved
- Message ordering within sender-receiver pairs
- Eventual consistency for broadcast messages

---

## Implementation Details

### Message Queue Implementation

```csharp
public class ConcurrentMessageQueue {
    private readonly ConcurrentQueue<MmMessage> queue;
    private readonly SemaphoreSlim semaphore;

    public void Enqueue(MmMessage message) {
        queue.Enqueue(message);
        semaphore.Release();
    }

    public async Task<MmMessage> DequeueAsync(CancellationToken ct) {
        await semaphore.WaitAsync(ct);
        queue.TryDequeue(out var message);
        return message;
    }
}
```

### Work Distribution Strategy

```csharp
public class WorkDistributor {
    private readonly WorkStealingQueue<MmMessage>[] queues;
    private readonly int workerCount;

    public void Distribute(MmMessage message, MmRoutingTable routingTable) {
        // Partition based on scene graph locality
        int partition = GetPartition(message, routingTable);
        queues[partition].Push(message);
    }

    private int GetPartition(MmMessage message, MmRoutingTable table) {
        // Hash-based partitioning with locality awareness
        return (message.SourceNode.GetHashCode() & 0x7FFFFFFF) % workerCount;
    }
}
```

### Thread Pool Management

```csharp
public class MessageDispatcherPool {
    private readonly Thread[] workers;
    private readonly BlockingCollection<MmMessage> workQueue;

    public void Start(int threadCount) {
        for (int i = 0; i < threadCount; i++) {
            workers[i] = new Thread(WorkerLoop) {
                Name = $"MercuryWorker-{i}",
                IsBackground = true
            };
            workers[i].Start();
        }
    }

    private void WorkerLoop() {
        foreach (var message in workQueue.GetConsumingEnumerable()) {
            ProcessMessage(message);
        }
    }
}
```

---

## Unity Integration

### Unity Job System Compatibility

```csharp
[BurstCompile]
public struct MessageDispatchJob : IJobParallelFor {
    [ReadOnly] public NativeArray<MessageData> messages;
    [NativeDisableContainerSafetyRestriction]
    public NativeHashMap<int, ResponderData> routingTable;

    public void Execute(int index) {
        var message = messages[index];
        // Process message with Burst optimization
    }
}
```

### Main Thread Synchronization

```csharp
public class MainThreadDispatcher {
    private readonly Queue<Action> mainThreadQueue;
    private readonly object queueLock = new object();

    public void Update() {
        lock (queueLock) {
            while (mainThreadQueue.Count > 0) {
                mainThreadQueue.Dequeue()?.Invoke();
            }
        }
    }
}
```

---

## Performance Characteristics

### Throughput
- Single-threaded baseline: 980 msg/sec
- Target with parallelization: 10,000+ msg/sec
- Scaling: Near-linear up to 8 cores

### Latency
- Average message delivery: <1ms
- 99th percentile: <5ms
- Maximum under load: <10ms

### Memory Usage
- Per-thread overhead: ~1MB
- Message queue buffer: Configurable (default 10,000)
- Routing table cache: O(n) where n = responder count

---

## Configuration

```csharp
public class ParallelDispatchConfig {
    public int WorkerThreadCount { get; set; } = Environment.ProcessorCount;
    public int MessageQueueCapacity { get; set; } = 10000;
    public bool UseWorkStealing { get; set; } = true;
    public bool EnableBurstCompilation { get; set; } = true;
    public int PartitioningStrategy { get; set; } = PartitionStrategy.SceneGraphLocality;
}
```

---

## Testing Strategy

### Correctness Testing
- Race condition detection using Thread Sanitizer
- Message ordering verification
- Deadlock prevention validation
- Memory consistency checks

### Performance Testing
- Throughput benchmarks at various scales
- Latency distribution analysis
- CPU utilization profiling
- Memory allocation tracking

### Stress Testing
- 10,000+ messages per second sustained
- 1000+ concurrent responders
- Deep hierarchy (10+ levels)
- Random message patterns

---

## Known Limitations

1. **Unity Main Thread Requirements**
   - Component access must return to main thread
   - Transform modifications require synchronization
   - UI updates need main thread dispatch

2. **Message Ordering**
   - Total ordering not guaranteed across different senders
   - Broadcast messages have eventual consistency
   - Priority messages not yet supported

3. **Platform Support**
   - Requires .NET Standard 2.1 for concurrent collections
   - WebGL builds fall back to single-threaded
   - Mobile platforms limited by core count

---

## Dependencies

- Unity 2021.3+ (Job System, Burst Compiler)
- .NET Standard 2.1 (Concurrent Collections)
- MercuryMessaging core framework

---

*Last Updated: 2025-11-20*
*Estimated Implementation Time: 360 hours*