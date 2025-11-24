# Network Performance Optimization - Technical Context

**Last Updated:** 2025-11-18
**Status:** Planning - Not Started
**Priority:** HIGH (Phase 2.2 + 3.2)

---

## Status

**Current State:** Not Started
**Blockers:** None - ready to begin after routing-optimization
**Dependencies:** Routing optimization (Phase 2.1) for foundation
**Estimated Timeline:** 292 hours (7-8 weeks)

---

## Quick Resume

**Where to start if beginning this task:**
1. Review `USE_CASE.md` for business context and target scenarios
2. Read this document completely for technical architecture
3. Review `network-performance-tasks.md` for detailed checklist
4. Study `Assets/MercuryMessaging/Protocol/MmNetworkResponder.cs` (current implementation)
5. Review archived master plan Phase 2.2 and 3.2
6. Set up network testing environment (2 Unity instances)

**First 3 steps to take:**
1. Design state tracking architecture (8h)
2. Implement property change detection system (16h)
3. Create delta serialization prototype (20h)

**Key files to read first:**
- `Assets/MercuryMessaging/Protocol/MmNetworkResponder.cs`
- `Assets/MercuryMessaging/Protocol/MmMessage.cs`
- `Assets/MercuryMessaging/Protocol/MmNetworkResponderPhoton.cs`

---

## Technical Overview

This initiative focuses on optimizing Mercury's network synchronization capabilities to enable the distributed XR use cases detailed in `USE_CASE.md`. The optimization involves four core improvements:

1. **Delta State Synchronization** - Only send changed properties
2. **Priority-Based Message Queuing** - Critical messages first
3. **Message Batching & Pooling** - Reduce packets and GC pressure
4. **Reliability Tiers** - Guaranteed vs best-effort delivery

**Expected Impact:**
- 50-80% reduction in network traffic for state sync
- Zero GC allocations in hot paths
- Sub-frame latency for critical messages
- Graceful degradation under packet loss

---

## Current State Analysis

### Existing Network Architecture

**MmNetworkResponder.cs** provides basic networking:
- UNET/Photon integration
- Message serialization (all fields sent)
- Network filter (Local/Network/All)
- `IsDeserialized` flag to detect network messages
- `FlipNetworkFlagOnSend` prevents deep propagation

**Current Limitations:**
1. **Full State Serialization** - Sends entire message even if only one field changed
2. **No Prioritization** - All messages treated equally
3. **No Batching** - One message = one packet
4. **No Reliability Options** - Best-effort only (UNET/Photon defaults)
5. **GC Allocations** - Message objects not pooled
6. **No Conflict Resolution** - Last-write-wins implicit

### Performance Characteristics (Current)

- **Serialization:** ~0.001ms per message
- **Network overhead:** 40-200 bytes per message (full state)
- **Throughput:** ~500-1000 messages/sec before lag
- **Memory:** ~100 bytes per message (GC pressure)
- **Reliability:** Best-effort (95%+ delivery under normal conditions)

---

## Architecture Design

### Phase 2.2: Network Synchronization 2.0

#### 1. Automatic State Diffing

**Property Change Tracking:**

```csharp
public interface IMmStateful {
    Dictionary<string, object> GetStateDelta(int lastKnownVersion);
    void ApplyStateDelta(Dictionary<string, object> delta);
    int StateVersion { get; }
}

public class MmStatefulResponder : MmBaseResponder, IMmStateful {
    private Dictionary<string, object> _previousState = new Dictionary<string, object>();
    private int _stateVersion = 0;

    public Dictionary<string, object> GetStateDelta(int lastKnownVersion) {
        var delta = new Dictionary<string, object>();

        // Compare current state with previous
        foreach (var property in GetTrackedProperties()) {
            var currentValue = property.GetValue(this);
            if (_previousState.TryGetValue(property.Name, out var prevValue)) {
                if (!Equals(currentValue, prevValue)) {
                    delta[property.Name] = currentValue;
                    _previousState[property.Name] = currentValue;
                    _stateVersion++;
                }
            } else {
                delta[property.Name] = currentValue;
                _previousState[property.Name] = currentValue;
            }
        }

        return delta;
    }
}
```

