$ErrorActionPreference = 'Stop'

# Ensure artifacts root exists
$artRoot = Join-Path -Path 'build' -ChildPath '_artifacts'
if (-not (Test-Path $artRoot)) {
    New-Item -ItemType Directory -Path $artRoot | Out-Null
}

# Pick latest versioned artifact folder or create a local one
$verDir = Get-ChildItem $artRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $verDir) {
    $verDir = New-Item -ItemType Directory -Path (Join-Path $artRoot 'local-dev')
}

# Publish console app to the versioned artifact path
$pubConsole = Join-Path $verDir.FullName 'publish\console'
Write-Host "Publishing console app to: $pubConsole"

# Clean stale publish directory to avoid leftover plugin.json from previous builds
if (Test-Path $pubConsole) {
    Write-Host "Cleaning previous publish output..."
    Remove-Item -Recurse -Force $pubConsole
}

# Build console only (no plugins) to avoid plugin publish blockers
# Route plugin manifests to subfolders to avoid file collisions
& dotnet publish 'dotnet/console-app/LablabBean.Console/LablabBean.Console.csproj' -c Debug -o $pubConsole -r win-x64 --self-contained true /p:PluginJsonTargetSubfolder=true
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

Write-Host "Console artifact created at: $pubConsole"
