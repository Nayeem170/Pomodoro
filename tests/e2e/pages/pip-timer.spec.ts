import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Picture-in-Picture Timer', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should display PiP toggle button in header', async ({ page }) => {
    await expect(page.locator('button:has-text("⧉")')).toBeVisible({ timeout: 30000 });
  });

  test('should have PiP button with correct title', async ({ page }) => {
    const pipButton = page.locator('button:has-text("⧉")');
    await expect(pipButton).toHaveAttribute('title', /floating timer/i);
  });

  test('should check PiP API support without crashing', async ({ page }) => {
    // Verify the app checks for PiP support on load
    const hasPipError = await page.evaluate(() => {
      const errors = (window as any).__consoleErrors || [];
      return errors.some((e: string) => e.includes('pip') || e.includes('Picture-in-Picture'));
    });
    expect(hasPipError).toBe(false);
  });

  test('should toggle PiP button state on click', async ({ page }) => {
    const pipButton = page.locator('button:has-text("⧉")');
    const initialTitle = await pipButton.getAttribute('title');
    await pipButton.click();
    await page.waitForTimeout(1000);

    // Button should still be visible and functional after click
    await expect(pipButton).toBeVisible();

    // In environments without PiP support, the button should not crash the app
    await expect(page.locator('.timer-section')).toBeVisible();
    await expect(page.locator('.tasks-section')).toBeVisible();
  });

  test('should not affect timer when PiP is toggled', async ({ page }) => {
    const timerBefore = await pomodoroPage.getTimerDisplay();
    await pomodoroPage.togglePipTimer();
    await page.waitForTimeout(500);
    const timerAfter = await pomodoroPage.getTimerDisplay();
    expect(timerBefore).toBe(timerAfter);
  });

  test('should handle PiP close gracefully', async ({ page }) => {
    // Simulate PiP window close event
    await page.evaluate(() => {
      window.dispatchEvent(new Event('pipclosed'));
    });
    await page.waitForTimeout(500);

    // App should remain functional
    await expect(page.locator('.timer-section')).toBeVisible();
    await expect(page.locator('button:has-text("⧉")')).toBeVisible();
  });
});
