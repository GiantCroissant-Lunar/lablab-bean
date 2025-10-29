# Phase 2: Terminal Emulator Setup Guide

**Prerequisites**: Phase 1 Complete ✅
**Duration**: 3-4 hours
**Difficulty**: Medium

## Overview

Phase 2 validates Kitty graphics protocol rendering in actual terminal emulators. This requires installing and configuring terminal emulators that support the Kitty graphics protocol.

## Tasks in Phase 2

| Task | Description | Terminal Required |
|------|-------------|-------------------|
| T076 | Test in WezTerm: verify Kitty rendering | WezTerm |
| T077 | Test in xterm: verify glyph fallback | xterm (Linux/WSL) |
| T085 | Screenshot console rendering | WezTerm/Kitty |
| T086 | Screenshot SadConsole | Windows app |
| T093 | Verify pixel graphics rendering | WezTerm/Kitty |

## Step 1: Install WezTerm (Windows)

### Option A: WinGet (Recommended)

```powershell
winget install wez.wezterm
```

### Option B: Chocolatey

```powershell
choco install wezterm
```

### Option C: Direct Download

1. Visit: https://wezfurlong.org/wezterm/installation.html
2. Download Windows installer
3. Run installer
4. Restart terminal

### Verify Installation

```powershell
wezterm --version
```

Expected output: `wezterm 20240203-110809-5046fc22`

## Step 2: Configure WezTerm for Kitty Graphics

Create or edit `~/.wezterm.lua` (or `%USERPROFILE%\.wezterm.lua` on Windows):

```lua
-- WezTerm configuration for Kitty Graphics Protocol
local wezterm = require 'wezterm'
local config = {}

-- Enable Kitty graphics protocol
config.enable_kitty_graphics = true

-- Font configuration (optional but recommended)
config.font = wezterm.font('JetBrains Mono', { weight = 'Regular' })
config.font_size = 11.0

-- Color scheme (optional)
config.color_scheme = 'Tokyo Night'

-- Performance
config.max_fps = 60
config.animation_fps = 60

-- Window configuration
config.window_background_opacity = 1.0
config.window_padding = {
  left = 2,
  right = 2,
  top = 2,
  bottom = 2,
}

return config
```

### Verify Kitty Support

```powershell
# In WezTerm terminal:
echo $env:TERM_PROGRAM
# Should output: WezTerm

# Check for Kitty support in logs when running app
```

## Step 3: Prepare Test Environment

### Build Console Application

```powershell
cd dotnet
dotnet build console-app\LablabBean.Console\LablabBean.Console.csproj -c Release
```

### Create Sample Tileset (Optional)

For full image mode testing, create a simple tileset:

**Option 1**: Use placeholder (will test fallback)
```powershell
# Leave tileset missing - app will fallback gracefully
```

**Option 2**: Create minimal tileset
```powershell
# Create 16x16 pixel tiles using any image editor
# Save as: assets/tiles.png
# Format: PNG with transparent background
# Layout: Grid of tiles (e.g., 16 tiles = 4x4 grid = 64x64 pixels)
```

**Recommended Tiles for MVP**:
- Tile 0: Floor (gray square)
- Tile 1: Wall (dark square with border)
- Tile 10: Player (@ symbol or character sprite)
- Tile 11: Enemy (E symbol or monster sprite)

## Step 4: Execute T076 - Test in WezTerm

### Launch Console App in WezTerm

```powershell
# Open WezTerm
wezterm

# Navigate to project root
cd D:\lunar-snake\personal-work\yokan-projects\lablab-bean

# Run console app
.\dotnet\console-app\LablabBean.Console\bin\Release\net8.0\LablabBean.Console.exe
```

### Verify Kitty Rendering

**Expected Behavior**:
1. ✅ Console window opens in WezTerm
2. ✅ Log message: "Terminal capabilities: Kitty=True, Sixel=True, TrueColor=True"
3. ✅ If tileset present: Renders tiles as pixel graphics
4. ✅ If tileset missing: Falls back to glyph mode (ASCII characters)
5. ✅ No errors or crashes

**Look for in logs** (`dotnet/console-app/LablabBean.Console/bin/Release/net8.0/logs/latest.log`):
```
[INFO] Renderer supports image mode: true
[INFO] Kitty graphics protocol enabled
[INFO] Loading tileset: ./assets/tiles.png
[INFO] Using Kitty graphics protocol for high-quality rendering
```

OR (if tileset missing):
```
[WARN] Tileset not found: ./assets/tiles.png
[INFO] Falling back to glyph mode
```

### Troubleshooting

**Issue**: "Kitty graphics not detected"
- Check WezTerm configuration: `enable_kitty_graphics = true`
- Restart WezTerm after config change
- Verify `TERM_PROGRAM` environment variable

**Issue**: "Escape sequences visible"
- Kitty protocol may not be fully enabled
- Try updating WezTerm to latest version
- Check terminal capabilities in logs

