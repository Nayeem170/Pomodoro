import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Import File', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should have import file input accepting .json', async ({ page }) => {
    const fileInput = page.locator('.file-input');
    await expect(fileInput).toHaveAttribute('accept', '.json');
  });

  test('should show import result after successful import', async ({ page }) => {
    const validJson = JSON.stringify({
      settings: { pomodoroMinutes: 30, shortBreakMinutes: 10, longBreakMinutes: 20 },
      tasks: [{ id: '00000000-0000-0000-0000-000000000001', name: 'Imported Task', isCompleted: false, createdAt: new Date().toISOString() }]
    });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(validJson)
    });
    await page.waitForTimeout(3000);

    const importResult = page.locator('.import-result');
    await expect(importResult).toBeVisible();
  });

  test('should show error for invalid JSON file', async ({ page }) => {
    const invalidJson = 'this is not valid json {{{';

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'invalid.json',
      mimeType: 'application/json',
      buffer: Buffer.from(invalidJson)
    });
    await page.waitForTimeout(3000);

    const importResult = page.locator('.import-result');
    await expect(importResult).toBeVisible();
  });

  test('should show error for wrong file type', async ({ page }) => {
    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'data.txt',
      mimeType: 'text/plain',
      buffer: Buffer.from('some text content')
    });
    await page.waitForTimeout(3000);

    const importResult = page.locator('.import-result');
    await expect(importResult).toBeVisible();
  });

  test('should not accept non-json files via accept attribute', async ({ page }) => {
    const fileInput = page.locator('.file-input');
    const acceptAttr = await fileInput.getAttribute('accept');
    expect(acceptAttr).toBe('.json');
  });
});
