# Demo Plugin

A simple demonstration plugin for the Lablab Bean plugin system.

## Purpose

This plugin validates that the plugin system works correctly by demonstrating:

- ✅ Plugin discovery via `plugin.json` manifest
- ✅ Assembly loading in isolated AssemblyLoadContext (ALC)
- ✅ Dependency resolution and loading
- ✅ Plugin lifecycle: `InitializeAsync` → `StartAsync` → `StopAsync`
- ✅ Context-based initialization with logger, configuration, and registry
- ✅ Multi-profile support (console and SadConsole)

## Structure

```
LablabBean.Plugin.Demo/
├── DemoPlugin.cs              # Main plugin implementation
├── plugin.json                # Plugin manifest
├── LablabBean.Plugin.Demo.csproj
└── README.md                  # This file
```

## Plugin Manifest

The `plugin.json` manifest defines plugin metadata and entry points:

```json
{
  "id": "demo-plugin",
  "name": "Demo Plugin",
  "version": "1.0.0",
  "description": "A simple demo plugin to validate the plugin system",
  "author": "Lablab Bean Team",
  "profiles": {
    "dotnet.console": {
      "entryAssembly": "LablabBean.Plugin.Demo.dll",
      "entryType": "LablabBean.Plugin.Demo.DemoPlugin"
    },
    "dotnet.sadconsole": {
      "entryAssembly": "LablabBean.Plugin.Demo.dll",
      "entryType": "LablabBean.Plugin.Demo.DemoPlugin"
    }
  },
  "dependencies": [],
  "tags": ["demo", "example", "test"]
}
```

## Building

```bash
# Build the plugin
dotnet build --configuration Release

# The output will be in:
# bin/Release/net8.0/
```

## Deployment

```bash
# Copy to plugins directory
.\scripts\deploy-demo-plugin.ps1

# Or manually:
$dest = ".\plugins\demo-plugin"
mkdir -Force $dest
Copy-Item ".\dotnet\examples\LablabBean.Plugin.Demo\bin\Release\net8.0\*" -Destination $dest -Recurse -Force
```

## Expected Output

When loaded, the plugin logs:

```
[Information] DemoPlugin initialized
[Information] Plugin ID: demo-plugin, Name: Demo Plugin, Version: 1.0.0
[Information] Configuration available: True
[Information] Registry available: True
[Information] DemoPlugin started
[Information] This is a simple test plugin demonstrating:
[Information]   ✓ Plugin discovery and loading
[Information]   ✓ Context initialization with logger, config, and registry
[Information]   ✓ Lifecycle management (Initialize → Start → Stop)
[Information]   ✓ Host communication via IPluginHost
[Information]   ✓ AssemblyLoadContext isolation
```

## Testing

To test the plugin system with this demo plugin:

1. Build the plugin (see Building section above)
2. Deploy to `plugins/demo-plugin/` directory
3. Configure the host application's `appsettings.json`:

   ```json
   {
     "Plugins": {
       "Paths": ["plugins"],
       "Profile": "dotnet.console"
     }
   }
   ```

4. Run the console or Windows host application
5. Check logs for plugin initialization messages

## Implementation Details

### DemoPlugin Class

The plugin implements `IPlugin` interface with:

- **Properties**: `Id`, `Name`, `Version` for identification
- **InitializeAsync**: Receives `IPluginContext` with logger, config, registry, and host
- **StartAsync**: Performs startup tasks and logs demonstration messages
- **StopAsync**: Gracefully shuts down (no cleanup needed for this simple demo)

### Dependencies

The plugin depends on:

- `LablabBean.Plugins.Contracts` (netstandard2.1) - Core plugin interfaces
- Microsoft.Extensions.Logging.Abstractions - For ILogger

## Next Steps

After validating this demo plugin works:

1. **Phase 3**: Integrate into Console and Windows hosts
2. **Phase 4**: Add observability and metrics
3. **Phase 6**: Document plugin development guide
4. Create more complex example plugins demonstrating:
   - Service registration via `IRegistry`
   - Host event subscriptions
   - Inter-plugin dependencies
   - Hot reload scenarios
