# Feature Specification: Status Effects Plugin Migration

**Feature Branch**: `006-status-effects-plugin-migration`
**Created**: 2025-10-21
**Status**: Draft
**Input**: "Migrate status effects system into a plugin using the new tiered architecture."

## Goals

- Encapsulate effect application, ticking, and stat modification in a plugin.
- Provide clean DI surface for combat/actor systems to query modifiers.

## Scenarios (P1)

- Plugin tracks active effects per entity and updates durations on turn ticks.
- Combat queries net modifiers from plugin service.
- HUD shows active effects via plugin read model/events.

## Requirements

- FR-001: `IStatusEffectService` with APIs: Apply, Remove, Tick, GetNetModifiers, GetActiveEffects.
- FR-002: ECS components and systems co-located in plugin.
- FR-003: Backward compatibility with `specs/002-status-effects` scenarios.

## Success Criteria

- Reproduce acceptance tests from spec-002.
- Unload/reload does not corrupt actor stats.
