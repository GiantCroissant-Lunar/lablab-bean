# Tasks: FastReport Reporting Infrastructure (SPEC-010)

**Input**: Design documents from `/specs/010-fastreport-reporting/`

**Prerequisites**: `plan.md` ✅, `spec.md` ✅

**Tests Mapping**: Functional requirements FR-001..FR-044 and success criteria SC-001..SC-008 in `spec.md`
**Organization**: Tasks are grouped by phase for incremental delivery. Use [P] to indicate tasks that can run in parallel.

## Format: `[ID] [P] Description`
- **[P]**: Can run in parallel (no ordering/dependency conflicts)

  ## Path Conventions
  - Framework libs: `dotnet/framework/`
    - `LablabBean.Reporting.Abstractions/` (netstandard2.1)
    - `LablabBean.Reporting.SourceGen/` (netstandard2.0, Analyzer)
    - `LablabBean.Reporting.Build/` (net8.0)
    - `LablabBean.Reporting.Analytics/` (net8.0)
  - Plugin: `dotnet/plugins/LablabBean.Plugins.FastReport/` (net8.0)
  - Console app (CLI): `console-app/LablabBean.Console/`
  - Tests: `dotnet/tests/`
    - `LablabBean.Reporting.SourceGen.Tests/`
    - `LablabBean.Reporting.Build.Tests/`
    - `LablabBean.Plugins.FastReport.Tests/`
    - `LablabBean.Reporting.Integration.Tests/`
  - Spec docs: `specs/010-fastreport-reporting/`

---

## Phase 0: Research & Unknowns (10 tasks) ✅ COMPLETE
**Purpose**: Complete research and consolidate into `research.md`.

- [x] T001 [P] Research FastReport.OpenSource 2026.1.0 API for programmatic report definition
- [x] T002 [P] Research FastReport.OpenSource.Export.PdfSimple 2026.1.2 PDF export options
- [x] T003 [P] Validate FastReport template (.frx) structure and XML schema
- [x] T004 [P] Research NFun-Report source generator pattern for attribute-driven discovery
- [x] T005 [P] Identify data model requirements for build metrics (test results, coverage, timing)
- [x] T006 [P] Identify data model requirements for game session stats (playtime, combat, items)
- [x] T007 [P] Identify data model requirements for plugin health (state, memory, load times)
- [x] T008 [P] Validate incremental source generator best practices (Roslyn 4.9.2+)
- [x] T009 Consolidate research findings into `research.md`
- [x] T010 Document known limitations and future considerations in `research.md`

**Checkpoint**: ✅ Research complete; data model and API requirements documented in `research.md`.

---

## Phase 1: Data Model & Contracts (10 tasks) ✅ COMPLETE
**Purpose**: Define core interfaces and data contracts.

- [x] T011 Create `specs/010-fastreport-reporting/contracts/` directory
- [x] T012 Create `data-model.md` documenting all report data structures
- [x] T013 [P] Define `IReportProvider` interface in contracts (GetReportData, GetMetadata)
- [x] T014 [P] Define `IReportRenderer` interface in contracts (Render, SupportedFormats)
- [x] T015 [P] Define `ReportProviderAttribute` class design (name, category, priority)
- [x] T016 [P] Define `BuildMetricsData` model (tests, coverage, duration)
- [x] T017 [P] Define `SessionStatisticsData` model (playtime, combat, progression)
- [x] T018 [P] Define `PluginHealthData` model (state, memory, load times)
- [x] T019 [P] Define `ReportFormat` enum (HTML, PDF, PNG, CSV)
- [x] T020 Document contracts in `data-model.md` with examples

**Checkpoint**: ✅ All data contracts defined and documented in `data-model.md` and `contracts/`; ready for implementation.

---

## Phase 2: Abstractions Library (12 tasks) ✅ COMPLETE
**Purpose**: Create `LablabBean.Reporting.Abstractions` with attributes and interfaces.

