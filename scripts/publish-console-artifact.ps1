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

# Post-publish validation checklist
Write-Host "`n=== Post-Publish Validation Checklist ==="
Write-Host "Verifying plugin manifest layout..."

$pluginsDir = Join-Path $pubConsole 'plugins'
if (Test-Path $pluginsDir) {
    $pluginDirs = Get-ChildItem $pluginsDir -Directory
    Write-Host "Found $($pluginDirs.Count) plugin directories:"
    
    $missingManifests = @()
    foreach ($dir in $pluginDirs) {
        $manifestPath = Join-Path $dir.FullName 'plugin.json'
        if (Test-Path $manifestPath) {
            Write-Host "  ✓ $($dir.Name)/plugin.json"
        } else {
            Write-Host "  ✗ $($dir.Name)/plugin.json - MISSING"
            $missingManifests += $dir.Name
        }
    }
    
    if ($missingManifests.Count -gt 0) {
        Write-Warning "Missing plugin.json in: $($missingManifests -join ', ')"
    } else {
        Write-Host "`n✅ All plugin manifests present"
    }
} else {
    Write-Warning "Plugins directory not found: $pluginsDir"
}

Write-Host "=========================================`n"
