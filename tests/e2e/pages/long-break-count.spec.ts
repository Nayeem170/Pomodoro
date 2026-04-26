import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Long Break Count', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 600000 });

  test('should offer Long Break as default after completing N pomodoros', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.setSettingViaIndexedDB('longBreakInterval', 2);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.setPomodoroMinutes(1);

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.addTask('Long Break Test');
    await pomodoroPage.selectTask('Long Break Test');

    await pomodoroPage.startTimer();
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
    await page.waitForTimeout(1000);

    const isAlreadyRunning = await page.locator('button[aria-label="Pause timer"]').isVisible({ timeout: 3000 }).catch(() => false);
    if (!isAlreadyRunning) {
      await pomodoroPage.startTimer();
    }
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.btn-option').filter({ hasText: 'Long Break' })).toBeVisible();

    await page.locator('.btn-option').filter({ hasText: 'Long Break' }).click();
    await page.waitForTimeout(500);

    await expect(page.locator('.consent-modal-overlay')).not.toBeVisible({ timeout: 5000 });
    await expect(page.locator('.timer-mode-label')).toContainText(/Long break/i);
  });
});
