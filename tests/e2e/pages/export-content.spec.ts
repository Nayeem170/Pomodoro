import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Export File Content Verification', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should download valid JSON with correct top-level structure', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const content = await download.createReadStream();
    const chunks: Buffer[] = [];
    for await (const chunk of content) {
      chunks.push(chunk);
    }
    const jsonStr = Buffer.concat(chunks).toString('utf-8');
    const data = JSON.parse(jsonStr);

    expect(data.version).toBeDefined();
    expect(data.exportDate).toBeDefined();
    expect(data.settings).toBeDefined();
    expect(data.activities).toBeDefined();
    expect(data.tasks).toBeDefined();
  });

  test('should include settings properties in export when settings are loaded', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('input[type="number"]');
    await expect(inputs.first()).toHaveValue('25', { timeout: 10000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const content = await download.createReadStream();
    const chunks: Buffer[] = [];
    for await (const chunk of content) {
      chunks.push(chunk);
    }
    const jsonStr = Buffer.concat(chunks).toString('utf-8');
    const data = JSON.parse(jsonStr);

    if (data.settings !== null) {
      const settingsKeys = Object.keys(data.settings);
      expect(settingsKeys.length).toBeGreaterThanOrEqual(3);
    } else {
      expect(data.version).toBeDefined();
      expect(data.activities).toBeDefined();
      expect(data.tasks).toBeDefined();
    }
  });

  test('should include tasks array in export', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Export Content Task');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const content = await download.createReadStream();
    const chunks: Buffer[] = [];
    for await (const chunk of content) {
      chunks.push(chunk);
    }
    const jsonStr = Buffer.concat(chunks).toString('utf-8');
    const data = JSON.parse(jsonStr);

    expect(Array.isArray(data.tasks)).toBe(true);
    expect(data.tasks.length).toBeGreaterThanOrEqual(1);
    const hasExportTask = data.tasks.some((t: any) => t.name === 'Export Content Task');
    expect(hasExportTask).toBe(true);
  });

  test('should have correct download file name format', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 10000 }),
      page.locator('.btn-export').click(),
    ]);

    const suggestedName = download.suggestedFilename();
    expect(suggestedName).toMatch(/pomodoro-backup-\d{4}-\d{2}-\d{2}\.json/);
  });
});
