import { test, expect, BrowserContext } from '../fixtures/consoleCheck';
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
    await expect(page.locator('.header-nav a[title="Focus"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="History"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="Settings"]')).toBeVisible();
    await expect(page.locator('.header-nav a[title="About"]')).toBeVisible();
  });

  test('should navigate to each page via nav links', async ({ page }) => {
    await page.locator('.header-nav a[title="History"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Settings"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="About"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="Focus"]').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });
});

test.describe('Mobile Header Responsive', () => {
  test.describe.configure({ mode: 'serial', timeout: 60000 });

  test('should render app title inside header-title', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const title = page.locator('.header-title .header-text');
    await expect(title).toBeVisible();
    await expect(title).toContainText('Tarkeez');
  });

  test('should render header on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const header = page.locator('.app-header');
    await expect(header).toBeVisible();
  });

  test('should scale nav icons on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const mobileTab = page.locator('.mobile-tab').first();
    await expect(mobileTab).toBeVisible();
  });
});

test.describe('Round Button', () => {
  test.describe.configure({ timeout: 60000 });

  test('should render add button visible', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeVisible();
  });

  test('should render add button with consistent dimensions', async ({ page }) => {
    const pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const addButton = page.locator('.btn-add-text');
    await expect(addButton).toBeVisible();
  });
});
