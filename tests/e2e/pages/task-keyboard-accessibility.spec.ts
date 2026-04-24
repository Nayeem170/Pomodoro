import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Task Row Keyboard Accessibility', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should select task by pressing Enter on focused task row', async ({ page }) => {
    await pomodoroPage.addTask('Keyboard Task');
    await page.waitForTimeout(500);

    const taskRow = page.locator('.task-row').filter({ hasText: 'Keyboard Task' }).first();
    await taskRow.focus();
    await page.waitForTimeout(200);

    await page.keyboard.press('Enter');
    await page.waitForTimeout(300);

    await expect(page.locator('.task-row.selected')).toBeVisible();
    await expect(page.locator('.task-row.selected')).toContainText('Keyboard Task');
  });

  test('should select task by pressing Space on focused task row', async ({ page }) => {
    await pomodoroPage.addTask('Space Task');
    await page.waitForTimeout(500);

    const taskRow = page.locator('.task-row').filter({ hasText: 'Space Task' }).first();
    await taskRow.focus();
    await page.waitForTimeout(200);

    await page.keyboard.press('Space');
    await page.waitForTimeout(300);

    await expect(page.locator('.task-row.selected')).toBeVisible();
    await expect(page.locator('.task-row.selected')).toContainText('Space Task');
  });

  test('should navigate to task row via Tab key', async ({ page }) => {
    await pomodoroPage.addTask('Tab Task');
    await page.waitForTimeout(500);

    await page.keyboard.press('Tab');
    await page.waitForTimeout(200);

    const focusedElement = page.locator(':focus');
    const isTaskRow = await focusedElement.evaluate(el => el.closest('.task-row') !== null);
    expect(isTaskRow).toBe(true);
  });
});
