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
    await expect(page.locator('.btn-add-task')).toBeVisible({ timeout: 30000 });

    // Add a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Persistence Test Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(1000);

    // Verify task exists
    await expect(page.locator('.task-item').filter({ hasText: 'Persistence Test Task' })).toBeVisible();
    const taskCountBefore = await page.locator('.task-item').count();

    // Reload the page
    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    // Verify task still exists after reload
    await expect(page.locator('.task-item').filter({ hasText: 'Persistence Test Task' })).toBeVisible();
    const taskCountAfter = await page.locator('.task-item').count();
    expect(taskCountAfter).toBe(taskCountBefore);

    // Clean up: delete the test task
    const taskItem = page.locator('.task-item').filter({ hasText: 'Persistence Test Task' }).first();
    await taskItem.locator('button:has-text("🗑")').click();
    await page.waitForTimeout(500);
  });

  test('should persist settings after page reload', async ({ page }) => {
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    // Change a setting
    const input = page.locator('input[type="number"]').first();
    await input.fill('30');
    await input.dispatchEvent('change');
    await page.waitForTimeout(500);

    // Save the setting
    await page.locator('.btn-save').click();
    await expect(page.locator('.settings-toast')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(500);

    // Reload the page
    await page.reload();
    await pomodoroPage.openSettings();
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    // Verify setting persisted
    await expect(input).toHaveValue('30');

    // Reset to defaults for cleanup
    await page.locator('.btn-reset-defaults').click();
    await page.waitForTimeout(500);
  });

  test('should persist selected task after page reload', async ({ page }) => {
    await expect(page.locator('.btn-add-task')).toBeVisible({ timeout: 30000 });

    // Add and select a task
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Selected Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItem = page.locator('.task-item').filter({ hasText: 'Selected Task' }).first();
    await taskItem.click();
    await page.waitForTimeout(300);

    // Verify task is selected
    await expect(page.locator('.task-item.selected').filter({ hasText: 'Selected Task' })).toBeVisible();

    // Reload the page
    await page.reload();
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    // Verify task is still selected after reload
    await expect(page.locator('.task-item.selected').filter({ hasText: 'Selected Task' })).toBeVisible();

    // Clean up
    await taskItem.locator('button:has-text("🗑")').click();
    await page.waitForTimeout(500);
  });
});
