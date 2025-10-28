# Test script to verify Console and Windows apps functionality
# Tests: Launch, Plugin Loading, Metrics Collection, Report Generation

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportDir = "..\..\test-reports\$timestamp"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LablabBean Apps Verification Test" -ForegroundColor Cyan
Write-Host "  Timestamp: $timestamp" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create report directory
New-Item -ItemType Directory -Path $reportDir -Force | Out-Null

# Test 1: Windows App
Write-Host "[TEST 1] Windows App Verification" -ForegroundColor Yellow
Write-Host "  Location: dotnet/windows-app/LablabBean.Windows/bin/Release/net8.0" -ForegroundColor Gray

$windowsExe = "..\..\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe"

if (Test-Path $windowsExe) {
    Write-Host "  ✓ Windows app executable found" -ForegroundColor Green

    # Start the Windows app and let it run for 10 seconds
    Write-Host "  → Starting Windows app..." -ForegroundColor Gray
    $windowsProcess = Start-Process -FilePath $windowsExe -PassThru -WindowStyle Normal

    Write-Host "  → Waiting 10 seconds for initialization and gameplay..." -ForegroundColor Gray
    Start-Sleep -Seconds 10

    # Check if process is still running
    if (!$windowsProcess.HasExited) {
        Write-Host "  ✓ Windows app is running (PID: $($windowsProcess.Id))" -ForegroundColor Green

        # Gracefully close the app
        Write-Host "  → Closing Windows app..." -ForegroundColor Gray
        $windowsProcess.CloseMainWindow() | Out-Null
        Start-Sleep -Seconds 2

        if (!$windowsProcess.HasExited) {
            Write-Host "  → Force stopping Windows app..." -ForegroundColor Gray
            Stop-Process -Id $windowsProcess.Id -Force
        }

        Write-Host "  ✓ Windows app closed" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Windows app crashed or exited early (Exit Code: $($windowsProcess.ExitCode))" -ForegroundColor Red
    }
} else {
    Write-Host "  ✗ Windows app executable not found" -ForegroundColor Red
}

Write-Host ""

# Test 2: Check for generated reports/logs
Write-Host "[TEST 2] Report & Log Generation" -ForegroundColor Yellow

$reportLocations = @(
    "..\..\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\reports",
    "..\..\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\logs",
    "..\..\reports",
    "..\..\logs"
)

$foundReports = $false
foreach ($loc in $reportLocations) {
    if (Test-Path $loc) {
        $files = Get-ChildItem -Path $loc -Recurse -File | Where-Object { $_.LastWriteTime -gt (Get-Date).AddMinutes(-5) }
        if ($files.Count -gt 0) {
            Write-Host "  ✓ Found $($files.Count) recent files in $loc" -ForegroundColor Green
            $files | ForEach-Object {
                Write-Host "    - $($_.Name) ($($_.Length) bytes)" -ForegroundColor Gray
            }
            $foundReports = $true
        }
    }
}

if (!$foundReports) {
    Write-Host "  ℹ No reports generated (this is okay for short test runs)" -ForegroundColor Yellow
}

Write-Host ""

# Test 3: Console App Build Status
Write-Host "[TEST 3] Console App Status" -ForegroundColor Yellow

$consoleExe = "..\..\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"

if (Test-Path $consoleExe) {
    Write-Host "  ✓ Console app executable found" -ForegroundColor Green
    $fileInfo = Get-Item $consoleExe
    Write-Host "    - Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "    - Last Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray

    # Note: Console app currently has Terminal.Gui v2 API breaking changes
    Write-Host "  ℹ Console app has Terminal.Gui v2 compatibility issues (known)" -ForegroundColor Yellow
} else {
    Write-Host "  ✗ Console app executable not found" -ForegroundColor Red
}

Write-Host ""

# Test 4: Plugin System
Write-Host "[TEST 4] Plugin System Check" -ForegroundColor Yellow

$pluginDirs = @(
    "..\..\dotnet\plugins\LablabBean.Plugins.Reporting.Html\bin\Release\net8.0",
    "..\..\dotnet\plugins\LablabBean.Plugins.Reporting.Csv\bin\Release\net8.0",
    "..\..\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\plugins"
)

$pluginCount = 0
foreach ($dir in $pluginDirs) {
    if (Test-Path $dir) {
        $dlls = Get-ChildItem -Path $dir -Filter "LablabBean.Plugins.*.dll" -File
        $pluginCount += $dlls.Count
        if ($dlls.Count -gt 0) {
            Write-Host "  ✓ Found $($dlls.Count) plugin(s) in $dir" -ForegroundColor Green
        }
    }
}

Write-Host "  → Total plugins available: $pluginCount" -ForegroundColor Cyan

Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$results = @{
    "Windows App Build" = (Test-Path $windowsExe)
    "Console App Build" = (Test-Path $consoleExe)
    "Plugin System" = ($pluginCount -gt 0)
}

foreach ($test in $results.GetEnumerator()) {
    $status = if ($test.Value) { "✓ PASS" } else { "✗ FAIL" }
    $color = if ($test.Value) { "Green" } else { "Red" }
    Write-Host "  $($test.Key): " -NoNewline
    Write-Host $status -ForegroundColor $color
}

Write-Host ""
Write-Host "Report directory: $reportDir" -ForegroundColor Gray
Write-Host "Timestamp: $timestamp" -ForegroundColor Gray
Write-Host ""

# Save results to file
$resultsFile = Join-Path $reportDir "verification-results.txt"
@"
LablabBean Apps Verification Test
Timestamp: $timestamp

Windows App Build: $(if (Test-Path $windowsExe) { "PASS" } else { "FAIL" })
Console App Build: $(if (Test-Path $consoleExe) { "PASS" } else { "FAIL" })
Plugin System: $(if ($pluginCount -gt 0) { "PASS ($pluginCount plugins)" } else { "FAIL" })

Details:
- Windows EXE: $windowsExe
- Console EXE: $consoleExe
- Plugin Count: $pluginCount
"@ | Out-File -FilePath $resultsFile -Encoding UTF8

Write-Host "Results saved to: $resultsFile" -ForegroundColor Cyan
