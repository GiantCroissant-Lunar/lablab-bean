const path = require('path');

/**
 * PM2 Development Configuration
 * 
 * This configuration runs the stack in development mode with hot reload:
 * - Astro dev server (hot reload enabled)
 * - PTY terminal backend (no hot reload needed)
 * - Console app (from latest build artifacts)
 * 
 * Usage: pnpm pm2:dev or task dev-stack
 */

module.exports = {
  apps: [
    {
      name: 'lablab-web-dev',
      script: path.join(__dirname, 'website', 'apps', 'web', 'package.json'),
      args: 'dev',
      cwd: path.join(__dirname, 'website', 'apps', 'web'),
      interpreter: 'pnpm',
      interpreter_args: 'run',
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
      script: path.join(__dirname, 'website', 'packages', 'terminal', 'package.json'),
      args: 'dev',
      cwd: path.join(__dirname, 'website', 'packages', 'terminal'),
      interpreter: 'pnpm',
      interpreter_args: 'run',
      watch: false, // TypeScript watch mode handles rebuilds
      instances: 1,
      exec_mode: 'fork',
      env: {
        NODE_ENV: 'development'
      },
      error_file: path.join(__dirname, 'logs', 'dev-pty-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-pty-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    },
    {
      name: 'lablab-console-dev',
      script: 'dotnet',
      args: 'run',
      cwd: path.join(__dirname, 'dotnet', 'console-app', 'LablabBean.Console'),
      watch: false,
      instances: 1,
      exec_mode: 'fork',
      env: {
        DOTNET_ENVIRONMENT: 'Development'
      },
      error_file: path.join(__dirname, 'logs', 'dev-console-error.log'),
      out_file: path.join(__dirname, 'logs', 'dev-console-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    }
  ]
};
