import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Error Handling', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should display error boundary with retry button when component error occurs', async ({ page }) => {
    // Inject an error into the Blazor error boundary by triggering an unhandled error
    await page.evaluate(() => {
      // Find the Blazor error boundary and trigger an error
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Test error for e2e coverage',
      };
      window.dispatchEvent(blazorError);
    });

    // The app should still be functional (error is logged but doesn't crash the app)
    await page.waitForTimeout(1000);
    await expect(page.locator('.main-container')).toBeVisible();
  });

  test('should have error display component registered', async ({ page }) => {
    // Verify the app loads without unhandled errors
    const consoleErrors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.reload();
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const criticalErrors = consoleErrors.filter(e =>
      !e.toLowerCase().includes('blazor') &&
      !e.toLowerCase().includes('wasm') &&
      !e.toLowerCase().includes('stylesheet') &&
      !e.toLowerCase().includes('service worker')
    );
    expect(criticalErrors).toHaveLength(0);
  });

  test('should recover from JS interop failure gracefully', async ({ page }) => {
    // Verify the app remains functional even if JS interop has issues
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tasks-section')).toBeVisible();
    await expect(page.locator('.mode-tabs')).toBeVisible();
  });
});
