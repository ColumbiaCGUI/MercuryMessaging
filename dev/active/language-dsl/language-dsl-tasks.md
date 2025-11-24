# Language DSL Implementation Tasks

**Last Updated:** 2025-11-24 18:00 PST

## Overview

Detailed task breakdown for implementing the Domain-Specific Language (DSL) for MercuryMessaging.

Total estimated effort: 240 hours (6 weeks)
**Actual effort to date:** ~8 hours
**Core functionality:** ✅ COMPLETE (Phase 1)

## Session Summary (2025-11-24)
- Marked Phase 1 as COMPLETE in README.md
- Consolidated HANDOFF_NOTES.md into README (implementation knowledge preserved)
- Deleted language-dsl-context-update.md (outdated)
- Archived test-fixing session that validated DSL tests

## Task Categories

- 🔴 Critical Path (blocks other work)
- 🟡 Important (needed for full functionality)
- 🟢 Nice to Have (enhancement)
- 🔬 Research/Evaluation

---

## Phase 1: Core Operator Implementation ✅ COMPLETE (Fluent API chosen over operators)

### Task 1.1: Basic Infrastructure ✅ COMPLETE
**Effort**: 12 hours (Actual: 2 hours)
**Priority**: Critical

**Subtasks**:
- [x] Create `Assets/MercuryMessaging/Protocol/DSL/` folder structure
- [x] Create fluent API instead of operators (better discoverability)
- [x] Implement Send() extension methods for MmRelayNode
- [x] Add type-specific overloads for all message types
- [x] Write comprehensive unit tests (20+ test cases)

**Acceptance Criteria**:
- Compiles without errors
- `relay :> "Hello"` creates valid message context
- All primitive types supported

### Task 1.2: Message Context Structure ✅ COMPLETE
**Effort**: 8 hours (Actual: 2 hours)
**Priority**: Critical

**Subtasks**:
- [x] Create `MmFluentMessage.cs` struct
- [x] Store relay reference and message
- [x] Implement routing methods instead of `>>` operator
- [x] Add Execute() terminator method
- [x] Handle error cases gracefully

**Code Structure**:
```csharp
public struct MmMessageContext {
    private readonly MmRelayNode relay;
    private readonly object message;

    public MmRoutedMessage operator >>(MmRouteBuilder route);
}
```

### Task 1.3: Routed Message Execution 🔴
**Effort**: 12 hours
**Priority**: Critical

**Subtasks**:
- [ ] Create `MmRoutedMessage.cs` struct
- [ ] Implement Execute() method
- [ ] Convert objects to MmMessage types
- [ ] Apply routing metadata
- [ ] Add performance logging

### Task 1.4: Operator Chaining 🟡
**Effort**: 16 hours
**Priority**: Important

**Subtasks**:
- [ ] Implement sequential message chains
- [ ] Add `:>` operator for MmRoutedMessage
- [ ] Support conditional chaining
- [ ] Handle execution order correctly
- [ ] Add chain debugging support

### Task 1.5: Combination Operators 🟢
**Effort**: 12 hours
**Priority**: Nice to Have

**Subtasks**:
- [ ] Implement `+` operator for route combination
- [ ] Add `&` operator for intersection
- [ ] Create `|` operator for union
- [ ] Handle precedence correctly
- [ ] Write comprehensive tests

---

## Phase 2: Fluent Builder API (80 hours)

### Task 2.1: Core Route Builder 🔴
**Effort**: 16 hours
**Priority**: Critical

**Subtasks**:
- [ ] Create `MmRouteBuilder.cs` struct
- [ ] Implement bit-packed field storage
- [ ] Add basic target properties (Self, Children, Parents)
- [ ] Implement Build() method
- [ ] Optimize for struct copying

**Performance Requirements**:
- Struct size ≤ 16 bytes
- Zero heap allocation
- All methods inlined

### Task 2.2: Filter Properties 🔴
**Effort**: 12 hours
**Priority**: Critical

**Subtasks**:
- [ ] Add Active/IncludeInactive properties
- [ ] Add Selected/All properties
- [ ] Add Network/LocalOnly properties
- [ ] Implement bit flag manipulation
- [ ] Write property tests

### Task 2.3: Tag System 🔴
**Effort**: 8 hours
**Priority**: Critical

**Subtasks**:
- [ ] Add Tag0-Tag7 properties
- [ ] Implement Tagged(string) method
- [ ] Create tag name mapper
- [ ] Support tag combinations
- [ ] Add tag validation

### Task 2.4: Spatial Extensions 🟡
**Effort**: 20 hours
**Priority**: Important

