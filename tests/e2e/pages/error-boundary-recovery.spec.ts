import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Error Boundary Recovery', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should keep all major sections visible after blazor error', async ({ page }) => {
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

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.ring-area')).toBeVisible();
    await expect(page.locator('.task-card')).toBeVisible();
  });

  test('should recover after navigating to settings and back', async ({ page }) => {
    await page.evaluate(() => {
      const blazorError = new Event('blazorerror', {
        bubbles: true,
        cancelable: true,
      });
      (blazorError as any).detail = {
        type: 'error',
        message: 'Navigation recovery test',
      };
      window.dispatchEvent(blazorError);
    });

    await page.waitForTimeout(1000);
    await expect(page.locator('.main-container')).toBeVisible();

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.ring-area')).toBeVisible();
  });
});
