---
doc_id: DOC-2025-00074
title: "Plugin Development Quickstart"
doc_type: guide
status: draft
canonical: true
created: 2025-10-21
tags: [plugins, guide, quickstart, development]
summary: Quick guide to creating plugins for Lablab Bean with step-by-step instructions
---

# Plugin Development Quickstart

Quick guide to creating plugins for Lablab Bean.

## Prerequisites

- .NET 8 SDK
- Understanding of C# and async/await
- Familiarity with dependency injection

## Step 1: Create Plugin Project

```bash
# Create new class library
dotnet new classlib -n MyPlugin -f net8.0

# Add reference to contracts
cd MyPlugin
dotnet add reference ../../framework/LablabBean.Plugins.Contracts/LablabBean.Plugins.Contracts.csproj
```

**Update .csproj**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <Nullable>enable</Nullable>
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

## Step 2: Implement IPlugin

```csharp
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

namespace MyPlugin;

public class MyPlugin : IPlugin
{
    private ILogger? _logger;
    private IPluginHost? _host;

    public string Id => "my-plugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _host = context.Host;

        _logger.LogInformation("MyPlugin initialized");

        // Register services if needed
        // context.Registry.Register<IMyService, MyService>(priority: 100);

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("MyPlugin started");

        // Start background work, subscribe to events, etc.

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("MyPlugin stopping");

        // Clean up resources

        return Task.CompletedTask;
    }
}
```

## Step 3: Create plugin.json

```json
{
  "id": "my-plugin",
  "name": "My Plugin",
  "version": "1.0.0",
  "description": "A brief description of what this plugin does",
  "author": "Your Name",
  "entryPoint": {
    "dotnet.console": "MyPlugin.dll,MyPlugin.MyPlugin",
    "dotnet.sadconsole": "MyPlugin.dll,MyPlugin.MyPlugin"
  },
  "dependencies": [],
  "capabilities": ["my-feature"],
  "priority": 100
}
```

### Entry Point Format

The `entryPoint` dictionary maps profile names to entry points:

```
"profile-name": "AssemblyName.dll,Namespace.TypeName"
```

**Profiles**:

- `dotnet.console` - Console host (Terminal.Gui)
- `dotnet.sadconsole` - SadConsole host
- `unity` - Unity engine (future)

### Manifest Schema

| Field | Required | Description |
|-------|----------|-------------|
| `id` | ✅ | Unique plugin identifier (kebab-case) |
| `name` | ✅ | Human-readable name |
| `version` | ✅ | Semantic version (e.g., "1.0.0") |
| `description` | ❌ | Brief description |
| `author` | ❌ | Plugin author |
| `entryPoint` | ✅* | Profile-to-entry-point mapping |
| `entryAssembly` | ✅* | Legacy: Assembly name (if no entryPoint) |
| `entryType` | ✅* | Legacy: Type name (if no entryPoint) |
| `dependencies` | ❌ | Plugin dependencies (see below) |
| `capabilities` | ❌ | Features exposed by plugin |
| `priority` | ❌ | Load order priority (default: 100) |

\* Either `entryPoint` dictionary OR both `entryAssembly` and `entryType` required.

### Dependencies

```json
{
  "dependencies": [
    {
      "id": "other-plugin",
      "versionRange": ">=1.0.0 <2.0.0",
      "optional": false
    }
  ]
}
```

## Step 4: Build and Deploy

```bash
# Build
dotnet build --configuration Release

# Deploy manually
$dest = "../../plugins/my-plugin"
mkdir -Force $dest
Copy-Item "bin/Release/net8.0/*" -Destination $dest -Recurse -Force
```

**Or create a deployment script** (see `scripts/deploy-demo-plugin.ps1` as template).

## Step 5: Test

### Option 1: Test Harness

```bash
# Add your plugin to plugins/my-plugin/
# Run test harness
dotnet run --project .\dotnet\examples\PluginTestHarness
```

### Option 2: Full Host

```bash
# Update appsettings.json
{
  "Plugins": {
    "Paths": ["plugins"],
    "Profile": "dotnet.console"
  }
}

# Run host
dotnet run --project .\dotnet\console-app\LablabBean.Console
```

## Plugin Context API

### IPluginContext

Provided during `InitializeAsync`:

