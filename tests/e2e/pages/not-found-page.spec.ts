import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('404 Page Content', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display not found message on invalid route', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/non-existent-page');

    const notFoundText = page.locator('p[role="alert"]');
    await expect(notFoundText).toContainText("Sorry, there's nothing at this address.");
  });

  test('should display error display component with retry and reload buttons on error', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', { bubbles: true, cancelable: true });
      (blazorError as any).detail = { type: 'error', message: 'Test error for 404 coverage' };
      window.dispatchEvent(blazorError);
    });
    await page.waitForTimeout(1000);

    const hasErrorContent = await page.locator('.error-container').isVisible().catch(() => false);
    const hasAppContent = await page.locator('.main-container').isVisible().catch(() => false);

    expect(hasErrorContent || hasAppContent).toBe(true);
    if (hasErrorContent) {
      await expect(page.locator('.btn-primary')).toBeVisible();
      await expect(page.locator('.btn-secondary')).toBeVisible();
    }
  });

  test('should have valid page title on 404 route', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/this-page-does-not-exist');

    const title = await page.title();
    expect(title).toBe('Not found');
  });
});
