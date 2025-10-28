param(
    [switch] $AutoWezTerm,
    [string] $WezTermPath,
    [switch] $Dashboard = $true
)

$ErrorActionPreference = 'Stop'

# Resolve repo root from script location
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

# WezTerm mandatory for dev
$env:ASPIRE_LAUNCH_WEZTERM = '1'

# Resolve WezTerm path preference
function Set-WezTermPathFromTools {
    # Try extracted WezTerm directory first (has all dependencies)
    $extractedWez = Get-ChildItem (Join-Path $repoRoot 'tools/wezterm') -Filter 'wezterm.exe' -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.DirectoryName -notlike '*\tools\wezterm' } |
        Select-Object -First 1
    if ($extractedWez) {
        $env:LABLAB_WEZTERM_PATH = $extractedWez.FullName
        Write-Host "Using WezTerm (tools/extracted): $($extractedWez.FullName)"
        return $true
    }
    # Fallback to root wezterm.exe
    $toolsWez = Join-Path 'tools' 'wezterm/wezterm.exe'
    if (Test-Path $toolsWez) {
        $resolved = (Resolve-Path $toolsWez).Path
        $env:LABLAB_WEZTERM_PATH = $resolved
        Write-Host "Using WezTerm (tools): $resolved"
        return $true
    }
    return $false
}

if ($WezTermPath -and (Test-Path $WezTermPath)) {
    $env:LABLAB_WEZTERM_PATH = (Resolve-Path $WezTermPath).Path
    Write-Host "Using WezTerm (param): $($env:LABLAB_WEZTERM_PATH)"
} elseif ($AutoWezTerm) {
    if (-not (Set-WezTermPathFromTools)) {
        $wez = Get-ChildItem -Path 'build\_artifacts' -Recurse -Filter 'wezterm.exe' -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        if ($null -ne $wez) {
            $env:LABLAB_WEZTERM_PATH = $wez.FullName
            Write-Host "Using WezTerm (artifacts): $($wez.FullName)"
        } else {
            Write-Host "wezterm.exe not found under build/_artifacts"
        }
    }
} else {
    if (-not $env:LABLAB_WEZTERM_PATH) { [void](Set-WezTermPathFromTools) }
}

# Detect latest artifact version directory
$artRoot = Join-Path -Path 'build' -ChildPath '_artifacts'
if (Test-Path $artRoot) {
    $verDir = Get-ChildItem $artRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -ne $verDir) {
        $env:LABLAB_ARTIFACT_DIR = $verDir.FullName
        Write-Host "Using Artifact Dir: $($env:LABLAB_ARTIFACT_DIR)"
    }
}

# Run Aspire
$aspireCli = Join-Path $env:USERPROFILE '.dotnet\tools\aspire.exe'
$ranAspireCli = $false
if (Test-Path $aspireCli) {
    if ($Dashboard) { $env:ASPIRE_DASHBOARD_ENABLED = '1'; $env:DOTNET_ASPIRE_DASHBOARD_ENABLED = '1' }
    try {
        $appHostDir = Join-Path $repoRoot 'dotnet/apphost/LablabBean.AppHost'
        $appHostProj = Join-Path $appHostDir 'LablabBean.AppHost.csproj'
        if (Test-Path $appHostProj) {
            & $aspireCli run --project $appHostProj
        } else {
            & $aspireCli run --project $appHostDir
        }
        $ranAspireCli = $LASTEXITCODE -eq 0
    } catch {
        Write-Host "Aspire CLI failed: $($_.Exception.Message)"
        $ranAspireCli = $false
    }
} else {
    try {
        aspire --version | Out-Null
        if ($Dashboard) { $env:ASPIRE_DASHBOARD_ENABLED = '1'; $env:DOTNET_ASPIRE_DASHBOARD_ENABLED = '1' }
        $appHostDir = Join-Path $repoRoot 'dotnet/apphost/LablabBean.AppHost'
        $appHostProj = Join-Path $appHostDir 'LablabBean.AppHost.csproj'
        if (Test-Path $appHostProj) {
            aspire run --project $appHostProj
        } else {
            aspire run --project $appHostDir
        }
        $ranAspireCli = $LASTEXITCODE -eq 0
    } catch {
        $ranAspireCli = $false
    }
}

if (-not $ranAspireCli) {
    Write-Host 'Falling back to dotnet run for AppHost'
    # Disable dashboard to avoid missing CliPath/DashboardPath errors when running without Aspire CLI
    $env:ASPIRE_DASHBOARD_ENABLED = 'false'
    $env:DOTNET_ASPIRE_DASHBOARD_ENABLED = 'false'
    dotnet run --project 'dotnet/apphost/LablabBean.AppHost'
}
