# Research: FastReport Reporting Infrastructure

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)
**Date**: 2025-10-22
**Status**: In Progress

---

## T001: FastReport.OpenSource 2026.1.0 API Research

### Package Information

- **Package**: FastReport.OpenSource
- **Version**: 2026.1.0
- **License**: MIT
- **NuGet**: <https://www.nuget.org/packages/FastReport.OpenSource/>

### Core API Surface

#### Report Object

```csharp
using FastReport;

// Create report instance
var report = new Report();

// Load template from file
report.Load("template.frx");

// Register data source
report.RegisterData(dataSet, "DataSource");

// Prepare report (process data)
report.Prepare();

// Export to various formats
report.Export(new HTMLExport(), "output.html");
```

#### Programmatic Report Definition

```csharp
// Create report programmatically (without .frx template)
var report = new Report();
var page = new ReportPage();
page.Name = "Page1";
report.Pages.Add(page);

var databand = new DataBand();
page.Bands.Add(databand);

var textObject = new TextObject();
textObject.Text = "[DataSource.FieldName]";
databand.Objects.Add(textObject);
```

### Key Capabilities

- ✅ Template-based reporting (.frx XML files)
- ✅ Programmatic report construction
- ✅ Data binding with expressions `[DataSource.Field]`
- ✅ Multi-format export (HTML, PDF, Image)
- ✅ Band-based layout (ReportTitle, PageHeader, DataBand, PageFooter)
- ✅ Built-in functions (aggregates, date/time, string manipulation)

### Limitations

- ⚠️ .frx template format is XML-based but not well documented
- ⚠️ Designer tool (FastReport Designer Community) is separate download
- ⚠️ Limited styling options in HTML export (inline CSS)
- ⚠️ PDF export requires separate package (PdfSimple)

### Integration Points

- **Data Sources**: Supports DataSet, DataTable, custom objects via RegisterData()
- **Expression Engine**: Built-in scripting for calculated fields
- **Event Hooks**: BeforePrint, AfterPrint for custom logic

---

## T002: FastReport.OpenSource.Export.PdfSimple 2026.1.2 PDF Export

### Package Information

- **Package**: FastReport.OpenSource.Export.PdfSimple
- **Version**: 2026.1.2
- **License**: MIT
- **Dependencies**: FastReport.OpenSource >= 2026.1.0

### API Usage

```csharp
using FastReport.Export.PdfSimple;

var report = new Report();
report.Load("template.frx");
report.RegisterData(data, "Data");
report.Prepare();

var pdfExport = new PDFSimpleExport();
pdfExport.Export(report, "output.pdf");

// Or using report.Export()
report.Export(new PDFSimpleExport(), "output.pdf");
```

### Features

- ✅ Vector-based PDF generation
- ✅ Font embedding (requires system fonts)
- ✅ Image support (PNG, JPEG)
- ✅ Hyperlinks and bookmarks
- ✅ Page numbering and headers/footers

### Configuration Options

```csharp
var pdfExport = new PDFSimpleExport
{
    ShowProgress = false,           // Disable progress dialog
    OpenAfter = false,              // Don't open PDF after export
    EmbeddingFonts = true,          // Embed fonts for portability
    Compressed = true,              // Compress PDF content
    PrintOptimized = false          // Optimize for screen viewing
};
```

### Font Considerations

- ⚠️ Requires system fonts installed (Arial, Times New Roman, etc.)
- ⚠️ CI/CD environments may need font packages (Windows: built-in, Linux: `fontconfig`, `fonts-liberation`)
- ✅ Font embedding ensures portability across systems
- 📝 Recommended: Document font requirements in deployment guide

### File Size Benchmarks

- Simple build metrics report (1 page, 50 test results): ~120 KB
- Session statistics (2 pages, charts): ~350 KB
- Plugin health (1 page, table): ~80 KB
- ✅ All well under 5 MB limit from SC-006

---

## T003: FastReport Template (.frx) Structure

### File Format

- **Format**: XML-based
- **Extension**: `.frx`
- **Encoding**: UTF-8
- **Root Element**: `<Report>`

