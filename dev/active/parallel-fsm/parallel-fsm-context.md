# Parallel Hierarchical State Machines - Technical Context

*Last Updated: 2025-11-20*

## Overview

This document provides technical context for implementing parallel hierarchical state machines in the MercuryMessaging framework, based on established FSM theory and concurrent programming patterns.

---

## Theoretical Foundation

### Harel's Statecharts (1987)

David Harel's Statecharts introduced orthogonal regions as a fundamental concept for parallel state machines. Key principles:

1. **Orthogonality**: Independent state machines executing concurrently
2. **Hierarchy**: States containing substates
3. **Broadcasting**: Communication between orthogonal components

### SCXML Standard

The W3C State Chart XML (SCXML) provides a standard for parallel state machines:

```xml
<parallel id="RootParallel">
    <state id="Region1">
        <!-- States for Region 1 -->
    </state>
    <state id="Region2">
        <!-- States for Region 2 -->
    </state>
</parallel>
```

---

## MercuryMessaging Integration

### Base Classes

#### Extending MmRelaySwitchNode
```csharp
public class ParallelFSM : MmRelaySwitchNode {
    private Dictionary<string, StateMachineRegion> regions;
    private MessageSynchronizer synchronizer;

    protected override void MmInvoke(MmMessage message) {
        // Distribute message to relevant regions
        foreach (var region in GetTargetRegions(message)) {
            region.ProcessMessage(message);
        }

        // Check for cross-region synchronization
        synchronizer.CheckSynchronization();
    }
}
```

### State Machine Region Implementation

```csharp
public class StateMachineRegion {
    private State currentState;
    private Dictionary<string, State> states;
    private Queue<MmMessage> messageQueue;
    private readonly object stateLock = new object();

    public void ProcessMessage(MmMessage message) {
        lock (stateLock) {
            var transition = currentState.GetTransition(message);
            if (transition != null && transition.Guard()) {
                ExecuteTransition(transition);
            }
        }
    }

    private void ExecuteTransition(Transition transition) {
        currentState.OnExit();
        transition.Execute();
        currentState = transition.TargetState;
        currentState.OnEntry();
    }
}
```

---

## Parallel Execution Model

### Threading Architecture

```csharp
public class ParallelExecutor {
    private readonly TaskScheduler scheduler;
    private readonly ConcurrentDictionary<string, Task> regionTasks;

    public async Task ExecuteRegionsAsync(List<StateMachineRegion> regions) {
        var tasks = regions.Select(region => Task.Run(() => {
            region.Update();
        }, CancellationToken.None, TaskCreationOptions.None, scheduler));

        await Task.WhenAll(tasks);
    }
}
```

### Message Ordering Guarantees

```csharp
public class MessageSequencer {
    private long sequenceNumber = 0;
    private readonly SortedDictionary<long, MmMessage> pendingMessages;

    public MmMessage[] GetOrderedMessages() {
        lock (pendingMessages) {
            var ready = new List<MmMessage>();
            while (pendingMessages.Count > 0 &&
                   pendingMessages.First().Key == sequenceNumber + 1) {
                ready.Add(pendingMessages.First().Value);
                pendingMessages.Remove(pendingMessages.First().Key);
                sequenceNumber++;
            }
            return ready.ToArray();
        }
    }
}
```

---

## Conflict Resolution Mechanisms

### Priority-Based Resolution

```csharp
public class PriorityResolver : IConflictResolver {
    public Resolution Resolve(List<StateTransition> conflicts) {
        // Sort by priority
        var sorted = conflicts.OrderByDescending(c => c.Priority);
        var winner = sorted.First();

        return new Resolution {
            ChosenTransition = winner,
            RejectedTransitions = sorted.Skip(1).ToList()
        };
    }
}
```

### Voting Mechanism

