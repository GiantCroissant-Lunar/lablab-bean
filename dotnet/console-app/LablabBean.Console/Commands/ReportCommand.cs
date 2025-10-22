using System.CommandLine;
using LablabBean.Reporting.Abstractions.Contracts;
using LablabBean.Reporting.Abstractions.Models;
using LablabBean.Reporting.Renderers.Html;
using LablabBean.Reporting.Renderers.Csv;
using LablabBean.Reporting.Providers.Build;
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

            // Create sample data
            var sampleData = new SessionStatisticsData
            {
                SessionId = Guid.NewGuid().ToString(),
                SessionStartTime = DateTime.UtcNow.AddHours(-2),
                SessionEndTime = DateTime.UtcNow,
                TotalPlaytime = TimeSpan.FromHours(2),
                TotalKills = 47,
                TotalDeaths = 3,
                TotalDamageDealt = 15420,
                TotalDamageTaken = 8350,
                ItemsCollected = 23,
                LevelsCompleted = 5,
                AchievementsUnlocked = 3,
                AverageFrameRate = 60,
                TotalLoadTime = TimeSpan.FromSeconds(12)
            };

            IReportRenderer renderer = format.ToLowerInvariant() switch
            {
                "html" => new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance),
                "csv" => new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var request = new ReportRequest
            {
                Format = format.ToLowerInvariant() == "html" ? ReportFormat.HTML : ReportFormat.CSV,
                OutputPath = output.FullName,
                DataPath = dataFile?.FullName
            };

            var result = await renderer.RenderAsync(request, sampleData, CancellationToken.None);

            if (result.IsSuccess)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✅ Report generated: {result.OutputPath}");
                System.Console.ResetColor();
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

            // Create sample data
            var sampleData = new PluginHealthData
            {
                TotalPlugins = 5,
                RunningPlugins = 4,
                FailedPlugins = 0,
                DegradedPlugins = 1,
                SuccessRate = 80.0m,
                TotalMemoryUsageMB = 245,
                TotalLoadTime = TimeSpan.FromMilliseconds(627),
                Plugins = new List<PluginStatus>
                {
                    new PluginStatus
                    {
                        Name = "Analytics Plugin",
                        Version = "1.0.0",
                        State = "Running",
                        MemoryUsageMB = 45,
                        LoadDuration = TimeSpan.FromMilliseconds(150)
                    },
                    new PluginStatus
                    {
                        Name = "Logging Plugin",
                        Version = "1.2.0",
                        State = "Running",
                        HealthStatusReason = "High memory usage",
                        MemoryUsageMB = 120,
                        LoadDuration = TimeSpan.FromMilliseconds(95)
                    },
                    new PluginStatus
                    {
                        Name = "Reporting Plugin",
                        Version = "1.0.0",
                        State = "Running",
                        MemoryUsageMB = 35,
                        LoadDuration = TimeSpan.FromMilliseconds(180)
                    },
                    new PluginStatus
                    {
                        Name = "Game Mechanics Plugin",
                        Version = "2.1.3",
                        State = "Running",
                        MemoryUsageMB = 25,
                        LoadDuration = TimeSpan.FromMilliseconds(102)
                    },
                    new PluginStatus
                    {
                        Name = "UI Theme Plugin",
                        Version = "1.5.0",
                        State = "Running",
                        MemoryUsageMB = 20,
                        LoadDuration = TimeSpan.FromMilliseconds(100)
                    }
                }
            };

            IReportRenderer renderer = format.ToLowerInvariant() switch
            {
                "html" => new HtmlReportRenderer(NullLogger<HtmlReportRenderer>.Instance),
                "csv" => new CsvReportRenderer(NullLogger<CsvReportRenderer>.Instance),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var request = new ReportRequest
            {
                Format = format.ToLowerInvariant() == "html" ? ReportFormat.HTML : ReportFormat.CSV,
                OutputPath = output.FullName,
                DataPath = dataFile?.FullName
            };

            var result = await renderer.RenderAsync(request, sampleData, CancellationToken.None);

            if (result.IsSuccess)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✅ Report generated: {result.OutputPath}");
                System.Console.ResetColor();
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
