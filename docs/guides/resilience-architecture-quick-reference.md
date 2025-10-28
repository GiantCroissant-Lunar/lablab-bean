# Resilience Service - Tiered Architecture Quick Reference

## ✅ Current Implementation Status

The resilience implementation **already correctly follows** the Tier 1-4 pattern described in the discussion.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                         CONSUMERS                               │
│  (Use IService via DI - don't know about tiers)                 │
│                                                                 │
│  var service = sp.GetRequiredService<IService>();               │
│  await service.ExecuteWithRetryAsync(...);                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│ TIER 1: Interface Contract (Default ALC)                        │
│ Location: framework/LablabBean.Contracts.Resilience             │
│                                                                 │
│  public interface IService                                      │
│  {                                                              │
│      ICircuitBreaker CreateCircuitBreaker(...);                 │
│      Task ExecuteWithRetryAsync(...);                           │
│      Task ExecuteWithResilienceAsync(...);                      │
│      ResilienceHealthInfo GetHealthStatus();                    │
│      // ... more methods                                        │
│  }                                                              │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│ TIER 2: Proxy Service (Default ALC)                             │
│ Location: framework/LablabBean.Contracts.Resilience/            │
│           Services/Proxy/Service.cs                             │
│                                                                 │
│  [RealizeService(typeof(IService))]                             │
│  public partial class Service : IService                        │
│  {                                                              │
│      private readonly IRegistry _registry;                      │
│                                                                 │
│      // Generated implementation delegates all calls to:        │
│      // _registry.Resolve<IService>()                           │
│  }                                                              │
│                                                                 │
│  Registered in DI: services.AddSingleton<IService, Service>()   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Delegates via IRegistry
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│ TIER 3: Real Service Implementation (Plugin ALC)                │
│ Location: plugins/LablabBean.Plugins.Resilience.Polly/          │
│           Services/ResilienceService.cs                         │
│                                                                 │
│  public class ResilienceService : IService, IDisposable         │
│  {                                                              │
│      private readonly ILogger _logger;                          │
│      private readonly ConcurrentDictionary<string,              │
│                           PollyCircuitBreaker> _circuitBreakers;│
│                                                                 │
│      public ICircuitBreaker CreateCircuitBreaker(...)           │
│      {                                                          │
│          var cb = new PollyCircuitBreaker(...); // Tier 4       │
│          _circuitBreakers[operationKey] = cb;                   │
│          return cb;                                             │
│      }                                                          │
│                                                                 │
│      public async Task ExecuteWithRetryAsync(...)               │
│      {                                                          │
│          var policy = new PollyRetryPolicy(...); // Tier 4      │
│          var pipeline = new ResiliencePipelineBuilder()         │
│              .AddRetry(...)                                     │
│              .Build();                                          │
│          await pipeline.ExecuteAsync(...);                      │
│      }                                                          │
│      // ... implements all IService methods using Polly         │
│  }                                                              │
│                                                                 │
│  Registered in plugin: context.Registry.Register<IService>(     │
│      new ResilienceService(...),                                │
│      new ServiceMetadata { Priority = 100 }                     │
│  )                                                              │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Uses
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│ TIER 4: Providers (Plugin ALC)                                  │
│ Location: plugins/LablabBean.Plugins.Resilience.Polly/Providers │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │ PollyCircuitBreaker : ICircuitBreaker                     │ │
│  │   - Manages circuit breaker state machine                 │ │
│  │   - Tracks failures, enforces thresholds                  │ │
│  │   - Handles Open → HalfOpen → Closed transitions          │ │
│  └───────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │ PollyRetryPolicy : IRetryPolicy                           │ │
│  │   - Defines retry behavior (max attempts, delays)         │ │
│  │   - Calculates exponential backoff                        │ │
│  │   - Determines if retry should continue                   │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## Key Points

### ✅ Tier 1 - Interface (Stable Contract)

- Lives in **default ALC** (shared across host and plugins)
- Never moves or changes namespace
- Defines the public API consumers depend on
- Includes `IService`, `ICircuitBreaker`, `IRetryPolicy` interfaces

### ✅ Tier 2 - Proxy (Delegation Layer)

- Lives in **default ALC** (registered in DI at startup)
- Uses `[RealizeService]` attribute for source generation
- **Does almost nothing** except delegate to registry-selected Tier 3
- Registered before plugins load; consumers get this instance

### ✅ Tier 3 - Real Service (Actual Implementation)

- Lives in **plugin ALC**
- Implements `IService` from Tier 1 (shared contract)
- Contains actual resilience logic using Polly v8
- Registered into `IRegistry` with priority during plugin initialization
- Can be replaced/supplemented by other plugins

### ✅ Tier 4 - Providers (Building Blocks)

- Lives in **plugin ALC**
- Implement shared interfaces (`ICircuitBreaker`, `IRetryPolicy`)
- Used internally by Tier 3 to build resilience patterns
- Specific to Polly implementation; other plugins could use different providers

## Registry Selection

The proxy uses `IRegistry` to resolve which Tier 3 implementation to use:

```csharp
// In proxy (generated code)
var implementation = _registry.Resolve<IService>(); // Gets highest priority
return await implementation.ExecuteWithRetryAsync(...);
```

Selection modes:

- **First**: Use first registered implementation
- **HighestPriority**: Use implementation with highest priority (default, priority=100)
- **All**: (Future) Aggregate multiple implementations

## Type Identity

```
IService interface (Tier 1)
  ↑                    ↑
  │                    │
  │                    │
Proxy                Real Service
(Tier 2)             (Tier 3)
Default ALC          Plugin ALC

Both implement the SAME interface from default ALC
→ No type identity issues
→ DI resolution works seamlessly
```

## Flow Example

```
1. Host starts
   └─> Registers Tier 2 proxy in DI as IService

2. Plugin loads
   └─> Creates Tier 3 ResilienceService
       └─> Registers into IRegistry with priority=100

3. Consumer requests IService from DI
   └─> Gets Tier 2 proxy

4. Consumer calls service.ExecuteWithRetryAsync(...)
   └─> Proxy delegates to IRegistry
       └─> Registry returns Tier 3 ResilienceService (highest priority)
           └─> Tier 3 creates Tier 4 PollyRetryPolicy
               └─> Builds Polly pipeline and executes
```

## Benefits

1. ✅ **Type Safety**: Shared interface in default ALC prevents type conflicts
2. ✅ **Separation**: Proxy doesn't know implementation details
3. ✅ **Extensibility**: Real service can be swapped via plugins
4. ✅ **Provider Flexibility**: Tier 4 providers can be replaced
5. ✅ **Late Binding**: Service selected at runtime
6. ✅ **Priority Control**: Multiple plugins can compete; registry picks winner
7. ✅ **Clean Dependencies**: Host only depends on contracts

## Comparison: With vs Without Plugin Route

### With Plugin Route (Current - Tiered)

```
Host:
  - Tier 1 (IService contract)
  - Tier 2 (Proxy)

Plugin:
  - Tier 3 (ResilienceService)
  - Tier 4 (PollyCircuitBreaker, PollyRetryPolicy)

Benefits: Extensible, replaceable, runtime selection
```

### Without Plugin Route (Alternative - Direct)

```
Host:
  - Tier 1 (IService contract)
  - Tier 3 (ResilienceService) - directly in host
  - Tier 4 (Providers) - directly in host

No Tier 2 proxy needed
No plugin, no registry, no late binding

Benefits: Simpler, fewer moving parts
Use when: No need for plugin extensibility
```

## Files Involved

```
framework/LablabBean.Contracts.Resilience/
├── Interfaces/
│   └── ResilienceInterfaces.cs       # ICircuitBreaker, IRetryPolicy (Tier 1)
├── Services/
│   ├── IService.cs                   # IService interface (Tier 1)
│   └── Proxy/
│       └── Service.cs                # Proxy implementation (Tier 2)
├── Classes/
│   └── ResilienceClasses.cs          # DTOs (ResilienceHealthInfo, etc.)
└── Enums/
    └── ResilienceEnums.cs            # CircuitBreakerState

plugins/LablabBean.Plugins.Resilience.Polly/
├── ResiliencePollyPlugin.cs          # Plugin entry point (registers Tier 3)
├── Services/
│   └── ResilienceService.cs          # Real implementation (Tier 3)
└── Providers/
    ├── PollyCircuitBreaker.cs        # Circuit breaker provider (Tier 4)
    └── PollyRetryPolicy.cs           # Retry policy provider (Tier 4)
```

## Next Steps (Optional Enhancements)

1. **Multiple Provider Support**: Allow plugins to register policy providers that Tier 3 discovers
2. **Config-Driven Pipelines**: Load pipeline definitions from config
3. **Alternative Implementations**: Add plugins for other frameworks (Hystrix, etc.)
4. **Composite Mode**: Support "All" selection to chain multiple implementations
5. **Telemetry Provider**: Add OpenTelemetry integration as Tier 4 provider

---

**Status**: ✅ **Implementation Complete and Correct**
**Last Verified**: 2025-10-23
