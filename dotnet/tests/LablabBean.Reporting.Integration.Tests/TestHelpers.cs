using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using LablabBean.Reporting.Providers.Build;
using LablabBean.Reporting.Analytics;
using LablabBean.Plugins.Reporting.Html;
using LablabBean.Plugins.Reporting.Csv;
using LablabBean.Reporting.Contracts.Models;

namespace LablabBean.Reporting.Integration.Tests;

/// <summary>
/// Test helper for creating instances with null loggers
/// </summary>
internal static class TestHelpers
{
    public static BuildMetricsProvider CreateBuildMetricsProvider()
    {
        return new BuildMetricsProvider(NullLogger<BuildMetricsProvider>.Instance);
    }

    public static SessionStatisticsProvider CreateSessionStatisticsProvider()
    {
        return new SessionStatisticsProvider(NullLogger<SessionStatisticsProvider>.Instance);
    }

    public static PluginHealthProvider CreatePluginHealthProvider()
    {
        return new PluginHealthProvider(NullLogger<PluginHealthProvider>.Instance);
    }

    public static HtmlReportRenderer CreateHtmlRenderer()
    {
        return new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance);
    }

    public static CsvReportRenderer CreateCsvRenderer()
    {
        return new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance);
    }

    public static ReportRequest CreateRequest(string? dataPath = null)
    {
        return new ReportRequest
        {
            DataPath = dataPath,

            OutputPath = string.Empty, // Will be set by tests
            Format = ReportFormat.HTML // Default format
        };
    }
}
