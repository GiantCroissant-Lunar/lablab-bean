#!/usr/bin/env node
/**
 * Standalone Terminal Server
 * 
 * This server provides WebSocket terminal access for development mode.
 * It runs on port 3001 and provides PTY sessions via WebSocket.
 * 
 * By default, it automatically runs the console app in the terminal.
 * Set TERMINAL_AUTO_RUN_CONSOLE=false to disable this behavior.
 */

import http from 'http';
import path from 'path';
import { fileURLToPath } from 'url';
import { TerminalServer } from './server.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = process.env.TERMINAL_PORT || 3001;
const HOST = process.env.TERMINAL_HOST || '0.0.0.0';
const AUTO_RUN_CONSOLE = process.env.TERMINAL_AUTO_RUN_CONSOLE !== 'false';

// Calculate console app path relative to this file
// From: website/packages/terminal/dist/standalone-server.js
// To:   dotnet/console-app/LablabBean.Console
const CONSOLE_APP_PATH = path.resolve(__dirname, '..', '..', '..', '..', 'dotnet', 'console-app', 'LablabBean.Console');

// Create HTTP server
const server = http.createServer((req, res) => {
  // Health check endpoint
  if (req.url === '/health') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ status: 'ok', service: 'terminal-server' }));
    return;
  }
  
  // Debug endpoint
  if (req.url === '/debug') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ 
      autoRunConsoleApp: AUTO_RUN_CONSOLE,
      consoleAppPath: CONSOLE_APP_PATH,
      platform: process.platform
    }));
    return;
  }
  
  // Default response
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('Terminal WebSocket Server\nConnect via WebSocket to /terminal');
});

// Attach terminal server with console app configuration
const terminalServer = new TerminalServer(server, {
  path: '/terminal',
  autoRunConsoleApp: AUTO_RUN_CONSOLE,
  consoleAppPath: CONSOLE_APP_PATH,
});

// Start server
server.listen(Number(PORT), HOST, () => {
  console.log(`Terminal WebSocket server running on http://${HOST}:${PORT}`);
  console.log(`WebSocket endpoint: ws://${HOST}:${PORT}/terminal`);
  console.log(`Health check: http://${HOST}:${PORT}/health`);
});

// Graceful shutdown
process.on('SIGTERM', () => {
  console.log('SIGTERM received, shutting down gracefully...');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});

process.on('SIGINT', () => {
  console.log('SIGINT received, shutting down gracefully...');
  server.close(() => {
    console.log('Server closed');
    process.exit(0);
  });
});
