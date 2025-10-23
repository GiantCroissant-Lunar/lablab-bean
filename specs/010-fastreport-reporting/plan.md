# Implementation Plan: Reporting Infrastructure with FastReport

**Branch**: `010-fastreport-reporting` | **Date**: 2025-10-22 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/010-fastreport-reporting/spec.md`

## Summary

Implement a comprehensive reporting infrastructure for lablab-bean using FastReport.OpenSource for report generation and NFun-Report's attribute-driven source generation pattern for compile-time provider discovery. The system will generate HTML, PDF, and PNG reports for build metrics (test results, code coverage, build duration), game session statistics (playtime, combat metrics), and plugin system health (memory usage, load times). Reports are triggered via CLI commands and integrated with the Nuke build system for automated CI/CD reporting.

**Primary Technical Approach**: Create a layered reporting system with:

1. **Abstractions Layer** (netstandard2.1): Attribute-driven provider metadata for compile-time discovery
2. **Source Generator** (netstandard2.0): Roslyn incremental generator for zero-reflection provider registry
3. **FastReport Plugin** (net8.0): Optional plugin wrapping FastReport.OpenSource for HTML/PDF/PNG export
4. **Data Providers** (net8.0): Build metrics, game statistics, and plugin health data collectors
5. **CLI Integration**: Report commands integrated into existing console/windows applications

## Technical Context

**Language/Version**: C# 12.0 / .NET 8.0 (aligns with existing `dotnet/Directory.Build.props`)
**Primary Dependencies**:

- FastReport.OpenSource 2026.1.0 (MIT license)
- FastReport.OpenSource.Export.PdfSimple 2026.1.2
- Microsoft.CodeAnalysis.CSharp 4.9.2 (for source generator)
- Microsoft.Extensions.DependencyInjection 8.0.0 (existing)
- Microsoft.Extensions.Logging.Abstractions 8.0.0 (existing)
- System.CommandLine 2.0.0-beta4 (existing)
- Existing plugin infrastructure (LablabBean.Plugins.Contracts, LablabBean.Plugins.Core)

**Storage**: File-based (reports exported to `artifacts/reports/`, data from existing JSON/XML files)
**Testing**: xUnit 2.5.3 + FluentAssertions 6.12.0 (existing test stack)
**Target Platform**: .NET 8 console/terminal applications (Terminal.Gui, SadConsole)
**Project Type**: Plugin + Framework libraries (follows existing plugin architecture)

**Performance Goals**:

- Report generation under 5 seconds for 500 test results
- Session report generation under 2 seconds
- Source generator incremental compilation under 100ms

**Constraints**:

- Report file sizes under 5 MB for typical datasets
- Zero-reflection provider discovery (compile-time only)
- FastReport plugin must be optional (can be disabled)
- Must integrate with existing AnalyticsPlugin event bus

**Scale/Scope**:

- 3 new framework projects (Abstractions, SourceGen, FastReport plugin)
- 4 report templates (.frx files for build, game, plugin, CI/CD)
- 3 data provider implementations (build metrics, game stats, plugin health)
- CLI integration into 2 existing apps (console, windows)
- ~5-7 weeks of development (phased: P1 build metrics → P2 game stats → P3 plugin health)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### P-1: Documentation-First Development ✅

- **Check**: Feature has comprehensive spec.md with requirements, user scenarios, success criteria
- **Status**: PASS - SPEC-010 fully documented with 4 user stories, 44 functional requirements, 8 success criteria

### P-2: Clear Code Over Clever Code ✅

- **Check**: Design favors simplicity and maintainability
- **Status**: PASS - Attribute-driven pattern is explicit and proven (see SPEC-009 proxy generator)
- **Justification**: Source generator complexity is encapsulated; provider registration is declarative

### P-3: Testing Matters ✅

- **Check**: Critical functionality must be tested
- **Status**: PASS - Plan includes unit tests for source generator, integration tests for report generation
- **Test Coverage**: Source generator verification tests, report template tests, provider tests

### P-4: Security Consciousness ✅

- **Check**: No hardcoded secrets, input validation
- **Status**: PASS - Reports use data from existing validated sources (xUnit XML, coverlet JSON, plugin metrics)
- **Input Validation**: File paths validated, template existence checked, malformed data handled gracefully

### P-5: User Experience Focus ✅

- **Check**: Terminal UX should be intuitive
- **Status**: PASS - CLI commands follow existing pattern (`lablabbean report build --format pdf --output ./report.pdf`)
- **UX**: Clear success/error messages, format flag defaults to HTML, graceful degradation

### P-6: Separation of Concerns ✅

- **Check**: Clear layer boundaries
- **Status**: PASS - Reporting is isolated as plugin; data providers are separate from report generation
- **Layers**: Abstractions (contracts) → Providers (data) → FastReport plugin (rendering) → CLI (interface)

### P-7: Performance Awareness ✅

- **Check**: Monitor memory and performance
- **Status**: PASS - FastReport plugin runs in isolated ALC; source generator is incremental compilation
- **Performance Tracking**: Success criteria include specific timing goals (5s for builds, 2s for sessions)

### P-8: Build Automation ✅

- **Check**: Reproducible builds
- **Status**: PASS - Integrates with existing Nuke build system; report generation is CLI-driven
- **Automation**: CI/CD can run `lablabbean report build --format pdf --output <artifacts-path>`

### P-9: Version Control Hygiene ✅

- **Check**: Conventional commits
- **Status**: PASS - Plan includes commit message examples (e.g., `feat(reporting): add FastReport plugin`)

### P-10: When in doubt, ask ✅

- **Check**: Clarify requirements before implementing
- **Status**: PASS - Spec has zero [NEEDS CLARIFICATION] markers; all requirements fully specified

### R-DOC: Documentation Rules ✅

- **Check**: New docs go to `docs/_inbox/` first, YAML front-matter required
- **Status**: PASS - Plan includes documentation in `specs/010-fastreport-reporting/`
- **Compliance**: Evaluation docs already in `docs/findings/` with proper front-matter

### R-CODE: Code Quality Rules ✅

- **Check**: No secrets, meaningful names, comments for non-obvious code
- **Status**: PASS - No secrets involved; provider naming follows `<Domain><Type>Provider` pattern

### R-TST: Testing Rules ✅

- **Check**: Test critical paths, builds must pass
- **Status**: PASS - Source generator, report generation, and data providers all have test coverage
- **Critical Paths**: Template loading, data binding, format export, provider discovery

### R-GIT: Git Rules ✅

- **Check**: Descriptive commits (conventional format), no secrets
- **Status**: PASS - Plan uses conventional commits; no secrets in report data

### R-PRC: Process Rules ✅

- **Check**: ADRs for architecture decisions
- **Status**: PASS - Decision to use FastReport.OpenSource documented in evaluation (docs/findings/)
- **ADR**: FastReport re-evaluation docs explain rationale for adoption over custom implementation

### R-SEC: Security Rules ✅

- **Check**: Input validation, least privilege
- **Status**: PASS - File paths validated, FastReport plugin runs in isolated ALC, reports use read-only data
- **Validation**: Template files checked for existence, output paths validated, malformed data handled

### R-TOOL: Tool Rules ✅

- **Check**: Spec-Kit for features
- **Status**: PASS - Following `/speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement` workflow

**GATE RESULT**: ✅ **PASS** - All constitution checks satisfied. Proceed to Phase 0 research.

## Project Structure

### Documentation (this feature)

```
specs/010-fastreport-reporting/
├── spec.md              # Feature specification (COMPLETED)
├── plan.md              # This file (IN PROGRESS)
├── research.md          # Phase 0 output (PENDING)
├── data-model.md        # Phase 1 output (PENDING)
├── quickstart.md        # Phase 1 output (PENDING)
├── contracts/           # Phase 1 output (PENDING)
│   ├── IReportingService.cs
│   ├── IReportProvider.cs
│   ├── ReportDefinition.cs
│   └── ReportExportFormat.cs
├── checklists/
│   └── requirements.md  # Spec quality checklist (COMPLETED)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/
├── framework/
│   ├── LablabBean.Reporting.Abstractions/           # NEW - netstandard2.1
│   │   ├── Attributes/
│   │   │   ├── ReportProviderAttribute.cs
│   │   │   └── ReportExporterAttribute.cs
│   │   ├── Contracts/
│   │   │   ├── IReportProvider.cs
│   │   │   ├── IReportExporter.cs
│   │   │   └── IReportingService.cs
│   │   ├── Models/
│   │   │   ├── ReportDefinition.cs
│   │   │   ├── ReportExportFormat.cs
│   │   │   └── ReportMetadata.cs
│   │   └── LablabBean.Reporting.Abstractions.csproj
│   │
│   ├── LablabBean.Reporting.SourceGen/              # NEW - netstandard2.0 (Analyzer)
│   │   ├── Generators/
│   │   │   └── ReportRegistryGenerator.cs
│   │   ├── Templates/
│   │   │   └── ReportRegistryTemplate.txt
│   │   └── LablabBean.Reporting.SourceGen.csproj
│   │
│   ├── LablabBean.Reporting.Build/                  # NEW - net8.0
│   │   ├── Providers/
│   │   │   ├── BuildMetricsProvider.cs
│   │   │   ├── TestResultsProvider.cs
│   │   │   └── CodeCoverageProvider.cs
│   │   ├── Models/
│   │   │   ├── BuildMetricsAggregate.cs
│   │   │   ├── TestResultsAggregate.cs
│   │   │   └── CoverageAggregate.cs
│   │   ├── Parsers/
│   │   │   ├── XUnitXmlParser.cs
│   │   │   └── CoverletJsonParser.cs
│   │   └── LablabBean.Reporting.Build.csproj
│   │
│   └── LablabBean.Reporting.Analytics/              # NEW - net8.0
│       ├── Providers/
│       │   ├── GameStatisticsProvider.cs
│       │   └── CombatStatisticsProvider.cs
│       ├── Models/
│       │   ├── GameSessionAggregate.cs
│       │   └── CombatAggregate.cs
│       └── LablabBean.Reporting.Analytics.csproj
│
├── plugins/
│   └── LablabBean.Plugins.FastReport/               # NEW - net8.0
│       ├── plugin.json
│       ├── FastReportPlugin.cs
│       ├── FastReportService.cs
│       ├── Templates/                               # .frx template files
│       │   ├── build-metrics.frx
│       │   ├── game-session.frx
│       │   ├── plugin-health.frx
│       │   └── ci-cd-summary.frx
│       └── LablabBean.Plugins.FastReport.csproj
│
├── tests/
│   ├── LablabBean.Reporting.SourceGen.Tests/        # NEW
│   │   ├── ReportRegistryGeneratorTests.cs
│   │   └── GeneratorTestHelper.cs
│   ├── LablabBean.Reporting.Build.Tests/            # NEW
│   │   ├── BuildMetricsProviderTests.cs
│   │   ├── XUnitXmlParserTests.cs
│   │   └── CoverletJsonParserTests.cs
│   ├── LablabBean.Plugins.FastReport.Tests/         # NEW
│   │   ├── FastReportServiceTests.cs
│   │   └── TemplateLoadingTests.cs
│   └── LablabBean.Reporting.Integration.Tests/      # NEW
│       ├── EndToEndReportGenerationTests.cs
│       └── TestData/
│           ├── sample-build.xml
│           ├── sample-coverage.json
│           └── sample-session.json
│
└── console-app/LablabBean.Console/
    └── Commands/
        └── ReportCommand.cs                         # NEW - CLI integration

