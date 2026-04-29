import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Session Switch Preservation', () => {
  let pomodoroPage: PomodoroPage;

  test('should preserve paused state and remaining time when switching session type mid-timer', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.startTimer();

    await page.waitForTimeout(3000);

    const timeBeforeSwitch = await pomodoroPage.getTimerDisplay();

    await pomodoroPage.switchToShortBreak();

    const timerType = await pomodoroPage.getTimerType();
    expect(timerType).toContain('BREAK');

    await pomodoroPage.switchToPomodoro();

    const timerTypeAfter = await pomodoroPage.getTimerType();
    expect(timerTypeAfter).toContain('FOCUSING');

    const isPaused = await pomodoroPage.isTimerPaused();
    expect(isPaused).toBe(true);

    const timeAfterSwitch = await pomodoroPage.getTimerDisplay();
    expect(timeAfterSwitch).toBe(timeBeforeSwitch);
  });

  test('should show reset button (not start button) after switching back to original session type', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.fastSetup1MinPomodoro();
    await pomodoroPage.startTimer();
    await page.waitForTimeout(2000);

    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.switchToPomodoro();

    const isResetVisible = await pomodoroPage.isTimerStarted();
    expect(isResetVisible).toBe(true);
  });
});
