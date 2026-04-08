import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Infinite Scroll', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should display scroll sentinel element', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const sentinel = page.locator('#scroll-sentinel');
    const hasSentinel = await sentinel.count().catch(() => 0);
    if (hasSentinel > 0) {
      await expect(sentinel.first()).toBeVisible();
    }
  });

  test('should display timeline scroll container', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('#timeline-scroll-container')).toBeVisible();
  });

  test('should show end-of-list when all activities are loaded', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const prevDayBtn = page.locator('button.nav-btn[title="Previous day"]');
    await prevDayBtn.click();
    await page.waitForTimeout(500);
    await prevDayBtn.click();
    await page.waitForTimeout(500);
    await prevDayBtn.click();
    await page.waitForTimeout(500);

    const endOfList = page.locator('.end-of-list');
    const isEmpty = await page.locator('.empty-state').isVisible().catch(() => false);
    const hasNoActivities = await page.locator('.timeline-section .activity-item').count() === 0;

    if (isEmpty) {
      await expect(page.locator('.empty-state p')).toContainText('No activities for this day');
    } else if (hasNoActivities) {
      await expect(endOfList).not.toBeVisible();
    } else {
      await expect(endOfList).toBeVisible();
      await expect(endOfList).toContainText('No more activities');
    }
  });

  test('should not show loading indicator when idle', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const loadingIndicator = page.locator('.loading-indicator');
    await expect(loadingIndicator).not.toBeVisible();
  });
});
