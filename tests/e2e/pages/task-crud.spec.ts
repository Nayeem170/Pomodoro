import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe.configure({ mode: 'serial' });

test.describe('Task CRUD', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should add a task and verify it appears in the task list', async ({ page }) => {
    await pomodoroPage.addTask('First task');
    await expect(page.locator('.task-row').filter({ hasText: 'First task' })).toBeVisible();
    await expect(page.locator('.task-row .task-text').filter({ hasText: 'First task' })).toBeVisible();
  });

  test('should add multiple tasks and verify order', async ({ page }) => {
    await pomodoroPage.addTask('Second task');
    await pomodoroPage.addTask('Third task');
    const taskRows = page.locator('.task-items .task-row');
    await expect(taskRows.filter({ hasText: 'First task' })).toBeVisible();
    await expect(taskRows.filter({ hasText: 'Second task' })).toBeVisible();
    await expect(taskRows.filter({ hasText: 'Third task' })).toBeVisible();
  });

  test('should complete a task and show it as completed', async ({ page }) => {
    await pomodoroPage.completeTask('First task');
    await expect(page.locator('.completed-section .task-row').filter({ hasText: 'First task' })).toBeVisible();
    await expect(page.locator('.completed-section .task-text.completed').filter({ hasText: 'First task' })).toBeVisible();
  });

  test('should delete a task and verify it is removed', async ({ page }) => {
    await pomodoroPage.deleteTask('Second task');
    await expect(page.locator('.task-row').filter({ hasText: 'Second task' })).toHaveCount(0);
  });

  test('should select a task and update active task indicator', async ({ page }) => {
    await pomodoroPage.selectTask('Third task');
    await expect(page.locator('.active-task')).toContainText('Third task');
  });

  test('should disable add button when task input is empty', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.task-input')).toBeVisible();
    await expect(page.locator('.btn-icon-small.btn-add')).toBeDisabled();
  });

  test('should trim whitespace from task name', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await page.locator('.task-input').click();
    await page.locator('.task-input').pressSequentially('  Trimmed task  ');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.task-row').filter({ hasText: 'Trimmed task' })).toBeVisible();
    await expect(page.locator('.task-row').filter({ hasText: '  Trimmed task  ' })).toHaveCount(0);
  });
});
