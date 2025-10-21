# Feature Specification: Proxy Service Source Generator

**Feature Branch**: `009-proxy-service-source-generator`
**Created**: 2025-10-22
**Status**: Draft
**Prerequisites**: 
- Spec 007 (Tiered Contract Architecture) ✅ Complete
- Spec 008 (Extended Contract Assemblies) ✅ Complete

## Overview

Implement a Roslyn incremental source generator that automatically creates proxy service implementations, eliminating manual delegation boilerplate. Plugin developers mark partial classes with `[RealizeService]` attribute, and the generator creates all interface method/property/event implementations that delegate to `IRegistry`.

**Problem**: Every service interface method requires 3-5 lines of manual proxy delegation code. For contracts with 50+ methods, this is hundreds of lines of repetitive, error-prone boilerplate.

**Solution**: Source generator analyzes partial classes at compile-time and generates proxy implementations automatically, reducing developer effort by 90%+.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Plugin Developer Uses Source Generator for Game Service (Priority: P1)

A plugin developer wants to create a proxy for the game service contract without writing manual delegation code. They need to mark a partial class with `[RealizeService(typeof(IService))]` and have all interface methods automatically implemented.

**Why this priority**: This is the core value proposition - eliminating boilerplate. Without it, developers must manually write delegation code for every interface member.

**Independent Test**: Create a contract with `IService` interface (5 methods), mark a partial class with `[RealizeService(typeof(IService))]`, build the project, and verify the source generator creates implementations for all 5 methods.

**Acceptance Scenarios**:

1. **Given** a partial class marked with `[RealizeService(typeof(Game.Services.IService))]`, **When** the project is built, **Then** the source generator creates proxy implementations for all interface methods
2. **Given** the generated proxy class, **When** a method is called, **Then** it delegates to `_registry.Get<IService>()` and invokes the corresponding method
3. **Given** a service interface with 50 methods, **When** the source generator runs, **Then** all 50 methods are implemented without manual code

---

### User Story 2 - Source Generator Handles Complex Interface Members (Priority: P1)

A plugin developer creates a proxy for a service interface that includes properties, events, generic methods, and ref/out parameters. They need the source generator to handle all C# interface member types correctly.

**Why this priority**: Real service contracts use the full range of C# features. The generator must support them all to be production-ready.

**Independent Test**: Create an interface with properties (get/set), events (add/remove), generic methods with constraints, and methods with ref/out parameters. Verify the generator creates correct implementations for all member types.

**Acceptance Scenarios**:

1. **Given** an interface with properties, **When** the generator runs, **Then** get/set accessors are implemented with proper delegation
2. **Given** an interface with events, **When** the generator runs, **Then** add/remove accessors are implemented correctly
3. **Given** a generic method with type constraints, **When** the generator runs, **Then** the generated method preserves all type parameters and constraints
4. **Given** a method with ref/out parameters, **When** the generator runs, **Then** the generated method correctly passes ref/out to the delegated call

---

### User Story 3 - Source Generator Supports Selection Strategies (Priority: P1)

A plugin developer wants to control how services are retrieved from the registry (One, HighestPriority, All). They need to use `[SelectionStrategy(SelectionMode.HighestPriority)]` attribute to specify the selection mode.

**Why this priority**: Different use cases require different selection strategies. The generator must support all modes defined in `IRegistry`.

**Independent Test**: Create three proxy classes with different selection strategies, verify each generates code using the correct `SelectionMode` when calling `_registry.Get<T>()`.

**Acceptance Scenarios**:

1. **Given** `[SelectionStrategy(SelectionMode.One)]`, **When** the generator runs, **Then** generated methods use `_registry.Get<T>(SelectionMode.One)`
2. **Given** `[SelectionStrategy(SelectionMode.HighestPriority)]`, **When** the generator runs, **Then** generated methods use `_registry.Get<T>(SelectionMode.HighestPriority)`
3. **Given** no `[SelectionStrategy]` attribute, **When** the generator runs, **Then** generated methods use default `_registry.Get<T>()` (defaults to HighestPriority)

---

### Edge Cases