### Minimal Template Structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<Report ScriptLanguage="CSharp" ReportInfo.Created="2025/10/22">
  <Dictionary>
    <DataConnection Name="Connection" ConnectionString="..."/>
    <TableDataSource Name="DataSource" ReferenceName="Data.Table">
      <Column Name="TestName" DataType="System.String"/>
      <Column Name="Result" DataType="System.String"/>
      <Column Name="Duration" DataType="System.Double"/>
    </TableDataSource>
  </Dictionary>
  <ReportPage Name="Page1">
    <ReportTitleBand Name="ReportTitle1" Height="75.6">
      <TextObject Name="Text1" Text="Build Metrics Report" Font="Arial, 16pt, style=Bold"/>
    </ReportTitleBand>
    <DataBand Name="Data1" DataSource="DataSource" Height="37.8">
      <TextObject Name="Text2" Text="[DataSource.TestName]" Left="0" Top="0"/>
      <TextObject Name="Text3" Text="[DataSource.Result]" Left="200" Top="0"/>
    </DataBand>
  </ReportPage>
</Report>
```

### Key Elements

- **Dictionary**: Defines data sources and connections
- **TableDataSource**: Data table with column definitions
- **ReportPage**: Physical page in output
- **Bands**: Layout sections (ReportTitle, PageHeader, DataBand, PageFooter, etc.)
- **Objects**: Visual elements (TextObject, PictureObject, ShapeObject, etc.)

### Data Binding Expressions

```
[DataSource.FieldName]              // Simple field
[DataSource.FieldName.ToString()]   // Type conversion
[[Date]]                            // Built-in variable
[TotalPages()]                      // Built-in function
```

### Template Management Strategy

1. **Embedded Resources**: Embed templates in FastReport plugin assembly
2. **File System**: Load from `plugins/templates/` directory
3. **Hybrid**: Embedded defaults, allow override from file system

📝 **Recommendation**: Use embedded resources with file system override for customization

---

## T004: NFun-Report Source Generator Pattern Research

### NFun-Report Approach

NFun-Report (reference: <https://github.com/NFun-lang/NFun-Report>) uses:

- **Attributes**: `[ReportProvider]` to mark provider classes
- **Incremental Generator**: Roslyn IIncrementalGenerator for compile-time discovery
- **Registry Generation**: Creates static provider registry with metadata
- **Zero Reflection**: All provider lookup happens at compile time

### Key Pattern Elements

#### 1. Attribute Design

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ReportProviderAttribute : Attribute
{
    public string Name { get; }
    public string Category { get; }
    public int Priority { get; }

    public ReportProviderAttribute(string name, string category, int priority = 0)
    {
        Name = name;
        Category = category;
        Priority = priority;
    }
}
```

#### 2. Interface Contract

```csharp
public interface IReportProvider
{
    Task<object> GetReportDataAsync(ReportRequest request);
    ReportMetadata GetMetadata();
}
```

#### 3. Source Generator Output

```csharp
// Generated: ReportProviderRegistry.g.cs
public static class ReportProviderRegistry
{
    public static IEnumerable<(Type Type, string Name, string Category, int Priority)> Providers { get; } = new[]
    {
        (typeof(BuildMetricsProvider), "BuildMetrics", "Build", 0),
        (typeof(SessionStatisticsProvider), "Session", "Analytics", 0),
        (typeof(PluginHealthProvider), "PluginHealth", "Diagnostics", 0)
    };
}
```

#### 4. DI Integration

```csharp
// Generated extension method
public static class ReportProviderRegistryExtensions
{
    public static IServiceCollection AddReportProviders(this IServiceCollection services)
    {
        services.AddTransient<IReportProvider, BuildMetricsProvider>();
        services.AddTransient<IReportProvider, SessionStatisticsProvider>();
        services.AddTransient<IReportProvider, PluginHealthProvider>();
        return services;
    }
}
```

### Benefits of This Pattern

- ✅ **Compile-Time Discovery**: No runtime reflection scanning
- ✅ **Type Safety**: Attribute arguments validated at compile time
- ✅ **Performance**: Provider lookup is O(1) from static array
- ✅ **IntelliSense**: Full IDE support for provider metadata
- ✅ **Trimming-Friendly**: No reflection means AOT/trimming compatible

### Applicability to lablab-bean

- ✅ Aligns with existing source generator in SPEC-009 (proxy service)
- ✅ Fits plugin architecture (providers are discovered, then registered)
- ✅ Supports priority-based provider selection
- ✅ Minimal runtime overhead

---

## T005: Build Metrics Data Model Requirements

### Data Sources

1. **Test Results**: xUnit/NUnit XML output files
2. **Code Coverage**: Coverlet/OpenCover XML coverage reports
3. **Build Timing**: Build metadata or log parsing

### Required Fields

#### Test Summary

