# Tasks: Tiered Plugin Architecture

## Phase 0 - Foundations

- [ ] Constitution check against `.agent/base` and Spec-Kit
- [ ] Confirm DI/logging packages and versions in `Directory.Packages.props`

## Phase 1 - Contracts (netstandard2.1)

- [ ] Define `IPlugin`, `IPluginHost`, `IPluginRegistry`, `PluginManifest`, `PluginDependency`, `PluginState`
- [ ] Add XML docs and examples

## Phase 2 - Core & Registry (netstandard2.1)

- [ ] Implement `PluginRegistry` (in-memory) with thread-safety
- [ ] Implement manifest parser and validator
- [ ] Implement dependency resolver with cycle detection
- [ ] Unit tests for parser/resolver

## Phase 3 - Host Loader (net8.0)

- [ ] Add `PluginLoaderHostedService` to `LablabBean.Console`
- [ ] Add same to `LablabBean.Windows`
- [ ] ALC loading, start order, error isolation
- [ ] Configurable `plugins.paths` and `hotReload`

## Phase 4 - Observability

- [ ] Structured logging categories per plugin
- [ ] Metrics counters/histograms

## Phase 5 - E2E Demo

- [ ] Create `LablabBean.Plugins.DungeonGame` demo plugin
- [ ] E2E: discover → load → start → stop → unload → reload

## Phase 6 - Hand-off

- [ ] Document Quickstart and troubleshooting
- [ ] Plan migrations for Inventory and Status Effects (specs 005, 006)
