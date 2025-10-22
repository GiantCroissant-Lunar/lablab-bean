using System.Text;
using FluentAssertions;
using LablabBean.Reporting.Contracts.Models;
using LablabBean.Plugins.Reporting.Csv;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Plugins.Reporting.Csv.Tests;

public class CsvReportRendererTests
{
    private readonly CsvReportRenderer _renderer;

    public CsvReportRendererTests()
    {
        _renderer = new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance);
    }

    [Fact]
    public void SupportedFormats_ShouldIncludeCSV()
    {
        // Assert
        _renderer.SupportedFormats.Should().Contain(ReportFormat.CSV);
    }

    [Fact]
    public async Task RenderAsync_BuildMetricsData_ShouldSucceed()
    {
        // Arrange
        var buildMetrics = new BuildMetricsData
        {
            TotalTests = 100,
            PassedTests = 95,
            FailedTests = 3,
            SkippedTests = 2,
            PassPercentage = 95.0m,
            LineCoveragePercentage = 85.5m,
            BranchCoveragePercentage = 78.2m,
            BuildDuration = TimeSpan.FromMinutes(5),
            BuildStartTime = DateTime.UtcNow.AddMinutes(-5),
            BuildEndTime = DateTime.UtcNow,
            BuildNumber = "123",
            Repository = "lablab-bean",
            Branch = "main",
            CommitHash = "abc123",
            FailedTestDetails = new List<TestResult>
            {
                new() { Name = "Test1", ErrorMessage = "Assertion failed" }
            }
        };

        var request = new ReportRequest
        {
            OutputPath = Path.GetTempFileName()
        };

        try
        {
            // Act
            var result = await _renderer.RenderAsync(request, buildMetrics);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.FileSizeBytes.Should().BeGreaterThan(0);
            File.Exists(request.OutputPath).Should().BeTrue();

            var csvContent = await File.ReadAllTextAsync(request.OutputPath);
            csvContent.Should().Contain("Build Summary");
            csvContent.Should().Contain("Test Results");
            csvContent.Should().Contain("Code Coverage");
            csvContent.Should().Contain("95");
            csvContent.Should().Contain("85.5");
        }
        finally
        {
            if (File.Exists(request.OutputPath))
                File.Delete(request.OutputPath);
        }
    }

    [Fact]
    public async Task RenderAsync_SessionStatisticsData_ShouldSucceed()
    {
        // Arrange
        var sessionStats = new SessionStatisticsData
        {
            SessionId = "session-123",
            SessionStartTime = DateTime.UtcNow.AddHours(-2),
            SessionEndTime = DateTime.UtcNow,
            TotalPlaytime = TimeSpan.FromHours(2),
            TotalKills = 50,
            TotalDeaths = 10,
            KillDeathRatio = 5.0m,
            TotalDamageDealt = 5000,
            TotalDamageTaken = 1200,
            ItemsCollected = 25,
            LevelsCompleted = 3,
            AchievementsUnlocked = 5,
            KeyEvents = new List<SessionEvent>
            {
                new() { EventType = "BossDefeated", Timestamp = DateTime.UtcNow.AddHours(-1) }
            }
        };

        var request = new ReportRequest
        {
            OutputPath = Path.GetTempFileName()
        };

        try
        {
            // Act
            var result = await _renderer.RenderAsync(request, sessionStats);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.FileSizeBytes.Should().BeGreaterThan(0);

            var csvContent = await File.ReadAllTextAsync(request.OutputPath);
            csvContent.Should().Contain("Session Summary");
            csvContent.Should().Contain("Combat Statistics");
            csvContent.Should().Contain("Progression");
            csvContent.Should().Contain("50");
            csvContent.Should().Contain("BossDefeated");
        }
        finally
        {
            if (File.Exists(request.OutputPath))
                File.Delete(request.OutputPath);
        }
    }

    [Fact]
    public async Task RenderAsync_PluginHealthData_ShouldSucceed()
    {
        // Arrange
        var pluginHealth = new PluginHealthData
        {
            TotalPlugins = 5,
            RunningPlugins = 4,
            FailedPlugins = 1,
            DegradedPlugins = 0,
            TotalMemoryUsageMB = 250,
            Plugins = new List<PluginStatus>
            {
                new()
                {
                    Name = "AudioPlugin",
                    Version = "1.0.0",
                    State = "Running",
                    MemoryUsageMB = 50,
                    LoadDuration = TimeSpan.FromSeconds(2)
                },
                new()
                {
                    Name = "GraphicsPlugin",
                    Version = "1.0.0",
                    State = "Failed",
                    MemoryUsageMB = 0,
                    LoadDuration = TimeSpan.Zero,
                    HealthStatusReason = "Failed to initialize"
                }
            }
        };

        var request = new ReportRequest
        {
            OutputPath = Path.GetTempFileName()
        };

        try
        {
            // Act
            var result = await _renderer.RenderAsync(request, pluginHealth);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.FileSizeBytes.Should().BeGreaterThan(0);

            var csvContent = await File.ReadAllTextAsync(request.OutputPath);
            csvContent.Should().Contain("AudioPlugin");
            csvContent.Should().Contain("GraphicsPlugin");
            csvContent.Should().Contain("Summary");
            csvContent.Should().Contain("Failed to initialize");
        }
        finally
        {
            if (File.Exists(request.OutputPath))
                File.Delete(request.OutputPath);
        }
    }

    [Fact]
    public async Task RenderAsync_UnsupportedDataType_ShouldReturnFailure()
    {
        // Arrange
        var unsupportedData = new { Foo = "bar" };
        var request = new ReportRequest { };

        // Act
        var result = await _renderer.RenderAsync(request, unsupportedData);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RenderAsync_WithoutOutputPath_ShouldReturnData()
    {
        // Arrange
        var buildMetrics = new BuildMetricsData
        {
            TotalTests = 10,
            PassedTests = 10,
            FailedTests = 0,
            SkippedTests = 0,
            PassPercentage = 100m,
            LineCoveragePercentage = 90m,
            BranchCoveragePercentage = 85m,
            BuildDuration = TimeSpan.FromMinutes(2),
            BuildStartTime = DateTime.UtcNow.AddMinutes(-2),
            BuildEndTime = DateTime.UtcNow,
            BuildNumber = "456"
        };

        var request = new ReportRequest
        {
            // No OutputPath
        };

        // Act
        var result = await _renderer.RenderAsync(request, buildMetrics);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.FileSizeBytes.Should().BeGreaterThan(0);
    }
}
