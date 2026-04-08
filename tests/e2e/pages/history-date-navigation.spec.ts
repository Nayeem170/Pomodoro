import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('History Date Navigation', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
  });

  test.describe.configure({ timeout: 60000 });

  test('should display date navigator with current date', async ({ page }) => {
    await expect(page.locator('.date-navigator')).toBeVisible();
    await expect(page.locator('.date-navigator .current-date')).toBeVisible();
  });

  test('should display previous day button', async ({ page }) => {
    await expect(page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' })).toBeVisible();
  });

  test('should display next day button', async ({ page }) => {
    await expect(page.locator('.date-navigator .nav-btn').filter({ hasText: '▶' })).toBeVisible();
  });

  test('should display today button', async ({ page }) => {
    await expect(page.locator('.date-navigator .today-btn')).toBeVisible();
    await expect(page.locator('.date-navigator .today-btn')).toContainText('Today');
  });

  test('should navigate to previous day', async ({ page }) => {
    const currentDate = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const newDate = await page.locator('.date-navigator .current-date').textContent();
    expect(newDate).not.toBe(currentDate);
  });

  test('should navigate to next day', async ({ page }) => {
    const currentDate = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const prevDate = await page.locator('.date-navigator .current-date').textContent();
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '▶' }).click();
    await page.waitForTimeout(500);
    const nextDate = await page.locator('.date-navigator .current-date').textContent();
    expect(prevDate).not.toBe(currentDate);
    expect(nextDate).toBe(currentDate);
  });

  test('should return to today when today button is clicked', async ({ page }) => {
    await page.locator('.date-navigator .nav-btn').filter({ hasText: '◀' }).click();
    await page.waitForTimeout(500);
    const prevDate = await page.locator('.date-navigator .current-date').textContent();

    await page.locator('.date-navigator .today-btn').click();
    await page.waitForTimeout(500);
    const todayDate = await page.locator('.date-navigator .current-date').textContent();
    expect(todayDate).not.toBe(prevDate);
  });
});
