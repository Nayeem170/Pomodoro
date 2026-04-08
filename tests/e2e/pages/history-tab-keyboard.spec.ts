import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Tab Keyboard Navigation', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/history');
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });
  });

  test('should switch to weekly tab with arrow key', async ({ page }) => {
    const dailyTab = page.locator('#daily-tab');
    const weeklyTab = page.locator('#weekly-tab');

    await expect(dailyTab).toHaveAttribute('aria-selected', 'true');
    await expect(weeklyTab).toHaveAttribute('aria-selected', 'false');

    await dailyTab.focus();
    await page.keyboard.press('ArrowRight');
    await page.waitForTimeout(300);

    await expect(weeklyTab).toHaveAttribute('aria-selected', 'true');
    await expect(dailyTab).toHaveAttribute('aria-selected', 'false');
  });

  test('should switch back to daily tab with arrow key', async ({ page }) => {
    const dailyTab = page.locator('#daily-tab');
    const weeklyTab = page.locator('#weekly-tab');

    await weeklyTab.click();
    await page.waitForTimeout(300);

    await expect(weeklyTab).toHaveAttribute('aria-selected', 'true');

    await weeklyTab.focus();
    await page.keyboard.press('ArrowLeft');
    await page.waitForTimeout(300);

    await expect(dailyTab).toHaveAttribute('aria-selected', 'true');
    await expect(weeklyTab).toHaveAttribute('aria-selected', 'false');
  });

  test('should have correct ARIA attributes on tabs', async ({ page }) => {
    const tabList = page.locator('[role="tablist"]');
    await expect(tabList).toBeVisible();

    const dailyTab = page.locator('#daily-tab');
    const weeklyTab = page.locator('#weekly-tab');

    await expect(dailyTab).toHaveAttribute('role', 'tab');
    await expect(weeklyTab).toHaveAttribute('role', 'tab');
    await expect(dailyTab).toHaveAttribute('tabindex', '0');
    await expect(weeklyTab).toHaveAttribute('tabindex', '-1');
  });
});
