const fs = require('fs');
const path = require('path');

// Find the latest version in build/_artifacts
function getLatestVersion() {
  const artifactsDir = path.join(__dirname, '..', 'build', '_artifacts');
  
  if (!fs.existsSync(artifactsDir)) {
    console.error('❌ No artifacts directory found. Please run: task build-release');
    process.exit(1);
  }
  
  const versions = fs.readdirSync(artifactsDir)
    .filter(f => {
      const fullPath = path.join(artifactsDir, f);
      return fs.statSync(fullPath).isDirectory() && f !== '.gitkeep';
    })
    .sort()
    .reverse();
  
  if (versions.length === 0) {
    console.error('❌ No version artifacts found. Please run: task build-release');
    process.exit(1);
  }
  
  return versions[0];
}

// Validate artifacts exist
function validateArtifacts(artifactsPath) {
  const requiredPaths = [
    path.join(artifactsPath, 'console', 'LablabBean.Console.exe'),
    path.join(artifactsPath, 'website', 'package.json')
  ];
  
  const missing = requiredPaths.filter(p => !fs.existsSync(p));
  
  if (missing.length > 0) {
    console.error('❌ Missing required artifacts:');
    missing.forEach(p => console.error(`   - ${p}`));
    console.error('\nPlease run: task build-release');
    process.exit(1);
  }
}

const version = process.env.LABLAB_VERSION || getLatestVersion();
const artifactsPath = path.join(__dirname, '..', 'build', '_artifacts', version, 'publish');
const logsPath = path.join(__dirname, '..', 'build', '_artifacts', version, 'logs');

// Ensure logs directory exists
if (!fs.existsSync(logsPath)) {
  fs.mkdirSync(logsPath, { recursive: true });
}

console.log(`✓ Using version: ${version}`);
console.log(`✓ Artifacts path: ${artifactsPath}`);
console.log(`✓ Logs path: ${logsPath}`);

// Validate before starting
validateArtifacts(artifactsPath);

module.exports = {
  apps: [
    {
      name: 'lablab-web',
      script: path.join(artifactsPath, 'website', 'server', 'entry.mjs'),
      cwd: path.join(artifactsPath, 'website'),
      watch: false,
      instances: 1,
      exec_mode: 'fork',
      env: {
        NODE_ENV: 'production',
        HOST: '0.0.0.0',
        PORT: 3000,
        LABLAB_VERSION: version
      },
      error_file: path.join(logsPath, 'web-error.log'),
      out_file: path.join(logsPath, 'web-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    },
    {
      name: 'lablab-console',
      script: path.join(artifactsPath, 'console', 'LablabBean.Console.exe'),
      cwd: path.join(artifactsPath, 'console'),
      watch: false,
      instances: 1,
      exec_mode: 'fork',
      env: {
        DOTNET_ENVIRONMENT: 'Production',
        LABLAB_VERSION: version
      },
      error_file: path.join(logsPath, 'console-error.log'),
      out_file: path.join(logsPath, 'console-out.log'),
      log_date_format: 'YYYY-MM-DD HH:mm:ss Z'
    }
  ]
};
