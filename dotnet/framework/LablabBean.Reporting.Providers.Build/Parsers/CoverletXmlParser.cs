using System.Xml.Linq;
using LablabBean.Reporting.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Providers.Build.Parsers;

/// <summary>
/// Parses Coverlet/OpenCover XML coverage output.
/// </summary>
public class CoverletXmlParser
{
    private readonly ILogger _logger;

    public CoverletXmlParser(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<CoverageSummary> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Parsing coverage XML: {FilePath}", filePath);

        var xml = await File.ReadAllTextAsync(filePath, cancellationToken);
        var doc = XDocument.Parse(xml);

        var totalLines = 0;
        var coveredLines = 0;
        var totalBranches = 0;
        var coveredBranches = 0;
        var fileCoverages = new List<FileCoverage>();

        // OpenCover/Coverlet XML format
        var modules = doc.Descendants("Module");
        
        foreach (var module in modules)
        {
            var files = module.Descendants("File");
            
            foreach (var file in files)
            {
                var fullPath = file.Attribute("fullPath")?.Value ?? "Unknown";
                var fileLines = 0;
                var fileCoveredLines = 0;

                // Get all sequence points for this file
                var fileId = file.Attribute("uid")?.Value;
                if (fileId != null)
                {
                    var sequencePoints = module.Descendants("SequencePoint")
                        .Where(sp => sp.Attribute("fileid")?.Value == fileId);

                    foreach (var sp in sequencePoints)
                    {
                        var visitCount = int.Parse(sp.Attribute("vc")?.Value ?? "0");
                        fileLines++;
                        totalLines++;
                        
                        if (visitCount > 0)
                        {
                            fileCoveredLines++;
                            coveredLines++;
                        }
                    }
                }

                if (fileLines > 0)
                {
                    var fileLineCoverage = (decimal)fileCoveredLines / fileLines * 100;
                    
                    fileCoverages.Add(new FileCoverage
                    {
                        FilePath = fullPath,
                        CoveredLines = fileCoveredLines,
                        TotalLines = fileLines,
                        CoveragePercentage = fileLineCoverage
                    });
                }
            }

            // Parse branch coverage from BranchPoint elements
            var branchPoints = module.Descendants("BranchPoint");
            foreach (var bp in branchPoints)
            {
                totalBranches++;
                var visitCount = int.Parse(bp.Attribute("vc")?.Value ?? "0");
                if (visitCount > 0)
                    coveredBranches++;
            }
        }

        var lineCoverage = totalLines > 0 ? (decimal)coveredLines / totalLines * 100 : 0;
        var branchCoverage = totalBranches > 0 ? (decimal)coveredBranches / totalBranches * 100 : 0;

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
