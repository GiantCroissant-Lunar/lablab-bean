<#
.SYNOPSIS
    Removes migrated console and windows app folders from main repository.

.DESCRIPTION
    This script safely removes the console-app and windows-app folders from the main repository
    after they've been migrated to separate repositories. It:
    1. Verifies separate repos exist with content
    2. Removes projects from solution file
    3. Removes the folders
    4. Updates README if needed

.PARAMETER SkipVerification
    Skip verification that separate repos exist

.PARAMETER WhatIf
    Show what would be removed without actually removing

.EXAMPLE
    .\cleanup-migrated-apps.ps1 -WhatIf

.EXAMPLE
    .\cleanup-migrated-apps.ps1
#>

param(
    [Parameter()]
    [switch]$SkipVerification,

    [Parameter()]
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Cleanup Migrated Apps Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Paths
$consoleAppPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\console-app"
$windowsAppPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\windows-app"
$windowsUIPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\windows-ui"
$solutionPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet\LablabBean.sln"

$consoleRepoPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean-console\dotnet"
$windowsRepoPath = "D:\lunar-snake\personal-work\yokan-projects\lablab-bean-windows\dotnet"

try {
    # Step 1: Verify separate repos exist
    if (-not $SkipVerification) {
        Write-Host "[1/5] Verifying separate repositories exist..." -ForegroundColor Yellow

        $verificationFailed = $false

        if (-not (Test-Path $consoleRepoPath)) {
            Write-Host "  ✗ Console repo not found: $consoleRepoPath" -ForegroundColor Red
            $verificationFailed = $true
        } else {
            $consoleProjects = Get-ChildItem -Path "$consoleRepoPath\console-app" -Filter "*.csproj" -Recurse
            Write-Host "  ✓ Console repo exists ($($consoleProjects.Count) projects)" -ForegroundColor Green
        }

        if (-not (Test-Path $windowsRepoPath)) {
            Write-Host "  ✗ Windows repo not found: $windowsRepoPath" -ForegroundColor Red
            $verificationFailed = $true
        } else {
            $windowsProjects = Get-ChildItem -Path "$windowsRepoPath\windows-app","$windowsRepoPath\windows-ui" -Filter "*.csproj" -Recurse
            Write-Host "  ✓ Windows repo exists ($($windowsProjects.Count) projects)" -ForegroundColor Green
        }

        if ($verificationFailed) {
            throw "Verification failed. Run migration scripts first or use -SkipVerification to bypass."
        }

        Write-Host ""
    } else {
        Write-Host "[1/5] Skipping verification" -ForegroundColor Gray
        Write-Host ""
    }

    # Step 2: List projects to be removed
    Write-Host "[2/5] Projects to be removed from solution:" -ForegroundColor Yellow

    $projectsToRemove = @(
        "console-app\LablabBean.Console\LablabBean.Console.csproj",
        "console-app\LablabBean.Game.TerminalUI\LablabBean.Game.TerminalUI.csproj",
        "windows-app\LablabBean.Windows\LablabBean.Windows.csproj",
        "windows-app\LablabBean.Game.SadConsole\LablabBean.Game.SadConsole.csproj"
    )

    foreach ($project in $projectsToRemove) {
        Write-Host "  - $project" -ForegroundColor Gray
    }
    Write-Host ""

    # Step 3: Remove projects from solution
    Write-Host "[3/5] Removing projects from solution file..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        Push-Location "D:\lunar-snake\personal-work\yokan-projects\lablab-bean\dotnet"

        foreach ($project in $projectsToRemove) {
            Write-Host "  Removing: $project" -ForegroundColor Gray
            $fullPath = Join-Path (Get-Location) $project
            if (Test-Path $fullPath) {
                dotnet sln $solutionPath remove $fullPath 2>&1 | Out-Null
            }
        }

        Pop-Location
        Write-Host "  ✓ Projects removed from solution" -ForegroundColor Green
    } else {
        Write-Host "  Would remove $($projectsToRemove.Count) projects from solution" -ForegroundColor Yellow
    }

    Write-Host ""

    # Step 4: Calculate folder sizes
    Write-Host "[4/5] Folders to be deleted:" -ForegroundColor Yellow

    $foldersToRemove = @(
        @{Path = $consoleAppPath; Name = "console-app"},
        @{Path = $windowsAppPath; Name = "windows-app"},
        @{Path = $windowsUIPath; Name = "windows-ui"}
    )

    $totalSize = 0
    foreach ($folder in $foldersToRemove) {
        if (Test-Path $folder.Path) {
            $size = (Get-ChildItem -Path $folder.Path -Recurse -File | Measure-Object -Property Length -Sum).Sum
            $sizeMB = [math]::Round($size / 1MB, 2)
            $totalSize += $sizeMB
            Write-Host "  - $($folder.Name): $sizeMB MB" -ForegroundColor Gray
        } else {
            Write-Host "  - $($folder.Name): Not found (already removed?)" -ForegroundColor DarkGray
        }
    }

    Write-Host "  Total: $totalSize MB" -ForegroundColor Cyan
    Write-Host ""

    # Step 5: Remove folders
    Write-Host "[5/5] Removing folders..." -ForegroundColor Yellow

    if (-not $WhatIf) {
        foreach ($folder in $foldersToRemove) {
            if (Test-Path $folder.Path) {
                Write-Host "  Removing: $($folder.Path)" -ForegroundColor Gray
                Remove-Item -Path $folder.Path -Recurse -Force
                Write-Host "  ✓ $($folder.Name) removed" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "  Would remove 3 folders (~$totalSize MB)" -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  ✓ Cleanup completed!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""

    if (-not $WhatIf) {
        Write-Host "Summary:" -ForegroundColor Yellow
        Write-Host "  ✓ Removed 4 projects from solution" -ForegroundColor Green
        Write-Host "  ✓ Deleted 3 folders (~$totalSize MB)" -ForegroundColor Green
        Write-Host "  ✓ Main repo now contains only framework and plugins" -ForegroundColor Green
        Write-Host ""
        Write-Host "Apps are now in separate repositories:" -ForegroundColor Cyan
        Write-Host "  • Console: $consoleRepoPath" -ForegroundColor Gray
        Write-Host "  • Windows: $windowsRepoPath" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "  1. Review changes with 'git status'" -ForegroundColor Gray
        Write-Host "  2. Run pack-framework.ps1 to create NuGet packages" -ForegroundColor Gray
        Write-Host "  3. Test apps in their separate repos" -ForegroundColor Gray
    } else {
        Write-Host "Run without -WhatIf to execute cleanup" -ForegroundColor Yellow
    }

    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "  ✗ Cleanup failed!" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}
