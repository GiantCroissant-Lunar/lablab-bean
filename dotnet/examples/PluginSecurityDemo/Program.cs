using LablabBean.Plugins.Core;
using LablabBean.Plugins.Core.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("🔒 Plugin System Security Demo\n");
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

// Start the host
await host.StartAsync();

Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine("🔒 SECURITY & SANDBOXING DEMONSTRATION");
Console.WriteLine("=".PadRight(60, '=') + "\n");

// Get security services
var securityManager = host.Services.GetRequiredService<PluginSecurityManager>();
var auditLog = host.Services.GetRequiredService<SecurityAuditLog>();

// 1. Create security profiles for demo plugins
Console.WriteLine("📋 1. SECURITY PROFILES");
Console.WriteLine("-".PadRight(60, '-'));

var standardProfile = new PluginSecurityProfile
{
    PluginId = "demo-plugin",
    GrantedPermissions = PluginPermission.Standard,
    IsSandboxed = true,
    TrustLevel = "Standard",
    ResourceLimits = new ResourceLimits
    {
        MaxMemoryBytes = 50 * 1024 * 1024, // 50 MB
        MaxThreads = 5,
        MaxExecutionTime = TimeSpan.FromMinutes(2)
    }
};

securityManager.RegisterProfile(standardProfile);
Console.WriteLine($"✅ Registered profile for: {standardProfile.PluginId}");
Console.WriteLine($"   Trust Level: {standardProfile.TrustLevel}");
Console.WriteLine($"   Sandboxed: {standardProfile.IsSandboxed}");
Console.WriteLine($"   Permissions: {standardProfile.GrantedPermissions}");
Console.WriteLine($"   Max Memory: {standardProfile.ResourceLimits.MaxMemoryBytes / 1024.0 / 1024.0:F0} MB");
Console.WriteLine($"   Max Threads: {standardProfile.ResourceLimits.MaxThreads}");
Console.WriteLine();

// 2. Test permission checks
Console.WriteLine("🔑 2. PERMISSION CHECKS");
Console.WriteLine("-".PadRight(60, '-'));

var permissions = new[]
{
    PluginPermission.FileSystemRead,
    PluginPermission.FileSystemWrite,
    PluginPermission.FileSystemDelete,
    PluginPermission.NetworkAccess,
    PluginPermission.ProcessCreate,
    PluginPermission.UnsafeCode
};

foreach (var permission in permissions)
{
    var check = securityManager.CheckPermission("demo-plugin", permission);
    var emoji = check.IsAllowed ? "✅" : "❌";
    Console.WriteLine($"{emoji} {permission}: {(check.IsAllowed ? "ALLOWED" : "DENIED")}");
}
Console.WriteLine();

// 3. Simulate resource usage tracking
Console.WriteLine("📊 3. RESOURCE USAGE TRACKING");
Console.WriteLine("-".PadRight(60, '-'));

// Simulate some resource usage
securityManager.RecordResourceUsage("demo-plugin", usage =>
{
    usage.MemoryUsage = 25 * 1024 * 1024; // 25 MB
    usage.ThreadCount = 3;
    usage.FileHandles = 15;
    usage.NetworkConnections = 2;
    usage.TotalExecutionTime = TimeSpan.FromSeconds(30);
});

var resourceUsage = securityManager.GetResourceUsage("demo-plugin");
if (resourceUsage != null)
{
    Console.WriteLine($"Memory: {resourceUsage.MemoryUsage / 1024.0 / 1024.0:F2} MB");
    Console.WriteLine($"Threads: {resourceUsage.ThreadCount}");
    Console.WriteLine($"File Handles: {resourceUsage.FileHandles}");
    Console.WriteLine($"Network Connections: {resourceUsage.NetworkConnections}");
    Console.WriteLine($"Execution Time: {resourceUsage.TotalExecutionTime.TotalSeconds:F1}s");
}
Console.WriteLine();

// 4. Check resource limits
Console.WriteLine("⚖️ 4. RESOURCE LIMIT CHECKS");
Console.WriteLine("-".PadRight(60, '-'));

var limitCheck = securityManager.CheckResourceLimits("demo-plugin");
Console.WriteLine($"Within Limits: {(limitCheck.IsWithinLimits ? "✅ YES" : "❌ NO")}");
if (limitCheck.Violations.Any())
{
    Console.WriteLine("Violations:");
    foreach (var violation in limitCheck.Violations)
    {
        Console.WriteLine($"  ⚠️  {violation}");
    }
}
else
{
    Console.WriteLine("No violations detected");
}
Console.WriteLine();

// 5. Demonstrate permission management
Console.WriteLine("🔧 5. PERMISSION MANAGEMENT");
Console.WriteLine("-".PadRight(60, '-'));

// Grant a new permission
Console.WriteLine("Granting NetworkListen permission...");
securityManager.GrantPermission("demo-plugin", PluginPermission.NetworkListen);
auditLog.LogPermissionGranted("demo-plugin", PluginPermission.NetworkListen);

var newCheck = securityManager.CheckPermission("demo-plugin", PluginPermission.NetworkListen);
Console.WriteLine($"NetworkListen now: {(newCheck.IsAllowed ? "✅ ALLOWED" : "❌ DENIED")}");
Console.WriteLine();

// Revoke a permission
Console.WriteLine("Revoking FileSystemWrite permission...");
securityManager.RevokePermission("demo-plugin", PluginPermission.FileSystemWrite);

newCheck = securityManager.CheckPermission("demo-plugin", PluginPermission.FileSystemWrite);
Console.WriteLine($"FileSystemWrite now: {(newCheck.IsAllowed ? "✅ ALLOWED" : "❌ DENIED")}");
Console.WriteLine();

// 6. Security audit log
Console.WriteLine("📝 6. SECURITY AUDIT LOG");
Console.WriteLine("-".PadRight(60, '-'));

// Log some sample events
auditLog.LogPermissionDenied("demo-plugin", PluginPermission.UnsafeCode, "Untrusted plugin");
auditLog.LogSecurityViolation("demo-plugin", "Attempted to access restricted API", "Warning");

Console.WriteLine($"Total Events: {auditLog.EventCount}");
Console.WriteLine("\nRecent Events:");

var recentEvents = auditLog.GetAllEvents().TakeLast(5);
foreach (var evt in recentEvents)
{
    var emoji = evt.Severity switch
    {
        "Critical" => "🔴",
        "Error" => "❌",
        "Warning" => "⚠️",
        _ => "ℹ️"
    };
    Console.WriteLine($"{emoji} [{evt.Timestamp:HH:mm:ss}] {evt.EventType}: {evt.Description}");
}
Console.WriteLine();

// 7. Security summary
Console.WriteLine("📈 7. SECURITY SUMMARY");
Console.WriteLine("-".PadRight(60, '-'));

var allProfiles = securityManager.GetAllProfiles();
Console.WriteLine($"Total Plugins: {allProfiles.Count}");
Console.WriteLine($"Sandboxed: {allProfiles.Count(p => p.Value.IsSandboxed)}");
Console.WriteLine($"Audit Events: {auditLog.EventCount}");

var violations = auditLog.GetSecurityViolations();
Console.WriteLine($"Security Violations: {violations.Count}");

var deniedEvents = auditLog.GetEventsByType(SecurityAuditEventType.PermissionDenied);
Console.WriteLine($"Permission Denials: {deniedEvents.Count}");

Console.WriteLine("\n" + "=".PadRight(60, '='));
Console.WriteLine("✨ Security Demo Complete!");
Console.WriteLine("=".PadRight(60, '='));

await host.StopAsync();
