# Research: Prior Art Summary

## CrossMilo (Contracts-first)
- Build config lists many `CrossMilo.Contracts.*` packages under `ref-projects/cross-milo/dotnet/framework/src/*`.
- Pattern: granular contracts packages with netstandard targeting.
- Implication: Split lablab-bean plugin interfaces and models into a dedicated contracts assembly.

Reference:
- `ref-projects/cross-milo/build-config.json`
- `ref-projects/cross-milo/dotnet/framework/`

## PluginManoi (Loader & Registry)
- Solution includes `PluginManoi.Contracts`, `PluginManoi.Core`, `PluginManoi.Loader`, `PluginManoi.Registry`.
- Tests cover advanced scenarios: lifecycle, hard/soft deps, process filtering.
- Implication: Reuse concepts: ALC loader, `IPluginRegistry`, dep resolver with cycle detection.

Reference:
- `ref-projects/plugin-manoi/build-config.json`
- `ref-projects/plugin-manoi/dotnet/framework/src/*`
- `ref-projects/plugin-manoi/dotnet/framework/tests/*`

## Winged Bean (Hosts & Migration)
- Completed migration of framework-agnostic plugins; clear split between framework and console-specific plugins.
- Dungeon generation architecture and host services provide a template for game plugin integration.
- Implication: Follow folder structure and multi-profile strategy (contracts/core vs console/windows hosts).

Reference:
- `ref-projects/winged-bean/PLUGIN-MIGRATION-SUMMARY.md`
- `ref-projects/winged-bean/DUNGEON-GEN-ARCHITECTURE.md`
- `ref-projects/winged-bean/development/dotnet/console/src/host/*`

## Conclusion
- Adopt contracts-first layering (CrossMilo), robust loader/registry (PluginManoi), and profile-specific hosts (Winged Bean).
- Proceed with MVP scope (manifest, loader, lifecycle, DI, basic hot reload); defer signing/sandboxing.
