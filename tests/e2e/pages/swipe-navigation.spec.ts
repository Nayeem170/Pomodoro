import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function simulateSwipe(page: any, startX: number, endX: number, y: number) {
  await page.evaluate(({ startX: sx, endX: ex, y: sy }) => {
    const target = document.querySelector('.app-content') || document.body;
    target.dispatchEvent(new TouchEvent('touchstart', {
      bubbles: true,
      cancelable: true,
      touches: [new Touch({ identifier: 0, target, clientX: sx, clientY: sy })],
      changedTouches: [new Touch({ identifier: 0, target, clientX: sx, clientY: sy })]
    }));
    target.dispatchEvent(new TouchEvent('touchend', {
      bubbles: true,
      cancelable: true,
      touches: [],
      changedTouches: [new Touch({ identifier: 0, target, clientX: ex, clientY: sy })]
    }));
  }, { startX, endX, y });
}

test.describe('Swipe Navigation', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to history page on swipe left', async ({ page }) => {
    await simulateSwipe(page, 300, 100, 200);

    await expect(page).toHaveURL(/\/history/, { timeout: 5000 });
  });

  test('should block swipe right on first page (home)', async ({ page }) => {
    const initialUrl = page.url();

    await simulateSwipe(page, 100, 300, 200);

    await page.waitForTimeout(500);
    expect(page.url()).toBe(initialUrl);
  });

  test('should block swipe left on last page (about)', async ({ page }) => {
    await page.locator('.header-nav a[title="About"]').click();
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });

    const urlBeforeSwipe = page.url();

    await simulateSwipe(page, 300, 100, 200);

    await page.waitForTimeout(500);
    expect(page.url()).toBe(urlBeforeSwipe);
  });

  test('should have swipeNavigation module initialized', async ({ page }) => {
    const isInitialized = await page.evaluate(() => {
      return (window as any).swipeNavigation !== null &&
             (window as any).swipeNavigation !== undefined &&
             (window as any).swipeNavigation._dotNetRef !== null;
    });

    expect(isInitialized).toBe(true);
  });
});
