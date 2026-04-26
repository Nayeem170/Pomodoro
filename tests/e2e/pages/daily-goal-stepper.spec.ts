import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Daily Goal Stepper', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display daily goal as 4th step-input', async ({ page }) => {
    const inputs = page.locator('.step-input');
    await expect(inputs).toHaveCount(4);
    const dailyGoalInput = inputs.nth(3);
    await expect(dailyGoalInput).toBeVisible();
    const value = await dailyGoalInput.inputValue();
    expect(parseInt(value)).toBeGreaterThan(0);
  });

  test('should increment daily goal when increase button is clicked', async ({ page }) => {
    const dailyGoalInput = page.locator('.step-input').nth(3);
    const increaseBtns = page.locator('.step-btn[aria-label="Increase"]');
    const initialValue = await dailyGoalInput.inputValue();

    await increaseBtns.nth(3).click();
    await page.waitForTimeout(300);

    const newValue = await dailyGoalInput.inputValue();
    expect(parseInt(newValue)).toBe(parseInt(initialValue) + 1);
  });

  test('should decrement daily goal when decrease button is clicked', async ({ page }) => {
    const dailyGoalInput = page.locator('.step-input').nth(3);
    const decreaseBtns = page.locator('.step-btn[aria-label="Decrease"]');
    const initialValue = await dailyGoalInput.inputValue();

    await decreaseBtns.nth(3).click();
    await page.waitForTimeout(300);

    const newValue = await dailyGoalInput.inputValue();
    expect(parseInt(newValue)).toBe(parseInt(initialValue) - 1);
  });

  test('should not decrement below min value of 1', async ({ page }) => {
    const dailyGoalInput = page.locator('.step-input').nth(3);
    const decreaseBtns = page.locator('.step-btn[aria-label="Decrease"]');

    for (let i = 0; i < 20; i++) {
      const isDisabled = await decreaseBtns.nth(3).isDisabled();
      if (isDisabled) break;
      await decreaseBtns.nth(3).click();
      await page.waitForTimeout(100);
    }

    const value = await dailyGoalInput.inputValue();
    expect(parseInt(value)).toBe(1);

    const isDisabled = await decreaseBtns.nth(3).isDisabled();
    expect(isDisabled).toBe(true);
  });

  test('should not increment above max value of 20', async ({ page }) => {
    const dailyGoalInput = page.locator('.step-input').nth(3);
    const increaseBtns = page.locator('.step-btn[aria-label="Increase"]');

    for (let i = 0; i < 20; i++) {
      const isDisabled = await increaseBtns.nth(3).isDisabled();
      if (isDisabled) break;
      await increaseBtns.nth(3).click();
      await page.waitForTimeout(100);
    }

    const value = await dailyGoalInput.inputValue();
    expect(parseInt(value)).toBe(20);

    const isDisabled = await increaseBtns.nth(3).isDisabled();
    expect(isDisabled).toBe(true);
  });

  test('should show daily goal label and subtitle', async ({ page }) => {
    const dailyGoalRow = page.locator('.sr').filter({ hasText: 'Daily goal' });
    await expect(dailyGoalRow).toBeVisible();
    await expect(dailyGoalRow.locator('.sr-lbl')).toContainText('Daily goal');
  });
});
