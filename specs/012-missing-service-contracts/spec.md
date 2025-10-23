# Feature Specification: Missing Service Contracts

**Feature Branch**: `012-missing-service-contracts`
**Created**: 2025-10-23
**Status**: Draft
**Reference**: `ref-projects/soy-bean/packages/scoped-3208/com.giantcroissant.yokan.game/Runtime/Core`

## Executive Summary

This specification defines the implementation of 12 missing service contract projects in the dotnet framework, based on the reference implementation from the soy-bean Unity project. These services provide comprehensive functionality for diagnostics, object pooling, audio, localization, storage, performance monitoring, scheduling, serialization, analytics, Firebase integration, service health, and resilience patterns.

## User Scenarios & Testing (mandatory)

### User Story 1 - Developer uses diagnostic services (Priority: P1)

As a developer, I need comprehensive diagnostic and monitoring capabilities so I can track system health, performance metrics, and troubleshoot issues in production.

- Acceptance Scenarios:
  1. Given the Diagnostic service, when I start collection, then diagnostic data is captured at specified intervals.
  2. Given performance thresholds, when metrics exceed limits, then performance alerts are generated.
  3. Given diagnostic events, when I log an exception, then it's captured with full context and breadcrumbs.

---

### User Story 2 - Developer uses object pooling (Priority: P1)

As a developer, I need efficient object pooling so I can reduce garbage collection pressure and improve performance in resource-intensive scenarios.

- Acceptance Scenarios:
  1. Given an ObjectPool service, when I create a pool, then objects are pre-allocated according to configuration.
  2. Given a pool with max size, when capacity is reached, then additional requests are handled according to policy.
  3. Given pool statistics, when I query metrics, then accurate usage data is provided.

---

### User Story 3 - Developer uses audio services (Priority: P1)

As a developer, I need audio playback capabilities so I can provide sound effects, music, and audio feedback in the application.

- Acceptance Scenarios:
  1. Given an Audio service, when I play a sound, then it plays with correct volume, pitch, and spatial settings.
  2. Given audio categories, when I adjust category volume, then all sounds in that category are affected.
  3. Given audio handles, when I control playback, then I can pause, resume, and stop sounds dynamically.

---

### User Story 4 - Developer uses localization (Priority: P2)

As a developer, I need localization support so users can experience the application in their preferred language.

- Acceptance Scenarios:
  1. Given multiple languages, when I change locale, then all localized strings update accordingly.
  2. Given missing translations, when I request a key, then fallback language is used.
  3. Given localization changes, when locale changes, then change events notify subscribers.

---

### User Story 5 - Developer uses persistent storage (Priority: P2)

As a developer, I need persistent storage so I can save and retrieve user data across application sessions.

- Acceptance Scenarios:
  1. Given storage service, when I save data, then it persists across application restarts.
  2. Given storage backends, when I query data, then the correct provider handles the request.
  3. Given storage operations, when errors occur, then appropriate error handling is triggered.

---

### User Story 6 - Developer uses serialization (Priority: P2)

As a developer, I need flexible serialization so I can convert objects to/from various formats (JSON, Binary, etc.).

- Acceptance Scenarios:
  1. Given serialization service, when I serialize an object, then it's converted to the requested format.
  2. Given deserialization, when I parse data, then objects are reconstructed correctly.
  3. Given type information, when serializing, then type metadata is preserved for polymorphic scenarios.

---

### User Story 7 - Developer uses performance monitoring (Priority: P3)

As a developer, I need performance metrics so I can identify bottlenecks and optimize critical paths.

- Acceptance Scenarios:
  1. Given performance service, when I measure operations, then accurate timing data is captured.
  2. Given performance budgets, when thresholds are exceeded, then warnings are generated.
  3. Given performance history, when I query trends, then historical data is available.

---

### User Story 8 - Developer uses scheduler (Priority: P3)

As a developer, I need task scheduling so I can execute operations at specific times or intervals.

- Acceptance Scenarios:
  1. Given scheduler service, when I schedule a task, then it executes at the specified time.
  2. Given recurring tasks, when scheduled, then they execute according to the cron expression or interval.
  3. Given task cancellation, when I cancel a scheduled task, then it stops executing.

