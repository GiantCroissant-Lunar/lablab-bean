#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and deploys the demo plugin to the plugins directory.

.DESCRIPTION
    This script builds the LablabBean.Plugin.Demo project and copies the output
    to the plugins/demo-plugin directory for testing.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.EXAMPLE
    .\scripts\deploy-demo-plugin.ps1
    
.EXAMPLE
    .\scripts\deploy-demo-plugin.ps1 -Configuration Debug
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot "..\dotnet\examples\LablabBean.Plugin.Demo"
$outputPath = Join-Path $projectPath "bin\$Configuration\net8.0"
$destPath = Join-Path $PSScriptRoot "..\plugins\demo-plugin"

Write-Host "🔨 Building demo plugin..." -ForegroundColor Cyan
dotnet build $projectPath --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "📦 Deploying to plugins directory..." -ForegroundColor Cyan
if (Test-Path $destPath) {
    Remove-Item $destPath -Recurse -Force
}

New-Item -ItemType Directory -Path $destPath -Force | Out-Null
Copy-Item "$outputPath\*" -Destination $destPath -Recurse -Force

Write-Host "✅ Demo plugin deployed successfully to: $destPath" -ForegroundColor Green
Write-Host ""
Write-Host "Files deployed:" -ForegroundColor Yellow
Get-ChildItem $destPath | Select-Object Name, Length | Format-Table -AutoSize

Write-Host ""
Write-Host "To test the plugin:" -ForegroundColor Cyan
Write-Host "  1. Ensure appsettings.json has 'plugins' in Plugins.Paths"
Write-Host "  2. Run the console or Windows host application"
Write-Host "  3. Check logs for plugin initialization messages"
