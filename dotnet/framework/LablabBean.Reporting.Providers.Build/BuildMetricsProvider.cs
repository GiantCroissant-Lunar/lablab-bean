using System.Xml.Linq;
using LablabBean.Reporting.Abstractions.Attributes;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Providers.Build.Parsers;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Providers.Build;

/// <summary>
/// Provides build metrics data by parsing xUnit XML and Coverlet coverage files.
/// </summary>
[ReportProvider("build-metrics", "Build")]
public class BuildMetricsProvider : IReportProvider
{
    private readonly ILogger<BuildMetricsProvider> _logger;

    public BuildMetricsProvider(ILogger<BuildMetricsProvider> logger)
    {
        _logger = logger;
    }

    public ReportMetadata GetMetadata()
    {
        return new ReportMetadata
        {
            Name = "Build Metrics",
            Description = "Provides test results and code coverage metrics from build artifacts",
            Category = "Build",
            SupportedFormats = new[] { ReportFormat.HTML, ReportFormat.CSV },
            DataSourcePattern = "*.xml;coverage*.json"
        };
    }

    public async Task<object> GetReportDataAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading build metrics from: {SourcePath}", request.DataPath ?? "current directory");

        var dataPath = request.DataPath ?? Directory.GetCurrentDirectory();
        
        if (!Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException($"Data path not found: {dataPath}");
        }

        // Parse test results
        var testResults = await ParseTestResultsAsync(dataPath, cancellationToken);
        
        // Parse code coverage
        var coverage = await ParseCoverageAsync(dataPath, cancellationToken);

        // Build the metrics data
        var data = new BuildMetricsData
        {
            BuildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "local",
            Repository = Path.GetFileName(dataPath),
            Branch = Environment.GetEnvironmentVariable("BRANCH_NAME") ?? "unknown",
            CommitHash = Environment.GetEnvironmentVariable("COMMIT_SHA") ?? "unknown",
            BuildStartTime = testResults.StartTime,
            BuildEndTime = testResults.EndTime,
            BuildDuration = testResults.Duration,
            TotalTests = testResults.Total,
            PassedTests = testResults.Passed,
            FailedTests = testResults.Failed,
            SkippedTests = testResults.Skipped,
            PassPercentage = testResults.Total > 0 
                ? (decimal)testResults.Passed / testResults.Total * 100 
                : 0,
            LineCoveragePercentage = coverage.LineCoverage,
            BranchCoveragePercentage = coverage.BranchCoverage,
            FailedTestDetails = testResults.FailedTests,
            LowCoverageFiles = coverage.LowCoverageFiles
        };

        _logger.LogInformation(
            "Build metrics loaded: {Total} tests ({Passed} passed, {Failed} failed), {Coverage}% line coverage",
            data.TotalTests, data.PassedTests, data.FailedTests, data.LineCoveragePercentage);

        return data;
    }

    private async Task<TestResultsSummary> ParseTestResultsAsync(string dataPath, CancellationToken cancellationToken)
    {
        var testResultFiles = Directory.GetFiles(dataPath, "*results*.xml", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(dataPath, "*test*.xml", SearchOption.AllDirectories))
            .Distinct()
            .ToArray();

        if (testResultFiles.Length == 0)
        {
            _logger.LogWarning("No test result files found in {DataPath}", dataPath);
            return new TestResultsSummary();
        }

        _logger.LogInformation("Found {Count} test result files", testResultFiles.Length);

        var allTests = new List<TestResult>();
        var earliestStart = DateTime.MaxValue;
        var latestEnd = DateTime.MinValue;

        foreach (var file in testResultFiles)
        {
            try
            {
                var parser = new XUnitXmlParser(_logger);
                var results = await parser.ParseAsync(file, cancellationToken);
                
                allTests.AddRange(results.Tests);
                
                if (results.StartTime < earliestStart)
                    earliestStart = results.StartTime;
                if (results.EndTime > latestEnd)
                    latestEnd = results.EndTime;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse test result file: {File}", file);
            }
        }

        var passed = allTests.Count(t => t.Result == "Passed");
        var failed = allTests.Count(t => t.Result == "Failed");
        var skipped = allTests.Count(t => t.Result == "Skipped");

        return new TestResultsSummary
        {
            Total = allTests.Count,
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            StartTime = earliestStart != DateTime.MaxValue ? earliestStart : DateTime.UtcNow,
            EndTime = latestEnd != DateTime.MinValue ? latestEnd : DateTime.UtcNow,
            Duration = latestEnd != DateTime.MinValue && earliestStart != DateTime.MaxValue 
                ? latestEnd - earliestStart 
                : TimeSpan.Zero,
            FailedTests = allTests.Where(t => t.Result == "Failed").ToList()
        };
    }

    private async Task<CoverageSummary> ParseCoverageAsync(string dataPath, CancellationToken cancellationToken)
    {
        var coverageFiles = Directory.GetFiles(dataPath, "coverage*.json", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(dataPath, "coverage*.xml", SearchOption.AllDirectories))
            .ToArray();

        if (coverageFiles.Length == 0)
        {
            _logger.LogWarning("No coverage files found in {DataPath}", dataPath);
            return new CoverageSummary();
        }

        _logger.LogInformation("Found {Count} coverage files", coverageFiles.Length);

        // For now, just parse the first coverage file
        var coverageFile = coverageFiles[0];
        
        try
        {
            if (coverageFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                var parser = new CoverletJsonParser(_logger);
                return await parser.ParseAsync(coverageFile, cancellationToken);
            }
            else if (coverageFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var parser = new CoverletXmlParser(_logger);
                return await parser.ParseAsync(coverageFile, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse coverage file: {File}", coverageFile);
        }

        return new CoverageSummary();
    }
}

public class TestResultsSummary
{
    public int Total { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<TestResult> FailedTests { get; set; } = new();
}

public class CoverageSummary
{
    public decimal LineCoverage { get; set; }
    public decimal BranchCoverage { get; set; }
    public List<FileCoverage> LowCoverageFiles { get; set; } = new();
}
