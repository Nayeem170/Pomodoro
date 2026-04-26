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
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Alpha Task');
    await page.waitForTimeout(300);
    await pomodoroPage.addTask('Beta Task');
    await page.waitForTimeout(300);
    await pomodoroPage.addTask('Gamma Task');
    await page.waitForTimeout(300);

    const taskRows = page.locator('.task-items .task-row');
    const count = await taskRows.count();
    expect(count).toBeGreaterThanOrEqual(3);

    const firstTask = taskRows.first();
    await expect(firstTask).toContainText('Gamma Task');

    const lastTask = taskRows.last();
    await expect(lastTask).toContainText('Alpha Task');
  });
});
