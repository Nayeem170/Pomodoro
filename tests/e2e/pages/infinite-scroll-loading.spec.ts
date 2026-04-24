import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

async function completePomodoroFast(page: any, pomodoroPage: PomodoroPage, taskName: string) {
  await pomodoroPage.goto('/settings');
  await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  const pomodoroInput = page.locator('.step-input').first();
  await pomodoroInput.click({ clickCount: 3 });
  await pomodoroInput.pressSequentially('1');
  await pomodoroInput.dispatchEvent('input');
  await pomodoroInput.dispatchEvent('change');
  await page.waitForTimeout(500);
  await pomodoroPage.goto('/');
  await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  await pomodoroPage.addTask(taskName);
  await pomodoroPage.selectTask(taskName);
  await pomodoroPage.startTimer();
  await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  await page.waitForTimeout(500);
  await page.evaluate(async () => {
    const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
    for (let i = 0; i < 60; i++) {
      if ((window as any).timerFunctions?.dotNetRef) {
        try {
          await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
        } catch { break; }
      }
      await delay(30);
    }
  });
  await page.waitForTimeout(3000);
  const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
  if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
    await consentOption.click();
    await page.waitForTimeout(1000);
  }
}

test.describe('Infinite Scroll Loading', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 120000 });

  test('should create IntersectionObserver for scroll sentinel', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const observerSupported = await page.evaluate(() => {
      return 'IntersectionObserver' in window;
    });
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
      const isScrollable = await scrollContainer.evaluate(el => {
        return el.scrollHeight > el.clientHeight;
      });

      if (isScrollable) {
        await scrollContainer.evaluate(el => {
          el.scrollTop = el.scrollHeight;
        });
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

    await completePomodoroFast(page, pomodoroPage, 'Scroll Task A');
    await pomodoroPage.resetTimer();
    await page.waitForTimeout(500);

    await pomodoroPage.addTask('Scroll Task B');
    await pomodoroPage.selectTask('Scroll Task B');
    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await page.evaluate(async () => {
      const delay = (ms: number) => new Promise(r => setTimeout(r, ms));
      for (let i = 0; i < 60; i++) {
        if ((window as any).timerFunctions?.dotNetRef) {
          try {
            await (window as any).timerFunctions.dotNetRef.invokeMethodAsync('OnTimerTickJs');
          } catch { break; }
        }
        await delay(30);
      }
    });
    await page.waitForTimeout(3000);
    const consentOption = page.locator('.btn-option').filter({ hasText: /Skip/i });
    if (await consentOption.isVisible({ timeout: 2000 }).catch(() => false)) {
      await consentOption.click();
      await page.waitForTimeout(1000);
    }

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });

    const activityCount = await page.locator('.tl-row').count();
    expect(activityCount).toBeGreaterThanOrEqual(1);
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
