# Tasks: Missing Plugin Implementations (SPEC-013)

**Input**: `specs/013-missing-plugin-implementations/spec.md`
**Dependencies**: SPEC-012 (Missing Service Contracts)

**Objective**: Implement Tier 2 proxy services for all contracts, and Tier 3/4 plugin implementations for 11 missing services.

---

## Format: `[ID] [P] Description`

- **[P]**: Can run in parallel (no ordering/dependency conflicts)
- **[C]**: Critical path item

## Path Conventions

- Proxy services: `dotnet/framework/LablabBean.Contracts.{Service}/Services/Proxy/Service.cs`
- Plugin projects: `dotnet/plugins/LablabBean.Plugins.{Service}.{Provider}/`
- Test projects: `dotnet/tests/LablabBean.Plugins.{Service}.{Provider}.Tests/`

---

## Phase 0: CRITICAL - Add Missing Proxy Services (Tier 2)

**Purpose**: Create proxy services for ALL 18 contracts to complete Tier 2 architecture.

### New Contracts from SPEC-012 (12 services)

- [ ] T001 [C] Create proxy service for Analytics contract
- [ ] T002 [C] Create proxy service for Audio contract
- [ ] T003 [C] Create proxy service for Diagnostic contract
- [ ] T004 [C] Create proxy service for Firebase contract
- [ ] T005 [C] Create proxy service for Localization contract
- [ ] T006 [C] Create proxy service for ObjectPool contract
- [ ] T007 [C] Create proxy service for Performance contract
- [ ] T008 [C] Create proxy service for PersistentStorage contract
- [ ] T009 [C] Create proxy service for Resilience contract
- [ ] T010 [C] Create proxy service for Scheduler contract
- [ ] T011 [C] Create proxy service for Serialization contract
- [ ] T012 [C] Create proxy service for ServiceHealth contract

### Existing Contracts (6 services)

- [ ] T013 [P] Create proxy service for Config contract
- [ ] T014 [P] Create proxy service for Game contract
- [ ] T015 [P] Create proxy service for Input contract
- [ ] T016 [P] Create proxy service for Resource contract
- [ ] T017 [P] Create proxy service for Scene contract
- [ ] T018 [P] Create proxy service for UI contract

**Checkpoint 1**: All 18 contracts have proxy services with [RealizeService] attributes.

- [ ] T019 Verify all proxy services build successfully
- [ ] T020 Verify source generator creates delegation code in obj/generated/

---

## Phase 1: Priority 1 Plugins (Essential Infrastructure)

### Resilience.Polly Plugin (CRITICAL)

- [ ] T030 Create `LablabBean.Plugins.Resilience.Polly/` project directory and .csproj
- [ ] T031 Add NuGet packages: Polly, Polly.Extensions, Microsoft.Extensions.Logging.Abstractions
- [ ] T032 Add project references: LablabBean.Plugins.Contracts, LablabBean.Contracts.Resilience
- [ ] T033 Create `ResiliencePlugin.cs` implementing IPlugin
- [ ] T034 Create `Services/ResilienceService.cs` (Tier 3) implementing IService
- [ ] T035 Create `Providers/PollyRetryProvider.cs` implementing IRetryPolicy
- [ ] T036 Create `Providers/PollyCircuitBreakerProvider.cs` implementing ICircuitBreaker
- [ ] T037 Create `Providers/PollyTimeoutProvider.cs` (optional)
- [ ] T038 Implement InitializeAsync to register service with IRegistry
- [ ] T039 Build and verify plugin loads correctly

**Checkpoint 2**: Resilience.Polly plugin implements retry and circuit breaker patterns.

### Serialization.Json Plugin

- [ ] T040 Create `LablabBean.Plugins.Serialization.Json/` project
- [ ] T041 Add NuGet package: System.Text.Json
- [ ] T042 Create `SerializationPlugin.cs` implementing IPlugin
- [ ] T043 Create `Services/SerializationService.cs` implementing IService
- [ ] T044 Implement Serialize/Deserialize methods with System.Text.Json
- [ ] T045 Handle edge cases: null, empty, invalid JSON
- [ ] T046 Add configuration options (camelCase, indentation, etc.)
- [ ] T047 Build and test serialization round-trips

