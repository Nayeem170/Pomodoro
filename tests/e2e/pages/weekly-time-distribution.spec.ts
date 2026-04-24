import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Weekly Time Distribution', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
  });

  test.describe.configure({ timeout: 60000 });

  test('should display time distribution card title in weekly view', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);

    const timeDistTitle = page.locator('.card-title').filter({ hasText: 'Time distribution' });
    await expect(timeDistTitle).toBeVisible();
  });

  test('should display sessions per day chart in weekly view', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);

    await expect(page.locator('.card-title').filter({ hasText: 'Sessions per day' })).toBeVisible();
    await expect(page.locator('.weekly-chart')).toBeVisible();
  });

  test('should display time distribution section after sessions per day chart', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);

    const cardTitles = page.locator('.card-title');
    await expect(cardTitles.filter({ hasText: 'Sessions per day' })).toBeVisible();
    await expect(cardTitles.filter({ hasText: 'Time distribution' })).toBeVisible();
  });

  test('should show weekly stat grid with focus time and pomodoros', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);

    await expect(page.locator('.stat-grid').first()).toBeVisible();
    await expect(page.locator('.sl').filter({ hasText: 'Focus time' })).toBeVisible();
    await expect(page.locator('.sl').filter({ hasText: 'Pomodoros' })).toBeVisible();
  });
});