build/
└── _artifacts/<version>/
    └── reports/                                     # NEW - Generated reports output
        ├── build-metrics-<timestamp>.html
        ├── build-metrics-<timestamp>.pdf
        ├── session-<timestamp>.html
        └── plugin-health-<timestamp>.html
```

**Structure Decision**: Multi-project plugin architecture following existing lablab-bean patterns. The reporting system is decomposed into:

1. **Framework Libraries** (dotnet/framework/):
   - **Abstractions** (netstandard2.1): Shared across AssemblyLoadContext boundaries, minimal contract set
   - **SourceGen** (netstandard2.0): Analyzer/source generator for compile-time provider discovery
   - **Build** (net8.0): Build metrics data providers and parsers
   - **Analytics** (net8.0): Game statistics data providers

2. **Plugin** (dotnet/plugins/):
   - **FastReport Plugin** (net8.0): Optional plugin wrapping FastReport.OpenSource, contains templates

3. **Tests** (dotnet/tests/):
   - Unit tests for each component
   - Integration tests for end-to-end report generation
   - Source generator verification tests

This structure aligns with:

- **P-6 (Separation of Concerns)**: Clear layers (abstractions → providers → rendering → CLI)
- **Existing Plugin Architecture**: FastReport is an optional plugin, can be disabled/replaced
- **AssemblyLoadContext Isolation**: FastReport plugin runs in isolated ALC
- **Reusability**: Abstractions can support alternative report renderers beyond FastReport

## Complexity Tracking

*No Constitution Check violations. This section is not required.*

## Phase 0: Research & Unknowns

### Research Tasks

#### R-001: FastReport.OpenSource Template Best Practices

**Question**: What are the best practices for creating maintainable `.frx` templates in FastReport.OpenSource?
**Research Needed**:

- Optimal template structure for build reports (tables, charts, summaries)
- Data binding patterns for strongly-typed models
- Template versioning and migration strategies
- Performance considerations (large datasets)

**Output Location**: `research.md` - Section: "FastReport Template Design"

---

#### R-002: Source Generator Incremental Compilation

**Question**: How to implement incremental compilation in Roslyn source generators to optimize build performance?
**Research Needed**:

- `IIncrementalGenerator` best practices (vs legacy `ISourceGenerator`)
- Caching strategies for attribute scanning
- Diagnostic emission for duplicate provider IDs
- Testing patterns for source generators

**Output Location**: `research.md` - Section: "Source Generator Implementation"

---

#### R-003: xUnit XML Output Format

**Question**: What is the exact structure of xUnit XML test results for reliable parsing?
**Research Needed**:

- XML schema for xUnit v2 output (`--logger "xunit;LogFileName=results.xml"`)
- Handling edge cases (skipped tests, test failures, stack traces)
- Parsing libraries vs manual XDocument parsing

**Output Location**: `research.md` - Section: "Test Result Parsing"

---

#### R-004: Coverlet JSON Coverage Format

**Question**: What is the structure of coverlet's JSON coverage output?
**Research Needed**:

- JSON schema for coverlet coverage files
- Calculating line coverage % and branch coverage %
- Identifying files with low coverage
- Handling multiple test assemblies

**Output Location**: `research.md` - Section: "Code Coverage Parsing"

---

#### R-005: CLI Command Integration Pattern

**Question**: How to integrate new `report` commands into existing console/windows apps using System.CommandLine?
**Research Needed**:

- System.CommandLine 2.0.0-beta4 command registration
- Subcommand patterns (`report build`, `report session`, `report plugin-status`)
- Option/argument binding (`--format`, `--output`)
- Help text generation

**Output Location**: `research.md` - Section: "CLI Integration"

---

#### R-006: AnalyticsPlugin Event Bus Integration

**Question**: How does the existing AnalyticsPlugin collect events via the event bus?
**Research Needed**:

- Review `LablabBean.Plugins.Analytics` implementation
- MessagePipe pub/sub patterns used
- Event types tracked (spawns, moves, combat)
- Session data persistence strategy

**Output Location**: `research.md` - Section: "Game Statistics Collection"

---

#### R-007: Plugin Metrics Data Availability

**Question**: What plugin metrics are currently tracked by PluginSystemMetrics and PluginHealthChecker?
**Research Needed**:

- Review `LablabBean.Plugins.Core/PluginMetrics.cs`
- Available data: memory usage, load duration, health state
- Export format from `PluginAdminService.ExportMetrics()`
- JSON schema for plugin health snapshots

**Output Location**: `research.md` - Section: "Plugin Health Reporting"

---

#### R-008: FastReport PDF Export Limitations

**Question**: What are the limitations of `FastReport.OpenSource.Export.PdfSimple` vs commercial FastReport.Core?
**Research Needed**:

- Features available in PdfSimple (basic PDF export)
- Missing features (encryption, digital signatures, font embedding)
- When to recommend upgrading to FastReport.Core
- Alternative PDF libraries if PdfSimple insufficient

**Output Location**: `research.md` - Section: "Report Export Formats"

---

### Research Dispatch Plan

**Parallel Research** (can run concurrently):

- R-001 (FastReport templates)
- R-002 (Source generator)
- R-003 (xUnit XML)
- R-004 (Coverlet JSON)
- R-005 (CLI integration)
- R-008 (PDF export)

**Sequential Research** (depends on codebase exploration):

- R-006 (AnalyticsPlugin) → Explore existing plugin
- R-007 (Plugin metrics) → Explore existing infrastructure

**Estimated Timeline**: 3-5 days for all research tasks

---

## Phase 1: Data Model & Contracts

*Prerequisites: Phase 0 research.md complete*

### Data Model Design

**Output**: `data-model.md`

#### Entity 1: ReportProvider

**Purpose**: Metadata for a report data provider discovered at compile time.

**Fields**:

- `Id` (string): Unique identifier (e.g., "lablab.build-metrics")
- `Name` (string, optional): Human-readable name (e.g., "Build Metrics Provider")
- `Version` (string, optional): Provider version (e.g., "1.0.0")
- `Category` (string, optional): Grouping category (e.g., "Build", "Analytics", "Performance")

**Relationships**: None (metadata only)

**Validation Rules**:

- `Id` must be non-empty
- `Id` must be unique across all providers (enforced by source generator diagnostic)

**State Transitions**: Immutable (discovered at compile time)

---

#### Entity 2: ReportTemplate

**Purpose**: Represents a FastReport template file for report rendering.

**Fields**:

- `TemplatePath` (string): Path to `.frx` template file
- `Name` (string): Template name (e.g., "Build Metrics Report")
- `SupportedFormats` (ReportExportFormat[]): Formats this template can export to

**Relationships**: None

**Validation Rules**:

- `TemplatePath` must point to existing `.frx` file
- `SupportedFormats` must not be empty

**State Transitions**: Immutable (loaded at runtime)

---

#### Entity 3: BuildMetricsAggregate

**Purpose**: Aggregated build data for report generation.

**Fields**:

- `BuildStartTime` (DateTimeOffset): When build started
- `BuildEndTime` (DateTimeOffset): When build ended
- `BuildDuration` (TimeSpan): Total build time
- `TotalTests` (int): Total number of tests executed
- `PassedTests` (int): Number of passing tests
- `FailedTests` (int): Number of failing tests
- `SkippedTests` (int): Number of skipped tests
- `PassPercentage` (double): (PassedTests / TotalTests) * 100
- `LineCoveragePercentage` (double): Line coverage %
- `BranchCoveragePercentage` (double): Branch coverage %
- `FailureDetails` (TestFailure[]): Details of failed tests

**Relationships**:

- Has many `TestFailure` (composition)

**Validation Rules**:

- `TotalTests` = `PassedTests` + `FailedTests` + `SkippedTests`
- Coverage percentages between 0 and 100

**State Transitions**: Immutable (computed from source data)

---

#### Entity 4: TestFailure

**Purpose**: Details of a single test failure.

**Fields**:

- `TestName` (string): Full test name
- `ErrorMessage` (string): Failure message
- `StackTrace` (string): Stack trace of failure

**Relationships**: Part of `BuildMetricsAggregate`

**Validation Rules**:

- `TestName` must be non-empty

---

#### Entity 5: GameSessionAggregate

**Purpose**: Gameplay statistics for a single session.

**Fields**:

- `SessionId` (string): Unique session identifier
- `SessionStartTime` (DateTimeOffset): Session start
- `SessionEndTime` (DateTimeOffset): Session end
- `TotalPlaytime` (TimeSpan): Total playtime
- `Kills` (int): Enemy kills
- `Deaths` (int): Player deaths
- `DamageDealt` (long): Total damage dealt
- `DamageTaken` (long): Total damage taken
- `KillDeathRatio` (double): Kills / Deaths (handle Deaths = 0)
- `AverageDamagePerKill` (double): DamageDealt / Kills

**Relationships**: None

**Validation Rules**:

- `TotalPlaytime` = `SessionEndTime` - `SessionStartTime`
- Counts must be non-negative

**State Transitions**: Immutable (computed from event log)

---

#### Entity 6: PluginHealthSnapshot

**Purpose**: Health information for a single plugin.

**Fields**:

- `PluginId` (string): Plugin identifier
- `PluginName` (string): Plugin name
- `State` (PluginState enum): Running, Failed, Degraded, Stopped
- `MemoryUsageMB` (double): Memory usage in megabytes
- `LoadDurationMs` (long): Load duration in milliseconds
- `HealthStatus` (HealthStatus enum): Healthy, Degraded, Unhealthy, Unknown
- `HealthStatusReason` (string, optional): Reason for degraded/unhealthy state
- `LastHealthCheckTime` (DateTimeOffset): Timestamp of last health check

**Relationships**: None

**Validation Rules**:

- `PluginId` must be non-empty
- `MemoryUsageMB` and `LoadDurationMs` must be non-negative
- If `HealthStatus` is Degraded/Unhealthy, `HealthStatusReason` should be present

**State Transitions**: None (snapshot at a point in time)

---

#### Entity 7: ReportDefinition

**Purpose**: Specification for generating a report.

**Fields**:

- `ProviderId` (string): ID of data provider to use
- `TemplatePath` (string): Path to `.frx` template
- `OutputFormat` (ReportExportFormat enum): HTML, PDF, PNG, CSV
- `OutputPath` (string): Where to save generated report
- `Data` (object): Strongly-typed data object (e.g., BuildMetricsAggregate)

**Relationships**:

- References `ReportProvider` by ID
- References `ReportTemplate` by path

**Validation Rules**:

- `ProviderId` must exist in registry
- `TemplatePath` must exist
- `OutputPath` must be writable
- `Data` must match provider's expected type

**State Transitions**: Immutable (created per generation request)

---

### API Contracts

**Output**: `contracts/` directory

#### Contract 1: IReportProvider<TData>

```csharp
namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Marker interface for report data providers.
/// Implementers are discovered at compile time via [ReportProvider] attribute.
/// </summary>
/// <typeparam name="TData">Type of data this provider produces</typeparam>
public interface IReportProvider<out TData> where TData : class
{
    /// <summary>
    /// Load report data from the specified source.
    /// </summary>
    /// <param name="sourcePath">Path to data source (directory, file, etc.)</param>
    /// <returns>Aggregated report data</returns>
    TData LoadData(string sourcePath);
}
```

**File**: `contracts/IReportProvider.cs`

---

#### Contract 2: IReportingService

```csharp
namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Service for generating reports from templates and data.
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generate a report based on the provided definition.
    /// </summary>
    /// <param name="definition">Report generation specification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to generated report file</returns>
    Task<string> GenerateReportAsync(
        ReportDefinition definition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific export format is supported.
    /// </summary>
    /// <param name="format">Format to check</param>
    /// <returns>True if supported, false otherwise</returns>
    bool SupportsFormat(ReportExportFormat format);

    /// <summary>
    /// Get all supported export formats.
    /// </summary>
    /// <returns>Array of supported formats</returns>
    ReportExportFormat[] GetSupportedFormats();
}
```

**File**: `contracts/IReportingService.cs`

---

#### Contract 3: ReportDefinition

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Specification for generating a report.
/// </summary>
public sealed class ReportDefinition
{
    /// <summary>
    /// ID of the data provider to use.
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// Path to the report template (.frx file).
    /// </summary>
    public required string TemplatePath { get; init; }

    /// <summary>
    /// Desired export format.
    /// </summary>
    public required ReportExportFormat OutputFormat { get; init; }

    /// <summary>
    /// Path where the generated report should be saved.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Data object to bind to the template.
    /// </summary>
    public required object Data { get; init; }
}
```

