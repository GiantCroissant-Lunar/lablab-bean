# Research: Hierarchical Dependency Injection Container System

**Feature**: 018-hierarchical-di-container
**Date**: 2025-10-24
**Phase**: 0 - Research & Decision Making

## Overview

This document captures research findings and technology decisions for implementing a hierarchical DI container system compatible with Microsoft.Extensions.DependencyInjection.

## Key Research Areas

### 1. MSDI Interface Compatibility

**Research Question**: Which MSDI interfaces must we implement for full compatibility?

**Decision**: Implement the following core interfaces:

- `IServiceProvider` - Core service resolution
- `IServiceScope` - Scope creation and management
- `IServiceScopeFactory` - Factory for creating scopes
- `ISupportRequiredService` - Required service resolution with better error messages

**Rationale**:

- These are the minimum interfaces required for third-party library compatibility
- `IServiceProviderFactory<TContainerBuilder>` is optional but recommended for Generic Host integration
- `IServiceCollection` is only needed during registration, not at runtime

**Alternatives Considered**:

- Minimal implementation (IServiceProvider only) - Rejected: Insufficient for scope support
- Full MSDI re-implementation - Rejected: Unnecessary complexity, leverage existing MSDI for leaf containers

**References**:

- [Microsoft.Extensions.DependencyInjection.Abstractions](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions)
- [ASP.NET Core DI Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)

### 2. Service Lifetime Implementation Strategy

**Research Question**: How should we handle Singleton, Scoped, and Transient lifetimes across container hierarchies?

**Decision**: Hybrid approach - delegate to inner MSDI containers per level

**Implementation Strategy**:

```
HierarchicalServiceProvider (Global)
├── Contains: ServiceProvider (MSDI instance for global services)
├── Children: List<HierarchicalServiceProvider>
└── Parent: null

HierarchicalServiceProvider (Scene)
├── Contains: ServiceProvider (MSDI instance for scene services)
├── Children: List<HierarchicalServiceProvider>
└── Parent: HierarchicalServiceProvider (Global)
```

**Lifetime Behavior**:

- **Singleton**: Resolved at registration level, cached there, shared with children
- **Scoped**: Created per scope at current level only, not shared across hierarchy
- **Transient**: New instance every time, created at registration level

**Rationale**:

- Leverages battle-tested MSDI lifetime management
- Avoids reimplementing complex lifetime logic (generics, disposal tracking, etc.)
- Each hierarchy level uses its own ServiceProvider for local services
- Parent fallback is simple dictionary lookup or delegation

**Alternatives Considered**:

1. **Full custom implementation**: Track all instances ourselves
   - Rejected: Reinventing wheel, high risk of bugs (disposal, threading, etc.)
2. **Single shared ServiceProvider with metadata**: All services in one container with hierarchy metadata
   - Rejected: Complex shadowing logic, breaks MSDI semantics
3. **Wrapper-only approach**: No inner containers, pure delegation
   - Rejected: Can't leverage MSDI lifetime management

**References**:

