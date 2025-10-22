---
title: "NFun-Report Framework Adoption Evaluation"
date: 2025-10-22
type: findings
status: revised
tags: [reporting, architecture, evaluation, nfun-report, observability, fastreport]
related:
  - docs/findings/2025-10-22-fastreport-reevaluation.md
  - docs/findings/2025-10-22-plugin-manoi-adoption-evaluation.md
  - docs/findings/2025-10-22-application-verification.md
author: Claude Code
revision_notes: "See docs/findings/2025-10-22-fastreport-reevaluation.md for updated FastReport assessment"
---

> **⚠️ REVISION NOTICE**: This document's FastReport assessment has been revised. See [FastReport Re-evaluation](2025-10-22-fastreport-reevaluation.md) for the updated recommendation to **ADOPT FastReport.OpenSource as a reporting plugin**.

# NFun-Report Framework Adoption Evaluation

## Executive Summary

**Recommendation: PARTIAL ADOPTION** - Extract and adapt core concepts, but DO NOT adopt wholesale.

Lablab-bean already has solid observability foundations (plugin metrics, health checks, security audits, Serilog logging). NFun-Report's attribute-driven source generation pattern is valuable for **build reporting** and **game analytics**, but the framework is tightly coupled to Unity/FastReport ecosystems that don't align with lablab-bean's .NET console/terminal architecture.

**Key Finding**: Adopt the **attribute + source generator pattern** for creating typed report registries, but implement a custom lablab-bean-specific reporting abstraction tailored to console/terminal output and build metrics.

---

## What is NFun-Report?

NFun-Report is a **professional reporting system** for Unity projects with FastReport integration. It uses:

1. **Attribute-Driven Metadata** - Mark classes/methods with reporting attributes
2. **Roslyn Source Generators** - Compile-time registry generation
3. **Data Providers** - Extract data from Unity builds, assets, runtime metrics
4. **Template System** - FastReport templates for PDF/HTML/Excel output

**Core Components**:
- `Plate.Reporting.Abstractions` - Marker attributes (netstandard2.1)
- `Plate.Reporting.SourceGen` - Roslyn incremental generator
- `Plate.Reporting.Unity` - Unity build report models
- `Plate.Reporting.Winged` - Resource bundles, Asciinema, runtime metrics

---

## Component Analysis

### 1. Reporting Abstractions (Attributes)

**File**: `Plate.Reporting.Abstractions/Attributes/ReportAttributes.cs`

```csharp
[ReportProvider("id")]              // Mark report data providers
[ReportDataProvider("id")]          // Mark data extraction classes
[ReportAction("id")]                // Mark report generation methods
[ReportTemplate("id", "contentType")] // Mark report templates
```

**Purpose**: Compile-time metadata for auto-discovery of reporting components.

**Value Proposition**:
- ✅ Type-safe report provider registration
- ✅ Compile-time validation (missing providers = build error)
- ✅ Zero-reflection runtime discovery (all providers known at compile time)
- ✅ Version tracking per provider

**Alignment with Lablab-bean**: ⭐⭐⭐⭐⭐ **EXCELLENT**

Lablab-bean's plugin system already uses attribute-driven discovery. This pattern fits perfectly for:
- Build metrics providers
- Game statistics collectors
- Performance profilers
- Test result aggregators

---

### 2. Source Generator (Registry Generation)

**File**: `Plate.Reporting.SourceGen/Generators/ReportRegistryGenerator.cs`

**What it does**:
1. Scans compilation for classes/methods with report attributes
2. Extracts metadata (IDs, versions, categories)
3. Generates static `ReportRegistry` class with compile-time provider lists

**Generated Output**:
```csharp
namespace Plate.Reporting.Generated
{
    internal static class ReportRegistry
    {
        public static string Version => "0.2";
        public static string[] Providers => new string[] { "winged.resource-bundles", ... };
        public static string[] DataProviders => new string[] { ... };
        public static string[] Actions => new string[] { ... };
        public static string[] Templates => new string[] { ... };
    }
}
```

