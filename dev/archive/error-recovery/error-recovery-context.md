# Error Recovery & Graceful Degradation - Technical Context

*Last Updated: 2025-11-20*

## Overview

This document provides technical context for implementing error recovery and graceful degradation in the MercuryMessaging framework, focusing on distributed systems patterns and fault tolerance mechanisms.

---

## Technical Background

### Circuit Breaker Pattern

The circuit breaker pattern prevents cascading failures in distributed systems by monitoring failure rates and temporarily blocking calls to failing services.

#### States
```csharp
public enum CircuitState {
    Closed,     // Normal operation, requests pass through
    Open,       // Failure threshold exceeded, requests blocked
    HalfOpen    // Testing recovery, limited requests allowed
}
```

#### Implementation Model
```csharp
public class CircuitBreaker {
    private CircuitState state = CircuitState.Closed;
    private int failureCount = 0;
    private DateTime lastFailureTime;
    private readonly int failureThreshold = 5;
    private readonly TimeSpan timeout = TimeSpan.FromSeconds(60);

    public T Execute<T>(Func<T> action) {
        if (state == CircuitState.Open) {
            if (DateTime.Now - lastFailureTime > timeout) {
                state = CircuitState.HalfOpen;
            } else {
                throw new CircuitBreakerOpenException();
            }
        }

        try {
            var result = action();
            OnSuccess();
            return result;
        } catch (Exception ex) {
            OnFailure();
            throw;
        }
    }
}
```

### Health Monitoring Architecture

#### Health Check Components
1. **Active Health Checks**: Periodic probing of components
2. **Passive Health Checks**: Monitoring actual traffic
3. **Hybrid Approach**: Combination of active and passive

#### Health Score Calculation
```csharp
public class HealthScore {
    private readonly CircularBuffer<HealthSample> samples;
    private readonly int windowSize = 100;

    public float Calculate() {
        var recentSamples = samples.GetRecent(windowSize);
        var successRate = recentSamples.Count(s => s.Success) / (float)recentSamples.Count;
        var avgLatency = recentSamples.Average(s => s.Latency);
        var errorRate = recentSamples.Count(s => s.Error) / (float)recentSamples.Count;

        // Weighted health score
        return (successRate * 0.5f) +
               ((1f - Math.Min(avgLatency / 1000f, 1f)) * 0.3f) +
               ((1f - errorRate) * 0.2f);
    }
}
```

---

## MercuryMessaging Integration

### Failure Points in Message Routing

1. **Node Failures**
   - Responder unresponsive
   - Component destroyed
   - Script exceptions

2. **Route Failures**
   - Invalid hierarchy
   - Circular dependencies
   - Broken parent-child links

3. **Network Failures**
   - Message serialization errors
   - Network timeouts
   - Desynchronization

### Recovery Integration Points

#### MmRelayNode Extension
```csharp
public class ResilientMmRelayNode : MmRelayNode {
    private readonly Dictionary<string, CircuitBreaker> circuitBreakers;
    private readonly HealthMonitor healthMonitor;

    protected override void RouteMessage(MmMessage message) {
        var routes = GetRoutes(message);
        var primaryRoute = routes.First();

        try {
            if (circuitBreakers[primaryRoute].State == CircuitState.Open) {
                UseFallbackRoute(message, routes.Skip(1));
            } else {
                base.RouteMessage(message);
            }
        } catch (Exception ex) {
            HandleRoutingFailure(message, ex);
        }
    }
}
```

---

## Failure Detection Algorithms

### Statistical Anomaly Detection

#### Moving Average Detection
```csharp
public class AnomalyDetector {
    private readonly MovingAverage latencyAvg;
    private readonly StandardDeviation latencyStdDev;
    private readonly float zScoreThreshold = 3.0f;

    public bool IsAnomaly(float currentLatency) {
        float mean = latencyAvg.Value;
        float stdDev = latencyStdDev.Value;
        float zScore = Math.Abs((currentLatency - mean) / stdDev);
        return zScore > zScoreThreshold;
    }
}
```

### Pattern-Based Detection

#### Failure Pattern Recognition
```csharp
public class FailurePatternDetector {
    private readonly List<FailurePattern> knownPatterns;

    public FailurePattern DetectPattern(List<HealthEvent> events) {
        foreach (var pattern in knownPatterns) {
            if (pattern.Matches(events)) {
                return pattern;
            }
        }
        return null;
    }
}

public class FailurePattern {
    public string Name { get; set; }
    public Func<List<HealthEvent>, bool> Matcher { get; set; }
    public Action<MmRelayNode> RecoveryStrategy { get; set; }
}
```

---

## Recovery Strategies

### Fallback Routing

#### Alternative Route Discovery
```csharp
public class FallbackRouter {
    private readonly Dictionary<string, List<string>> alternativeRoutes;

    public string FindAlternativeRoute(string failedRoute) {
        if (alternativeRoutes.ContainsKey(failedRoute)) {
            return alternativeRoutes[failedRoute]
                .FirstOrDefault(r => IsHealthy(r));
        }

        // Dynamic route discovery
        return DiscoverNewRoute(failedRoute);
    }

    private string DiscoverNewRoute(string failedRoute) {
        // BFS/DFS to find alternative paths
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        // ... path finding algorithm
    }
}
```

### State Preservation

