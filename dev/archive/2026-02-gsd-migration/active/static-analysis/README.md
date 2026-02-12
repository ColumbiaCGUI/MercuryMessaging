# Static Analysis and Hybrid Safety Verification Layer

## Overview

This research contribution introduces a novel hybrid safety verification system for the MercuryMessaging framework that combines compile-time static analysis with runtime safety checks to prevent message routing failures, infinite loops, and type mismatches in hierarchical message-passing systems.

**For business context and use cases, see [`USE_CASE.md`](./USE_CASE.md)**

## Research Contribution (UIST Major Contribution III)

### Problem Statement

Hierarchical message-passing systems face critical safety challenges:
- **Cycle Detection**: Circular references in GameObject hierarchies can cause infinite message loops
- **Type Safety**: Message type mismatches between senders and receivers cause runtime failures
- **Performance Overhead**: Runtime safety checks must be minimal (<200ns per message)
- **Developer Experience**: Safety violations should be caught at edit-time when possible

### Novel Technical Approach

Our hybrid verification layer combines three complementary techniques:

1. **Tarjan's Algorithm for Static Cycle Detection**
   - Strongly connected component analysis at editor-time
   - O(V+E) complexity for hierarchy validation
   - Prevents circular parent-child relationships

2. **Bloom Filter-Based Runtime Loop Prevention**
   - Probabilistic O(1) cycle detection during message routing
   - Space-efficient visited node tracking (128-bit filters)
   - False positive rate <0.001% with proper sizing

3. **Type Safety Handshake Protocol**
   - Compile-time message type verification
   - Runtime type compatibility checking
   - Generic constraint propagation for custom message types

## Technical Innovation

### 1. Static Analysis Engine

```csharp
// Editor-time hierarchy validation using Tarjan's algorithm
public class MmStaticAnalyzer : EditorWindow {
    private TarjanSCC tarjanAnalyzer;

    [MenuItem("Mercury/Static Analysis/Validate Hierarchy")]
    public static void ValidateHierarchy() {
        // Find strongly connected components
        var sccs = tarjanAnalyzer.FindSCCs(GetHierarchyGraph());

        // Report cycles larger than size 1
        foreach (var scc in sccs.Where(s => s.Count > 1)) {
            EditorUtility.DisplayDialog("Cycle Detected",
                $"Circular dependency: {string.Join(" -> ", scc)}", "OK");
        }
    }
}
```

### 2. Bloom Filter Implementation

```csharp
public struct MmBloomFilter {
    private ulong hash1, hash2; // 128-bit filter

    public bool AddAndCheck(int nodeId) {
        ulong mask1 = 1UL << (nodeId & 63);
        ulong mask2 = 1UL << ((nodeId >> 6) & 63);

        bool exists = (hash1 & mask1) != 0 && (hash2 & mask2) != 0;
        hash1 |= mask1;
        hash2 |= mask2;

        return exists; // True if possibly visited before
    }
}
```

### 3. Type Safety Verification

```csharp
[MmMessageConstraint(typeof(ISerializable))]
public class SafeMessage<T> : MmMessage where T : ISerializable {
    public T Payload { get; set; }

    public override bool ValidateType(Type receiverType) {
        return receiverType.GetInterfaces().Contains(typeof(IMmReceiver<T>));
    }
}
```

## Evaluation Methodology

### Performance Benchmarks

Target performance metrics:
- **Static Analysis**: <100ms for 1000-node hierarchies
- **Bloom Filter Check**: <50ns per message
- **Type Validation**: <150ns per message
- **Combined Overhead**: <200ns total per message

### Safety Guarantees

Formal verification properties:
- **P1**: No infinite loops in acyclic hierarchies
- **P2**: Type mismatches caught before runtime
- **P3**: Bounded message propagation depth
- **P4**: Deterministic routing behavior

### User Study Design

Developer productivity study (N=20):
- **Control**: Mercury without safety verification
- **Treatment**: Mercury with hybrid verification layer
- **Metrics**:
  - Debug time for routing errors
  - Number of runtime failures
  - Perceived confidence in system behavior
- **Hypothesis**: 50% reduction in debugging time for routing-related errors

## Implementation Plan

### Phase 1: Static Analysis Infrastructure (80 hours)
- Implement Tarjan's SCC algorithm for Unity hierarchy
- Create editor window for validation UI
- Integrate with Unity's scene dirty tracking
- Add visual indicators for detected cycles

### Phase 2: Bloom Filter Runtime (60 hours)
- Implement space-efficient Bloom filter
- Integrate with MmRelayNode message routing
- Add performance monitoring and metrics
- Create fallback to exact tracking for small graphs

### Phase 3: Type Safety System (100 hours)
- Design constraint attribute system
- Implement compile-time type checking
- Create runtime validation layer
- Add generic type inference for custom messages