**Benefits**:
- ✅ Zero-reflection provider discovery
- ✅ Compile-time errors for duplicate IDs
- ✅ Minimal runtime overhead (static arrays)
- ✅ Incremental generator (fast rebuilds)

**Alignment with Lablab-bean**: ⭐⭐⭐⭐⭐ **EXCELLENT**

This is the **most valuable component** for lablab-bean. The pattern is proven in SPEC-009 (Proxy Service Source Generator).

---

### 3. Data Providers (Winged Examples)

**File**: `Plate.Reporting.Winged/WingedProviders.cs`

**Example Providers**:
```csharp
[ReportProvider("winged.resource-bundles", Name = "...", Version = "0.1")]
public static class WingedResourceBundleProvider
{
    public static ResourceBundleAggregate LoadFromArtifacts(string dir) { ... }
}

[ReportProvider("winged.runtime-metrics", Name = "...", Version = "0.1")]
public static class WingedRuntimeMetricsProvider
{
    public static RuntimeMetricsAggregate LoadFromArtifacts(string dir) { ... }
}
```

**Pattern**:
- Static classes with `[ReportProvider]` attributes
- Load data from JSON files in artifact directories
- Return strongly-typed aggregates
- Graceful error handling (malformed JSON ignored)

**Value Proposition**:
- ✅ Clear provider contract
- ✅ Version tracking per provider
- ✅ Category-based organization
- ✅ Artifact-based data loading

**Alignment with Lablab-bean**: ⭐⭐⭐⭐ **GOOD**

The pattern is excellent, but implementations are Unity/Winged-specific. Lablab-bean would need:
- Plugin metrics providers
- Build metrics providers
- Test result providers
- Game statistics providers

---

### 4. Unity Integration

**Not applicable** - Lablab-bean is NOT a Unity project.

NFun-Report's Unity integration (build reports, asset analysis, Editor windows) has **zero value** for lablab-bean's console/terminal architecture.

---

## Existing Lablab-bean Reporting Infrastructure

### ✅ What Lablab-bean Already Has

| Category | Component | Completeness |
|----------|-----------|--------------|
| **Plugin Metrics** | PluginMetrics, PluginSystemMetrics | 🟢 **Production-Grade** |
| **Health Monitoring** | PluginHealthChecker | 🟢 **Production-Grade** |
| **Security Audit** | SecurityAuditLog | 🟢 **Production-Grade** |
| **Logging** | Serilog with file output | 🟢 **Production-Grade** |
| **Analytics** | AnalyticsPlugin (event-driven) | 🟡 **Basic** |
| **Build Metadata** | Version info JSON | 🟡 **Basic** |
| **Testing** | xUnit + coverlet (code coverage) | 🟡 **Basic** |

### ❌ What Lablab-bean is Missing

| Gap | Priority | NFun-Report Helps? |
|-----|----------|-------------------|
| **Build Reports** | 🔥 **High** | ⚠️ Pattern Yes, Implementation No |
| **Test Reports** | 🔥 **High** | ⚠️ Pattern Yes, Implementation No |
| **Game Statistics** | 🔥 **High** | ⚠️ Pattern Yes, Implementation No |
| **Performance Profiling** | 📊 **Medium** | ⚠️ Pattern Yes, Implementation No |
| **Report Output Formats** | 📊 **Medium** | ❌ No (FastReport is Unity-focused) |
| **Report Templates** | 📉 **Low** | ❌ No (Unity Editor only) |

---

## Gap Analysis: What Lablab-bean Needs

### 1. Build Reporting - **HIGH PRIORITY** 🔥

**Current State**: Nuke build creates version JSON, but no comprehensive build reports.

**Needs**:
- Build duration metrics
- Test result aggregation (pass/fail/skip counts)
- Code coverage summaries
- Artifact size tracking
- Dependency analysis
- Build failure diagnostics

**NFun-Report Alignment**: ⭐⭐⭐⭐ **Good Pattern, Wrong Implementation**

