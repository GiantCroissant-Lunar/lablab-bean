using FluentAssertions;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Reporting.Renderers.Html.Tests;

public class HtmlReportRendererTests
{
    private readonly HtmlReportRenderer _renderer;

    public HtmlReportRendererTests()
    {
        _renderer = new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance);
    }

    [Fact]
    public void SupportedFormats_ShouldContainHtml()
    {
        // Assert
        _renderer.SupportedFormats.Should().Contain(ReportFormat.HTML);
    }

    [Fact]
    public async Task RenderAsync_BuildMetricsData_ShouldGenerateHtmlReport()
    {
        // Arrange
        var data = new BuildMetricsData
        {
            BuildNumber = "123",
            Repository = "test-repo",
            Branch = "main",
            CommitHash = "abc123def456",
            BuildDuration = TimeSpan.FromMinutes(5),
            BuildStartTime = DateTime.UtcNow.AddMinutes(-5),
            BuildEndTime = DateTime.UtcNow,
            TotalTests = 100,
            PassedTests = 95,
            FailedTests = 5,
            SkippedTests = 0,
            PassPercentage = 95.0m,
            LineCoveragePercentage = 85.5m,
            BranchCoveragePercentage = 80.0m,
            FailedTestDetails = new List<TestResult>(),
            LowCoverageFiles = new List<FileCoverage>()
        };

        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = Path.Combine(Path.GetTempPath(), "test-build-metrics.html")
        };

        // Act
        var result = await _renderer.RenderAsync(request, data);

        // Assert - Print errors for debugging
        if (!result.IsSuccess)
        {
            var errors = string.Join(", ", result.Errors);
            Assert.Fail($"Rendering failed with errors: {errors}");
        }
        
        result.IsSuccess.Should().BeTrue();
        result.OutputPath.Should().NotBeNullOrEmpty();
        result.FileSizeBytes.Should().BeGreaterThan(0);
        
        // Verify file was created
        File.Exists(result.OutputPath).Should().BeTrue();
        
        // Verify HTML content
        var htmlContent = await File.ReadAllTextAsync(result.OutputPath!);
        htmlContent.Should().Contain("<!DOCTYPE html>");
        htmlContent.Should().Contain("Build Metrics");
        htmlContent.Should().Contain("123"); // Build number
        htmlContent.Should().Contain("95"); // Passed tests
        
        // Cleanup
        File.Delete(result.OutputPath!);
    }

    [Fact]
    public async Task RenderAsync_SessionStatisticsData_ShouldGenerateHtmlReport()
    {
        // Arrange
        var data = new SessionStatisticsData
        {
            SessionId = "session-456",
            SessionStartTime = DateTime.UtcNow.AddHours(-2),
            SessionEndTime = DateTime.UtcNow,
            TotalPlaytime = TimeSpan.FromHours(2),
            TotalKills = 150,
            TotalDeaths = 10,
            KillDeathRatio = 15.0m,
            TotalDamageDealt = 50000,
            TotalDamageTaken = 10000,
            ItemsCollected = 45,
            LevelsCompleted = 5,
            AchievementsUnlocked = 3,
            AverageFrameRate = 60,
            KeyEvents = new List<SessionEvent>()
        };

        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = Path.Combine(Path.GetTempPath(), "test-session-stats.html")
        };

        // Act
        var result = await _renderer.RenderAsync(request, data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OutputPath.Should().NotBeNullOrEmpty();
        
        // Verify file was created
        File.Exists(result.OutputPath).Should().BeTrue();
        
        // Verify HTML content
        var htmlContent = await File.ReadAllTextAsync(result.OutputPath!);
        htmlContent.Should().Contain("<!DOCTYPE html>");
        htmlContent.Should().Contain("Session Statistics");
        htmlContent.Should().Contain("session-456");
        htmlContent.Should().Contain("150"); // Total kills
        
        // Cleanup
        File.Delete(result.OutputPath!);
    }

    [Fact]
    public async Task RenderAsync_PluginHealthData_ShouldGenerateHtmlReport()
    {
        // Arrange
        var data = new PluginHealthData
        {
            TotalPlugins = 5,
            RunningPlugins = 3,
            FailedPlugins = 1,
            DegradedPlugins = 1,
            SuccessRate = 60.0m,
            Plugins = new List<PluginStatus>
            {
                new()
                {
                    Name = "TestPlugin",
                    Version = "1.0.0",
                    State = "Running",
                    LoadDuration = TimeSpan.FromMilliseconds(500),
                    MemoryUsageMB = 64
                }
            }
        };

        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = Path.Combine(Path.GetTempPath(), "test-plugin-health.html")
        };

        // Act
        var result = await _renderer.RenderAsync(request, data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OutputPath.Should().NotBeNullOrEmpty();
        
        // Verify file was created
        File.Exists(result.OutputPath).Should().BeTrue();
        
        // Verify HTML content
        var htmlContent = await File.ReadAllTextAsync(result.OutputPath!);
        htmlContent.Should().Contain("<!DOCTYPE html>");
        htmlContent.Should().Contain("Plugin Health");
        htmlContent.Should().Contain("TestPlugin");
        htmlContent.Should().Contain("Running");
        
        // Cleanup
        File.Delete(result.OutputPath!);
    }

    [Fact]
    public async Task RenderAsync_WithoutOutputPath_ShouldStillSucceed()
    {
        // Arrange
        var data = new BuildMetricsData
        {
            BuildNumber = "456",
            TotalTests = 50,
            PassedTests = 50,
            PassPercentage = 100.0m,
            FailedTestDetails = new List<TestResult>(),
            LowCoverageFiles = new List<FileCoverage>()
        };

        var request = new ReportRequest
        {
            Format = ReportFormat.HTML
            // No OutputPath specified
        };

        // Act
        var result = await _renderer.RenderAsync(request, data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.FileSizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_InvalidDataType_ShouldFail()
    {
        // Arrange
        var invalidData = new { Test = "invalid" };
        var request = new ReportRequest { Format = ReportFormat.HTML };

        // Act
        var result = await _renderer.RenderAsync(request, invalidData);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RenderAsync_MultipleCallsSameDataType_ShouldUseCachedTemplate()
    {
        // Arrange
        var data1 = new BuildMetricsData
        {
            BuildNumber = "1",
            TotalTests = 10,
            PassedTests = 10,
            PassPercentage = 100.0m,
            FailedTestDetails = new List<TestResult>(),
            LowCoverageFiles = new List<FileCoverage>()
        };
        
        var data2 = new BuildMetricsData
        {
            BuildNumber = "2",
            TotalTests = 20,
            PassedTests = 18,
            PassPercentage = 90.0m,
            FailedTestDetails = new List<TestResult>(),
            LowCoverageFiles = new List<FileCoverage>()
        };

        var request1 = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = Path.Combine(Path.GetTempPath(), "test1.html")
        };
        
        var request2 = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = Path.Combine(Path.GetTempPath(), "test2.html")
        };

        // Act
        var result1 = await _renderer.RenderAsync(request1, data1);
        var result2 = await _renderer.RenderAsync(request2, data2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result2.Duration.Should().BeLessThanOrEqualTo(result1.Duration); // Second should be same or faster due to cached template

        // Cleanup
        if (File.Exists(result1.OutputPath!)) File.Delete(result1.OutputPath!);
        if (File.Exists(result2.OutputPath!)) File.Delete(result2.OutputPath!);
    }
}
