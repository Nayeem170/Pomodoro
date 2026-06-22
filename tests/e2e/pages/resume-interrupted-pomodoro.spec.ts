import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Resume Interrupted Pomodoro', () => {
  let page: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
  });

  test('should show Resume Pomodoro option in consent modal after interrupting for break', async () => {
    await page.goto('/settings');
    await expect(page.page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await page.setPomodoroMinutes(1);

    const shortBreakInput = page.page.locator('.step-input').nth(1);
    const shortBreakVal = parseInt(await shortBreakInput.inputValue());
    const shortBreakDiff = 1 - shortBreakVal;
    if (shortBreakDiff !== 0) {
      const btnLabel = shortBreakDiff > 0 ? 'Increase' : 'Decrease';
      const btn = page.page.locator('.step-btn[aria-label="' + btnLabel + '"]').nth(1);
      for (let i = 0; i < Math.abs(shortBreakDiff); i++) {
        await btn.click();
        await page.page.waitForTimeout(50);
      }
    }

    await page.goto('/');
    await expect(page.page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.addTask('Interrupt Test');
    await page.selectTask('Interrupt Test');
    await page.startTimer();

    await page.page.clock.fastForward(30000);
    const timeBeforeSwitch = await page.getTimerDisplay();

    await page.switchToShortBreak();
    await expect(page.page.locator('.timer-mode-label')).toContainText('SHORT BREAK', { timeout: 5000 });

    await page.startTimer();
    await page.completePomodoroFast();

    await expect(page.page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.page.locator('.btn-option').filter({ hasText: 'Resume Pomodoro' })).toBeVisible();
    await expect(page.page.locator('.btn-option').filter({ hasText: 'Resume Pomodoro' })).toHaveClass(/default/);

    await page.page.locator('.btn-option').filter({ hasText: 'Resume Pomodoro' }).click();
    await expect(page.page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 5000 });

    await expect(page.page.locator('.timer-mode-label')).toContainText('FOCUSING');
    const timeAfterResume = await page.getTimerDisplay();
    expect(timeAfterResume).not.toBe('25:00');
    expect(timeAfterResume).toBe(timeBeforeSwitch);
  });

  test('should not show Resume Pomodoro when no pomodoro was interrupted', async () => {
    await page.goto('/settings');
    await expect(page.page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await page.setPomodoroMinutes(1);
    await page.goto('/');
    await expect(page.page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.addTask('No Interrupt Test');
    await page.selectTask('No Interrupt Test');
    await page.startTimer();
    await page.completePomodoroFast();

    await expect(page.page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.page.locator('.btn-option').filter({ hasText: 'Resume Pomodoro' })).not.toBeVisible();
    await page.skipConsentModal();
  });
});