---

### User Story 9 - Developer uses analytics (Priority: P3)

As a developer, I need analytics tracking so I can understand user behavior and application usage patterns.

- Acceptance Scenarios:
  1. Given analytics service, when I track an event, then it's sent to configured analytics providers.
  2. Given user properties, when set, then they're included in all subsequent events.
  3. Given custom parameters, when tracked, then they're properly formatted for each provider.

---

### User Story 10 - Developer uses Firebase integration (Priority: P3)

As a developer, I need Firebase services so I can leverage cloud features like remote config, analytics, and authentication.

- Acceptance Scenarios:
  1. Given Firebase service, when initialized, then all Firebase modules are properly configured.
  2. Given remote config, when fetched, then latest values are retrieved and cached.
  3. Given Firebase events, when triggered, then they're properly logged to Firebase Analytics.

---

### User Story 11 - Developer uses resilience patterns (Priority: P3)

As a developer, I need resilience capabilities so I can handle transient failures and network issues gracefully.

- Acceptance Scenarios:
  1. Given retry policy, when an operation fails, then it's retried according to configured strategy.
  2. Given circuit breaker, when failure threshold is reached, then circuit opens to prevent cascading failures.
  3. Given timeout policy, when operation exceeds limit, then it's cancelled appropriately.

---

### User Story 12 - Developer monitors service health (Priority: P3)

As a developer, I need service health monitoring so I can track the status of all application services.

- Acceptance Scenarios:
  1. Given health service, when I check health, then status of all registered services is returned.
  2. Given unhealthy services, when detected, then health degradation events are raised.
  3. Given health checks, when performed, then response times and error rates are tracked.

## Requirements (mandatory)

### Naming & Structure

- **FR-001**: All contract projects MUST follow naming pattern `LablabBean.Contracts.{Service}`.
- **FR-002**: All contract projects MUST target `netstandard2.1` for compatibility.
- **FR-003**: All contract projects MUST use `ImplicitUsings=disable` and explicit using statements.
- **FR-004**: All contract projects MUST include `IsExternalInit` polyfill for C# records.

### Contract Architecture

- **FR-005**: Each service MUST have an `IService` interface in `Services/` directory.
- **FR-006**: Each service MUST have supporting types in appropriate subdirectories (Classes/, Enums/, Models/).
- **FR-007**: Each service SHOULD have Events defined if applicable.
- **FR-008**: Each service MAY have Extensions defined for convenience methods.

### Proxy Services

- **FR-009**: Each contract project MUST include proxy service with `[RealizeService]` attribute.
- **FR-010**: Proxy services MUST be in `Services/Proxy/` namespace.
- **FR-011**: Proxy services MUST reference `LablabBean.SourceGenerators.Proxy` as analyzer.
- **FR-012**: Proxy services MUST delegate to `IRegistry` for service resolution.

### Adaptation from Unity

- **FR-013**: Unity-specific types (GameObject, Transform, etc.) MUST be removed or abstracted.
- **FR-014**: UniTask MUST be replaced with standard Task/ValueTask.
- **FR-015**: UniRx observables MUST be replaced with System.Reactive or removed.
- **FR-016**: ScriptableObject patterns MUST be replaced with plain C# classes or removed.

### Build & Verification

- **FR-017**: All new contract projects MUST build successfully with zero errors.
- **FR-018**: All new contract projects MUST be added to the solution file.
- **FR-019**: All new contract projects SHOULD have corresponding test projects.
- **FR-020**: Solution MUST build successfully after all additions.

## Services to Implement

### Priority 1: Core Services (Essential)

#### 1. Diagnostic Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Diagnostic/`
- **Purpose**: System monitoring, performance tracking, health checks, observability
- **Key Interfaces**: `IService`, `IDiagnosticProvider`, `IDiagnosticSpan`
- **Key Types**: `DiagnosticEvent`, `DiagnosticData`, `PerformanceMetrics`, `SystemHealth`, `PerformanceAlert`
- **Key Enums**: `DiagnosticLevel`, `SystemHealth`, `DiagnosticExportFormat`
- **Adaptat ions**: Remove Unity-specific diagnostics, replace UniTask with Task

