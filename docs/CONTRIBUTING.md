---
doc_id: DOC-2025-00019
title: Contributing to Lablab Bean
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [contributing, development, workflow]
summary: >
  Guidelines and instructions for contributing to Lablab Bean including
  setup, workflow, and coding standards.
---

# Contributing to Lablab Bean

Thank you for your interest in contributing to Lablab Bean! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- Go 1.21 or higher
- [Task](https://taskfile.dev) or Make
- [Pre-commit](https://pre-commit.com)
- Git

### Setting Up Your Development Environment

1. **Fork and clone the repository**

   ```bash
   git clone https://github.com/your-username/lablab-bean.git
   cd lablab-bean
   ```

2. **Install dependencies**

   ```bash
   task install
   ```

   Or using Make:

   ```bash
   make install
   ```

3. **Install pre-commit hooks**

   ```bash
   task pre-commit-install
   ```

   Or using Make:

   ```bash
   make pre-commit-install
   ```

## Development Workflow

### 1. Create a Branch

Create a new branch for your feature or bug fix:

```bash
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test additions or modifications

### 2. Make Your Changes

- Write clean, idiomatic Go code
- Follow the existing code style
- Add tests for new functionality
- Update documentation as needed

### 3. Run Checks

Before committing, ensure all checks pass:

```bash
task check
```

Or using Make:

```bash
make check
```

This will run:
- Code formatting (`gofmt`, `goimports`)
- Linting (`golangci-lint`)
- Vetting (`go vet`)
- Tests (`go test`)

### 4. Commit Your Changes

Pre-commit hooks will run automatically when you commit:

```bash
git add .
git commit -m "feat: add new feature"
```

#### Commit Message Format

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(cli): add new speck generate command
fix(config): resolve viper configuration loading issue
docs(readme): update installation instructions
```

### 5. Push and Create a Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Code Style Guidelines

### Go Code Style

- Follow the [Effective Go](https://golang.org/doc/effective_go) guidelines
- Use `gofmt` and `goimports` for formatting
- Keep functions small and focused
- Write descriptive variable and function names
- Add comments for exported functions and types

### Testing

- Write unit tests for all new functionality
- Aim for high test coverage (>80%)
- Use table-driven tests where appropriate
- Mock external dependencies

Example test structure:

```go
func TestFunctionName(t *testing.T) {
    tests := []struct {
        name    string
        input   string
        want    string
        wantErr bool
    }{
        {
            name:    "valid input",
            input:   "test",
            want:    "expected",
            wantErr: false,
        },
        // Add more test cases
    }

    for _, tt := range tests {
        t.Run(tt.name, func(t *testing.T) {
            got, err := FunctionName(tt.input)
            if (err != nil) != tt.wantErr {
                t.Errorf("FunctionName() error = %v, wantErr %v", err, tt.wantErr)
                return
            }
            if got != tt.want {
                t.Errorf("FunctionName() = %v, want %v", got, tt.want)
            }
        })
    }
}
```

## Pre-commit Hooks

The project uses pre-commit hooks to ensure code quality. These hooks will:

- Format your code
- Run linters
- Check for common issues
- Validate YAML and Markdown files

If a hook fails, fix the issues and commit again.

To manually run all hooks:

```bash
task pre-commit-run
```

## Pull Request Process

1. **Ensure all checks pass** - CI must be green
2. **Update documentation** - If you've changed APIs or added features
3. **Add tests** - For new functionality
4. **Keep PRs focused** - One feature or fix per PR
5. **Write a clear description** - Explain what and why
6. **Respond to feedback** - Address review comments promptly

### PR Title Format

Use the same format as commit messages:

```
feat(scope): brief description
```

### PR Description Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe the tests you've added or run

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review of my code
- [ ] I have commented my code where necessary
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally
```

## Reporting Issues

### Bug Reports

When reporting bugs, include:

- **Description** - Clear description of the bug
- **Steps to reproduce** - Detailed steps
- **Expected behavior** - What should happen
- **Actual behavior** - What actually happens
- **Environment** - OS, Go version, etc.
- **Logs/Screenshots** - If applicable

### Feature Requests

When requesting features, include:

- **Description** - Clear description of the feature
- **Use case** - Why this feature is needed
- **Proposed solution** - How you envision it working
- **Alternatives** - Other solutions you've considered

## Questions?

If you have questions, feel free to:

- Open an issue with the `question` label
- Start a discussion on GitHub Discussions
- Reach out to the maintainers

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

Thank you for contributing! 🎉
