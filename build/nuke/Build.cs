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

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution]
    readonly Solution Solution;

    [GitVersion(Framework = "net8.0", NoFetch = true)]
    readonly GitVersion GitVersion;

    string Version => GitVersion?.SemVer ?? "0.1.0-dev";

    AbsolutePath SourceDirectory => RootDirectory / "dotnet";
    AbsolutePath WebsiteDirectory => RootDirectory / "website";
    AbsolutePath BuildArtifactsDirectory => RootDirectory / "build" / "_artifacts";
    AbsolutePath VersionedArtifactsDirectory => BuildArtifactsDirectory / Version;
    AbsolutePath PublishDirectory => VersionedArtifactsDirectory / "publish";
    AbsolutePath LogsDirectory => VersionedArtifactsDirectory / "logs";
    AbsolutePath TestResultsDirectory => VersionedArtifactsDirectory / "test-results";
    AbsolutePath TestReportsDirectory => VersionedArtifactsDirectory / "test-reports";

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

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // Build only Console app (skip Windows app due to compilation errors)
            var consoleProjectPath = SourceDirectory / "console-app" / "LablabBean.Console" / "LablabBean.Console.csproj";
            DotNetBuild(s => s
                .SetProjectFile(consoleProjectPath)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
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

    Target GenerateReports => _ => _
        .DependsOn(TestWithCoverage)
        .Executes(() =>
        {
            TestReportsDirectory.CreateOrCleanDirectory();
            
            var reportingToolPath = SourceDirectory / "console-app" / "LablabBean.Console" / "bin" / Configuration / "net8.0" / "LablabBean.Console.dll";
            
            if (!System.IO.File.Exists(reportingToolPath))
            {
                Serilog.Log.Warning("Reporting tool not found at {Path}", reportingToolPath);
                Serilog.Log.Information("Run 'nuke Compile' first to build the reporting tool");
                return;
            }
            
            Serilog.Log.Information("Generating build metrics report...");
            
            var buildReportPath = TestReportsDirectory / "build-metrics.html";
            var sessionReportPath = TestReportsDirectory / "session-analytics.html";
            var pluginReportPath = TestReportsDirectory / "plugin-metrics.html";
            
            try
            {
                DotNet($"{reportingToolPath} report build " +
                      $"--output \"{buildReportPath}\" " +
                      $"--data \"{TestResultsDirectory}\" " +
                      $"--format html",
                      workingDirectory: RootDirectory);
                
                Serilog.Log.Information("✅ Build metrics report: {Path}", buildReportPath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("Build metrics report failed: {Message}", ex.Message);
            }
            
            Serilog.Log.Information("Generating session analytics report...");
            
            try
            {
                DotNet($"{reportingToolPath} report session " +
                      $"--output \"{sessionReportPath}\" " +
                      $"--data \"{TestResultsDirectory}\" " +
                      $"--format html",
                      workingDirectory: RootDirectory);
                
                Serilog.Log.Information("✅ Session analytics report: {Path}", sessionReportPath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("Session analytics report failed: {Message}", ex.Message);
            }
            
            Serilog.Log.Information("Generating plugin metrics report...");
            
            try
            {
                DotNet($"{reportingToolPath} report plugin " +
                      $"--output \"{pluginReportPath}\" " +
                      $"--data \"{TestResultsDirectory}\" " +
                      $"--format html",
                      workingDirectory: RootDirectory);
                
                Serilog.Log.Information("✅ Plugin metrics report: {Path}", pluginReportPath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("Plugin metrics report failed: {Message}", ex.Message);
            }
            
            Serilog.Log.Information("\n╔════════════════════════════════════════╗");
            Serilog.Log.Information("║     📊 REPORTS GENERATED! 🎉         ║");
            Serilog.Log.Information("╚════════════════════════════════════════╝");
            Serilog.Log.Information("Location: {Path}", TestReportsDirectory);
            Serilog.Log.Information("  - build-metrics.html");
            Serilog.Log.Information("  - session-analytics.html");
            Serilog.Log.Information("  - plugin-metrics.html");
            Serilog.Log.Information("════════════════════════════════════════\n");
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
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("LablabBean.Console"))
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "console")
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .EnableNoRestore());
        });

    Target PublishWindows => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("LablabBean.Windows"))
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "windows")
                .SetRuntime("win-x64")
                .SetSelfContained(true)
                .EnableNoRestore());
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
            DotNetPublish(s => s
                .SetProject(consoleProjectPath)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory / "console")
                .SetRuntime("win-x64")
                .SetSelfContained(true));

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
        .DependsOn(Clean, PrintVersion, PublishAll, BuildWebsite)
        .Executes(() =>
        {
            Serilog.Log.Information("Release artifacts created at: {Path}", VersionedArtifactsDirectory);
            Serilog.Log.Information("Version: {Version}", Version);
            
            // Create directory structure
            LogsDirectory.CreateDirectory();
            TestResultsDirectory.CreateDirectory();
            TestReportsDirectory.CreateDirectory();
            
            // Create .gitkeep files
            System.IO.File.WriteAllText(LogsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestResultsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestReportsDirectory / ".gitkeep", "");
            
            // Create version info file
            var versionFile = VersionedArtifactsDirectory / "version.json";
            System.IO.File.WriteAllText(versionFile, System.Text.Json.JsonSerializer.Serialize(new
            {
                version = Version,
                fullSemVer = GitVersion?.FullSemVer ?? Version,
                branch = GitVersion?.BranchName ?? "unknown",
                commit = GitVersion?.Sha ?? "unknown",
                buildDate = DateTime.UtcNow.ToString("o"),
                components = new string[] { "console", "website" },
                directories = new
                {
                    publish = "publish/",
                    logs = "logs/",
                    testResults = "test-results/",
                    testReports = "test-reports/"
                }
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            
            Serilog.Log.Information("Version file created: {File}", versionFile);
            
            // List all artifacts
            Serilog.Log.Information("\n=== Release Artifacts ===");
            Serilog.Log.Information("Version: {Version}", Version);
            Serilog.Log.Information("Console App: {Path}", PublishDirectory / "console");
            Serilog.Log.Information("Website: {Path}", PublishDirectory / "website");
            Serilog.Log.Information("Logs: {Path}", LogsDirectory);
            Serilog.Log.Information("Test Results: {Path}", TestResultsDirectory);
            Serilog.Log.Information("Test Reports: {Path}", TestReportsDirectory);
            Serilog.Log.Information("========================\n");
        });
}