**Subtasks**:
- [ ] Implement Within(radius) method
- [ ] Add InDirection(direction, angle) method
- [ ] Create InBounds(bounds) method
- [ ] Add InCone(direction, angle) method
- [ ] Implement InLineOfSight(layerMask) method
- [ ] Write spatial tests with GameObjects

**Test Scenarios**:
```csharp
[Test]
public void WithinFiltersCorrectly() {
    // Create 10 objects at various distances
    // Apply Within(5f) filter
    // Verify only nearby objects matched
}
```

### Task 2.5: Type Filters 🟡
**Effort**: 12 hours
**Priority**: Important

**Subtasks**:
- [ ] Implement OfType<T>() generic method
- [ ] Add WithComponent<T>() alias
- [ ] Create HasComponent(Type) method
- [ ] Support interface type filtering
- [ ] Add type hierarchy checks

### Task 2.6: Custom Predicates 🟡
**Effort**: 12 hours
**Priority**: Important

**Subtasks**:
- [ ] Implement Where(Func<GameObject, bool>)
- [ ] Add Where(Func<MmRelayNode, bool>) overload
- [ ] Store predicates efficiently
- [ ] Combine multiple predicates
- [ ] Handle null references safely

---

## Phase 3: Type Inference and Extensions (60 hours)

### Task 3.1: Message Factory 🔴
**Effort**: 16 hours
**Priority**: Critical

**Subtasks**:
- [ ] Create `MmMessageFactory.cs`
- [ ] Add single-argument overloads
- [ ] Implement params object[] version
- [ ] Add generic Create<T>() method
- [ ] Handle custom message types

**Supported Types**:
- bool, int, float, string
- Vector2, Vector3, Vector4
- Quaternion, Transform
- GameObject, byte[]
- Custom MmMessage subclasses

### Task 3.2: Convenience Extensions 🟡
**Effort**: 12 hours
**Priority**: Important

**Subtasks**:
- [ ] Create `MmRelayNodeExtensions.cs`
- [ ] Add Broadcast(message) method
- [ ] Implement SendTo(target, message) method
- [ ] Create Notify(message) for parents
- [ ] Add Query/Response patterns

### Task 3.3: Async/Await Support 🟢
**Effort**: 20 hours
**Priority**: Nice to Have

**Subtasks**:
- [ ] Implement Request<T>() method
- [ ] Add timeout support
- [ ] Create response handler registration
- [ ] Support cancellation tokens
- [ ] Write async tests

**Example API**:
```csharp
var health = await relay.Request<int>(
    "GetHealth",
    Children.OfType<Player>(),
    timeout: 2f
);
```

### Task 3.4: Temporal Extensions 🟢
**Effort**: 12 hours
**Priority**: Nice to Have

**Subtasks**:
- [ ] Implement After(seconds) delayed execution
- [ ] Add Every(interval) for repeating messages
- [ ] Create When(condition) for conditional timing
- [ ] Support cancellation
- [ ] Integrate with Unity coroutines

---

## Phase 4: Testing and Performance (40 hours)

### Task 4.1: Unit Test Suite 🔴
**Effort**: 16 hours
**Priority**: Critical

**Test Categories**:
- [ ] Operator functionality tests
- [ ] Route builder tests
- [ ] Type inference tests
- [ ] Spatial filter tests
- [ ] Edge case handling

**Coverage Target**: >90%

### Task 4.2: Integration Tests 🔴
**Effort**: 12 hours
**Priority**: Critical

**Test Scenarios**:
- [ ] End-to-end message routing with DSL
- [ ] Complex filter combinations
- [ ] Performance under load
- [ ] Compatibility with existing code
- [ ] Network message synchronization

### Task 4.3: Performance Benchmarking 🔬
**Effort**: 8 hours
**Priority**: Research

**Benchmarks**:
- [ ] DSL vs traditional API overhead
- [ ] Memory allocation comparison
- [ ] Compilation time impact
- [ ] IntelliSense responsiveness
- [ ] Runtime execution time

**Target**: Zero overhead (< 2% difference)

### Task 4.4: Code Metrics Study 🔬
**Effort**: 4 hours
**Priority**: Research

**Metrics to Measure**:
- [ ] Lines of code reduction
- [ ] Token count reduction
- [ ] Cyclomatic complexity reduction
- [ ] Time to implement features
- [ ] Error rate comparison

---

## Phase 5: Migration and Documentation (Included above, 40 hours total)

### Task 5.1: Migration Tool 🟡
**Effort**: 16 hours
**Priority**: Important

**Subtasks**:
- [ ] Create Roslyn-based code analyzer
- [ ] Detect old-style MmInvoke calls
- [ ] Generate DSL equivalent code
- [ ] Provide refactoring suggestions
- [ ] Add batch migration support

