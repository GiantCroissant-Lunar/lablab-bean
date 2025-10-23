# Plan: Status Effects Plugin Migration

- Create `LablabBean.Plugins.StatusEffects` (net8.0)
- Public DI: `IStatusEffectService` + DTOs (EffectInfo, StatMods)
- Systems: application, duration decrement, expiration, stat aggregation
- Events to host/HUD: EffectApplied, EffectExpired, EffectsChanged

Phases:

1) Boot plugin & service
2) Port components/systems
3) Integrate with combat system for net modifiers
4) HUD integration via events
5) Tests mirroring spec-002
