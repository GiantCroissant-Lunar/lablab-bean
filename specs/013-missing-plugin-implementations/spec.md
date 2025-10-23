# Feature Specification: Missing Plugin Implementations (Tier 3/4)

**Feature Branch**: `013-missing-plugin-implementations`
**Created**: 2025-10-23
**Status**: Draft
**Dependencies**: SPEC-012 (Missing Service Contracts)

## Executive Summary

This specification defines the implementation of Tier 3/4 plugin projects for the 12 service contracts created in SPEC-012. These plugins provide actual service implementations and provider-specific backends, completing the four-tier architecture (Contracts → Proxies → Services → Providers).

## Context: Four-Tier Architecture

```
Tier 1: Contracts (Interfaces)     - dotnet/framework/LablabBean.Contracts.*/Services/IService.cs
Tier 2: Proxies (Delegation)       - dotnet/framework/LablabBean.Contracts.*/Services/Proxy/Service.cs [MISSING]
Tier 3: Services (Implementation)  - dotnet/plugins/LablabBean.Plugins.*/Services/Service.cs
Tier 4: Providers (Backends)       - dotnet/plugins/LablabBean.Plugins.*/Providers/*Provider.cs
```

**Current State**: SPEC-012 completed Tier 1 (Contracts) only.
**This Spec**: Implement Tier 2 (Proxies), Tier 3 (Services), and Tier 4 (Providers).

## User Scenarios & Testing (mandatory)

### User Story 1 - Developer uses Resilience with Polly (Priority: P1)

As a developer, I need resilience patterns (retry, circuit breaker, timeout) implemented using Polly library so I can handle transient failures gracefully.

- Acceptance Scenarios:
  1. Given a Resilience.Polly plugin, when I execute an operation with retry policy, then transient failures are automatically retried.
  2. Given circuit breaker configuration, when failure threshold is reached, then circuit opens and subsequent calls fail fast.
  3. Given timeout policy, when operation exceeds duration, then it's cancelled appropriately.

---

### User Story 2 - Developer uses Serialization with System.Text.Json (Priority: P1)

As a developer, I need serialization using System.Text.Json so I can serialize/deserialize objects efficiently.

- Acceptance Scenarios:
  1. Given Serialization.Json plugin, when I serialize an object, then valid JSON is produced.
  2. Given JSON string, when I deserialize, then object is correctly reconstructed.
  3. Given serialization options, when configured, then custom settings are applied (camelCase, indentation, etc.).

---

### User Story 3 - Developer uses ObjectPool with default implementation (Priority: P1)

As a developer, I need object pooling so I can reuse objects and reduce GC pressure.

- Acceptance Scenarios:
  1. Given ObjectPool plugin, when I create a pool, then objects are pre-allocated according to configuration.
  2. Given pool usage, when I get/return objects, then pool correctly manages lifecycle.
  3. Given pool statistics, when queried, then accurate metrics are provided.

---

### User Story 4 - Developer uses PersistentStorage with JSON files (Priority: P2)

As a developer, I need file-based persistent storage so I can save/load application data.

- Acceptance Scenarios:
  1. Given PersistentStorage.Json plugin, when I save data, then JSON file is created.
  2. Given existing JSON file, when I load, then data is correctly deserialized.
  3. Given storage operations, when errors occur, then appropriate exceptions are thrown.

---

### User Story 5 - Developer uses Scheduler with System.Threading.Timer (Priority: P2)

As a developer, I need task scheduling so I can execute operations at specific times or intervals.

- Acceptance Scenarios:
  1. Given Scheduler plugin, when I schedule a delayed task, then it executes after specified delay.
  2. Given recurring task, when scheduled, then it executes repeatedly at specified interval.
  3. Given task cancellation, when cancelled, then task stops executing.

---

### User Story 6 - Developer uses Diagnostic with Console output (Priority: P2)

As a developer, I need diagnostic output so I can monitor application health and performance.

