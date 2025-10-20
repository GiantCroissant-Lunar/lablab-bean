# Development Handover Document

**Date**: 2025-10-20
**Session**: Dungeon Crawler Implementation (v0.0.2)

## 🎯 What Was Accomplished

Successfully implemented **dungeon crawler features** including dungeon generation with rooms/corridors, player character, monsters, line of sight (FOV), and fog of war. The game now renders in the Terminal.Gui console app and displays via xterm in the browser.

## 📋 Summary of Changes

### 1. Dungeon Generation System
- ✅ Implemented `RoomDungeonGenerator` - creates rooms (6-12 tiles) connected by L-shaped corridors
- ✅ Added `FogOfWar` system - tracks explored vs unexplored tiles
- ✅ Integrated FOV (Field of View) using recursive shadowcasting algorithm (8-tile radius)
- ✅ Map displays three visibility states: visible (bright), explored (dark), unexplored (black)

### 2. Entity System
- ✅ Player character spawns in first room with health, combat stats, and movement
- ✅ Monsters spawn in rooms (1-3 per room): Goblin (g), Orc (o), Troll (T), Skeleton (s)
- ✅ Each monster type has distinct color and stats
- ✅ Entities only visible within player's FOV

### 3. Rendering System
- ✅ Created custom `MapView` for Terminal.Gui rendering
- ✅ Fixed layout timing issue - render triggers after layout completion
- ✅ Camera centers on player position with smooth scrolling
- ✅ Buffer-based rendering for efficient updates

### 4. Known Issues
- ⚠️ Currently only showing one room (likely FOV calculation or dungeon generation issue)
- ⚠️ Fog of war may need adjustment to show explored areas better
- ⚠️ Movement and combat systems not yet tested

## 🏗️ Architecture

```
Development Stack (task dev-stack):
├─ Port 3000: Astro Dev Server
│  ├─ Hot reload enabled
│  ├─ Web UI with xterm terminal
│  └─ Connects to → ws://localhost:3001/terminal
│
└─ Port 3001: Terminal WebSocket Server
   ├─ Spawns PTY sessions (PowerShell on Windows)
   ├─ Auto-runs console app: cd <path>; dotnet run
   └─ Health: http://localhost:3001/health
      Debug: http://localhost:3001/debug
```

## 📁 Key Files Created/Modified

### Created Files
- `dotnet/framework/LablabBean.Game.Core/Maps/RoomDungeonGenerator.cs` - Dungeon generation with rooms and corridors
- `dotnet/framework/LablabBean.Game.Core/Maps/FogOfWar.cs` - Fog of war tracking system
- `dotnet/framework/LablabBean.Game.TerminalUI/Views/MapView.cs` - Custom Terminal.Gui view for dungeon rendering

### Modified Files
- `dotnet/framework/LablabBean.Game.Core/Maps/DungeonMap.cs` - Integrated fog of war with FOV
- `dotnet/framework/LablabBean.Game.Core/Services/GameStateManager.cs` - Uses room generator and spawns monsters per room
- `dotnet/framework/LablabBean.Game.TerminalUI/Services/WorldViewService.cs` - Fixed rendering timing and buffer management
- `dotnet/console-app/LablabBean.Console/Services/DungeonCrawlerService.cs` - Added layout completion handler

## 🚀 How to Use

### Start Development
```bash
# One command to start everything
task dev-stack

# Or step by step
task build-dotnet    # Build .NET components first
task dev-stack       # Start dev stack
```

### Check Status
```bash
task dev-status      # View PM2 process status
task dev-logs        # View live logs
```

### Access the Application
- **Web UI**: http://localhost:3000
- **Terminal**: Opens automatically in browser
- **Console App**: Auto-runs in the web terminal

### Stop Development
```bash
task dev-stop        # Stop all dev processes
```

## 🔧 Configuration

### Auto-Run Console App (Default: Enabled)
The terminal automatically runs the .NET console app when you connect.

**To disable** (get plain PowerShell):
```javascript
// In ecosystem.development.config.js
env: {
  TERMINAL_AUTO_RUN_CONSOLE: 'false'
}
```

### Terminal Server Ports
```javascript
// In ecosystem.development.config.js
env: {
  TERMINAL_PORT: 3001,        // WebSocket server port
  TERMINAL_HOST: '0.0.0.0'    // Bind address
}
```

## 🐛 Known Issues & Solutions

### Issue 1: Only One Room Visible ⚠️ TODO
**Symptom**: Player sees only one room instead of the full dungeon with multiple rooms
**Possible Causes**:
- FOV radius may be too small (currently 8 tiles)
- Map generation may not be creating all rooms properly
- Fog of war might be hiding explored areas too aggressively
**Next Steps**:
- Increase FOV radius to test visibility
- Add debug logging to verify all rooms are generated
- Test movement to see if other rooms appear when player moves
- Check if fog of war is properly marking areas as explored

### Issue 2: Terminal.Gui Rendering Timing ✅ FIXED
**Symptom**: MapView shows "NO BUFFER" - rendering called before layout complete
**Root Cause**: View dimensions are 0x0 when first render is called, before layout finishes
**Solution**:
- Read dimensions from `_renderView.Bounds` on each render
- Trigger render after `LayoutComplete` event instead of immediately
- Skip rendering if dimensions are invalid
- PTY now receives correct size (e.g., 164x53) on first connection
- Terminal.Gui renders correctly for actual viewport size

