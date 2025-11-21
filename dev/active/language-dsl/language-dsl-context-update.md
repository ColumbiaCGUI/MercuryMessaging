# Language DSL Context Update

**Last Updated**: 2025-11-21
**Status**: Documentation complete, implementation not started

## Current Implementation State

### ✅ Documentation Phase Complete
All four core documentation files have been created and are comprehensive:
- README.md: 150+ lines, research-oriented DSL overview
- language-dsl-plan.md: 600+ lines, detailed implementation with syntax examples
- language-dsl-context.md: 700+ lines, design rationale and philosophy
- language-dsl-tasks.md: 500+ lines, granular task breakdown

### ⏸️ Implementation Phase Not Started
No code has been written yet. This is purely a research planning and documentation task.

## Key Decisions This Session

### 1. Operator Selection: `:>` for Message Flow
**Decision**: Use `:>` as the primary message sending operator

**Rationale**:
- Visually distinctive (arrow-like appearance)
- Available for overloading in C#
- Cannot be confused with existing operators
- Suggests "sending to" or "casting to"

**Alternatives Evaluated**:
| Operator | Pros | Cons | Verdict |
|----------|------|------|---------|
| `->` | Familiar | Not available in C# | ❌ |
| `=>` | Lambda-like | Already means lambda | ❌ |
| `>>` | Stream-like | Ambiguous alone | Used for routing |
| `:>` | Unique, arrow-like | Non-standard | ✅ Selected |
| `|>` | F# pipe style | Unfamiliar | ❌ |

### 2. Zero-Cost Abstraction Principle
**Decision**: DSL must compile to equivalent code as manual API calls

**Requirements**:
- No runtime overhead (< 2% acceptable)
- Zero additional memory allocation
- Aggressive inlining for all methods
- Struct-based builders (value semantics)

**Validation**:
```csharp
// DSL version
relay :> "Hello" >> Children.Active;

// Must compile to exactly:
relay.MmInvoke(
    MmMethod.MessageString,
    new MmMessageString("Hello"),
    MmMetadataBlock.ChildrenActive
);
```

### 3. Struct-Based Builder Design
**Decision**: Use struct (not class) for MmRouteBuilder

**Rationale**:
- Avoids heap allocation
- Value semantics (immutable transformations)
- Fits in 16 bytes (cache-friendly)
- Can be inlined completely

**Performance**:
```csharp
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct MmRouteBuilder {
    private ulong flags;  // 64 bits for boolean flags
    private ulong tags;   // 64 bits for tag mask
}
```

### 4. Progressive Disclosure Syntax
**Decision**: Support 5 levels of complexity

**Levels**:
1. **Simplest**: `relay :> "Hello" >> Children;`
2. **One filter**: `relay :> "Hello" >> Children.Active;`
3. **Multiple filters**: `relay :> "Hello" >> Children.Active.Tag0.Network;`
4. **Custom predicates**: `relay :> "Hello" >> Children.Where(c => c.IsReady);`
5. **Complex**: Full spatial, temporal, type filtering

**Rationale**: Beginners can start simple, experts can use full power

### 5. Fluent Builder API Pattern
**Decision**: Use fluent interface with property chaining

**Design**:
```csharp
Children              // Target
    .Active          // Activity filter
    .Tag0            // Tag filter
    .Within(10f)     // Spatial filter
    .OfType<Enemy>() // Type filter
    .Network;        // Network filter
```

**Advantages**:
- IntelliSense-friendly
- Self-documenting
- Type-safe
- Composable

## Files Modified

### New Files Created
```
dev/active/language-dsl/README.md
dev/active/language-dsl/language-dsl-plan.md
dev/active/language-dsl/language-dsl-context.md
dev/active/language-dsl/language-dsl-tasks.md
dev/active/language-dsl/language-dsl-context-update.md (this file)
```

### No Existing Files Modified
This is a greenfield documentation task.

## Blockers and Issues

### None Currently

All documentation is complete and ready for implementation when resources are available.

## Next Immediate Steps

### For Implementation
1. Create `Assets/MercuryMessaging/Protocol/DSL/` folder
2. Implement `MmOperators.cs` with `:>` operator (Task 1.1)
3. Create `MmRouteBuilder.cs` struct (Task 2.1)
4. Implement `MmMessageContext.cs` (Task 1.2)

### For Research
1. Prepare code examples for user study
2. Design metrics collection tools
3. Recruit study participants
4. Create tutorial materials

## Integration Points Discovered

### With MmRelayNode
- Operator overloading on existing class
- Extension methods for convenience
- Backward compatibility required
- Performance parity essential

### With Type System
- Generic type inference for messages
- Implicit conversions for common types
- Variance support for handlers
- Compile-time validation via Roslyn