- Acceptance Scenarios:
  1. Given Diagnostic.Console plugin, when diagnostic events occur, then they're written to console.
  2. Given performance metrics, when collected, then they're formatted and displayed.
  3. Given diagnostic session, when started/stopped, then session lifecycle is managed.

---

### User Story 7 - Developer uses Localization with JSON resources (Priority: P3)

As a developer, I need JSON-based localization so I can provide multi-language support.

- Acceptance Scenarios:
  1. Given Localization.Json plugin, when locale is changed, then strings are loaded from correct JSON file.
  2. Given missing translation key, when requested, then fallback language is used.
  3. Given pluralization, when count varies, then correct plural form is selected.

---

### User Story 8 - Developer uses Audio with NAudio (Priority: P3)

As a developer, I need audio playback capabilities so I can play sounds in the application.

- Acceptance Scenarios:
  1. Given Audio.NAudio plugin, when I play sound, then audio is output through default device.
  2. Given volume control, when adjusted, then audio playback volume changes.
  3. Given spatial audio, when position changes, then panning is adjusted.

---

### User Story 9 - Developer uses Performance monitoring (Priority: P3)

As a developer, I need performance metrics collection so I can identify bottlenecks.

- Acceptance Scenarios:
  1. Given Performance plugin, when I record metrics, then they're stored and aggregated.
  2. Given performance activities, when measured, then duration and status are tracked.
  3. Given recommendations, when thresholds exceeded, then alerts are generated.

---

### User Story 10 - Developer uses Analytics with console logging (Priority: P3)

As a developer, I need analytics event tracking so I can understand application usage.

- Acceptance Scenarios:
  1. Given Analytics.Console plugin, when events are tracked, then they're logged to console.
  2. Given user properties, when set, then they're included in event context.
  3. Given screen views, when tracked, then navigation flow is recorded.

---

## Requirements (mandatory)

### Plugin Architecture

- **FR-001**: All plugins MUST follow naming pattern `LablabBean.Plugins.{Service}.{Provider}`.
- **FR-002**: All plugins MUST target `net8.0` (consistent with existing plugins).
- **FR-003**: All plugins MUST implement `IPlugin` interface with Initialize/Start/Stop lifecycle.
- **FR-004**: All plugins MUST register their services with `IRegistry` in InitializeAsync.

### Tier 2: Proxy Services (Critical - Missing from SPEC-012)

- **FR-005**: ALL contracts MUST have proxy services in `Services/Proxy/Service.cs`.
- **FR-006**: Proxy services MUST use `[RealizeService(typeof(IService))]` attribute.
- **FR-007**: Proxy services MUST delegate to `IRegistry` for service resolution.
- **FR-008**: Proxy services MUST reference `LablabBean.SourceGenerators.Proxy` as analyzer.

### Tier 3: Service Implementations

- **FR-009**: Each plugin SHOULD have a service implementation if business logic is needed.
- **FR-010**: Services MAY delegate to Tier 4 providers if multiple backends exist.
- **FR-011**: Services MUST implement the contract interface from Tier 1.

### Tier 4: Providers (Backend Implementations)

- **FR-012**: Plugins with multiple backend options MUST use provider pattern.
- **FR-013**: Provider interfaces SHOULD be defined in contract projects when applicable.
- **FR-014**: Providers MUST be registered with `IRegistry` using appropriate keys/strategies.

### Build & Integration

- **FR-015**: All plugins MUST build successfully with zero errors.
- **FR-016**: All plugins MUST be added to the solution file.
- **FR-017**: Plugins SHOULD have corresponding test projects.
- **FR-018**: Solution MUST build successfully after all additions.

## Gap Analysis: Contracts vs Plugins

### Contracts WITH existing plugins (7/18)

