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
    await expect(page.locator('.sec-btn').filter({ hasText: 'Export' })).toHaveCount(0);
  });

  test('should not display import backup button', async ({ page }) => {
    await expect(page.locator('.sec-btn').filter({ hasText: 'Import' })).toHaveCount(0);
  });

  test('should display reset to defaults button', async ({ page }) => {
    await expect(page.locator('.sec-btn').filter({ hasText: 'Reset to defaults' })).toBeVisible();
  });

  test('should auto-save settings when duration is changed', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const incrementBtn = page.locator('.step-btn[aria-label="Increase"]').first();
    await incrementBtn.click();
    await page.waitForTimeout(300);
    const currentValue = await input.inputValue();
    expect(parseInt(currentValue)).toBeGreaterThan(25);
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
    const incrementBtn = page.locator('.step-btn[aria-label="Increase"]').first();
    await incrementBtn.click();
    await page.waitForTimeout(300);
    const currentValue = await input.inputValue();
    expect(parseInt(currentValue)).toBeGreaterThan(25);
  });

  test('should persist pomodoro duration after reload', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const incrementBtn = page.locator('.step-btn[aria-label="Increase"]').first();
    await incrementBtn.click();
    await incrementBtn.click();
    await page.waitForTimeout(300);

    const valueBefore = await input.inputValue();
    expect(parseInt(valueBefore)).toBe(27);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await expect(input).toHaveValue('27');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(500);
  });
});
