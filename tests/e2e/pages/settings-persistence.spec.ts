import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Settings Persistence', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should apply new pomodoro duration to timer after save and reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('30');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);
    await expect(page.locator('.settings-toast')).toBeVisible();

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/30:00/);

    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });
    await page.locator('.btn-reset-defaults').click();
    await page.waitForTimeout(500);
  });

  test('should persist sound toggle after save and reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('label[for="soundToggle"]').click();
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const soundToggle = page.locator('#soundToggle');
    const isChecked = await soundToggle.isChecked();
    expect(isChecked).toBe(false);

    await page.locator('label[for="soundToggle"]').click();
    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);
  });

  test('should persist auto-start setting after save and reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const autoStartToggle = page.locator('#autoStartEnabled');
    const isCurrentlyChecked = await autoStartToggle.isChecked();

    if (isCurrentlyChecked) {
      await page.evaluate(() => {
        const cb = document.getElementById('autoStartEnabled') as HTMLInputElement;
        if (cb) { cb.checked = false; cb.dispatchEvent(new Event('change', { bubbles: true })); }
      });
      await page.waitForTimeout(500);
      await page.locator('.btn-save').click();
      await page.waitForTimeout(2000);
    }

    await page.evaluate(() => {
      const cb = document.getElementById('autoStartEnabled') as HTMLInputElement;
      if (cb) { cb.checked = true; cb.dispatchEvent(new Event('change', { bubbles: true })); }
    });
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const isChecked = await autoStartToggle.isChecked();
    expect(isChecked).toBe(true);

    await page.evaluate(() => {
      const cb = document.getElementById('autoStartEnabled') as HTMLInputElement;
      if (cb) { cb.checked = false; cb.dispatchEvent(new Event('change', { bubbles: true })); }
    });
    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);
  });

  test('should show changed duration on timer after navigating away and back', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('20');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await page.waitForTimeout(2000);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/20:00/);

    await pomodoroPage.openSettings();
    await page.locator('.btn-reset-defaults').click();
    await page.waitForTimeout(500);
  });
});
