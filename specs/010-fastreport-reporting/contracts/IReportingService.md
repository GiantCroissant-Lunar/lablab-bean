# IReportingService Interface

**Location**: `dotnet/framework/LablabBean.Reporting.Abstractions/Contracts/IReportingService.cs`

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Reporting.Abstractions.Models;

namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Orchestrates report generation by coordinating providers and renderers.
/// Main entry point for report generation.
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates a report by resolving the provider, retrieving data, and rendering.
    /// </summary>
    /// <param name="providerName">Name of the provider (e.g., "BuildMetrics", "Session")</param>
    /// <param name="request">Report request with format, output path, etc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing output file path and any errors</returns>
    Task<ReportResult> GenerateReportAsync(string providerName, ReportRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all available report providers.
    /// </summary>
    IEnumerable<ReportMetadata> GetAvailableProviders();
}
```

**Implementation Example**:
```csharp
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Core;

public class ReportingService : IReportingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportingService> _logger;
    
    public ReportingService(IServiceProvider serviceProvider, ILogger<ReportingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<ReportResult> GenerateReportAsync(string providerName, ReportRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating report: {ProviderName}, Format: {Format}", providerName, request.Format);
        
        // Resolve provider by name
        var providerType = ReportProviderRegistry.Providers
            .FirstOrDefault(p => p.Name == providerName)?.Type;
        
        if (providerType == null)
            return ReportResult.Failure($"Provider '{providerName}' not found");
        
        var provider = (IReportProvider)_serviceProvider.GetRequiredService(providerType);
        
        // Get data from provider
        var data = await provider.GetReportDataAsync(request, cancellationToken);
        
        // Resolve renderer
        var renderer = _serviceProvider.GetRequiredService<IReportRenderer>();
        
        // Render report
        var result = await renderer.RenderAsync(request, data, cancellationToken);
        
        _logger.LogInformation("Report generated: {OutputPath}, Success: {Success}", result.OutputPath, result.IsSuccess);
        return result;
    }
    
    public IEnumerable<ReportMetadata> GetAvailableProviders()
    {
        return ReportProviderRegistry.Providers
            .Select(p =>
            {
                var provider = (IReportProvider)_serviceProvider.GetRequiredService(p.Type);
                return provider.GetMetadata();
            });
    }
}
```

**CLI Usage Example**:
```csharp
// In CLI command handler
var service = serviceProvider.GetRequiredService<IReportingService>();
var request = new ReportRequest
{
    Format = ReportFormat.HTML,
    OutputPath = "artifacts/reports/build-metrics.html",
    DataPath = "TestResults/*.xml"
};

var result = await service.GenerateReportAsync("BuildMetrics", request);
if (result.IsSuccess)
{
    Console.WriteLine($"✅ Report generated: {result.OutputPath}");
    Console.WriteLine($"   Size: {result.FileSizeBytes / 1024} KB");
    Console.WriteLine($"   Duration: {result.Duration.TotalSeconds:F2}s");
}
else
{
    Console.WriteLine($"❌ Report generation failed:");
    foreach (var error in result.Errors)
        Console.WriteLine($"   - {error}");
}
```
