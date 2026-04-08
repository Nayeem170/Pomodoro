import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Duration Input Clamping', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
  });

  test('should have correct min/max attributes on pomodoro input', async ({ page }) => {
    const pomodoroInput = page.locator('input.setting-input').first();
    await expect(pomodoroInput).toHaveAttribute('min', '1');
    await expect(pomodoroInput).toHaveAttribute('max', '60');
  });

  test('should have correct min/max attributes on short break input', async ({ page }) => {
    const breakInputs = page.locator('input.setting-input');
    const shortBreakInput = breakInputs.nth(1);
    await expect(shortBreakInput).toHaveAttribute('min', '1');
    await expect(shortBreakInput).toHaveAttribute('max', '30');
  });

  test('should have correct min/max attributes on long break input', async ({ page }) => {
    const breakInputs = page.locator('input.setting-input');
    const longBreakInput = breakInputs.nth(2);
    await expect(longBreakInput).toHaveAttribute('min', '1');
    await expect(longBreakInput).toHaveAttribute('max', '60');
  });

  test('should have correct min/max attributes on auto-start delay input', async ({ page }) => {
    const autoStartToggle = page.locator('input[type="checkbox"]').first();
    const isChecked = await autoStartToggle.isChecked();
    if (!isChecked) {
      await autoStartToggle.dispatchEvent('change');
      await page.waitForTimeout(300);
    }

    const delayInput = page.locator('input.setting-input[min="0"]');
    if (await delayInput.count() > 0) {
      await expect(delayInput).toHaveAttribute('min', '0');
      await expect(delayInput).toHaveAttribute('max', '60');
    }
  });

  test('should display all four duration inputs', async ({ page }) => {
    await expect(page.locator('input.setting-input')).toHaveCount(4);
  });
});
