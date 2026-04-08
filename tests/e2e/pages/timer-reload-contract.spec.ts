import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Reload Contract', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should not persist running timer state across page reload', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Reload Contract Task');
    await pomodoroPage.selectTask('Reload Contract Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(3000);

    const runningDisplay = await pomodoroPage.getTimerDisplay();
    expect(runningDisplay).not.toBe('25:00');

    await page.reload();
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const reloadedDisplay = await pomodoroPage.getTimerDisplay();
    expect(reloadedDisplay).toMatch(/25:00/);

    const isRunning = await pomodoroPage.isTimerRunning();
    expect(isRunning).toBe(false);
  });

  test('should show start button after reload regardless of previous timer state', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Start Btn Reload Task');
    await pomodoroPage.selectTask('Start Btn Reload Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await pomodoroPage.pauseTimer();
    await expect(page.locator('.btn-resume')).toBeVisible();

    await page.reload();
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    await expect(page.locator('.btn-start')).toBeVisible();
    const isPaused = await pomodoroPage.isTimerPaused();
    expect(isPaused).toBe(false);
  });
});
