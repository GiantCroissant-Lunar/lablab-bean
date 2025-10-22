using System.CommandLine;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Renderers.Html;
using LablabBean.Reporting.Renderers.Csv;
using LablabBean.Reporting.Providers.Build;
using LablabBean.Reporting.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LablabBean.Console.Commands;

public static class ReportCommand
{
    public static Command Create()
    {
        var reportCommand = new Command("report", "Generate reports from build/session/plugin data");

        reportCommand.AddCommand(CreateBuildCommand());
        reportCommand.AddCommand(CreateSessionCommand());
        reportCommand.AddCommand(CreatePluginCommand());

        return reportCommand;
    }

    private static Command CreateBuildCommand()
    {
        var command = new Command("build", "Generate build metrics report");

        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            getDefaultValue: () => "html",
            description: "Output format (html, csv)");

        var outputOption = new Option<FileInfo>(
            aliases: new[] { "--output", "-o" },
            description: "Output file path");
        outputOption.IsRequired = true;

        var dataOption = new Option<DirectoryInfo?>(
            aliases: new[] { "--data", "-d" },
            description: "Data directory (test results, coverage files)");

        command.AddOption(formatOption);
        command.AddOption(outputOption);
        command.AddOption(dataOption);

        command.SetHandler(async (format, output, data) =>
        {
            await HandleBuildReportAsync(format, output, data);
        }, formatOption, outputOption, dataOption);

