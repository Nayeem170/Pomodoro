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
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.weekly-view')).toBeVisible();
  });

  test('should display weekly summary section', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.weekly-summary-section')).toBeVisible();
    await expect(page.locator('.weekly-summary-section h2')).toContainText('Weekly Summary');
  });

  test('should display weekly stats', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.weekly-stats')).toBeVisible();
  });

  test('should display minutes this week stat', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-label').filter({ hasText: 'Minutes This Week' })).toBeVisible();
  });

  test('should display pomodoros stat', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-label').filter({ hasText: 'Pomodoros' })).toBeVisible();
  });

  test('should display daily average stat', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.stat-label').filter({ hasText: 'Daily Average' })).toBeVisible();
  });

  test('should display weekly trend chart section', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.weekly-chart-section')).toBeVisible();
    await expect(page.locator('.weekly-chart-section h2')).toContainText('Weekly Trend');
  });

  test('should display week navigator', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.date-navigator-container')).toBeVisible();
    await expect(page.locator('.date-navigator .current-date')).toBeVisible();
  });

  test('should display this week button', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await expect(page.locator('.date-navigator .today-btn')).toContainText('This Week');
  });

  test('should navigate to previous week', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    const currentWeek = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.date-navigator .current-date').textContent();
    expect(prevWeek).not.toBe(currentWeek);
  });

  test('should navigate to next week', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    const currentWeek = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '▶' }).click();
    await page.waitForTimeout(500);
    const nextWeek = await page.locator('.date-navigator .current-date').textContent();
    expect(prevWeek).not.toBe(currentWeek);
    expect(nextWeek).toBe(currentWeek);
  });

  test('should return to this week when button is clicked', async ({ page }) => {
    await page.locator('button:has-text("Weekly")').click();
    await page.waitForTimeout(500);
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const prevWeek = await page.locator('.date-navigator .current-date').textContent();

    await page.locator('.date-navigator .today-btn').click();
    await page.waitForTimeout(500);
    const thisWeek = await page.locator('.date-navigator .current-date').textContent();
    expect(thisWeek).not.toBe(prevWeek);
  });
});
