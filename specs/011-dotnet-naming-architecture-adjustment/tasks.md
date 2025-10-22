# Tasks: .NET Project Naming and Architecture Adjustment (SPEC-011)

**Input**: `docs/_inbox/2025-10-22-dotnet-naming-architecture-adjustment--DOC-2025-00042.md`
**Spec**: `specs/011-dotnet-naming-architecture-adjustment/spec.md`
**Plan**: `specs/011-dotnet-naming-architecture-adjustment/plan.md`

**Objective**: Standardize naming, introduce contract proxies, convert renderers to plugins, and prep platform-agnostic loader.

---

## Format: `[ID] [P] Description`
- **[P]**: Can run in parallel (no ordering/dependency conflicts)

## Path Conventions (post-refactor)
- Contracts projects: `dotnet/framework/LablabBean.Reporting.Contracts/`
- Source generators: `dotnet/framework/LablabBean.SourceGenerators.Reporting/`
- Reporting core: `dotnet/framework/LablabBean.Reporting.*`
- Renderer plugins: `dotnet/plugins/LablabBean.Plugins.Reporting.{Csv|Html|FastReport}/`
- Tests: `dotnet/tests/`

---

## Phase 1: Simple Renames (Low Risk) ✓ COMPLETE
**Purpose**: Apply unified naming rules without changing behavior.

- [x] T001 Validate current repo state and solution projects
- [x] T002 Rename folder: `LablabBean.Reporting.Abstractions/` → `LablabBean.Reporting.Contracts/`
- [x] T003 Rename csproj: `...Reporting.Abstractions.csproj` → `...Reporting.Contracts.csproj`
- [x] T004 Update namespaces in code: `LablabBean.Reporting.Abstractions.*` → `LablabBean.Reporting.Contracts.*`
- [x] T005 Update all `<ProjectReference>` paths to `Reporting.Contracts`
- [x] T006 Update all `using` statements to `LablabBean.Reporting.Contracts`
- [x] T007 Rename folder: `LablabBean.Reporting.SourceGen/` → `LablabBean.SourceGenerators.Reporting/`
- [x] T008 Rename csproj accordingly and fix analyzer references
- [x] T009 Update namespaces in generator code to `LablabBean.SourceGenerators.Reporting`
- [x] T010 Update solution `LablabBean.sln` entries for the renamed projects
- [x] T011 Search/replace verification pass (no stale `Reporting.Abstractions`/`Reporting.SourceGen`)
- [x] T012 Build + Test: `dotnet build` and `dotnet test` succeed

**Checkpoint**: ✓ All renames compile; tests pass (13/13 tests passed); no namespace errors.

---

## Phase 2: Add Proxy Services to Contract Projects ✓ COMPLETE
**Purpose**: Enable tier-2 DI via generated proxies in contract assemblies.

- [x] T020 Audit contract projects for SourceGenerators.Proxy analyzer reference
- [x] T021 Add analyzer reference where missing (OutputItemType="Analyzer")
- [x] T022 [P] For each target interface, add proxy partial with `[RealizeService]`
- [x] T023 [P] Ensure proxy class holds `IRegistry` and selection strategy attributes as needed
- [x] T024 Build with `-v detailed` and verify generated files under `obj/.../generated/`
- [x] T025 Add usage doc snippet to SPEC-011 (consumer uses proxy rather than registry)
- [x] T026 Unit tests: add minimal tests covering proxy delegation to registry

**Checkpoint**: ✓ Proxies generate and compile; basic proxy tests pass (6/6 tests passed).

---

## Phase 3: Convert Reporting Renderers to Plugins ✓ COMPLETE
**Purpose**: Move Csv/Html renderers to plugins and use dynamic discovery.

- [x] T030 Move `dotnet/framework/LablabBean.Reporting.Renderers.Csv/` → `dotnet/plugins/LablabBean.Plugins.Reporting.Csv/`
- [x] T031 Move `dotnet/framework/LablabBean.Reporting.Renderers.Html/` → `dotnet/plugins/LablabBean.Plugins.Reporting.Html/`
- [x] T032 Create plugin classes implementing `IPlugin` and register `IReportRenderer` with metadata
- [x] T033 Add `plugin.json` manifests for each renderer plugin
- [x] T034 Update `IReportingService` to resolve renderer by format via `IRegistry.GetAll<IReportRenderer>()`
- [x] T035 Update/correct namespaces within moved renderers
- [x] T036 Adjust solution and project references for new plugin locations
- [x] T037 Tests: move renderer tests to `dotnet/tests/LablabBean.Plugins.Reporting.{Csv|Html}.Tests/`
- [x] T038 Tests: add plugin lifecycle tests and format-resolution tests
- [x] T039 Integration tests: end-to-end report path with discovered renderer

**Checkpoint**: ✓ Renderer plugins load and are discovered; end-to-end render works (13/13 tests passed).

---

## Phase 4: Platform-Agnostic Architecture (Future) ✓ COMPLETE
**Purpose**: Prepare loader abstraction to support ALC and future HybridCLR.

- [x] T040 Define `IPluginLoader` abstraction in `framework/LablabBean.Plugins.Core`
- [x] T041 ~~Create `dotnet/plugins/LablabBean.Plugins.Loader.ALC/`~~ (Kept in Core due to internal access)
- [x] T042 Update docs explaining loader selection and platform boundaries
- [x] T043 (Optional) Add basic tests for loader contract

**Checkpoint**: ✓ Loader abstraction defined; ALC loader implements interface; factory available.

---

## Verification & Tooling

- [ ] T050 Update `specs/README.md` to include SPEC-011
- [ ] T051 `check-prerequisites.ps1` passes for plan/tasks
- [ ] T052 Run `update-agent-context.ps1 -AgentType windsurf` (optional) after plan ready
- [ ] T053 CI validation: `dotnet build` and `dotnet test` succeed on Windows/Linux agents
- [ ] T054 Documentation: add migration and rollback notes per DOC-2025-00042

---

## Summary
- Phases 1–3 deliver naming, proxies, and renderer plugins without platform changes.
- Phase 4 prepares platform abstraction for future work.
- Success: solution builds, tests pass, and reporting renderers are pluggable/discoverable.
