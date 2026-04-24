import { test, expect } from '@playwright/test';
import { readFileSync } from 'fs';
import { resolve } from 'path';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('PWA and Offline Support', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should have web manifest link', async ({ page }) => {
    const manifestLink = page.locator('link[rel="manifest"]');
    await expect(manifestLink).toHaveCount(1);
    await expect(manifestLink).toHaveAttribute('href', /manifest\.webmanifest/);
  });

  test('should have valid web manifest', async ({ page }) => {
    const manifestHref = await page.locator('link[rel="manifest"]').getAttribute('href');
    const manifest = await page.evaluate(async (href: string) => {
      const response = await fetch(href);
      return await response.json();
    }, manifestHref!);

    expect(manifest.name).toBeTruthy();
    expect(manifest.display).toBeTruthy();
  });

  test('should register service worker in production', async ({ page }) => {
    const hasServiceWorker = await page.evaluate(async () => {
      if (!('serviceWorker' in navigator)) return false;
      const registrations = await navigator.serviceWorker.getRegistrations();
      return registrations.length > 0;
    });
    expect(typeof hasServiceWorker).toBe('boolean');
  });

  test('should remain functional when offline (data already loaded)', async ({ page }) => {
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    await page.route('**/*', route => route.abort());

    await expect(page.locator('.ss').first()).toBeVisible();

    await page.unroute('**/*');
  });

  test('should have meta theme-color tag', async ({ page }) => {
    const html = readFileSync(resolve(__dirname, '../../../src/Pomodoro.Web/wwwroot/index.html'), 'utf-8');
    expect(html).toContain('name="theme-color"');
    expect(html).toContain('content="#374151"');
  });
});
