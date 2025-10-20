#!/usr/bin/env pwsh
# Quick build and run script for Lablab Bean

Write-Host "🔨 Lablab Bean - Build & Run" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Check if task is available
if (-not (Get-Command task -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Task (go-task) is not installed!" -ForegroundColor Red
    Write-Host "   Install from: https://taskfile.dev/installation/" -ForegroundColor Yellow
    exit 1
}

# Check if pm2 is available
if (-not (Get-Command pm2 -ErrorAction SilentlyContinue)) {
    Write-Host "❌ PM2 is not installed!" -ForegroundColor Red
    Write-Host "   Install with: npm install -g pm2" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Prerequisites check passed" -ForegroundColor Green
Write-Host ""

# Build release
Write-Host "📦 Building release..." -ForegroundColor Cyan
task build-release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Build completed successfully!" -ForegroundColor Green
Write-Host ""

# Ask user if they want to start the stack
$response = Read-Host "Do you want to start the stack now? (Y/n)"
if ($response -eq "" -or $response -eq "Y" -or $response -eq "y") {
    Write-Host ""
    Write-Host "🚀 Starting stack..." -ForegroundColor Cyan
    task stack-run
    
    Write-Host ""
    Write-Host "✓ Stack started!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Useful commands:" -ForegroundColor Cyan
    Write-Host "   task stack-status    - Check status" -ForegroundColor Gray
    Write-Host "   task stack-logs      - View logs" -ForegroundColor Gray
    Write-Host "   task stack-stop      - Stop stack" -ForegroundColor Gray
    Write-Host "   task stack-monit     - Monitor dashboard" -ForegroundColor Gray
    Write-Host "   task test-web        - Run Playwright tests" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🌐 Web interface: http://localhost:3000" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "✓ Build complete. Start the stack with: task stack-run" -ForegroundColor Green
}
