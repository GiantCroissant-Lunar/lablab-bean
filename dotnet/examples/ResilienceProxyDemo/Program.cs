using LablabBean.Contracts.Resilience.Extensions;
using LablabBean.Contracts.Resilience.Services;
using LablabBean.Plugins.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Demo: Source-generated DI registration for Resilience Proxy
// This demonstrates the auto-generated AddResilienceServiceProxy() method

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Step 1: Add plugin system (registers IRegistry)
        services.AddPluginSystem(context.Configuration);
        
        // Step 2: Add resilience proxy (auto-generated method!)
        services.AddResilienceServiceProxy();
        
        Console.WriteLine("✅ Resilience proxy registered via source-generated extension method");
    })
    .Build();

// Verify the proxy is registered
var resilienceService = host.Services.GetRequiredService<IService>();
Console.WriteLine($"✅ IService resolved: {resilienceService.GetType().Name}");
Console.WriteLine($"   Namespace: {resilienceService.GetType().Namespace}");

// Wait for plugins to load
await host.StartAsync();
await Task.Delay(2000);

// Test the service (will throw if no plugin registered yet)
try
{
    var healthInfo = resilienceService.GetHealthStatus();
    Console.WriteLine($"✅ Service call succeeded - Healthy: {healthInfo.IsHealthy}");
    Console.WriteLine($"   Circuit Breakers: {healthInfo.TotalCircuitBreakers}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"⚠️  Expected exception (no plugin loaded yet): {ex.Message}");
    Console.WriteLine("   This is correct behavior when plugin hasn't registered implementation");
}

await host.StopAsync();
Console.WriteLine("\n✅ Demo complete - source-generated DI registration working!");
