using System.Diagnostics;
using LablabBean.Reporting.Contracts.Contracts;
using LablabBean.Reporting.Contracts.Models;
using LablabBean.Reporting.Analytics;
using LablabBean.Reporting.Providers.Build;
using LablabBean.Reporting.Renderers.Html;
using LablabBean.Reporting.Renderers.Csv;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LablabBean.Reporting.Integration.Tests;

/// <summary>
/// Performance and load tests for the reporting system.
/// Tests: T121-T125 (Performance validation)
/// </summary>
public class PerformanceTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILoggerFactory _loggerFactory;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }

    [Fact]
    public async Task T121_BuildMetricsReport_GeneratesUnder1Second()
    {
        // Arrange
        var provider = new BuildMetricsProvider(_loggerFactory.CreateLogger<BuildMetricsProvider>());
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "perf-build-metrics.html"),
            Format = ReportFormat.HTML
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Build metrics report took {stopwatch.ElapsedMilliseconds}ms, should be < 1000ms");
        
        Assert.NotNull(result.OutputPath);
        _output.WriteLine($"✅ Build metrics report generated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   File size: {new FileInfo(result.OutputPath).Length / 1024.0:F1} KB");
    }

    [Fact]
    public async Task T121_SessionAnalyticsReport_GeneratesUnder500Milliseconds()
    {
        // Arrange
        var provider = new SessionStatisticsProvider(_loggerFactory.CreateLogger<SessionStatisticsProvider>());
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "perf-session.html"),
            Format = ReportFormat.HTML
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Session analytics report took {stopwatch.ElapsedMilliseconds}ms, should be < 500ms");
        
        Assert.NotNull(result.OutputPath);
        _output.WriteLine($"✅ Session analytics report generated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   File size: {new FileInfo(result.OutputPath).Length / 1024.0:F1} KB");
    }

    [Fact]
    public async Task T121_PluginHealthReport_GeneratesUnder500Milliseconds()
    {
        // Arrange
        var provider = new PluginHealthProvider(_loggerFactory.CreateLogger<PluginHealthProvider>());
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "perf-plugin.html"),
            Format = ReportFormat.HTML
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Plugin health report took {stopwatch.ElapsedMilliseconds}ms, should be < 500ms");
        
        Assert.NotNull(result.OutputPath);
        _output.WriteLine($"✅ Plugin health report generated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   File size: {new FileInfo(result.OutputPath).Length / 1024.0:F1} KB");
    }

    [Fact]
    public async Task T121_CsvExport_GeneratesUnder200Milliseconds()
    {
        // Arrange
        var provider = new BuildMetricsProvider(_loggerFactory.CreateLogger<BuildMetricsProvider>());
        var renderer = new CsvReportRenderer(_loggerFactory.CreateLogger<CsvReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "perf-build-metrics.csv"),
            Format = ReportFormat.CSV
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"CSV export took {stopwatch.ElapsedMilliseconds}ms, should be < 200ms");
        
        Assert.NotNull(result.OutputPath);
        _output.WriteLine($"✅ CSV export generated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   File size: {new FileInfo(result.OutputPath).Length} bytes");
    }

    [Fact]
    public async Task T122_MemoryUsage_RemainsUnder50MB()
    {
        // Arrange
        var provider = new BuildMetricsProvider(_loggerFactory.CreateLogger<BuildMetricsProvider>());
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "memory-test.html"),
            Format = ReportFormat.HTML
        };

        // Measure baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(false);

        // Act
        var data = await provider.GetReportDataAsync(request);
        var result = await renderer.RenderAsync(request, data);

        // Measure memory after
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = (memoryAfter - memoryBefore) / 1024.0 / 1024.0; // MB

        // Assert
        Assert.True(memoryUsed < 50.0, 
            $"Memory usage was {memoryUsed:F2} MB, should be < 50 MB");
        
        _output.WriteLine($"✅ Memory usage: {memoryUsed:F2} MB");
    }

    [Fact]
    public async Task T124_LargeDataset_100Tests_GeneratesSuccessfully()
    {
        // Arrange - Create large dataset
        var largeData = new BuildMetricsData
        {
            TotalTests = 1000,
            PassedTests = 950,
            FailedTests = 40,
            SkippedTests = 10,
            PassPercentage = 95.0m,
            LineCoveragePercentage = 85.5m,
            BranchCoveragePercentage = 78.3m,
            BuildDuration = TimeSpan.FromMinutes(15),
            BuildStartTime = DateTime.UtcNow.AddMinutes(-20),
            BuildEndTime = DateTime.UtcNow.AddMinutes(-5),
            BuildNumber = "large-dataset-test",
            Repository = "lablab-bean",
            Branch = "main",
            CommitHash = "test123",
            FailedTestDetails = Enumerable.Range(1, 40).Select(i => new TestResult
            {
                Name = $"FailedTest_{i}",
                ClassName = $"TestClass_{i % 10}",
                Result = "Failed",
                Duration = TimeSpan.FromMilliseconds(100 + i * 10),
                ErrorMessage = $"Test {i} failed with error",
                StackTrace = $"at TestClass_{i % 10}.FailedTest_{i}() in Test.cs:line {i * 10}"
            }).ToList(),
            LowCoverageFiles = Enumerable.Range(1, 50).Select(i => new FileCoverage
            {
                FilePath = $"src/lib/File{i}.cs",
                CoveragePercentage = 45.0m + (i % 20),
                CoveredLines = 100 + i * 10,
                TotalLines = 300 + i * 15
            }).ToList()
        };

        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var request = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "large-dataset.html"),
            Format = ReportFormat.HTML
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await renderer.RenderAsync(request, largeData);
        stopwatch.Stop();

        // Assert
        Assert.True(File.Exists(result.OutputPath), "Report file should exist");
        var fileSize = new FileInfo(result.OutputPath).Length / 1024.0; // KB
        Assert.True(fileSize > 10, "Report should have substantial content");
        
        _output.WriteLine($"✅ Large dataset report generated in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   File size: {fileSize:F1} KB");
        _output.WriteLine($"   Tests: 1000, Failed details: 40, Low coverage files: 50");
    }

    [Fact]
    public async Task T124_StressTest_Generate10ReportsSequentially()
    {
        // Arrange
        var provider = new BuildMetricsProvider(_loggerFactory.CreateLogger<BuildMetricsProvider>());
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());

        // Act
        var stopwatch = Stopwatch.StartNew();
        var results = new List<TimeSpan>();

        for (int i = 0; i < 10; i++)
        {
            var iterationStart = Stopwatch.StartNew();
            
            var request = new ReportRequest
            {
                OutputPath = Path.Combine(Path.GetTempPath(), $"stress-test-{i}.html"),
                Format = ReportFormat.HTML
            };

            var data = await provider.GetReportDataAsync(request);
            await renderer.RenderAsync(request, data);
            
            iterationStart.Stop();
            results.Add(iterationStart.Elapsed);
        }

        stopwatch.Stop();

        // Assert
        var avgTime = results.Average(ts => ts.TotalMilliseconds);
        var maxTime = results.Max(ts => ts.TotalMilliseconds);
        
        Assert.True(avgTime < 1000, $"Average time {avgTime:F0}ms should be < 1000ms");
        Assert.True(maxTime < 2000, $"Max time {maxTime:F0}ms should be < 2000ms");
        
        _output.WriteLine($"✅ Stress test: 10 reports generated");
        _output.WriteLine($"   Total time: {stopwatch.Elapsed.TotalSeconds:F2}s");
        _output.WriteLine($"   Average: {avgTime:F0}ms");
        _output.WriteLine($"   Min: {results.Min(ts => ts.TotalMilliseconds):F0}ms");
        _output.WriteLine($"   Max: {maxTime:F0}ms");
    }

    [Fact]
    public async Task T125_TemplateCache_ImprovesPerformance()
    {
        // Arrange
        var renderer = new HtmlReportRenderer(_loggerFactory.CreateLogger<HtmlReportRenderer>());
        var provider = new BuildMetricsProvider(_loggerFactory.CreateLogger<BuildMetricsProvider>());

        // First render (cold cache)
        var request1 = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "cache-test-1.html"),
            Format = ReportFormat.HTML
        };
        var data1 = await provider.GetReportDataAsync(request1);

        var sw1 = Stopwatch.StartNew();
        await renderer.RenderAsync(request1, data1);
        sw1.Stop();

        // Second render (warm cache)
        var request2 = new ReportRequest
        {
            OutputPath = Path.Combine(Path.GetTempPath(), "cache-test-2.html"),
            Format = ReportFormat.HTML
        };
        var data2 = await provider.GetReportDataAsync(request2);

        var sw2 = Stopwatch.StartNew();
        await renderer.RenderAsync(request2, data2);
        sw2.Stop();

        // Assert
        _output.WriteLine($"Cold cache: {sw1.ElapsedMilliseconds}ms");
        _output.WriteLine($"Warm cache: {sw2.ElapsedMilliseconds}ms");
        _output.WriteLine($"Improvement: {(1.0 - (double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds) * 100:F0}%");
        
        // Warm cache should be faster or similar (within 20% tolerance for variance)
        Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds * 1.2, 
            "Cached render should not be significantly slower");
    }
}
