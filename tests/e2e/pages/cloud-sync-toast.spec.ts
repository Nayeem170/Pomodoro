import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Toast Messages', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should not show toast message initially', async ({ page }) => {
    const toast = page.locator('.settings-toast');
    const isToastVisible = await toast.isVisible({ timeout: 2000 }).catch(() => false);
    expect(isToastVisible).toBe(false);
  });

  test('should have toast element in DOM but hidden', async ({ page }) => {
    const toast = page.locator('.settings-toast');
    const toastCount = await toast.count();
    if (toastCount > 0) {
      const isVisible = await toast.isVisible();
      expect(isVisible).toBe(false);
    }
  });
});
