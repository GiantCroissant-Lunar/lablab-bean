# Feature Specification: Inventory System Plugin Migration

**Feature Branch**: `005-inventory-plugin-migration`
**Created**: 2025-10-21
**Status**: Draft
**Input**: "Migrate inventory system into a self-contained plugin using the new tiered plugin architecture."

## Goals

- Encapsulate inventory components and systems behind a plugin boundary.
- Expose minimal host-facing interfaces; no direct coupling to host UI.

## Scenarios (P1)

- Host loads `Inventory` plugin; plugin registers systems/services.
- Player can pick up, use, equip items as before; HUD integrates via plugin-exposed read model.

## Requirements

- FR-001: Provide `IInventoryService` in DI with methods: pickup/use/equip/query.
- FR-002: Maintain ECS components in plugin assembly (Item, Stackable, Equippable, Inventory, EquipmentSlots).
- FR-003: Event hooks for HUD updates (no direct UI dependency).
- FR-004: Backward-compatible data model with `specs/001-*`.

## Success Criteria

- No regressions against `specs/001-inventory-system` acceptance scenarios.
- Plugin loads and starts independently; unload does not crash host.
