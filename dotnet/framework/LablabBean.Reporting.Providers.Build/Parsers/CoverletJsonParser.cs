using System.Text.Json;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Providers.Build.Parsers;

/// <summary>
/// Parses Coverlet JSON coverage output.
/// </summary>
public class CoverletJsonParser
{
    private readonly ILogger _logger;

    public CoverletJsonParser(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<CoverageSummary> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Parsing Coverlet JSON: {FilePath}", filePath);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var doc = JsonDocument.Parse(json);

        var totalLines = 0;
        var coveredLines = 0;
        var totalBranches = 0;
        var coveredBranches = 0;
        var fileCoverages = new List<FileCoverage>();

        // Coverlet JSON format: { "module": { "files": { "file.cs": { "Lines": {...}, "Branches": {...} } } } }
        foreach (var module in doc.RootElement.EnumerateObject())
        {
            if (!module.Value.TryGetProperty("Files", out var files))
                continue;

            foreach (var file in files.EnumerateObject())
            {
                var filePath2 = file.Name;
                var fileData = file.Value;

                var fileLines = 0;
                var fileCoveredLines = 0;
                var fileBranches = 0;
                var fileCoveredBranches = 0;

                // Count lines
                if (fileData.TryGetProperty("Lines", out var lines))
                {
                    foreach (var line in lines.EnumerateObject())
                    {
                        fileLines++;
                        totalLines++;
                        
                        if (line.Value.GetInt32() > 0)
                        {
                            fileCoveredLines++;
                            coveredLines++;
                        }
                    }
                }

                // Count branches
                if (fileData.TryGetProperty("Branches", out var branches))
                {
                    foreach (var branch in branches.EnumerateArray())
                    {
                        fileBranches++;
                        totalBranches++;
                        
                        if (branch.TryGetProperty("Hits", out var hits) && hits.GetInt32() > 0)
                        {
                            fileCoveredBranches++;
                            coveredBranches++;
                        }
                    }
                }

                var fileLineCoverage = fileLines > 0 ? (decimal)fileCoveredLines / fileLines * 100 : 0;
                var fileBranchCoverage = fileBranches > 0 ? (decimal)fileCoveredBranches / fileBranches * 100 : 0;

                fileCoverages.Add(new FileCoverage
                {
                    FilePath = filePath2,
                    CoveredLines = fileCoveredLines,
                    TotalLines = fileLines,
                    CoveragePercentage = fileLineCoverage
                });
            }
        }

        var lineCoverage = totalLines > 0 ? (decimal)coveredLines / totalLines * 100 : 0;
        var branchCoverage = totalBranches > 0 ? (decimal)coveredBranches / totalBranches * 100 : 0;

        // Find low coverage files (< 70%)
        var lowCoverageFiles = fileCoverages
            .Where(f => f.CoveragePercentage < 70)
            .OrderBy(f => f.CoveragePercentage)
            .Take(10)
            .ToList();

        _logger.LogDebug("Parsed coverage: {LineCoverage}% line, {BranchCoverage}% branch", 
            lineCoverage, branchCoverage);

        return new CoverageSummary
        {
            LineCoverage = Math.Round(lineCoverage, 1),
            BranchCoverage = Math.Round(branchCoverage, 1),
            LowCoverageFiles = lowCoverageFiles
        };
    }
}
