#!/usr/bin/env node
/**
 * Standalone Terminal Server
 * 
 * This server provides WebSocket terminal access for development mode.
 * It runs on port 3001 and provides PTY sessions via WebSocket.
 */

import http from 'http';
import { TerminalServer } from './server.js';

const PORT = process.env.TERMINAL_PORT || 3001;
const HOST = process.env.TERMINAL_HOST || '0.0.0.0';

// Create HTTP server
const server = http.createServer((req, res) => {
  // Health check endpoint
  if (req.url === '/health') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ status: 'ok', service: 'terminal-server' }));
    return;
  }
  
  // Default response
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('Terminal WebSocket Server\nConnect via WebSocket to /terminal');
});

// Attach terminal server
const terminalServer = new TerminalServer(server, {
  path: '/terminal',
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
