import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Keyboard Shortcuts', () => {
  test.describe.configure({ mode: 'serial' });

  let pomodoroPage: PomodoroPage;

  test.beforeAll(async ({ browser }) => {
    const page = await browser.newPage();
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
  });

  test('should open keyboard help modal with button', async () => {
    await pomodoroPage.openKeyboardHelp();
    await expect(pomodoroPage.page.locator('.keyboard-help-modal.visible')).toBeVisible();
    await pomodoroPage.closeKeyboardHelp();
    await expect(pomodoroPage.page.locator('.keyboard-help-modal.visible')).not.toBeVisible({ timeout: 5000 });
  });

  test('should display timer controls shortcuts in help modal', async () => {
    await pomodoroPage.openKeyboardHelp();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: 'Space' })).toBeVisible();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: 'Ctrl+R' })).toBeVisible();
    await pomodoroPage.closeKeyboardHelp();
  });

  test('should display session switching shortcuts in help modal', async () => {
    await pomodoroPage.openKeyboardHelp();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: 'Ctrl+P' })).toBeVisible();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: 'Ctrl+S' })).toBeVisible();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: 'Ctrl+L' })).toBeVisible();
    await pomodoroPage.closeKeyboardHelp();
  });

  test('should display help shortcut in help modal', async () => {
    await pomodoroPage.openKeyboardHelp();
    await expect(pomodoroPage.page.locator('.shortcut-item kbd').filter({ hasText: '?' })).toBeVisible();
    await pomodoroPage.closeKeyboardHelp();
  });

  test('should close keyboard help modal with close button', async () => {
    await pomodoroPage.openKeyboardHelp();
    await pomodoroPage.closeKeyboardHelp();
    await expect(pomodoroPage.page.locator('.keyboard-help-modal.visible')).not.toBeVisible();
  });

  test('should close keyboard help modal with Escape key', async () => {
    await pomodoroPage.openKeyboardHelp();
    await pomodoroPage.page.keyboard.press('Escape');
    await expect(pomodoroPage.page.locator('.keyboard-help-modal.visible')).not.toBeVisible({ timeout: 5000 });
  });

  test('should start timer with Space key when task is selected', async () => {
    await pomodoroPage.addTask('Space key task');
    await pomodoroPage.selectTask('Space key task');
    await pomodoroPage.page.keyboard.press('Space');
    await expect(pomodoroPage.page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
  });

  test('should pause timer with Space key when running', async () => {
    await expect(pomodoroPage.page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 10000 });
    await pomodoroPage.page.keyboard.press('Space');
    await expect(pomodoroPage.page.locator('button[aria-label="Resume timer"]')).toBeVisible({ timeout: 5000 });
  });

  test('should resume timer with Space key when paused', async () => {
    await expect(pomodoroPage.page.locator('button[aria-label="Resume timer"]')).toBeVisible({ timeout: 10000 });
    await pomodoroPage.page.keyboard.press('Space');
    await expect(pomodoroPage.page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
  });

  test('should reset timer with Ctrl+R', async () => {
    await pomodoroPage.page.keyboard.press('Control+r');
    await expect(pomodoroPage.page.locator('button[aria-label="Start timer"]')).toBeVisible({ timeout: 5000 });
  });

  test('should switch to short break with Ctrl+S', async () => {
    await pomodoroPage.page.keyboard.press('Control+s');
    const shortBreakButton = pomodoroPage.page.locator('.mode-tabs button').filter({ hasText: 'Short break' });
    await expect(shortBreakButton).toHaveClass(/active/);
  });

  test('should switch to long break with Ctrl+L', async () => {
    await pomodoroPage.page.keyboard.press('Control+l');
    const longBreakButton = pomodoroPage.page.locator('.mode-tabs button').filter({ hasText: 'Long break' });
    await expect(longBreakButton).toHaveClass(/active/);
  });

  test('should switch to pomodoro with Ctrl+P', async () => {
    await pomodoroPage.page.keyboard.press('Control+p');
    const pomodoroButton = pomodoroPage.page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should not switch session with bare letter keys', async () => {
    // Ensure pomodoro is active first
    await pomodoroPage.page.keyboard.press('Control+p');
    const pomodoroButton = pomodoroPage.page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);

    // Bare letters (no Ctrl) must NOT trigger session shortcuts,
    // even when focus is on a button or body (not an input field).
    await pomodoroPage.page.keyboard.press('l');
    await pomodoroPage.page.keyboard.press('s');
    await pomodoroPage.page.keyboard.press('p');
    await expect(pomodoroButton).toHaveClass(/active/);
  });

  test('should not trigger shortcuts when typing in task input', async () => {
    await pomodoroPage.closeKeyboardHelp();
    await pomodoroPage.page.locator('.task-input').click();
    await pomodoroPage.page.locator('.task-input').pressSequentially('typing test');
    const pomodoroButton = pomodoroPage.page.locator('.mode-tabs button').filter({ hasText: 'Pomodoro' });
    await expect(pomodoroButton).toHaveClass(/active/);
    await pomodoroPage.page.keyboard.press('Escape');
    await expect(pomodoroPage.page.locator('.task-input')).toHaveValue('');
  });

  test('should clear add task input with Escape key', async () => {
    await pomodoroPage.closeKeyboardHelp();
    await pomodoroPage.page.locator('.task-input').click();
    await pomodoroPage.page.locator('.task-input').pressSequentially('escape test');
    await pomodoroPage.page.keyboard.press('Escape');
    await expect(pomodoroPage.page.locator('.task-input')).toHaveValue('');
  });
});
