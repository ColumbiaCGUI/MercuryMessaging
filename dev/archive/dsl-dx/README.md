# DSL/DX Improvements

**Status:** Nearly Complete - Phases 1-4 Complete, Phase 5 (docs) remaining
**Priority:** P3 (after Networking)
**Estimated Effort:** 92 hours (84h complete, 8h remaining)
**Created:** 2025-11-27
**Last Updated:** 2025-12-11

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

### Phase 3: Roslyn Analyzers (20h) ✅ COMPLETE

**15 Analyzers Implemented:**
| Rule | Description | Severity |
|------|-------------|----------|
| MM001 | Suggest DSL for verbose MmInvoke | Info |
| MM002 | Self-only level filter warning | Warning |
| MM003 | Network message without OverNetwork filter | Warning |
| MM004 | Suggest Broadcast convenience methods | Info |
| MM005 | Warn on missing .Execute() | Warning |
| MM006 | Missing MmRefreshResponders after AddComponent | Warning |
| MM007 | Potential infinite message loop | Warning |
| MM008 | SetParent without routing table update | Warning |
| MM009 | Missing base.MmInvoke call in override | Warning |
| MM010 | [MmGenerateDispatch] on non-partial class | Error |
| MM011 | Suggest MmExtendableResponder for many handlers | Info |
| MM012 | Tag without TagCheckEnabled | Warning |
| MM013 | Responder without relay node reference | Info |
| MM014 | Misspelled handler method name | Warning |
| MM015 | Bitwise filter equality check | Warning |

**Quick Fix Providers:** MM005 (add .Execute()), MM010 (add partial keyword)

**Deployment:** `Assets/MercuryMessaging/Protocol/Analyzers/MercuryMessaging.Analyzers.dll`

### Phase 4: Source Generators (40h) ✅ COMPLETE

**[MmHandler] Attribute for Custom Methods:**
```csharp
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder
{
    [MmHandler(1000)]
    private void OnCustomColor(MmMessage msg) { }

    [MmHandler(1001, Name = "ScaleHandler")]
    private void OnCustomScale(MmMessage msg) { }
}
// Generated: switch dispatch for method IDs without reflection
```

**Generator Diagnostics:** MMG001-MMG003 for invalid configurations
**Test Coverage:** `Tests/Generators/MmHandlerAttributeTests.cs`
**Documentation:** `Documentation/SourceGenerators/README.md`

### Phase 5: Documentation (8h)

- Update DSL_API_GUIDE.md with new To syntax
- Add XML documentation to all public APIs
- Create migration examples

---

## Success Metrics

- [x] 95% verbosity reduction (48 → 32 chars) ✅ Phase 1
- [x] Eliminate mandatory `.Execute()` for simple cases ✅ Phase 1
- [x] MM005 analyzer warns on forgotten Execute ✅ Phase 3
- [x] MM010 analyzer catches non-partial classes ✅ Phase 3
- [x] Zero allocations in property-based syntax (struct builder) ✅ Phase 1
- [ ] Source generator enhancements (Phase 4)
- [ ] Documentation updates (Phase 5)

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
