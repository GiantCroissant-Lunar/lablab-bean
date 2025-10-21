# Spec 009: Proxy Service Source Generator

**Status**: 📋 Draft  
**Created**: 2025-10-22  
**Prerequisites**: Spec 007 ✅, Spec 008 ✅

## Overview

Roslyn incremental source generator that automatically creates proxy service implementations, eliminating 90%+ of manual delegation boilerplate.

## Problem

Every service interface method requires 3-5 lines of manual proxy delegation code:

```csharp
// Manual proxy (repetitive boilerplate)
public partial class GameServiceProxy : Game.Services.IService
{
    private readonly IRegistry _registry;
    
    public void StartGame()
    {
        _registry.Get<Game.Services.IService>().StartGame();
    }
    
    public Task ProcessTurnAsync(CancellationToken ct)
    {
        return _registry.Get<Game.Services.IService>().ProcessTurnAsync(ct);
    }
    
    // ... 50 more methods like this ...
}
```

For interfaces with 50+ methods, this is **hundreds of lines of error-prone boilerplate**.

## Solution

Source generator automatically creates all implementations:

```csharp
// Developer writes this (10 lines)
[RealizeService(typeof(Game.Services.IService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class GameServiceProxy
{
    private readonly IRegistry _registry;
    
    public GameServiceProxy(IRegistry registry)
    {
        _registry = registry;
    }
}

// Generator creates this automatically (200+ lines)
// - All method implementations
// - All property implementations
// - All event implementations
// - Proper delegation to IRegistry
```

## Key Features

✅ **Automatic Method Generation** - All interface methods implemented  
✅ **Property & Event Support** - Get/set accessors, add/remove handlers  
✅ **Generic Methods** - Type parameters and constraints preserved  
✅ **Advanced Features** - ref/out/params, async, nullable, defaults  
✅ **Selection Strategies** - Control service retrieval (One, HighestPriority, All)  
✅ **Quality Code** - No warnings, nullable-safe, XML docs  
✅ **Clear Diagnostics** - Helpful error messages with fix suggestions  

## Documents

- **[spec.md](spec.md)** - Complete feature specification (46 requirements)
- **[plan.md](plan.md)** - Implementation plan (7 phases, 20-30 hours)
- **[tasks.md](tasks.md)** - Detailed task breakdown (101 tasks)

## Quick Start (After Implementation)

1. **Add attributes to your partial class**:
```csharp
[RealizeService(typeof(MyContract.IService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class MyServiceProxy
{
    private readonly IRegistry _registry;
    
    public MyServiceProxy(IRegistry registry) => _registry = registry;
}
```

2. **Build the project** - Generator runs automatically

3. **Use the proxy**:
```csharp
var proxy = new MyServiceProxy(registry);
proxy.SomeMethod(); // Automatically delegates to IRegistry
```

## Benefits

| Metric | Manual | With Generator | Improvement |
|--------|--------|----------------|-------------|
| Lines of code | 200+ | 10 | **95% reduction** |
| Time to implement | 30-60 min | <30 sec | **100x faster** |
| Error rate | High | Zero | **100% reduction** |
| Maintenance | High | Zero | **Automatic updates** |

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│ Developer Code (10 lines)                               │
│ [RealizeService(typeof(IService))]                      │
│ public partial class MyProxy { ... }                    │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│ Source Generator (Compile-Time)                         │
│ - Analyzes partial class                                │
│ - Reads interface definition                            │
│ - Generates implementations                             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│ Generated Code (200+ lines)                             │
│ public partial class MyProxy : IService                 │
│ {                                                        │
│     public void Method() =>                             │
│         _registry.Get<IService>().Method();             │
│     // ... all other methods ...                        │
│ }                                                        │
└─────────────────────────────────────────────────────────┘
```

## Implementation Status

- [ ] Phase 0: Project Setup (6 tasks)
- [ ] Phase 1: Basic Method Generation (12 tasks)
- [ ] Phase 2: Property and Event Generation (10 tasks)
- [ ] Phase 3: Advanced Method Features (14 tasks)
- [ ] Phase 4: Selection Strategy Support (8 tasks)
- [ ] Phase 5: Nullable and Code Quality (10 tasks)
- [ ] Phase 6: Error Handling and Diagnostics (10 tasks)
- [ ] Phase 7: Integration and Documentation (12 tasks)

**Total**: 0/101 tasks complete (0%)

## Dependencies

```
Spec 009: Proxy Service Source Generator
├── Spec 007: Tiered Contract Architecture ✅
│   ├── IRegistry interface
│   ├── SelectionMode enum
│   └── IEventBus
├── Spec 008: Extended Contract Assemblies ✅
│   ├── Scene.Services.IService
│   ├── Input.Router.IService<T>
│   ├── Input.Mapper.IService
│   ├── Config.Services.IService
│   └── Resource.Services.IService
└── Roslyn SDK
    └── Microsoft.CodeAnalysis.CSharp
```

## Success Criteria

1. ✅ Generate proxy for 50+ method interface without errors
2. ✅ Generated code compiles without warnings
3. ✅ Developer creates proxy in <30 seconds
4. ✅ 90%+ boilerplate reduction
5. ✅ All interface member types supported
6. ✅ Generic constraints preserved (100%)
7. ✅ Ref/out parameters work (100%)
8. ✅ Async methods work (100%)
9. ✅ Clear diagnostic messages
10. ✅ Readable generated code
11. ✅ Build time impact <1 second
12. ✅ Documentation with 3+ examples

## Related Specs

- **Spec 007**: Tiered Contract Architecture (Event Bus + IRegistry)
- **Spec 008**: Extended Contract Assemblies (Service interfaces to proxy)

---

**Next Steps**: Begin Phase 0 (Project Setup) when ready to implement
