# Development Handover Document

**Date**: 2025-10-20  
**Session**: Local PM2 & Hot Reload Implementation

## 🎯 What Was Accomplished

Successfully refactored the project to use **local PM2** with **hot reload development mode** and implemented **automatic console app execution in the web terminal**.

## 📋 Summary of Changes

### 1. Local PM2 Implementation
- ✅ Added PM2 as local dependency (no global installation needed)
- ✅ Created separate ecosystem configs for dev and production
- ✅ Updated all task commands to use local PM2
- ✅ Fixed Windows compatibility issues (cmd.exe wrapper)

### 2. Hot Reload Development Mode
- ✅ Astro dev server with instant hot reload (port 3000)
- ✅ Standalone terminal WebSocket server (port 3001)
- ✅ Separate build paths: development vs production

### 3. Web Terminal with Auto-Run Console App
- ✅ Created standalone terminal server with PTY sessions
- ✅ Fixed ESM imports (.js extensions)
- ✅ Configured auto-run of .NET console app in terminal
- ✅ Fixed xterm.js hydration error with dynamic imports

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
- `ecosystem.development.config.js` - PM2 dev config with hot reload
- `website/packages/terminal/src/standalone-server.ts` - WebSocket server
- `docs/DEVELOPMENT.md` - Comprehensive development guide
- `QUICKSTART-DEV.md` - Quick start guide
- `MIGRATION.md` - Migration instructions
- `HANDOVER.md` - This document

### Modified Files
- `Taskfile.yml` - Added dev-stack tasks, updated to use local PM2
- `website/package.json` - Added PM2 scripts
- `website/packages/terminal/src/manager.ts` - Auto-run console app logic
- `website/packages/terminal/src/server.ts` - Pass console app options
- `website/packages/terminal/src/types.ts` - Added autoRunConsoleApp option
- `website/packages/terminal/src/index.ts` - Fixed ESM imports
- `website/apps/web/src/components/Terminal.tsx` - Fixed hydration error
- `README.md` - Updated with new workflows

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

### Issue 1: Terminal Not Showing Console App
**Symptom**: Blank terminal or only PowerShell prompt  
**Solution**: 
1. Check server config: `curl http://localhost:3001/debug`
2. Verify `autoRunConsoleApp: true` in response
3. Rebuild terminal package: `cd website/packages/terminal && npm run build`
4. Restart: `task dev-stop && task dev-stack`

### Issue 2: Xterm Hydration Error
**Symptom**: "Cannot read properties of undefined (reading 'Terminal')"  
**Solution**: Already fixed with dynamic imports in Terminal.tsx

### Issue 3: Terminal.Gui Rendering Issues
**Symptom**: Garbled text or weird characters in browser terminal  
**Expected**: Terminal.Gui uses advanced terminal features that may not render perfectly in xterm.js  
**Workaround**: Run console app in separate terminal if needed

### Issue 4: PM2 Process Keeps Restarting
**Symptom**: High restart count in `task dev-status`  
**Solution**: Check logs with `task dev-logs` for errors

## 📊 Current Status

### Working ✅
- Local PM2 installation and execution
- Hot reload for Astro dev server
- Terminal WebSocket server on port 3001
- PTY session creation
- Auto-run console app configuration
- Dynamic xterm.js imports (no hydration errors)

### Verified ✅
- `task dev-stack` starts both processes
- `task dev-status` shows online status
- http://localhost:3000 loads web UI
- http://localhost:3001/health returns OK
- http://localhost:3001/debug shows correct config

### Pending Testing ⏳
- Console app actually rendering in browser terminal
- Terminal.Gui compatibility with xterm.js
- User interaction with console app in browser

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

## 📝 Commits Made

1. `feat: add local PM2 with hot reload development mode`
2. `fix: resolve YAML parsing and PM2 Windows compatibility issues`
3. `fix: use cmd.exe wrapper for PM2 on Windows`
4. `feat: add standalone terminal WebSocket server for development`
5. `refactor: remove console app from dev stack`
6. `feat: auto-run console app in xterm terminal`
7. `fix: resolve xterm hydration error with dynamic imports`

## ⚠️ Important Notes

- **Console app removed from PM2 dev stack** because Terminal.Gui requires interactive terminal
- **Auto-run is configured** but Terminal.Gui may not render perfectly in browser
- **All processes use local PM2** - no global installation needed
- **Windows-specific** cmd.exe wrapper for PM2 compatibility
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
