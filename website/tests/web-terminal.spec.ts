import { test, expect } from '@playwright/test';

test.describe('Web Terminal', () => {
  test('should load the homepage', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/Lablab Bean/i);
  });

  test('should have terminal component', async ({ page }) => {
    await page.goto('/');
    
    // Wait for terminal to be present
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
  });

  test('should allow terminal interaction', async ({ page }) => {
    await page.goto('/');
    
    // Wait for terminal
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    
    // Wait a bit for terminal to be ready
    await page.waitForTimeout(2000);
    
    // Try to type in terminal
    await terminal.click();
    await page.keyboard.type('echo "Hello from Playwright"');
    await page.keyboard.press('Enter');
    
    // Wait for command to execute
    await page.waitForTimeout(1000);
    
    // Check if terminal has content
    const terminalContent = await terminal.textContent();
    expect(terminalContent).toBeTruthy();
  });

  test('should connect to WebSocket', async ({ page }) => {
    // Listen for WebSocket connections
    const wsPromise = page.waitForEvent('websocket', { timeout: 10000 });
    
    await page.goto('/');
    
    const ws = await wsPromise;
    expect(ws.url()).toContain('terminal');
    
    // Wait for connection to be established
    await page.waitForTimeout(1000);
  });

  test('should handle terminal resize', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    
    // Change viewport size
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.waitForTimeout(500);
    
    // Terminal should still be visible
    await expect(terminal).toBeVisible();
  });

  test('should execute basic commands', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(2000);
    
    // Execute pwd command
    await terminal.click();
    await page.keyboard.type('pwd');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(1000);
    
    // Execute echo command
    await page.keyboard.type('echo "test"');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(1000);
    
    const content = await terminal.textContent();
    expect(content).toContain('test');
  });

  test('should handle multiple terminal sessions', async ({ page, context }) => {
    // Open first page
    await page.goto('/');
    const terminal1 = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal1).toBeVisible({ timeout: 10000 });
    
    // Open second page in new tab
    const page2 = await context.newPage();
    await page2.goto('/');
    const terminal2 = page2.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal2).toBeVisible({ timeout: 10000 });
    
    // Both should be functional
    await expect(terminal1).toBeVisible();
    await expect(terminal2).toBeVisible();
    
    await page2.close();
  });

  test('should persist terminal history', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(2000);
    
    // Type a command
    await terminal.click();
    await page.keyboard.type('echo "history test"');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(1000);
    
    // Use arrow up to recall history
    await page.keyboard.press('ArrowUp');
    await page.waitForTimeout(500);
    
    // The command should be recalled
    const content = await terminal.textContent();
    expect(content).toContain('history test');
  });
});

test.describe('Console App TUI Integration', () => {
  test('should display Terminal.Gui welcome screen', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    
    // Wait for console app to initialize
    await page.waitForTimeout(3000);
    
    // Check for Terminal.Gui title
    const content = await terminal.textContent();
    expect(content).toContain('Lablab Bean - Interactive TUI');
    expect(content).toContain('Welcome to Lablab Bean Interactive TUI');
  });

  test('should display console app features and commands', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    const content = await terminal.textContent();
    
    // Check for features
    expect(content).toContain('Features:');
    expect(content).toContain('Runs in browser via xterm.js');
    expect(content).toContain('Full keyboard support');
    expect(content).toContain('Mouse support');
    
    // Check for commands
    expect(content).toContain('Commands:');
    expect(content).toContain('ESC - Exit application');
    expect(content).toContain('F1  - Show help');
    expect(content).toContain('F5  - Refresh');
  });

  test('should display status bar with menu items', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    const content = await terminal.textContent();
    
    // Check for status bar items
    expect(content).toContain('ESC');
    expect(content).toContain('Quit');
    expect(content).toContain('F1');
    expect(content).toContain('Help');
    expect(content).toContain('F5');
    expect(content).toContain('Refresh');
  });

  test('should trigger F5 refresh and show timestamp', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    // Press F5 to refresh
    await terminal.click();
    await page.keyboard.press('F5');
    await page.waitForTimeout(1000);
    
    const content = await terminal.textContent();
    
    // Check for refresh message with timestamp
    expect(content).toMatch(/\[\d{2}:\d{2}:\d{2}\] View refreshed!/);
  });

  test('should show help dialog when F1 is pressed', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    // Press F1 to show help
    await terminal.click();
    await page.keyboard.press('F1');
    await page.waitForTimeout(1000);
    
    const content = await terminal.textContent();
    
    // Check for help dialog content
    expect(content).toContain('Help');
    expect(content).toContain('Keyboard Shortcuts:');
    expect(content).toContain('ESC - Quit application');
    expect(content).toContain('F1  - Show this help');
    expect(content).toContain('F5  - Refresh view');
    expect(content).toContain('Browser (via xterm.js)');
    expect(content).toContain('PTY session (node-pty)');
    expect(content).toContain('Managed by PM2');
  });

  test('should close help dialog with Enter', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    // Get initial content
    const initialContent = await terminal.textContent();
    
    // Press F1 to show help
    await terminal.click();
    await page.keyboard.press('F1');
    await page.waitForTimeout(1000);
    
    // Press Enter to close help dialog
    await page.keyboard.press('Enter');
    await page.waitForTimeout(500);
    
    const finalContent = await terminal.textContent();
    
    // Help dialog should be closed (content should be different)
    expect(finalContent).not.toBe(initialContent);
  });

  test('should handle multiple F5 refreshes', async ({ page }) => {
    await page.goto('/');
    
    const terminal = page.locator('.xterm, [data-testid="terminal"]');
    await expect(terminal).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(3000);
    
    await terminal.click();
    
    // Press F5 three times
    await page.keyboard.press('F5');
    await page.waitForTimeout(500);
    await page.keyboard.press('F5');
    await page.waitForTimeout(500);
    await page.keyboard.press('F5');
    await page.waitForTimeout(500);
    
    const content = await terminal.textContent();
    
    // Should have three refresh messages
    const refreshMatches = content.match(/View refreshed!/g);
    expect(refreshMatches).toBeTruthy();
    expect(refreshMatches?.length).toBeGreaterThanOrEqual(3);
  });
});

test.describe('Web App Navigation', () => {
  test('should have working navigation', async ({ page }) => {
    await page.goto('/');
    
    // Check for common navigation elements
    const nav = page.locator('nav, header');
    await expect(nav).toBeVisible({ timeout: 5000 });
  });

  test('should be responsive', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await expect(page).toHaveTitle(/Lablab Bean/i);
    
    // Test tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/');
    await expect(page).toHaveTitle(/Lablab Bean/i);
    
    // Test desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    await expect(page).toHaveTitle(/Lablab Bean/i);
  });
});
