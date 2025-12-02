# DSL/DX Tasks

**Last Updated:** 2025-12-01

---

## Phase 1: Property-Based Syntax (16h)

### 1.1: Create/Enhance MmRouteBuilder Struct
- [ ] Update `Protocol/DSL/MmRoutingBuilder.cs`
- [ ] Implement as `readonly struct` for zero GC
- [ ] Add direction properties: Children, Parents, Descendants, Ancestors, Siblings, All
- [ ] Add filter methods: Active(), WithTag(), Selected()
- [ ] Add Send<T>() method that auto-executes

### 1.2: Add To Property to MmRelayNode
- [ ] Add `public MmRouteBuilder To` property to MmRelayNode
- [ ] Ensure thread-safe (struct copy semantics)
- [ ] Add XML documentation with examples

### 1.3: Add To Property to MmBaseResponder
- [ ] Add `public MmRouteBuilder To` property to MmBaseResponder
- [ ] Null-safe: return no-op builder if no relay node

### 1.4: Tests
- [ ] Add unit tests for MmRouteBuilder
- [ ] Verify auto-execute behavior
- [ ] Verify zero allocations (struct)
- [ ] Test all direction combinations

---

## Phase 2: Builder API for Advanced Cases (8h)

### 2.1: Add Build() Method
- [ ] Add `relay.Build()` that returns non-auto-executing builder
- [ ] Preserve existing fluent chain compatibility
- [ ] Document when to use Build() vs To

### 2.2: Integration with Existing API
- [ ] Ensure To and Send() coexist with existing API
- [ ] No breaking changes to current fluent chain

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
- [ ] Add XML docs to MmRouteBuilder
- [ ] Add XML docs to To properties
- [ ] Include code examples in docs

---

## Progress Summary

| Phase | Status | Tasks |
|-------|--------|-------|
| 1 | Not Started | 0/13 |
| 2 | Not Started | 0/4 |
| 3 | Not Started | 0/14 |
| 4 | Not Started | 0/6 |
| 5 | Not Started | 0/4 |
| **Total** | **Not Started** | **0/41** |

---

*Merged task checklist for DSL/DX improvements - Updated 2025-12-01*
