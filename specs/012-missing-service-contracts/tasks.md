# Tasks: Missing Service Contracts (SPEC-012)

**Input**: `ref-projects/soy-bean/packages/scoped-3208/com.giantcroissant.yokan.game/Runtime/Core/`
**Spec**: `specs/012-missing-service-contracts/spec.md`

**Objective**: Create 12 missing service contract projects following SPEC-011 patterns, adapted from Unity soy-bean reference implementation to .NET Standard 2.1.

---

## Format: `[ID] [P] Description`

- **[P]**: Can run in parallel (no ordering/dependency conflicts)

## Path Conventions

- Contract projects: `dotnet/framework/LablabBean.Contracts.{Service}/`
- Proxy services: `dotnet/framework/LablabBean.Contracts.{Service}/Services/Proxy/Service.cs`
- Test projects: `dotnet/tests/LablabBean.Contracts.{Service}.Tests/`

---

## Phase 1: Project Setup (Foundation)

**Purpose**: Create all 12 contract project structures before porting code.

- [ ] T001 [P] Create `LablabBean.Contracts.Diagnostic/` project directory and .csproj
- [ ] T002 [P] Create `LablabBean.Contracts.ObjectPool/` project directory and .csproj
- [ ] T003 [P] Create `LablabBean.Contracts.Audio/` project directory and .csproj
- [ ] T004 [P] Create `LablabBean.Contracts.Localization/` project directory and .csproj
- [ ] T005 [P] Create `LablabBean.Contracts.PersistentStorage/` project directory and .csproj
- [ ] T006 [P] Create `LablabBean.Contracts.Serialization/` project directory and .csproj
- [ ] T007 [P] Create `LablabBean.Contracts.Performance/` project directory and .csproj
- [ ] T008 [P] Create `LablabBean.Contracts.Scheduler/` project directory and .csproj
- [ ] T009 [P] Create `LablabBean.Contracts.Analytics/` project directory and .csproj
- [ ] T010 [P] Create `LablabBean.Contracts.Firebase/` project directory and .csproj
- [ ] T011 [P] Create `LablabBean.Contracts.ServiceHealth/` project directory and .csproj
- [ ] T012 [P] Create `LablabBean.Contracts.Resilience/` project directory and .csproj

**Checkpoint 1**: All 12 project directories created with .csproj files.

- [ ] T013 Add all 12 projects to `dotnet/LablabBean.sln`
- [ ] T014 [P] Add Polyfills.cs (IsExternalInit) to all 12 projects
- [ ] T015 Verify solution loads without errors

**Checkpoint 2**: Solution recognizes all 12 projects.

---

## Phase 2: Priority 1 Services (Diagnostic, ObjectPool, Audio)

### Diagnostic Service

- [ ] T020 Create `Services/IService.cs` with interface from reference
- [ ] T021 Adapt interface: Replace UniTask → Task, UniRx → System (or remove)
- [ ] T022 Create `Classes/` directory with diagnostic types:
  - DiagnosticEvent.cs
  - DiagnosticData.cs
  - PerformanceMetrics.cs
  - PerformanceAlert.cs
  - HealthCheckResult.cs
  - DiagnosticSessionSummary.cs
  - DiagnosticSessionConfig.cs
  - SystemInfo.cs (platform-agnostic)
- [ ] T023 Create `Enums/` directory:
  - DiagnosticLevel.cs
  - SystemHealth.cs
  - DiagnosticExportFormat.cs
- [ ] T024 Create `Interfaces/` directory:
  - IDiagnosticProvider.cs
  - IDiagnosticSpan.cs
- [ ] T025 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T026 Add necessary using statements (System, System.Collections.Generic, etc.)
- [ ] T027 Build project and fix compilation errors

**Checkpoint 3**: Diagnostic contract project builds successfully.

### ObjectPool Service

- [ ] T030 Create `Services/IService.cs` with interface from reference
- [ ] T031 Adapt interface: Remove IGameObjectPool (Unity-specific), replace UniTask → Task
- [ ] T032 Create `Classes/` directory:
  - PoolStatistics.cs
  - ObjectPoolStatistics.cs
  - PoolInfo.cs
  - ObjectPoolGlobalSettings.cs
- [ ] T033 Create `Interfaces/` directory:
  - IObjectPool.cs (generic version only)
- [ ] T034 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T035 Add necessary using statements
- [ ] T036 Build project and fix compilation errors

