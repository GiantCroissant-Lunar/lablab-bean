<#
.SYNOPSIS
    Migrates windows app to separate repository with NuGet package references.

.DESCRIPTION
    This script:
    1. Copies windows-app/ and windows-ui/ to lablab-bean-windows/dotnet/
    2. Creates NuGet.config pointing to local feed
    3. Converts ProjectReference to PackageReference in .csproj files
    4. Creates new solution file for windows app
    5. Backs up original files

.PARAMETER SourcePath
    Source path of windows app. Default: ..\dotnet\windows-app

.PARAMETER SourceUIPath
    Source path of windows UI libraries. Default: ..\dotnet\windows-ui

.PARAMETER DestinationRepo
    Destination repository path. Default: D:\lunar-snake\personal-work\yokan-projects\lablab-bean-windows

.PARAMETER FrameworkVersion
    Version of framework packages to reference. Default: 0.0.3

.PARAMETER WhatIf
    Show what would happen without making changes

.EXAMPLE
    .\migrate-windows-app.ps1

.EXAMPLE
    .\migrate-windows-app.ps1 -FrameworkVersion "0.0.4" -WhatIf
#>

param(
    [Parameter()]
    [string]$SourcePath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\windows-app",

    [Parameter()]
    [string]$SourceUIPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\windows-ui",

    [Parameter()]
    [string]$DestinationRepo = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean-windows",

    [Parameter()]
    [string]$FrameworkVersion = "0.0.3",

    [Parameter()]
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Windows App Migration Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Source (App): $SourcePath" -ForegroundColor Green
Write-Host "Source (UI): $SourceUIPath" -ForegroundColor Green
Write-Host "Destination: $DestinationRepo\dotnet" -ForegroundColor Green
Write-Host "Framework Version: $FrameworkVersion" -ForegroundColor Green
if ($WhatIf) {
    Write-Host "Mode: DRY RUN" -ForegroundColor Yellow
}
Write-Host ""

# Validate sources exist
if (-not (Test-Path $SourcePath)) {
    throw "Source path does not exist: $SourcePath"
}

if (-not (Test-Path $SourceUIPath)) {
    throw "Source UI path does not exist: $SourceUIPath"
}

$destinationAppPath = Join-Path $DestinationRepo "dotnet\windows-app"
$destinationUIPath = Join-Path $DestinationRepo "dotnet\windows-ui"

try {
    # Step 1: Create destination directory
    Write-Host "[1/7] Creating destination directory structure..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        if (-not (Test-Path $DestinationRepo)) {
            New-Item -Path $DestinationRepo -ItemType Directory -Force | Out-Null
        }

        $dotnetPath = Join-Path $DestinationRepo "dotnet"
        if (-not (Test-Path $dotnetPath)) {
            New-Item -Path $dotnetPath -ItemType Directory -Force | Out-Null
        }
    }

    Write-Host "  ✓ Directory structure ready" -ForegroundColor Green
    Write-Host ""

    # Step 2: Copy windows-app folder
    Write-Host "[2/7] Copying windows app files..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        # Remove destination if it exists
        if (Test-Path $destinationAppPath) {
            Write-Host "  Removing existing windows-app..." -ForegroundColor Gray
            Remove-Item -Path $destinationAppPath -Recurse -Force
        }

        # Copy entire windows-app folder
        Copy-Item -Path $SourcePath -Destination $destinationAppPath -Recurse -Force
        Write-Host "  ✓ Windows app files copied" -ForegroundColor Green
    } else {
        Write-Host "  Would copy: $SourcePath -> $destinationAppPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 3: Copy windows-ui folder
    Write-Host "[3/7] Copying windows UI files..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        # Remove destination if it exists
        if (Test-Path $destinationUIPath) {
            Write-Host "  Removing existing windows-ui..." -ForegroundColor Gray
            Remove-Item -Path $destinationUIPath -Recurse -Force
        }

        # Copy entire windows-ui folder
        Copy-Item -Path $SourceUIPath -Destination $destinationUIPath -Recurse -Force
        Write-Host "  ✓ Windows UI files copied" -ForegroundColor Green
    } else {
        Write-Host "  Would copy: $SourceUIPath -> $destinationUIPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 4: Create NuGet.config
    Write-Host "[4/7] Creating NuGet.config..." -ForegroundColor Yellow

    $nugetConfigPath = Join-Path (Join-Path $DestinationRepo "dotnet") "NuGet.config"
    $nugetConfigContent = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Local development feed -->
    <add key="LablabBeanLocal" value="D:\lunar-snake\packages\nuget-repo" />
    <!-- Official NuGet feed -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>

  <packageSourceMapping>
    <!-- Map LablabBean packages to local feed -->
    <packageSource key="LablabBeanLocal">
      <package pattern="LablabBean.*" />
    </packageSource>
    <!-- Map all other packages to NuGet.org -->
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
'@

    if (-not $WhatIf) {
        Set-Content -Path $nugetConfigPath -Value $nugetConfigContent -Force
        Write-Host "  ✓ NuGet.config created" -ForegroundColor Green
    } else {
        Write-Host "  Would create: $nugetConfigPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 5: Convert ProjectReferences to PackageReferences
    Write-Host "[5/7] Converting project references to package references..." -ForegroundColor Yellow

    $csprojFiles = @()
    $csprojFiles += Get-ChildItem -Path $destinationAppPath -Filter "*.csproj" -Recurse
    $csprojFiles += Get-ChildItem -Path $destinationUIPath -Filter "*.csproj" -Recurse

    $conversionMap = @{
        # Framework references
        '..\..\framework\LablabBean.Core\LablabBean.Core.csproj' = 'LablabBean.Core'
        '..\..\framework\LablabBean.Infrastructure\LablabBean.Infrastructure.csproj' = 'LablabBean.Infrastructure'
        '..\..\framework\LablabBean.Reactive\LablabBean.Reactive.csproj' = 'LablabBean.Reactive'
        '..\..\framework\LablabBean.Game.Core\LablabBean.Game.Core.csproj' = 'LablabBean.Game.Core'
        '..\..\framework\LablabBean.Plugins.Core\LablabBean.Plugins.Core.csproj' = 'LablabBean.Plugins.Core'
        '..\..\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj' = 'LablabBean.Plugins.Contracts'
        '..\..\framework\LablabBean.Reporting.Contracts\LablabBean.Reporting.Contracts.csproj' = 'LablabBean.Reporting.Contracts'
        '..\..\framework\LablabBean.Reporting.Analytics\LablabBean.Reporting.Analytics.csproj' = 'LablabBean.Reporting.Analytics'
        '..\..\framework\LablabBean.SourceGenerators.Reporting\LablabBean.SourceGenerators.Reporting.csproj' = 'LablabBean.SourceGenerators.Reporting'
        '..\..\framework\LablabBean.Contracts.UI\LablabBean.Contracts.UI.csproj' = 'LablabBean.Contracts.UI'
        '..\..\framework\LablabBean.Contracts.Game.UI\LablabBean.Contracts.Game.UI.csproj' = 'LablabBean.Contracts.Game.UI'
        '..\..\framework\LablabBean.Rendering.Contracts\LablabBean.Rendering.Contracts.csproj' = 'LablabBean.Rendering.Contracts'

        # Plugin references
        '..\..\plugins\LablabBean.Plugins.Reporting.Html\LablabBean.Plugins.Reporting.Html.csproj' = 'LablabBean.Plugins.Reporting.Html'
        '..\..\plugins\LablabBean.Plugins.Reporting.Csv\LablabBean.Plugins.Reporting.Csv.csproj' = 'LablabBean.Plugins.Reporting.Csv'
    }

    foreach ($csproj in $csprojFiles) {
        Write-Host "  Processing: $($csproj.Name)" -ForegroundColor Gray

        if ($WhatIf) {
            Write-Host "    Would convert ProjectReferences to PackageReferences" -ForegroundColor Yellow
            continue
        }

        $content = Get-Content $csproj.FullName -Raw
        $originalContent = $content
        $changesCount = 0

        # Convert each ProjectReference
        foreach ($projectPath in $conversionMap.Keys) {
            $packageName = $conversionMap[$projectPath]

            # Handle both regular and analyzer references
            $patterns = @(
                "<ProjectReference Include=`"$([regex]::Escape($projectPath))`" />",
                "<ProjectReference Include=`"$([regex]::Escape($projectPath))`"`s+OutputItemType=`"Analyzer`"`s+ReferenceOutputAssembly=`"false`"`s*/>"
            )

            foreach ($pattern in $patterns) {
                if ($content -match $pattern) {
                    $isAnalyzer = $pattern -like "*Analyzer*"

                    if ($isAnalyzer) {
                        $replacement = "<PackageReference Include=`"$packageName`" Version=`"$FrameworkVersion`" OutputItemType=`"Analyzer`" ReferenceOutputAssembly=`"false`" />"
                    } else {
                        $replacement = "<PackageReference Include=`"$packageName`" Version=`"$FrameworkVersion`" />"
                    }

                    $content = $content -replace [regex]::Escape($Matches[0]), $replacement
                    Write-Host "    - Converted: $packageName" -ForegroundColor Green
                    $changesCount++
                }
            }
        }

        # Save if changes were made
        if ($content -ne $originalContent) {
            Set-Content -Path $csproj.FullName -Value $content -NoNewline
            Write-Host "    ✓ $changesCount reference(s) converted" -ForegroundColor Green
        } else {
            Write-Host "    No conversions needed" -ForegroundColor DarkGray
        }
    }

    Write-Host ""

    # Step 6: Create solution file
    Write-Host "[6/7] Creating solution file..." -ForegroundColor Yellow

    $solutionPath = Join-Path (Join-Path $DestinationRepo "dotnet") "LablabBean.Windows.sln"

    if (-not $WhatIf) {
        Push-Location (Join-Path $DestinationRepo "dotnet")

        # Create new solution
        dotnet new sln -n "LablabBean.Windows" -o . --force | Out-Null

        # Add windows-app projects
        $appProjects = Get-ChildItem -Path $destinationAppPath -Filter "*.csproj" -Recurse
        foreach ($csproj in $appProjects) {
            dotnet sln $solutionPath add $csproj.FullName --solution-folder "windows-app" | Out-Null
            Write-Host "  Added (app): $($csproj.Name)" -ForegroundColor Gray
        }

        # Add windows-ui projects
        $uiProjects = Get-ChildItem -Path $destinationUIPath -Filter "*.csproj" -Recurse
        foreach ($csproj in $uiProjects) {
            dotnet sln $solutionPath add $csproj.FullName --solution-folder "windows-ui" | Out-Null
            Write-Host "  Added (ui): $($csproj.Name)" -ForegroundColor Gray
        }

        Pop-Location
        Write-Host "  ✓ Solution created: LablabBean.Windows.sln" -ForegroundColor Green
    } else {
        Write-Host "  Would create: $solutionPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 7: Copy Directory.Build.props and Directory.Packages.props
    Write-Host "[7/7] Copying build configuration files..." -ForegroundColor Yellow

    $sourceBuildProps = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\Directory.Build.props"
    $sourcePackagesProps = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\Directory.Packages.props"

    $destBuildProps = Join-Path (Join-Path $DestinationRepo "dotnet") "Directory.Build.props"
    $destPackagesProps = Join-Path (Join-Path $DestinationRepo "dotnet") "Directory.Packages.props"

    if (-not $WhatIf) {
        if (Test-Path $sourceBuildProps) {
            Copy-Item -Path $sourceBuildProps -Destination $destBuildProps -Force
            Write-Host "  ✓ Directory.Build.props copied" -ForegroundColor Green
        }

        if (Test-Path $sourcePackagesProps) {
            Copy-Item -Path $sourcePackagesProps -Destination $destPackagesProps -Force
            Write-Host "  ✓ Directory.Packages.props copied" -ForegroundColor Green
        }
    } else {
        Write-Host "  Would copy build configuration files" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  ✓ Windows app migration completed!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""

    if (-not $WhatIf) {
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Ensure framework packages are built (run pack-framework.ps1)" -ForegroundColor Gray
        Write-Host "  2. cd $DestinationRepo\dotnet" -ForegroundColor Gray
        Write-Host "  3. dotnet restore" -ForegroundColor Gray
        Write-Host "  4. dotnet build" -ForegroundColor Gray
        Write-Host "  5. Test the windows app" -ForegroundColor Gray
    } else {
        Write-Host "Run without -WhatIf to execute migration" -ForegroundColor Yellow
    }

    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "  ✗ Migration failed!" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}
