import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Offline Timer', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should keep timer page functional when going offline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.route('**/*', route => route.abort());

    await expect(page.locator('.main-container')).toBeVisible();
    await expect(page.locator('.mode-tabs')).toBeVisible();
    await expect(page.locator('.ring-area')).toBeVisible();

    await page.unroute('**/*');
  });

  test('should display timer controls when offline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Offline Timer Task');
    await pomodoroPage.selectTask('Offline Timer Task');

    await page.route('**/*', route => route.abort());

    await expect(page.locator('button[aria-label="Start timer"]')).toBeVisible();
    await expect(page.locator('button[aria-label="Start timer"]')).toBeEnabled();

    await page.unroute('**/*');
  });

  test('should allow session switching when offline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.route('**/*', route => route.abort());

    await pomodoroPage.switchToShortBreak();
    const timerType = await pomodoroPage.getTimerType();
    expect(timerType.toUpperCase()).toContain('SHORT BREAK');

    await pomodoroPage.switchToLongBreak();
    const timerTypeLong = await pomodoroPage.getTimerType();
    expect(timerTypeLong.toUpperCase()).toContain('LONG BREAK');

    await pomodoroPage.switchToPomodoro();
    const timerTypePom = await pomodoroPage.getTimerType();
    expect(timerTypePom.toUpperCase()).toContain('POMODORO');

    await page.unroute('**/*');
  });

  test('should display task list when offline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await pomodoroPage.addTask('Offline Task');

    await page.route('**/*', route => route.abort());

    await expect(page.locator('.task-card')).toBeVisible();
    await expect(page.locator('.task-row').filter({ hasText: 'Offline Task' })).toBeVisible();

    await page.unroute('**/*');
  });
});