**File**: `contracts/ReportDefinition.cs`

---

#### Contract 4: ReportExportFormat

```csharp
namespace LablabBean.Reporting.Abstractions.Models;

/// <summary>
/// Supported report export formats.
/// </summary>
public enum ReportExportFormat
{
    /// <summary>
    /// HTML format (web-viewable).
    /// </summary>
    Html,

    /// <summary>
    /// PDF format (portable document).
    /// </summary>
    Pdf,

    /// <summary>
    /// PNG image format.
    /// </summary>
    Png,

    /// <summary>
    /// CSV format (data export).
    /// </summary>
    Csv
}
```

**File**: `contracts/ReportExportFormat.cs`

---

### Agent Context Update

**Script**: `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`

**New Technologies to Add** (between `<!-- AUTO-GENERATED TECH STACK -->` markers):

```markdown
### Reporting Infrastructure (SPEC-010)
- **FastReport.OpenSource 2026.1.0**: MIT-licensed report generation library
- **FastReport.OpenSource.Export.PdfSimple 2026.1.2**: PDF export plugin
- **Roslyn Source Generator**: Compile-time report provider discovery
- **Attributes**: `[ReportProvider]` for marking data providers
- **Report Formats**: HTML, PDF, PNG, CSV
```

---

## Phase 2: Quickstart & Examples

