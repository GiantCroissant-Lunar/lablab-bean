#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Phase 4 Integration Test - Terminal.Gui UI Stack
.DESCRIPTION
    Tests the Terminal UI plugin with live game state to verify:
    - UI initialization
    - Plugin discovery and loading
    - HUD rendering
    - World view rendering
    - Activity log integration
#>

param(
    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "🎯 Phase 4 - Terminal.Gui Integration Test" -ForegroundColor Cyan
Write-Host ""

# Build if needed
if (-not $SkipBuild) {
    Write-Host "📦 Building console app..." -ForegroundColor Yellow
    Push-Location "dotnet/console-app/LablabBean.Console"
    try {
        dotnet build --no-restore -c Debug
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Build succeeded" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
    Write-Host ""
}

# Run the app (should load UI.Terminal plugin)
Write-Host "🚀 Starting LablabBean console app with Terminal UI..." -ForegroundColor Yellow
Write-Host "   (Press Ctrl+Q to exit Terminal.Gui)" -ForegroundColor Gray
Write-Host ""

Push-Location "dotnet/console-app/LablabBean.Console"
try {
    # Run without any CLI args so it enters interactive mode
    dotnet run --no-build -c Debug
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "✅ Test complete" -ForegroundColor Green
