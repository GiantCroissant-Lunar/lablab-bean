---
doc_id: DOC-2025-00037
title: Plugin Development Quick-Start Guide
doc_type: guide
status: draft
canonical: false
created: 2025-10-21
tags: [plugins, development, quickstart, tutorial]
summary: >
  5-minute guide to creating a new plugin for Lablab Bean using the plugin system
source:
  author: agent
  agent: claude
  model: sonnet-4.5
---

# Plugin Development Quick-Start Guide

Create a new Lablab Bean plugin in 5 minutes.

## Prerequisites

- .NET 8 SDK installed
- Basic C# knowledge
- Lablab Bean codebase cloned

## Step 1: Create Plugin Project (1 minute)

```bash
cd dotnet/plugins
dotnet new classlib -n LablabBean.Plugins.YourFeature -f net8.0
cd LablabBean.Plugins.YourFeature
```

Edit `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="plugin.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

**Key settings:**
- `EnableDynamicLoading` - Required for AssemblyLoadContext isolation
- Reference only `LablabBean.Plugins.Contracts` - Keep dependencies minimal
- Copy `plugin.json` to output directory

## Step 2: Create Plugin Manifest (1 minute)

Create `plugin.json` in your plugin directory:

```json
{
  "id": "your-feature",
  "name": "Your Feature Plugin",
  "version": "1.0.0",
  "description": "Brief description of what your plugin does",
  "author": "Your Name",
  "entryPoint": {
    "dotnet.console": "LablabBean.Plugins.YourFeature.dll,LablabBean.Plugins.YourFeature.YourFeaturePlugin"
  },
  "dependencies": [],
  "capabilities": ["gameplay"],
  "priority": 100
}
```

**Field reference:**
- `id` - Unique lowercase identifier (kebab-case recommended)
- `name` - Human-readable display name
- `version` - Semantic version (MAJOR.MINOR.PATCH)
- `entryPoint` - Assembly and fully-qualified class name
  - `dotnet.console` - Profile for Terminal.Gui console app
  - `dotnet.sadconsole` - Profile for SadConsole UI (optional)
- `dependencies` - Array of required plugin IDs (empty if none)
- `capabilities` - Tags for plugin categorization
- `priority` - Load order (higher = loaded later, default 100)

## Step 3: Implement IPlugin Interface (2 minutes)

Create `YourFeaturePlugin.cs`:

```csharp
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.YourFeature;

public class YourFeaturePlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginHost? _host;

    // Plugin metadata (must match plugin.json)
    public string Id => "your-feature";
    public string Name => "Your Feature Plugin";
    public string Version => "1.0.0";

    // Called once during plugin load
    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _host = context.Host;

        _logger.LogInformation("Initializing {PluginName} v{Version}", Name, Version);

        // Register services with the host
        // Example: context.Registry.Register<IYourService>(new YourService(), priority: 100);

        return Task.CompletedTask;
    }

    // Called after all plugins initialized (start background work)
    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("{PluginName} started", Name);

        // Start hosted services, timers, event subscriptions, etc.

        return Task.CompletedTask;
    }

    // Called during shutdown (cleanup resources)
    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("{PluginName} stopping", Name);

        // Unsubscribe events, dispose resources, etc.

        return Task.CompletedTask;
    }
}
```

## Step 4: Build and Deploy (1 minute)

```bash
# Build plugin
dotnet build