NFun-Report's provider pattern is ideal, but implementations are Unity-specific. Lablab-bean needs:
```csharp
[ReportProvider("lablab.build-metrics", Category = "Build")]
public static class BuildMetricsProvider
{
    public static BuildMetricsAggregate LoadFromArtifacts(string artifactsDir) { ... }
}

[ReportProvider("lablab.test-results", Category = "Build")]
public static class TestResultsProvider
{
    public static TestResultsAggregate LoadFromArtifacts(string artifactsDir) { ... }
}
```

---

### 2. Game Statistics - **HIGH PRIORITY** 🔥

**Current State**: AnalyticsPlugin tracks basic events (spawns, moves, combat), but no persistent reporting.

**Needs**:
- Player session statistics (playtime, actions per minute)
- Combat analytics (kills, deaths, accuracy, damage)
- Level progression (time per level, completion rate)
- Inventory/loot statistics
- Achievement tracking
- Session summaries (export to JSON/CSV)

**NFun-Report Alignment**: ⭐⭐⭐⭐⭐ **Excellent Pattern**

```csharp
[ReportProvider("lablab.game-stats", Category = "Analytics")]
public static class GameStatisticsProvider
{
    public static GameSessionAggregate LoadFromSessions(string sessionDir) { ... }
}

[ReportProvider("lablab.combat-stats", Category = "Analytics")]
public static class CombatStatisticsProvider
{
    public static CombatAggregate LoadFromSessions(string sessionDir) { ... }
}
```

---

### 3. Performance Profiling - **MEDIUM PRIORITY** 📊

**Current State**: Plugin load times and memory tracked. No runtime performance metrics.

**Needs**:
- Frame time metrics (avg, min, max, P95, P99)
- GC pressure tracking
- Game tick timing
- Rendering performance (if using SadConsole)
- CPU/memory hotspots

**NFun-Report Alignment**: ⭐⭐⭐⭐ **Good Pattern**

```csharp
[ReportProvider("lablab.performance-metrics", Category = "Performance")]
public static class PerformanceMetricsProvider
{
    public static PerformanceAggregate LoadFromRuntimeLogs(string logsDir) { ... }
}
```

---

### 4. Test Reporting - **HIGH PRIORITY** 🔥

**Current State**: Tests run via xUnit, coverage via coverlet. No aggregated reports.

**Needs**:
- Test result summaries (pass/fail/skip counts)
- Test duration metrics
- Code coverage reports (line/branch coverage %)
- Test failure analysis
- Historical trend tracking

**NFun-Report Alignment**: ⭐⭐⭐⭐ **Good Pattern**

```csharp
[ReportProvider("lablab.test-results", Category = "Testing")]
public static class TestResultsProvider
{
    public static TestResultsAggregate LoadFromTestOutput(string outputDir) { ... }
}

[ReportProvider("lablab.code-coverage", Category = "Testing")]
public static class CodeCoverageProvider
{
    public static CoverageAggregate LoadFromCoverletOutput(string outputDir) { ... }
}
```

---

## Adoption Recommendations by Component

### ✅ 1. Reporting Abstractions (Attributes) - **ADOPT & ADAPT**

**What to Adopt**:
- Attribute pattern for marking report providers
- Compile-time metadata extraction
- Version tracking per provider
- Category-based organization

**How to Adapt**:
```csharp
namespace LablabBean.Reporting.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ReportProviderAttribute : Attribute
{
    public ReportProviderAttribute(string id) => Id = id;
    public string Id { get; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Category { get; set; }  // "Build", "Analytics", "Performance", "Testing"
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ReportExporterAttribute : Attribute
{
    public ReportExporterAttribute(string format) => Format = format; // "json", "csv", "markdown"
    public string Format { get; }
}
```

**Migration Cost**: Low (create new project)

---

### ✅ 2. Source Generator - **ADOPT & ADAPT**

