import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

// Index page requires more time due to Blazor WASM initialization and data loading
test.describe('Index Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  // Set longer timeout for all tests in this describe block
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
    await expect(page.locator('.task-list')).toBeVisible();
  });

  test('should render timer section', async ({ page }) => {
    await expect(page.locator('.timer-section')).toBeVisible();
    await expect(page.locator('.timer-controls')).toBeVisible();
  });

  test('should render session tabs', async ({ page }) => {
    await expect(page.locator('.session-tabs')).toBeVisible();
    await expect(page.locator('button:has-text("Pomodoro")')).toBeVisible();
    await expect(page.locator('button:has-text("Short Break")')).toBeVisible();
    await expect(page.locator('button:has-text("Long Break")')).toBeVisible();
  });

  test('should switch to Short Break', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToShortBreak();
    const shortBreakButton = page.locator('button:has-text("Short Break")');
    await expect(shortBreakButton).toHaveClass(/active/);
  });

  test('should switch to Long Break', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToLongBreak();
    const longBreakButton = page.locator('button:has-text("Long Break")');
    await expect(longBreakButton).toHaveClass(/active/);
  });

  test('should switch back to Pomodoro', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.switchToShortBreak();
    await pomodoroPage.switchToPomodoro();
    const pomodoroButton = page.locator('button:has-text("Pomodoro")');
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should render summary section', async ({ page }) => {
    await expect(page.locator('.summary-section')).toBeVisible({ timeout: 30000 });
  });

  test('should render header actions', async ({ page }) => {
    await expect(page.locator('.header-actions')).toBeVisible();
    await expect(page.locator('button:has-text("?")')).toBeVisible();
    await expect(page.locator('button:has-text("⧉")')).toBeVisible();
  });
});
