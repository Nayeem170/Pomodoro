import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe.configure({ mode: 'serial' });

test.describe('Mobile Viewport (375x812)', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    pomodoroPage = new PomodoroPage(page);
  });

  test('should render timer page on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.ring-area')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.timer-time')).toBeVisible();
  });

  test('should show mode tabs on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.mode-tabs')).toBeVisible({ timeout: 30000 });
  });

  test('should show task list on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.task-card')).toBeVisible({ timeout: 30000 });
  });

  test('should show add task button on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.task-add-btn')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to settings on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to about on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await page.locator('.mobile-tab[href="/about"]').click();
    await expect(page.locator('.about-body')).toBeVisible({ timeout: 30000 });
  });

  test('should add and complete task on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await pomodoroPage.addTask('Mobile task');
    await expect(page.locator('.task-row').filter({ hasText: 'Mobile task' })).toBeVisible();
    await page.locator('.task-row').filter({ hasText: 'Mobile task' }).locator('button[aria-label="Complete"]').click();
    await expect(page.locator('.completed-section')).toBeVisible();
  });

  test('should display settings sections on mobile', async ({ page }) => {
    await pomodoroPage.openSettings();
    await expect(page.locator('.ss-hdr').first()).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.step-input').first()).toBeVisible();
  });

  test('should show navigation on mobile', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.mobile-nav')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.mobile-tab')).toHaveCount(4);
  });
});

test.describe('Tablet Viewport (768x1024)', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    pomodoroPage = new PomodoroPage(page);
  });

  test('should render timer page on tablet', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.ring-area')).toBeVisible({ timeout: 30000 });
    await expect(page.locator('.timer-time')).toBeVisible();
  });

  test('should show task list on tablet', async ({ page }) => {
    await pomodoroPage.goto('/');
    await expect(page.locator('.task-card')).toBeVisible({ timeout: 30000 });
  });

  test('should navigate to settings on tablet', async ({ page }) => {
    await pomodoroPage.goto('/');
    await pomodoroPage.openSettings();
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
  });

  test('should add task on tablet', async ({ page }) => {
    await pomodoroPage.goto('/');
    await pomodoroPage.addTask('Tablet task');
    await expect(page.locator('.task-row').filter({ hasText: 'Tablet task' })).toBeVisible();
  });
});
