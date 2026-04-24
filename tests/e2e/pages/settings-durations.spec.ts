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
    await expect(page.locator('.step-input')).toHaveCount(4);
  });

  test('should have correct default pomodoro duration', async ({ page }) => {
    const inputs = page.locator('.step-input');
    const pomodoroInput = inputs.nth(0);
    await expect(pomodoroInput).toHaveValue('25');
  });

  test('should have correct default short break duration', async ({ page }) => {
    const inputs = page.locator('.step-input');
    const shortBreakInput = inputs.nth(1);
    await expect(shortBreakInput).toHaveValue('5');
  });

  test('should have correct default long break duration', async ({ page }) => {
    const inputs = page.locator('.step-input');
    const longBreakInput = inputs.nth(2);
    await expect(longBreakInput).toHaveValue('15');
  });

  test('should allow changing pomodoro duration', async ({ page }) => {
    const inputs = page.locator('.step-input');
    const pomodoroInput = inputs.nth(0);
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('30');
    await pomodoroInput.dispatchEvent('input');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);
    await expect(pomodoroInput).toHaveValue('30');
  });

  test('should auto-save when duration is changed', async ({ page }) => {
    const inputs = page.locator('.step-input');
    await inputs.nth(0).click({ clickCount: 3 });
    await inputs.nth(0).pressSequentially('30');
    await inputs.nth(0).dispatchEvent('input');
    await inputs.nth(0).dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const reloadedInputs = page.locator('.step-input');
    await expect(reloadedInputs.nth(0)).toHaveValue('30');
  });

  test('should reset durations to defaults', async ({ page }) => {
    const inputs = page.locator('.step-input');
    await inputs.nth(0).click({ clickCount: 3 });
    await inputs.nth(0).pressSequentially('30');
    await inputs.nth(0).dispatchEvent('input');
    await inputs.nth(0).dispatchEvent('change');
    await page.waitForTimeout(500);

    const resetButton = page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' });
    await expect(resetButton).not.toBeDisabled();
    await resetButton.click();
    await page.waitForTimeout(500);

    await expect(inputs.nth(0)).toHaveValue('25');
    await expect(inputs.nth(1)).toHaveValue('5');
    await expect(inputs.nth(2)).toHaveValue('15');
  });
});
