#!/usr/bin/env pwsh
# Quick launcher for versioned media player artifact

$BuildVersion = "0.0.4-021-unified-media-player.1"
$ArtifactPath = "build\_artifacts\$BuildVersion\publish\console"
$Executable = Join-Path $ArtifactPath "LablabBean.Console.exe"

# Check if executable exists
if (-not (Test-Path $Executable)) {
    Write-Error "Executable not found: $Executable"
    Write-Host "Available options:" -ForegroundColor Yellow
    Write-Host "  1. Run 'task publish' to build artifacts" -ForegroundColor Yellow
    Write-Host "  2. Run 'scripts\test\test-apps-verification.ps1' to verify builds" -ForegroundColor Yellow
    exit 1
}

Write-Host "🎬 Launching Unified Media Player" -ForegroundColor Cyan
Write-Host "Version: $BuildVersion" -ForegroundColor Gray
Write-Host "Location: $ArtifactPath" -ForegroundColor Gray
Write-Host ""

Push-Location $ArtifactPath
& ".\LablabBean.Console.exe" $args
Pop-Location
