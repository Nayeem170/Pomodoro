import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Countdown', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should display initial timer value matching default pomodoro duration', async ({ page }) => {
    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/25:00/);
  });

  test('should display correct duration when switched to Short Break', async ({ page }) => {
    await pomodoroPage.switchToShortBreak();
    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/05:00/);
  });

  test('should display correct duration when switched to Long Break', async ({ page }) => {
    await pomodoroPage.switchToLongBreak();
    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/15:00/);
  });

  test('should decrement timer when running', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Countdown Test');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.locator('button[aria-label="Start timer"]').click();
    await page.waitForTimeout(2000);

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).not.toBe('25:00');
    expect(timerDisplay).toMatch(/\d{1,2}:\d{2}/);
  });

  test('should stop decrementing when paused', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Pause Test');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.locator('button[aria-label="Start timer"]').click();
    await page.waitForTimeout(2000);

    await page.locator('button[aria-label="Pause timer"]').click();
    await page.waitForTimeout(500);

    const pausedTime = await pomodoroPage.getTimerDisplay();
    await page.waitForTimeout(2000);

    const stillPausedTime = await pomodoroPage.getTimerDisplay();
    expect(stillPausedTime).toBe(pausedTime);
  });

  test('should reset timer to full duration', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Reset Test');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.locator('button[aria-label="Start timer"]').click();
    await page.waitForTimeout(2000);

    await page.locator('button[aria-label="Reset timer"]').click();
    await page.waitForTimeout(500);

    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/25:00/);
  });

  test('should apply paused theme class when timer is paused', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Theme Test');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.locator('button[aria-label="Start timer"]').click();
    await page.waitForTimeout(1000);

    await page.locator('button[aria-label="Pause timer"]').click();
    await page.waitForTimeout(500);

    const timerCard = page.locator('.timer-card');
    await expect(timerCard).toHaveClass(/paused/);
  });
});
