using FluentAssertions;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Analytics;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Reporting.Analytics.Tests;

public class PluginHealthProviderTests
{
    private readonly PluginHealthProvider _provider;
    private readonly string _testDataPath;

    public PluginHealthProviderTests()
    {
        _provider = new PluginHealthProvider(NullLogger<PluginHealthProvider>.Instance);
        _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData", "sample-plugins.json");
    }

    [Fact]
    public async Task GetReportDataAsync_WithValidJson_ShouldParseCorrectly()
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
        result.Should().BeOfType<PluginHealthData>();
        
        var pluginData = (PluginHealthData)result;
        pluginData.TotalPlugins.Should().Be(4);
        pluginData.RunningPlugins.Should().Be(3);
        pluginData.FailedPlugins.Should().Be(1);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldCalculateSuccessRate()
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
        var pluginData = (PluginHealthData)result;

        // Assert
        // 3 running / 4 total = 75%
        pluginData.SuccessRate.Should().BeGreaterThan(50m);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldCalculateTotalMemory()
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
        var pluginData = (PluginHealthData)result;

        // Assert
        // Should have some memory usage tracked
        pluginData.TotalMemoryUsageMB.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldTrackAverageLoadTime()
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
        var pluginData = (PluginHealthData)result;

        // Assert
        // Should have load time tracked
        pluginData.TotalLoadTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldIncludePluginDetails()
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
        var pluginData = (PluginHealthData)result;

        // Assert
        pluginData.Plugins.Should().HaveCount(4);
        pluginData.Plugins.Should().Contain(p => p.Name == "InventoryPlugin" && p.State == "Running");
        pluginData.Plugins.Should().Contain(p => p.Name == "AudioPlugin" && p.State == "Failed");
    }

    [Fact]
    public async Task GetReportDataAsync_ShouldTrackErrorCounts()
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
        var pluginData = (PluginHealthData)result;

        // Assert
        var audioPlugin = pluginData.Plugins.FirstOrDefault(p => p.Name == "AudioPlugin");
        audioPlugin.Should().NotBeNull();
        audioPlugin!.State.Should().Be("Failed");
        audioPlugin.ErrorMessage.Should().NotBeNullOrEmpty();
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
        var pluginData = (PluginHealthData)result;
        pluginData.TotalPlugins.Should().BeGreaterThan(0);
        pluginData.Plugins.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetReportDataAsync_WithInvalidPath_ShouldFallbackToSampleData()
    {
        // Arrange
        var request = new ReportRequest
        {
            Format = ReportFormat.HTML,
            OutputPath = "test-output.html",
            DataPath = "C:\\NonExistent\\plugins.json"
        };

        // Act
        var result = await _provider.GetReportDataAsync(request);

        // Assert
        result.Should().NotBeNull();
        var pluginData = (PluginHealthData)result;
        pluginData.TotalPlugins.Should().BeGreaterThan(0);
    }
}
