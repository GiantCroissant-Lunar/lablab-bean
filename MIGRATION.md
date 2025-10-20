# Migration Guide: Local PM2 & Hot Reload Development

This document explains the changes made to support local PM2 and hot reload development.

## What Changed?

### 1. PM2 is Now Local (Not Global)

**Before:**
```bash
# Required global PM2 installation
npm install -g pm2
pm2 start ecosystem.config.js
```

**After:**
```bash
# PM2 is a local dependency in website/package.json
cd website
pnpm pm2 start ../ecosystem.development.config.js
# Or use task commands
task dev-stack
```

### 2. Two Build Modes

**Development Mode** (New):
- Astro dev server with hot reload
- TypeScript watch mode for PTY
- .NET console app in development mode
- Fast iteration, no bundling

**Production Mode** (Existing):
- Bundled Astro app
- Compiled artifacts
- Versioned releases
- For testing and deployment

### 3. New Configuration Files

**Created:**
- `ecosystem.development.config.js` - Development stack configuration
- `docs/DEVELOPMENT.md` - Comprehensive development guide
- `QUICKSTART-DEV.md` - Quick start for development mode

**Modified:**
- `website/package.json` - Added local PM2 scripts
- `Taskfile.yml` - Added development tasks
- `README.md` - Updated with development workflow

## Migration Steps

### If You Were Using Global PM2

1. **Stop existing PM2 processes:**
   ```bash
   pm2 stop all
   pm2 delete all
   ```

2. **PM2 is already installed locally** (in `website/node_modules`):
   ```bash
   cd website
   pnpm install  # If needed
   ```

3. **Use new commands:**
   ```bash
   # Development
   task dev-stack
   
   # Production
   task stack-run
   ```

### If You Were Running Components Manually

**Before:**
```bash
# Terminal 1
cd website
pnpm dev

# Terminal 2
cd dotnet/console-app/LablabBean.Console
dotnet run
```

**After:**
```bash
# Single command
task dev-stack

# Everything runs in PM2
task dev-logs  # View logs
```

## New Commands Reference

### Development Commands (New)

| Old Workflow | New Command | Description |
|--------------|-------------|-------------|
| `cd website && pnpm dev` | `task dev-stack` | Start full dev stack |
| Manual terminal management | `task dev-logs` | View all logs |
| Ctrl+C in multiple terminals | `task dev-stop` | Stop everything |
| N/A | `task dev-status` | Check status |
| N/A | `task dev-restart` | Restart all |

### Production Commands (Updated)

| Old Command | New Command | Notes |
|-------------|-------------|-------|
| `pm2 start ecosystem.config.js` | `task stack-run` | Uses local PM2 |
| `pm2 stop all` | `task stack-stop` | Uses local PM2 |
| `pm2 logs` | `task stack-logs` | Uses local PM2 |
| `pm2 status` | `task stack-status` | Uses local PM2 |

## Breaking Changes

### ⚠️ Global PM2 No Longer Required

If you have global PM2 installed, it won't be used. All commands now use the local PM2 installation.

**To uninstall global PM2** (optional):
```bash
npm uninstall -g pm2
```

### ⚠️ Old Ecosystem Config Renamed

- `ecosystem.config.js` → Still exists (for backward compatibility)
- `ecosystem.production.config.js` → Production configuration
- `ecosystem.development.config.js` → New development configuration

### ⚠️ Task Command Changes

Some task commands now behave differently:

| Task | Old Behavior | New Behavior |
|------|--------------|--------------|
| `task build` | Build everything | Build .NET only (website uses hot reload) |
| `task stack-run` | Uses global PM2 | Uses local PM2 |
| `task stack-stop` | Stops specific config | Stops all PM2 processes |

## Benefits of New Approach

### 1. No Global Dependencies
- PM2 is project-local
- Easier onboarding for new developers
- No version conflicts

### 2. Hot Reload Development
- Instant Astro updates
- Auto-recompile TypeScript
- Faster development cycle

### 3. Better Process Management
- All processes in one place
- Easy log viewing
- Unified status checking

### 4. Separate Dev/Prod Workflows
- Development: Fast iteration
- Production: Versioned artifacts
- Clear separation of concerns

## Troubleshooting

### "pm2: command not found"

**Solution:** Use task commands or pnpm:
```bash
# Instead of: pm2 status
task dev-status
# Or: cd website && pnpm pm2 status
```

### "Port 3000 already in use"

**Solution:** Stop all PM2 processes:
```bash
task dev-stop
task stack-stop
```

### "Processes not starting"

**Solution:** Clean PM2 state:
```bash
cd website
pnpm pm2 delete all
pnpm pm2 kill
task dev-stack
```

### "Hot reload not working"

**Solution:** Restart development stack:
```bash
task dev-restart
```

## Rollback (If Needed)

If you need to revert to the old workflow:

1. **Use old ecosystem config:**
   ```bash
   cd website
   pnpm pm2 start ecosystem.config.js
   ```

2. **Or run components manually:**
   ```bash
   # Terminal 1
   cd website && pnpm dev
   
   # Terminal 2
   cd dotnet/console-app/LablabBean.Console && dotnet run
   ```

## Questions?

- See [QUICKSTART-DEV.md](QUICKSTART-DEV.md) for quick start
- See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed guide
- See [README.md](README.md) for all commands

## Summary

✅ **PM2 is now local** - No global installation needed  
✅ **Hot reload development** - Instant updates for Astro  
✅ **Two build modes** - Development (fast) and Production (bundled)  
✅ **Better task commands** - `task dev-stack` for development  
✅ **Comprehensive docs** - Guides for both workflows  

The new workflow is designed to speed up development while maintaining robust production builds.