**Checkpoint 3**: Serialization.Json plugin handles all common scenarios.

### ObjectPool.Standard Plugin

- [ ] T050 Create `LablabBean.Plugins.ObjectPool.Standard/` project
- [ ] T051 Add NuGet package: Microsoft.Extensions.ObjectPool
- [ ] T052 Create `ObjectPoolPlugin.cs` implementing IPlugin
- [ ] T053 Create `Services/ObjectPoolService.cs` implementing IService
- [ ] T054 Implement CreatePoolAsync with pre-allocation logic
- [ ] T055 Implement Get/Return pool lifecycle methods
- [ ] T056 Implement pool statistics tracking
- [ ] T057 Add cleanup/disposal logic
- [ ] T058 Build and test pool operations

**Checkpoint 4**: ObjectPool.Standard plugin provides functional pooling.

---

## Phase 2: Priority 2 Plugins (Data & Storage)

### PersistentStorage.Json Plugin

- [ ] T060 Create `LablabBean.Plugins.PersistentStorage.Json/` project
- [ ] T061 Add NuGet package: System.Text.Json
- [ ] T062 Create `PersistentStoragePlugin.cs` implementing IPlugin
- [ ] T063 Create `Services/PersistentStorageService.cs` implementing IService
- [ ] T064 Create `Providers/JsonFileStorageProvider.cs`
- [ ] T065 Implement Save/Load methods with file I/O
- [ ] T066 Implement backup/restore functionality
- [ ] T067 Add error handling for file access
- [ ] T068 Build and test file operations

**Checkpoint 5**: PersistentStorage.Json plugin saves and loads data correctly.

### Localization.Json Plugin

- [ ] T070 Create `LablabBean.Plugins.Localization.Json/` project
- [ ] T071 Add NuGet package: System.Text.Json
- [ ] T072 Create `LocalizationPlugin.cs` implementing IPlugin
- [ ] T073 Create `Services/LocalizationService.cs` implementing IService
- [ ] T074 Create `Providers/JsonLocalizationProvider.cs`
- [ ] T075 Implement locale loading from JSON files
- [ ] T076 Implement fallback language logic
- [ ] T077 Implement string formatting and pluralization (basic)
- [ ] T078 Add locale change event notifications
- [ ] T079 Build and test locale switching

**Checkpoint 6**: Localization.Json plugin supports multi-language strings.

### Scheduler.Standard Plugin

- [ ] T080 Create `LablabBean.Plugins.Scheduler.Standard/` project
- [ ] T081 Create `SchedulerPlugin.cs` implementing IPlugin
- [ ] T082 Create `Services/SchedulerService.cs` implementing IService
- [ ] T083 Create `Providers/TimerSchedulerProvider.cs`
- [ ] T084 Implement ScheduleDelayedAsync using System.Threading.Timer
- [ ] T085 Implement ScheduleRepeatingAsync for recurring tasks
- [ ] T086 Implement task cancellation logic
- [ ] T087 Implement task state management (running, paused, cancelled)
- [ ] T088 Build and test scheduling operations

**Checkpoint 7**: Scheduler.Standard plugin schedules and executes tasks.

---

## Phase 3: Priority 3 Plugins (Monitoring & Diagnostics)

### Diagnostic.Console Plugin

- [ ] T090 Create `LablabBean.Plugins.Diagnostic.Console/` project
- [ ] T091 Create `DiagnosticPlugin.cs` implementing IPlugin
- [ ] T092 Create `Services/DiagnosticService.cs` implementing IService
- [ ] T093 Create `Providers/ConsoleDiagnosticProvider.cs`
- [ ] T094 Implement diagnostic event collection and logging
- [ ] T095 Implement performance metrics formatting
- [ ] T096 Implement health check execution
- [ ] T097 Add console output with colors/formatting
- [ ] T098 Build and test diagnostic output

**Checkpoint 8**: Diagnostic.Console plugin outputs diagnostic information.

### Performance.Standard Plugin

- [ ] T100 Create `LablabBean.Plugins.Performance.Standard/` project
- [ ] T101 Create `PerformancePlugin.cs` implementing IPlugin
- [ ] T102 Create `Services/PerformanceService.cs` implementing IService
- [ ] T103 Implement metric recording (counters, gauges, timers)
- [ ] T104 Implement activity tracking with duration measurement
- [ ] T105 Implement statistics aggregation
- [ ] T106 Implement performance recommendations logic
- [ ] T107 Build and test metric collection

