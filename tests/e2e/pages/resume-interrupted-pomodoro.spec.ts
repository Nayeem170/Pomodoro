import { test, expect } from '../fixtures/consoleCheck';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('No Resume Pomodoro', () => {
  let page: PomodoroPage;

  test.describe.configure({ timeout: 180000 });

  test.beforeEach(async ({ page: p }) => {
    page = new PomodoroPage(p);
  });

  test('should not show Resume Pomodoro option after pomodoro completion', async () => {
    await page.goto('/settings');
    await expect(page.page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await page.setPomodoroMinutes(1);
    await page.goto('/');
    await expect(page.page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await page.addTask('Test Task');
    await page.selectTask('Test Task');
    await page.startTimer();
    await page.completePomodoroFast();

    await expect(page.page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.page.locator('.btn-option').filter({ hasText: 'Resume Pomodoro' })).not.toBeVisible();
  });
});
