---
title: "Plugin System Quick Reference"
type: reference
status: draft
created: 2025-10-21
tags: [plugins, reference, quickstart]
---

# Plugin System Quick Reference

One-page reference for the Lablab Bean plugin system.

## 📦 For Plugin Developers

### Minimal Plugin

```csharp
using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;

public class MyPlugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "my-plugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Plugin stopped");
        return Task.CompletedTask;
    }
}
```

### Minimal Manifest (plugin.json)

```json
{
  "id": "my-plugin",
  "name": "My Plugin",
  "version": "1.0.0",
  "entryPoint": {
    "dotnet.console": "MyPlugin.dll,MyNamespace.MyPlugin"
  }
}
```

### Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\framework\LablabBean.Plugins.Contracts\..." />
  </ItemGroup>

  <ItemGroup>
    <None Update="plugin.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

### Build & Deploy

```bash
dotnet build --configuration Release
Copy-Item "bin/Release/net8.0/*" -Destination "../../plugins/my-plugin/" -Recurse
```

## 🖥️ For Host Integrators

### Add to Any Host

```csharp
// 1. Add reference in .csproj
<ProjectReference Include="..\..\framework\LablabBean.Plugins.Core\..." />

// 2. Add using
using LablabBean.Plugins.Core;

// 3. Register in ConfigureServices
services.AddPluginSystem(configuration);

// 4. Configure in appsettings.json
{
  "Plugins": {
    "Paths": ["plugins"],
    "Profile": "dotnet.console",
    "HotReload": false
  },
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "LablabBean.Plugins": "Information"
      }
    }
  }
}
```

### Generic Host (Automatic)

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddPluginSystem(context.Configuration);
        // Other services...
    })
    .Build();

await host.RunAsync(); // Plugins start/stop automatically
```

### Manual DI (Explicit)

```csharp
var services = new ServiceCollection();
services.AddPluginSystem(configuration);
var provider = services.BuildServiceProvider();

// Start
var pluginService = provider.GetService<IHostedService>();
await pluginService?.StartAsync(CancellationToken.None);

// Your app logic...

// Stop
await pluginService?.StopAsync(CancellationToken.None);
```

## 📁 Directory Structure

```
your-project/
├── plugins/
│   └── my-plugin/
│       ├── MyPlugin.dll
│       ├── plugin.json
│       └── *.deps.json
├── dotnet/
│   ├── console-app/              # Host with plugins
│   ├── examples/
│   │   └── MyPlugin/             # Plugin source
│   └── framework/
│       ├── LablabBean.Plugins.Contracts/  # Interfaces
│       └── LablabBean.Plugins.Core/       # Loader
```

## 🔧 Configuration

### appsettings.json

```json
{
  "Plugins": {
    "Paths": [
      "plugins",              // Relative to executable
      "../../../plugins"      // Project root (for dev builds)
    ],
    "Profile": "dotnet.console",  // Or "dotnet.sadconsole"
    "HotReload": false             // Enable for development
  }
}
```

### Manifest Schema

```json
{
  "id": "plugin-id",           // Required: Unique identifier
  "name": "Plugin Name",       // Required: Display name
  "version": "1.0.0",          // Required: Semantic version
  "description": "...",        // Optional
  "author": "...",             // Optional
  "entryPoint": {              // Required: Profile → Entry mapping
    "dotnet.console": "Assembly.dll,Namespace.Type",
    "dotnet.sadconsole": "Assembly.dll,Namespace.Type"
  },
  "dependencies": [            // Optional: Plugin dependencies
    {
      "id": "other-plugin",
      "versionRange": ">=1.0.0 <2.0.0",
      "optional": false
    }
  ],
  "capabilities": [],          // Optional: Feature tags
  "priority": 100              // Optional: Load order (default: 100)
}
```

## 🎯 Context API

### IPluginContext

```csharp
public interface IPluginContext
{
    IRegistry Registry { get; }      // Service registration
    IConfiguration Configuration { get; } // Read-only config
    ILogger Logger { get; }          // Logger (category = plugin ID)
    IPluginHost Host { get; }        // Host services
}
```

### Common Usage

```csharp
public Task InitializeAsync(IPluginContext context, CancellationToken ct)
{
    // Get logger
    _logger = context.Logger;

    // Read config
    var setting = context.Configuration["MyPlugin:Setting"];

    // Register service (future)
    context.Registry.Register<IMyService, MyServiceImpl>(priority: 100);

    // Access host (future)
    var eventBus = context.Host.EventBus;

    return Task.CompletedTask;
}
```

## 🐛 Debugging

### Enable Debug Logging

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "LablabBean.Plugins": "Debug",
        "my-plugin": "Debug"
      }
    }
  }
}
```

### Common Issues

| Issue | Check |
|-------|-------|
| Plugin not discovered | ✅ `plugin.json` in plugin directory<br>✅ Plugin dir under configured path<br>✅ Valid JSON syntax |
| Plugin not loaded | ✅ Entry point format: `"Assembly.dll,Namespace.Type"`<br>✅ Profile matches host configuration<br>✅ Assembly file exists |
| Type cast error | ✅ `EnableDynamicLoading=true` in .csproj<br>✅ Don't copy Contracts DLL to plugin output<br>✅ Reference Contracts, don't embed |

## 📊 Lifecycle

```
Discovery → Validation → Loading → Initialize → Start
                                      ↓
                                   Running
                                      ↓
                                    Stop → Unload
```

### States

| State | Description |
|-------|-------------|
| **Discovered** | Manifest found and parsed |
| **Validated** | Dependencies resolved |
| **Loaded** | Assembly loaded in ALC |
| **Initialized** | InitializeAsync() completed |
| **Started** | StartAsync() completed |
| **Stopped** | StopAsync() completed |
| **Unloaded** | ALC unloaded (if collectible) |

## ⚡ Performance

| Metric | Typical Value |
|--------|---------------|
| Load time | 10-20ms per plugin |
| Memory overhead | ~1-2MB per plugin ALC |
| Discovery time | <1ms per directory |
| Manifest parse | <1ms per file |

## 🔒 Security

### Best Practices

- ✅ Validate plugin signatures (future feature)
- ✅ Run plugins in isolated ALCs
- ✅ Use principle of least privilege
- ✅ Audit plugin dependencies
- ❌ Don't load plugins from untrusted sources
- ❌ Don't grant plugins unrestricted host access

## 📚 Documentation

- **Phase 5 Report**: `PLUGIN_SYSTEM_PHASE5_COMPLETE.md`
- **Phase 3 Report**: `PLUGIN_SYSTEM_PHASE3_COMPLETE.md`
- **Quickstart Guide**: `PLUGIN_DEVELOPMENT_QUICKSTART.md`
- **Demo Plugin**: `dotnet/examples/LablabBean.Plugin.Demo/README.md`

## 🚀 Examples

### Demo Plugin

See: `dotnet/examples/LablabBean.Plugin.Demo/`

A complete working example demonstrating:

- Basic IPlugin implementation
- Context usage (logger, config, registry, host)
- Multi-profile support
- Proper manifest format

### Test Harness

See: `dotnet/examples/PluginTestHarness/`

Standalone console app for testing plugins without full game host.

---

**Need Help?**

- Check logs with Debug level enabled
- Compare to demo plugin
- Review Phase 5 completion report for troubleshooting

**Version**: 1.0.0 | **Last Updated**: 2025-10-21
