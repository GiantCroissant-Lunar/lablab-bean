---
doc_id: DOC-2025-00059
title: Reporting System Troubleshooting
doc_type: guide
status: active
canonical: true
created: 2025-10-22
tags: [troubleshooting, debugging, faq, reporting]
summary: Common issues and solutions for the LablabBean reporting system
---

# Reporting System Troubleshooting Guide

Common issues and solutions for the LablabBean reporting system.

## Table of Contents

- [Installation & Setup](#installation--setup)
- [Report Generation](#report-generation)
- [Data Issues](#data-issues)
- [CI/CD Problems](#cicd-problems)
- [Performance](#performance)
- [Known Limitations](#known-limitations)

## Installation & Setup

### Issue: "dotnet command not found"

**Symptom**: Command line reports "dotnet: command not found"

**Solution**:

1. Install .NET 8.0 SDK from <https://dotnet.microsoft.com/download>
2. Verify installation: `dotnet --version`
3. Restart terminal to refresh PATH

### Issue: "Project not found"

**Symptom**: Cannot find LablabBean.Console project

**Solution**:

```bash
# Ensure you're in the project root
cd /path/to/lablab-bean

# Verify project exists
ls dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj

# Build the project
task build
```

### Issue: "Task command not found"

**Symptom**: `task` command doesn't work

**Solution**:

Option 1: Install Task runner

```bash
# Windows (Chocolatey)
choco install go-task

# Windows (Scoop)
scoop install task

# macOS (Homebrew)
brew install go-task
```

Option 2: Use Nuke directly

```bash
pwsh -NoProfile -ExecutionPolicy Bypass -File build/nuke/build.ps1 --target GenerateReports
```

## Report Generation

### Issue: "Reporting tool not found"

**Symptom**:

```
❌ Reporting tool not found at D:\...\LablabBean.Console.dll
Run 'nuke Compile' first to build the reporting tool
```

**Solution**:

```bash
# Build the console application
task build

# Or use Nuke directly
nuke Compile

# Verify the build
ls dotnet/console-app/LablabBean.Console/bin/Debug/net8.0/LablabBean.Console.dll
```

### Issue: "Report generation failed"

**Symptom**: Error during report generation with stack trace

**Solutions**:

1. **Check data path**:

   ```bash
   # Ensure data directory exists
   ls build/_artifacts/*/test-results

   # If missing, run tests first
   task test:coverage
   ```

2. **Check output directory permissions**:

   ```bash
   # Create output directory
   mkdir -p reports

   # Generate report
   lablabbean.exe report build --output reports/test.html
   ```

3. **Check for missing dependencies**:

   ```bash
   # Restore NuGet packages
   dotnet restore dotnet/LablabBean.sln

   # Rebuild
   task build
   ```

### Issue: Empty or corrupt HTML files

**Symptom**: HTML file generated but shows blank page or errors

**Solutions**:

1. **Check file size**:

   ```bash
   ls -lh reports/*.html
   # Should be ~10-20 KB for HTML reports
   ```

2. **Verify content**:

   ```bash
   # Check if file contains HTML
   head -n 5 reports/build-metrics.html
   # Should show: <!DOCTYPE html>
   ```

3. **Regenerate report**:

   ```bash
   # Delete and regenerate
   rm reports/build-metrics.html
   lablabbean.exe report build --output reports/build-metrics.html
   ```

## Data Issues

### Issue: "Using sample data" message

**Symptom**:

```
ℹ️ No test results found at specified path
📊 Using sample data for demonstration
```

**Is this an error?**: No! This is expected behavior when real data is unavailable.

**To use real data**:

**Build Metrics**:

```bash
# Run tests first to generate test results
task test:coverage

# Then generate report with test data
lablabbean.exe report build \
  --data build/_artifacts/*/test-results \
  --output build-metrics.html
```

**Session Analytics**:

```bash
# Provide analytics log file
lablabbean.exe report session \
  --data logs/analytics.jsonl \
  --output session-stats.html
```

**Plugin Health**:

```bash
# Provide plugin health JSON
lablabbean.exe report plugin \
  --data logs/plugin-health.json \
  --output plugin-health.html
```

### Issue: "No coverage data found"

**Symptom**: Report shows 0% code coverage or missing coverage section

**Solutions**:

1. **Run tests with coverage collector**:

   ```bash
   # Use Nuke target that includes coverage
   nuke TestWithCoverage

   # Or use dotnet test directly
   dotnet test --collect:"XPlat Code Coverage"
   ```

2. **Check coverage files**:

   ```bash
   # Look for coverage.cobertura.xml files
   find build/_artifacts -name "coverage.cobertura.xml"

   # Should find files like:
   # build/_artifacts/*/test-results/{GUID}/coverage.cobertura.xml
   ```

3. **Install coverlet**:

   ```bash
   # Should be included in test projects, but verify:
   dotnet add package coverlet.collector
   ```

### Issue: Incorrect or missing test results

**Symptom**: Report shows wrong number of tests or old data

**Solutions**:

1. **Clean and rebuild**:

   ```bash
   task clean
   task test:coverage
   task reports
   ```

2. **Check test result timestamps**:

   ```bash
   # Verify .trx files are recent
   ls -lt build/_artifacts/*/test-results/*.trx
   ```

3. **Check test project references**:

   ```bash
   # Ensure test projects are in solution
   dotnet sln list | grep Tests
   ```

## CI/CD Problems

### Issue: GitHub Actions workflow fails

**Symptom**: CI workflow fails at report generation step

**Solutions**:

1. **Check workflow logs**:
   - Go to Actions tab in GitHub
   - Click on failed run
   - Expand "Generate reports" step
   - Look for specific error messages

2. **Verify continue-on-error**:

   ```yaml
   # In .github/workflows/build-and-test.yml
   - name: 📊 Generate reports
     continue-on-error: true  # Should be present
   ```

3. **Check artifact paths**:

   ```yaml
   # Ensure paths use wildcards for version
   path: build/_artifacts/*/test-reports/
   ```

4. **Test locally first**:

   ```bash
   # Simulate CI environment
   $env:BUILD_NUMBER="123"
   nuke TestWithCoverage
   nuke GenerateReports
   ```

### Issue: Artifacts not uploading

**Symptom**: Workflow succeeds but no artifacts visible

**Solutions**:

1. **Check artifact size**:
   - GitHub has a 500 MB limit per artifact
   - Check: `du -sh build/_artifacts/*/test-reports/`

2. **Verify paths exist**:

   ```yaml
   # Add debug step before upload
   - name: List files
     run: ls -R build/_artifacts/
   ```

3. **Check retention settings**:

   ```yaml
   retention-days: 30  # Ensure this is set
   ```

### Issue: Linux build fails (future)

**Symptom**: Ubuntu workflow fails while Windows succeeds

**Solutions**:

1. **Path separators**:

   ```csharp
   // Use Nuke's path construction
   AbsolutePath path = RootDirectory / "build" / "reports";

   // Not:
   string path = "build\\reports";  // Windows only!
   ```

2. **Case sensitivity**:

   ```bash
   # Linux is case-sensitive
   # Ensure consistent casing in paths
   ```

3. **Line endings**:

   ```bash
   # Configure git to handle line endings
   git config core.autocrlf input
   ```

## Performance

### Issue: Report generation is slow

**Symptom**: Takes > 30 seconds to generate reports

**Solutions**:

1. **Check data size**:

   ```bash
   # Large test result files can slow generation
   du -sh build/_artifacts/*/test-results/
   ```

2. **Parallel generation**:

   ```bash
   # Generate reports in parallel (PowerShell)
   Start-Job { lablabbean.exe report build --output reports/build.html }
   Start-Job { lablabbean.exe report session --output reports/session.html }
   Start-Job { lablabbean.exe report plugin --output reports/plugin.html }
   Get-Job | Wait-Job | Receive-Job
   ```

3. **Optimize data parsing**:
   - Limit test results to recent runs only
   - Use file filters in `--data` parameter

### Issue: High memory usage

**Symptom**: System runs out of memory during report generation

**Solutions**:

1. **Split large reports**:

   ```bash
   # Generate reports separately instead of all at once
   lablabbean.exe report build --output reports/build.html
   # Wait for completion, then:
   lablabbean.exe report session --output reports/session.html
   ```

2. **Increase memory limits** (if using containers):

   ```yaml
   resources:
     limits:
       memory: 2Gi
   ```

## Known Limitations

### PDF Export Not Available

**Current**: HTML and CSV formats only

**Workaround**: Use browser "Print to PDF" feature for HTML reports

**Future**: FastReport plugin will enable native PDF export

### No Custom Themes

**Current**: Fixed styling (professional blue/white theme)

**Workaround**: Modify HTML reports with external CSS

**Future**: Theme system planned

### Sample Data Only

**Expected**: System uses sample data when real data unavailable

**This is by design**: Ensures reports always generate for demo purposes

**To disable**: Not currently configurable (future enhancement)

### Windows-Only Testing

**Current**: CI/CD tested on Windows only

**Workaround**: Use WSL for Linux testing locally

**Future**: Linux CI validation planned (see `validate-linux` job)

### No Real-time Updates

**Current**: Reports are static snapshots

**Workaround**: Regenerate reports periodically

**Future**: Live dashboard planned

## Error Messages Reference

### "Failed to parse test results"

**Cause**: Malformed .trx file or unsupported format

**Solution**: Regenerate test results with compatible format

### "Output directory not writable"

**Cause**: Permission issues

**Solution**: Run with elevated permissions or change output path

### "Invalid data format"

**Cause**: Wrong file format for data source

**Solution**: Check data file format matches report type expectations

### "Template not found"

**Cause**: Missing embedded report template

**Solution**: Rebuild console application to include embedded resources

## Getting Help

### Enable Debug Logging

```bash
# Set environment variable for verbose output
$env:LABLABBEAN_LOG_LEVEL="Debug"
lablabbean.exe report build --output report.html
```

### Collect Diagnostic Information

```bash
# System info
dotnet --info

# Project info
dotnet list dotnet/LablabBean.sln package

# Build info
nuke --version

# File info
ls -R build/_artifacts/
```

### Report an Issue

When reporting issues, include:

1. **Error message** (full stack trace)
2. **Command used** (exact command line)
3. **Environment**:
   - OS version
   - .NET version
   - Project version
4. **Steps to reproduce**
5. **Diagnostic logs** (if available)

### Support Channels

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/your-org/lablab-bean/issues)
- 💬 **Questions**: [GitHub Discussions](https://github.com/your-org/lablab-bean/discussions)
- 📖 **Documentation**: [Reporting Docs](../specs/010-fastreport-reporting/)
- 📧 **Email**: <support@lablabbean.dev>

## Frequently Asked Questions

### Q: Why does it use sample data?

**A**: Sample data ensures the reporting system always works, even without real game/test data. It's perfect for demonstrations and testing the report format.

### Q: Can I customize report styling?

**A**: Not directly in v1.0. You can modify the generated HTML files with external CSS, or wait for the theme system in a future release.

### Q: How do I export to PDF?

**A**: Currently, use your browser's "Print to PDF" feature on HTML reports. Native PDF export will be available when the FastReport plugin is implemented.

### Q: Do reports work offline?

**A**: Yes! Reports are self-contained HTML files with embedded CSS. No internet connection required to view them.

### Q: Can I automate report generation?

**A**: Yes! Use Task commands, Nuke targets, or call the CLI directly from scripts. See [CI/CD Integration](./CI-CD-INTEGRATION.md) for examples.

### Q: How long are CI artifacts retained?

**A**: GitHub Actions artifacts are retained for 30 days by default. Adjust `retention-days` in the workflow file if needed.

---

**Last Updated**: 2025-10-22
**Version**: 1.0.0
**Status**: Production-ready ✅

Still having issues? [Open an issue](https://github.com/your-org/lablab-bean/issues) and we'll help! 🚀
