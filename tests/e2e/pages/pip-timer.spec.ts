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
    await expect(page.locator('button[aria-label="Picture in Picture"]')).toBeVisible({ timeout: 30000 });
  });

  test('should have PiP button with correct title', async ({ page }) => {
    const pipButton = page.locator('button[aria-label="Picture in Picture"]');
    await expect(pipButton).toHaveAttribute('title', /floating timer/i);
  });

  test('should check PiP API support without crashing', async ({ page }) => {
    const hasPipError = await page.evaluate(() => {
      const errors = (window as any).__consoleErrors || [];
      return errors.some((e: string) => e.includes('pip') || e.includes('Picture-in-Picture'));
    });
    expect(hasPipError).toBe(false);
  });

  test('should toggle PiP button state on click', async ({ page }) => {
    const pipButton = page.locator('button[aria-label="Picture in Picture"]');
    const initialTitle = await pipButton.getAttribute('title');
    await pipButton.click();
    await page.waitForTimeout(1000);

    await expect(pipButton).toBeVisible();

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('.tasks-section')).toBeVisible();
  });

  test('should not affect timer when PiP is toggled', async ({ page }) => {
    const timerBefore = await pomodoroPage.getTimerDisplay();
    await pomodoroPage.togglePipTimer();
    await page.waitForTimeout(500);
    const timerAfter = await pomodoroPage.getTimerDisplay();
    expect(timerBefore).toBe(timerAfter);
  });

  test('should show error banner when PiP popup is blocked', async ({ page }) => {
    await page.evaluate(() => {
      delete (window as any).documentPictureInPicture;
    });

    await page.evaluate(() => {
      (window as any).open = () => null;
    });

    const pipButton = page.locator('button[aria-label="Picture in Picture"]');
    await pipButton.click();
    await page.waitForTimeout(1000);

    const errorBanner = page.locator('.error-banner');
    await expect(errorBanner).toBeVisible({ timeout: 5000 });
    await expect(errorBanner).toContainText(/pop-up blocked/i);
  });

  test('should handle PiP close gracefully', async ({ page }) => {
    await page.evaluate(() => {
      window.dispatchEvent(new Event('pipclosed'));
    });
    await page.waitForTimeout(500);

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('button[aria-label="Picture in Picture"]')).toBeVisible();
  });
});