### Phase 4: Evaluation and Testing (40 hours)
- Performance benchmark suite
- Safety property verification
- User study protocol
- Statistical analysis

## Research Impact

### Novelty
- First hybrid static-runtime verification for hierarchical message systems
- Novel application of Bloom filters to game engine architectures
- Formal safety properties for message-passing frameworks

### Significance
- Reduces debugging time by 50% (hypothesis)
- Prevents entire class of runtime failures
- Enables confident refactoring of large hierarchies
- Minimal performance impact (<200ns)

### Comparison with Related Work
- Unity SendMessage: No safety guarantees, no cycle detection
- UnityEvents: Type-safe but no hierarchy validation
- Photon RPC: Network-only, no local hierarchy support
- Our approach: Comprehensive safety with minimal overhead

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| Go Verification | 2020 | PLACES | Behavioral type checking for Go | Go-specific concurrency patterns | Unity/C# game engine focus |
| NVLang | 2025 | arXiv | BEAM/Erlang actor verification | BEAM VM specific, not game engines | Unity scene graph integration |
| CAMP | 2020 | OOPSLA | Deadlock detection for distributed systems | Distributed systems focus | Local hierarchical messages |
| PARCOACH | 2020 | Correctness | MPI collective verification | HPC/MPI specific | Game development patterns |
| Unity Static Analysis | 2023 | Various | General C# code analysis | No message routing focus | Message-specific safety |

### Literature Gap Analysis

**What exists:**
- Static verification for actor systems (Go, Erlang) - different paradigm
- Deadlock detection for distributed systems (CAMP) - network focus
- MPI collective verification (PARCOACH) - HPC specific
- General C# static analysis - no message routing awareness

**What doesn't exist:**
- Static verification for **Unity scene graph hierarchies**
- Bloom filter-based cycle detection for **game engine message routing**
- Hybrid static-runtime verification for **hierarchical message systems**
- Type safety verification integrated with **MercuryMessaging patterns**

### Novelty Claims

1. **Novel application** of Tarjan's SCC to Unity scene graph hierarchy validation
2. **FIRST** Bloom filter-based runtime loop prevention in game engine routing (<50ns)
3. **Hybrid static-runtime** verification combining compile-time and runtime checks
4. **Game-engine-specific** safety properties (vs distributed/HPC systems)
5. **Integration** with existing MercuryMessaging infrastructure

### Key Citations

```bibtex
@inproceedings{go_verification2020,
  title={Behavioral Type Checking for Go},
  booktitle={Workshop on Programming Language Approaches to Concurrency and Communication-centric Software (PLACES)},
  year={2020}
}

@article{nvlang2025,
  title={NVLang: Type-Safe Session Types for the BEAM},
  journal={arXiv preprint},
  year={2025}
}

@inproceedings{camp2020,
  title={CAMP: Cost-Aware Multiparty Protocol Verification},
  booktitle={OOPSLA},
  year={2020}
}

@inproceedings{parcoach2020,
  title={PARCOACH: Detecting MPI Collective Errors at Compile-Time},
  booktitle={Workshop on Correctness in High-Performance Computing},
  year={2020}
}
```

### Research Impact

**Target Venues:** ICSE (primary), PLDI, OOPSLA

**Novelty Assessment:** MEDIUM
- Static verification for message systems is well-studied in distributed systems
- **Novel contribution** is application to Unity/game engine context
- Bloom filter for game engine routing is novel implementation
- Hybrid static-runtime approach provides differentiation

**Risk Assessment:**
- Well-studied area requires demonstrating game-engine-specific contributions
- Must show benefits beyond applying known techniques to new domain
- User study critical to validate productivity improvements

**Mitigation Strategy:**
- Focus on **Unity scene graph** specific challenges
- Emphasize **performance constraints** (<200ns) unique to game engines
- Highlight **developer experience** improvements over existing tools

---

## Key Publications to Reference

- Tarjan, R. (1972). "Depth-first search and linear graph algorithms"
- Bloom, B. H. (1970). "Space/time trade-offs in hash coding"
- Liskov, B. & Wing, J. (1994). "Behavioral subtyping"
- Unity Technologies (2023). "Performance best practices"

## Acceptance Criteria

- [ ] Static analysis catches 100% of hierarchy cycles
- [ ] Bloom filter false positive rate <0.001%
- [ ] Type validation prevents all type mismatches
- [ ] Combined overhead <200ns per message
- [ ] Editor integration with real-time feedback
- [ ] Comprehensive test coverage (>90%)
- [ ] User study shows 50% debugging reduction

## Files in This Folder

- `README.md` - This overview document
- `static-analysis-plan.md` - Detailed implementation plan
- `static-analysis-context.md` - Technical context and algorithms
- `static-analysis-tasks.md` - Specific development tasks