**What to Adopt**:
- Incremental generator pattern
- Compile-time registry generation
- Static arrays for zero-reflection lookup

**How to Adapt**:
```csharp
namespace LablabBean.Reporting.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class ReportRegistryGenerator : IIncrementalGenerator
{
    // Generate:
    // - ReportRegistry.Providers[] - All report provider IDs
    // - ReportRegistry.GetProvider(id) - Lookup by ID
    // - ReportRegistry.GetProvidersByCategory(category) - Filter by category
}
```

**Generated Output**:
```csharp
namespace LablabBean.Reporting.Generated
{
    internal static class ReportRegistry
    {
        public static string[] Providers => new[]
        {
            "lablab.build-metrics",
            "lablab.test-results",
            "lablab.game-stats"
        };

        public static IReadOnlyDictionary<string, ProviderMetadata> Metadata => ...;
    }
}
```

**Migration Cost**: Medium (adapt NFun-Report generator)

---

### ❌ 3. Unity Integration - **DO NOT ADOPT**

**Reason**: Lablab-bean is NOT a Unity project.

**Migration Cost**: N/A

---

### ⚠️ 4. Data Providers (Winged) - **PATTERN ONLY**

**What to Adopt**: Provider pattern, NOT implementations.

**What NOT to Adopt**:
- Unity-specific providers
- FastReport templates
- Unity Editor integration

**What TO Implement**:
- Build metrics providers
- Test result providers
- Game statistics providers
- Performance metric providers

**Migration Cost**: High (all providers need custom implementation)

---

### ❌ 5. FastReport Templates - **DO NOT ADOPT**

**Reason**: FastReport is Unity-focused. Lablab-bean needs:
- Console/terminal output (Spectre.Console, System.CommandLine)
- Markdown reports (for GitHub Actions)
- JSON/CSV export (for automation)
- HTML reports (static files for CI/CD)

**Alternative**: Use existing .NET libraries:
- **Spectre.Console** - Terminal tables, charts, progress bars
- **MarkdownBuilder** - Generate markdown reports
- **System.Text.Json** - JSON export
- **CsvHelper** - CSV export

**Migration Cost**: N/A

---

## Recommended Action Plan

### Phase 1: Create Reporting Abstractions ✅ **RECOMMENDED**

**Timeline**: 1 week

**Tasks**:
1. Create `LablabBean.Reporting.Abstractions` project (netstandard2.1)
2. Define attributes:
   - `[ReportProvider(id)]`
   - `[ReportExporter(format)]`
   - `[ReportCategory(category)]`
3. Define base interfaces:
   - `IReportProvider<T>` - Generic provider contract
   - `IReportExporter` - Export to various formats
4. Document provider contract

**Outcome**: Attribute-driven reporting foundation ready for source generation.

---

### Phase 2: Implement Source Generator ✅ **RECOMMENDED**

**Timeline**: 1-2 weeks

**Tasks**:
1. Create `LablabBean.Reporting.SourceGen` project (netstandard2.0)
2. Adapt NFun-Report's `ReportRegistryGenerator`:
   - Scan for `[ReportProvider]` attributes
   - Extract metadata (ID, name, version, category)
   - Generate `ReportRegistry` class with provider lookup
3. Add unit tests (generator verification tests)
4. Document usage in README

**Outcome**: Compile-time report provider discovery with zero reflection.

---

### Phase 3: Implement Build Metrics Providers 🔥 **HIGH PRIORITY**

**Timeline**: 2 weeks

**Tasks**:
1. Create `LablabBean.Reporting.Build` project
2. Implement providers:
   ```csharp
   [ReportProvider("lablab.build-metrics", Category = "Build")]
   public static class BuildMetricsProvider { ... }

   [ReportProvider("lablab.test-results", Category = "Build")]
   public static class TestResultsProvider { ... }

   [ReportProvider("lablab.code-coverage", Category = "Build")]
   public static class CodeCoverageProvider { ... }
   ```
