<#
.SYNOPSIS
Launch console app in WezTerm for Phase 2 testing (T076, T093)

.DESCRIPTION
This script launches the console app in WezTerm with Kitty graphics enabled,
captures logs, and helps verify rendering behavior.

.NOTES
Run this script to execute T076 and T093 tests.
#>

$ErrorActionPreference = "Continue"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SPEC-025 T076: Test Console App in WezTerm" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Paths
$consolePath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"
$logPath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\logs"
$testReportDir = "specs\025-kitty-graphics-scene-rendering\test-reports"
$screenshotDir = "$testReportDir\screenshots"

# Create directories
New-Item -ItemType Directory -Force -Path $screenshotDir | Out-Null

# Check if app exists
if (-not (Test-Path $consolePath)) {
    Write-Host "❌ Console app not found at: $consolePath" -ForegroundColor Red
    Write-Host "   Run: cd dotnet; dotnet build console-app\LablabBean.Console\LablabBean.Console.csproj -c Release" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Console app found" -ForegroundColor Green
Write-Host ""

# Display test instructions
Write-Host "📋 Test Instructions:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   This will launch the console app in WezTerm." -ForegroundColor White
Write-Host "   WezTerm should automatically use Kitty graphics protocol." -ForegroundColor White
Write-Host ""
Write-Host "   What to look for:" -ForegroundColor Cyan
Write-Host "   1. Check if app starts without errors" -ForegroundColor White
Write-Host "   2. Look for log messages about terminal capabilities" -ForegroundColor White
Write-Host "   3. Verify rendering (glyph mode expected with missing tileset)" -ForegroundColor White
Write-Host "   4. Press Ctrl+C to exit when done" -ForegroundColor White
Write-Host ""

# Check for WezTerm
$weztermPath = (Get-Command wezterm -ErrorAction SilentlyContinue).Source
if (-not $weztermPath) {
    Write-Host "❌ WezTerm not found in PATH" -ForegroundColor Red
    exit 1
}

Write-Host "✅ WezTerm found: $weztermPath" -ForegroundColor Green
Write-Host ""

# Clear old logs
if (Test-Path $logPath) {
    Write-Host "📝 Clearing old logs..." -ForegroundColor Cyan
    Remove-Item "$logPath\*.log" -ErrorAction SilentlyContinue
}

Write-Host "🚀 Launching console app in WezTerm..." -ForegroundColor Green
Write-Host "   (Press Ctrl+C in the console to exit when done)" -ForegroundColor Yellow
Write-Host ""

# Launch in WezTerm
$fullPath = Resolve-Path $consolePath
Start-Process wezterm -ArgumentList "start", "--", "powershell", "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Console app will start in 2 seconds...'; Start-Sleep 2; & '$fullPath'"

Write-Host ""
Write-Host "⏳ Waiting 10 seconds for app to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check logs
Write-Host ""
Write-Host "📋 Checking logs for capability detection..." -ForegroundColor Cyan

if (Test-Path $logPath) {
    $latestLog = Get-ChildItem $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($latestLog) {
        Write-Host "   Latest log: $($latestLog.Name)" -ForegroundColor Green
        Write-Host ""
        
        $logContent = Get-Content $latestLog.FullName -Raw
        
        # Look for key log messages
        $patterns = @{
            "Terminal Capabilities" = "Terminal|Capability|Kitty|Sixel"
            "Renderer Mode" = "Renderer.*image mode|SupportsImageMode"
            "Tileset Loading" = "Tileset|Loading tileset|Fallback.*glyph"
            "Kitty Rendering" = "Kitty graphics|RenderViaKittyGraphics|Using Kitty"
            "Errors" = "Error|Exception|Failed"
        }
        
        Write-Host "   Key Log Entries:" -ForegroundColor Yellow
        Write-Host ""
        
        foreach ($pattern in $patterns.GetEnumerator()) {
            Write-Host "   [$($pattern.Key)]" -ForegroundColor Cyan
            $matches = $logContent | Select-String -Pattern $pattern.Value -AllMatches
            
            if ($matches) {
                foreach ($match in ($matches | Select-Object -First 5)) {
                    Write-Host "      $($match.Line.Trim())" -ForegroundColor White
                }
            } else {
                Write-Host "      (No matches found)" -ForegroundColor DarkGray
            }
            Write-Host ""
        }
        
        # Save excerpt to report
        $excerpt = $logContent | Select-String -Pattern "Terminal|Kitty|Renderer|Tileset" | Select-Object -First 20
        $excerpt | Out-File "$testReportDir\t076-log-excerpt.txt"
        Write-Host "   ✅ Log excerpt saved to: t076-log-excerpt.txt" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  No log files found" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️  Log directory not found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Next Steps" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. 📸 Take a screenshot of the WezTerm window" -ForegroundColor Yellow
Write-Host "   Save to: $screenshotDir\wezterm-console-t076.png" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. 📝 Document observations:" -ForegroundColor Yellow
Write-Host "   - Did the app start successfully?" -ForegroundColor White
Write-Host "   - What rendering mode was used?" -ForegroundColor White
Write-Host "   - Were there any errors?" -ForegroundColor White
Write-Host "   - How did the UI look?" -ForegroundColor White
Write-Host ""
Write-Host "3. 🔍 Review logs at:" -ForegroundColor Yellow
Write-Host "   $logPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. ✅ Mark T076 as complete when satisfied" -ForegroundColor Yellow
Write-Host ""

# Create placeholder test report
$reportTemplate = @"
# T076 Test Results - Console App in WezTerm

**Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
**Tester**: [Your Name]
**Environment**: Windows + WezTerm 20221119-145034-49b9839f

## Test Execution

**Status**: IN PROGRESS / PASS / FAIL

### Setup
- ✅ WezTerm installed and configured
- ✅ Kitty graphics enabled in .wezterm.lua
- ✅ Console app built (Release mode)
- ⚠️  Tileset missing (testing fallback mode)

### Observations

**App Launch**:
- [ ] App started successfully
- [ ] No errors on startup
- [ ] UI rendered correctly

**Terminal Capabilities**:
- [ ] Kitty graphics detected: YES / NO
- [ ] Log shows capability detection
- [ ] TERM_PROGRAM environment variable correct

**Rendering Mode**:
- [ ] Image mode attempted: YES / NO
- [ ] Fallback to glyph mode: YES / NO
- [ ] Rendering quality: GOOD / ACCEPTABLE / POOR

**Visual Quality**:
- [ ] Text is readable
- [ ] Colors are correct
- [ ] Layout is proper
- [ ] No rendering artifacts

### Screenshots

![WezTerm Console]($screenshotDir\wezterm-console-t076.png)

### Log Excerpt

``````
[Paste relevant log entries here]
``````

### Issues Found

[List any issues or unexpected behavior]

### Conclusion

**Test Result**: PASS / FAIL
**Reason**: [Brief explanation]

### Next Steps

- [ ] T085 - Take more screenshots
- [ ] T093 - Verify pixel graphics quality (if image mode works)
- [ ] Create actual tileset for full image mode testing

---

**Test completed**: [Date]
"@

$reportTemplate | Out-File "$testReportDir\t076-wezterm-test.md" -Encoding UTF8

Write-Host "📄 Test report template created: t076-wezterm-test.md" -ForegroundColor Green
Write-Host "   Edit this file with your observations" -ForegroundColor Cyan
Write-Host ""
