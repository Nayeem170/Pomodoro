import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Reset Session Isolation', () => {
  test('resetting one session should not clear other paused sessions', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Reset Test');
    await pomodoroPage.selectTask('Reset Test');
    await pomodoroPage.startTimer();

    await pomodoroPage.pauseTimer();

    const pomodoroTime = await pomodoroPage.getTimerDisplay();

    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.startTimer();

    await pomodoroPage.pauseTimer();
    await pomodoroPage.resetTimer();

    await pomodoroPage.switchToPomodoro();

    const timerTypeAfter = await pomodoroPage.getTimerType();
    expect(timerTypeAfter).toContain('FOCUSING');

    const isPaused = await pomodoroPage.isTimerPaused();
    expect(isPaused).toBe(true);

    const timeAfterReset = await pomodoroPage.getTimerDisplay();
    expect(timeAfterReset).toBe(pomodoroTime);
  });
});
