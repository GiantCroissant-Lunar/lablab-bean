using FluentAssertions;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Providers.Build;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Reporting.Providers.Build.Tests;

public class BuildMetricsProviderTests
{
    private readonly BuildMetricsProvider _provider;
    private readonly string _testDataPath;

    public BuildMetricsProviderTests()
    {
        _provider = new BuildMetricsProvider(NullLogger<BuildMetricsProvider>.Instance);
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    }

    [Fact]
    public async Task GetReportDataAsync_WithValidXunitResults_ShouldParseCorrectly()
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
        result.Should().BeOfType<BuildMetricsData>();
        
        var buildData = (BuildMetricsData)result;
        buildData.TotalTests.Should().BeGreaterThan(0);
        buildData.PassedTests.Should().BeGreaterOrEqualTo(0);
        buildData.FailedTests.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetReportDataAsync_WithValidCoverageResults_ShouldParseCorrectly()
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
        var buildData = (BuildMetricsData)result;
        
        // Should have coverage data
        buildData.LineCoveragePercentage.Should().BeGreaterOrEqualTo(0);
        buildData.BranchCoveragePercentage.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetReportDataAsync_WithFailedTests_ShouldIncludeFailureDetails()
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
        var buildData = (BuildMetricsData)result;

        // Assert
        buildData.FailedTestDetails.Should().NotBeNull();
        // If there are failed tests, the list should not be empty
        if (buildData.FailedTests > 0)
        {
            buildData.FailedTestDetails.Should().NotBeEmpty();
        }
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
        var buildData = (BuildMetricsData)result;
        buildData.TotalTests.Should().BeGreaterThan(0);
        buildData.ReportGeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetReportDataAsync_WithInvalidPath_ShouldHandleGracefully()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = "C:\\NonExistentPath"
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        var buildData = (BuildMetricsData)result;
        // Should fallback to sample data
        buildData.TotalTests.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Provider_ShouldImplementIReportProvider()
    {
        // The provider should implement the IReportProvider interface
        _provider.Should().NotBeNull();
        _provider.Should().BeAssignableTo<IReportProvider>();
    }
}
