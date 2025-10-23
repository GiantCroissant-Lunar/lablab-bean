using System.Xml.Linq;
using LablabBean.Reporting.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Providers.Build.Parsers;

/// <summary>
/// Parses xUnit v2 XML test result files.
/// </summary>
public class XUnitXmlParser
{
    private readonly ILogger _logger;

    public XUnitXmlParser(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<ParsedTestResults> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Parsing xUnit XML: {FilePath}", filePath);

        var xml = await File.ReadAllTextAsync(filePath, cancellationToken);
        var doc = XDocument.Parse(xml);

        var tests = new List<TestResult>();
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;

        // xUnit v2 format: <assemblies><assembly><collection><test>
        var assemblies = doc.Descendants("assembly");

        foreach (var assembly in assemblies)
        {
            var assemblyName = assembly.Attribute("name")?.Value ?? "Unknown";
            var runDate = assembly.Attribute("run-date")?.Value;
            var runTime = assembly.Attribute("run-time")?.Value;

            if (runDate != null && runTime != null && DateTime.TryParse($"{runDate} {runTime}", out var parsed))
            {
                startTime = parsed;
            }

            var testElements = assembly.Descendants("test");

            foreach (var testElement in testElements)
            {
                var test = new TestResult
                {
                    Name = testElement.Attribute("name")?.Value ?? "Unknown",
                    ClassName = testElement.Attribute("type")?.Value ?? "Unknown",
                    Result = testElement.Attribute("result")?.Value ?? "Unknown",
                    Duration = TimeSpan.FromSeconds(
                        double.TryParse(testElement.Attribute("time")?.Value, out var time) ? time : 0)
                };

                // Get failure details if test failed
                if (test.Result == "Fail")
                {
                    var failure = testElement.Element("failure");
                    if (failure != null)
                    {
                        test.ErrorMessage = failure.Element("message")?.Value;
                        test.StackTrace = failure.Element("stack-trace")?.Value;
                    }
                }

                // Normalize result values
                test.Result = test.Result switch
                {
                    "Pass" => "Passed",
                    "Fail" => "Failed",
                    "Skip" => "Skipped",
                    _ => test.Result
                };

                tests.Add(test);
            }
        }

        // Calculate end time based on duration
        if (tests.Any())
        {
            var totalDuration = TimeSpan.FromSeconds(tests.Sum(t => t.Duration.TotalSeconds));
            endTime = startTime.Add(totalDuration);
        }

        _logger.LogDebug("Parsed {Count} tests from {FilePath}", tests.Count, filePath);

        return new ParsedTestResults
        {
            Tests = tests,
            StartTime = startTime,
            EndTime = endTime
        };
    }
}

public class ParsedTestResults
{
    public List<TestResult> Tests { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
