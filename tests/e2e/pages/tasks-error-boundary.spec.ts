import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Tasks Error Boundary Retry', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should keep tasks section functional after blazor error event', async ({ page }) => {
    await expect(page.locator('.task-card')).toBeVisible();

    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Test error for tasks error boundary',
      };
      window.dispatchEvent(blazorError);
    });

    await page.waitForTimeout(1000);

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('.task-card')).toBeVisible();
  });

  test('should remain interactive after blazor error event', async ({ page }) => {
    await expect(page.locator('.task-card')).toBeVisible();

    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Test error for boundary recovery',
      };
      window.dispatchEvent(blazorError);
    });

    await page.waitForTimeout(1000);

    await expect(page.locator('.task-add-btn')).toBeVisible();
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.task-input')).toBeVisible();
  });
});
