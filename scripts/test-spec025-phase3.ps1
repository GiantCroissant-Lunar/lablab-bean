<#
.SYNOPSIS
Phase 3 Environment Testing for SPEC-025

.DESCRIPTION
Automated testing for T095 (cross-platform tileset loading).
T078 (SSH) requires manual setup.

.NOTES
Part of SPEC-025 testing suite.
#>

$ErrorActionPreference = "Continue"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SPEC-025 Phase 3: Environment Testing" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Paths
$tileset = "assets\tiles.png"
$consolePath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"
$windowsPath = "dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe"
$reportDir = "specs\025-kitty-graphics-scene-rendering\test-reports"

# Results
$results = @{
    T078 = @{ Status = "MANUAL"; Notes = "Requires SSH setup" }
    T095 = @{ Status = "PENDING"; TilesetExists = $false; TilesetValid = $false; ConsoleBuild = $false; WindowsBuild = $false }
}

# T078 - SSH Testing (Manual)
Write-Host "[T078] SSH Session Testing" -ForegroundColor Yellow
Write-Host "  Status: Requires manual setup" -ForegroundColor White
Write-Host "  Guide: PHASE3_GUIDE.md (SSH section)" -ForegroundColor Cyan
Write-Host ""

# T095 - Tileset Loading
Write-Host "[T095] Cross-Platform Tileset Loading" -ForegroundColor Yellow
Write-Host ""

# Check tileset
Write-Host "  [Step 1] Checking tileset..." -ForegroundColor Cyan

if (Test-Path $tileset) {
    $results.T095.TilesetExists = $true
    $file = Get-Item $tileset
    Write-Host "    ✅ Tileset found: $tileset" -ForegroundColor Green
    Write-Host "       Size: $($file.Length) bytes" -ForegroundColor White
    
    # Validate PNG
    try {
        $header = [System.IO.File]::ReadAllBytes($file.FullName) | Select-Object -First 8
        if ($header[0] -eq 0x89 -and $header[1] -eq 0x50 -and $header[2] -eq 0x4E -and $header[3] -eq 0x47) {
            $results.T095.TilesetValid = $true
            Write-Host "    ✅ Valid PNG file" -ForegroundColor Green
        } else {
            Write-Host "    ❌ Invalid PNG header" -ForegroundColor Red
        }
    } catch {
        Write-Host "    ❌ Error reading file: $_" -ForegroundColor Red
    }
} else {
    Write-Host "    ❌ Tileset not found: $tileset" -ForegroundColor Red
    Write-Host "       Create a tileset to run T095" -ForegroundColor Yellow
    Write-Host "       See PHASE3_GUIDE.md for instructions" -ForegroundColor Yellow
}

Write-Host ""

# Check builds
Write-Host "  [Step 2] Checking application builds..." -ForegroundColor Cyan

if (Test-Path $consolePath) {
    $results.T095.ConsoleBuild = $true
    Write-Host "    ✅ Console app (Release): Found" -ForegroundColor Green
} else {
    Write-Host "    ❌ Console app (Release): Not found" -ForegroundColor Red
    Write-Host "       Build: cd dotnet; dotnet build console-app\...\LablabBean.Console.csproj -c Release" -ForegroundColor Yellow
}

if (Test-Path $windowsPath) {
    $results.T095.WindowsBuild = $true
    Write-Host "    ✅ Windows app (Release): Found" -ForegroundColor Green
} else {
    Write-Host "    ❌ Windows app (Release): Not found" -ForegroundColor Red
    Write-Host "       Build: cd dotnet; dotnet build windows-app\...\LablabBean.Windows.csproj -c Release" -ForegroundColor Yellow
}

Write-Host ""

# Test readiness
$canRunT095 = $results.T095.TilesetExists -and $results.T095.TilesetValid -and 
              $results.T095.ConsoleBuild -and $results.T095.WindowsBuild

