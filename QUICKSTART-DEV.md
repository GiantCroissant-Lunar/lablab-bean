# Quick Start - Development Mode

Get up and running with hot reload in under 2 minutes.

## Prerequisites

- Node.js 18+
- pnpm 8+
- .NET 8 SDK
- Task CLI

## First Time Setup

```bash
# 1. Install dependencies
cd website
pnpm install

# 2. Build .NET components (one-time)
cd ..
task build-dotnet
```

## Start Development

```bash
# Start the full stack with hot reload
task dev-stack
```

That's it! 🎉

- **Web App**: http://localhost:3000 (hot reload enabled)
- **PTY Backend**: Auto-rebuilds on changes
- **Console App**: Running in development mode

## Daily Workflow

```bash
# Morning: Start stack
task dev-stack

# Work on your code
# - Edit Astro/React files → instant updates
# - Edit TypeScript → auto-recompile
# - Edit .NET → rebuild with `task build-dotnet`

# Check status anytime
task dev-status

# View logs
task dev-logs

# End of day: Stop stack
task dev-stop
```

## Common Commands

| Command | What it does |
|---------|--------------|
| `task dev-stack` | Start everything |
| `task dev-stop` | Stop everything |
| `task dev-status` | Check what's running |
| `task dev-logs` | See live logs |
| `task dev-restart` | Restart all processes |

## What Has Hot Reload?

✅ **Astro/React** - Instant hot reload  
✅ **TypeScript (PTY)** - Auto-recompile  
❌ **C# (.NET)** - Manual rebuild needed

## Troubleshooting

### Port 3000 already in use?
```bash
task dev-stop
task stack-stop
```

### Changes not showing?
```bash
task dev-restart
```

### Something weird?
```bash
# Nuclear option
cd website
pnpm pm2 delete all
task dev-stack
```

## Next Steps

- Read [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for detailed guide
- See [README.md](README.md) for all available commands
- Check [docs/RELEASE.md](docs/RELEASE.md) for production builds

## Key Differences from Production

| | Development | Production |
|-|-------------|------------|
| **Astro** | Dev server | Bundled app |
| **Speed** | Instant updates | Requires rebuild |
| **PM2** | Local (pnpm) | Can be global |
| **Use for** | Active coding | Testing/Deploy |

## Pro Tips

1. **Keep logs open** in a separate terminal: `task dev-logs`
2. **Build .NET once**, then focus on web development
3. **Use production mode** before committing: `task release-and-run`
4. **Monitor with PM2**: `cd website && pnpm pm2 monit`

Happy coding! 🚀
