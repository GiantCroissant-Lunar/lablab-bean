---
doc_id: DOC-2025-00029
title: Project Setup Summary
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [setup, configuration, pre-commit, taskfile]
summary: >
  Overview of Lablab Bean project setup including all integrated tools
  and configurations.
---

# Project Setup Summary

This document provides an overview of the Lablab Bean project setup, including all integrated tools and configurations.

## 🎯 What's Been Added

### 1. Pre-commit Hooks (`.pre-commit-config.yaml`)

Automated code quality checks that run before each commit:

- **General Checks**: Trailing whitespace, EOF fixer, YAML validation, large file detection, merge conflict detection
- **Go Checks**: go fmt, go vet, go imports, go mod tidy, golangci-lint
- **Markdown**: markdownlint with auto-fix
- **YAML**: pretty-format-yaml with auto-fix

**Usage:**
```bash
task pre-commit-install  # Install hooks
task pre-commit-run      # Run manually
```

### 2. Task Automation (`Taskfile.yml`)

Comprehensive task automation using [Task](https://taskfile.dev):

**Key Tasks:**
- `task install` - Install all dependencies and tools
- `task build` - Build the application
- `task run` - Build and run
- `task test` - Run tests with race detection
- `task test-coverage` - Generate coverage reports
- `task lint` - Run linters
- `task fmt` - Format code
- `task check` - Run all checks (fmt, vet, lint, test)
- `task clean` - Clean build artifacts
- `task build-all` - Build for all platforms

**View all tasks:**
```bash
task --list
```

### 3. Speck-kit Integration

Code generation and templating system:

**Components:**
- `internal/speck/speck.go` - Core speck-kit logic
- `internal/cmd/speck.go` - CLI commands for speck-kit
- `templates/` - Template files for code generation
  - `model.tmpl` - Model template
  - `api.tmpl` - API handler template

**Configuration:** `.lablab-bean.yaml`
```yaml
speck:
  enabled: true
  project_name: lablab-bean
  output_dir: ./generated
  templates:
    api: ./templates/api.tmpl
    model: ./templates/model.tmpl
```

**Usage:**
```bash
./bin/lablab-bean speck init
./bin/lablab-bean speck generate model User
```

## 📁 Project Structure

```
lablab-bean/
├── .github/
│   └── workflows/
│       └── ci.yml                 # GitHub Actions CI/CD
├── .pre-commit-config.yaml        # Pre-commit hooks config
├── .golangci.yml                  # golangci-lint config
├── .lablab-bean.yaml              # Application config
├── .gitignore                     # Git ignore rules
├── .dockerignore                  # Docker ignore rules
├── Taskfile.yml                   # Task automation
├── Makefile                       # Make alternative
├── Dockerfile                     # Docker build
├── docker-compose.yml             # Docker compose
├── go.mod                         # Go dependencies
├── README.md                      # Main documentation
├── QUICKSTART.md                  # Quick start guide
├── CONTRIBUTING.md                # Contribution guidelines
├── CHANGELOG.md                   # Version history
├── PROJECT_SETUP.md               # This file
├── setup.sh                       # Unix setup script
├── setup.ps1                      # Windows setup script
├── cmd/
│   └── main.go                    # Application entry point
├── internal/
│   ├── cmd/                       # CLI commands
│   │   ├── root.go                # Root command
│   │   ├── version.go             # Version command
│   │   └── speck.go               # Speck commands
│   └── speck/                     # Speck-kit integration
│       ├── speck.go               # Speck logic
│       └── speck_test.go          # Speck tests
├── templates/                     # Code generation templates
│   ├── model.tmpl                 # Model template
│   └── api.tmpl                   # API template
├── bin/                           # Compiled binaries (gitignored)
└── generated/                     # Generated code (gitignored)
```

## 🛠️ Development Tools

### Installed Tools

1. **golangci-lint** - Comprehensive Go linter
2. **goimports** - Import formatter
3. **pre-commit** - Git hook framework
4. **task** - Task automation
5. **cobra** - CLI framework
6. **viper** - Configuration management

### Configuration Files

- `.golangci.yml` - Linter configuration
- `.pre-commit-config.yaml` - Pre-commit hooks
- `.lablab-bean.yaml` - Application configuration
- `Taskfile.yml` - Task definitions
- `Makefile` - Make targets

## 🚀 Getting Started

### Quick Setup

**Automated (Recommended):**
```bash
# Windows
.\setup.ps1

# Unix/Linux/macOS
chmod +x setup.sh
./setup.sh
```

**Manual:**
```bash
task install
task pre-commit-install
task build
```

### First Run

```bash
task run
# or
./bin/lablab-bean --help
```

## 📋 Common Workflows

### Development Workflow

1. **Make changes**
2. **Check code:**
   ```bash
   task check
   ```
3. **Commit** (hooks run automatically):
   ```bash
   git add .
   git commit -m "feat: add feature"
   ```
4. **Build and test:**
   ```bash
   task build
   task test
   ```

### Building for Production

```bash
# Build for current platform
task build

# Build for all platforms
task build-all

# Build Docker image
task docker-build
```

### Running Tests

```bash
# Run tests
task test

# Run with coverage
task test-coverage

# Run specific test
go test -v ./internal/speck/...
```

### Code Quality

```bash
# Format code
task fmt

# Run linters
task lint

# Run all checks
task check
```

## 🐳 Docker Support

### Build and Run

```bash
# Using Task
task docker-build
task docker-run

# Using Docker directly
docker build -t lablab-bean:latest .
docker run --rm -it lablab-bean:latest

# Using Docker Compose
docker-compose up -d
docker-compose logs -f
docker-compose down
```

## 🔧 Configuration

### Application Configuration

Edit `.lablab-bean.yaml`:
```yaml
app:
  name: lablab-bean
  version: 0.1.0
  environment: development

speck:
  enabled: true
  project_name: lablab-bean
  output_dir: ./generated
  templates:
    api: ./templates/api.tmpl
    model: ./templates/model.tmpl

logging:
  level: info
  format: json
```

### Environment Variables

```bash
export LABLAB_BEAN_LOGGING_LEVEL=debug
export LABLAB_BEAN_ENVIRONMENT=production
./bin/lablab-bean
```

### Command-line Flags

```bash
./bin/lablab-bean --config ./custom-config.yaml --verbose
```

## 📚 Documentation

- **README.md** - Comprehensive project documentation
- **QUICKSTART.md** - Quick start guide for new users
- **CONTRIBUTING.md** - How to contribute to the project
- **CHANGELOG.md** - Version history and changes
- **PROJECT_SETUP.md** - This file (setup overview)

## 🧪 Testing

### Running Tests

```bash
# All tests
task test

# With coverage
task test-coverage

# Specific package
go test -v ./internal/speck/

# With race detection
go test -race ./...

# Benchmarks
task bench
```

### Test Structure

Tests follow Go conventions:
- Test files: `*_test.go`
- Test functions: `TestXxx(t *testing.T)`
- Benchmark functions: `BenchmarkXxx(b *testing.B)`

## 🔍 CI/CD

GitHub Actions workflow (`.github/workflows/ci.yml`):

- **Test Job**: Runs tests on Go 1.21 and 1.22
- **Lint Job**: Runs golangci-lint
- **Pre-commit Job**: Runs pre-commit hooks
- **Build Job**: Builds the application

Triggered on:
- Push to `main` or `develop`
- Pull requests to `main` or `develop`

## 📦 Dependencies

### Core Dependencies

- `github.com/spf13/cobra` - CLI framework
- `github.com/spf13/viper` - Configuration management

### Development Dependencies

- `github.com/golangci/golangci-lint` - Linter
- `golang.org/x/tools/cmd/goimports` - Import formatter

## 🎓 Learning Resources

- [Task Documentation](https://taskfile.dev)
- [Pre-commit Documentation](https://pre-commit.com)
- [Cobra Documentation](https://cobra.dev)
- [Viper Documentation](https://github.com/spf13/viper)
- [golangci-lint Documentation](https://golangci-lint.run)

## 🆘 Troubleshooting

### Common Issues

**Task not found:**
```bash
go install github.com/go-task/task/v3/cmd/task@latest
export PATH=$PATH:$(go env GOPATH)/bin
```

**Pre-commit not working:**
```bash
pip install pre-commit
task pre-commit-install
```

**Build fails:**
```bash
task clean
task install
task build
```

**Tests fail:**
```bash
go test -v ./...
```

## 📞 Support

- Check documentation in `README.md` and `QUICKSTART.md`
- Run `task --list` for available commands
- Run `./bin/lablab-bean --help` for CLI help
- Open an issue on GitHub for bugs or questions

## ✅ Next Steps

1. ✅ Pre-commit hooks configured
2. ✅ Task automation set up
3. ✅ Speck-kit integrated
4. ✅ Documentation complete
5. ✅ CI/CD pipeline ready
6. ✅ Docker support added

**You're ready to start developing!** 🎉

Run `task --list` to see all available commands and start building your application.
