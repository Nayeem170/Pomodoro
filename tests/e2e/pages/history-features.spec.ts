import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Features', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
  });

  test('should show empty state when no activities exist for selected date', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const prevDayBtn = page.locator('button.nav-btn[title="Previous day"]');
    await prevDayBtn.click();
    await page.waitForTimeout(500);
    await prevDayBtn.click();
    await page.waitForTimeout(500);
    await prevDayBtn.click();
    await page.waitForTimeout(500);

    const emptyState = page.locator('.empty-state');
    const isTimelineEmpty = await emptyState.isVisible().catch(() => false);
    const hasNoActivities = await page.locator('.timeline-section .activity-item').count() === 0;

    if (isTimelineEmpty) {
      await expect(page.locator('.empty-state p')).toContainText('No activities for this day');
    } else {
      expect(hasNoActivities).toBe(true);
    }
  });

  test('should disable Today button when already on today', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const todayBtn = page.locator('button.today-btn');
    await expect(todayBtn).toBeVisible();

    const isDisabled = await todayBtn.isDisabled();
    expect(isDisabled).toBe(true);
  });

  test('should enable Today button after navigating away from today', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const prevDayBtn = page.locator('button.nav-btn[title="Previous day"]');
    await prevDayBtn.click();
    await page.waitForTimeout(500);

    const todayBtn = page.locator('button.today-btn');
    const isDisabled = await todayBtn.isDisabled();
    expect(isDisabled).toBe(false);

    await todayBtn.click();
    await page.waitForTimeout(500);

    const isDisabledAgain = await todayBtn.isDisabled();
    expect(isDisabledAgain).toBe(true);
  });

  test('should disable next day button when on today', async ({ page }) => {
    await pomodoroPage.openHistory();
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    const nextDayBtn = page.locator('button.nav-btn[title="Next day"]');
    const isDisabled = await nextDayBtn.isDisabled();
    expect(isDisabled).toBe(true);
  });
});
