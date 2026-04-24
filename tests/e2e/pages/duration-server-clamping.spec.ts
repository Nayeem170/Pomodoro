import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Duration Server-Side Clamping', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should clamp pomodoro value to minimum of 1 when entering 0', async ({ page }) => {
    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('0');
    await pomodoroInput.dispatchEvent('input');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });

  test('should clamp pomodoro value to maximum of 120 when entering 999', async ({ page }) => {
    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('999');
    await pomodoroInput.dispatchEvent('input');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeLessThanOrEqual(120);
  });

  test('should clamp short break value to minimum of 1 when entering 0', async ({ page }) => {
    const shortBreakInput = page.locator('.step-input').nth(1);
    await shortBreakInput.click({ clickCount: 3 });
    await shortBreakInput.pressSequentially('0');
    await shortBreakInput.dispatchEvent('input');
    await shortBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const val = await inputs.nth(1).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });

  test('should clamp long break value to maximum of 60 when entering 999', async ({ page }) => {
    const longBreakInput = page.locator('.step-input').nth(2);
    await longBreakInput.click({ clickCount: 3 });
    await longBreakInput.pressSequentially('999');
    await longBreakInput.dispatchEvent('input');
    await longBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const val = await inputs.nth(2).inputValue();
    expect(parseInt(val)).toBeLessThanOrEqual(60);
  });

  test('should clamp negative pomodoro value to minimum of 1', async ({ page }) => {
    const pomodoroInput = page.locator('.step-input').first();
    await pomodoroInput.click({ clickCount: 3 });
    await pomodoroInput.pressSequentially('-5');
    await pomodoroInput.dispatchEvent('input');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const inputs = page.locator('.step-input');
    const val = await inputs.nth(0).inputValue();
    expect(parseInt(val)).toBeGreaterThanOrEqual(1);
  });
});
