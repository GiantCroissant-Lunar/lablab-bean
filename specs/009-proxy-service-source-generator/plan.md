# Implementation Plan: Proxy Service Source Generator

**Branch**: `009-proxy-service-source-generator` | **Date**: 2025-10-22 | **Spec**: [spec.md](./spec.md)

## Summary

Implement a Roslyn incremental source generator that automatically creates proxy service implementations for interfaces marked with `[RealizeService]` attribute. This eliminates 90%+ of manual delegation boilerplate, making service contracts practical to use.

**Technical Approach**: Create a .NET Standard 2.0 source generator project using Roslyn APIs, implement incremental generation pipeline, analyze partial classes with `[RealizeService]` attribute, generate method/property/event implementations that delegate to `IRegistry.Get<T>()`, support all C# interface features (generics, constraints, ref/out, async, nullable).

## Technical Context

**Language/Version**: C# / .NET Standard 2.0 (generator), .NET 8 (target projects)
**Primary Dependencies**:
- Microsoft.CodeAnalysis.CSharp (4.8.0+) - Roslyn SDK
- Microsoft.CodeAnalysis.Analyzers (3.3.4+) - Analyzer utilities
- Existing: LablabBean.Plugins.Contracts (IRegistry, SelectionMode)

**Testing**: xUnit with Roslyn test helpers, snapshot testing for generated code
**Target Platform**: Cross-platform (.NET 8: Windows, Linux, macOS)
**Project Type**: Source generator library (.NET Standard 2.0)

**Performance Goals**:
- Build time impact: <1 second for 10 proxy services
- Incremental generation: Only regenerate changed files
- Memory usage: <50MB during generation

**Constraints**:
- Must target .NET Standard 2.0 for compatibility
- Generated code must compile without warnings
- Must preserve all interface member signatures exactly
- Must support all C# interface features

**Scale/Scope**:
- 1 source generator assembly
- 2 attribute classes
- 46 functional requirements
- Expected: Generate 50-200 methods per proxy service

## Phases

### Phase 0: Project Setup (1-2 hours)

**Goal**: Create source generator project structure and basic infrastructure

**Tasks**:
1. Create `LablabBean.SourceGenerators.Proxy` project (.NET Standard 2.0)
2. Add Roslyn NuGet packages (Microsoft.CodeAnalysis.CSharp)
3. Create test project `LablabBean.SourceGenerators.Proxy.Tests`
4. Add attributes to `LablabBean.Plugins.Contracts`:
   - `[RealizeService(Type)]`
   - `[SelectionStrategy(SelectionMode)]`
5. Configure project as source generator (OutputItemType, IncludeBuildOutput)
6. Create basic generator class inheriting from `IIncrementalGenerator`

**Success Criteria**: Project builds, generator is discovered by compiler

---

### Phase 1: Basic Method Generation (4-6 hours)

**Goal**: Generate simple method implementations (no generics, no ref/out)

**Tasks**:
1. Implement syntax receiver to find partial classes with `[RealizeService]`
2. Extract interface type from attribute
3. Validate interface exists and is accessible
4. Validate `_registry` field exists
5. Generate method implementations for simple methods (value parameters, value returns)
6. Generate proper delegation: `return _registry.Get<T>().Method(args);`
7. Handle void methods (no return statement)
8. Write tests for basic method generation

**Success Criteria**: Generator creates working implementations for simple methods

---

### Phase 2: Property and Event Generation (3-4 hours)

**Goal**: Generate property and event implementations

**Tasks**:
1. Generate property getter implementations
2. Generate property setter implementations
3. Handle read-only properties (get-only)
4. Handle write-only properties (set-only)
5. Generate event add accessor implementations
6. Generate event remove accessor implementations
7. Write tests for properties and events

**Success Criteria**: Generator creates working implementations for properties and events

---

### Phase 3: Advanced Method Features (4-6 hours)

**Goal**: Support generics, constraints, ref/out, async, params