#### 2. ObjectPool Service

- **Location**: `dotnet/framework/LablabBean.Contracts.ObjectPool/`
- **Purpose**: Object pooling for memory efficiency and performance
- **Key Interfaces**: `IService`, `IObjectPool<T>`
- **Key Types**: `PoolStatistics`, `ObjectPoolStatistics`, `PoolInfo`, `ObjectPoolGlobalSettings`
- **Adaptations**: Remove `IGameObjectPool` (Unity-specific), keep generic `IObjectPool<T>`

#### 3. Audio Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Audio/`
- **Purpose**: Audio playback and management
- **Key Interfaces**: `IService`
- **Key Types**: `AudioRequest`, `AudioHandle`, `AudioStats`
- **Key Enums**: `AudioCategory`, `AudioPriority`, `AudioSourceType`
- **Adaptations**: Remove Unity AudioClip/AudioSource, use abstract handle pattern

### Priority 2: Data Services

#### 4. Localization Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Localization/`
- **Purpose**: Multi-language support and string localization
- **Key Interfaces**: `IService`
- **Key Enums**: `LocalizationFormat`
- **Adaptations**: Use standard CultureInfo, replace UniTask with Task

#### 5. PersistentStorage Service

- **Location**: `dotnet/framework/LablabBean.Contracts.PersistentStorage/`
- **Purpose**: Data persistence across application sessions
- **Key Interfaces**: `IService`
- **Key Types**: `StorageRequest`, `StorageStatistics`, `StorageDebugInfo`
- **Key Enums**: `StorageProviderType`
- **Adaptations**: Replace UniTask with Task

#### 6. Serialization Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Serialization/`
- **Purpose**: Object serialization/deserialization
- **Key Interfaces**: `IService`
- **Adaptations**: Use standard .NET serialization patterns, replace UniTask with Task

### Priority 3: Supporting Services

#### 7. Performance Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Performance/`
- **Purpose**: Performance monitoring and profiling
- **Key Interfaces**: `IService`
- **Adaptations**: Remove Unity Profiler integration

#### 8. Scheduler Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Scheduler/`
- **Purpose**: Task scheduling and timing
- **Key Interfaces**: `IService`
- **Adaptations**: Replace UniTask with Task, use standard System.Threading.Timer

#### 9. Analytics Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Analytics/`
- **Purpose**: Event tracking and analytics
- **Key Interfaces**: `IService`
- **Adaptations**: Remove Unity Analytics specifics, keep provider-agnostic interface

#### 10. Firebase Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Firebase/`
- **Purpose**: Firebase integration (Remote Config, Analytics, etc.)
- **Key Interfaces**: `IService`
- **Key Enums**: `FirebaseServiceType`
- **Adaptations**: Use .NET Firebase SDK patterns

#### 11. ServiceHealth Service

- **Location**: `dotnet/framework/LablabBean.Contracts.ServiceHealth/`
- **Purpose**: Service health monitoring and status tracking
- **Key Interfaces**: `IService`
- **Adaptations**: Replace UniTask with Task

#### 12. Resilience Service

- **Location**: `dotnet/framework/LablabBean.Contracts.Resilience/`
- **Purpose**: Fault tolerance, retry logic, circuit breakers
- **Key Interfaces**: `IService`, `IRetryPolicy`, `ICircuitBreaker`, `ITimeout`
- **Adaptations**: Align with Polly library patterns, replace UniTask with Task

## Success Criteria (mandatory)

- **SC-001**: All 12 contract projects created and added to solution.
- **SC-002**: All contract projects build successfully with zero errors.
- **SC-003**: Each contract project includes proper proxy service with source generator integration.
- **SC-004**: All Unity-specific dependencies removed or abstracted.
- **SC-005**: All UniTask replaced with standard Task/ValueTask.
- **SC-006**: Solution builds successfully with all new projects.
- **SC-007**: Naming conventions consistent with existing SPEC-011 patterns.

## Assumptions

- The source generator `LablabBean.SourceGenerators.Proxy` is functional and tested (from SPEC-009).
- The plugin system (`LablabBean.Plugins.Contracts`, `IRegistry`) is stable.
- .NET Standard 2.1 is the appropriate target framework for all contracts.
- Reference implementation in soy-bean provides accurate interface definitions.

