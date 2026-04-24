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
    await expect(page.locator('.about-body')).toBeVisible();
  });

  test('should display about hero section', async ({ page }) => {
    await expect(page.locator('.about-hero')).toBeVisible();
    await expect(page.locator('.about-hero-title')).toContainText('Pomodoro');
    await expect(page.locator('.about-hero-sub')).toBeVisible();
  });

  test('should display what is it section', async ({ page }) => {
    await expect(page.locator('.collapse-toggle').filter({ hasText: 'What is Pomodoro' })).toBeVisible();
  });

  test('should display how it works section with 5 steps', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'How It Works' });
    await toggle.click();
    await expect(page.locator('.step-card')).toHaveCount(5);
  });

  test('should display benefits section with 6 cards', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Benefits' });
    await toggle.click();
    await expect(page.locator('.benefit-card')).toHaveCount(6);
  });

  test('should display tips section', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Tips' });
    await toggle.click();
    await expect(page.locator('.tips-list li')).toHaveCount(6);
  });

  test('should display default timer settings', async ({ page }) => {
    const toggle = page.locator('.collapse-toggle').filter({ hasText: 'Default Timer' });
    await toggle.click();
    await expect(page.locator('.time-card')).toHaveCount(3);
    await expect(page.locator('.time-card.pomodoro .time-value')).toContainText('25');
    await expect(page.locator('.time-card.short-break .time-value')).toContainText('5');
    await expect(page.locator('.time-card.long-break .time-value')).toContainText('15');
  });
});