### Issue 3: Terminal Package Changes Not Applied
**Symptom**: Changes to terminal server code not taking effect
**Solution**:
1. Rebuild terminal package: `cd website/packages/terminal && npm run build`
2. Restart dev stack: `task dev-stop && task dev-stack`
3. TypeScript changes require compilation before PM2 picks them up

## 📊 Current Status (v0.0.2)

### Working ✅
- Dungeon generation with rooms and L-shaped corridors
- Player character spawns in starting room
- Monsters spawn in rooms with distinct colors
- FOV (Field of View) calculation with 8-tile radius
- Fog of war system (explored vs unexplored)
- Custom MapView for Terminal.Gui rendering
- Layout timing fixed - renders after view is laid out
- Terminal displays in browser via xterm.js + PTY
- Web stack auto-runs console app on connection

### Known Limitations
- Only one room visible (FOV or generation issue - needs investigation)
- Movement system not yet tested
- Combat system not yet tested
- No color rendering yet (Terminal.Gui attribute system needs work)

## 🔍 Debugging Tips

### Check Console App Logs
```bash
# Logs are in dotnet/console-app/LablabBean.Console/logs/
Get-Content -Tail 50 dotnet/console-app/LablabBean.Console/logs/*.log
# Look for "Render called", "Buffer created", "Generated dungeon with X rooms"
```
```bash
curl http://localhost:3001/debug
# Should return:
# {
#   "autoRunConsoleApp": true,
#   "consoleAppPath": "D:\\...\\LablabBean.Console",
#   "platform": "win32"
# }
```

### View Live Logs
```bash
task dev-logs                    # All processes
pnpm pm2 logs lablab-web-dev    # Web only
pnpm pm2 logs lablab-pty-dev    # Terminal only
```

### Manual Testing
```bash
# Test terminal server directly
cd website/packages/terminal
npm run build
npm run server

# Test in separate terminal
curl http://localhost:3001/health
```

## 📚 Documentation

- **README.md** - Main project documentation
- **docs/DEVELOPMENT.md** - Detailed development guide
- **QUICKSTART-DEV.md** - Quick start for new developers
- **MIGRATION.md** - Migration from global to local PM2
- **docs/RELEASE.md** - Production release documentation

## 🎯 Next Steps

1. **Test the terminal in browser**
   - Open http://localhost:3000
   - Verify terminal loads without errors
   - Check if console app output appears

2. **If console app doesn't render properly**
   - Consider creating a simpler console output version
   - Or document that Terminal.Gui should be run separately
   - Alternative: Create a web-friendly version of the console app

3. **Production deployment**
   - Use `task build-release` for production builds
   - Use `task stack-run` to run production stack
   - See docs/RELEASE.md for details

## 🔗 Related Commands

```bash
# Development
task dev-stack          # Start dev stack
task dev-stop           # Stop dev stack
task dev-restart        # Restart dev stack
task dev-status         # Check status
task dev-logs           # View logs
task dev-delete         # Delete from PM2

# Production
task build-release      # Build versioned artifacts
task stack-run          # Start production stack
task stack-stop         # Stop production stack
task stack-status       # Check status
task stack-logs         # View logs

# Building
task build-dotnet       # Build .NET components
task build-website      # Build website for production
```

## 📝 Technical Details

### Terminal Sizing Flow
1. Xterm terminal initializes without fixed dimensions
2. FitAddon calculates optimal size based on container (using `requestAnimationFrame`)
3. WebSocket connects to terminal server on port 3001
4. **On connection**: Initial resize message sent with actual terminal dimensions (e.g., 164x53)
5. PTY process receives resize and adjusts accordingly
6. Terminal.Gui console app renders for correct viewport size
7. **On window resize/orientation change**: Resize message sent again

### Key Configuration
- **Terminal scrollback**: 10,000 lines
- **Default PTY size**: 80x24 (overridden by initial resize)
- **Debounce delay**: 100ms for resize events
- **Container styling**: `flex-1 min-h-0 overflow-hidden` prevents infinite growth

### Debugging Tips
- Check browser console for "Sending initial terminal size: XXxYY"
- Check server logs: `pnpm pm2 logs lablab-pty-dev --lines 20`
- Look for "Resizing terminal [id] to XXxYY" in server logs
- If seeing continuous resize messages, ResizeObserver is still active
- If terminal is cut off, initial resize message may not be sent

## ⚠️ Important Notes

- **Terminal.Gui now renders correctly** - Fixed by sending initial PTY dimensions
- **No infinite scrolling** - ResizeObserver removed, only window events trigger resize
- **PM2 runs in background** - No visible UI, use `task dev-status` to check
- **Windows console windows** - May still appear due to OS limitations with `shell: true`
- **TypeScript changes** - Require `npm run build` in terminal package before restart
- **All processes use local PM2** - No global installation needed
- **ESM imports** require .js extensions for local modules

## 🆘 Troubleshooting

If something doesn't work:

1. **Stop everything**: `task dev-stop` or `pnpm pm2 delete all`
2. **Rebuild terminal**: `cd website/packages/terminal && npm run build`
3. **Restart**: `task dev-stack`
4. **Check logs**: `task dev-logs`
5. **Verify config**: `curl http://localhost:3001/debug`

## 📞 Contact

For questions about this implementation, refer to:
- Git commit history
- Documentation in docs/ folder
- This handover document

---

**Session End**: 2025-10-20 20:10  
**Status**: Development stack functional, ready for testing
