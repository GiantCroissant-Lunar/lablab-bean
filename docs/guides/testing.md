---
doc_id: DOC-2025-00032
title: Testing Guide
doc_type: guide
status: active
canonical: true
created: 2025-10-20
tags: [testing, playwright, e2e, automation]
summary: >
  Complete guide for end-to-end testing with Playwright for the
  web terminal interface.
---

# Testing Guide

Complete guide for testing the Lablab Bean stack with Playwright.

## Overview

The project uses Playwright for end-to-end testing of the web terminal interface. All test results are stored in versioned artifact directories.

## Prerequisites

- Node.js 18+
- pnpm 8+
- Stack must be built and running

## Quick Start

```bash
# Install Playwright browsers (first time only)
task test-install

# Build and start the stack
task build-release
task stack-run

# Run tests
task test-web

# View test report
task test-report
```

## Test Commands

### Basic Testing

```bash
# Run all tests
task test-web

# Run tests in UI mode (interactive)
task test-web-ui

# Run tests in headed mode (see browser)
task test-web-headed

# Debug tests (step through)
task test-web-debug
```

### Complete Workflow

```bash
# Build, start, test, and report (all-in-one)
task test-full
```

This command will:

1. Build the release
2. Start the stack
3. Wait for services to be ready
4. Run all Playwright tests
5. Generate test reports
6. Show report location

### View Reports

```bash
# Open HTML test report in browser
task test-report
```

## Test Structure

### Test Files

Tests are located in `website/tests/`:

```
website/tests/
└── web-terminal.spec.ts    # Web terminal E2E tests
```

### Test Configuration

Configuration is in `website/playwright.config.ts`:

```typescript
{
  testDir: './tests',
  outputDir: 'build/_artifacts/<version>/test-results',
  reporter: [
    ['html', { outputFolder: 'build/_artifacts/<version>/test-reports/html' }],
    ['json', { outputFile: 'build/_artifacts/<version>/test-reports/results.json' }],
    ['junit', { outputFile: 'build/_artifacts/<version>/test-reports/junit.xml' }]
  ]
}
```

## Test Coverage

### Web Terminal Tests

**Basic Functionality:**

- ✅ Homepage loads correctly
- ✅ Terminal component is visible
- ✅ Terminal accepts user input
- ✅ WebSocket connection established
- ✅ Terminal handles resize events

**Command Execution:**

- ✅ Execute `pwd` command
- ✅ Execute `echo` command
- ✅ Command output is displayed
- ✅ Terminal history works (arrow up)

**Advanced Features:**

- ✅ Multiple terminal sessions
- ✅ Terminal persistence
- ✅ Responsive design (mobile, tablet, desktop)

### Navigation Tests

- ✅ Navigation elements present
- ✅ Responsive across viewports

## Test Output

### Directory Structure

```
build/_artifacts/<version>/
├── test-results/          # Test artifacts
│   ├── screenshots/       # Failure screenshots
│   ├── videos/           # Test videos
│   └── traces/           # Playwright traces
└── test-reports/         # Test reports
    ├── html/             # Interactive HTML report
    │   └── index.html
    ├── results.json      # JSON results
    └── junit.xml         # JUnit XML
```

### Report Types

**HTML Report** (`test-reports/html/index.html`):

- Interactive web interface
- Test results with screenshots
- Video recordings
- Trace viewer
- Filter and search capabilities

**JSON Report** (`test-reports/results.json`):

- Machine-readable format
- Complete test results
- Timing information
- Error details

**JUnit XML** (`test-reports/junit.xml`):

- CI/CD integration format
- Compatible with Jenkins, GitLab CI, etc.
- Test suite and case information

## Writing Tests

