import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Mode Tab Keyboard Hints', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('pomodoro tab should have title attribute with keyboard shortcut', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    const pomodoroTab = page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    const title = await pomodoroTab.getAttribute('title');
    expect(title).toBeTruthy();
    expect(title).toContain('P');
  });

  test('short break tab should have title attribute with keyboard shortcut', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    const shortBreakTab = page.locator('.mode-tabs button').filter({ hasText: 'Short break' });
    const title = await shortBreakTab.getAttribute('title');
    expect(title).toBeTruthy();
    expect(title).toContain('S');
  });

  test('long break tab should have title attribute with keyboard shortcut', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    const longBreakTab = page.locator('.mode-tabs button').filter({ hasText: 'Long break' });
    const title = await longBreakTab.getAttribute('title');
    expect(title).toBeTruthy();
    expect(title).toContain('L');
  });

  test('all mode tabs should have role="tab" attribute', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    const tabs = page.locator('.mode-tabs button[role="tab"]');
    await expect(tabs).toHaveCount(3);
  });

  test('active tab should have aria-selected attribute set to true', async ({ page }) => {
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    const activeTab = page.locator('.mode-tabs button.active');
    await expect(activeTab).toBeVisible();
    const hasAriaSelected = await activeTab.evaluate(el => el.hasAttribute('aria-selected'));
    expect(hasAriaSelected).toBe(true);
  });
});