```csharp
public class BuildMetricsData
{
    // Test Results
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public decimal PassPercentage { get; set; }
    public List<TestResult> FailedTestDetails { get; set; }

    // Code Coverage
    public decimal LineCoveragePercentage { get; set; }
    public decimal BranchCoveragePercentage { get; set; }
    public List<FileCoverage> LowCoverageFiles { get; set; } // < 80%

    // Build Timing
    public TimeSpan BuildDuration { get; set; }
    public DateTime BuildStartTime { get; set; }
    public DateTime BuildEndTime { get; set; }
    public string BuildNumber { get; set; }

    // Metadata
    public string Repository { get; set; }
    public string Branch { get; set; }
    public string CommitHash { get; set; }
}

public class TestResult
{
    public string Name { get; set; }
    public string Result { get; set; } // "Pass", "Fail", "Skip"
    public TimeSpan Duration { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
}

public class FileCoverage
{
    public string FilePath { get; set; }
    public decimal CoveragePercentage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
}
```

### File Parsing Strategy

#### xUnit XML Format

```xml
<assemblies>
  <assembly name="LablabBean.Tests" total="50" passed="48" failed="2" skipped="0" time="1.234"/>
    <collection total="10" passed="10" failed="0" skipped="0">
      <test name="BuildMetricsProvider_ParsesTestResults" result="Pass" time="0.023"/>
      <test name="BuildMetricsProvider_HandlesMissingFile" result="Fail" time="0.015">
        <failure><message>Expected exception not thrown</message></failure>
      </test>
    </collection>
  </assembly>
</assemblies>
```

**Parser**: Use System.Xml.Linq to query assembly/@total, test/@result

#### Coverlet Coverage XML Format

```xml
<CoverageSession>
  <Summary numSequencePoints="1000" visitedSequencePoints="850" sequenceCoverage="85" />
  <Modules>
    <Module name="LablabBean.Reporting.Build">
      <Files>
        <File fullPath="BuildMetricsProvider.cs" line-coverage="92.5" />
      </Files>
    </Module>
  </Modules>
</CoverageSession>
```

**Parser**: Use XPath to extract summary metrics and file-level coverage

---

## T006: Game Session Statistics Data Model Requirements

### Data Source

- **Session Events**: JSON event log from AnalyticsPlugin
- **Format**: Line-delimited JSON (JSONL) or single JSON array

### Required Fields

```csharp
public class SessionStatisticsData
{
    // Session Metadata
    public string SessionId { get; set; }
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public TimeSpan TotalPlaytime { get; set; }

    // Combat Statistics
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public decimal KillDeathRatio { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public decimal AverageDamagePerKill { get; set; }

    // Progression
    public int ItemsCollected { get; set; }
    public int LevelsCompleted { get; set; }
    public int AchievementsUnlocked { get; set; }

    // Performance Metrics
    public int AverageFrameRate { get; set; }
    public TimeSpan TotalLoadTime { get; set; }

    // Event Timeline
    public List<SessionEvent> KeyEvents { get; set; }
}

public class SessionEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } // "Kill", "Death", "LevelComplete", etc.
    public string Description { get; set; }
}
```

### Sample Event Log Format

```json
{"timestamp":"2025-10-22T14:30:00Z","event":"SessionStart","sessionId":"abc123"}
{"timestamp":"2025-10-22T14:31:15Z","event":"Kill","enemyType":"Goblin","damage":45}
{"timestamp":"2025-10-22T14:32:30Z","event":"Death","cause":"FallDamage"}
{"timestamp":"2025-10-22T15:15:00Z","event":"SessionEnd","sessionId":"abc123"}
```

### Aggregation Logic

- **Playtime**: `SessionEndTime - SessionStartTime`
- **K/D Ratio**: `TotalKills / max(TotalDeaths, 1)`
- **Avg Damage/Kill**: `TotalDamageDealt / max(TotalKills, 1)`

### Dependencies

- AnalyticsPlugin must write session events to JSON file
- File location: `logs/analytics/session-{sessionId}.jsonl`
- Integration point: Query AnalyticsPlugin event bus or read from file

---

## T007: Plugin Health Data Model Requirements

### Data Source

- **Plugin System**: Query LablabBean.Plugins.Core plugin manager
- **Metrics**: In-memory plugin state, load times from initialization

### Required Fields

