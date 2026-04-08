import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Debug Test Cleanup', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should find task input on main page with correct selector', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Input Debug Task');
    await page.waitForTimeout(500);

    const taskItem = page.locator('.task-item').filter({ hasText: 'Input Debug Task' });
    await expect(taskItem).toBeVisible();
  });

  test('should allow typing in task input via add task form', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await page.locator('.btn-add-task').click();
    await page.waitForTimeout(500);

    const taskInput = page.locator('.task-input');
    await expect(taskInput).toBeVisible();
    await taskInput.fill('Typed Task Name');
    await page.waitForTimeout(300);

    const inputValue = await taskInput.inputValue();
    expect(inputValue).toBe('Typed Task Name');

    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    await expect(page.locator('.task-item')).toContainText('Typed Task Name');
  });
});
