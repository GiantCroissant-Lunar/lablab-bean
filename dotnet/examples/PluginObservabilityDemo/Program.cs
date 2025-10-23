using LablabBean.Plugins.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("🔍 Plugin System Observability Demo\n");
Console.WriteLine("=".PadRight(60, '='));

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddPluginSystem(context.Configuration);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

// Start the host (this will trigger plugin loading)
await host.StartAsync();

Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine("🔍 OBSERVABILITY DEMONSTRATION");
Console.WriteLine("=".PadRight(60, '=') + "\n");

// Get observability services
var adminService = host.Services.GetRequiredService<PluginAdminService>();
var healthChecker = host.Services.GetRequiredService<PluginHealthChecker>();
var metrics = host.Services.GetRequiredService<PluginSystemMetrics>();

// 1. Display system status
Console.WriteLine("📊 1. SYSTEM STATUS");
Console.WriteLine("-".PadRight(60, '-'));
var systemStatus = await adminService.GetSystemStatusAsync();
Console.WriteLine($"Total Plugins: {systemStatus.TotalPlugins}");
Console.WriteLine($"Loaded: {systemStatus.LoadedPlugins}");
Console.WriteLine($"Failed: {systemStatus.FailedPlugins}");
Console.WriteLine($"System Health: {systemStatus.SystemHealth}");
Console.WriteLine($"Checked At: {systemStatus.CheckedAt:yyyy-MM-dd HH:mm:ss}\n");

// 2. Display individual plugin status
Console.WriteLine("📦 2. PLUGIN DETAILS");
Console.WriteLine("-".PadRight(60, '-'));
foreach (var plugin in systemStatus.Plugins)
{
    var emoji = plugin.Health switch
    {
        PluginHealthStatus.Healthy => "✅",
        PluginHealthStatus.Degraded => "⚠️",
        PluginHealthStatus.Unhealthy => "❌",
        _ => "❓"
    };

    Console.WriteLine($"{emoji} {plugin.Name} v{plugin.Version}");
    Console.WriteLine($"   Profile: {plugin.Profile}");
    Console.WriteLine($"   Status: {(plugin.IsLoaded ? "Loaded" : "Not Loaded")}");
    Console.WriteLine($"   Health: {plugin.Health} - {plugin.HealthMessage}");

    if (plugin.LoadDuration.HasValue)
        Console.WriteLine($"   Load Time: {plugin.LoadDuration.Value.TotalMilliseconds:F0}ms");

    if (plugin.MemoryUsage.HasValue)
        Console.WriteLine($"   Memory: {plugin.MemoryUsage.Value / 1024.0:F1} KB");

    if (!string.IsNullOrEmpty(plugin.LoadError))
        Console.WriteLine($"   Error: {plugin.LoadError}");

    Console.WriteLine();
}

// 3. Display aggregated metrics
Console.WriteLine("📈 3. AGGREGATED METRICS");
Console.WriteLine("-".PadRight(60, '-'));
Console.WriteLine(metrics.GetSummary());

// 4. Export metrics to JSON
Console.WriteLine("\n💾 4. METRICS EXPORT");
Console.WriteLine("-".PadRight(60, '-'));
var jsonMetrics = adminService.ExportMetrics();
Console.WriteLine("Metrics exported to JSON:");
Console.WriteLine(jsonMetrics.Substring(0, Math.Min(200, jsonMetrics.Length)) + "...\n");

// 5. Health check demonstration
Console.WriteLine("🏥 5. HEALTH CHECK");
Console.WriteLine("-".PadRight(60, '-'));
var healthResults = await healthChecker.CheckAllAsync();
foreach (var result in healthResults)
{
    var statusSymbol = result.Status switch
    {
        PluginHealthStatus.Healthy => "✅",
        PluginHealthStatus.Degraded => "⚠️",
        PluginHealthStatus.Unhealthy => "❌",
        _ => "❓"
    };
    Console.WriteLine($"{statusSymbol} {result.PluginName}: {result.Status}");
    if (result.Data.Any())
    {
        foreach (var kvp in result.Data)
        {
            Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
        }
    }
}

Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine("✨ Observability Demo Complete!");
Console.WriteLine("=".PadRight(60, '='));

await host.StopAsync();
