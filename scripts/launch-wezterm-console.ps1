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
