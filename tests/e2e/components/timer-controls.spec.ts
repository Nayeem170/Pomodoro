import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Controls Component', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    // Add a task so we can start the timer
    await pomodoroPage.addTask('Test Task');
    await pomodoroPage.selectTask('Test Task');
  });

  test('should display start button initially', async ({ page }) => {
    // First check if the timer-controls container is visible
    await expect(page.locator('.timer-controls')).toBeVisible({ timeout: 10000 });
    
    // Then check if the start button is visible
    await expect(page.locator('.btn-start')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('.btn-start')).toHaveClass(/btn-start/);
  });

  test('should display pause button when timer is running', async ({ page }) => {
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-pause')).toBeVisible();
  });

  test('should display resume button when timer is paused', async ({ page }) => {
    await pomodoroPage.startTimer();
    await pomodoroPage.pauseTimer();
    await expect(page.locator('.btn-resume')).toBeVisible();
  });

  test('should display reset button after starting timer', async ({ page }) => {
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-reset')).toBeVisible();
  });

  test('should not display reset button initially', async ({ page }) => {
    await expect(page.locator('.btn-reset')).not.toBeVisible();
  });

  test('should hide start button after starting timer', async ({ page }) => {
    await pomodoroPage.startTimer();
    await expect(page.locator('.btn-start')).not.toBeVisible();
  });

  test('should have correct button classes', async ({ page }) => {
    await expect(page.locator('.btn-icon-large')).toBeVisible();
  });
});
