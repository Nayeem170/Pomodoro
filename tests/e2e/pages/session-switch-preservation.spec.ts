import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Session Switch Preservation', () => {
  test('should preserve paused state and remaining time when switching session type mid-timer', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await pomodoroPage.setSettingViaIndexedDB('pomodoroMinutes', 1);
    await page.reload();
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.locator('button[aria-label="Start timer"]').click();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });

    await page.clock.install({ time: Date.now() });
    await page.clock.fastForward(3000);

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

  test('should show resume button after switching back to original session type', async ({ browser }) => {
    const context = await browser.newContext();
    try {
      const page = await context.newPage();
      const pomodoroPage = new PomodoroPage(page);
      await pomodoroPage.goto('/');
      await pomodoroPage.setSettingViaIndexedDB('pomodoroMinutes', 1);
      await page.reload();
      await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

      await page.locator('button[aria-label="Start timer"]').click();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });

      await page.clock.install({ time: Date.now() });
      await page.clock.fastForward(2000);

      await pomodoroPage.switchToShortBreak();
      await pomodoroPage.switchToPomodoro();

      const isResumeVisible = await pomodoroPage.isTimerPaused();
      expect(isResumeVisible).toBe(true);
    } finally {
      await context.close();
    }
  });
});
