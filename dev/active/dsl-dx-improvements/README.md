# DSL/DX Improvements

**Status:** Planning
**Priority:** P3 (after Performance Optimization + Networking)
**Estimated Effort:** 120 hours (3 weeks)
**Created:** 2025-11-27

---

## Overview

Improve MercuryMessaging's developer experience through shorter syntax, better tooling, comprehensive tutorials, and performance documentation. This task directly supports the User Study (P5) and makes Mercury competitive with MessagePipe's clean API.

---

## Current State

### DSL Implementation (Complete)
- 27 DSL files in `Assets/Framework/MercuryMessaging/Protocol/DSL/`
- **86% verbosity reduction** achieved (210 chars → 48 chars)
- Two-tier API: Auto-Execute + Fluent Chain
- Works on both MmRelayNode and MmBaseResponder
- Spatial, temporal, type, and custom predicate filtering

### Current Syntax
```csharp
// Traditional (210 chars, 7 lines)
relay.MmInvoke(MmMethod.MessageString, "Hello",
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.Active,
        MmSelectedFilter.All, MmNetworkFilter.Local));

// Current Fluent (48 chars, 1 line)
relay.Send("Hello").ToChildren().Active().Execute();

// Current Quick (32 chars)
relay.BroadcastValue("Hello");
```

### Gap Analysis
- Still requires `.Execute()` for fluent chains
- No operator-based syntax (like `:>` or `>>`)
- Missing tutorials for spatial, network, migration
- No Roslyn analyzers for deprecation warnings
- No source generators for handlers

---

## Target State

### Phase 1: Even Shorter Syntax (40h)

**Proposed Improvements:**
```csharp
// Level 1: Property-based routing (32 chars)
relay.To.Children.Send("Hello");

// Level 2: Operator overloads (24 chars)
relay >> "Hello" >> Children;

// Level 3: Extension properties (20 chars)
relay.Children << "Hello";
```

**Implementation:**
- Add `To` property that returns a routing builder
- Implement `>>` operator on MmRelayNode
- Create implicit conversions for MmLevelFilter
- Auto-execute on assignment/cast

### Phase 2: Source Generators (40h)

**Message Handler Generation:**
```csharp
// User writes:
[MmHandler("Damage")]
void OnDamage(int amount) {
    health -= amount;
}

// Generated:
protected override void ReceivedMessage(MmMessageInt msg) {
    if (msg.context == "Damage") OnDamage(msg.value);
}
```

**Benefits:**
- Zero-reflection handler dispatch
- Compile-time type safety
- Automatic registration
- IntelliSense for handler names

### Phase 3: Roslyn Analyzers (20h)

**Analyzer Rules:**
1. `MM001`: Warn on deprecated MmQuickExtensions usage
2. `MM002`: Suggest DSL equivalents for verbose MmInvoke
3. `MM003`: Detect unused message handlers
4. `MM004`: Validate tag combinations
5. `MM005`: Warn on missing Execute() calls

**Quick Fixes:**
- Auto-migrate MmInvoke to fluent API
- Remove deprecated method calls
- Add missing Execute()

### Phase 4: Tutorials & Documentation (20h)

**Missing Tutorials:**
| Tutorial | Description | Priority |
|----------|-------------|----------|
| SpatialFilteringTutorial.cs | Within(), InBounds(), InCone() | P1 |
| NetworkMessagingTutorial.cs | OverNetwork(), LocalOnly() | P1 |
| MigrationGuide.md | Traditional → DSL conversion | P1 |
| CustomResponderTutorial.cs | MmExtendableResponder usage | P2 |
| PerformanceBestPractices.md | When to use which API tier | P2 |

**Documentation Updates:**
- Enrich XML docs with examples on all methods
- Add code snippets for Visual Studio
- Create cheat sheet (single-page API reference)

---

## Success Metrics

### Syntax Reduction
- [ ] Achieve 95% verbosity reduction (48 → 20 chars)
- [ ] Eliminate mandatory `.Execute()` for simple cases
- [ ] Support operator-based syntax

### Developer Productivity
- [ ] <5 minutes to understand API (via tutorials)
- [ ] <30 seconds for common patterns (via snippets)
- [ ] Zero deprecated API usage in codebase

### Tooling
- [ ] All 5 Roslyn analyzers implemented
- [ ] Source generators working for handlers
- [ ] IntelliSense shows examples for all methods

---

## Files to Modify

### Priority 1 (Syntax)
- `Protocol/DSL/MmRelayNode.cs` - Add `To` property, operators
- `Protocol/DSL/MmFluentMessage.cs` - Auto-execute support
- `Protocol/DSL/MmMessagingExtensions.cs` - Operator overloads
- `Protocol/DSL/MmQuickExtensions.cs` - Replace deprecated methods

### Priority 2 (Generators)
- Create `Editor/SourceGenerators/MmHandlerGenerator.cs`
- Create `Protocol/DSL/Attributes/MmHandlerAttribute.cs`

### Priority 3 (Analyzers)
- Create `Editor/Analyzers/MmDslAnalyzer.cs`
- Create `Editor/Analyzers/MmDeprecationAnalyzer.cs`

### Priority 4 (Tutorials)
- Create `Examples/Tutorials/DSL/SpatialFilteringTutorial.cs`
- Create `Examples/Tutorials/DSL/NetworkMessagingTutorial.cs`
- Create `Examples/Tutorials/DSL/MigrationGuide.md`

---

## Research Publication Potential

**Paper:** "Fluent APIs for Game Engine Communication: Performance and Usability Analysis"

**Venue:** CHI LBW 2025 or DIS 2025

**Study Design:**
1. **Micro-benchmarks:** DSL vs Traditional API latency
2. **Allocation comparison:** Heap allocations per message
3. **Developer productivity:** Time to implement common patterns (N=15-20)

**Hypotheses:**
- H1: DSL syntax reduces implementation time by 40%
- H2: DSL has <5% performance overhead vs traditional API
- H3: Developers prefer DSL syntax 4:1 over traditional

---

## Dependencies

- **Blocking:** None
- **Supports:** User Study (P5) - participants will use DSL
- **Parallel:** Can run alongside Performance Optimization (P1)

---

## Next Steps

1. Review and approve shorter syntax proposals
2. Create feature branch: `feature/dsl-dx-improvements`
3. Implement Phase 1 (shorter syntax) first
4. Add tutorials before User Study begins
5. Implement analyzers and generators last

---

*Document Version: 1.0*
*Created: 2025-11-27*
*Owner: Framework Team*
