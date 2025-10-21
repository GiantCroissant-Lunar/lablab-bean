---
doc_id: DOC-2025-00022
title: Quick Start Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [quickstart, getting-started, release]
summary: >
  Get started with Lablab Bean in 3 steps: build, start, and access.
---

# Quick Start Guide

## 🚀 Get Started in 3 Steps

### 1. Build the Release
```bash
task build-release
```

### 2. Start the Stack
```bash
task stack-run
```

### 3. Access Your Apps
- 🌐 **Web App**: http://localhost:3000
- 💻 **Console App**: Running in PM2
- 🎮 **Windows App**: Available in artifacts

---

## ⚡ One-Command Start

```bash
task release-and-run
```

Or use the PowerShell script:
```powershell
.\build-and-run.ps1
```

---

## 📊 Essential Commands

| Command | Description |
|---------|-------------|
| `task stack-status` | Check if stack is running |
| `task stack-logs` | View live logs |
| `task stack-stop` | Stop the stack |
| `task stack-restart` | Restart the stack |
| `task list-versions` | List available versions |

---

## 🔍 Monitoring

### View All Logs
```bash
task stack-logs
```

### View Specific App Logs
```bash
task stack-logs-web      # Web app only
task stack-logs-console  # Console app only
```

### PM2 Dashboard
```bash
task stack-monit
```

---

## 🛠️ Troubleshooting

### Stack won't start?
```bash
# Check if artifacts exist
task list-versions

# Rebuild if needed
task build-release

# Try starting again
task stack-run
```

### Need to clean restart?
```bash
task stack-delete    # Remove from PM2
task build-release   # Rebuild
task stack-run       # Start fresh
```

---

## 📦 What Gets Built?

The build creates versioned artifacts in:
```
build/_artifacts/<version>/publish/
├── console/    # Self-contained .NET console app
├── windows/    # Self-contained .NET Windows app
└── website/    # Built Astro website + Node.js
```

---

## 🎯 Development vs Production

### Development (Hot Reload)
```bash
cd website
pnpm dev
```

### Production (Versioned Artifacts)
```bash
task release-and-run
```

---

## 📚 More Information

- **Full Release Guide**: [RELEASE.md](RELEASE.md)
- **Project README**: [README.md](README.md)
- **All Tasks**: `task --list`

---

## 💡 Tips

1. **Version Management**: Set `LABLAB_VERSION` env var to use specific version
2. **Logs Location**: Check `logs/` directory for historical logs
3. **Version Info**: Each build includes `version.json` with metadata
4. **PM2 Commands**: All standard PM2 commands work (e.g., `pm2 list`, `pm2 monit`)

---

**Need help?** Run `task --list` to see all available commands.
