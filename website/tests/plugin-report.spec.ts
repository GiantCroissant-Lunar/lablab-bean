import { test, expect } from "@playwright/test";
import fs from "node:fs";
import path from "node:path";

function findLatestReport(baseDir: string): string | null {
  if (!fs.existsSync(baseDir)) return null;
  const files = fs
    .readdirSync(baseDir)
    .filter((f) => f.startsWith("plugin-health") && f.endsWith(".html"))
    .map((f) => path.join(baseDir, f))
    .sort((a, b) => fs.statSync(b).mtimeMs - fs.statSync(a).mtimeMs);
  return files[0] ?? null;
}

test("plugin report shows version and contract probes", async ({ page }) => {
  const verifyDir = path.join(
    process.cwd(),
    "build",
    "_artifacts",
    "verify_all",
  );
  const reportPath = findLatestReport(verifyDir);
  expect(
    reportPath,
    "Expected a generated plugin-health HTML report",
  ).not.toBeNull();
  await page.goto("file://" + reportPath!.replace(/\\/g, "/"));

  // Version appears in header subtitle when available
  const header = page.locator(".header");
  await expect(header).toContainText("Plugin Health Report");
  // Version text is optional; assert if present that label exists
  const hasVersion = await header.locator("text=Version:").count();
  if (hasVersion > 0) {
    await expect(header.locator("text=Version:")).toBeVisible();
  }

  // Contract probe badges render when ContractProbes present
  // At least ensure the CSS class exists somewhere
  const anyBadge = page.locator(".badge");
  await expect(anyBadge.first()).toBeVisible();
});