3. Integrate with Nuke build:
   - Capture build duration
   - Parse xUnit test results (XML output)
   - Parse coverlet coverage (JSON output)
   - Export aggregated JSON report
4. Generate markdown summary for CI/CD

**Outcome**: Comprehensive build reporting in CI/CD pipelines.

---

### Phase 4: Implement Game Statistics Providers 🔥 **HIGH PRIORITY**

**Timeline**: 2 weeks

**Tasks**:
1. Create `LablabBean.Reporting.Analytics` project
2. Enhance existing `AnalyticsPlugin`:
   - Add session tracking (start time, end time, playtime)
   - Add combat statistics (kills, deaths, damage)
   - Add level progression tracking
   - Export session data to JSON
3. Implement providers:
   ```csharp
   [ReportProvider("lablab.game-stats", Category = "Analytics")]
   public static class GameStatisticsProvider { ... }

   [ReportProvider("lablab.combat-stats", Category = "Analytics")]
   public static class CombatStatisticsProvider { ... }
   ```
4. Add exporters:
   - JSON export (for automation)
   - CSV export (for analysis in Excel)
   - Markdown export (for session summaries)

**Outcome**: Player-facing session statistics and analytics.

---

### Phase 5: Implement Report Exporters 📊 **MEDIUM PRIORITY**

**Timeline**: 1 week

**Tasks**:
1. Create `LablabBean.Reporting.Exporters` project
2. Implement exporters:
   - `JsonReportExporter` - System.Text.Json
   - `CsvReportExporter` - CsvHelper
   - `MarkdownReportExporter` - Custom markdown builder
   - `ConsoleReportExporter` - Spectre.Console tables
3. Add exporter registry (attribute-driven)
4. Document exporter usage

**Outcome**: Flexible report output formats.

---

### Phase 6: Implement Performance Profiling (Optional) 📊 **LOW PRIORITY**

**Timeline**: 2 weeks

**Tasks**:
1. Create `LablabBean.Reporting.Performance` project
2. Add runtime metrics collection:
   - Frame time tracking
   - GC metrics
   - Game tick timing
3. Implement providers:
   ```csharp
   [ReportProvider("lablab.performance-metrics", Category = "Performance")]
   public static class PerformanceMetricsProvider { ... }
   ```
4. Integrate with game loop

**Outcome**: Runtime performance diagnostics.

---

## Risk Analysis

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Source Generator Complexity** | 🟡 **Medium** | Adapt proven NFun-Report generator; thorough testing |
| **Over-Engineering** | 🟡 **Medium** | Start with build metrics only; iterate based on value |
| **Performance Overhead** | 🟢 **Low** | Source generation is compile-time; zero runtime cost |
| **Attribute Pollution** | 🟢 **Low** | Attributes are opt-in and self-documenting |

### Business Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Scope Creep** | 🟡 **Medium** | Phased approach; validate value after each phase |
| **Time Investment** | 🟡 **Medium** | Phases 1-2 are foundational (1-3 weeks); high ROI |
| **Maintenance Burden** | 🟢 **Low** | Minimal - source generators are stable once working |

---

## Comparison: NFun-Report vs Lablab-bean Needs

| Feature | NFun-Report | Lablab-bean Needs | Alignment |
|---------|-------------|-------------------|-----------|
| **Attribute-Driven Metadata** | ✅ Yes | ✅ Yes | 🟢 **Perfect** |
| **Source Generation** | ✅ Yes | ✅ Yes | 🟢 **Perfect** |
| **Unity Integration** | ✅ Yes | ❌ Not needed | 🔴 **Mismatch** |
| **FastReport Templates** | ✅ Yes | ❌ Not needed | 🔴 **Mismatch** |
| **Build Metrics** | ❌ No | ✅ Needed | 🟡 **Gap** |
| **Game Statistics** | ❌ No | ✅ Needed | 🟡 **Gap** |
| **Test Reporting** | ❌ No | ✅ Needed | 🟡 **Gap** |
| **Performance Profiling** | ⚠️ Limited | ✅ Needed | 🟡 **Gap** |
| **Console Output** | ❌ No | ✅ Needed | 🟡 **Gap** |
| **JSON/CSV Export** | ⚠️ Limited | ✅ Needed | 🟡 **Gap** |

