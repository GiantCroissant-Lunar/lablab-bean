# Tasks: Proxy Service Source Generator

**Input**: Design documents from `/specs/009-proxy-service-source-generator/`
**Prerequisites**: plan.md ✅, spec.md ✅

**Tests**: Tests are included per spec requirements (FR-038 through FR-046, SC-001 through SC-012)

**Organization**: Tasks are grouped by phase to enable incremental implementation and testing.

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)

## Path Conventions
- Source generator: `dotnet/framework/LablabBean.SourceGenerators.Proxy/`
- Tests: `dotnet/tests/LablabBean.SourceGenerators.Proxy.Tests/`
- Attributes: `dotnet/framework/LablabBean.Plugins.Contracts/`

---

## Phase 0: Project Setup (6 tasks)

**Purpose**: Create source generator project structure and basic infrastructure

- [ ] T001 Create `LablabBean.SourceGenerators.Proxy` project (.NET Standard 2.0) in `dotnet/framework/LablabBean.SourceGenerators.Proxy/`
- [ ] T002 Add NuGet packages: Microsoft.CodeAnalysis.CSharp (4.8.0+), Microsoft.CodeAnalysis.Analyzers (3.3.4+)
- [ ] T003 Configure project as source generator (set OutputItemType="Analyzer", IncludeBuildOutput="false")
- [ ] T004 [P] Create `RealizeServiceAttribute.cs` in `dotnet/framework/LablabBean.Plugins.Contracts/Attributes/`
- [ ] T005 [P] Create `SelectionStrategyAttribute.cs` in `dotnet/framework/LablabBean.Plugins.Contracts/Attributes/`
- [ ] T006 Create test project `LablabBean.SourceGenerators.Proxy.Tests` in `dotnet/tests/LablabBean.SourceGenerators.Proxy.Tests/`

**Checkpoint**: Projects build, attributes available, generator infrastructure ready

---

## Phase 1: Basic Method Generation (12 tasks)

**Purpose**: Generate simple method implementations (no generics, no ref/out)

### Generator Core

- [ ] T007 Create `ProxyServiceGenerator.cs` implementing `IIncrementalGenerator` in `dotnet/framework/LablabBean.SourceGenerators.Proxy/`
- [ ] T008 Implement `Initialize()` method with incremental generation pipeline
- [ ] T009 Create syntax provider to find partial classes with `[RealizeService]` attribute
- [ ] T010 Extract interface type from `[RealizeService(typeof(IService))]` attribute argument

### Validation

- [ ] T011 Validate that target type is an interface (generate diagnostic if not)
- [ ] T012 Validate that partial class has `IRegistry _registry` field (generate diagnostic if missing)
- [ ] T013 Validate that interface is accessible from the partial class

### Code Generation

- [ ] T014 Generate partial class with same name and namespace as input class
- [ ] T015 Generate method implementations for simple methods (value parameters, value return types)
- [ ] T016 Generate delegation code: `return _registry.Get<T>().Method(arg1, arg2);`
- [ ] T017 Handle void methods (no return statement): `_registry.Get<T>().Method(args);`

### Testing

- [ ] T018 [P] Write test: Generator finds partial class with `[RealizeService]`
- [ ] T019 [P] Write test: Generator creates method implementation for simple interface
- [ ] T020 [P] Write test: Generated method delegates correctly
- [ ] T021 [P] Write test: Diagnostic when `_registry` field is missing
- [ ] T022 [P] Write test: Diagnostic when target is not an interface

**Checkpoint**: Basic method generation working, simple interfaces can be proxied

---

## Phase 2: Property and Event Generation (10 tasks)

**Purpose**: Generate property and event implementations

### Property Generation

- [ ] T023 Generate property getter implementations: `get => _registry.Get<T>().PropertyName;`
- [ ] T024 Generate property setter implementations: `set => _registry.Get<T>().PropertyName = value;`
- [ ] T025 Handle read-only properties (get-only, no setter)
- [ ] T026 Handle write-only properties (set-only, no getter)
- [ ] T027 Handle auto-property syntax in generated code

