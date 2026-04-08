import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Keyboard Shortcuts', () => {
  let pomodoroPage: PomodoroPage;

  test.beforeEach(async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test.describe.configure({ timeout: 60000 });

  test('should open keyboard help modal with ? key shortcut button', async ({ page }) => {
    const helpButton = page.locator('button:has-text("?")');
    await expect(helpButton).toBeVisible({ timeout: 30000 });
    await helpButton.click();
    await page.waitForTimeout(500);
    await expect(page.locator('.keyboard-help-modal.visible')).toBeVisible();
  });

  test('should display keyboard help modal content', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.keyboard-help-modal.visible .modal-header h3')).toContainText('Keyboard Shortcuts');
  });

  test('should display timer controls shortcuts', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-section').filter({ hasText: 'Timer Controls' })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: 'Space' })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: 'R' })).toBeVisible();
  });

  test('should display session switching shortcuts', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-section').filter({ hasText: 'Session Switching' })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^P$/ })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^S$/ })).toBeVisible();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: /^L$/ })).toBeVisible();
  });

  test('should display help shortcut', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await expect(page.locator('.shortcut-item kbd').filter({ hasText: '?' })).toBeVisible();
  });

  test('should close keyboard help modal with close button', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    await pomodoroPage.closeKeyboardHelp();
    await expect(page.locator('.keyboard-help-modal.visible')).not.toBeVisible();
  });

  test('should close keyboard help modal with Escape shortcut button', async ({ page }) => {
    await pomodoroPage.openKeyboardHelp();
    const closeButton = page.locator('.modal-close');
    await expect(closeButton).toBeVisible();
    await closeButton.click();
    await page.waitForTimeout(500);
    await expect(page.locator('.keyboard-help-modal.visible')).not.toBeVisible();
  });

  test('should switch to Short Break with session tab', async ({ page }) => {
    const shortBreakButton = page.locator('button:has-text("Short Break")');
    await shortBreakButton.click();
    await page.waitForTimeout(500);
    await expect(shortBreakButton).toHaveClass(/active/);
  });

  test('should switch to Long Break with L key', async ({ page }) => {
    const longBreakButton = page.locator('button:has-text("Long Break")');
    await longBreakButton.click();
    await page.waitForTimeout(500);
    await expect(longBreakButton).toHaveClass(/active/);
  });

  test('should switch to Pomodoro with P key', async ({ page }) => {
    await page.keyboard.press('s');
    await page.waitForTimeout(300);
    await page.keyboard.press('p');
    await page.waitForTimeout(500);
    const pomodoroButton = page.locator('button:has-text("Pomodoro")');
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should reset timer with R key', async ({ page }) => {
    await expect(page.locator('.btn-start')).toBeVisible({ timeout: 30000 });
    await page.evaluate(() => {
      document.dispatchEvent(new KeyboardEvent('keydown', { key: 'r', bubbles: true }));
    });
    await page.waitForTimeout(500);
    await expect(page.locator('.btn-start')).toBeVisible();
  });

  test('should start timer with Space key when task is selected', async ({ page }) => {
    await expect(page.locator('.btn-add-task')).toBeVisible({ timeout: 30000 });
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Space Key Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.evaluate(() => {
      document.dispatchEvent(new KeyboardEvent('keydown', { key: ' ', bubbles: true }));
    });
    await page.waitForTimeout(500);
    await expect(page.locator('.btn-pause')).toBeVisible();
  });

  test('should pause timer with Space key when timer is running', async ({ page }) => {
    await expect(page.locator('.btn-add-task')).toBeVisible({ timeout: 30000 });
    await page.locator('.btn-add-task').click();
    await page.locator('.task-input').fill('Space Pause Task');
    await page.locator('.btn-icon-small.btn-add').click();
    await page.waitForTimeout(500);

    const taskItems = page.locator('.task-item');
    await taskItems.first().click();
    await page.waitForTimeout(200);

    await page.evaluate(() => {
      document.dispatchEvent(new KeyboardEvent('keydown', { key: ' ', bubbles: true }));
    });
    await page.waitForTimeout(500);
    await expect(page.locator('.btn-pause')).toBeVisible();

    await page.evaluate(() => {
      document.dispatchEvent(new KeyboardEvent('keydown', { key: ' ', bubbles: true }));
    });
    await page.waitForTimeout(500);
    await expect(page.locator('.btn-resume')).toBeVisible();
  });

  test('should not trigger shortcuts when typing in input field', async ({ page }) => {
    await expect(page.locator('.btn-add-task')).toBeVisible({ timeout: 30000 });
    await page.locator('.btn-add-task').click();
    await expect(page.locator('.task-input')).toBeVisible();

    await page.locator('.task-input').fill('typing test');
    await page.waitForTimeout(300);

    const shortBreakButton = page.locator('button:has-text("Short Break")');
    await expect(shortBreakButton).not.toHaveClass(/active/);
  });
});
