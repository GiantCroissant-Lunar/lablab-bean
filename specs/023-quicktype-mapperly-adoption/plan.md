# Implementation Plan: quicktype and Mapperly Production Adoption

**Branch**: `023-quicktype-mapperly-adoption` | **Date**: 2025-10-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/023-quicktype-mapperly-adoption/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

This plan outlines the migration from manual JSON parsing and object mapping to automated code generation using quicktype (for external API models) and Mapperly (for internal object mapping). The primary focus is migrating the Qdrant vector store plugin and AI.Actors persistence layer from brittle manual code to maintainable, type-safe generated code. Infrastructure already exists (packages installed, Framework.Generated project created, build integration configured), but zero production code currently uses these tools. This is a pure refactoring effort with no functional changes - all existing tests must pass.

## Technical Context

**Language/Version**: C# / .NET 8.0 (SDK 9.0.111 available)
**Primary Dependencies**:

- Riok.Mapperly 4.1.0 (installed, source generator)
- Newtonsoft.Json 13.0.3 (installed, JSON serialization)
- System.Text.Json 8.0.5 (installed, alternative JSON serialization)
- quicktype CLI (integrated in NUKE build)
**Storage**: Akka.NET Persistence.Sql, Firebase Admin SDK, Qdrant vector database
**Testing**: xunit 2.5.3, FluentAssertions 6.12.0, Moq 4.20.70, NSubstitute 5.1.0
**Target Platform**: Windows/Linux cross-platform (.NET runtime)
**Project Type**: Multi-project solution - game engine with plugin architecture
**Performance Goals**: No regression in API response times (currently sub-500ms for Qdrant operations)
**Constraints**:
- All existing integration tests must pass unchanged
- Build time increase ≤10 seconds
- Zero breaking changes to public APIs
- Framework.Generated must remain isolated (no reverse dependencies)
**Scale/Scope**:
- 2 production projects to migrate (Qdrant plugin, AI.Actors)
- ~300 lines of manual parsing/mapping code to replace
- 10+ integration tests to validate
- 3-5 external API endpoints to model

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Note**: Constitution template exists but is not yet configured for this project. The following checks apply standard software engineering principles:

### Standard Engineering Gates

**✅ PASS** - **Test Coverage Preservation**: All existing integration tests for Qdrant and AI.Actors must pass after migration

- **Justification**: This is a refactoring effort - functional behavior must remain identical
- **Verification**: Run full test suite before and after each migration step

**✅ PASS** - **Isolated Dependencies**: Framework.Generated project has no reverse dependencies (only consumed by other projects)

- **Justification**: Generated code is a utility layer, not a core domain dependency
- **Verification**: Check project references - no projects should depend on Framework.Generated for domain logic

**✅ PASS** - **Build Process Determinism**: Type generation must be idempotent (same input → same output)

- **Justification**: Prevents spurious diffs and build inconsistencies
- **Verification**: Generate types twice, compare output files

**⚠️ CONDITIONAL** - **Performance Regression**: Generated code may have different performance characteristics than hand-optimized parsing

- **Justification**: Type safety and maintainability justify minor performance trade-offs
- **Mitigation**: Benchmark critical paths before/after migration, reject if >10% regression
- **Verification**: Run BenchmarkDotNet tests on Qdrant search operations

**✅ PASS** - **No External API Changes**: Migration is internal refactoring only

- **Justification**: Zero functional changes, only implementation swaps
- **Verification**: Public API surface remains unchanged (verify with API compatibility tools)

### Re-evaluation After Phase 1 Design

*This section will be updated after Phase 1 design completes*

## Project Structure

### Documentation (this feature)

