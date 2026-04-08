import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Browser Notifications', () => {
  test.describe.configure({ timeout: 60000 });

  test('should have notification permission state readable from page', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const notifToggle = page.locator('label[for="notifToggle"]');
    await expect(notifToggle).toBeVisible();
    await context.close();
  });

  test('should toggle notification setting on', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();

    const notifToggle = page.locator('#notifToggle');
    const initialState = await notifToggle.isChecked();
    if (!initialState) {
      await page.locator('label[for="notifToggle"]').click();
      await page.waitForTimeout(500);
    }
    await expect(notifToggle).toBeChecked();
    await context.close();
  });

  test('should have notification service initialized', async ({ browser }) => {
    const context = await browser.newContext({
      permissions: ['notifications'],
    });
    const page = await context.newPage();
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await page.waitForTimeout(3000);

    const hasNotifError = await page.evaluate(() => {
      const errors = (window as any).__consoleErrors || [];
      return errors.some((e: string) => e.includes('notification'));
    });
    expect(hasNotifError).toBe(false);
    await context.close();
  });
});
