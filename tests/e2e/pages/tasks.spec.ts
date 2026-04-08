import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Management', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display task list', async ({ page }) => {
    await expect(page.locator('.task-list')).toBeVisible();
    await expect(page.locator('.task-list-header h3')).toContainText('📋 TASKS');
  });

  test('should display add task button', async ({ page }) => {
    await expect(page.locator('.btn-add-task')).toBeVisible();
    await expect(page.locator('.btn-add-task')).toContainText('+ Add Task');
  });

  test('should show task input when add task button is clicked', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await expect(page.locator('.add-task-form')).toBeVisible();
    await expect(page.locator('.task-input')).toBeVisible();
  });

  test('should add a new task', async ({ page }) => {
    const initialCount = await page.locator('.task-item').count();
    
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const newCount = await page.locator('.task-item').count();
    expect(newCount).toBe(initialCount + 1);
    
    await expect(page.locator('.task-item')).toContainText('Test Task');
  });

  test('should select a task', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    await expect(page.locator('.task-item.selected')).toHaveCount(1);
  });

  test('should complete a task', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Click the complete button (✓ icon) - first button for non-completed task
    const completeButton = page.locator('.task-item.selected .btn-icon').first();
    await completeButton.click();
    await page.waitForTimeout(500);
    
    await expect(page.locator('.task-item.completed')).toHaveCount(1);
  });

  test('should uncomplete a task', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    // Click the complete button first
    const completeButton = page.locator('.task-item.selected .btn-icon').first();
    await completeButton.click();
    await page.waitForTimeout(500);

    // Then click the undo button - use the first task item's button
    const firstTaskItem = page.locator('.task-item').first();
    const undoButton = firstTaskItem.locator('.btn-icon').first();
    await undoButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.task-item.completed')).toHaveCount(0);
  });

  test('should delete a task', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    
    const initialCount = await page.locator('.task-item').count();
    
    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);
    
    // Click the delete button (🗑 icon) - second button for non-completed task
    const deleteButton = page.locator('.task-item.selected .btn-icon').nth(1);
    await deleteButton.click();
    await page.waitForTimeout(500);
    
    const newCount = await page.locator('.task-item').count();
    expect(newCount).toBe(initialCount - 1);
  });

  test('should cancel adding a task', async ({ page }) => {
    await page.locator('.btn-add-task').click();
    await expect(page.locator('.add-task-form')).toBeVisible();
    
    await page.locator('.btn-icon-small.btn-cancel').click();
    await expect(page.locator('.add-task-form')).not.toBeVisible();
  });
});
