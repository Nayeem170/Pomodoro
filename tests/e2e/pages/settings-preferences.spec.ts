import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Preferences', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display sound toggle', async ({ page }) => {
    await expect(page.locator('label[for="soundToggle"]')).toBeVisible();
  });

  test('should display notifications toggle', async ({ page }) => {
    await expect(page.locator('label[for="notifToggle"]')).toBeVisible();
  });

  test('should toggle sound setting', async ({ page }) => {
    const soundToggle = page.locator('#soundToggle');
    const initialState = await soundToggle.isChecked();
    await page.locator('label[for="soundToggle"]').click();
    await page.waitForTimeout(500);
    const toggledState = await soundToggle.isChecked();
    expect(toggledState).toBe(!initialState);
  });

  test('should toggle notifications setting', async ({ page }) => {
    const notifToggle = page.locator('#notifToggle');
    const initialState = await notifToggle.isChecked();
    await page.locator('label[for="notifToggle"]').click();
    await page.waitForTimeout(500);
    const toggledState = await notifToggle.isChecked();
    expect(toggledState).toBe(!initialState);
  });

  test('should enable save button when sound is toggled', async ({ page }) => {
    const saveButton = page.locator('.btn-save');
    await expect(saveButton).toBeDisabled();
    await page.locator('label[for="soundToggle"]').click();
    await page.waitForTimeout(500);
    await expect(saveButton).not.toBeDisabled();
  });

  test('should enable save button when notifications is toggled', async ({ page }) => {
    const saveButton = page.locator('.btn-save');
    await expect(saveButton).toBeDisabled();
    await page.locator('label[for="notifToggle"]').click();
    await page.waitForTimeout(500);
    await expect(saveButton).not.toBeDisabled();
  });
});
