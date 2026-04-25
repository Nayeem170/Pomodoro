import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Persistence', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should apply new pomodoro duration to timer after reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.setPomodoroMinutes(30);
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/30:00/);

    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.resetToDefaults();
  });

  test('should persist sound toggle after reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const soundToggle = page.locator('.sr-lbl').filter({ hasText: 'Sound on completion' }).locator('..').locator('.tog');
    await soundToggle.click();
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const hasActiveClass = await soundToggle.evaluate(el => el.classList.contains('on'));
    expect(hasActiveClass).toBe(false);

    await soundToggle.click();
    await page.waitForTimeout(500);
  });

  test('should persist auto-start setting after reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const autoStartToggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isCurrentlyOn = await autoStartToggle.evaluate(el => el.classList.contains('on'));

    if (isCurrentlyOn) {
      await autoStartToggle.click();
      await page.waitForTimeout(500);
    }

    await autoStartToggle.click();
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const isChecked = await autoStartToggle.evaluate(el => el.classList.contains('on'));
    expect(isChecked).toBe(true);

    await autoStartToggle.click();
    await page.waitForTimeout(500);
  });

  test('should show changed duration on timer after navigating away and back', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.setPomodoroMinutes(20);
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/20:00/);

    await pomodoroPage.openSettings();
    await pomodoroPage.resetToDefaults();
  });
});
