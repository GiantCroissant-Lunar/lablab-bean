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

module.exports = {
  apps: [
    {
      name: 'lablab-web-dev',
      script: path.join(__dirname, '..', '..', 'scripts', 'start-web-dev.js'),
      cwd: path.join(__dirname, '..', '..'),
      watch: false, // Astro handles its own hot reload
      instances: 1,
      exec_mode: 'fork',
      windowsHide: true, // Hide console window on Windows
      error_file: path.join(__dirname, 'logs', 'dev-web-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-web-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    },
    {
      name: 'lablab-pty-dev',
      script: path.join(__dirname, '..', '..', 'scripts', 'start-terminal-server.js'),
      cwd: path.join(__dirname, '..', '..'),
      watch: false, // Runs standalone WebSocket server
      instances: 1,
      exec_mode: 'fork',
      windowsHide: true, // Hide console window on Windows
      error_file: path.join(__dirname, 'logs', 'dev-pty-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-pty-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    }
    // Console app removed from dev stack - run manually when needed
    // Terminal.Gui apps require an interactive terminal to display their TUI
  ]
};
