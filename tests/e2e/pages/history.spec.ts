import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
  });

  test.describe.configure({ timeout: 60000 });

  test('should load history page', async ({ page }) => {
    await expect(page.locator('.hist-body')).toBeVisible();
  });

  test('should display history tabs', async ({ page }) => {
    await expect(page.locator('.vtoggle')).toBeVisible();
  });

  test('should display daily view by default', async ({ page }) => {
    await expect(page.locator('.stat-grid').first()).toBeVisible();
  });

  test('should display date navigator', async ({ page }) => {
    await expect(page.locator('.period-nav')).toBeVisible();
  });

  test('should display daily summary section', async ({ page }) => {
    await expect(page.locator('.stat-grid').first()).toBeVisible();
  });

  test('should display daily stats', async ({ page }) => {
    await expect(page.locator('.sc').first()).toBeVisible();
  });

  test('should display pomodoros stat', async ({ page }) => {
    await expect(page.locator('.sl').filter({ hasText: 'Pomodoros' })).toBeVisible();
  });

  test('should display focus time stat', async ({ page }) => {
    await expect(page.locator('.sl').filter({ hasText: 'Focus time' })).toBeVisible();
  });

  test('should display tasks done stat', async ({ page }) => {
    await expect(page.locator('.sl').filter({ hasText: 'Tasks done' })).toBeVisible();
  });

  test('should display timeline card', async ({ page }) => {
    await expect(page.locator('.card-title').filter({ hasText: /Timeline/ })).toBeVisible();
  });

  test('should display activity count', async ({ page }) => {
    await expect(page.locator('.activity-count')).toBeVisible();
  });

  test('should display timeline scroll container', async ({ page }) => {
    await expect(page.locator('.timeline-scroll-container')).toBeVisible();
  });

  test('should display activity timeline', async ({ page }) => {
    await expect(page.locator('.timeline-scroll-container')).toBeVisible();
  });
});
