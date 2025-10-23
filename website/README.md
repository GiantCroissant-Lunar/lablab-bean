# Lablab Bean Website

Monorepo for the Lablab Bean web application using pnpm workspaces.

## Structure

```
website/
├── apps/
│   └── web/              # Astro.js web application
├── packages/
│   └── terminal/         # Terminal package (node-pty + WebSocket)
└── pnpm-workspace.yaml   # Workspace configuration
```

## Prerequisites

- Node.js 18+
- pnpm 8+

## Installation

```bash
pnpm install
```

## Development

```bash
# Run web app
pnpm dev

# Run all apps in parallel
pnpm dev:all
```

## Build

```bash
# Build web app
pnpm build

# Build all packages
pnpm build:all
```

## Packages

### @lablab-bean/web

Astro.js web application with terminal interface.

**Tech Stack:**

- Astro.js (SSR)
- React
- Tailwind CSS
- xterm.js

### @lablab-bean/terminal

Terminal backend package using node-pty.

**Features:**

- PTY process management
- WebSocket server
- Session management
- Cross-platform shell support

## Scripts

- `pnpm dev` - Start development server
- `pnpm build` - Build for production
- `pnpm preview` - Preview production build
- `pnpm clean` - Clean all build artifacts
- `pnpm lint` - Lint all packages
- `pnpm format` - Format code with Prettier

## Architecture

The application uses a monorepo structure with pnpm workspaces:

1. **Web App** (`apps/web`): Frontend application built with Astro.js
2. **Terminal Package** (`packages/terminal`): Shared terminal logic and WebSocket server

The terminal package is consumed by the web app and provides:

- WebSocket server for terminal communication
- PTY process management using node-pty
- Session handling and lifecycle management

## WebSocket Protocol

### Client → Server

- **Terminal Input**: Raw string data
- **Resize**: `{ type: 'resize', cols: number, rows: number }`

### Server → Client

- **Terminal Output**: Raw string data (ANSI escape sequences)

## Platform Support

- **Windows**: PowerShell
- **macOS/Linux**: Bash

The shell is automatically detected based on the platform.