*Prerequisites: Phase 1 data-model.md and contracts/ complete*

**Output**: `quickstart.md`

### Quickstart Content Outline

1. **Installation**: How to enable FastReport plugin
2. **Basic Usage**: Generate a build metrics report
3. **CLI Commands**: Examples of `lablabbean report` commands
4. **Custom Providers**: How to create a new report provider
5. **Custom Templates**: How to customize `.frx` templates
6. **Troubleshooting**: Common issues and solutions

### Example: Generate Build Metrics Report

```bash
# After running tests and coverage
dotnet test --logger "xunit;LogFileName=results.xml"
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate HTML report
lablabbean report build --format html --output ./reports/build-metrics.html

# Generate PDF report
lablabbean report build --format pdf --output ./reports/build-metrics.pdf
```

### Example: Custom Report Provider

```csharp
using LablabBean.Reporting.Abstractions.Attributes;
using LablabBean.Reporting.Abstractions.Contracts;

[ReportProvider("my-custom-metrics", Name = "Custom Metrics", Version = "1.0.0", Category = "Custom")]
public class CustomMetricsProvider : IReportProvider<CustomMetricsAggregate>
{
    public CustomMetricsAggregate LoadData(string sourcePath)
    {
        // Load data from files/database/API
        return new CustomMetricsAggregate
        {
            // ... populate fields
        };
    }
}
```

---

## Next Steps

1. **Complete Phase 0**: Execute research tasks R-001 through R-008, consolidate findings in `research.md`
2. **Complete Phase 1**: Create `data-model.md` with all entities, generate contract files in `contracts/`
3. **Run agent context update**: Execute `.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude`
4. **Re-evaluate Constitution Check**: Verify design still complies with all principles and rules
5. **Proceed to `/speckit.tasks`**: Generate actionable task breakdown for implementation

---

**Status**: ✅ **Planning Phase Complete** - Ready for research and design phases.
**Branch**: `010-fastreport-reporting`
**Plan File**: `D:\lunar-snake\personal-work\yokan-projects\lablab-bean\specs\010-fastreport-reporting\plan.md`
