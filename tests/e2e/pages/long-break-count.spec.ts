import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Long Break Count', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 300000 });

  test('should offer Long Break as default after completing N pomodoros', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/settings');
    await expect(page.locator('.sett-body')).toBeVisible({ timeout: 30000 });

    const toggle = page.locator('.sr-lbl').filter({ hasText: 'Auto-start pomodoros' }).locator('..').locator('.tog');
    const isOn = await toggle.evaluate(el => el.classList.contains('on'));
    if (!isOn) {
      await toggle.click();
      await page.waitForTimeout(500);
    }

    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });
    await pomodoroPage.addTask('Long Break Test');
    await pomodoroPage.selectTask('Long Break Test');

    for (let i = 1; i <= 3; i++) {
      await pomodoroPage.startTimer();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
      await pomodoroPage.completePomodoroFast();

      await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
      const defaultOption = page.locator('.btn-option.default');
      await expect(defaultOption).toContainText('Another Pomodoro');

      await page.locator('.btn-option').filter({ hasText: 'Another Pomodoro' }).click();
      await page.waitForTimeout(2000);
    }

    await pomodoroPage.startTimer();
    await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
    await pomodoroPage.completePomodoroFast();

    await expect(page.locator('.consent-modal-overlay')).toBeVisible({ timeout: 10000 });
    const defaultOption = page.locator('.btn-option.default');
    await expect(defaultOption).toBeVisible();
    await expect(defaultOption).toContainText('Long Break');

    await expect(page.locator('.btn-option').filter({ hasText: 'Long Break' })).toBeVisible();
  });
});