**Delta Serialization:**

```csharp
public class MmNetworkMessage : MmMessage {
    public bool IsDelta = false;
    public Dictionary<string, object> StateDelta;
    public int BaseVersion; // Version this delta applies to

    public override byte[] Serialize() {
        if (IsDelta && StateDelta.Count < 3) {
            // Delta is smaller, use it
            return SerializeDelta();
        } else {
            // Full state is smaller, send that
            return SerializeFullState();
        }
    }
}
```

**Conflict Resolution Strategies:**

```csharp
public enum MmConflictStrategy {
    LastWriteWins,      // Default: most recent timestamp wins
    FirstWriteWins,     // First to arrive sticks
    ServerAuthoritative, // Server always wins
    ClientPredicted,    // Client predicts, server corrects
    Custom             // User-defined resolver
}

public class MmConflictResolver {
    public object Resolve(
        object localValue,
        object remoteValue,
        DateTime localTimestamp,
        DateTime remoteTimestamp,
        MmConflictStrategy strategy)
    {
        switch (strategy) {
            case MmConflictStrategy.LastWriteWins:
                return remoteTimestamp > localTimestamp ? remoteValue : localValue;
            case MmConflictStrategy.ServerAuthoritative:
                return IsServer ? localValue : remoteValue;
            // ... other strategies
        }
    }
}
```

#### 2. Priority-Based Message Queuing

**Priority Levels:**

```csharp
public enum MmNetworkPriority {
    Critical = 0,  // Send immediately (no batching, no delay)
    High = 1,      // Send next frame (minimal batching)
    Normal = 2,    // 100ms buffer window (default)
    Low = 3        // 1s buffer window (background sync)
}

public class MmNetworkOptions {
    public MmNetworkPriority Priority = MmNetworkPriority.Normal;
    public MmNetworkReliability Reliability = MmNetworkReliability.Unreliable;
    public bool EnableCompression = false;
    public float TimeoutSeconds = 5.0f;
}
```

**Priority Queue System:**

```csharp
public class MmPriorityMessageQueue {
    private Queue<MmMessage>[] _queues = new Queue<MmMessage>[4]; // One per priority
    private float[] _flushTimers = new float[4];
    private int[] _maxBatchSizes = { 1, 10, 50, 100 };

    public void Enqueue(MmMessage message) {
        int priority = (int)message.NetworkOptions.Priority;
        _queues[priority].Enqueue(message);

        // Critical messages flush immediately
        if (priority == 0) {
            FlushQueue(0);
        }
    }

    public void Update(float deltaTime) {
        for (int i = 0; i < 4; i++) {
            _flushTimers[i] += deltaTime;

            // Check if buffer window expired or batch size reached
            if (_flushTimers[i] >= GetBufferWindow(i) ||
                _queues[i].Count >= _maxBatchSizes[i]) {
                FlushQueue(i);
                _flushTimers[i] = 0;
            }
        }
    }

    private float GetBufferWindow(int priority) {
        return new float[] { 0f, 0.016f, 0.1f, 1.0f }[priority];
    }
}
```

**Adaptive Rate Limiting:**

```csharp
public class MmAdaptiveRateLimiter {
    private float _currentBandwidth = 0f;
    private float _maxBandwidth = 10000f; // 10 KB/s default
    private float _bandwidthUsage = 0f;

    public bool CanSend(int messageSize) {
        return _bandwidthUsage + messageSize <= _maxBandwidth;
    }

    public void RecordSent(int messageSize) {
        _bandwidthUsage += messageSize;
    }

    public void Update(float deltaTime) {
        // Decay bandwidth usage over time
        _bandwidthUsage = Mathf.Max(0, _bandwidthUsage - _maxBandwidth * deltaTime);

        // Measure actual bandwidth
        _currentBandwidth = _bandwidthUsage / deltaTime;

        // Adjust max if approaching limits
        if (_currentBandwidth > _maxBandwidth * 0.9f) {
            // Reduce quality, increase batching
            AdjustQuality(0.8f);
        }
    }
}
```

