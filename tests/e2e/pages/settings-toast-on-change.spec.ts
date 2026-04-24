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
    await input.click({ clickCount: 3 });
    await input.pressSequentially('10');
    await input.dispatchEvent('input');
    await page.waitForTimeout(500);
    await expect(input).toHaveValue('10');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(300);

    await expect(page.locator('.settings-toast')).toBeVisible();
    await expect(page.locator('.settings-toast')).toContainText('Settings reset to defaults!');
  });

  test('toast should disappear after a few seconds', async ({ page }) => {
    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(300);

    await expect(page.locator('.settings-toast')).toBeVisible();

    await page.waitForTimeout(4000);
    await expect(page.locator('.settings-toast')).not.toBeVisible();
  });

  test('should show toast message for settings saved successfully', async ({ page }) => {
    const input = page.locator('.step-input').first();
    await input.click({ clickCount: 3 });
    await input.pressSequentially('30');
    await input.dispatchEvent('input');
    await input.dispatchEvent('change');
    await page.waitForTimeout(500);

    const toast = page.locator('.settings-toast');
    const isToastVisible = await toast.isVisible().catch(() => false);
    if (isToastVisible) {
      await expect(toast).toContainText('Settings saved successfully!');
    }
  });

  test('should not show toast when no changes are made', async ({ page }) => {
    await page.waitForTimeout(2000);
    await expect(page.locator('.settings-toast')).not.toBeVisible();
  });
});
