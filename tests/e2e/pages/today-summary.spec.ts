import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Today Summary', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should display today summary section', async ({ page }) => {
    await expect(page.locator('.pomo-row')).toBeVisible({ timeout: 30000 });
  });

  test('should display pomodoro emoji', async ({ page }) => {
    await expect(page.locator('.pomo-emoji')).toBeVisible({ timeout: 30000 });
  });

  test('should display pomodoro count', async ({ page }) => {
    await expect(page.locator('.pomo-num')).toBeVisible({ timeout: 30000 });
  });

  test('should display daily goal text', async ({ page }) => {
    await expect(page.locator('.pomo-sub')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.pomo-sub')).toContainText('completed today');
  });

  test('should display focus time', async ({ page }) => {
    await expect(page.locator('.pomo-focus-time')).toBeVisible({ timeout: 30000 });
  });

  test('should display focus time label', async ({ page }) => {
    await expect(page.locator('.pomo-focus-label')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.pomo-focus-label')).toContainText('focus time');
  });

  test('should display progress bar', async ({ page }) => {
    await expect(page.locator('.progress-bar')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.progress-bar-fill')).toBeVisible();
  });

  test('should show initial zero values', async ({ page }) => {
    await expect(page.locator('.pomo-num')).toContainText('0');
    await expect(page.locator('.pomo-focus-time')).toContainText('0m');
  });
});