#### 3. Reliability Tiers

**Reliability Options:**

```csharp
public enum MmNetworkReliability {
    Unreliable,           // Best effort, no ACK (default)
    Reliable,             // Guaranteed delivery with ACK
    ReliableOrdered,      // Guaranteed in-order delivery
    ReliableSequenced     // Latest only, discard old
}
```

**ACK System:**

```csharp
public class MmReliableDelivery {
    private Dictionary<int, MmMessage> _pendingAcks = new Dictionary<int, MmMessage>();
    private Dictionary<int, float> _sendTimes = new Dictionary<int, float>();
    private int _nextSequenceNumber = 0;

    public void SendReliable(MmMessage message) {
        message.SequenceNumber = _nextSequenceNumber++;
        _pendingAcks[message.SequenceNumber] = message;
        _sendTimes[message.SequenceNumber] = Time.time;

        SendMessage(message);
    }

    public void ReceiveAck(int sequenceNumber) {
        if (_pendingAcks.ContainsKey(sequenceNumber)) {
            // Calculate RTT
            float rtt = Time.time - _sendTimes[sequenceNumber];
            UpdateRTTEstimate(rtt);

            // Remove from pending
            _pendingAcks.Remove(sequenceNumber);
            _sendTimes.Remove(sequenceNumber);
        }
    }

    public void Update() {
        // Retry messages that haven't been ACKed
        foreach (var kvp in _pendingAcks.ToList()) {
            float elapsed = Time.time - _sendTimes[kvp.Key];

            if (elapsed > GetRetryTimeout()) {
                // Resend with exponential backoff
                SendMessage(kvp.Value);
                _sendTimes[kvp.Key] = Time.time;
            }
        }
    }

    private float GetRetryTimeout() {
        // Based on measured RTT + variance
        return _estimatedRTT + 4 * _rttVariance;
    }
}
```

### Phase 3.2: Message Processing Optimization (Network-Specific)

#### 1. Message Batching

**Batch Serialization:**

```csharp
public class MmMessageBatcher {
    private List<MmMessage> _batch = new List<MmMessage>(100);
    private float _batchTimeout = 0.016f; // ~1 frame
    private int _maxBatchSize = 100;
    private int _maxPacketSize = 1200; // MTU-safe

    public void QueueMessage(MmMessage msg) {
        _batch.Add(msg);

        // Check if should flush
        if (GetBatchSize() >= _maxPacketSize ||
            _batch.Count >= _maxBatchSize) {
            Flush();
        }
    }

    public void Flush() {
        if (_batch.Count == 0) return;

        // Serialize all messages into single packet
        var packet = new MmBatchPacket {
            MessageCount = _batch.Count,
            Messages = _batch.ToArray()
        };

        SendPacket(packet);
        _batch.Clear();
    }

    private int GetBatchSize() {
        int totalSize = 0;
        foreach (var msg in _batch) {
            totalSize += msg.GetSerializedSize();
        }
        return totalSize;
    }
}
```

#### 2. Object Pooling

**Message Pool:**

