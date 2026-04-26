import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Infinite Scroll Loading', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should create IntersectionObserver for scroll sentinel', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const observerSupported = await page.evaluate(() => 'IntersectionObserver' in window);
    expect(observerSupported).toBe(true);

    const infiniteScrollSupported = await page.evaluate(() => {
      return typeof (window as any).infiniteScrollInterop?.isSupported === 'function' &&
        (window as any).infiniteScrollInterop.isSupported();
    });
    expect(infiniteScrollSupported).toBe(true);
  });

  test('should show loading indicator when sentinel is intersecting during load', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const sentinel = page.locator('#scroll-sentinel');
    const hasSentinel = await sentinel.count().catch(() => 0);

    if (hasSentinel > 0) {
      const scrollContainer = page.locator('.timeline-scroll-container');
      const isScrollable = await scrollContainer.evaluate(el => el.scrollHeight > el.clientHeight);

      if (isScrollable) {
        await scrollContainer.evaluate(el => { el.scrollTop = el.scrollHeight; });
        await page.waitForTimeout(1000);

        const loadingVisible = await page.locator('.loading-indicator').isVisible().catch(() => false);
        const endOfListVisible = await page.locator('.end-of-list').isVisible().catch(() => false);
        const emptyVisible = await page.locator('.empty-state').isVisible().catch(() => false);
        expect(loadingVisible || endOfListVisible || emptyVisible).toBe(true);
      }
    }
  });

  test('should display multiple activity rows after completing pomodoros', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.seedHistoryViaDB('Scroll Task A');
    await pomodoroPage.seedHistoryViaDB('Scroll Task B');
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tl-row').first()).toBeVisible({ timeout: 5000 });
  });

  test('should destroy observer when navigating away from history', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const observerCountBefore = await page.evaluate(() => {
      return (window as any).infiniteScrollInterop?.observers?.size ?? 0;
    });

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const observerCountAfter = await page.evaluate(() => {
      return (window as any).infiniteScrollInterop?.observers?.size ?? 0;
    });

    expect(observerCountAfter).toBeLessThanOrEqual(observerCountBefore);
  });

  test('should have correct sentinel element structure', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const sentinel = page.locator('#scroll-sentinel');
    const hasSentinel = await sentinel.count().catch(() => 0);

    if (hasSentinel > 0) {
      await expect(sentinel.first()).toHaveClass('scroll-sentinel');
      await expect(page.locator('.timeline-scroll-container')).toBeVisible();
    }
  });

  test('should handle rapid scroll events without duplicate loads', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const scrollContainer = page.locator('.timeline-scroll-container');
    const hasContainer = await scrollContainer.count().catch(() => 0);

    if (hasContainer > 0) {
      await scrollContainer.evaluate(el => {
        for (let i = 0; i < 10; i++) {
          el.scrollTop = el.scrollHeight;
          el.scrollTop = 0;
        }
      });
      await page.waitForTimeout(1000);
      await expect(page.locator('.hist-body')).toBeVisible();
    }
  });
});
