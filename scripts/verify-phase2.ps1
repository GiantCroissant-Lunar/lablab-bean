#!/usr/bin/env pwsh
# SPEC-025 Phase 2 Verification Helper
# Guides you through manual verification steps

$ErrorActionPreference = "Stop"
$SpecDir = "specs\025-kitty-graphics-scene-rendering"
$ReportsDir = "$SpecDir\test-reports"
$ScreenshotsDir = "$ReportsDir\screenshots"

Write-Host "`n=== SPEC-025 Phase 2 Verification Helper ===" -ForegroundColor Cyan
Write-Host "This script will guide you through completing Phase 2 verification`n" -ForegroundColor Gray

# Step 1: Check WezTerm process
Write-Host "[Step 1/6] Checking WezTerm status..." -ForegroundColor Yellow
$wezterm = Get-Process wezterm-gui -ErrorAction SilentlyContinue
if ($wezterm) {
    Write-Host "✅ WezTerm is running (PID: $($wezterm.Id), Started: $($wezterm.StartTime))" -ForegroundColor Green
    Write-Host "   Window Title: $($wezterm.MainWindowTitle)" -ForegroundColor Gray
} else {
    Write-Host "❌ WezTerm is not running" -ForegroundColor Red
    Write-Host "   Run: .\scripts\test-spec025-t076-wezterm.ps1" -ForegroundColor Yellow
    exit 1
}

# Step 2: Check for log files
Write-Host "`n[Step 2/6] Searching for log files..." -ForegroundColor Yellow
$logLocations = @(
    "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\logs",
    "dotnet\console-app\LablabBean.Console\bin\Debug\net8.0\logs",
    "logs"
)

$foundLogs = $false
foreach ($logDir in $logLocations) {
    if (Test-Path $logDir) {
        $logs = Get-ChildItem $logDir -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
        if ($logs) {
            Write-Host "✅ Found logs in: $logDir" -ForegroundColor Green
            $latestLog = $logs[0]
            Write-Host "   Latest: $($latestLog.Name) (Modified: $($latestLog.LastWriteTime))" -ForegroundColor Gray
            $foundLogs = $true
            
            # Search for key terms
            $content = Get-Content $latestLog.FullName -Raw -ErrorAction SilentlyContinue
            if ($content) {
                $keywords = @("Terminal", "Kitty", "WezTerm", "Renderer", "Capability", "Tileset", "Fallback")
                Write-Host "`n   Searching for keywords in log:" -ForegroundColor Gray
                foreach ($keyword in $keywords) {
                    if ($content -match $keyword) {
                        Write-Host "   ✓ Found: $keyword" -ForegroundColor DarkGray
                    }
                }
            }
            break
        }
    }
}

if (-not $foundLogs) {
    Write-Host "⚠️  No log files found in expected locations" -ForegroundColor Yellow
    Write-Host "   The app might still be initializing or logging may not be configured" -ForegroundColor Gray
}

