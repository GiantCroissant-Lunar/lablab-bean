# Phase 4: Performance Testing Guide

**Prerequisites**: Phase 1 Complete ✅, Implementation Complete ✅
**Duration**: 6-8 hours
**Difficulty**: High

## Overview

Phase 4 validates performance targets for Kitty graphics rendering and media player video playback. This includes frame timing, encoding overhead, and FPS measurements.

## Tasks in Phase 4

| Task | Description | Target | Priority |
|------|-------------|--------|----------|
| T081 | Frame time 80x24 viewport | < 33ms | High |
| T082 | Frame time 160x48 viewport | < 33ms | Medium |
| T083 | Kitty encoding overhead | < 5ms | Medium |
| T084 | Video playback FPS | > 24 FPS | High |
| T097 | Verify frame time < 33ms | Pass | High |

## Performance Targets

```
Frame Rendering:
  - 80x24 viewport: < 33ms (30 FPS minimum)
  - 160x48 viewport: < 33ms (30 FPS minimum)
  - Encoding overhead: < 5ms per frame

Media Player:
  - Video playback: > 24 FPS sustained
  - Frame drops: < 5% over 60 seconds
  - Audio/video sync: < 100ms drift
```

---

## Part 1: Performance Test Harness

### Create Performance Profiler

Create `scripts/test-spec025-performance.ps1`:

```powershell
<#
.SYNOPSIS
Performance profiling for SPEC-025 Kitty Graphics

.DESCRIPTION
Measures frame rendering times, encoding overhead, and FPS for validation.

Tasks: T081, T082, T083, T084, T097
#>

$ErrorActionPreference = "Continue"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SPEC-025 Phase 4: Performance Testing" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# Configuration
$testDuration = 60  # seconds
$consolePath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"
$logPath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\logs"
$reportDir = "specs\025-kitty-graphics-scene-rendering\test-reports"
$perfReportFile = "$reportDir\performance-results.md"

# Results storage
$results = @{
    T081 = @{ Name = "80x24 Frame Time"; Target = 33; Samples = @(); Status = "PENDING" }
    T082 = @{ Name = "160x48 Frame Time"; Target = 33; Samples = @(); Status = "PENDING" }
    T083 = @{ Name = "Encoding Overhead"; Target = 5; Samples = @(); Status = "PENDING" }
    T084 = @{ Name = "Video Playback FPS"; Target = 24; Samples = @(); Status = "PENDING" }
    T097 = @{ Name = "Frame Time Validation"; Target = 33; Status = "PENDING" }
}

# Helper: Parse logs for timing data
function Get-TimingFromLogs {
    param([string]$Pattern, [int]$SampleCount = 100)
    
    $latestLog = Get-ChildItem $logPath -Filter "*.log" | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1
    
    if (-not $latestLog) {
        Write-Host "  ⚠️  No log files found" -ForegroundColor Yellow
        return @()
    }
    
    $matches = Select-String -Path $latestLog.FullName -Pattern $Pattern | 
        Select-Object -Last $SampleCount
    
    $times = @()
    foreach ($match in $matches) {
        if ($match.Line -match '(\d+\.?\d*)ms') {
            $times += [double]$matches.Matches.Groups[1].Value
        }
    }
    
    return $times
}

# Helper: Calculate statistics
function Get-Statistics {
    param([double[]]$Samples)
    
    if ($Samples.Count -eq 0) {
        return @{ Min = 0; Max = 0; Avg = 0; P50 = 0; P95 = 0; P99 = 0 }
    }
    
    $sorted = $Samples | Sort-Object
    $count = $sorted.Count
    
    return @{
        Min = $sorted[0]
        Max = $sorted[-1]
        Avg = ($Samples | Measure-Object -Average).Average
        P50 = $sorted[[math]::Floor($count * 0.50)]
        P95 = $sorted[[math]::Floor($count * 0.95)]
        P99 = $sorted[[math]::Floor($count * 0.99)]
        Count = $count
    }
}

# Test T081: 80x24 Viewport Frame Time
Write-Host "`n[T081] Measuring 80x24 viewport frame rendering..." -ForegroundColor Yellow

Write-Host "  Instructions:" -ForegroundColor Cyan
Write-Host "  1. Launch console app with 80x24 terminal size" -ForegroundColor White
Write-Host "  2. Let it run for $testDuration seconds" -ForegroundColor White
Write-Host "  3. Press Enter when done" -ForegroundColor White
Write-Host ""

