#!/usr/bin/env pwsh
# Website build script with GitVersion integration

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "🌐 Building website..." -ForegroundColor Cyan

# Get version from GitVersion
$gitVersionPath = Join-Path $PSScriptRoot ".." "build" "_artifacts"
$versionInfo = $null

try {
    Push-Location (Join-Path $PSScriptRoot "..")
    $gitVersionJson = & dotnet tool run gitversion /showvariable FullSemVer 2>$null
    if ($LASTEXITCODE -eq 0) {
        $version = $gitVersionJson.Trim()
        Write-Host "✓ Version: $version" -ForegroundColor Green

        # Set version in environment for build
        $env:LABLAB_VERSION = $version
    } else {
        Write-Host "⚠ GitVersion not available, using default version" -ForegroundColor Yellow
        $version = "0.1.0-dev"
        $env:LABLAB_VERSION = $version
    }
} finally {
    Pop-Location
}

# Install dependencies if needed
if (-not (Test-Path "node_modules")) {
    Write-Host "📦 Installing dependencies..." -ForegroundColor Cyan
    pnpm install
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install dependencies"
    }
}

# Build all packages
Write-Host "🔨 Building packages..." -ForegroundColor Cyan
pnpm run build:all
if ($LASTEXITCODE -ne 0) {
    throw "Failed to build packages"
}

Write-Host "✓ Website built successfully" -ForegroundColor Green
Write-Host "📦 Output: apps/web/dist" -ForegroundColor Cyan
