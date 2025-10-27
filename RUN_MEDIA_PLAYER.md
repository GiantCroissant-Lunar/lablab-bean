# 🎬 Running the Unified Media Player

**Quick Start Guide for Versioned Artifacts**

---

## 📦 Current Build Version

**Build**: `0.0.4-021-unified-media-player.1`  
**Location**: `build\_artifacts\0.0.4-021-unified-media-player.1\publish\console\`  
**Status**: ✅ Ready for Testing

---

## 🚀 Running from Versioned Artifact

### **Option 1: Direct Execution (Recommended)**

```powershell
# Navigate to versioned artifact
cd build\_artifacts\0.0.4-021-unified-media-player.1\publish\console

# Run the console app
.\LablabBean.Console.exe
```

### **Option 2: Task Command (Recommended)**

```powershell
# Run the media player
task run:media-player

# With arguments (pass through to executable)
task run:media-player -- plugins list
task run:media-player -- plugins info MediaPlayer.Core
task run:media-player -- play --file "video.mp4" --renderer braille
```

### **Option 3: From Any Location (Full Path)**

```powershell
# Run with full path
.\build\_artifacts\0.0.4-021-unified-media-player.1\publish\console\LablabBean.Console.exe
```

### **Option 4: With Arguments (Direct)**

```powershell
cd build\_artifacts\0.0.4-021-unified-media-player.1\publish\console

# List all plugins
.\LablabBean.Console.exe plugins list

# Show plugin info
.\LablabBean.Console.exe plugins info MediaPlayer.Core

# Play a video (example - adjust command as needed)
.\LablabBean.Console.exe play --file "path\to\video.mp4" --renderer braille
```

---

## 📂 Artifact Structure

```
build\_artifacts\0.0.4-021-unified-media-player.1\publish\console\
├── LablabBean.Console.exe          ← Main executable (151 KB)
├── *.dll                           ← Core libraries
└── plugins\                        ← Plugin directory (42 plugins)
    ├── LablabBean.Plugins.MediaPlayer.Core\
    │   ├── LablabBean.Plugins.MediaPlayer.Core.dll
    │   └── plugin.json
    ├── LablabBean.Plugins.MediaPlayer.FFmpeg\
    │   ├── LablabBean.Plugins.MediaPlayer.FFmpeg.dll
    │   └── plugin.json
    ├── LablabBean.Plugins.MediaPlayer.Terminal.Braille\
    │   ├── LablabBean.Plugins.MediaPlayer.Terminal.Braille.dll
    │   └── plugin.json
    ├── LablabBean.Plugins.MediaPlayer.Terminal.Kitty\
    │   ├── LablabBean.Plugins.MediaPlayer.Terminal.Kitty.dll
    │   └── plugin.json
    ├── LablabBean.Plugins.MediaPlayer.Terminal.Sixel\
    │   ├── LablabBean.Plugins.MediaPlayer.Terminal.Sixel.dll
    │   └── plugin.json
    └── [... 37 other plugins ...]
```

---

## 🎯 Testing Media Player Features

### **Test User Story 1: Basic Playback**
```powershell
# Using task command
task run:media-player -- play --file "test-video.mp4" --renderer braille

# Or direct execution
cd build\_artifacts\0.0.4-021-unified-media-player.1\publish\console
.\LablabBean.Console.exe play --file "test-video.mp4" --renderer braille
```

### **Test User Story 2: Interactive Controls**
```powershell
# Once playing, test controls:
# - SPACE: Play/Pause
# - Q: Quit
# - Arrow keys: Seek forward/backward
```

### **Test User Story 3: Multiple Renderers**
```powershell
# Test Braille renderer
task run:media-player -- play --file "video.mp4" --renderer braille

# Test Sixel renderer
task run:media-player -- play --file "video.mp4" --renderer sixel

# Test Kitty renderer
task run:media-player -- play --file "video.mp4" --renderer kitty
```

### **Test User Story 4: Plugin System**
```powershell
# Verify media player plugins loaded
task run:media-player -- plugins list

# Get detailed plugin info
task run:media-player -- plugins info MediaPlayer.Core
task run:media-player -- plugins info MediaPlayer.FFmpeg
```

---

## 🔍 Verification Commands

### **Check All Plugins Loaded**
```powershell
# Using task command
task run:media-player -- plugins list

# Or direct execution
cd build\_artifacts\0.0.4-021-unified-media-player.1\publish\console
.\LablabBean.Console.exe plugins list
```

**Expected Output** (should include):
```
✅ LablabBean.Plugins.MediaPlayer.Core
✅ LablabBean.Plugins.MediaPlayer.FFmpeg
✅ LablabBean.Plugins.MediaPlayer.Terminal.Braille
✅ LablabBean.Plugins.MediaPlayer.Terminal.Kitty
✅ LablabBean.Plugins.MediaPlayer.Terminal.Sixel
```

### **Verify Media Player Plugin Info**
```powershell
# Using task command
task run:media-player -- plugins info MediaPlayer.Core

# Or direct execution
cd build\_artifacts\0.0.4-021-unified-media-player.1\publish\console
.\LablabBean.Console.exe plugins info MediaPlayer.Core
```

**Expected**: Should show plugin metadata, version, dependencies, etc.

---

## 📝 Quick PowerShell Script

The `run-media-player.ps1` script is now integrated with the task command.

**Usage via Task Command (Recommended)**:
```powershell
# Basic usage
task run:media-player

# With arguments
task run:media-player -- plugins list
task run:media-player -- play --file video.mp4 --renderer braille
task run:media-player -- --help
```

**Direct Script Usage** (if needed):
```powershell
.\run-media-player.ps1
.\run-media-player.ps1 plugins list
.\run-media-player.ps1 play --file video.mp4 --renderer braille
```

---

## 🛠️ Troubleshooting

### **If executable not found:**
```powershell
# Rebuild and publish
task publish
```

### **If plugins not loading:**
```powershell
# Check plugins directory exists
Test-Path "build\_artifacts\0.0.4-021-unified-media-player.1\publish\console\plugins"

# List plugin directories
Get-ChildItem "build\_artifacts\0.0.4-021-unified-media-player.1\publish\console\plugins" -Directory
```

### **If FFmpeg errors:**
- Ensure FFmpeg is installed and in PATH
- Check FFmpeg version: `ffmpeg -version`
- Install if needed: `choco install ffmpeg` or download from ffmpeg.org

---

## 📊 Performance Tips

1. **First Run**: May be slower due to plugin loading and JIT compilation
2. **Subsequent Runs**: Should be faster with cached assemblies
3. **Large Videos**: Consider testing with smaller files first
4. **Terminal Performance**: Braille is fastest, Sixel/Kitty may be slower

---

## 🎯 Next Steps After Testing

1. **Document Issues**: Note any errors or unexpected behavior
2. **Test Video Formats**: Try various formats (MP4, AVI, MOV, etc.)
3. **Terminal Compatibility**: Test in different terminals (Windows Terminal, ConEmu, etc.)
4. **Performance Metrics**: Measure playback FPS and resource usage

---

## 📚 Additional Resources

- **Specification**: `specs\021-unified-media-player\README.md`
- **Implementation Status**: `specs\021-unified-media-player\IMPLEMENTATION_STATUS.md`
- **Build Verification**: `specs\021-unified-media-player\BUILD_VERIFICATION.md`
- **Issue Fix Details**: `ISSUE_FIX_COMPLETE.md`
- **Complete Status**: `FINAL_STATUS.md`

---

**Generated**: 2025-10-26  
**Build Version**: 0.0.4-021-unified-media-player.1  
**Status**: ✅ Ready for Manual Testing
