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

## Phase 3: Roslyn Analyzers (20h) - COMPLETE

### 3.1: Project Setup
- [x] Create `SourceGenerators/MercuryMessaging.Analyzers/` project
- [x] Configure as netstandard2.0 analyzer
- [x] Add Microsoft.CodeAnalysis.CSharp reference (v4.3.0)

### 3.2: MM005 - Missing Execute Analyzer
- [x] Detect MmFluentMessage<T> returned but not used
- [x] Show warning: "Did you forget to call .Execute()?"
- [x] Implement quick fix: Add .Execute() (MM005CodeFixProvider.cs)
- [x] Add unit tests (AnalyzerTestCases.cs)

### 3.3: MM010 - Non-Partial Class Analyzer
- [x] Detect [MmGenerateDispatch] on non-partial class
- [x] Show error: "Class must be partial"
- [x] Implement quick fix: Add partial keyword (MM010CodeFixProvider.cs)
- [x] Add unit tests

### 3.4: Additional Analyzers (12 more implemented)
- [x] MM001: Suggest DSL for verbose MmInvoke
- [x] MM002: Self-only level filter warning
- [x] MM003: Network message without OverNetwork filter
- [x] MM004: Suggest Broadcast convenience methods
- [x] MM006: Missing MmRefreshResponders after AddComponent
- [x] MM007: Potential infinite message loop
- [x] MM008: SetParent without routing table update
- [x] MM009: Missing base.MmInvoke call in override
- [x] MM011: Suggest MmExtendableResponder for many handlers
- [x] MM012: Tag without TagCheckEnabled
- [x] MM013: Responder without relay node reference
- [x] MM014: Misspelled handler method name
- [x] MM015: Bitwise filter equality check

### 3.5: Distribution
- [x] Configure .meta file with RoslynAnalyzer label
- [x] Deploy to `Assets/MercuryMessaging/Protocol/Analyzers/`
- [x] Test in Unity Editor (AnalyzerTestCases.cs with #define MM_ANALYZER_TEST)

---

## Phase 4: Source Generator Enhancements (40h) - COMPLETE

### 4.1: Review Existing Generator
- [x] Review `SourceGenerators/MercuryMessaging.Generators/MmDispatchGenerator.cs`
- [x] Document current capabilities

### 4.2: Enhance for New Use Cases
- [x] Add diagnostics for invalid configurations (MMG001-MMG004)
- [x] Improve error messages (fixed RS1032 warnings)
- [x] Add analyzer release tracking (AnalyzerReleases.Shipped.md, AnalyzerReleases.Unshipped.md)

### 4.3: Handler Generation
- [x] Design `[MmHandler(methodId)]` attribute (`Protocol/Attributes/MmHandlerAttribute.cs`)
- [x] Generate switch dispatch without reflection (enhanced MmDispatchGenerator.cs)
- [x] Auto-register handlers at compile time
- [x] Add runtime integration tests (`Tests/Generators/MmHandlerAttributeTests.cs`)
- [x] Update documentation (`Documentation/SourceGenerators/README.md`)

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
| 3 | **Complete** | 20/20 |
| 4 | **Complete** | 10/10 |
| 5 | Partial | 3/4 |
| **Total** | **Nearly Complete** | **50/51** |

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

### Phase 4 (Completed 2025-12-11)
- `Protocol/Attributes/MmHandlerAttribute.cs` - NEW: Attribute for custom method handlers
- `SourceGenerators/MercuryMessaging.Generators/MmDispatchGenerator.cs` - Enhanced with [MmHandler] support
- `SourceGenerators/MercuryMessaging.Generators/AnalyzerReleases.Shipped.md` - NEW: Analyzer release tracking
- `SourceGenerators/MercuryMessaging.Generators/AnalyzerReleases.Unshipped.md` - NEW: Analyzer release tracking
- `Tests/Generators/MmHandlerAttributeTests.cs` - NEW: Runtime integration tests
- `Tests/Analyzers/AnalyzerTestCases.cs` - Added MMG001-MMG003 test cases
- `Documentation/SourceGenerators/README.md` - Updated with [MmHandler] documentation

---

### Phase 3 (Completed 2025-12-11)
- `SourceGenerators/MercuryMessaging.Analyzers/` - NEW: 15 Roslyn analyzers
- `Assets/MercuryMessaging/Protocol/Analyzers/MercuryMessaging.Analyzers.dll` - Deployed analyzer DLL
- `Assets/MercuryMessaging/Tests/Analyzers/AnalyzerTestCases.cs` - Test cases for all analyzers

---

*Updated 2025-12-11 - Phases 1-4 Complete, Phase 5 (docs) remaining*
