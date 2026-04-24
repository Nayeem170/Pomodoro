import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Validation', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should disable add button when task name is empty', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    const addButton = page.locator('.btn-icon-small.btn-add');
    await expect(addButton).toBeDisabled();
  });

  test('should enable add button when task name is entered', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.task-input').fill('Valid Task');
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-icon-small.btn-add');
    await expect(addButton).toBeEnabled();
  });

  test('should disable add button when task name is whitespace only', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.task-input').fill('   ');
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-icon-small.btn-add');
    await expect(addButton).toBeDisabled();
  });

  test('should allow adding task with duplicate name', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Duplicate Task');
    await pomodoroPage.addTask('Duplicate Task');

    const taskCount = await page.locator('.task-row').filter({ hasText: 'Duplicate Task' }).count();
    expect(taskCount).toBe(2);
  });

  test('should add task by pressing Enter key', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.task-input').fill('Enter Key Task');
    await page.locator('.task-input').press('Enter');
    await page.waitForTimeout(500);

    await expect(page.locator('.task-row')).toContainText('Enter Key Task');
    await expect(page.locator('.add-task-form')).not.toBeVisible();
  });

  test('should cancel adding task by pressing Escape key', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.add-task-form')).toBeVisible();

    await page.locator('.task-input').fill('Escape Key Task');
    await page.locator('.btn-icon-small.btn-cancel').click();
    await page.waitForTimeout(300);

    await expect(page.locator('.add-task-form')).not.toBeVisible();
    await expect(page.locator('.task-row').filter({ hasText: 'Escape Key Task' })).toHaveCount(0);
  });

  test('should allow adding task with long name', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
    await page.locator('.task-add-btn').click();

    const longName = 'A'.repeat(200);
    await page.locator('.task-input').fill(longName);
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-icon-small.btn-add');
    await expect(addButton).toBeEnabled();

    await addButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.task-row')).toContainText(longName);
  });
});