# Step 3: Check screenshots directory
Write-Host "`n[Step 3/6] Checking screenshots directory..." -ForegroundColor Yellow
if (Test-Path $ScreenshotsDir) {
    $screenshots = Get-ChildItem $ScreenshotsDir -Filter "*.png" -ErrorAction SilentlyContinue
    if ($screenshots) {
        Write-Host "✅ Found $($screenshots.Count) screenshot(s)" -ForegroundColor Green
        foreach ($ss in $screenshots) {
            Write-Host "   - $($ss.Name) ($([math]::Round($ss.Length/1KB))KB)" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠️  No screenshots found yet" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️  Screenshots directory exists but is empty" -ForegroundColor Yellow
}

# Step 4: Manual verification checklist
Write-Host "`n[Step 4/6] Manual Verification Checklist" -ForegroundColor Yellow
Write-Host "Please check the following in the WezTerm window:`n" -ForegroundColor Gray

$checklist = @(
    "Is the WezTerm window visible and active?",
    "Does it show a Terminal.Gui interface with borders?",
    "Are there ASCII characters visible (@, #, ., E, etc.)?",
    "Is there a HUD/status bar at the top or bottom?",
    "Is there an activity log or game view?",
    "Are there any error messages visible?",
    "Can you interact with the UI using arrow keys?"
)

foreach ($i in 0..($checklist.Count - 1)) {
    Write-Host "   $($i + 1). $($checklist[$i])" -ForegroundColor Cyan
}

Write-Host "`n📸 ACTION: Take screenshots of the WezTerm window" -ForegroundColor Yellow
Write-Host "   Suggested names:" -ForegroundColor Gray
Write-Host "   - wezterm-console-full.png (full window)" -ForegroundColor Gray
Write-Host "   - wezterm-ui-detail.png (UI close-up)" -ForegroundColor Gray
Write-Host "   - wezterm-terminal-info.png (if terminal info is shown)" -ForegroundColor Gray
Write-Host "`n   Save to: $ScreenshotsDir\" -ForegroundColor Gray

# Step 5: Ask user for verification status
Write-Host "`n[Step 5/6] Verification Status" -ForegroundColor Yellow
$response = Read-Host "Did the app render successfully in WezTerm? (y/n)"

if ($response -eq 'y') {
    Write-Host "✅ Great! Marking T076 as PASS" -ForegroundColor Green
    $testStatus = "PASS"
    $testNotes = "App rendered successfully in WezTerm with Kitty graphics protocol enabled"
} else {
    Write-Host "⚠️  Marking T076 as FAIL or PARTIAL" -ForegroundColor Yellow
    $testStatus = "FAIL"
    $failReason = Read-Host "Brief description of issue"
    $testNotes = "Issue: $failReason"
}

# Step 6: Update test report
Write-Host "`n[Step 6/6] Updating test report..." -ForegroundColor Yellow
$reportPath = "$ReportsDir\t076-wezterm-test.md"

if (Test-Path $reportPath) {
    $report = Get-Content $reportPath -Raw
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    # Update status
    $report = $report -replace '\*\*Status\*\*:.*', "**Status**: $testStatus"
    $report = $report -replace '\*\*Date\*\*:.*', "**Date**: $timestamp"
    
    # Add notes section if FAIL
    if ($testStatus -eq "FAIL") {
        $report += "`n`n## Additional Notes`n`n$testNotes`n"
    }
    
    Set-Content $reportPath $report -NoNewline
    Write-Host "✅ Updated: $reportPath" -ForegroundColor Green
}

# Summary
Write-Host "`n=== Phase 2 Verification Summary ===" -ForegroundColor Cyan
Write-Host "WezTerm Status: " -NoNewline
Write-Host "✅ Running" -ForegroundColor Green
Write-Host "T076 Test: " -NoNewline
if ($testStatus -eq "PASS") {
    Write-Host "✅ $testStatus" -ForegroundColor Green
} else {
    Write-Host "❌ $testStatus" -ForegroundColor Red
}
Write-Host "Screenshots: " -NoNewline
if ((Test-Path $ScreenshotsDir) -and (Get-ChildItem $ScreenshotsDir -Filter "*.png")) {
    Write-Host "✅ Captured" -ForegroundColor Green
} else {
    Write-Host "⚠️  Pending" -ForegroundColor Yellow
}

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Review log files for detailed diagnostic info" -ForegroundColor Gray
Write-Host "   2. Complete screenshot collection (T085)" -ForegroundColor Gray
Write-Host "   3. Verify rendering quality (T093)" -ForegroundColor Gray
Write-Host "   4. Update PHASE2_IN_PROGRESS.md with final results" -ForegroundColor Gray
Write-Host "   5. Move to Phase 3 when ready" -ForegroundColor Gray

Write-Host "`n✨ Phase 2 verification helper complete!`n" -ForegroundColor Cyan
