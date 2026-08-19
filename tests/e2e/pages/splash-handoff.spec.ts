import { test, expect } from '../fixtures/consoleCheck';

test.describe('Splash Handoff', () => {
  test.describe.configure({ timeout: 60000 });

  test('splash clock persists through the Blazor handoff until the app is ready', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });

    const splash = page.locator('#splash');
    await expect(splash).toBeVisible({ timeout: 10000 });
    await expect(splash.locator('.loading-spinner')).toBeVisible();

    await expect(page.locator('#app #splash')).toHaveCount(0);

    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(splash).not.toBeVisible({ timeout: 10000 });
  });
});
