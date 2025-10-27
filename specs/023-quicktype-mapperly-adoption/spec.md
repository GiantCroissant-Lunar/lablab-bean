# Feature Specification: quicktype and Mapperly Production Adoption

**Feature Branch**: `023-quicktype-mapperly-adoption`
**Created**: 2025-10-27
**Status**: Draft
**Input**: User description: "adopt the use of quicktype and mapperly"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - External API Integration with Type Safety (Priority: P1)

A developer working with the Qdrant vector store API needs to parse API responses reliably without manual JSON parsing. Currently, the code uses manual `JsonDocument` parsing with string-based property access, which is error-prone and lacks compile-time type safety. After migration, developers should use auto-generated strongly-typed models that provide IntelliSense support and catch errors at compile time.

**Why this priority**: This is the highest-value migration because external API parsing is the most brittle and error-prone code in the system. The Qdrant plugin is actively used in production and manual JSON parsing has the highest risk of runtime failures from API changes.

**Independent Test**: Can be fully tested by executing Qdrant vector search operations and verifying that results are parsed correctly using generated types. Success is demonstrated when all Qdrant integration tests pass with zero manual JSON parsing code remaining.

**Acceptance Scenarios**:

1. **Given** a developer needs to call the Qdrant search API, **When** they examine the code, **Then** they see strongly-typed request and response models with IntelliSense support
2. **Given** the Qdrant API returns search results, **When** the response is parsed, **Then** the system uses generated `QdrantSearchResponse` models instead of manual `JsonDocument` parsing
3. **Given** a developer adds a new Qdrant API endpoint, **When** they need response types, **Then** they can generate models from JSON Schema or sample responses using the documented build process
4. **Given** the Qdrant API response structure changes, **When** the developer regenerates types from the updated schema, **Then** compile-time errors immediately identify all affected code locations

---

### User Story 2 - Internal Object Mapping with Consistency (Priority: P2)

A developer working with AI.Actors persistence needs to convert between domain objects (e.g., `AvatarState` to `AvatarStateSnapshot`) for serialization. Currently, this uses manual property-by-property assignment which is tedious, error-prone, and becomes inconsistent when properties are added or renamed. After migration, developers should use Mapperly-generated mappers that automatically handle property mapping with compile-time validation.

**Why this priority**: Internal object mapping is the second highest priority because it affects multiple framework components (AI.Actors, persistence, API layers). While less risky than external API parsing, manual mapping creates significant maintenance burden and bugs slip through when object structures evolve.

**Independent Test**: Can be fully tested by executing avatar state serialization/deserialization flows and verifying that all properties are correctly mapped. Success is demonstrated when persistence integration tests pass and the `AvatarStateSerializer` uses Mapperly-generated mappers instead of manual `Create()` methods.

**Acceptance Scenarios**:

1. **Given** a developer needs to map `AvatarState` to `AvatarStateSnapshot`, **When** they examine the code, **Then** they see a Mapperly `[Mapper]` interface with partial method declarations
2. **Given** the system needs to persist avatar state, **When** the mapping executes, **Then** Mapperly-generated code automatically copies all matching properties without manual assignment
3. **Given** a developer adds a new property to `AvatarState`, **When** they rebuild the project, **Then** Mapperly automatically includes the new property in the mapping without manual code changes
4. **Given** property types are incompatible (e.g., `string` to `int`), **When** the developer compiles the project, **Then** Mapperly generates compile-time errors identifying the type mismatch
5. **Given** complex nested objects require mapping, **When** the developer configures nested mapper relationships, **Then** Mapperly handles deep object graphs automatically

---

### User Story 3 - Future External API Integrations (Priority: P3)

A developer adding a new external API integration (e.g., Firebase, third-party analytics) needs to quickly create strongly-typed models without writing manual parsing code. After establishing the adoption pattern, developers should have documented workflows for generating types from JSON Schema, OpenAPI specs, or sample responses, and integrating them into the build process.

**Why this priority**: This is the lowest priority because it addresses future integrations rather than existing production code. However, establishing the pattern now prevents accumulation of new technical debt and makes future integrations faster and more reliable.

**Independent Test**: Can be fully tested by adding a new mock external API integration, generating types following the documented process, and verifying the generated models work correctly with sample API responses. Success is demonstrated when a developer can complete a new integration in under 2 hours with zero manual JSON parsing code.

**Acceptance Scenarios**:

1. **Given** a developer needs to integrate a new external API, **When** they consult the documentation, **Then** they find clear step-by-step instructions for generating types with quicktype
2. **Given** the API provides OpenAPI/Swagger documentation, **When** the developer runs the type generation command, **Then** the build system automatically generates C# models from the specification
3. **Given** the API only provides sample JSON responses, **When** the developer saves samples to the schemas directory, **Then** quicktype infers types and generates models from the examples
4. **Given** types are generated, **When** the project builds, **Then** the Framework.Generated project is automatically referenced and models are available for import
5. **Given** the external API has breaking changes, **When** types are regenerated, **Then** compile-time errors identify all affected integration points

---

### Edge Cases

