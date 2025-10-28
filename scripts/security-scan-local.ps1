#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Local security scanning using TruffleHog via Docker
.DESCRIPTION
    Runs TruffleHog in a Docker container to scan for secrets locally.
    This is useful for testing before pushing to GitHub.
.PARAMETER FullScan
    Run a full repository scan instead of just recent changes
.PARAMETER OutputFile
    Specify output file for results (default: security-scan-results.json)
#>

param(
    [switch]$FullScan,
    [string]$OutputFile = "security-scan-results.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "🔍 Local Security Scan with TruffleHog" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is available
try {
    docker --version | Out-Null
    Write-Host "✅ Docker is available" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not available or not running" -ForegroundColor Red
    Write-Host "   Please install Docker Desktop and ensure it's running" -ForegroundColor Yellow
    Write-Host "   Download: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

# Prepare scan parameters
$scanArgs = @(
    "run", "--rm", "-v", "${PWD}:/workdir", "-w", "/workdir",
    "trufflesecurity/trufflehog:latest",
    "filesystem", "/workdir"
)

if ($FullScan) {
    Write-Host "🔍 Running full repository scan..." -ForegroundColor Yellow
    $scanArgs += @("--only-verified", "--json")
} else {
    Write-Host "🔍 Running scan on recent changes..." -ForegroundColor Yellow
    # Get recent commits (last 10)
    $recentCommits = git log --oneline -10 --pretty=format:"%H" | Select-Object -First 1
    if ($recentCommits) {
        $scanArgs += @("--since-commit", $recentCommits, "--only-verified", "--json")
    } else {
        Write-Host "⚠️  No recent commits found, running full scan" -ForegroundColor Yellow
        $scanArgs += @("--only-verified", "--json")
    }
}

# Add output file
$scanArgs += @("--output", $OutputFile)

Write-Host "📋 Scan configuration:" -ForegroundColor Gray
Write-Host "   Mode: $(if ($FullScan) { 'Full repository' } else { 'Recent changes' })" -ForegroundColor Gray
Write-Host "   Output: $OutputFile" -ForegroundColor Gray
Write-Host "   Verified secrets only: Yes" -ForegroundColor Gray
Write-Host ""

# Run the scan
Write-Host "🚀 Starting TruffleHog scan..." -ForegroundColor Yellow
try {
    & docker @scanArgs
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "✅ Scan completed successfully - No secrets found!" -ForegroundColor Green
    } elseif ($exitCode -eq 183) {
        Write-Host "⚠️  Scan completed - Secrets detected!" -ForegroundColor Red
        Write-Host "   Check $OutputFile for details" -ForegroundColor Yellow
    } else {
        Write-Host "❌ Scan failed with exit code: $exitCode" -ForegroundColor Red
    }

    # Show results summary if file exists
    if (Test-Path $OutputFile) {
        $fileSize = (Get-Item $OutputFile).Length
        if ($fileSize -gt 0) {
            Write-Host ""
            Write-Host "📊 Results saved to: $OutputFile ($fileSize bytes)" -ForegroundColor Cyan

            # Try to parse and show summary
            try {
                $results = Get-Content $OutputFile | ConvertFrom-Json
                if ($results.Count -gt 0) {
                    Write-Host "🔍 Found $($results.Count) potential secret(s):" -ForegroundColor Yellow
                    $results | ForEach-Object {
                        Write-Host "   - $($_.DetectorName): $($_.SourceMetadata.Data.Filesystem.file)" -ForegroundColor Red
                    }
                }
            } catch {
                Write-Host "   (Could not parse results for summary)" -ForegroundColor Gray
            }
        } else {
            Write-Host "✅ No secrets detected" -ForegroundColor Green
        }
    }

} catch {
    Write-Host "❌ Error running TruffleHog: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Cyan
Write-Host "   - Use -FullScan for complete repository scan" -ForegroundColor Gray
Write-Host "   - Results are saved in JSON format for analysis" -ForegroundColor Gray
Write-Host "   - GitHub workflow runs this automatically on push/PR" -ForegroundColor Gray