```csharp
public class MmMessagePool<T> where T : MmMessage, new() {
    private Queue<T> _pool = new Queue<T>(1000);
    private int _maxSize = 1000;
    private int _allocatedCount = 0;
    private int _recycledCount = 0;

    public T Acquire() {
        T message;
        if (_pool.Count > 0) {
            message = _pool.Dequeue();
            _recycledCount++;
        } else {
            message = new T();
            _allocatedCount++;
        }
        return message;
    }

    public void Release(T message) {
        if (_pool.Count < _maxSize) {
            message.Reset();  // Clear all data
            _pool.Enqueue(message);
        }
    }

    public PoolStatistics GetStatistics() {
        return new PoolStatistics {
            CurrentPoolSize = _pool.Count,
            TotalAllocated = _allocatedCount,
            TotalRecycled = _recycledCount,
            RecycleRate = (float)_recycledCount / (_allocatedCount + _recycledCount)
        };
    }
}

// Global pool manager
public static class MmMessagePools {
    private static Dictionary<Type, object> _pools = new Dictionary<Type, object>();

    public static T Acquire<T>() where T : MmMessage, new() {
        if (!_pools.ContainsKey(typeof(T))) {
            _pools[typeof(T)] = new MmMessagePool<T>();
        }
        return ((MmMessagePool<T>)_pools[typeof(T)]).Acquire();
    }

    public static void Release<T>(T message) where T : MmMessage, new() {
        if (_pools.ContainsKey(typeof(T))) {
            ((MmMessagePool<T>)_pools[typeof(T)]).Release(message);
        }
    }
}
```

#### 3. Compression

**Adaptive Compression:**

```csharp
public class MmMessageCompressor {
    private bool _compressionEnabled = true;
    private int _compressionThreshold = 200; // bytes

    public byte[] Compress(MmMessage message) {
        var data = message.Serialize();

        // Only compress if larger than threshold
        if (!_compressionEnabled || data.Length < _compressionThreshold) {
            return data;
        }

        using (var output = new MemoryStream()) {
            using (var gzip = new GZipStream(output, CompressionMode.Compress)) {
                gzip.Write(data, 0, data.Length);
            }

            var compressed = output.ToArray();

            // Only use if actually smaller
            return compressed.Length < data.Length ? compressed : data;
        }
    }

    public MmMessage Decompress(byte[] data, bool isCompressed) {
        if (!isCompressed) {
            return MmMessage.Deserialize(data);
        }

        using (var input = new MemoryStream(data)) {
            using (var gzip = new GZipStream(input, CompressionMode.Decompress)) {
                using (var output = new MemoryStream()) {
                    gzip.CopyTo(output);
                    return MmMessage.Deserialize(output.ToArray());
                }
            }
        }
    }
}
```

---

## Design Decisions

### Decision 1: Delta Sync vs Full State

**Options:**
A. Always send full state (simple, reliable)
B. Always send delta (efficient, complex)
C. Adaptive (auto-choose based on size)

**Decision:** C (Adaptive)

**Rationale:**
- Full state safer for initial sync and large changes
- Delta more efficient for frequent small updates
- Auto-selection optimizes both cases
- Fallback to full state on desync

### Decision 2: Conflict Resolution Default

**Options:**
A. Last-write-wins (simple, can lose data)
B. Server-authoritative (requires server role)
C. CRDT-based (complex, always consistent)

**Decision:** A (Last-write-wins) with option for B

**Rationale:**
- Most games use last-write-wins successfully
- Server-authoritative available for critical state
- CRDT too complex for initial implementation
- Can add CRDT later if needed

### Decision 3: Reliability Implementation

**Options:**
A. Use underlying transport (UNET/Photon)
B. Custom ACK/retry layer
C. Hybrid (use transport when available)

**Decision:** C (Hybrid)

**Rationale:**
- Leverage existing reliability when available
- Custom layer for fine-grained control
- Consistent API across transports
- Better monitoring and diagnostics

---

## Implementation Strategy

### Phase 2.2 Implementation (156 hours)

**Week 1-2: State Tracking (44h)**
1. Design state tracking architecture (8h)
2. Implement property change detection (16h)
3. Create delta serialization system (20h)

**Week 3-4: Priority & Reliability (60h)**
4. Implement priority queue system (12h)
5. Add reliability tiers with ACK logic (24h)
6. Create message batching system (16h)
7. Add conflict resolution strategies (16h)

