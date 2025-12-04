# DSL/DX Improvements

**Status:** In Progress - Phases 1-2 Complete
**Priority:** P3 (after Networking)
**Estimated Effort:** 92 hours (24h complete, 68h remaining)
**Created:** 2025-11-27
**Last Updated:** 2025-12-03

---

## Overview

Improve MercuryMessaging's developer experience through shorter syntax, Roslyn analyzers, and source generators. This task directly supports the User Study (P5) and makes Mercury competitive with MessagePipe's clean API.

**Note:** Wiki tutorials are tracked separately in `dev/active/wiki-tutorials/`.

---

## Key Decisions (2025-12-01)

### Syntax: Level 1 Property-Based
```csharp
// Current (48 chars) - requires .Execute()
relay.Send("Hello").ToChildren().Active().Execute();

// New Level 1 (32 chars) - auto-executes
relay.To.Children.Send("Hello");
```

**Rationale:**
- Most familiar to C#/Unity developers (like LINQ, transform.GetChild())
- Full IntelliSense support at each `.`
- Self-documenting code
- Use `struct` builders for zero GC allocation

### Auto-Execute Strategy: Dual API + Analyzer

**Option A: Dual API**
```csharp
// Auto-execute API (simple cases, 90% of usage)
relay.To.Children.Send("Hello");  // Sends immediately

// Builder API (advanced cases, 10% of usage)
var builder = relay.Build().ToChildren();
if (ready) builder.Send("Hello").Execute();  // Conditional
```

**Option C: Analyzer Warning**
```csharp
relay.Send("Hello").ToChildren();  // MM005: Did you forget .Execute()?
```

### Zero-Config for End Users
- Pre-configure `.meta` files with RoslynAnalyzer label
- Analyzers activate automatically in Unity
- Source generators require opt-in: `[MmGenerateDispatch]` + `partial`

---

## Phases

### Phase 1: Property-Based Syntax (16h)

**Tasks:**
1. Create `MmRouteBuilder` readonly struct in `Protocol/DSL/`
2. Add `To` property to `MmRelayNode`
3. Add `To` property to `MmBaseResponder` (null-safe)
4. Implement direction properties: Children, Parents, Descendants, Ancestors, Siblings, All
5. Implement filter methods: Active(), WithTag(), Selected()
6. Implement `Send<T>()` with auto-execute
7. Add unit tests

### Phase 2: Builder API for Advanced Cases (8h)

**Tasks:**
1. Add `relay.Build()` that returns non-auto-executing builder
2. Preserve existing fluent chain compatibility
3. Document when to use Build() vs To

### Phase 3: Roslyn Analyzers (20h)

**Analyzers:**
| Rule | Description | Severity | Quick Fix |
|------|-------------|----------|-----------|
| MM005 | Warn on missing .Execute() | Warning | Add .Execute() |
| MM010 | [MmGenerateDispatch] on non-partial class | Error | Add partial |
| MM001 | Suggest DSL for verbose MmInvoke | Info | Convert to fluent |

**Distribution:**
- Create `SourceGenerators/MercuryMessaging.Analyzers/` project
- Configure as netstandard2.0 analyzer
- Pre-configure .meta with RoslynAnalyzer label

### Phase 4: Source Generators (40h)

**Message Handler Generation:**
```csharp
// User writes:
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageInt msg) { }
    protected override void ReceivedMessage(MmMessageString msg) { }
}

// Generated: Optimized dispatch without virtual calls
```

**Note:** Existing `[MmGenerateDispatch]` generator in `SourceGenerators/` - extend for new use cases.

### Phase 5: Documentation (8h)

- Update DSL_API_GUIDE.md with new To syntax
- Add XML documentation to all public APIs
- Create migration examples

---

## Success Metrics

- [ ] 95% verbosity reduction (48 → 32 chars)
- [ ] Eliminate mandatory `.Execute()` for simple cases
- [ ] MM005 analyzer warns on forgotten Execute
- [ ] MM010 analyzer catches non-partial classes
- [ ] Zero allocations in property-based syntax (struct builder)

---

## Files to Modify

### Phase 1-2 (Syntax)
- `Protocol/DSL/MmRoutingBuilder.cs` - Create/enhance MmRouteBuilder struct
- `Protocol/MmRelayNode.cs` - Add `To` property
- `Protocol/MmBaseResponder.cs` - Add `To` property (optional)

### Phase 3 (Analyzers)
- `SourceGenerators/MercuryMessaging.Analyzers/MM005Analyzer.cs`
- `SourceGenerators/MercuryMessaging.Analyzers/MM010Analyzer.cs`
- `SourceGenerators/MercuryMessaging.Analyzers/MM001Analyzer.cs`

### Phase 4 (Generators)
- `SourceGenerators/MercuryMessaging.Generators/` - Extend existing

---

## Dependencies

- **Blocking:** None
- **Supports:** User Study (P5) - participants will use improved DSL
- **Related:** Wiki Tutorials (documents new syntax)

---

*Merged from dsl-dx-improvements and dsl-dx-syntax on 2025-12-01*
