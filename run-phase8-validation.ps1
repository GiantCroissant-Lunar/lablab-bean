# Phase 8 Minimal Validation Script
# Tests: T076, T085, T093 (WezTerm smoke test + visual check)

Write-Host ""
Write-Host "======================================"
Write-Host " Phase 8 Minimal Validation Tests"
Write-Host "======================================"
Write-Host ""

$ErrorActionPreference = "Continue"

# Check WezTerm installation
Write-Host "[1/5] Checking WezTerm..." -ForegroundColor Cyan
$wezterm = Get-Command wezterm -ErrorAction SilentlyContinue
if ($wezterm) {
    Write-Host "  ✅ WezTerm found: $($wezterm.Source)" -ForegroundColor Green
} else {
    Write-Host "  ❌ WezTerm not found - install from https://wezfurlong.org/wezterm/" -ForegroundColor Red
    exit 1
}

# Check build status
Write-Host ""
Write-Host "[2/5] Checking build status..." -ForegroundColor Cyan
$consoleDll = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.dll"
if (Test-Path $consoleDll) {
    Write-Host "  ✅ Console app built: $consoleDll" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Console app not built - building now..." -ForegroundColor Yellow
    dotnet build dotnet\console-app\LablabBean.Console\LablabBean.Console.csproj --configuration Release --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Build succeeded" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Build failed" -ForegroundColor Red
        exit 1
    }
}

# Check tileset
Write-Host ""
Write-Host "[3/5] Checking tileset asset..." -ForegroundColor Cyan
if (Test-Path "assets\tiles.png") {
    $size = (Get-Item "assets\tiles.png").Length
    Write-Host "  ✅ Tileset found: assets\tiles.png ($size bytes)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Tileset not found - fallback to glyph mode will be tested" -ForegroundColor Yellow
}

# Check configuration
Write-Host ""
Write-Host "[4/5] Checking configuration..." -ForegroundColor Cyan
$config = Get-Content "dotnet\console-app\LablabBean.Console\appsettings.json" | ConvertFrom-Json
if ($config.Rendering.Terminal.Tileset) {
    Write-Host "  ✅ Tileset config: $($config.Rendering.Terminal.Tileset)" -ForegroundColor Green
    Write-Host "  ✅ Tile size: $($config.Rendering.Terminal.TileSize)" -ForegroundColor Green
    Write-Host "  ✅ PreferHighQuality: $($config.Rendering.Terminal.PreferHighQuality)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  No tileset configured" -ForegroundColor Yellow
}

# Launch in WezTerm
Write-Host ""
Write-Host "[5/5] Launching console in WezTerm..." -ForegroundColor Cyan
Write-Host ""
Write-Host "  Instructions:" -ForegroundColor Yellow
Write-Host "  1. WezTerm window will open with console app" -ForegroundColor White
Write-Host "  2. Verify app launches without crashes (T076)" -ForegroundColor White
Write-Host "  3. Check rendering mode in logs:" -ForegroundColor White
Write-Host "     - Look for 'Kitty graphics' or 'glyph mode'" -ForegroundColor White
Write-Host "  4. Observe visual output (T085, T093):" -ForegroundColor White
Write-Host "     - Kitty: Should see tile graphics" -ForegroundColor White
Write-Host "     - Fallback: Should see ASCII/Unicode glyphs" -ForegroundColor White
Write-Host "  5. Press CTRL+C to exit when done" -ForegroundColor White
Write-Host ""
Write-Host "  Press any key to launch..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')

Write-Host ""
Write-Host "  Launching..." -ForegroundColor Green

# Set TERM environment for WezTerm (enables Kitty detection)
$env:TERM = "wezterm"
$env:TERM_PROGRAM = "WezTerm"

# Launch in WezTerm
wezterm start --cwd (Get-Location) -- dotnet run --project dotnet\console-app\LablabBean.Console\LablabBean.Console.csproj --configuration Release --no-build

Write-Host ""
Write-Host "======================================"
Write-Host " Test Complete"
Write-Host "======================================"
Write-Host ""
Write-Host "Results to verify:" -ForegroundColor Cyan
Write-Host "  [ ] T076: App launched without crashes" -ForegroundColor White
Write-Host "  [ ] T085: Observed console rendering" -ForegroundColor White
Write-Host "  [ ] T093: Confirmed Kitty graphics or glyph fallback" -ForegroundColor White
Write-Host ""
Write-Host "Check logs at: logs\lablab-bean-*.log" -ForegroundColor Yellow
Write-Host ""