# Copy to plugin directory (manual deployment)
cp -r bin/Debug/net8.0/* ../../../plugins/your-feature/

# Or use the deployment script (if available)
# pwsh ../../../scripts/deploy-demo-plugin.ps1 -PluginName your-feature
```

The plugin loader will automatically discover your plugin if `plugin.json` is present.

## Step 5: Verify Plugin Loads

Run the console application:

```bash
cd ../../../
npm run console
```

Check logs for:
```
[Information] Initializing Your Feature Plugin v1.0.0
[Information] Your Feature Plugin started
```

## Common Patterns

### Pattern 1: Register a Service

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    var myService = new MyService(context.Logger);

    // Register with priority (higher = preferred if multiple implementations)
    context.Registry.Register<IMyService>(myService, priority: 100);

    return Task.CompletedTask;
}
```

**Priority guidelines:**
- Framework services: 1000+
- Game plugins: 100-500
- UI plugins: 50-99
- Default: 100

### Pattern 2: Consume Host Services

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    // Access host configuration
    var maxItems = context.Configuration.GetValue<int>("YourFeature:MaxItems");

    // Get logger
    var logger = context.Logger; // Pre-configured with plugin ID category

    // Access service provider for Microsoft.Extensions services
    var hostService = context.Host.Services.GetService<ISomeHostService>();

    return Task.CompletedTask;
}
```

### Pattern 3: Publish Events

```csharp
public Task StartAsync(CancellationToken ct)
{
    // Publish event to host event bus
    _host?.PublishEvent(new YourCustomEvent
    {
        Message = "Something happened",
        Timestamp = DateTime.UtcNow
    });

    return Task.CompletedTask;
}
```

### Pattern 4: Plugin Dependencies

If your plugin depends on another plugin, update `plugin.json`:

```json
{
  "id": "advanced-feature",
  "dependencies": [
    {
      "id": "inventory",
      "versionRange": ">=1.0.0 <2.0.0",
      "optional": false
    }
  ]
}
```

Plugins load in dependency order automatically.

### Pattern 5: Background Work

```csharp
private Timer? _timer;

public Task StartAsync(CancellationToken ct)
{
    // Start periodic background work
    _timer = new Timer(OnTimerTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

    return Task.CompletedTask;
}

private void OnTimerTick(object? state)
{
    _logger?.LogDebug("Background task running");
    // Do periodic work
}

public Task StopAsync(CancellationToken ct)
{
    // Clean up
    _timer?.Dispose();
    return Task.CompletedTask;
}
```

## Project Structure

```
dotnet/plugins/LablabBean.Plugins.YourFeature/
├── YourFeaturePlugin.cs          # IPlugin implementation
├── plugin.json                   # Manifest (required)
├── LablabBean.Plugins.YourFeature.csproj
└── Services/                     # Optional service implementations
    ├── IYourService.cs
    └── YourService.cs
```

## Configuration

Add plugin-specific configuration to `appsettings.json`:

```json
{
  "YourFeature": {
    "MaxItems": 100,
    "EnableDebug": true
  }
}
```

Access via `context.Configuration.GetSection("YourFeature")`.

## Debugging

### Enable plugin-specific logging

In `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "LablabBean.Plugins.YourFeature": "Debug"
      }
    }
  }
}
```

### Attach debugger to plugin

1. Build plugin in Debug configuration
2. Set breakpoints in plugin code
3. Attach debugger to console app process
4. Breakpoints hit when plugin loads

## Testing

See `dotnet/examples/PluginTestHarness/` for plugin testing patterns.

## Next Steps

- Read [Plugin Contracts API Reference](./2025-10-21-plugin-contracts-api--DOC-2025-00038.md) for detailed interface documentation
- Review [Plugin Manifest Schema](./2025-10-21-plugin-manifest-schema--DOC-2025-00039.md) for complete manifest options
- Examine `dotnet/examples/LablabBean.Plugin.Demo/` for working example
- See `dotnet/plugins/LablabBean.Plugins.Inventory/` for production example

## Troubleshooting

**Plugin not discovered:**
- Ensure `plugin.json` exists in output directory
- Check `Plugins:Paths` in `appsettings.json` includes your plugin directory
- Verify `entryPoint` assembly and class name are correct

**Type load errors:**
- Ensure `EnableDynamicLoading=true` in `.csproj`
- Only reference `LablabBean.Plugins.Contracts` (not Core assemblies)
- Check that shared types are defined in Contracts project

**Service registration fails:**
- Verify service interface is public and shared across assemblies
- Check priority conflicts (use unique priorities for critical services)
- Ensure service is registered in `InitializeAsync`, not constructor

## Related Documentation

- **Architecture**: See `specs/004-tiered-plugin-architecture/spec.md`
- **Contracts**: See `dotnet/framework/LablabBean.Plugins.Contracts/`
- **Examples**: See `dotnet/examples/` directory

---

**Version**: 1.0.0
**Created**: 2025-10-21
**Author**: Claude (Sonnet 4.5)
