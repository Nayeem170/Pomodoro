import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe.configure({ mode: 'serial' });

test.describe('Settings Flow', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test('should change pomodoro duration using stepper buttons', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const incrementBtn = page.locator('.step-btn[aria-label="Increase"]').first();
    await incrementBtn.click();
    await page.waitForTimeout(300);
    const currentValue = await input.inputValue();
    expect(parseInt(currentValue)).toBeGreaterThan(25);
  });

  test('should decrement pomodoro duration using stepper buttons', async ({ page }) => {
    const input = page.locator('.step-input').first();
    const decrementBtn = page.locator('.step-btn[aria-label="Decrease"]').first();
    await decrementBtn.click();
    await page.waitForTimeout(300);
    const currentValue = await input.inputValue();
    expect(parseInt(currentValue)).toBeLessThan(25);
  });

  test('should toggle auto-start pomodoros setting', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(300);
    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);
  });

  test('should toggle auto-start breaks setting', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start breaks' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(300);
    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);
  });

  test('should toggle sound setting', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(300);
    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);
  });

  test('should toggle notifications setting', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(300);
    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);
  });

  test('should reset to defaults and verify values reset', async ({ page }) => {
    const input = page.locator('.step-input').first();
    await input.click();
    await input.pressSequentially('10');
    await page.waitForTimeout(500);
    await expect(input).toHaveValue('10');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(500);
    await expect(input).toHaveValue('25');
  });

  test('should show clear data confirmation modal', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await expect(page.locator('.confirmation-modal')).toContainText('Clear All Data?');
    await expect(page.locator('.btn-confirm-danger')).toBeVisible();
    await expect(page.locator('.btn-cancel-action')).toBeVisible();
  });

  test('should cancel clear data action', async ({ page }) => {
    await page.locator('.danger-btn').filter({ hasText: 'Clear' }).click();
    await expect(page.locator('.confirmation-modal')).toBeVisible();
    await page.locator('.btn-cancel-action').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.confirmation-modal')).not.toBeVisible();
  });
});
