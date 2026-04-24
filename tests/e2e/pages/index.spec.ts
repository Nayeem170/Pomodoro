import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Index Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should render main container', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible();
  });

  test('should render timer display', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    const timerDisplay = await pomodoroPage.getTimerDisplay();
    expect(timerDisplay).toMatch(/\d{1,2}:\d{2}/);
  });

  test('should render timer type', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType).toBeTruthy();
  });

  test('should render task list section', async ({ page }) => {
    await expect(page.locator('.tasks-section')).toBeVisible();
    await expect(page.locator('.task-card')).toBeVisible();
  });

  test('should render timer section', async ({ page }) => {
    await expect(page.locator('.timer-card')).toBeVisible();
    await expect(page.locator('.timer-controls')).toBeVisible();
  });

  test('should render session tabs', async ({ page }) => {
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' })).toBeVisible();
    await expect(page.locator('.mode-tabs button').filter({ hasText: 'Short break' })).toBeVisible();
    await expect(page.locator('.mode-tabs button').filter({ hasText: 'Long break' })).toBeVisible();
  });

  test('should switch to Short Break', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToShortBreak();
    const shortBreakButton = page.locator('.mode-tabs button').filter({ hasText: 'Short break' });
    await expect(shortBreakButton).toHaveClass(/active/);
  });

  test('should switch to Long Break', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToLongBreak();
    const longBreakButton = page.locator('.mode-tabs button').filter({ hasText: 'Long break' });
    await expect(longBreakButton).toHaveClass(/active/);
  });

  test('should switch back to Pomodoro', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.switchToPomodoro();
    const pomodoroButton = page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should render today summary', async ({ page }) => {
    await expect(page.locator('.pomo-row')).toBeVisible({ timeout: 30000 });
  });

  test('should render pip and keyboard help buttons', async ({ page }) => {
    await expect(page.locator('button[aria-label="Picture in Picture"]')).toBeVisible();
    await expect(page.locator('button[aria-label="Keyboard shortcuts"]')).toBeVisible();
  });
});
