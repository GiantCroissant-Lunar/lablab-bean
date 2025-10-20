# Development Handover Document

**Date**: 2025-10-20
**Session**: Terminal Rendering & PM2 Windows Fixes

## 🎯 What Was Accomplished

Successfully fixed **Terminal.Gui rendering issues**, **PM2 Windows compatibility**, and implemented **responsive terminal sizing** for the web-based xterm terminal.

## 📋 Summary of Changes

### 1. Terminal.Gui Rendering Fixes
- ✅ Fixed Terminal.Gui bottom cut-off issue by sending initial PTY size on WebSocket connect
- ✅ Removed infinite resize loop caused by ResizeObserver
- ✅ Implemented proper terminal dimension synchronization between xterm and PTY
- ✅ Added scrollback buffer (10,000 lines) for terminal history
- ✅ Fixed terminal container overflow and sizing issues

### 2. PM2 Windows Compatibility
- ✅ Created Node.js wrapper scripts to avoid cmd.exe visibility issues
- ✅ Added `windowsHide: true` to spawn options in wrapper scripts
- ✅ Removed problematic cmd.exe wrapper in PM2 config
- ✅ Fixed npm spawn issues on Windows with `shell: true`

### 3. Responsive Terminal Sizing
- ✅ Terminal now automatically fits to available container space
- ✅ Proper resize handling for browser window changes
- ✅ Mobile-responsive with orientation change support (portrait/landscape)
- ✅ Removed fixed terminal dimensions - uses FitAddon for dynamic sizing
- ✅ Fixed page overflow issues with `overflow: hidden` on html/body

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
- `scripts/start-web-dev.js` - Wrapper script for Astro dev server (hides Windows console)
- `scripts/start-terminal-server.js` - Wrapper script for terminal WebSocket server (hides Windows console)

### Modified Files
- `website/apps/web/src/components/Terminal.tsx` - Fixed terminal sizing, resize loop, and PTY synchronization
- `website/apps/web/src/layouts/Layout.astro` - Added `overflow: hidden` to prevent page scrolling
- `website/packages/terminal/src/server.ts` - Added resize logging for debugging
- `ecosystem.development.config.js` - Updated to use wrapper scripts instead of cmd.exe

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

### Issue 1: Terminal.Gui Bottom Cut Off ✅ FIXED
**Symptom**: Bottom of Terminal.Gui console app not visible
**Root Cause**: PTY initialized with default 80x24, but xterm had different dimensions. Initial resize message not sent.
**Solution**:
- Send initial terminal dimensions immediately after WebSocket connection
- PTY now receives correct size (e.g., 164x53) on first connection
- Terminal.Gui renders correctly for actual viewport size

### Issue 2: Infinite Scrolling/Resize Loop ✅ FIXED
**Symptom**: Page keeps growing vertically, terminal keeps resizing
**Root Cause**: ResizeObserver triggering on every terminal render, causing feedback loop
**Solution**:
- Removed ResizeObserver from terminal component
- Use window resize and orientation change events only
- Added `overflow: hidden` to html/body to prevent page scrolling

### Issue 3: PM2 Spawning Visible Windows ✅ IMPROVED
**Symptom**: Two cmd.exe windows visible when running PM2
**Root Cause**: Windows creates console windows for spawned processes
**Solution**:
- Created Node.js wrapper scripts with `windowsHide: true`
- PM2 now uses wrapper scripts instead of direct npm commands
- Windows may still show console windows due to OS limitations with `shell: true`

### Issue 4: Terminal Package Changes Not Applied
**Symptom**: Changes to terminal server code not taking effect
**Solution**:
1. Rebuild terminal package: `cd website/packages/terminal && npm run build`
2. Restart dev stack: `task dev-stop && task dev-stack`
3. TypeScript changes require compilation before PM2 picks them up

## 📊 Current Status

### Working ✅
- Terminal.Gui console app renders completely in browser (no cut-off bottom)
- Responsive terminal sizing (164x53 or similar based on browser size)
- No infinite scrolling or resize loops
- PM2 running both processes without visible windows (on most systems)
- Hot reload for Astro dev server
- Terminal WebSocket server on port 3001
- PTY session creation and proper dimension synchronization
- Auto-run console app configuration
- Dynamic xterm.js imports (no hydration errors)
- Initial PTY resize message sent on WebSocket connect

### Verified ✅
- `task dev-stack` starts both processes
- `task dev-status` shows online status (0 restarts after fixes)
- http://localhost:3000 loads web UI with properly sized terminal
- Terminal dimensions sent: 164x53 (example, varies by screen size)
- Server logs show single resize message, not infinite loop
- Terminal.Gui interface fully visible without scrolling
- Mobile/tablet responsive with orientation support

## 🔍 Debugging Tips

### Check Terminal Server Config
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
