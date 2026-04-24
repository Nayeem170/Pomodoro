import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Duration Input Enter Key Validation', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should clamp pomodoro duration to max on Enter', async ({ page }) => {
    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.fill('999');
    await page.waitForTimeout(300);

    await pomodoroInput.press('Enter');
    await page.waitForTimeout(500);

    const maxAttr = await pomodoroInput.getAttribute('max');
    const maxValue = maxAttr ? parseInt(maxAttr) : 60;
    const currentValue = await pomodoroInput.inputValue();
    expect(parseInt(currentValue)).toBeLessThanOrEqual(maxValue);
  });

  test('should clamp short break duration to max on Enter', async ({ page }) => {
    const breakInputs = page.locator('.step-input');
    const shortBreakInput = breakInputs.nth(1);
    await shortBreakInput.click({ clickCount: 3 });
    await shortBreakInput.fill('999');
    await page.waitForTimeout(300);

    await shortBreakInput.press('Enter');
    await page.waitForTimeout(500);

    const maxAttr = await shortBreakInput.getAttribute('max');
    const maxValue = maxAttr ? parseInt(maxAttr) : 30;
    const currentValue = await shortBreakInput.inputValue();
    expect(parseInt(currentValue)).toBeLessThanOrEqual(maxValue);
  });

  test('should clamp long break duration to max on Enter', async ({ page }) => {
    const breakInputs = page.locator('.step-input');
    const longBreakInput = breakInputs.nth(2);
    await longBreakInput.click({ clickCount: 3 });
    await longBreakInput.fill('999');
    await page.waitForTimeout(300);

    await longBreakInput.press('Enter');
    await page.waitForTimeout(500);

    const maxAttr = await longBreakInput.getAttribute('max');
    const maxValue = maxAttr ? parseInt(maxAttr) : 60;
    const currentValue = await longBreakInput.inputValue();
    expect(parseInt(currentValue)).toBeLessThanOrEqual(maxValue);
  });
});
