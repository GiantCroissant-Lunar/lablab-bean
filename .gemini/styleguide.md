# Lablab Bean Repository Review Guide

This style guide augments Gemini's default review focus areas with project-specific expectations for the lablab-bean multi-platform development toolkit.

## Project Context

**Lablab Bean** is a multi-platform development toolkit featuring:

- **Task Automation** - Powered by Task with comprehensive workflow management
- **Web Terminal** - Astro.js + xterm.js + node-pty for browser-based terminal access
- **Console Applications** - Terminal.Gui v2 TUI and SadConsole ASCII graphics
- **Event-Driven Plugin Architecture** - High-performance pub-sub messaging (1.1M+ events/sec)
- **Gameplay Systems** - Complete dungeon crawler with ECS architecture (Arch)
- **Spec-Kit Integration** - Template-based code generation and specifications
- **Multi-Language Support** - .NET 8, Node.js/TypeScript, Python tooling

## Languages & Stacks

- **.NET 8.0+** (Console/Windows apps, plugin architecture, gameplay systems)
- **Node.js/TypeScript** (Web terminal, PTY service, Astro.js frontend)
- **Python 3.8+** (Utilities, validation scripts, tooling)
- **PowerShell** (Build automation, PM2 process management)
- **Task** (Taskfile.yml for workflow automation)

## General Principles

1. **Event-driven architecture first** - Plugins communicate via IEventBus, not direct references
2. **Performance-conscious** - Target 1M+ events/sec, sub-millisecond latency
3. **Cross-platform compatibility** - Support Windows, macOS, Linux
4. **Documentation-driven development** - Specs in docs/specs/, guides in docs/guides/
5. **Template-based generation** - Use spec-kit for consistent code generation
6. **Process management** - PM2 for development and production stacks
7. **Minimize noise** - Focus on high-value, actionable feedback

## .NET / C# Guidelines

### Plugin Architecture Specific

- **Event-Driven Design**: Plugins must use `IEventBus` for communication, avoid direct service dependencies
- **ECS Integration**: Validate proper Arch ECS usage (entities, components, systems)
- **Service Registration**: Use Microsoft.Extensions.DependencyInjection properly
- **Performance**: Maintain 1M+ events/sec throughput, sub-millisecond latency
- **Plugin Lifecycle**: Proper initialization, activation, and cleanup

### Gameplay Systems

- **Arch ECS**: Validate entity/component/system patterns
- **Event Publishing**: Game actions should publish events for loose coupling
- **Service Contracts**: Use interfaces for testability and modularity
- **Data-Driven**: Configuration via JSON/YAML, not hardcoded values

### General C# Standards

- Follow Microsoft C# coding conventions
- Use nullable reference types where appropriate
- Prefer `async/await` over `Task.Result` or `.Wait()`
- Use `CancellationToken` for long-running operations
- Validate proper disposal patterns for resources

### What to Skip

- Formatting issues (handled by `dotnet format`)
- Style preferences already enforced by EditorConfig

## Node.js / TypeScript Guidelines

### Web Terminal Specific

- **PTY Management**: Validate proper process lifecycle and cleanup
- **WebSocket Handling**: Check connection management and error recovery
- **Cross-Platform**: Ensure shell detection works on Windows/Unix
- **Performance**: Monitor memory usage in long-running terminal sessions

### Astro.js Frontend

- **SSR/SSG**: Validate proper static generation and hydration
- **Component Architecture**: Check React component patterns
- **Build Optimization**: Ensure proper bundling and asset optimization

### What to Review

- Async/await usage and error handling
- WebSocket connection management
- Process spawning and cleanup
- Type safety and null checks
- Memory leak prevention

### What to Skip

- Formatting (handled by Prettier)
- Linting issues already caught by ESLint

## Python Guidelines

### Tooling & Validation

- **Script Quality**: Validation scripts should be robust and well-tested
- **Documentation Validation**: Check docs/index/registry.json maintenance
- **Cross-Platform**: Ensure scripts work on Windows and Unix

### What to Review

- Error handling and explicit exceptions
- File path handling (use pathlib)
- Documentation validation logic
- Script argument validation

### What to Skip

- Style changes already covered by Ruff configuration
- Formatting (handled by `ruff format`)

## PowerShell Guidelines

### Process Management

- **PM2 Integration**: Validate ecosystem.config.js management
- **Error Handling**: Use `$ErrorActionPreference = 'Stop'` for critical scripts
- **Cross-Platform**: Ensure compatibility with PowerShell Core

### Build Automation

- **Task Integration**: Scripts should work with Taskfile.yml
- **Version Management**: Proper artifact versioning and cleanup
- **Development vs Production**: Clear separation of dev and prod stacks

### What to Review

- Error handling completeness
- Process lifecycle management
- Cross-platform compatibility
- PM2 configuration validation

## Task Automation Guidelines

### Taskfile.yml Structure

- **Task Organization**: Group related tasks logically
- **Dependencies**: Proper task dependencies and ordering
- **Variables**: Use consistent variable naming
- **Documentation**: Include task descriptions

### Development Workflow

- **Hot Reload**: Validate dev-stack tasks enable proper hot reload
- **Build Pipeline**: Ensure build-release creates proper artifacts
- **Testing Integration**: Tasks should support test automation

### What to Review

- Task dependency correctness
- Variable usage and scoping
- Cross-platform command compatibility
- Error handling in task scripts

## Plugin Architecture Guidelines

