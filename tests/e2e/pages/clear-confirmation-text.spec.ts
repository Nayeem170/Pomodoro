import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Clear Confirmation Modal Text', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should show clear confirmation modal with correct title and warning', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible({ timeout: 5000 });

    await expect(page.locator('.confirmation-modal')).toContainText('Clear All Data?');
    await expect(page.locator('.confirmation-modal')).toContainText('cannot be undone');
  });

  test('should mention local data in clear confirmation modal', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible({ timeout: 5000 });

    const modalText = await page.locator('.confirmation-modal').textContent();
    expect(modalText?.toLowerCase()).toContain('device');
  });

  test('should close clear confirmation modal with cancel button', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible({ timeout: 5000 });

    await page.locator('.btn-cancel-action').click();
    await page.waitForTimeout(500);

    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });
});
