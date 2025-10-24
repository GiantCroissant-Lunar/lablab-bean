# Implementation Plan — Noesis Wrapper Vendoring (Windows-only)

## Overview

Vendor the Noesis MonoGame wrapper and expose a thin integration layer for the Windows app. Keep DesktopGL as default; enable overlay only under a WindowsDX build.

## Steps

1. Vendor Wrapper

- Copy `ref-projects/NoesisGUI.MonoGameWrapper/NoesisGUI.MonoGameWrapper/*` to `dotnet/windows-ui/NoesisGUI.MonoGameWrapper/*` (preserve project structure)
- Ensure `TargetFramework` remains `net8.0-windows`; keep package refs (Noesis 3.2.9, MonoGame.WindowsDX 3.8.4, SharpDX 4.0.1)

1. Create Noesis UI Layer

- New project: `dotnet/windows-ui/LablabBean.UI.Noesis`
- Add a `NoesisLayer` service that:
  - Calls `NoesisWrapper.Init(licenseName, licenseKey)`
  - Builds `NoesisConfig` (providers, viewport callback, input settings)
  - Creates/owns `NoesisWrapper` instance
  - Methods: `Initialize(rootXaml, themeXaml)`, `UpdateInput(GameTime, isActive)`, `Update(GameTime)`, `PreRender()`, `Render()`, `Dispose()`

1. Providers & Assets

- Use `NoesisProviderManager` with folder-based providers
- Add `Assets/` folder for sample XAML + theme
- Provide an example `Root.xaml` with a simple overlay panel

1. Windows App Wiring

- Add WindowsDX build flavor to `LablabBean.Windows` csproj (multi-target or configuration property) and reference:
  - `MonoGame.Framework.WindowsDX`
  - `dotnet/windows-ui/NoesisGUI.MonoGameWrapper`
  - `dotnet/windows-ui/LablabBean.UI.Noesis`
- In `Program.cs`, under WindowsDX symbol/config:
  - Resolve `NoesisLayer` from DI
  - Initialize with license + xaml paths after `Game.Create(builder)` but before first frame
  - On per-frame update, call `UpdateInput`/`Update`
  - Surround draw with `PreRender`/`Render`

1. Configuration & License

- Read license name/key from env vars or `appsettings.json` (not checked in)
- Add guard to disable Noesis overlay if license values are missing

1. Validation

- Build DesktopGL configuration (unchanged) — ensure no Noesis references
- Build WindowsDX configuration — confirm overlay renders sample XAML
- Resize window to verify viewport update

## Notes

- If needed, introduce `#if WINDOWSDX` compiler symbol to conditionally compile wiring code
- Keep changes isolated to avoid affecting cross-platform workflows
