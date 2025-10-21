# Implementation Checklist: Proxy Service Source Generator

**Purpose**: Guide implementation and validate progress for Spec 009  
**Created**: 2025-10-22  
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [tasks.md](../tasks.md)  
**Status**: 📋 Ready for Implementation

---

## Phase 0: Project Setup (6 tasks)

### CHK001: Source Generator Project Created
- [ ] `LablabBean.SourceGenerators.Proxy` project exists in `dotnet/framework/`
- [ ] Project targets .NET Standard 2.0
- [ ] Project configured as source generator (OutputItemType="Analyzer")
- [ ] Project builds successfully

**Validation**: Run `dotnet build` on source generator project

---

### CHK002: NuGet Dependencies Added
- [ ] Microsoft.CodeAnalysis.CSharp (4.8.0+) added
- [ ] Microsoft.CodeAnalysis.Analyzers (3.3.4+) added
- [ ] All packages restore successfully

**Validation**: Check `.csproj` file and run `dotnet restore`

---

### CHK003: Attributes Created
- [ ] `RealizeServiceAttribute.cs` exists in `LablabBean.Plugins.Contracts/Attributes/`
- [ ] `SelectionStrategyAttribute.cs` exists in `LablabBean.Plugins.Contracts/Attributes/`
- [ ] Both attributes compile without errors
- [ ] Attributes are public and accessible

**Validation**: Build contracts project, verify attributes in IntelliSense

---

### CHK004: Test Project Created
- [ ] `LablabBean.SourceGenerators.Proxy.Tests` project exists
- [ ] Test project references source generator project
- [ ] Test project has xUnit packages
- [ ] Test project builds successfully

**Validation**: Run `dotnet test` (should pass with 0 tests initially)

---

### CHK005: Generator Infrastructure
- [ ] `ProxyServiceGenerator.cs` class created
- [ ] Class implements `IIncrementalGenerator`
- [ ] `Initialize()` method defined
- [ ] Generator is discovered by compiler

**Validation**: Add generator to test project, verify it's loaded

---

## Phase 1: Basic Method Generation (12 tasks)

### CHK006: Syntax Provider Implemented
- [ ] Syntax provider finds partial classes with `[RealizeService]`
- [ ] Interface type extracted from attribute argument
- [ ] Provider filters correctly (only partial classes with attribute)

**Validation**: Unit test with sample partial class

---

### CHK007: Validation Logic
- [ ] Validates target type is an interface
- [ ] Validates `_registry` field exists
- [ ] Generates diagnostic for missing field
- [ ] Generates diagnostic for non-interface target

**Validation**: Unit tests for all validation scenarios

---

### CHK008: Simple Method Generation
- [ ] Generates method implementations for value parameters
- [ ] Generates correct delegation: `_registry.Get<T>().Method(args)`
- [ ] Handles void methods (no return statement)
- [ ] Handles return types correctly

**Validation**: Generate proxy for interface with 5 simple methods, verify code compiles

---

### CHK009: Generated Code Quality
- [ ] Generated code compiles without errors
- [ ] Generated code compiles without warnings
- [ ] Method signatures match interface exactly
- [ ] Parameter names preserved

**Validation**: Build project with generated proxy, check for warnings

---

## Phase 2: Property and Event Generation (10 tasks)

### CHK010: Property Generation
- [ ] Property getters implemented: `get => _registry.Get<T>().PropertyName;`
- [ ] Property setters implemented: `set => _registry.Get<T>().PropertyName = value;`
- [ ] Read-only properties handled (no setter)
- [ ] Write-only properties handled (no getter)

**Validation**: Interface with properties, verify generated code

---

### CHK011: Event Generation
- [ ] Event add accessor: `add => _registry.Get<T>().EventName += value;`
- [ ] Event remove accessor: `remove => _registry.Get<T>().EventName -= value;`
- [ ] Events compile and work correctly

**Validation**: Interface with events, test add/remove

---

## Phase 3: Advanced Method Features (14 tasks)

### CHK012: Generic Methods
- [ ] Generic methods generated with type parameters
- [ ] Type constraints preserved: `where T : class, new()`
- [ ] Multiple type parameters handled
- [ ] Nested generic types handled: `Task<List<T>>`

**Validation**: Interface with generic methods, verify constraints

---

### CHK013: Parameter Modifiers
- [ ] Ref parameters: `ref int value` → `ref value`
- [ ] Out parameters: `out string result` → `out result`
- [ ] In parameters: `in ReadOnlySpan<byte>` → `in data`
- [ ] Params arrays: `params string[] args` → `args`

**Validation**: Interface with all parameter types, verify delegation

---

### CHK014: Async Methods
- [ ] Async methods returning Task generated correctly
- [ ] Async methods returning Task<T> generated correctly
- [ ] No unnecessary async/await keywords

**Validation**: Interface with async methods, verify compilation

---

### CHK015: Default Parameters
- [ ] Default parameter values preserved in signature
- [ ] Default values compile correctly

**Validation**: Method with defaults, verify signature matches

---

## Phase 4: Selection Strategy Support (8 tasks)

### CHK016: Strategy Detection
- [ ] `[SelectionStrategy]` attribute read from class
- [ ] SelectionMode enum value extracted
- [ ] Missing attribute handled (default behavior)

**Validation**: Three classes with different strategies

---

### CHK017: Strategy Code Generation
- [ ] SelectionMode.One: `_registry.Get<T>(SelectionMode.One)`
- [ ] SelectionMode.HighestPriority: `_registry.Get<T>(SelectionMode.HighestPriority)`
- [ ] SelectionMode.All: `_registry.GetAll<T>()`
- [ ] No strategy: `_registry.Get<T>()`

