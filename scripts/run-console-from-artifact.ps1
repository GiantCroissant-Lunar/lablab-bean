$ErrorActionPreference = 'Stop'

$artRoot = Join-Path -Path 'build' -ChildPath '_artifacts'
if (-not (Test-Path $artRoot)) { throw "Artifacts root not found: $artRoot" }

$verDir = Get-ChildItem $artRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $verDir) { throw "No versioned artifact folder found under $artRoot" }

$exe = Join-Path $verDir.FullName 'publish\console\LablabBean.Console.exe'
if (-not (Test-Path $exe)) { throw "Console exe not found at $exe. Did you run publish-console-artifact.ps1?" }

Write-Host "Running: $exe --help"
& $exe --help
$exit = $LASTEXITCODE
Write-Host "Console exited with code: $exit"
exit 0