**Checkpoint 4**: ObjectPool contract project builds successfully.

### Audio Service

- [ ] T040 Create `Services/IService.cs` with interface from reference
- [ ] T041 Adapt interface: Remove Unity AudioClip/AudioSource references, replace UniTask → Task
- [ ] T042 Create `Classes/` directory:
  - AudioRequest.cs
  - AudioHandle.cs (abstract handle pattern)
  - AudioStats.cs
- [ ] T043 Create `Enums/` directory:
  - AudioCategory.cs
  - AudioPriority.cs
  - AudioSourceType.cs
- [ ] T044 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T045 Add necessary using statements
- [ ] T046 Build project and fix compilation errors

**Checkpoint 5**: Audio contract project builds successfully.

---

## Phase 3: Priority 2 Services (Localization, PersistentStorage, Serialization)

### Localization Service

- [ ] T050 Create `Services/IService.cs` with interface from reference
- [ ] T051 Adapt interface: Replace UniTask → Task, use standard CultureInfo
- [ ] T052 Create `Enums/` directory:
  - LocalizationFormat.cs
- [ ] T053 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T054 Add necessary using statements (System.Globalization)
- [ ] T055 Build project and fix compilation errors

**Checkpoint 6**: Localization contract project builds successfully.

### PersistentStorage Service

- [ ] T060 Create `Services/IService.cs` with interface from reference
- [ ] T061 Adapt interface: Replace UniTask → Task
- [ ] T062 Create `Classes/` directory:
  - StorageRequest.cs
  - StorageStatistics.cs
  - StorageDebugInfo.cs
- [ ] T063 Create `Enums/` directory:
  - StorageProviderType.cs
- [ ] T064 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T065 Add necessary using statements
- [ ] T066 Build project and fix compilation errors

**Checkpoint 7**: PersistentStorage contract project builds successfully.

### Serialization Service

- [ ] T070 Create `Services/IService.cs` with interface from reference
- [ ] T071 Adapt interface: Replace UniTask → Task, use standard .NET serialization patterns
- [ ] T072 Create supporting classes if needed
- [ ] T073 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T074 Add necessary using statements (System.Runtime.Serialization)
- [ ] T075 Build project and fix compilation errors

**Checkpoint 8**: Serialization contract project builds successfully.

---

## Phase 4: Priority 3 Services (Performance, Scheduler, Analytics, Firebase, ServiceHealth, Resilience)

### Performance Service

- [ ] T080 Create `Services/IService.cs` with interface from reference
- [ ] T081 Adapt interface: Remove Unity Profiler integration, replace UniTask → Task
- [ ] T082 Create supporting classes/enums
- [ ] T083 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T084 Build project and fix compilation errors

### Scheduler Service

- [ ] T090 Create `Services/IService.cs` with interface from reference
- [ ] T091 Adapt interface: Replace UniTask → Task, use System.Threading.Timer patterns
- [ ] T092 Create supporting classes/enums (CronExpression, ScheduledTask, etc.)
- [ ] T093 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T094 Build project and fix compilation errors

### Analytics Service

- [ ] T100 Create `Services/IService.cs` with interface from reference
- [ ] T101 Adapt interface: Remove Unity Analytics specifics, replace UniTask → Task
- [ ] T102 Create supporting classes/enums (AnalyticsEvent, UserProperty, etc.)
- [ ] T103 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T104 Build project and fix compilation errors

### Firebase Service

- [ ] T110 Create `Services/IService.cs` with interface from reference
- [ ] T111 Adapt interface: Use .NET Firebase SDK patterns, replace UniTask → Task
- [ ] T112 Create `Enums/` directory:
  - FirebaseServiceType.cs
- [ ] T113 Create supporting classes (RemoteConfigValue, etc.)
- [ ] T114 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T115 Build project and fix compilation errors

### ServiceHealth Service

- [ ] T120 Create `Services/IService.cs` with interface from reference
- [ ] T121 Adapt interface: Replace UniTask → Task
- [ ] T122 Create supporting classes (HealthStatus, ServiceMetrics, etc.)
- [ ] T123 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T124 Build project and fix compilation errors

### Resilience Service

- [ ] T130 Create `Services/IService.cs` with interface from reference
- [ ] T131 Adapt interface: Align with Polly patterns, replace UniTask → Task
- [ ] T132 Create `Interfaces/` directory:
  - IRetryPolicy.cs
  - ICircuitBreaker.cs
  - ITimeout.cs