**Checkpoint 9**: Performance.Standard plugin collects and aggregates metrics.

### ServiceHealth.Standard Plugin

- [ ] T110 Create `LablabBean.Plugins.ServiceHealth.Standard/` project
- [ ] T111 Create `ServiceHealthPlugin.cs` implementing IPlugin
- [ ] T112 Create `Services/ServiceHealthService.cs` implementing IService
- [ ] T113 Implement health check registration
- [ ] T114 Implement health check execution
- [ ] T115 Implement system health reporting
- [ ] T116 Add health status aggregation logic
- [ ] T117 Build and test health monitoring

**Checkpoint 10**: ServiceHealth.Standard plugin monitors service health.

---

## Phase 4: Optional Plugins (Future Implementation)

### Audio.NAudio Plugin

- [ ] T120 Create `LablabBean.Plugins.Audio.NAudio/` project
- [ ] T121 Add NuGet package: NAudio
- [ ] T122 Create `AudioPlugin.cs` implementing IPlugin
- [ ] T123 Create `Services/AudioService.cs` implementing IService
- [ ] T124 Create `Providers/NAudioProvider.cs`
- [ ] T125 Implement PlayAsync with NAudio WaveOut
- [ ] T126 Implement volume/pitch control
- [ ] T127 Implement spatial audio (panning)
- [ ] T128 Implement audio statistics tracking
- [ ] T129 Build and test audio playback

### Analytics.Console Plugin (Update Existing)

- [ ] T130 Review existing Analytics plugin
- [ ] T131 Update to implement new Analytics contract interface
- [ ] T132 Add console output for tracked events
- [ ] T133 Build and verify integration

### Firebase.Standard Plugin

- [ ] T140 Create `LablabBean.Plugins.Firebase.Standard/` project
- [ ] T141 Add NuGet packages: FirebaseAdmin, Google.Cloud.Firestore
- [ ] T142 Create `FirebasePlugin.cs` implementing IPlugin
- [ ] T143 Create `Services/FirebaseService.cs` implementing IService
- [ ] T144 Implement Firebase initialization
- [ ] T145 Implement dependency checking
- [ ] T146 Build and test Firebase connection

---

## Phase 5: Build Verification & Integration

- [ ] T150 [P] Build all plugin projects individually
- [ ] T151 Add all new plugins to `dotnet/LablabBean.sln`
- [ ] T152 Build entire solution and verify 0 errors
- [ ] T153 Verify all proxy services generated code correctly
- [ ] T154 Run all existing tests to ensure no regressions
- [ ] T155 Create smoke tests for each new plugin

**Checkpoint 11**: Full solution builds with all new plugins.

---

## Phase 6: Testing & Documentation

- [ ] T160 [P] Create test projects for Priority 1 plugins
- [ ] T161 [P] Write unit tests for Resilience.Polly
- [ ] T162 [P] Write unit tests for Serialization.Json
- [ ] T163 [P] Write unit tests for ObjectPool.Standard
- [ ] T164 Create integration tests for plugin loading
- [ ] T165 Create README.md for each plugin with usage examples
- [ ] T166 Document plugin registration patterns
- [ ] T167 Create completion report

**Checkpoint 12**: All plugins have tests and documentation.

---

## Proxy Service Template (for T001-T018)

Each proxy service should follow this pattern:

```csharp
// File: Services/Proxy/Service.cs
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

**Steps for each contract**:

1. Create `Services/Proxy/` directory in contract project
2. Create `Service.cs` file with template above
3. Replace `{Service}` with actual service name
4. Ensure contract project references `LablabBean.SourceGenerators.Proxy` as Analyzer
5. Build and verify generated code appears in `obj/Debug/net8.0/generated/`

---

## Verification Commands

### Verify Proxy Services

```bash
# Check all proxy services exist
for service in Analytics Audio Config Diagnostic Firebase Game Input Localization ObjectPool Performance PersistentStorage Resilience Resource Scene Scheduler Serialization ServiceHealth UI; do
  file="dotnet/framework/LablabBean.Contracts.$service/Services/Proxy/Service.cs"
  if [ -f "$file" ]; then
    echo "✓ $service"
  else
    echo "✗ $service (MISSING)"
  fi
