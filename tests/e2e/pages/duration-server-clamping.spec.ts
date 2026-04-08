import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Duration Server-Side Clamping', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
  });

  test('should clamp pomodoro value to minimum of 1 when entering 0', async ({ page }) => {
    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('0');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const saveBtn = page.locator('.btn-save');
    const isDisabled = await saveBtn.isDisabled();
    if (!isDisabled) {
      await saveBtn.click();
      await page.waitForTimeout(2000);
      await pomodoroPage.openSettings();
      await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    }

    const inputs = page.locator('input[type="number"]');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });

  test('should clamp pomodoro value to maximum of 120 when entering 999', async ({ page }) => {
    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('999');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const saveBtn = page.locator('.btn-save');
    const isDisabled = await saveBtn.isDisabled();
    if (!isDisabled) {
      await saveBtn.click();
      await page.waitForTimeout(2000);
      await pomodoroPage.openSettings();
      await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    }

    const inputs = page.locator('input[type="number"]');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeLessThanOrEqual(120);
  });

  test('should clamp short break value to minimum of 1 when entering 0', async ({ page }) => {
    const shortBreakInput = page.locator('input[type="number"]').nth(1);
    await shortBreakInput.fill('0');
    await shortBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const saveBtn = page.locator('.btn-save');
    const isDisabled = await saveBtn.isDisabled();
    if (!isDisabled) {
      await saveBtn.click();
      await page.waitForTimeout(2000);
      await pomodoroPage.openSettings();
      await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    }

    const inputs = page.locator('input[type="number"]');
    const val = await inputs.nth(1).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });

  test('should clamp long break value to maximum of 60 when entering 999', async ({ page }) => {
    const longBreakInput = page.locator('input[type="number"]').nth(2);
    await longBreakInput.fill('999');
    await longBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const saveBtn = page.locator('.btn-save');
    const isDisabled = await saveBtn.isDisabled();
    if (!isDisabled) {
      await saveBtn.click();
      await page.waitForTimeout(2000);
      await pomodoroPage.openSettings();
      await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    }

    const inputs = page.locator('input[type="number"]');
    const val = await inputs.nth(2).inputValue();
    expect(parseInt(val)).toBeLessThanOrEqual(60);
  });

  test('should clamp negative pomodoro value to minimum of 1', async ({ page }) => {
    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('-5');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const saveBtn = page.locator('.btn-save');
    const isDisabled = await saveBtn.isDisabled();
    if (!isDisabled) {
      await saveBtn.click();
      await page.waitForTimeout(2000);
      await pomodoroPage.openSettings();
      await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    }

    const inputs = page.locator('input[type="number"]');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });
});
