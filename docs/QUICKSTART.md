# Quick Start Guide

Get up and running with Lablab Bean in 5 minutes!

## Prerequisites

- **Go 1.21+**: [Download here](https://golang.org/dl/)
- **Task**: Install with `go install github.com/go-task/task/v3/cmd/task@latest`
- **Pre-commit** (optional): Install with `pip install pre-commit` or `brew install pre-commit`

## Installation

### Option 1: Automated Setup (Recommended)

**Windows (PowerShell):**
```powershell
.\setup.ps1
```

**Linux/macOS:**
```bash
chmod +x setup.sh
./setup.sh
```

### Option 2: Manual Setup

1. **Install dependencies:**
   ```bash
   task install
   ```

2. **Set up pre-commit hooks:**
   ```bash
   task pre-commit-install
   ```

3. **Build the application:**
   ```bash
   task build
   ```

## First Run

Run the application:
```bash
task run
```

Or directly:
```bash
./bin/lablab-bean --help
```

## Basic Commands

### View Available Tasks
```bash
task --list
```

### Build and Run
```bash
task build
task run
```

### Run Tests
```bash
task test
```

### Check Code Quality
```bash
task check
```

This runs:
- Code formatting
- Linting
- Tests
- Static analysis

### Format Code
```bash
task fmt
```

### Run Linter
```bash
task lint
```

## Using Speck-kit

### Initialize Speck-kit
```bash
./bin/lablab-bean speck init
```

### Generate Code from Template
```bash
./bin/lablab-bean speck generate model User
```

### Configure Templates

Edit `.lablab-bean.yaml`:
```yaml
speck:
  enabled: true
  project_name: lablab-bean
  output_dir: ./generated
  templates:
    api: ./templates/api.tmpl
    model: ./templates/model.tmpl
```

## Development Workflow

1. **Make changes** to your code

2. **Format and check:**
   ```bash
   task check
   ```

3. **Commit** (pre-commit hooks run automatically):
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

4. **Build and test:**
   ```bash
   task build
   task test
   ```

## Common Tasks Reference

| Task | Description |
|------|-------------|
| `task install` | Install dependencies and tools |
| `task build` | Build the application |
| `task run` | Build and run the application |
| `task test` | Run tests |
| `task test-coverage` | Run tests with coverage report |
| `task lint` | Run linters |
| `task fmt` | Format code |
| `task check` | Run all checks |
| `task clean` | Clean build artifacts |
| `task pre-commit-install` | Install pre-commit hooks |
| `task pre-commit-run` | Run pre-commit hooks manually |

## Using Make (Alternative)

If you prefer Make over Task:

```bash
make help        # Show available commands
make install     # Install dependencies
make build       # Build the application
make run         # Run the application
make test        # Run tests
make check       # Run all checks
```

## Configuration

The application uses `.lablab-bean.yaml` for configuration:

```yaml
app:
  name: lablab-bean
  version: 0.1.0
  environment: development

speck:
  enabled: true
  project_name: lablab-bean
  output_dir: ./generated

logging:
  level: info
  format: json
```

You can also use environment variables:
```bash
export LABLAB_BEAN_LOGGING_LEVEL=debug
./bin/lablab-bean
```

## Troubleshooting

### Task not found
Install Task:
```bash
go install github.com/go-task/task/v3/cmd/task@latest
```

Add `$GOPATH/bin` to your PATH:
```bash
export PATH=$PATH:$(go env GOPATH)/bin
```

### Pre-commit hooks not working
Install pre-commit:
```bash
pip install pre-commit
```

Then install hooks:
```bash
task pre-commit-install
```

### Build fails
Clean and rebuild:
```bash
task clean
task install
task build
```

### Tests fail
Run with verbose output:
```bash
go test -v ./...
```

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Check [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines
- Explore the code in `cmd/` and `internal/` directories
- Customize templates in `templates/` directory
- Configure your own speck-kit templates

## Getting Help

- Run `task --list` to see all available tasks
- Run `./bin/lablab-bean --help` for CLI help
- Check the [README.md](README.md) for detailed documentation
- Open an issue on GitHub for bugs or questions

Happy coding! 🚀