### Event Generation

- [ ] T028 Generate event add accessor: `add => _registry.Get<T>().EventName += value;`
- [ ] T029 Generate event remove accessor: `remove => _registry.Get<T>().EventName -= value;`

### Testing

- [ ] T030 [P] Write test: Property getter generation
- [ ] T031 [P] Write test: Property setter generation
- [ ] T032 [P] Write test: Event add/remove generation
- [ ] T033 [P] Write test: Read-only and write-only properties

**Checkpoint**: Properties and events generate correctly

---

## Phase 3: Advanced Method Features (14 tasks)

**Purpose**: Support generics, constraints, ref/out, async, params

### Generic Methods

- [ ] T034 Generate generic methods with type parameters: `T Method<T>()`
- [ ] T035 Preserve type constraints: `where T : class, new()`
- [ ] T036 Handle multiple type parameters: `TResult Method<T1, T2, TResult>()`
- [ ] T037 Handle nested generic types: `Task<List<T>>`

### Parameter Modifiers

- [ ] T038 Handle ref parameters: `ref int value` → `_registry.Get<T>().Method(ref value)`
- [ ] T039 Handle out parameters: `out string result` → `_registry.Get<T>().Method(out result)`
- [ ] T040 Handle in parameters: `in ReadOnlySpan<byte>` → `_registry.Get<T>().Method(in data)`
- [ ] T041 Handle params arrays: `params string[] args` → `_registry.Get<T>().Method(args)`

### Async and Defaults

- [ ] T042 Handle async methods returning Task: `async Task Method()` → `return _registry.Get<T>().Method();`
- [ ] T043 Handle async methods returning Task<T>: `async Task<int> Method()` → `return _registry.Get<T>().Method();`
- [ ] T044 Preserve default parameter values: `Method(int x = 10)` → `Method(int x = 10)`

### Testing

- [ ] T045 [P] Write test: Generic method generation
- [ ] T046 [P] Write test: Type constraints preserved
- [ ] T047 [P] Write test: Ref/out parameters work correctly
- [ ] T048 [P] Write test: Async methods generate correctly
- [ ] T049 [P] Write test: Params arrays work
- [ ] T050 [P] Write test: Default parameter values preserved

**Checkpoint**: All advanced C# method features supported

---

## Phase 4: Selection Strategy Support (8 tasks)

**Purpose**: Support `[SelectionStrategy]` attribute for controlling service retrieval

### Strategy Detection

- [ ] T051 Read `[SelectionStrategy]` attribute from partial class
- [ ] T052 Extract SelectionMode enum value from attribute
- [ ] T053 Handle missing `[SelectionStrategy]` (default to no explicit mode)

### Code Generation

- [ ] T054 Generate code with SelectionMode.One: `_registry.Get<T>(SelectionMode.One)`
- [ ] T055 Generate code with SelectionMode.HighestPriority: `_registry.Get<T>(SelectionMode.HighestPriority)`
- [ ] T056 Generate code with SelectionMode.All: `_registry.GetAll<T>()`
- [ ] T057 Generate code with no strategy: `_registry.Get<T>()` (uses default)

### Testing

- [ ] T058 [P] Write test: SelectionMode.One generates correct code
- [ ] T059 [P] Write test: SelectionMode.HighestPriority generates correct code
- [ ] T060 [P] Write test: SelectionMode.All generates correct code
- [ ] T061 [P] Write test: No strategy uses default behavior

**Checkpoint**: Selection strategies work correctly

---

## Phase 5: Nullable and Code Quality (10 tasks)

**Purpose**: Ensure generated code passes all quality checks

### Nullable Reference Types

- [ ] T062 Add `#nullable enable` directive to generated files
- [ ] T063 Preserve nullable annotations from interface: `string?` → `string?`
- [ ] T064 Handle nullable return types: `Task<string?>` → `Task<string?>`
- [ ] T065 Handle nullable parameters: `void Method(string? value)` → `void Method(string? value)`

### Code Quality

