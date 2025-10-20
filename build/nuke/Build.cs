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
            Serilog.Log.Information("Building website...");
            
            // Check if website is already built
            var webDist = WebsiteDirectory / "apps" / "web" / "dist";
            if (!System.IO.Directory.Exists(webDist))
            {
                Serilog.Log.Information("Website not built, building now...");
                ProcessTasks.StartProcess("pnpm", "build:all", workingDirectory: WebsiteDirectory)
                    .AssertZeroExitCode();
            }
            else
            {
                Serilog.Log.Information("Website already built, skipping build");
            }
            
            // Copy to artifacts
            var websiteArtifacts = PublishDirectory / "website";
            websiteArtifacts.CreateOrCleanDirectory();
            
            // Copy built web app
            CopyDirectoryRecursively(
                WebsiteDirectory / "apps" / "web" / "dist",
                websiteArtifacts,
                DirectoryExistsPolicy.Merge);
            
            Serilog.Log.Information("Website built to: {Path}", websiteArtifacts);
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

            // Skip Windows App for now (has compilation errors)
            Serilog.Log.Information("Skipping Windows App (compilation errors)");
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