**Source Generator**:
- What happens when a partial class is missing the `IRegistry _registry` field? (Should generate compile error with helpful message)
- How does the generator handle interface methods with default parameter values? (Should preserve default values in generated signature)
- What happens when an interface inherits from multiple base interfaces? (Should generate implementations for all inherited members)
- How does the generator handle nullable reference types? (Should preserve nullable annotations in generated code)
- What happens when the same interface is used in multiple `[RealizeService]` attributes? (Should generate separate implementations for each class)
- How does the generator handle async methods (Task/Task<T> return types)? (Should generate proper async delegation)
- What happens when an interface has indexer properties? (Should generate indexer implementations)

## Requirements *(mandatory)*

### Functional Requirements

#### Source Generator Core (Tier 2)

- **FR-001**: System MUST provide a Roslyn incremental source generator in `LablabBean.SourceGenerators.Proxy` assembly
- **FR-002**: Source generator MUST target .NET Standard 2.0 for maximum compatibility
- **FR-003**: Source generator MUST use incremental generation to minimize build time impact
- **FR-004**: Source generator MUST detect partial classes marked with `[RealizeService(Type)]` attribute
- **FR-005**: Source generator MUST validate that the target type is an interface
- **FR-006**: Source generator MUST validate that the partial class has an `IRegistry _registry` field
- **FR-007**: Source generator MUST generate a compile error if `_registry` field is missing
- **FR-008**: Source generator MUST generate a compile error if target type is not an interface

#### Method Generation

- **FR-009**: Source generator MUST generate implementations for all interface methods (including inherited)
- **FR-010**: Generated methods MUST delegate to `_registry.Get<T>(selectionMode).MethodName(args)`
- **FR-011**: Source generator MUST preserve method signatures exactly (parameters, return types, constraints)
- **FR-012**: Source generator MUST preserve generic type parameters and constraints
- **FR-013**: Source generator MUST preserve nullable reference type annotations
- **FR-014**: Source generator MUST handle methods with default parameter values
- **FR-015**: Source generator MUST handle ref and out parameters correctly
- **FR-016**: Source generator MUST handle async methods (Task/Task<T> return types)
- **FR-017**: Source generator MUST handle methods with params arrays

#### Property Generation

- **FR-018**: Source generator MUST generate implementations for all interface properties
- **FR-019**: Generated property getters MUST delegate to `_registry.Get<T>().PropertyName`
- **FR-020**: Generated property setters MUST delegate to `_registry.Get<T>().PropertyName = value`
- **FR-021**: Source generator MUST handle read-only properties (get-only)
- **FR-022**: Source generator MUST handle write-only properties (set-only)
- **FR-023**: Source generator MUST handle indexer properties

#### Event Generation

- **FR-024**: Source generator MUST generate implementations for all interface events
- **FR-025**: Generated event add accessors MUST delegate to `_registry.Get<T>().EventName += value`
- **FR-026**: Generated event remove accessors MUST delegate to `_registry.Get<T>().EventName -= value`

#### Attributes

- **FR-027**: System MUST provide `[RealizeService(Type)]` attribute in `LablabBean.Plugins.Contracts`
- **FR-028**: System MUST provide `[SelectionStrategy(SelectionMode)]` attribute in `LablabBean.Plugins.Contracts`
- **FR-029**: `[RealizeService]` attribute MUST accept a Type parameter specifying the interface to implement
- **FR-030**: `[SelectionStrategy]` attribute MUST accept a SelectionMode enum value
- **FR-031**: `[SelectionStrategy]` attribute MUST be optional (defaults to HighestPriority)

#### Selection Strategy

- **FR-032**: Source generator MUST read `[SelectionStrategy]` attribute from the partial class
- **FR-033**: Generated code MUST use the specified SelectionMode when calling `_registry.Get<T>()`
- **FR-034**: If no `[SelectionStrategy]` is specified, generated code MUST use `_registry.Get<T>()` (default behavior)
- **FR-035**: Source generator MUST support SelectionMode.One
- **FR-036**: Source generator MUST support SelectionMode.HighestPriority
- **FR-037**: Source generator MUST support SelectionMode.All (for methods returning collections)

#### Code Quality

- **FR-038**: Generated code MUST compile without errors or warnings
- **FR-039**: Generated code MUST pass nullable reference type analysis
- **FR-040**: Generated code MUST include XML documentation comments from interface
- **FR-041**: Generated code MUST include `#nullable enable` directive
- **FR-042**: Generated code MUST include `// <auto-generated />` comment
- **FR-043**: Generated code MUST use proper indentation and formatting

