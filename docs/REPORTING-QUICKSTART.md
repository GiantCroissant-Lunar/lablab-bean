---
title: Reporting System Quickstart
category: Getting Started
tags: [quickstart, reporting, cli, tutorial]
date: 2025-10-22
author: LablabBean Team
version: 1.0.0
status: production-ready
---

# Reporting System Quickstart

Get up and running with LablabBean's reporting system in 5 minutes! 🚀

## What You'll Learn

- Generate build metrics reports
- Export session analytics
- Monitor plugin health
- Use reports in CI/CD

## Prerequisites

- .NET 8.0 SDK installed
- LablabBean console app built
- (Optional) Test results or sample data

## Quick Start

### 1. Build the Tool

```bash
# Using Task
task build

# Or directly with .NET
dotnet build dotnet/console-app/LablabBean.Console
```

### 2. Generate Your First Report

```bash
# Navigate to the project root
cd lablab-bean

# Generate build metrics (uses sample data if no real data)
dotnet run --project dotnet/console-app/LablabBean.Console -- report build \
  --output my-first-report.html
```

🎉 Open `my-first-report.html` in your browser!

## Report Types

### 1. Build Metrics

**What it shows**: Test results, code coverage, build timing

```bash
# HTML report
lablabbean.exe report build --output build-metrics.html

# CSV export for Excel
lablabbean.exe report build --output build-metrics.csv --format csv

# With real test data
lablabbean.exe report build --data build/_artifacts/*/test-results --output report.html
```

**Sample output**:
- ✅ 45 tests passed
- 📊 87% code coverage
- ⏱️ 2m 34s build time

### 2. Session Analytics

**What it shows**: Gameplay stats, K/D ratio, progression, performance

```bash
# HTML report
lablabbean.exe report session --output session-stats.html

# With real analytics data
lablabbean.exe report session --data logs/analytics.jsonl --output report.html
```

**Sample output**:
- 🎮 2h 15m playtime
- 🎯 K/D ratio: 2.3
- 📈 Level progression: 5 → 12
- 🔥 60 FPS average

### 3. Plugin Health

**What it shows**: Plugin status, memory usage, load times

```bash
# HTML report
lablabbean.exe report plugin --output plugin-health.html

# With real plugin data
lablabbean.exe report plugin --data logs/plugin-health.json --output report.html
```

**Sample output**:
- ✅ 8 plugins running
- ⚠️ 1 degraded (high memory)
- ❌ 1 failed (timeout)
- 📊 Success rate: 88.9%

## Command Reference

### Basic Syntax

```bash
lablabbean.exe report <type> [options]
```

### Report Types

| Type | Description |
|------|-------------|
| `build` | Test results and code coverage |
| `session` | Gameplay analytics and stats |
| `plugin` | Plugin health and performance |

### Options

| Option | Description | Default |
|--------|-------------|---------|
| `--output <path>` | Output file path | `report.html` |
| `--format <fmt>` | Output format (html, csv) | `html` |
| `--data <path>` | Input data directory/file | *(sample data)* |

## Examples

### Generate All Reports

```bash
# HTML reports
lablabbean.exe report build --output reports/build.html
lablabbean.exe report session --output reports/session.html
lablabbean.exe report plugin --output reports/plugin.html

# CSV exports
lablabbean.exe report build --output reports/build.csv --format csv
lablabbean.exe report session --output reports/session.csv --format csv
lablabbean.exe report plugin --output reports/plugin.csv --format csv
```

### Using Task

```bash
# Run tests and generate reports
task reports:ci

# Just generate reports (after tests)
task reports
```

### CI/CD Integration

```bash
# In your CI pipeline
nuke TestWithCoverage    # Run tests with coverage
nuke GenerateReports     # Generate all reports
```

See [CI/CD Integration Guide](./CI-CD-INTEGRATION.md) for details.

## Troubleshooting

### Issue: "Reporting tool not found"

**Solution**: Build the console app first

```bash
task build
# Or
dotnet build dotnet/console-app/LablabBean.Console
```

### Issue: "No data found, using sample data"

**Expected**: This is by design! The system uses sample data as a fallback.

**To use real data**:

```bash
# Build metrics - point to test results
lablabbean.exe report build --data build/_artifacts/*/test-results

# Session analytics - point to logs
lablabbean.exe report session --data logs/analytics.jsonl

# Plugin health - point to health file
lablabbean.exe report plugin --data logs/plugin-health.json
```

### Issue: "Permission denied"

**Solution**: Run from project root or use absolute paths

```bash
cd /path/to/lablab-bean
lablabbean.exe report build --output ~/Desktop/report.html
```

### Issue: CSV file won't open in Excel

**Solution**: Use explicit format flag