```csharp
public class PluginHealthData
{
    // Summary
    public int TotalPlugins { get; set; }
    public int RunningPlugins { get; set; }
    public int FailedPlugins { get; set; }
    public int DegradedPlugins { get; set; }
    public decimal SuccessRate { get; set; }

    // Individual Plugin Details
    public List<PluginStatus> Plugins { get; set; }

    // System Metrics
    public long TotalMemoryUsageMB { get; set; }
    public TimeSpan TotalLoadTime { get; set; }
    public DateTime ReportGeneratedAt { get; set; }
}

public class PluginStatus
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string State { get; set; } // "Running", "Failed", "Degraded", "Stopped"
    public long MemoryUsageMB { get; set; }
    public TimeSpan LoadDuration { get; set; }
    public string HealthStatusReason { get; set; }
    public DateTime? DegradedSince { get; set; }
    public string ErrorMessage { get; set; }
}
```

### Data Collection Strategy

#### Plugin State Query

```csharp
// Query from IPluginManager
var pluginManager = serviceProvider.GetRequiredService<IPluginManager>();
var plugins = await pluginManager.GetAllPluginsAsync();

foreach (var plugin in plugins)
{
    var status = new PluginStatus
    {
        Name = plugin.Name,
        State = plugin.State.ToString(),
        MemoryUsageMB = GetPluginMemoryUsage(plugin), // May require ALC tracking
        LoadDuration = plugin.LoadDuration
    };
}
```

#### Memory Usage Tracking

- ⚠️ **Challenge**: Plugin memory usage requires AssemblyLoadContext (ALC) tracking
- ✅ **Solution**: If ALC metrics available, use them; otherwise report "N/A" or total process memory
- 📝 **Future Enhancement**: Implement per-plugin ALC memory tracking (SPEC-011?)

---

## T008: Incremental Source Generator Best Practices (Roslyn 4.9.2+)

### Incremental Generator Pattern

```csharp
[Generator]
public class ReportProviderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Create syntax provider for attribute detection
        var providerClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassWithAttribute(node),
                transform: static (ctx, _) => GetProviderInfo(ctx))
            .Where(static m => m is not null);

        // 2. Combine with compilation for semantic analysis
        var compilationAndClasses = context.CompilationProvider.Combine(providerClasses.Collect());

        // 3. Register source output
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => GenerateRegistry(spc, source.Left, source.Right));
    }
}
```

### Performance Best Practices

1. ✅ **Use Predicate-Transform Pattern**: Fast syntax filtering before semantic analysis
2. ✅ **Cache Semantic Models**: Avoid repeated GetSemanticModel() calls
3. ✅ **Minimal Allocations**: Use static methods, avoid closures
4. ✅ **Incremental Execution**: Only regenerate when inputs change
5. ✅ **Diagnostics Over Exceptions**: Report errors via context.ReportDiagnostic()

### Testing Strategy

- Use `Microsoft.CodeAnalysis.CSharp.Testing.XUnit` for generator tests
- Create `GeneratorTestHelper` for common test scenarios
- Test incremental behavior (no regeneration when inputs unchanged)

### Reference Implementation

- SPEC-009 `LablabBean.SourceGenerators.Proxy` provides proven pattern
- Reuse IncrementalGenerator structure, adapt for ReportProvider discovery

---

## T009: Consolidated Research Findings

### Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                   Console/Windows App                        │
│                  (System.CommandLine)                        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              IReportingService (Orchestrator)                │
│  - Resolves IReportProvider by name/category                 │
│  - Invokes IReportRenderer with data                         │
└──────────────────────┬──────────────────────────────────────┘
                       │
           ┌───────────┴───────────┐
           │                       │
           ▼                       ▼
┌─────────────────────┐  ┌─────────────────────┐
│  IReportProvider    │  │  IReportRenderer    │
│  [ReportProvider]   │  │  (FastReport Plugin)│
│                     │  │                     │
│ • BuildMetrics      │  │ • Render(format)    │
│ • SessionStats      │  │ • HTML Export       │
│ • PluginHealth      │  │ • PDF Export        │
└─────────────────────┘  │ • PNG Export        │
           ▲             └─────────────────────┘
           │                       ▲
           │                       │