### Test Template

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do something', async ({ page }) => {
    await page.goto('/');

    // Your test code here
    const element = page.locator('selector');
    await expect(element).toBeVisible();
  });
});
```

### Best Practices

1. **Use descriptive test names**

   ```typescript
   test('should execute echo command and display output', async ({ page }) => {
     // ...
   });
   ```

2. **Wait for elements properly**

   ```typescript
   await expect(terminal).toBeVisible({ timeout: 10000 });
   ```

3. **Add appropriate timeouts**

   ```typescript
   await page.waitForTimeout(2000); // For terminal to be ready
   ```

4. **Use data-testid attributes**

   ```typescript
   const terminal = page.locator('[data-testid="terminal"]');
   ```

5. **Clean up after tests**

   ```typescript
   test.afterEach(async ({ page }) => {
     await page.close();
   });
   ```

## Debugging Tests

### UI Mode

Run tests in interactive UI mode:

```bash
task test-web-ui
```

Features:

- Watch mode
- Time travel debugging
- Pick locators
- View traces

### Headed Mode

See the browser while tests run:

```bash
task test-web-headed
```

### Debug Mode

Step through tests with debugger:

```bash
task test-web-debug
```

Features:

- Pause execution
- Step through code
- Inspect elements
- Console access

### Trace Viewer

View recorded traces:

```bash
npx playwright show-trace build/_artifacts/<version>/test-results/trace.zip
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Install pnpm
        run: npm install -g pnpm

      - name: Install dependencies
        run: pnpm install
        working-directory: website

      - name: Install Playwright browsers
        run: pnpm exec playwright install --with-deps
        working-directory: website

      - name: Build release
        run: task build-release

      - name: Start stack
        run: task stack-run

      - name: Run tests
        run: task test-web

      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: build/_artifacts/*/test-reports/
```

### GitLab CI Example

```yaml
test:
  stage: test
  script:
    - task test-install
    - task build-release
    - task stack-run
    - task test-web
  artifacts:
    when: always
    paths:
      - build/_artifacts/*/test-reports/
    reports:
      junit: build/_artifacts/*/test-reports/junit.xml
```

## Troubleshooting

### Tests Failing

**Stack not running:**

```bash
task stack-status
pm2 status
```

**Web app not accessible:**

```bash
curl http://localhost:3000
```

**Port already in use:**

```bash
# Stop existing stack
task stack-stop

# Or kill process on port 3000
# Windows:
netstat -ano | findstr :3000
taskkill /PID <PID> /F

# Linux/Mac:
lsof -ti:3000 | xargs kill -9
```

### Browsers Not Installed

```bash
task test-install
# or
cd website
pnpm exec playwright install
```

### Tests Timeout

Increase timeout in `playwright.config.ts`:

```typescript
use: {
  timeout: 30000, // 30 seconds
}
```

### Flaky Tests

Add retry logic:

```typescript
test.describe.configure({ retries: 2 });
```

Or in config:

```typescript
retries: process.env.CI ? 2 : 0,
```

### Screenshots Not Captured

Ensure screenshot setting in config:

```typescript
use: {
  screenshot: 'only-on-failure',
}
```

## Environment Variables

### Test Configuration

```bash
# Use specific version
$env:LABLAB_VERSION = "0.1.0-alpha.1"

# Skip web server startup (if already running)
$env:SKIP_WEBSERVER = "true"

# Change base URL
$env:BASE_URL = "http://localhost:3000"

# CI mode
$env:CI = "true"
```

### Example Usage

```bash
# Test against specific version
$env:LABLAB_VERSION = "0.1.0-alpha.1"
task test-web

# Test with existing server
$env:SKIP_WEBSERVER = "true"
task test-web
```

## Performance Testing

### Measuring Load Time

```typescript
test('should load quickly', async ({ page }) => {
  const start = Date.now();
  await page.goto('/');
  const loadTime = Date.now() - start;

  expect(loadTime).toBeLessThan(3000); // 3 seconds
});
```

### Network Throttling

```typescript
test('should work on slow connection', async ({ page, context }) => {
  await context.route('**/*', route => {
    setTimeout(() => route.continue(), 100); // 100ms delay
  });

  await page.goto('/');
  // Test functionality
});
```

## Accessibility Testing

### Basic A11y Checks

```typescript
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test('should not have accessibility violations', async ({ page }) => {
  await page.goto('/');

  const accessibilityScanResults = await new AxeBuilder({ page })
    .analyze();

  expect(accessibilityScanResults.violations).toEqual([]);
});
```

## Visual Regression Testing

### Screenshot Comparison

```typescript
test('should match visual snapshot', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveScreenshot('homepage.png');
});
```

## Best Practices Summary

1. ✅ Run tests in CI/CD pipeline
2. ✅ Store test results in versioned artifacts
3. ✅ Use multiple reporters (HTML, JSON, JUnit)
4. ✅ Add retries for flaky tests
5. ✅ Capture screenshots and videos on failure
6. ✅ Use trace viewer for debugging
7. ✅ Test across multiple browsers
8. ✅ Test responsive design
9. ✅ Include accessibility tests
10. ✅ Monitor test performance

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Playwright CI/CD](https://playwright.dev/docs/ci)
- [Playwright Trace Viewer](https://playwright.dev/docs/trace-viewer)

## See Also

- [QUICKSTART.md](QUICKSTART.md) - Quick start guide
- [RELEASE.md](RELEASE.md) - Release & deployment guide
- [ORGANIZATION.md](ORGANIZATION.md) - Project organization
