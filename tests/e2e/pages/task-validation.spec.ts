import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Validation', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should disable add button when task name is empty', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeDisabled();
  });

  test('should enable add button when task name is entered', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-input').pressSequentially('Valid Task');
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeEnabled();
  });

  test('should disable add button when task name is whitespace only', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-input').pressSequentially('   ');
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeDisabled();
  });

  test('should allow adding task with duplicate name', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Duplicate Task');
    await expect(page.locator('.task-row').filter({ hasText: 'Duplicate Task' })).toBeVisible({ timeout: 5000 });
    await pomodoroPage.addTask('Duplicate Task');
    await expect(page.locator('.task-row').filter({ hasText: 'Duplicate Task' }).nth(1)).toBeVisible({ timeout: 5000 });

    const taskCount = await page.locator('.task-row').filter({ hasText: 'Duplicate Task' }).count();
    expect(taskCount).toBe(2);
  });

  test('should add task by pressing Enter key', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-input').pressSequentially('Enter Key Task');
    await page.locator('.task-input').press('Enter');
    await page.waitForTimeout(500);

    await expect(page.locator('.task-row')).toContainText('Enter Key Task');
    await expect(page.locator('.task-input')).toHaveValue('');
  });

  test('should clear task input with Escape key', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-input').pressSequentially('Escape Key Task');
    await page.locator('.task-input').press('Escape');
    await page.waitForTimeout(300);

    await expect(page.locator('.task-input')).toHaveValue('');
    await expect(page.locator('.task-row').filter({ hasText: 'Escape Key Task' })).toHaveCount(0);
  });

  test('should allow adding task with long name', async ({ page }) => {
    await expect(page.locator('.task-input')).toBeVisible({ timeout: 30000 });

    const longName = 'A'.repeat(200);
    await page.locator('.task-input').pressSequentially(longName);
    await page.waitForTimeout(200);

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeEnabled();

    await addButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.task-row')).toContainText(longName);
  });
});
