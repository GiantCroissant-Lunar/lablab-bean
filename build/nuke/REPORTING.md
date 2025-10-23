# 🚀 Nuke Build Integration - Reporting System

This Nuke build now includes integrated reporting capabilities for build metrics, session analytics, and plugin health monitoring.

## 📊 New Build Targets

### `TestWithCoverage`

Runs all tests with code coverage collection and generates TRX files.

```bash
dotnet build\nuke\bin\Debug\Build.dll TestWithCoverage
```

**What it does:**

- Runs all `*.Tests` projects
- Collects XPlat Code Coverage
- Generates `.trx` files for each test project
- Saves results to `build/_artifacts/{version}/test-results/`

### `GenerateReports`

Generates HTML reports from test results and build data.

```bash
dotnet build\nuke\bin\Debug\Build.dll GenerateReports
```

**Dependencies:** Requires `TestWithCoverage` (runs automatically)

**What it does:**

- Generates `build-metrics.html` - Build and test metrics with coverage data
- Generates `session-analytics.html` - Session statistics and analytics
- Generates `plugin-metrics.html` - Plugin health and performance metrics
- Saves reports to `build/_artifacts/{version}/test-reports/`

**Sample Output:**

```
╔════════════════════════════════════════╗
║     📊 REPORTS GENERATED! 🎉         ║
╚════════════════════════════════════════╝
Location: build/_artifacts/0.0.4-010/test-reports
  - build-metrics.html
  - session-analytics.html
  - plugin-metrics.html
════════════════════════════════════════
```

## 🎯 Quick Usage

### Run tests with coverage and generate reports

```bash
# Full workflow
dotnet build\nuke\bin\Debug\Build.dll GenerateReports

# Or use the Compile target first
dotnet build\nuke\bin\Debug\Build.dll Compile
dotnet build\nuke\bin\Debug\Build.dll GenerateReports
```

### Skip tests and regenerate reports (if test results exist)

```bash
dotnet build\nuke\bin\Debug\Build.dll GenerateReports --skip TestWithCoverage
```

### View generated reports

```bash
# Open in default browser (Windows)
start build\_artifacts\{version}\test-reports\build-metrics.html
start build\_artifacts\{version}\test-reports\session-analytics.html
start build\_artifacts\{version}\test-reports\plugin-metrics.html
```

## 📁 Output Structure

After running `GenerateReports`, you'll have:

```
build/
└── _artifacts/
    └── {version}/
        ├── test-results/           # Test execution results
        │   ├── *.trx              # xUnit test results
        │   └── */coverage.*.xml   # Code coverage files
        │
        └── test-reports/           # Generated HTML reports
            ├── build-metrics.html
            ├── session-analytics.html
            └── plugin-metrics.html
```

## 🔧 Integration with CI/CD

### Add to existing targets

Update your `Release` target to include reporting:

```csharp
Target Release => _ => _
    .DependsOn(Clean, PrintVersion, TestWithCoverage, GenerateReports, PublishAll, BuildWebsite)
    .Executes(() => { /* ... */ });
```

### GitHub Actions example

```yaml
- name: Run tests and generate reports
  run: dotnet build/nuke/bin/Release/Build.dll GenerateReports

- name: Upload test reports
  uses: actions/upload-artifact@v3
  with:
    name: test-reports
    path: build/_artifacts/**/test-reports/*.html
```

## 📈 Report Contents

### Build Metrics Report

- Test execution statistics
- Code coverage percentages (line and branch)
- Test project breakdown
- File sizes and metadata

### Session Analytics Report

- Playtime statistics
- K/D ratios
- Level progression
- Session identifiers

### Plugin Metrics Report

- Plugin status (running/failed/stopped)
- Success rates
- Memory usage
- Error counts

## 🛠️ Sample Data

When no actual data is found, the providers generate sample data for demonstration:

- ✅ Perfect for testing the integration
- ✅ Verifies report generation works
- ✅ Shows expected data format

To use real data:

- Run `TestWithCoverage` first
- Ensure test results are in `test-results/`
- Providers will automatically pick up real data

## 💡 Tips

1. **First-time setup:** Run `Compile` before `GenerateReports`
2. **Faster iteration:** Use `--skip TestWithCoverage` when regenerating reports
3. **CI/CD:** Always run full `GenerateReports` target for comprehensive results
4. **Debugging:** Check `test-results/` directory for raw data files

## 🎉 Success

Your Nuke build now has production-ready reporting integration!

---

**Created:** 2025-10-22
**SPEC-010 Phase 6:** CLI Integration ✅ Complete
