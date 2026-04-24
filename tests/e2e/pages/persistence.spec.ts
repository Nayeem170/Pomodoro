import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('IndexedDB Persistence', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should persist tasks after page reload', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Persistence Test Task');

    await expect(page.locator('.task-row').filter({ hasText: 'Persistence Test Task' })).toBeVisible();
    const taskCountBefore = await page.locator('.task-row').count();

    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.task-row').filter({ hasText: 'Persistence Test Task' })).toBeVisible();
    const taskCountAfter = await page.locator('.task-row').count();
    expect(taskCountAfter).toBe(taskCountBefore);

    await pomodoroPage.deleteTask('Persistence Test Task');
    await page.waitForTimeout(500);
  });

  test('should persist settings after page reload', async ({ page }) => {
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const input = page.locator('.step-input').first();
    await input.click({ clickCount: 3 });
    await input.pressSequentially('30');
    await input.dispatchEvent('input');
    await page.waitForTimeout(500);

    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await expect(input).toHaveValue('30');

    await pomodoroPage.resetToDefaults();
    await page.waitForTimeout(500);
  });

  test('should persist selected task after page reload', async ({ page }) => {
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Selected Task');
    await page.waitForTimeout(500);

    const taskItem = page.locator('.task-row').filter({ hasText: 'Selected Task' }).first();
    await taskItem.click();
    await page.waitForTimeout(300);

    await expect(page.locator('.task-row.selected').filter({ hasText: 'Selected Task' })).toBeVisible();

    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.task-row.selected').filter({ hasText: 'Selected Task' })).toBeVisible();

    await pomodoroPage.deleteTask('Selected Task');
    await page.waitForTimeout(500);
  });
});
