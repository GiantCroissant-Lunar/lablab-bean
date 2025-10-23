---
title: CI/CD Integration Guide
category: DevOps
tags: [ci-cd, github-actions, nuke, automation, reporting]
date: 2025-10-22
author: LablabBean Team
version: 1.0.0
status: production-ready
---

# CI/CD Integration Guide

Complete guide for integrating the LablabBean reporting system into your CI/CD pipelines.

## Table of Contents

- [Overview](#overview)
- [Local Development](#local-development)
- [GitHub Actions](#github-actions)
- [Nuke Build Integration](#nuke-build-integration)
- [Artifacts](#artifacts)
- [Troubleshooting](#troubleshooting)

## Overview

The LablabBean reporting system automatically generates HTML and CSV reports for:

- **Build Metrics**: Test results, code coverage, build timing
- **Session Analytics**: Gameplay statistics, K/D ratios, progression
- **Plugin Health**: Plugin status, memory usage, performance

### Key Features

✅ **Timestamped Reports**: Each report includes build number and timestamp
✅ **Multiple Formats**: HTML (visual) and CSV (data analysis)
✅ **Latest Snapshots**: Symbolic links to most recent reports
✅ **CI/CD Ready**: GitHub Actions integration included
✅ **Error Handling**: Graceful fallback when data is missing
✅ **Artifact Upload**: Automatic upload to CI artifacts

## Local Development

### Prerequisites

- .NET 8.0 SDK
- PowerShell 7+ (recommended)
- Task (optional, for shortcuts)

### Quick Start

```bash
# Run tests with coverage and generate reports
task reports:ci

# Or step-by-step:
task test:coverage     # Run tests with coverage
task reports           # Generate all reports
```

### Available Commands

```bash
# Testing
task test              # Run all tests
task test:coverage     # Run tests with code coverage

# Reports
task reports           # Generate HTML and CSV reports
task reports:ci        # Full CI workflow (test + reports)

# Build
task build             # Build the solution
task clean             # Clean artifacts
```

### Manual Report Generation

```bash
# Build metrics
dotnet run --project dotnet/console-app/LablabBean.Console -- report build \
  --output reports/build-metrics.html \
  --format html

# Session analytics
dotnet run --project dotnet/console-app/LablabBean.Console -- report session \
  --output reports/session-analytics.html \
  --format html

# Plugin health
dotnet run --project dotnet/console-app/LablabBean.Console -- report plugin \
  --output reports/plugin-metrics.html \
  --format html

# CSV export
dotnet run --project dotnet/console-app/LablabBean.Console -- report build \
  --output reports/build-metrics.csv \
  --format csv
```

## GitHub Actions

### Workflow Configuration

The included GitHub Actions workflow (`.github/workflows/build-and-test.yml`) automatically:

1. ✅ Builds the solution
2. ✅ Runs tests with code coverage
3. ✅ Generates all reports (HTML + CSV)
4. ✅ Uploads artifacts (retained for 30 days)
5. ✅ Creates build summary

### Triggers

```yaml
on:
  push:
    branches: [main, develop, feature/**]
  pull_request:
    branches: [main, develop]
  workflow_dispatch:
```

### Artifacts

After each CI run, the following artifacts are available:

| Artifact Name | Contents | Retention |
|--------------|----------|-----------|
| `test-results-{os}` | Raw test results (.trx files) | 30 days |
| `html-reports-{os}` | Visual HTML reports | 30 days |
| `csv-reports-{os}` | CSV data exports | 30 days |

### Viewing Reports

1. Go to **Actions** tab in GitHub
2. Click on a workflow run
3. Scroll to **Artifacts** section
4. Download `html-reports-windows-latest`
5. Open the HTML files in your browser

### Environment Variables

```yaml
env:
  BUILD_NUMBER: ${{ github.run_number }}  # Used for timestamping
  DOTNET_VERSION: '8.0.x'
```

## Nuke Build Integration

### Targets

```bash
# Full workflow
nuke TestWithCoverage    # Run tests with coverage
nuke GenerateReports     # Generate all reports

# Or combined
nuke TestWithCoverage GenerateReports
```

### Configuration

The Nuke build is configured to:

```csharp
// Artifact paths
AbsolutePath VersionedArtifactsDirectory => BuildArtifactsDirectory / Version;
AbsolutePath TestResultsDirectory => VersionedArtifactsDirectory / "test-results";
AbsolutePath TestReportsDirectory => VersionedArtifactsDirectory / "test-reports";
```

### Timestamped Files

Reports are generated with the following naming convention:

```
build-metrics-{BUILD_NUMBER}-{TIMESTAMP}.html
build-metrics-{BUILD_NUMBER}-{TIMESTAMP}.csv
session-analytics-{BUILD_NUMBER}-{TIMESTAMP}.html
...
```

Where:

- `{BUILD_NUMBER}`: CI build number or GitVersion (e.g., `0.1.0-dev`)
- `{TIMESTAMP}`: UTC timestamp (e.g., `20251022-153045`)

### Latest Snapshots

The build also creates "latest" symlinks:

```
build-metrics-latest.html → build-metrics-0.1.0-dev-20251022-153045.html
session-analytics-latest.html → ...
plugin-metrics-latest.html → ...
```

## Artifacts

### Directory Structure

```
build/_artifacts/
└── {version}/           # e.g., 0.1.0-dev
    ├── test-results/    # Raw test results
    │   ├── *.trx
    │   └── coverage/
    └── test-reports/    # Generated reports
        ├── build-metrics-{build}-{ts}.html
        ├── build-metrics-{build}-{ts}.csv
        ├── session-analytics-{build}-{ts}.html
        ├── session-analytics-{build}-{ts}.csv
        ├── plugin-metrics-{build}-{ts}.html
        ├── plugin-metrics-{build}-{ts}.csv
        ├── build-metrics-latest.html
        ├── session-analytics-latest.html
        └── plugin-metrics-latest.html
```

### Report Sizes

Typical report sizes:

| Report Type | HTML Size | CSV Size |
|------------|-----------|----------|
| Build Metrics | ~11 KB | ~600 B |
| Session Analytics | ~14 KB | ~750 B |
| Plugin Health | ~21 KB | ~600 B |

## Troubleshooting

### Missing Reports

**Symptom**: Reports not generated in CI

**Solutions**:

1. Check that `nuke Compile` ran successfully
2. Verify reporting tool exists at expected path
3. Review build logs for error messages
4. Ensure test results are in expected directory

### No Data in Reports

**Symptom**: Reports show sample data instead of real data

**Expected**: This is by design! The reporting system uses fallback sample data when real data files are missing.

**To get real data**:

- For build metrics: Ensure `.trx` test results exist
- For session analytics: Provide `analytics.jsonl` file
- For plugin health: Provide `plugin-health.json` file

### CI Failures

**Symptom**: GitHub Actions workflow fails

**Solutions**:

```yaml
# Reports generation uses continue-on-error
- name: 📊 Generate reports
  continue-on-error: true  # Won't fail the build
```

This ensures report generation failures don't block the CI pipeline.

### Linux/macOS Support

**Current Status**: Windows-only (validated on `windows-latest`)

**Future**: Ubuntu support planned (see `validate-linux` job in workflow)

**Path Issues**: Use cross-platform path separators in Nuke build:

```csharp
// Good
AbsolutePath path = RootDirectory / "build" / "artifacts";

// Avoid
string path = "build\\artifacts";  // Windows-only
```

### Font Issues (PDF Reports)

**Note**: Currently generating HTML/CSV only. PDF support requires FastReport plugin.

**If adding PDF support**:

- Ensure CI images include system fonts
- Add font packages to Docker images
- Test font rendering in headless environments

## Best Practices

### 1. Version Control

```gitignore
# Add to .gitignore
build/_artifacts/
test-reports/
*.trx
```

### 2. Retention Policies

```yaml
# GitHub Actions
retention-days: 30  # Balance storage costs vs. historical data
```

### 3. Parallel Builds

```yaml
strategy:
  matrix:
    os: [windows-latest, ubuntu-latest]  # Multi-platform validation
```

### 4. Conditional Execution

```yaml
# Skip reports on draft PRs
if: github.event.pull_request.draft == false
```

### 5. Build Badges

Add CI status badges to README:

```markdown
[![Build Status](https://github.com/your-org/lablab-bean/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/your-org/lablab-bean/actions)
```

## Advanced Configuration

### Custom Build Numbers

```bash
# Set custom build number
export BUILD_NUMBER="1.2.3-beta.4"
nuke GenerateReports
```

### Multiple Report Formats

```csharp
// Generate both HTML and CSV
DotNet($"{reportingToolPath} report build --output report.html --format html");
DotNet($"{reportingToolPath} report build --output report.csv --format csv");
```

### Report Metadata

Each report includes:

- Build number
- Timestamp (UTC)
- Git commit SHA (if available)
- Environment information

### Notification Integration

```yaml
# Add Slack notifications
- name: 📢 Notify Slack
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    payload: |
      {
        "text": "Build failed: ${{ github.repository }}"
      }
```

## Performance

### Build Times

| Step | Duration |
|------|----------|
| Restore | ~30s |
| Build | ~1m |
| Tests with Coverage | ~2-5m |
| Report Generation | ~5-10s |
| **Total** | **~4-7m** |

### Optimization Tips

1. **Cache dependencies**:

   ```yaml
   - uses: actions/cache@v3
     with:
       path: ~/.nuget/packages
       key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
   ```

2. **Parallel tests**: Already configured via Nuke

3. **Incremental builds**: Use `--no-restore` and `--no-build` flags

## Support

For issues or questions:

- 📖 [Full Documentation](../specs/010-fastreport-reporting/)
- 🐛 [Report Issues](https://github.com/your-org/lablab-bean/issues)
- 💬 [Discussions](https://github.com/your-org/lablab-bean/discussions)

---

**Last Updated**: 2025-10-22
**Version**: 1.0.0
**Status**: Production-ready ✅
