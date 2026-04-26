import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Behavior', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should auto-select newly added task as current', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('First Task');
    await pomodoroPage.selectTask('First Task');

    const selectedCountBefore = await page.locator('.task-row.selected').count();
    expect(selectedCountBefore).toBe(1);
    await expect(page.locator('.task-row.selected')).toContainText('First Task');

    await pomodoroPage.addTask('Second Task');

    const selectedCountAfter = await page.locator('.task-row.selected').count();
    expect(selectedCountAfter).toBe(1);
    await expect(page.locator('.task-row.selected')).toContainText('Second Task');
  });

  test('should move completed task to Completed section with undo button', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Complete Test Task');
    await pomodoroPage.selectTask('Complete Test Task');

    await pomodoroPage.completeTask('Complete Test Task');
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section')).toBeVisible();
    await expect(page.locator('.completed-section h4')).toContainText('Completed');

    const completedTask = page.locator('.completed-section .task-row');
    await expect(completedTask).toContainText('Complete Test Task');
    await expect(completedTask.locator('.task-text.completed')).toBeVisible();

    const undoButton = completedTask.locator('.task-action-btn[aria-label="Undo"]');
    await expect(undoButton).toBeVisible();

    await undoButton.click();
    await page.waitForTimeout(500);

    await expect(page.locator('.completed-section')).not.toBeVisible();
    const activeTask = page.locator('.task-items').locator('.task-row').filter({ hasText: 'Complete Test Task' });
    await expect(activeTask).toBeVisible();
    await expect(activeTask.locator('.task-text.completed')).toHaveCount(0);
  });

  test('should display task pomo count after completing a pomodoro', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Stats Task');
    await pomodoroPage.selectTask('Stats Task');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();
    await pomodoroPage.skipConsentModal();

    const taskItem = page.locator('.task-row').filter({ hasText: 'Stats Task' });
    await expect(taskItem).toBeVisible();

    const pomoCount = taskItem.locator('.task-pomo-count');
    await expect(pomoCount).toBeVisible();

    const countText = await pomoCount.textContent();
    expect(countText).toMatch(/\d+m/);
  });

  test('should show selected state only on selected task', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Badge Task A');
    await pomodoroPage.addTask('Badge Task B');
    await pomodoroPage.selectTask('Badge Task A');

    await expect(page.locator('.task-row.selected')).toHaveCount(1);
    await expect(page.locator('.task-row.selected')).toContainText('Badge Task A');
  });
});