#### Checkpoint System
```csharp
public class StateCheckpoint {
    private readonly Dictionary<string, object> state;
    private readonly DateTime timestamp;

    public void Restore(MmRelayNode node) {
        foreach (var kvp in state) {
            node.SetState(kvp.Key, kvp.Value);
        }
    }
}

public class CheckpointManager {
    private readonly CircularBuffer<StateCheckpoint> checkpoints;
    private readonly TimeSpan checkpointInterval = TimeSpan.FromSeconds(30);

    public StateCheckpoint GetLastHealthyCheckpoint() {
        return checkpoints
            .Reverse()
            .FirstOrDefault(c => c.IsHealthy);
    }
}
```

---

## Message Queue Management

### Priority Queue Implementation
```csharp
public class PriorityMessageQueue {
    private readonly SortedDictionary<int, Queue<MmMessage>> queues;

    public void Enqueue(MmMessage message, int priority) {
        if (!queues.ContainsKey(priority)) {
            queues[priority] = new Queue<MmMessage>();
        }
        queues[priority].Enqueue(message);
    }

    public MmMessage Dequeue() {
        foreach (var kvp in queues.Reverse()) {
            if (kvp.Value.Count > 0) {
                return kvp.Value.Dequeue();
            }
        }
        return null;
    }
}
```

### Backpressure Management
```csharp
public class BackpressureController {
    private readonly int maxQueueSize = 1000;
    private readonly float pressureThreshold = 0.8f;

    public BackpressureResponse CheckPressure(int currentQueueSize) {
        float utilization = currentQueueSize / (float)maxQueueSize;

        if (utilization > pressureThreshold) {
            return new BackpressureResponse {
                ShouldThrottle = true,
                ReductionFactor = 1f - utilization
            };
        }

        return BackpressureResponse.None;
    }
}
```

---

## Graceful Degradation Strategies

### Service Level Reduction
```csharp
public class DegradationController {
    private readonly List<DegradationLevel> levels;
    private int currentLevel = 0;

    public void Degrade() {
        if (currentLevel < levels.Count - 1) {
            currentLevel++;
            ApplyDegradationLevel(levels[currentLevel]);
        }
    }

    public void Recover() {
        if (currentLevel > 0) {
            currentLevel--;
            ApplyDegradationLevel(levels[currentLevel]);
        }
    }
}

public class DegradationLevel {
    public string Name { get; set; }
    public float MessageRateLimit { get; set; }
    public HashSet<MmMethod> DisabledMethods { get; set; }
    public int MaxHierarchyDepth { get; set; }
}
```

---

## Diagnostic System

### Failure Diagnostics
```csharp
public class DiagnosticCollector {
    private readonly CircularBuffer<DiagnosticEvent> events;

    public DiagnosticReport GenerateReport(DateTime from, DateTime to) {
        var relevantEvents = events
            .Where(e => e.Timestamp >= from && e.Timestamp <= to);

        return new DiagnosticReport {
            FailureCount = relevantEvents.Count(e => e.Type == EventType.Failure),
            RecoveryCount = relevantEvents.Count(e => e.Type == EventType.Recovery),
            AverageRecoveryTime = CalculateAverageRecoveryTime(relevantEvents),
            FailurePatterns = IdentifyPatterns(relevantEvents)
        };
    }
}
```

### Performance Metrics
```csharp
public class ResilienceMetrics {
    public float MeanTimeBetweenFailures { get; set; }
    public float MeanTimeToRecovery { get; set; }
    public float AvailabilityPercentage { get; set; }
    public int CircuitBreakerTrips { get; set; }
    public int FallbacksUsed { get; set; }
}
```

---

## Testing Approaches

### Chaos Engineering
```csharp
public class ChaosMonkey {
    private readonly Random random = new Random();
    private readonly float failureProbability = 0.1f;

    public void InjectFailure(MmRelayNode node) {
        if (random.NextDouble() < failureProbability) {
            var failureType = (FailureType)random.Next(0, 4);

            switch (failureType) {
                case FailureType.NodeFailure:
                    SimulateNodeFailure(node);
                    break;
                case FailureType.NetworkPartition:
                    SimulateNetworkPartition(node);
                    break;
                case FailureType.HighLatency:
                    SimulateHighLatency(node);
                    break;
                case FailureType.ResourceExhaustion:
                    SimulateResourceExhaustion(node);
                    break;
            }
        }
    }
}
```

---

## Performance Considerations

### Memory Management
- Bounded buffers for event history
- Checkpoint pruning strategies
- Message queue size limits

### CPU Optimization
- Async health checks
- Batch processing for diagnostics
- Lazy evaluation of alternative routes

### Threading
- Lock-free circuit breaker implementation
- Thread-safe health score calculation
- Concurrent queue operations

---

## Configuration Parameters

```csharp
public class ResilienceConfig {
    // Circuit Breaker
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerTimeout { get; set; } = TimeSpan.FromSeconds(60);
    public int HalfOpenTestLimit { get; set; } = 3;

    // Health Monitoring
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(10);
    public int HealthHistorySize { get; set; } = 100;
    public float HealthThreshold { get; set; } = 0.7f;

    // Recovery
    public TimeSpan RecoveryTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    // Degradation
    public bool EnableGracefulDegradation { get; set; } = true;
    public int DegradationLevels { get; set; } = 3;
    public float MinimumServiceLevel { get; set; } = 0.3f;
}
```

---

## Implementation Dependencies

- `System.Collections.Concurrent` for thread-safe collections
- Unity Profiler for performance monitoring
- MercuryMessaging core (MmRelayNode, MmMessage)

---

*This context document provides the technical foundation for implementing robust error recovery and graceful degradation in the MercuryMessaging framework.*