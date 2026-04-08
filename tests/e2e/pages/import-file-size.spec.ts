import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Import File Size Validation', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
  });

  test('should reject import file exceeding 10MB size limit', async ({ page }) => {
    const largeBuffer = Buffer.alloc(11 * 1024 * 1024, 'a');
    const validJson = JSON.stringify({
      version: 1,
      exportDate: new Date().toISOString(),
      settings: { pomodoroMinutes: 25, shortBreakMinutes: 5, longBreakMinutes: 15 },
      activities: [],
      tasks: []
    });

    const oversizedBuffer = Buffer.concat([Buffer.from(validJson), largeBuffer]);

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'large-backup.json',
      mimeType: 'application/json',
      buffer: oversizedBuffer
    });
    await page.waitForTimeout(3000);

    const importResult = page.locator('.import-result');
    await expect(importResult).toBeVisible();
    await expect(importResult).toContainText('File too large');
    await expect(importResult).toContainText('10 MB');
  });
});