```bash
lablabbean.exe report build --output report.csv --format csv
```

## Advanced Usage

### Custom Data Paths

```bash
# Multiple test result directories
lablabbean.exe report build \
  --data "build/_artifacts/*/test-results" \
  --output comprehensive-report.html

# Specific analytics file
lablabbean.exe report session \
  --data "logs/session-2025-10-22.jsonl" \
  --output today-stats.html
```

### Batch Report Generation

```bash
# PowerShell script
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputDir = "reports/$timestamp"
New-Item -ItemType Directory -Path $outputDir -Force

lablabbean.exe report build --output "$outputDir/build.html"
lablabbean.exe report session --output "$outputDir/session.html"
lablabbean.exe report plugin --output "$outputDir/plugin.html"

Write-Host "Reports generated in: $outputDir"
```

### CI/CD with Custom Build Numbers

```bash
# Set build number
$BUILD_NUMBER = "1.2.3-rc.1"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

lablabbean.exe report build \
  --output "artifacts/build-metrics-$BUILD_NUMBER-$timestamp.html"
```

## Report Features

### Visual Reports (HTML)

- 📊 **Charts**: Bar charts, metrics cards
- 🎨 **Styling**: Professional, responsive design
- 📱 **Mobile-friendly**: Works on all devices
- 🖨️ **Print-ready**: Clean print layouts

### Data Exports (CSV)

- 📈 **Excel-compatible**: Open directly in Excel/Sheets
- 🔍 **Filterable**: Sort and filter data
- 🔢 **Analyzable**: Use in data analysis tools
- 🤖 **Machine-readable**: Parse programmatically

### Metadata

Every report includes:
- Generation timestamp
- Build number (if available)
- Data source paths
- Tool version

## Integration Examples

### Git Pre-push Hook

```bash
#!/bin/bash
# .git/hooks/pre-push

echo "Generating reports before push..."
task reports:ci

if [ $? -eq 0 ]; then
  echo "✅ Reports generated successfully"
  exit 0
else
  echo "❌ Report generation failed"
  exit 1
fi
```

### CI/CD Pipeline (GitHub Actions)

```yaml
- name: Generate Reports
  run: |
    nuke TestWithCoverage
    nuke GenerateReports

- name: Upload Reports
  uses: actions/upload-artifact@v4
  with:
    name: reports
    path: build/_artifacts/*/test-reports/
```

### Scheduled Reporting

```bash
# Windows Task Scheduler or cron job
0 2 * * * cd /path/to/lablab-bean && task reports:ci
```

## Sample Data

The reporting system includes built-in sample data for demonstration:

**Build Metrics**:
- 45 tests (42 passed, 2 failed, 1 skipped)
- 87% code coverage
- 2m 34s duration

**Session Analytics**:
- 2h 15m playtime
- K/D ratio: 2.3
- Level 5 → 12
- 60 FPS average

**Plugin Health**:
- 10 plugins (8 running, 1 degraded, 1 failed)
- Various memory usage patterns
- Load time metrics

## Next Steps

- 📖 [Full Specification](../specs/010-fastreport-reporting/spec.md)
- 🏗️ [CI/CD Integration](./CI-CD-INTEGRATION.md)
- 🔧 [Technical Documentation](../specs/010-fastreport-reporting/REPORTING.md)
- 🐛 [Troubleshooting Guide](./TROUBLESHOOTING-REPORTING.md)

## Tips & Tricks

### Tip 1: Alias the Command

```bash
# PowerShell profile
Set-Alias -Name report -Value "dotnet run --project dotnet/console-app/LablabBean.Console --"

# Usage
report build --output my-report.html
```

### Tip 2: Watch for Changes

```bash
# Generate reports when tests change
fswatch build/_artifacts/*/test-results | xargs -n1 -I{} task reports
```

### Tip 3: Compare Reports

```bash
# Generate timestamped reports
lablabbean.exe report build --output "reports/build-$(Get-Date -Format 'yyyyMMdd-HHmmss').html"

# Then compare in browser to track progress
```

### Tip 4: Custom Themes (Future)

```bash
# Coming soon: Custom CSS themes
lablabbean.exe report build --output report.html --theme dark
```

## Support

Need help? Check these resources:

- 📖 [Documentation](../specs/010-fastreport-reporting/)
- 🐛 [Issue Tracker](https://github.com/your-org/lablab-bean/issues)
- 💬 [Discussions](https://github.com/your-org/lablab-bean/discussions)
- 📧 [Email Support](mailto:support@lablabbean.dev)

---

**Last Updated**: 2025-10-22  
**Version**: 1.0.0  
**Status**: Production-ready ✅

Happy reporting! 🎉📊
