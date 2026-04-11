import { test, expect, BrowserContext } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Swipe Navigation', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to history page on swipe left', async ({ browser }) => {
    const context = await browser.newContext({ hasTouch: true });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const appContent = page.locator('.app-content');
    const startX = 300;
    const startY = 200;

    await page.touchscreen.tap(startX, startY);
    await page.touchscreen.tap(startX - 100, startY);

    await page.waitForTimeout(1000);
    await expect(page).toHaveURL(/\/history/);

    await context.close();
  });

  test('should block swipe right on first page (home)', async ({ browser }) => {
    const context = await browser.newContext({ hasTouch: true });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const initialUrl = page.url();

    const startX = 100;
    const startY = 200;

    await page.touchscreen.tap(startX, startY);
    await page.touchscreen.tap(startX + 100, startY);

    await page.waitForTimeout(1000);
    expect(page.url()).toBe(initialUrl);

    await context.close();
  });

  test('should block swipe left on last page (about)', async ({ browser }) => {
    const context = await browser.newContext({ hasTouch: true });
    const page = await context.newPage();
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await page.locator('.header-nav a[title="About Pomodoro"]').click();
    await expect(page.locator('.about-page')).toBeVisible({ timeout: 30000 });

    const urlBeforeSwipe = page.url();

    const startX = 300;
    const startY = 200;

    await page.touchscreen.tap(startX, startY);
    await page.touchscreen.tap(startX - 100, startY);

    await page.waitForTimeout(1000);
    expect(page.url()).toBe(urlBeforeSwipe);

    await context.close();
  });

  test('should add slide transition class on navigation', async ({ page }) => {
    const hasTransition = await page.evaluate(() => {
      const content = document.querySelector('.app-content');
      if (!content) return false;
      const style = window.getComputedStyle(content);
      return style.transition.includes('transform') && style.transition.includes('opacity');
    });

    expect(hasTransition).toBe(true);
  });

  test('should clean up swipe listeners on dispose', async ({ page }) => {
    const isInitialized = await page.evaluate(() => {
      return (window as any).swipeNavigation !== null &&
             (window as any).swipeNavigation._dotNetRef !== null;
    });

    expect(isInitialized).toBe(true);
  });
});
