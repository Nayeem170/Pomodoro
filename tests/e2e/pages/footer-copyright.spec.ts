import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Footer Dynamic Copyright Year', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 60000 });

  test('should not display footer as it does not exist', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    await expect(page.locator('.footer-copy')).not.toBeAttached();
    await expect(page.locator('.footer-made')).not.toBeAttached();
    await expect(page.locator('.app-footer')).not.toBeAttached();
  });
});
