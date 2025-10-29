#!/usr/bin/env pwsh
# SPEC-025 Phase 3 Manual Execution Helper
# Simplified launcher and log checker

$ErrorActionPreference = "Stop"

Write-Host "`n=== SPEC-025 Phase 3: Environment Testing ===" -ForegroundColor Cyan
Write-Host "Focus: T095 - Cross-Platform Tileset Loading`n" -ForegroundColor Gray

$consolePath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"
$windowsPath = "dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe"
$tilesetPath = "assets\tiles.png"
$screenshotDir = "specs\025-kitty-graphics-scene-rendering\test-reports\screenshots"

# Verify prerequisites
Write-Host "[1/4] Checking Prerequisites..." -ForegroundColor Yellow

$ready = $true
if (Test-Path $tilesetPath) {
    $size = (Get-Item $tilesetPath).Length
    Write-Host "  ✅ Tileset: assets\tiles.png ($size bytes)" -ForegroundColor Green
} else {
    Write-Host "  ❌ Tileset not found: $tilesetPath" -ForegroundColor Red
    $ready = $false
}

if (Test-Path $consolePath) {
    Write-Host "  ✅ Console app: Built (Release)" -ForegroundColor Green
} else {
    Write-Host "  ❌ Console app not built" -ForegroundColor Red
    Write-Host "     Run: cd dotnet; dotnet build console-app\LablabBean.Console\LablabBean.Console.csproj -c Release" -ForegroundColor Yellow
    $ready = $false
}

if (Test-Path $windowsPath) {
    Write-Host "  ✅ Windows app: Built (Release)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Windows app not built (optional)" -ForegroundColor Yellow
}

if (-not $ready) {
    Write-Host "`n❌ Prerequisites not met. Exiting.`n" -ForegroundColor Red
    exit 1
}

# Test selection
Write-Host "`n[2/4] Test Selection" -ForegroundColor Yellow
Write-Host "  Choose which test to run:`n" -ForegroundColor Gray
Write-Host "  1. T095-A: Launch Console App + Check Tileset Loading" -ForegroundColor Cyan
Write-Host "  2. T095-B: Launch Windows App + Check Tileset Loading" -ForegroundColor Cyan
Write-Host "  3. T095-C: Launch Both Apps (side-by-side)" -ForegroundColor Cyan
Write-Host "  4. View Log Analysis" -ForegroundColor Cyan
Write-Host "  5. Exit" -ForegroundColor Gray

