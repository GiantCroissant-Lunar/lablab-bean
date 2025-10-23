# IReportRenderer Interface

**Location**: `dotnet/framework/LablabBean.Reporting.Abstractions/Contracts/IReportRenderer.cs`

```csharp
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
```

**Implementation Example (FastReport Plugin)**:

```csharp
using FastReport;
using FastReport.Export.Html;
using FastReport.Export.PdfSimple;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;

namespace LablabBean.Plugins.FastReport;

public class FastReportRenderer : IReportRenderer
{
    public IEnumerable<ReportFormat> SupportedFormats => new[]
    {
        ReportFormat.HTML,
        ReportFormat.PDF,
        ReportFormat.PNG
    };

    public async Task<ReportResult> RenderAsync(ReportRequest request, object data, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var report = new Report();

            // Load template (embedded or file)
            if (!string.IsNullOrEmpty(request.TemplatePath))
                report.Load(request.TemplatePath);
            else
                LoadEmbeddedTemplate(report, GetTemplateName(data));

            // Register data
            report.RegisterData(data, "Data");
            report.Prepare();

            // Export to format
            switch (request.Format)
            {
                case ReportFormat.PDF:
                    report.Export(new PDFSimpleExport(), request.OutputPath);
                    break;
                case ReportFormat.HTML:
                    report.Export(new HTMLExport(), request.OutputPath);
                    break;
                case ReportFormat.PNG:
                    report.Export(new ImageExport { ImageFormat = ImageExportFormat.Png }, request.OutputPath);
                    break;
            }

            var fileInfo = new FileInfo(request.OutputPath);
            return ReportResult.Success(request.OutputPath, fileInfo.Length, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            return ReportResult.Failure($"Render failed: {ex.Message}");
        }
    }
}
```
