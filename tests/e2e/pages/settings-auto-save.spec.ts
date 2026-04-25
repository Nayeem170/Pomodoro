import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Auto-Save', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test.afterEach(async ({ page }) => {
    await pomodoroPage.resetToDefaults();
  });

  test('should persist pomodoro duration after reload without save button', async ({ page }) => {
    await pomodoroPage.setPomodoroMinutes(30);
    await page.waitForTimeout(1000);

    const input = page.locator('.step-input').first();
    await expect(input).toHaveValue('30');

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const reloadedInput = page.locator('.step-input').first();
    await expect(reloadedInput).toHaveValue('30');
  });

  test('should persist sound toggle state after reload', async ({ page }) => {
    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const classBefore = await soundToggle.getAttribute('class');
    await soundToggle.click();
    await page.waitForTimeout(500);

    const classAfter = await soundToggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const reloadedToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    const classReloaded = await reloadedToggle.getAttribute('class');
    expect(classReloaded).toBe(classAfter);
  });

  test('should persist auto-start pomodoros toggle after reload', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(500);

    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const reloadedToggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const classReloaded = await reloadedToggle.getAttribute('class');
    expect(classReloaded).toBe(classAfter);
  });

  test('should persist notifications toggle after reload', async ({ page }) => {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const classBefore = await toggle.getAttribute('class');
    await toggle.click();
    await page.waitForTimeout(500);

    const classAfter = await toggle.getAttribute('class');
    expect(classBefore).not.toBe(classAfter);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const reloadedToggle = page.locator('.sr-lbl').filter({ hasText: 'Browser notifications' }).locator('..').locator('.tog');
    const classReloaded = await reloadedToggle.getAttribute('class');
    expect(classReloaded).toBe(classAfter);
  });
});
