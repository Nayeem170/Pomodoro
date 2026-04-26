import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Toast On Change', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test.describe.configure({ timeout: 60000 });

  test('should show toast when reset to defaults is clicked', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const currentValue = await input.inputValue();
    if (currentValue === '25') {
      await pomodoroPage.setPomodoroMinutes(10);
      await page.waitForTimeout(500);
    }

    const resetButton = page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' });
    await expect(resetButton).toBeEnabled({ timeout: 3000 });
    await resetButton.click();

    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 3000 });
    await expect(page.locator('.settings-toast')).toContainText('Settings reset to defaults!');
  });

  test('toast should disappear after a few seconds', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const currentValue = await input.inputValue();
    if (currentValue === '25') {
      await pomodoroPage.setPomodoroMinutes(10);
      await page.waitForTimeout(500);
    }

    const resetButton = page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' });
    await expect(resetButton).toBeEnabled({ timeout: 3000 });
    await resetButton.click();

    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 3000 });
    await expect(page.locator('.settings-toast')).not.toBeVisible({ timeout: 10000 });
  });
});
