# Static Analysis Implementation Plan

## Executive Summary

Implement a comprehensive static analysis and safety verification system for MercuryMessaging that combines compile-time analysis with runtime checks to prevent routing failures and ensure type safety.

## Architecture Design

### System Components

```
┌─────────────────────────────────────────────────┐
│             Static Analysis Layer               │
├─────────────────────────────────────────────────┤
│   Editor-Time          │    Runtime             │
│   ┌────────────┐       │    ┌────────────┐     │
│   │  Tarjan    │       │    │   Bloom    │     │
│   │    SCC     │       │    │   Filter   │     │
│   └────────────┘       │    └────────────┘     │
│   ┌────────────┐       │    ┌────────────┐     │
│   │   Type     │       │    │    Type    │     │
│   │  Analyzer  │       │    │ Validator  │     │
│   └────────────┘       │    └────────────┘     │
└─────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────┐
│          MercuryMessaging Core                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │MmRelayNode│  │MmMessage │  │MmRouting │     │
│  └──────────┘  └──────────┘  └──────────┘     │
└─────────────────────────────────────────────────┘
```

### Data Flow

1. **Edit-Time**: Hierarchy changes → Tarjan analysis → Cycle warnings
2. **Compile-Time**: Message definitions → Type analysis → Constraint validation
3. **Runtime**: Message dispatch → Bloom check → Type verify → Route

## Phase 1: Tarjan's Algorithm Implementation

### Core Algorithm

```csharp
public class TarjanSCC {
    private int index = 0;
    private Stack<GameObject> stack = new Stack<GameObject>();
    private Dictionary<GameObject, TarjanNode> nodes;

    public List<List<GameObject>> FindSCCs(GameObject root) {
        nodes = new Dictionary<GameObject, TarjanNode>();
        var sccs = new List<List<GameObject>>();

        // Build graph from Unity hierarchy
        BuildHierarchyGraph(root);

        // Run Tarjan's algorithm
        foreach (var node in nodes.Keys) {
            if (nodes[node].Index == -1) {
                StrongConnect(node, sccs);
            }
        }

        return sccs;
    }

    private void StrongConnect(GameObject v, List<List<GameObject>> sccs) {
        var vNode = nodes[v];
        vNode.Index = vNode.LowLink = index++;
        stack.Push(v);
        vNode.OnStack = true;

        // Check all relay connections
        var relay = v.GetComponent<MmRelayNode>();
        if (relay != null) {
            foreach (var parent in relay.MmParents) {
                if (!nodes.ContainsKey(parent.gameObject))
                    continue;

                var wNode = nodes[parent.gameObject];
                if (wNode.Index == -1) {
                    StrongConnect(parent.gameObject, sccs);
                    vNode.LowLink = Math.Min(vNode.LowLink, wNode.LowLink);
                } else if (wNode.OnStack) {
                    vNode.LowLink = Math.Min(vNode.LowLink, wNode.Index);
                }
            }
        }

        // Found SCC root?
        if (vNode.LowLink == vNode.Index) {
            var scc = new List<GameObject>();
            GameObject w;
            do {
                w = stack.Pop();
                nodes[w].OnStack = false;
                scc.Add(w);
            } while (w != v);

            if (scc.Count > 1) // Cycle detected
                sccs.Add(scc);
        }
    }
}
```

### Editor Integration

```csharp
[InitializeOnLoad]
public class MmHierarchyValidator {
    static MmHierarchyValidator() {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged() {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // Debounce validation
        EditorApplication.delayCall += ValidateHierarchy;
    }

    private static void ValidateHierarchy() {
        var analyzer = new TarjanSCC();
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var root in roots) {
            var sccs = analyzer.FindSCCs(root);
            foreach (var scc in sccs) {
                LogCycle(scc);
                HighlightCycleInHierarchy(scc);
            }
        }
    }
}
```

## Phase 2: Bloom Filter Runtime Safety

### Optimized Bloom Filter

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct MmBloomFilter {
    private const int NUM_HASHES = 3;
    private ulong bits1, bits2; // 128-bit filter

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TestAndAdd(int nodeId) {
        // MurmurHash3-inspired mixing
        uint h1 = (uint)nodeId;
        uint h2 = h1 * 0xcc9e2d51;
        uint h3 = h1 * 0x1b873593;

        // Calculate bit positions
        int pos1 = (int)(h1 & 0x3F);
        int pos2 = (int)(h2 & 0x3F);
        int pos3 = (int)(h3 & 0x3F);

        // Check if already set (all bits must be 1)
        ulong mask1 = (1UL << pos1) | (1UL << pos2);
        ulong mask2 = (1UL << pos3);

        bool exists = ((bits1 & mask1) == mask1) &&
                      ((bits2 & mask2) == mask2);

        // Add to filter
        bits1 |= mask1;
        bits2 |= mask2;

        return exists;
    }

    public void Reset() {
        bits1 = bits2 = 0;
    }
}
```

### Integration with MmRelayNode

```csharp
public partial class MmRelayNode {
    // Add to message structure
    public struct MessageContext {
        public MmBloomFilter VisitedFilter;
        public int PropagationDepth;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckAndAddNode(int nodeId) {
            // Check Bloom filter first (fast path)
            if (VisitedFilter.TestAndAdd(nodeId))
                return true; // Possibly visited (stop)

            // Check depth limit
            return ++PropagationDepth > MaxPropagationDepth;
        }
    }