done

# Verify generated files exist
find dotnet/framework/LablabBean.Contracts.*/obj -name "*Service.g.cs" -path "*/generated/*" 2>/dev/null
```

### Verify Plugin Projects

```bash
# List all plugins
ls -d dotnet/plugins/LablabBean.Plugins.* | xargs -n1 basename

# Build each plugin
cd dotnet/plugins
for dir in LablabBean.Plugins.*/; do
  echo "Building $dir"
  dotnet build "$dir" --verbosity quiet
done

# Build solution
cd dotnet
dotnet build --verbosity minimal
```

### Verify Plugin Registration

```bash
# Check each plugin implements IPlugin
grep -r "IPlugin" dotnet/plugins/LablabBean.Plugins.*/  --include="*Plugin.cs" | grep "class"

# Check registry registration calls
grep -r "Registry.Register" dotnet/plugins/LablabBean.Plugins.*/ --include="*Plugin.cs"
```

---

## Success Metrics

### Phase 0: Proxy Services

- ✅ 18/18 contracts have proxy services
- ✅ All proxy services build successfully
- ✅ Source generator produces delegation code
- ✅ All contracts reference SourceGenerators.Proxy

### Phase 1: Essential Plugins

- ✅ Resilience.Polly implements retry + circuit breaker
- ✅ Serialization.Json handles JSON operations
- ✅ ObjectPool.Standard provides pooling
- ✅ All 3 plugins build with 0 errors

### Phase 2: Data Plugins

- ✅ PersistentStorage.Json saves/loads files
- ✅ Localization.Json supports multi-language
- ✅ Scheduler.Standard schedules tasks
- ✅ All 3 plugins build with 0 errors

### Phase 3: Monitoring Plugins

- ✅ Diagnostic.Console outputs diagnostics
- ✅ Performance.Standard collects metrics
- ✅ ServiceHealth.Standard monitors health
- ✅ All 3 plugins build with 0 errors

### Overall

- ✅ Minimum 8 new plugins created
- ✅ All plugins added to solution
- ✅ Solution builds with 0 errors
- ✅ No regressions in existing tests

---

## Dependencies & References

**Required for all tasks**:

- SPEC-012 contracts must exist
- Source generator must be functional
- IPlugin interface must be defined
- IRegistry interface must be defined

**NuGet Packages**:

- Polly (>= 8.0.0)
- System.Text.Json (>= 8.0.0)
- Microsoft.Extensions.ObjectPool (>= 8.0.0)
- NAudio (>= 2.0.0)
- LiteDB (>= 5.0.0) - optional
- FirebaseAdmin (>= 3.0.0) - optional

---

## Estimated Effort

- **Phase 0** (Proxy Services): 3-4 hours (18 services × 10-15 min each)
- **Phase 1** (Essential Plugins): 6-8 hours
  - Resilience.Polly: 3-4 hours
  - Serialization.Json: 1-2 hours
  - ObjectPool.Standard: 2-3 hours
- **Phase 2** (Data Plugins): 5-7 hours
  - PersistentStorage.Json: 2-3 hours
  - Localization.Json: 2-3 hours
  - Scheduler.Standard: 2-3 hours
- **Phase 3** (Monitoring Plugins): 4-6 hours
  - Diagnostic.Console: 2-3 hours
  - Performance.Standard: 1-2 hours
  - ServiceHealth.Standard: 1-2 hours
- **Phase 4** (Optional): 6-10 hours
- **Phase 5** (Verification): 1-2 hours
- **Phase 6** (Testing/Docs): 4-6 hours

**Total Estimated Time**: 29-43 hours

---

## Notes

- **CRITICAL**: Phase 0 must be completed first - plugins cannot work without proxy services
- Proxy services enable the four-tier architecture to function
- Each plugin should be independently testable
- Follow existing patterns from Reporting.Csv and Reporting.Html plugins
- Use dependency injection for all dependencies
- Implement proper logging using ILogger
- Provider pattern allows multiple backend implementations
- Test proxy service generation after Phase 0 before proceeding