```
specs/023-quicktype-mapperly-adoption/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (technology decisions)
├── data-model.md        # Phase 1 output (entity models and mappings)
├── quickstart.md        # Phase 1 output (developer onboarding guide)
├── contracts/           # Phase 1 output (JSON schemas for quicktype)
│   ├── qdrant-search-request.json
│   ├── qdrant-search-response.json
│   └── qdrant-point.json
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/
├── framework/
│   ├── LablabBean.Framework.Generated/    # ← Target project for generated code
│   │   ├── Models/                         # ← quicktype-generated API models
│   │   │   ├── Qdrant/
│   │   │   │   ├── QdrantSearchRequest.g.cs
│   │   │   │   ├── QdrantSearchResponse.g.cs
│   │   │   │   └── QdrantPoint.g.cs
│   │   │   └── README.md                   # Generation instructions
│   │   ├── Mappers/                        # ← Mapperly mapper interfaces
│   │   │   ├── AvatarMapper.cs             # IAvatarMapper partial interface
│   │   │   └── README.md                   # Mapper usage guide
│   │   ├── Tests/                          # Integration tests for generated code
│   │   └── LablabBean.Framework.Generated.csproj
│   └── LablabBean.AI.Actors/               # ← Migration target (Mapperly)
│       └── Persistence/
│           └── AvatarStateSerializer.cs    # Will use IAvatarMapper
├── plugins/
│   └── LablabBean.Plugins.VectorStore.Qdrant/  # ← Migration target (quicktype)
│       └── QdrantVectorStore.cs            # Will use generated models
└── LablabBean.sln

schemas/                                     # ← JSON schemas for quicktype input
├── qdrant/
│   ├── search-request.schema.json
│   ├── search-response.schema.json
│   └── point.schema.json
└── README.md                                # Schema maintenance guide

build/nuke/
└── Build.cs                                 # NUKE build with GenerateApiTypes target
```

**Structure Decision**: This solution uses a multi-project architecture with a dedicated Framework.Generated project for all generated code (both quicktype models and Mapperly mappers). The `schemas/` directory at repository root stores JSON schemas for external APIs. Migration will focus on two existing projects:

1. **Qdrant Plugin** (quicktype) - External API integration requiring JSON parsing
2. **AI.Actors** (Mapperly) - Internal domain mapping for persistence

The build process (NUKE) already has infrastructure for type generation but it's not yet connected to production code.

## Complexity Tracking

*No constitution violations - this section is empty*

## Phase 0: Research & Technology Decisions

**Objective**: Resolve technical unknowns and document decisions for Mapperly and quicktype adoption in the .NET 8 codebase.

### Research Questions

1. **Qdrant API Contract Capture**: How do we obtain accurate JSON schemas for Qdrant's search API (request/response structures)?
   - Option A: Use Qdrant's OpenAPI/Swagger spec if available
   - Option B: Capture live API responses and infer schema with quicktype
   - Option C: Manually write JSON Schema based on API documentation

2. **JSON Serialization Choice**: Should we use System.Text.Json or Newtonsoft.Json for deserialization with quicktype-generated models?
   - System.Text.Json: Native .NET, better performance, already in use
   - Newtonsoft.Json: More features, flexible, already in dependencies
   - Both are available - need consistency guidance

3. **Mapperly Configuration for Complex Scenarios**: How do we handle edge cases in avatar state mapping?
   - Nested object mapping (Avatar → AvatarSnapshot with nested properties)
   - Collection mapping (List<T> transformations)
   - Custom value converters (DateTime formats, enums)

4. **Build Integration Details**: How does the existing NUKE GenerateApiTypes target work and what modifications are needed?
   - Current: Generates examples only
   - Needed: Generate production models and trigger before compilation

5. **Handling Null and Optional Properties**: How should generated code handle missing/null JSON properties to match existing behavior?
   - Qdrant API may omit properties or return explicit nulls
   - Need consistent nullable reference type annotations

### Research Outputs

*Phase 0 will generate `research.md` with documented decisions for each question above. Each decision will include:*

- **Decision**: What was chosen
- **Rationale**: Why this option was selected
- **Alternatives Considered**: What else was evaluated
- **Implementation Guidance**: Concrete next steps

**Dependencies**: Read Qdrant API documentation, examine existing QdrantVectorStore.cs parsing logic, review Mapperly documentation for advanced scenarios.

## Phase 1: Design & Contracts

**Prerequisites**: `research.md` complete with all decisions documented

### 1.1 Data Model Design (`data-model.md`)

**Objective**: Document all entities involved in the migration and their transformation mappings.

**Key Entities from Spec**:

