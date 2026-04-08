import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should display today summary section', async ({ page }) => {
    await expect(page.locator('.today-summary')).toBeVisible();
  });

  test('should display summary heading', async ({ page }) => {
    await expect(page.locator('.today-summary h3')).toContainText("TODAY'S SUMMARY");
  });

  test('should display focused time stat', async ({ page }) => {
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'focused' })).toBeVisible();
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'focused' }).locator('.stat-icon')).toContainText('⏱️');
  });

  test('should display pomodoros stat', async ({ page }) => {
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'pomodoros' })).toBeVisible();
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'pomodoros' }).locator('.stat-icon')).toContainText('🍅');
  });

  test('should display tasks stat', async ({ page }) => {
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'tasks' })).toBeVisible();
    await expect(page.locator('.today-summary .stat-item').filter({ hasText: 'tasks' }).locator('.stat-icon')).toContainText('📋');
  });

  test('should display all three stat items', async ({ page }) => {
    await expect(page.locator('.today-summary .stat-item')).toHaveCount(3);
  });

  test('should show initial zero values', async ({ page }) => {
    const focusedValue = page.locator('.today-summary .stat-item').filter({ hasText: 'focused' }).locator('.stat-value');
    const pomodorosValue = page.locator('.today-summary .stat-item').filter({ hasText: 'pomodoros' }).locator('.stat-value');
    const tasksValue = page.locator('.today-summary .stat-item').filter({ hasText: 'tasks' }).locator('.stat-value');

    await expect(focusedValue).toContainText('0m');
    await expect(pomodorosValue).toContainText('0');
    await expect(tasksValue).toContainText('0');
  });
});