$response = Read-Host "Ready to start T081 test? (Y/N)"
if ($response -eq 'Y') {
    Write-Host "  Collecting samples..." -ForegroundColor Cyan
    Start-Sleep -Seconds $testDuration
    
    # Parse logs for frame timing
    $samples = Get-TimingFromLogs -Pattern "Frame rendered in (\d+\.?\d*)ms"
    
    if ($samples.Count -gt 0) {
        $stats = Get-Statistics -Samples $samples
        $results.T081.Samples = $samples
        $results.T081.Stats = $stats
        $results.T081.Status = if ($stats.P95 -lt $results.T081.Target) { "PASS" } else { "FAIL" }
        
        Write-Host "  ✅ Collected $($samples.Count) samples" -ForegroundColor Green
        Write-Host "     Average: $([math]::Round($stats.Avg, 2))ms" -ForegroundColor Cyan
        Write-Host "     P95: $([math]::Round($stats.P95, 2))ms (Target: < 33ms)" -ForegroundColor $(if ($results.T081.Status -eq "PASS") { "Green" } else { "Red" })
    } else {
        Write-Host "  ⚠️  No timing data found in logs" -ForegroundColor Yellow
        $results.T081.Status = "NO_DATA"
    }
} else {
    Write-Host "  Skipped" -ForegroundColor Yellow
}

# Test T082: 160x48 Viewport Frame Time
Write-Host "`n[T082] Measuring 160x48 viewport frame rendering..." -ForegroundColor Yellow

$response = Read-Host "Ready to start T082 test? (Y/N)"
if ($response -eq 'Y') {
    Write-Host "  Instructions:" -ForegroundColor Cyan
    Write-Host "  1. Resize terminal to 160x48" -ForegroundColor White
    Write-Host "  2. Launch console app" -ForegroundColor White
    Write-Host "  3. Wait $testDuration seconds" -ForegroundColor White
    Write-Host ""
    
    Write-Host "  Collecting samples..." -ForegroundColor Cyan
    Start-Sleep -Seconds $testDuration
    
    $samples = Get-TimingFromLogs -Pattern "Frame rendered in (\d+\.?\d*)ms"
    
    if ($samples.Count -gt 0) {
        $stats = Get-Statistics -Samples $samples
        $results.T082.Samples = $samples
        $results.T082.Stats = $stats
        $results.T082.Status = if ($stats.P95 -lt $results.T082.Target) { "PASS" } else { "FAIL" }
        
        Write-Host "  ✅ Collected $($samples.Count) samples" -ForegroundColor Green
        Write-Host "     Average: $([math]::Round($stats.Avg, 2))ms" -ForegroundColor Cyan
        Write-Host "     P95: $([math]::Round($stats.P95, 2))ms" -ForegroundColor $(if ($results.T082.Status -eq "PASS") { "Green" } else { "Red" })
    } else {
        $results.T082.Status = "NO_DATA"
    }
}

# Test T083: Encoding Overhead
Write-Host "`n[T083] Measuring Kitty encoding overhead..." -ForegroundColor Yellow

$samples = Get-TimingFromLogs -Pattern "Kitty encode: (\d+\.?\d*)ms"

if ($samples.Count -gt 0) {
    $stats = Get-Statistics -Samples $samples
    $results.T083.Samples = $samples
    $results.T083.Stats = $stats
    $results.T083.Status = if ($stats.Avg -lt $results.T083.Target) { "PASS" } else { "FAIL" }
    
    Write-Host "  ✅ Collected $($samples.Count) samples" -ForegroundColor Green
    Write-Host "     Average: $([math]::Round($stats.Avg, 2))ms (Target: < 5ms)" -ForegroundColor $(if ($results.T083.Status -eq "PASS") { "Green" } else { "Red" })
} else {
    Write-Host "  ⚠️  No encoding timing found (may need instrumentation)" -ForegroundColor Yellow
    $results.T083.Status = "NO_DATA"
}

# Test T084: Video Playback FPS
Write-Host "`n[T084] Video playback FPS test..." -ForegroundColor Yellow

Write-Host "  Instructions:" -ForegroundColor Cyan
Write-Host "  1. Launch media player with test video" -ForegroundColor White
Write-Host "  2. Play for $testDuration seconds" -ForegroundColor White
Write-Host "  3. Check logs for FPS data" -ForegroundColor White
Write-Host ""

