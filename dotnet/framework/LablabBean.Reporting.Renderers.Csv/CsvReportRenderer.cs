using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace LablabBean.Reporting.Renderers.Csv;

/// <summary>
/// CSV report renderer using CsvHelper.
/// Supports BuildMetricsData, SessionStatisticsData, and PluginHealthData.
/// </summary>
public class CsvReportRenderer : IReportRenderer
{
    private readonly ILogger<CsvReportRenderer> _logger;

    public CsvReportRenderer(ILogger<CsvReportRenderer> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ReportFormat> SupportedFormats => new[] { ReportFormat.CSV };

    public async Task<ReportResult> RenderAsync(
        ReportRequest request,
        object data,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var dataType = data.GetType();
            _logger.LogInformation("Rendering CSV report for data type: {DataType}", dataType.Name);

            byte[] csvBytes;

            if (data is BuildMetricsData buildMetrics)
            {
                csvBytes = await RenderBuildMetricsAsync(buildMetrics, cancellationToken);
            }
            else if (data is SessionStatisticsData sessionStats)
            {
                csvBytes = await RenderSessionStatisticsAsync(sessionStats, cancellationToken);
            }
            else if (data is PluginHealthData pluginHealth)
            {
                csvBytes = await RenderPluginHealthAsync(pluginHealth, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"Data type {dataType.Name} is not supported for CSV rendering");
            }

            // Write to file if specified
            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                await File.WriteAllBytesAsync(request.OutputPath, csvBytes, cancellationToken);
                _logger.LogInformation("CSV report written to: {OutputPath}", request.OutputPath);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("CSV report rendered in {Duration}ms", duration.TotalMilliseconds);

            return new ReportResult
            {
                IsSuccess = true,
                OutputPath = request.OutputPath,
                FileSizeBytes = csvBytes.Length,
                Duration = duration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render CSV report");
            
            return new ReportResult
            {
                IsSuccess = false,
                Errors = { ex.Message }
            };
        }
    }

    private Task<byte[]> RenderBuildMetricsAsync(BuildMetricsData data, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        // Section 1: Build Summary
        csv.WriteField("Section");
        csv.WriteField("Metric");
        csv.WriteField("Value");
        csv.NextRecord();

        csv.WriteField("Build Summary");
        csv.WriteField("Build Duration");
        csv.WriteField(data.BuildDuration.ToString());
        csv.NextRecord();

        csv.WriteField("Build Summary");
        csv.WriteField("Start Time");
        csv.WriteField(data.BuildStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        csv.NextRecord();

        csv.WriteField("Build Summary");
        csv.WriteField("End Time");
        csv.WriteField(data.BuildEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        csv.NextRecord();

        // Section 2: Test Results
        csv.WriteField("Test Results");
        csv.WriteField("Total Tests");
        csv.WriteField(data.TotalTests);
        csv.NextRecord();

        csv.WriteField("Test Results");
        csv.WriteField("Passed Tests");
        csv.WriteField(data.PassedTests);
        csv.NextRecord();

        csv.WriteField("Test Results");
        csv.WriteField("Failed Tests");
        csv.WriteField(data.FailedTests);
        csv.NextRecord();

        csv.WriteField("Test Results");
        csv.WriteField("Skipped Tests");
        csv.WriteField(data.SkippedTests);
        csv.NextRecord();

        csv.WriteField("Test Results");
        csv.WriteField("Pass Rate");
        csv.WriteField($"{data.PassPercentage:F2}%");
        csv.NextRecord();

        // Section 3: Code Coverage
        csv.WriteField("Code Coverage");
        csv.WriteField("Line Coverage");
        csv.WriteField($"{data.LineCoveragePercentage:F2}%");
        csv.NextRecord();

        csv.WriteField("Code Coverage");
        csv.WriteField("Branch Coverage");
        csv.WriteField($"{data.BranchCoveragePercentage:F2}%");
        csv.NextRecord();

        // Section 4: Failed Tests Details
        if (data.FailedTests > 0 && data.FailedTestDetails != null)
        {
            csv.NextRecord();
            csv.WriteField("Failed Test Details");
            csv.WriteField("Test Name");
            csv.WriteField("Error Message");
            csv.NextRecord();

            foreach (var test in data.FailedTestDetails)
            {
                csv.WriteField("Failed Test");
                csv.WriteField(test.Name);
                csv.WriteField(test.ErrorMessage ?? "");
                csv.NextRecord();
            }
        }

        writer.Flush();
        return Task.FromResult(memoryStream.ToArray());
    }

    private Task<byte[]> RenderSessionStatisticsAsync(SessionStatisticsData data, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        // Section 1: Session Summary
        csv.WriteField("Section");
        csv.WriteField("Metric");
        csv.WriteField("Value");
        csv.NextRecord();

        csv.WriteField("Session Summary");
        csv.WriteField("Session ID");
        csv.WriteField(data.SessionId ?? "N/A");
        csv.NextRecord();

        csv.WriteField("Session Summary");
        csv.WriteField("Total Playtime");
        csv.WriteField(data.TotalPlaytime.ToString());
        csv.NextRecord();

        csv.WriteField("Session Summary");
        csv.WriteField("Start Time");
        csv.WriteField(data.SessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        csv.NextRecord();

        csv.WriteField("Session Summary");
        csv.WriteField("End Time");
        csv.WriteField(data.SessionEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        csv.NextRecord();

        // Section 2: Combat Statistics
        csv.WriteField("Combat Statistics");
        csv.WriteField("Total Kills");
        csv.WriteField(data.TotalKills);
        csv.NextRecord();

        csv.WriteField("Combat Statistics");
        csv.WriteField("Total Deaths");
        csv.WriteField(data.TotalDeaths);
        csv.NextRecord();

        csv.WriteField("Combat Statistics");
        csv.WriteField("K/D Ratio");
        csv.WriteField($"{data.KillDeathRatio:F2}");
        csv.NextRecord();

        csv.WriteField("Combat Statistics");
        csv.WriteField("Damage Dealt");
        csv.WriteField(data.TotalDamageDealt);
        csv.NextRecord();

        csv.WriteField("Combat Statistics");
        csv.WriteField("Damage Taken");
        csv.WriteField(data.TotalDamageTaken);
        csv.NextRecord();

        // Section 3: Progression Statistics
        csv.WriteField("Progression");
        csv.WriteField("Items Collected");
        csv.WriteField(data.ItemsCollected);
        csv.NextRecord();

        csv.WriteField("Progression");
        csv.WriteField("Levels Completed");
        csv.WriteField(data.LevelsCompleted);
        csv.NextRecord();

        csv.WriteField("Progression");
        csv.WriteField("Achievements Unlocked");
        csv.WriteField(data.AchievementsUnlocked);
        csv.NextRecord();

        // Section 4: Events
        if (data.KeyEvents != null && data.KeyEvents.Any())
        {
            csv.NextRecord();
            csv.WriteField("Event Details");
            csv.WriteField("Event Type");
            csv.WriteField("Timestamp");
            csv.NextRecord();

            foreach (var evt in data.KeyEvents.OrderBy(e => e.Timestamp))
            {
                csv.WriteField("Event");
                csv.WriteField(evt.EventType);
                csv.WriteField(evt.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.NextRecord();
            }
        }

        writer.Flush();
        return Task.FromResult(memoryStream.ToArray());
    }

    private Task<byte[]> RenderPluginHealthAsync(PluginHealthData data, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        // Header row
        csv.WriteField("Plugin Name");
        csv.WriteField("Version");
        csv.WriteField("State");
        csv.WriteField("Memory (MB)");
        csv.WriteField("Load Time");
        csv.WriteField("Health Reason");
        csv.NextRecord();

        // Plugin rows
        if (data.Plugins != null)
        {
            foreach (var plugin in data.Plugins)
            {
                csv.WriteField(plugin.Name);
                csv.WriteField(plugin.Version);
                csv.WriteField(plugin.State);
                csv.WriteField(plugin.MemoryUsageMB);
                csv.WriteField(plugin.LoadDuration.ToString());
                csv.WriteField(plugin.HealthStatusReason ?? "");
                csv.NextRecord();
            }
        }

        // Summary section
        csv.NextRecord();
        csv.WriteField("Summary");
        csv.WriteField("Total Plugins");
        csv.WriteField(data.TotalPlugins);
        csv.NextRecord();

        csv.WriteField("Summary");
        csv.WriteField("Running Plugins");
        csv.WriteField(data.RunningPlugins);
        csv.NextRecord();

        csv.WriteField("Summary");
        csv.WriteField("Degraded Plugins");
        csv.WriteField(data.DegradedPlugins);
        csv.NextRecord();

        csv.WriteField("Summary");
        csv.WriteField("Failed Plugins");
        csv.WriteField(data.FailedPlugins);
        csv.NextRecord();

        csv.WriteField("Summary");
        csv.WriteField("Total Memory (MB)");
        csv.WriteField(data.TotalMemoryUsageMB);
        csv.NextRecord();

        writer.Flush();
        return Task.FromResult(memoryStream.ToArray());
    }
}
