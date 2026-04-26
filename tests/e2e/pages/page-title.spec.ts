import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Page Title', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should have default title on home page', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title.toLowerCase()).toContain('pomodoro');
  });

  test('should have default title on history page', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title.toLowerCase()).toContain('pomodoro');
  });

  test('should have default title on settings page', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title.toLowerCase()).toContain('pomodoro');
  });

  test('should have default title on about page', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/about');
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });

    const title = await page.title();
    expect(title).toBeTruthy();
    expect(title.toLowerCase()).toContain('pomodoro');
  });

  test('should have not found title on non-existent route', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/non-existent-page');

    const title = await page.title();
    expect(title).toBe('Not found');
  });

  test('should keep same title when navigating between pages', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const homeTitle = await page.title();

    await page.locator('.header-nav a[title="History"]').click();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const historyTitle = await page.title();
    expect(historyTitle).toBe(homeTitle);
  });
});
