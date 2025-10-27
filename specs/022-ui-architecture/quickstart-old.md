# UI Architecture Quickstart (Plugins)

**Status**: Draft
**Last Updated**: 2025-10-27

## Terminal (Console host)

- Build and run the console app without subcommands to start the UI plugin.
- Ensure the following plugins are present and loadable:
  - UI.Terminal (capabilities: ui, ui:terminal)
  - Rendering.Terminal (capabilities: renderer, renderer:terminal)
- Verify: the plugin loader reports one active UI and one active renderer.

Windows (Windows app host)

- Update Windows host to load plugins on startup (mirrors console host).
- Ensure these plugins are present:
  - UI.SadConsole (capabilities: ui, ui:windows)
  - Rendering.SadConsole (capabilities: renderer, renderer:windows)
- Verify: GameScreen/HUD are driven by the UI.SadConsole plugin, not the app.

Config and Selection

- Configure preferred UI/renderer in appsettings (e.g., Plugins:Capabilities:Prefer = ["ui:terminal", "renderer:terminal"]).
- Loader enforces exactly one active UI + one active renderer; fails fast otherwise.

Troubleshooting

- If Terminal.Gui fails to init due to type load, confirm plugin isolation and dependency versions.
- If plugins don’t load, check Plugins:Paths and plugin.json capability tags.
