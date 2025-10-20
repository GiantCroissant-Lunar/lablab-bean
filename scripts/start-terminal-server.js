#!/usr/bin/env node
/**
 * Wrapper script to start terminal WebSocket server
 * This avoids PM2 Windows shell issues
 */

const { spawn } = require('child_process');
const path = require('path');

const cwd = path.join(__dirname, '..', 'website', 'packages', 'terminal');

console.log(`Starting terminal WebSocket server in: ${cwd}`);

const proc = spawn('npm', ['run', 'dev:server'], {
  cwd,
  stdio: 'inherit',
  shell: true, // Required on Windows to find npm
  windowsHide: true, // Hide the spawned window on Windows
  env: {
    ...process.env,
    NODE_ENV: 'development',
    TERMINAL_PORT: '3001',
    TERMINAL_HOST: '0.0.0.0'
  }
});

proc.on('error', (error) => {
  console.error(`Failed to start terminal server: ${error.message}`);
  process.exit(1);
});

proc.on('exit', (code) => {
  console.log(`Terminal server exited with code ${code}`);
  process.exit(code || 0);
});

// Forward signals
process.on('SIGTERM', () => proc.kill('SIGTERM'));
process.on('SIGINT', () => proc.kill('SIGINT'));
