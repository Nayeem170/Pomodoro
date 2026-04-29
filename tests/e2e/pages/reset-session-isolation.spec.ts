import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Reset Session Isolation', () => {
  test('resetting one session should not clear other paused sessions', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Reset Test');
    await pomodoroPage.selectTask('Reset Test');
    await page.locator('button[aria-label="Start timer"]').click();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });

    await page.clock.install({ time: Date.now() });
    await page.clock.fastForward(3000);

    const timeBeforeSwitch = await pomodoroPage.getTimerDisplay();

    await pomodoroPage.switchToShortBreak();

    const timerType = await pomodoroPage.getTimerType();
    expect(timerType).toContain('BREAK');

    await page.locator('button[aria-label="Reset timer"]').click();

    await pomodoroPage.switchToPomodoro();

    const timerTypeAfter = await pomodoroPage.getTimerType();
    expect(timerTypeAfter).toContain('FOCUSING');

    const isPaused = await pomodoroPage.isTimerPaused();
    expect(isPaused).toBe(true);

    const timeAfterReset = await pomodoroPage.getTimerDisplay();
    expect(timeAfterReset).toBe(timeBeforeSwitch);
  });
});
