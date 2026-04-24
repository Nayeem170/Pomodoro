import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Weekly View', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display weekly view when switched', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-grid').first()).toBeVisible();
  });

  test('should display weekly summary section', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-grid').first()).toBeVisible();
  });

  test('should display weekly stats', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-grid').first()).toBeVisible();
  });

  test('should display focus time stat', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.sl').filter({ hasText: 'Focus time' })).toBeVisible();
  });

  test('should display pomodoros stat', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.sl').filter({ hasText: 'Pomodoros' })).toBeVisible();
  });

  test('should display daily average stat', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.sl').filter({ hasText: 'Daily avg' })).toBeVisible();
  });

  test('should display weekly trend chart section', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.card-title').filter({ hasText: 'Sessions per day' })).toBeVisible();
    await expect(page.locator('.weekly-chart')).toBeVisible();
  });

  test('should display week navigator', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.period-nav')).toBeVisible();
    await expect(page.locator('.period-lbl')).toBeVisible();
  });

  test('should display this week button', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await expect(page.locator('button.nav-qbtn')).toContainText('This Week');
  });

  test('should navigate to previous week', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    const currentWeek = await page.locator('.period-lbl').textContent();
    await page.locator('button.nav-arr[title="Previous week"]').click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.period-lbl').textContent();
    expect(prevWeek).not.toBe(currentWeek);
  });

  test('should navigate to next week', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    const currentWeek = await page.locator('.period-lbl').textContent();
    await page.locator('button.nav-arr[title="Previous week"]').click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.period-lbl').textContent();
    await page.locator('button.nav-arr[title="Next week"]').click();
    await page.waitForTimeout(500);
    const nextWeek = await page.locator('.period-lbl').textContent();
    expect(prevWeek).not.toBe(currentWeek);
    expect(nextWeek).toBe(currentWeek);
  });

  test('should return to this week when button is clicked', async ({ page }) => {
    await page.locator('#weekly-tab').click();
    await page.waitForTimeout(500);
    await page.locator('button.nav-arr[title="Previous week"]').click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.period-lbl').textContent();

    await page.locator('button.nav-qbtn').click();
    await page.waitForTimeout(500);
    const thisWeek = await page.locator('.period-lbl').textContent();
    expect(thisWeek).not.toBe(prevWeek);
  });
});
