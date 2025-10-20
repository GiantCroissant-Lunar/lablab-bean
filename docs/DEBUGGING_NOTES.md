# Debugging Notes - Stack Testing

## Status: ✅ Partially Working

### What Works

1. **✅ Conventional Commits** - Pre-commit hooks installed and working
2. **✅ Website Dependencies** - pnpm install successful (node-pty compiled)
3. **✅ .NET Console App** - Builds and runs successfully
4. **✅ Terminal.Gui** - SimpleWindow displays correctly

### Issues Encountered

1. **node-pty Compilation**
   - **Problem**: Python 3.13 removed `distutils` module
   - **Solution**: Installed `setuptools` via pip
   - **Problem**: node-pty 1.0.0 had C++ compilation errors
   - **Solution**: Upgraded to node-pty 1.1.0-beta17

2. **Serilog Configuration**
   - **Problem**: Missing `Serilog.Settings.Configuration` package
   - **Solution**: Added package to Directory.Packages.props

3. **Terminal.Gui API Changes**
   - **Problem**: Terminal.Gui v2.0.0-pre.71 has breaking API changes
   - **Solution**: Created SimpleWindow with basic functionality
   - **Excluded**: MainWindow.cs and InteractiveWindow.cs (incompatible APIs)

4. **SadConsole Issues**
   - **Problem**: Multiple API compatibility issues with SadConsole 10.x
   - **Status**: Not fixed yet (Windows app doesn't build)

## Current Stack Status

### Console App (Terminal.Gui)
```bash
cd dotnet/console-app/LablabBean.Console
dotnet run
```
**Status**: ✅ Working
- Displays welcome screen
- Shows keyboard shortcuts
- Status bar functional
- ESC, F1, F5 keys work

### Website (Astro + xterm.js + node-pty)
```bash
cd website
pnpm dev
```
**Status**: ⏳ Ready to test
- Dependencies installed
- node-pty compiled successfully
- Ready for integration testing

### PM2 Stack Management
```bash
cd website
pnpm stack:start
```
**Status**: ⏳ Ready to test
- PM2 configured
- ecosystem.config.js created
- Needs testing with full stack

## Next Steps

1. **Test Website**
   ```bash
   cd website
   pnpm dev
   ```
   - Verify Astro server starts
   - Check xterm.js loads
   - Test WebSocket connection

2. **Test PTY Integration**
   - Start console app via node-pty
   - Verify Terminal.Gui renders in browser
   - Test keyboard input forwarding

3. **Test PM2 Stack**
   ```bash
   pnpm stack:start
   ```
   - Verify both processes start
   - Check logs with `pnpm pm2:logs`
   - Test process management

4. **Fix SadConsole** (Optional)
   - Update to compatible API
   - Or remove from initial release

## Files Modified for Compatibility

### Console App
- `LablabBean.Console.csproj` - Excluded incompatible views
- `Services/TerminalGuiService.cs` - Use SimpleWindow
- `Views/SimpleWindow.cs` - New compatible window

### Website
- `packages/terminal/package.json` - Upgraded node-pty to 1.1.0-beta17

### .NET Infrastructure
- `Directory.Packages.props` - Added Serilog.Settings.Configuration
- `LablabBean.Infrastructure.csproj` - Added Serilog package reference

## Testing Commands

### Individual Components
```bash
# Console App
cd dotnet/console-app/LablabBean.Console
dotnet run

# Website
cd website
pnpm dev

# Build with NUKE
cd build/nuke
dotnet run -- Compile
```

### Full Stack
```bash
# Via PM2
cd website
pnpm stack:start
pnpm pm2:logs
pnpm stack:stop

# Via Task
task stack-start
task stack-logs
task stack-stop
```

## Known Limitations

1. **Terminal.Gui v2 API** - Many breaking changes from v1
   - File dialogs API changed
   - Keyboard handling changed
   - ListView events changed

2. **SadConsole 10.x API** - Significant changes
   - Console namespace conflicts
   - Cursor.Print signature changed
   - Game.Services API changed

3. **Cross-platform** - node-pty requires native compilation
   - Windows: Requires Visual Studio Build Tools
   - Linux: Requires build-essential
   - macOS: Requires Xcode Command Line Tools

## Success Criteria

- [x] Conventional commits working
- [x] .NET solution builds
- [x] Console app runs
- [ ] Website starts
- [ ] xterm.js displays terminal
- [ ] PTY connects to console app
- [ ] Keyboard input works
- [ ] PM2 manages both processes

## Performance Notes

- node-pty compilation: ~1-2 minutes
- .NET restore: ~3-5 seconds
- .NET build: ~2-3 seconds
- pnpm install: ~1.5 minutes

## Environment

- **OS**: Windows 10/11
- **Node.js**: v23.11.0
- **pnpm**: 8.15.0
- **.NET**: 8.0
- **Python**: 3.13.1 (with setuptools)
- **Visual Studio**: 2022 Community

## Conclusion

The core infrastructure is working. The console TUI app runs successfully with Terminal.Gui. The website dependencies are installed and ready. Next step is to test the full integration of web + PTY + console app.