### Event-Driven Patterns

- **IEventBus Usage**: Plugins should publish/subscribe to events
- **Loose Coupling**: Avoid direct service dependencies between plugins
- **Performance**: Validate event handling doesn't block main thread
- **Event Naming**: Use consistent event naming conventions

### Service Registration

- **DI Container**: Proper service registration and lifetime management
- **Interface Segregation**: Services should have focused, single-purpose interfaces
- **Priority System**: Multiple implementations should use priority-based selection

### What to Validate

- Event subscription/unsubscription patterns
- Service lifetime management
- Plugin activation/deactivation
- Cross-plugin communication via events

## Gameplay Systems Guidelines

### Arch ECS Architecture

- **Entity Management**: Proper entity creation and cleanup
- **Component Design**: Components should be data-only (no behavior)
- **System Implementation**: Systems should be stateless and focused
- **Query Performance**: Validate efficient entity queries

### Game Logic

- **Event-Driven Gameplay**: Game actions should publish events
- **State Management**: Game state should be predictable and testable
- **Configuration**: Use data files for game content, not hardcoded values

### What to Review

- ECS pattern adherence
- Performance implications of queries
- Event publishing for game actions
- Data-driven configuration usage

## Documentation Guidelines

### Structured Documentation

- **YAML Front-matter**: All docs should include proper metadata
- **Category Organization**: guides/, specs/, findings/, archive/
- **Registry Maintenance**: Keep docs/index/registry.json updated
- **Cross-References**: Validate internal links work

### Spec-Kit Integration

- **Template Usage**: Validate proper template variable usage
- **Specification Format**: Follow established spec format
- **Code Generation**: Ensure templates generate valid code

### What to Review

- Documentation completeness and accuracy
- Broken internal links
- YAML front-matter correctness
- Template variable consistency

## Process Management Guidelines

### PM2 Configuration

- **Ecosystem Files**: Validate ecosystem.config.js structure
- **Process Lifecycle**: Proper start/stop/restart handling
- **Log Management**: Appropriate log rotation and retention
- **Development vs Production**: Clear separation of environments

### Hot Reload Support

- **File Watching**: Validate proper file change detection
- **Restart Logic**: Ensure clean restarts without state corruption
- **Performance**: Hot reload shouldn't impact production performance

### What to Review

- PM2 configuration correctness
- Process restart reliability
- Log management setup
- Development workflow efficiency

## Security Guidelines

### General Security

- **No Hardcoded Secrets**: Use environment variables or config files
- **Input Validation**: Validate all external inputs
- **Process Isolation**: Ensure proper process boundaries
- **File Permissions**: Validate appropriate file access controls

### Web Terminal Security

- **Command Injection**: Validate shell command sanitization
- **WebSocket Security**: Proper authentication and authorization
- **Process Limits**: Prevent resource exhaustion attacks

### What to Review

- Input sanitization completeness
- Authentication/authorization logic
- Resource usage limits
- Security boundary enforcement

## Performance Guidelines

### Event System Performance

- **Throughput**: Maintain 1M+ events/sec capability
- **Latency**: Keep event handling under 1ms
- **Memory Usage**: Monitor for event handler memory leaks
- **Backpressure**: Handle high event volumes gracefully

### ECS Performance

- **Query Optimization**: Efficient entity component queries
- **System Scheduling**: Proper system execution ordering
- **Memory Layout**: Consider cache-friendly component design

### What to Review

- Performance-critical code paths
- Memory allocation patterns
- Async operation efficiency
- Resource cleanup completeness

## Review Priorities (Descending Order)

1. **Architecture Integrity** - Event-driven patterns, plugin architecture
2. **Performance** - Event throughput, ECS efficiency, memory usage
3. **Cross-Platform Compatibility** - Windows/macOS/Linux support
4. **Security** - Input validation, process isolation, authentication
5. **Documentation** - Accuracy, completeness, spec-kit integration
6. **Process Management** - PM2 configuration, hot reload, lifecycle
7. **Functional Correctness** - Logic errors, edge cases, error handling
8. **Maintainability** - Code organization, testing, cleanup

## Noise Reduction

### Avoid Commenting On

- Formatting already auto-managed by tools
- Linting issues caught by pre-commit hooks
- Build artifacts in ignored directories
- Reference projects (`ref-projects/`)
- Lock files and generated code
- PM2 logs and process files

### Focus On

- Event-driven architecture violations
- Performance regressions or bottlenecks
- Cross-platform compatibility issues
- Plugin architecture integrity
- Documentation accuracy and completeness
- Process management reliability
- Security vulnerabilities

## Special Considerations

### Multi-Platform Development

- Respect platform-specific conventions
- Validate cross-platform compatibility
- Consider platform-specific optimizations

### Event-Driven Architecture

- Maintain loose coupling between components
- Validate event handling performance
- Check for proper event lifecycle management

### Development Workflow

- Ensure hot reload works reliably
- Validate build and deployment processes
- Check development vs production separation

### Template-Based Generation

- Validate template variable usage
- Check generated code quality
- Ensure template consistency

## When Unsure

Prefer asking a clarifying question rather than proposing speculative changes. Consider:

- Does this affect the event-driven architecture?
- Is this a performance-critical code path?
- Does this impact cross-platform compatibility?
- Is this already handled by automated tooling?
- Does the suggested change align with project architecture?

Focus on high-impact issues that affect architecture, performance, security, or maintainability rather than style preferences or minor optimizations.
