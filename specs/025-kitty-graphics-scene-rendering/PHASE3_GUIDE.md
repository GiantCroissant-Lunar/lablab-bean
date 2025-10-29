# Phase 3: Environment Testing Guide

**Prerequisites**: Phase 1 Complete ✅, Phase 2 In Progress 🔄
**Duration**: 4-6 hours
**Difficulty**: Medium-High

## Overview

Phase 3 validates the Kitty graphics implementation across different network and platform environments, testing SSH sessions and cross-platform tileset loading.

## Tasks in Phase 3

| Task | Description | Requirements | Priority |
|------|-------------|--------------|----------|
| T078 | Test via SSH | SSH server + client | Medium |
| T095 | Verify tileset on both platforms | Console + Windows apps | High |

---

## Part 1: T078 - SSH Session Testing

### Goal
Verify that the console app detects SSH sessions and uses appropriate rendering mode (likely glyph fallback, not Kitty graphics).

### Background
The implementation includes SSH detection via environment variables:
- `SSH_CONNECTION`
- `SSH_CLIENT`
- `SSH_TTY`

When detected, the app should log a warning and potentially disable Kitty graphics.

### Setup Options

#### Option A: WSL SSH (Recommended for Windows)

**Step 1: Enable WSL SSH Server**

```powershell
# Install WSL if not already installed
wsl --install

# Inside WSL (Ubuntu)
sudo apt update
sudo apt install openssh-server
sudo service ssh start

# Check SSH is running
sudo service ssh status
```

**Step 2: Get WSL IP Address**

```bash
# Inside WSL
ip addr show eth0 | grep inet
# Note the IP address (e.g., 172.x.x.x)
```

**Step 3: Configure SSH Access**

```bash
# Inside WSL - edit sshd_config if needed
sudo nano /etc/ssh/sshd_config

# Ensure these settings:
# PasswordAuthentication yes
# PermitRootLogin no

# Restart SSH
sudo service ssh restart

# Set a password if needed
passwd
```

#### Option B: Windows OpenSSH Server

```powershell
# Enable OpenSSH Server (Windows 10/11)
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

# Start the service
Start-Service sshd

# Set to start automatically
Set-Service -Name sshd -StartupType 'Automatic'

# Check status
Get-Service sshd
```

#### Option C: Remote Linux Server

If you have access to a remote Linux server, use that directly.

### Test Execution

#### Test 1: SSH from PowerShell to WSL

```powershell
# From Windows PowerShell, SSH into WSL
ssh username@172.x.x.x  # Use your WSL IP

# Inside SSH session, verify environment variables
echo $SSH_CONNECTION
echo $SSH_CLIENT
echo $SSH_TTY

# Navigate to the project (if accessible)
cd /mnt/d/lunar-snake/personal-work/yokan-projects/lablab-bean

# Run the console app
./dotnet/console-app/LablabBean.Console/bin/Release/net8.0/LablabBean.Console
```

#### Test 2: Verify Remote Session Detection

**Expected Log Output**:
```
[INFO] Detecting terminal capabilities...
[INFO] SSH session detected: SSH_CONNECTION=172.x.x.x
[WARN] Kitty graphics may not work over SSH, will verify on first render
[INFO] Using glyph mode for remote session
```

#### Test 3: Verify Rendering

- **Expected**: Glyph mode (ASCII characters)
- **No Kitty escape sequences** should be sent
- **No errors** related to graphics protocol

### Success Criteria

- [ ] SSH session detected correctly
- [ ] Log shows "SSH session detected"
- [ ] App falls back to glyph mode
- [ ] No Kitty graphics escape sequences sent
- [ ] UI renders properly over SSH
- [ ] No crashes or errors

### Test Report Template

```markdown
# T078 Test Results - SSH Session Detection

**Date**: YYYY-MM-DD
**Environment**: Windows -> WSL/Linux via SSH

## Setup
- SSH Server: [WSL/Linux/Windows OpenSSH]
- SSH Client: PowerShell/Terminal
- Connection: localhost / remote

## Test Execution

### SSH Connection
- [x] SSH server started successfully
- [x] Connected via SSH
- [x] Environment variables present:
  - SSH_CONNECTION: [value]
  - SSH_CLIENT: [value]
  - SSH_TTY: [value]

### Console App Launch
- [x] App started in SSH session
- [x] No connection errors
- [x] UI displayed

### Detection Results
- [x] SSH session detected: YES/NO
- [x] Log message present: YES/NO
- [x] Rendering mode: Glyph / Kitty / Unknown

### Visual Quality
- [x] Text readable
- [x] Colors correct
- [x] Layout proper
- [x] No artifacts

## Log Excerpt
\`\`\`
[Paste relevant logs]
\`\`\`

## Screenshots
- SSH session terminal: [filename]
- Console app in SSH: [filename]

## Issues
[List any issues]

## Conclusion
**Result**: PASS / FAIL
**Reason**: [Brief explanation]
```

---

## Part 2: T095 - Cross-Platform Tileset Loading

