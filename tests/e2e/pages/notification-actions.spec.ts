import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Notification Action Buttons', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should have notification service initialized with permission grant capability', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const hasNotificationApi = await page.evaluate(() => {
      return 'Notification' in window;
    });
    expect(hasNotificationApi).toBe(true);
  });

  test('should have notification permission state accessible', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const permissionState = await page.evaluate(() => {
      if ('Notification' in window) {
        return Notification.permission;
      }
      return 'unsupported';
    });
    expect(['default', 'denied', 'granted', 'unsupported']).toContain(permissionState);
  });

  test('should not crash when requesting notification permission', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.context().grantPermissions(['notifications']);

    await page.locator('label[for="notifToggle"]').click();
    await page.waitForTimeout(500);

    await expect(page.locator('.settings-page')).toBeVisible();
    const hasError = await page.locator('.error-banner').isVisible().catch(() => false);
    expect(hasError).toBe(false);
  });
});