**Alignment Score**: 40% (2/5 core features align)

**Conclusion**: NFun-Report's **pattern is excellent**, but **implementation is wrong ecosystem**.

---

## Final Recommendation

### ✅ DO ADOPT (Pattern Only)

1. **Attribute-driven provider registration** - Proven pattern, fits lablab-bean's plugin architecture
2. **Roslyn source generator** - Compile-time discovery, zero reflection overhead
3. **Provider contract pattern** - Clear separation of concerns

### ❌ DO NOT ADOPT (Implementation)

1. **Unity-specific providers** - Wrong platform
2. **FastReport templates** - Wrong output format
3. **Unity Editor integration** - Not applicable

### ✅ DO IMPLEMENT (Custom for Lablab-bean)

1. **Build metrics providers** - Nuke integration, test results, coverage
2. **Game statistics providers** - Enhance AnalyticsPlugin
3. **Report exporters** - JSON, CSV, Markdown, Console (Spectre.Console)

---

## Implementation Strategy

**Recommended Approach**: **"Inspired By, Not Cloned From"**

1. **Extract Core Pattern** (Phases 1-2):
   - Create lablab-bean-specific reporting abstractions
   - Adapt NFun-Report's source generator for lablab-bean
   - Focus on compile-time provider discovery

2. **Implement High-Value Providers** (Phases 3-4):
   - Start with build metrics (CI/CD value)
   - Add game statistics (player value)
   - Validate ROI before expanding

3. **Iterate Based on Need** (Phases 5-6):
   - Add exporters as needed
   - Add performance profiling if bottlenecks identified

**Success Metrics**:
- ✅ Build reports generated in CI/CD
- ✅ Test coverage visible in build artifacts
- ✅ Game session statistics exportable to JSON/CSV
- ✅ Zero-reflection provider discovery working
- ✅ Minimal performance overhead

---

## Spec-Kit Recommendation

**Should we follow spec-kit to create a spec?** ✅ **YES**

Given the scope (6 phases, multiple projects, source generator complexity), a spec is recommended:

**Recommended Spec Structure**:

```
SPEC-011: Reporting Infrastructure
├── Phase 0: Foundation Setup
│   ├── Create LablabBean.Reporting.Abstractions
│   ├── Define attribute contracts
│   └── Document provider pattern
├── Phase 1: Source Generator
│   ├── Adapt NFun-Report generator
│   ├── Implement ReportRegistry generation
│   └── Add unit tests
├── Phase 2: Build Reporting
│   ├── BuildMetricsProvider
│   ├── TestResultsProvider
│   ├── CodeCoverageProvider
│   └── Nuke integration
├── Phase 3: Game Analytics
│   ├── Enhance AnalyticsPlugin
│   ├── GameStatisticsProvider
│   ├── CombatStatisticsProvider
│   └── Session export (JSON/CSV)
├── Phase 4: Report Exporters
│   ├── JsonReportExporter
│   ├── CsvReportExporter
│   ├── MarkdownReportExporter
│   └── ConsoleReportExporter (Spectre.Console)
└── Phase 5: Performance Profiling (Optional)
    ├── Runtime metrics collection
    ├── PerformanceMetricsProvider
    └── Game loop integration
```

**Use Spec-Kit Commands**:
1. `/speckit.specify` - Create initial specification from requirements
2. `/speckit.plan` - Generate detailed implementation plan
3. `/speckit.tasks` - Generate task breakdown
4. `/speckit.implement` - Execute implementation plan

---

## Conclusion

### Summary Table

