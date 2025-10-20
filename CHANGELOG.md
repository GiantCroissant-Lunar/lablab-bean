# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project setup with Go 1.21
- Pre-commit hooks configuration with multiple linters and formatters
- Task automation using Taskfile.yml
- Makefile as alternative to Taskfile
- Speck-kit integration for code generation
- CLI framework using Cobra and Viper
- Sample commands: version, speck init, speck generate
- Configuration management with .lablab-bean.yaml
- Comprehensive documentation (README, QUICKSTART, CONTRIBUTING)
- GitHub Actions CI/CD workflow
- Docker support with Dockerfile and docker-compose
- Code templates for API and model generation
- golangci-lint configuration
- Setup scripts for Windows (PowerShell) and Unix (Bash)
- Unit tests for speck package
- .gitignore with comprehensive exclusions
- .dockerignore for optimized Docker builds

### Pre-commit Hooks
- General file checks (trailing whitespace, EOF fixer, YAML validation)
- Go-specific hooks (go fmt, go vet, go imports, go mod tidy, golangci-lint)
- Markdown linting with auto-fix
- YAML formatting with auto-fix

### Task Automation
- install: Install dependencies and tools
- build: Build the application
- build-all: Build for all platforms (Linux, Windows, macOS)
- run: Build and run the application
- test: Run tests with race detection
- test-coverage: Generate coverage reports
- lint: Run golangci-lint
- fmt: Format code with gofmt and goimports
- check: Run all checks (fmt, vet, lint, test)
- clean: Clean build artifacts
- pre-commit-install: Install pre-commit hooks
- pre-commit-run: Run pre-commit hooks manually
- pre-commit-update: Update pre-commit hooks
- mod-tidy: Tidy Go modules
- mod-verify: Verify Go modules
- mod-update: Update all dependencies
- docker-build: Build Docker image
- docker-run: Run Docker container

### Documentation
- README.md: Comprehensive project documentation
- QUICKSTART.md: Quick start guide for new users
- CONTRIBUTING.md: Contribution guidelines
- CHANGELOG.md: This changelog file

## [0.1.0] - 2025-10-20

### Added
- Initial release
- Basic CLI structure
- Pre-commit hooks integration
- Task automation setup
- Speck-kit foundation

[Unreleased]: https://github.com/yokan-projects/lablab-bean/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/yokan-projects/lablab-bean/releases/tag/v0.1.0