- **Deeply nested JSON structures**: What happens when API responses contain 5+ levels of nested objects? Can quicktype handle this and generate usable models?
- **Circular references in object graphs**: How does Mapperly handle scenarios where `Avatar` references `Team` and `Team` references `Avatar[]`?
- **Polymorphic JSON responses**: What happens when an API returns different object shapes based on a discriminator field (e.g., `type: "user"` vs `type: "admin"`)?
- **Custom mapping logic requirements**: How does Mapperly handle scenarios requiring custom transformation logic (e.g., converting timestamps, normalizing strings, computing derived properties)?
- **Large array responses with performance concerns**: What happens when Qdrant returns 10,000+ search results? Does the generated parsing code have acceptable memory/performance characteristics?
- **Null handling and optional properties**: How does the generated code handle JSON properties that are sometimes missing vs explicitly `null`?
- **Build process failures**: What happens when quicktype generation fails mid-build? Does the build process fail gracefully with clear error messages?
- **Versioning of external API models**: How do we maintain backward compatibility when regenerating types for API version upgrades?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST replace all manual `JsonDocument` parsing in the Qdrant plugin with quicktype-generated strongly-typed models
- **FR-002**: System MUST replace all manual property assignment in `AvatarStateSerializer.Create()` with Mapperly-generated mappers
- **FR-003**: Build process MUST automatically generate types from JSON schemas located in the `schemas/` directory before compiling application code
- **FR-004**: Generated models MUST be placed in the `LablabBean.Framework.Generated` project and automatically referenced by consuming projects
- **FR-005**: All existing integration tests for Qdrant vector store operations MUST pass after migration with no test modifications required
- **FR-006**: All existing integration tests for AI.Actors persistence MUST pass after migration with no test modifications required
- **FR-007**: Documentation MUST be updated to reflect actual production usage with concrete examples from the migrated code
- **FR-008**: Developers MUST have a documented workflow for adding new external API integrations using quicktype
- **FR-009**: Developers MUST have a documented workflow for adding new internal mappers using Mapperly
- **FR-010**: System MUST maintain current functionality and behavior - migration is a refactoring with no user-visible changes
- **FR-011**: Build process MUST fail with clear error messages if type generation fails or generated code doesn't compile
- **FR-012**: Project references MUST be correctly configured so that Qdrant plugin and AI.Actors projects can import generated types
- **FR-013**: Generated code MUST handle nullable and optional properties consistently with existing parsing behavior
- **FR-014**: Mapperly configurations MUST be documented with examples covering common scenarios (nested objects, collections, custom mappings)
- **FR-015**: System MUST remove all unused example/demo code after production migration is verified (ExampleMapper.cs, MapperlyExamples.cs)

### Key Entities

- **Generated Type Models**: Strongly-typed C# classes representing external API request/response structures (e.g., `QdrantSearchResponse`, `QdrantPoint`). These are generated from JSON schemas or sample responses and live in Framework.Generated.
- **Mapper Interfaces**: Partial interfaces decorated with `[Mapper]` attributes that Mapperly uses to generate mapping code (e.g., `IAvatarMapper`). Developers define the interface signatures, Mapperly generates the implementations.
- **JSON Schemas**: JSON Schema or sample JSON files stored in `schemas/` directory that serve as input to quicktype for type generation. These represent the source of truth for external API contracts.
- **Build Integration Points**: NUKE build targets and MSBuild configurations that orchestrate type generation before compilation. This includes the `GenerateApiTypes` target and project dependencies.
- **Mapping Configurations**: Attributes and configuration code that customize Mapperly behavior (e.g., `[MapProperty]`, `[MapperIgnore]`, custom value converters). These handle edge cases where automatic mapping needs guidance.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero manual `JsonDocument.ParseAsync` calls remain in the Qdrant plugin (currently 1 instance at QdrantVectorStore.cs:64-93)
- **SC-002**: Zero manual property assignment mapping methods remain in AI.Actors (currently `AvatarStateSerializer.Create()` performs manual mapping)
- **SC-003**: At least two production projects (Qdrant plugin and AI.Actors) successfully reference and use `LablabBean.Framework.Generated`
- **SC-004**: All existing integration tests pass after migration (100% pass rate maintained)
- **SC-005**: Build process successfully generates types on every clean build without manual intervention
- **SC-006**: Documentation includes at least two concrete production examples (one quicktype, one Mapperly) from the migrated code
- **SC-007**: Lines of manual JSON parsing and mapping code reduced by at least 60% in affected files
- **SC-008**: New developers can add a new external API integration in under 2 hours following documented process (measurable via onboarding time tracking)
- **SC-009**: Build time increases by no more than 10 seconds due to type generation (measured from clean build baseline)
- **SC-010**: Code review feedback mentions "manual JSON parsing" or "manual mapping" issues zero times in the 3 months following adoption (quality metric)

## Assumptions

- The existing Qdrant API contract is stable and documented with JSON schemas or reliable sample responses
- The current test coverage for Qdrant and AI.Actors is sufficient to catch regression issues
- The NUKE build system is the authoritative build process and can be modified
- Developers have access to PowerShell or Bash for running build scripts
- The Framework.Generated project structure is already correct and only needs project references added
- Performance characteristics of generated code are acceptable for production use (can be validated during migration)
- The team agrees that generated code is preferable to manual code even if it's more verbose

## Dependencies

- Mapperly NuGet package (v4.1.0 already installed)
- Newtonsoft.Json (v13.0.3 already installed) or System.Text.Json for deserialization
- quicktype CLI tool (already integrated in NUKE build)
- NUKE build system configured and functional
- Existing test infrastructure for Qdrant and AI.Actors

## Out of Scope

- Migrating projects beyond Qdrant plugin and AI.Actors (can be done in future iterations)
- Optimizing build process performance beyond ensuring no major regression
- Creating new API integrations (only establishing the pattern for future use)
- Changing external API contracts or internal domain models (pure refactoring)
- Upgrading or changing Mapperly/quicktype versions (use existing versions)
- Performance tuning of generated code (unless critical issues discovered during migration)
- Adding new features to the Qdrant plugin or AI.Actors functionality
