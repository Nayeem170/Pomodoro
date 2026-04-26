import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Notification URL Parameter Handling', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should load home page without crashing when notification action URL params are present', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/?action=startBreak&type=ShortBreak');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should load home page with unknown action parameter', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/?action=unknownAction&type=Pomodoro');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });

  test('should load home page with partial URL parameters', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/?action=startPomodoro');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
  });
});
