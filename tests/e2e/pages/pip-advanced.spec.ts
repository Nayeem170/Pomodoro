import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('PiP Advanced', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should have BroadcastChannel API available', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const hasBroadcastChannel = await page.evaluate(() => typeof BroadcastChannel !== 'undefined');
    expect(hasBroadcastChannel).toBe(true);
  });

  test('should have pip script loaded or available', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const hasPipFunctions = await page.evaluate(() => {
      return typeof (window as any).pipFunctions === 'object';
    }).catch(() => false);

    const hasPipScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script[src*="pip"]'));
      return scripts.length > 0;
    }).catch(() => false);

    expect(hasPipFunctions || hasPipScript).toBe(true);
  });

  test('should not affect timer state when PiP is toggled', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const timerBefore = await pomodoroPage.getTimerDisplay();

    const pipBtn = page.locator('button[title*="floating timer"]');
    await pipBtn.click();
    await page.waitForTimeout(1000);

    const timerAfter = await pomodoroPage.getTimerDisplay();
    expect(timerAfter).toBe(timerBefore);
  });

  test('should handle pipclosed event gracefully', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      window.dispatchEvent(new CustomEvent('pipclosed'));
    });
    await page.waitForTimeout(1000);

    await expect(page.locator('.timer-section')).toBeVisible();
    await expect(page.locator('.session-tabs')).toBeVisible();
  });
});
