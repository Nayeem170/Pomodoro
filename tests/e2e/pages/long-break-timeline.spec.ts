import { test, expect } from '@playwright/test';
import { PomodoroPage } from '../fixtures/pomodoro.page';

test.describe('Long Break Activity Timeline Rendering', () => {
  let pomodoroPage: PomodoroPage;

  test.describe.configure({ timeout: 300000 });

  test('should render long break entry in history timeline', async ({ page }) => {
    pomodoroPage = new PomodoroPage(page);
    await pomodoroPage.goto('/');
    await expect(page.locator('.main-container')).toBeVisible({ timeout: 30000 });

    for (let i = 1; i <= 4; i++) {
      await pomodoroPage.addTask(`Timeline Task ${i}`);
      await pomodoroPage.selectTask(`Timeline Task ${i}`);
      await pomodoroPage.startTimer();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible();
      await pomodoroPage.completePomodoroFast();
      await pomodoroPage.skipConsentModal();
    }

    const longBreakOption = page.locator('.btn-option').filter({ hasText: /Long Break/i });
    if (await longBreakOption.isVisible({ timeout: 5000 }).catch(() => false)) {
      await longBreakOption.click();
      await page.waitForTimeout(500);

      await pomodoroPage.startTimer();
      await expect(page.locator('button[aria-label="Pause timer"]')).toBeVisible({ timeout: 5000 });
      await pomodoroPage.completePomodoroFast();
      await pomodoroPage.skipConsentModal();
    }

    await pomodoroPage.openHistory();
    await expect(page.locator('.hist-body')).toBeVisible({ timeout: 30000 });
    await page.waitForTimeout(2000);

    const longBreakBadge = page.locator('.tl-badge').filter({ hasText: /Long break/i });
    const hasLongBreak = await longBreakBadge.isVisible({ timeout: 5000 }).catch(() => false);
    expect(hasLongBreak).toBe(true);
  });
});
