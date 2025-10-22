# IReportProvider Interface

**Location**: `dotnet/framework/LablabBean.Reporting.Abstractions/Contracts/IReportProvider.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Reporting.Abstractions.Models;

namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Provides data for report generation.
/// Implementations are discovered via [ReportProvider] attribute and source generator.
/// </summary>
public interface IReportProvider
{
    /// <summary>
    /// Retrieves report data based on the request parameters.
    /// </summary>
    /// <param name="request">Report request containing data path, filters, etc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report data object (typically BuildMetricsData, SessionStatisticsData, or PluginHealthData)</returns>
    Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets metadata about this provider (name, supported data sources, etc.)
    /// </summary>
    ReportMetadata GetMetadata();
}
```

**Implementation Example**:
```csharp
using LablabBean.Reporting.Abstractions.Attributes;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;

namespace LablabBean.Reporting.Build;

[ReportProvider("BuildMetrics", "Build", priority: 0)]
public class BuildMetricsProvider : IReportProvider
{
    public async Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        var data = new BuildMetricsData();
        // Parse test results, coverage, build timing
        return data;
    }
    
    public ReportMetadata GetMetadata() => new()
    {
        Name = "BuildMetrics",
        Description = "Build metrics including tests, coverage, and timing",
        Category = "Build",
        SupportedFormats = new[] { ReportFormat.HTML, ReportFormat.PDF },
        DataSourcePattern = "TestResults/*.xml"
    };
}
```
