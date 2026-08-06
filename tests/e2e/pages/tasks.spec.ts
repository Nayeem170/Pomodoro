import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Management', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display task list', async ({ page }) => {
    await expect(page.locator('.task-card')).toBeVisible();
  });

  test('should display add task input', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible();
  });

  test('should always show task input', async ({ page }) => {
    await expect(page.locator('.add-task-form')).toBeVisible();
    await expect(page.locator('.task-input')).toBeVisible();
  });

  test('should add a new task', async ({ page }) => {
    const initialCount = await page.locator('.task-row').count();

    await page.locator('.task-input').pressSequentially('Test Task');
    await page.locator('.btn-add-text').click();
    await page.waitForTimeout(500);

    const newCount = await page.locator('.task-row').count();
    expect(newCount).toBe(initialCount + 1);

    await expect(page.locator('.task-row')).toContainText('Test Task');
  });

  test('should select a task', async ({ page }) => {
    await page.locator('.task-input').pressSequentially('Test Task');
    await page.locator('.btn-add-text').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    await expect(page.locator('.task-row.selected')).toHaveCount(1);
  });

  test('should complete a task', async ({ page }) => {
    await page.locator('.task-input').pressSequentially('Test Task');
    await page.locator('.btn-add-text').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    const completeButton = page.locator('.task-row.selected .task-checkbox').first();
    await completeButton.click();
    await page.waitForTimeout(500);

    await page.locator('.completed-toggle').click();
    await page.waitForTimeout(200);

    await expect(page.locator('.completed-section .task-row')).toHaveCount(1);
  });

  test('should uncomplete a task', async ({ page }) => {
    await page.locator('.task-input').pressSequentially('Test Task');
    await page.locator('.btn-add-text').click();
    await page.waitForTimeout(500);

    const taskRows = page.locator('.task-row');
    await taskRows.first().click();
    await page.waitForTimeout(200);

    const completeButton = page.locator('.task-row.selected .task-checkbox').first();
    await completeButton.click();
    await page.waitForTimeout(500);

    await page.locator('.completed-toggle').click();
    await page.waitForTimeout(200);

    const completedTask = page.locator('.completed-section .task-row').first();
    const undoButton = completedTask.locator('.task-checkbox').first();
    await undoButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section .task-row')).toHaveCount(0);
  });

  test('should delete a task', async ({ page }) => {
    await page.locator('.task-input').pressSequentially('Test Task');
    await page.locator('.btn-add-text').click();
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

  test('should clear task input with Escape key', async ({ page }) => {
    await page.locator('.task-input').pressSequentially('Some task');
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.task-input').press('Escape');
    await page.waitForTimeout(300);

    await expect(page.locator('.add-task-form')).toBeVisible();
    await expect(page.locator('.task-input')).toHaveValue('');
  });
});
