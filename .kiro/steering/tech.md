---
title: Technical Stack & Constraints
mode: always
---

# Lablab-Bean Technical Stack

## Core Technologies

### Backend (.NET)
- **.NET 8**: Latest LTS version
- **C# 12**: Modern C# language features
- **Terminal.Gui**: Cross-platform terminal UI framework
- **FastReport**: Reporting system for game analytics and exports

### Frontend (Web)
- **TypeScript**: Type-safe JavaScript
- **xterm.js**: Terminal emulator in the browser
- **Node.js**: JavaScript runtime
- **npm**: Package management

### Process Management
- **PM2**: Production process manager for Node.js applications
- **dotnet CLI**: .NET build and runtime management

### Build & Development
- **npm scripts**: Frontend build automation
- **dotnet CLI**: Backend compilation and testing
- **Nuke Build**: Advanced .NET build automation (optional)

## Testing Frameworks

### Backend
- **xUnit**: .NET testing framework
- **NSubstitute**: Mocking framework
- **FluentAssertions**: Assertion library

### Frontend
- **Jest**: JavaScript testing framework (if configured)
- **TypeScript compiler**: Type checking

## Development Tools

### Version Control
- **Git**: Source control
- **Conventional Commits**: Commit message format

### Documentation
- **Markdown**: All documentation format
- **YAML front matter**: Metadata in docs
- **Python scripts**: Doc validation

### AI Assistants
- **Claude Code**: Primary development assistant
- **GitHub Copilot**: Code completion
- **Windsurf**: Alternative AI assistant
- **Google Gemini**: Multi-modal AI assistant
- **Kiro**: This assistant (steering-based)

## Technical Constraints

### Cross-Platform Requirements
- **Paths**: Always use relative paths, never absolute
  - Use `Path.Combine()` in C#
  - Use `./` or `../` in config files
  - NO Windows-style (`D:\...`) or Unix-style (`/home/...`) absolute paths
- **Line Endings**: Git handles automatically
- **File System**: Case-sensitive aware

### Performance Requirements
- **Startup**: Backend must start within 5 seconds
- **Responsiveness**: UI updates within 100ms
- **Memory**: Backend < 500MB memory footprint

### Security Constraints
- **No Hardcoded Secrets**: Use environment variables or secure vaults
- **Input Validation**: Validate all external input
- **Least Privilege**: Run with minimum required permissions

### Code Quality Standards
- **Meaningful Names**: Clear, descriptive variable and function names
- **Comments**: Required for non-obvious code
- **Testing**: Critical paths must have tests
- **Builds**: Must pass before commit

## Architecture Patterns

### Backend
- **Dependency Injection**: Use built-in .NET DI container
- **Repository Pattern**: For data access (if applicable)
- **Service Layer**: Business logic separation
- **SOLID Principles**: Follow object-oriented best practices

### Frontend
- **Module Pattern**: Organized TypeScript modules
- **Event-Driven**: xterm.js event handling
- **Type Safety**: Leverage TypeScript's type system

## Libraries & Frameworks

### Backend Dependencies
```xml
<PackageReference Include="Terminal.Gui" />
<PackageReference Include="FastReport.Core" />
<PackageReference Include="xUnit" />
<PackageReference Include="NSubstitute" />
<PackageReference Include="FluentAssertions" />
```

### Frontend Dependencies
```json
{
  "dependencies": {
    "xterm": "^5.x",
    "xterm-addon-fit": "^0.x"
  },
  "devDependencies": {
    "typescript": "^5.x"
  }
}
```

## Integration Points

### Spec-Kit Integration (REQUIRED)
- **Purpose**: Structured feature development
- **Workflow**: `/speckit.specify` → `/speckit.plan` → `/speckit.tasks` → `/speckit.implement`
- **Location**: `specs/` directory for specifications
- **Config**: `.specify/` directory

### Task Runner
- **Tool**: `task` command (Taskfile)
- **Purpose**: Standardized operations across the project

## Forbidden Technologies

- ❌ **Absolute Paths**: Never in code or configs
- ❌ **Hardcoded Secrets**: Use environment variables
- ❌ **Deprecated APIs**: Keep dependencies updated
- ❌ **Global State**: Minimize or eliminate

## Version Requirements

- **Node.js**: >= 18.x LTS
- **.NET SDK**: >= 8.0
- **npm**: >= 9.x
- **Git**: >= 2.30

## Development Environment

### Required Tools
- .NET 8 SDK
- Node.js 18+ with npm
- Git
- Text editor or IDE (VS Code, Rider, Visual Studio)
- PM2 (for running services)

### Optional Tools
- Python 3.x (for validation scripts)
- Docker (for containerization, future)

---

**For complete project rules and standards, see**: `.agent/adapters/kiro.md`
**For base technical rules, see**: `.agent/base/20-rules.md` (R-CODE section)
