import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Loading States', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('index page should not show loading container after load', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.loading-container')).not.toBeVisible();
  });

  test('history page should not show loading container after load', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.loading-container')).not.toBeVisible();
  });

  test('index page should show main content after loading resolves', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.timer-card')).toBeVisible();
    await expect(page.locator('.tasks-section')).toBeVisible();
  });

  test('history page should show main content after loading resolves', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.period-nav')).toBeVisible();
  });

  test('navigating from index to history resolves loading states for both', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.loading-container')).not.toBeVisible();

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.loading-container')).not.toBeVisible();
  });
});
