import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Import Duplicate Detection', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should not duplicate tasks when importing the same backup twice', async ({ page }) => {
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const backupJson = JSON.stringify({
      version: 1,
      exportDate: '2026-04-05T12:00:00Z',
      settings: {
        pomodoroMinutes: 25,
        shortBreakMinutes: 5,
        longBreakMinutes: 15,
        soundEnabled: true,
        notificationsEnabled: true,
        autoStartEnabled: true,
        autoStartDelaySeconds: 5
      },
      activities: [],
      tasks: [{
        id: '00000000-0000-0000-0000-000000000001',
        name: 'Dup Test Task',
        createdAt: '2026-04-05T12:00:00Z',
        isCompleted: false,
        totalFocusMinutes: 0,
        pomodoroCount: 0,
        isDeleted: false
      }]
    });

    const importInput = page.locator('input[type="file"][accept=".json"]');

    await importInput.setInputFiles({
      name: 'pomodoro-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(backupJson)
    });
    await page.waitForTimeout(3000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const countAfterFirst = await page.locator('.task-item').count();
    expect(countAfterFirst).toBe(1);
    await expect(page.locator('.task-item')).toContainText('Dup Test Task');

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await importInput.setInputFiles({
      name: 'pomodoro-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(backupJson)
    });
    await page.waitForTimeout(3000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const countAfterSecond = await page.locator('.task-item').count();
    expect(countAfterSecond).toBe(1);
  });
});
