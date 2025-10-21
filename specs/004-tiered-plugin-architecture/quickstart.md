# Quickstart: Building and Hosting a Plugin

## 1) Create a Plugin (net8.0)

```csharp
using LablabBean.Plugins.Contracts;

namespace LablabBean.Plugins.DungeonGame;

public sealed class DungeonGamePlugin : IPlugin
{
    public string Id => "lablab.dungeongame";
    public string Name => "Dungeon Game";
    public string Version => "0.1.0";

    private ILogger _logger = null!;

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing DungeonGame plugin...");

        // Register services via Registry (cross-ALC, priority-based)
        var gameLoop = new GameLoop(_logger);
        context.Registry.Register<IGameLoop>(gameLoop, priority: 100);

        _logger.LogInformation("DungeonGame plugin initialized");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting DungeonGame plugin...");
        // Start background work here if needed
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping DungeonGame plugin...");
        // Cleanup resources
        return Task.CompletedTask;
    }
}
```

Manifest: `plugin.json`
```json
{
  "id": "lablab.dungeongame",
  "name": "Dungeon Game",
  "version": "0.1.0",
  "entryAssembly": "LablabBean.Plugins.DungeonGame.dll",
  "entryType": "LablabBean.Plugins.DungeonGame.DungeonGamePlugin",
  "dependencies": []
}
```

## 2) Place Plugin
- Build plugin to `./plugins/LablabBean.Plugins.DungeonGame/bin/Debug/net8.0/` (or copy to `./build/plugins`)
- Ensure `plugin.json` is alongside the DLL.

## 3) Integrate Loader in Host (net8.0)
- Console host (`dotnet/console-app/LablabBean.Console`):
  - Add a hosted service that scans `plugins/`, parses manifests, loads assemblies via ALC, and starts plugins.
  - Configure via `appsettings.json`:
```json
{
  "plugins": {
    "paths": [ "./plugins", "%PROGRAMDATA%/lablab/plugins" ],
    "hotReload": true
  }
}
```

## 4) Run
- Task: `task dotnet-run-console`
- Verify logs show discovery and startup of `lablab.dungeongame`.

## 5) Hot Reload (optional)
- Replace plugin DLL; the hosted service detects changes and reloads.

## References
- Winged Bean plugin docs: `ref-projects/winged-bean/docs/*`
- PluginManoi loader/registry tests: `ref-projects/plugin-manoi/dotnet/framework/tests/*`
