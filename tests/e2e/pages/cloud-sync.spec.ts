import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Cloud Sync Settings', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should show Connect button when not connected', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.ss-hdr').filter({ hasText: 'Cloud Sync' })).toBeVisible();
    await expect(page.locator('.sec-btn').filter({ hasText: 'Connect' })).toBeVisible();
  });

  test('should show Google Drive label and Sync across devices subtitle', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Google Drive' })).toBeVisible();
    await expect(page.locator('.sr-sub').filter({ hasText: 'Sync across devices' })).toBeVisible();
  });

  test('should show Data section with Clear button', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.ss-hdr').filter({ hasText: 'Data' })).toBeVisible();
    await expect(page.locator('.danger-btn').filter({ hasText: 'Clear' })).toBeVisible();
  });

  test('should show Clear confirmation modal on Clear click', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await expect(page.locator('.confirmation-modal')).toContainText('Clear All Data');
  });

  test('should dismiss modal on Cancel', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-cancel-action').filter({ hasText: 'Cancel' }).click();
    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });
});
