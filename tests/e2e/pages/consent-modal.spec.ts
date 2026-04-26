import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function setupPomodoroTest(page: any, pomodoroPage: PomodoroPage, taskName: string, autoStart: boolean = true) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

  if (autoStart) {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isOn = await toggle.evaluate(el => el.classList.contains('on'));
    if (!isOn) {
      await toggle.click();
    }
  } else {
    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isOn = await toggle.evaluate(el => el.classList.contains('on'));
    if (isOn) {
      await toggle.click();
    }
  }

  await pomodoroPage.setPomodoroMinutes(1);
  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await pomodoroPage.completePomodoroFast();
}

test.describe('Consent Modal', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

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
    await expect(page.locator('.consent-icon-wrap')).toContainText('🍅');
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
    await expect(page.locator('.consent-footer')).toBeVisible();
    await expect(page.locator('.auto-continue-row span')).toContainText('Auto-continuing');
    await expect(page.locator('.consent-progress-track')).toBeVisible();
    await expect(page.locator('.consent-progress-fill')).toBeVisible();
  });

  test('should allow selecting an option from consent modal', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'Select Option Test', true);
    await page.locator('.btn-option').filter({ hasText: 'Long Break' }).click();
    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 5000 });
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('LONG BREAK');
  });

  test('should not show consent modal when auto-start is disabled', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await setupPomodoroTest(page, pomodoroPage, 'No Auto Test', false);
    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 5000 });
  });

  test('should show consent modal after break completes with auto-start enabled', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isOn = await toggle.evaluate(el => el.classList.contains('on'));
    if (!isOn) {
      await toggle.click();
    }

    await pomodoroPage.setPomodoroMinutes(1);

    const shortBreakInput = page.locator('.step-input').nth(1);
    const currentShortBreak = parseInt(await shortBreakInput.inputValue());
    const diff = 1 - currentShortBreak;
    if (diff !== 0) {
      const btnLabel = diff > 0 ? 'Increase' : 'Decrease';
      const btn = page.locator('.step-btn[aria-label="' + btnLabel + '"]').nth(1);
      for (let i = 0; i < Math.abs(diff); i++) {
        await btn.click();
        await page.waitForTimeout(50);
      }
    }

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Break Consent Test');
    await pomodoroPage.selectTask('Break Consent Test');
    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await page.locator('.btn-option').filter({ hasText: 'Short Break' }).click();
    await page.waitForTimeout(2000);

    const isRunning = await page.locator('button[aria-label="Pause timer"]').isVisible().catch(() => false);
    if (!isRunning) {
      const startOrResume = page.locator('button[aria-label="Start timer"], button[aria-label="Resume timer"]').first();
      await expect(startOrResume).toBeVisible({ timeout: 5000 });
      await startOrResume.click();
    }
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.consent-header h2')).toContainText('Break Complete');
  });
});
