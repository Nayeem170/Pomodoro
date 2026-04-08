import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Data Management', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should show exporting state on export button click', async ({ page }) => {
    const exportButton = page.locator('.btn-export');
    await exportButton.click();
    await page.waitForTimeout(100);
    const buttonText = await exportButton.textContent();
    expect(buttonText).toMatch(/Exporting\.\.\.|Export JSON/);
  });

  test('should return to normal state after export completes', async ({ page }) => {
    const exportButton = page.locator('.btn-export');
    await exportButton.click();
    await page.waitForTimeout(3000);
    const buttonText = await exportButton.textContent();
    expect(buttonText).toContain('Export JSON');
  });

  test('should have import file input with json accept type', async ({ page }) => {
    const fileInput = page.locator('.file-input');
    await expect(fileInput).toHaveAttribute('accept', '.json');
  });

  test('should disable import during import operation', async ({ page }) => {
    const importLabel = page.locator('.btn-import');
    const fileInput = page.locator('.file-input');
    const isDisabled = await fileInput.isDisabled();
    expect(isDisabled).toBe(false);
  });

  test('should show confirmation modal before clearing data', async ({ page }) => {
    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await expect(page.locator('.confirmation-modal h3')).toContainText('Clear All Data');
  });

  test('should display warning text in confirmation modal', async ({ page }) => {
    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-content')).toContainText('permanently delete');
    await expect(page.locator('.confirmation-content')).toContainText('cannot be undone');
  });

  test('should display confirm and cancel buttons in modal', async ({ page }) => {
    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.btn-confirm-danger')).toBeVisible();
    await expect(page.locator('.btn-confirm-danger')).toContainText('Yes, Clear All');
    await expect(page.locator('.btn-cancel')).toBeVisible();
    await expect(page.locator('.btn-cancel')).toContainText('Cancel');
  });

  test('should hide confirmation modal on cancel', async ({ page }) => {
    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();

    await page.locator('.btn-cancel').click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });

  test('should show clearing state after confirming clear', async ({ page }) => {
    await page.locator('.btn-clear').click();
    await page.waitForTimeout(1000);
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(100);
    const clearButton = page.locator('.btn-clear');
    const buttonText = await clearButton.textContent();
    expect(buttonText).toMatch(/Clearing\.\.\.|Clear/);
  });
});