**Validation**: Verify generated code for each mode

---

## Phase 5: Nullable and Code Quality (10 tasks)

### CHK018: Nullable Reference Types
- [ ] `#nullable enable` directive added
- [ ] Nullable annotations preserved: `string?` → `string?`
- [ ] Nullable return types handled
- [ ] Nullable parameters handled

**Validation**: Interface with nullable types, verify annotations

---

### CHK019: Code Quality
- [ ] `// <auto-generated />` comment present
- [ ] XML documentation copied from interface
- [ ] Code formatted with proper indentation
- [ ] Blank lines between members

**Validation**: Review generated code for readability

---

### CHK020: No Warnings
- [ ] Generated code compiles without warnings
- [ ] Nullable analysis passes
- [ ] No CS8600-CS8999 warnings

**Validation**: Build with TreatWarningsAsErrors=true

---

## Phase 6: Error Handling and Diagnostics (10 tasks)

### CHK021: Diagnostic Descriptors
- [ ] Missing `_registry` field diagnostic defined
- [ ] Non-interface target diagnostic defined
- [ ] Inaccessible interface diagnostic defined
- [ ] All diagnostics have helpful messages

**Validation**: Review diagnostic messages for clarity

---

### CHK022: Diagnostic Reporting
- [ ] Missing field produces error with location
- [ ] Non-interface produces error with location
- [ ] Generator doesn't crash on invalid input
- [ ] Graceful degradation on errors

**Validation**: Test with invalid inputs, verify diagnostics

---

## Phase 7: Integration and Documentation (12 tasks)

### CHK023: Example Implementation
- [ ] Example proxy service created in `plugins/examples/`
- [ ] Example uses `[RealizeService]` attribute
- [ ] Example uses `[SelectionStrategy]` attribute
- [ ] Example builds and works correctly

**Validation**: Build and run example

---

### CHK024: Documentation
- [ ] `USAGE.md` created with step-by-step guide
- [ ] At least 3 examples included
- [ ] Common errors documented
- [ ] Troubleshooting section present

**Validation**: Follow documentation to create new proxy

---

### CHK025: Completion
- [ ] CHANGELOG.md updated
- [ ] COMPLETION.md created
- [ ] spec.md status updated to Complete
- [ ] All 101 tasks marked complete

**Validation**: Review all completion documents

---

## Success Criteria Validation

### CHK026: SC-001 - 50+ Methods
- [ ] Generator handles interface with 50+ methods
- [ ] No errors or warnings
- [ ] All methods implemented correctly

**Validation**: Create large interface, verify generation

---

### CHK027: SC-002 - No Warnings
- [ ] Generated code compiles without warnings
- [ ] Nullable reference type analysis passes

**Validation**: Build with warnings as errors

---

### CHK028: SC-003 - <30 Seconds
- [ ] Developer can create proxy in <30 seconds
- [ ] Only 10 lines of code required

**Validation**: Time yourself creating a new proxy

---

### CHK029: SC-004 - 90% Reduction
- [ ] Manual proxy: 200+ lines
- [ ] With generator: 10 lines
- [ ] 95% reduction achieved

**Validation**: Compare manual vs generated line counts

---

### CHK030: SC-005 - All Member Types
- [ ] Methods work ✅
- [ ] Properties work ✅
- [ ] Events work ✅

**Validation**: Interface with all types

---

### CHK031: SC-006 - Generic Constraints
- [ ] Type parameters preserved
- [ ] Constraints preserved
- [ ] 100% accuracy

**Validation**: Complex generic interface

---

### CHK032: SC-007 - Ref/Out Parameters
- [ ] Ref parameters work
- [ ] Out parameters work
- [ ] 100% accuracy

**Validation**: Methods with ref/out

---

### CHK033: SC-008 - Async Methods
- [ ] Task methods work
- [ ] Task<T> methods work
- [ ] 100% accuracy

**Validation**: Async interface

---

### CHK034: SC-009 - Clear Diagnostics
- [ ] Error messages are clear
- [ ] Error messages are actionable
- [ ] Locations are accurate

**Validation**: Review all diagnostic messages

---

### CHK035: SC-010 - Readable Code
- [ ] Generated code is well-formatted
- [ ] Generated code matches hand-written quality

**Validation**: Code review of generated output

---

### CHK036: SC-011 - <1 Second Build
- [ ] Build time impact measured
- [ ] <1 second for 10 proxies
- [ ] Incremental generation working

**Validation**: Benchmark build times

---

### CHK037: SC-012 - 3+ Examples
- [ ] Example 1: Basic proxy
- [ ] Example 2: Properties/events
- [ ] Example 3: Generic methods

**Validation**: Count examples in documentation

---

## Final Validation

### CHK038: All Tests Passing
- [ ] Unit tests: 100% passing
- [ ] Integration tests: 100% passing
- [ ] Snapshot tests: 100% passing

**Validation**: `dotnet test` shows all green

---

### CHK039: Production Ready
- [ ] All 101 tasks complete
- [ ] All 12 success criteria met
- [ ] All 46 functional requirements implemented
- [ ] Documentation complete

**Validation**: Review completion checklist

---

### CHK040: Ready for Use
- [ ] Can be used in real projects
- [ ] Reduces boilerplate by 90%+
- [ ] No known blocking issues

**Validation**: Use in actual project

---

## Summary

**Total Checklist Items**: 40  
**Phases Covered**: 7  
**Success Criteria**: 12  
**Estimated Time**: 20-30 hours

**Status**: 📋 Ready - Begin with Phase 0

---

**Created**: 2025-10-22  
**Last Updated**: 2025-10-22  
**Next**: Start implementation with T001
