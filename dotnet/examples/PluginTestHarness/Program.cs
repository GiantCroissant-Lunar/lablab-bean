using LablabBean.Plugins.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add plugin system
builder.Services.AddPluginSystem(builder.Configuration);

var host = builder.Build();

Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         Lablab Bean Plugin System Test Harness               ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Starting plugin system...");
Console.WriteLine("Press Ctrl+C to stop");
Console.WriteLine();

await host.RunAsync();

