import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Ordering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should display most recently added task first', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Alpha Task');
    await page.waitForTimeout(300);
    await pomodoroPage.addTask('Beta Task');
    await page.waitForTimeout(300);
    await pomodoroPage.addTask('Gamma Task');
    await page.waitForTimeout(300);

    const taskItems = page.locator('.task-items .task-item');
    const count = await taskItems.count();
    expect(count).toBeGreaterThanOrEqual(3);

    const firstTask = taskItems.first();
    await expect(firstTask).toContainText('Gamma Task');

    const lastTask = taskItems.last();
    await expect(lastTask).toContainText('Alpha Task');
  });

  test('should move completed task to completed section', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Complete Order A');
    await page.waitForTimeout(300);
    await pomodoroPage.addTask('Complete Order B');
    await page.waitForTimeout(300);

    await pomodoroPage.completeTask('Complete Order A');
    await page.waitForTimeout(1000);

    await expect(page.locator('.completed-section')).toBeVisible();
    const completedTask = page.locator('.completed-section .task-item').filter({ hasText: 'Complete Order A' });
    await expect(completedTask).toBeVisible();
    await expect(completedTask).toHaveClass(/completed/);

    const activeTask = page.locator('.task-items').locator('.task-item').filter({ hasText: 'Complete Order B' });
    await expect(activeTask).toBeVisible();
    await expect(activeTask).not.toHaveClass(/completed/);
  });
});
