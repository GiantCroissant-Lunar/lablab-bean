<#
.SYNOPSIS
    Updates all LablabBean.* package references in a project or solution to a specific version.

.DESCRIPTION
    This script scans .csproj files and updates PackageReference elements for all LablabBean.* packages
    to a specified version. Useful when updating apps to consume new framework versions.

.PARAMETER Path
    Path to project file (.csproj) or directory containing projects. Default: current directory

.PARAMETER Version
    Target version for LablabBean packages (e.g., "0.0.3" or "0.0.4-alpha")

.PARAMETER WhatIf
    Show what would be changed without making actual changes

.EXAMPLE
    .\update-package-versions.ps1 -Version "0.0.4"

.EXAMPLE
    .\update-package-versions.ps1 -Path .\console-app -Version "0.0.4-beta" -WhatIf
#>

param(
    [Parameter()]
    [string]$Path = ".",

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter()]
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  LablabBean Package Version Updater" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Target Version: $Version" -ForegroundColor Green
Write-Host "Search Path: $Path" -ForegroundColor Green
if ($WhatIf) {
    Write-Host "Mode: DRY RUN (no changes will be made)" -ForegroundColor Yellow
}
Write-Host ""

# Find all .csproj files
$projects = Get-ChildItem -Path $Path -Filter "*.csproj" -Recurse

Write-Host "Found $($projects.Count) project(s)" -ForegroundColor Cyan
Write-Host ""

$updatedCount = 0
$projectsModified = 0

foreach ($project in $projects) {
    $projectName = $project.BaseName
    $content = Get-Content $project.FullName -Raw
    $originalContent = $content

    # Find all LablabBean.* PackageReference elements
    $pattern = '<PackageReference\s+Include="(LablabBean\.[^"]+)"\s+Version="([^"]+)"\s*/>'
    $packageMatches = [regex]::Matches($content, $pattern)

    if ($packageMatches.Count -eq 0) {
        Write-Host "  ○ $projectName (no LablabBean packages)" -ForegroundColor DarkGray
        continue
    }

    Write-Host "  • $projectName" -ForegroundColor White

    foreach ($match in $packageMatches) {
        $packageName = $match.Groups[1].Value
        $currentVersion = $match.Groups[2].Value

        if ($currentVersion -eq $Version) {
            Write-Host "    - $packageName : $currentVersion (already up to date)" -ForegroundColor DarkGray
        } else {
            Write-Host "    - $packageName : $currentVersion → $Version" -ForegroundColor Yellow

            # Replace version
            $oldTag = $match.Value
            $newTag = $oldTag -replace 'Version="[^"]+"', "Version=`"$Version`""
            $content = $content.Replace($oldTag, $newTag)

            $updatedCount++
        }
    }

    # Save if changes were made
    if ($content -ne $originalContent) {
        $projectsModified++

        if (-not $WhatIf) {
            Set-Content -Path $project.FullName -Value $content -NoNewline
            Write-Host "    ✓ Updated" -ForegroundColor Green
        } else {
            Write-Host "    ⊘ Would update (dry run)" -ForegroundColor Yellow
        }
    }

    Write-Host ""
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Projects scanned: $($projects.Count)" -ForegroundColor White
Write-Host "  Projects modified: $projectsModified" -ForegroundColor $(if ($projectsModified -gt 0) { 'Green' } else { 'Gray' })
Write-Host "  Packages updated: $updatedCount" -ForegroundColor $(if ($updatedCount -gt 0) { 'Green' } else { 'Gray' })

if ($WhatIf -and $updatedCount -gt 0) {
    Write-Host ""
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