- [ ] T133 Create supporting classes/enums (ResilienceContext, CircuitState, etc.)
- [ ] T134 Create `Services/Proxy/Service.cs` with [RealizeService] attribute
- [ ] T135 Build project and fix compilation errors

**Checkpoint 9**: All Priority 3 services build successfully.

---

## Phase 5: Build Verification & Integration

- [ ] T140 Build all 12 contract projects individually (parallel verification)
- [ ] T141 Build entire `dotnet/LablabBean.sln` solution
- [ ] T142 Verify all proxy services have generated code in obj/.../generated/
- [ ] T143 Fix any cross-project reference issues
- [ ] T144 Run `dotnet build --verbosity detailed` and verify 0 errors
- [ ] T145 Document any Unity → .NET Standard adaptation decisions made

**Checkpoint 10**: Full solution builds with 0 errors.

---

## Phase 6: Documentation & Finalization (Optional)

- [ ] T150 Create completion report documenting:
  - All 12 services created
  - Key adaptation decisions (UniTask→Task, removed Unity types, etc.)
  - Build results
  - Any deviations from reference implementation
- [ ] T151 Update `specs/README.md` to include SPEC-012 entry
- [ ] T152 Create basic README.md in each contract project explaining its purpose

---

## Verification Commands

```bash
# Build all contract projects
cd dotnet/framework
for dir in LablabBean.Contracts.*/; do
  echo "Building $dir"
  dotnet build "$dir" --verbosity quiet
done

# Build entire solution
cd dotnet
dotnet build --verbosity minimal

# Check for namespace errors
dotnet build 2>&1 | grep "error CS"

# Verify generated proxy files exist
find dotnet/framework/LablabBean.Contracts.*/obj -name "Service.g.cs" 2>/dev/null
```

## Success Metrics

- ✅ 12 contract projects created
- ✅ 12 .csproj files added to solution
- ✅ 12 proxy services with [RealizeService] attributes
- ✅ All UniTask replaced with Task
- ✅ All Unity-specific types removed or abstracted
- ✅ Solution builds with 0 errors
- ✅ All proxy services generate successfully

## Reference Mapping

| Service | Reference Path | Target Path |
|---------|---------------|-------------|
| Diagnostic | `ref-projects/.../Core/Diagnostic/` | `dotnet/framework/LablabBean.Contracts.Diagnostic/` |
| ObjectPool | `ref-projects/.../Core/ObjectPool/` | `dotnet/framework/LablabBean.Contracts.ObjectPool/` |
| Audio | `ref-projects/.../Core/Audio/` | `dotnet/framework/LablabBean.Contracts.Audio/` |
| Localization | `ref-projects/.../Core/Localization/` | `dotnet/framework/LablabBean.Contracts.Localization/` |
| PersistentStorage | `ref-projects/.../Core/PersistentStorage/` | `dotnet/framework/LablabBean.Contracts.PersistentStorage/` |
| Serialization | `ref-projects/.../Core/Serialization/` | `dotnet/framework/LablabBean.Contracts.Serialization/` |
| Performance | `ref-projects/.../Core/Performance/` | `dotnet/framework/LablabBean.Contracts.Performance/` |
| Scheduler | `ref-projects/.../Core/Scheduler/` | `dotnet/framework/LablabBean.Contracts.Scheduler/` |
| Analytics | `ref-projects/.../Core/Analytics/` | `dotnet/framework/LablabBean.Contracts.Analytics/` |
| Firebase | `ref-projects/.../Core/Firebase/` | `dotnet/framework/LablabBean.Contracts.Firebase/` |
| ServiceHealth | `ref-projects/.../Core/ServiceHealth/` | `dotnet/framework/LablabBean.Contracts.ServiceHealth/` |
| Resilience | `ref-projects/.../Core/Resilience/` | `dotnet/framework/LablabBean.Contracts.Resilience/` |

## Notes

- Each service contract is independent and can be implemented in parallel after Phase 1
- Follow SPEC-011 patterns for consistency
- Use explicit using statements (no ImplicitUsings)
- Include IsExternalInit polyfill for C# records
- Keep Platform-agnostic (no Unity, no Windows-specific code)
- Prioritize completing P1 services first for immediate value