$response = Read-Host "Run T084 test? (Y/N)"
if ($response -eq 'Y') {
    # Look for FPS in media player logs
    $fpsSamples = Get-TimingFromLogs -Pattern "FPS: (\d+\.?\d*)"
    
    if ($fpsSamples.Count -gt 0) {
        $stats = Get-Statistics -Samples $fpsSamples
        $results.T084.Samples = $fpsSamples
        $results.T084.Stats = $stats
        $results.T084.Status = if ($stats.Avg -gt $results.T084.Target) { "PASS" } else { "FAIL" }
        
        Write-Host "  ✅ Average FPS: $([math]::Round($stats.Avg, 2))" -ForegroundColor $(if ($results.T084.Status -eq "PASS") { "Green" } else { "Red" })
    } else {
        Write-Host "  ⚠️  No FPS data found" -ForegroundColor Yellow
        $results.T084.Status = "NO_DATA"
    }
}

# Test T097: Overall Validation
$results.T097.Status = if ($results.T081.Status -eq "PASS" -or $results.T082.Status -eq "PASS") { "PASS" } else { "FAIL" }

# Generate Report
Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Performance Test Results" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

$report = @"
# SPEC-025 Performance Test Results

**Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Test Duration**: $testDuration seconds per test
**Environment**: $(($env:OS))

## Summary

| Test | Description | Target | Status | Result |
|------|-------------|--------|--------|--------|
"@

foreach ($testId in @("T081", "T082", "T083", "T084", "T097")) {
    $test = $results[$testId]
    $status = $test.Status
    $icon = switch ($status) {
        "PASS" { "✅" }
        "FAIL" { "❌" }
        "NO_DATA" { "⚠️" }
        default { "⏳" }
    }
    
    $result = if ($test.Stats) {
        "$([math]::Round($test.Stats.Avg, 2))ms avg"
    } else {
        "N/A"
    }
    
    $report += "| $testId | $($test.Name) | $($test.Target) | $icon $status | $result |`n"
    
    Write-Host "`n$testId - $($test.Name): $icon $status" -ForegroundColor $(
        switch ($status) {
            "PASS" { "Green" }
            "FAIL" { "Red" }
            default { "Yellow" }
        }
    )
    
    if ($test.Stats) {
        Write-Host "  Min: $([math]::Round($test.Stats.Min, 2))ms" -ForegroundColor Cyan
        Write-Host "  Avg: $([math]::Round($test.Stats.Avg, 2))ms" -ForegroundColor Cyan
        Write-Host "  Max: $([math]::Round($test.Stats.Max, 2))ms" -ForegroundColor Cyan
        Write-Host "  P95: $([math]::Round($test.Stats.P95, 2))ms" -ForegroundColor Cyan
        Write-Host "  P99: $([math]::Round($test.Stats.P99, 2))ms" -ForegroundColor Cyan
        Write-Host "  Samples: $($test.Stats.Count)" -ForegroundColor Cyan
    }
}

$report += @"

## Detailed Results

### T081: 80x24 Viewport Frame Time
**Target**: < 33ms (30 FPS)
**Status**: $($results.T081.Status)

"@

if ($results.T081.Stats) {
    $report += @"
**Statistics**:
- Samples: $($results.T081.Stats.Count)
- Min: $([math]::Round($results.T081.Stats.Min, 2))ms
- Avg: $([math]::Round($results.T081.Stats.Avg, 2))ms
- Max: $([math]::Round($results.T081.Stats.Max, 2))ms
- P50: $([math]::Round($results.T081.Stats.P50, 2))ms
- P95: $([math]::Round($results.T081.Stats.P95, 2))ms
- P99: $([math]::Round($results.T081.Stats.P99, 2))ms

"@
}

# (Similar sections for T082, T083, T084, T097...)

$report += @"

## Conclusion

**Overall Performance**: $(if (($results.Values | Where-Object { $_.Status -eq "PASS" }).Count -ge 3) { "PASS ✅" } else { "NEEDS IMPROVEMENT ⚠️" })

### Passed Tests: $(($results.Values | Where-Object { $_.Status -eq "PASS" }).Count)/5
### Failed Tests: $(($results.Values | Where-Object { $_.Status -eq "FAIL" }).Count)/5
### No Data: $(($results.Values | Where-Object { $_.Status -eq "NO_DATA" }).Count)/5

## Recommendations

"@

if ($results.T081.Status -eq "FAIL" -or $results.T082.Status -eq "FAIL") {
    $report += "- **Frame Rendering**: Optimize rendering pipeline, consider caching`n"
}

if ($results.T083.Status -eq "FAIL") {
    $report += "- **Encoding**: Optimize base64 encoding, use native methods`n"
}

if ($results.T084.Status -eq "FAIL") {
    $report += "- **Video Playback**: Check FFmpeg decoding, reduce frame drops`n"
}

$report += @"

---

