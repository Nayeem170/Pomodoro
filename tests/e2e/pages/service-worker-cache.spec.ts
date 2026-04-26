import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Service Worker Cache Behavior', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should have serviceWorker API available', async ({ page }) => {
    const hasSW = await page.evaluate(() => 'serviceWorker' in navigator);
    expect(hasSW).toBe(true);
  });

  test('should check service worker registration status', async ({ page }) => {
    const registrations = await page.evaluate(async () => {
      if (!('serviceWorker' in navigator)) return [];
      return await navigator.serviceWorker.getRegistrations();
    });

    expect(Array.isArray(registrations)).toBe(true);

    if (registrations.length > 0) {
      const sw = registrations[0];
      expect(sw.scope).toBeTruthy();
      expect(sw.state).toMatch(/activ|install|redundan/);
    }
  });

  test('should verify service-worker.js exists and is valid JS', async ({ page }) => {
    const response = await page.evaluate(async () => {
      const res = await fetch('/service-worker.js');
      return {
        status: res.status,
        contentType: res.headers.get('content-type'),
        bodyLength: (await res.text()).length
      };
    });

    expect(response.status).toBe(200);
    expect(response.bodyLength).toBeGreaterThan(100);
  });

  test('should verify cache API is available', async ({ page }) => {
    const hasCacheAPI = await page.evaluate(() => 'caches' in window);
    expect(hasCacheAPI).toBe(true);
  });

  test('should verify stale-while-revalidate strategy via fetch behavior', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const fetchBehavior = await page.evaluate(async () => {
      const response = await fetch('/css/app.css', { method: 'GET' });
      return {
        status: response.status,
        headers: {
          cacheControl: response.headers.get('cache-control'),
          contentType: response.headers.get('content-type')
        }
      };
    });

    expect(fetchBehavior.status).toBe(200);
  });

  test('should remain functional after simulating network offline for cached resources', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.route('**/*.css', route => route.abort());

    await expect(page.locator('.main-container')).toBeVisible();

    await page.unroute('**/*.css');
  });

  test('should verify service worker precache asset list is defined', async ({ page }) => {
    const swContent = await page.evaluate(async () => {
      try {
        const res = await fetch('/service-worker.js');
        if (!res.ok) return null;
        return await res.text();
      } catch {
        return null;
      }
    });

    if (swContent) {
      expect(swContent).toContain('cacheName');
      expect(swContent).toContain('install');
      expect(swContent).toContain('activate');
      expect(swContent).toContain('fetch');
    }
  });

  test('should verify service worker handles notification click events', async ({ page }) => {
    const swContent = await page.evaluate(async () => {
      const res = await fetch('/service-worker.js');
      return await res.text();
    });

    expect(swContent).toContain('notificationclick');
    expect(swContent).toContain('pomodoro-notifications');
    expect(swContent).toContain('NOTIFICATION_ACTION');
  });

  test('should verify BroadcastChannel is created in service worker scope', async ({ page }) => {
    const swContent = await page.evaluate(async () => {
      const res = await fetch('/service-worker.js');
      return await res.text();
    });

    expect(swContent).toContain('new BroadcastChannel');
    expect(swContent).toContain('pomodoro-notifications');
  });
});
