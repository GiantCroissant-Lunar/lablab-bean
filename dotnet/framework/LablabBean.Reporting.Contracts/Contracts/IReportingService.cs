using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Reporting.Contracts.Models;

namespace LablabBean.Reporting.Contracts.Contracts;

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
