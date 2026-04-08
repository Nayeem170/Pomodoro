import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('About Page', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/about');
  });

  test.describe.configure({ timeout: 60000 });

  test('should load about page', async ({ page }) => {
    await expect(page.locator('.about-page')).toBeVisible();
  });

  test('should display about header', async ({ page }) => {
    await expect(page.locator('.about-header h1')).toBeVisible();
    await expect(page.locator('.about-header h1')).toContainText('Pomodoro Technique');
    await expect(page.locator('.about-header .subtitle')).toBeVisible();
  });

  test('should display what is it section', async ({ page }) => {
    await expect(page.locator('h2').filter({ hasText: 'What is Pomodoro' })).toBeVisible();
  });

  test('should display how it works section with 5 steps', async ({ page }) => {
    await expect(page.locator('h2').filter({ hasText: 'How It Works' })).toBeVisible();
    await expect(page.locator('.step-card')).toHaveCount(5);
  });

  test('should display benefits section with 6 cards', async ({ page }) => {
    await expect(page.locator('h2').filter({ hasText: 'Benefits' })).toBeVisible();
    await expect(page.locator('.benefit-card')).toHaveCount(6);
  });

  test('should display tips section', async ({ page }) => {
    await expect(page.locator('h2').filter({ hasText: 'Tips' })).toBeVisible();
    await expect(page.locator('.tips-list li')).toHaveCount(6);
  });

  test('should display default timer settings', async ({ page }) => {
    await expect(page.locator('h2').filter({ hasText: 'Default Timer' })).toBeVisible();
    await expect(page.locator('.time-card')).toHaveCount(3);
    await expect(page.locator('.time-card.pomodoro .time-value')).toContainText('25');
    await expect(page.locator('.time-card.short-break .time-value')).toContainText('5');
    await expect(page.locator('.time-card.long-break .time-value')).toContainText('15');
  });

  test('should display call to action with link to home', async ({ page }) => {
    await expect(page.locator('.cta-section')).toBeVisible();
    await expect(page.locator('.cta-button')).toBeVisible();
    await expect(page.locator('.cta-button')).toHaveAttribute('href', '/');
  });

  test('should navigate to home from CTA button', async ({ page }) => {
    await page.locator('.cta-button').click();
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });
  });
});