        return command;
    }

    private static Command CreateSessionCommand()
    {
        var command = new Command("session", "Generate game session statistics report");

        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            getDefaultValue: () => "html",
            description: "Output format (html, csv)");

        var outputOption = new Option<FileInfo>(
            aliases: new[] { "--output", "-o" },
            description: "Output file path");
        outputOption.IsRequired = true;

        var dataOption = new Option<FileInfo?>(
            aliases: new[] { "--data", "-d" },
            description: "Session data file (JSON)");

        command.AddOption(formatOption);
        command.AddOption(outputOption);
        command.AddOption(dataOption);

        command.SetHandler(async (format, output, data) =>
        {
            await HandleSessionReportAsync(format, output, data);
        }, formatOption, outputOption, dataOption);

        return command;
    }

    private static Command CreatePluginCommand()
    {
        var command = new Command("plugin", "Generate plugin health report");

        var formatOption = new Option<string>(
            aliases: new[] { "--format", "-f" },
            getDefaultValue: () => "html",
            description: "Output format (html, csv)");

        var outputOption = new Option<FileInfo>(
            aliases: new[] { "--output", "-o" },
            description: "Output file path");
        outputOption.IsRequired = true;

        var dataOption = new Option<FileInfo?>(
            aliases: new[] { "--data", "-d" },
            description: "Plugin metrics file (JSON)");

        command.AddOption(formatOption);
        command.AddOption(outputOption);
        command.AddOption(dataOption);

        command.SetHandler(async (format, output, data) =>
        {
            await HandlePluginReportAsync(format, output, data);
        }, formatOption, outputOption, dataOption);

        return command;
    }

    private static async Task<int> HandleBuildReportAsync(string format, FileInfo output, DirectoryInfo? dataDir)
    {
        try
        {
            System.Console.WriteLine($"Generating build metrics report ({format})...");

            // Use real data provider
            var provider = new BuildMetricsProvider(NullLogger<BuildMetricsProvider>.Instance);
            
            var request = new ReportRequest
            {
                Format = format.ToLowerInvariant() == "html" ? ReportFormat.HTML : ReportFormat.CSV,
                OutputPath = output.FullName,
                DataPath = dataDir?.FullName
            };
            
            var data = await provider.GetReportDataAsync(request);
            
            if (data is not BuildMetricsData buildData)
            {
                throw new InvalidOperationException("Provider returned unexpected data type");
            }

            IReportRenderer renderer = format.ToLowerInvariant() switch
            {
                "html" => new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance),
                "csv" => new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var result = await renderer.RenderAsync(request, buildData, CancellationToken.None);

            if (result.IsSuccess)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✅ Report generated: {result.OutputPath}");
                System.Console.ResetColor();
                System.Console.WriteLine($"   Tests: {buildData.TotalTests} ({buildData.PassedTests} passed, {buildData.FailedTests} failed)");
                System.Console.WriteLine($"   Coverage: {buildData.LineCoveragePercentage:F1}% line, {buildData.BranchCoveragePercentage:F1}% branch");
                if (result.FileSizeBytes > 0)
                {
                    System.Console.WriteLine($"   File size: {result.FileSizeBytes:N0} bytes");
                }
                return 0;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Report generation failed:");
                foreach (var error in result.Errors)
                {
                    System.Console.WriteLine($"   {error}");
                }
                System.Console.ResetColor();
                return 1;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.ResetColor();
            return 1;
        }
    }

    private static async Task<int> HandleSessionReportAsync(string format, FileInfo output, FileInfo? dataFile)
    {
        try
        {
            System.Console.WriteLine($"Generating session statistics report ({format})...");

            // Use real data provider
            var provider = new SessionStatisticsProvider(NullLogger<SessionStatisticsProvider>.Instance);
            
            var request = new ReportRequest
            {
                Format = format.ToLowerInvariant() == "html" ? ReportFormat.HTML : ReportFormat.CSV,
                OutputPath = output.FullName,
                DataPath = dataFile?.FullName
            };
            
            var data = await provider.GetReportDataAsync(request);
            
            if (data is not SessionStatisticsData sessionData)
            {
                throw new InvalidOperationException("Provider returned unexpected data type");
            }

            IReportRenderer renderer = format.ToLowerInvariant() switch
            {
                "html" => new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance),
                "csv" => new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var result = await renderer.RenderAsync(request, sessionData, CancellationToken.None);

            if (result.IsSuccess)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✅ Report generated: {result.OutputPath}");
                System.Console.ResetColor();
                System.Console.WriteLine($"   Session: {sessionData.SessionId}");
                System.Console.WriteLine($"   Playtime: {sessionData.TotalPlaytime}");
                System.Console.WriteLine($"   K/D Ratio: {sessionData.KillDeathRatio:F2} ({sessionData.TotalKills} kills, {sessionData.TotalDeaths} deaths)");
                System.Console.WriteLine($"   Levels: {sessionData.LevelsCompleted}");
                if (result.FileSizeBytes > 0)
                {
                    System.Console.WriteLine($"   File size: {result.FileSizeBytes:N0} bytes");
                }
                return 0;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Report generation failed:");
                foreach (var error in result.Errors)
                {
                    System.Console.WriteLine($"   {error}");
                }
                System.Console.ResetColor();
                return 1;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.ResetColor();
            return 1;
        }
    }

    private static async Task<int> HandlePluginReportAsync(string format, FileInfo output, FileInfo? dataFile)
    {
        try
        {
            System.Console.WriteLine($"Generating plugin health report ({format})...");

            // Use real data provider
            var provider = new PluginHealthProvider(NullLogger<PluginHealthProvider>.Instance);
            
            var request = new ReportRequest
            {
                Format = format.ToLowerInvariant() == "html" ? ReportFormat.HTML : ReportFormat.CSV,
                OutputPath = output.FullName,
                DataPath = dataFile?.FullName
            };
            
            var data = await provider.GetReportDataAsync(request);
            
            if (data is not PluginHealthData pluginData)
            {
                throw new InvalidOperationException("Provider returned unexpected data type");
            }

            IReportRenderer renderer = format.ToLowerInvariant() switch
            {
                "html" => new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance),
                "csv" => new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var result = await renderer.RenderAsync(request, pluginData, CancellationToken.None);

            if (result.IsSuccess)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✅ Report generated: {result.OutputPath}");
                System.Console.ResetColor();
                System.Console.WriteLine($"   Plugins: {pluginData.TotalPlugins} total ({pluginData.RunningPlugins} running, {pluginData.FailedPlugins} failed)");
                System.Console.WriteLine($"   Success Rate: {pluginData.SuccessRate:F1}%");
                System.Console.WriteLine($"   Memory: {pluginData.TotalMemoryUsageMB} MB");
                if (result.FileSizeBytes > 0)
                {
                    System.Console.WriteLine($"   File size: {result.FileSizeBytes:N0} bytes");
                }
                return 0;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Report generation failed:");
                foreach (var error in result.Errors)
                {
                    System.Console.WriteLine($"   {error}");
                }
                System.Console.ResetColor();
                return 1;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.ResetColor();
            return 1;
        }
    }
}
