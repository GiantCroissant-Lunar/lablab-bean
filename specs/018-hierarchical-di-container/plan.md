# Implementation Plan: Hierarchical Dependency Injection Container System

**Branch**: `018-hierarchical-di-container` | **Date**: 2025-10-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/018-hierarchical-di-container/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create a hierarchical dependency injection container system that implements Microsoft.Extensions.DependencyInjection interfaces while supporting parent-child container relationships for multi-scene game development. The system enables global services (save system, audio, events) accessible across all scenes, while allowing scene-specific services to remain isolated. Child containers can resolve services from parents, supporting multi-level hierarchies (Global → Dungeon → Floor) with automatic disposal cascading.

## Technical Context

**Language/Version**: C# / .NET 8
**Primary Dependencies**: Microsoft.Extensions.DependencyInjection.Abstractions 8.0+
**Storage**: N/A (in-memory container only)
**Testing**: xUnit, FluentAssertions, NSubstitute (existing test stack)
**Target Platform**: .NET 8+ (Windows/Linux cross-platform)
**Project Type**: Framework library (shared library within dotnet/framework/)
**Performance Goals**:

- Service resolution through 3-level hierarchy with < 1μs overhead vs flat resolution
- Support 1000+ container create/dispose cycles without memory leaks
- Scene transitions with container disposal < 16ms (60fps budget)
**Constraints**:
- Must implement IServiceProvider for MSDI compatibility
- Zero breaking changes to existing game services
- No external DI container dependencies (Pure.DI/VContainer integration out of scope for v1)
**Scale/Scope**:
- ~5-10 new classes in framework library
- ~20-30 unit tests
- ~3-5 integration tests demonstrating scene hierarchies

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Initial Check (Pre-Research)

| Principle | Status | Notes |
|-----------|--------|-------|
| Library-First | ✅ PASS | Feature implemented as standalone library in dotnet/framework/ |
| Zero Breaking Changes | ✅ PASS | New library, no changes to existing game code required |
| Test Coverage | ✅ PASS | Unit tests + integration tests planned (20-30 unit, 3-5 integration) |
| MSDI Compatibility | ✅ PASS | Primary requirement - implements IServiceProvider interface |
| Performance Constraints | ✅ PASS | Performance goals defined and measurable (< 1μs overhead, < 16ms disposal) |
| Simplicity | ⚠️ REVIEW | Adding new abstraction layer - must justify complexity |

**Complexity Justification Required**: The hierarchical container adds architectural complexity. However, this is justified because:

1. Scene-based games require service isolation (current MSDI doesn't support multi-container hierarchies)
2. Alternative (global singleton services only) leads to memory leaks and coupling
3. Manual service lifetime management is error-prone and requires boilerplate in every scene

**Verdict**: ✅ **APPROVED** to proceed with research phase

## Project Structure

### Documentation (this feature)

```
specs/018-hierarchical-di-container/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── IHierarchicalServiceProvider.cs  # Core interface contract
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
dotnet/framework/
├── LablabBean.DependencyInjection/              # New project
│   ├── LablabBean.DependencyInjection.csproj
│   ├── HierarchicalServiceProvider.cs           # Core container implementation
│   ├── HierarchicalServiceProviderFactory.cs    # IServiceProviderFactory<T>
│   ├── ServiceLifetimeManager.cs                # Lifetime tracking
│   ├── ServiceDescriptorExtensions.cs           # Helper extensions
│   ├── SceneContainerManager.cs                 # Scene lifecycle management
│   └── Exceptions/
│       ├── ServiceResolutionException.cs
│       └── ContainerDisposedException.cs
│
└── LablabBean.Game.Core/                        # Existing - integration point
    └── Services/
        └── (existing game services will use new DI)

dotnet/tests/
└── LablabBean.DependencyInjection.Tests/        # New test project
    ├── LablabBean.DependencyInjection.Tests.csproj
    ├── Unit/
    │   ├── HierarchicalServiceProviderTests.cs
    │   ├── ServiceLifetimeTests.cs
    │   ├── ServiceResolutionTests.cs
    │   └── DisposalTests.cs
    └── Integration/
        ├── MultiLevelHierarchyTests.cs
        ├── SceneContainerManagerTests.cs
        └── PerformanceTests.cs
```

**Structure Decision**: Framework library pattern

This feature creates a new framework library (`LablabBean.DependencyInjection`) within the existing `dotnet/framework/` directory structure. This aligns with the project's modular architecture where framework libraries provide reusable components that can be consumed by console-app, windows-app, and other applications.

The library will be added to the existing solution (`LablabBean.sln`) and use Central Package Management (CPM) defined in `Directory.Packages.props`.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

No violations requiring justification.

## Post-Design Constitution Check

*Re-evaluation after Phase 1 design complete*

| Principle | Status | Notes |
|-----------|--------|-------|
| Library-First | ✅ PASS | Implemented as `LablabBean.DependencyInjection` framework library |
| Zero Breaking Changes | ✅ PASS | New library, no modifications to existing code |
| Test Coverage | ✅ PASS | Comprehensive test plan: 20-30 unit tests, 3-5 integration tests, performance benchmarks |
| MSDI Compatibility | ✅ PASS | Implements IServiceProvider, IServiceScope, IServiceScopeFactory per design |
| Performance Constraints | ✅ PASS | Hybrid approach (inner MSDI containers) keeps overhead minimal |
| Simplicity | ✅ PASS | Design leverages existing MSDI for lifetime management, avoids reinventing wheel |
| API Surface | ✅ PASS | Clean public API: 2 main interfaces, 1 manager class, 1 extension class |
| Documentation | ✅ PASS | Comprehensive docs: research.md, data-model.md, quickstart.md, inline XML comments |

**Post-Design Assessment**:

The design successfully balances:

1. **Simplicity**: Delegates complex lifetime management to battle-tested MSDI
2. **Performance**: Hybrid approach adds < 1μs overhead per hierarchy level
3. **Usability**: Clean API with both low-level (IHierarchicalServiceProvider) and high-level (SceneContainerManager) options
4. **Maintainability**: Small surface area (~5-10 classes), well-documented

**Risks Mitigated**:

- Memory leaks → Automatic cascading disposal
- Performance overhead → Benchmarked against targets in CI
- Thread safety → Documented threading model, leverages MSDI thread safety
- MSDI compatibility → Implements standard interfaces, validated via integration tests

**Verdict**: ✅ **APPROVED** to proceed with task generation (`/speckit.tasks`)
