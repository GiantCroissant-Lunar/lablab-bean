# Vendor NoesisGUI MonoGame Wrapper (Windows-only Overlay)

## Summary

Adopt and vendor the reference project `ref-projects/NoesisGUI.MonoGameWrapper` to enable NoesisGUI-powered XAML UI as an overlay on top of our SadConsole/MonoGame game loop. Scope this as a Windows-only integration (Direct3D 11 via SharpDX) while preserving the existing DesktopGL path as default.

## User Scenarios & Testing

- P1 Overlay HUD: As a player, I see a Noesis XAML HUD overlayed on the SadConsole world without flicker; toggling visibility does not disrupt input or rendering.
- P1 Menu Screen: As a user, I can open a Noesis-styled menu (Root.xaml) and interact with buttons; focus is correctly handled between Noesis and SadConsole.
- P2 Theming: As a designer, I can switch a theme ResourceDictionary and see styles update without code changes.
- P3 Resize: As a user, when resizing the window, the overlay resizes and remains sharp.

Acceptance checks per scenario are measurable by visuals on launch, button click handlers firing, and no exceptions on resize.

## Requirements

### Functional Requirements

- FR-001: Initialize Noesis license and GUI exactly once per process.
- FR-002: Provide a `NoesisLayer` service exposing Initialize/UpdateInput/Update/PreRender/Render/Dispose.
- FR-003: Load a root XAML (file path) and optional theme ResourceDictionary via folder providers.
- FR-004: Respect viewport changes and GraphicsProfile.HiDef; update view size accordingly.
- FR-005: Work only for WindowsDX build; DesktopGL remains unaffected.
- FR-006: Fail gracefully if license missing; disable overlay with log message.

### Non-Functional Requirements

- NFR-001: Zero measurable regressions to SadConsole render loop.
- NFR-002: Integration guarded via config or compile-time symbol.
- NFR-003: No platform warnings when building DesktopGL variant.

## Constraints & Assumptions

- net8.0-windows; MonoGame.Framework.WindowsDX 3.8.4; Noesis 3.2.9; SharpDX 4.0.1.
- Requires GraphicsProfile.HiDef.
- License provided via env or appsettings; not hardcoded.

## Success Criteria

- WindowsDX build shows Noesis overlay on launch with sample XAML.
- Toggling overlay leaves SadConsole world rendering intact.
- No crashes or visual corruption on window resize.

## Risks

- Windows-only backend; mitigated by keeping DesktopGL default.
- SharpDX maintenance status; acceptable for Windows path.
- Licensing mishandling; use external configuration.

## Deliverables

- Vendored wrapper under `dotnet/windows-ui/NoesisGUI.MonoGameWrapper`.
- New project `dotnet/windows-ui/LablabBean.UI.Noesis` exposing `NoesisLayer`.
- Windows app wiring and sample XAML assets.

## References

- `ref-projects/NoesisGUI.MonoGameWrapper` (source)
- `NoesisWrapper.cs`, `NoesisViewWrapper.cs`, `Config/NoesisConfig.cs`

