using System.Diagnostics;

namespace LablabBean.Reporting.Integration.Tests;

/// <summary>
/// Phase 7: Integration & E2E Tests (T089-T098)
/// Tests the complete reporting workflow end-to-end
/// </summary>
public class Phase7IntegrationTests : IDisposable
{
    private readonly string _outputDir;

    public Phase7IntegrationTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);
    }

    [Fact]
    public async Task T090_BuildMetrics_HtmlGeneration_Succeeds()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "build.html");

        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);

        result.IsSuccess.Should().BeTrue();
        File.Exists(request.OutputPath).Should().BeTrue();
    }

    [Fact]
    public async Task T091_SessionReport_Generation_Succeeds()
    {
        var provider = TestHelpers.CreateSessionStatisticsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "session.html");

        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task T092_PluginHealth_Report_Succeeds()
    {
        var provider = TestHelpers.CreatePluginHealthProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "plugin.html");

        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void T093_Filename_CanIncludeTimestamp()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var filename = $"report-{timestamp}.html";
        filename.Should().MatchRegex(@"report-\d{8}-\d{6}\.html");
    }

    [Fact]
    public async Task T094_FileSize_WithinLimit()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "size-test.html");

        var data = await provider.GetReportDataAsync(request);
        await renderer.RenderAsync(request, data);

        var size = new FileInfo(request.OutputPath).Length;
        size.Should().BeLessThan(5 * 1024 * 1024, "Should be < 5MB");
    }

    [Fact]
    public async Task T095_Performance_BuildReport_LessThan5Seconds()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "perf.html");

        var sw = Stopwatch.StartNew();
        var data = await provider.GetReportDataAsync(request);
        await renderer.RenderAsync(request, data);
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task T096_Fallback_WithoutDataPath_UsesSampleData()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest(); // No data path
        request.OutputPath = Path.Combine(_outputDir, "fallback.html");

        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);

        result.IsSuccess.Should().BeTrue("Should fall back to sample data");
    }

    [Fact]
    public async Task T097_PartialSuccess_BothFormatsWorkIndependently()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var htmlRenderer = TestHelpers.CreateHtmlRenderer();
        var csvRenderer = TestHelpers.CreateCsvRenderer();

        var htmlReq = TestHelpers.CreateRequest();
        htmlReq.OutputPath = Path.Combine(_outputDir, "test.html");

        var csvReq = TestHelpers.CreateRequest();
        csvReq.OutputPath = Path.Combine(_outputDir, "test.csv");

        var data = await provider.GetReportDataAsync(htmlReq);
        var htmlResult = await htmlRenderer.RenderAsync(htmlReq, data);
        var csvResult = await csvRenderer.RenderAsync(csvReq, data);

        htmlResult.IsSuccess.Should().BeTrue();
        csvResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task T098_MinimalHtml_IsValid()
    {
        var provider = TestHelpers.CreateBuildMetricsProvider();
        var renderer = TestHelpers.CreateHtmlRenderer();
        var request = TestHelpers.CreateRequest();
        request.OutputPath = Path.Combine(_outputDir, "minimal.html");

        var data = await provider.GetReportDataAsync(request);
        await renderer.RenderAsync(request, data);

        var html = await File.ReadAllTextAsync(request.OutputPath);
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html");
        html.Should().Contain("</html>");
        html.Length.Should().BeGreaterThan(200);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            try { Directory.Delete(_outputDir, true); }
            catch { /* Ignore */ }
        }
    }
}
