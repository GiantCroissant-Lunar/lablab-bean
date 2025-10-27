#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploy plugins for testing
.DESCRIPTION
    Copies built plugins to the console app's expected plugin directory
#>

param(
    [string]$Configuration = "Debug"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path $PSScriptRoot -Parent
$consoleAppPluginsDir = Join-Path $projectRoot "dotnet\console-app\LablabBean.Console\bin\$Configuration\net8.0\plugins"
$sourcePluginsDir = Join-Path $projectRoot "dotnet\plugins"

Write-Host "🔧 Deploying plugins for integration testing..." -ForegroundColor Cyan
Write-Host "   From: $sourcePluginsDir" -ForegroundColor Gray
Write-Host "   To: $consoleAppPluginsDir" -ForegroundColor Gray
Write-Host ""

# Create target directory if it doesn't exist
if (-not (Test-Path $consoleAppPluginsDir)) {
    New-Item -ItemType Directory -Path $consoleAppPluginsDir -Force | Out-Null
    Write-Host "✅ Created plugins directory" -ForegroundColor Green
}

# Find all plugin DLLs
$pluginProjects = @(
    "LablabBean.Plugins.UI.Terminal",
    "LablabBean.Plugins.Rendering.Terminal"
)

foreach ($pluginName in $pluginProjects) {
    $sourcePath = Join-Path $sourcePluginsDir "$pluginName\bin\$Configuration\net8.0"
    if (-not (Test-Path $sourcePath)) {
        Write-Host "⚠️  Plugin not built: $pluginName" -ForegroundColor Yellow
        continue
    }

    $targetPath = Join-Path $consoleAppPluginsDir $pluginName
    if (-not (Test-Path $targetPath)) {
        New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
    }

    # Copy all files from source to target
    Copy-Item -Path "$sourcePath\*" -Destination $targetPath -Recurse -Force
    Write-Host "✅ Deployed $pluginName" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Plugin deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Deployed plugins:" -ForegroundColor Cyan
Get-ChildItem -Path $consoleAppPluginsDir -Directory | ForEach-Object {
    $dllPath = Join-Path $_.FullName "$($_.Name).dll"
    if (Test-Path $dllPath) {
        Write-Host "   ✓ $($_.Name)" -ForegroundColor Green
    }
}