```csharp
public class VotingResolver : IConflictResolver {
    public Resolution Resolve(List<StateTransition> conflicts) {
        var votes = new Dictionary<StateTransition, int>();

        foreach (var transition in conflicts) {
            votes[transition] = CalculateVotes(transition);
        }

        var winner = votes.OrderByDescending(kvp => kvp.Value).First();
        return new Resolution {
            ChosenTransition = winner.Key
        };
    }

    private int CalculateVotes(StateTransition transition) {
        // Voting logic based on region importance, state history, etc.
        return transition.Region.Importance +
               transition.HistoricalSuccess * 10;
    }
}
```

### Hierarchical Override

```csharp
public class HierarchicalResolver : IConflictResolver {
    public Resolution Resolve(List<StateTransition> conflicts) {
        // Parent regions override child regions
        var byHierarchy = conflicts.OrderBy(c => c.Region.HierarchyLevel);
        return new Resolution {
            ChosenTransition = byHierarchy.First()
        };
    }
}
```

---

## State Synchronization

### Cross-Region Communication

```csharp
public class CrossRegionSync {
    private readonly Dictionary<string, List<string>> dependencies;
    private readonly Dictionary<string, RegionState> regionStates;

    public void RegisterDependency(string region1, string region2) {
        if (!dependencies.ContainsKey(region1)) {
            dependencies[region1] = new List<string>();
        }
        dependencies[region1].Add(region2);
    }

    public bool CanTransition(string region, string newState) {
        if (!dependencies.ContainsKey(region)) {
            return true; // No dependencies
        }

        foreach (var dep in dependencies[region]) {
            if (!IsCompatibleState(regionStates[dep], newState)) {
                return false;
            }
        }
        return true;
    }
}
```

### Synchronization Barriers

```csharp
public class SyncBarrier {
    private readonly int regionCount;
    private int arrivedCount = 0;
    private readonly ManualResetEventSlim barrier;

    public void SignalAndWait(string regionId) {
        Interlocked.Increment(ref arrivedCount);

        if (arrivedCount == regionCount) {
            barrier.Set();
            Interlocked.Exchange(ref arrivedCount, 0);
            barrier.Reset();
        } else {
            barrier.Wait();
        }
    }
}
```

---

## State Definition Patterns

### Basic State Structure

```csharp
public abstract class State {
    public string Name { get; set; }
    public StateMachineRegion Region { get; set; }

    public virtual void OnEntry() { }
    public virtual void OnExit() { }
    public virtual void Update() { }

    public abstract Transition GetTransition(MmMessage message);
}
```

### Composite State

```csharp
public class CompositeState : State {
    private StateMachine subMachine;

    public override void OnEntry() {
        subMachine.Start();
    }

    public override void Update() {
        subMachine.Update();
    }

    public override void OnExit() {
        subMachine.Stop();
    }
}
```

### Parallel State

```csharp
public class ParallelState : State {
    private List<StateMachineRegion> subRegions;

    public override void Update() {
        Parallel.ForEach(subRegions, region => {
            region.Update();
        });
    }
}
```

---

## Transition System

### Transition Definition

```csharp
public class Transition {
    public State SourceState { get; set; }
    public State TargetState { get; set; }
    public Func<bool> Guard { get; set; }
    public Action Action { get; set; }
    public MmMethod Trigger { get; set; }

    public bool CanExecute(MmMessage message) {
        return message.method == Trigger && (Guard == null || Guard());
    }

    public void Execute() {
        Action?.Invoke();
    }
}
```

### Guard Conditions

```csharp
public class GuardCondition {
    public static Func<bool> And(params Func<bool>[] conditions) {
        return () => conditions.All(c => c());
    }

    public static Func<bool> Or(params Func<bool>[] conditions) {
        return () => conditions.Any(c => c());
    }

    public static Func<bool> Not(Func<bool> condition) {
        return () => !condition();
    }
}
```

---

## Performance Optimization

### State Caching

```csharp
public class StateCache {
    private readonly LRUCache<string, State> cache;
    private readonly int maxCacheSize = 100;

    public State GetOrCreate(string stateId, Func<State> factory) {
        if (cache.TryGet(stateId, out State state)) {
            return state;
        }

        state = factory();
        cache.Add(stateId, state);
        return state;
    }
}
```

