# Spec 008: Extended Contract Assemblies

**Status**: Draft  
**Prerequisites**: [Spec 007: Tiered Contract Architecture](../007-tiered-contract-architecture/spec.md) (Complete)  
**Created**: 2025-10-21

## Overview

This specification extends the tiered contract architecture from Spec 007 by adding 4 critical contract assemblies identified through gap analysis with the cross-milo reference architecture:

1. **LablabBean.Contracts.Scene** - Scene/level management, camera, viewport
2. **LablabBean.Contracts.Input** - Input routing, action mapping, scope management
3. **LablabBean.Contracts.Config** - Configuration management with change notifications
4. **LablabBean.Contracts.Resource** - Async resource loading with caching

## Why These Contracts?

### Spec 007 Foundation
Spec 007 established:
- ✅ `IEventBus` - Event-driven communication (1.1M+ events/sec)
- ✅ `LablabBean.Contracts.Game` - Game service contracts
- ✅ `LablabBean.Contracts.UI` - UI service contracts
- ✅ Event definitions and service patterns

### Spec 008 Extensions
These 4 contracts are essential for a functional dungeon crawler:

| Contract | Why Critical | Without It |
|----------|-------------|------------|
| **Scene** | Level loading, camera control | Can't navigate dungeons, no viewport management |
| **Input** | Modal UI, action mapping | Input conflicts between plugins, no keybinding support |
| **Config** | Game settings, preferences | No difficulty settings, no config persistence |
| **Resource** | Asset loading, caching | Duplicate assets in memory, slow loading |

## Relationship to Spec 007

```
Spec 007 (Foundation)          Spec 008 (Extensions)
├── IEventBus                  ├── Scene Contract
├── IRegistry                  ├── Input Contract
├── Game Contract              ├── Config Contract
└── UI Contract                └── Resource Contract
                               
All use IEventBus for events ──┘
All use IRegistry for services ─┘
```

## Key Differences from Spec 007

| Aspect | Spec 007 | Spec 008 |
|--------|----------|----------|
| **Scope** | Foundation + 2 contracts | 4 additional contracts |
| **Priority** | P0 (blocking) | P1 (critical for dungeon crawler) |
| **Dependencies** | None | Requires Spec 007 complete |
| **Complexity** | Medium | Medium-High (4 domains) |
| **User Stories** | 4 stories | 4 stories (1 per contract) |
| **Success Criteria** | 8 criteria | 8 criteria |
| **Functional Requirements** | 37 requirements | 69 requirements |

## Success Criteria Summary

1. ✅ Scene loader without UI framework dependencies
2. ✅ Modal UI with correct input capture
3. ✅ Scene loading <100ms (up to 100 entities)
4. ✅ Input scope stack no memory leaks (1,000 cycles)
5. ✅ Config change events <10ms latency
6. ✅ Resource service handles 50+ concurrent loads
7. ✅ Resource cache >90% hit rate
8. ✅ Complete dungeon workflow <2 hours (with docs)

## Documentation

- **[spec.md](spec.md)** - Complete specification
- **[data-model.md](data-model.md)** - Entity relationships and data structures (TODO)
- **[plan.md](plan.md)** - Implementation plan with design artifacts (TODO)
- **[tasks.md](tasks.md)** - Dependency-ordered task list (TODO)
- **[quickstart.md](quickstart.md)** - Developer quick start guide (TODO)

## Implementation Phases

### Phase 1: Scene Contract (P1)
- Scene loading/unloading
- Camera and viewport management
- Scene transition events
- **Estimated**: 2-3 days

### Phase 2: Input Contract (P1)
- Input scope stack
- Input routing (topmost scope)
- Action mapping
- **Estimated**: 2-3 days

### Phase 3: Config Contract (P1)
- Get/Set operations
- Hierarchical sections
- Change notifications
- **Estimated**: 1-2 days

### Phase 4: Resource Contract (P1)
- Async loading
- Caching and preloading
- Load success/failure events
- **Estimated**: 2-3 days

**Total Estimated**: 7-11 days

## Out of Scope (Future Specs)

Deferred to future specifications:
- **Spec 009**: Audio, Analytics, Diagnostics contracts
- **Spec 010**: Source generators for proxy services
- **Spec 011**: Advanced resource features (streaming, compression)
- **Spec 012**: Advanced config features (schema validation, type safety)

## References

### Gap Analysis Documents
- [contract-gap-analysis.md](../../docs/_inbox/contract-gap-analysis.md) - Detailed gap analysis
- [cross-milo-contract-adoption-analysis.md](../../docs/_inbox/cross-milo-contract-adoption-analysis.md) - Cross-milo patterns

### Related Specs
- [Spec 007: Tiered Contract Architecture](../007-tiered-contract-architecture/spec.md) - Foundation
- [Spec 001: Inventory System](../001-inventory-system/spec.md) - Example plugin
- [Spec 002: Status Effects](../002-status-effects/spec.md) - Example plugin

### Cross-Milo Reference
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Scene/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Input/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Config/`
- `ref-projects/cross-milo/dotnet/framework/src/CrossMilo.Contracts.Resource/`

## Next Steps

1. **Review spec.md** - Validate requirements and success criteria
2. **Run `/speckit.plan`** - Generate implementation plan
3. **Run `/speckit.tasks`** - Generate task list
4. **Create data-model.md** - Define entity relationships
5. **Begin Phase 1** - Implement Scene contract

---

**Version**: 1.0.0  
**Status**: Draft  
**Last Updated**: 2025-10-21