- [x] T021 Create `dotnet/framework/LablabBean.Reporting.Abstractions/` project (netstandard2.1)
- [x] T022 Configure project properties: TargetFramework, nullable enable, LangVersion 12
- [x] T023 Add NuGet packages: Microsoft.Extensions.Logging.Abstractions 8.0.0
- [x] T024 [P] Create `Attributes/ReportProviderAttribute.cs` (name, category, priority)
- [x] T025 [P] Create `Contracts/IReportProvider.cs` interface
- [x] T026 [P] Create `Contracts/IReportRenderer.cs` interface
- [x] T027 [P] Create `Contracts/IReportingService.cs` (orchestrator interface)
- [x] T028 [P] Create `Models/ReportFormat.cs` enum
- [x] T029 [P] Create `Models/ReportMetadata.cs` class
- [x] T030 [P] Create `Models/ReportRequest.cs` class (format, output, data path)
- [x] T031 [P] Create `Models/ReportResult.cs` class (success, path, errors)
- [x] T032 Build and validate package references

**Checkpoint**: ✅ Abstractions library builds successfully; all interfaces, attributes, and models available.

---

## Phase 3: Source Generator (18 tasks)
**Purpose**: Create Roslyn incremental generator for compile-time provider discovery.

### Generator Core
- [ ] T033 Create `dotnet/framework/LablabBean.Reporting.SourceGen/` project (netstandard2.0)
- [ ] T034 Add NuGet: Microsoft.CodeAnalysis.CSharp 4.9.2, Microsoft.CodeAnalysis.Analyzers 3.3.4
- [ ] T035 Configure as analyzer: OutputItemType="Analyzer", IncludeBuildOutput="false"
- [ ] T036 Reference LablabBean.Reporting.Abstractions
- [ ] T037 Create `ReportProviderGenerator.cs` implementing IIncrementalGenerator
- [ ] T038 Implement Initialize() with incremental pipeline

### Discovery & Validation
- [ ] T039 Create syntax provider to find classes with [ReportProvider] attribute
- [ ] T040 Extract attribute arguments (name, category, priority)
- [ ] T041 Validate class implements IReportProvider (generate diagnostic if not)
- [ ] T042 Validate class has parameterless constructor or IServiceProvider ctor

### Code Generation
- [ ] T043 Generate `ReportProviderRegistry.g.cs` with static provider list
- [ ] T044 Generate registration extension method: AddReportProviders(IServiceCollection)
- [ ] T045 Include provider metadata (name, category, priority) in registry
- [ ] T046 Handle multiple providers per category (sorted by priority)

### Testing
- [ ] T047 Create `dotnet/tests/LablabBean.Reporting.SourceGen.Tests/`
- [ ] T048 Test: Generator finds [ReportProvider] classes
- [ ] T049 Test: Generator creates registry with correct metadata
- [ ] T050 Test: Diagnostic when IReportProvider not implemented

**Checkpoint**: Source generator builds; provider registry generated at compile time.

---

## Phase 4: Data Providers & Parsers (16 tasks)
**Purpose**: Implement concrete providers for build, session, and plugin data.

### Build Metrics Provider
- [ ] T051 Create `dotnet/framework/LablabBean.Reporting.Build/` project (net8.0)
- [ ] T052 Reference Abstractions, add [ReportProvider("BuildMetrics", "Build")]
- [ ] T053 Implement BuildMetricsProvider : IReportProvider
- [ ] T054 Parse test results from xUnit/NUnit XML files
- [ ] T055 Parse code coverage from Coverlet/OpenCover XML files
- [ ] T056 Extract build duration from build logs or metadata files
- [ ] T057 Tests: BuildMetricsProvider parses sample test results
- [ ] T058 Tests: BuildMetricsProvider handles missing coverage files gracefully

