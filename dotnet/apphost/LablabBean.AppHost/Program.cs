using Aspire.Hosting;

// Ensure Aspire dashboard does not crash the AppHost
// Provide safe defaults for dashboard-related environment variables and allow disabling via LABLAB_ENABLE_DASHBOARD.
var lablabEnableDashboard = Environment.GetEnvironmentVariable("LABLAB_ENABLE_DASHBOARD") ?? "0";
if (!string.Equals(lablabEnableDashboard, "1", StringComparison.Ordinal))
{
    Environment.SetEnvironmentVariable("DOTNET_ASPIRE_DASHBOARD_ENABLED", "false");
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_ENABLED", "false");
}
// Always ensure required vars have sane defaults to satisfy option validation
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://127.0.0.1:18888");
}
// Allow HTTP for local dev unless explicitly disabled
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT")))
{
    Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");
}
var otlpGrpc = Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL");
var otlpHttp = Environment.GetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL");
if (string.IsNullOrEmpty(otlpGrpc) && string.IsNullOrEmpty(otlpHttp))
{
    Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL", "http://127.0.0.1:18889");
}

var builder = DistributedApplication.CreateBuilder(args);

// Dashboard control is handled via env vars externally (scripts/start-aspire.ps1)
// Avoid referencing DashboardOptions here to keep compatibility across Aspire versions.

// Determine repo root by searching upwards for Taskfile.yml and build/nuke/build.ps1
static string FindRepoRoot()
{
    var cur = Directory.GetCurrentDirectory();
    for (var i = 0; i < 8; i++)
    {
        if (File.Exists(Path.Combine(cur, "Taskfile.yml")) &&
            File.Exists(Path.Combine(cur, "build", "nuke", "build.ps1")))
        {
            return cur;
        }
        var parent = Directory.GetParent(cur);
        if (parent == null) break;
        cur = parent.FullName;
    }
    return Directory.GetCurrentDirectory();
}

var repoRoot = FindRepoRoot();

// 2) Web dev server (Astro) on port 3000
var web = builder.AddExecutable(
        name: "web",
        command: "node",
        workingDirectory: repoRoot,
        args: new [] { "scripts/start-web-dev.js" })
    .WithEnvironment("HOST", "0.0.0.0")
    .WithEnvironment("PORT", "3000");

// 3) PTY WebSocket server on port 3001
var term = builder.AddExecutable(
        name: "terminal",
        command: "node",
        workingDirectory: repoRoot,
        args: new [] { "scripts/start-terminal-server.js" })
    .WithEnvironment("TERMINAL_HOST", "0.0.0.0")
    .WithEnvironment("TERMINAL_PORT", "3001");

// 4) Optional WezTerm console launcher (only acts when ASPIRE_LAUNCH_WEZTERM=1)
var aspireLaunchWez = Environment.GetEnvironmentVariable("ASPIRE_LAUNCH_WEZTERM") ?? "0";
if (aspireLaunchWez == "1")
{
    var wezterm = builder.AddExecutable(
            name: "wezterm-launcher",
            command: "pwsh",
            workingDirectory: repoRoot,
            args: new []
            {
                "-NoProfile",
                "-ExecutionPolicy", "Bypass",
                "-File", "scripts/launch-wezterm-console.ps1"
            })
        .WithEnvironment("ASPIRE_LAUNCH_WEZTERM", aspireLaunchWez)
        .WithEnvironment("LABLAB_WEZTERM_PATH", Environment.GetEnvironmentVariable("LABLAB_WEZTERM_PATH") ?? string.Empty)
        .WithEnvironment("LABLAB_ARTIFACT_DIR", Environment.GetEnvironmentVariable("LABLAB_ARTIFACT_DIR") ?? string.Empty);
}

builder.Build().Run();