$choice = Read-Host "`n  Choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host "`n[3/4] Launching Console App..." -ForegroundColor Yellow
        Write-Host "  Path: $consolePath" -ForegroundColor Gray
        Write-Host "  This will open in a new terminal window`n" -ForegroundColor Gray
        
        Start-Process $consolePath -WorkingDirectory (Get-Location)
        
        Write-Host "✅ Console app launched!" -ForegroundColor Green
        Write-Host "`n[4/4] Manual Verification Steps:" -ForegroundColor Yellow
        Write-Host "  1. Check the console window for UI rendering" -ForegroundColor Gray
        Write-Host "  2. Look for tileset loading messages in logs" -ForegroundColor Gray
        Write-Host "  3. Take screenshot (Win+Shift+S)" -ForegroundColor Gray
        Write-Host "     Save as: console-tileset-t095.png" -ForegroundColor Gray
        Write-Host "  4. Press Ctrl+C to exit the app" -ForegroundColor Gray
        
        Write-Host "`nWaiting 15 seconds for app to initialize..." -ForegroundColor Gray
        Start-Sleep -Seconds 15
        
        # Check logs
        Write-Host "`n📋 Checking logs for tileset references..." -ForegroundColor Cyan
        $logDir = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\logs"
        if (Test-Path $logDir) {
            $latestLog = Get-ChildItem $logDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            if ($latestLog) {
                Write-Host "Latest log: $($latestLog.Name)`n" -ForegroundColor Gray
                $content = Get-Content $latestLog.FullName -Raw
                
                $keywords = @("tileset", "tiles.png", "Tileset", "tile", "png", "image", "loading")
                $found = $false
                foreach ($keyword in $keywords) {
                    $matches = $content | Select-String -Pattern $keyword -AllMatches
                    if ($matches) {
                        $found = $true
                        Write-Host "  Found '$keyword' in logs" -ForegroundColor Green
                    }
                }
                
                if (-not $found) {
                    Write-Host "  ⚠️  No tileset keywords found in logs" -ForegroundColor Yellow
                    Write-Host "     The app may not be logging tileset loading" -ForegroundColor Gray
                }
            }
        }
    }
    
    "2" {
        if (-not (Test-Path $windowsPath)) {
            Write-Host "`n❌ Windows app not found at: $windowsPath" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "`n[3/4] Launching Windows App..." -ForegroundColor Yellow
        Write-Host "  Path: $windowsPath" -ForegroundColor Gray
        Write-Host "  This will open in a graphical window`n" -ForegroundColor Gray
        
        Start-Process $windowsPath -WorkingDirectory (Get-Location)
        
        Write-Host "✅ Windows app launched!" -ForegroundColor Green
        Write-Host "`n[4/4] Manual Verification Steps:" -ForegroundColor Yellow
        Write-Host "  1. Check the Windows app window for rendering" -ForegroundColor Gray
        Write-Host "  2. Look for tileset graphics vs ASCII fallback" -ForegroundColor Gray
        Write-Host "  3. Take screenshot (Win+Shift+S)" -ForegroundColor Gray
        Write-Host "     Save as: windows-tileset-t095.png" -ForegroundColor Gray
        Write-Host "  4. Close the app window" -ForegroundColor Gray
    }
    
    "3" {
        Write-Host "`n[3/4] Launching Both Apps..." -ForegroundColor Yellow
        
        Write-Host "  Starting Console app..." -ForegroundColor Gray
        Start-Process $consolePath -WorkingDirectory (Get-Location)
        Start-Sleep -Seconds 3
        
        if (Test-Path $windowsPath) {
            Write-Host "  Starting Windows app..." -ForegroundColor Gray
            Start-Process $windowsPath -WorkingDirectory (Get-Location)
        }
        
        Write-Host "`n✅ Both apps launched!" -ForegroundColor Green
        Write-Host "`n[4/4] Manual Verification Steps:" -ForegroundColor Yellow
        Write-Host "  1. Arrange windows side-by-side" -ForegroundColor Gray
        Write-Host "  2. Compare rendering quality" -ForegroundColor Gray
        Write-Host "  3. Verify both load the same tileset" -ForegroundColor Gray
        Write-Host "  4. Take screenshot of both" -ForegroundColor Gray
        Write-Host "     Save as: both-apps-comparison-t095.png" -ForegroundColor Gray
    }
    
    "4" {
        Write-Host "`n[3/4] Analyzing Logs..." -ForegroundColor Yellow
        
        $logLocations = @(
            "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\logs",
            "dotnet\console-app\LablabBean.Console\bin\Debug\net8.0\logs",
            "dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\logs"
        )
        
        foreach ($logDir in $logLocations) {
            if (Test-Path $logDir) {
                Write-Host "`nLogs in: $logDir" -ForegroundColor Cyan
                $logs = Get-ChildItem $logDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
                
                if ($logs) {
                    Write-Host "  Latest: $($logs.Name)" -ForegroundColor Gray
                    $content = Get-Content $logs.FullName
                    
                    $tilesetLines = $content | Select-String -Pattern "tileset|tiles\.png|Tileset" -CaseSensitive:$false
                    if ($tilesetLines) {
                        Write-Host "  Tileset-related lines:" -ForegroundColor Green
                        $tilesetLines | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
                    } else {
                        Write-Host "  No tileset references found" -ForegroundColor Yellow
                    }
                }
            }
        }
    }
    
    "5" {
        Write-Host "`nExiting...`n" -ForegroundColor Gray
        exit 0
    }
    
    default {
        Write-Host "`n❌ Invalid choice. Exiting.`n" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n📊 Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Capture screenshots as instructed" -ForegroundColor Gray
Write-Host "  2. Save to: $screenshotDir\" -ForegroundColor Gray
Write-Host "  3. Document findings in test report" -ForegroundColor Gray
Write-Host "  4. Update PROGRESS.md when complete" -ForegroundColor Gray

Write-Host "`n✅ Phase 3 helper complete!`n" -ForegroundColor Cyan