**Week 5-6: Optimization & Testing (52h)**
8. Implement compression (12h)
9. Network testing and optimization (20h)
10. Documentation and examples (12h)

### Phase 3.2 Implementation (136 hours)

**Week 1-2: Batching & Pooling (52h)**
1. Design batching architecture (8h)
2. Implement MmMessageBatcher (16h)
3. Create object pool system (12h)
4. Add pooling to all message types (16h)

**Week 3-4: Testing & Integration (52h)**
5. Profile and optimize hot paths (20h)
6. GC allocation testing (12h)
7. Integration testing (16h)

**Week 5: Documentation (32h)**
8. Documentation (8h)
9. Network configuration guide (12h)
10. Best practices document (12h)

---

## Testing Strategy

### Network Test Scenarios

1. **Packet Loss Testing**
   - 0%, 5%, 10%, 20% packet loss
   - Measure: delivery rate, retry count, latency

2. **Latency Testing**
   - 0ms, 50ms, 100ms, 200ms simulated latency
   - Measure: end-to-end message time, jitter

3. **Bandwidth Testing**
   - Saturate connection with messages
   - Measure: throughput, dropped messages, degradation

4. **Concurrent Players**
   - 2, 4, 8, 16 simultaneous connections
   - Measure: per-client performance, fairness

5. **State Sync Testing**
   - Rapid state changes (100+ updates/sec)
   - Measure: bandwidth usage, desync rate

### Performance Benchmarks

**Target Metrics:**
- 50-80% reduction in network traffic (delta sync)
- Zero GC allocations in steady state
- < 16ms latency for Critical priority
- 99%+ delivery for Reliable mode
- 1000+ messages/sec throughput

**Comparison Baseline:**
- Current MmNetworkResponder performance
- Unity HLAPI performance
- Photon Bolt performance
- Mirror networking performance

---

## Migration Path

### For Existing Network Code

**Step 1:** No changes required - existing code continues to work

**Step 2:** Enable delta sync for high-frequency updates
```csharp
public class PlayerController : MmStatefulResponder {
    [MmTrackedProperty] // New attribute
    public Vector3 position;

    [MmTrackedProperty]
    public Quaternion rotation;
}
```

**Step 3:** Add priority to critical messages
```csharp
relay.MmInvoke(MmMethod.MessageString, "HitPlayer", new MmMetadataBlock {
    NetworkFilter = MmNetworkFilter.All,
    NetworkOptions = new MmNetworkOptions {
        Priority = MmNetworkPriority.Critical,
        Reliability = MmNetworkReliability.Reliable
    }
});
```

**Step 4:** Enable object pooling
```csharp
// Acquire from pool instead of new
var message = MmMessagePools.Acquire<MmMessageVector3>();
message.value = playerPosition;
relay.MmInvoke(message);
MmMessagePools.Release(message); // Return to pool
```

---

## Open Questions

1. **Should compression be per-message or per-batch?**
   - Batch compression likely more efficient
   - Need to measure compression ratio

2. **How to handle host migration with delta state?**
   - May need full state snapshot on migration
   - Or maintain complete state on all clients

3. **What MTU size to target?**
   - 1200 bytes safe for most networks
   - Could auto-detect and adjust

---

## References

### Key Files
- `Assets/MercuryMessaging/Protocol/MmNetworkResponder.cs` - Current implementation
- `Assets/MercuryMessaging/Protocol/MmNetworkResponderPhoton.cs` - Photon integration
- `Assets/MercuryMessaging/Protocol/MmMessage.cs` - Base message class

### Related Documents
- Master Plan: Phase 2.2 (Network Sync 2.0) and Phase 3.2 (Message Processing)
- Tasks Checklist: `network-performance-tasks.md`
- Routing Optimization: Dependencies for routing enhancements

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Network Performance Team
