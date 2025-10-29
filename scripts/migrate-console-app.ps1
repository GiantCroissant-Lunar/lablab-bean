<#
.SYNOPSIS
    Migrates console app to separate repository with NuGet package references.

.DESCRIPTION
    This script:
    1. Copies console-app/ to lablab-bean-console/dotnet/
    2. Creates NuGet.config pointing to local feed
    3. Converts ProjectReference to PackageReference in .csproj files
    4. Creates new solution file for console app
    5. Backs up original files

.PARAMETER SourcePath
    Source path of console app. Default: ..\dotnet\console-app

.PARAMETER DestinationRepo
    Destination repository path. Default: D:\lunar-snake\personal-work\yokan-projects\lablab-bean-console

.PARAMETER FrameworkVersion
    Version of framework packages to reference. Default: 0.0.3

.PARAMETER WhatIf
    Show what would happen without making changes

.EXAMPLE
    .\migrate-console-app.ps1

.EXAMPLE
    .\migrate-console-app.ps1 -FrameworkVersion "0.0.4" -WhatIf
#>

param(
    [Parameter()]
    [string]$SourcePath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\console-app",

    [Parameter()]
    [string]$DestinationRepo = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean-console",

    [Parameter()]
    [string]$FrameworkVersion = "0.0.3",

    [Parameter()]
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Console App Migration Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Source: $SourcePath" -ForegroundColor Green
Write-Host "Destination: $DestinationRepo\dotnet\console-app" -ForegroundColor Green
Write-Host "Framework Version: $FrameworkVersion" -ForegroundColor Green
if ($WhatIf) {
    Write-Host "Mode: DRY RUN" -ForegroundColor Yellow
}
Write-Host ""

# Validate source exists
if (-not (Test-Path $SourcePath)) {
    throw "Source path does not exist: $SourcePath"
}

$destinationPath = Join-Path $DestinationRepo "dotnet\console-app"

try {
    # Step 1: Create destination directory
    Write-Host "[1/6] Creating destination directory structure..." -ForegroundColor Yellow

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

    # Step 2: Copy console-app folder
    Write-Host "[2/6] Copying console app files..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        # Remove destination if it exists
        if (Test-Path $destinationPath) {
            Write-Host "  Removing existing destination..." -ForegroundColor Gray
            Remove-Item -Path $destinationPath -Recurse -Force
        }

        # Copy entire console-app folder
        Copy-Item -Path $SourcePath -Destination $destinationPath -Recurse -Force
        Write-Host "  ✓ Files copied" -ForegroundColor Green
    } else {
        Write-Host "  Would copy: $SourcePath -> $destinationPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 3: Create NuGet.config
    Write-Host "[3/6] Creating NuGet.config..." -ForegroundColor Yellow

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

    # Step 4: Convert ProjectReferences to PackageReferences
    Write-Host "[4/6] Converting project references to package references..." -ForegroundColor Yellow

    $csprojFiles = Get-ChildItem -Path $destinationPath -Filter "*.csproj" -Recurse

    $conversionMap = @{
        # Framework references
        '..\..\framework\LablabBean.Core\LablabBean.Core.csproj' = 'LablabBean.Core'
        '..\..\framework\LablabBean.Infrastructure\LablabBean.Infrastructure.csproj' = 'LablabBean.Infrastructure'
        '..\..\framework\LablabBean.Reactive\LablabBean.Reactive.csproj' = 'LablabBean.Reactive'
        '..\..\framework\LablabBean.Game.Core\LablabBean.Game.Core.csproj' = 'LablabBean.Game.Core'
        '..\..\framework\LablabBean.Contracts.Diagnostic\LablabBean.Contracts.Diagnostic.csproj' = 'LablabBean.Contracts.Diagnostic'
        '..\..\framework\LablabBean.Plugins.Core\LablabBean.Plugins.Core.csproj' = 'LablabBean.Plugins.Core'
        '..\..\framework\LablabBean.Reporting.Contracts\LablabBean.Reporting.Contracts.csproj' = 'LablabBean.Reporting.Contracts'
        '..\..\framework\LablabBean.Reporting.Providers.Build\LablabBean.Reporting.Providers.Build.csproj' = 'LablabBean.Reporting.Providers.Build'
        '..\..\framework\LablabBean.Reporting.Analytics\LablabBean.Reporting.Analytics.csproj' = 'LablabBean.Reporting.Analytics'
        '..\..\framework\LablabBean.SourceGenerators.Reporting\LablabBean.SourceGenerators.Reporting.csproj' = 'LablabBean.SourceGenerators.Reporting'
        '..\..\framework\LablabBean.AI.Core\LablabBean.AI.Core.csproj' = 'LablabBean.AI.Core'
        '..\..\framework\LablabBean.AI.Actors\LablabBean.AI.Actors.csproj' = 'LablabBean.AI.Actors'
        '..\..\framework\LablabBean.AI.Agents\LablabBean.AI.Agents.csproj' = 'LablabBean.AI.Agents'
        '..\..\framework\LablabBean.Plugins.Contracts\LablabBean.Plugins.Contracts.csproj' = 'LablabBean.Plugins.Contracts'
        '..\..\framework\LablabBean.Contracts.Scene\LablabBean.Contracts.Scene.csproj' = 'LablabBean.Contracts.Scene'
        '..\..\framework\LablabBean.Contracts.Input\LablabBean.Contracts.Input.csproj' = 'LablabBean.Contracts.Input'
        '..\..\framework\LablabBean.Contracts.Config\LablabBean.Contracts.Config.csproj' = 'LablabBean.Contracts.Config'
        '..\..\framework\LablabBean.Contracts.Resource\LablabBean.Contracts.Resource.csproj' = 'LablabBean.Contracts.Resource'
        '..\..\framework\LablabBean.Contracts.UI\LablabBean.Contracts.UI.csproj' = 'LablabBean.Contracts.UI'
        '..\..\framework\LablabBean.Contracts.Game.UI\LablabBean.Contracts.Game.UI.csproj' = 'LablabBean.Contracts.Game.UI'
        '..\..\framework\LablabBean.Rendering.Contracts\LablabBean.Rendering.Contracts.csproj' = 'LablabBean.Rendering.Contracts'

        # Plugin references
        '..\..\plugins\LablabBean.Plugins.Reporting.Html\LablabBean.Plugins.Reporting.Html.csproj' = 'LablabBean.Plugins.Reporting.Html'
        '..\..\plugins\LablabBean.Plugins.Reporting.Csv\LablabBean.Plugins.Reporting.Csv.csproj' = 'LablabBean.Plugins.Reporting.Csv'
        '..\..\plugins\LablabBean.Plugins.MediaPlayer.Core\LablabBean.Plugins.MediaPlayer.Core.csproj' = 'LablabBean.Plugins.MediaPlayer.Core'
        '..\..\plugins\LablabBean.Plugins.MediaPlayer.FFmpeg\LablabBean.Plugins.MediaPlayer.FFmpeg.csproj' = 'LablabBean.Plugins.MediaPlayer.FFmpeg'
        '..\..\plugins\LablabBean.Plugins.MediaPlayer.Terminal.Braille\LablabBean.Plugins.MediaPlayer.Terminal.Braille.csproj' = 'LablabBean.Plugins.MediaPlayer.Terminal.Braille'
        '..\\..\\plugins\\LablabBean.Plugins.Rendering.Terminal\\LablabBean.Plugins.Rendering.Terminal.csproj' = 'LablabBean.Plugins.Rendering.Terminal'
        '..\\..\\plugins\\LablabBean.Plugins.UI.Terminal\\LablabBean.Plugins.UI.Terminal.csproj' = 'LablabBean.Plugins.UI.Terminal'
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

    # Step 5: Create solution file
    Write-Host "[5/6] Creating solution file..." -ForegroundColor Yellow

    $solutionPath = Join-Path (Join-Path $DestinationRepo "dotnet") "LablabBean.Console.sln"

    if (-not $WhatIf) {
        Push-Location (Join-Path $DestinationRepo "dotnet")

        # Create new solution
        dotnet new sln -n "LablabBean.Console" -o . --force | Out-Null

        # Add all projects
        foreach ($csproj in $csprojFiles) {
            dotnet sln $solutionPath add $csproj.FullName --solution-folder "console-app" | Out-Null
            Write-Host "  Added: $($csproj.Name)" -ForegroundColor Gray
        }

        Pop-Location
        Write-Host "  ✓ Solution created: LablabBean.Console.sln" -ForegroundColor Green
    } else {
        Write-Host "  Would create: $solutionPath" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 6: Copy Directory.Packages.props if it exists
    Write-Host "[6/6] Copying Directory.Packages.props..." -ForegroundColor Yellow

    $sourcePackagesProps = Join-Path $SourcePath "Directory.Packages.props"
    $destPackagesProps = Join-Path $destinationPath "Directory.Packages.props"

    if (Test-Path $sourcePackagesProps) {
        if (-not $WhatIf) {
            Copy-Item -Path $sourcePackagesProps -Destination $destPackagesProps -Force
            Write-Host "  ✓ Directory.Packages.props copied" -ForegroundColor Green
        } else {
            Write-Host "  Would copy Directory.Packages.props" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  No Directory.Packages.props found (skipping)" -ForegroundColor DarkGray
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  ✓ Console app migration completed!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""

    if (-not $WhatIf) {
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Run pack-framework.ps1 to create NuGet packages" -ForegroundColor Gray
        Write-Host "  2. cd $DestinationRepo\dotnet" -ForegroundColor Gray
        Write-Host "  3. dotnet restore" -ForegroundColor Gray
        Write-Host "  4. dotnet build" -ForegroundColor Gray
        Write-Host "  5. Test the console app" -ForegroundColor Gray
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