**Tasks**:
1. Generate generic methods with type parameters
2. Preserve type constraints (where T : class, etc.)
3. Handle ref parameters correctly
4. Handle out parameters correctly
5. Handle async methods (Task/Task<T>)
6. Handle params arrays
7. Handle default parameter values
8. Write tests for all advanced features

**Success Criteria**: Generator handles all C# method features correctly

---

### Phase 4: Selection Strategy Support (2-3 hours)

**Goal**: Support `[SelectionStrategy]` attribute for controlling service retrieval

**Tasks**:
1. Read `[SelectionStrategy]` attribute from partial class
2. Generate code using specified SelectionMode
3. Handle missing attribute (default behavior)
4. Support SelectionMode.One
5. Support SelectionMode.HighestPriority
6. Support SelectionMode.All
7. Write tests for all selection modes

**Success Criteria**: Generated code uses correct SelectionMode

---

### Phase 5: Nullable and Code Quality (3-4 hours)

**Goal**: Ensure generated code passes all quality checks

**Tasks**:
1. Preserve nullable reference type annotations
2. Add `#nullable enable` directive
3. Add `// <auto-generated />` comment
4. Copy XML documentation from interface
5. Format generated code properly (indentation, spacing)
6. Ensure no compiler warnings
7. Write tests for nullable scenarios

**Success Criteria**: Generated code compiles without warnings, passes nullable analysis

---

### Phase 6: Error Handling and Diagnostics (3-4 hours)

**Goal**: Provide helpful error messages for common mistakes

**Tasks**:
1. Diagnostic: Missing `_registry` field
2. Diagnostic: Target type is not an interface
3. Diagnostic: Interface not accessible
4. Diagnostic: Duplicate `[RealizeService]` on same class
5. Add helpful fix suggestions to diagnostics
6. Test all diagnostic scenarios
7. Ensure generator doesn't crash on invalid input

**Success Criteria**: All error cases produce clear, actionable diagnostics

---

### Phase 7: Integration and Documentation (3-4 hours)

**Goal**: Integrate with existing codebase and document usage

**Tasks**:
1. Create example proxy service using generator
2. Update existing proxy services to use generator (optional)
3. Write usage documentation
4. Create tutorial with step-by-step examples
5. Add performance benchmarks
6. Update CHANGELOG
7. Create completion document

**Success Criteria**: Documentation complete, examples working

---

## Total Estimated Time: 20-30 hours (2.5-4 days)

## Testing Strategy

### Unit Tests
- Syntax receiver finds correct classes
- Attribute parsing works correctly
- Method signature generation is accurate
- Property/event generation is correct
- Selection strategy is applied correctly

### Integration Tests
- Generated code compiles successfully
- Generated proxy delegates correctly
- All C# interface features work
- Nullable analysis passes
- No compiler warnings

### Snapshot Tests
- Compare generated code against expected output
- Detect unintended changes in generation

## Success Metrics

- ✅ All 46 functional requirements implemented
- ✅ All 12 success criteria met
- ✅ 100% test coverage for generator logic
- ✅ Build time impact <1 second
- ✅ Zero compiler warnings in generated code
- ✅ Documentation with 3+ examples

## Risks and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Roslyn API complexity | High | Start simple, iterate, comprehensive testing |
| Build time impact | Medium | Use incremental generation, profile performance |
| Edge cases missed | Medium | Comprehensive test suite, real-world testing |
| Breaking Roslyn changes | Low | Target stable version, test across SDKs |

## Dependencies

```
Spec 009 Proxy Service Source Generator
├── Spec 007: Tiered Contract Architecture ✅ (IRegistry, SelectionMode)
├── Spec 008: Extended Contract Assemblies ✅ (Service interfaces to proxy)
└── Roslyn SDK (Microsoft.CodeAnalysis.CSharp)
```

## Next Steps After Completion

1. Apply generator to existing proxy services
2. Measure boilerplate reduction
3. Gather developer feedback
4. Consider additional generator features (logging, validation)
5. Publish as NuGet package (optional)
