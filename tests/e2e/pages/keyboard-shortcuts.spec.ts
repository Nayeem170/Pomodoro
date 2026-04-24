import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe.configure({ mode: 'serial' });

test.describe('Keyboard Shortcuts', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should open keyboard help modal with button', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.keyboard-help-modal.visible')).toBeVisible();
  });

  test('should display timer controls shortcuts in help modal', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: 'Space' })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: 'R' })).toBeVisible();
  });

  test('should display session switching shortcuts in help modal', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^P$/ })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^S$/ })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^L$/ })).toBeVisible();
  });

  test('should display help shortcut in help modal', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: '?' })).toBeVisible();
  });

  test('should close keyboard help modal with close button', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await pomodoroPage.closeKeyboardHelp();
    await expect(page.locator('.keyboard-help-modal.visible')).not.toBeVisible();
  });

  test('should close keyboard help modal with Escape key', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await page.keyboard.press('Escape');
    await page.waitForTimeout(500);
    await expect(page.locator('.keyboard-help-modal.visible')).not.toBeVisible();
  });

  test('should start timer with Space key when task is selected', async ({ page }) => {
    await pomodoroPage.addTask('Space key task');
    await pomodoroPage.selectTask('Space key task');
    await page.keyboard.press('Space');
    await page.waitForTimeout(500);
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should pause timer with Space key when running', async ({ page }) => {
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 10000 });
    await page.keyboard.press('Space');
    await page.waitForTimeout(500);
    await expect(page.locator('button[aria-label="Resume timer"]')).toBeVisible();
  });

  test('should resume timer with Space key when paused', async ({ page }) => {
    await expect(page.locator('button[aria-label="Resume timer"]')).toBeVisible({ timeout: 10000 });
    await page.keyboard.press('Space');
    await page.waitForTimeout(500);
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
  });

  test('should reset timer with R key', async ({ page }) => {
    await page.keyboard.press('r');
    await page.waitForTimeout(500);
    await expect(page.locator('button[aria-label="Start timer"]')).toBeVisible();
  });

  test('should switch to short break with S key', async ({ page }) => {
    await page.keyboard.press('s');
    await page.waitForTimeout(500);
    const shortBreakButton = page.locator('.mode-tabs button').filter({ hasText: 'Short break' });
    await expect(shortBreakButton).toHaveClass(/active/);
  });

  test('should switch to long break with L key', async ({ page }) => {
    await page.keyboard.press('l');
    await page.waitForTimeout(500);
    const longBreakButton = page.locator('.mode-tabs button').filter({ hasText: 'Long break' });
    await expect(longBreakButton).toHaveClass(/active/);
  });

  test('should switch to pomodoro with P key', async ({ page }) => {
    await page.keyboard.press('p');
    await page.waitForTimeout(500);
    const pomodoroButton = page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should not trigger shortcuts when typing in task input', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.task-input')).toBeVisible();
    await page.locator('.task-input').click();
    await page.locator('.task-input').pressSequentially('typing test');
    await page.waitForTimeout(300);
    const pomodoroButton = page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should cancel add task form with Escape key', async ({ page }) => {
    await page.locator('.task-add-btn').click();
    await expect(page.locator('.task-input')).toBeVisible();
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    await expect(page.locator('.task-input')).not.toBeVisible();
  });
});