#### Error Handling

- **FR-044**: Source generator MUST report diagnostic errors for invalid usage
- **FR-045**: Diagnostic errors MUST include helpful messages with fix suggestions
- **FR-046**: Source generator MUST not crash on invalid input (graceful degradation)

### Key Entities *(include if feature involves data)*

- **Proxy Service**: A partial class marked with `[RealizeService]` that gets interface implementations generated by the source generator
- **Source Generator**: A Roslyn analyzer that runs at compile-time to generate proxy service implementations
- **RealizeService Attribute**: Marks a partial class for proxy generation, specifying the interface to implement
- **SelectionStrategy Attribute**: Specifies which SelectionMode to use when retrieving services from IRegistry
- **Generated Code**: C# source code created by the generator, implementing interface members with delegation to IRegistry

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Source Generator Core**:
- **SC-001**: Source generator successfully generates proxy implementations for interfaces with 50+ methods without errors
- **SC-002**: Generated proxy code compiles without warnings and passes all nullable reference type checks
- **SC-003**: Developers can create a proxy service in under 30 seconds (write 10 lines of code, generator fills in the rest)
- **SC-004**: Using source generator reduces boilerplate code by at least 90% compared to manual proxy implementation

**Feature Coverage**:
- **SC-005**: Source generator correctly handles all interface member types (methods, properties, events)
- **SC-006**: Source generator preserves generic type parameters and constraints in 100% of test cases
- **SC-007**: Source generator correctly handles ref/out parameters in 100% of test cases
- **SC-008**: Source generator correctly handles async methods in 100% of test cases

**Developer Experience**:
- **SC-009**: Diagnostic errors provide clear, actionable messages for all common mistakes
- **SC-010**: Generated code is readable and matches hand-written code quality
- **SC-011**: Build time impact is less than 1 second for projects with 10 proxy services
- **SC-012**: Documentation includes at least 3 complete examples of using the source generator

## Assumptions

- Developers are familiar with C# partial classes
- Developers understand the IRegistry service locator pattern
- Source generator runs during compilation (not at runtime)
- Generated code is not checked into source control (generated on-demand)
- All service interfaces follow the `IService` naming convention
- The `_registry` field is provided by the developer (not generated)
- Selection strategies are defined in existing `SelectionMode` enum

## Out of Scope

**Not Planned**:
- Runtime proxy generation (compile-time only)
- Proxy generation for abstract classes (interfaces only)
- Automatic `_registry` field generation (developer must provide)
- Proxy generation for non-service interfaces
- Custom delegation logic (always delegates to IRegistry)
- Proxy caching or memoization
- Aspect-oriented programming features (logging, validation, etc.)
- Support for .NET Framework (targets .NET Standard 2.0+)

## Dependencies

- Existing `IRegistry` interface and `SelectionMode` enum
- Microsoft.CodeAnalysis.CSharp (Roslyn SDK)
- .NET Standard 2.0 (for source generator)
- .NET 8 (for target projects)

## Risks

- **Roslyn API complexity**: Source generators are complex to implement correctly. **Mitigation**: Start with simple cases, add complexity incrementally, comprehensive testing.
- **Build time impact**: Source generators can slow down builds. **Mitigation**: Use incremental generation, profile performance, optimize hot paths.
- **Debugging difficulty**: Generated code is harder to debug. **Mitigation**: Generate readable code, include source maps, provide clear diagnostics.
- **Breaking changes in Roslyn**: Roslyn APIs may change between versions. **Mitigation**: Target stable Roslyn version, test across .NET SDK versions.
- **Complex interface scenarios**: Edge cases (indexers, explicit interface implementation) may be missed. **Mitigation**: Comprehensive test suite covering all C# interface features.

## Notes

- This is Tier 2 infrastructure that makes all contracts practical to use
- Without source generator, every interface method requires 3-5 lines of manual delegation
- Source generator eliminates hundreds of lines of boilerplate across the codebase
- Generated code should be indistinguishable from hand-written code in quality
- This feature builds on Spec 007 (Event Bus + IRegistry) and Spec 008 (Contract Assemblies)
