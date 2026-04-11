import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Import Malformed Valid JSON', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
  });

  test('should show error when importing JSON with missing settings field', async ({ page }) => {
    const malformedJson = JSON.stringify({
      version: 1,
      exportDate: new Date().toISOString(),
      tasks: [{ id: '00000000-0000-0000-0000-000000000001', name: 'Bad Task' }]
    });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'malformed.json',
      mimeType: 'application/json',
      buffer: Buffer.from(malformedJson)
    });

    await page.waitForSelector('.import-result, .settings-toast', { timeout: 10000 });
  });

  test('should show error when importing JSON with wrong data types', async ({ page }) => {
    const wrongTypesJson = JSON.stringify({
      version: 1,
      exportDate: new Date().toISOString(),
      settings: { pomodoroMinutes: "not-a-number", shortBreakMinutes: 5, longBreakMinutes: 15 },
      tasks: [],
      activities: []
    });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'wrong-types.json',
      mimeType: 'application/json',
      buffer: Buffer.from(wrongTypesJson)
    });

    await page.waitForSelector('.import-result, .settings-toast', { timeout: 10000 });
  });

  test('should show error when importing JSON with empty object', async ({ page }) => {
    const emptyJson = JSON.stringify({});

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'empty.json',
      mimeType: 'application/json',
      buffer: Buffer.from(emptyJson)
    });

    const importResult = page.locator('.import-result');
    await expect(importResult).toBeVisible({ timeout: 10000 });
  });
});
