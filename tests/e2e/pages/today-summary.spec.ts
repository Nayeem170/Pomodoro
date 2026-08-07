import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display today summary strip', async ({ page }) => {
    await expect(page.locator('.timer-pane-summary')).toBeVisible({ timeout: 30000 });
  });

  test('should display focused stat', async ({ page }) => {
    await expect(page.locator('.summary-lbl', { hasText: 'Focused' })).toBeVisible({ timeout: 30000 });
  });

  test('should display pomodoros stat', async ({ page }) => {
    await expect(page.locator('.summary-lbl', { hasText: 'Pomodoros' })).toBeVisible({ timeout: 30000 });
  });

  test('should display tasks stat', async ({ page }) => {
    await expect(page.locator('.summary-lbl', { hasText: 'Tasks' })).toBeVisible({ timeout: 30000 });
  });

  test('should show initial zero values', async ({ page }) => {
    const cells = page.locator('.summary-cell');
    await expect(cells.nth(0).locator('.summary-val')).toContainText('0m', { timeout: 30000 });
    await expect(cells.nth(1).locator('.summary-val')).toContainText('0');
  });
});
