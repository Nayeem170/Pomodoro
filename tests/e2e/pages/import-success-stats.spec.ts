import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Import Success Statistics', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
  });

  test('should show imported records count on first import', async ({ page }) => {
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
      tasks: [
        { id: '00000000-0000-0000-0000-000000000001', name: 'Stat Task A', createdAt: '2026-04-05T12:00:00Z', isCompleted: false, totalFocusMinutes: 0, pomodoroCount: 0, isDeleted: false },
        { id: '00000000-0000-0000-0000-000000000002', name: 'Stat Task B', createdAt: '2026-04-05T12:01:00Z', isCompleted: false, totalFocusMinutes: 0, pomodoroCount: 0, isDeleted: false }
      ]
    });

    const fileInput = page.locator('.file-input');
    await fileInput.setInputFiles({
      name: 'stats-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(backupJson)
    });
    await page.waitForTimeout(3000);

    const toast = page.locator('.settings-toast');
    await expect(toast).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);
    await expect(toast).toContainText('imported');
    await expect(toast).toContainText('records');
  });

  test('should show skipped duplicates count on re-import', async ({ page }) => {
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
      tasks: [
        { id: '00000000-0000-0000-0000-000000000001', name: 'Dup Stat Task', createdAt: '2026-04-05T12:00:00Z', isCompleted: false, totalFocusMinutes: 0, pomodoroCount: 0, isDeleted: false }
      ]
    });

    const fileInput = page.locator('.file-input');

    await fileInput.setInputFiles({
      name: 'dup-stats-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(backupJson)
    });
    await page.waitForTimeout(3000);

    await fileInput.setInputFiles({
      name: 'dup-stats-backup.json',
      mimeType: 'application/json',
      buffer: Buffer.from(backupJson)
    });
    await page.waitForTimeout(3000);

    const toast = page.locator('.settings-toast');
    await expect(toast).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);
    await expect(toast).toContainText('skipped');
    await expect(toast).toContainText('duplicates');
  });
});
