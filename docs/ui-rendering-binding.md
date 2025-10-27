# UI Rendering Binding (Plugin-Only) and Rebind Helpers

Status: Adopted

Last Updated: 2025-10-27

Overview

- Binding = connecting a renderer to a concrete UI surface.
- We use plugin-only binding for both Terminal and SadConsole:
  - Terminal: renderer target is the inner map render view (Terminal.Gui View).
  - Windows (SadConsole): renderer target is GameScreen.WorldSurface (SadConsole ScreenSurface).

Why plugin-only

- Keeps adapters free of renderer-specific types.
- UI plugin already references both the adapter and renderer, making it the right place to orchestrate binding.
- Easier to rebind on lifecycle changes (layout/resize, screen re-create).

Terminal binding

- Adapter: exposes `GetWorldRenderView()` (inner map view) and does NOT bind.
- Plugin: binds and rebinds via layout events.
  - File: `dotnet/plugins/LablabBean.Plugins.UI.Terminal/TerminalUiPlugin.cs`
    - `RebindRendererTarget()` sets target on `TerminalSceneRenderer`.
    - Called once after window is added and on `LayoutComplete`.

SadConsole binding

- GameScreen: exposes explicit world surface.
  - File: `dotnet/windows-app/LablabBean.Game.SadConsole/Screens/GameScreen.cs` -> `WorldSurface`.
- Adapter: does NOT bind; builds TileBuffer and calls renderer.
- Plugin: binds and exposes a public rebind helper.
  - File: `dotnet/plugins/LablabBean.Plugins.UI.SadConsole/SadConsoleUiPlugin.cs`
    - `RebindRendererTarget()` binds `SadConsoleSceneRenderer` to `GameScreen.WorldSurface`.

Host-triggered rebinds

- A lightweight service is registered so hosts can trigger rebinds without holding plugin instances:
  - Interface: `dotnet/framework/LablabBean.Plugins.Contracts/IRenderTargetBinder.cs`
  - Registered by UI plugins with `UiId` and `Rebind()`.
- Example: rebind all UI render targets from a host or tool:

  ```csharp
  var registry = serviceProvider.GetRequiredService<LablabBean.Plugins.Contracts.IRegistry>();
  foreach (var binder in registry.GetAll<LablabBean.Plugins.Contracts.IRenderTargetBinder>())
  {
      binder.Rebind();
  }
  ```

Data-driven styles (color + glyph)

- Terminal: `Rendering:Terminal:Palette` (CSV), `Rendering:Terminal:Styles:*`
- SadConsole: `Rendering:SadConsole:Palette` (CSV), `Rendering:SadConsole:Styles:*`
- Each style supports `Glyph`, `Foreground`, `Background` (colors accept `#RRGGBB`, `#AARRGGBB`, `0xAARRGGBB`, or decimal ARGB).

Rendering pipeline summary

- Adapters convert world/map state into a TileBuffer (glyph mode):
  - Map cells colored by Styles
  - Entity overlays colored by `Renderable.Foreground/Background` with Z-order
- Renderers draw TileBuffer to bound targets:
  - Terminal: `TerminalSceneRenderer.SetRenderTarget(View)`
  - SadConsole: `SadConsoleSceneRenderer.SetRenderTarget(ScreenSurface)`

Operational notes

- Only one UI and one renderer are active; capability validator enforces this.
- Terminal rebind occurs automatically after layout; SadConsole rebind available via binder service.
