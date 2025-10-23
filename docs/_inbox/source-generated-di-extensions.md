---
title: Source-Generated DI Extensions for Proxy Services
category: Architecture
status: Active
created: 2025-10-23
tags: [source-generation, di, proxy, resilience]
---

# Source-Generated DI Extensions for Proxy Services

## Overview

The `DIExtensionsGenerator` automatically creates DI registration extension methods for proxy services marked with `[RealizeService]`. This eliminates manual registration code and ensures type-safe, consistent proxy setup.

## How It Works

### 1. Mark Proxy with Attribute

```csharp
// In: framework/LablabBean.Contracts.Resilience/Services/Proxy/Service.cs
namespace LablabBean.Contracts.Resilience.Services.Proxy;

[RealizeService(typeof(IService))]
public partial class Service : IService
{
    private readonly IRegistry _registry;

    public Service(IRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }
}
```

### 2. Generator Creates Extension Method

The source generator analyzes the `[RealizeService]` attribute and generates:

```csharp
// Auto-generated: LablabBean.Contracts.Resilience.Extensions.ServiceCollectionExtensions.g.cs
namespace LablabBean.Contracts.Resilience.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the IService proxy that delegates to the plugin registry.
    /// </summary>
    public static IServiceCollection AddResilienceServiceProxy(
        this IServiceCollection services,
        SelectionMode mode = SelectionMode.HighestPriority)
    {
        services.AddSingleton<LablabBean.Contracts.Resilience.Services.IService>(sp =>
        {
            var registry = sp.GetRequiredService<IRegistry>();
            return new LablabBean.Contracts.Resilience.Services.Proxy.Service(registry);
        });
        return services;
    }
}
```

### 3. Use in Host Application

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Step 1: Register plugin system (IRegistry)
        services.AddPluginSystem(context.Configuration);

        // Step 2: Register resilience proxy (auto-generated!)
        services.AddResilienceServiceProxy();
    })
    .Build();
```

## Benefits

### ✅ Zero Boilerplate

- No manual `AddSingleton<IService>()` calls
- No lambda expressions to write
- No risk of typos in registration

### ✅ Type Safety

- Compiler-verified at build time
- IntelliSense support
- Refactoring-safe

### ✅ Consistent Naming

- Method name derived from namespace and interface: `Add{Domain}{Service}Proxy()`
- Example: `IService` in `Resilience` → `AddResilienceServiceProxy()`

### ✅ Documentation Built-in

- XML comments auto-generated
- Usage notes included
- Parameter descriptions

## Naming Convention

| Namespace | Interface | Generated Method |
|-----------|-----------|------------------|
| `LablabBean.Contracts.Resilience` | `IService` | `AddResilienceServiceProxy()` |
| `LablabBean.Contracts.Input` | `IInputHandler` | `AddInputInputHandlerProxy()` |
| `LablabBean.Contracts.Scene` | `ISceneManager` | `AddSceneSceneManagerProxy()` |

Pattern: `Add{DomainFromNamespace}{InterfaceWithoutI}Proxy()`

## Implementation Details

### Generator: DIExtensionsGenerator

**Location**: `framework/LablabBean.SourceGenerators.Proxy/DIExtensionsGenerator.cs`

**Algorithm**:

1. Scan for classes with `[RealizeService]` attribute
2. Extract service interface type from attribute argument
3. Derive namespace and interface name
4. Group by root namespace (e.g., `LablabBean.Contracts.Resilience`)
5. Generate one `ServiceCollectionExtensions` class per namespace
6. Create `Add{Domain}{Service}Proxy()` methods

**Output**: `.g.cs` files in `obj/Debug/net8.0/generated/`

### Parameters

The generated method includes:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `services` | `IServiceCollection` | - | DI container |
| `mode` | `SelectionMode` | `HighestPriority` | How to select implementation when multiple exist |

### Selection Modes

```csharp
public enum SelectionMode
{
    One,              // Exactly one implementation required
    HighestPriority,  // Select implementation with highest priority
    All               // Return all implementations (use GetAll<T>())
}
```

## Advanced Usage

### Custom Selection Mode

```csharp
services.AddResilienceServiceProxy(SelectionMode.One);
```

This will throw if multiple plugins provide `IService` implementations.

### Multiple Proxies

Each domain automatically gets its own extension:

```csharp
services.AddPluginSystem()
    .AddResilienceServiceProxy()
    .AddInputInputHandlerProxy()
    .AddSceneSceneManagerProxy();
```

### Conditional Registration

```csharp
if (configuration.GetValue<bool>("Features:Resilience"))
{
    services.AddResilienceServiceProxy();
}
```

## Troubleshooting

### Method Not Found

**Symptom**: `AddResilienceServiceProxy()` not available in IntelliSense

**Solutions**:

1. Rebuild the `LablabBean.Contracts.Resilience` project
2. Check that `[RealizeService]` attribute is present on proxy class
3. Verify `LablabBean.SourceGenerators.Proxy` is referenced as `Analyzer`
4. Clean and rebuild (`dotnet clean && dotnet build`)

### Wrong Method Name

**Symptom**: Method is named `AddServiceProxy()` instead of `AddResilienceServiceProxy()`

**Cause**: Namespace doesn't follow `LablabBean.Contracts.{Domain}` pattern

**Solution**: Ensure proxy class is in a namespace like `LablabBean.Contracts.Resilience.Services.Proxy`

### Build Errors

**Symptom**: Source generator fails to compile

**Check**:

- Generator project targets `netstandard2.0`
- No `required` keyword used (not supported in netstandard2.0)
- All dependencies are compatible with analyzer restrictions

## Viewing Generated Code

To inspect the generated source:

```bash
cd dotnet/framework/LablabBean.Contracts.Resilience
dotnet build /p:EmitCompilerGeneratedFiles=true
cat obj/Debug/net8.0/generated/LablabBean.SourceGenerators.Proxy/*/ServiceCollectionExtensions.g.cs
```

## Testing

Example demo project: `dotnet/examples/ResilienceProxyDemo`

```bash
cd dotnet/examples/ResilienceProxyDemo
dotnet run
```

Expected output:

```
✅ Resilience proxy registered via source-generated extension method
✅ IService resolved: Service
   Namespace: LablabBean.Contracts.Resilience.Services.Proxy
⚠️  Expected exception (no plugin loaded yet): No implementations registered...
```

## Best Practices

### ✅ DO

- Always call `AddPluginSystem()` before proxy registration
- Use meaningful domain names in namespace
- One `[RealizeService]` per interface
- Keep proxy classes in `*.Proxy` namespace

### ❌ DON'T

- Register proxy without `AddPluginSystem()`
- Mix manual registration with generated methods
- Use same interface in multiple domains
- Modify generated `.g.cs` files (they're overwritten on build)

## Related

- **Proxy Generation**: See `ProxyServiceGenerator.cs` for implementation generation
- **Plugin System**: See `AddPluginSystem()` for registry setup
- **Architecture**: Tier 1-4 plugin pattern documentation

## References

- Source: `framework/LablabBean.SourceGenerators.Proxy/DIExtensionsGenerator.cs`
- Example: `examples/ResilienceProxyDemo/Program.cs`
- Tests: (TBD - integration tests for generator)

---
**Last Updated**: 2025-10-23
**Generator Version**: 1.0.0