### Session Statistics Provider
- [ ] T059 Create `dotnet/framework/LablabBean.Reporting.Analytics/` project (net8.0)
- [ ] T060 Reference Abstractions, AnalyticsPlugin contracts
- [ ] T061 Implement SessionStatisticsProvider with [ReportProvider("Session", "Analytics")]
- [ ] T062 Parse session JSON from AnalyticsPlugin event log
- [ ] T063 Calculate aggregates: total playtime, K/D ratio, damage stats
- [ ] T064 Tests: SessionStatisticsProvider parses sample session data
- [ ] T065 Tests: SessionStatisticsProvider handles partial/incomplete sessions

### Plugin Health Provider
- [ ] T066 Implement PluginHealthProvider in LablabBean.Reporting.Analytics
- [ ] T067 Query plugin system for loaded plugins, states, memory usage
- [ ] T068 Collect load times from plugin initialization metrics
- [ ] T069 Tests: PluginHealthProvider returns correct plugin count and states

**Checkpoint**: All three providers implemented and tested.

---

## Phase 5: FastReport Plugin & Templates (16 tasks)
**Purpose**: Create FastReport.OpenSource plugin for multi-format rendering.

### Plugin Project
- [ ] T070 Create `dotnet/plugins/LablabBean.Plugins.FastReport/` project (net8.0)
- [ ] T071 Add NuGet: FastReport.OpenSource 2026.1.0, PdfSimple 2026.1.2
- [ ] T072 Reference Abstractions, LablabBean.Plugins.Contracts
- [ ] T073 Implement FastReportPlugin : IPlugin, IReportRenderer
- [ ] T074 Register plugin metadata (name, version, health endpoint)

### Rendering Engine
- [ ] T075 Implement Render(ReportRequest) method with format switch
- [ ] T076 Load .frx template from embedded resources or file path
- [ ] T077 Bind data to FastReport Report object
- [ ] T078 Export HTML using HTMLExport
- [ ] T079 Export PDF using PDFSimpleExport
- [ ] T080 Export PNG using ImageExport
- [ ] T081 Handle template not found errors gracefully
- [ ] T082 Add logging for render start/complete/errors

### Templates
- [ ] T083 Create `templates/build-metrics.frx` with test/coverage/duration layout
- [ ] T084 Create `templates/session-statistics.frx` with combat/playtime layout
- [ ] T085 Create `templates/plugin-health.frx` with plugin state table
- [ ] T086 Embed templates as resources or configure file path convention

### Testing
- [ ] T087 Create `dotnet/tests/LablabBean.Plugins.FastReport.Tests/`
- [ ] T088 Test: Plugin renders HTML with sample BuildMetricsData
- [ ] T089 Test: Plugin renders PDF with sample SessionStatisticsData
- [ ] T090 Test: Plugin handles missing template gracefully
- [ ] T091 Test: Plugin validates file size limits (<5 MB)

**Checkpoint**: Plugin renders all formats; templates available and customizable.

---

  ## Phase 6: CLI Integration (System.CommandLine) (12 tasks)
  **Purpose**: Provide `lablabbean report` command with subcommands.

  - [ ] T077 Create `console-app/LablabBean.Console/` (net8.0) with System.CommandLine 2.0.0-beta4
  - [ ] T078 Implement `report` root command and subcommands:
         `build`, `session`, `plugin-status`
  - [ ] T079 Implement options: `--format`, `--output`, `--data-path`, `--template`
- [ ] T080 Default format to HTML; generate success message with output path
- [ ] T081 Wire DI to resolve `IReportingService` from plugin
- [ ] T082 Add graceful errors for missing data/invalid template

- [ ] T083 Tests: CLI parses commands/options correctly
- [ ] T084 Tests: `lablabbean report build` generates HTML by default
- [ ] T085 Tests: `--format pdf` produces PDF via plugin
- [ ] T086 Tests: Bad input yields non-zero exit code with clear message
- [ ] T087 Package publish profile if needed for CI usage
- [ ] T088 If console app not desired, document Windows app integration limitations

**Checkpoint**: CLI usable locally and in CI.

---

## Phase 7: Integration & E2E (10 tasks)
**Purpose**: End-to-end validation across components.

