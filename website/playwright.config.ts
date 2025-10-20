import { defineConfig, devices } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

// Get version from environment or find latest
function getVersion(): string {
  if (process.env.LABLAB_VERSION) {
    return process.env.LABLAB_VERSION;
  }
  
  const artifactsDir = path.join(__dirname, '..', 'build', '_artifacts');
  if (!fs.existsSync(artifactsDir)) {
    throw new Error('No artifacts directory found. Run: task build-release');
  }
  
  const versions = fs.readdirSync(artifactsDir)
    .filter(f => fs.statSync(path.join(artifactsDir, f)).isDirectory() && f !== '.gitkeep')
    .sort()
    .reverse();
  
  if (versions.length === 0) {
    throw new Error('No version artifacts found. Run: task build-release');
  }
  
  return versions[0];
}

const version = getVersion();
const testResultsDir = path.join(__dirname, '..', 'build', '_artifacts', version, 'test-results');
const testReportsDir = path.join(__dirname, '..', 'build', '_artifacts', version, 'test-reports');

// Ensure directories exist
[testResultsDir, testReportsDir].forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

console.log(`📦 Testing version: ${version}`);
console.log(`📊 Test results: ${testResultsDir}`);
console.log(`📋 Test reports: ${testReportsDir}`);

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  
  reporter: [
    ['html', { outputFolder: path.join(testReportsDir, 'html'), open: 'never' }],
    ['json', { outputFile: path.join(testReportsDir, 'results.json') }],
    ['junit', { outputFile: path.join(testReportsDir, 'junit.xml') }],
    ['list']
  ],
  
  outputDir: testResultsDir,
  
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chrome',
      use: { 
        ...devices['Desktop Chrome'],
        channel: 'chrome', // Use installed Chrome instead of Chromium
      },
    },
  ],

  webServer: process.env.SKIP_WEBSERVER ? undefined : {
    command: 'pm2 start ecosystem.production.config.js && pm2 logs --nostream',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
    cwd: path.join(__dirname),
  },
});
