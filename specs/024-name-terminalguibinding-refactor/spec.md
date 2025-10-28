# Feature Specification: Terminal.Gui v2 Binding + Aspire/WezTerm Stabilization

**Feature Branch**: `024-name-terminalguibinding-refactor`
**Created**: 2025-10-28
**Status**: Draft
**Input**: Stabilize Terminal.Gui v2 console experience launched via Aspire/WezTerm by formalizing a terminal render-binding contract, making capability selection deterministic, removing reflection and loader-side service wiring, and hardening AppHost + publish hygiene.

## User Scenarios & Testing (mandatory)

### User Story 1 - Launch console in WezTerm and see Terminal.Gui v2 reliably (Priority: P1)

As a developer, when I run Aspire with WezTerm or use the wezterm launcher, the console app starts and the Terminal.Gui v2 UI initializes consistently without plugin load/type-identity errors.

Why this priority: This is the primary developer experience loop; instability blocks all UI work.

Independent Test: Publish console artifact, run `scripts/launch-wezterm-console.ps1`, observe UI init in logs and the visible WezTerm window. No PluginLoadException or Terminal.Gui type cast errors.

Acceptance Scenarios:

1. Given a clean publish output, when running the WezTerm launcher, then the console starts and logs “Terminal UI adapter initialized with full HUD, WorldView, and ActivityLog”.
2. Given both UI and renderer plugins available, when capability selection runs, then `ui-terminal` and `rendering-terminal` are selected deterministically.

---

### User Story 2 - Deterministic UI/renderer selection (Priority: P2)

As a developer, I can control which UI and renderer plugins load via config (`Plugins:PreferredUI`, `Plugins:PreferredRenderer`, `Plugins:Skip`, `Plugins:Only`), and the loader clearly logs the outcome.

Why this priority: Avoids surprises when multiple capabilities exist; eases troubleshooting.

Independent Test: Set preferences in appsettings.json, run `plugins list`, verify the selected/excluded plugins match config and logs summarize the decision.

Acceptance Scenarios:

1. Given PreferredUI=ui-terminal, when multiple ui plugins exist, then ui-terminal is selected and others are excluded with a clear reason.
2. Given Skip includes a plugin ID, when discovery runs, then that plugin is excluded before capability selection and logged once.

---

### User Story 3 - Remove reflection and loader-side service wiring (Priority: P3)

As a maintainer, I want an explicit terminal render-binding contract between the renderer and UI plugins and no implicit EventBus registration in the loader, to reduce fragility and improve readability.

Why this priority: Reflection and hidden wiring create confusing failure modes and coupling.

Independent Test: UI plugin resolves `ITerminalRenderBinding` and calls `SetRenderTarget(View)`; `ServiceRegistry` already contains EventBus from DI setup. No loader fallbacks.

Acceptance Scenarios:

1. Given `ITerminalRenderBinding` is registered by `rendering-terminal`, when the UI starts, then `SetRenderTarget` is invoked without reflection.
2. Given EventBus is registered during DI setup, when Reactive services/plugins resolve `IEventBus`, then no “No implementations registered” error occurs.

---

### Edge Cases

- UI plugin present but renderer missing → UI fails fast with clear error and capability logs mention missing dependency.
- Multiple UI/renderer plugins deployed → Preferences and priority determine one, with explicit exclusion reasons.
- Terminal.Gui loaded from different ALCs → Preload + Default ALC binding ensures a single type identity across plugins.
- Publish layout missing plugin.json or DLLs → Loader base-dir fallback for DLLs; publish clean step avoids stale manifests.

## Requirements (mandatory)

### Functional Requirements

- FR-001 (Binding): Provide `ITerminalRenderBinding` interface in a small `LablabBean.Rendering.Terminal.Contracts` package.
  - Methods: `SetRenderTarget(global::Terminal.Gui.View view)`, `Rebind()`.
  - Implement in `rendering-terminal`; register into `ServiceRegistry`.
- FR-002 (UI integration): `ui-terminal` resolves `ITerminalRenderBinding` and calls `SetRenderTarget()`; remove reflection-based binding.
- FR-003 (ALC): Loader and PluginLoadContext MUST bind `Terminal.Gui` to Default ALC and preload it before plugin loads.
- FR-004 (Capabilities): Capability selection MUST honor `Plugins:PreferredUI`, `Plugins:PreferredRenderer`, `Plugins:Skip`, `Plugins:Only`.
- FR-005 (Registry wiring): DI setup MUST register EventBus into `ServiceRegistry` before any plugins start; the loader MUST NOT auto-register services.
- FR-006 (Publish hygiene): Publishing MUST clean `publish/console` and place `plugin.json` under `plugins/<Plugin>/`; loader MUST fall back to base dir for DLLs.
- FR-007 (AppHost): AppHost MUST not crash if dashboard envs are missing; provide safe defaults and an opt-out var.
- FR-008 (Logging): On startup, log selected UI/renderer and exclusions once; reduce duplicate warnings.

### Key Entities (include if feature involves data)

- ITerminalRenderBinding: Bridge between renderer and UI; owned by renderer plugin; consumed by UI plugin.
- Capability Preferences: Config keys guiding selection (`PreferredUI`, `PreferredRenderer`, `Skip`, `Only`).

## Success Criteria (mandatory)

### Measurable Outcomes

- SC-001: WezTerm launch shows Terminal.Gui UI within 3 seconds after console process start in Debug publish.
- SC-002: Zero occurrences of `InvalidOperationException: No implementations registered for service type IEventBus` across 10 consecutive runs.
- SC-003: Zero `TypeLoadException` / `ArgumentException` caused by Terminal.Gui type identity across UI/renderer boundary.
- SC-004: Capability logs list exactly one selected UI and renderer; excluded plugins are listed once with reasons.

## Non-Functional Requirements

- NFR-001: No reflection used for UI/renderer binding.
- NFR-002: Changes maintain existing plugin hot-reload setup (where applicable).
- NFR-003: No network calls required at runtime for selection/binding.

## Out of Scope

- Full in-game rendering parity or gameplay features.
- Dashboard UI theming or metrics export.

## Open Questions

- Should `ITerminalRenderBinding` live alongside existing rendering contracts or remain a small separate package to keep dependencies minimal?
- Should we codify a soft-dependency graph for optional gameplay plugins (boss, quest, etc.) to suppress repeated warnings?

## Milestones

1. Contract + Renderer implementation (FR-001, FR-002)
2. ALC + preload hardening (FR-003)
3. Capability preferences & logs (FR-004, FR-008)
4. Registry wiring cleanup (FR-005)
5. AppHost defaults (FR-007)
6. Publish hygiene verification (FR-006)

## Validation Plan

- Scripted: `scripts/publish-console-artifact.ps1` then `scripts/launch-wezterm-console.ps1`; verify logs show selected UI/renderer and Terminal.Gui initialization.
- CLI: `LablabBean.Console.exe plugins list` under publish dir to validate selection and load outcomes.
- Tests: Add loader tests for base-dir fallback, ALC type identity, and capability selection honoring preferences.
