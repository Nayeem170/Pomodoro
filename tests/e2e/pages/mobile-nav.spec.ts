import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Mobile Nav Menu', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display header navigation links', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.header-nav')).toBeVisible();
    await expect(page.locator('.header-nav a')).toHaveCount(4);
  });

  test('should have all four navigation links with correct titles', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.header-nav a[title="Timer"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="History"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="Settings"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="About Pomodoro"]')).toBeVisible();
  });

  test('should navigate to each page via nav links', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="History"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.history-page')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Settings"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.settings-page')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="About Pomodoro"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.about-page')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Timer"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
  });
});
