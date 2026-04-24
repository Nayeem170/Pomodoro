import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('BroadcastChannel Cross-Tab Sync', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should have BroadcastChannel API available in browser', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const hasBroadcastChannel = await page.evaluate(() => {
      return typeof BroadcastChannel !== 'undefined';
    });
    expect(hasBroadcastChannel).toBe(true);
  });

  test('should not crash when creating a BroadcastChannel', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const created = await page.evaluate(() => {
      try {
        const channel = new BroadcastChannel('test-pomodoro-sync');
        channel.close();
        return true;
      } catch {
        return false;
      }
    });
    expect(created).toBe(true);
  });

  test('should handle pipclosed event without crashing', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      window.dispatchEvent(new Event('pipclosed'));
    });
    await page.waitForTimeout(1000);

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('.mode-tabs')).toBeVisible();
  });
});
