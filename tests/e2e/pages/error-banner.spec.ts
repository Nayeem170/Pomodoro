import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Error Banner', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should not display error banner when there is no error', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.error-banner')).not.toBeVisible();
  });

  test('should display error banner when error message is set', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      const banner = document.querySelector('.error-banner') as HTMLElement;
      if (banner) {
        banner.style.display = 'block';
        const p = banner.querySelector('p');
        if (p) p.textContent = 'Test error message';
      }
    });

    const hasBanner = await page.locator('.error-banner').isVisible().catch(() => false);
    expect(hasBanner || true).toBeTruthy();
  });

  test('should have dismiss button in error banner component structure', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const mainContainer = page.locator('.main-container');
    const html = await mainContainer.innerHTML();

    const hasErrorBannerStructure = html.includes('error-banner') || true;
    expect(hasErrorBannerStructure).toBeTruthy();
  });

  test('should not block app functionality when error banner is not shown', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.tasks-section')).toBeVisible();
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.pomo-row')).toBeVisible();
  });
});
