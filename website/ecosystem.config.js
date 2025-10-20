module.exports = {
  apps: [
    {
      name: 'lablab-web',
      script: 'pnpm',
      args: 'dev',
      cwd: './',
      watch: false,
      env: {
        NODE_ENV: 'development',
        PORT: 3000
      }
    },
    {
      name: 'lablab-console-tui',
      script: 'dotnet',
      args: 'run',
      cwd: '../dotnet/console-app/LablabBean.Console',
      watch: false,
      env: {
        DOTNET_ENVIRONMENT: 'Development'
      }
    }
  ]
};
