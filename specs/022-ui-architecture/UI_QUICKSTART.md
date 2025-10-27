# UI Architecture Quickstart Guide

**Version**: 1.0
**Status**: Complete
**Last Updated**: 2025-10-27

## Overview

This guide helps you get started with the pluggable UI architecture that supports both Terminal (console) and Windows (SadConsole) interfaces through a unified plugin system.

## Architecture Overview

```
Application Host (Console/Windows)
    ↓
Plugin Loader + Capability Validator
    ↓
[UI Plugin] ←→ [Renderer Plugin]
    ↓
Game Core (ECS + Contracts)
```

**Key Principle**: Only **ONE UI** and **ONE Renderer** active at runtime.

## Quick Start: Console App

### 1. Check Plugin Directory Structure

```
dotnet/console-app/LablabBean.Console/bin/Debug/net8.0/
└── plugins/
    ├── rendering-terminal/
    │   ├── plugin.json
    │   └── LablabBean.Plugins.Rendering.Terminal.dll
    └── ui-terminal/
        ├── plugin.json
        └── LablabBean.Plugins.UI.Terminal.dll
```

### 2. Run Console App

```powershell
cd dotnet/console-app/LablabBean.Console
dotnet run
```

**Expected Output**:

```
[INF] Loading plugin: rendering-terminal
[INF] Loading plugin: ui-terminal
[INF] Selected single ui plugin: ui-terminal
[INF] Selected single renderer plugin: rendering-terminal
[INF] Terminal UI initialized
```

### 3. Verify UI is Active

The Terminal.Gui interface should appear with:

- **HUD** (top): Player stats, health, inventory count
- **World View** (center): Dungeon map with FOV
- **Activity Log** (bottom): Game events

## Quick Start: Windows App

### 1. Configure for SadConsole

**appsettings.json**:

```json
{
  "Plugins": {
    "SearchPaths": ["plugins"],
    "PreferredUI": "ui-sadconsole",
    "PreferredRenderer": "rendering-sadconsole"
  }
}
```

### 2. Deploy Plugins

```powershell
./scripts/deploy-plugins-for-windows.ps1
```

### 3. Run Windows App

```powershell
cd dotnet/windows-app/LablabBean.Windows
dotnet run
```

**Expected**: SadConsole window opens with tile-based graphics.

## Configuration

### appsettings.json Options

```json
{
  "Plugins": {
    "SearchPaths": ["plugins"],
    "PreferredUI": null,
    "PreferredRenderer": null,
    "StrictCapabilityMode": true
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| `PreferredUI` | Force specific UI plugin | `null` (auto-select by priority) |
| `PreferredRenderer` | Force specific renderer | `null` (auto-select by priority) |
| `StrictCapabilityMode` | Error on conflicts vs. warn | `true` |

### Plugin Priority

If no preference is set, plugins are selected by **priority** (highest wins):

| Plugin | Priority | Capability Tags |
|--------|----------|-----------------|
| `ui-terminal` | 100 | `ui`, `ui:terminal` |
| `ui-sadconsole` | 100 | `ui`, `ui:windows` |
| `rendering-terminal` | 100 | `renderer`, `renderer:terminal` |
| `rendering-sadconsole` | 100 | `renderer`, `renderer:sadconsole` |

## Common Scenarios

### Scenario 1: Default Console App

**Config**: None (uses defaults)

**Result**:

```
✓ rendering-terminal loaded
✓ ui-terminal loaded
✗ rendering-sadconsole excluded (single renderer policy)
✗ ui-sadconsole excluded (single UI policy)
```

### Scenario 2: Force SadConsole in Console

**Config**:

```json
{
  "Plugins": {
    "PreferredUI": "ui-sadconsole",
    "PreferredRenderer": "rendering-sadconsole"
  }
}
```

**Result**: Console app uses SadConsole UI (if SadConsole libs are available).

### Scenario 3: Development/Testing

**Config**:

```json
{
  "Plugins": {
    "StrictCapabilityMode": false
  }
}
```

**Result**: Warnings instead of errors for multiple UI/renderer plugins.

## Troubleshooting

### Problem: "No UI service loaded"

**Symptoms**: App runs but no UI appears.

**Causes & Solutions**:

1. **Missing plugins**

   ```powershell
   # Deploy plugins
   ./scripts/deploy-plugins-for-test.ps1
   ```

2. **Plugin load failed**
   - Check logs for dependency errors
   - Verify `plugin.json` is valid JSON
   - Check assembly references

3. **Wrong profile**
   - Console app uses profile: `dotnet.console`
   - Windows app uses profile: `dotnet.windows`
   - Check `entryPoint` in `plugin.json`

### Problem: Multiple UI plugins conflict

**Symptoms**: Error "Only one ui plugin allowed"

**Solutions**:

1. **Set preferred plugin**:

   ```json
   { "Plugins": { "PreferredUI": "ui-terminal" } }
   ```

2. **Remove unwanted plugins**:

   ```powershell
   Remove-Item -Recurse plugins/ui-sadconsole
   ```

3. **Disable strict mode** (not recommended for production):

   ```json
   { "Plugins": { "StrictCapabilityMode": false } }
   ```

### Problem: UI loads but doesn't render

**Cause**: UI service not properly initialized.

**Solution**: Ensure initialization order:

```csharp
// 1. Load plugins
await pluginLoader.DiscoverAndLoadAsync(pluginPaths);

