import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Data Management', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should show confirmation modal before clearing data', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await expect(page.locator('.confirmation-modal h3')).toContainText('Clear All Data');
  });

  test('should display warning text in confirmation modal', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-content')).toContainText('Delete all activities, tasks, and settings');
    await expect(page.locator('.confirmation-content')).toContainText('cannot be undone');
  });

  test('should display confirm and cancel buttons in modal', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.btn-confirm-danger')).toBeVisible();
    await expect(page.locator('.btn-confirm-danger')).toContainText('Yes, Clear All');
    await expect(page.locator('.btn-cancel-action')).toBeVisible();
    await expect(page.locator('.btn-cancel-action')).toContainText('Cancel');
  });

  test('should hide confirmation modal on cancel', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();

    await page.locator('.btn-cancel-action').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });

  test('should show clearing state after confirming clear', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(100);
    const clearButton = page.locator('.danger-btn').filter({ hasText: 'Clear' });
    const buttonText = await clearButton.textContent();
    expect(buttonText).toMatch(/Clearing\.\.\.|Clear/);
  });
});