| Component | Action | Reason |
|-----------|--------|--------|
| **Attribute Pattern** | ✅ **Adopt** | Proven, fits lablab-bean architecture |
| **Source Generator** | ✅ **Adapt** | High value, proven in SPEC-009 |
| **Unity Providers** | ❌ **Reject** | Wrong platform (Unity vs .NET console) |
| **FastReport** | ❌ **Reject** | Wrong output format (need console/JSON/CSV) |
| **Build Metrics** | ✅ **Implement** | High ROI for CI/CD |
| **Game Analytics** | ✅ **Implement** | Player-facing value |
| **Report Exporters** | ✅ **Implement** | Flexible output formats |

### ROI Assessment

**Investment**: 6-10 weeks (phased)
**Return**:
- ✅ Comprehensive build reporting (CI/CD visibility)
- ✅ Test coverage metrics (quality assurance)
- ✅ Game session statistics (player engagement)
- ✅ Zero-reflection provider discovery (performance)
- ✅ Extensible reporting framework (future growth)

**Verdict**: **HIGH ROI** - Pattern is proven, implementation is incremental, value is immediate.

---

## Next Steps

1. **Review this evaluation** with stakeholders
2. **Decide on phased approach** (all phases vs subset)
3. **Run spec-kit commands** if proceeding:
   - `/speckit.specify` - Generate SPEC-011
   - `/speckit.plan` - Create implementation plan
   - `/speckit.tasks` - Generate actionable tasks
4. **Start with Phase 1-2** (Abstractions + Source Generator) as foundation
5. **Validate ROI** after Phase 3 (Build Metrics) before expanding

---

## Appendix: Proposed Project Structure

```
lablab-bean/dotnet/framework/
├── LablabBean.Reporting.Abstractions/      (netstandard2.1)
│   ├── Attributes/
│   │   ├── ReportProviderAttribute.cs
│   │   ├── ReportExporterAttribute.cs
│   │   └── ReportCategoryAttribute.cs
│   └── Contracts/
│       ├── IReportProvider.cs
│       └── IReportExporter.cs
│
├── LablabBean.Reporting.SourceGen/         (netstandard2.0, Analyzer)
│   └── Generators/
│       └── ReportRegistryGenerator.cs
│
├── LablabBean.Reporting.Build/             (net8.0)
│   ├── Providers/
│   │   ├── BuildMetricsProvider.cs
│   │   ├── TestResultsProvider.cs
│   │   └── CodeCoverageProvider.cs
│   └── Models/
│       ├── BuildMetricsAggregate.cs
│       └── TestResultsAggregate.cs
│
├── LablabBean.Reporting.Analytics/         (net8.0)
│   ├── Providers/
│   │   ├── GameStatisticsProvider.cs
│   │   └── CombatStatisticsProvider.cs
│   └── Models/
│       ├── GameSessionAggregate.cs
│       └── CombatAggregate.cs
│
└── LablabBean.Reporting.Exporters/         (net8.0)
    ├── JsonReportExporter.cs
    ├── CsvReportExporter.cs
    ├── MarkdownReportExporter.cs
    └── ConsoleReportExporter.cs
```

---

## References

**NFun-Report Framework**:
- `/ref-projects/nfun-report/dotnet/Plate.Reporting.Abstractions/`
- `/ref-projects/nfun-report/dotnet/Plate.Reporting.SourceGen/`
- `/ref-projects/nfun-report/dotnet/Plate.Reporting.Winged/`

**Lablab-bean Existing Infrastructure**:
- `/dotnet/framework/LablabBean.Plugins.Core/PluginMetrics.cs`
- `/dotnet/framework/LablabBean.Plugins.Core/PluginHealthCheck.cs`
- `/dotnet/framework/LablabBean.Plugins.Core/PluginAdminService.cs`
- `/plugins/LablabBean.Plugins.Analytics/AnalyticsPlugin.cs`

**Related Specifications**:
- SPEC-009: Proxy Service Source Generator (proven source generator pattern)

---

**Document Version**: 1.0
**Last Updated**: 2025-10-22
**Author**: Claude Code
**Status**: Final
**Next Review**: Before implementing SPEC-011 (if approved)