- [ ] T089 Create `LablabBean.Reporting.Integration.Tests`
- [ ] T090 Add E2E test: build metrics HTML + PDF generation with sample data
- [ ] T091 Add E2E test: session report generation with sample session JSON
- [ ] T092 Add E2E test: plugin health report with sample metrics
- [ ] T093 Verify output filenames include timestamp and/or build number
- [ ] T094 Enforce file size limits (<5 MB typical)
- [ ] T095 Measure performance (build report <5s, session <2s)
- [ ] T096 Validate fallback behavior when templates missing
- [ ] T097 Verify partial success when one format fails
- [ ] T098 Snapshot-test minimal HTML output

**Checkpoint**: E2E scenarios pass and meet success criteria.

---

## Phase 8: CI/CD & Build Integration (10 tasks)
**Purpose**: Automate reports in CI and publish artifacts.

- [ ] T099 Add/build task to run CLI after tests/coverage (Nuke or existing pipelines)
- [ ] T100 Configure artifacts path: `build/_artifacts/<version>/reports/`
- [ ] T101 Timestamped filenames: `build-metrics-<build>-<ts>.html`
- [ ] T102 Add failure conditions for missing inputs with clear messages
- [ ] T103 Generate multiple formats when configured (HTML+PDF)
- [ ] T104 Ensure CI images include fonts for PDF if needed
- [ ] T105 Document CI usage in `quickstart.md`
- [ ] T106 Add Taskfile/Nuke entry points for local dev
- [ ] T107 Validate on Windows and at least one Linux agent
- [ ] T108 Add build log links to generated reports if applicable

**Checkpoint**: Reports generated automatically by CI with reliable artifacts.

---

## Phase 9: Documentation & Developer Experience (10 tasks)
**Purpose**: Developer quickstart and reference.

- [ ] T109 Create `quickstart.md` with install, usage, CLI examples
- [ ] T110 Add examples for each report type and format
- [ ] T111 Add troubleshooting section (missing data, template issues, PDF limits)
- [ ] T112 Cross-link `spec.md`, `plan.md`, `research.md`, `data-model.md`
- [ ] T113 Update `specs/README.md` with Spec-010 entry if needed
- [ ] T114 Update checklists/requirements.md validation matrix
- [ ] T115 Update CHANGELOG.md with Spec-010 entries
- [ ] T116 If available, run agent context update script:
       `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude` (optional)
- [ ] T117 Add `README.md` in plugin folder with template notes
- [ ] T118 Document extension points (alternate renderers)

**Checkpoint**: Docs complete; onboarding time minimized.

---

## Phase 10: Performance, Telemetry & Polish (8 tasks)
**Purpose**: Final tuning and guardrails.

- [ ] T119 Add minimal logging in providers and plugin (info/warn)
- [ ] T120 Add metrics hooks if AnalyticsPlugin targets build-time events
- [ ] T121 Cache heavy computations or template loads where safe
- [ ] T122 Validate memory usage and ALC unloading behavior
- [ ] T123 Verify CSV export path (if implemented) or mark as future
- [ ] T124 Add sample large datasets to test limits
- [ ] T125 Re-run perf tests and record results in `research.md`
- [ ] T126 Final review against FR/SC checklist and mark complete

---

## Summary
- **Total Tasks**: 126
- **Estimated Time**: 5–7 weeks (phased delivery: P1 build metrics → P2 session → P3 plugin health)
- **Critical Dependencies**:
  - FastReport.OpenSource 2026.1.0, PdfSimple 2026.1.2
  - Roslyn 4.9.x for source generator
  - AnalyticsPlugin and Plugin metrics availability
  - System.CommandLine 2.0.0-beta4

## Completion Criteria
- All FR-001..FR-044 satisfied
- All SC-001..SC-008 met or exceeded
- CLI and CI flows generate reports reliably on Windows and Linux
- Templates customizable and versioned; plugin optional and replaceable