### Goal
Verify that both Console (Terminal.Gui) and Windows (SadConsole) apps load the same tileset PNG and render consistently.

### Prerequisites

1. **Tileset File**: `assets/tiles.png` must exist
2. **Both Apps Built**:
   - Console: `dotnet/console-app/LablabBean.Console/bin/Release/net8.0/`
   - Windows: `dotnet/windows-app/LablabBean.Windows/bin/Release/net8.0/`

### Creating the Tileset

If you don't have a tileset yet, here are options:

#### Option 1: Use Existing Roguelike Tilesets

Free tilesets available:
- **Kenney Roguelike Pack**: https://kenney.nl/assets/roguelike-characters-pack
- **Dwarf Fortress Tilesets**: Various 16x16 tilesets
- **OrxRoguelike**: https://opengameart.org/

Download and save as `assets/tiles.png`

#### Option 2: Create Minimal Tileset with GIMP/Paint.NET

**Steps**:
1. Create 64x64 pixel canvas (for 4x4 grid of 16x16 tiles)
2. Fill with transparent background
3. Draw 16 tiles:
   - Tile 0 (0,0): Floor - gray square
   - Tile 1 (16,0): Wall - dark gray with border
   - Tile 2 (32,0): Door - brown
   - Tile 10 (32,16): Player - yellow @ or character
   - Tile 11 (48,16): Enemy - red E or monster
4. Save as PNG: `assets/tiles.png`

#### Option 3: Use Placeholder Script

```powershell
# Create a simple solid-color tileset for testing
# This creates a basic PNG programmatically
# (Requires ImageMagick or similar tool)

magick -size 64x64 xc:transparent -fill gray -draw "rectangle 0,0 15,15" -fill darkgray -draw "rectangle 16,0 31,15" assets/tiles.png
```

### Test Execution

#### Step 1: Build Both Applications

```powershell
# Console app
cd dotnet
dotnet build console-app\LablabBean.Console\LablabBean.Console.csproj -c Release

# Windows app
dotnet build windows-app\LablabBean.Windows\LablabBean.Windows.csproj -c Release
```

#### Step 2: Verify Tileset Exists

```powershell
if (Test-Path "assets\tiles.png") {
    $file = Get-Item "assets\tiles.png"
    Write-Host "✅ Tileset found: $($file.Length) bytes"
    
    # Check if it's a valid PNG
    $header = [System.IO.File]::ReadAllBytes($file.FullName) | Select-Object -First 8
    if ($header[0] -eq 0x89 -and $header[1] -eq 0x50) {
        Write-Host "✅ Valid PNG file"
    }
} else {
    Write-Host "❌ Tileset not found!"
}
```

#### Step 3: Launch Console App

```powershell
# Launch in regular terminal (not WezTerm for this test)
.\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe
```

**Look for in logs**:
```
[INFO] Loading tileset: ./assets/tiles.png
[INFO] Tileset loaded successfully: 64x64 pixels, 16 tiles
[INFO] Renderer supports image mode: true/false
```

#### Step 4: Launch Windows App

```powershell
.\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe
```

**Look for in logs**:
```
[INFO] Loading tileset: ./assets/tiles.png
[INFO] Tileset loaded successfully
[INFO] SadConsole initialized with tileset
```

#### Step 5: Visual Comparison

**Take screenshots of both**:
- Console app with tileset
- Windows app with same tileset

**Compare**:
- [ ] Same tiles visible
- [ ] Same layout
- [ ] Same colors
- [ ] Same dimensions
- [ ] Consistent visual quality

### Success Criteria

- [ ] Tileset file exists and is valid PNG
- [ ] Console app loads tileset without errors
- [ ] Windows app loads tileset without errors
- [ ] Both apps show "Tileset loaded successfully"
- [ ] Visual rendering is consistent
- [ ] Same tile IDs map to same graphics
- [ ] No rendering artifacts on either platform

### Troubleshooting

**Issue**: "Tileset not found"
- Check file path is relative to executable
- Verify `assets/tiles.png` exists
- Check appsettings.json configuration

**Issue**: "Invalid PNG format"
- Use image viewer to verify PNG
- Check file isn't corrupted
- Ensure it's not a different format renamed to .png

**Issue**: "Tileset size mismatch"
- Verify grid size matches tile count
- Check TileSize configuration (should be 16)
- Ensure PNG is correct dimensions

### Test Report Template

```markdown
# T095 Test Results - Cross-Platform Tileset Loading

**Date**: YYYY-MM-DD

## Tileset Information
- **File**: assets/tiles.png
- **Size**: [file size] bytes
- **Dimensions**: [width]x[height] pixels
- **Tile Count**: [expected count]
- **Tile Size**: 16x16 pixels

## Console App (Terminal.Gui)
- [x] App launched successfully
- [x] Tileset load attempted
- [x] Load status: SUCCESS / FAIL
- [x] Rendering mode: Image / Glyph
- [x] Visual quality: GOOD / ACCEPTABLE / POOR

### Log Excerpt
\`\`\`
[Console app logs]
\`\`\`

## Windows App (SadConsole)
- [x] App launched successfully
- [x] Tileset load attempted
- [x] Load status: SUCCESS / FAIL
- [x] Visual quality: GOOD / ACCEPTABLE / POOR

### Log Excerpt
\`\`\`
[Windows app logs]
\`\`\`

## Visual Comparison
- [x] Same tiles render on both platforms
- [x] Colors match
- [x] Layout consistent
- [x] No artifacts

### Screenshots
- Console: [filename]
- Windows: [filename]
- Side-by-side: [filename]

## Issues
[List any discrepancies]

## Conclusion
**Result**: PASS / FAIL
**Consistency**: EXCELLENT / GOOD / POOR
**Reason**: [Brief explanation]
```

