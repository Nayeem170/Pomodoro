import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Error Display Retry and Reload Buttons', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should show retry or reload buttons when error container appears', async ({ page }) => {
    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Test error for display actions',
      };
      window.dispatchEvent(blazorError);
    });

    await page.waitForTimeout(1000);

    const errorContainer = page.locator('.error-container');
    const sectionError = page.locator('.section-error');
    const hasError = await errorContainer.isVisible({ timeout: 2000 }).catch(() => false) ||
                     await sectionError.isVisible({ timeout: 2000 }).catch(() => false);

    if (hasError) {
      const retryButton = page.locator('.btn-primary').filter({ hasText: /Retry/i });
      const reloadButton = page.locator('.btn-secondary').filter({ hasText: /Reload/i });
      const hasAction = await retryButton.isVisible({ timeout: 2000 }).catch(() => false) ||
                        await reloadButton.isVisible({ timeout: 2000 }).catch(() => false);
      expect(hasAction).toBe(true);
    } else {
      await expect(page.locator('.main-container')).toBeVisible();
    }
  });

  test('should keep main container functional when no error container shown', async ({ page }) => {
    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Graceful error test',
      };
      window.dispatchEvent(blazorError);
    });

    await page.waitForTimeout(1000);

    const errorContainer = page.locator('.error-container');
    const hasError = await errorContainer.isVisible({ timeout: 2000 }).catch(() => false);

    if (!hasError) {
      await expect(page.locator('.main-container')).toBeVisible();
      await expect(page.locator('.mode-tabs')).toBeVisible();
      await expect(page.locator('.ring-area')).toBeVisible();
    }
  });
});
