import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function setupPomodoroTest(page: any, pomodoroPage: PomodoroPage, taskName: string, autoStart: boolean = true) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

  if (autoStart) {
    await page.locator('#autoStartEnabled').check({ force: true });
  } else {
    await page.evaluate(() => {
      const cb = document.getElementById('autoStartEnabled') as HTMLInputElement;
      if (cb) { cb.checked = false; cb.dispatchEvent(new Event('change', { bubbles: true })); }
    });
  }
  await page.waitForTimeout(500);

  const pomodoroInput = page.locator('input[type="number"]').first();
  await pomodoroInput.fill('1');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);

  await page.locator('.btn-save').click();
  await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
  await page.waitForTimeout(500);

  await pomodoroPage.goto('/');
  await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);

  await pomodoroPage.startTimer();
  await expect(page.locator('.btn-pause')).toBeVisible();
  await page.waitForTimeout(500);

  // Directly invoke timer ticks via the JS timer function to complete the 1-min pomodoro instantly
  // This bypasses the need for real time or clock manipulation
  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    // The timer runs at 1000ms intervals, invoke 60 ticks with small delays for interop
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
}

test.describe('Consent Modal', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should display consent modal after pomodoro completes with auto-start enabled', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Consent Modal Test', true);

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.consent-modal')).toBeVisible();
  });

  test('should display correct title and message for pomodoro completion', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Title Test', true);

    await expect(page.locator('.consent-header h2')).toContainText('Pomodoro Complete');
    await expect(page.locator('.consent-message')).toContainText('Great work');
    await expect(page.locator('.consent-icon')).toContainText('🍅');
  });

  test('should display session options in consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Options Test', true);

    await expect(page.locator('.consent-options')).toBeVisible();
    await expect(page.locator('.btn-option')).toHaveCount(3);
    await expect(page.locator('.btn-option').filter({ hasText: 'Short Break' })).toBeVisible();
    await expect(page.locator('.btn-option').filter({ hasText: 'Long Break' })).toBeVisible();
    await expect(page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' })).toBeVisible();
  });

  test('should display default option with highlighted style', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Default Test', true);

    const defaultOption = page.locator('.btn-option.default');
    await expect(defaultOption).toBeVisible();
    await expect(defaultOption).toContainText('Another Pomodoro');
  });

  test('should display countdown timer in consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Countdown Test', true);

    await expect(page.locator('.countdown-section')).toBeVisible();
    await expect(page.locator('.countdown-text')).toContainText('Auto-continuing');
    await expect(page.locator('.countdown-text')).toContainText('seconds');
    await expect(page.locator('.countdown-bar')).toBeVisible();
    await expect(page.locator('.countdown-progress')).toBeVisible();
  });

  test('should allow selecting an option from consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Select Option Test', true);

    await page.locator('.btn-option').filter({ hasText: 'Long Break' }).click();
    await page.waitForTimeout(2000);

    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible();
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('LONG BREAK');
  });

  test('should not show consent modal when auto-start is disabled', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'No Auto Test', false);

    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible();
  });

  test('should show consent modal after break completes with auto-start enabled', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('#autoStartEnabled').check({ force: true });
    await page.locator('#autoStartEnabled').dispatchEvent('change', { bubbles: true });
    await page.waitForTimeout(500);

    const pomodoroInput = page.locator('input[type="number"]').first();
    await pomodoroInput.fill('1');
    await pomodoroInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    const shortBreakInput = page.locator('input[type="number"]').nth(1);
    await shortBreakInput.fill('1');
    await shortBreakInput.dispatchEvent('change');
    await page.waitForTimeout(500);

    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Break Consent Test');
    await pomodoroPage.selectTask('Break Consent Test');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(500);

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
    await page.waitForTimeout(2000);

    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('SHORT BREAK');

    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.consent-header h2')).toContainText('Break Complete');
  });
});