1. **Generated Type Models** (quicktype output)
   - `QdrantSearchRequest`: Request structure for Qdrant search API
   - `QdrantSearchResponse`: Response wrapper containing result array
   - `QdrantPoint`: Individual search result with id, score, payload, vector
   - All models include proper nullable annotations matching API behavior

2. **Mapper Interfaces** (Mapperly input)
   - `IAvatarMapper`: Maps between `AvatarState` and `AvatarStateSnapshot`
   - Includes configuration attributes for custom mappings
   - Handles nested object graphs and collections

3. **Mapping Relationships**:
   - `AvatarState` (domain) → `AvatarStateSnapshot` (persistence DTO)
   - Bidirectional if needed for deserialization
   - Property name mappings (if source/target names differ)

**Output**: `data-model.md` with entity definitions, field lists, validation rules, and mapping configurations.

### 1.2 API Contracts (`contracts/` directory)

**Objective**: Create JSON schemas for all external APIs to feed into quicktype generation.

**Contracts to Create**:

1. **`qdrant-search-request.json`**: JSON Schema for Qdrant search request

   ```json
   {
     "$schema": "http://json-schema.org/draft-07/schema#",
     "title": "QdrantSearchRequest",
     "type": "object",
     "properties": {
       "vector": { "type": "array", "items": { "type": "number" } },
       "limit": { "type": "integer" },
       "with_payload": { "type": "boolean" },
       "with_vector": { "type": "boolean" }
     },
     "required": ["vector"]
   }
   ```

2. **`qdrant-search-response.json`**: JSON Schema for Qdrant search response
   - Based on actual API responses captured in research phase
   - Includes nested `result[]` array with score, id, payload

3. **`qdrant-point.json`**: JSON Schema for individual search result points
   - Extracted from response schema for reusability

**Generation Commands** (documented for developers):

```bash
# Generate C# models from schemas
quicktype --src schemas/qdrant/*.json \
  --lang csharp \
  --namespace LablabBean.Framework.Generated.Models.Qdrant \
  --out dotnet/framework/LablabBean.Framework.Generated/Models/Qdrant/ \
  --features complete
```

**Output**: 3+ JSON schema files in `contracts/` directory with clear documentation headers.

### 1.3 Quickstart Guide (`quickstart.md`)

**Objective**: Provide developers with a simple onboarding guide for using and extending generated code.

**Content Outline**:

1. **For Users of Generated Code**:
   - How to import generated Qdrant models
   - How to use IAvatarMapper in persistence code
   - Example code snippets (before/after comparison)

2. **For Adding New External APIs**:
   - Step 1: Capture API responses or obtain OpenAPI spec
   - Step 2: Create JSON Schema in `schemas/`
   - Step 3: Run quicktype generation (or trigger build)
   - Step 4: Add project reference to Framework.Generated
   - Step 5: Replace manual parsing with generated models

3. **For Adding New Mappers**:
   - Step 1: Create partial interface with `[Mapper]` attribute
   - Step 2: Define mapping method signatures
   - Step 3: Add configuration attributes for edge cases
   - Step 4: Build project (Mapperly generates implementation)
   - Step 5: Inject and use mapper via DI

4. **Troubleshooting Common Issues**:
   - Build failures (regeneration needed)
   - Nullable reference warnings
   - Custom mapping scenarios

**Output**: `quickstart.md` with code examples and step-by-step workflows.

### 1.4 Agent Context Update

**Objective**: Update Claude Code's project context with newly adopted technologies.

**Command**:

```powershell
.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude
```

**What Gets Added**:

- "quicktype for external API model generation"
- "Mapperly for internal object mapping"
- "Framework.Generated project for all generated code"
- "JSON schemas in schemas/ directory"

**Output**: Updated `.agent/adapters/claude.md` or equivalent agent context file.

### 1.5 Constitution Check Re-evaluation

After Phase 1 design artifacts are created, re-evaluate the gates:

**✅ Test Coverage Preservation**: Design maintains all existing test interfaces
**✅ Isolated Dependencies**: Framework.Generated has no new reverse dependencies
**✅ Build Determinism**: quicktype and Mapperly are deterministic generators
**⚠️ Performance**: Will be validated during Phase 2 implementation with benchmarks
**✅ API Compatibility**: No public API changes in design

