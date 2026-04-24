import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Settings', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should show Connect button when not connected', async ({ page }) => {
    await expect(page.locator('.ss-data')).toBeVisible();
    await expect(page.locator('.ss-hdr')).toContainText('Cloud Sync');
    await expect(page.locator('button.sec-btn', { hasText: 'Connect' })).toBeVisible();
  });

  test('should show Sync and Disconnect buttons when connected', async ({ page }) => {
    await expect(page.locator('.sync-actions')).toBeVisible();
    await expect(page.locator('button.sec-btn', { hasText: 'Sync' })).toBeVisible();
    await expect(page.locator('button.danger-btn', { hasText: 'Disconnect' })).toBeVisible();
  });

  test('should show Last synced time', async ({ page }) => {
    await expect(page.locator('.sr-sub')).toContainText('Last synced');
  });

  test('should show Never when never synced', async ({ page }) => {
    await expect(page.locator('.sr-sub')).toContainText('Never');
  });

  test('should show Data section with Clear button', async ({ page }) => {
    await expect(page.locator('.ss-data').nth(1)).toBeVisible();
    await expect(page.locator('.ss-hdr').nth(1)).toContainText('Data');
    await expect(page.locator('button.danger-btn', { hasText: 'Clear' })).toBeVisible();
  });

  test('should show Clear confirmation modal on Clear click', async ({ page }) => {
    await page.locator('button.danger-btn', { hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await expect(page.locator('.confirmation-modal')).toContainText('Clear All Data?');
  });

  test('should dismiss modal on Cancel', async ({ page }) => {
    await page.locator('button.danger-btn', { hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('button.btn-cancel-action', { hasText: 'Cancel' }).click();
    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });
});
