#!/usr/bin/env node
/**
 * Wrapper script to start Astro dev server
 * This avoids PM2 Windows shell issues
 */

const { spawn } = require('child_process');
const path = require('path');

const cwd = path.join(__dirname, '..', 'website', 'apps', 'web');

console.log(`Starting Astro dev server in: ${cwd}`);

const proc = spawn('npm', ['run', 'dev'], {
  cwd,
  stdio: 'inherit',
  shell: true, // Required on Windows to find npm
  windowsHide: true, // Hide the spawned window on Windows
  env: {
    ...process.env,
    NODE_ENV: 'development',
    HOST: '0.0.0.0',
    PORT: '3000'
  }
});

proc.on('error', (error) => {
  console.error(`Failed to start Astro dev server: ${error.message}`);
  process.exit(1);
});

proc.on('exit', (code) => {
  console.log(`Astro dev server exited with code ${code}`);
  process.exit(code || 0);
});

// Forward signals
process.on('SIGTERM', () => proc.kill('SIGTERM'));
process.on('SIGINT', () => proc.kill('SIGINT'));
