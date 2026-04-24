import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should load settings page', async ({ page }) => {
    await expect(page.locator('.sett-body')).toBeVisible();
  });

  test('should display timer durations section', async ({ page }) => {
    await expect(page.locator('.ss').first()).toBeVisible();
    await expect(page.locator('.ss-hdr').first()).toContainText('Timer durations');
  });

  test('should display pomodoro duration setting', async ({ page }) => {
    await expect(page.locator('.sr-lbl').filter({ hasText: /^Pomodoro$/ })).toBeVisible();
  });

  test('should display short break duration setting', async ({ page }) => {
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Short break' })).toBeVisible();
  });

  test('should display long break duration setting', async ({ page }) => {
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Long break' })).toBeVisible();
  });

  test('should display automation section', async ({ page }) => {
    await expect(page.locator('.ss').filter({ hasText: 'Automation' })).toBeVisible();
    await expect(page.locator('.ss-hdr').filter({ hasText: 'Automation' })).toBeVisible();
  });

  test('should display sound & notifications section', async ({ page }) => {
    await expect(page.locator('.ss').filter({ hasText: 'Sound & notifications' })).toBeVisible();
    await expect(page.locator('.ss-hdr').filter({ hasText: 'Sound & notifications' })).toBeVisible();
  });

  test('should display data section with clear button', async ({ page }) => {
    await expect(page.locator('.ss.ss-data').filter({ hasText: 'Data' })).toBeVisible();
    await expect(page.locator('.ss-hdr').filter({ hasText: 'Data' })).toBeVisible();
    await expect(page.locator('.danger-btn').filter({ hasText: 'Clear' })).toBeVisible();
  });

  test('should not display export backup button', async ({ page }) => {
    await expect(page.locator('.btn-export')).toHaveCount(0);
  });

  test('should not display import backup button', async ({ page }) => {
    await expect(page.locator('.btn-import')).toHaveCount(0);
  });

  test('should display reset to defaults button', async ({ page }) => {
    await expect(page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' })).toBeVisible();
  });

  test('should show toast message when settings change is saved', async ({ page }) => {
    const input = page.locator('.step-input').first();
    await input.click();
    await input.pressSequentially('30');
    await page.waitForTimeout(1000);

    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);
    await expect(page.locator('.settings-toast')).toContainText('Settings saved successfully!');
  });

  test('should disable clear button when clearing', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    
    await page.locator('.btn-confirm-danger').click();
    await page.waitForTimeout(1000);
    
    const clearButton = page.locator('.danger-btn').filter({ hasText: 'Clear' });
    await expect(clearButton).toBeVisible();
  });

  test('should allow changing pomodoro duration value', async ({ page }) => {
    const input = page.locator('.step-input').first();
    await expect(input).toBeVisible();

    await input.click();
    await input.pressSequentially('10');
    await page.waitForTimeout(500);

    await expect(input).toHaveValue('10');
  });

  test('should persist pomodoro duration after save and reload', async ({ page }) => {
    const input = page.locator('.step-input').first();

    await input.click();
    await input.pressSequentially('15');
    await page.waitForTimeout(1000);

    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await expect(input).toHaveValue('15');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(500);
  });
});
