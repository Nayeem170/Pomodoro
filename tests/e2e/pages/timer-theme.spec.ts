import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Theme', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should apply pomodoro theme class by default', async ({ page }) => {
    const timerCard = page.locator('.timer-card');
    await expect(timerCard).toHaveClass(/pomodoro/);
  });

  test('should apply short-break theme when switched', async ({ page }) => {
    await pomodoroPage.switchToShortBreak();
    const timerCard = page.locator('.timer-card');
    await expect(timerCard).toHaveClass(/short-break/);
  });

  test('should apply long-break theme when switched', async ({ page }) => {
    await pomodoroPage.switchToLongBreak();
    const timerCard = page.locator('.timer-card');
    await expect(timerCard).toHaveClass(/long-break/);
  });

  test('should revert to pomodoro theme when switching back', async ({ page }) => {
    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.switchToPomodoro();
    const timerCard = page.locator('.timer-card');
    await expect(timerCard).toHaveClass(/pomodoro/);
  });

  test('should display correct timer type label for Pomodoro', async ({ page }) => {
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('POMODORO');
  });

  test('should display correct timer type label for Short Break', async ({ page }) => {
    await pomodoroPage.switchToShortBreak();
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('SHORT BREAK');
  });

  test('should display correct timer type label for Long Break', async ({ page }) => {
    await pomodoroPage.switchToLongBreak();
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('LONG BREAK');
  });
});