**Issue**: "Garbled output"
- Verify RGBA byte order (should be RGBA32)
- Check base64 encoding is correct
- Review Kitty escape sequence format

## Step 5: Execute T085 - Screenshot Console Rendering

### Take Screenshots

**In WezTerm**:
1. Launch console app
2. Wait for rendering to complete
3. Use Windows Snipping Tool or WezTerm's built-in screenshot feature
4. Save as: `specs/025-.../test-reports/screenshots/wezterm-kitty-mode.png`

**Capture Types**:
- Full window (includes WezTerm frame)
- Terminal content only (crop to console area)
- Close-up of tile rendering

### Screenshot Checklist

- [ ] Window title visible (confirms app name)
- [ ] Terminal size visible (e.g., 80x24)
- [ ] Tiles clearly visible (if image mode)
- [ ] No rendering artifacts
- [ ] Proper colors and transparency
- [ ] Legend/HUD visible (if applicable)

## Step 6: Execute T093 - Verify Pixel Graphics

### Visual Inspection

**Image Mode (Kitty)**:
- ✅ Tiles appear as crisp pixel graphics
- ✅ Edges are clean (no anti-aliasing artifacts)
- ✅ Colors match tileset design
- ✅ Transparency works (background visible)
- ✅ No flickering or tearing

**Glyph Mode (Fallback)**:
- ✅ ASCII characters rendered
- ✅ @ for player, # for wall, . for floor
- ✅ Colors applied correctly
- ✅ Layout matches expected scene

### Comparison

Compare WezTerm rendering with expected output:
1. Check tile positions match game state
2. Verify player position correct
3. Confirm visible tiles render (FOV)
4. Validate colors and styling

## Step 7: Execute T086 - Screenshot SadConsole (Windows)

### Build Windows Application

```powershell
cd dotnet
dotnet build windows-app\LablabBean.Windows\LablabBean.Windows.csproj -c Release
```

### Launch Windows App

```powershell
.\dotnet\windows-app\LablabBean.Windows\bin\Release\net8.0\LablabBean.Windows.exe
```

### Take Screenshots

Save as: `specs/025-.../test-reports/screenshots/sadconsole-windows.png`

## Step 8: Execute T077 - Test in xterm (Optional)

**Requirements**: Linux or WSL with xterm installed

### Install xterm (Ubuntu/Debian)

```bash
sudo apt-get update
sudo apt-get install xterm
```

### Launch Console App

```bash
cd /mnt/d/lunar-snake/personal-work/yokan-projects/lablab-bean
xterm -e "./dotnet/console-app/LablabBean.Console/bin/Release/net8.0/LablabBean.Console"
```

### Verify Fallback

**Expected Behavior**:
- ✅ Log: "Kitty graphics not supported, using glyph mode"
- ✅ ASCII characters rendered instead of pixel graphics
- ✅ No Kitty escape sequences in output
- ✅ No errors or warnings about missing protocol

## Test Report Template

Create `specs/025-.../test-reports/phase2-terminal-setup.md` with results:

```markdown
# Phase 2: Terminal Emulator Setup - Test Report

**Date**: YYYY-MM-DD
**Tester**: [Your Name]
**Environment**: Windows 11 + WezTerm X.X.X

## T076: Test in WezTerm
**Status**: PASS / FAIL
**Details**: [Description of what happened]
**Screenshot**: [Link to screenshot]

## T077: Test in xterm
**Status**: PASS / FAIL / SKIPPED
**Details**: [Description]

## T085: Screenshot Console Rendering
**Status**: PASS / FAIL
**Screenshots**:
- WezTerm: [Link]
- xterm: [Link]

## T086: Screenshot SadConsole
**Status**: PASS / FAIL
**Screenshot**: [Link]

## T093: Verify Pixel Graphics
**Status**: PASS / FAIL
**Observations**: [Visual quality notes]

## Issues Found
[List any issues]

## Recommendations
[Any improvements or notes]
```

## Success Criteria

Phase 2 is complete when:

- [x] WezTerm installed and configured
- [x] Console app runs in WezTerm
- [x] Kitty graphics detected (or fallback works)
- [x] Screenshots captured
- [x] Visual quality validated
- [x] Test report created

## Next Steps

After Phase 2:
- **Phase 3**: Environment testing (SSH, WSL)
- **Phase 4**: Performance benchmarking
- **Phase 5**: Visual comparison and final validation

## Resources

- **WezTerm Docs**: https://wezfurlong.org/wezterm/
- **Kitty Graphics Protocol**: https://sw.kovidgoyal.net/kitty/graphics-protocol/
- **Test Plan**: `specs/025-.../TESTING_PLAN.md`
- **Phase 1 Report**: `specs/025-.../test-reports/phase1-local-validation.md`

---

**Guide Version**: 1.0
**Last Updated**: 2025-10-28
