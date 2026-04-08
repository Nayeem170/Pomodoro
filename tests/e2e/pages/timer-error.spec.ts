import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Error Handling', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should disable start button when no task is selected for pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const startButton = page.locator('.btn-start');
    await expect(startButton).toBeDisabled();
    await expect(startButton).toHaveAttribute('title', 'Select a task first');

    await expect(page.locator('.task-hint')).toContainText('Select a task to start');
  });

  test('should enable start button after selecting a task for pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const startButton = page.locator('.btn-start');
    await expect(startButton).toBeDisabled();

    await pomodoroPage.addTask('Enable Test Task');
    await page.waitForTimeout(500);

    await expect(startButton).toBeEnabled();
    await expect(startButton).toHaveAttribute('title', 'Start');
  });

  test('should not show error when starting break without a task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.startTimer();

    await expect(page.locator('.error-banner')).not.toBeVisible({ timeout: 3000 });
    await expect(page.locator('.btn-pause')).toBeVisible({ timeout: 5000 });
    await pomodoroPage.pauseTimer();
  });
});
