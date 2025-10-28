$ErrorActionPreference = 'Stop'

$slnPath = Join-Path 'dotnet' 'LablabBean.sln'
if (-not (Test-Path $slnPath)) { throw "Solution not found: $slnPath" }

$backup = "$slnPath.bak"
Copy-Item $slnPath $backup -Force
Write-Host "[fix-sln] Backed up to: $backup"

$content = Get-Content -Raw -Path $slnPath

# 1) Remove stale tool project blocks
$patterns = @(
    '(?ms)^\s*Project\(".*?"\)\s=\s"LablabBean\.NukeRunner".*?^EndProject\s*\r?\n',
    '(?ms)^\s*Project\(".*?"\)\s=\s"LablabBean\.WezTermLauncher".*?^EndProject\s*\r?\n'
)
foreach ($p in $patterns) { $content = $content -replace $p, '' }

# 2) Remove ProjectConfigurationPlatforms + NestedProjects entries for removed GUIDs
$guids = @('9C2D4E5F-77A0-4C2A-9E8A-2B2B1B1B1B1B','A0A1A2A3-B4B5-46C7-8888-1234567890AB')
foreach ($g in $guids) {
    $gEsc = [Regex]::Escape($g)
    # Config lines like: {GUID}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
    # NestedProjects lines like: {GUID} = {PARENTGUID}
    $content = $content -replace ("(?m)^\s*\{$gEsc\}\s=.*\r?\n"), ''
}

# 3) Remove any NestedProjects block(s) entirely to avoid invalid references
$regexNestedAny = [regex]::new('(?ms)^\s*GlobalSection\(NestedProjects\)\s=\spreSolution.*?^\s*EndGlobalSection\s*', [System.Text.RegularExpressions.RegexOptions]::Multiline -bor [System.Text.RegularExpressions.RegexOptions]::Singleline)
if ($regexNestedAny.IsMatch($content)) {
    $content = $regexNestedAny.Replace($content, '')
    Write-Host "[fix-sln] Removed all NestedProjects blocks."
}

# 4) Deduplicate solution folder projects named 'tests'
$sfGuidLiteral = '{2150E333-8FDC-42A3-9474-1A3956D46DE8}'
$patternText = '(?ms)^\s*Project\("' + [Regex]::Escape($sfGuidLiteral) + '"\)\s=\s"(?<name>[^"]+)",\s"(?<path>[^"]*)",\s"(?<guid>\{[0-9A-Fa-f\-]+\})"\s*\r?\n.*?^\s*EndProject\s*\r?\n'
$projPattern = [regex]::new($patternText)
$matches = $projPattern.Matches($content)
$seen = @{}
foreach ($m in $matches) {
    $name = $m.Groups['name'].Value
    if ($name -ieq 'tests') {
        if ($seen.ContainsKey($name)) {
            $guid = $m.Groups['guid'].Value
            # Remove this entire Project block
            $block = $m.Value
            $content = $content.Replace($block, '')
            # Also remove any config lines or mapping lines for this guid just in case
            $gEsc = [Regex]::Escape(($guid.Trim('{}')))
            $content = $content -replace ("(?m)^\s*\{$gEsc\}\..*\r?\n"), ''
            $content = $content -replace ("(?m)^\s*\{$gEsc\}\s=.*\r?\n"), ''
            Write-Host "[fix-sln] Removed duplicate solution folder 'tests' with GUID $guid"
        } else {
            $seen[$name] = $true
        }
    }
}

Set-Content -Path $slnPath -Value $content -Encoding UTF8
Write-Host "[fix-sln] Solution sanitized: $slnPath"
