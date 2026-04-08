import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Footer Dynamic Copyright Year', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should display current year in footer copyright', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    const footerCopy = page.locator('.footer-copy');
    await expect(footerCopy).toBeVisible();

    const currentYear = new Date().getFullYear().toString();
    await expect(footerCopy).toContainText(currentYear);
  });

  test('should display copyright owner in footer', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.footer-copy')).toContainText('BitOps');
  });

  test('should display made with text in footer', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.timer-section')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.footer-made')).toBeVisible();
  });
});