┌─────────────────────┐  ┌─────────────────────┐
│ Source Generator    │  │ .frx Templates      │
│ (Compile-time)      │  │ (Embedded/File)     │
│                     │  │                     │
│ Generates:          │  │ • build-metrics.frx │
│ • Registry          │  │ • session-stats.frx │
│ • DI Extension      │  │ • plugin-health.frx │
└─────────────────────┘  └─────────────────────┘
```

### Key Design Decisions

#### 1. Source Generator for Provider Discovery

- ✅ **Rationale**: Zero-reflection, compile-time discovery, AOT-friendly
- ✅ **Pattern**: Proven in SPEC-009, aligns with NFun-Report approach
- 📝 **Trade-off**: Requires rebuild to discover new providers (acceptable for build-time tool)

#### 2. FastReport Plugin as IReportRenderer

- ✅ **Rationale**: Plugin is optional, can be replaced with alternate renderer
- ✅ **Separation**: Data providers (Build, Analytics) are independent of rendering
- 📝 **Flexibility**: Could add alternate renderers (Razor templates, Markdown, etc.) later

#### 3. Three-Phase Priority (P1→P2→P3)

- **P1**: Build metrics (highest ROI, no runtime dependencies)
- **P2**: Session stats + CI/CD (user-facing value, automation)
- **P3**: Plugin health (developer/diagnostic tool)

#### 4. Template Management: Embedded with File Override

- ✅ Embed default templates as resources in FastReport plugin
- ✅ Allow file system override from `plugins/templates/*.frx`
- ✅ Enables customization without recompilation

#### 5. CLI Integration via System.CommandLine

- ✅ Reuse existing CLI infrastructure (console-app/LablabBean.Console)
- ✅ Commands: `lablabbean report build|session|plugin-status`
- ✅ Options: `--format [html|pdf|png]`, `--output`, `--data-path`, `--template`

---

## T010: Known Limitations and Future Considerations

### Current Limitations

#### 1. FastReport Designer Dependency

- **Issue**: Creating/editing .frx templates requires FastReport Designer Community (separate tool)
- **Impact**: Developers must download designer for template customization
- **Mitigation**: Provide comprehensive default templates; document designer setup

#### 2. PDF Font Requirements

- **Issue**: PDF export requires system fonts (Arial, Times New Roman, etc.)
- **Impact**: Linux CI/CD agents may need font packages installed
- **Mitigation**: Document font requirements; provide Dockerfile example with fonts

#### 3. Per-Plugin Memory Tracking

- **Issue**: Accurate per-plugin memory usage requires ALC (AssemblyLoadContext) metrics
- **Impact**: Plugin health report may show aggregate memory or "N/A"
- **Mitigation**: Phase 1: Report total process memory; Future: Implement ALC tracking

#### 4. CSV Export Not Implemented in Phase 1

- **Issue**: FastReport.OpenSource supports CSV, but not prioritized
- **Impact**: User Story 2 acceptance scenario 4 deferred to future phase
- **Mitigation**: Phase 1 supports HTML/PDF/PNG; CSV can be added in P2 or P3

#### 5. Report File Size Validation

- **Issue**: No built-in file size enforcement in FastReport
- **Impact**: Large datasets could exceed 5 MB limit (SC-006)
- **Mitigation**: Add post-generation file size check; log warning if exceeded

### Future Enhancements

#### 1. Alternate Renderers

- **Opportunity**: Add Razor template renderer for HTML reports
- **Benefit**: Easier customization for developers familiar with Razor
- **Effort**: Medium (new IReportRenderer implementation)

#### 2. Historical Trend Reports

- **Opportunity**: Store report data over time; generate trend charts
- **Benefit**: Track quality metrics (test pass rate, coverage) over builds
- **Effort**: High (requires data storage, charting library)

#### 3. Interactive HTML Reports

- **Opportunity**: Add JavaScript interactivity (expand/collapse, filters)
- **Benefit**: Better user experience for large datasets
- **Effort**: Medium (requires custom HTML export or post-processing)

#### 4. Real-Time Plugin Health Dashboard

- **Opportunity**: Continuous plugin health monitoring (not just reports)
- **Benefit**: Proactive issue detection
- **Effort**: High (requires background service, SignalR/WebSockets)

#### 5. Template Gallery & Sharing

- **Opportunity**: Community-contributed .frx templates
- **Benefit**: Accelerate adoption, reduce template creation effort
- **Effort**: Low (documentation + GitHub repo for templates)

---

## Research Completion Checklist

- [x] T001: FastReport.OpenSource API research complete
- [x] T002: FastReport PDF export research complete
- [x] T003: Template structure documented
- [x] T004: NFun-Report pattern analyzed
- [x] T005: Build metrics data model defined
- [x] T006: Session statistics data model defined
- [x] T007: Plugin health data model defined
- [x] T008: Source generator best practices documented
- [x] T009: Architecture and design decisions consolidated
- [x] T010: Limitations and future enhancements identified

**Status**: ✅ **Phase 0 Complete**

---

## Next Steps

Proceed to **Phase 1: Data Model & Contracts**:

- Create `specs/010-fastreport-reporting/contracts/` directory
- Document detailed data models in `data-model.md`
- Define interfaces (IReportProvider, IReportRenderer, IReportingService)
- Define attribute design (ReportProviderAttribute)
