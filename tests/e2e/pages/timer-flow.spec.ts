import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe.configure({ mode: 'serial' });

test.describe('Timer Flow', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should add a task, select it, start pomodoro, and verify timer counts down', async ({ page }) => {
    await pomodoroPage.addTask('Flow test task');
    await expect(page.locator('.task-row').filter({ hasText: 'Flow test task' })).toBeVisible();
    await pomodoroPage.selectTask('Flow test task');
    await expect(page.locator('.active-task')).toContainText('Flow test task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    const timeBefore = await pomodoroPage.getTimerDisplay();
    await page.waitForTimeout(2000);
    const timeAfter = await pomodoroPage.getTimerDisplay();
    expect(timeBefore).not.toBe(timeAfter);
  });

  test('should pause and resume the timer', async ({ page }) => {
    await pomodoroPage.pauseTimer();
    await expect(page.locator('button[aria-label="Resume timer"]')).toBeVisible();
    await pomodoroPage.resumeTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should reset the timer', async ({ page }) => {
    await pomodoroPage.resetTimer();
    await expect(page.locator('button[aria-label="Start timer"]')).toBeVisible();
  });

  test('should switch between pomodoro, short break, and long break modes', async ({ page }) => {
    await pomodoroPage.switchToShortBreak();
    await expect(page.locator('.timer-mode-label')).toContainText('SHORT BREAK');

    await pomodoroPage.switchToLongBreak();
    await expect(page.locator('.timer-mode-label')).toContainText('LONG BREAK');

    await pomodoroPage.switchToPomodoro();
    await expect(page.locator('.timer-mode-label')).toContainText('FOCUSING');
  });

  test('should show active task indicator with selected task name', async ({ page }) => {
    await pomodoroPage.selectTask('Flow test task');
    await expect(page.locator('.active-task')).toContainText('Flow test task');
  });

  test('should complete a task and move it to completed section', async ({ page }) => {
    await pomodoroPage.completeTask('Flow test task');
    await expect(page.locator('.completed-section')).toBeVisible();
    await expect(page.locator('.completed-section .task-row').filter({ hasText: 'Flow test task' })).toBeVisible();
  });
});