---

## Phase 3 Test Script

Create `scripts/test-spec025-phase3.ps1`:

```powershell
# Automated Phase 3 test runner
# T078 - SSH detection (manual)
# T095 - Tileset loading (automated)

$ErrorActionPreference = "Continue"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SPEC-025 Phase 3: Environment Testing" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

# T095 - Tileset verification
Write-Host "`n[T095] Cross-Platform Tileset Loading" -ForegroundColor Yellow

$tileset = "assets\tiles.png"
$results = @{
    TilesetExists = $false
    TilesetValid = $false
    ConsoleBuild = $false
    WindowsBuild = $false
    ConsoleRun = "NOT_TESTED"
    WindowsRun = "NOT_TESTED"
}

# Check tileset
if (Test-Path $tileset) {
    $results.TilesetExists = $true
    Write-Host "  ✅ Tileset found: $tileset" -ForegroundColor Green
    
    $file = Get-Item $tileset
    Write-Host "     Size: $($file.Length) bytes" -ForegroundColor Cyan
    
    # Validate PNG header
    $header = [System.IO.File]::ReadAllBytes($file.FullName) | Select-Object -First 8
    if ($header[0] -eq 0x89 -and $header[1] -eq 0x50) {
        $results.TilesetValid = $true
        Write-Host "  ✅ Valid PNG file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Invalid PNG file" -ForegroundColor Red
    }
} else {
    Write-Host "  ❌ Tileset not found: $tileset" -ForegroundColor Red
    Write-Host "     Create a tileset to complete T095" -ForegroundColor Yellow
}

# Build verification
Write-Host "`n[Build Check]" -ForegroundColor Yellow

$consolePath = "dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe"
$windowsPath = "dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe"

if (Test-Path $consolePath) {
    $results.ConsoleBuild = $true
    Write-Host "  ✅ Console app built" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Console app not built (Release mode)" -ForegroundColor Yellow
}

if (Test-Path $windowsPath) {
    $results.WindowsBuild = $true
    Write-Host "  ✅ Windows app built" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Windows app not built (Release mode)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Phase 3 Readiness" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host "`nT078 - SSH Testing:" -ForegroundColor Yellow
Write-Host "  Status: Manual testing required" -ForegroundColor White
Write-Host "  Guide: specs\025-kitty-graphics-scene-rendering\PHASE3_GUIDE.md" -ForegroundColor Cyan

Write-Host "`nT095 - Tileset Loading:" -ForegroundColor Yellow
if ($results.TilesetExists -and $results.TilesetValid -and $results.ConsoleBuild -and $results.WindowsBuild) {
    Write-Host "  Status: ✅ Ready to test" -ForegroundColor Green
    Write-Host "  Next: Launch both apps and compare rendering" -ForegroundColor Cyan
} else {
    Write-Host "  Status: ⚠️  Missing prerequisites" -ForegroundColor Yellow
    if (-not $results.TilesetExists) { Write-Host "    - Create tileset: assets\tiles.png" -ForegroundColor White }
    if (-not $results.ConsoleBuild) { Write-Host "    - Build console app (Release)" -ForegroundColor White }
    if (-not $results.WindowsBuild) { Write-Host "    - Build Windows app (Release)" -ForegroundColor White }
}

Write-Host ""
```

---

## Phase 3 Timeline

### Immediate (If Prerequisites Ready)
- **T095**: 30-60 minutes (if tileset exists)
  - Build both apps
  - Launch and verify
  - Screenshot comparison

### Short Term (This Week)
- **T078**: 2-3 hours (SSH setup + testing)
  - Configure SSH server
  - Test SSH detection
  - Verify rendering over SSH

### Estimated Total: 3-4 hours

---

## Success Metrics

**Phase 3 Complete When**:
- [x] T078: SSH session detection verified
- [x] T095: Both apps load same tileset successfully
- [x] Test reports completed
- [x] Screenshots captured

---

## Resources

- **OpenSSH Documentation**: https://learn.microsoft.com/en-us/windows-server/administration/openssh/
- **WSL Guide**: https://learn.microsoft.com/en-us/windows/wsl/
- **Tileset Resources**: https://opengameart.org/ (free assets)
- **GIMP Tutorial**: https://www.gimp.org/tutorials/

---

**Guide Version**: 1.0
**Last Updated**: 2025-10-28
**Prerequisites**: Phase 1 Complete, Phase 2 In Progress
