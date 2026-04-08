import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Controls', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display timer controls', async ({ page }) => {
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.timer-controls')).toBeVisible();
  });

  test('should display start button initially', async ({ page }) => {
    await expect(page.locator('.btn-start')).toBeVisible({ timeout: 30000 });
  });

  test('should show task hint when no task is selected', async ({ page }) => {
    await expect(page.locator('.current-task-indicator')).toBeVisible();
    await expect(page.locator('.task-hint')).toBeVisible();
    await expect(page.locator('.task-hint')).toContainText('Select a task to start');
  });

  test('should not be able to start timer without task', async ({ page }) => {
    const startButton = page.locator('.btn-start');
    await expect(startButton).toBeDisabled();
  });

  test('should start timer when task is selected', async ({ page }) => {
    // Add a task first
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    // Select the task
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Click start button
    const startButton = page.locator('.btn-start');
    await startButton.click();
    
    // Verify timer is running
    await expect(page.locator('.btn-pause')).toBeVisible();
  });

  test('should show pause button when timer is running', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    // Verify pause button is visible
    await expect(page.locator('.btn-pause')).toBeVisible();
  });

  test('should pause timer', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    // Pause timer
    const pauseButton = page.locator('.btn-pause');
    await pauseButton.click();
    
    // Verify resume button is visible
    await expect(page.locator('.btn-resume')).toBeVisible();
  });

  test('should show resume button when timer is paused', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start and pause timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const pauseButton = page.locator('.btn-pause');
    await pauseButton.click();
    await page.waitForTimeout(500);
    
    // Verify resume button is visible
    await expect(page.locator('.btn-resume')).toBeVisible();
  });

  test('should show reset button when timer is started', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    // Verify reset button is visible
    await expect(page.locator('.btn-reset')).toBeVisible();
  });

  test('should resume timer', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start and pause timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const pauseButton = page.locator('.btn-pause');
    await pauseButton.click();
    await page.waitForTimeout(500);
    
    // Resume timer
    const resumeButton = page.locator('.btn-resume');
    await resumeButton.click();
    
    // Verify pause button is visible again
    await expect(page.locator('.btn-pause')).toBeVisible();
  });

  test('should reset timer', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Start timer
    const startButton = page.locator('.btn-start');
    await startButton.click();
    await page.waitForTimeout(500);
    
    // Reset timer
    const resetButton = page.locator('.btn-reset');
    await resetButton.click();
    
    // Verify start button is visible again
    await expect(page.locator('.btn-start')).toBeVisible();
  });

  test('should display current task indicator', async ({ page }) => {
    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Verify current task indicator is visible
    await expect(page.locator('.current-task-indicator')).toBeVisible();
    await expect(page.locator('.current-task-indicator')).toContainText('Current Task');
  });

  test('should show select task prompt when no task selected', async ({ page }) => {
    // Verify task hint is shown when no task is selected
    await expect(page.locator('.btn-start')).toBeDisabled();
    await expect(page.locator('.task-hint')).toBeVisible();
    await expect(page.locator('.task-hint')).toContainText('Select a task to start');
  });
});
