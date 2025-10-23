using System.Threading;
using System.Threading.Tasks;
using LablabBean.Reporting.Contracts.Models;

namespace LablabBean.Reporting.Contracts.Contracts;

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