## Out of Scope

- Actual implementation of services (plugins) - only contracts/interfaces.
- Unit tests for contract projects (can be added later).
- Plugin implementations for any of the services.
- Migration of existing code to use new services.
- Documentation beyond inline XML comments.

## Dependencies

- `LablabBean.Plugins.Contracts` - For IRegistry, IPlugin, ServiceMetadata
- `LablabBean.SourceGenerators.Proxy` - For proxy service generation
- Reference project: `ref-projects/soy-bean/.../Runtime/Core/` for interface definitions

## Implementation Phases

### Phase 1: Project Setup (All Services)

Create all 12 contract projects with basic structure:

- Create project directories
- Create .csproj files with correct references
- Add Polyfills.cs for IsExternalInit
- Add to solution file

### Phase 2: Priority 1 Services (Diagnostic, ObjectPool, Audio)

Implement interfaces and types for essential services:

- Port interfaces from reference
- Adapt Unity-specific types
- Create supporting classes/enums
- Add proxy services

### Phase 3: Priority 2 Services (Localization, PersistentStorage, Serialization)

Implement interfaces and types for data services:

- Port interfaces from reference
- Adapt Unity-specific types
- Create supporting classes/enums
- Add proxy services

### Phase 4: Priority 3 Services (Performance, Scheduler, Analytics, Firebase, ServiceHealth, Resilience)

Implement interfaces and types for supporting services:

- Port interfaces from reference
- Adapt Unity-specific types
- Create supporting classes/enums
- Add proxy services

### Phase 5: Build Verification

- Build all contract projects individually
- Build entire solution
- Verify all proxy services generate correctly
- Fix any compilation issues

## Migration Guide

### Unity to .NET Standard Adaptations

#### UniTask → Task

```csharp
// Unity (Before)
UniTask<T> MethodAsync(CancellationToken ct = default);

// .NET Standard (After)
Task<T> MethodAsync(CancellationToken ct = default);
```

#### IObservable (UniRx) → System.Reactive

```csharp
// Unity (Before)
using UniRx;
IObservable<T> Stream { get; }

// .NET Standard (After)
using System;
IObservable<T> Stream { get; }
// OR remove if System.Reactive not available
```

#### GameObject/Component → Abstract Types

```csharp
// Unity (Before)
IGameObjectPool CreatePoolAsync(GameObject prefab, ...);

// .NET Standard (After)
// Remove entirely or create abstract IPoolableObject interface
```

#### ScriptableObject → Plain Classes

```csharp
// Unity (Before)
public class Settings : ScriptableObject { }

// .NET Standard (After)
public class Settings { }
```

## File Structure Template

Each service contract project should follow this structure:

```
LablabBean.Contracts.{Service}/
├── LablabBean.Contracts.{Service}.csproj
├── Polyfills.cs (IsExternalInit)
├── Services/
│   ├── IService.cs (main service interface)
│   └── Proxy/
│       └── Service.cs (proxy implementation with [RealizeService])
├── Models/ (if needed)
│   └── *.cs (data models)
├── Classes/ (if needed)
│   └── *.cs (supporting classes)
├── Enums/ (if needed)
│   └── *.cs (enumerations)
├── Events.cs (if applicable)
└── Extensions.cs (if applicable)
```

## Notes

- Follow SPEC-011 naming conventions: `LablabBean.Contracts.*`
- Use source generators for proxy services as established in SPEC-009
- Reference soy-bean implementation but adapt for .NET Standard
- Prioritize P1 services for immediate value
- Keep contracts platform-agnostic (no Unity, no Windows-specific code)
- Use explicit `using` statements (ImplicitUsings disabled)
- Include comprehensive XML documentation comments

## References

- **SPEC-009**: Proxy Service Source Generator
- **SPEC-011**: .NET Naming and Architecture Adjustment
- **Reference Implementation**: `ref-projects/soy-bean/packages/scoped-3208/com.giantcroissant.yokan.game/Runtime/Core/`
- **Four-Tier Architecture**: Contracts (Tier 1) → Proxies (Tier 2) → Services (Tier 3) → Providers (Tier 4)