**Status**: All gates remain green, ready for Phase 2 task generation.

## Phase 2: Task Generation

**Prerequisites**:

- `research.md` complete
- `data-model.md` complete
- `contracts/` directory populated
- `quickstart.md` complete
- Agent context updated
- Constitution re-check passed

**Command**: `/speckit.tasks`

**Note**: This phase is executed by a separate command and is NOT part of `/speckit.plan`. The `/speckit.tasks` command will:

1. Read all Phase 0 and Phase 1 artifacts
2. Read the original feature spec
3. Generate a dependency-ordered `tasks.md` file with concrete implementation tasks

**Expected Task Structure** (preview):

- **Phase 1**: Schema Capture and Validation
  - Capture Qdrant API responses
  - Create JSON schemas in schemas/qdrant/
  - Validate schemas against live API

- **Phase 2**: Build Integration
  - Update NUKE GenerateApiTypes target
  - Configure quicktype for production generation
  - Add build dependencies (generate before compile)

- **Phase 3**: Qdrant Plugin Migration (quicktype)
  - Generate Qdrant models from schemas
  - Add Framework.Generated project reference
  - Replace manual JsonDocument parsing
  - Run integration tests

- **Phase 4**: AI.Actors Migration (Mapperly)
  - Create IAvatarMapper interface
  - Configure Mapperly attributes
  - Replace manual AvatarStateSnapshot.Create()
  - Run persistence integration tests

- **Phase 5**: Cleanup and Documentation
  - Remove example/demo code
  - Update production documentation
  - Benchmark performance
  - Final validation

**Output**: `tasks.md` generated by `/speckit.tasks` (not by this plan command).

## Success Validation

After Phase 2 implementation (when `/speckit.implement` is run), validate against spec success criteria:

**Measurable Outcomes** (from spec):

- [ ] **SC-001**: Zero manual `JsonDocument.ParseAsync` calls in Qdrant plugin
- [ ] **SC-002**: Zero manual property assignment in AI.Actors
- [ ] **SC-003**: Two production projects reference Framework.Generated
- [ ] **SC-004**: All integration tests pass (100% rate)
- [ ] **SC-005**: Build generates types automatically
- [ ] **SC-006**: Documentation has two production examples
- [ ] **SC-007**: Manual code reduced by ≥60% in affected files
- [ ] **SC-008**: New API integration takes <2 hours (validated during onboarding)
- [ ] **SC-009**: Build time increase ≤10 seconds
- [ ] **SC-010**: Zero code review mentions of manual parsing (3-month metric)

**Validation Methods**:

- Automated: Grep for `JsonDocument.Parse`, count lines changed, run tests
- Manual: Review code diffs, time new integration workflow
- Long-term: Monitor code reviews for 3 months post-adoption

## Risk Mitigation

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Generated code has bugs | Medium | High | Extensive integration testing, manual review of generated output |
| Performance regression | Low | Medium | Benchmark critical paths, optimize if >10% slower |
| Build complexity increase | Medium | Low | Clear documentation, fail-fast error messages |
| Qdrant API schema inaccuracy | Medium | High | Validate schemas against live API, capture multiple response samples |
| Mapperly edge cases | Low | Medium | Research phase identifies complex scenarios, add custom converters |
| Developer adoption resistance | Low | Low | Clear quickstart guide, demonstrate time savings |

## Next Steps

1. **Continue Planning**: Review this plan with stakeholders, adjust if needed
2. **Execute Phase 0**: Run research workflow (automated agent tasks for each question)
3. **Execute Phase 1**: Generate design artifacts (data-model, contracts, quickstart)
4. **Gate Check**: Re-evaluate constitution compliance post-design
5. **Generate Tasks**: Run `/speckit.tasks` to create implementation task list
6. **Implement**: Execute tasks via `/speckit.implement` or manually

**Current Status**: ✅ Plan complete, ready for Phase 0 research

---

**Plan Version**: 1.0
**Last Updated**: 2025-10-27
**Branch**: 023-quicktype-mapperly-adoption
