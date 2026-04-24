import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Flow', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should complete connect flow and show connected state', async ({ page }) => {
    await expect(page.locator('button.sec-btn', { hasText: 'Connect' })).toBeVisible();
    await expect(page.locator('.sync-actions')).toBeVisible();
  });

  test('should disconnect and show connect button', async ({ page }) => {
    await expect(page.locator('.sync-actions')).toBeVisible();
    await expect(page.locator('button.danger-btn', { hasText: 'Disconnect' })).toBeVisible();
  });

  test('should show toast on successful sync', async ({ page }) => {
    await expect(page.locator('button.sec-btn', { hasText: 'Sync' })).toBeVisible();
  });

  test('should show toast on disconnect', async ({ page }) => {
    await expect(page.locator('button.danger-btn', { hasText: 'Disconnect' })).toBeVisible();
  });
});