- [ ] T066 Add `// <auto-generated />` comment at top of generated file
- [ ] T067 Add generator version and timestamp to header comment
- [ ] T068 Copy XML documentation comments from interface to generated implementation
- [ ] T069 Format generated code with proper indentation (4 spaces)
- [ ] T070 Add blank lines between members for readability

### Testing

- [ ] T071 [P] Write test: Nullable annotations preserved
- [ ] T072 [P] Write test: Generated code compiles without warnings
- [ ] T073 [P] Write test: XML documentation copied correctly
- [ ] T074 [P] Write snapshot test: Verify generated code format

**Checkpoint**: Generated code is high quality and warning-free

---

## Phase 6: Error Handling and Diagnostics (10 tasks)

**Purpose**: Provide helpful error messages for common mistakes

### Diagnostic Definitions

- [ ] T075 Create diagnostic descriptor: Missing `_registry` field (error, with fix suggestion)
- [ ] T076 Create diagnostic descriptor: Target type is not an interface (error)
- [ ] T077 Create diagnostic descriptor: Interface not accessible (error)
- [ ] T078 Create diagnostic descriptor: Duplicate `[RealizeService]` on same class (warning)
- [ ] T079 Create diagnostic descriptor: Interface has no members (warning)

### Diagnostic Reporting

- [ ] T080 Report diagnostic when `_registry` field is missing
- [ ] T081 Report diagnostic when target type is not an interface
- [ ] T082 Report diagnostic when interface is not accessible
- [ ] T083 Ensure generator doesn't crash on invalid input (graceful degradation)

### Testing

- [ ] T084 [P] Write test: Missing `_registry` field produces error
- [ ] T085 [P] Write test: Non-interface target produces error
- [ ] T086 [P] Write test: Inaccessible interface produces error
- [ ] T087 [P] Write test: Generator handles invalid syntax gracefully

**Checkpoint**: All error cases produce clear diagnostics

---

## Phase 7: Integration and Documentation (12 tasks)

**Purpose**: Integrate with existing codebase and document usage

### Example Implementation

- [ ] T088 Create example proxy service in `plugins/examples/ProxyServiceExample/`
- [ ] T089 Mark example class with `[RealizeService(typeof(Game.Services.IService))]`
- [ ] T090 Add `[SelectionStrategy(SelectionMode.HighestPriority)]`
- [ ] T091 Verify example builds and generated code works

### Documentation

- [ ] T092 Create `USAGE.md` with step-by-step guide in `specs/009-proxy-service-source-generator/`
- [ ] T093 Add example: Basic proxy service (5 methods)
- [ ] T094 Add example: Proxy with properties and events
- [ ] T095 Add example: Proxy with generic methods
- [ ] T096 Document common errors and solutions
- [ ] T097 Add troubleshooting section

### Completion

- [ ] T098 Update CHANGELOG.md with source generator feature
- [ ] T099 Create COMPLETION.md documenting success criteria validation
- [ ] T100 Update spec.md status to Complete
- [ ] T101 Run all tests and verify 100% pass

**Checkpoint**: Documentation complete, feature ready for production

---

## Summary

**Total Tasks**: 101
**Estimated Time**: 20-30 hours (2.5-4 days)

**Phase Breakdown**:
- Phase 0: Setup (6 tasks, 1-2 hours)
- Phase 1: Basic Methods (12 tasks, 4-6 hours)
- Phase 2: Properties/Events (10 tasks, 3-4 hours)
- Phase 3: Advanced Features (14 tasks, 4-6 hours)
- Phase 4: Selection Strategy (8 tasks, 2-3 hours)
- Phase 5: Code Quality (10 tasks, 3-4 hours)
- Phase 6: Diagnostics (10 tasks, 3-4 hours)
- Phase 7: Integration (12 tasks, 3-4 hours)

**Dependencies**:
- Spec 007 (IRegistry, SelectionMode) ✅ Complete
- Spec 008 (Service interfaces) ✅ Complete
- Roslyn SDK (NuGet package)

**Success Criteria**:
- All 101 tasks complete
- All tests passing
- Generated code compiles without warnings
- Documentation with 3+ examples
- Build time impact <1 second