- [Service Lifetimes in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Understanding Scopes](https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/#what-is-a-scope)

### 3. Service Resolution Strategy

**Research Question**: How should service resolution work across parent-child boundaries?

**Decision**: Cascading resolution with explicit priority

**Resolution Algorithm**:

```
GetService(Type serviceType):
1. Try resolve from current level's ServiceProvider
2. If not found AND parent exists:
   - Call parent.GetService(serviceType)
3. If not found at any level:
   - Return null (or throw if GetRequiredService)
```

**Shadowing Behavior**:

- Child registrations shadow parent registrations for the same service type
- No warning or error - this is intentional feature (allow overrides)
- Closest registration wins

**Rationale**:

- Simple, predictable resolution order
- Matches developer mental model (child overrides parent)
- Enables testing (mock parent services in child)

**Alternatives Considered**:

1. **Explicit parent access only**: `GetService()` never checks parent, must call `parent.GetService()` manually
   - Rejected: Defeats purpose of transparent hierarchy
2. **Registration-time flattening**: Copy all parent services to child at creation
   - Rejected: Breaks singleton semantics, memory waste
3. **Named services**: Disambiguate with service names
   - Rejected: Adds complexity, breaks MSDI compatibility

**References**:

- [Unity Container Hierarchies](https://github.com/unitycontainer/unity/wiki/Unity-Container-Hierarchies)
- [Autofac Lifetime Scopes](https://autofaccn.readthedocs.io/en/latest/lifetime/working-with-scopes.html)

### 4. Disposal and Lifecycle Management

**Research Question**: How should we handle disposal in hierarchical structures?

**Decision**: Cascading disposal with ownership tracking

**Disposal Strategy**:

```
Dispose():
1. Dispose all child containers (recursive)
2. Dispose current level's ServiceProvider
3. Clear child collection
4. Mark container as disposed
5. Subsequent GetService calls throw ObjectDisposedException
```

**Ownership Rules**:

- Parent owns children
- Disposing parent disposes all descendants
- Disposing child does NOT affect parent or siblings
- Children can be explicitly removed and disposed independently

**Rationale**:

- Prevents memory leaks from orphaned children
- Matches scene unloading semantics (unload dungeon → unload all floors)
- Follows IDisposable best practices

**Alternatives Considered**:

1. **Manual disposal only**: No cascading, user must dispose each level
   - Rejected: Error-prone, easy to leak memory
2. **Weak references to children**: Let GC handle it
   - Rejected: Non-deterministic disposal, services might hold resources (files, connections)

**References**:

- [IDisposable Pattern](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
- [Dispose in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#disposal-of-services)

### 5. Thread Safety Considerations

**Research Question**: What level of thread safety do we need?

**Decision**: Thread-safe service resolution, single-threaded mutation

**Thread Safety Guarantees**:

- ✅ `GetService()` is thread-safe (delegates to MSDI ServiceProvider which is thread-safe)
- ✅ `Dispose()` is thread-safe (uses lock during disposal)
- ❌ `CreateChildContainer()` is NOT thread-safe (must be called from scene loading code, single-threaded)
- ❌ Concurrent disposal of parent and child is undefined behavior (don't do this)

**Rationale**:

- Game scene loading is single-threaded by nature (Unity, Godot, etc.)
- Service resolution happens from multiple systems (combat, rendering, etc.) - must be thread-safe
- Over-engineering thread safety adds complexity for no benefit

**Implementation Notes**:

- Use `lock` for disposal to prevent concurrent dispose
- Document thread safety in XML comments
- No locks needed for GetService (inner MSDI handles this)

**Alternatives Considered**:

1. **Full thread safety**: Lock everything
   - Rejected: Performance cost, unnecessary for game use case
2. **No thread safety**: Document as single-threaded only
   - Rejected: Service resolution needs to be thread-safe

**References**:

- [Thread Safety in .NET](https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices)
- [ServiceProvider Thread Safety](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection/src/ServiceProvider.cs)

### 6. Performance Optimization Strategy

**Research Question**: How do we meet the < 1μs overhead performance goal?

**Decision**: Minimize allocations, cache aggressively

**Optimization Techniques**:

1. **Fast-path for local services**: Check local ServiceProvider first (no allocations)
2. **Parent chain caching**: Store direct reference to parent (no traversal)
3. **Avoid LINQ**: Use for loops for child iteration
4. **Lazy child list**: Only allocate List<> if children created
5. **Struct-based resolution** (future): Consider ValueTask-style resolution for hot paths

**Performance Testing Plan**:

- Benchmark: Flat MSDI vs 1-level vs 3-level hierarchy
- Measure: Service resolution time (GetService<T>)
- Target: < 1μs additional overhead per level
- Tool: BenchmarkDotNet

**Rationale**:

- Game code calls GetService frequently (every frame for some services)
- 60fps = 16.67ms budget, 1μs is negligible
- Focus on avoiding allocations (GC pressure is bigger concern than raw speed)

**Alternatives Considered**:

1. **Service caching in child**: Cache all resolved parent services
   - Rejected: Memory waste, breaks scoped semantics
2. **Source generation**: Generate service accessors at compile time
   - Deferred: Good future optimization, out of scope for v1

**References**:

- [High Performance .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-dependency-injection)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)

### 7. Error Handling and Diagnostics

**Research Question**: How should we handle and report errors?

**Decision**: Clear exceptions with hierarchical context

**Exception Strategy**:

```csharp
// When service not found
throw new ServiceResolutionException(
    $"Service of type '{serviceType.Name}' not found in current container or any parent. " +
    $"Hierarchy: {GetHierarchyPath()}"  // e.g., "Global -> Dungeon -> Floor1"
);

// When accessing disposed container
throw new ObjectDisposedException(
    $"Cannot access disposed container '{Name}'. " +
    $"Parent: {Parent?.Name ?? "none"}"
);
```

**Diagnostic Features**:

- `ToString()` shows hierarchy path
- Optional `IServiceProviderIsService` implementation for service existence checks
- Debug-only logging (via ILogger if available)

**Rationale**:

- Developers need to know where in hierarchy resolution failed
- Clear error messages reduce debugging time
- Follows MSDI exception patterns

**Alternatives Considered**:

1. **Silent failures**: Return null on errors
   - Rejected: Hides bugs, hard to debug
2. **Extensive logging**: Log every resolution attempt
   - Rejected: Performance cost, noise in logs

**References**:

- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

## Technology Stack Summary

| Component | Technology | Version | Justification |
|-----------|-----------|---------|---------------|
| Core Framework | .NET | 8.0+ | Project standard, LTS support |
| DI Abstractions | Microsoft.Extensions.DependencyInjection.Abstractions | 8.0+ | MSDI compatibility requirement |
| Testing Framework | xUnit | 2.6+ | Project standard |
| Assertion Library | FluentAssertions | 6.12+ | Readable test assertions |
| Mocking | NSubstitute | 5.1+ | Project standard for mocking |
| Benchmarking | BenchmarkDotNet | 0.13+ | Performance validation |

## Open Questions for Implementation

1. **Naming**: Should we use `HierarchicalServiceProvider` or `ScopedServiceProvider` or `ContainerScope`?
   - **Decision**: `HierarchicalServiceProvider` - clearest intent

2. **Factory pattern**: Should we expose `IServiceProviderFactory<HierarchicalServiceProvider>`?
   - **Decision**: Yes, enables Generic Host integration

3. **Service collection builder**: Should we support `IServiceCollection.BuildHierarchicalServiceProvider()`?
   - **Decision**: Yes, add extension method for convenience

4. **Scene name requirement**: Should containers require names or support anonymous containers?
   - **Decision**: Names optional but recommended, SceneContainerManager requires names

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| MSDI breaking changes in future .NET | Low | High | Target abstractions package only, minimal surface area |
| Performance regression | Medium | Medium | Benchmark tests in CI, fail build if > 1μs overhead |
| Memory leaks from disposal | Medium | High | Integration tests with repeated create/dispose cycles |
| Thread safety bugs | Low | Medium | Document threading model, stress tests |
| Scope semantics confusion | High | Low | Extensive documentation and examples in quickstart |

## Next Steps

- ✅ Research complete
- → Proceed to Phase 1: Data Model & Contracts
- → Define entity model for container hierarchy
- → Create interface contracts for public API
