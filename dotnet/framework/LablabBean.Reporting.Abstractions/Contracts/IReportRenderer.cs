using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LablabBean.Reporting.Abstractions.Models;

namespace LablabBean.Reporting.Abstractions.Contracts;

/// <summary>
/// Renders report data to various output formats.
/// Typically implemented by reporting plugins (e.g., FastReportPlugin).
/// </summary>
public interface IReportRenderer
{
    /// <summary>
    /// Renders report data to the specified format.
    /// </summary>
    /// <param name="request">Report request containing format, output path, template, etc.</param>
    /// <param name="data">Report data from IReportProvider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing output file path and any errors</returns>
    Task<ReportResult> RenderAsync(ReportRequest request, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets supported output formats.
    /// </summary>
    IEnumerable<ReportFormat> SupportedFormats { get; }
}