1. ✅ **Analytics** → `LablabBean.Plugins.Analytics` (exists)
2. ✅ **Config** → `LablabBean.Plugins.ConfigManager` (exists)
3. ✅ **Input** → `LablabBean.Plugins.InputHandler` (exists)
4. ✅ **Resource** → `LablabBean.Plugins.ResourceLoader` (exists)
5. ✅ **Scene** → `LablabBean.Plugins.SceneLoader` (exists)
6. ✅ **Game** → `LablabBean.Plugins.MockGame`, `LablabBean.Plugins.Inventory`, `LablabBean.Plugins.StatusEffects` (exists)
7. ✅ **UI** → `LablabBean.Plugins.ReactiveUI` (exists)

### Contracts MISSING plugins (11/18)

8. ❌ **Audio** → NO PLUGIN
9. ❌ **Diagnostic** → NO PLUGIN
10. ❌ **Firebase** → NO PLUGIN
11. ❌ **Localization** → NO PLUGIN
12. ❌ **ObjectPool** → NO PLUGIN
13. ❌ **Performance** → NO PLUGIN
14. ❌ **PersistentStorage** → NO PLUGIN
15. ❌ **Resilience** → NO PLUGIN (CRITICAL - needs Polly implementation)
16. ❌ **Scheduler** → NO PLUGIN
17. ❌ **Serialization** → NO PLUGIN
18. ❌ **ServiceHealth** → NO PLUGIN

### CRITICAL: ALL Contracts Missing Tier 2 Proxies (18/18)

All contracts created in SPEC-012 are missing `Services/Proxy/Service.cs` files with `[RealizeService]` attributes.

## Plugins to Implement

### Priority 1: Essential Foundations

#### 1. Resilience.Polly Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Resilience.Polly/`
- **Purpose**: Implement resilience patterns using Polly library
- **NuGet Packages**: `Polly`, `Polly.Extensions`
- **Providers**:
  - `PollyRetryProvider` - Retry policies
  - `PollyCircuitBreakerProvider` - Circuit breaker
  - `PollyTimeoutProvider` - Timeout policies
  - `PollyBulkheadProvider` - Bulkhead isolation
- **Service**: `ResilienceService` - Coordinates policies and providers

#### 2. Serialization.Json Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Serialization.Json/`
- **Purpose**: JSON serialization using System.Text.Json
- **NuGet Packages**: `System.Text.Json`
- **Provider**: `JsonSerializationProvider`
- **Service**: `SerializationService` - Format routing and validation

#### 3. Serialization.Xml Plugin (Optional)

- **Location**: `dotnet/plugins/LablabBean.Plugins.Serialization.Xml/`
- **Purpose**: XML serialization using System.Xml.Serialization
- **Provider**: `XmlSerializationProvider`

#### 4. ObjectPool.Standard Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.ObjectPool.Standard/`
- **Purpose**: Generic object pooling implementation
- **NuGet Packages**: `Microsoft.Extensions.ObjectPool`
- **Service**: `ObjectPoolService` - Pool management and lifecycle

### Priority 2: Data & Storage

#### 5. PersistentStorage.Json Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.PersistentStorage.Json/`
- **Purpose**: JSON file-based storage
- **NuGet Packages**: `System.Text.Json`
- **Provider**: `JsonFileStorageProvider`
- **Service**: `PersistentStorageService` - Provider coordination

#### 6. PersistentStorage.LiteDB Plugin (Optional)

- **Location**: `dotnet/plugins/LablabBean.Plugins.PersistentStorage.LiteDB/`
- **Purpose**: Embedded database storage
- **NuGet Packages**: `LiteDB`
- **Provider**: `LiteDBStorageProvider`

#### 7. Localization.Json Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Localization.Json/`
- **Purpose**: JSON-based localization resources
- **NuGet Packages**: `System.Text.Json`
- **Provider**: `JsonLocalizationProvider`
- **Service**: `LocalizationService` - Locale management, fallback logic

#### 8. Scheduler.Standard Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Scheduler.Standard/`
- **Purpose**: Task scheduling with System.Threading.Timer
- **Provider**: `TimerSchedulerProvider`
- **Service**: `SchedulerService` - Task lifecycle, cancellation

### Priority 3: Monitoring & Diagnostics

