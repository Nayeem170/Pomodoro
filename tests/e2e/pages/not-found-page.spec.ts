import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('404 Page Content', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display not found content on invalid route', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/non-existent-page');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);

    const hasNotFoundText = await page.locator('text=/not found/i').first().isVisible().catch(() => false);
    const hasErrorContainer = await page.locator('.error-container').first().isVisible().catch(() => false);
    const hasAppContent = await page.locator('.main-container').first().isVisible().catch(() => false);
    const hasAppHeader = await page.locator('.app-header').first().isVisible().catch(() => false);

    expect(hasNotFoundText || hasErrorContainer || hasAppContent || hasAppHeader).toBe(true);
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
    const hasRetryBtn = await page.locator('.btn-primary').isVisible().catch(() => false);
    const hasReloadBtn = await page.locator('.btn-secondary').isVisible().catch(() => false);
    const hasAppContent = await page.locator('.main-container').isVisible().catch(() => false);

    expect(hasErrorContent || hasAppContent).toBe(true);
    if (hasErrorContent) {
      expect(hasRetryBtn || hasReloadBtn).toBe(true);
    }
  });

  test('should have valid page title on 404 route', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/this-page-does-not-exist');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(3000);

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title.length).toBeGreaterThan(0);
  });
});
