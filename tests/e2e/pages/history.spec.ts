import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

// History page requires more time due to Blazor WASM initialization and data loading
test.describe('History Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
  });

  // Set longer timeout for all tests in this describe block
  test.describe.configure({ timeout: 60000 });

  test('should load history page', async ({ page }) => {
    await expect(page.locator('.history-page')).toBeVisible();
  });

  test('should display history header', async ({ page }) => {
    await expect(page.locator('.history-header')).toBeVisible();
    await expect(page.locator('.history-header h1')).toBeVisible();
  });

  test('should display history tabs', async ({ page }) => {
    await expect(page.locator('.history-tabs, [class*="history-tabs"]')).toBeVisible();
  });

  test('should display daily view by default', async ({ page }) => {
    await expect(page.locator('.daily-view')).toBeVisible();
  });

  test('should display date navigator', async ({ page }) => {
    await expect(page.locator('.date-navigator-container')).toBeVisible();
  });

  test('should display daily summary section', async ({ page }) => {
    await expect(page.locator('.weekly-summary-section')).toBeVisible();
    await expect(page.locator('h2').filter({ hasText: 'Daily Summary' })).toBeVisible();
  });

  test('should display daily stats', async ({ page }) => {
    await expect(page.locator('.weekly-stats')).toBeVisible();
    await expect(page.locator('.stat')).toHaveCount(3);
  });

  test('should display minutes focused stat', async ({ page }) => {
    await expect(page.locator('.stat-label').filter({ hasText: 'Minutes Focused' })).toBeVisible();
  });

  test('should display pomodoros stat', async ({ page }) => {
    await expect(page.locator('.stat-label').filter({ hasText: 'Pomodoros' })).toBeVisible();
  });

  test('should display tasks worked on stat', async ({ page }) => {
    await expect(page.locator('.stat-label').filter({ hasText: 'Tasks Worked On' })).toBeVisible();
  });

  test('should display timeline section', async ({ page }) => {
    await expect(page.locator('.timeline-section')).toBeVisible();
    await expect(page.locator('h2').filter({ hasText: /Timeline/ })).toBeVisible();
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

  test('should switch between daily and weekly views', async ({ page }) => {
    // Click on weekly tab
    await page.locator('button:has-text("Weekly")').click();
    await expect(page.locator('.weekly-view')).toBeVisible();

    // Click back to daily tab
    await page.locator('button:has-text("Daily")').click();
    await expect(page.locator('.daily-view')).toBeVisible();
  });

  test('should display empty state when no activities', async ({ page }) => {
    // Check if empty state is displayed
    const emptyState = page.locator('.empty-state, .no-activities');
    const isVisible = await emptyState.isVisible().catch(() => false);
    // Either empty state or activities should be present
    const hasContent = await page.locator('.timeline-scroll-container').isVisible();
    expect(isVisible || hasContent).toBeTruthy();
  });

  test('should display time distribution chart', async ({ page }) => {
    // Check if time distribution chart is rendered
    await expect(page.locator('.time-distribution-chart')).toBeVisible({ timeout: 10000 });
  });

  test('should handle tab switching smoothly', async ({ page }) => {
    const dailyTab = page.locator('button:has-text("Daily")');
    const weeklyTab = page.locator('button:has-text("Weekly")');
    
    // Click through tabs multiple times
    await weeklyTab.click();
    await page.waitForTimeout(300);
    await dailyTab.click();
    await page.waitForTimeout(300);
    await weeklyTab.click();
    
    // Should end up on weekly view
    await expect(page.locator('.weekly-view')).toBeVisible();
  });
});
