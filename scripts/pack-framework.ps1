<#
.SYNOPSIS
    Builds and packages all Lablab Bean framework and plugin projects to local NuGet feed.

.DESCRIPTION
    This script:
    1. Cleans previous build outputs
    2. Restores NuGet packages
    3. Builds all framework and plugin projects
    4. Creates NuGet packages (.nupkg)
    5. Publishes packages to local NuGet feed

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER Version
    Override package version. If not specified, uses version from Directory.Build.props

.PARAMETER LocalFeedPath
    Path to local NuGet feed. Default: D:\lunar-snake\packages\nuget-repo

.PARAMETER SkipBuild
    Skip building and only pack existing binaries. Default: false

.PARAMETER SkipTests
    Skip running tests before packaging. Default: false

.EXAMPLE
    .\pack-framework.ps1

.EXAMPLE
    .\pack-framework.ps1 -Configuration Debug -Version 0.0.4-alpha

.EXAMPLE
    .\pack-framework.ps1 -SkipBuild -SkipTests
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$Version,

    [Parameter()]
    [string]$LocalFeedPath = 'D:\lunar-snake\packages\nuget-repo',

    [Parameter()]
    [switch]$SkipBuild,

    [Parameter()]
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Navigate to dotnet folder
$scriptRoot = Split-Path -Parent $PSScriptRoot
$dotnetRoot = Join-Path $scriptRoot 'dotnet'
Push-Location $dotnetRoot

try {
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Lablab Bean Framework Packaging Script" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""

    # Validate local feed exists
    if (-not (Test-Path $LocalFeedPath)) {
        Write-Host "Creating local NuGet feed directory: $LocalFeedPath" -ForegroundColor Yellow
        New-Item -Path $LocalFeedPath -ItemType Directory -Force | Out-Null
    }

    Write-Host "Configuration: $Configuration" -ForegroundColor Green
    Write-Host "Local Feed: $LocalFeedPath" -ForegroundColor Green
    if ($Version) {
        Write-Host "Version Override: $Version" -ForegroundColor Green
    }
    Write-Host ""

    # Step 1: Clean previous outputs
    if (-not $SkipBuild) {
        Write-Host "[1/6] Cleaning previous build outputs..." -ForegroundColor Yellow
        dotnet clean --configuration $Configuration --verbosity quiet

        # Remove bin/obj folders for thorough clean
        Get-ChildItem -Path . -Include bin,obj -Recurse -Directory |
            Where-Object { $_.FullName -notlike "*\node_modules\*" } |
            Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

        Write-Host "  ✓ Clean completed" -ForegroundColor Green
        Write-Host ""
    }

    # Step 2: Restore packages
    if (-not $SkipBuild) {
        Write-Host "[2/6] Restoring NuGet packages..." -ForegroundColor Yellow
        dotnet restore --verbosity quiet
        Write-Host "  ✓ Restore completed" -ForegroundColor Green
        Write-Host ""
    }

    # Step 3: Build solution
    if (-not $SkipBuild) {
        Write-Host "[3/6] Building solution..." -ForegroundColor Yellow

        $buildArgs = @(
            'build'
            '--configuration', $Configuration
            '--no-restore'
            '--verbosity', 'minimal'
        )

        if ($Version) {
            $buildArgs += "/p:Version=$Version"
        }

        dotnet @buildArgs

        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        Write-Host "  ✓ Build completed" -ForegroundColor Green
        Write-Host ""
    }

    # Step 4: Run tests (optional)
    if (-not $SkipTests -and -not $SkipBuild) {
        Write-Host "[4/6] Running tests..." -ForegroundColor Yellow

        $testArgs = @(
            'test'
            '--configuration', $Configuration
            '--no-build'
            '--verbosity', 'minimal'
            '--logger', 'console;verbosity=minimal'
        )

        dotnet @testArgs

        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠ Tests failed, but continuing with packaging..." -ForegroundColor Yellow
        } else {
            Write-Host "  ✓ Tests passed" -ForegroundColor Green
        }
        Write-Host ""
    } else {
        Write-Host "[4/6] Skipping tests" -ForegroundColor Gray
        Write-Host ""
    }

    # Step 5: Pack framework and plugins
    Write-Host "[5/6] Creating NuGet packages..." -ForegroundColor Yellow

    # Find all packable projects (framework and plugins, excluding tests, examples, apps)
    $packableProjects = @()

    # Framework projects
    $frameworkProjects = Get-ChildItem -Path .\framework -Filter *.csproj -Recurse |
        Where-Object {
            $_.FullName -notlike "*\tests\*" -and
            $_.Directory.Name -ne 'tests'
        }
    $packableProjects += $frameworkProjects

    # Plugin projects
    $pluginProjects = Get-ChildItem -Path .\plugins -Filter *.csproj -Recurse |
        Where-Object {
            $_.FullName -notlike "*\tests\*"
        }
    $packableProjects += $pluginProjects

    Write-Host "  Found $($packableProjects.Count) packable projects" -ForegroundColor Cyan

    $packedCount = 0
    $skippedCount = 0

    foreach ($project in $packableProjects) {
        $projectName = $project.BaseName
        Write-Host "  Packing: $projectName" -ForegroundColor Gray

        $packArgs = @(
            'pack'
            $project.FullName
            '--configuration', $Configuration
            '--no-build'
            '--output', $LocalFeedPath
            '--verbosity', 'quiet'
        )

        if ($Version) {
            $packArgs += "/p:PackageVersion=$Version"
        }

        dotnet @packArgs

        if ($LASTEXITCODE -eq 0) {
            $packedCount++
            Write-Host "    ✓ $projectName" -ForegroundColor Green
        } else {
            $skippedCount++
            Write-Host "    ⊘ $projectName (not packable or error)" -ForegroundColor DarkGray
        }
    }

    Write-Host ""
    Write-Host "  Packed: $packedCount packages" -ForegroundColor Green
    Write-Host "  Skipped: $skippedCount projects" -ForegroundColor DarkGray
    Write-Host ""

    # Step 6: List created packages
    Write-Host "[6/6] Packages in local feed:" -ForegroundColor Yellow

    $packages = Get-ChildItem -Path $LocalFeedPath -Filter "LablabBean.*.nupkg" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 10

    foreach ($package in $packages) {
        $size = [math]::Round($package.Length / 1KB, 1)
        Write-Host "  • $($package.Name) ($size KB)" -ForegroundColor Cyan
    }

    $totalPackages = (Get-ChildItem -Path $LocalFeedPath -Filter "LablabBean.*.nupkg").Count
    if ($totalPackages -gt 10) {
        Write-Host "  ... and $($totalPackages - 10) more packages" -ForegroundColor DarkGray
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  ✓ Packaging completed successfully!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Update app projects to use these packages" -ForegroundColor Gray
    Write-Host "  2. Run 'dotnet restore' in app repositories" -ForegroundColor Gray
    Write-Host "  3. Build and test apps with new framework versions" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "  ✗ Packaging failed!" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