// 2. Get UI service
var uiService = serviceRegistry.GetService<IService>();

// 3. Initialize UI
await uiService.InitializeAsync(new UIInitOptions
{
    ViewportWidth = 80,
    ViewportHeight = 24
});

// 4. Start rendering
await uiService.RenderViewportAsync(viewport, entities);
await uiService.UpdateDisplayAsync();
```

## API Reference

### Core Interfaces

**IService** (UI.Contracts.Services):

```csharp
Task InitializeAsync(UIInitOptions options, CancellationToken ct = default);
Task RenderViewportAsync(ViewportBounds viewport, IReadOnlyCollection<EntitySnapshot> entities);
Task UpdateDisplayAsync();
Task HandleInputAsync(InputCommand command);
ViewportBounds GetViewport();
void SetViewportCenter(Position centerPosition);
```

**IDungeonCrawlerUI** (Game.UI.Contracts):

```csharp
Task ShowDialogueAsync(string npcName, string dialogue);
Task ShowQuestLogAsync();
Task ShowInventoryAsync();
Task ToggleHudVisibilityAsync();
void EnableCameraFollow(bool enabled);
```

**ISceneRenderer** (Rendering.Contracts):

```csharp
Task RenderSceneAsync(TileBuffer buffer);
Task ClearAsync();
Task FlushAsync();
```

### Plugin Manifest Reference

**Required Fields**:

- `id`: Unique plugin identifier
- `name`: Display name
- `version`: Semantic version
- `capabilities`: List of capability tags
- `entryPoint`: Profile-specific entry points

**Example**:

```json
{
  "id": "ui-terminal",
  "name": "Terminal UI",
  "version": "1.0.0",
  "description": "Terminal.Gui-based UI host.",
  "capabilities": ["ui", "ui:terminal"],
  "priority": 100,
  "dependencies": [
    {
      "id": "rendering-terminal",
      "optional": false
    }
  ],
  "entryPoint": {
    "dotnet.console": "LablabBean.Plugins.UI.Terminal.dll,LablabBean.Plugins.UI.Terminal.TerminalUiPlugin"
  }
}
```

## Testing Your Setup

### 1. Verify Plugin Discovery

```powershell
dotnet run -- --list-plugins
```

**Expected Output**:

```
Available Plugins:
  ✓ rendering-terminal (v1.0.0) - renderer, renderer:terminal
  ✓ ui-terminal (v1.0.0) - ui, ui:terminal
  ✗ rendering-sadconsole (excluded: single renderer policy)
  ✗ ui-sadconsole (excluded: single UI policy)
```

### 2. Test UI Initialization

```csharp
// In your test
var uiService = serviceRegistry.GetService<IService>();
Assert.NotNull(uiService);

await uiService.InitializeAsync(new UIInitOptions
{
    ViewportWidth = 80,
    ViewportHeight = 24
});

var viewport = uiService.GetViewport();
Assert.Equal(80, viewport.Width);
Assert.Equal(24, viewport.Height);
```

### 3. Test Rendering

```csharp
var entities = new List<EntitySnapshot>
{
    new EntitySnapshot { Position = new Position(10, 10), Glyph = '@' }
};

await uiService.RenderViewportAsync(viewport, entities);
await uiService.UpdateDisplayAsync();

// UI should display the entity
```

## Next Steps

1. **Customize UI**: Modify Terminal.Gui views in `LablabBean.Game.TerminalUI`
2. **Add Custom Plugins**: See [Creating UI Plugins Guide](./creating-ui-plugins.md)
3. **Integrate with Game Loop**: See [Game Integration Guide](./game-integration.md)
4. **Performance Tuning**: See [Performance Guide](./performance.md)

## Additional Resources

- **Spec 022**: Full architecture specification
- **Phase Reports**: Implementation history and decisions
- **API Docs**: Generated API documentation
- **Sample Code**: See `samples/ui-examples/`

---

**Version**: 1.0.0
**Last Updated**: 2025-10-27
**Status**: Production Ready