if ($canRunT095) {
    Write-Host "  [Step 3] Ready to test!" -ForegroundColor Green
    Write-Host ""
    Write-Host "    Manual steps:" -ForegroundColor Yellow
    Write-Host "    1. Launch console app: $consolePath" -ForegroundColor White
    Write-Host "    2. Verify tileset loads (check logs)" -ForegroundColor White
    Write-Host "    3. Take screenshot" -ForegroundColor White
    Write-Host "    4. Launch Windows app: $windowsPath" -ForegroundColor White
    Write-Host "    5. Verify tileset loads (check logs)" -ForegroundColor White
    Write-Host "    6. Take screenshot" -ForegroundColor White
    Write-Host "    7. Compare both screenshots" -ForegroundColor White
    Write-Host ""
    Write-Host "    Screenshots save to: $reportDir\screenshots\" -ForegroundColor Cyan
    Write-Host ""
    
    $response = Read-Host "    Launch Console app now? (Y/N)"
    if ($response -eq 'Y') {
        Write-Host "    🚀 Launching console app..." -ForegroundColor Green
        Start-Process $consolePath
        Start-Sleep -Seconds 3
        Write-Host "    ✅ Console app launched" -ForegroundColor Green
        Write-Host ""
    }
    
    $response = Read-Host "    Launch Windows app now? (Y/N)"
    if ($response -eq 'Y') {
        Write-Host "    🚀 Launching Windows app..." -ForegroundColor Green
        Start-Process $windowsPath
        Start-Sleep -Seconds 3
        Write-Host "    ✅ Windows app launched" -ForegroundColor Green
        Write-Host ""
    }
    
    $results.T095.Status = "LAUNCHED"
} else {
    Write-Host "  [Step 3] Prerequisites missing" -ForegroundColor Red
    Write-Host ""
    if (-not $results.T095.TilesetExists) {
        Write-Host "    ❌ Create tileset: $tileset" -ForegroundColor Yellow
    }
    if (-not $results.T095.ConsoleBuild) {
        Write-Host "    ❌ Build console app (Release mode)" -ForegroundColor Yellow
    }
    if (-not $results.T095.WindowsBuild) {
        Write-Host "    ❌ Build Windows app (Release mode)" -ForegroundColor Yellow
    }
    Write-Host ""
    $results.T095.Status = "BLOCKED"
}

# Summary
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Phase 3 Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "T078 - SSH Testing: $($results.T078.Status)" -ForegroundColor Yellow
Write-Host "  Guide: PHASE3_GUIDE.md" -ForegroundColor Cyan
Write-Host ""

Write-Host "T095 - Tileset Loading: $($results.T095.Status)" -ForegroundColor $(
    switch ($results.T095.Status) {
        "LAUNCHED" { "Green" }
        "BLOCKED" { "Red" }
        default { "Yellow" }
    }
)

if ($results.T095.Status -eq "LAUNCHED") {
    Write-Host "  ✅ Both apps launched" -ForegroundColor Green
    Write-Host "  Next: Verify tileset loading in logs and take screenshots" -ForegroundColor Cyan
} elseif ($results.T095.Status -eq "BLOCKED") {
    Write-Host "  ❌ Prerequisites missing - see above" -ForegroundColor Red
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# Create test report template
$reportContent = @"
# Phase 3 Test Results

**Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Tester**: [Your Name]

## T078 - SSH Session Testing

**Status**: MANUAL / PENDING / COMPLETE

### Setup
- SSH Server: [WSL / Linux / Windows OpenSSH]
- Connection: [localhost / remote]

### Test Results
- [ ] SSH session detected
- [ ] Log shows detection
- [ ] Glyph mode used
- [ ] No Kitty escape sequences
- [ ] UI renders properly

**Screenshots**: [Add links]

**Conclusion**: PASS / FAIL / SKIPPED

---

## T095 - Cross-Platform Tileset Loading

**Status**: $($results.T095.Status)

### Prerequisites
- Tileset exists: $(if ($results.T095.TilesetExists) { "✅" } else { "❌" })
- Tileset valid: $(if ($results.T095.TilesetValid) { "✅" } else { "❌" })
- Console built: $(if ($results.T095.ConsoleBuild) { "✅" } else { "❌" })
- Windows built: $(if ($results.T095.WindowsBuild) { "✅" } else { "❌" })

### Console App
- [ ] App launched successfully
- [ ] Tileset loaded
- [ ] Log shows: "Tileset loaded successfully"
- [ ] Rendering looks correct

**Log Excerpt**:
``````
[Paste console app logs]
``````

### Windows App
- [ ] App launched successfully
- [ ] Tileset loaded
- [ ] Log shows: "Tileset loaded successfully"
- [ ] Rendering looks correct

**Log Excerpt**:
``````
[Paste Windows app logs]
``````

### Visual Comparison
- [ ] Same tiles visible on both
- [ ] Colors match
- [ ] Layout consistent
- [ ] Same quality

**Screenshots**:
- Console: [filename]
- Windows: [filename]
- Comparison: [filename]

**Conclusion**: PASS / FAIL

---

**Phase 3 Status**: IN PROGRESS / COMPLETE
**Overall Result**: $(if ($canRunT095) { "Ready for testing" } else { "Prerequisites needed" })

"@

$reportFile = "$reportDir\phase3-environment-tests.md"
$reportContent | Out-File -FilePath $reportFile -Encoding UTF8

Write-Host "📄 Test report template created: $reportFile" -ForegroundColor Green
Write-Host ""