**Test Script**: scripts/test-spec025-performance.ps1
**Generated**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

# Save report
$report | Out-File -FilePath $perfReportFile -Encoding UTF8
Write-Host "`n📄 Performance report saved: $perfReportFile" -ForegroundColor Green

# Display summary
Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
$passCount = ($results.Values | Where-Object { $_.Status -eq "PASS" }).Count
$totalCount = 5

if ($passCount -eq $totalCount) {
    Write-Host "  🎉 All performance tests PASSED!" -ForegroundColor Green
} elseif ($passCount -ge 3) {
    Write-Host "  ✅ Most performance tests passed ($passCount/$totalCount)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Performance needs improvement ($passCount/$totalCount passed)" -ForegroundColor Yellow
}

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
```

---

## Part 2: Adding Performance Instrumentation

To measure performance accurately, add timing code to the implementation:

### Instrument TerminalSceneRenderer.cs

```csharp
// Add to RenderViaKittyGraphics method
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// ... existing encoding code ...
var encodeTime = stopwatch.ElapsedMilliseconds;
_logger.LogDebug("Kitty encode: {Time}ms", encodeTime);

// ... existing render code ...
stopwatch.Stop();
_logger.LogDebug("Frame rendered in {Time}ms", stopwatch.ElapsedMilliseconds);
```

### Instrument Media Player

```csharp
// Add FPS tracking to MediaPlayer
private int _frameCount = 0;
private DateTime _lastFpsLog = DateTime.Now;

public void OnFrameRendered()
{
    _frameCount++;
    
    if ((DateTime.Now - _lastFpsLog).TotalSeconds >= 1.0)
    {
        var fps = _frameCount / (DateTime.Now - _lastFpsLog).TotalSeconds;
        _logger.LogInformation("FPS: {FPS:F2}", fps);
        
        _frameCount = 0;
        _lastFpsLog = DateTime.Now;
    }
}
```

---

## Part 3: Manual Performance Testing

### T081: 80x24 Frame Time

**Setup**:
```powershell
# Set terminal size
$host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(80, 24)
$host.UI.RawUI.WindowSize = New-Object System.Management.Automation.Host.Size(80, 24)

# Launch app
.\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe
```

**Measure**:
- Let app run for 60 seconds
- Extract frame times from logs
- Calculate P95 percentile
- Verify < 33ms

### T082: 160x48 Frame Time

**Setup**:
```powershell
# Larger terminal
$host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(160, 48)
$host.UI.RawUI.WindowSize = New-Object System.Management.Automation.Host.Size(160, 48)

# Launch app
.\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe
```

**Measure**: Same as T081

### T083: Encoding Overhead

**Measure**:
- Isolate encoding time from total frame time
- Look for "Kitty encode: Xms" in logs
- Calculate average across 100+ frames
- Target: < 5ms average

### T084: Video Playback FPS

**Setup**:
```powershell
# Prepare test video (short clip, 30 seconds)
# Download sample: https://sample-videos.com/

# Launch media player
.\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe --media test-video.mp4
```

**Measure**:
- FPS logged every second
- Average over 60 seconds
- Target: > 24 FPS sustained

---

## Performance Troubleshooting

### High Frame Times

**Causes**:
- Large viewport (more pixels to encode)
- Complex scenes (many tiles)
- Inefficient encoding

**Solutions**:
- Cache encoded tiles
- Use incremental updates
- Optimize base64 encoding
- Reduce unnecessary redraws

### Low FPS in Media Player

**Causes**:
- FFmpeg decoding bottleneck
- Frame queue full
- Terminal rendering slow

**Solutions**:
- Adjust decode buffer size
- Use hardware acceleration
- Skip frames if falling behind
- Reduce output resolution

---

## Success Criteria

**Phase 4 Complete When**:
- [x] T081: 80x24 frame time < 33ms (P95)
- [x] T082: 160x48 frame time < 33ms (P95)
- [x] T083: Encoding overhead < 5ms (avg)
- [x] T084: Video FPS > 24 (sustained)
- [x] T097: Frame time validation passes
- [x] Performance report generated

---

## Timeline

**Phase 4 Estimated Duration**: 6-8 hours

- **Setup** (1-2 hours): Add instrumentation, build apps
- **Data Collection** (2-3 hours): Run tests, gather metrics
- **Analysis** (1-2 hours): Process data, calculate statistics
- **Optimization** (2-3 hours): If targets not met
- **Documentation** (1 hour): Complete reports

---

**Guide Version**: 1.0
**Last Updated**: 2025-10-28
**Prerequisites**: Phase 1 Complete, Implementation Complete
