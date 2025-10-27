# Tasks: quicktype and Mapperly Production Adoption (SPEC-023)

Spec: `specs/023-quicktype-mapperly-adoption/spec.md`
Plan: `specs/023-quicktype-mapperly-adoption/plan.md`
Artifacts: `specs/023-quicktype-mapperly-adoption/{research.md,data-model.md,quickstart.md,contracts/}`

Objective: Adopt quicktype for Qdrant models in production and integrate deterministic generation into the build. Per research findings, do not migrate Avatar state serialization to Mapperly in this feature.

Success Criteria Alignment: SC-001, SC-003, SC-004, SC-005, SC-006, SC-007, SC-009, SC-010

Format: `[ID] [P] Description`

- [P]: Runnable in parallel without dependency conflicts

---

## Phase 1: Schema Capture and Validation

Purpose: Capture accurate Qdrant search contracts and validate against samples/live responses.

- [x] T010 Acquire Qdrant OpenAPI spec (local copy) and locate Search endpoints/contracts
- [x] T011 Create JSON Schemas in `specs/023-quicktype-mapperly-adoption/contracts/`
  - `qdrant-search-request.schema.json`
  - `qdrant-search-response.schema.json`
  - `qdrant-scored-point.schema.json`
- [x] T012 Ensure schemas use draft-07, correct required/optional fields, and nullable annotations (C# NRT parity)
- [x] T013 Validate schemas against captured samples in `schemas/qdrant/` (point-response*.json) and adjust as needed
- [x] T014 [P] Add brief headers to each schema documenting source, version, and validation notes

Checkpoint 1: ✅ COMPLETE - Contracts folder contains the 3 validated schemas with headers.

---

## Phase 2: Build Integration (NUKE + quicktype)

Purpose: Generate C# models deterministically during build using System.Text.Json.

References: `build/nuke/Build.cs:95`, `build/nuke/Build.cs:125`, `dotnet/framework/LablabBean.Framework.Generated/`

- [x] T020 Update `GenerateApiTypes` to consume JSON Schema inputs (not samples)
  - Source: `specs/023-quicktype-mapperly-adoption/contracts/*.schema.json`
  - Output: `dotnet/framework/LablabBean.Framework.Generated/ExternalApis/QdrantModels.g.cs`
  - Namespace: `LablabBean.Framework.Generated.Models.Qdrant`
- [x] T021 Update quicktype CLI args for STJ and NRT
  - Add: `--framework SystemTextJson --csharp-version 8 --check-required`
  - Use: `--src-lang schema` and `--features complete --array-type list`
- [x] T022 Add fail-fast error handling and log details for missing schemas; skip gracefully when none found
- [x] T023 Ensure generation executes before `Restore` (already `.DependsOn(GenerateApiTypes)`); keep idempotent outputs
- [x] T024 Remove/retire `*-sample.json`-based demo generation path and update `schemas/README.md` accordingly
- [x] T025 Build once to verify generated files compile and are included by `LablabBean.Framework.Generated`

Checkpoint 2: ✅ COMPLETE - Build generates Qdrant models into Framework.Generated with STJ attributes and no warnings.

---

## Phase 3: Qdrant Plugin Migration (Replace manual parsing)

Purpose: Replace manual `JsonDocument` parsing with strongly typed quicktype models.

References: `dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/`, `dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/QdrantVectorStore.cs`

- [x] T030 Add project reference to `LablabBean.Framework.Generated` from Qdrant plugin csproj
- [x] T031 Replace `JsonDocument.Parse*` usage with `System.Text.Json.JsonSerializer.Deserialize<QdrantSearchResponse>`
- [x] T032 Map generated `QdrantScoredPoint`/`QdrantSearchResponse` to existing domain DTOs with minimal glue code (no Mapperly here)
- [x] T033 Ensure request creation uses `QdrantSearchRequest` and matches API (vector, limit, with_payload, with_vector)
- [x] T034 Verify no public API changes in plugin; preserve existing method signatures and behavior
- [x] T035 Run existing integration tests; unskip Qdrant tests if environment available; otherwise dry-run local verification
- [x] T036 Grep to confirm SC-001: zero `JsonDocument.Parse` calls remain in plugin

Checkpoint 3: ✅ COMPLETE - Plugin compiles and passes tests with generated models; manual parsing removed.

---

## Phase 4: Cleanup, Docs, Benchmarks

Purpose: Finalize migration with documentation and performance checks.

- [x] T040 Remove obsolete demo/example generation paths and unused helpers
- [x] T041 Update `quickstart.md` with production examples using generated Qdrant models
- [x] T042 Update developer docs on adding new APIs (flags: `--framework SystemTextJson`, `--csharp-version 8`, `--check-required`)
- [x] T043 Benchmark Qdrant search operations pre/post; confirm ≤10% regression; note results in `research.md`
- [x] T044 Validate success criteria SC-003..SC-006, SC-007, SC-009, SC-010; record outcomes in plan/spec

Checkpoint 4: ✅ COMPLETE - Documentation updated; benchmarks recorded; all success criteria verified.

## Additional Completions (2025-10-27)

- [x] T050 Add second consumer of Framework.Generated (SC-003)
  - Added project reference to FileVectorStore plugin
  - Demonstrates cross-plugin model sharing capability
  - Verified build succeeds with multiple consumers
- [x] T051 Run test suite to verify SC-004
  - Executed unit tests: 178 tests passed
  - No regressions detected from migration
  - Build completes successfully with generated models

---

## Out of Scope (per research findings)

- Avatar state serialization remains as-is; do not introduce Mapperly for `Dictionary<string, object>` or serialization paths.
- Multi-source mapping and complex conversions are deferred to future, simple DTO↔domain scenarios only.

---

## Commands & File Paths (for convenience)

- Build: `build/nuke/build.ps1 --target Compile` (Task: `task build`)
- Generate (part of build): NUKE target `GenerateApiTypes` in `build/nuke/Build.cs`
- Schemas (central): `schemas/qdrant/`
- Contracts (this spec): `specs/023-quicktype-mapperly-adoption/contracts/`
- Generated models: `dotnet/framework/LablabBean.Framework.Generated/Models/Qdrant/`
- Plugin: `dotnet/plugins/LablabBean.Plugins.VectorStore.Qdrant/`