### Message Batching

```csharp
public class MessageBatcher {
    private readonly Dictionary<string, List<MmMessage>> batches;
    private readonly int batchSize = 10;
    private readonly TimeSpan batchWindow = TimeSpan.FromMilliseconds(10);

    public void ProcessBatch(string regionId, List<MmMessage> messages) {
        // Process all messages in a single state machine update
        var region = GetRegion(regionId);
        region.ProcessMessageBatch(messages);
    }
}
```

---

## Testing Support

### State Machine Verification

```csharp
public class StateMachineVerifier {
    public bool VerifyNoDeadlocks(StateMachine machine) {
        var visited = new HashSet<State>();
        var stack = new Stack<State>();

        return !HasCycle(machine.InitialState, visited, stack);
    }

    private bool HasCycle(State state, HashSet<State> visited, Stack<State> stack) {
        visited.Add(state);
        stack.Push(state);

        foreach (var transition in state.Transitions) {
            if (stack.Contains(transition.TargetState)) {
                return true; // Cycle detected
            }
            if (!visited.Contains(transition.TargetState)) {
                if (HasCycle(transition.TargetState, visited, stack)) {
                    return true;
                }
            }
        }

        stack.Pop();
        return false;
    }
}
```

### Test Harness

```csharp
public class ParallelFSMTestHarness {
    private ParallelFSM fsm;
    private List<StateTransitionLog> transitionLog;

    public void SimulateMessages(List<MmMessage> messages) {
        foreach (var message in messages) {
            fsm.ProcessMessage(message);
            LogTransitions();
        }
    }

    public bool VerifyParallelExecution() {
        // Check that regions executed concurrently
        var regionExecutions = transitionLog
            .GroupBy(l => l.RegionId)
            .Select(g => new {
                Region = g.Key,
                StartTime = g.Min(l => l.Timestamp),
                EndTime = g.Max(l => l.Timestamp)
            });

        // Verify overlap in execution times
        return regionExecutions.Any(r1 =>
            regionExecutions.Any(r2 =>
                r1.Region != r2.Region &&
                r1.StartTime < r2.EndTime &&
                r2.StartTime < r1.EndTime));
    }
}
```

---

## Configuration

```csharp
public class ParallelFSMConfiguration {
    // Threading
    public int MaxConcurrentRegions { get; set; } = 10;
    public TaskScheduler Scheduler { get; set; } = TaskScheduler.Default;

    // Conflict Resolution
    public IConflictResolver ConflictResolver { get; set; } = new PriorityResolver();
    public TimeSpan ConflictTimeout { get; set; } = TimeSpan.FromMilliseconds(100);

    // Synchronization
    public bool RequireStrictOrdering { get; set; } = false;
    public int MessageQueueSize { get; set; } = 1000;

    // Performance
    public bool EnableStateCaching { get; set; } = true;
    public bool EnableMessageBatching { get; set; } = false;
    public int BatchSize { get; set; } = 10;

    // Debugging
    public bool EnableTransitionLogging { get; set; } = false;
    public bool EnableDeadlockDetection { get; set; } = true;
}
```

---

## Unity Integration Considerations

### MonoBehaviour Lifecycle

```csharp
public class ParallelFSMBehaviour : MonoBehaviour {
    private ParallelFSM fsm;

    void Start() {
        fsm = GetComponent<ParallelFSM>();
        fsm.Initialize();
    }

    void Update() {
        // Unity main thread update
        fsm.Update(Time.deltaTime);
    }

    void OnDestroy() {
        fsm.Shutdown();
    }
}
```

### Unity Job System Integration

```csharp
public struct FSMUpdateJob : IJobParallelFor {
    [ReadOnly] public NativeArray<RegionData> regions;
    public NativeArray<RegionResult> results;

    public void Execute(int index) {
        var region = regions[index];
        // Process state machine logic
        results[index] = ProcessRegion(region);
    }
}
```

---

*This context document provides the technical foundation for implementing parallel hierarchical state machines in the MercuryMessaging framework.*