### With Unity Editor
- IntelliSense XML documentation
- Code snippets for common patterns
- Quick actions and refactorings
- Syntax highlighting (if possible)

## Testing Approach

### Unit Tests
- Operator functionality (correct message creation)
- Route builder (correct metadata generation)
- Type inference (correct message types)
- Edge cases (null, empty, invalid)

### Performance Tests
- DSL vs traditional API (< 2% difference)
- Memory allocation (zero for common paths)
- Compilation time (< 5% increase)
- Binary size (< 1% increase)

### User Study
- N=20 developers (10 novice, 10 experienced)
- Within-subjects design
- 10 message routing tasks
- Measure: time, errors, preference
- Hypothesis: 40% time reduction, 50% error reduction

### Code Metrics
- Line count reduction: 50-70%
- Token count reduction: 60-80%
- Cyclomatic complexity: -30%
- IntelliSense completions: 3x improvement

## Research Context

### Novelty Claims
1. First DSL for hierarchical message-passing in game engines
2. Novel use of fluent interfaces for spatial routing
3. Custom operators for visual programming metaphors
4. Zero-cost abstraction with 70% code reduction

### Comparison Points
- Unity SendMessage: Verbose, no fluent API
- UnityEvents: Complex setup, no DSL
- LINQ: Similar patterns, different domain
- Rx.NET: Reactive patterns, higher overhead

### User Study Design
- **Participants**: N=20 Unity developers
- **Duration**: 2 hours per participant
- **Tasks**: 10 message routing scenarios
- **Conditions**: Within-subjects (both APIs)
- **Metrics**: Time, errors, preference, cognitive load
- **Hypotheses**:
  - H1: 40% faster implementation time
  - H2: 50% fewer errors
  - H3: Higher subjective preference
  - H4: Lower cognitive load (NASA-TLX)

## Observations

### Syntax Design Quality
The `:>` operator creates intuitive visual flow:
```csharp
// Reads like natural language
player :> "Jump" >> Self;
// "Player sends Jump to Self"

// Arrow shows direction
source :> data >> destination;
```

### API Consistency
The fluent API follows established patterns:
- Similar to LINQ (Where, Select, etc.)
- Similar to Rx.NET (Observable chains)
- Similar to Unity's Addressables
- Familiar to C# developers

### Performance Strategy
Multiple optimization layers:
1. Struct-based builders (no allocation)
2. Aggressive inlining (compiler optimization)
3. Bit-packed fields (cache-friendly)
4. Compile-time constants (for common patterns)
5. Optional source generators (future)

### Alignment with UIST
This work aligns perfectly with UIST Major Contribution IV:
- Novel DSL design
- Strong usability evaluation
- Practical developer impact
- Zero-cost abstraction

## Temporary Workarounds

None - no implementation started yet.

## Uncommitted Changes

This is a new folder with all new files. All files should be committed together:

```bash
git add dev/active/language-dsl/
git commit -m "docs: Create language-dsl task for UIST Contribution IV

- Add comprehensive DSL overview (README.md)
- Add detailed syntax design and implementation (language-dsl-plan.md)
- Add design rationale and philosophy (language-dsl-context.md)
- Add granular task breakdown (language-dsl-tasks.md)
- 240 hours estimated effort across 4 phases
- Target: 70% code reduction with zero runtime overhead"
```

## Advanced Features Planned

### Spatial Extensions
```csharp
relay :> "Alert" >> Children
    .Within(10f)               // Sphere cast
    .InDirection(forward, 45f) // Cone cast
    .InLineOfSight()           // Raycast check
```

### Temporal Extensions
```csharp
relay :> "Tick" >> Children.Every(1.0f);    // Repeated
relay :> "Explode" >> Self.After(3.0f);     // Delayed
relay :> "Start" >> Children.When(() => GameManager.IsReady);
```

### Async/Await Support
```csharp
var response = await (relay :> "GetStatus" >> target);
```

### Type Filtering
```csharp
relay :> "Damage" >> Children
    .OfType<Enemy>()
    .Where(e => e.Health > 0)
```

## Migration Strategy

### Three-Phase Approach
1. **Phase 1**: Add DSL alongside existing code (parallel usage)
2. **Phase 2**: Migrate simple cases first (quick wins)
3. **Phase 3**: Full migration with automated tools

### Migration Tool
- Roslyn-based code analyzer
- Detects old-style MmInvoke calls
- Suggests DSL equivalents
- Provides one-click refactoring
- Generates migration report

### Backward Compatibility
- Original API remains available
- Extension methods provide DSL on existing types
- No breaking changes
- Gradual migration possible

---

**Status Summary**: Documentation complete and ready for implementation phase. No blockers identified. DSL design is well-thought-out with strong research framing.