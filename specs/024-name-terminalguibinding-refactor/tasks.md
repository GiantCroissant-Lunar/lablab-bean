---
description: "Tasks for Terminal.Gui v2 Binding + Aspire/WezTerm Stabilization"
---

# Tasks: Terminal.Gui v2 Binding + Aspire/WezTerm Stabilization

**Input**: spec.md + plan.md in `specs/024-name-terminalguibinding-refactor/`
**Prerequisites**: plan.md finalized; spec.md user stories P1–P3

## Conventions

- [P] = parallelizable
- [US1|US2|US3] = user story grouping

---

## Phase 1: Contracts + Binding (FR-001, FR-002)

- [X] T001 [P] [US1] Add `dotnet/framework/LablabBean.Rendering.Terminal.Contracts/ITerminalRenderBinding.cs` (netstandard2.1)
      - Methods: `SetRenderTarget(global::Terminal.Gui.View view)`, `Rebind()`
- [X] T002 [US1] rendering-terminal: implement `ITerminalRenderBinding` (e.g., TerminalSceneRendererBinding)
      - Register into ServiceRegistry during plugin initialization
- [X] T003 [US1] ui-terminal: resolve `ITerminalRenderBinding` and call `SetRenderTarget()`
      - Remove reflection-based SetRenderTarget code path and exception handling

## Phase 2: Capability Selection (FR-004, FR-008)

- [X] T010 [US2] CapabilityValidator: read `Plugins:PreferredUI`, `PreferredRenderer`, `Skip`, `Only`
- [X] T011 [US2] CapabilityValidator: log one-line summary of selected UI/renderer and exclusions
- [X] T012 [US2] appsettings: document default `PreferredUI=ui-terminal`, `PreferredRenderer=rendering-terminal`

## Phase 3: Registry Wiring Cleanup (FR-005)

- [ ] T020 [US3] ServiceCollectionExtensions: register EventBus into ServiceRegistry immediately after creation
- [ ] T021 [US3] PluginLoader: remove EventBus fallback registration added as a safety net

## Phase 4: ALC + Preload Hardening (FR-003)

- [ ] T030 [P] [US1] Verify Terminal.Gui preload + Default ALC binding remains intact post-refactor
- [ ] T031 [P] [US1] Add loader test for Terminal.Gui type identity across UI/renderer

## Phase 5: AppHost Defaults (FR-007)

- [ ] T040 [US2] AppHost Program.cs: finalize dashboard env handling (default off; LABLAB_ENABLE_DASHBOARD=1 enables; safe OTLP/URL defaults)

## Phase 6: Publish Hygiene Verification (FR-006)

- [ ] T050 [P] [US1] scripts/publish-console-artifact.ps1: keep clean publish + add post-publish checklist log for plugin manifests
- [ ] T051 [US1] Validate artifact layout: `plugins/ui-terminal/plugin.json`, `plugins/rendering-terminal/plugin.json` present

## Validation & Tests

- [ ] T060 [US1] Verify WezTerm run shows Terminal.Gui UI and logs initialization strings
- [ ] T061 [US2] `plugins list` shows deterministic selection per preferences; exclusions summarized once
- [ ] T062 [US3] Zero `IEventBus` missing errors over 10 runs

## Documentation

- [ ] T070 [P] Update docs/guides with PreferredUI/Renderer and WezTerm/Aspire launch notes
