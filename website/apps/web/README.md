# @lablab-bean/web

Astro.js web application with integrated terminal using xterm.js and node-pty.

## Features

- **Astro.js**: Modern web framework with SSR support
- **React**: For interactive components
- **Tailwind CSS**: Utility-first CSS framework
- **xterm.js**: Terminal emulator in the browser
- **node-pty**: PTY bindings for Node.js (terminal backend)
- **WebSocket**: Real-time communication between frontend and backend

## Development

```bash
pnpm dev
```

## Build

```bash
pnpm build
```

## Preview

```bash
pnpm preview
```

## Architecture

- **Frontend**: Astro + React + xterm.js
- **Backend**: Node.js + node-pty + WebSocket
- **Communication**: WebSocket for bidirectional terminal I/O

## Terminal Integration

The terminal component (`Terminal.tsx`) connects to a WebSocket server that manages PTY processes using node-pty. This allows for a full-featured terminal experience in the browser.

### How it works

1. Client opens WebSocket connection to `/terminal`
2. Server creates a new PTY process (PowerShell on Windows, Bash on Unix)
3. Terminal input is sent via WebSocket to PTY
4. PTY output is sent back via WebSocket to terminal
5. Terminal renders output using xterm.js
