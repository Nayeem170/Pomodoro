import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Persistence', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should reset to full duration after page reload (timer does not persist)', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Timer Persist Task');
    await pomodoroPage.selectTask('Timer Persist Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(2000);

    await pomodoroPage.pauseTimer();
    await expect(page.locator('.btn-resume')).toBeVisible();

    const pausedDisplay = await pomodoroPage.getTimerDisplay();
    expect(pausedDisplay).not.toBe('25:00');

    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toBe('25:00');
  });

  test('should reset to full duration after reload when timer was started', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Time Persist Task');
    await pomodoroPage.selectTask('Time Persist Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
    await page.waitForTimeout(2000);

    const runningDisplay = await pomodoroPage.getTimerDisplay();
    expect(runningDisplay).not.toBe('25:00');

    await pomodoroPage.pauseTimer();
    await expect(page.locator('.btn-resume')).toBeVisible();

    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(1000);

    const reloadedDisplay = await pomodoroPage.getTimerDisplay();
    expect(reloadedDisplay).toBe('25:00');
  });

  test('should reset to full duration after reload when timer was not started', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const initialDisplay = await pomodoroPage.getTimerDisplay();

    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const reloadedDisplay = await pomodoroPage.getTimerDisplay();
    expect(reloadedDisplay).toBe(initialDisplay);
    expect(reloadedDisplay).toMatch(/25:00/);
  });
});
