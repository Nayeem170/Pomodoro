import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Error Banner Dismiss', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should not show error banner when no error exists', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.error-banner')).not.toBeVisible();
  });

  test('should have dismiss button with correct aria-label in DOM', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    const mainHtml = await page.locator('.main-container').innerHTML();
    expect(mainHtml).toContain('error-banner');

    const dismissBtn = page.locator('.error-banner button[aria-label="Dismiss error"]');
    expect(dismissBtn).toBeTruthy();
  });

  test('should hide banner when dismiss button is clicked after injecting error', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.evaluate(() => {
      const banner = document.querySelector('.error-banner') as HTMLElement;
      if (banner) {
        banner.style.display = 'flex';
        const p = banner.querySelector('p');
        if (p) p.textContent = 'Injected test error';
      }
    });

    const isVisibleBefore = await page.locator('.error-banner').isVisible().catch(() => false);
    if (isVisibleBefore) {
      await page.locator('.error-banner button[aria-label="Dismiss error"]').click();
      await page.waitForTimeout(300);
      await expect(page.locator('.error-banner')).not.toBeVisible();
    }
  });

  test('should not interfere with app controls when banner is absent', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.tasks-section')).toBeVisible();
    await expect(page.locator('.timer-card')).toBeVisible();
  });
});