```csharp
public interface IPluginContext
{
    IRegistry Registry { get; }           // Service registration
    IConfiguration Configuration { get; }  // Read-only config
    ILogger Logger { get; }               // Logger (category = plugin ID)
    IPluginHost Host { get; }             // Host services
}
```

### IRegistry

Register services across ALC boundaries:

```csharp
// Register service (runtime type matching)
context.Registry.Register<IMyService, MyServiceImpl>(priority: 100);

// Resolve service (not yet implemented in demo)
var service = context.Registry.Resolve<IMyService>();
```

### IPluginHost

Access host services (stub in current implementation):

```csharp
// Subscribe to events (future)
context.Host.EventBus.Subscribe<GameEvent>(OnGameEvent);

// Access host services (future)
var gameState = context.Host.GetService<IGameState>();
```

### Configuration

Read host configuration:

```csharp
var mySetting = context.Configuration["MyPlugin:Setting"];
var typedConfig = context.Configuration.GetSection("MyPlugin").Get<MyPluginConfig>();
```

## Common Patterns

### Service Registration

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    // Register singleton service
    context.Registry.Register<IInventorySystem, MyInventorySystem>(
        priority: 100,
        singleton: true
    );

    return Task.CompletedTask;
}
```

### Background Work

```csharp
private CancellationTokenSource? _cts;

public Task StartAsync(CancellationToken ct)
{
    _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    _ = Task.Run(() => BackgroundWorkAsync(_cts.Token), _cts.Token);
    return Task.CompletedTask;
}

public Task StopAsync(CancellationToken ct)
{
    _cts?.Cancel();
    return Task.CompletedTask;
}

private async Task BackgroundWorkAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        // Do work
        await Task.Delay(1000, ct);
    }
}
```

### Event Subscriptions

```csharp
public Task StartAsync(CancellationToken ct)
{
    // Subscribe to host events (future)
    _host?.EventBus?.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
    return Task.CompletedTask;
}

private void OnPlayerMoved(PlayerMovedEvent evt)
{
    _logger?.LogDebug("Player moved to {Position}", evt.NewPosition);
}
```

## Debugging

### Enable Debug Logging

**appsettings.json**:

```json
{
  "Logging": {
    "LogLevel": {
      "LablabBean.Plugins": "Debug",
      "your-plugin-id": "Debug"
    }
  }
}
```

### Common Issues

#### Plugin Not Discovered

- ✅ Check `plugin.json` is in plugin directory
- ✅ Check plugin directory is under configured plugin path
- ✅ Check manifest is valid JSON
- ✅ Check `id`, `name`, `version` are present

#### Plugin Not Loaded

- ✅ Check entry point format: `"Assembly.dll,Namespace.Type"`
- ✅ Check assembly and type names match exactly
- ✅ Check profile matches configured profile
- ✅ Check plugin implements `IPlugin` interface

#### Type Cast Exception

- ✅ Ensure `LablabBean.Plugins.Contracts` is referenced, not embedded
- ✅ Check `EnableDynamicLoading=true` in .csproj
- ✅ Don't copy contracts DLL to plugin directory (let it be shared)

## Best Practices

### ✅ DO

- Use structured logging with semantic properties
- Handle cancellation tokens properly
- Register services with appropriate priority
- Clean up resources in `StopAsync`
- Use nullable reference types
- Document your plugin's capabilities

### ❌ DON'T

- Block in lifecycle methods
- Catch and swallow exceptions silently
- Depend on plugin load order (use explicit dependencies)
- Embed `LablabBean.Plugins.Contracts` in plugin output
- Use hardcoded paths or configuration
- Assume plugin will never be unloaded

## Examples

See `dotnet/examples/LablabBean.Plugin.Demo` for a complete working example.

## Next Steps

- 📚 Read [Plugin System Architecture](#) (coming soon)
- 🔍 Explore [API Reference](#) (coming soon)
- 🧪 Check [Testing Guide](#) (coming soon)
- 💬 Join [Discord Community](#) for help

## Troubleshooting

For issues:

1. Check logs with `Debug` level enabled
2. Review [PLUGIN_SYSTEM_PHASE5_COMPLETE.md](PLUGIN_SYSTEM_PHASE5_COMPLETE.md) for known issues
3. Compare your plugin to the demo plugin
4. Open an issue on GitHub with full logs

---

**Happy Plugin Development! 🚀**
