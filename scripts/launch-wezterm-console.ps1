param(
    [int] $TimeoutSeconds = 300,
    [int] $PollIntervalMs = 2000
)

$ErrorActionPreference = 'Stop'

Write-Host "[WezTerm-Launcher] Waiting for console artifact under build/_artifacts (timeout: ${TimeoutSeconds}s)"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

$exePath = $null
while ((Get-Date) -lt $deadline) {
    $artRoot = Join-Path -Path 'build' -ChildPath '_artifacts'
    if (Test-Path $artRoot) {
        $verDir = Get-ChildItem $artRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($null -ne $verDir) {
            $candidate = Join-Path $verDir.FullName 'publish\console\LablabBean.Console.exe'
            if (Test-Path $candidate) {
                $exePath = $candidate
                break
            }
        }
    }
    Start-Sleep -Milliseconds $PollIntervalMs
}

if (-not $exePath) {
    Write-Error "[WezTerm-Launcher] Timed out waiting for LablabBean.Console.exe in build/_artifacts"
    exit 1
}

Write-Host "[WezTerm-Launcher] Found console artifact: $exePath"

# Try to locate WezTerm from env, repo tools, or PATH
$wezterm = $null
try {
    if ($env:LABLAB_WEZTERM_PATH -and (Test-Path $env:LABLAB_WEZTERM_PATH)) {
        $wezterm = (Resolve-Path $env:LABLAB_WEZTERM_PATH).Path
        Write-Host "[WezTerm-Launcher] Using WezTerm from LABLAB_WEZTERM_PATH: $wezterm"
    }
    if (-not $wezterm) {
        $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
        # Try extracted WezTerm directory first (has all dependencies)
        $extractedWez = Get-ChildItem (Join-Path $repoRoot 'tools/wezterm') -Filter 'wezterm.exe' -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.DirectoryName -notlike '*\tools\wezterm' } |
            Select-Object -First 1
        if ($extractedWez) {
            $wezterm = $extractedWez.FullName
            Write-Host "[WezTerm-Launcher] Using WezTerm from tools (extracted): $wezterm"
        } else {
            # Fallback to root wezterm.exe
            $toolsWez = Join-Path $repoRoot 'tools/wezterm/wezterm.exe'
            if (Test-Path $toolsWez) {
                $wezterm = (Resolve-Path $toolsWez).Path
                Write-Host "[WezTerm-Launcher] Using WezTerm from tools: $wezterm"
            }
        }
    }
    if (-not $wezterm) {
        $cmd = $null
        try { $cmd = Get-Command wezterm -ErrorAction Stop } catch {}
        if ($cmd -and $cmd.Source -and (Test-Path $cmd.Source)) {
            $wezterm = $cmd.Source
            Write-Host "[WezTerm-Launcher] Using WezTerm from PATH: $wezterm"
        }
    }
} catch {
    Write-Host "[WezTerm-Launcher] WezTerm detection error: $($_.Exception.Message)"
}

if ($wezterm) {
    Write-Host "[WezTerm-Launcher] Launching console via WezTerm..."
    # Use wezterm-gui if available for better window visibility
    $weztermGui = Join-Path (Split-Path $wezterm -Parent) 'wezterm-gui.exe'
    $weztermExe = if (Test-Path $weztermGui) { $weztermGui } else { $wezterm }
    $exeDir = Split-Path $exePath -Parent
    & $weztermExe start --cwd $exeDir -- $exePath @args
    $exit = $LASTEXITCODE
    Write-Host "[WezTerm-Launcher] WezTerm launch completed with exit code: $exit"
    exit 0
}

Write-Host "[WezTerm-Launcher] WezTerm not found. Falling back to direct execution."

# Defer actual execution to the existing helper which supports arbitrary args
$helper = Join-Path $PSScriptRoot 'run-console-with-args.ps1'
if (-not (Test-Path $helper)) {
    Write-Error "[WezTerm-Launcher] Helper not found: $helper"
    exit 1
}

# Forward any arguments passed to this script
& $helper @args
$exit = $LASTEXITCODE
Write-Host "[WezTerm-Launcher] Console exited with code: $exit"
exit 0