### Task 5.2: API Documentation 🔴
**Effort**: 12 hours
**Priority**: Critical

**Documentation Items**:
- [ ] XML documentation for all public APIs
- [ ] Code examples for common patterns
- [ ] Migration guide from old API
- [ ] Best practices document
- [ ] Performance tuning guide

### Task 5.3: IntelliSense Enhancements 🟢
**Effort**: 8 hours
**Priority**: Nice to Have

**Subtasks**:
- [ ] Create code snippets for Visual Studio
- [ ] Add ReSharper annotations
- [ ] Create Rider live templates
- [ ] Generate quick action suggestions
- [ ] Add context-sensitive help

### Task 5.4: Video Tutorials 🟢
**Effort**: 4 hours
**Priority**: Nice to Have

**Tutorial Topics**:
- [ ] Basic message sending with DSL
- [ ] Complex routing patterns
- [ ] Migration from old API
- [ ] Performance optimization tips
- [ ] Advanced spatial filtering

---

## Milestone Schedule

### Milestone 1: Core Operators (Week 1-1.5)
- ✅ `:>` operator working
- ✅ `>>` routing working
- ✅ Basic message types supported

### Milestone 2: Fluent Builder (Week 2-3)
- ✅ Route builder complete
- ✅ All filter properties working
- ✅ Spatial extensions functional

### Milestone 3: Type System (Week 4)
- ✅ Message factory working
- ✅ Extension methods complete
- ✅ Type inference accurate

### Milestone 4: Testing (Week 5)
- ✅ All tests passing
- ✅ Performance validated
- ✅ Code metrics collected

### Milestone 5: Polish (Week 6)
- ✅ Migration tool working
- ✅ Documentation complete
- ✅ User study ready

---

## Dependencies

### External Dependencies
- Unity 2021.3+ (for C# 9.0 features)
- Roslyn 4.0+ (for source generators)
- BenchmarkDotNet (for performance testing)

### Internal Dependencies
- MmRelayNode API stability
- MmMessage type system
- MmMetadataBlock structure

---

## Risk Register

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Operator precedence issues | High | Medium | Extensive testing, parentheses guidance |
| IntelliSense confusion | Medium | High | Clear documentation, code snippets |
| Performance regression | High | Low | Aggressive optimization, benchmarking |
| Breaking changes | Medium | Medium | Compatibility layer, gradual migration |
| Learning curve | Medium | High | Tutorials, examples, migration tools |

---

## Definition of Done

Each task is complete when:
- [ ] Code implemented and compiles
- [ ] Unit tests written and passing (>90% coverage)
- [ ] Performance benchmarks met (within 2% of baseline)
- [ ] XML documentation complete
- [ ] Code examples provided
- [ ] Integration tests passing
- [ ] Code reviewed

---

## Performance Targets

| Metric | Baseline | DSL Target | Measurement |
|--------|----------|------------|-------------|
| Execution time | 142ns | <145ns (<2% overhead) | BenchmarkDotNet |
| Memory allocation | 88 bytes | 88 bytes (zero overhead) | Memory Profiler |
| Compilation time | 1.0x | <1.05x (<5% increase) | Build timing |
| Binary size | 1.0x | <1.01x (<1% increase) | Assembly size |
| Lines of code | 1.0x | 0.3x (70% reduction) | Code metrics |

---

## Code Metrics Goals

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Line count reduction | 50-70% | Before/after comparison |
| Token count reduction | 60-80% | Roslyn token analysis |
| Cyclomatic complexity | -30% | Code metrics tools |
| Time to implement | -40% | Developer study |
| Error rate | -50% | Bug tracking |

---

## User Study Protocol

### Study Design
- **Participants**: N=20 Unity developers (10 novice, 10 experienced)
- **Duration**: 2 hours per participant
- **Tasks**: 10 message routing scenarios
- **Conditions**: Within-subjects (both APIs)
- **Order**: Counterbalanced to avoid learning effects

### Measurements
1. **Quantitative**:
   - Task completion time
   - Error count
   - Code quality (lines, complexity)

2. **Qualitative**:
   - Likert scale preference (1-7)
   - NASA-TLX cognitive load
   - Open-ended feedback

### Hypotheses
- H1: DSL reduces implementation time by 40%
- H2: DSL reduces error rate by 50%
- H3: DSL improves subjective preference
- H4: DSL reduces cognitive load

---

## Notes

- Prioritize developer experience over clever features
- Keep syntax simple and predictable
- Maintain zero-cost abstraction principle
- Ensure backward compatibility
- Document all design decisions