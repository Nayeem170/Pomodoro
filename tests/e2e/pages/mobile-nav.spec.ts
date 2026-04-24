import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Mobile Nav Menu', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test.describe.configure({ timeout: 60000 });

  test('should display header navigation links', async ({ page }) => {
    await expect(page.locator('.header-nav')).toBeVisible();
    await expect(page.locator('.header-nav a')).toHaveCount(4);
  });

  test('should have all four navigation links with correct titles', async ({ page }) => {
    await expect(page.locator('.header-nav a[title="Timer"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="History"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="Settings"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="About Pomodoro"]')).toBeVisible();
  });

  test('should navigate to each page via nav links', async ({ page }) => {
    await page.locator('.header-nav a[title="History"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Settings"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="About Pomodoro"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Timer"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });
});

test.describe('Mobile Header Responsive', () => {
  test.describe.configure({ timeout: 60000 });

  test('should render tagline inside header-title', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const tagline = page.locator('.header-title .header-text');
    await expect(tagline).toBeVisible();
    await expect(tagline).toContainText('Focus. Work. Achieve.');
  });

  test('should render header with left and right borders on mobile viewport', async ({ browser }) => {
    const context = await browser.newContext({ viewport: { width: 375, height: 667 } });
    const page = await context.newPage();
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const header = page.locator('.app-header');
    await expect(header).toBeVisible();

    const borderWidth = await header.evaluate(el => {
      const style = window.getComputedStyle(el);
      return {
        borderLeft: style.borderLeftWidth,
        borderRight: style.borderRightWidth
      };
    });

    expect(parseInt(borderWidth.borderLeft)).toBeGreaterThan(0);
    expect(parseInt(borderWidth.borderRight)).toBeGreaterThan(0);
    await context.close();
  });

  test('should scale nav icons on mobile viewport', async ({ browser }) => {
    const context = await browser.newContext({ viewport: { width: 375, height: 667 } });
    const page = await context.newPage();
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const navIconSize = await page.locator('.header-nav a').first().evaluate(el => {
      const style = window.getComputedStyle(el);
      return { width: style.width, height: style.height };
    });

    expect(parseInt(navIconSize.width)).toBeLessThanOrEqual(38);
    expect(parseInt(navIconSize.height)).toBeLessThanOrEqual(38);
    await context.close();
  });
});

test.describe('Round Button', () => {
  test.describe.configure({ timeout: 60000 });

  test('should render add button as round circle', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-add-btn').waitFor({ state: 'visible', timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await page.waitForTimeout(500);

    const addButton = page.locator('.btn-icon-small.btn-add');
    await expect(addButton).toBeVisible();

    const borderRadius = await addButton.evaluate(el => {
      return window.getComputedStyle(el).borderRadius;
    });

    expect(borderRadius).toBe('50%');
  });

  test('should have equal width and height for round shape', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.locator('.task-add-btn').waitFor({ state: 'visible', timeout: 30000 });
    await page.locator('.task-add-btn').click();
    await page.waitForTimeout(500);

    const addButton = page.locator('.btn-icon-small.btn-add');
    const dimensions = await addButton.evaluate(el => {
      const style = window.getComputedStyle(el);
      return { width: style.width, height: style.height };
    });

    expect(dimensions.width).toBe(dimensions.height);
  });
});
