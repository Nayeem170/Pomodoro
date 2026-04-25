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
    await pomodoroPage.setPomodoroMinutes(10);
    await page.waitForTimeout(500);

    const input = page.locator('.step-input').first();
    await expect(input).toHaveValue('10');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(300);

    await expect(page.locator('.settings-toast')).toBeVisible();
    await expect(page.locator('.settings-toast')).toContainText('Settings reset to defaults!');
  });

  test('toast should disappear after a few seconds', async ({ page }) => {
    await pomodoroPage.setPomodoroMinutes(10);
    await page.waitForTimeout(500);

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(300);

    await expect(page.locator('.settings-toast')).toBeVisible();
    await page.waitForTimeout(3000);
    await expect(page.locator('.settings-toast')).not.toBeVisible();
  });
});
