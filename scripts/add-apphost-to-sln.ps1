$ErrorActionPreference = 'Stop'

$slnPath = Join-Path 'dotnet' 'LablabBean.sln'
if (-not (Test-Path $slnPath)) { throw "Solution not found: $slnPath" }

$content = Get-Content -Path $slnPath

# Check if AppHost project entry exists
$hasAppHost = $content -match 'LablabBean\.AppHost\\LablabBean\.AppHost\.csproj'
if ($hasAppHost) {
  Write-Host '[add-apphost] AppHost project already present.'
  exit 0
}

$projectBlock = @(
  'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "LablabBean.AppHost", "apphost\LablabBean.AppHost\LablabBean.AppHost.csproj", "{5D6F6B1C-6F3B-4BBF-9E62-5D1E9A2B9A11}"',
  'EndProject'
)

# Insert the block right before the first 'Global' line
$globalIndex = ($content | Select-String -Pattern '^\s*Global\s*$' -SimpleMatch:$false).LineNumber | Select-Object -First 1
if (-not $globalIndex) { throw 'Could not locate Global section in solution file.' }

# Build new content
$before = $content[0..($globalIndex-2)]
$after = $content[($globalIndex-1)..($content.Length-1)]
$newContent = @()
$newContent += $before
$newContent += $projectBlock
$newContent += $after

Set-Content -Path $slnPath -Value $newContent -Encoding UTF8
Write-Host '[add-apphost] Inserted AppHost project into solution.'
