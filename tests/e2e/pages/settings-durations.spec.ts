import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Timer Durations', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display three duration inputs', async ({ page }) => {
    await expect(page.locator('input[type="number"]')).toHaveCount(4);
  });

  test('should have correct default pomodoro duration', async ({ page }) => {
    const inputs = page.locator('input[type="number"]');
    const pomodoroInput = inputs.nth(0);
    await expect(pomodoroInput).toHaveValue('25');
  });

  test('should have correct default short break duration', async ({ page }) => {
    const inputs = page.locator('input[type="number"]');
    const shortBreakInput = inputs.nth(1);
    await expect(shortBreakInput).toHaveValue('5');
  });

  test('should have correct default long break duration', async ({ page }) => {
    const inputs = page.locator('input[type="number"]');
    const longBreakInput = inputs.nth(2);
    await expect(longBreakInput).toHaveValue('15');
  });

  test('should allow changing pomodoro duration', async ({ page }) => {
    const inputs = page.locator('input[type="number"]');
    const pomodoroInput = inputs.nth(0);
    await pomodoroInput.fill('30');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);
    await expect(pomodoroInput).toHaveValue('30');
  });

  test('should enable save button when duration is changed', async ({ page }) => {
    const saveButton = page.locator('.btn-save');
    await expect(saveButton).toBeDisabled();
    const inputs = page.locator('input[type="number"]');
    await inputs.nth(0).fill('30');
    await inputs.nth(0).dispatchEvent('change');
    await page.waitForTimeout(500);
    await expect(saveButton).not.toBeDisabled();
  });

  test('should reset durations to defaults', async ({ page }) => {
    const inputs = page.locator('input[type="number"]');
    await inputs.nth(0).fill('30');
    await inputs.nth(0).dispatchEvent('change');
    await page.waitForTimeout(500);

    const resetButton = page.locator('.btn-reset-defaults');
    await expect(resetButton).not.toBeDisabled();
    await resetButton.click();
    await page.waitForTimeout(500);

    await expect(inputs.nth(0)).toHaveValue('25');
    await expect(inputs.nth(1)).toHaveValue('5');
    await expect(inputs.nth(2)).toHaveValue('15');
  });
});
