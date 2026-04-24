import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Preferences', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display sound toggle', async ({ page }) => {
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' })).toBeVisible();
  });

  test('should display notifications toggle', async ({ page }) => {
    await expect(page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' })).toBeVisible();
  });

  test('should toggle sound setting', async ({ page }) => {
    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const initialState = await soundToggle.evaluate(el => el.classList.contains('on'));
    await soundToggle.click();
    await page.waitForTimeout(500);
    const toggledState = await soundToggle.evaluate(el => el.classList.contains('on'));
    expect(toggledState).toBe(!initialState);
  });

  test('should toggle notifications setting', async ({ page }) => {
    const notifToggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const initialState = await notifToggle.evaluate(el => el.classList.contains('on'));
    await notifToggle.click();
    await page.waitForTimeout(500);
    const toggledState = await notifToggle.evaluate(el => el.classList.contains('on'));
    expect(toggledState).toBe(!initialState);
  });

  test('should auto-save when sound is toggled', async ({ page }) => {
    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    await soundToggle.click();
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const toggledState = await soundToggle.evaluate(el => el.classList.contains('on'));
    expect(toggledState).toBe(false);
  });

  test('should auto-save when notifications is toggled', async ({ page }) => {
    const notifToggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    await notifToggle.click();
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const toggledState = await notifToggle.evaluate(el => el.classList.contains('on'));
    expect(toggledState).toBe(false);
  });
});
