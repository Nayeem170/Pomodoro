import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Navigation', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should display header with app title', async ({ page }) => {
    await expect(page.locator('.header-title')).toBeVisible();
    await expect(page.locator('.header-text')).toContainText('Pomodoro');
  });

  test('should not display footer', async ({ page }) => {
    await expect(page.locator('.app-footer')).not.toBeAttached();
    await expect(page.locator('.footer-copy')).not.toBeAttached();
  });

  test('should navigate to history page via nav link', async ({ page }) => {
    await page.locator('.header-nav a[title="History"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to settings page via nav link', async ({ page }) => {
    await page.locator('.header-nav a[title="Settings"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to about page via nav link', async ({ page }) => {
    await page.locator('.header-nav a[title="About"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to home page via nav link', async ({ page }) => {
    await page.locator('.header-nav a[title="History"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Timer"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.ring-area')).toBeVisible({ timeout: 30000 });
  });

  test('should highlight active nav link', async ({ page }) => {
    const timerLink = page.locator('.header-nav a[title="Timer"]');
    await expect(timerLink).toHaveClass(/active/);
  });

  test('should display all four navigation links', async ({ page }) => {
    await expect(page.locator('.header-nav a')).toHaveCount(4);
    await expect(page.locator('.header-nav a[title="Timer"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="History"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="Settings"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="About"]')).toBeVisible();
  });
});
