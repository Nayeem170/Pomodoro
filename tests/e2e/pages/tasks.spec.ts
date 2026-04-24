import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Management', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display task list', async ({ page }) => {
    await expect(page.locator('.task-card')).toBeVisible();
    await expect(page.locator('.task-card-title')).toContainText('Tasks');
  });

  test('should display add task button', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible();
    await expect(page.locator('.task-add-btn')).toContainText('+ Add');
  });

  test('should show task input when add task button is clicked', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();
    await expect(page.locator('.task-input')).toBeVisible();
  });

  test('should add a new task', async ({ page }) => {
    const initialCount = await page.locator('.task-row').count();

    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const newCount = await page.locator('.task-row').count();
    expect(newCount).toBe(initialCount + 1);

    await expect(page.locator('.task-row')).toContainText('Test Task');
  });

  test('should select a task', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    await expect(page.locator('.task-row.selected')).toHaveCount(1);
  });

  test('should complete a task', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    const completeButton = page.locator('.task-row.selected .task-checkbox').first();
    await completeButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section .task-row')).toHaveCount(1);
  });

  test('should uncomplete a task', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    const completeButton = page.locator('.task-row.selected .task-checkbox').first();
    await completeButton.click();
    await page.waitForTimeout(500);

    const completedTask = page.locator('.completed-section .task-row').first();
    const undoButton = completedTask.locator('.task-checkbox').first();
    await undoButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section .task-row')).toHaveCount(0);
  });

  test('should delete a task', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').fill('Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const initialCount = await page.locator('.task-row').count();

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    const deleteButton = page.locator('.task-row.selected .task-action-btn.delete').first();
    await deleteButton.click();
    await page.waitForTimeout(500);

    const newCount = await page.locator('.task-row').count();
    expect(newCount).toBe(initialCount - 1);
  });

  test('should cancel adding a task', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.btn-icon-small.btn-cancel').click();
    await expect(page.locator('.add-task-form')).not.toBeVisible();
  });
});