#### 9. Diagnostic.Console Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Diagnostic.Console/`
- **Purpose**: Console-based diagnostic output
- **Provider**: `ConsoleDiagnosticProvider`
- **Service**: `DiagnosticService` - Event collection, provider coordination

#### 10. Performance.Standard Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Performance.Standard/`
- **Purpose**: Performance metrics collection
- **NuGet Packages**: `System.Diagnostics.PerformanceCounter` (Windows), cross-platform alternatives
- **Service**: `PerformanceService` - Metric aggregation, recommendations

#### 11. ServiceHealth.Standard Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.ServiceHealth.Standard/`
- **Purpose**: Service health monitoring
- **Service**: `ServiceHealthService` - Health checks, status reporting

### Priority 4: Optional/Future

#### 12. Audio.NAudio Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Audio.NAudio/`
- **Purpose**: Audio playback using NAudio
- **NuGet Packages**: `NAudio`
- **Provider**: `NAudioProvider`
- **Service**: `AudioService` - Playback lifecycle, volume mixing

#### 13. Audio.BASS Plugin (Alternative)

- **Location**: `dotnet/plugins/LablabBean.Plugins.Audio.BASS/`
- **Purpose**: Audio playback using BASS library
- **Provider**: `BassAudioProvider`

#### 14. Analytics.Console Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Analytics.Console/`
- **Purpose**: Console logging for analytics events
- **Service**: `AnalyticsService` - Event tracking (already exists, may just need update)

#### 15. Firebase.Standard Plugin

- **Location**: `dotnet/plugins/LablabBean.Plugins.Firebase.Standard/`
- **Purpose**: Firebase integration for .NET
- **NuGet Packages**: `FirebaseAdmin`, `Google.Cloud.Firestore`
- **Service**: `FirebaseService` - Firebase initialization and coordination

## Success Criteria (mandatory)

- **SC-001**: All 18 contracts have Tier 2 proxy services with `[RealizeService]` attributes.
- **SC-002**: Minimum 8 Tier 3/4 plugin projects created (Priority 1 & 2).
- **SC-003**: All plugins build successfully with zero errors.
- **SC-004**: All plugins properly registered in solution file.
- **SC-005**: Resilience.Polly plugin fully implements circuit breaker and retry patterns.
- **SC-006**: Serialization.Json plugin handles all common scenarios.
- **SC-007**: ObjectPool.Standard plugin provides functional pooling.
- **SC-008**: Solution builds successfully with all new plugins.

## Assumptions

- Polly library is the standard for resilience patterns in .NET.
- System.Text.Json is preferred over Newtonsoft.Json for new code.
- Microsoft.Extensions.ObjectPool provides adequate pooling capabilities.
- LiteDB is suitable for embedded database scenarios.
- NAudio is the standard for .NET audio playback.

## Out of Scope

- Advanced audio features (3D spatialization, DSP effects, mixing).
- Cloud storage providers (Azure, AWS S3).
- Enterprise monitoring integrations (Application Insights, Datadog).
- Advanced scheduling (cron expressions, distributed scheduling).
- Production-grade Firebase implementations (requires specific project setup).

## Dependencies

- **SPEC-012**: Missing Service Contracts (Tier 1) - COMPLETE
- **SPEC-009**: Proxy Service Source Generator (for Tier 2) - Should exist
- NuGet packages: Polly, System.Text.Json, LiteDB, NAudio, etc.

## Implementation Phases

### Phase 0: CRITICAL - Add Missing Proxy Services

Create Tier 2 proxy services for ALL 18 contracts:

- Add `Services/Proxy/Service.cs` to each contract project
- Apply `[RealizeService(typeof(IService))]` attribute
- Reference SourceGenerators.Proxy analyzer
- Verify source generation works

**Estimated Time**: 3-4 hours

### Phase 1: Priority 1 Plugins (Essential)

Implement core infrastructure plugins:

- Resilience.Polly (most critical)
- Serialization.Json
- ObjectPool.Standard

**Estimated Time**: 6-8 hours

