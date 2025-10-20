const path = require('path');

/**
 * PM2 Development Configuration
 * 
 * This configuration runs the stack in development mode with hot reload:
 * - Astro dev server (hot reload enabled)
 * - PTY terminal backend (WebSocket server on port 3001)
 * 
 * Note: Console app is NOT included in dev stack because Terminal.Gui
 * requires an interactive terminal. Run it manually when needed:
 *   cd dotnet/console-app/LablabBean.Console && dotnet run
 * 
 * Usage: pnpm pm2:dev or task dev-stack
 */

// Detect platform
const isWindows = process.platform === 'win32';

module.exports = {
  apps: [
    {
      name: 'lablab-web-dev',
      script: isWindows ? 'cmd.exe' : 'npm',
      args: isWindows ? '/c npm run dev' : 'run dev',
      cwd: path.join(__dirname, 'website', 'apps', 'web'),
      watch: false, // Astro handles its own hot reload
      instances: 1,
      exec_mode: 'fork',
      env: {
        NODE_ENV: 'development',
        HOST: '0.0.0.0',
        PORT: 3000
      },
      error_file: path.join(__dirname, 'logs', 'dev-web-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-web-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    },
    {
      name: 'lablab-pty-dev',
      script: isWindows ? 'cmd.exe' : 'npm',
      args: isWindows ? '/c npm run dev:server' : 'run dev:server',
      cwd: path.join(__dirname, 'website', 'packages', 'terminal'),
      watch: false, // Runs standalone WebSocket server
      instances: 1,
      exec_mode: 'fork',
      env: {
        NODE_ENV: 'development',
        TERMINAL_PORT: 3001,
        TERMINAL_HOST: '0.0.0.0'
      },
      error_file: path.join(__dirname, 'logs', 'dev-pty-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-pty-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    }
    // Console app removed from dev stack - run manually when needed
    // Terminal.Gui apps require an interactive terminal to display their TUI
  ]
};
