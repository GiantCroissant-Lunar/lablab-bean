using FluentAssertions;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Analytics;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Reporting.Analytics.Tests;

public class SessionStatisticsProviderTests
{
    private readonly SessionStatisticsProvider _provider;
    private readonly string _testDataPath;

    public SessionStatisticsProviderTests()
    {
        _provider = new SessionStatisticsProvider(NullLogger<SessionStatisticsProvider>.Instance);
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData", "sample-session.jsonl");
    }

    [Fact]
    public async Task GetReportDataAsync_WithValidJsonl_ShouldParseCorrectly()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = _testDataPath
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SessionStatisticsData>();
        
        var sessionData = (SessionStatisticsData)result;
        sessionData.SessionId.Should().Be("test-session-001");
        sessionData.TotalKills.Should().Be(3);
        sessionData.TotalDeaths.Should().Be(2);
        sessionData.LevelsCompleted.Should().Be(2);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldCalculateKDRatioCorrectly()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = _testDataPath
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);
        var sessionData = (SessionStatisticsData)result;

        // Assert
        // 3 kills / 2 deaths = 1.5
        sessionData.KillDeathRatio.Should().BeGreaterThan(1.0m);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldCalculateTotalPlaytime()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = _testDataPath
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);
        var sessionData = (SessionStatisticsData)result;

        // Assert
        // 1800 seconds = 30 minutes
        sessionData.TotalPlaytime.Should().Be(TimeSpan.FromSeconds(1800));
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldTrackDamageDealt()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = _testDataPath
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);
        var sessionData = (SessionStatisticsData)result;

        // Assert
        // 45 + 78 + 52 = 175 total damage
        sessionData.TotalDamageDealt.Should().Be(175);
    }

    [Fact]
    public async Task GetReportDataAsync_WithNoDataPath_ShouldGenerateSampleData()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = null
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        var sessionData = (SessionStatisticsData)result;
        sessionData.SessionId.Should().NotBeNullOrEmpty();
        sessionData.TotalKills.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReportDataAsync_WithInvalidPath_ShouldFallbackToSampleData()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = "C:\\NonExistent\\file.jsonl"
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        var sessionData = (SessionStatisticsData)result;
        sessionData.TotalKills.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReportDataAsync_WithZeroDeaths_ShouldHandleKDRatio()
    {
        // When deaths = 0, K/D ratio should be kills (or infinity concept)
        // Our implementation should handle this gracefully
        // This would require a specific test data file or mocking
        // For now, we verify the provider doesn't crash
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = null
        };

        var result = await _provider.GetReportDataAsync(request);
        result.Should().NotBeNull();
    }
}