### Phase 2: Priority 2 Plugins (Data & Storage)

Implement data handling plugins:

- PersistentStorage.Json
- Localization.Json
- Scheduler.Standard

**Estimated Time**: 5-7 hours

### Phase 3: Priority 3 Plugins (Monitoring)

Implement diagnostic plugins:

- Diagnostic.Console
- Performance.Standard
- ServiceHealth.Standard

**Estimated Time**: 4-6 hours

### Phase 4: Optional Plugins (Future)

Implement remaining plugins as needed:

- Audio.NAudio
- Firebase.Standard
- Additional providers

**Estimated Time**: 6-10 hours

## Plugin Template Structure

```
LablabBean.Plugins.{Service}.{Provider}/
├── LablabBean.Plugins.{Service}.{Provider}.csproj
├── {Service}Plugin.cs (implements IPlugin)
├── Services/
│   └── {Service}Service.cs (Tier 3 - if needed)
├── Providers/ (Tier 4 - if multiple backends)
│   └── {Provider}{Service}Provider.cs
└── README.md (usage documentation)
```

## .csproj Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj" />
    <ProjectReference Include="..\..\framework\LablabBean.Contracts.{Service}\LablabBean.Contracts.{Service}.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Provider-specific NuGet packages -->
    <PackageReference Include="{ProviderPackage}" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
```

## Plugin Implementation Template

```csharp
using LablabBean.Plugins.Contracts;
using LablabBean.Contracts.{Service}.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LablabBean.Plugins.{Service}.{Provider};

/// <summary>
/// Plugin that provides {Service} capabilities using {Provider}.
/// </summary>
public class {Service}Plugin : IPlugin
{
    private ILogger? _logger;

    public string Id => "lablab-bean.{service}.{provider}";
    public string Name => "{Service} Plugin ({Provider})";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;

        // Create service instance
        var loggerFactory = context.Host.Services.GetRequiredService<ILoggerFactory>();
        var service = new {Service}Service(loggerFactory.CreateLogger<{Service}Service>());

        // Register with registry
        context.Registry.Register<IService>(service);

        _logger.LogInformation("{Service} plugin ({Provider}) initialized", Name, "{Provider}");
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("{Service} plugin started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("{Service} plugin stopped");
        return Task.CompletedTask;
    }
}
```

## Proxy Service Template (CRITICAL)

```csharp
using System;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Contracts.Attributes;

namespace LablabBean.Contracts.{Service}.Services.Proxy;

/// <summary>
/// Proxy implementation of IService that delegates to the plugin registry.
/// The actual implementation is generated by the ProxyServiceGenerator.
/// </summary>
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

## Priority Provider Implementations

### Resilience.Polly - Example

```csharp
// Providers/PollyRetryProvider.cs
using Polly;
using Polly.Retry;

namespace LablabBean.Plugins.Resilience.Polly.Providers;

public class PollyRetryProvider : IRetryPolicy
{
    private readonly AsyncRetryPolicy _policy;

    public PollyRetryProvider(int maxRetries, TimeSpan delay)
    {
        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + delay
            );
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        return await _policy.ExecuteAsync(async () => await action());
    }
}
```

## Notes

- **CRITICAL**: Phase 0 (proxy services) MUST be completed before plugins can work correctly.
- Follow existing plugin patterns from Reporting.Csv and Reporting.Html.
- Use dependency injection for all dependencies.
- Implement proper logging using ILogger.
- Each plugin should be self-contained and independently testable.
- Provider pattern allows multiple backend implementations per service.

## References

- **SPEC-011**: .NET Naming and Architecture Adjustment
- **SPEC-012**: Missing Service Contracts
- **Existing Plugins**: `dotnet/plugins/LablabBean.Plugins.Reporting.{Csv,Html}/`
- **Four-Tier Architecture**: Contracts → Proxies → Services → Providers
- **Polly Documentation**: <https://github.com/App-vNext/Polly>
- **NAudio Documentation**: <https://github.com/naudio/NAudio>
