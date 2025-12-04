# DSL/DX Tasks

**Last Updated:** 2025-12-03

---

## Phase 1: Property-Based Syntax (16h) - COMPLETE

### 1.1: Create/Enhance MmRouteBuilder Struct
- [x] Update `Protocol/DSL/MmRoutingBuilder.cs`
- [x] Implement as `struct` for zero GC
- [x] Add direction properties: Children, Parents, Descendants, Ancestors, Siblings, All
- [x] Add filter methods: Active(), WithTag(), Selected()
- [x] Add Send<T>() method that auto-executes

### 1.2: Add To Property to MmRelayNode
- [x] Add `public MmRoutingBuilder To` property to MmRelayNode (line 168)
- [x] Ensure thread-safe (struct copy semantics)
- [x] Add XML documentation with examples

### 1.3: Add To Property to MmBaseResponder
- [x] Add `public MmRoutingBuilder To()` extension to MmBaseResponder (MmResponderExtensions.cs:169)
- [x] Null-safe: return default struct that no-ops if no relay node

### 1.4: Tests
- [x] Add unit tests for MmRoutingBuilder (PropertyRoutingTests.cs - 14 tests)
- [x] Verify auto-execute behavior
- [x] Verify struct semantics
- [x] Test all direction combinations

---

## Phase 2: Builder API for Advanced Cases (8h) - COMPLETE

### 2.1: Add Build() Method
- [x] Add `relay.Build()` that returns non-auto-executing builder (MmRelayNode.cs:175)
- [x] Created `MmDeferredRoutingBuilder` struct (Protocol/DSL/MmDeferredRoutingBuilder.cs)
- [x] Preserve existing fluent chain compatibility
- [x] Document when to use Build() vs To

### 2.2: Integration with Existing API
- [x] Ensure To and Build() coexist with existing API
- [x] No breaking changes to current fluent chain
- [x] Add responder extension (MmResponderExtensions.cs:181)
- [x] Add unit tests (BuilderApiTests.cs - 12 tests)

**Key Difference:**
```csharp
// Auto-execute (relay.To) - sends immediately
relay.To.Children.Send("Hello");

// Deferred (relay.Build()) - requires .Execute()
var msg = relay.Build().ToChildren().Send("Hello");
if (condition)
    msg.Execute();  // Only sends when called
```

---

## Phase 3: Roslyn Analyzers (20h)

### 3.1: Project Setup
- [ ] Create `SourceGenerators/MercuryMessaging.Analyzers/` project
- [ ] Configure as netstandard2.0 analyzer
- [ ] Add Microsoft.CodeAnalysis.CSharp reference

### 3.2: MM005 - Missing Execute Analyzer
- [ ] Detect MmFluentMessage<T> returned but not used
- [ ] Show warning: "Did you forget to call .Execute()?"
- [ ] Implement quick fix: Add .Execute()
- [ ] Add unit tests

### 3.3: MM010 - Non-Partial Class Analyzer
- [ ] Detect [MmGenerateDispatch] on non-partial class
- [ ] Show error: "Class must be partial"
- [ ] Implement quick fix: Add partial keyword
- [ ] Add unit tests

### 3.4: MM001 - Suggest DSL Analyzer (Optional)
- [ ] Detect verbose MmInvoke with MmMetadataBlock
- [ ] Show info: "Consider using fluent API"
- [ ] Implement quick fix: Convert to fluent
- [ ] Add unit tests

### 3.5: Distribution
- [ ] Configure .meta file with RoslynAnalyzer label
- [ ] Test in Unity Editor
- [ ] Verify zero-config activation for end users

---

## Phase 4: Source Generator Enhancements (40h)

### 4.1: Review Existing Generator
- [ ] Review `SourceGenerators/MercuryMessaging.Generators/MmDispatchGenerator.cs`
- [ ] Document current capabilities

### 4.2: Enhance for New Use Cases
- [ ] Optimize dispatch for property-based syntax
- [ ] Add diagnostics for invalid configurations
- [ ] Improve error messages

### 4.3: Handler Generation (Future)
- [ ] Design `[MmHandler("Name")]` attribute
- [ ] Generate switch dispatch without reflection
- [ ] Auto-register handlers

---

## Phase 5: Documentation (8h)

### 5.1: Update Existing Docs
- [ ] Update DSL_API_GUIDE.md with new To syntax
- [ ] Add migration examples (old fluent → new property)

### 5.2: XML Documentation
- [x] Add XML docs to MmRoutingBuilder (already complete)
- [x] Add XML docs to MmDeferredRoutingBuilder (complete)
- [x] Add XML docs to To/Build properties (complete)
- [ ] Include code examples in API guide docs

---

## Progress Summary

| Phase | Status | Tasks |
|-------|--------|-------|
| 1 | **Complete** | 13/13 |
| 2 | **Complete** | 4/4 |
| 3 | Not Started | 0/14 |
| 4 | Not Started | 0/6 |
| 5 | Partial | 3/4 |
| **Total** | **In Progress** | **20/41** |

---

## Files Created/Modified

### Phase 1 (Previously Completed)
- `Protocol/DSL/MmRoutingBuilder.cs` - Property-based routing builder
- `Protocol/Nodes/MmRelayNode.cs:168` - Added `To` property
- `Protocol/DSL/MmResponderExtensions.cs:169` - Added `To()` extension
- `Tests/Protocol/DSL/PropertyRoutingTests.cs` - 14 tests

### Phase 2 (Completed 2025-12-03)
- `Protocol/DSL/MmDeferredRoutingBuilder.cs` - NEW: Deferred routing builder (353 lines)
- `Protocol/Nodes/MmRelayNode.cs:175` - Added `Build()` method
- `Protocol/DSL/MmResponderExtensions.cs:181` - Added `Build()` extension
- `Tests/Protocol/DSL/BuilderApiTests.cs` - NEW: 12 tests for deferred execution

---

*Updated 2025-12-03 - Phase 2 Complete*