    protected override void MmInvokeInternal(MmMessage message) {
        // Safety check using Bloom filter
        if (message.Context.CheckAndAddNode(GetInstanceID())) {
            if (MmLogger.safetyViolations)
                Debug.LogWarning($"Potential cycle detected at {name}");
            return;
        }

        // Continue normal processing
        base.MmInvokeInternal(message);
    }
}
```

## Phase 3: Type Safety Verification

### Compile-Time Constraints

```csharp
// Attribute for message constraints
[AttributeUsage(AttributeTargets.Class)]
public class MmMessageConstraintAttribute : Attribute {
    public Type RequiredInterface { get; }
    public bool AllowDerivedTypes { get; }

    public MmMessageConstraintAttribute(Type requiredInterface,
                                        bool allowDerived = true) {
        RequiredInterface = requiredInterface;
        AllowDerivedTypes = allowDerived;
    }
}

// Roslyn analyzer for compile-time checking
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MmTypeAnalyzer : DiagnosticAnalyzer {
    public override void Analyze(SyntaxNodeAnalysisContext context) {
        // Check MmInvoke calls for type compatibility
        if (context.Node is InvocationExpressionSyntax invocation) {
            var symbol = context.SemanticModel.GetSymbolInfo(invocation);
            if (IsMmInvokeMethod(symbol.Symbol)) {
                ValidateMessageTypes(context, invocation);
            }
        }
    }

    private void ValidateMessageTypes(SyntaxNodeAnalysisContext context,
                                      InvocationExpressionSyntax invocation) {
        // Extract message type and receiver type
        var messageType = GetMessageType(invocation);
        var receiverType = GetReceiverType(context);

        // Check constraints
        var constraints = messageType.GetCustomAttributes<MmMessageConstraintAttribute>();
        foreach (var constraint in constraints) {
            if (!IsCompatible(receiverType, constraint)) {
                context.ReportDiagnostic(Diagnostic.Create(
                    TypeMismatchRule,
                    invocation.GetLocation(),
                    messageType.Name,
                    receiverType.Name
                ));
            }
        }
    }
}
```

### Runtime Type Validation

```csharp
public abstract class MmTypeSafeMessage : MmMessage {
    private static readonly Dictionary<(Type, Type), bool> typeCache =
        new Dictionary<(Type, Type), bool>();

    public override bool ValidateReceiver(IMmResponder responder) {
        var key = (GetType(), responder.GetType());

        // Fast path: cached result
        if (typeCache.TryGetValue(key, out bool cached))
            return cached;

        // Slow path: validate and cache
        bool isValid = ValidateTypeCompatibility(responder);
        typeCache[key] = isValid;
        return isValid;
    }

    protected virtual bool ValidateTypeCompatibility(IMmResponder responder) {
        // Check if responder implements required interface
        var requiredInterface = typeof(IMmMessageHandler<>)
            .MakeGenericType(GetType());
        return requiredInterface.IsAssignableFrom(responder.GetType());
    }
}
```

## Phase 4: Performance Optimization

### Benchmark Suite

```csharp
[TestFixture]
public class SafetyPerformanceTests {
    [Test]
    public void BloomFilterPerformance() {
        var filter = new MmBloomFilter();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 1_000_000; i++) {
            filter.TestAndAdd(i);
        }

        sw.Stop();
        var nsPerOp = sw.ElapsedTicks * 1_000_000_000L /
                      (Stopwatch.Frequency * 1_000_000);

        Assert.Less(nsPerOp, 50, "Bloom filter should be <50ns per op");
    }

    [Test]
    public void TypeValidationPerformance() {
        var message = new TestMessage();
        var responder = new TestResponder();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 1_000_000; i++) {
            message.ValidateReceiver(responder);
        }

        sw.Stop();
        var nsPerOp = sw.ElapsedTicks * 1_000_000_000L /
                      (Stopwatch.Frequency * 1_000_000);

        Assert.Less(nsPerOp, 150, "Type validation should be <150ns per op");
    }
}
```

## Deliverables

1. **Core Implementation**
   - TarjanSCC.cs - Cycle detection algorithm
   - MmBloomFilter.cs - Optimized Bloom filter
   - MmTypeAnalyzer.cs - Roslyn analyzer
   - MmSafetyValidator.cs - Runtime validation

2. **Editor Tools**
   - Hierarchy validation window
   - Cycle visualization in scene view
   - Type mismatch warnings

3. **Documentation**
   - API reference for safety attributes
   - Performance tuning guide
   - Migration guide for existing projects

4. **Testing**
   - Unit tests for algorithms
   - Integration tests with Mercury
   - Performance benchmark suite
   - User study materials

## Timeline

- **Week 1-2**: Tarjan's algorithm and editor integration
- **Week 3-4**: Bloom filter implementation and optimization
- **Week 5-6**: Type safety system and Roslyn analyzer
- **Week 7**: Performance optimization and benchmarking
- **Week 8**: User study and evaluation

## Risk Mitigation

1. **Performance Regression**
   - Mitigation: Conditional compilation for safety checks
   - Fallback: Make verification opt-in for development

2. **False Positives**
   - Mitigation: Tunable Bloom filter parameters
   - Fallback: Exact tracking for small hierarchies

3. **Breaking Changes**
   - Mitigation: Backward compatibility layer
   - Fallback: Legacy mode without verification