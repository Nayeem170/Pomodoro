import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Timer Controls', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display timer controls', async ({ page }) => {
    await expect(page.locator('.timer-card')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.timer-controls')).toBeVisible();
  });

  test('should display start button initially', async ({ page }) => {
    await expect(page.locator('.timer-controls button.ibtn.lg')).toBeVisible({ timeout: 30000 });
  });

  test('should show task hint when no task is selected', async ({ page }) => {
    await expect(page.locator('.active-task')).toBeVisible();
    await expect(page.locator('.task-hint')).toBeVisible();
    await expect(page.locator('.task-hint')).toContainText('Select a task to start');
  });

  test('should not be able to start timer without task', async ({ page }) => {
    const startButton = page.locator('button[aria-label="Select a task first"]');
    await expect(startButton).toBeDisabled();
  });

  test('should start timer when task is selected', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should show pause button when timer is running', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should pause timer', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const pauseButton = page.locator('button[aria-label="Pause timer"]');
    await pauseButton.click();
    
    await expect(page.locator('button[aria-label="Resume timer"]')).toBeVisible();
  });

  test('should show resume button when timer is paused', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const pauseButton = page.locator('button[aria-label="Pause timer"]');
    await pauseButton.click();
    await page.waitForTimeout(500);
    
    await expect(page.locator('button[aria-label="Resume timer"]')).toBeVisible();
  });

  test('should show reset button when timer is started', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    await expect(page.locator('button[aria-label="Reset timer"]')).toBeVisible();
  });

  test('should resume timer', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const pauseButton = page.locator('button[aria-label="Pause timer"]');
    await pauseButton.click();
    await page.waitForTimeout(500);
    
    const resumeButton = page.locator('button[aria-label="Resume timer"]');
    await resumeButton.click();
    
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should reset timer', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    const startButton = page.locator('button[aria-label="Start timer"]');
    await startButton.click();
    await page.waitForTimeout(500);
    
    const resetButton = page.locator('button[aria-label="Reset timer"]');
    await resetButton.click();
    
    await expect(page.locator('button[aria-label="Start timer"]')).toBeVisible();
  });

  test('should display current task indicator', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-row');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    await expect(page.locator('.active-task')).toBeVisible();
    await expect(page.locator('.active-task')).toContainText('Test Task');
  });

  test('should show select task prompt when no task selected', async ({ page }) => {
    await expect(page.locator('button[aria-label="Select a task first"]')).toBeDisabled();
    await expect(page.locator('.task-hint')).toBeVisible();
    await expect(page.locator('.task-hint')).toContainText('Select a task to start');
  });
});
