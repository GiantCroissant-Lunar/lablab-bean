using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.PathConstruction;
using System.IO.Compression;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text.Json;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild, IBuildConfig
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution]
    readonly Solution Solution;

    [GitVersion(Framework = "net8.0", NoFetch = true)]
    readonly GitVersion GitVersion;

    string Version => GitVersion?.SemVer ?? "0.1.0-dev";

    // Provide concrete accessors for configuration to avoid relying on default interface implementations
    AbsolutePath BuildConfigPath => RootDirectory / "build" / "nuke" / "build.config.json";
    BuildConfig Config => BuildConfig.Load(BuildConfigPath);

    AbsolutePath SourceDirectory => RootDirectory / (Config?.SourceDir ?? "dotnet");
    AbsolutePath WebsiteDirectory => RootDirectory / (Config?.WebsiteDir ?? "website");
    AbsolutePath SchemasDirectory => RootDirectory / "schemas";
    AbsolutePath GeneratedDirectory => SourceDirectory / "framework" / "LablabBean.Framework.Generated";
    AbsolutePath BuildArtifactsDirectory => RootDirectory / "build" / "_artifacts";
    AbsolutePath VersionedArtifactsDirectory => BuildArtifactsDirectory / Version;
    AbsolutePath PublishDirectory => VersionedArtifactsDirectory / "publish";
    AbsolutePath NugetDirectory => VersionedArtifactsDirectory / "nuget";
    AbsolutePath LogsDirectory => VersionedArtifactsDirectory / "logs";
    AbsolutePath TestResultsDirectory => VersionedArtifactsDirectory / "test-results";
    AbsolutePath TestReportsDirectory => VersionedArtifactsDirectory / "test-reports";
    AbsolutePath SessionReportsDirectory => VersionedArtifactsDirectory / "reports" / "sessions";

    Target PrintVersion => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Version: {Version}", Version);
            if (GitVersion != null)
            {
                Serilog.Log.Information("Branch: {Branch}", GitVersion.BranchName);
                Serilog.Log.Information("Commit: {Commit}", GitVersion.Sha);
            }
            Serilog.Log.Information("Artifacts: {Path}", VersionedArtifactsDirectory);
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d =>
            {
                if (System.IO.Directory.Exists(d))
                    DeleteDirectory(d);
            });

            // Skip website cleaning if directory doesn't exist or is incomplete
            if (System.IO.Directory.Exists(WebsiteDirectory))
            {
                try
                {
                    WebsiteDirectory.GlobDirectories("**/dist").ForEach(d =>
                    {
                        if (System.IO.Directory.Exists(d))
                            DeleteDirectory(d);
                    });
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Serilog.Log.Warning("Skipping website cleanup due to missing directories");
                }
            }

            BuildArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target GenerateApiTypes => _ => _
        .Executes(() =>
        {
            if (!System.IO.Directory.Exists(SchemasDirectory))
            {
                Serilog.Log.Warning("Schemas directory not found at {Path}, skipping API type generation", SchemasDirectory);
                return;
            }

            var outputDir = GeneratedDirectory / "ExternalApis";

            // Clean up stale generated files
            if (System.IO.Directory.Exists(outputDir))
            {
                var existingFiles = outputDir.GlobFiles("*.g.cs");
                foreach (var file in existingFiles)
                {
                    System.IO.File.Delete(file);
                }
                Serilog.Log.Information("Cleaned {Count} existing generated files", existingFiles.Count());
            }

            outputDir.CreateDirectory();

            // Look for JSON Schema files (*.schema.json) in specs/023 contracts directory
            var contractSchemas = (RootDirectory / "specs" / "023-quicktype-mapperly-adoption" / "contracts").GlobFiles("*.schema.json");

            if (!contractSchemas.Any())
            {
                Serilog.Log.Information("No *.schema.json files found, skipping generation");
                return;
            }

            try
            {
                // Validate all JSON schemas
                foreach (var schemaFile in contractSchemas)
                {
                    var schemaContent = System.IO.File.ReadAllText(schemaFile);
                    using (var jsonDoc = JsonDocument.Parse(schemaContent))
                    {
                        // Schema is valid JSON
                    }
                    Serilog.Log.Information("Validated schema: {Schema}", System.IO.Path.GetFileName(schemaFile));
                }

                // Generate all Qdrant models in a single file
                var outputFile = outputDir / "QdrantModels.g.cs";
                var schemaArgs = string.Join(" ", contractSchemas.Select(s => $"\"{s}\""));

                Serilog.Log.Information("Generating Qdrant models from {Count} schemas", contractSchemas.Count());

                var result = ProcessTasks.StartProcess(
                    "npx",
                    $"quicktype --src {schemaArgs} --src-lang schema --lang csharp --framework SystemTextJson --namespace LablabBean.Framework.Generated.Models.Qdrant --array-type list --out \"{outputFile}\"",
                    RootDirectory,
                    timeout: 120000
                ).AssertWaitForExit();

                if (result.ExitCode == 0)
                {
                    Serilog.Log.Information("✓ Successfully generated Qdrant models");
                }
                else
                {
                    throw new Exception($"quicktype failed with exit code {result.ExitCode}");
                }
            }
            catch (JsonException ex)
            {
                Serilog.Log.Error("✗ Invalid JSON in schema: {Error}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("✗ Failed to generate API types: {Error}", ex.Message);
                throw;
            }

            Serilog.Log.Information("API type generation complete");
        });

    static string ConvertKebabToPascal(string kebab)
    {
        return string.Join("", kebab.Split('-').Select(word =>
            char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    Target Restore => _ => _
        .DependsOn(GenerateApiTypes)
        .Executes(() =>
        {
            try
            {
                DotNetRestore(s => s
                    .SetProjectFile(SourceDirectory / "LablabBean.sln"));
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("Solution restore failed: {Message}. Falling back to per-project restore for console and windows apps.", ex.Message);

                var consoleProjectPath = SourceDirectory / "console-app" / "LablabBean.Console" / "LablabBean.Console.csproj";
                var windowsProjectPath = SourceDirectory / "windows-app" / "LablabBean.Windows" / "LablabBean.Windows.csproj";

                DotNetRestore(s => s
                    .SetProjectFile(consoleProjectPath));

                DotNetRestore(s => s
                    .SetProjectFile(windowsProjectPath));
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // Build Console app
            var consoleProjectPath = SourceDirectory / "console-app" / "LablabBean.Console" / "LablabBean.Console.csproj";
            Serilog.Log.Information("Building Console App...");
            DotNetBuild(s => s
                .SetProjectFile(consoleProjectPath)
                .SetConfiguration(Configuration)
                .EnableNoRestore());

            // Build Windows app
            var windowsProjectPath = SourceDirectory / "windows-app" / "LablabBean.Windows" / "LablabBean.Windows.csproj";
            Serilog.Log.Information("Building Windows App...");
            DotNetBuild(s => s
                .SetProjectFile(windowsProjectPath)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                // Use explicit solution path to avoid null Solution injection in some environments
                .SetProjectFile(SourceDirectory / "LablabBean.sln")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    // Formats code using dotnet-format and optionally JetBrains ReSharper Global Tools (jb)
    Target Format => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // dotnet-format (built into .NET SDK): applies whitespace, style, and analyzer code fixes when available
            Serilog.Log.Information("Running dotnet format (whitespace, style, analyzers)...");
            DotNet($"format \"{SourceDirectory / "LablabBean.sln"}\" --verbosity minimal", workingDirectory: RootDirectory);

            // ReSharper Global Tools (optional): enforces EditorConfig wrapping like one-parameter-per-line
            // Tries default installation path then PATH
            try
            {
                var jbDefault = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".dotnet", "tools", "jb.exe");

                bool ranReSharper = false;
                if (System.IO.File.Exists(jbDefault))
                {
                    Serilog.Log.Information("Running ReSharper cleanupcode via {Path}...", jbDefault);
                    var p1 = ProcessTasks.StartProcess(jbDefault, $"cleanupcode \"{SourceDirectory / "LablabBean.sln"}\"", RootDirectory);
                    p1.AssertZeroExitCode();
                    ranReSharper = true;
                }
                if (!ranReSharper)
                {
                    try
                    {
                        Serilog.Log.Information("Trying 'jb' from PATH for ReSharper cleanupcode...");
                        var p2 = ProcessTasks.StartProcess("jb", $"cleanupcode \"{SourceDirectory / "LablabBean.sln"}\"", RootDirectory);
                        p2.AssertZeroExitCode();
                        ranReSharper = true;
                    }
                    catch { /* ignore if not installed */ }
                }

                if (!ranReSharper)
                {
                    Serilog.Log.Information("ReSharper Global Tools (jb) not found. To enable ReSharper-based formatting: dotnet tool install -g JetBrains.ReSharper.GlobalTools");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("ReSharper cleanup skipped: {Message}", ex.Message);
            }
        });

    // Verifies formatting and analyzer diagnostics without changing files (CI-friendly)
    Target FormatCheck => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information("Verifying formatting (no changes)...");
            DotNet($"format \"{SourceDirectory / "LablabBean.sln"}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);

            Serilog.Log.Information("Verifying analyzer diagnostics (no changes)...");
            DotNet($"format analyzers \"{SourceDirectory / "LablabBean.sln"}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);
        });

    // Runs analyzer fixes (where possible) and builds to enforce analyzer severities
    Target Analyze => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information("Running analyzer fixes where available (dotnet format analyzers)...");
            DotNet($"format analyzers \"{SourceDirectory / "LablabBean.sln"}\" --verbosity minimal", workingDirectory: RootDirectory);

            Serilog.Log.Information("Building solution with analyzers enabled...");
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target TestWithCoverage => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();

            var testProjects = Solution.GetAllProjects("*.Tests");

            foreach (var testProject in testProjects)
            {
                var projectName = testProject.Name;
                var testResultFile = TestResultsDirectory / $"{projectName}.trx";

                Serilog.Log.Information("Running tests for {Project}...", projectName);

                DotNetTest(s => s
                    .SetProjectFile(testProject)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLoggers($"trx;LogFileName={testResultFile}")
                    .SetDataCollector("XPlat Code Coverage")
                    .SetResultsDirectory(TestResultsDirectory));
            }

            Serilog.Log.Information("Test results saved to: {Path}", TestResultsDirectory);
        });

    // Builds NuGet packages for framework and plugin projects into versioned artifacts
    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            NugetDirectory.CreateOrCleanDirectory();

            var frameworkRoots = (Config?.FrameworkDirs ?? new System.Collections.Generic.List<string> { "framework" })
                .Select(d => SourceDirectory / d);

            var pluginRoots = (Config?.PluginDirs ?? new System.Collections.Generic.List<string> { "plugins" })
                .Select(d => SourceDirectory / d);

            var frameworkProjects = frameworkRoots
                .SelectMany(r => r.GlobFiles("**/*.csproj"))
                .Where(p => !p.ToString().EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var pluginProjects = pluginRoots
                .SelectMany(r => r.GlobFiles("**/*.csproj"))
                .Where(p => !p.ToString().Contains("/tests/", StringComparison.OrdinalIgnoreCase))
                .Where(p => !p.ToString().EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Optional name-based filters for plugins from config
            if (Config?.IncludePluginNames != null && Config.IncludePluginNames.Count > 0)
            {
                pluginProjects = pluginProjects
                    .Where(p => Config.IncludePluginNames.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }
            if (Config?.ExcludePluginNames != null && Config.ExcludePluginNames.Count > 0)
            {
                pluginProjects = pluginProjects
                    .Where(p => !Config.ExcludePluginNames.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }

            if ((Config?.PackFramework ?? true) && frameworkProjects.Count == 0)
                Serilog.Log.Warning("No framework projects found to pack under: {Roots}", string.Join(", ", frameworkRoots));
            if ((Config?.PackPlugins ?? true) && pluginProjects.Count == 0)
                Serilog.Log.Warning("No plugin projects found to pack under: {Roots}", string.Join(", ", pluginRoots));

            void PackProject(AbsolutePath csproj)
            {
                Serilog.Log.Information("Packing {Project} -> {OutDir}", csproj, NugetDirectory);
                // Build first to ensure outputs exist (supports projects relying on GeneratePackageOnBuild)
                DotNetBuild(s => s
                    .SetProjectFile(csproj)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());

                // Now run pack without building to place packages in the NuGet artifacts directory
                DotNetPack(s => s
                    .SetProject(csproj)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetOutputDirectory(NugetDirectory)
                    .SetVersion(Version)
                    .SetIncludeSymbols(true)
                    .SetSymbolPackageFormat(Nuke.Common.Tools.DotNet.DotNetSymbolPackageFormat.snupkg)
                    .SetProperty("RepositoryBranch", GitVersion?.BranchName ?? "local")
                    .SetProperty("RepositoryCommit", GitVersion?.Sha ?? "local"));

                // Also copy any packages generated by build (GeneratePackageOnBuild)
                try
                {
                    var binDir = csproj.Parent / "bin" / Configuration;
                    if (System.IO.Directory.Exists(binDir))
                    {
                        var pkgs = System.IO.Directory.GetFiles(binDir, "*.nupkg", System.IO.SearchOption.AllDirectories)
                            .Concat(System.IO.Directory.GetFiles(binDir, "*.snupkg", System.IO.SearchOption.AllDirectories))
                            .ToArray();
                        foreach (var pkg in pkgs)
                        {
                            var dest = NugetDirectory / System.IO.Path.GetFileName(pkg);
                            System.IO.File.Copy(pkg, dest, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("Failed to copy build-generated packages for {Project}: {Message}", csproj, ex.Message);
                }
            }

            if (Config?.PackFramework ?? true)
                frameworkProjects.ForEach(PackProject);
            if (Config?.PackPlugins ?? true)
                pluginProjects.ForEach(PackProject);

            Serilog.Log.Information("NuGet packages created at: {Path}", NugetDirectory);
        });

    Target GenerateReports => _ => _
        .Executes(() =>
        {
            TestReportsDirectory.CreateOrCleanDirectory();

            var reportingToolPath = VersionedArtifactsDirectory / "publish" / "console" / "LablabBean.Console.exe";

            if (!System.IO.File.Exists(reportingToolPath))
            {
                Serilog.Log.Error("❌ Reporting tool not found at {Path}", reportingToolPath);
                Serilog.Log.Information("Run 'nuke Compile' first to build the reporting tool");
                throw new Exception("Reporting tool not built. Run 'nuke Compile' first.");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? Version;

            var buildReportPath = TestReportsDirectory / $"build-metrics-{Version}-{buildNumber}-{timestamp}.html";
            var buildReportCsvPath = TestReportsDirectory / $"build-metrics-{Version}-{buildNumber}-{timestamp}.csv";
            var sessionReportPath = TestReportsDirectory / $"session-analytics-{Version}-{buildNumber}-{timestamp}.html";
            var sessionReportCsvPath = TestReportsDirectory / $"session-analytics-{Version}-{buildNumber}-{timestamp}.csv";
            var pluginReportPath = TestReportsDirectory / $"plugin-metrics-{Version}-{buildNumber}-{timestamp}.html";
            var pluginReportCsvPath = TestReportsDirectory / $"plugin-metrics-{Version}-{buildNumber}-{timestamp}.csv";
            var windowsReportPath = TestReportsDirectory / $"windows-session-{Version}-{buildNumber}-{timestamp}.html";
            var windowsReportCsvPath = TestReportsDirectory / $"windows-session-{Version}-{buildNumber}-{timestamp}.csv";

            var latestBuildReportPath = TestReportsDirectory / "build-metrics-latest.html";
            var latestSessionReportPath = TestReportsDirectory / "session-analytics-latest.html";
            var latestPluginReportPath = TestReportsDirectory / "plugin-metrics-latest.html";
            var latestWindowsReportPath = TestReportsDirectory / "windows-session-latest.html";

            Serilog.Log.Information("Generating build metrics report...");

            bool buildMetricsSuccess = false;
            try
            {
                var pBuildHtml = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report build --output \"{buildReportPath}\" --data \"{TestResultsDirectory}\" --format html",
                    RootDirectory);
                pBuildHtml.AssertZeroExitCode();

                var pBuildCsv = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report build --output \"{buildReportCsvPath}\" --data \"{TestResultsDirectory}\" --format csv",
                    RootDirectory);
                pBuildCsv.AssertZeroExitCode();

                if (System.IO.File.Exists(buildReportPath))
                {
                    System.IO.File.Copy(buildReportPath, latestBuildReportPath, true);
                }

                Serilog.Log.Information("✅ Build metrics report: {Path}", buildReportPath);
                Serilog.Log.Information("✅ Build metrics CSV: {Path}", buildReportCsvPath);
                buildMetricsSuccess = true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("❌ Build metrics report failed: {Message}", ex.Message);
            }

            Serilog.Log.Information("Generating session analytics report...");

            bool sessionSuccess = false;
            try
            {
                var pSessionHtml = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report session --output \"{sessionReportPath}\" --data \"{TestResultsDirectory}\" --format html",
                    RootDirectory);
                pSessionHtml.AssertZeroExitCode();

                var pSessionCsv = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report session --output \"{sessionReportCsvPath}\" --data \"{TestResultsDirectory}\" --format csv",
                    RootDirectory);
                pSessionCsv.AssertZeroExitCode();

                if (System.IO.File.Exists(sessionReportPath))
                {
                    System.IO.File.Copy(sessionReportPath, latestSessionReportPath, true);
                }

                Serilog.Log.Information("✅ Session analytics report: {Path}", sessionReportPath);
                Serilog.Log.Information("✅ Session analytics CSV: {Path}", sessionReportCsvPath);
                sessionSuccess = true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("❌ Session analytics report failed: {Message}", ex.Message);
            }

            Serilog.Log.Information("Generating plugin metrics report...");

            bool pluginSuccess = false;
            try
            {
                var pPluginHtml = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report plugin --output \"{pluginReportPath}\" --data \"{TestResultsDirectory}\" --format html",
                    RootDirectory);
                pPluginHtml.AssertZeroExitCode();

                var pPluginCsv = ProcessTasks.StartProcess(
                    reportingToolPath,
                    $"report plugin --output \"{pluginReportCsvPath}\" --data \"{TestResultsDirectory}\" --format csv",
                    RootDirectory);
                pPluginCsv.AssertZeroExitCode();

                if (System.IO.File.Exists(pluginReportPath))
                {
                    System.IO.File.Copy(pluginReportPath, latestPluginReportPath, true);
                }

                Serilog.Log.Information("✅ Plugin metrics report: {Path}", pluginReportPath);
                Serilog.Log.Information("✅ Plugin metrics CSV: {Path}", pluginReportCsvPath);
                pluginSuccess = true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("❌ Plugin metrics report failed: {Message}", ex.Message);
            }

            // Check for Windows session reports
            Serilog.Log.Information("Checking for Windows session reports...");
            bool windowsSuccess = false;
            try
            {
                if (System.IO.Directory.Exists(SessionReportsDirectory))
                {
                    var windowsSessionFiles = System.IO.Directory.GetFiles(SessionReportsDirectory, "windows-session-*.json")
                        .OrderByDescending(f => System.IO.File.GetCreationTimeUtc(f))
                        .ToArray();

                    if (windowsSessionFiles.Length > 0)
                    {
                        var latestWindowsSession = windowsSessionFiles[0];
                        Serilog.Log.Information("Found Windows session: {Path}", latestWindowsSession);

                        // Generate HTML report from Windows session
                        DotNet($"{reportingToolPath} report session " +
                              $"--output \"{windowsReportPath}\" " +
                              $"--data \"{latestWindowsSession}\" " +
                              $"--format html",
                              workingDirectory: RootDirectory);

                        // Generate CSV report
                        DotNet($"{reportingToolPath} report session " +
                              $"--output \"{windowsReportCsvPath}\" " +
                              $"--data \"{latestWindowsSession}\" " +
                              $"--format csv",
                              workingDirectory: RootDirectory);

                        if (System.IO.File.Exists(windowsReportPath))
                        {
                            System.IO.File.Copy(windowsReportPath, latestWindowsReportPath, true);
                        }

                        Serilog.Log.Information("✅ Windows session report: {Path}", windowsReportPath);
                        Serilog.Log.Information("✅ Windows session CSV: {Path}", windowsReportCsvPath);
                        windowsSuccess = true;
                    }
                    else
                    {
                        Serilog.Log.Information("ℹ️  No Windows session reports found. Run the Windows app to generate session data.");
                    }
                }
                else
                {
                    Serilog.Log.Information("ℹ️  Windows session reports directory does not exist yet.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("⚠️  Windows session report processing failed: {Message}", ex.Message);
            }

            Serilog.Log.Information("\n╔════════════════════════════════════════╗");
            Serilog.Log.Information("║     📊 REPORTS GENERATED! 🎉         ║");
            Serilog.Log.Information("╚════════════════════════════════════════╝");
            Serilog.Log.Information("Build: {Build} | Timestamp: {Timestamp}", buildNumber, timestamp);
            Serilog.Log.Information("Location: {Path}", TestReportsDirectory);
            if (buildMetricsSuccess)
            {
                Serilog.Log.Information("  ✅ build-metrics-{0}-{1}.html/.csv", buildNumber, timestamp);
            }
            if (sessionSuccess)
            {
                Serilog.Log.Information("  ✅ session-analytics-{0}-{1}.html/.csv", buildNumber, timestamp);
            }
            if (pluginSuccess)
            {
                Serilog.Log.Information("  ✅ plugin-metrics-{0}-{1}.html/.csv", buildNumber, timestamp);
            }
            if (windowsSuccess)
            {
                Serilog.Log.Information("  ✅ windows-session-{0}-{1}.html/.csv", buildNumber, timestamp);
            }
            Serilog.Log.Information("════════════════════════════════════════\n");

            if (!buildMetricsSuccess || !sessionSuccess || !pluginSuccess)
            {
                Serilog.Log.Warning("⚠️  Some reports failed to generate. Check logs above.");
            }
        });

    Target GenerateWindowsMetrics => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            SessionReportsDirectory.CreateOrCleanDirectory();

            var windowsAppPath = SourceDirectory / "windows-app" / "LablabBean.Windows" / "bin" / Configuration / "net8.0" / "LablabBean.Windows.dll";

            if (!System.IO.File.Exists(windowsAppPath))
            {
                Serilog.Log.Error("❌ Windows app not found at {Path}", windowsAppPath);
                Serilog.Log.Information("Run 'nuke Compile' first to build the Windows app");
                throw new Exception("Windows app not built. Run 'nuke Compile' first.");
            }

            Serilog.Log.Information("╔════════════════════════════════════════╗");
            Serilog.Log.Information("║  🎮 Windows App Build-Time Metrics    ║");
            Serilog.Log.Information("╚════════════════════════════════════════╝");
            Serilog.Log.Information("");
            Serilog.Log.Information("Windows app compiled successfully!");
            Serilog.Log.Information("Binary: {Path}", windowsAppPath);
            Serilog.Log.Information("Size: {Size:N0} bytes", new System.IO.FileInfo(windowsAppPath).Length);
            Serilog.Log.Information("");
            Serilog.Log.Information("Session reports will be generated when you run the app.");
            Serilog.Log.Information("Reports location: {Path}", SessionReportsDirectory);
            Serilog.Log.Information("");
            Serilog.Log.Information("To generate session data:");
            Serilog.Log.Information("  1. Run: dotnet run --project dotnet/windows-app/LablabBean.Windows");
            Serilog.Log.Information("  2. Play the game (kill enemies, collect items, etc.)");
            Serilog.Log.Information("  3. Exit the game to save session report");
            Serilog.Log.Information("  4. Run: nuke GenerateReports");
            Serilog.Log.Information("");
            Serilog.Log.Information("════════════════════════════════════════");

            // Create a build-time metrics file
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? Version;
            var buildMetricsPath = SessionReportsDirectory / $"windows-build-metrics-{Version}-{buildNumber}-{timestamp}.json";

            var buildMetrics = new
            {
                buildNumber = buildNumber,
                version = Version,
                timestamp = DateTime.UtcNow.ToString("o"),
                binaryPath = windowsAppPath.ToString(),
                binarySize = new System.IO.FileInfo(windowsAppPath).Length,
                configuration = Configuration,
                framework = "net8.0",
                platform = "win-x64",
                features = new[]
                {
                    "SessionMetrics",
                    "KillTracking",
                    "DeathTracking",
                    "ItemCollection",
                    "LevelProgression",
                    "DepthTracking",
                    "DungeonCompletion"
                }
            };

            System.IO.File.WriteAllText(buildMetricsPath, System.Text.Json.JsonSerializer.Serialize(buildMetrics, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            Serilog.Log.Information("Build metrics saved: {Path}", buildMetricsPath);
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("LablabBean.Console"))
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "console")
                .EnableNoRestore());

            DotNetPublish(s => s
                .SetProject(Solution.GetProject("LablabBean.Windows"))
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "windows")
                .EnableNoRestore());
        });

    Target PublishConsole => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var consoleOut = PublishDirectory / "console";
            DotNetPublish(s => s
                .SetProject(SourceDirectory / "console-app" / "LablabBean.Console" / "LablabBean.Console.csproj")
                .SetConfiguration(Configuration)
                .SetOutput(consoleOut)
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .SetProcessArgumentConfigurator(args => args.Add("/p:PluginJsonTargetSubfolder=true")));

            // Publish plugins next to console app for runtime loading
            var pluginsRoot = consoleOut / "plugins";
            pluginsRoot.CreateOrCleanDirectory();

            var pluginProjects = (RootDirectory / "dotnet" / "plugins")
                .GlobFiles("**/*.csproj")
                .Where(p => {
                    var n = p.ToString().Replace('\\','/');
                    return !n.Contains("/tests/", StringComparison.OrdinalIgnoreCase)
                           && !n.Contains("/LablabBean.Plugins.Merchant/", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            foreach (var pluginCsproj in pluginProjects)
            {
                var pluginName = System.IO.Path.GetFileNameWithoutExtension(pluginCsproj);
                var dest = pluginsRoot / pluginName;
                Serilog.Log.Information("Publishing plugin {Plugin} -> {Dest}", pluginName, dest);
                DotNetPublish(s => s
                    .SetProject(pluginCsproj)
                    .SetConfiguration(Configuration)
                    .SetOutput(dest)
                    // Route all plugin.json to a subfolder during publish to avoid collisions between referenced plugins
                    .SetProcessArgumentConfigurator(args => args.Add("/p:PluginJsonTargetSubfolder=true")));

                // After publish, copy this project's own plugin.json back to root for loader simplicity
                try
                {
                    var thisPluginName = System.IO.Path.GetFileNameWithoutExtension(pluginCsproj);
                    var nested = dest / "plugins" / thisPluginName / "plugin.json";
                    var rootManifest = dest / "plugin.json";
                    if (System.IO.File.Exists(nested))
                    {
                        System.IO.File.Copy(nested, rootManifest, true);
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("Could not normalize plugin.json for {Plugin}: {Message}", pluginCsproj, ex.Message);
                }
            }
        });

    Target PublishWindows => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(SourceDirectory / "windows-app" / "LablabBean.Windows" / "LablabBean.Windows.csproj")
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "windows")
                .SetRuntime("win-x64")
                .SetSelfContained(true));
        });

    Target BuildWebsite => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Copying website artifacts...");

            // Expect website to be already built by separate build process
            var webDist = WebsiteDirectory / "apps" / "web" / "dist";
            if (!System.IO.Directory.Exists(webDist))
            {
                throw new Exception("Website not built. Please run: task build-website first");
            }

            // Copy built website to artifacts
            var websiteArtifacts = PublishDirectory / "website";
            websiteArtifacts.CreateOrCleanDirectory();

            CopyDirectoryRecursively(
                webDist,
                websiteArtifacts,
                DirectoryExistsPolicy.Merge);

            Serilog.Log.Information("Website artifacts copied to: {Path}", websiteArtifacts);
        });

    Target PublishAll => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Publish Console App
            var consoleProjectPath = SourceDirectory / "console-app" / "LablabBean.Console" / "LablabBean.Console.csproj";
            Serilog.Log.Information("Publishing Console App...");
            var consoleOut = PublishDirectory / "console";
            DotNetPublish(s => s
                .SetProject(consoleProjectPath)
                .SetConfiguration(Configuration)
                .SetOutput(consoleOut)
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .SetProcessArgumentConfigurator(args => args.Add("/p:PluginJsonTargetSubfolder=true")));

            // Publish Plugins next to console app
            var pluginsRoot = consoleOut / "plugins";
            pluginsRoot.CreateOrCleanDirectory();
            var pluginProjects = (RootDirectory / "dotnet" / "plugins")
                .GlobFiles("**/*.csproj")
                .Where(p => {
                    var n = p.ToString().Replace('\\','/');
                    return !n.Contains("/tests/", StringComparison.OrdinalIgnoreCase)
                           && !n.Contains("/LablabBean.Plugins.Merchant/", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            foreach (var pluginCsproj in pluginProjects)
            {
                var pluginName = System.IO.Path.GetFileNameWithoutExtension(pluginCsproj);
                var dest = pluginsRoot / pluginName;
                Serilog.Log.Information("Publishing plugin {Plugin} -> {Dest}", pluginName, dest);
                DotNetPublish(s => s
                    .SetProject(pluginCsproj)
                    .SetConfiguration(Configuration)
                    .SetOutput(dest)
                    .SetProcessArgumentConfigurator(args => args.Add("/p:PluginJsonTargetSubfolder=true")));

                try
                {
                    var thisPluginName = System.IO.Path.GetFileNameWithoutExtension(pluginCsproj);
                    var nested = dest / "plugins" / thisPluginName / "plugin.json";
                    var rootManifest = dest / "plugin.json";
                    if (System.IO.File.Exists(nested))
                    {
                        System.IO.File.Copy(nested, rootManifest, true);
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("Could not normalize plugin.json for {Plugin}: {Message}", pluginCsproj, ex.Message);
                }
            }

            // Publish Windows App
            var windowsProjectPath = SourceDirectory / "windows-app" / "LablabBean.Windows" / "LablabBean.Windows.csproj";
            Serilog.Log.Information("Publishing Windows App...");
            DotNetPublish(s => s
                .SetProject(windowsProjectPath)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "windows")
                .SetRuntime("win-x64")
                .SetSelfContained(true));
        });

    Target Release => _ => _
        .DependsOn(Clean, PrintVersion, PublishAll, Pack, SyncNugetLocal, BuildWebsite)
        .Executes(() =>
        {
            Serilog.Log.Information("Release artifacts created at: {Path}", VersionedArtifactsDirectory);
            Serilog.Log.Information("Version: {Version}", Version);

            // Create directory structure
            LogsDirectory.CreateDirectory();
            TestResultsDirectory.CreateDirectory();
            TestReportsDirectory.CreateDirectory();
            SessionReportsDirectory.CreateDirectory();

            // Create .gitkeep files
            System.IO.File.WriteAllText(LogsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestResultsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestReportsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(SessionReportsDirectory / ".gitkeep", "");

            // Create version info file
            var versionFile = VersionedArtifactsDirectory / "version.json";
            System.IO.File.WriteAllText(versionFile, System.Text.Json.JsonSerializer.Serialize(new
            {
                version = Version,
                fullSemVer = GitVersion?.FullSemVer ?? Version,
                branch = GitVersion?.BranchName ?? "unknown",
                commit = GitVersion?.Sha ?? "unknown",
                buildDate = DateTime.UtcNow.ToString("o"),
                components = new string[] { "console", "windows", "website", "nuget" },
                directories = new
                {
                    publish = "publish/",
                    nuget = "nuget/",
                    logs = "logs/",
                    testResults = "test-results/",
                    testReports = "test-reports/",
                    sessionReports = "reports/sessions/"
                }
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            Serilog.Log.Information("Version file created: {File}", versionFile);

            // List all artifacts
            Serilog.Log.Information("\n=== Release Artifacts ===");
            Serilog.Log.Information("Version: {Version}", Version);
            Serilog.Log.Information("Console App: {Path}", PublishDirectory / "console");
            Serilog.Log.Information("Windows App: {Path}", PublishDirectory / "windows");
            Serilog.Log.Information("Website: {Path}", PublishDirectory / "website");
            Serilog.Log.Information("NuGet: {Path}", NugetDirectory);
            Serilog.Log.Information("Logs: {Path}", LogsDirectory);
            Serilog.Log.Information("Test Results: {Path}", TestResultsDirectory);
            Serilog.Log.Information("Test Reports: {Path}", TestReportsDirectory);
            Serilog.Log.Information("Session Reports: {Path}", SessionReportsDirectory);
            Serilog.Log.Information("========================\n");
        });

    // Syncs produced NuGet packages to a configurable local feed in two layouts: flat and hierarchical (id/version)
    Target SyncNugetLocal => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            if (!(Config?.SyncLocalNugetFeed ?? false))
            {
                Serilog.Log.Information("Local NuGet feed sync is disabled by config.");
                return;
            }

            var rootPath = Config?.LocalNugetFeedRoot;
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                Serilog.Log.Warning("LocalNugetFeedRoot not configured; skipping feed sync.");
                return;
            }

            AbsolutePath feedRoot = (AbsolutePath)rootPath;
            var flatDir = feedRoot / (Config?.LocalNugetFeedFlatSubdir ?? "flat");
            var hierDir = feedRoot / (Config?.LocalNugetFeedHierarchicalSubdir ?? "hierarchical");

            flatDir.CreateDirectory();
            hierDir.CreateDirectory();

            var packages = System.IO.Directory.GetFiles(NugetDirectory, "*.nupkg")
                .Concat(System.IO.Directory.GetFiles(NugetDirectory, "*.snupkg"))
                .ToArray();

            var versionsById = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.SortedSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var pkg in packages)
            {
                var fileName = System.IO.Path.GetFileName(pkg);
                var destFlat = flatDir / fileName;
                System.IO.File.Copy(pkg, destFlat, true);

                // Write SHA512 alongside (NuGet folder source convention)
                WriteSha512(destFlat);

                // Compute hierarchical path by reading nuspec id + version from the package
                try
                {
                    var (id, version) = ReadIdVersionFromPackage(pkg);
                    if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(version))
                    {
                        var idLower = id.ToLowerInvariant();
                        var versionLower = version.ToLowerInvariant();
                        var destDir = hierDir / idLower / versionLower;
                        destDir.CreateDirectory();
                        var ext = System.IO.Path.GetExtension(fileName);
                        var standardName = $"{idLower}.{versionLower}{ext}";
                        var destHier = destDir / standardName;
                        System.IO.File.Copy(pkg, destHier, true);

                        // Write SHA512 alongside hierarchical package
                        WriteSha512(destHier);

                        // Optional: extract nuspec alongside
                        try
                        {
                            var nuspecContent = ReadNuspecFromPackage(pkg);
                            if (!string.IsNullOrEmpty(nuspecContent))
                            {
                                var nuspecPath = destDir / ($"{idLower}.nuspec");
                                System.IO.File.WriteAllText(nuspecPath, nuspecContent);
                            }
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Warning("Failed to extract nuspec for {Package}: {Message}", fileName, ex.Message);
                        }

                        // Track versions for index
                        if (!versionsById.TryGetValue(idLower, out var set))
                        {
                            set = new System.Collections.Generic.SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                            versionsById[idLower] = set;
                        }
                        set.Add(versionLower);
                    }
                    else
                    {
                        Serilog.Log.Warning("Could not determine id/version for package: {Package}", fileName);
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Warning("Failed hierarchical sync for {Package}: {Message}", fileName, ex.Message);
                }
            }

            // Generate lightweight indices
            try
            {
                GenerateLocalFeedIndexes(hierDir, versionsById);
                // Optional: Create a minimal V3 service index if a base URL is configured
                if (!string.IsNullOrWhiteSpace(Config?.LocalNugetFeedBaseUrl))
                {
                    var baseUrl = Config.LocalNugetFeedBaseUrl.TrimEnd('/') + "/";
                    var serviceIndex = new
                    {
                        version = "3.0.0",
                        resources = new object[]
                        {
                            new { @id = baseUrl, @type = "PackageBaseAddress/3.0.0", comment = "Static flat container" }
                        }
                    };
                    var serviceIndexPath = hierDir / "index.json";
                    System.IO.File.WriteAllText(serviceIndexPath, JsonSerializer.Serialize(serviceIndex, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("Failed to generate local feed indexes: {Message}", ex.Message);
            }

            Serilog.Log.Information("Local feed sync complete. Flat={Flat} Hierarchical={Hier}", flatDir, hierDir);
        });

    static (string id, string version) ReadIdVersionFromPackage(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var nuspec = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        if (nuspec == null) return (null, null);
        using var s = nuspec.Open();
        var doc = XDocument.Load(s);
        var root = doc.Root;
        if (root == null) return (null, null);
        var ns = root.Name.Namespace;
        var md = root.Element(ns + "metadata") ?? root.Element("metadata");
        var id = md?.Element(ns + "id")?.Value ?? md?.Element("id")?.Value;
        var version = md?.Element(ns + "version")?.Value ?? md?.Element("version")?.Value;
        return (id, version);
    }

    static string ReadNuspecFromPackage(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var nuspec = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        if (nuspec == null) return null;
        using var s = nuspec.Open();
        using var sr = new System.IO.StreamReader(s);
        return sr.ReadToEnd();
    }

    static void WriteSha512(AbsolutePath packagePath)
    {
        try
        {
            using var sha = SHA512.Create();
            using var fs = System.IO.File.OpenRead(packagePath);
            var hash = sha.ComputeHash(fs);
            var b64 = Convert.ToBase64String(hash);
            var shaPath = packagePath + ".sha512";
            System.IO.File.WriteAllText(shaPath, b64);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning("Failed to write sha512 for {Path}: {Message}", packagePath, ex.Message);
        }
    }

    static void GenerateLocalFeedIndexes(AbsolutePath hierRoot, System.Collections.Generic.Dictionary<string, System.Collections.Generic.SortedSet<string>> versionsById)
    {
        // Root index: lists all package IDs and count of versions
        var rootIndex = new
        {
            generated = DateTime.UtcNow.ToString("o"),
            packages = versionsById.OrderBy(k => k.Key).Select(kv => new
            {
                id = kv.Key,
                versions = kv.Value.OrderBy(v => v).ToArray(),
                latest = kv.Value.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).LastOrDefault()
            }).ToArray()
        };
        var rootIndexPath = hierRoot / "packages-index.json";
        System.IO.File.WriteAllText(rootIndexPath, JsonSerializer.Serialize(rootIndex, new JsonSerializerOptions { WriteIndented = true }));

        // Per-package index similar to packageBaseAddress {id}/index.json
        foreach (var kv in versionsById)
        {
            var pkgDir = hierRoot / kv.Key;
            pkgDir.CreateDirectory();
            var perIndex = new { versions = kv.Value.OrderBy(v => v).ToArray() };
            var perIndexPath = pkgDir / "index.json";
            System.IO.File.WriteAllText(perIndexPath, JsonSerializer.Serialize(perIndex, new JsonSerializerOptions { WriteIndented = true }));
        }
    }


    Target ReleaseConsole => _ => _
        .DependsOn(PrintVersion, PublishConsole, VerifyArtifact, GenerateReports)
        .Executes(() =>
        {
            Serilog.Log.Information("ReleaseConsole complete. Artifacts at: {Path}", VersionedArtifactsDirectory);
        });

    // Verifies the published console artifact by invoking its built-in verification and reporting commands.
    Target VerifyArtifact => _ => _
        .DependsOn(PublishConsole)
        .Executes(() =>
        {
            var consoleDir = PublishDirectory / "console";
            var exePath = consoleDir / "LablabBean.Console.exe";
            if (!System.IO.File.Exists(exePath))
            {
                throw new Exception($"Console not found at {exePath}. Run PublishConsole first.");
            }

            TestReportsDirectory.CreateDirectory();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? Version;
            var verifyJson = TestReportsDirectory / $"plugin-health-artifact-{Version}-{buildNumber}-{timestamp}.json";
            var verifyHtml = TestReportsDirectory / $"plugin-health-artifact-{Version}-{buildNumber}-{timestamp}.html";
            var verifyCsv  = TestReportsDirectory / $"plugin-health-artifact-{Version}-{buildNumber}-{timestamp}.csv";

            Serilog.Log.Information("Running artifact verification from: {Dir}", consoleDir);

            // Run verification against local plugins folder inside the artifact
            var pVerify = ProcessTasks.StartProcess(
                exePath,
                $"plugins verify --paths plugins --output \"{verifyJson}\"",
                consoleDir);
            pVerify.AssertZeroExitCode();

            // Render HTML and CSV using the same artifact exe
            var pHtml = ProcessTasks.StartProcess(
                exePath,
                $"report plugin --output \"{verifyHtml}\" --data \"{verifyJson}\" --format html",
                consoleDir);
            pHtml.AssertZeroExitCode();

            var pCsv = ProcessTasks.StartProcess(
                exePath,
                $"report plugin --output \"{verifyCsv}\" --data \"{verifyJson}\" --format csv",
                consoleDir);
            pCsv.AssertZeroExitCode();

            Serilog.Log.Information("Artifact verification complete:");
            Serilog.Log.Information("  JSON: {Path}", verifyJson);
            Serilog.Log.Information("  HTML: {Path}", verifyHtml);
            Serilog.Log.Information("  CSV : {Path}", verifyCsv);
        });
}
